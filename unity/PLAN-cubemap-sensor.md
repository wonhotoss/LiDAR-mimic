# Plan · cubemap sensor, spin pattern, per-frame sweep

**Status:** implemented on `claude/sleepy-feynman-ftufjw` (2026-11-10), READMEs updated; **editor verification
pending** (checklist at the end, scene wiring below). Delete this file once verified.

Replace the single perspective sensor camera with a **cube of 90° face cameras**, define the scan pattern as
**directions** (azimuth × elevation, like a spinning LiDAR) instead of projection-plane XY, and in realtime
**update only the azimuth sector the motor swept during the frame**. Together these give a 360° × up-to-
hemispherical field of view (Hesai JT128 class) with the existing one-`Load`-per-ray, readback-free pipeline.

## Why

- A projection matrix is linear, so no choice of `Camera.projectionMatrix` yields a polar / equirectangular
  image, and a perspective raster is capped below 180° (practically ~150°).
- A non-linear projection in the `id_write` vertex shader was rejected: the rasterizer interpolates straight
  edges, so large triangles (the ground) land in the wrong place unless tessellated, and WebGPU has no
  tessellation. It also breaks the `ZTest LEqual` overlay on the camera's own opaque depth.
- A cube of 90° perspective faces is the standard fix (it is exactly how reflection probes capture). Each face
  is an ordinary raster, so interpolation, culling and depth work unchanged; every direction is covered; the
  per-ray critical path is still one `Load`.
- The pattern must live in angle space regardless of where it is stored: the scan directions are the sensor's
  physical property, the cubemap is just the lookup store.

## Settled decisions

These were argued through in design; do not re-litigate them without a new reason.

| Decision | Choice | Why |
|---|---|---|
| Capture | 6 child face cameras created in code, each to its own 2D `RenderTexture` | Reuses the existing `id_pass` and camera flow untouched. `Camera.RenderToCubemap` under URP RenderGraph + `endCameraRendering` is unverified. `CopyTexture` into a cube RT and hardware `TextureCube` sampling were rejected: they add a copy, a WebGPU unknown, and Unity's cube-face orientation convention as a bug source. |
| Face lookup | Dominant axis of the lidar-local direction → face index; texel via that face's forward `vp` | The forward `vp * world → ndc → texel` mapping is the exact inverse of the `inv_vp` path that already works, so it inherits the platform Y-flip handling for free. No hand-written sign tables. |
| Stored depth | **Range** (metres from sensor) in G, not NDC depth | Reconstruction becomes `lidar_pos + dir * range`; `inv_vp` disappears; float32 range has no NDC precision cliff. |
| Pattern | One mode: **spin** (channels × azimuth steps), azimuth-major order | The JT128 "hemisphere" is a 360° spin whose elevation band reaches the zenith, not a dome around the forward axis. A dome mode is not needed. Azimuth-major order makes a sweep sector a contiguous buffer range, so the sector dispatch needs no per-ray branch. |
| Channel elevations | Equal spacing between `elevation_min` / `elevation_max` | The runtime panel can drive two sliders; it cannot edit a 128-entry table. Per-channel tables (datasheet angles) are a follow-up. |
| Faces rendered | Only the faces some ray actually selects (`Camera.enabled` per face, decided on `rebuild`) | JT128 (-4.4° .. 90.5°) needs 5 faces; a narrow band needs 4. Decided from the generated pattern with the same `face_of` rule the compute uses, so there is one source of truth. |
| Sweep | Realtime only: per frame, reconstruct the columns swept by `scan_hz × Δt`; the point buffer persists | Visual authenticity of a spinning sensor at zero extra cost. Sub-frame motion skew (per-point timestamps, `captureDeltaTime` sub-stepping, velocity-buffer correction) is **out of scope**: see below. |
| Sensor pose within a frame | Current pose for the whole sector | Rotation interpolation within the frame is a data-accuracy feature, not a visual one. Excluded with the rest of the skew work. |
| `map_resolution` default | 1024 → **512** per face | 90° / 512 = 0.18° per texel, finer than the JT128 ray spacing (0.4° × 0.74°). 1024² × 6 faces is 50 MB for nothing. |

### Why motion skew is out of scope (record this in the README)

This method's advantage is amortization: one raster shared by all rays. Modelling the motor's per-column
timing means re-rastering the scene per sub-step, and the rays-per-raster ratio collapses with the step:

| step | rays per raster (JT128, 128 ch × 900 cols) |
|---|---|
| 60° (realtime, 6 frames / rev at 60 fps, 10 Hz) | 19,200 |
| 4° | 1,280 |
| 0.4° (one column, 111 µs) | 128 |

