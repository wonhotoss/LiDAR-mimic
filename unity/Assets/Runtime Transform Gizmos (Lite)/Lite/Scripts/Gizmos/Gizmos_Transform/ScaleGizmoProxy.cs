using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ScaleGizmoProxy (Public Class)
    // Desc: Scale gizmo proxy class which implements the scale gizmo logic.
    //-----------------------------------------------------------------------------
    public class ScaleGizmoProxy : GizmoProxy
    {
        #region Private Fields
        GizmoLineSlider[]           mAxisSlidersP   = new GizmoLineSlider[3] { new GizmoLineSlider(), new GizmoLineSlider(), new GizmoLineSlider() };    // Positive XYZ slider handles
        GizmoLineSlider[]           mAxisSlidersN   = new GizmoLineSlider[3] { new GizmoLineSlider(), new GizmoLineSlider(), new GizmoLineSlider() };    // Negative XYZ slider handles
        GizmoCap[]                  mAxisCapsP      = new GizmoCap[3]        { new GizmoCap(), new GizmoCap(), new GizmoCap() };                         // Positive XYZ slider caps
        GizmoCap[]                  mAxisCapsN      = new GizmoCap[3]        { new GizmoCap(), new GizmoCap(), new GizmoCap() };                         // Negative XYZ slider caps
        GizmoPlaneSlider[]          mDblAxisSliders = new GizmoPlaneSlider[3]{ new GizmoPlaneSlider(), new GizmoPlaneSlider(), new GizmoPlaneSlider() }; // Double-axis sliders
        GizmoCap                    mUniformCap     = new GizmoCap();       // Uniform scale cap

        // Buffers used to avoid memory allocations
        List<GizmoLineSlider>       mSortedSlidersBuffer            = new List<GizmoLineSlider>();
        List<GizmoPlaneSlider>      mSortedDblAxisSlidersBuffer     = new List<GizmoPlaneSlider>();
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------  
        // Name: ScaleGizmoProxy() (Public Constructor)
        // Desc: Creates a scale gizmo proxy for the specified gizmo.
        // Parm: gizmo             - The gizmo.
        //       baseHoverPriority - All handles created by the proxy will have this
        //                           base hover priority. Useful when a gizmo uses more
        //                           than one proxy because it allows us to control the
        //                           hover priority across proxies.
        //-----------------------------------------------------------------------------  
        public ScaleGizmoProxy(Gizmo gizmo, int baseHoverPriority) 
            : base(gizmo, baseHoverPriority)
        {}
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: OnUpdateHandles() (Public Function)
        // Desc: Called to allow the gizmo to update its handles. This could mean setting
        //       handle styles, updating their transforms etc. This function is called
        //       once for each camera that renders the gizmo. It is also called once during
        //       the update step to allow for correct interaction between the active camera
        //       and the gizmo handles.
        // Parm: updateReason    - The reason behind the update request.
        //       camera          - The camera that interacts with or renders the gizmo.
        //       scaleGizmoStyle - The scale gizmo style used to define the look & feel
        //                         of the gizmo handles.
        //-----------------------------------------------------------------------------
        public void OnUpdateHandles(EGizmoHandleUpdateReason updateReason, Camera camera, ScaleGizmoStyle scaleGizmoStyle)
        {
            // Set slider directions
            mAxisSlidersP[0].SetDirection(mGizmo.transform.right);
            mAxisSlidersP[1].SetDirection(mGizmo.transform.up);
            mAxisSlidersP[2].SetDirection(mGizmo.transform.forward);
            mAxisSlidersN[0].SetDirection(-mAxisSlidersP[0].direction);
            mAxisSlidersN[1].SetDirection(-mAxisSlidersP[1].direction);
            mAxisSlidersN[2].SetDirection(-mAxisSlidersP[2].direction);

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
            count = mAxisSlidersP.Length;
            GlobalGizmoStyle globalStyle = RTGizmos.get.skin.globalGizmoStyle;
            for (int i = 0; i < count; ++i)
            {
                // Is this slider affected by the current drag operation?
                bool scaleLength = mDrag.active && scaleGizmoStyle.sliderScaleEnabled &&
                    ((mDrag.desc.dragType == EGizmoDragType.Scale && mDrag.desc.axisIndex0 == i) ||
                     (mDrag.desc.dragType == EGizmoDragType.DblScale && (mDrag.desc.axisIndex0 == i || mDrag.desc.axisIndex1 == i)) ||
                      mDrag.desc.dragType == EGizmoDragType.UniformScale);

                // Update positive slider
                var style = scaleGizmoStyle.GetAxisSliderStyle(i, true);
                style.color = globalStyle.GetAxisColor(i);
                mAxisSlidersP[i].sliderStyle = style;
                mAxisSlidersP[i].lengthOffset = scaleLength ? mDrag.totalDrag[i] : 0.0f;

                // Update negative slider
                style = scaleGizmoStyle.GetAxisSliderStyle(i, false);
                style.color = globalStyle.GetAxisColor(i);
                mAxisSlidersN[i].sliderStyle = style;
                mAxisSlidersN[i].lengthOffset = scaleLength ? mDrag.totalDrag[i] : 0.0f;

                // Apply offset from origin
                mAxisSlidersP[i].transform.position = mGizmo.transform.position + mAxisSlidersP[i].direction * mAxisSlidersP[i].FVal(scaleGizmoStyle.axisOffset, camera);
                mAxisSlidersN[i].transform.position = mGizmo.transform.position + mAxisSlidersN[i].direction * mAxisSlidersN[i].FVal(scaleGizmoStyle.axisOffset, camera);
            }

            // Update axis caps
            count = mAxisCapsP.Length;
            for (int i = 0; i < count; ++i)
            {
                var style = scaleGizmoStyle.GetAxisCapStyle(i, true);
                style.color = globalStyle.GetAxisColor(i);
                mAxisCapsP[i].capStyle = style;

                style = scaleGizmoStyle.GetAxisCapStyle(i, false);
                style.color = globalStyle.GetAxisColor(i);
                mAxisCapsN[i].capStyle = style;
            }

            // Update dbl-axis sliders
            var dblAxisStyle            = scaleGizmoStyle.GetDblAxisSliderStyle(EPlane.XY);
            dblAxisStyle.color          = globalStyle.GetAxisColor(2).WithAlpha(globalStyle.planeSliderAlpha);
            dblAxisStyle.borderColor    = globalStyle.GetAxisColor(2);
            mDblAxisSliders[(int)EPlane.XY].sliderStyle = dblAxisStyle;

            dblAxisStyle                = scaleGizmoStyle.GetDblAxisSliderStyle(EPlane.YZ);
            dblAxisStyle.color          = globalStyle.GetAxisColor(0).WithAlpha(globalStyle.planeSliderAlpha);
            dblAxisStyle.borderColor    = globalStyle.GetAxisColor(0);
            mDblAxisSliders[(int)EPlane.YZ].sliderStyle = dblAxisStyle;

            dblAxisStyle                = scaleGizmoStyle.GetDblAxisSliderStyle(EPlane.ZX);
            dblAxisStyle.color          = globalStyle.GetAxisColor(1).WithAlpha(globalStyle.planeSliderAlpha);
            dblAxisStyle.borderColor    = globalStyle.GetAxisColor(1);
            mDblAxisSliders[(int)EPlane.ZX].sliderStyle = dblAxisStyle;

            // Update uniform scale cap
            mUniformCap.capStyle = scaleGizmoStyle.uniformCapStyle;

            // Cap sliders
            count = mAxisCapsP.Length;
            for (int i = 0; i < count; ++i)
            {
                mAxisCapsP[i].CapSlider(mAxisSlidersP[i], camera);
                mAxisCapsN[i].CapSlider(mAxisSlidersN[i], camera);
            }

            // Make dbl-axis planes sit between the slider pairs.
            // Note: The way we specify the slider indices affects the way in which the drag operation is setup inside 'OnStartDrag'.
            bool bestVisibility = true;
            mDblAxisSliders[(int)EPlane.XY].SitRightAngle(mGizmo.transform.position, mAxisSlidersP[0], mAxisSlidersP[1], scaleGizmoStyle.dblAxisOffset, camera, bestVisibility);
            mDblAxisSliders[(int)EPlane.YZ].SitRightAngle(mGizmo.transform.position, mAxisSlidersP[1], mAxisSlidersP[2], scaleGizmoStyle.dblAxisOffset, camera, bestVisibility);
            mDblAxisSliders[(int)EPlane.ZX].SitRightAngle(mGizmo.transform.position, mAxisSlidersP[2], mAxisSlidersP[0], scaleGizmoStyle.dblAxisOffset, camera, bestVisibility);
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

            // Uniform scale cap
            mUniformCap.Render(mHRenderArgs);
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
                    desc.dragType   = EGizmoDragType.Scale;
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
                    desc.dragType   = EGizmoDragType.Scale;
                    desc.axis0      = mAxisSlidersN[i].direction;
                    desc.axisIndex0 = i;
                    return mDrag.Start(desc, mGizmo, mGizmo.transform, camera) ? mDrag : null;
                }
            }

            // Match dbl-axis sliders
            count = mDblAxisSliders.Length;
            for (int i = 0; i < count; ++i)
            {
                // Match?
                if (dragHandle == mDblAxisSliders[i])
                {
                    // Start dragging
                    desc.dragType   = EGizmoDragType.DblScale;
                    desc.axis0      = mDblAxisSliders[i].slideDirection0;
                    desc.axis1      = mDblAxisSliders[i].slideDirection1;
                    desc.axisIndex0 = i;
                    desc.axisIndex1 = (i + 1) % 3;
                    return mDrag.Start(desc, mGizmo, mGizmo.transform, camera) ? mDrag : null;
                }
            }

            // Match uniform scale cap
            if (dragHandle == mUniformCap)
            {
                desc.dragType   = EGizmoDragType.UniformScale;
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
            // Store sliders and slider caps
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
                mAxisSlidersP[i].hoverPriority   = mBaseHoverPriority;
                mAxisSlidersN[i].hoverPriority   = mBaseHoverPriority;
                mAxisCapsP[i].hoverPriority      = mBaseHoverPriority;
                mAxisCapsN[i].hoverPriority      = mBaseHoverPriority;
            }

            // Store dbl-axis sliders
            count = mDblAxisSliders.Length;
            for (int i = 0; i < count; ++i)
            {
                handles.Add(mDblAxisSliders[i]);
                mDblAxisSliders[i].hoverPriority = mBaseHoverPriority + 1;
                mDblAxisSliders[i].tag = (EGizmoHandleTag)((int)EGizmoHandleTag.XY + i);
                mDblAxisSliders[i].zoomScaleMode  = EGizmoHandleZoomScaleMode.FromGizmo;
                mDblAxisSliders[i].zoomScaleGizmo = mGizmo;
            }

            // Store uniform scale cap
            handles.Add(mUniformCap);
            mUniformCap.hoverPriority = mBaseHoverPriority + 2;
        }
        #endregion
    }
    #endregion
}
