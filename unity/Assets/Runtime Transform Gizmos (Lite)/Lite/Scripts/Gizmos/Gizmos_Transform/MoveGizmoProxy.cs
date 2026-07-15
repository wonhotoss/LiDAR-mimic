using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EGizmoMoveMode (Public Enum)
    // Desc: Defines different gizmo move modes. A move mode controls the way in which
    //       entities can be moved around by a move gizmo.
    //-----------------------------------------------------------------------------
    public enum EGizmoMoveMode
    {
        Normal = 0, // Normal mode
        Screen,     // The gizmo sliders are aligned with the camera axes
        VertexSnap, // Vertex snap mode
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: MoveGizmoProxy (Public Class)
    // Desc: Move gizmo proxy class which implements the move gizmo logic.
    //-----------------------------------------------------------------------------
    public class MoveGizmoProxy : GizmoProxy
    {
        #region Private Fields
        GizmoLineSlider[]           mAxisSlidersP   = new GizmoLineSlider[3] { new GizmoLineSlider(), new GizmoLineSlider(), new GizmoLineSlider() };    // Positive axis slider handles
        GizmoLineSlider[]           mAxisSlidersN   = new GizmoLineSlider[3] { new GizmoLineSlider(), new GizmoLineSlider(), new GizmoLineSlider() };    // Negative axis slider handles
        GizmoCap[]                  mAxisCapsP      = new GizmoCap[3]        { new GizmoCap(), new GizmoCap(), new GizmoCap() };                         // Positive axis slider caps
        GizmoCap[]                  mAxisCapsN      = new GizmoCap[3]        { new GizmoCap(), new GizmoCap(), new GizmoCap() };                         // Negative axis slider caps
        GizmoPlaneSlider[]          mDblAxisSliders = new GizmoPlaneSlider[3]{ new GizmoPlaneSlider(), new GizmoPlaneSlider(), new GizmoPlaneSlider() }; // Double-axis sliders
        GizmoCap                    mVSnapCap       = new GizmoCap();   // Vertex snap cap

        EGizmoMoveMode              mMoveMode = EGizmoMoveMode.Normal;  // Active gizmo move mode
        Vector3                     mVSnapPosRestore;                   // Used to restore the gizmo position post vertex snapping 

        // Buffers used to avoid memory allocations
        List<GizmoLineSlider>       mSortedSlidersBuffer            = new List<GizmoLineSlider>();
        List<GizmoPlaneSlider>      mSortedDblAxisSlidersBuffer     = new List<GizmoPlaneSlider>();
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------  
        // Name: MoveGizmoProxy() (Public Constructor)
        // Desc: Creates a move gizmo proxy for the specified gizmo.
        // Parm: gizmo             - The gizmo.
        //       baseHoverPriority - All handles created by the proxy will have this
        //                           base hover priority. Useful when a gizmo uses more
        //                           than one proxy because it allows us to control the
        //                           hover priority across proxies.
        //-----------------------------------------------------------------------------  
        public MoveGizmoProxy(Gizmo gizmo, int baseHoverPriority) 
            : base(gizmo, baseHoverPriority)
        {}
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: moveMode (Public Property)
        // Desc: Returns or sets the gizmo move mode.
        //-----------------------------------------------------------------------------
        public EGizmoMoveMode moveMode 
        { 
            get { return mMoveMode; } 
            set 
            {
                // No-op?
                if (mMoveMode == value)
                    return;

                // Set mode
                var oldMode = mMoveMode;
                mMoveMode = value;
                OnMoveModeChanged(oldMode);
            } 
        }

        //-----------------------------------------------------------------------------
        // Name: vSnapPivotObjects (Public Property)
        // Desc: Returns or sets the vertex snap pivot objects list. Required for vertex
        //       snapping. The gizmo doesn't modify this list. It uses it only to select
        //       the snap pivot when the vertex snap mode is enabled.
        //-----------------------------------------------------------------------------
        public IReadOnlyList<GameObject> vSnapPivotObjects { get; set; } = null;
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: OnDragEnd() (Protected Function)
        // Desc: Called when the gizmo's drag operation ends.
        // Parm: drag - The drag operation that ended.
        //-----------------------------------------------------------------------------
        public void OnDragEnd(GizmoDrag drag)
        {
            // If we are vertex snapping, apply the total drag offset to the restore position.
            // This is necessary because we might have dragged the gizmo around and we want
            // to apply the same transformation to the restore position.
            if (drag.desc.dragType == EGizmoDragType.VertexSnap)
                mVSnapPosRestore += drag.totalDrag;
        }

        //-----------------------------------------------------------------------------
        // Name: OnUpdateHandles() (Public Function)
        // Desc: Called to allow the gizmo to update its handles. This could mean setting
        //       handle styles, updating their transforms etc. This function is called
        //       once for each camera that renders the gizmo. It is also called once during
        //       the update step to allow for correct interaction between the active camera
        //       and the gizmo handles.
        // Parm: updateReason   - The reason behind the update request.
        //       camera         - The camera that interacts with or renders the gizmo.
        //       moveGizmoStyle - The move gizmo style used to define the look & feel
        //                        of the gizmo handles.
        //-----------------------------------------------------------------------------
        public void OnUpdateHandles(EGizmoHandleUpdateReason updateReason, Camera camera, MoveGizmoStyle moveGizmoStyle)
        {
            // If vertex snapping is enabled, but we're not dragging, select snap pivot
            if (!mGizmo.dragging && mMoveMode == EGizmoMoveMode.VertexSnap)
            {
                // Do we have any pivot objects?
                if (vSnapPivotObjects != null)
                {
                    // Select pivot and update gizmo position
                    if (GizmoVertexSnap.SelectPivot(vSnapPivotObjects, camera, out Vector3 pivot))
                        mGizmo.transform.position = pivot;
                }
            }

            // Set slider directions
            mAxisSlidersP[0].SetDirection(mGizmo.transform.right);
            mAxisSlidersP[1].SetDirection(mGizmo.transform.up);
            mAxisSlidersP[2].SetDirection(mGizmo.transform.forward);
            mAxisSlidersN[0].SetDirection(-mAxisSlidersP[0].direction);
            mAxisSlidersN[1].SetDirection(-mAxisSlidersP[1].direction);
            mAxisSlidersN[2].SetDirection(-mAxisSlidersP[2].direction);

            // Take gizmo move mode into account
            if (mMoveMode == EGizmoMoveMode.Screen)
            {
                // Align the X and Y sliders with the camera right and up vectors
                mAxisSlidersP[0].SetDirection( camera.transform.right);
                mAxisSlidersN[0].SetDirection(-camera.transform.right);
                mAxisSlidersP[1].SetDirection( camera.transform.up);
                mAxisSlidersN[1].SetDirection(-camera.transform.up);
            }

            // Calculate cap local rotations
            int count = mAxisSlidersP.Length;
            for (int i = 0; i < count; ++i)
            {
                // Same rotation as the sliders
                mAxisCapsP[i].transform.rotation = mAxisSlidersP[i].transform.rotation;
                mAxisCapsN[i].transform.rotation = mAxisSlidersN[i].transform.rotation;

                // Track caps
                mAxisSlidersP[i].trackedCap = mAxisCapsP[i];
                mAxisSlidersN[i].trackedCap = mAxisCapsN[i];
            }

            // Update axis sliders
            GlobalGizmoStyle globalStyle = RTGizmos.get.skin.globalGizmoStyle;
            count = mAxisSlidersP.Length;
            for (int i = 0; i < count; ++i)
            {
                // Update style
                var style       = moveGizmoStyle.GetAxisSliderStyle(i, true);
                style.color     = globalStyle.GetAxisColor(i);
                mAxisSlidersP[i].sliderStyle = style;

                style           = moveGizmoStyle.GetAxisSliderStyle(i, false);
                style.color     = globalStyle.GetAxisColor(i);
                mAxisSlidersN[i].sliderStyle = style;

                // Apply offset from origin
                mAxisSlidersP[i].transform.position = mGizmo.transform.position + mAxisSlidersP[i].direction * mAxisSlidersP[i].FVal(moveGizmoStyle.axisOffset, camera);
                mAxisSlidersN[i].transform.position = mGizmo.transform.position + mAxisSlidersN[i].direction * mAxisSlidersN[i].FVal(moveGizmoStyle.axisOffset, camera);
            }

            // Update axis caps and cap sliders
            count = mAxisCapsP.Length;
            for (int i = 0; i < count; ++i)
            {
                // Update style
                var style       = moveGizmoStyle.GetAxisCapStyle(i, true);
                style.color     = globalStyle.GetAxisColor(i);
                mAxisCapsP[i].capStyle = style;

                style           = moveGizmoStyle.GetAxisCapStyle(i, false);
                style.color     = globalStyle.GetAxisColor(i);
                mAxisCapsN[i].capStyle = style;

                // Cap sliders
                mAxisCapsP[i].CapSlider(mAxisSlidersP[i], camera);
                mAxisCapsN[i].CapSlider(mAxisSlidersN[i], camera);
            }

            // Update dbl-axis sliders
            var dblAxisStyle                = moveGizmoStyle.GetDblAxisSliderStyle(EPlane.XY);
            dblAxisStyle.color              = globalStyle.GetAxisColor(2).WithAlpha(globalStyle.planeSliderAlpha);
            dblAxisStyle.borderColor        = globalStyle.GetAxisColor(2);
            mDblAxisSliders[(int)EPlane.XY].sliderStyle = dblAxisStyle;

            dblAxisStyle                    = moveGizmoStyle.GetDblAxisSliderStyle(EPlane.YZ);
            dblAxisStyle.color              = globalStyle.GetAxisColor(0).WithAlpha(globalStyle.planeSliderAlpha);
            dblAxisStyle.borderColor        = globalStyle.GetAxisColor(0);
            mDblAxisSliders[(int)EPlane.YZ].sliderStyle = dblAxisStyle;

            dblAxisStyle                    = moveGizmoStyle.GetDblAxisSliderStyle(EPlane.ZX);
            dblAxisStyle.color              = globalStyle.GetAxisColor(1).WithAlpha(globalStyle.planeSliderAlpha);
            dblAxisStyle.borderColor        = globalStyle.GetAxisColor(1);
            mDblAxisSliders[(int)EPlane.ZX].sliderStyle = dblAxisStyle;

            // Make dbl-axis planes sit between the slider pairs.
            // Note: The way we specify the slider indices affects the way in which the drag operation is setup inside 'OnStartDrag'.
            bool bestVisibility = mMoveMode != EGizmoMoveMode.Screen;
            mDblAxisSliders[(int)EPlane.XY].SitRightAngle(mGizmo.transform.position, mAxisSlidersP[0], mAxisSlidersP[1], moveGizmoStyle.dblAxisOffset, camera, bestVisibility);
            mDblAxisSliders[(int)EPlane.YZ].SitRightAngle(mGizmo.transform.position, mAxisSlidersP[1], mAxisSlidersP[2], moveGizmoStyle.dblAxisOffset, camera, bestVisibility);
            mDblAxisSliders[(int)EPlane.ZX].SitRightAngle(mGizmo.transform.position, mAxisSlidersP[2], mAxisSlidersP[0], moveGizmoStyle.dblAxisOffset, camera, bestVisibility);

            // Are we vertex snapping?
            if (mMoveMode == EGizmoMoveMode.VertexSnap)
            {
                // Update VSnap cap
                mVSnapCap.transform.rotation = camera.transform.rotation;
                mVSnapCap.capStyle = moveGizmoStyle.vSnapCapStyle;
                mVSnapCap.visible = true;
            }
            else mVSnapCap.visible = false;
        }

        //-----------------------------------------------------------------------------
        // Name: OnRender() (Public Function)
        // Desc: Called when the gizmo must be rendered.
        // Parm: camera    - The camera which renders the gizmo.
        //       rasterCtx - The raster graph context.
        //-----------------------------------------------------------------------------
        public void OnRender(Camera camera, RasterGraphContext rasterCtx)
        {
            // Fill render args
            mHRenderArgs.camera    = camera;
            mHRenderArgs.rasterCtx = rasterCtx;

            // Sort sliders and render them
            GizmoHandle.ZSortSliders(mGizmo.internal_lineSliders, mHRenderArgs, mSortedSlidersBuffer);
            int count = mSortedSlidersBuffer.Count;
            for (int i = 0; i < count; ++i)
            {
                // Render slider and cap in sorted order
                GizmoHandle.ZSortRenderSliderAndCap(mSortedSlidersBuffer[i], mSortedSlidersBuffer[i].trackedCap, mHRenderArgs);
            }

            // Sort dbl-axis sliders and render them
            GizmoHandle.ZSortHandles(mGizmo.internal_planeSliders, mHRenderArgs, mSortedDblAxisSlidersBuffer);
            count = mSortedDblAxisSlidersBuffer.Count;
            for (int i = 0; i < count; ++i)
                mSortedDblAxisSlidersBuffer[i].Render(mHRenderArgs);

            // VSnap cap
            mVSnapCap.Render(mHRenderArgs);
        }

        //-----------------------------------------------------------------------------
        // Name: OnStartDrag() (Public Function)
        // Desc: Called to notify the gizmo that a drag operation can start.
        // Parm: camera - The camera that interacts with the gizmo.
        // Rtrn: An instance 'GizmoDrag' if the drag operation can start and null otherwise.
        //-----------------------------------------------------------------------------
        public GizmoDrag OnStartDrag(Camera camera)
        {
            // Cache data
            GizmoHandle dragHandle  = mGizmo.dragData.handle;
            GizmoDragDesc desc      = new GizmoDragDesc();
            
            // Match axis sliders and caps
            int count = mAxisSlidersP.Length;
            for (int i = 0; i < count; ++i)
            {
                // Match positive sliders and caps
                if (dragHandle == mAxisSlidersP[i]   ||
                    dragHandle == mAxisCapsP[i])
                {
                    // Start dragging
                    desc.dragType   = EGizmoDragType.Move;
                    desc.axis0      = mAxisSlidersP[i].direction;
                    desc.axisIndex0 = i;
                    return mDrag.Start(desc, mGizmo, mGizmo.transform, camera) ? mDrag : null;            
                }
                else
                // Match negative sliders and caps
                if (dragHandle == mAxisSlidersN[i]   ||
                    dragHandle == mAxisCapsN[i])
                {
                    // Start dragging
                    desc.dragType   = EGizmoDragType.Move;
                    desc.axis0      = mAxisSlidersN[i].direction;
                    desc.axisIndex0 = i;
                    return mDrag.Start(desc, mGizmo, mGizmo.transform, camera) ? mDrag : null;
                }
            }

            // Match dbl-axis sliders
            count = mDblAxisSliders.Length;
            for (int i = 0; i< count; ++i)
            {
                // Match?
                if (dragHandle == mDblAxisSliders[i])
                {
                    // Start dragging
                    desc.dragType   = EGizmoDragType.DblMove;
                    desc.axis0      = mDblAxisSliders[i].slideDirection0;
                    desc.axis1      = mDblAxisSliders[i].slideDirection1;
                    desc.axisIndex0 = i;
                    desc.axisIndex1 = (i + 1) % 3;
                    return mDrag.Start(desc, mGizmo, mGizmo.transform, camera) ? mDrag : null;
                }
            }

            // Match VSnap cap
            if (dragHandle == mVSnapCap)
            {
                // Start dragging
                desc.dragType           = EGizmoDragType.VertexSnap;
                desc.vSnapPivotObjects  = vSnapPivotObjects;
                return mDrag.Start(desc, mGizmo, mGizmo.transform, camera) ? mDrag : null;
            }

            // Can't start dragging
            return null;
        }

        //-----------------------------------------------------------------------------
        // Name: OnGUI() (Public Function)
        // Desc: Called during the 'OnGUI' event to allow the gizmo to draw GUI elements.
        // Parm: camera - The camera that renders the GUI.
        //-----------------------------------------------------------------------------
        public void OnGUI(Camera camera)
        { 
            // Are we dragging?
            if (mDrag.active)
            {
                // Draw label at the top of the handle collection
                RTGizmos.get.skin.globalGizmoStyle.GUILabel_HandlesTop(mGizmo.internal_handles, camera, mDrag.totalDrag.ToString("F3"), EGizmoTextType.DragInfo);
            }
        }
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: OnCreateProxyHandles() (Protected Function)
        // Desc: Called in order to allow the proxy to create the gizmo handles.
        // Parm: handles - Returns the list of handles.
        //-----------------------------------------------------------------------------
        protected override void OnCreateProxyHandles(List<GizmoHandle> handles)
        {
            // Store axis sliders and caps and establish render hover connections
            int count = mAxisSlidersP.Length;
            for (int i = 0; i < count; ++i)
            {
                // Store handles
                handles.Add(mAxisSlidersP[i]);
                handles.Add(mAxisSlidersN[i]);
                handles.Add(mAxisCapsP[i]);
                handles.Add(mAxisCapsN[i]);

                // Set tags
                mAxisSlidersP[i].tag = (EGizmoHandleTag)i;
                mAxisSlidersN[i].tag = (EGizmoHandleTag)i;               
                mAxisCapsP[i].tag    = (EGizmoHandleTag)i;
                mAxisCapsN[i].tag    = (EGizmoHandleTag)i;

                // Establish render hover connections
                mAxisCapsP[i].AddRenderHoverConnection(mAxisSlidersP[i]);
                mAxisCapsN[i].AddRenderHoverConnection(mAxisSlidersN[i]);

                // Track slider caps
                mAxisSlidersP[i].trackedCap = mAxisCapsP[i];
                mAxisSlidersN[i].trackedCap = mAxisCapsN[i];

                // Set base priority
                mAxisSlidersP[i].hoverPriority  = mBaseHoverPriority;
                mAxisSlidersN[i].hoverPriority  = mBaseHoverPriority;
                mAxisCapsP[i].hoverPriority     = mBaseHoverPriority;
                mAxisCapsN[i].hoverPriority     = mBaseHoverPriority;
            }

            // Store dbl-axis sliders
            count = mDblAxisSliders.Length;
            for (int i = 0; i < count; ++i)
            {
                handles.Add(mDblAxisSliders[i]);
                mDblAxisSliders[i].hoverPriority = mBaseHoverPriority + 1;
                mDblAxisSliders[i].tag = (EGizmoHandleTag)((int)EGizmoHandleTag.XY + i);

                // In order for these sliders to sit nicely in a corner, they need to use
                // a common zoom scale value. Otherwise, they will be out of sync.
                mDblAxisSliders[i].zoomScaleMode    = EGizmoHandleZoomScaleMode.FromGizmo;
                mDblAxisSliders[i].zoomScaleGizmo   = mGizmo;
            }

            // VSnap cap
            handles.Add(mVSnapCap);
            mVSnapCap.tag = EGizmoHandleTag.VertexSnap;
            mVSnapCap.hoverPriority = mBaseHoverPriority;
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
            // If we're coming out of vertex snapping, restore position.
            // Note: We also need to take into account the fact that the user may still be
            //       dragging. So we also have to end the current drag.
            if (oldMode == EGizmoMoveMode.VertexSnap)
            {
                mGizmo.transform.position = mVSnapPosRestore;
                mGizmo.Internal_EndDrag();
            }

            // If we entered vertex snapping, update restore position
            if (mMoveMode == EGizmoMoveMode.VertexSnap)
                mVSnapPosRestore = mGizmo.transform.position;

            // Update handle visibility and hoverable states based on the current mode
            if (mMoveMode == EGizmoMoveMode.Screen)
            {
                // Hide the Z axis slider and cap. Also, only the XY dbl-axis slider is visible.
                mAxisSlidersP[2].visible                = false;
                mAxisSlidersN[2].visible                = false;
                mAxisCapsP[2].visible                   = false;
                mAxisCapsN[2].visible                   = false;
                mDblAxisSliders[(int)EPlane.YZ].visible = false;
                mDblAxisSliders[(int)EPlane.ZX].visible = false;
            }
            else
            if (mMoveMode == EGizmoMoveMode.VertexSnap)
            {
                // All sliders and caps can no longer be hovered
                int count = mAxisSlidersP.Length;
                for (int i = 0; i < count; ++i)
                {
                    mAxisSlidersP[i].hoverable   = false;
                    mAxisSlidersN[i].hoverable   = false;
                    mAxisCapsP[i].hoverable      = false;
                    mAxisCapsN[i].hoverable      = false;
                }

                // Hide dbl-axis sliders
                mDblAxisSliders[(int)EPlane.XY].visible = false;
                mDblAxisSliders[(int)EPlane.YZ].visible = false;
                mDblAxisSliders[(int)EPlane.ZX].visible = false;
            }
            else
            {
                // Restore hoverable state
                int count = mAxisSlidersP.Length;
                for (int i = 0; i < count; ++i)
                {
                    mAxisSlidersP[i].hoverable   = true;
                    mAxisSlidersN[i].hoverable   = true;
                    mAxisCapsP[i].hoverable      = true;
                    mAxisCapsN[i].hoverable      = true;
                }

                // Restore visibility
                mAxisSlidersP[2].visible = true;
                mAxisSlidersN[2].visible = true;
                mAxisCapsP[2].visible    = true;
                mAxisCapsN[2].visible    = true;

                mDblAxisSliders[(int)EPlane.XY].visible = true;
                mDblAxisSliders[(int)EPlane.YZ].visible = true;
                mDblAxisSliders[(int)EPlane.ZX].visible = true;
            }
        }
        #endregion
    }
    #endregion
}
