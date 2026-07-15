using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: MoveGizmo (Public Class)
    // Desc: Implements a gizmo which can be used to move entities around.
    //-----------------------------------------------------------------------------
    public class MoveGizmo : Gizmo
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
        MoveGizmoProxy mProxy;  // Move gizmo proxy which provides the actual implementation
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: style (Public Property)
        // Desc: Returns or sets the move gizmo style.
        //-----------------------------------------------------------------------------
        public MoveGizmoStyle style         { get; set; } = null;

        //-----------------------------------------------------------------------------
        // Name: activeStyle (Public Property)
        // Desc: Returns the active style used by the gizmo. This is 'style' if it was set
        //       to a valid style. Otherwise, it is the active skin's move gizmo style.
        //-----------------------------------------------------------------------------
        public MoveGizmoStyle activeStyle   { get { return style == null ? RTGizmos.get.skin.moveGizmoStyle : style; } }

        //-----------------------------------------------------------------------------
        // Name: moveMode (Public Property)
        // Desc: Returns or sets the gizmo move mode.
        //-----------------------------------------------------------------------------
        public EGizmoMoveMode moveMode      { get { return mProxy.moveMode; } set { if (value != mProxy.moveMode) { var oldMode = mProxy.moveMode; mProxy.moveMode = value; if (moveModeChanged != null) moveModeChanged(oldMode, this); } } }

        //-----------------------------------------------------------------------------
        // Name: vSnapPivotObjects (Public Property)
        // Desc: Returns or sets the vertex snap pivot objects list. Required for vertex
        //       snapping. The gizmo doesn't modify this list. It uses it only to select
        //       the snap pivot when the vertex snap mode is enabled.
        //-----------------------------------------------------------------------------
        public IReadOnlyList<GameObject> vSnapPivotObjects { get { return mProxy.vSnapPivotObjects; } set { mProxy.vSnapPivotObjects = value; } }
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: OnDragEnd() (Protected Function)
        // Desc: Called when the gizmo's drag operation ends.
        // Parm: drag - The drag operation that ended.
        //-----------------------------------------------------------------------------
        protected override void OnDragEnd(GizmoDrag drag)
        {
            mProxy.OnDragEnd(drag);
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
            // Create the proxy and create handles
            mProxy = new MoveGizmoProxy(this, 0);
            mProxy.OnCreateHandles(handles);
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
            mProxy.OnUpdateHandles(updateReason, camera, activeStyle);
        }

        //-----------------------------------------------------------------------------
        // Name: OnRender() (Protected Function)
        // Desc: Called when the gizmo must be rendered.
        // Parm: camera    - The camera which renders the gizmo.
        //       rasterCtx - The raster graph context.
        //-----------------------------------------------------------------------------
        protected override void OnRender(Camera camera, RasterGraphContext rasterCtx)
        {
            mProxy.OnRender(camera, rasterCtx);
        }
        
        //-----------------------------------------------------------------------------
        // Name: OnStartDrag() (Protected Function)
        // Desc: Called to notify the gizmo that a drag operation can start.
        // Parm: camera - The camera that interacts with the gizmo.
        // Rtrn: An instance 'GizmoDrag' if the drag operation can start and null otherwise.
        //-----------------------------------------------------------------------------
        protected override GizmoDrag OnStartDrag(Camera camera)
        {
            return mProxy.OnStartDrag(camera);
        }

        //-----------------------------------------------------------------------------
        // Name: OnGUI() (Protected Function)
        // Desc: Called during the 'OnGUI' event to allow the gizmo to draw GUI elements.
        // Parm: camera - The camera that renders the GUI.
        //-----------------------------------------------------------------------------
        protected override void OnGUI(Camera camera)
        {
            mProxy.OnGUI(camera);
        }
        #endregion
    }
    #endregion
}
