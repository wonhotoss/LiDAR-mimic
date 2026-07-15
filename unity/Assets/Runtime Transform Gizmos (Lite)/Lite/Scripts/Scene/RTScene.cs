using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
#if RTGUI_PRESENT
using RTGUIFramework;
#endif

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: SceneRayHit (Public Struct)
    // Desc: Stores information for a scene ray hit.
    //-----------------------------------------------------------------------------
    public struct SceneRayHit
    {
        #region Public Fields
        public ObjectRayHit objectHit;  // Object hit
        public RTGridRayHit gridHit;    // Grid hit
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: anyHit (Public Property)
        // Desc: Returns true if there was a hit (object or grid or both) and false
        //       otherwise.
        //-----------------------------------------------------------------------------
        public bool     anyHit          { get { return objectHit.gameObject != null || gridHit.grid != null; } }

        //-----------------------------------------------------------------------------
        // Name: hasObjectHit (Public Property)
        // Desc: Returns true if an object was hit and false otherwise.
        //-----------------------------------------------------------------------------
        public bool     hasObjectHit    { get { return objectHit.gameObject != null; } }

        //-----------------------------------------------------------------------------
        // Name: hasGridHit (Public Property)
        // Desc: Returns true if a grid was hit and false otherwise.
        //-----------------------------------------------------------------------------
        public bool     hasGridHit      { get { return gridHit.grid != null; } }

        //-----------------------------------------------------------------------------
        // Name: closestHit (Public Property)
        // Desc: Returns the closest hit point. If no entity was hit, this returns the
        //       zero vector.
        //-----------------------------------------------------------------------------
        public Vector3  closestHit
        {
            get
            {
                // Valid object hit?
                if (objectHit.gameObject != null)
                {
                    // Valid grid hit? Then return closest hit.
                    if (gridHit.grid != null)
                        return gridHit.t <= objectHit.t ? gridHit.point : objectHit.point;

                    // Return the object hit
                    return objectHit.point;
                }
                else
                // Valid grid hit?
                if (gridHit.grid != null)
                    return gridHit.point;

                // No hit
                return Vector3.zero;
            }
        }
        #endregion
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTScene (Public Singleton Class)
    // Desc: Implements scene management and query functionality.
    //-----------------------------------------------------------------------------
    public class RTScene : MonoSingleton<RTScene>
    {
        #region Private Fields     
        SceneObjectTree     mObjectTree             = new SceneObjectTree(); // The object tree which is used to speed up queries such as raycasts and overlap tests

        // Indicates whether pointer interaction has been explicitly captured
        // during the current frame by a non-scene system (e.g. UI element,
        // view gizmo label, custom overlay control).
        bool                mPointerInteractionCaptured;

        // Buffers used to avoid memory allocations
        List<GameObject>    mRootBuffer             = new List<GameObject>();
        List<GameObject>    mObjectBuffer           = new List<GameObject>();
        List<RaycastResult> mRaycastResultBuffer    = new List<RaycastResult>();
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Internal_Update() (Public Function)
        // Desc: Called by the system to allow the scene to update itself.
        //-----------------------------------------------------------------------------
        public void Internal_Update()
        {
            mPointerInteractionCaptured = false;
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_LoadCurrent() (Public Function)
        // Desc: Called by the system to load the currently active scene.
        //-----------------------------------------------------------------------------
        public void Internal_LoadCurrent()
        {
            // Clear old data
            mObjectTree.Clear();

            // Get all root objects in the active scene
            Scene activeScene       = SceneManager.GetActiveScene();
            mRootBuffer.Capacity    = activeScene.rootCount;
            activeScene.GetRootGameObjects(mRootBuffer);

            // Loop through each root object and register its hierarchy
            int rootCount = mRootBuffer.Count;
            for (int i = 0; i < rootCount; ++i)
                RegisterObjectHierarchy(mRootBuffer[i]);

            // Clear data
            mRootBuffer.Clear();
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_Unload() (Public Function)
        // Desc: Called by the system to unload the scene.
        //-----------------------------------------------------------------------------
        public void Internal_Unload()
        {
            mObjectTree.Clear();
        }

        //-----------------------------------------------------------------------------
        // Name: SetObjectTRS() (Public Function)
        // Desc: Sets an object's TRS data and calls 'OnObjectTransformChanged' afterwards.
        // Parm: gameObject - The game object whose transform is affected.
        //       position   - Absolute position.
        //       rotation   - Absolute rotation.
        //       scale      - Absolute scale.
        //-----------------------------------------------------------------------------
        public void SetObjectTRS(GameObject gameObject, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            // Set TRS
            Transform t = gameObject.transform;
            t.position = position;
            t.rotation = rotation;
            t.SetScale(scale);

            // Notify
            OnObjectTransformChanged(gameObject);
        }

        //-----------------------------------------------------------------------------
        // Name: SetObjectPosition() (Public Function)
        // Desc: Sets an object's position and calls 'OnObjectTransformChanged' afterwards.
        // Parm: gameObject - The game object whose transform is affected.
        //       position   - Absolute position.
        //-----------------------------------------------------------------------------
        public void SetObjectPosition(GameObject gameObject, Vector3 position)
        {
            // Set position and notify
            gameObject.transform.position = position;
            OnObjectTransformChanged(gameObject);
        }

        //-----------------------------------------------------------------------------
        // Name: MoveObject() (Public Function)
        // Desc: Moves an object by the specified offset and calls 'OnObjectTransformChanged'
        //       afterwards.
        // Parm: gameObject - The game object whose transform is affected.
        //       offset     - Offset to apply to the object's absolute position.
        //-----------------------------------------------------------------------------
        public void MoveObject(GameObject gameObject, Vector3 offset)
        {
            // Move object and notify
            gameObject.transform.position += offset;
            OnObjectTransformChanged(gameObject);
        }

        //-----------------------------------------------------------------------------
        // Name: SetObjectRotation() (Public Function)
        // Desc: Sets an object's rotation and calls 'OnObjectTransformChanged' afterwards.
        // Parm: gameObject - The game object whose transform is affected.
        //       rotation   - Absolute rotation.
        //-----------------------------------------------------------------------------
        public void SetObjectRotation(GameObject gameObject, Quaternion rotation)
        {
            // Set rotation and notify
            gameObject.transform.rotation = rotation;
            OnObjectTransformChanged(gameObject);
        }

        //-----------------------------------------------------------------------------
        // Name: RotateObject() (Public Function)
        // Desc: Rotates an object and calls 'OnObjectTransformChanged' afterwards.
        // Parm: gameObject - The game object whose transform is affected.
        //       rotation   - Rotation to apply to the object's absolute rotation.
        //-----------------------------------------------------------------------------
        public void RotateObject(GameObject gameObject, Quaternion rotation)
        {
            // Rotate object and notify
            gameObject.transform.rotation = rotation * gameObject.transform.rotation;
            OnObjectTransformChanged(gameObject);
        }

        //-----------------------------------------------------------------------------
        // Name: RotateObjectAroundPivot() (Public Function)
        // Desc: Rotates an object and calls 'OnObjectTransformChanged' afterwards.
        // Parm: gameObject - The game object whose transform is affected.
        //       rotation   - Rotation to apply to the object's absolute rotation.
        //       pivot      - Rotation pivot.
        //-----------------------------------------------------------------------------
        public void RotateObjectAroundPivot(GameObject gameObject, Quaternion rotation, Vector3 pivot)
        {
            // Rotate object and notify
            gameObject.transform.RotateAroundPivot(rotation, pivot);
            OnObjectTransformChanged(gameObject);
        }

        //-----------------------------------------------------------------------------
        // Name: SetObjectScale() (Public Function)
        // Desc: Sets an object's scale and calls 'OnObjectTransformChanged' afterwards.
        // Parm: gameObject - The game object whose transform is affected.
        //       scale      - Absolute scale.
        //-----------------------------------------------------------------------------
        public void SetObjectScale(GameObject gameObject, Vector3 scale)
        {
            // Set scale and notify
            gameObject.transform.SetScale(scale);
            OnObjectTransformChanged(gameObject);
        }

        //-----------------------------------------------------------------------------
        // Name: OnObjectTransformChanged() (Public Function)
        // Desc: Must be called when the transform of an object changes.
        // Parm: gameObject - The game object whose transform changed.
        //-----------------------------------------------------------------------------
        public void OnObjectTransformChanged(GameObject gameObject)
        {
            mObjectTree.OnObjectTransformChanged(gameObject);
        }

        //-----------------------------------------------------------------------------
        // Name: OnObjectComponentsChanged() (Public Function)
        // Desc: Must be called whenever adding/removing components to/from a game object.
        // Parm: gameObject - The game object whose components changed.
        //-----------------------------------------------------------------------------
        public void OnObjectComponentsChanged(GameObject gameObject)
        {
            // Notify object that its type has changed
            gameObject.OnObjectTypeDirty();

            // Notify scene object tree. The object may need to be removed from the tree or re-integrated.
            mObjectTree.OnObjectComponentsChanged(gameObject);
        }

        //-----------------------------------------------------------------------------
        // Name: RegisterObjectHierarchies() (Public Function)
        // Desc: Registers multiple object hierarchies with the scene tree.
        // Parm: roots - Collection of hierarchy roots.
        //-----------------------------------------------------------------------------
        public void RegisterObjectHierarchies(IReadOnlyList<GameObject> roots)
        {
            // Loop through each root and register it
            int count = roots.Count;
            for (int r = 0; r < count; ++r)
                RegisterObjectHierarchy(roots[r]);
        }

        //-----------------------------------------------------------------------------
        // Name: RegisterObjectHierarchy() (Public Function)
        // Desc: Must be called whenever a new object hierarchy is created.
        // Parm: root - The root of the created hierarchy.
        //-----------------------------------------------------------------------------
        public void RegisterObjectHierarchy(GameObject root)
        {
            // Get all object in hierarchy
            root.CollectMeAndChildren(true, true, mObjectBuffer);

            // Loop through each object
            int objectCount = mObjectBuffer.Count;
            for (int i = 0; i < objectCount; ++i)
            {
                // Register this object with the scene tree
                mObjectTree.RegisterObject(mObjectBuffer[i]);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: UnregisterObjectHierarchy() (Public Function)
        // Desc: Must be called whenever a game object is about to be destroyed.
        // Parm: root - The root of the hierarchy which is about to be destroyed.
        //-----------------------------------------------------------------------------
        public void UnregisterObjectHierarchy(GameObject root)
        {
            // Get all objects in the hierarchy
            root.CollectMeAndChildren(true, true, mObjectBuffer);

            // Loop through each object
            int objectCount = mObjectBuffer.Count;
            for (int i = 0; i < objectCount; ++i)
            {
                // Remove object from the scene tree
                mObjectTree.UnregisterObject(mObjectBuffer[i]);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: BoxCollect() (Public Function)
        // Desc: Collects all scene objects whose bounding volumes intersect or are 
        //       fully contained within the specified oriented box.
        // Parm: box         - The oriented box used for intersection testing.
        //       filter      - Optional object filter. Can be null to include all objects.
        //       gameObjects - Receives the collected objects.
        // Rtrn: True if at least one object was collected; false otherwise.
        //-----------------------------------------------------------------------------
        public bool BoxCollect(OBox box, ObjectFilter filter, List<GameObject> gameObjects)
        {
            return mObjectTree.BoxCollect(box, filter, gameObjects);
        }

        //-----------------------------------------------------------------------------
        // Name: BoxOverlap() (Public Function)
        // Desc: Checks if the specified box overlaps with at least one scene object.
        // Parm: box     - The oriented box used for intersection testing.
        //       filter  - Optional filter used to include/exclude objects. Can be null.
        // Rtrn: True if at least one object is overlapped; false otherwise.
        //-----------------------------------------------------------------------------
        public bool BoxOverlap(OBox box, ObjectFilter filter)
        {
            return mObjectTree.BoxOverlap(box, filter);
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Performs a raycast and returns the information about the closest hit.
        // Parm: ray        - Query ray.
        //       filter     - Object filter. Can be null if no filtering is needed.
        //       raycastGrid - Specifies whether the grid should be raycast.
        //       sceneHit   - Returns the scene hit information.
        // Rtrn: True if the ray hits a scene entity and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray, ObjectFilter filter, bool raycastGrid, out SceneRayHit sceneHit)
        {
            return Raycast(ray, filter, raycastGrid, true, out sceneHit);
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Performs a raycast and returns the information about the closest hit.
        // Parm: ray             - Query ray.
        //       filter          - Object filter. Can be null if no filtering is needed.
        //       raycastGrid     - Specifies whether the grid should be raycast.
        //       ignoreBackFaces - Specifies whether mesh back faces should be ignored.
        //       sceneHit        - Returns the scene hit information.
        // Rtrn: True if the ray hits a scene entity and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray, ObjectFilter filter, bool raycastGrid, bool ignoreBackFaces, out SceneRayHit sceneHit)
        {
            // Raycast scene entities
            mObjectTree.Raycast(ray, filter, ignoreBackFaces, out sceneHit.objectHit);
            if (raycastGrid) RTGrid.get.Raycast(ray, out sceneHit.gridHit);
            else sceneHit.gridHit = new RTGridRayHit();

            // Return result
            return sceneHit.anyHit;
        }

        //-----------------------------------------------------------------------------
        // Name: ScreenRectCollectInside() (Public Function)
        // Desc: Collects all objects that lie completely within the specified screen
        //       rectangle.
        // Parm: screenRect     - The query rectangle.
        //       camera         - The camera that sees the rectangle.
        //       filter         - Object filter. Can be null if no filtering is needed.
        //       gameObjects    - Returns all objects that lie completely within the rectangle.
        // Rtrn: True if at least one object was collected and false otherwise.
        //-----------------------------------------------------------------------------
        public bool ScreenRectCollectInside(Rect screenRect, Camera camera, ObjectFilter filter, List<GameObject> gameObjects)
        {
            return mObjectTree.ScreenRectCollectInside(screenRect, camera, filter, gameObjects);
        }

        //-----------------------------------------------------------------------------
        // Name: IsUGUIHovered() (Public Function)
        // Desc: Returns true if the pointer is currently positioned over any
        //       Unity UGUI element. When true, the pointer is considered
        //       hover-owned by UGUI.
        // Rtrn: True if the pointer is hovering UGUI. False otherwise.
        //-----------------------------------------------------------------------------
        public bool IsUGUIHovered()
        {
            // No event system?
            if (EventSystem.current == null)
                return false;

            // Get the input device's screen coords. If the coords are not available, return false.
            var inputDevice = RTInput.get.pointingInputDevice;
            if (!inputDevice.hasPointer) return false;

            // Construct the pointer event data instance needed for the raycast
            PointerEventData evData = new PointerEventData(EventSystem.current);
            evData.position = inputDevice.position;

            // Raycast all and collect results
            mRaycastResultBuffer.Clear();
            EventSystem.current.RaycastAll(evData, mRaycastResultBuffer);
            mRaycastResultBuffer.RemoveAll(r => !(r.module is GraphicRaycaster));

            // Do we have a hit?
            return mRaycastResultBuffer.Count != 0;
        }

        //-----------------------------------------------------------------------------
        // Name: IsUGUIPointerCaptured() (Public Function)
        // Desc: Returns true if Unity UGUI is actively capturing pointer input
        //       through an ongoing pointer-driven interaction.
        //       When true, pointer ownership belongs to UGUI.
        // Rtrn: True if UGUI has captured pointer input. False otherwise.
        //-----------------------------------------------------------------------------
        public bool IsUGUIPointerCaptured()
        {
            // No EventSystem available?
            if (EventSystem.current == null)
                return false;

            // Pointer must be over a UGUI element
            if (!EventSystem.current.IsPointerOverGameObject())
                return false;

            // If the primary mouse button is held, UGUI is considered to be
            // consuming pointer input (e.g. dragging, slider interaction, etc.)
            if (RTInput.get.pointingInputDevice.pickButtonPressed)
                return true;

            // Otherwise, UGUI is not actively consuming pointer input
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: IsUIHovered() (Public Function)
        // Desc: Returns true if the pointer is currently positioned over any UI system.
        //       When true, the pointer is considered hover-owned by UI.
        // Rtrn: True if the pointer is hovering UI. False otherwise.
        //-----------------------------------------------------------------------------
        public bool IsUIHovered()
        {
            // Check if RTUI is hovered
            #if RTGUI_PRESENT
            if (RTUI.get != null && RTUI.get.isHovered)
                return true;
            #endif

            // Check if UGUI is hovered
            if (IsUGUIHovered())
                return true;

            // No UI system hovered
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: IsUIPointerCaptured() (Public Function)
        // Desc: Returns true if any UI system is actively capturing pointer input
        //       through an ongoing pointer-driven interaction.
        //       When true, pointer ownership belongs to UI.
        // Rtrn: True if UI has captured pointer input. False otherwise.
        //-----------------------------------------------------------------------------
        public bool IsUIPointerCaptured()
        {
            // Check if RTUI pointer is captured
            #if RTGUI_PRESENT
            if (RTUI.get != null && RTUI.get.isPointerCaptured)
                return true;
            #endif

            // Check if UGUI pointer is captured
            if (IsUGUIPointerCaptured())
                return true;

            // No UI system captured the pointer
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: IsUIPointerBlocked() (Public Function)
        // Desc: Returns true if pointer interaction should be blocked by UI.
        //       This occurs when the pointer is hovering UI or when UI has
        //       actively captured pointer input.
        // Rtrn: True if scene-level pointer logic should be suppressed due to UI.
        //-----------------------------------------------------------------------------
        public bool IsUIPointerBlocked()
        {
            return IsUIHovered() || IsUIPointerCaptured();
        }

        //-----------------------------------------------------------------------------
        // Name: IsGizmoPointerCaptured() (Public Function)
        // Desc: Returns true if the gizmo subsystem currently captures pointer input.
        //       Capture may result from hover, active drag, or GUI-space gizmo
        //       elements owning pointer interaction.
        // Rtrn: True if pointer ownership belongs to a gizmo. False otherwise.
        //-----------------------------------------------------------------------------
        public bool IsGizmoPointerCaptured()
        {
            return RTGizmos.get.hoveredGizmo != null ||
                   RTGizmos.get.draggedGizmo != null ||
                   RTGizmos.get.dragEndedThisFrame   || 
                   RTGizmos.get.IsGizmoGUIHovered();
        }

        //-----------------------------------------------------------------------------
        // Name: CapturePointerInteraction() (Public Function)
        // Desc: Marks pointer interaction as explicitly captured for the current frame.
        //       When captured, the pointer is considered owned by the caller and will
        //       be reported as blocked by 'IsPointerInteractionBlocked()'.
        //-----------------------------------------------------------------------------
        public void CapturePointerInteraction()
        {
            mPointerInteractionCaptured = true;
        }

        //-----------------------------------------------------------------------------
        // Name: IsPointerInteractionCaptured() (Public Function)
        // Desc: Returns true if pointer interaction is currently captured or owned
        //       by any system (UI, gizmos, or explicit capture).
        //       When true, pointer input should not propagate to scene-level logic.
        // Rtrn: True if pointer interaction is captured. False otherwise.
        //-----------------------------------------------------------------------------
        public bool IsPointerInteractionCaptured()
        {
            return mPointerInteractionCaptured || IsUIPointerBlocked() || IsGizmoPointerCaptured();
        }
        #endregion
    }
    #endregion
}