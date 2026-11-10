# LiDAR-mimic — Unity implementation

**English** · [한국어](README.ko.md)

🔗 **Live demo:** [wonhotoss.github.io/LiDAR-mimic/unity](https://wonhotoss.github.io/LiDAR-mimic/unity/) — needs a WebGPU-capable browser (recent Chrome/Edge).

This is a **Unity 6 / URP 17 / RenderGraph** implementation of the platform-independent idea in the root
[README.md](../README.md). This document covers (1) how the idea maps to Unity features, and (2) what can be
controlled at runtime. (Scene wiring is part of the code/components, so it is described via each field's
inspector help and code comments.)

- Environment: **Unity 6000.4.10**, **URP 17.4.0**, RenderGraph API, Input System package.
- Tested on: **Windows + desktop Chrome (WebGPU)**.
- URP settings: RequireDepthTexture / RequireOpaqueTexture on, DepthPriming off, MSAA off, Forward+.
- Code: [Assets/Scripts/Lidar/](Assets/Scripts/Lidar/), UI: [Assets/UI/](Assets/UI/).

---

## 1. Idea → Unity mapping

| Concept (root README) | Unity implementation |
|---|---|
| Sensor = a point with six 90° faces | A plain GameObject with the `lidar` component (its transform is the sensor pose). At runtime it creates six child face Cameras (`CubemapFace` order), each rendering into its own `RenderTexture` (`face_rt[i]`). Faces no ray selects are disabled. |
| Sensor-view pass (range+ID prepass) | The `id_pass` in `lidar_render_feature`, run on every face camera. Draws the scene with the override material `lidar/id_write`, writing R=ID, G=range (m). |
| Offscreen buffer | `face_rt[0..5]` — **RGFloat** RenderTextures (R=ID as float, G=range). Independent of screen resolution (`map_resolution` per face, default 512²). `id_rt` is the +Z face (debug view, diagnostics). |
| Reconstruction pass (compute) | `lidar_reconstruct.compute`. Dispatched once every enabled face has rendered (counted in `RenderPipelineManager.endCameraRendering`). With `sweep` on, only the azimuth sector swept this frame. |
| Point buffer | `ComputeBuffer<pc_point>` (`pc_point = { float3 world; uint id }`). No readback. |
| Integration render pass | The `point_pass` in `lidar_render_feature`. Splats via `DrawProcedural` (6×N triangles) on the main camera. Shader `lidar/point`. |
| Object ID | Assigned if the `lidar_receiver` component is present. `lidar_receiver_registry` auto-issues a small sequential integer ≥ 1 on enable. ID 0 = background / non-receiver. |

### Pass order

A single `ScriptableRendererFeature` (`lidar_render_feature`) branches by camera.

```mermaid
flowchart TD
    subgraph LC["LiDAR face cameras ×6 (lower depth → render first)"]
        L1["id_pass<br/>draws all opaque with lidar/id_write<br/>→ face_rt[i] (R=ID, G=range)"]
        L2["compute reconstruct<br/>dispatched after the last face<br/>per ray: dir → face → Load → origin + dir×range"]
        L1 --> L2
    end
    subgraph MC["Main camera (higher depth → renders later)"]
        M1["main opaque<br/>ordinary objects only (LidarOnly layer excluded)"]
        M2["point_pass<br/>DrawProcedural 6×N → fixed screen-size splats<br/>directly into the main color+depth attachment"]
        M1 --> M2
    end
    L2 -->|"pc buffer (ComputeBuffer, GPU)"| M2
    L2 -.->|"optional: lidar_capture async readback"| CAP["PLY sequence"]
```

1. **LiDAR face cameras** — each enqueues only `id_pass`. At `AfterRenderingOpaques` it draws all scene
   opaque with the override material into that face's `face_rt`, writing ID+range. `lidar/id_write` uses
   `ZWrite Off / ZTest LEqual`, overlaying on the depth the camera's opaque pass already wrote so only the
   nearest surface's ID remains. The ID is read from the renderer's `MaterialPropertyBlock` (`_LidarID`);
   the range is `distance(world, _WorldSpaceCameraPos)`, the face camera sitting at the sensor origin.
2. **Reconstruction (compute)** — dispatched outside RenderGraph once the last enabled face has rendered.
   Per ray (a lidar-local direction), it picks the face by dominant axis, projects the direction with that
   face's `GL.GetGPUProjectionMatrix(proj, false) * worldToCamera` to a texel, reads ID·range via `Load`
   (point sample) and writes `lidar_pos + dir × range` into the `pc` buffer. The C# and HLSL `face_of`
   share one tie rule so a face disabled on the CPU is never read on the GPU.
3. **Main camera opaque** — URP default. Ordinary objects only (a layer filter excludes pc-only objects).
4. **Integration draw** — `AfterRenderingOpaques`. Reads the `pc` buffer and splats fixed screen-size,
   axis-aligned squares directly into the main color+depth attachment. Depth read/write on → occlusion
   handled by the hardware depth test.

The execution order of the face and main cameras is guaranteed by the **camera `depth` value**
(`lidar.camera_depth` < main camera depth). `scan_mask` is the faces' culling mask; exclude `Viz` so the
sensor does not scan its own marker.

---

## 2. Object modes

To make an object a point-cloud source, add the
[Assets/Scripts/Lidar/lidar_receiver.cs](Assets/Scripts/Lidar/lidar_receiver.cs) component. Objects without it
are normal-only (ordinary render), treated as ID-0 occluders in the LiDAR pass.

`receiver_mode` has three values, each implemented as a **(layer, `_LidarID`)** combination — it only moves
the object's layer and sets the MPB value, without re-wiring the scene/prefab/camera.

| Mode | Layer | `_LidarID` | Main view | Result |
|---|---|---|---|---|
| `pc_only` | `LidarOnly` | id | hidden (excluded from cullingMask) | shown **as points only** |
| `both` | original layer | id | solid | solid + points together (overlap eased by depth bias) |
| `solid` | original layer | 0 | solid | ordinary render only, occludes but **no points** |

- **Active toggle** — `lidar_receiver.active` toggles `Renderer.enabled` (not `GameObject.SetActive`).
  Turning it off removes the object from every camera so solid·occlusion·points all disappear, but registry
  registration and mode state are kept, so turning it back on restores everything.
- Every receiver starts as `pc_only`.

---

## 3. Scan pattern

`generate()` in [Assets/Scripts/Lidar/lidar.cs](Assets/Scripts/Lidar/lidar.cs) builds the array of ray
directions (lidar-local unit vectors). It is a **spinning-sensor pattern**: `channel_count` elevation layers
equally spaced from `elevation_min` to `elevation_max`, each fired at `azimuth_steps` columns around 360°,
optionally staggered per channel by `channel_azimuth_offset`. The defaults are a Hesai JT128
(128 × 900, -4.4° … 90.5°). The array is azimuth-major (`index = azimuth × channel_count + channel`) so a
sweep sector is a contiguous range, and its length always equals exactly `azimuth_steps × channel_count`
(buffer allocation and dispatch depend on this). Density is deliberately **not** uniform: channels are rings
of constant elevation, which is what a spinning scan looks like.

**Sweep.** With `sweep` on, each frame reconstructs only the columns the motor advanced
(`scan_hz × Time.deltaTime` revolutions); the rest of the point buffer persists, so a revolution builds up
over several frames. Off, the whole buffer is rebuilt every frame.

`generate()` is the **single source**: both the GPU buffer used for the actual scan and the UI's pattern
preview image are drawn from the same array.

When a pattern parameter changes, `lidar.rebuild()` refills the ray buffer (reallocating if the point count
changed) and refreshes the preview. During editor play, `OnValidate` is called automatically. Parameters are
**clamped to ≥1** in `OnValidate` (to avoid zero/negative-length buffer exceptions).

---

## 4. Point render modes

The global render mode is one of two `point_render_mode` values, switchable at runtime (`lidar.render_mode`,
default `depth_map`).

- **`per_object`** — draws with each object's (ID's) color and size. Colors/sizes are gathered from each
  `lidar_receiver` into a per-ID style buffer and passed to the shader.
