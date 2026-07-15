using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: TRSGizmo (Public Class)
    // Desc: Implements a gizmo which can be used to move, rotate and scale entities.
    //-----------------------------------------------------------------------------
    public class TRSGizmo : Gizmo
    {
        #region Public Delegates & Events
        //-----------------------------------------------------------------------------
        // Name: MoveModeChangedHandler() (Public Delegate)
        // Desc: Handler for the move mode changed event.
        // Parm: oldMoveMode - The old move mode.
        //       gizmo       - The gizmo that fires the event.
        //-----------------------------------------------------------------------------
        public delegate void    MoveModeChangedHandler(EGizmoMoveMode oldMoveMode, Gizmo gizmo);
        public event            MoveModeChangedHandler moveModeChanged;
        #endregion

        #region Private Fields
        MoveGizmoProxy      mMoveProxy;     // Move proxy which implements the move functionality
        RotateGizmoProxy    mRotateProxy;   // Rotate proxy which implements the rotate functionality
        ScaleGizmoProxy     mScaleProxy;    // Scale proxy which implements the scale functionality
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: style (Public Property)
        // Desc: Returns or sets the TRS gizmo style.
        //-----------------------------------------------------------------------------
        public TRSGizmoStyle style          { get; set; } = null;

        //-----------------------------------------------------------------------------
        // Name: activeStyle (Public Property)
        // Desc: Returns the active style used by the gizmo. This is 'style' if it was set
        //       to a valid style. Otherwise, it is the active skin's TRS gizmo style.
        //-----------------------------------------------------------------------------
        public TRSGizmoStyle activeStyle    { get { return style == null ? RTGizmos.get.skin.trsGizmoStyle : style; } }

        //-----------------------------------------------------------------------------
        // Name: moveMode (Public Property)
        // Desc: Returns or sets the TRS move gizmo mode.
        //-----------------------------------------------------------------------------
        public EGizmoMoveMode moveMode      { get { return mMoveProxy.moveMode; } set { if (value != mMoveProxy.moveMode) { var oldMode = mMoveProxy.moveMode; mMoveProxy.moveMode = value; OnMoveModeChanged(oldMode); } } }

        //-----------------------------------------------------------------------------
        // Name: vSnapPivotObjects (Public Property)
        // Desc: Returns or sets the vertex snap pivot objects list. Required for vertex
        //       snapping. The gizmo doesn't modify this list. It uses it only to select
        //       the snap pivot when the vertex snap mode is enabled.
        //-----------------------------------------------------------------------------
        public IReadOnlyList<GameObject> vSnapPivotObjects { get { return mMoveProxy.vSnapPivotObjects; } set { mMoveProxy.vSnapPivotObjects = value; } }
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: OnDragEnd() (Protected Function)
        // Desc: Called when the gizmo's drag operation ends.
        // Parm: drag - The drag operation that ended.
        //-----------------------------------------------------------------------------
        protected override void OnDragEnd(GizmoDrag drag)
        {
            mMoveProxy.OnDragEnd(drag);
        }

        //-----------------------------------------------------------------------------
        // Name: GetActiveGizmoStyle() (Protected Function)
        // Desc: Returns the active gizmo style.
        //-----------------------------------------------------------------------------
        protected override GizmoStyle GetActiveGizmoStyle()
        {
            return activeStyle;
        }

        //-----------------------------------------------------------------------------
        // Name: OnCreateHandles() (Protected Function)
        // Desc: Called during gizmo initialization to allow the gizmo to create all the
        //       handles that it needs. On function return, these handles will be made
        //       children of the gizmo transform.
        // Parm: handles - Returns the list of handles.
        //-----------------------------------------------------------------------------
        protected override void OnCreateHandles(List<GizmoHandle> handles)
        {
            // Create the proxies and create handles
            mMoveProxy = new MoveGizmoProxy(this, 10);
            mRotateProxy = new RotateGizmoProxy(this, 0);
            mScaleProxy = new ScaleGizmoProxy(this, 20);
            mMoveProxy.OnCreateHandles(handles);
            mRotateProxy.OnCreateHandles(handles);
            mScaleProxy.OnCreateHandles(handles);
        }

        //-----------------------------------------------------------------------------
        // Name: OnUpdateHandles() (Protected Function)
        // Desc: Called to allow the gizmo to update its handles. This could mean setting
        //       handle styles, updating their transforms etc. This function is called
        //       once for each camera that renders the gizmo. It is also called once during
        //       the update step to allow for correct interaction between the active camera
        //       and the gizmo handles.
        // Parm: updateReason  - The reason behind the update request.
        //       camera        - The camera that interacts with or renders the gizmo.
        //-----------------------------------------------------------------------------
        protected override void OnUpdateHandles(EGizmoHandleUpdateReason updateReason, Camera camera)
        {
            // Sync scale
            activeStyle.moveStyle.scale     = activeStyle.scale;
            activeStyle.rotateStyle.scale   = activeStyle.scale;
            activeStyle.scaleStyle.scale    = activeStyle.scale;

            // Update proxies
            mMoveProxy.OnUpdateHandles(updateReason, camera, activeStyle.moveStyle);
            mRotateProxy.OnUpdateHandles(updateReason, camera, activeStyle.rotateStyle);
            mScaleProxy.OnUpdateHandles(updateReason, camera, activeStyle.scaleStyle);
        }

        //-----------------------------------------------------------------------------
        // Name: OnRender() (Protected Function)
        // Desc: Called when the gizmo must be rendered.
        // Parm: camera    - The camera which renders the gizmo.
        //       rasterCtx - The raster graph context.
        //-----------------------------------------------------------------------------
        protected override void OnRender(Camera camera, RasterGraphContext rasterCtx)
        {
            // Note: Order is important. This seems to produce the best results.
            mRotateProxy.OnRender(camera, rasterCtx);
            mMoveProxy.OnRender(camera, rasterCtx);
            mScaleProxy.OnRender(camera, rasterCtx);
        }
        
        //-----------------------------------------------------------------------------
        // Name: OnStartDrag() (Protected Function)
        // Desc: Called to notify the gizmo that a drag operation can start.
        // Parm: camera - The camera that interacts with the gizmo.
        // Rtrn: An instance 'GizmoDrag' if the drag operation can start and null otherwise.
        //-----------------------------------------------------------------------------
        protected override GizmoDrag OnStartDrag(Camera camera)
        {
            // Notify proxies that they need to start dragging
            GizmoDrag drag = mMoveProxy.OnStartDrag(camera);
            if (drag == null) drag = mRotateProxy.OnStartDrag(camera);
            if (drag == null) drag = mScaleProxy.OnStartDrag(camera);
            return drag;
        }

        //-----------------------------------------------------------------------------
        // Name: OnGUI() (Protected Function)
        // Desc: Called during the 'OnGUI' event to allow the gizmo to draw GUI elements.
        // Parm: camera - The camera that renders the GUI.
        //-----------------------------------------------------------------------------
        protected override void OnGUI(Camera camera)
        {
            mMoveProxy.OnGUI(camera);
            mRotateProxy.OnGUI(camera);
            mScaleProxy.OnGUI(camera);
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: OnMoveModeChanged() (Protected Function)
        // Desc: Called when the gizmo's move mode changes.
        // Parm: oldMode - The old move mode.
        //-----------------------------------------------------------------------------
        void OnMoveModeChanged(EGizmoMoveMode oldMode)
        {
            // Update handle visibility and hoverable states based on the current mode.
            // Note: We only need to take care of the rotate and scale handles. The move
            //       gizmo proxy takes care of the rest.
            if (moveMode == EGizmoMoveMode.Screen)
            {
                // Hide rotate handles, but leave the view rotation cap and rotation arc visible visible
                int count = mRotateProxy.handles.Count;
                for (int i = 0; i < count; ++i)
                {
                    var handle = mRotateProxy.handles[i];
                    if (handle.tag != EGizmoHandleTag.ViewRotation &&
                        handle.tag != EGizmoHandleTag.RotationArc)
                        handle.visible = false;
                }

                // Hide scale handles
                count = mScaleProxy.handles.Count;
                for (int i = 0; i < count; ++i)
                    mScaleProxy.handles[i].visible = false;
            }
            else
            if (moveMode == EGizmoMoveMode.VertexSnap)
            {
                // Hide rotate handles
                int count = mRotateProxy.handles.Count;
                for (int i = 0; i < count; ++i)
                    mRotateProxy.handles[i].visible = false;

                // Hide scale handles
                count = mScaleProxy.handles.Count;
                for (int i = 0; i < count; ++i)
                    mScaleProxy.handles[i].visible = false;
            }
            else
            {
                // Show rotate handles
                int count = mRotateProxy.handles.Count;
                for (int i = 0; i < count; ++i)
                    mRotateProxy.handles[i].visible = true;

                // Show scale handles
                count = mScaleProxy.handles.Count;
                for (int i = 0; i < count; ++i)
                    mScaleProxy.handles[i].visible = true;
            }

            // Fire event
            if (moveModeChanged != null) 
                moveModeChanged(oldMode, this);
        }
        #endregion
    }
    #endregion
}
