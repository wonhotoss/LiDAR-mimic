using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiDARMimic {
    // One reconstructed LiDAR point: world position + originating object id (0 = background / non-receiver).
    public struct pc_point {
        public Vector3 world;
        public uint id;
    }

    // How the integration pass colors/sizes points: each object's own style, or a shared depth colormap.
    public enum point_render_mode { per_object, depth_map }

    // LiDAR device. Renders the scene from its viewpoint into id_rt (R = id, G = NDC depth); a compute pass
    // then reconstructs each scan ray into a world-space point (pc_buffer). Also generates the scan pattern.
    // Scene wiring: put this on a Camera whose depth is LOWER than the main camera's, so it renders first.
    // The camera's target texture and clear are configured in code (OnEnable); assign reconstruct_cs below.
    [RequireComponent(typeof(Camera))]
    public class lidar : MonoBehaviour {
        [Header("Scan pattern")]
        [Tooltip("Number of concentric rings in the scan pattern.")]
        public int ring_count = 32;
        [Tooltip("Average points per ring; total point count = ring_count * points_per_ring.")]
        public int points_per_ring = 64;
        [Tooltip("Max NDC radius of the outermost ring (<= 1).")]
        public float radius = 0.95f;
        [Tooltip("Radians added per successive ring so rings don't align radially.")]
        public float ring_angle_offset = 0.1f;

        [Header("Rendering")]
        [Tooltip("Global point style: per-object color/size, or a shared depth colormap. Switchable at runtime.")]
        public point_render_mode render_mode = point_render_mode.depth_map;

        [Tooltip("id_rt size (px). Decoupled from screen resolution; not runtime-changeable (needs id_rt recreation).")]
        public int map_resolution = 1024;
        [Tooltip("Assign the lidar_reconstruct compute shader.")]
        public ComputeShader reconstruct_cs;

        public RenderTexture id_rt { get; private set; }
        public ComputeBuffer points => pc_buffer;
        public int point_count => ring_count * points_per_ring;

        ComputeBuffer pattern_buffer;
        ComputeBuffer pc_buffer;

        Camera cam => GetComponent<Camera>();

        void OnEnable() {
            if (Application.isPlaying) {
                id_rt = new RenderTexture(map_resolution, map_resolution, 24, RenderTextureFormat.RGFloat) { name = "lidar_map" };
                id_rt.Create();

                var c = cam;
                c.targetTexture = id_rt;
                c.clearFlags = CameraClearFlags.SolidColor;
                c.backgroundColor = Color.clear; // R = 0 => background id 0

                pattern_buffer = new ComputeBuffer(point_count, sizeof(float) * 2);
                pc_buffer = new ComputeBuffer(point_count, Marshal.SizeOf<pc_point>());
                rebuild();

                RenderPipelineManager.endCameraRendering += on_end_camera;
                lidar_registry.register(this);
            }
        }

        void OnDisable() {
            if (id_rt != null) {
                RenderPipelineManager.endCameraRendering -= on_end_camera;
                lidar_registry.unregister(this);
                cam.targetTexture = null;
                id_rt.Release();
                id_rt = null;
                pattern_buffer.Release();
                pc_buffer.Release();
            }
        }

        void OnValidate() {
            ring_count = Mathf.Max(1, ring_count);
            points_per_ring = Mathf.Max(1, points_per_ring);
            if (Application.isPlaying && pattern_buffer != null) {
                rebuild();
            }
        }

        // Regenerate the scan pattern and re-upload; reallocate the buffers if the point count changed.
        public void rebuild() {
            var pattern = generate();
            if (pattern_buffer.count != pattern.Length) {
                var next_pattern = new ComputeBuffer(pattern.Length, sizeof(float) * 2);
                var next_pc = new ComputeBuffer(pattern.Length, Marshal.SizeOf<pc_point>());
                pattern_buffer.Release();
                pc_buffer.Release();
                pattern_buffer = next_pattern;
                pc_buffer = next_pc;
            }
            pattern_buffer.SetData(pattern);
        }

        // Reconstruct the point cloud right after the LiDAR camera has filled id_rt.
        void on_end_camera(ScriptableRenderContext ctx, Camera c) {
            if (c == cam) {
                var vp = GL.GetGPUProjectionMatrix(cam.projectionMatrix, false) * cam.worldToCameraMatrix;
                var k = reconstruct_cs.FindKernel("reconstruct");
                reconstruct_cs.SetTexture(k, "id_map", id_rt);
                reconstruct_cs.SetBuffer(k, "pattern", pattern_buffer);
                reconstruct_cs.SetBuffer(k, "pc", pc_buffer);
                reconstruct_cs.SetMatrix("inv_vp", vp.inverse);
                reconstruct_cs.SetInt("count", point_count);
                reconstruct_cs.SetInt("res", map_resolution);
                reconstruct_cs.Dispatch(k, Mathf.CeilToInt(point_count / 64f), 1, 1);
            }
        }

        // Per-ray projXY in LiDAR NDC ([-1,1]^2). Concentric, equally-spaced rings with a per-ring angular offset.
        // Each ring's point count is proportional to its annulus area (~2r+1) so areal density is uniform instead
        // of crowding the center. Counts come from cumulative rounding of the area profile, so they always sum to
        // exactly point_count (= ring_count * points_per_ring), which the buffers and dispatch rely on.
        public Vector2[] generate() {
            var rings = ring_count;
            var total = point_count;
            int cum(int k) => Mathf.RoundToInt(total * ((float) (k * k) / (rings * rings))); // points through first k rings

            Vector2 ring_point(int r, int count, int p) {
                var radius_r = radius * (r + 1) / rings;
                var angle = p * (2f * Mathf.PI / count) + r * ring_angle_offset;
                return new Vector2(radius_r * Mathf.Cos(angle), radius_r * Mathf.Sin(angle));
            }

            return Enumerable.Range(0, rings)
                .SelectMany(r => {
                    var count = cum(r + 1) - cum(r);
                    return Enumerable.Range(0, count).Select(p => ring_point(r, count, p));
                })
                .ToArray();
        }
    }
}
