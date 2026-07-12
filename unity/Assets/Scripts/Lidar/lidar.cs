using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LiDARMimic {
    // One reconstructed LiDAR point: world position + originating object id (0 = background / non-receiver).
    public struct pc_point {
        public Vector3 world;
        public uint id;
    }

    // LiDAR device. Renders the scene from its viewpoint into id_rt (R = id, G = NDC depth); a compute pass
    // then reconstructs each scan ray into a world-space point (pc_buffer). Also generates the scan pattern.
    [RequireComponent(typeof(Camera))]
    public class lidar : MonoBehaviour {
        public int ring_count = 32;
        public int points_per_ring = 64;
        public float radius = 0.95f; // max NDC radius of the outermost ring (<= 1)
        public float ring_angle_offset = 0.1f; // radians added per successive ring so rings don't align radially

        public int map_resolution = 1024; // id_rt size; decoupled from screen (TODO2-implementation §3)
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

        // Per-ray projXY in LiDAR NDC ([-1,1]^2). Concentric rings with a per-ring angular offset.
        public Vector2[] generate() {
            Vector2 ring_point(int r, int p) {
                var radius_r = radius * (r + 1) / ring_count;
                var angle = p * (2f * Mathf.PI / points_per_ring) + r * ring_angle_offset;
                return new Vector2(radius_r * Mathf.Cos(angle), radius_r * Mathf.Sin(angle));
            }

            return Enumerable.Range(0, ring_count)
                .SelectMany(r => Enumerable.Range(0, points_per_ring).Select(p => ring_point(r, p)))
                .ToArray();
        }
    }

#if UNITY_EDITOR
    // Inspector preview of the scan pattern (§12). Editor-only, kept beside the runtime type.
    [CustomEditor(typeof(lidar))]
    class lidar_editor : Editor {
        const int preview_res = 256;
        Texture2D preview;

        public override void OnInspectorGUI() {
            EditorGUI.BeginChangeCheck();
            DrawDefaultInspector();
            if (EditorGUI.EndChangeCheck() || preview == null) {
                rebuild_preview();
            }
            var rect = GUILayoutUtility.GetAspectRect(1f);
            EditorGUI.DrawPreviewTexture(rect, preview);
        }

        void OnDisable() {
            if (preview != null) {
                DestroyImmediate(preview);
            }
        }

        void rebuild_preview() {
            var pts = ((lidar)target).generate();
            if (preview == null) {
                preview = new Texture2D(preview_res, preview_res, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            }
            var pixels = Enumerable.Repeat(Color.black, preview_res * preview_res).ToArray();
            foreach (var xy in pts) {
                var px = Mathf.RoundToInt((xy.x * 0.5f + 0.5f) * (preview_res - 1));
                var py = Mathf.RoundToInt((xy.y * 0.5f + 0.5f) * (preview_res - 1));
                if (px >= 0 && px < preview_res && py >= 0 && py < preview_res) {
                    pixels[py * preview_res + px] = Color.white;
                }
            }
            preview.SetPixels(pixels);
            preview.Apply();
        }
    }
#endif
}
