using UnityEngine;

namespace LiDARMimic {
    // Debug: reads back the reconstructed pc_buffer and draws each receiver point as a gizmo.
    // Editor verification aid for the compute reconstruction (uses a blocking readback; debug-only).
    public class lidar_debug_points : MonoBehaviour {
        public lidar device;
        public float gizmo_size = 0.03f;

        void OnDrawGizmos() {
            var buf = device.points;
            if (buf != null) {
                var data = new pc_point[buf.count];
                buf.GetData(data);
                Gizmos.color = Color.cyan;
                foreach (var p in data) {
                    if (p.id > 0) {
                        Gizmos.DrawSphere(p.world, gizmo_size);
                    }
                }
            }
        }
    }
}
