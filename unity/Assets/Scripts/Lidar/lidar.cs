using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using RTGLite;

namespace LiDARMimic {
    // One reconstructed LiDAR point: world position + originating object id (0 = background / non-receiver).
    public struct pc_point {
        public Vector3 world;
        public uint id;
    }

    // How the integration pass colors/sizes points: each object's own style, or a shared depth colormap.
    public enum point_render_mode { per_object, depth_map }

    // LiDAR device. Six 90° face cameras (children, created at runtime) render the scene from the sensor origin
    // into face_rt[i] (R = id, G = range in metres); a compute pass then reconstructs each scan ray (a lidar-local
    // direction) into a world-space point (pc_buffer). Also generates the scan pattern (spinning sensor:
    // channels × azimuth steps).
    // Scene wiring: put this on a plain GameObject (no Camera); its transform is the sensor pose. camera_depth
    // must be lower than the main camera's depth so the faces render first; exclude Viz from scan_mask.
    public class lidar : MonoBehaviour {
        [Header("Scan pattern")]
        [Tooltip("Elevation layers.")]
        public int channel_count = 128;
        [Tooltip("Columns per revolution (0.4° at 10 Hz for a JT128).")]
        public int azimuth_steps = 900;
        [Tooltip("Elevation (deg) of the first channel.")]
        public float elevation_min = -4.4f;
        [Tooltip("Elevation (deg) of the last channel.")]
        public float elevation_max = 90.5f;
        [Tooltip("Azimuth (deg) added per channel so columns don't align vertically.")]
        public float channel_azimuth_offset = 0f;

        [Header("Sweep")]
        [Tooltip("Motor revolutions per second.")]
        public float scan_hz = 10f;
        [Tooltip("On: each frame reconstructs only the azimuth sector swept since the last frame (other points persist). Off: the whole buffer every frame.")]
        public bool sweep = true;

        [Header("Faces")]
        [Tooltip("Layers the face cameras see. Exclude Viz so the sensor does not scan its own visualization.")]
        public LayerMask scan_mask = -1;
        public float near = 0.1f;
        public float far = 500f;
        [Tooltip("Camera depth of the face cameras; must be lower than the main camera's so they render first.")]
        public float camera_depth = -2f;

        [Header("Rendering")]
        [Tooltip("Global point style: per-object color/size, or a shared depth colormap. Switchable at runtime.")]
        public point_render_mode render_mode = point_render_mode.depth_map;

        [Tooltip("Per-face RT size (px). 512 = 0.18°/texel, finer than a JT128 ray spacing. Not runtime-changeable (needs RT recreation).")]
        public int map_resolution = 512;
        [Tooltip("Assign the lidar_reconstruct compute shader.")]
        public ComputeShader reconstruct_cs;

        public IReadOnlyList<Camera> faces => face_cams;
        public RenderTexture id_rt => face_rt[forward_face]; // +Z face: what the debug view and diagnostics look at
        public ComputeBuffer points => pc_buffer;
        public int point_count => azimuth_steps * channel_count;

        const int forward_face = 4;
        // CubemapFace order (+X, -X, +Y, -Y, +Z, -Z): local rotation of each face camera in the sensor frame.
        static readonly Vector3[] face_euler = { new(0, 90, 0), new(0, -90, 0), new(-90, 0, 0), new(90, 0, 0), Vector3.zero, new(0, 180, 0) };
        static readonly string[] face_names = { "+x", "-x", "+y", "-y", "+z", "-z" };

        Camera[] face_cams;
        RenderTexture[] face_rt;
        ComputeBuffer pattern_buffer;
        ComputeBuffer pc_buffer;
        float phase; // sweep position in revolutions [0,1); integrated over time, so genuinely state
        int faces_done; // face cameras finished this frame; dispatch once every enabled face is in

