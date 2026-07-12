# TODO2 구현 결정사항

> LiDAR-mimic — Unity 6 (6000.4.10) / URP 17.4 / RenderGraph
> 알고리즘 명세는 [TODO2.md](TODO2.md) 참조. 이 문서는 "어떻게 구현할지"의 결정만 담는다.

## 0. 환경 / 전제
- Unity 6000.4.10, URP 17.4.0, RenderGraph API.
- 현 파이프라인 설정: RequireDepthTexture / RequireOpaqueTexture on, DepthPriming off,
  MSAA off, RenderingMode = Forward+.
- 커스텀 패스는 하나의 `ScriptableRendererFeature`로 묶는다.
- **가정 금지**: 점 개수(=패턴 옵션이 런타임 결정)와 화면 해상도(=Unity 에디터/런타임 결정)를 코드에 하드코딩하지 않는다.
  라이다 RT만 우리가 정하는 내부 버퍼(§3).
- 스케일 목표: 점 < 50,000, fps > 60, 화면 해상도 > FHD.

## 1. 오브젝트 분류 & ID (컴포넌트 기반)
- **`LidarReceiver` 컴포넌트**로 pc 오브젝트를 지정한다. authored 필드: `enabled`, 색, 크기.
  컴포넌트 없는 오브젝트 = normal-only.
- **ID = 0보다 큰 순차 정수**, `0` = 배경/컴포넌트 없음 sentinel.
  ID는 **레지스트리가 enable 시 순차 자동 발급**(수동 지정 금지 — 충돌/gap 방지). 범위 작아도 됨.
- 컴포넌트는 자기 renderer에 `_LidarID`(MPB)를 실어 라이다 패스에 ID를 공급하고, 활성 receiver를
  정적 레지스트리에 등록한다. 메인 opaque용 일반 머티리얼은 그대로 둔다(ID는 라이다·통합 패스에서만 필요).
- 통합 패스는 레지스트리의 활성 receiver를 모아 **PC 집합 + per-ID 색/크기 버퍼**를 만든다.
  `enabled` 토글이 곧 pc 렌더 토글(재계산 불필요, 즉시).
- PC 집합 판정: `(1u << id) & pcMask` 비트마스크(오브젝트 ≤ 32/64/128) 또는 `isPC[id]` 룩업 버퍼.
  포인트마다 목록 루프 금지.
- 오브젝트 종류: **normal-only**(컴포넌트 없음) / **both**(일반 렌더 + 컴포넌트) /
  **pc-only**(컴포넌트 + **전용 레이어**로 메인 카메라 cullingMask에서 제외; 라이다 카메라는 포함해 계속 그림 → id·점 생성).

## 2. 패스 순서 (ScriptableRendererFeature)
1. **LiDAR 패스**(depth + ID) → **compute 재구성** — 메인 opaque 이전.
2. **메인 opaque** — URP 기본, 일반 attribute 오브젝트만(레이어 필터).
3. **통합 패스**(포인트 드로우) — opaque 이후, 메인 color+depth 타겟에 직접.

## 3. LiDAR 패스 (두 번째 뷰)
- **구현 결정(M2): 전용 라이다 Camera + RenderTexture (방식 2)**. 라이다 카메라가 자기 RT에 렌더 →
  URP가 라이다 프러스텀 컬링을 처리. 렌더 피처(`lidar_render_feature`)는 라이다 카메라에서만 override material로 id를 씀.
  (당초 §3안이던 수동 컬링(`ScriptableCullingParameters`)은 RG 배관 리스크로 보류 — 성능 필요 시 리팩터.)
- 라이다 view / projection 행렬: 위치·방향·FoV 파라미터화(요구사항: 실시간 조정).
- RT(=`id_rt`): **RGFloat** (R = id 실수, G = NDC depth) + depth 버퍼. 카메라 타깃이라 정수 포맷 대신 float
  (작은 정수 id는 정확). depth를 G에 32bit로 담아 compute가 바로 샘플 — 8bit 패킹이 아니라 정밀도 문제 없음.
