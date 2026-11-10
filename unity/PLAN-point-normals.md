# Plan · surface normals through the point pipeline

**Status:** not started. Written 2026-08-22. Delete this file once the change lands and `unity/README.md` is
updated.

**Re-base note (2026-11-10):** the cubemap sensor landed (`PLAN-cubemap-sensor.md`). `id_rt` is now six per-face
RTs (`face_rt[0..5]`, `id_rt` aliases the +Z face), so edit 1 applies to each face, and `inv_vp` no longer exists
(reconstruction is `lidar_pos + dir × range`), so edit 2 only adds the `oct_decode` line to the existing single
`Load`. Everything else below still holds.

Carry a per-point surface normal from the sensor-view raster through to the splat and the PLY export, then
use it to shade `per_object` mode from the sensor's own viewpoint. Five ordered edits, one breaking
dependency, `depth_map` mode untouched.

## Why

Splats are currently flat unlit colors, so a point cloud on a curved surface reads as a colored blob rather
than a surface. A normal fixes that, and it is cheap here because the hard part already exists: the
sensor-view raster already writes per-pixel data, and `NORMAL` is already in every vertex stream — including
skinned ones, so bone animation stays free exactly as it is for depth.

The per-ray critical path stays at exactly one `Load`, and nothing comes back to the CPU on the render path.
The README's `O(1)`-per-ray and readback-free claims both survive unchanged.

## Settled decisions

These were argued through in design; do not re-litigate them without a new reason.

| Decision | Choice | Why |
|---|---|---|
| Normal source | Written in the raster pass | Depth-derived normals quantize to `map_resolution` and go faceted on curved surfaces. The raster pass gives interpolated geometric normals for free. |
| Light position | **Sensor-locked**, no separate directional light | The direction is derivable from data already in the buffer, so a `sun` reference would be new authored state that can diverge from the scene. `per_object` mode stays coherent as "identity × sensor physics" only without a third semantic injected. |
| `depth_map` mode | **Unchanged** | The colormap is a data encoding that already owns the brightness axis. Multiplying it by incidence would make a dark red point ambiguous between "far" and "facing away". |
| Rim term | Included, `rim_strength` defaults to **0** | It is the only insurance if the sensor ends up near the camera, but its effect on the large ground plane is not predictable from the code. |
| PLY | Gains `nx/ny/nz` | Matches what CloudCompare and Open3D expect, so downstream normal-dependent work needs no estimation step. |

## Layout

`id_rt` widens from two float channels to four. Octahedral encoding fits a unit normal into the two new
channels, which keeps this to a single render target — no MRT plumbing in the RenderGraph pass.

| Channel | Holds | Status |
|---|---|---|
| R | per-object id | unchanged |
| G | NDC depth | unchanged |
| B, A | octahedral-encoded world normal | **new** |

`pc_point` appends the normal *after* `id`, so the existing 16-byte prefix stays byte-identical and every
current reader of `.world` / `.id` keeps working:

```
offset  0              12      16                    28
        [ float3 world ][uint id][   float3 normal   ]
        |<-- unchanged prefix -->|<---- appended ---->|
```

28 bytes, not padded to 32: a `ComputeBuffer` stride only has to be a multiple of 4, and
`float3 / uint / float3` is the standard tightly-packed Unity pattern. Padding for a clean second 16-byte row
would cost 4 bytes per point for no correctness gain. Both allocations in `lidar.cs` already call
`Marshal.SizeOf<pc_point>()`, so they pick the new size up with no edit.

## Edits, in pipeline order

### 1. Widen the sensor buffer and write the normal

`Assets/Scripts/Lidar/lidar.cs:59` · `Assets/Scripts/Lidar/Shaders/lidar_id_write.shader`

`RenderTextureFormat.RGFloat` -> `ARGBFloat`. At the default 1024² that is 8.4 MB -> 16.8 MB. The render
feature needs no edit: the pass already targets `resources.activeColorTexture`, which is the camera's
`id_rt`.