        void OnEnable() {
            if (Application.isPlaying) {
                face_rt = face_names.Select(n => {
                    var rt = new RenderTexture(map_resolution, map_resolution, 24, RenderTextureFormat.RGFloat) { name = $"lidar_face_{n}" };
                    rt.Create();
                    return rt;
                }).ToArray();
                face_cams = face_euler.Select((euler, i) => {
                    var go = new GameObject($"lidar_face_{face_names[i]}");
                    go.transform.SetParent(transform, false);
                    go.transform.localRotation = Quaternion.Euler(euler);
                    var c = go.AddComponent<Camera>();
                    c.fieldOfView = 90f;
                    c.aspect = 1f;
                    c.nearClipPlane = near;
                    c.farClipPlane = far;
                    c.cullingMask = scan_mask;
                    c.depth = camera_depth;
                    c.clearFlags = CameraClearFlags.SolidColor;
                    c.backgroundColor = Color.clear; // R = 0 => background id 0
                    c.targetTexture = face_rt[i];
                    return c;
                }).ToArray();
                // RTG's render feature draws its grid, background and gizmos on every camera it has not been told
                // about, after id_pass, alpha-blending into the id/range buffer. Opt the faces out.
                if (RTG.get != null) {
                    foreach (var c in face_cams) {
                        RTCamera.get.SetCameraRenderConfig(c, new RTCameraRenderConfig { renderFlags = ECameraRenderFlags.None });
                    }
                }

                pattern_buffer = new ComputeBuffer(point_count, sizeof(float) * 3);
                pc_buffer = new ComputeBuffer(point_count, Marshal.SizeOf<pc_point>());
                rebuild();

                RenderPipelineManager.endCameraRendering += on_end_camera;
                lidar_registry.register(this);
            }
        }

        void OnDisable() {
            if (face_cams != null) {
                RenderPipelineManager.endCameraRendering -= on_end_camera;
                lidar_registry.unregister(this);
                foreach (var c in face_cams) {
                    c.targetTexture = null;
                    Destroy(c.gameObject);
                }
                foreach (var rt in face_rt) {
                    rt.Release();
                }
                face_cams = null;
                face_rt = null;
                pattern_buffer.Release();
                pc_buffer.Release();
            }
        }

        void OnValidate() {
            channel_count = Mathf.Max(1, channel_count);
            azimuth_steps = Mathf.Max(1, azimuth_steps);
            elevation_max = Mathf.Max(elevation_min, elevation_max);
            if (Application.isPlaying && pattern_buffer != null) {
                rebuild();
            }
        }

        // Regenerate the scan pattern and re-upload; reallocate the buffers if the point count changed. The point
        // buffer is zeroed: with sweep on, rays not yet swept are drawn as-is, and a fresh ComputeBuffer is undefined.
        // Face cameras no ray selects are disabled (their stale RT is never read).
        public void rebuild() {
            var pattern = generate();
            if (pattern_buffer.count != pattern.Length) {
                var next_pattern = new ComputeBuffer(pattern.Length, sizeof(float) * 3);
                var next_pc = new ComputeBuffer(pattern.Length, Marshal.SizeOf<pc_point>());
                pattern_buffer.Release();
                pc_buffer.Release();
                pattern_buffer = next_pattern;
                pc_buffer = next_pc;
            }
            pattern_buffer.SetData(pattern);
            pc_buffer.SetData(new pc_point[pattern.Length]);
            var needed = pattern.Select(face_of).Distinct().ToArray();
            for (var i = 0; i < face_cams.Length; i++) {
                face_cams[i].enabled = needed.Contains(i);
            }
        }

        // Camera order among equal depths is not guaranteed, so count the faces rendered this frame instead of
        // waiting for a particular one.
        void on_end_camera(ScriptableRenderContext ctx, Camera c) {
            if (face_cams.Contains(c)) {
                faces_done++;
                if (faces_done == face_cams.Count(f => f.enabled)) {
                    faces_done = 0;
                    reconstruct();
                }
            }
        }