- **RT 해상도 = 화면과 독립** (라이다 depth/ID는 표시되지 않고 per-ray로만 소비). 종횡비는 **라이다 FoV** 기준.
  크기 기준 = **레이 밀도**(인접 레이가 같은 texel로 몰리지 않을 정도). <50K 점이면 고정 **1024²~FHD**로 충분(20~40:1 여유).
  under-resolution의 증상은 점 병합이 아니라 **depth 양자화**(계단식 배치); 패턴이 극단적으로 조밀해지면 상향.
- clear: depth = far(**reversed-Z에서 0.0**), ID = 0.
- 렌더: **active한 모든 opaque 오브젝트를 그린다**(전부 occluder 역할). 컴포넌트 없으면 ID 0(=클리어값),
  있으면 자기 ID. 최근접은 하드웨어 depth test가 결정 → 그 프래그먼트의 ID가 기록됨.
- ID 기록 방식(둘 중 택):
  - **A안(1패스)**: 전체를 override 머티리얼 + 렌더러 MPB(`_LidarID`)로 그려 ID를 한 번에 기록.
    → "override가 MPB를 존중하는가"(§11 A-1 참조) 검증 필요.
  - **B안(2패스, 검증 불요)**: ① 전체 opaque를 depth-only prime(per-object 데이터 없음) →
    ② 컴포넌트 오브젝트만 ID 머티리얼로 오버레이(`ZTest Equal/LEqual`). non-component는 손 안 대도 ID 0 유지.
    prime/overlay의 vertex 변환 일치만 주의.

## 4. Compute 재구성
- **버퍼 2개(입/출력 분리)**: `pattern`(`StructuredBuffer<float2>` = projXY, CPU 업로드) +
  `pc`(`RWStructuredBuffer<{ float3 world; uint id }>`, compute가 매 프레임 기록).
- **레이 패턴(projXY 생성)**: 동심원 패턴 — **링 수 + 세그먼트별 각도 offset**으로 시작(추후 고도화).
  **CPU에서 projXY 배열을 생성**해 pc버퍼에 업로드(단일 소스 — 프리뷰와 공유, §12).
  파라미터 변경 시 CPU 재생성 → pc버퍼 **재할당(개수 바뀌면) + 재업로드**(`lidar.rebuild()`; 에디터 플레이 중엔 `OnValidate`가 자동 호출).
- 디스패치: ray 개수만큼 스레드.
- 입력 샘플링: `id_rt`(R=id, G=NDC depth)를 `Load`로 **point 샘플**(bilinear 금지). uv = projXY*0.5+0.5.
- **재구성**: NDC `(projXY.xy, G)` × `inverse(GPU LiDAR VP)` → 월드 좌표. `pc[i] = { world, round(R) }`.
- no-hit ray(배경 클리어 → id 0)는 그대로 두면 통합 패스에서 자동 필터됨.
- **저장 좌표계 = 월드**(개요 line 10과 일치). compute가 레이당 1회 unproject → 통합 패스는 월드만 읽어 단순(재구성 중복 없음).
  `inverse(VP)` = `(GL.GetGPUProjectionMatrix(proj, false) * worldToCamera)`의 역행렬. (renderIntoTexture=**false** —
  `true`면 Y가 뒤집혀 재구성이 상하 반전. Windows/D3D11에서 확인. reversed-Z는 이 flag와 무관하게 적용됨.)

## 5. 통합 드로우 (포인트)
- **결정: Path 1** — `DrawProcedural`(triangles, 6×N, N = 레이 버퍼 크기 = CPU 상수). **리드백 없음.**
- 정점 셰이더: pc버퍼[vid/6]의 **월드 좌표 읽기 → 메인 clip**(unproject 불필요 — §4에서 이미 월드). corner(vid%6)로 **화면 고정 크기 축정렬 사각형**으로 확장.
  **AA 없음.** 크기는 per-ID 값(아래) 사용. 클립 z를 **카메라 쪽으로 살짝 bias**(both 오브젝트 coplanar z-fighting 완화).
- 필터: `id ∉ PC집합` 또는 `id == 0` → degenerate quad로 폐기.
- depth read/write **ON**, **메인 opaque와 동일한 depth attachment**에 그림
  (일반 오브젝트에 의한 차폐 + 점끼리 자기 차폐를 하드웨어 depth test로 처리).