```hlsl
// lidar_id_write.shader — vertex input gains NORMAL, frag returns float4
struct v_in  { float4 pos : POSITION; float3 normal : NORMAL; };
struct v_out { float4 pos : SV_POSITION; float3 n : TEXCOORD0; };

o.n = TransformObjectToWorldNormal(v.normal);

float4 frag(v_out i) : SV_Target {
    // renormalize: interpolation shortens the normal across the triangle
    return float4(_LidarID, i.pos.z, oct_encode(normalize(i.n)));
}
```

The camera clears to `Color.clear`, so background texels oct-decode to `+Z`. Harmless — `id == 0` points are
discarded downstream anyway.

### 2. Decode into the point buffer

`Assets/Scripts/Lidar/lidar_reconstruct.compute`

The struct mirror gains `normal`, and the existing single `Load` now yields all four channels, so the
per-ray cost does not change at all.

```hlsl
struct pc_point { float3 world; uint id; float3 normal; };

float4 s = id_map.Load(int3(px, 0));   // unchanged: still one Load
pc[i].normal = oct_decode(s.zw);
```

URP ships `PackNormalOctQuadEncode` in `Packing.hlsl`, but this `.compute` is standalone and pulling URP's
shader library into it to get one function is a poor trade. Hand-roll the four-line pair instead and keep
encode and decode adjacent as a matched set — mixing a built-in encode with a hand-rolled decode is the
fragile option.

### 3. Mirror the struct on the C# side

`Assets/Scripts/Lidar/lidar.cs:8`

```csharp
public struct pc_point {
    public Vector3 world;
    public uint id;
    public Vector3 normal;   // appended — field order must match the HLSL struct
}
```

`lidar_debug_points.cs` needs no change; it only touches `.world` and `.id`, so the struct grows
transparently underneath it.

### 4. Shade per_object mode from the sensor

`Assets/Scripts/Lidar/Shaders/lidar_point.shader` · `Assets/Scripts/Lidar/lidar_render_feature.cs`

Only the `else` branch changes. `lidar_pos` is already bound unconditionally, and `_WorldSpaceCameraPos`
comes free with the `Core.hlsl` the shader already includes — so the rim term adds no new uniform beyond its
two tuning values.

```hlsl
// abs() makes the -ray negation redundant, and survives two-sided assets
float incidence = abs(dot(p.normal, normalize(p.world - lidar_pos)));
float shade = lerp(shade_floor, 1.0, incidence);

float3 v = normalize(_WorldSpaceCameraPos - p.world);
float rim = pow(1.0 - abs(dot(p.normal, v)), rim_power);

// tinted, not white: keeps the per-object hue that this mode exists to encode
color = style[p.id].xyz * (shade + rim * rim_strength);
```

New editor-tunable globals on the feature, set next to the existing `depth_*` block: `shade_floor` (0.35),
`rim_strength` (0), `rim_power` (4).

Two deliberate omissions:

- **No `1/d²` falloff.** Over the 50 m default range that is a 2500× dynamic range and everything past a few
  meters goes black. Real sensors range-normalize in firmware for exactly this reason.
- **`shade_floor` exists** because a point cloud's information content *is* the point positions.
  `saturate(n·l)` would turn half of them black, which loses data rather than shading it.

### 5. Export normals in the PLY

`Assets/Scripts/Lidar/lidar_capture.cs:51,75`

Header gains three properties after `z` and before `id`, following the PLY convention of position -> normals
-> custom scalars.

```
property float nx
property float ny
property float nz
```

**Easy to miss:** `on_readback` flips `p.world.z` for the OpenGL right-handed export. The normal's `z` must
flip *identically* — a handedness conversion applies to every vector in the frame, not just position. Miss
it and the exported normals are silently wrong in the flipped frame, which downstream tools will happily
accept.

This changes the byte layout of exported files, but captures are per-run outputs rather than a persisted
format, so there is nothing to migrate.

## The one breaking dependency