        // Reconstruct the columns the motor swept this frame (all of them with sweep off). The pattern is
        // azimuth-major, so a sector is a contiguous ray range; a wrap past 360° is two ranges.
        void reconstruct() {
            var k = reconstruct_cs.FindKernel("reconstruct");
            for (var i = 0; i < face_cams.Length; i++) {
                reconstruct_cs.SetTexture(k, $"face{i}", face_rt[i]);
            }
            reconstruct_cs.SetMatrixArray("face_vp", face_cams.Select(f => GL.GetGPUProjectionMatrix(f.projectionMatrix, false) * f.worldToCameraMatrix).ToArray());
            reconstruct_cs.SetMatrix("lidar_to_world", Matrix4x4.Rotate(transform.rotation));
            reconstruct_cs.SetVector("lidar_pos", transform.position);
            reconstruct_cs.SetBuffer(k, "pattern", pattern_buffer);
            reconstruct_cs.SetBuffer(k, "pc", pc_buffer);
            reconstruct_cs.SetInt("res", map_resolution);

            void run(int from, int to) { // columns [from, to)
                if (to > from) {
                    var count = (to - from) * channel_count;
                    reconstruct_cs.SetInt("offset", from * channel_count);
                    reconstruct_cs.SetInt("count", count);
                    reconstruct_cs.Dispatch(k, Mathf.CeilToInt(count / 64f), 1, 1);
                }
            }

            var advance = sweep ? Time.deltaTime * scan_hz : 1f; // revolutions this frame (scene time: pauses with the scene)
            var c0 = Mathf.FloorToInt(phase * azimuth_steps);
            var c1 = Mathf.FloorToInt((phase + advance) * azimuth_steps);
            if (c1 - c0 >= azimuth_steps) {
                run(0, azimuth_steps);
            } else if (c1 > azimuth_steps) {
                run(c0, azimuth_steps);
                run(0, c1 - azimuth_steps);
            } else {
                run(c0, c1);
            }
            phase = Mathf.Repeat(phase + advance, 1f);
        }

        // Lidar-local unit direction (+Z forward, +Y up; azimuth clockwise from +Z), angles in degrees.
        public static Vector3 direction(float elevation, float azimuth) {
            var e = elevation * Mathf.Deg2Rad;
            var a = azimuth * Mathf.Deg2Rad;
            var ce = Mathf.Cos(e);
            return new Vector3(ce * Mathf.Sin(a), Mathf.Sin(e), ce * Mathf.Cos(a));
        }

        // Face whose 90° frustum contains d (dominant axis). Ties resolve x >= y >= z and >= 0 picks the positive
        // face; the HLSL copy in lidar_reconstruct.compute must keep the same rule, or a face disabled here could
        // be read there.
        static int face_of(Vector3 d) {
            var a = new Vector3(Mathf.Abs(d.x), Mathf.Abs(d.y), Mathf.Abs(d.z));
            return a.x >= a.y && a.x >= a.z ? (d.x >= 0 ? 0 : 1)
                 : a.y >= a.z ? (d.y >= 0 ? 2 : 3)
                 : (d.z >= 0 ? 4 : 5);
        }

        // Per-ray lidar-local direction, azimuth-major (index = azimuth * channel_count + channel). Channels are
        // equally spaced from elevation_min to elevation_max inclusive; each channel may be staggered in azimuth.
        // Length is always exactly point_count (= azimuth_steps * channel_count), which the buffers and dispatch
        // rely on.
        public Vector3[] generate() {
            Vector3 ray(int a, int c) {
                var t = channel_count > 1 ? c / (float) (channel_count - 1) : 0f;
                return direction(Mathf.Lerp(elevation_min, elevation_max, t), a * 360f / azimuth_steps + c * channel_azimuth_offset);
            }

            return Enumerable.Range(0, azimuth_steps)
                .SelectMany(a => Enumerable.Range(0, channel_count).Select(c => ray(a, c)))
                .ToArray();
        }
    }
}