- per-ID 색/크기: 룩업 버퍼 또는 셰이더 상수 참조. 라이다식 거리/높이/intensity 컬러맵은
  추후 **별도 포인트클라우드 셰이더**로 고도화.
- (선택 최적화 — Path 2, N이 크고 유효 비율 낮을 때만) compute **compaction**으로 유효 점만 append →
  `ComputeBuffer.CopyCount`로 indirect args에 GPU→GPU 복사 → `DrawProceduralIndirect`. **이 경로도 리드백 없음.**
  PC 멤버십은 드로우 때 판정(토글 재계산 회피). 프로파일링에서 무효 정점 오버헤드가 잡힐 때만 전환.

## 6. 메인 opaque 필터 & "both" 처리
- 메인/라이다는 **카메라별 cullingMask**로 분리: 메인은 pc-only 레이어 **제외**(solid 안 그림), 라이다는 **포함**(id·점 생성).
  (`Renderer.forceRenderingOff`은 방식 2에서 두 카메라 모두 숨겨 못 씀.)
- both 오브젝트: 일반 레이어 → 메인 opaque(solid) + 통합 패스(점) 양쪽 등장 → §5의 depth bias로 coplanar 완화.
- pc-only 오브젝트: pc-only 레이어 → 메인 opaque·카메라 depth에 안 나옴 → 점으로만 등장(coplanar 없음).

## 7. RenderGraph 리소스
- pc버퍼(ComputeBuffer): import, 프레임 지속(또는 매 프레임 재할당).
- 라이다 depth / ID 텍스처: 프레임 내 transient.
- (M3 구현) compute는 RG 밖에서 `RenderPipelineManager.endCameraRendering`(라이다 카메라)로 dispatch — id_rt가 채워진 직후.
  통합 드로우(M4, 메인 카메라)와의 순서는 **카메라 렌더 순서**(라이다 카메라가 메인보다 먼저)로 보장.

## 8. 프레임 동작
- 이동·본 애니메이션 대응을 위해 라이다 패스·compute는 **매 프레임 재실행**.
  라이다·씬이 모두 정적일 때만 프레임 간 캐시가 유효한 최적화.

## 9. 구현 시 체크리스트 (놓치기 쉬운 것)
- reversed-Z: far depth clear = 0.0 (URP 표준 clear면 자동, 수동이면 주의).
- depth·ID 둘 다 point/Load 샘플 — bilinear 시 실루엣 flying pixel / ID 오염.
- 통합 패스는 메인 opaque와 같은 depth attachment(패스 순서 보장).
- 라이다 RT 해상도 ≥ 유효 ray 밀도 — 낮으면 여러 ray가 같은 texel 샘플 → depth 양자화.
- 실루엣 flying pixel은 실제 라이다에도 있는 아티팩트 → 허용 가능(오히려 authentic).
- 라이다 FoV: 단일 perspective = 전방 프러스텀(<180°). 360° 회전형은 이 구조로 불가.
- 패턴 파라미터(ring_count/points_per_ring)는 **≥1로 clamp**(OnValidate) — 인스펙터 빈 칸(0)/음수가 zero-length·음수 버퍼 예외를 냄. `rebuild()`는 새 버퍼 생성 후 옛 것 해제(부분 해제 방지).

## 10. 요구사항 매핑
- 실시간 패턴 밀도/모양 조정 → §3/§4 pc버퍼(projXY) 재채움.
- pc/일반 렌더 토글 → §5 PC-ID 목록 변경(재계산 불필요, 즉시).
- 이동/본 애니메이션 → §8 매 프레임 재생성.
- 일반↔pc 오브젝트 차폐 → 라이다 depth(레이저 차폐) + 메인 depth test(카메라 차폐).
- opaque 전용 씬 → 전제.

## 11. 미결사항
아직 결정되지 않았거나, 코딩하면서 실측/검증해야 답이 나오는 것들.
우선순위: **D-12(스케일 목표)** 가 정해지면 A·B의 절반이 풀린다. 그다음 리스크는 **A-1(ID 주입)** 과 **C(RG 배관 검증)**.

