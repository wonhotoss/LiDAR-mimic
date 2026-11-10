# LiDAR-mimic — Unity 구현

[English](README.md) · **한국어**

🔗 **라이브 데모:** [wonhotoss.github.io/LiDAR-mimic/unity](https://wonhotoss.github.io/LiDAR-mimic/unity/) — WebGPU 지원 브라우저(최신 Chrome/Edge) 필요.

루트 [README.ko.md](../README.ko.md)의 플랫폼 독립적 아이디어를 **Unity 6 / URP 17 / RenderGraph**에서
구현한 것이다. 이 문서는 (1) 아이디어가 Unity의 어떤 기능으로 매핑되는지, (2) 런타임에 무엇을
조작할 수 있는지를 다룬다. (씬 배선은 코드/컴포넌트의 일부이므로 각 필드의 인스펙터 도움말과 코드 주석으로 기술한다.)

- 환경: **Unity 6000.4.10**, **URP 17.4.0**, RenderGraph API, Input System 패키지.
- 테스트 환경: **Windows + 데스크탑 Chrome (WebGPU)**.
- URP 설정: RequireDepthTexture / RequireOpaqueTexture on, DepthPriming off, MSAA off, Forward+.
- 코드: [Assets/Scripts/Lidar/](Assets/Scripts/Lidar/), UI: [Assets/UI/](Assets/UI/).

---

## 1. 아이디어 → Unity 매핑

| 개념(루트 README) | Unity 구현 |
|---|---|
| 센서 = 90° 면 여섯 개를 가진 한 점 | `lidar` 컴포넌트가 붙은 빈 GameObject(트랜스폼이 센서 자세). 런타임에 자식 면 Camera 여섯 개(`CubemapFace` 순서)를 만들어 각각 자기 `RenderTexture`(`face_rt[i]`)에 렌더. 어떤 광선도 선택하지 않는 면은 비활성. |
| 센서 시점 패스 (거리+ID 선획득) | `lidar_render_feature`의 `id_pass`, 모든 면 카메라에서 실행. override 머티리얼 `lidar/id_write`로 씬을 그려 R=ID, G=거리(m) 기록. |
| 오프스크린 버퍼 | `face_rt[0..5]` — **RGFloat** RenderTexture (R=ID 실수, G=거리). 화면 해상도와 독립(면당 `map_resolution`, 기본 512²). `id_rt`는 +Z 면(디버그 뷰·진단용). |
| 복원 패스 (compute) | `lidar_reconstruct.compute`. 활성 면이 모두 렌더되면(`RenderPipelineManager.endCameraRendering`에서 카운트) dispatch. `sweep`이 켜져 있으면 이번 프레임에 쓸고 간 방위각 부채꼴만. |
| 포인트 버퍼 | `ComputeBuffer<pc_point>` (`pc_point = { float3 world; uint id }`). 리드백 없음. |
| 통합 렌더 패스 | `lidar_render_feature`의 `point_pass`. 메인 카메라에서 `DrawProcedural`(6×N 삼각형)로 스플랫. 셰이더 `lidar/point`. |
| 오브젝트 ID | `lidar_receiver` 컴포넌트가 있으면 부여. `lidar_receiver_registry`가 enable 시 1 이상의 작은 순차 정수를 자동 발급. ID 0 = 배경/비수신자. |

### 패스 순서

하나의 `ScriptableRendererFeature`(`lidar_render_feature`)가 카메라에 따라 분기한다.

```mermaid
flowchart TD
    subgraph LC["라이다 면 카메라 ×6 (depth 낮음 → 먼저 렌더)"]
        L1["id_pass<br/>씬 opaque 전체를 lidar/id_write로 그림<br/>→ face_rt[i] (R=ID, G=거리)"]
        L2["compute reconstruct<br/>마지막 면 이후 dispatch<br/>광선별 방향 → 면 → Load → 원점 + 방향×거리"]
        L1 --> L2
    end
    subgraph MC["메인 카메라 (depth 높음 → 나중에 렌더)"]
        M1["메인 opaque<br/>일반 오브젝트만 (LidarOnly 레이어 제외)"]
        M2["point_pass<br/>DrawProcedural 6×N → 화면 고정 크기 스플랫<br/>메인 color+depth 어태치먼트에 직접"]
        M1 --> M2
    end
    L2 -->|"pc 버퍼 (ComputeBuffer, GPU)"| M2
    L2 -.->|"선택: lidar_capture 비동기 리드백"| CAP["PLY 시퀀스"]
```

1. **라이다 면 카메라** — 각각 `id_pass`만 큐잉. `AfterRenderingOpaques`에서 씬 opaque 전체를 override
   머티리얼로 그려 그 면의 `face_rt`에 ID+거리를 기록. `lidar/id_write`는 `ZWrite Off / ZTest LEqual`로,
   카메라 opaque 패스가 써둔 깊이 위에 겹쳐 그려 최근접 표면의 ID만 남긴다. ID는 렌더러의
   `MaterialPropertyBlock`(`_LidarID`)에서 읽고, 거리는 `distance(world, _WorldSpaceCameraPos)`다(면 카메라가
   센서 원점에 있다).
2. **복원(compute)** — 마지막 활성 면이 렌더된 뒤 RenderGraph 밖에서 dispatch. 광선(센서 로컬 방향)마다 지배
   축으로 면을 고르고, 그 면의 `GL.GetGPUProjectionMatrix(proj, false) * worldToCamera`로 방향을 텍셀에
   투영해 `Load`(point 샘플)로 ID·거리를 읽어 `lidar_pos + 방향 × 거리`를 `pc` 버퍼에 기록. C#과 HLSL의
   `face_of`는 같은 동점 규칙을 쓰므로 CPU에서 비활성화한 면을 GPU가 읽는 일이 없다.
3. **메인 카메라 opaque** — URP 기본. 일반 오브젝트만(레이어 필터로 pc-only 오브젝트는 제외).
4. **통합 드로우** — `AfterRenderingOpaques`. `pc` 버퍼를 읽어 화면 고정 크기 축정렬 사각형으로 스플랫.
   메인 color+depth 어태치먼트에 직접 그린다. depth read/write on → 차폐가 하드웨어 깊이 테스트로 처리.

면 카메라와 메인 카메라의 실행 순서는 **카메라 `depth` 값**으로 보장한다(`lidar.camera_depth` < 메인 카메라
depth). `scan_mask`가 면 카메라의 컬링 마스크다. 센서가 자기 마커를 스캔하지 않도록 `Viz`를 제외한다.

---

## 2. 오브젝트 모드

오브젝트를 포인트클라우드 소스로 만들려면 [Assets/Scripts/Lidar/lidar_receiver.cs](Assets/Scripts/Lidar/lidar_receiver.cs)
컴포넌트를 붙인다. 컴포넌트가 없는 오브젝트는 일반 렌더만 되는 normal-only이며, 라이다 패스에서는 ID 0
차폐물로만 취급된다.

`receiver_mode`는 세 가지이고, 각각은 **(레이어, `_LidarID`)** 조합으로 구현된다 — 씬/프리팹/카메라를
다시 배선하지 않고 레이어 이동 + MPB 값만 바꾼다.

| 모드 | 레이어 | `_LidarID` | 메인 화면 | 결과 |
|---|---|---|---|---|
| `pc_only` | `LidarOnly` | id | 숨김(cullingMask 제외) | **포인트로만** 표시 |
| `both` | 원래 레이어 | id | solid | solid + 포인트 동시(겹침은 depth bias로 완화) |
| `solid` | 원래 레이어 | 0 | solid | 일반 렌더만, 차폐는 하되 **포인트 없음** |

- **active 토글** — `lidar_receiver.active`는 `Renderer.enabled`를 켜고 끈다(`GameObject.SetActive`가 아님).
  끄면 모든 카메라에서 빠져 solid·차폐·포인트가 모두 사라지지만, 레지스트리 등록과 모드 상태는 유지되어
  다시 켜면 그대로 복원된다.
- 모든 receiver는 시작 시 `pc_only`로 초기화된다.

---

## 3. 스캔 패턴

[Assets/Scripts/Lidar/lidar.cs](Assets/Scripts/Lidar/lidar.cs)의 `generate()`가 광선 방향(센서 로컬 단위 벡터)
배열을 만든다. **회전형 센서 패턴**이다. `elevation_min`~`elevation_max`에 등간격으로 놓인 `channel_count`개의
고도각 채널이 360°를 `azimuth_steps`개 컬럼으로 나눠 발사하며, `channel_azimuth_offset`으로 채널마다 방위각을
어긋나게 할 수 있다. 기본값은 Hesai JT128(128 × 900, -4.4° … 90.5°)이다. 배열은 방위각 우선
(`index = azimuth × channel_count + channel`)이라 스윕 부채꼴이 연속 구간이 되고, 길이는 항상
`azimuth_steps × channel_count`와 정확히 일치한다(버퍼 할당과 dispatch가 이 수에 의존). 밀도는 의도적으로
균일하지 **않다**. 채널은 고도각이 일정한 링이고, 그것이 회전 스캔의 외형이다.

**스윕.** `sweep`이 켜져 있으면 매 프레임 모터가 전진한 컬럼(`scan_hz × Time.deltaTime` 회전)만 복원하고 포인트
버퍼의 나머지는 유지되므로 한 바퀴가 여러 프레임에 걸쳐 완성된다. 끄면 매 프레임 버퍼 전체를 다시 만든다.

`generate()`는 **단일 소스**다. 실제 스캔에 올리는 GPU 버퍼도, UI의 패턴 프리뷰 이미지도 같은 배열에서 그린다.

패턴 파라미터가 바뀌면 `lidar.rebuild()`가 광선 버퍼를 다시 채우고(포인트 수가 바뀌면 재할당) 프리뷰를 갱신한다.
에디터 플레이 중에는 `OnValidate`가 자동 호출된다. 파라미터는 `OnValidate`에서 **≥1로 clamp**된다(0/음수 버퍼 예외 방지).

---

## 4. 포인트 렌더 모드

전역 렌더 모드는 `point_render_mode` 두 가지이며 런타임에 전환한다(`lidar.render_mode`, 기본값 `depth_map`).

- **`per_object`** — 오브젝트(ID)별 색과 크기로 그린다. 색·크기는 각 `lidar_receiver`의 값에서 per-ID 스타일 버퍼로 모아 셰이더에 전달.
- **`depth_map`** — 센서로부터의 거리로 컬러맵을 입힌다. 포인트 크기는 전역 고정값. 정점 셰이더에서
  `distance(world, lidar_pos)`를 `[depth_min, depth_max]`로 정규화한 뒤 시간 기반 위상을 더해
  `frac(t + depth_offset)`으로 컬러맵 텍스처를 샘플한다.

`depth_map` 관련 전역값은 **렌더 피처(`PC_Renderer` 상의 `lidar_render_feature`)에서 에디터 전용으로** 편집한다
(런타임 UI에는 모드 토글만 있다):

| 필드 | 기본값 | 효과 |
|---|---|---|
| `global_point_size` | 4 | depth_map 모드의 포인트 크기(px) |
| `depth_colormap` | jet 유사(파랑→빨강→다시 파랑) | 거리→색 그라디언트. 순환 샘플하므로 끝 색을 시작 색과 맞춘다 |
| `depth_min` / `depth_max` | 0 / 50 | 컬러맵 양 끝에 매핑되는 거리(m) |
| `depth_emission` | 1 | 컬러맵 색에 곱하는 값. >1이면 Bloom을 먹여 발광 |
| `depth_scroll_speed` | 0.1 | 컬러맵 흐름 속도(cycles/sec). 음수면 반대 방향, 0이면 정지 |
| `depth_bias` | 0.0002 | 클립 z를 카메라 쪽으로 미세 이동(both 모드 coplanar z-fighting 완화). **부호는 플랫폼 의존** — 포인트가 자기 표면에 가려지면 부호를 뒤집는다 |

컬러맵은 256×1 룩업 텍스처로 baked되어 셰이더가 샘플한다. bake가 주기적이라(텍셀 `i` = 그라디언트 `i/256`,
`Repeat` 래핑) 스크롤이 이음매를 지나갈 때도 필터링이 매끄럽다.

---

## 5. 런타임 컨트롤 패널

모든 조작은 런타임 UI Toolkit 패널 하나로 통합되어 있어 **스탠드얼론 빌드에서도** 전 기능을 쓸 수 있다.
([Assets/Scripts/Lidar/lidar_control_panel.cs](Assets/Scripts/Lidar/lidar_control_panel.cs),
[Assets/UI/lidar_control_panel.uxml](Assets/UI/lidar_control_panel.uxml) / `.uss`)

접이식 섹션 순서: **Point Rendering → Receivers → Pattern → Recording → Debug.**

### Point Rendering
- `per-object` / `depth-map` 버튼 — 전역 포인트 렌더 모드 전환. 현재 모드가 하이라이트된다.

### Receivers
- **All** 행 — `pc-only` / `both` / `solid`로 모든 receiver를 한 번에 전환.
- **오브젝트별 행** — 오브젝트 이름 + on/off active 버튼(켜짐=녹색; 꺼지면 모드 버튼 비활성) + `pc-only`/`both`/`solid` 모드 버튼.
- **행에 마우스를 올리면** 해당 오브젝트 위치에 이름 마커가 3D로 표시된다(카메라 뒤면 숨김). 마커는 씬 클릭을 가로채지 않는다.
- **per-object 모드일 때만** 각 행에 색 편집기가 나타난다: 색 스와치 + `Emission` 슬라이더(1–8).
  스와치를 클릭하면 R/G/B 팝업 피커가 열린다. 색은 `base RGB × Emission`(HDR)이며 >1이면 Bloom으로 발광한다.
  (UI Toolkit `ColorField`는 에디터 전용이라 런타임은 스와치+슬라이더로 HDR 색을 구성.)

### Pattern
`lidar` 디바이스에 라이브 바인딩. 값 변경 → `rebuild()` → 프리뷰 갱신.

| 컨트롤 | 범위 | 대상 |
|---|---|---|
| `Channels` | 1–256 | `channel_count` — 고도각 채널 수 |
| `Azimuth steps` | 1–2048 | `azimuth_steps` — 회전당 컬럼 수(총수 = Channels×이 값) |
| `Elevation min` / `max` | -90–90 | `elevation_min` / `elevation_max` — 스캔 띠(도). min ≤ max로 유지 |
| `Az. stagger` | 0–1 | `channel_azimuth_offset` — 채널마다 더해지는 방위각(도), 컬럼이 수직으로 정렬되지 않게 함 |
| `Scan Hz` | 0–30 | `scan_hz` — 모터 회전 수/초(rebuild 없음) |
| `Sweep` | — | `sweep` — 프레임당 부채꼴 갱신 on/off(rebuild 없음) |
| `pattern_preview` | — | 등장방형 플롯(x = 방위각 0–360°, y = 고도각 -90–90°), 검은 배경 · 흰 점 |

### Recording
포인트 버퍼를 **비동기 GPU 리드백**으로 스냅샷해 프레임당 하나의 **바이너리 PLY**(x/y/z float + id uint)로 저장한다.
([Assets/Scripts/Lidar/lidar_capture.cs](Assets/Scripts/Lidar/lidar_capture.cs)) 캡처는 벽시계 기준으로 스로틀되어 앱 fps에 영향을 주지 않는다.

| 컨트롤 | 대상 | 효과 |
|---|---|---|
| `Prefix` | `prefix` | 출력 파일명 접두어(`{prefix}_{000000}.ply`) |
| `Capture FPS` | `capture_fps` | 초당 최대 캡처 수(≤0이면 렌더 프레임마다) |
| `Drop id==0` | `filter` | 미교차/배경 포인트 제거(끄면 오프라인 필터용 raw 덤프) |
| `OpenGL coords` | `opengl` | 켜면 z 부호를 뒤집어 우수좌표계(OpenGL)로 저장. 끄면 Unity 원시 월드 좌표 |
| `Output dir` | `output_dir` | 출력 폴더(빈 값 = `Application.persistentDataPath`) |
| `Browse…` | — | 네이티브 폴더 선택 대화상자(UnityStandaloneFileBrowser) |
| `Start/Stop Recording` | `recording` | 캡처 토글 |
| status | — | 녹화 상태 + 캡처된 프레임 수 |

### Debug (LiDAR view)
라이다 `id_rt`(+Z 면)를 `lidar/id_debug` 머티리얼로 blit한 텍스처를 이미지로 표시한다(센서 시점의 ID/거리
확인용. 머티리얼의 `_Far`가 흰색에 대응하는 거리).

---

## 6. 실시간에 무엇이 반영되는가

- **센서 이동/회전** — 매 프레임 면 카메라 행렬로 즉시 반영.
- **오브젝트 이동 / 본 애니메이션** — 센서 시점 패스·복원이 매 프레임 재실행되므로 즉시 반영(`sweep`이 켜져
  있으면 쓸고 간 부채꼴 안에서).
- **스캔 패턴 변경** — `rebuild()`로 광선 버퍼 재생성. 포인트 버퍼는 비워지고 필요한 면이 다시 활성화된다.
- **회전 속도 / 스윕** — 다음 프레임부터 반영.
- **일반↔포인트 토글** — 통합 드로우 시점 판정이라 재계산 없이 즉시.
- **오브젝트 색/크기** — per-ID 스타일 버퍼가 매 프레임 갱신.

`map_resolution`(면당 RT 해상도)과 면 카메라 설정(`scan_mask`, `near`, `far`, `camera_depth`)만은 런타임 변경
미지원이다(면을 만들 때 적용된다).

---

## 7. 눈으로 확인하는 요소 (요약)

직접 보면 알 수 있는 시각 요소는 다음과 같이 정리된다.

- **포인트 색** — per-object 모드: 오브젝트별 지정 색(HDR, Emission↑ 시 발광). depth-map 모드: 거리 컬러맵(기본 근거리=파랑 → 원거리=빨강).
- **포인트 크기** — 카메라 거리와 무관하게 화면 고정 크기 사각형. per-object 모드는 오브젝트별, depth-map 모드는 전역 고정.
- **패턴 프리뷰** — Pattern 섹션의 검은 배경 · 흰 점 등장방형 플롯이 스캔 띠를 보여준다. 실제 센서급 밀도에서는
  띠가 꽉 찬다.
- **호버 마커** — Receivers 행에 마우스를 올리면 해당 오브젝트에 노란 테두리 이름 박스.
- **Debug 뷰** — +Z 면의 ID/거리 맵.

---

## 관련 문서

- [../README.ko.md](../README.ko.md) — 플랫폼 독립적 핵심 아이디어.