`Assets/Scripts/Lidar/lidar_diagnostics.cs:44` reads `id_rt` back as `req.GetData<Vector2>()`. Widening the
format to `ARGBFloat` makes that read the wrong stride, so it must become `Vector4`. The `.x` / `.y`
indexing below it is unaffected, and the change is type-only — which matters, because that file is
deliberately written without LINQ or capturing lambdas to stay IL2CPP/WASM-safe. The class comment at line
36 documents the format as `RGFloat` and needs updating with it.

Two documentation sites describe the buffer as `RGFloat` as well: `unity/README.md` and the project memory
note. Both should be corrected in the same change so the docs do not drift.

## Risks, in descending order of concern

1. **WebGPU.** The hosted demo runs on WebGPU, where float render targets are the classic gotcha.
   `rgba32float` is renderable in core WebGPU — blending is the feature-gated part, and this pass does not
   blend — so it should hold, but that has to be verified in the actual build rather than assumed from the
   spec.
2. **Ground.** `Ground` is a receiver (fileID 2000 in `Main.unity`), and rim is a grazing-angle detector
   rather than a silhouette detector. A horizontal plane seen from a low camera is grazing across most of the
   frame, so the far ground brightens as a wide band instead of an outline. This may read as atmospheric
   depth or as a bug; it is the one outcome not predictable from the code, which is why `rim_strength`
   starts at 0.
3. **Shimmer.** The README already documents temporal shimmer from sensor-space fixed rays. It gets worse:
   brightness now wobbles along with position, not just position.
4. **Density vs rim band.** The rim band's screen width scales inversely with curvature. On small
   high-curvature receivers like `ball` the band can be narrower than the point spacing, so the rim reads as
   a few scattered bright dots rather than an outline.
5. **Normal maps.** Geometric normals only — the override material cannot reach each object's normal map.
   Arguably correct for a LiDAR (a beam measures macro geometry), but not what someone expecting
   normal-mapped detail in the shading would predict.
6. **Silhouettes.** Flying pixels at object boundaries now inherit a wrong normal alongside their wrong
   position. No worse than today, but the normal does not fix them.

## Explicitly out of scope

- **Dropout.** Discarding points below a return threshold is the most authentic-looking effect available
  here, but it is a different axis: it changes which points *exist* rather than how they look, and hiding
  points by grazing angle works against the mode whose job is showing which object is which. Separate
  change, orthogonal to `render_mode`.
- **Scene lighting.** No URP light loop, no shadow-map declarations in the pass builder. Densely lit points
  read as a dithered solid, and shadows at splat scale read as noise.
- **Per-object rim opt-out.** Would need the `style` buffer widened — its `float4` is fully spent on rgb +
  size. Global first; revisit only if the ground result demands it.
- **Specular.** A highlight on a 4px flat screen-aligned splat is an aliasing generator.
- **PLY intensity.** Shading stays presentation, not data. A consumer can recompute incidence from the
  normal — though only given the sensor position, which the PLY does not carry.

## Open question

A `comment sensor x y z` header line would let a consumer recompute incidence, closing the gap left by
omitting a PLY intensity field. It needs the sensor position captured at *request* time and threaded through
the closure alongside `idx`, because the readback is async and by callback time the sensor may have moved.
Cheap, but not yet decided — ask before folding it in.

## Verification

Unity cannot be compiled from the agent environment, so all of these are editor-side checks on the user's
machine. Report honestly which were and were not run.

- [ ] **A.** Run the control panel's Debug section and confirm the Stage A verdict is still clean after the
      `Vector4` change. That readback is the canary for the format switch.
- [ ] **B.** Sweep `rim_strength` up from 0 and watch `Ground` against the characters. This is the decision
      that cannot be made from the code.
- [ ] **C.** Rebuild the WebGPU demo and confirm the widened float target survives the browser, not just the
      editor.
- [ ] **D.** Open an exported PLY in CloudCompare or Open3D and check the normals point outward in the
      *flipped* frame — that is where the handedness bug would hide.