### A. 설계 선택이 열린 것 (착수 전 결정 필요)
1. ~~ID-write 셰이더 적용 방식~~ → **결정됨(§1/§3)**: 컴포넌트 기반, 라이다 패스에서 active 전체를 그리고
   컴포넌트 없으면 ID 0. 구현은 §3의 A안(override+MPB, 검증 필요) 또는 B안(depth-prime + 컴포넌트 오버레이, 검증 불요) 중 택.
   ID는 레지스트리가 순차 자동 발급.
2. ~~포인트 드로우 경로~~ → **결정됨: Path 1**(degenerate-quad `DrawProcedural`, no-readback). §5 참조.
   Path 2(compaction+indirect, 이 역시 `CopyCount`로 no-readback)는 N 크고 유효 비율 낮을 때 프로파일링 후 전환.
3. ~~레이 패턴 파라미터화~~ → **결정됨(§4)**: 동심원 — 링 수 + 세그먼트별 각도 offset으로 시작, 변경 시 pc버퍼 재할당+재채움. 고도화 추후.
4. ~~포인트 색상 규칙~~ → **결정됨(§5)**: ID별 색+크기로 시작. 라이다식 컬러맵은 추후 별도 PC 셰이더로.
5. ~~스플랫 형태~~ → **결정됨(§5)**: 축정렬 사각형, AA 없음, 크기는 per-ID.

### B. 실측·튜닝으로만 답이 나오는 것
6. **both 오브젝트 depth bias 크기/방식** — 작으면 z-fighting, 크면 표면에서 뜨거나 차폐물 관통. 눈으로 튜닝.
7. 라이다 RT 해상도 → **결정됨(§0/§3)**: 화면과 독립, FoV 종횡비, 레이 밀도 기준 고정 캡(1024²~FHD).
   **near/far만 튜닝 잔존** — 씬 범위에 맞게 좁혀야 z 재구성 정밀.
8. **temporal 안정성** — 레이가 라이다 프로젝션 공간에 고정이라 오브젝트/라이다 이동 시 점이 표면 위를 기어다님(crawl/shimmer).
   물리적으론 정지 라이다가 보는 모습이나 시각적으로 거슬릴 수 있음 → 허용/완화(temporal 처리) 여부 미정.

### C. URP 17 RenderGraph 배관 — 되는지 검증
9. 라이다 depth attachment + ID 텍스처를 compute 패스에서 직접 읽기(`UseTexture`) — 복사 없이 되는지.
10. ComputeBuffer를 정점 셰이더에서 읽으며 compute→raster 의존성 배리어가 RG에서 제대로 걸리는지.
11. (완화됨 — §1 컴포넌트 모델) 런타임 토글은 레지스트리/`enabled`, pc-only는 `forceRenderingOff`로 처리.
    남은 RG 확인: 라이다 패스가 active 전체를 그리도록 컬링/RendererList 구성(+ B안이면 depth-prime & 컴포넌트 오버레이 드로우).

### D. 범위 미정 (요구사항엔 없지만 A/B를 좌우)
12. ~~성능/스케일 목표~~ → **결정됨(§0)**: 점 < 50,000, fps > 60, 화면 해상도 > FHD.
    단 점 개수·화면 해상도는 **가정 금지**. 라이다 RT는 화면과 독립·고정 캡(§3).
13. 다중 라이다 지원 여부.
14. 라이다 FoV <180° 한계 — 360° 회전형 필요 시 별도 설계.

## 12. 패턴 프리뷰 (개발 도구)
- 목적: 라이다 조사 패턴(레이 projXY 분포)을 이미지로 시각화해 링 수·각도 offset을 **편집 중** 확인.
- **단일 소스**: 패턴 생성은 CPU에서 projXY 배열을 만들고(§4 pc버퍼 입력으로 업로드), **같은 배열로 프리뷰를 그린다**.
  생성 로직 중복 없음, 재생 없이 edit 모드에서도 동작.
- 그리기: projXY(NDC) → `uv = projXY*0.5+0.5` → 프리뷰 `Texture2D`(256²~512²)에 점. 검은 배경 + 흰 점.
  projXY가 이미 투영 좌표라 원근 처리 불필요(그대로 `[-1,1]²` 플롯 = 라이다가 보는 패턴). 오프스크린 — depth·씬 불필요.
