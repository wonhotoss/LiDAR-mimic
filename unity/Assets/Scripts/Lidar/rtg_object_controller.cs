using UnityEngine;
using UnityEngine.InputSystem;
using RTGLite;

namespace LiDARMimic {
    // Runtime TRS editing via Runtime Transform Gizmos (Lite): left-click selects scene geometry,
    // W/E/R switch move/rotate/scale. Picking a LiDAR position marker retargets the gizmo to the
    // LiDAR camera itself so the whole camera moves.
    // Setup: run Tools/RTG/Initialize and add the RTMainSRF render feature (see setup notes).
    public class rtg_object_controller : MonoBehaviour {
        public Camera pick_camera; // camera used for picking + gizmo rendering (the main camera)
        public lidar_control_panel ui; // required: clicks over this panel don't select/deselect scene objects

        enum mode { move, rotate, scale }
        mode current = mode.move;

        Gizmo move_gizmo;
        Gizmo rotate_gizmo;
        Gizmo scale_gizmo;
        GameObject selected;

        void Start() {
            Debug.Assert(RTG.get != null, "rtg_object_controller: RTG not in scene (run Tools/RTG/Initialize)");
            Debug.Assert(ui != null, "rtg_object_controller: ui (lidar_control_panel) not assigned");
            RTCamera.get.settings.targetCamera = pick_camera;
            move_gizmo = RTGizmos.get.CreateObjectMoveGizmo();
            rotate_gizmo = RTGizmos.get.CreateObjectRotateGizmo();
            scale_gizmo = RTGizmos.get.CreateObjectScaleGizmo();
            apply();
        }

        void Update() {
            var kb = Keyboard.current;
            if (kb.wKey.wasPressedThisFrame) {
                current = mode.move;
                apply();
            }
            if (kb.eKey.wasPressedThisFrame) {
                current = mode.rotate;
                apply();
            }
            if (kb.rKey.wasPressedThisFrame) {
                current = mode.scale;
                apply();
            }

            // Select on click, but not when the click lands on a gizmo handle (that click is a drag) or on the UI panel.
            var over_panel = ui.pointer_over_ui;
            if (Mouse.current.leftButton.wasPressedThisFrame && RTGizmos.get.hoveredGizmo == null && !over_panel) {
                if (RTScene.get.Raycast(RTCamera.get.GetPickRay(), new ObjectFilter(), false, out var hit) && hit.hasObjectHit) {
                    var l = hit.objectHit.gameObject.GetComponentInParent<lidar>();
                    select(l != null ? l.gameObject : hit.objectHit.gameObject);
                } else {
                    select(null);
                }
            }
        }

        void select(GameObject go) {
            selected = go;
            apply();
        }

        // Only the current-mode gizmo is enabled and targeted; the others are disconnected.
        void apply() {
            configure(move_gizmo, current == mode.move);
            configure(rotate_gizmo, current == mode.rotate);
            configure(scale_gizmo, current == mode.scale);
        }

        void configure(Gizmo g, bool active) {
            var on = active && selected != null;
            g.enabled = on;
            g.objectTransformGizmo.SetTarget(on ? selected : null);
        }
    }
}
