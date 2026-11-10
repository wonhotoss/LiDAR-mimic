using System.Linq;
using UnityEngine;
using RTGLite;

namespace LiDARMimic {
    // Visualizes the LiDAR: its scan band (the spherical zone elevation_min..elevation_max) as a 1-px wireframe of
    // latitude circles and meridian arcs, plus a position marker at the origin.
    // The marker is registered with RTG so it can be picked to move the whole LiDAR.
    // Marker + lines go on `viz_layer`, which must be excluded from the LiDAR scan_mask so the LiDAR does not
    // scan its own visualization (see setup notes).
    [RequireComponent(typeof(lidar))]
    public class lidar_frustum_viz : MonoBehaviour {
        public Material line_material;   // unlit line material (e.g. URP/Unlit)
        public Material marker_material; // marker surface material
        public float frustum_length = 1.5f; // visual radius of the scan band (not the camera far plane)
        public float marker_size = 0.4f;
        public int meridian_count = 6;
        public float latitude_step = 45f; // degrees between the latitude circles drawn inside the band
        public string viz_layer = "Viz";

        const int circle_segments = 64;
        const int arc_segments = 32;

        Mesh band;
        Vector2 built_band = new Vector2(float.NaN, float.NaN); // (elevation_min, elevation_max) the mesh was built for

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

            // Child of the lidar transform, so the mesh is built in lidar-local space and follows the sensor pose.
            var line_go = new GameObject("lidar_band") { layer = layer };
            line_go.transform.SetParent(transform, false);
            band = new Mesh { name = "lidar_band" };
            line_go.AddComponent<MeshFilter>().sharedMesh = band;
            line_go.AddComponent<MeshRenderer>().sharedMaterial = line_material;
        }

        // Rebuilt only when the band changes: the pattern is runtime-editable, but rebuilding every frame would
        // allocate per frame for nothing.
        void LateUpdate() {
            var device = GetComponent<lidar>();
            var current = new Vector2(device.elevation_min, device.elevation_max);
            if (current != built_band) {
                build(device.elevation_min, device.elevation_max);
                built_band = current;
            }
        }

        // Latitude circles: both band edges plus every multiple of latitude_step strictly inside the band.
        // Meridians: meridian_count arcs from elevation_min to elevation_max.
        // Every segment gets its own vertex pair (no index sharing); the mesh is tiny, so simplicity wins.
        void build(float elevation_min, float elevation_max) {
            Vector3 point(float elevation, float azimuth) => lidar.direction(elevation, azimuth) * frustum_length;
            Vector3[] segments(Vector3[] polyline) => polyline.Zip(polyline.Skip(1), (a, b) => new[] { a, b }).SelectMany(s => s).ToArray();

            var first = Mathf.FloorToInt(elevation_min / latitude_step);
            var last = Mathf.CeilToInt(elevation_max / latitude_step);
            var latitudes = Enumerable.Range(first, last - first + 1)
                .Select(i => i * latitude_step)
                .Where(e => e > elevation_min && e < elevation_max)
                .Prepend(elevation_min).Append(elevation_max);
            var circles = latitudes.Select(e => Enumerable.Range(0, circle_segments + 1).Select(i => point(e, i * 360f / circle_segments)).ToArray());
            var meridians = Enumerable.Range(0, meridian_count)
                .Select(m => m * 360f / meridian_count)
                .Select(a => Enumerable.Range(0, arc_segments + 1).Select(i => point(Mathf.Lerp(elevation_min, elevation_max, i / (float) arc_segments), a)).ToArray());
            var vertices = circles.Concat(meridians).SelectMany(segments).ToArray();

            band.Clear();
            band.SetVertices(vertices);
            band.SetIndices(Enumerable.Range(0, vertices.Length).ToArray(), MeshTopology.Lines, 0);
        }
    }
}