- 편집 통합: 패턴 설정 컴포넌트의 커스텀 Inspector(또는 EditorWindow)에 프리뷰 표시, 파라미터 변경 시 갱신.
- (선택) 런타임 오버레이가 필요하면 pc버퍼를 `DrawProcedural`로 RT에 그리는 GPU 방식도 가능.

## 13. 구현 진행 상황 & 남은 작업 (2026-07-13)

### 완료 — 코어 파이프라인 M1~M6
`unity/Assets/Scripts/Lidar/` 에 구현. 각 마일스톤은 에디터에서 검증 완료.

1. **M1 패턴 + 프리뷰 + 리시버/레지스트리** — `lidar.cs`(패턴 `generate()` + 에디터 프리뷰), `lidar_receiver.cs`, `lidar_receiver_registry.cs`.
2. **M2 라이다 패스(id + NDC depth)** — `lidar_render_feature.cs`(라이다 카메라 분기), `Shaders/lidar_id_write.shader`, 디버그: `lidar_debug_view.cs` + `Shaders/lidar_id_debug.shader`. 방식 2(전용 카메라+RGFloat `id_rt`), §3 A안(override material + MPB) 성립 확인.
3. **M3 compute 재구성** — `lidar_reconstruct.compute`(레이별 `id_rt` Load → 월드 복원), `lidar.cs`가 `endCameraRendering`에서 dispatch, 디버그: `lidar_debug_points.cs`(기즈모). 복원 행렬은 `GL.GetGPUProjectionMatrix(proj, false)`(Y-flip 없음).
4. **M4 통합 드로우** — `Shaders/lidar_point.shader`(화면 고정크기 사각형, per-id 색/크기, id==0 degenerate), `lidar_render_feature.cs` 메인 카메라 분기 + per-id 스타일 버퍼. depth-on 차폐, 카메라쪽 `depth_bias`.
5. **M5 오브젝트 모드** — normal-only / both / pc-only. pc-only는 전용 레이어를 메인 카메라 cullingMask에서 제외(코드 무변경, 씬 설정).
6. **M6 실시간 조정** — `lidar.rebuild()` + `OnValidate`(플레이 중 인스펙터 라이브), 파라미터 ≥1 clamp. 라이다 위치/방향/FoV는 매 프레임 카메라 행렬로 이미 라이브.

### 필요한 에디터 설정 (씬 배선 — 코드로 안 됨)
- 라이다 = Camera + `lidar` 컴포넌트. `reconstruct_cs` = `lidar_reconstruct`, 라이다 카메라 `depth` < 메인 카메라 `depth`.
- `PC_Renderer`에 `lidar_render_feature` 추가: `write_shader`=lidar/id_write, `point_shader`=lidar/point, `depth_bias` 튜닝.
- pc-only 레이어(예: `LidarOnly`)를 메인 카메라 cullingMask에서 해제.
- 디버그(선택): `lidar_debug_view`(device+debug_mat=lidar/id_debug), `lidar_debug_points`(device).

### 남은 작업 (전부 코어 아님 — 고도화/미세튜닝)
- **B-6 depth_bias 미세 튜닝** — both 모드 coplanar. 인스펙터로 가능(부호는 플랫폼 의존).
- **B-8 temporal shimmer** — 이동 시 점이 표면 위를 기어다님. 허용/완화 미정.
- **거리·높이·intensity 컬러맵** — 라이다식 룩. §5의 "별도 PC 셰이더"로.
- **pixelize** — TODO1의 미정 항목(화면공간 후처리). 미착수.
- **패턴 절차 고도화** — 현재 동심원(링/각도 offset)만.
- **C-9/C-10 RG 배관 대안** — 현재 compute는 RG 밖 `endCameraRendering` dispatch. RG-native로 옮길지.
- **D-13 다중 라이다**, **D-14 360° FoV**(단일 perspective 한계) — 요구사항 밖.
- **`map_resolution` 런타임 변경** — id_rt 재생성 필요, 미구현(패턴 파라미터만 라이브).
