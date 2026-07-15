using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GizmoVertexSnap (Public Static Class)
    // Desc: Implements utility functions used when implementing gizmo vertex snapping.
    //-----------------------------------------------------------------------------
    public static class GizmoVertexSnap
    {
        #region Private Static Fields
        static ObjectFilter  sRaycastFilter = new ObjectFilter();   // Used when raycasting

        // Buffers used to avoid memory allocations
        static List<Vector3> sVec3Buffer    = new List<Vector3>();
        #endregion

        #region Public Static Properties
        //-----------------------------------------------------------------------------
        // Name: snapLayerMask (Public Static Property)
        // Desc: Returns or sets the snap layer mask. When vertex snapping, the pivot
        //       objects will snap only to objects whose layers are set inside this 
        //       mask.
        //-----------------------------------------------------------------------------
        public static int snapLayerMask { get; set; } = ~0;
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: SelectPivot() (Public Static Function)
        // Desc: Selects a pivot position from the specified object collection. This is
        //       the position that is snapped to surrounding object verts when doing vertex
        //       snapping.
        // Parm: pivotObjects - The pivot will be selected from one of these objects.
        //       camera       - The camera that interacts with the vertex snap gizmo.
        //       pivot        - Returns the pivot position. Should be ignored if the function
        //                      returns false.
        // Rtrn: True if a pivot is selected and false otherwise.
        //-----------------------------------------------------------------------------
        public static bool SelectPivot(IReadOnlyList<GameObject> pivotObjects, Camera camera, out Vector3 pivot)
        {
            // Clear output
            pivot = Vector3.zero;

            // Cache data
            Vector2 devicePos = RTInput.get.pointingInputDevice.position;
            BoundsQueryConfig boundsQConfig = BoundsQueryConfig.defaultConfig;
            boundsQConfig.objectTypes = EGameObjectType.Mesh | EGameObjectType.Sprite;

            // Keep track of closest object
            float dMin = float.MaxValue;
            GameObject closestObject = null;

            // Loop through each object and find the one whose screen rectangle is closest to the input device
            int count = pivotObjects.Count;
            for (int i = 0; i < count; ++i)
            {
                // Calculate screen rectangle
                if (pivotObjects[i].CalculateScreenRect(camera, boundsQConfig, out Rect rect))
                {
                    // Calculate the distance between the screen rectangle and the input device position
                    float d = (rect.center - devicePos).magnitude;

                    // Found a closer object?
                    if (d < dMin)
                    {
                        dMin = d;
                        closestObject = pivotObjects[i];
                    }
                }
            }

            // Did we find an object?
            if (closestObject != null)
            {
                // Keep track of closest vertex
                dMin = float.MaxValue;

                // Check object type
                EGameObjectType objectType = closestObject.GetGameObjectType();
                if (objectType == EGameObjectType.Mesh)
                {
                    // Get object mesh
                    RTMesh rtMesh = RTMeshManager.GetRTMesh(closestObject.GetMesh());
                    if (rtMesh == null) return false;

                    // Find the vertex whose screen position is closest to the input device
                    count = rtMesh.vertexCount;
                    if (count == 0) return false;
                    for (int i = 0; i < count; ++i)
                    {
                        // Convert vertex to screen position
                        Vector3 vPos = closestObject.transform.TransformPoint(rtMesh.GetVertexPosition(i));
                        Vector2 screenPos = camera.WorldToScreenPoint(vPos);

                        // Is this vertex closer?
                        float d = (screenPos - devicePos).magnitude;
                        if (d < dMin)
                        {
                            dMin = d;
                            pivot = vPos;
                        }
                    }

                    // Success!
                    return true;
                }
                else
                {
                    // Get sprite world verts
                    var obb = closestObject.CalculateSpriteWorldOBB();
                    if (!obb.isValid) return false;

                    // Get sprite centers and corners
                    obb.CalculateCorners(sVec3Buffer);
                    sVec3Buffer.Add(obb.center);        // Let's also allow the center to be selected

                    // Find the closest point
                    count = sVec3Buffer.Count;
                    for (int i = 0; i < count; ++i)
                    {
                        // Convert vertex to screen position
                        Vector2 screenPos = camera.WorldToScreenPoint(sVec3Buffer[i]);

                        // Is this vertex closer?
                        float d = (screenPos - devicePos).magnitude;
                        if (d < dMin)
                        {
                            dMin = d;
                            pivot = sVec3Buffer[i];
                        }
                    }

                    // Success!
                    return true;
                }
            }

            // No object found
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: SnapPivot() (Public Static Function)
        // Desc: Snaps the specified pivot to a game object hovered by the input device.
        // Parm: pivotObjects - The game object collection which was initially passed 
        //                      to the 'SelectPivot' function.
        //       camera       - The camera that interacts with the vertex snap gizmo.
        //       pivot        - The current pivot position.
        //       snappedPivot - Returns the position of the snapped pivot. If the function
        //                      returns false, this value should be ignored.
        // Rtrn: True if the pivot is snapped and false otherwise.
        //-----------------------------------------------------------------------------
        public static bool SnapPivot(IReadOnlyList<GameObject> pivotObjects, Camera camera, Vector3 pivot, out Vector3 snappedPivot)
        {
            // Clear output
            snappedPivot = pivot;

            // Validate call
            if (pivotObjects.Count == 0)
                return false;

            // Raycast closest object
            sRaycastFilter.objectTypes = EGameObjectType.Mesh | EGameObjectType.Sprite;
            sRaycastFilter.SetIgnoredObjects(pivotObjects);
            sRaycastFilter.layerMask = snapLayerMask;
            if (RTScene.get.Raycast(RTInput.get.pointingInputDevice.GetPickRay(camera), sRaycastFilter, true, out SceneRayHit sceneHit))
            {
                // Did we hit a mesh?
                var objectHit = sceneHit.objectHit;
                RTMesh rtMesh = objectHit.rtMeshHit.rtMesh;
                if (rtMesh != null)
                {
                    // Get hit triangle corners and find the closest hit
                    rtMesh.CollectTriangleCorners(objectHit.rtMeshHit.subMeshIndex, objectHit.rtMeshHit.triIndex,
                        objectHit.gameObject.transform.localToWorldMatrix, sVec3Buffer);
                    int closestPt = objectHit.point.FindClosestPoint(sVec3Buffer);

                    // If we found a closest point, use it to snap the pivot
                    if (closestPt >= 0)
                    {
                        snappedPivot = sVec3Buffer[closestPt];
                        return true;
                    }
                }
                else
                // Did we hit a sprite?
                if (objectHit.spriteRenderer != null)
                {
                    // We've hit a sprite
                    SpriteRenderer r = objectHit.spriteRenderer;

                    // Calculate the sprite's OBB and find the OBB point closest to the hit point
                    OBox obb = r.gameObject.CalculateSpriteWorldOBB();
                    if (!obb.isValid) return false;
                    obb.CalculateCorners(sVec3Buffer);
                    sVec3Buffer.Add(obb.center);
                    int closestPt = objectHit.point.FindClosestPoint(sVec3Buffer);

                    // If we found a closest point, use it to snap the pivot
                    if (closestPt >= 0)
                    {
                        snappedPivot = sVec3Buffer[closestPt];
                        return true;
                    }
                }
                else
                // Did we hit a grid?
                if (sceneHit.hasGridHit)
                {
                    // Snap to grid
                    snappedPivot = Snap.GridSnapAxes(sceneHit.gridHit.point, sceneHit.gridHit.grid.snapDesc, new Vector3Int(1, 0, 1));
                    return true;
                }
            }

            // No hit
            return false;
        }
        #endregion
    }
    #endregion
}