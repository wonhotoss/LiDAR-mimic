using UnityEngine;
using UnityEngine.Rendering;

namespace LiDARMimic {
    // Debug helper invoked from the control panel's Debug section (lidar_diagnostics.run). Reads id_rt and
    // pc_buffer back and logs a verdict: whether the failure is the sensor pass (Stage A, id_rt), the compute
    // reconstruction (Stage B, pc_buffer), or the integration render (Stage C). Static + primitive types +
    // method-group callbacks (no LINQ / HashSet / StringBuilder / capturing lambdas) to stay IL2CPP/WASM-safe.
    static class lidar_diagnostics {
        const int sample_stride = 8; // subsample every Nth id_rt texel to bound analysis cost

        enum id_result { unknown, readback_failed, empty, clean, leaked }
        enum pc_result { unknown, readback_failed, none, bad_coords, ok }

        static id_result id_state;
        static pc_result pc_state;
        static string id_detail = "";
        static string pc_detail = "";
        static bool id_busy;
        static bool pc_busy;

        // Issue both readbacks; each logs its section when it lands. Ignored if a run is already in flight.
        public static void run(lidar device) {
            if (!id_busy && device.id_rt != null) {
                id_state = id_result.unknown;
                id_busy = true;
                AsyncGPUReadback.Request(device.id_rt, 0, on_id);
            }
            if (!pc_busy && device.points != null) {
                pc_state = pc_result.unknown;
                pc_busy = true;
                AsyncGPUReadback.Request(device.points, on_pc);
            }
        }

        // id_rt is RGFloat: x = id (should be a small integer), y = NDC depth. If x is mostly non-integer /
        // continuous, id_rt holds leaked shaded color instead of ids (the id_write override did not overwrite).
        static void on_id(AsyncGPUReadbackRequest req) {
            id_busy = false;
            if (req.hasError) {
                id_state = id_result.readback_failed;
                id_detail = "";
            } else {
                var data = req.GetData<Vector2>(); // x = R (id), y = G (NDC depth)
                var n = 0;
                var nonzero = 0;
                var integer_like = 0;
                var rmin = float.MaxValue;
                var rmax = float.MinValue;
                var gmin = float.MaxValue;
                var gmax = float.MinValue;
                var id0 = -1;
                var id1 = -1;
                var id2 = -1;
                for (var i = 0; i < data.Length; i += sample_stride) {
                    var r = data[i].x;
                    var g = data[i].y;
                    n++;
                    if (r < rmin) { rmin = r; }
                    if (r > rmax) { rmax = r; }
                    if (g < gmin) { gmin = g; }
                    if (g > gmax) { gmax = g; }
                    if (Mathf.Abs(r) > 0.5f) {
                        nonzero++;
                        if (Mathf.Abs(r - Mathf.Round(r)) < 0.05f) {
                            integer_like++;
                            var id = Mathf.RoundToInt(r);
                            if (id0 < 0) {
                                id0 = id;
                            } else if (id != id0 && id1 < 0) {
                                id1 = id;
                            } else if (id != id0 && id != id1 && id2 < 0) {
                                id2 = id;
                            }
                        }
                    }
                }
                var nonzero_frac = n > 0 ? (float) nonzero / n : 0f;
                var int_frac = nonzero > 0 ? (float) integer_like / nonzero : 0f;
                if (nonzero_frac < 0.001f) {
                    id_state = id_result.empty;
                } else if (int_frac > 0.9f) {
                    id_state = id_result.clean;
                } else {
                    id_state = id_result.leaked;
                }
                id_detail = "samples=" + n + "  nonzeroR=" + nonzero_frac.ToString("P0") + "  integerR=" + int_frac.ToString("P0") +
                    "\nR=[" + rmin.ToString("F2") + "," + rmax.ToString("F2") + "]  G=[" + gmin.ToString("F3") + "," + gmax.ToString("F3") + "]" +
                    "\nsample ids=" + id0 + "," + id1 + "," + id2;
            }
            Debug.Log("[lidar_diagnostics A]\n" + id_verdict() + "\n" + id_detail);
        }