- **`depth_map`** — applies a colormap by distance from the sensor. Point size is a global constant. In the
  vertex shader, `distance(world, lidar_pos)` is normalized to `[depth_min, depth_max]`, offset by a
  time-driven phase, and used to sample the colormap texture: `frac(t + depth_offset)`.

The `depth_map` globals are edited **on the render feature (`lidar_render_feature` on `PC_Renderer`),
editor-only** (the runtime UI only has the mode toggle):

| Field | Default | Effect |
|---|---|---|
| `global_point_size` | 4 | point size (px) in depth_map mode |
| `depth_colormap` | jet-like (blue → red → back to blue) | distance→color gradient. Sampled cyclically, so end it on the start color |
| `depth_min` / `depth_max` | 0 / 50 | distances (m) mapped to the colormap ends |
| `depth_emission` | 1 | multiplier on the colormap color; >1 feeds Bloom (glow) |
| `depth_scroll_speed` | 0.1 | colormap scroll in cycles/sec (negative reverses); 0 = static |
| `depth_bias` | 0.0002 | small clip-z nudge toward the camera (eases coplanar z-fighting in `both` mode). **Sign is platform-dependent** — flip it if points are hidden by their own surface |

The colormap is baked into a 256×1 lookup texture and sampled by the shader. The bake is periodic (texel `i`
= gradient at `i/256`, `Repeat` wrap), so the ramp filters seamlessly across the wrap point as it scrolls.

---

## 5. Runtime control panel

