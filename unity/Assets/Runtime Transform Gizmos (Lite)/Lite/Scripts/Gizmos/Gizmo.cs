using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EGizmoHandleUpdateReason (Public Enum)
    // Desc: Defines different gizmo handle update reasons. When a gizmo requires
    //       its handles to be updated, a member of this enum is used to specify the
    //       reason behind the update request.
    //-----------------------------------------------------------------------------
    public enum EGizmoHandleUpdateReason
    {
        Update = 0, // The gizmo handles are updated during the frame update
        Render,     // The gizmo handles are updated before being rendered
        OnGUI,      // The gizmo handles are updated before handling the OnGUI event
    }
    #endregion

    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: GizmoRayHit (Public Struct)
    // Desc: Provides storage for gizmo ray hit.
    //-----------------------------------------------------------------------------
    public struct GizmoRayHit
    {
        #region Public Fields
        public GizmoHandle  handle; // The hit handle
        public float        t;      // The distance from the cursor's ray origin where the intersection with the handle happens
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: GizmoHoverData (Public Struct)
    // Desc: Stores gizmo hover data.
    //-----------------------------------------------------------------------------
    public struct GizmoHoverData
    {
        #region Public Fields
        public GizmoHandle  handle; // The hovered handle
        public Vector3      point;  // Hover point
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Clear() (Public Function)
        // Desc: Clears the hover data.
        //-----------------------------------------------------------------------------
        public void Clear()
        {
            handle = null;
            point  = Vector3.zero;
        }
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: GizmoDragData (Public Struct)
    // Desc: Stores gizmo drag data.
    //-----------------------------------------------------------------------------
    public struct GizmoDragData
    {
        #region Public Fields
        public GizmoHandle          handle;                 // The drag handle
        public EGizmoDragType       dragType;               // The drag type
        public EGizmoDragChannel    dragChannel;            // The drag channel (i.e. where the drag output goes)
        public Vector3              dragStartHoverPoint;    // The gizmo hover point when the drag operation started
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Clear() (Public Function)
        // Desc: Clears the drag data.
        //-----------------------------------------------------------------------------
        public void Clear()
        {
            handle = null;
        }
        #endregion
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: Gizmo (Public Abstract Class)
    // Desc: Represents a gizmo. A gizmo is a collection of handles (i.e. shapes of
    //       all kinds) which can be interacted with to manipulate object state.
    //       For example, a move gizmo can be used to change object positions, a
    //       box collider gizmo changes the collider size etc. All gizmo types must
    //       derive from the 'Gizmo' class and implement the relevant functions.
    //-----------------------------------------------------------------------------
    public abstract class Gizmo
    {
        #region Public Delegates & Events
        //-----------------------------------------------------------------------------
        // Name: EnabledStateChangedHandler() (Public Delegate)
        // Desc: Handler for the enabled state changed event fired when the gizmo's
        //       enabled state changes.
        // Parm: gizmo - The gizmo whose enabled state has changed.
        //-----------------------------------------------------------------------------
        public delegate void    EnabledStateChangedHandler(Gizmo gizmo);
        public event            EnabledStateChangedHandler enabledStateChanged;
        #endregion

        #region Private Fields
        List<GizmoBehaviour>        mBehaviours         = new List<GizmoBehaviour>();       // Gizmo behaviours
        ObjectTransformGizmo        mObjectTransformGizmo;                                  // Cached object transform behaviour

        List<GizmoHandle>           mHandles            = new List<GizmoHandle>();          // Stores all gizmo handles
        List<GizmoLineSlider>       mLineSliders        = new List<GizmoLineSlider>();      // Stores all line slider handles
        List<GizmoPlaneSlider>      mPlaneSliders       = new List<GizmoPlaneSlider>();     // Stores all plane slider handles
        List<GizmoCap>              mCaps               = new List<GizmoCap>();             // Stores all cap handles

        GizmoTransform              mTransform          = new GizmoTransform();             // Gizmo transform
        GizmoHoverRaycastArgs       mHoverRaycastArgs   = new GizmoHoverRaycastArgs();      // Used for raycasts

        GizmoHoverData              mHoverData          = new GizmoHoverData();             // Gizmo hover data
        GizmoDragData               mDragData           = new GizmoDragData();              // Gizmo drag data
        GizmoDrag                   mActiveDrag;                                            // The active drag operation. This is null when the gizmo is not being dragged.
        
        float                       mZoomScale          = 1.0f;                             // Calculated during the update and render functions
        bool                        mEnabled            = true;                             // Is the gizmo enabled?
        #endregion

        #region Protected Fields
        // Buffers used in derived classes to avoid memory allocations
        protected List<GizmoLineSlider>     mSortedSlidersBuffer            = new List<GizmoLineSlider>();
        protected List<GizmoPlaneSlider>    mSortedDblAxisSlidersBuffer     = new List<GizmoPlaneSlider>();
        protected List<GizmoCap>            mSortedCapsBuffer               = new List<GizmoCap>();
        protected List<GizmoCap>            mCapsBuffer                     = new List<GizmoCap>();
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: transform (Public Property)
        // Desc: Returns the gizmo transform.
        //-----------------------------------------------------------------------------
        public GizmoTransform   transform   { get { return mTransform; } }

        //-----------------------------------------------------------------------------
        // Name: handleCount (Public Property)
        // Desc: Returns the number of handles that make up the gizmo.
        //-----------------------------------------------------------------------------
        public int              handleCount { get { return mHandles.Count; } }

        //-----------------------------------------------------------------------------
        // Name: hoverData (Public Property)
        // Desc: Returns the gizmo hover data.
        //-----------------------------------------------------------------------------
        public GizmoHoverData   hoverData   { get { return mHoverData; } }    

        //-----------------------------------------------------------------------------
        // Name: dragData (Public Property)
        // Desc: Returns the gizmo drag data.
        //-----------------------------------------------------------------------------
        public GizmoDragData    dragData    { get { return mDragData; } }

        //-----------------------------------------------------------------------------
        // Name: hovered (Public Property)
        // Desc: Returns whether the gizmo is hovered or not.
        //-----------------------------------------------------------------------------
        public bool             hovered     { get { return mHoverData.handle != null; } }
        
        //-----------------------------------------------------------------------------
        // Name: dragging (Public Property)
        // Desc: Returns whether the gizmo is being dragged or not.
        //-----------------------------------------------------------------------------
        public bool             dragging    { get { return mDragData.handle != null; } }

        //-----------------------------------------------------------------------------
        // Name: enabled (Public Property)
        // Desc: Returns or sets whether the gizmo is enabled. When the gizmo is disabled
        //       it doesn't render and it can't be interacted with.
        //-----------------------------------------------------------------------------
        public bool             enabled 
        { 
            get { return mEnabled; }
            set
            {
                // No-op?
                if (value == mEnabled) return;

                // Set new state
                mEnabled = value;

                // Update data
                if (!mEnabled)
                {
                    Internal_EndDrag();
                    Internal_EndHover();
                }

                // Fire event
                if (enabledStateChanged != null)
                    enabledStateChanged(this);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: objectTransformGizmo (Public Property)
        // Desc: Returns the object transform gizmo behaviour. If this behaviour doesn't
        //       exist, null will be returned.
        //-----------------------------------------------------------------------------
        public ObjectTransformGizmo     objectTransformGizmo        { get { return mObjectTransformGizmo; } }

        //-----------------------------------------------------------------------------
        // Name: internal_handles (Public Property)
        // Desc: Returns the list which contains all handles. Used internally.
        //-----------------------------------------------------------------------------
        public List<GizmoHandle>         internal_handles           { get { return mHandles; } }

        //-----------------------------------------------------------------------------
        // Name: internal_lineSliders (Public Property)
        // Desc: Returns the list which contains all line sliders. Used internally.
        //-----------------------------------------------------------------------------
        public List<GizmoLineSlider>     internal_lineSliders       { get { return mLineSliders; } }

        //-----------------------------------------------------------------------------
        // Name: internal_planeSliders (Public Property)
        // Desc: Returns the list which contains all plans sliders. Used internally.
        //-----------------------------------------------------------------------------
        public List<GizmoPlaneSlider>    internal_planeSliders      { get { return mPlaneSliders; } }

        //-----------------------------------------------------------------------------
        // Name: internal_caps (Public Property)
        // Desc: Returns the list which contains all caps. Used internally.
        //-----------------------------------------------------------------------------
        public List<GizmoCap>            internal_caps              { get { return mCaps; } }

        //-----------------------------------------------------------------------------
        // Name: internal_zoomScale (Public Property)
        // Desc: Returns the gizmo's zoom scale. Used internally.
        //-----------------------------------------------------------------------------
        public float                     internal_zoomScale         { get { return mZoomScale; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Internal_Init() (Public Function)
        // Desc: Called by the system to allow the gizmo to initialize itself.
        //-----------------------------------------------------------------------------
        public void Internal_Init()
        {
            // Create the handles
            OnCreateHandles(mHandles);

            // Identify handle types and make sure all handles are direct children of the gizmo's transform
            int count = mHandles.Count;
            for (int i = 0; i < count; ++i)
            {
                // Check handle type
                if (mHandles[i] is GizmoLineSlider)       mLineSliders.Add(mHandles[i] as GizmoLineSlider);
                else if (mHandles[i] is GizmoPlaneSlider) mPlaneSliders.Add(mHandles[i] as GizmoPlaneSlider);
                else if (mHandles[i] is GizmoCap)         mCaps.Add(mHandles[i] as GizmoCap);

                // Attach handle transform to gizmo transform
                mHandles[i].transform.parent = mTransform;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_Update() (Public Function)
        // Desc: Called by the system to allow the gizmo to update itself.
        //-----------------------------------------------------------------------------
        public void Internal_Update()
        {
            // Update zoom scale
            Camera camera = GetInteractionCamera();
            mZoomScale = RTGizmos.CalculateZoomScale(transform.position, camera);

            // Are we enabled?
            if (!mEnabled)
                return;

            // Not ready?
            if (!IsReady())
                return;

            // Can't update?
            var activeGizmoStyle = GetActiveGizmoStyle();
            if (!activeGizmoStyle.hoverable && !activeGizmoStyle.visible) // Note: If at least one is true, we can continue.
            {
                Internal_EndDrag();
                Internal_EndHover();
                return;
            }
           
            // If not hoverable, end drag and hover
            if (!activeGizmoStyle.hoverable)
            {
                Internal_EndDrag();
                Internal_EndHover();
            }

            // Cache data
            var     globalStyle = RTGizmos.get.skin.globalGizmoStyle;
            float   handleScale = globalStyle.scale * activeGizmoStyle.scale;
     
            // Apply style scale
            int count = mHandles.Count;
            for (int i = 0; i < count; ++i)
                mHandles[i].transform.localScale = handleScale;

            // Are we dragging?
            if (mActiveDrag != null)
            {
                // Update active drag
                if (mActiveDrag.Update())
                {
                    // Notify gizmo
                    OnDrag(mActiveDrag);

                    // Notify behaviours
                    count = mBehaviours.Count;
                    for (int i = 0; i < count; ++i)
                        mBehaviours[i].Internal_OnDrag(mActiveDrag);
                }
            }

            // Update handles
            OnUpdateHandles(EGizmoHandleUpdateReason.Update, camera);
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_OnClickedHoveredHandle() (Public Function)
        // Desc: Called by the system when the user clicks/taps the hovered handle. 
        //-----------------------------------------------------------------------------
        public void Internal_OnClickedHoveredHandle()
        {
            OnClickedHoveredHandle();
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_OnGUI() (Public Function)
        // Desc: Called during the 'OnGUI' event to allow the gizmo to draw GUI elements.
        // Parm: camera - The camera that renders the GUI.
        //-----------------------------------------------------------------------------
        public void Internal_OnGUI(Camera camera)
        {
            // Update zoom scale
            mZoomScale = RTGizmos.CalculateZoomScale(transform.position, camera);

            // Are we enabled?
            if (!mEnabled)
                return;

            // Not visible?
            var activeStyle = GetActiveGizmoStyle();
            if (!activeStyle.visible)
                return;

            // Not ready?
            if (!IsReady())
                return;

            // Is the camera valid?
            if (camera != GetOnGUICamera())
                return;

            // Update handles
            OnUpdateHandles(EGizmoHandleUpdateReason.OnGUI, camera);

            // Do GUI rendering
            OnGUI(camera);
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_Render() (Public Function)
        // Desc: Called by the system to allow the gizmo to render itself.
        // Parm: camera    - The camera which renders the gizmo.
        //       rasterCtx - The raster graph context.
        //-----------------------------------------------------------------------------
        public void Internal_Render(Camera camera, RasterGraphContext rasterCtx)
        {
            // Update zoom scale
            mZoomScale = RTGizmos.CalculateZoomScale(transform.position, camera);

            // Are we enabled?
            if (!mEnabled)
                return;

            // Not visible?
            var activeStyle = GetActiveGizmoStyle();
            if (!activeStyle.visible)
                return;

            // Not ready?
            if (!IsReady())
                return;

            // Is the camera allowed to render this gizmo?
            if (!AcceptRenderCamera(camera))
                return;

            // Update handles
            OnUpdateHandles(EGizmoHandleUpdateReason.Render, camera);

            // Render
            OnRender(camera, rasterCtx);
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_HoverRaycast() (Public Function)
        // Desc: Called by the system to check if the gizmo is hovered by the cursor by
        //       performing a raycast. The function also updates the gizmo's hover data.
        // Parm: rayHit - Returns the ray hit information. Should be ignored if the
        //                function returns false.
        // Rtrn: True if a gizmo handle is hit by the ray and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Internal_HoverRaycast(out GizmoRayHit rayHit)
        {
            // Clear output data
            rayHit = new GizmoRayHit();

            // Clear hover data
            mHoverData.Clear();

            // Are we enabled?
            if (!mEnabled)
                return false;

            // Not ready?
            if (!IsReady())
                return false;

            // Can't hover?
            var activeStyle = GetActiveGizmoStyle();
            if (!activeStyle.hoverable || !activeStyle.visible)
                return false;

            // If the interaction camera doesn't render gizmos, then it can't interact with them either
            Camera camera = GetInteractionCamera();
            RTCamera.get.GetCameraRenderConfig(camera, out RTCameraRenderConfig renderConfig);
            if ((renderConfig.renderFlags & ECameraRenderFlags.Gizmos) == 0)
                return false;

            // Keeps track of the closest handle
            float tMin = float.MaxValue;
            GizmoHandle hitHandle = null;

            // Calculate ray
            Ray ray = GetPickRay(camera);

            // Set raycast args
            mHoverRaycastArgs.camera    = camera;
            mHoverRaycastArgs.ray       = ray;

            // Loop through each handle
            int count = mHandles.Count;
            for (int i = 0; i < count; ++i)
            {
                // Raycast
                if (mHandles[i].HoverRaycast(mHoverRaycastArgs, out float t))
                {
                    // Is this intersection closer?
                    // A closer intersection can mean any of the following:
                    //      a) the hit handle has a higher hover priority.
                    //      b) the hit handle has the same priority but the hit distance is smaller.
                    if (hitHandle == null || mHandles[i].hoverPriority > hitHandle.hoverPriority || 
                        (mHandles[i].hoverPriority == hitHandle.hoverPriority && t < tMin))
                    {
                        hitHandle = mHandles[i];
                        tMin = t;
                    }
                }
            }

            // Create ray hit
            rayHit = new GizmoRayHit { t = tMin, handle = hitHandle };

            // Update hover data
            mHoverData.handle = hitHandle;
            mHoverData.point = ray.GetPoint(tMin);

            // Return result
            return rayHit.handle != null;
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_StartDrag() (Public Function)
        // Desc: Called by the system to notify the gizmo that it should start a drag
        //       operation. The function has no effect if the gizmo is not hovered.
        // Rtrn: The gizmo drag operation which started or null of no drag operation was
        //       possible.
        //-----------------------------------------------------------------------------
        public GizmoDrag Internal_StartDrag()
        {
            // Are we enabled?
            if (!mEnabled)
                return null;

            // Not hovered?
            if (mHoverData.handle == null)
                return null;

            // Not ready?
            if (!IsReady())
                return null;

            // Start a drag
            mDragData.handle                = mHoverData.handle;
            mDragData.dragStartHoverPoint   = mHoverData.point;
            mActiveDrag                     = OnStartDrag(GetInteractionCamera());

            // If we couldn't start dragging, clear drag data
            if (mActiveDrag == null) mDragData.Clear();
            else
            {
                // Otherwise, complete data
                mDragData.dragChannel           = mActiveDrag.dragChannel;
                mDragData.dragType              = mActiveDrag.desc.dragType;       
                
                // Notify behaviours
                int count = mBehaviours.Count;
                for (int i = 0; i < count; ++i)
                    mBehaviours[i].Internal_OnDragStart(mActiveDrag);
            }

            // Return result
            return mActiveDrag;
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_EndDrag() (Public Function)
        // Desc: Called by the system to notify the gizmo that it should end the current
        //       drag operation.
        //-----------------------------------------------------------------------------
        public void Internal_EndDrag()
        {
            // No-op?
            if (mActiveDrag == null)
                return;

            // End drag
            mActiveDrag.End();
            OnDragEnd(mActiveDrag);

            // Notify behaviours
            int count = mBehaviours.Count;
            for (int i = 0; i < count; ++i)
                mBehaviours[i].Internal_OnDragEnd(mActiveDrag);

            // Clear drag data
            mDragData.Clear();
            mActiveDrag = null;
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_EndHover() (Public Function)
        // Desc: Called by the system to notify the gizmo that it is no longer hovered.
        //-----------------------------------------------------------------------------
        public void Internal_EndHover()
        {
            // Clear hover data
            mHoverData.Clear();
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateAABB() (Public Function)
        // Desc: Calculates and returns the gizmo's AABB. Only the visible handles contribute
        //       to the gizmo's AABB.
        // Parm: camera - The camera that interacts with or renders the gizmo.
        // Rtrn: The gizmo's AABB or an invalid AABB if the gizmo doesn't have any handles
        //       or if all handles are invisible.
        //-----------------------------------------------------------------------------
        public Box CalculateAABB(Camera camera)
        {
            // No handles?
            if (mHandles.Count == 0)
                return Box.GetInvalid();

            // Loop through each handle and enclose it's AABB
            int count = mHandles.Count;
            Box aabb = Box.GetInvalid();
            for (int i = 0; i < count; ++i)
            {
                // Enclose handle AABB if visible
                if (mHandles[i].canRender)
                {
                    Box handleAABB = mHandles[i].CalculateAABB(camera);
                    if (aabb.isValid) aabb.EncloseBox(handleAABB);
                    else aabb = handleAABB;
                }
            }

            // Return AABB
            return aabb;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateBehaviour() (Public Function)
        // Desc: Creates a gizmo behaviour of the specified type. If a behaviour of the
        //       same type already exists, the existing one will be returned.
        // Parm: T - Gizmo behaviour type. Must derive from 'GizmoBehaviour'.
        // Rtrn: The created gizmo behaviour or the existing one if it already exists.
        //-----------------------------------------------------------------------------
        public T CreateBehaviour<T>() where T : GizmoBehaviour, new()
        {
            // Do we already have this behaviour?
            if (typeof(T) == typeof(ObjectTransformGizmo) &&
                mObjectTransformGizmo != null) return mObjectTransformGizmo as T;

            // Create behaviour
            T behaviour = new T();
            behaviour.Internal_Init(this);

            // Cache behaviour
            if (typeof(T) == typeof(ObjectTransformGizmo))
                mObjectTransformGizmo = behaviour as ObjectTransformGizmo;

            // Store behaviour and return it
            mBehaviours.Add(behaviour);
            return behaviour;
        }
        #endregion

        #region Public Virtual Functions
        //-----------------------------------------------------------------------------
        // Name: IsGUIHovered() (Public Virtual Function)
        // Desc: Checks if the gizmo's GUI is hovered. The gizmo GUI is whatever gets
        //       drawn inside 'OnGUI' that the user can interact with.
        // Rtrn: True if the gizmo's GUI is hovered and false otherwise.
        //-----------------------------------------------------------------------------
        public virtual bool IsGUIHovered()
        {
            return false;
        }
        #endregion

        #region Protected Virtual Functions
        //-----------------------------------------------------------------------------
        // Name: AcceptRenderCamera() (Protected Virtual Function)
        // Desc: Called before the gizmo is about to be rendered by the specified camera
        //       in order to check if the camera is allowed to render the gizmo.
        // Parm: renderCamera - The camera that wants to render the gizmo.
        // Rtrn: True if 'renderCamera' is allowed to render the gizmo and false otherwise.
        //-----------------------------------------------------------------------------
        protected virtual bool AcceptRenderCamera(Camera renderCamera) 
        {
            // Most gizmos will be rendered by a regular camera which is not a view gizmo camera
            return !RTGizmos.get.IsViewGizmoRenderCamera(renderCamera); 
        }

        //-----------------------------------------------------------------------------
        // Name: GetInteractionCamera() (Protected Virtual Function)
        // Desc: Returns the camera that interacts with the gizmo.
        // Rtrn: The camera that interacts with the gizmo.
        //-----------------------------------------------------------------------------
        protected virtual Camera GetInteractionCamera()
        {
            // Most gizmos will interact with the current active camera
            return RTCamera.get.settings.targetCamera;
        }

        //-----------------------------------------------------------------------------
        // Name: GetOnGUICamera() (Protected Virtual Function)
        // Desc: Returns the camera that is allowed to render the gizmo GUI during 'OnGUI'.
        //-----------------------------------------------------------------------------
        protected virtual Camera GetOnGUICamera()
        {
            return RTCamera.get.settings.targetCamera;
        }

        //-----------------------------------------------------------------------------
        // Name: GetPickRay() (Protected Virtual Function)
        // Desc: Calculates and returns a ray that can be used to pick gizmo handles.
        // Parm: camera - The camera that interacts with or renders the gizmo.
        // Rtrn: A ray that can be used to pick gizmo handles.
        //-----------------------------------------------------------------------------
        protected virtual Ray GetPickRay(Camera camera)
        {
            return RTInput.get.pointingInputDevice.GetPickRay(camera);
        }

        //-----------------------------------------------------------------------------
        // Name: IsReady() (Protected Virtual Function)
        // Desc: Checks if the gizmo is ready to use.
        // Rtrn: True if the gizmo is ready to use and false otherwise.
        //-----------------------------------------------------------------------------
        protected virtual bool IsReady()
        {
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: OnDrag() (Protected Virtual Function)
        // Desc: Called when the gizmo is dragged.
        // Parm: drag - The active drag operation.
        //-----------------------------------------------------------------------------
        protected virtual void OnDrag(GizmoDrag drag) { }

        //-----------------------------------------------------------------------------
        // Name: OnDragEnd() (Protected Virtual Function)
        // Desc: Called when the gizmo's drag operation ends.
        // Parm: drag - The drag operation that ended.
        //-----------------------------------------------------------------------------
        protected virtual void OnDragEnd(GizmoDrag drag) { }

        //-----------------------------------------------------------------------------
        // Name: OnClickedHoveredHandle() (Protected Virtual Function)
        // Desc: Called when the user clicks/taps the hovered handle.
        //-----------------------------------------------------------------------------
        protected virtual void OnClickedHoveredHandle() { }

        //-----------------------------------------------------------------------------
        // Name: OnGUI() (Protected Virtual Function)
        // Desc: Called during the 'OnGUI' event to allow the gizmo to draw GUI elements.
        // Parm: camera - The camera that renders the GUI.
        //-----------------------------------------------------------------------------
        protected virtual void OnGUI(Camera camera) { }
        #endregion

        #region Protected Abstract Functions
        //-----------------------------------------------------------------------------
        // Name: GetActiveGizmoStyle() (Protected Abstract Function)
        // Desc: Returns the active gizmo style.
        //-----------------------------------------------------------------------------
        protected abstract GizmoStyle GetActiveGizmoStyle();

        //-----------------------------------------------------------------------------
        // Name: OnUpdateHandles() (Protected Abstract Function)
        // Desc: Called to allow the gizmo to update its handles. This could mean setting
        //       handle styles, updating their transforms etc. This function is called
        //       once for each camera that renders the gizmo. It is also called once during
        //       the update step to allow for correct interaction between the active camera
        //       and the gizmo handles.
        // Parm: updateReason  - The reason behind the update request.
        //       camera        - The camera that interacts with or renders the gizmo.
        //-----------------------------------------------------------------------------
        protected abstract void OnUpdateHandles(EGizmoHandleUpdateReason updateReason, Camera camera);

        //-----------------------------------------------------------------------------
        // Name: OnCreateHandles() (Protected Abstract Function)
        // Desc: Called during gizmo initialization to allow the gizmo to create all the
        //       handles that it needs. On function return, these handles will be made
        //       children of the gizmo transform.
        // Parm: handles - Returns the list of handles.
        //-----------------------------------------------------------------------------
        protected abstract void OnCreateHandles(List<GizmoHandle> handles);

        //-----------------------------------------------------------------------------
        // Name: OnRender() (Protected Abstract Function)
        // Desc: Called when the gizmo must be rendered.
        // Parm: camera    - The camera which renders the gizmo.
        //       rasterCtx - The raster graph context.
        //-----------------------------------------------------------------------------
        protected abstract void OnRender(Camera camera, RasterGraphContext rasterCtx);

        //-----------------------------------------------------------------------------
        // Name: OnStartDrag() (Protected Abstract Function)
        // Desc: Called to notify the gizmo that a drag operation can start.
        // Parm: camera - The camera that interacts with the gizmo.
        // Rtrn: An instance 'GizmoDrag' if the drag operation can start and null otherwise.
        //-----------------------------------------------------------------------------
        protected abstract GizmoDrag OnStartDrag(Camera camera);
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: SetHandlesVisible() (Protected Function)
        // Desc: Sets the visibility state for all handles.
        // Parm: visible - The desired handle visibility state.
        //-----------------------------------------------------------------------------
        protected void SetHandlesVisible(bool visible)
        {
            int count = mHandles.Count;
            for (int i = 0; i < count; ++i)
                mHandles[i].visible = visible;
        }

        //-----------------------------------------------------------------------------
        // Name: ArrayHasDragHandle() (Protected Function)
        // Desc: Checks if the specified handle array contains the drag handle.
        // Parm: T          - Handle type. Must derive from 'GizmoHandle'.
        //       handles    - Array of handles.
        // Rtrn: True if the array contains the drag handle and false otherwise.
        //-----------------------------------------------------------------------------
        protected bool ArrayHasDragHandle<T>(T[] handles) where T : GizmoHandle
        {
            // Not dragging?
            if (dragData.handle == null)
                return false;

            // Loop through each handle in the array
            int count = handles.Length;
            for (int i = 0; i < count; ++i)
            {
                // Match?
                if (handles[i] == dragData.handle)
                    return true;
            }

            // No match
            return false;
        }
        #endregion
    }
    #endregion
}