At column granularity, rendering 5 faces to recover 128 rays is slower and less exact than 128 raycasts. That
regime belongs to a raycaster plus machine fan-out (revolutions are independent, and `captureDeltaTime`
makes replay deterministic), not to this pipeline. The pipeline's purpose stays: real-time, single machine,
visual-grade point clouds.

## Layout

```
lidar (plain GameObject = sensor pose)          scene wiring: no Camera component here any more
 ├─ lidar_face_+x  Camera 90°, aspect 1 → face_rt[0]
 ├─ lidar_face_-x  … → face_rt[1]
 ├─ lidar_face_+y  … → face_rt[2]
 ├─ lidar_face_-y  … → face_rt[3]           (disabled when no ray selects it)
 ├─ lidar_face_+z  … → face_rt[4]           = id_rt alias (debug view, diagnostics)
 └─ lidar_face_-z  … → face_rt[5]
```

Face order follows `CubemapFace` (+X, -X, +Y, -Y, +Z, -Z) for readability. Local rotations, in the lidar's
frame: `(0,90,0)`, `(0,-90,0)`, `(-90,0,0)`, `(90,0,0)`, `(0,0,0)`, `(0,180,0)`.

Each `face_rt` keeps today's format: `RGFloat`, square, R = id, **G = range (m)**.

`pattern_buffer` becomes `float3` per ray (lidar-local unit direction), index `= azimuth * channel_count +
channel`. `pc_point` is unchanged.

## Edits, in pipeline order

### 1. `lidar.cs` — faces, pattern, sweep

Drop `[RequireComponent(typeof(Camera))]` and the `cam` accessor. The component now sits on a plain
GameObject whose transform is the sensor pose; `lidar_pos` in the feature (`device.transform.position`)
already reads that.

**Fields** (replacing the four ring fields):

```csharp
[Header("Scan pattern")]
public int channel_count = 128;          // elevation layers
public int azimuth_steps = 900;          // columns per revolution (0.4° at 10 Hz for JT128)
public float elevation_min = -4.4f;      // deg, first channel
public float elevation_max = 90.5f;      // deg, last channel
public float channel_azimuth_offset = 0f;// deg added per channel (stagger), replaces ring_angle_offset

[Header("Sweep")]
public float scan_hz = 10f;              // motor revolutions per second
public bool sweep = true;                // realtime: update only the sector swept this frame; off = whole buffer every frame

[Header("Faces")]
public LayerMask scan_mask;              // what the faces see (exclude Viz); replaces the old camera's cullingMask
public float near = 0.1f;
public float far = 500f;
public float camera_depth = -2f;         // must be lower than the main camera's depth
```

`map_resolution` stays (now per face), default 512. `reconstruct_cs` stays.

**Public surface:** `points`, `point_count => azimuth_steps * channel_count`, `generate()`, `rebuild()`
stay. Add `public IReadOnlyList<Camera> faces` (the feature keys the id pass on it) and keep
`public RenderTexture id_rt => face_rt[4]` as the +Z face so the debug view and diagnostics compile unchanged.

**OnEnable:** create the six face cameras as children (the `lidar_frustum_viz` precedent for runtime-created
scene objects): `fieldOfView = 90`, `aspect = 1`, near/far, `cullingMask = scan_mask`, `clearFlags =
SolidColor`, `backgroundColor = Color.clear`, `targetTexture = face_rt[i]`, `depth = camera_depth`. They use
the default URP renderer, which is what the current LiDAR camera uses (`m_RendererIndex: -1` in the scene).
Allocate `pattern_buffer` with stride `sizeof(float) * 3`. **Clear `pc_buffer` on every allocation**
(`SetData(new pc_point[n])`): with sweep on, entries not yet swept are drawn as-is, and a fresh
`ComputeBuffer` has undefined contents.

**OnDisable:** destroy the face GameObjects, release the six RTs and both buffers, unregister.

**generate():** lidar-local unit directions, azimuth-major. Unity frame: +Z forward, +Y up, azimuth clockwise
from +Z.

```csharp
public Vector3[] generate() {
    Vector3 dir(int a, int c) {
        var t = channel_count > 1 ? c / (float) (channel_count - 1) : 0f;
        var elevation = Mathf.Lerp(elevation_min, elevation_max, t) * Mathf.Deg2Rad;
        var azimuth = (a * 360f / azimuth_steps + c * channel_azimuth_offset) * Mathf.Deg2Rad;
        var ce = Mathf.Cos(elevation);
        return new Vector3(ce * Mathf.Sin(azimuth), Mathf.Sin(elevation), ce * Mathf.Cos(azimuth));
    }
    return Enumerable.Range(0, azimuth_steps)
        .SelectMany(a => Enumerable.Range(0, channel_count).Select(c => dir(a, c)))
        .ToArray();
}
```

