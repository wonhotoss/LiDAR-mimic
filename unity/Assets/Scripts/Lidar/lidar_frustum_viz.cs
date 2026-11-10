using UnityEngine;
using RTGLite;

namespace LiDARMimic {
    // Visualizes the LiDAR: its scan band as a wireframe (LineRenderer) plus a position marker at the origin.
    // The marker is registered with RTG so it can be picked to move the whole LiDAR.
    // Marker + lines go on `viz_layer`, which must be excluded from the LiDAR scan_mask so the LiDAR does not
    // scan its own visualization (see setup notes).
    [RequireComponent(typeof(lidar))]
    public class lidar_frustum_viz : MonoBehaviour {
        public Material line_material;   // unlit line material (e.g. URP/Unlit)
        public Material marker_material; // marker surface material
        public float frustum_length = 12f; // visual radius of the scan band (not the camera far plane)
        public float marker_size = 0.4f;
        public float line_width = 0.03f;
        public string viz_layer = "Viz";

        const int segments = 64; // per latitude circle

        LineRenderer band;

        void Start() {
            var layer = LayerMask.NameToLayer(viz_layer);
            Debug.Assert(layer >= 0, "lidar_frustum_viz: viz layer missing (create it and exclude from LiDAR scan_mask)");

            var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.name = "lidar_marker";
            marker.layer = layer;
            marker.transform.SetParent(transform, false);
            marker.transform.localScale = Vector3.one * marker_size;
            Destroy(marker.GetComponent<Collider>()); // RTG picks meshes, not physics colliders
            marker.GetComponent<MeshRenderer>().sharedMaterial = marker_material;
            if (RTG.get != null) {
                RTScene.get.RegisterObjectHierarchy(marker); // runtime-created, so not auto-registered on load
            }

            var line_go = new GameObject("lidar_band") { layer = layer };
            line_go.transform.SetParent(transform, false);
            band = line_go.AddComponent<LineRenderer>();
            band.useWorldSpace = true;
            band.material = line_material;
            band.widthMultiplier = line_width;
            band.numCornerVertices = 0;
            band.positionCount = 2 * (segments + 1);
        }

        // One strip: the elevation_min circle (closed at azimuth 0), then the elevation_max circle from azimuth 0;
        // the jump between them is the meridian. For a band reaching the zenith the top circle is a small ring.
        void LateUpdate() {
            var device = GetComponent<lidar>();
            for (var i = 0; i <= segments; i++) {
                var azimuth = i * 360f / segments;
                band.SetPosition(i, point(device.elevation_min, azimuth));
                band.SetPosition(segments + 1 + i, point(device.elevation_max, azimuth));
            }
        }

        Vector3 point(float elevation, float azimuth) => transform.TransformPoint(lidar.direction(elevation, azimuth) * frustum_length);
    }
}
