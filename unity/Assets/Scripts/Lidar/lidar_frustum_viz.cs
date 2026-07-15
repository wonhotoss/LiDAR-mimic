using UnityEngine;
using RTGLite;

namespace LiDARMimic {
    // Visualizes the LiDAR camera: a frustum wireframe (LineRenderer) plus a position marker at the apex.
    // The marker is registered with RTG so it can be picked to move the whole LiDAR camera.
    // Marker + lines go on `viz_layer`, which must be excluded from the LiDAR camera cullingMask so the
    // LiDAR does not scan its own visualization (see setup notes).
    [RequireComponent(typeof(Camera))]
    public class lidar_frustum_viz : MonoBehaviour {
        public Material line_material;   // unlit line material (e.g. URP/Unlit)
        public Material marker_material; // marker surface material
        public float frustum_length = 12f; // visual far distance of the wireframe (not the camera far plane)
        public float marker_size = 0.4f;
        public float line_width = 0.03f;
        public string viz_layer = "Viz";

        LineRenderer frustum;

        // Cube-wireframe line strip over near(0-3) + far(4-7) corners; each rect ordered BL, BR, TR, TL.
        static readonly int[] strip = { 0, 1, 2, 3, 0, 4, 5, 6, 7, 4, 5, 1, 2, 6, 7, 3 };

        void Start() {
            var layer = LayerMask.NameToLayer(viz_layer);
            Debug.Assert(layer >= 0, "lidar_frustum_viz: viz layer missing (create it and exclude from LiDAR cullingMask)");

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

            var line_go = new GameObject("lidar_frustum") { layer = layer };
            line_go.transform.SetParent(transform, false);
            frustum = line_go.AddComponent<LineRenderer>();
            frustum.useWorldSpace = true;
            frustum.material = line_material;
            frustum.widthMultiplier = line_width;
            frustum.numCornerVertices = 0;
            frustum.positionCount = strip.Length;
        }

        void LateUpdate() {
            var cam = GetComponent<Camera>();
            for (var i = 0; i < strip.Length; i++) {
                frustum.SetPosition(i, corner(cam, strip[i]));
            }
        }

        // corner index: 0-3 near plane, 4-7 far plane; within a plane BL, BR, TR, TL.
        Vector3 corner(Camera cam, int idx) {
            var far = idx >= 4;
            var c = far ? idx - 4 : idx;
            var dist = far ? frustum_length : cam.nearClipPlane;
            var sx = (c == 1 || c == 2) ? 1f : -1f;
            var sy = (c == 2 || c == 3) ? 1f : -1f;
            var h = dist * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            var w = h * cam.aspect;
            return cam.transform.TransformPoint(new Vector3(sx * w, sy * h, dist));
        }
    }
}
