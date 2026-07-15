using System.Linq;
using SFB;
using UnityEngine;
using UnityEngine.UIElements;

namespace LiDARMimic {
    // Runtime UI Toolkit control panel. Consolidates what used to live in the editor Custom Inspectors
    // and the IMGUI debug view so the LiDAR is fully controllable in a standalone build.
    [RequireComponent(typeof(UIDocument))]
    public class lidar_control_panel : MonoBehaviour {
        public lidar device; // required
        public lidar_capture capture; // required: recording target
        public Material debug_mat; // required: blits device.id_rt into the viewable debug texture
        public int view_size = 512; // resolution of the debug scratch texture

        const int preview_res = 256;
        Texture2D pattern_tex;
        Image pattern_preview;
        RenderTexture scratch;
        Image debug_view;
        Button record_toggle;
        Label status;

        UIDocument doc => GetComponent<UIDocument>();

        // True while the mouse is anywhere within the panel subtree. Scene input (camera/gizmo) reads this so a
        // drag/click on the UI doesn't also drive the camera. Subtree-aware enter/leave keeps it true over child
        // widgets, and pointer capture keeps it true even if a slider drag wanders off the panel.
        public bool pointer_over_ui { get; private set; }

        void OnEnable() {
            Debug.Assert(device != null, "lidar_control_panel: device not assigned");
            Debug.Assert(capture != null, "lidar_control_panel: capture not assigned");
            Debug.Assert(debug_mat != null, "lidar_control_panel: debug_mat not assigned");
            var root = doc.rootVisualElement;
            root.pickingMode = PickingMode.Ignore; // empty screen stays click-through; only widgets are pickable
            var panel = root.Q("panel");
            panel.RegisterCallback<PointerEnterEvent>(_ => pointer_over_ui = true);
            panel.RegisterCallback<PointerLeaveEvent>(_ => pointer_over_ui = false);
            bind_pattern(root);
            bind_recording(root);
            bind_debug(root);
        }

        void OnDisable() {
            if (pattern_tex != null) {
                Destroy(pattern_tex);
                pattern_tex = null;
            }
            if (scratch != null) {
                scratch.Release();
                scratch = null;
            }
        }

        // Blit runs in Update into a persistent scratch texture the debug Image samples (§13 debug aid).
        void Update() {
            if (device.id_rt != null) { // null only before the LiDAR camera's first render
                Graphics.Blit(device.id_rt, scratch, debug_mat);
                debug_view.MarkDirtyRepaint();
            }
            status.text = capture.recording ? $"Recording...  {capture.captured} frames" : $"Idle  ({capture.captured} frames)";
        }

        void bind_pattern(VisualElement root) {
            var ring_count = root.Q<SliderInt>("ring_count");
            var points_per_ring = root.Q<SliderInt>("points_per_ring");
            var radius = root.Q<Slider>("radius");
            var ring_angle_offset = root.Q<Slider>("ring_angle_offset");
            pattern_preview = root.Q<Image>("pattern_preview");

            ring_count.value = device.ring_count;
            points_per_ring.value = device.points_per_ring;
            radius.value = device.radius;
            ring_angle_offset.value = device.ring_angle_offset;

            ring_count.RegisterValueChangedCallback(e => {
                device.ring_count = e.newValue;
                apply_pattern();
            });
            points_per_ring.RegisterValueChangedCallback(e => {
                device.points_per_ring = e.newValue;
                apply_pattern();
            });
            radius.RegisterValueChangedCallback(e => {
                device.radius = e.newValue;
                apply_pattern();
            });
            ring_angle_offset.RegisterValueChangedCallback(e => {
                device.ring_angle_offset = e.newValue;
                apply_pattern();
            });

            rebuild_preview();
        }

        // Re-upload the pattern (reallocates the pc buffer if the point count changed), then refresh the preview.
        void apply_pattern() {
            device.rebuild();
            rebuild_preview();
        }

        void bind_recording(VisualElement root) {
            var prefix = root.Q<TextField>("prefix");
            var capture_fps = root.Q<FloatField>("capture_fps");
            var filter = root.Q<Toggle>("filter");
            var opengl = root.Q<Toggle>("opengl");
            var output_dir = root.Q<TextField>("output_dir");
            var browse = root.Q<Button>("browse");
            record_toggle = root.Q<Button>("record_toggle");
            status = root.Q<Label>("status");

            prefix.value = capture.prefix;
            capture_fps.value = capture.capture_fps;
            filter.value = capture.filter;
            opengl.value = capture.opengl;
            output_dir.value = capture.output_dir;

            prefix.RegisterValueChangedCallback(e => capture.prefix = e.newValue);
            capture_fps.RegisterValueChangedCallback(e => capture.capture_fps = e.newValue);
            filter.RegisterValueChangedCallback(e => capture.filter = e.newValue);
            opengl.RegisterValueChangedCallback(e => capture.opengl = e.newValue);
            output_dir.RegisterValueChangedCallback(e => capture.output_dir = e.newValue); // Browse also routes through this

            browse.clicked += () => {
                var picked = StandaloneFileBrowser.OpenFolderPanel("Output folder", capture.output_dir, false);
                if (picked.Length > 0) { // empty on cancel
                    output_dir.value = picked[0];
                }
            };

            record_toggle.clicked += () => {
                capture.recording = !capture.recording;
                record_toggle.text = capture.recording ? "Stop Recording" : "Start Recording";
            };
        }

        void bind_debug(VisualElement root) {
            scratch = new RenderTexture(view_size, view_size, 0) { name = "lidar_debug" };
            scratch.Create();
            debug_view = root.Q<Image>("debug_view");
            debug_view.image = scratch;
        }

        // Plot generate()'s projXY into a point-filtered texture: black bg, white points (§12, shared source).
        void rebuild_preview() {
            if (pattern_tex == null) {
                pattern_tex = new Texture2D(preview_res, preview_res, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
                pattern_preview.image = pattern_tex;
            }
            var pixels = Enumerable.Repeat(Color.black, preview_res * preview_res).ToArray();
            foreach (var xy in device.generate()) {
                var px = Mathf.RoundToInt((xy.x * 0.5f + 0.5f) * (preview_res - 1));
                var py = Mathf.RoundToInt((xy.y * 0.5f + 0.5f) * (preview_res - 1));
                if (px >= 0 && px < preview_res && py >= 0 && py < preview_res) {
                    pixels[py * preview_res + px] = Color.white;
                }
            }
            pattern_tex.SetPixels(pixels);
            pattern_tex.Apply();
            pattern_preview.MarkDirtyRepaint();
        }
    }
}
