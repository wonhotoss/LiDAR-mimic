using UnityEngine;

namespace LiDARMimic {
    // Runtime appearance of a receiver: points only, solid + points, or solid only (occludes, emits no points).
    public enum receiver_mode { pc_only, both, solid }

    // Marks a renderer as a point-cloud source for the LiDAR pass.
    // Mode drives the object's layer (main-camera visibility) and the per-object id pushed to the renderer's MPB.
    // Scene wiring: pc_only mode moves the object to the "LidarOnly" layer, which must exist and be excluded from
    // the main camera's cullingMask (and included in the LiDAR camera's) so the object shows as points only.
    [RequireComponent(typeof(Renderer))]
    public class lidar_receiver : MonoBehaviour {
        [Tooltip("Per-object point color (per_object mode). HDR: Intensity is the initial emission; >1 feeds bloom.")]
        [ColorUsage(false, true)] public Color color = Color.white;
        [Tooltip("Splat size in screen pixels (per_object mode).")]
        public float size = 4f;

        // Assigned by lidar_receiver_registry on enable; used as the per-object id in the LiDAR/integration passes.
        public int id { get; set; }

        // Current mode. Initial value derives from the authored layer on enable; the setter applies it live.
        public receiver_mode mode {
            get => current_mode;
            set {
                current_mode = value;
                apply_mode();
            }
        }
        receiver_mode current_mode;

        // Inactive: renderer off in every camera -> no solid, no occlusion, no points. Mode state is preserved.
        public bool active {
            get => GetComponent<Renderer>().enabled;
            set => GetComponent<Renderer>().enabled = value;
        }

        int visible_layer; // layer used while main-camera-visible (both/solid); captured on enable

        void OnEnable() {
            lidar_receiver_registry.register(this);
            var pc_layer = LayerMask.NameToLayer("LidarOnly");
            visible_layer = gameObject.layer == pc_layer ? 0 : gameObject.layer; // layer to restore for both/solid (0 = Default)
            mode = receiver_mode.pc_only; // default: every receiver starts as pc-only (setter applies layer + id)
        }

        void OnDisable() {
            lidar_receiver_registry.unregister(this);
            set_id(0); // disabled receiver becomes a plain occluder (emits no points)
        }

        // LidarOnly layer is excluded from the main camera (no solid); id 0 suppresses this object's points.
        void apply_mode() {
            gameObject.layer = mode == receiver_mode.pc_only ? LayerMask.NameToLayer("LidarOnly") : visible_layer;
            set_id(mode == receiver_mode.solid ? 0 : id);
        }

        // Push the id to the renderer's MPB so the LiDAR pass's override material can read it per-object.
        void set_id(float v) {
            var r = GetComponent<Renderer>();
            var block = new MaterialPropertyBlock();
            r.GetPropertyBlock(block);
            block.SetFloat("_LidarID", v);
            r.SetPropertyBlock(block);
        }
    }
}
