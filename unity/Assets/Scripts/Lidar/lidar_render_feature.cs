using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace LiDARMimic {
    // LiDAR camera: writes per-object id + NDC depth into id_rt.
    // Other cameras: draws the reconstructed pc_buffer as fixed-size point splats into the main target.
    public class lidar_render_feature : ScriptableRendererFeature {
        [Tooltip("Assign the lidar/id_write shader (LiDAR pass: writes per-object id + NDC depth).")]
        public Shader write_shader;
        [Tooltip("Assign the lidar/point shader (integration pass: draws the reconstructed points).")]
        public Shader point_shader;
        [Tooltip("Clip-space z nudge toward the camera. Sign is platform-dependent; flip if points are hidden by their own surface.")]
        public float depth_bias = 0.0002f;

        // depth_map mode globals: editor-tunable only (no runtime UI). The mode itself is switched at runtime via lidar.render_mode.
        [Header("depth_map mode")]
        [Tooltip("Point size (px) used in depth_map mode.")]
        public float global_point_size = 4f;
        [Tooltip("Near -> far color ramp (jet-like by default). Sampled cyclically: end it on the start color to keep the loop seamless.")]
        public Gradient depth_colormap = make_default_colormap();
        [Tooltip("Range (m from sensor) mapped to the colormap start.")]
        public float depth_min = 0f;
        [Tooltip("Range (m from sensor) mapped to the colormap end.")]
        public float depth_max = 50f;
        [Tooltip("Multiplies the colormap color; >1 feeds bloom (emissive glow).")]
        public float depth_emission = 1f;
        [Tooltip("Colormap scroll speed in cycles/sec (negative scrolls the other way). 0 = static.")]
        public float depth_scroll_speed = 0.1f;

        Material write_mat;
        Material point_mat;
        ComputeBuffer style_buffer;
        Texture2D colormap_tex; // depth_colormap baked into a 1D lookup sampled by the point shader
        id_pass id;
        point_pass points;

        // Jet-like near(blue) -> far(red) ramp, closed back to blue at 1 so the scrolling lookup wraps without a seam.
        static Gradient make_default_colormap() {
            var g = new Gradient();
            g.SetKeys(
                new[] {
                    new GradientColorKey(new Color(0.2f, 0.3f, 1f), 0f),
                    new GradientColorKey(new Color(0f, 1f, 1f), 0.2f),
                    new GradientColorKey(new Color(0f, 1f, 0f), 0.4f),
                    new GradientColorKey(new Color(1f, 1f, 0f), 0.6f),
                    new GradientColorKey(new Color(1f, 0.15f, 0.15f), 0.8f),
                    new GradientColorKey(new Color(0.2f, 0.3f, 1f), 1f)
                },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
            return g;
        }

        public override void Create() {
            write_mat = CoreUtils.CreateEngineMaterial(write_shader);
            point_mat = CoreUtils.CreateEngineMaterial(point_shader);
            id = new id_pass(write_mat) { renderPassEvent = RenderPassEvent.AfterRenderingOpaques };
            points = new point_pass { renderPassEvent = RenderPassEvent.AfterRenderingOpaques };
            bake_colormap();
        }

        // Bake depth_colormap into a small 1D lookup texture. Runs on Create (and re-runs on inspector edits in the editor).
        // Sampled as a period-1 ramp (texel i = gradient at i/w, Repeat wrap) so the scrolled lookup blends across the seam.
        void bake_colormap() {
            const int w = 256;
            if (colormap_tex == null) {
                colormap_tex = new Texture2D(w, 1, TextureFormat.RGBA32, false) {
                    name = "lidar_depth_colormap", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear
                };
            }
            colormap_tex.SetPixels(Enumerable.Range(0, w).Select(i => depth_colormap.Evaluate(i / (float) w)).ToArray());
            colormap_tex.Apply();
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData data) {
            if (data.cameraData.camera.GetComponent<lidar>() != null) {
                renderer.EnqueuePass(id);
            } else {
                var device = lidar_registry.devices.FirstOrDefault();
                if (device != null && device.points != null) {
                    if (colormap_tex == null) { // Create's bake can be lost across domain reload / play-mode enter -> would bind gray
                        bake_colormap();
                    }
                    point_mat.SetBuffer("pc", device.points);
                    point_mat.SetBuffer("style", build_style()); // always bound; used only in per_object mode
                    point_mat.SetFloat("depth_bias", depth_bias);
                    point_mat.SetFloat("point_mode", device.render_mode == point_render_mode.depth_map ? 1f : 0f);
                    point_mat.SetFloat("global_size", global_point_size);
                    point_mat.SetVector("lidar_pos", device.transform.position);
                    point_mat.SetFloat("depth_min", depth_min);
                    point_mat.SetFloat("depth_max", depth_max);
                    point_mat.SetFloat("depth_emission", depth_emission);
                    point_mat.SetFloat("depth_offset", Mathf.Repeat(Time.time * depth_scroll_speed, 1f));
                    point_mat.SetTexture("colormap", colormap_tex);
                    points.setup(point_mat, device.point_count);
                    renderer.EnqueuePass(points);
                }
            }
        }

        protected override void Dispose(bool disposing) {
            CoreUtils.Destroy(write_mat);
            CoreUtils.Destroy(point_mat);
            CoreUtils.Destroy(colormap_tex);
            style_buffer?.Release();
        }

        // Per-id style buffer (index = id): rgb = color, w = size in pixels. Rebuilt from the active receivers.
        ComputeBuffer build_style() {
            var receivers = lidar_receiver_registry.receivers;
            var max_id = receivers.Count > 0 ? receivers.Max(r => r.id) : 0;
            var styles = new Vector4[max_id + 1];
            foreach (var r in receivers) {
                styles[r.id] = new Vector4(r.color.r, r.color.g, r.color.b, r.size);
            }
            if (style_buffer == null || style_buffer.count != styles.Length) {
                style_buffer?.Release();
                style_buffer = new ComputeBuffer(styles.Length, sizeof(float) * 4);
            }
            style_buffer.SetData(styles);
            return style_buffer;
        }

        class id_pass : ScriptableRenderPass {
            static readonly ShaderTagId forward = new("UniversalForward");
            Material mat;

            public id_pass(Material m) {
                mat = m;
            }

            class data {
                public RendererListHandle list;
            }

            public override void RecordRenderGraph(RenderGraph graph, ContextContainer frame) {
                var resources = frame.Get<UniversalResourceData>();
                var cam = frame.Get<UniversalCameraData>();
                var render = frame.Get<UniversalRenderingData>();

                var sort = new SortingSettings(cam.camera) { criteria = SortingCriteria.CommonOpaque };
                var draw = new DrawingSettings(forward, sort) { overrideMaterial = mat, overrideMaterialPassIndex = 0 };
                var filter = new FilteringSettings(RenderQueueRange.opaque);
                var list = graph.CreateRendererList(new RendererListParams(render.cullResults, draw, filter));

                using (var builder = graph.AddRasterRenderPass<data>("lidar_id", out var d)) {
                    d.list = list;
                    builder.UseRendererList(list);
                    builder.SetRenderAttachment(resources.activeColorTexture, 0);
                    builder.SetRenderAttachmentDepth(resources.activeDepthTexture);
                    builder.SetRenderFunc((data x, RasterGraphContext ctx) => ctx.cmd.DrawRendererList(x.list));
                }
            }
        }

        class point_pass : ScriptableRenderPass {
            Material mat;
            int count;

            public void setup(Material m, int n) {
                mat = m;
                count = n;
            }

            class data {
                public Material mat;
                public int count;
            }

            public override void RecordRenderGraph(RenderGraph graph, ContextContainer frame) {
                var resources = frame.Get<UniversalResourceData>();

                using (var builder = graph.AddRasterRenderPass<data>("lidar_points", out var d)) {
                    d.mat = mat;
                    d.count = count;
                    builder.SetRenderAttachment(resources.activeColorTexture, 0);
                    builder.SetRenderAttachmentDepth(resources.activeDepthTexture);
                    builder.AllowPassCulling(false);
                    builder.SetRenderFunc((data x, RasterGraphContext ctx) =>
                        ctx.cmd.DrawProcedural(Matrix4x4.identity, x.mat, 0, MeshTopology.Triangles, 6 * x.count));
                }
            }
        }
    }
}