**face_of(Vector3 d):** dominant axis of `|d|`, ties resolved `x ≥ y ≥ z`, `≥ 0` picks the positive face. The HLSL copy
must use the **same tie rule**; a direction on an exact diagonal must map to the same face on both sides,
otherwise C# may leave that face disabled while the GPU reads it.

**rebuild():** as today, plus `faces[i].enabled = pattern.Select(face_of).Distinct().Contains(i)`. Note the
threshold is not 45°: at diagonal azimuths the Y axis becomes dominant from `atan(1/√2) ≈ 35.3°`, which is
why this is computed from the pattern rather than from `elevation_max`.

**OnValidate:** clamp `channel_count`, `azimuth_steps` ≥ 1; `elevation_max = Max(elevation_min,
elevation_max)`.

**Dispatch:** `on_end_camera` fires per face. Count faces finished this frame and dispatch when the count
reaches the number of enabled faces, then reset. (Camera order among equal depths is not guaranteed, so do not
rely on "last face".)

```csharp
void on_end_camera(ScriptableRenderContext ctx, Camera c) {
    if (faces.Contains(c)) {
        faces_done++;
        if (faces_done == faces.Count(f => f.enabled)) {
            faces_done = 0;
            reconstruct();
        }
    }
}
```

`reconstruct()` binds `face0..face5` (all six; a disabled face's RT is stale but never selected),
`face_vp[6]` via `SetMatrixArray` (`GL.GetGPUProjectionMatrix(f.projectionMatrix, false) *
f.worldToCameraMatrix`, exactly the matrix the current code inverts), `lidar_to_world =
Matrix4x4.Rotate(transform.rotation)`, `lidar_pos`, `res`, then runs the sector logic:

```csharp
var advance = sweep ? Time.deltaTime * scan_hz : 1f;   // revolutions this frame
var c0 = Mathf.FloorToInt(phase * azimuth_steps);
var c1 = Mathf.FloorToInt((phase + advance) * azimuth_steps);
void run(int from, int to) {                            // columns [from, to)
    if (to > from) {
        reconstruct_cs.SetInt("offset", from * channel_count);
        reconstruct_cs.SetInt("count", (to - from) * channel_count);
        reconstruct_cs.Dispatch(k, Mathf.CeilToInt((to - from) * channel_count / 64f), 1, 1);
    }
}
if (c1 - c0 >= azimuth_steps) {
    run(0, azimuth_steps);                              // sweep off, or the frame covered a full revolution
} else if (c1 > azimuth_steps) {
    run(c0, azimuth_steps);                             // wrap: two contiguous ranges
    run(0, c1 - azimuth_steps);
} else {
    run(c0, c1);
}
phase = Mathf.Repeat(phase + advance, 1f);
```

`phase` (fraction of a revolution, `[0,1)`) is genuine state: it is integrated over time and cannot be
recomputed. `Time.deltaTime` (scene time) is right here, not unscaled: the sweep must pause with the scene.

### 2. `lidar_id_write.shader` — write range

```hlsl
struct v_in  { float4 pos : POSITION; };
struct v_out { float4 pos : SV_POSITION; float3 world : TEXCOORD0; };

o.world = TransformObjectToWorld(v.pos.xyz);

float2 frag(v_out i) : SV_Target {
    return float2(_LidarID, distance(i.world, _WorldSpaceCameraPos));   // G = range (m)
}
```

`_WorldSpaceCameraPos` is the face camera, which sits at the sensor origin (children at local zero, created
in code), so no new uniform. Background clears to 0 = range 0 with id 0, discarded downstream as today.

### 3. `lidar_reconstruct.compute` — direction → face → texel → range

```hlsl
Texture2D<float4> face0, face1, face2, face3, face4, face5; // R = id, G = range (m)
StructuredBuffer<float3> pattern;   // lidar-local unit direction per ray, azimuth-major
RWStructuredBuffer<pc_point> pc;
float4x4 face_vp[6];                // GPU view-projection of each face camera
float4x4 lidar_to_world;            // rotation only
float3 lidar_pos;
uint offset;                        // first ray of this dispatch (sector start)
uint count;                         // rays in this dispatch
uint res;

uint face_of(float3 d) {            // same tie rule as lidar.face_of
    float3 a = abs(d);
    return a.x >= a.y && a.x >= a.z ? (d.x >= 0 ? 0 : 1)
         : a.y >= a.z               ? (d.y >= 0 ? 2 : 3)
         :                            (d.z >= 0 ? 4 : 5);
}

float4 load(uint f, int2 px) {      // switch, not an array: texture resources cannot be dynamically indexed
    switch (f) { case 0: return face0.Load(int3(px, 0)); /* … */ default: return face5.Load(int3(px, 0)); }
}

[numthreads(64, 1, 1)]
void reconstruct(uint3 tid : SV_DispatchThreadID) {
    if (tid.x < count) {
        uint i = offset + tid.x;
        float3 d = pattern[i];
        uint f = face_of(d);
        float3 dw = mul((float3x3) lidar_to_world, d);
        float4 clip = mul(face_vp[f], float4(lidar_pos + dw, 1.0));
        float2 ndc = clip.xy / clip.w;                      // w > 0: d is in that face's forward half-space
        int2 px = min(int2((ndc * 0.5 + 0.5) * res), int2(res - 1, res - 1)); // clamp: ndc hits exactly ±1 on face edges
        float4 s = load(f, px);
        pc[i].world = lidar_pos + dw * s.g;
        pc[i].id = (uint) round(s.r);
    }
}
```

`inv_vp` is gone. The texel formula is the one in the current kernel, applied to a forward projection, so the
Y orientation is inherited from the path that already works.

### 4. `lidar_render_feature.cs` — key the id pass on face cameras

```csharp
var cam = data.cameraData.camera;
if (lidar_registry.devices.Any(d => d.faces.Contains(cam))) {
    renderer.EnqueuePass(id);
} else { /* unchanged */ }
```

No marker component: the registry already knows the devices. The point pass and all `depth_*` uniforms are
untouched; `lidar_pos` is already the device transform.

### 5. `lidar_frustum_viz.cs` — draw the scan band

`RequireComponent(Camera)` → `RequireComponent(lidar)`. Replace the near/far cube strip with one
`LineRenderer` strip at `frustum_length`: a latitude circle at `elevation_min`, one meridian up at azimuth 0,
a latitude circle at `elevation_max`. For JT128 the top circle is a small ring at the zenith and the whole
reads as a dome. `corner()` becomes `point(elevation, azimuth) =
transform.TransformPoint(dir * frustum_length)` using the same direction formula as `generate()`. Marker
unchanged.

### 6. `lidar_control_panel.cs` · `lidar_control_panel.uxml` — Pattern section

Replace the four pattern controls:

| Control | Element | Range | Target | Effect |
|---|---|---|---|---|
| `Channels` | SliderInt | 1–256 | `channel_count` | rebuild |
| `Azimuth steps` | SliderInt | 1–2048 | `azimuth_steps` | rebuild |
| `Elevation min` | Slider | -90–90 | `elevation_min` | rebuild |
| `Elevation max` | Slider | -90–90 | `elevation_max` | rebuild |
| `Az. stagger` | Slider | 0–1 (deg) | `channel_azimuth_offset` | rebuild |
| `Scan Hz` | Slider | 0–30 | `scan_hz` | direct |
| `Sweep` | Toggle | — | `sweep` | direct |

`rebuild_preview()` becomes an equirectangular plot of `generate()`: `x = atan2(d.x, d.z)` mapped `0..360°`
→ `0..w-1`, `y = asin(d.y)` mapped `-90..90°` → `0..h-1`. Keep the 256² texture; the plot is a diagram, the
aspect does not matter. At JT128 density the band fills solid, which is the honest picture; the stagger and
the elevation range stay readable.

Debug view keeps blitting `device.id_rt` (now the +Z face). In `lidar_id_debug.shader`, G is range in metres,
so the grayscale becomes `saturate(s.g / _Far) * 0.6` with a `_Far ("range mapped to white", Float) = 50`
property; far surfaces are now brighter. Drop the "reversed-Z" comment.

### 7. `lidar_diagnostics.cs` — comment only

`req.GetData<Vector2>()` and the `.x/.y` analysis still hold (`RGFloat`, +Z face via the `id_rt` alias).
Update the class comment: G is range, not NDC depth, and the readback covers one face.

### 8. `lidar_capture.cs` — no code change

With sweep on, one capture holds points acquired over up to `1 / scan_hz` seconds of frames. That is what a
real spinning scan looks like, and it is a README note rather than a code change.

### 9. `PLAN-point-normals.md` — mark for re-basing

Add one line to its Status: `id_rt` is now six per-face RTs and `inv_vp` no longer exists, so its edit 1 is
×6 and its edit 2 drops the `inv_vp` reference; everything else in it survives.

### 10. Documentation

- Root `README.md` / `README.ko.md`: "Why rasterization" (360° is no longer a counter-indication); pipeline
  pass 1 (a cube of 90° faces, 4–6 rasters) and pass 2 (direction → face → `Load` range); Limitations: replace
  "Single perspective FoV (<180°)" with the frame-granular-time note and the amortization argument above.
- `unity/README.md` / `README.ko.md`: §1 mapping table and pass-order diagram, scene wiring (no Camera on the
  lidar object; `scan_mask` must exclude `Viz`; `camera_depth` below the main camera), §3 scan pattern, §5
  Pattern controls table, §6/§7 lists; `map_resolution` is per face.
- `CLAUDE.md`: docs list already points here; update once the plan lands.

## Scene wiring (done locally, not in this change set)

- `lidar Camera` object: **remove the `Camera` component** (and its `UniversalAdditionalCameraData`). The
  `lidar` component stays and its transform is the sensor pose. A leftover Camera would render to the screen.
- Set `scan_mask` to the old culling mask (`m_Bits: 119` = Default, TransparentFX, Ignore Raycast, Water,
  UI, LidarOnly; `Viz` excluded).
- Set `camera_depth` below the main camera. **Both cameras are currently at depth -1** in `Main.unity`, so the
  current ordering is accidental; use -2 for the faces.
- `lidar_frustum_viz` stays on the same object (it now requires `lidar`, not `Camera`).
- Re-check `map_resolution` (512) and the pattern values; the serialized `ring_*` fields are dropped by Unity
  on load.

## Risks, in descending order of concern

1. **Camera count.** Six URP cameras each run their own opaque pass before the id override (today's design,
   ×6). At 512² id-only this should be cheap, but the per-camera fixed cost (culling, setup, RenderGraph
   compile) on WebGPU is the number to measure first. If it bites, the fallback is one camera re-rendered
   per face with `Camera.Render()` in `LateUpdate`, which keeps everything else in this plan.
2. **Sector seam.** The sweep boundary moves every frame; where the previous sector meets the new one, a
   moving object shows a step. This is the intended look, but at low frame rates (large sectors) it may read
   as tearing. `sweep = false` is the escape hatch.
3. **Face edges.** A direction exactly on a face edge (`ndc = ±1`) is clamped to the last texel. Two
   adjacent faces see the same surface from the same origin, so the range agrees; no visible seam expected,
   but verify with a wall across a diagonal.
4. **Stale faces.** A face disabled by `rebuild` keeps old content. By construction no ray selects it, and the
   C#/HLSL `face_of` tie rule is what guarantees that. Keep the two in sync.
5. **Depth colormap with sweep.** `distance(world, lidar_pos)` uses the current sensor position for points
   acquired up to one revolution ago. Only visible when the sensor moves fast; accepted.

## Explicitly out of scope

- **Per-point timestamps, `captureDeltaTime` capture mode, sub-stepping, velocity-buffer correction.** See
  the amortization table; that regime is a raycaster's.
- **Rotation interpolation within a frame.** Data accuracy, not visual.
- **Per-channel elevation tables.** The runtime panel cannot edit them; add when a datasheet import is
  wanted.
- **Partial-sector dispatch across the capture.** `lidar_capture` keeps its wall-clock throttle; per-
  revolution file boundaries belong with the timestamp work.
- **Dome (forward-axis) pattern mode.** Covered by `elevation_max = 90`.

## Verification

Unity cannot be compiled from the agent environment, so all of these are editor-side checks. Report honestly
which were and were not run.

- [ ] **A.** With `sweep = false`, JT128 defaults, a box around the sensor: points land on all six walls
      including the ceiling, no gaps or doubled surfaces along face diagonals (risk 3).
- [ ] **B.** Debug section → Analyze: Stage A reports integer ids on the +Z face; Stage B reports sane
      bounds (range reconstruction replaced `inv_vp`).
- [ ] **C.** `elevation_max = 30°`: the +Y face camera reports `enabled = false`; `elevation_max = 40°`:
      it is enabled (the 35.3° diagonal rule, risk 4).
- [ ] **D.** `sweep = true`, `scan_hz = 1`: a visible sector advances once around per second; moving a
      character shows a step at the sweep boundary and nothing elsewhere.
- [ ] **E.** Frame time at 60 fps before and after (risk 1), editor and WebGPU build.
- [ ] **F.** Recording with sweep on: a PLY opened in CloudCompare shows a moving object torn at the sweep
      boundary once per revolution, matching D.