        static void on_pc(AsyncGPUReadbackRequest req) {
            pc_busy = false;
            if (req.hasError) {
                pc_state = pc_result.readback_failed;
                pc_detail = "";
            } else {
                var data = req.GetData<pc_point>();
                var hit = 0;
                var bad = 0;
                var min = Vector3.positiveInfinity;
                var max = Vector3.negativeInfinity;
                var samples = "";
                for (var i = 0; i < data.Length; i++) {
                    var p = data[i];
                    if (p.id > 0) {
                        hit++;
                        var w = p.world;
                        if (float.IsNaN(w.x) || float.IsInfinity(w.x) || float.IsNaN(w.y) || float.IsInfinity(w.y) || float.IsNaN(w.z) || float.IsInfinity(w.z)) {
                            bad++;
                        } else {
                            min = Vector3.Min(min, w);
                            max = Vector3.Max(max, w);
                        }
                        if (hit <= 3) {
                            samples += "  [" + i + "] id=" + p.id + " world=(" + w.x.ToString("F2") + "," + w.y.ToString("F2") + "," + w.z.ToString("F2") + ")\n";
                        }
                    }
                }
                var span = Mathf.Max(Mathf.Abs(min.x), Mathf.Abs(min.y), Mathf.Abs(min.z), Mathf.Abs(max.x), Mathf.Abs(max.y), Mathf.Abs(max.z));
                var reasonable = hit > bad && span < 1e5f;
                if (hit == 0) {
                    pc_state = pc_result.none;
                } else if (bad > 0 || !reasonable) {
                    pc_state = pc_result.bad_coords;
                } else {
                    pc_state = pc_result.ok;
                }
                pc_detail = "total=" + data.Length + "  id>0=" + hit + "  id==0=" + (data.Length - hit) + "  bad(NaN/Inf)=" + bad +
                    "\nbounds min=(" + min.x.ToString("F2") + "," + min.y.ToString("F2") + "," + min.z.ToString("F2") + ") max=(" + max.x.ToString("F2") + "," + max.y.ToString("F2") + "," + max.z.ToString("F2") + ")\n" + samples;
            }
            Debug.Log("[lidar_diagnostics B]\n" + pc_verdict() + "\n" + pc_detail + "\n" + overall());
        }

        static string id_verdict() {
            switch (id_state) {
                case id_result.readback_failed: return "A) id_rt readback FAILED (this backend may block texture readback).";
                case id_result.empty: return "A) id_rt R is ~all 0 -> LiDAR camera wrote no ids. Check receiver layers, LiDAR cullingMask, _LidarID MPB.";
                case id_result.clean: return "A) id_rt R is integer-valued -> sensor id pass looks CORRECT.";
                case id_result.leaked: return "A) id_rt R is mostly NON-integer -> leaked shaded color: the id_write override did NOT overwrite the LiDAR camera's opaque.";
                default: return "A) id_rt not analyzed.";
            }
        }

        static string pc_verdict() {
            switch (pc_state) {
                case pc_result.readback_failed: return "B) pc_buffer readback FAILED (this backend may block buffer readback).";
                case pc_result.none: return "B) pc_buffer has NO id>0 points -> reconstruction produced nothing usable.";
                case pc_result.bad_coords: return "B) pc_buffer has points but coords are NaN/Inf or absurd -> reconstruction math/backend.";
                case pc_result.ok: return "B) pc_buffer has id>0 points with sane bounds -> DATA is OK; suspect the integration render.";
                default: return "B) pc_buffer not analyzed.";
            }
        }

        static string overall() {
            if (pc_state == pc_result.ok) {
                return ">> Likely RENDERING: data is present but nothing draws. Check the point draw / shader / active renderer.";
            }
            if (id_state == id_result.clean && (pc_state == pc_result.none || pc_state == pc_result.bad_coords)) {
                return ">> id_rt looks OK but pc_buffer does not -> suspect the COMPUTE reconstruction.";
            }
            if (id_state == id_result.leaked || id_state == id_result.empty) {
                return ">> Likely DATA COLLECTION: id_rt is wrong, so reconstruction and render have nothing valid.";
            }
            return ">> Inconclusive; see the A/B details above.";
        }
    }
}
