using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace LiDARMimic {
    // Records the reconstructed pc_buffer to a binary PLY sequence (one file per captured frame) via async GPU readback.
    // Capture rate is wall-clock throttled to `capture_fps` (app fps stays free). Default coords = OpenGL right-handed.
    // Runtime-driven: toggle `recording` from script/UI; the editor button below is only a test trigger.
    public class lidar_capture : MonoBehaviour {
        public lidar device;
        public bool recording;
        public bool filter = true; // drop id==0 (no-hit / background) points; off = raw dump for offline filtering
        public bool opengl = true; // right-handed (z-flip); off = Unity raw world
        public float capture_fps = 60f; // max captures per second (<= 0 = every rendered frame)
        public string output_dir = ""; // empty = Application.persistentDataPath
        public string prefix = "lidar";

        int frame_index; // monotonic capture index, drives the filename
        float last_capture_time;

        string dir => string.IsNullOrEmpty(output_dir) ? Application.persistentDataPath : output_dir;

        public int captured => frame_index; // frames captured this session; drives the panel status readout

        void OnEnable() {
            RenderPipelineManager.endFrameRendering += on_end_frame;
        }

        void OnDisable() {
            RenderPipelineManager.endFrameRendering -= on_end_frame;
            AsyncGPUReadback.WaitAllRequests(); // flush in-flight readbacks before the buffer goes away
        }

        // Fires once after all cameras render this frame, so the LiDAR dispatch that fills pc_buffer has run.
        void on_end_frame(ScriptableRenderContext ctx, Camera[] cams) {
            if (recording && device.points != null) {
                var interval = capture_fps > 0f ? 1f / capture_fps : 0f;
                if (Time.unscaledTime - last_capture_time >= interval) {
                    last_capture_time = Time.unscaledTime;
                    var idx = frame_index++;
                    AsyncGPUReadback.Request(device.points, req => on_readback(req, idx));
                }
            }
        }

        // Main-thread readback callback: snapshot the points (filter + coord flip) and hand off to a file write.
        void on_readback(AsyncGPUReadbackRequest req, int idx) {
            Debug.Assert(!req.hasError, "lidar_capture: async readback failed");
            var src = req.GetData<pc_point>();
            var pts = new List<pc_point>(src.Length);
            foreach (var s in src) {
                if (!filter || s.id != 0) {
                    var p = s;
                    if (opengl) {
                        p.world.z = -p.world.z; // Unity (left-handed) -> OpenGL (right-handed)
                    }
                    pts.Add(p);
                }
            }
            var path = Path.Combine(dir, $"{prefix}_{idx:000000}.ply");
            var buffer = pts.ToArray();
            Task.Run(() => write_ply(path, buffer));
        }

        // Binary little-endian PLY: x/y/z floats + id as a uint scalar field (offline id/ROI filtering).
        static void write_ply(string path, pc_point[] pts) {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var header = "ply\n" +
                "format binary_little_endian 1.0\n" +
                $"element vertex {pts.Length}\n" +
                "property float x\n" +
                "property float y\n" +
                "property float z\n" +
                "property uint id\n" +
                "end_header\n";
            using var w = new BinaryWriter(new FileStream(path, FileMode.Create, FileAccess.Write), System.Text.Encoding.ASCII);
            w.Write(System.Text.Encoding.ASCII.GetBytes(header));
            foreach (var p in pts) {
                w.Write(p.world.x);
                w.Write(p.world.y);
                w.Write(p.world.z);
                w.Write(p.id);
            }
        }
    }

}
