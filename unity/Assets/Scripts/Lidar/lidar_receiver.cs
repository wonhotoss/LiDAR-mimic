using UnityEngine;

namespace LiDARMimic {
    // Marks a renderer as a point-cloud source for the LiDAR pass.
    // Component enabled state acts as the pc-render toggle (registry membership + id on the renderer).
    [RequireComponent(typeof(Renderer))]
    public class lidar_receiver : MonoBehaviour {
        public Color color = Color.white;
        public float size = 4f; // splat size in screen pixels; consumed by the integration pass

        // Assigned by lidar_receiver_registry on enable; used as the per-object id in the LiDAR/integration passes.
        public int id { get; set; }

        void OnEnable() {
            lidar_receiver_registry.register(this);
            set_id(id);
        }

        void OnDisable() {
            lidar_receiver_registry.unregister(this);
            set_id(0); // disabled receiver becomes a plain occluder (emits no points)
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
