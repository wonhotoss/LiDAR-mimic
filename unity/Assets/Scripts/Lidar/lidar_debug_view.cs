using UnityEngine;

namespace LiDARMimic {
    // On-screen preview of the LiDAR map via lidar_id_debug.
    // Blit runs in Update into a persistent scratch texture; blitting inside OnGUI corrupts IMGUI rendering,
    // so OnGUI only draws the already-prepared scratch.
    public class lidar_debug_view : MonoBehaviour {
        public lidar device;
        public Material debug_mat;
        public float screen_fraction = 0.3f;
        public int view_size = 512;

        RenderTexture scratch;

        void OnEnable() {
            scratch = new RenderTexture(view_size, view_size, 0) { name = "lidar_debug" };
            scratch.Create();
        }

        void OnDisable() {
            scratch.Release();
            scratch = null;
        }

        void Update() {
            var rt = device.id_rt;
            if (rt != null) {
                Graphics.Blit(rt, scratch, debug_mat);
            }
        }

        void OnGUI() {
            var size = Screen.height * screen_fraction;
            GUI.DrawTexture(new Rect(10, 10, size, size), scratch, ScaleMode.ScaleToFit, false);
        }
    }
}
