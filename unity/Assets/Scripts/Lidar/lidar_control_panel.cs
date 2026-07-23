using System;
using System.Collections.Generic;
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
        public Camera view_camera; // required: the display camera; places the receiver hover marker over objects
        public VisualTreeAsset color_row_template;    // required: per-object swatch + emission slider markup (edit in UXML)
        public VisualTreeAsset color_picker_template; // required: R/G/B popup picker markup (edit in UXML)
        public int view_size = 512; // resolution of the debug scratch texture

        const int preview_res = 256;

        // Parallel option tables for the receiver mode buttons (order fixed left-to-right in each row).
        static readonly receiver_mode[] mode_order = { receiver_mode.pc_only, receiver_mode.both, receiver_mode.solid };
        static readonly string[] mode_labels = { "pc-only", "both", "solid" };

        Texture2D pattern_tex;
        Image pattern_preview;
        RenderTexture scratch;
        Image debug_view;
        Button record_toggle;
        Label status;

        Label hover_marker; // boxed object name overlaid on the hovered receiver's object
        lidar_receiver hovered_receiver; // receiver whose row the cursor is currently over (null = none)
        readonly List<VisualElement> receiver_color_editors = new(); // per-object HDR color editors; shown only in per_object mode

        UIDocument doc => GetComponent<UIDocument>();

        // True while the mouse is anywhere within the panel subtree. Scene input (camera/gizmo) reads this so a
        // drag/click on the UI doesn't also drive the camera. Subtree-aware enter/leave keeps it true over child
        // widgets, and pointer capture keeps it true even if a slider drag wanders off the panel.
        public bool pointer_over_ui { get; private set; }

        void OnEnable() {
            Debug.Assert(device != null, "lidar_control_panel: device not assigned");
            Debug.Assert(capture != null, "lidar_control_panel: capture not assigned");
            Debug.Assert(debug_mat != null, "lidar_control_panel: debug_mat not assigned");
            Debug.Assert(view_camera != null, "lidar_control_panel: view_camera not assigned");
            Debug.Assert(color_row_template != null, "lidar_control_panel: color_row_template not assigned");
            Debug.Assert(color_picker_template != null, "lidar_control_panel: color_picker_template not assigned");
            var root = doc.rootVisualElement;
            root.pickingMode = PickingMode.Ignore; // empty screen stays click-through; only widgets are pickable
            var panel = root.Q("panel");
            panel.RegisterCallback<PointerEnterEvent>(_ => pointer_over_ui = true);
            panel.RegisterCallback<PointerLeaveEvent>(_ => pointer_over_ui = false);
            bind_pattern(root);
            bind_point_render(root);
            bind_recording(root);
            bind_debug(root);
        }

        // Receiver rows are built in Start so every lidar_receiver has already registered (all OnEnables run first).
        void Start() {
            bind_receivers(doc.rootVisualElement);
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

            // While a receiver row is hovered, follow its object with the boxed name marker (hidden if behind camera).
            var show_marker = false;
            if (hovered_receiver != null) {
                var center = hovered_receiver.GetComponent<Renderer>().bounds.center;
                if (view_camera.WorldToViewportPoint(center).z > 0) {
                    var p = RuntimePanelUtils.CameraTransformWorldToPanel(hover_marker.panel, center, view_camera);
                    hover_marker.style.left = p.x;
                    hover_marker.style.top = p.y;
                    show_marker = true;
                }
            }
            hover_marker.style.display = show_marker ? DisplayStyle.Flex : DisplayStyle.None;
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

        // Global point-render mode toggle (per-object style vs shared depth colormap). Editor-only globals
        // (size, colormap, range) live on lidar_render_feature; only the mode is switchable here at runtime.
        void bind_point_render(VisualElement root) {
            var per_object = root.Q<Button>("pr_per_object");
            var depth = root.Q<Button>("pr_depth");

            void highlight() {
                per_object.EnableInClassList("mode_btn--active", device.render_mode == point_render_mode.per_object);
                depth.EnableInClassList("mode_btn--active", device.render_mode == point_render_mode.depth_map);
            }
            per_object.clicked += () => {
                device.render_mode = point_render_mode.per_object;
                highlight();
                update_object_color_visibility();
            };
            depth.clicked += () => {
                device.render_mode = point_render_mode.depth_map;
                highlight();
                update_object_color_visibility();
            };
            highlight();
        }

        // The per-object color editors are only meaningful in per_object mode; depth_map ignores per-object color.
        void update_object_color_visibility() {
            var show = device.render_mode == point_render_mode.per_object;
            foreach (var e in receiver_color_editors) {
                e.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        void bind_debug(VisualElement root) {
            scratch = new RenderTexture(view_size, view_size, 0) { name = "lidar_debug" };
            scratch.Create();
            debug_view = root.Q<Image>("debug_view");
            debug_view.image = scratch;
        }

        // One row per active receiver (object name + pc-only/both/solid buttons) plus the "All" buttons that
        // drive every receiver at once. Buttons keep the click inside the panel, so scene picking is unaffected.
        void bind_receivers(VisualElement root) {
            var rows_container = root.Q<VisualElement>("receiver_rows");
            var all_pc = root.Q<Button>("all_pc");
            var all_both = root.Q<Button>("all_both");
            var all_solid = root.Q<Button>("all_solid");

            hover_marker = new Label { pickingMode = PickingMode.Ignore }; // never intercepts scene clicks
            hover_marker.AddToClassList("object_marker");
            hover_marker.style.display = DisplayStyle.None;
            root.Add(hover_marker); // sibling after the panel -> overlays on top

            // Shared HDR color picker popup (R/G/B) from color_picker.uxml. A row's swatch opens it; a pointer-down
            // outside it closes it.
            var picker = color_picker_template.Instantiate().Q<VisualElement>("color_picker");
            picker.style.display = DisplayStyle.None;
            picker.RegisterCallback<PointerEnterEvent>(_ => pointer_over_ui = true); // popup counts as UI (sibling of panel)
            picker.RegisterCallback<PointerLeaveEvent>(_ => pointer_over_ui = false);
            var pick_r = picker.Q<Slider>("pick_r");
            var pick_g = picker.Q<Slider>("pick_g");
            var pick_b = picker.Q<Slider>("pick_b");
            Action<Vector3> picker_setter = null; // routes picker slider changes to the row that opened it
            void on_pick(ChangeEvent<float> _) {
                picker_setter?.Invoke(new Vector3(pick_r.value, pick_g.value, pick_b.value));
            }
            pick_r.RegisterValueChangedCallback(on_pick);
            pick_g.RegisterValueChangedCallback(on_pick);
            pick_b.RegisterValueChangedCallback(on_pick);
            root.Add(picker);

            // Close on any pointer-down outside the popup. Capture phase runs before the swatch's own open handler,
            // and the popup is still hidden while opening, so this never fights the open.
            root.RegisterCallback<PointerDownEvent>(evt => {
                var t = evt.target as VisualElement;
                if (picker.style.display.value == DisplayStyle.Flex && (t == null || !picker.Contains(t))) {
                    picker.style.display = DisplayStyle.None;
                }
            }, TrickleDown.TrickleDown);

            void open_picker(VisualElement anchor, Vector3 base_rgb, Action<Vector3> setter) {
                picker_setter = setter;
                pick_r.SetValueWithoutNotify(base_rgb.x);
                pick_g.SetValueWithoutNotify(base_rgb.y);
                pick_b.SetValueWithoutNotify(base_rgb.z);
                var b = anchor.worldBound; // panel-root space; picker is a root child, so left/top share that space
                picker.style.left = b.xMin;
                picker.style.top = b.yMax + 2f;
                picker.style.display = DisplayStyle.Flex;
            }

            var rows = new List<(lidar_receiver r, Action<receiver_mode> highlight)>();
            foreach (var r in lidar_receiver_registry.receivers) {
                var row = new VisualElement();
                row.AddToClassList("receiver_row");
                row.RegisterCallback<PointerEnterEvent>(_ => {
                    hovered_receiver = r;
                    hover_marker.text = r.gameObject.name;
                });
                row.RegisterCallback<PointerLeaveEvent>(_ => {
                    if (hovered_receiver == r) {
                        hovered_receiver = null;
                    }
                });
                var header = new VisualElement();
                header.AddToClassList("receiver_header");
                var active_btn = new Button();
                active_btn.AddToClassList("active_btn");
                var label = new Label(r.gameObject.name);
                label.AddToClassList("receiver_name");
                header.Add(active_btn);
                header.Add(label);

                var buttons_row = new VisualElement();
                buttons_row.AddToClassList("mode_row");

                var buttons = mode_order.Zip(mode_labels, (m, l) => {
                    var b = new Button { text = l };
                    b.AddToClassList("mode_btn");
                    buttons_row.Add(b);
                    return (b, m);
                }).ToArray();

                void highlight(receiver_mode sel) {
                    foreach (var (b, m) in buttons) {
                        b.EnableInClassList("mode_btn--active", m == sel);
                    }
                }
                foreach (var (b, m) in buttons) {
                    b.clicked += () => {
                        r.mode = m;
                        highlight(m);
                    };
                }
                highlight(r.mode);

                // Inactive greys out the mode buttons: the object is hidden but its mode is kept for reactivation.
                void set_active(bool on) {
                    r.active = on;
                    active_btn.text = on ? "on" : "off";
                    active_btn.EnableInClassList("active_btn--on", on);
                    buttons_row.SetEnabled(on);
                }
                active_btn.clicked += () => set_active(!r.active);
                set_active(r.active);

                // Per-object HDR color editor from receiver_color_row.uxml (swatch + emission slider). Clicking the
                // swatch opens the shared R/G/B picker. color = base RGB (0..1) x emission; >1 feeds bloom (emissive glow).
                var color_editor = color_row_template.Instantiate().Q<VisualElement>("color_editor");
                var swatch = color_editor.Q<VisualElement>("swatch");
                var emission = color_editor.Q<Slider>("emission");
                var c0 = r.color;
                var emission0 = Mathf.Max(c0.r, c0.g, c0.b, 1f); // decompose current color into base(0..1) x emission
                var base_rgb = new Vector3(c0.r / emission0, c0.g / emission0, c0.b / emission0);
                emission.SetValueWithoutNotify(emission0);

                void apply_color() {
                    var k = emission.value;
                    r.color = new Color(base_rgb.x * k, base_rgb.y * k, base_rgb.z * k, 1f);
                    swatch.style.backgroundColor = new Color(Mathf.Clamp01(r.color.r), Mathf.Clamp01(r.color.g), Mathf.Clamp01(r.color.b), 1f);
                }
                emission.RegisterValueChangedCallback(_ => apply_color());
                swatch.RegisterCallback<PointerDownEvent>(_ => open_picker(swatch, base_rgb, v => {
                    base_rgb = v;
                    apply_color();
                }));
                apply_color();

                row.Add(header);
                row.Add(buttons_row);
                row.Add(color_editor);
                rows_container.Add(row);
                receiver_color_editors.Add(color_editor);
                rows.Add((r, highlight));
            }

            void set_all(receiver_mode m) {
                foreach (var (r, highlight) in rows) {
                    r.mode = m;
                    highlight(m);
                }
            }
            all_pc.clicked += () => set_all(receiver_mode.pc_only);
            all_both.clicked += () => set_all(receiver_mode.both);
            all_solid.clicked += () => set_all(receiver_mode.solid);
            update_object_color_visibility(); // match the current global point-render mode
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