All controls are consolidated into one runtime UI Toolkit panel, so the full feature set works **even in a
standalone build**.
([Assets/Scripts/Lidar/lidar_control_panel.cs](Assets/Scripts/Lidar/lidar_control_panel.cs),
[Assets/UI/lidar_control_panel.uxml](Assets/UI/lidar_control_panel.uxml) / `.uss`)

Foldout section order: **Point Rendering → Receivers → Pattern → Recording → Debug.**

### Point Rendering
- `per-object` / `depth-map` buttons — switch the global point render mode. The current mode is highlighted.

### Receivers
- **All** row — switch every receiver at once to `pc-only` / `both` / `solid`.
- **Per-object row** — object name + an on/off active button (green when on; mode buttons disable when off) +
  `pc-only`/`both`/`solid` mode buttons.
- **Hovering a row** overlays a name marker in 3D at that object's position (hidden when behind the camera).
  The marker never intercepts scene clicks.
- **Only in per-object mode**, each row shows a color editor: a color swatch + an `Emission` slider (1–8).
  Clicking the swatch opens an R/G/B popup picker. The color is `base RGB × Emission` (HDR); >1 glows via
  Bloom. (UI Toolkit's `ColorField` is editor-only, so runtime composes the HDR color from swatch + sliders.)

### Pattern
Live-bound to the `lidar` device. A value change → `rebuild()` → preview refresh.

| Control | Range | Target |
|---|---|---|
| `Channels` | 1–256 | `channel_count` — elevation layers |
| `Azimuth steps` | 1–2048 | `azimuth_steps` — columns per revolution (total = Channels × this) |
| `Elevation min` / `max` | -90–90 | `elevation_min` / `elevation_max` — scan band (deg); min is kept ≤ max |
| `Az. stagger` | 0–1 | `channel_azimuth_offset` — degrees added per channel so columns don't align vertically |
| `Scan Hz` | 0–30 | `scan_hz` — motor revolutions per second (no rebuild) |
| `Sweep` | — | `sweep` — per-frame sector update on/off (no rebuild) |
| `pattern_preview` | — | equirectangular plot (x = azimuth 0–360°, y = elevation -90–90°), white points on black |

### Recording
Snapshots the point buffer via **async GPU readback** and saves one **binary PLY** (x/y/z float + id uint)
per frame.
([Assets/Scripts/Lidar/lidar_capture.cs](Assets/Scripts/Lidar/lidar_capture.cs)) Capture is throttled by
wall-clock time so it does not affect app fps.

| Control | Target | Effect |
|---|---|---|
| `Prefix` | `prefix` | output filename prefix (`{prefix}_{000000}.ply`) |
| `Capture FPS` | `capture_fps` | max captures per second (≤0 = every rendered frame) |
| `Drop id==0` | `filter` | drop no-hit/background points (off = raw dump for offline filtering) |
| `OpenGL coords` | `opengl` | on: flip z sign to save right-handed (OpenGL); off: raw Unity world coords |
| `Output dir` | `output_dir` | output folder (empty = `Application.persistentDataPath`) |
| `Browse…` | — | native folder-picker dialog (UnityStandaloneFileBrowser) |
| `Start/Stop Recording` | `recording` | toggle capture |
| status | — | recording state + number of captured frames |

### Debug (LiDAR view)
Shows a texture that blits the LiDAR `id_rt` (the +Z face) through the `lidar/id_debug` material (to inspect
the sensor-view ID/range; `_Far` on the material is the range mapped to white).

---

## 6. What updates in real time

- **Sensor move/rotate** — reflected instantly via the face camera matrices each frame.
- **Object movement / bone animation** — instant, because the sensor-view pass and reconstruction rerun every
  frame (with `sweep` on, within the swept sector).
- **Scan pattern change** — ray buffer regenerated via `rebuild()`; the point buffer is cleared and the
  needed faces re-enabled.
- **Scan rate / sweep** — take effect on the next frame.
- **Solid↔points toggle** — instant, decided at integration-draw time (no recomputation).
- **Object color/size** — the per-ID style buffer is refreshed every frame.

Only `map_resolution` (per-face RT resolution) and the face camera settings (`scan_mask`, `near`, `far`,
`camera_depth`) are not runtime-changeable (they are applied when the faces are created).

---

## 7. Things you can tell at a glance (summary)

Visual elements you can recognize by looking:

- **Point color** — per-object mode: each object's assigned color (HDR, glows as Emission↑). depth-map mode:
  distance colormap (default near=blue → far=red).
- **Point size** — fixed screen-size squares regardless of camera distance. Per-object in per-object mode,
  global in depth-map mode.
- **Pattern preview** — the black-background / white-point equirectangular plot in the Pattern section shows
  the scan band; at sensor-class density it fills solid.
- **Hover marker** — hovering a Receivers row shows a yellow-bordered name box on that object.
- **Debug view** — the +Z face's ID/range map.

---

## Related documents

- [../README.md](../README.md) — the platform-independent core idea.
