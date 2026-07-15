using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RotateGizmoProxy (Public Class)
    // Desc: Rotate gizmo proxy class which implements the rotate gizmo logic.
    //-----------------------------------------------------------------------------
    public class RotateGizmoProxy : GizmoProxy
    {
        #region Private Fields
        GizmoCap[]  mAxisCaps       = new GizmoCap[3] { new GizmoCap(), new GizmoCap(), new GizmoCap() } ;  // Axis caps
        GizmoCap    mArcBall        = new GizmoCap();                                                       // Arc-ball
        GizmoCap    mViewCap        = new GizmoCap();                                                       // View cap
        GizmoArc    mRotationArc    = new GizmoArc();                                                       // Gizmo arc used as rotation indicator
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------  
        // Name: RotateGizmoProxy() (Public Constructor)
        // Desc: Creates a rotate gizmo proxy for the specified gizmo.
        // Parm: gizmo             - The gizmo.
        //       baseHoverPriority - All handles created by the proxy will have this
        //                           base hover priority. Useful when a gizmo uses more
        //                           than one proxy because it allows us to control the
        //                           hover priority across proxies.
        //-----------------------------------------------------------------------------  
        public RotateGizmoProxy(Gizmo gizmo, int baseHoverPriority) 
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
        // Parm: updateReason       - The reason behind the update request.
        //       camera             - The camera that interacts with or renders the gizmo.
        //       rotateGizmoStyle   - The move gizmo style used to define the look & feel
        //                            of the gizmo handles.
        //-----------------------------------------------------------------------------
        public void OnUpdateHandles(EGizmoHandleUpdateReason updateReason, Camera camera, RotateGizmoStyle rotateGizmoStyle)
        {
            // Set styles
            mArcBall.capStyle = rotateGizmoStyle.arcBallStyle;
            mViewCap.capStyle = rotateGizmoStyle.viewCapStyle;

            // Axis caps
            var globalStyle = RTGizmos.get.skin.globalGizmoStyle;
            int count = mAxisCaps.Length;
            for (int i = 0; i < count; ++i)
            {
                mAxisCaps[i].capStyle           = rotateGizmoStyle.GetAxisCapStyle(i);
                mAxisCaps[i].capStyle.color     = globalStyle.GetAxisColor(i);
            }

            // Set XYZ axis caps local rotations
            mAxisCaps[0].transform.localRotation = Quaternion.Euler(0.0f, 90.0f, 0.0f);  // X axis cap in the YZ plane
            mAxisCaps[1].transform.localRotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);  // Y axis cap in the ZX plane
            mAxisCaps[2].transform.localRotation = Quaternion.identity;                  // Z axis cap in the XY plane

            // Turn the view cap into a sphere border
            mViewCap.ApproximateSphereBorder(mGizmo.transform.position, camera);

            // Setup the cull sphere
            Sphere cullSphere = new Sphere(mGizmo.transform.position, mArcBall.FVal(mArcBall.capStyle.sphereRadius, camera));
            mViewCap.cullSphere = cullSphere;
            for (int i = 0; i < count; ++i)
                mAxisCaps[i].cullSphere = cullSphere;

            // Set rotation arc style
            mRotationArc.arcStyle = globalStyle.rotationArcStyle;
        }

        //-----------------------------------------------------------------------------
        // Name: OnRender() (Public Function)
        // Desc: Called when the gizmo must be rendered.
        // Parm: camera    - The camera which renders the gizmo.
        //       rasterCtx - The raster graph context.
        //-----------------------------------------------------------------------------
        public void OnRender(Camera camera, RasterGraphContext rasterCtx)
        {
            int count;

            // Fill render args
            mHRenderArgs.camera    = camera;
            mHRenderArgs.rasterCtx = rasterCtx;

            // If we're dragging, we need to setup the rotation arc
            mRotationArc.visible = false;
            if (mGizmo.dragging)
            {
                // What are we dragging? The rotation arc appears when dragging the XYZ caps or the view cap.
                count = mAxisCaps.Length;
                for (int i = 0; i < count; ++i)
                {
                    // Match?
                    if (mAxisCaps[i] == mGizmo.dragData.handle)
                    {
                        // Store rotation arc radius based on the axis cap type
                        float rotationArcRadius = mAxisCaps[0].capStyle.circleRadius;
                        if (mAxisCaps[0].capStyle.capType == EGizmoCapType.Torus) rotationArcRadius = mAxisCaps[0].capStyle.torusRadius - 2.0f * mAxisCaps[0].capStyle.torusTubeRadius;
                        else if (mAxisCaps[0].capStyle.capType == EGizmoCapType.InsetCylinder) rotationArcRadius = mAxisCaps[0].capStyle.cylinderRadius - mAxisCaps[0].capStyle.insetCylinderThickness;

                        // Set arc position and rotation and mark as visible
                        mRotationArc.visible = true;
                        mRotationArc.transform.position = mAxisCaps[i].transform.position;
                        mRotationArc.transform.rotation = mAxisCaps[i].transform.rotation;
                        mRotationArc.Set(mGizmo.dragData.dragStartHoverPoint, 
                            mAxisCaps[i].FVal(rotationArcRadius, camera),
                            mDrag.totalDrag[i]);

                        break;
                    }
                }

                // If the arc is not visible, it means we haven't found the drag handle. Check the view cap.
                if (mViewCap == mGizmo.dragData.handle)
                {
                    // Set arc position and rotation and mark as visible
                    mRotationArc.visible = true;
                    mRotationArc.transform.position = mViewCap.transform.position;
                    mRotationArc.transform.rotation = mViewCap.transform.rotation;
                    mRotationArc.Set(mGizmo.dragData.dragStartHoverPoint, 
                        mViewCap.FVal(mViewCap.capStyle.circleRadius, camera),
                        mDrag.totalDrag[2]);
                }
            }

            // Render
            mRotationArc.Render(mHRenderArgs);
            mArcBall.Render(mHRenderArgs);
            mViewCap.Render(mHRenderArgs);

            count = mAxisCaps.Length;
            for (int i = 0; i < count; ++i)
                mAxisCaps[i].Render(mHRenderArgs);
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

            // Match axis caps
            int count = mAxisCaps.Length;
            for (int i = 0; i < count; ++i)
            {
                // Match?
                if (dragHandle == mAxisCaps[i])
                {
                    // Start dragging
                    desc.axis0              = mAxisCaps[i].transform.forward;    // Note: Tied to the internal representation of the cap.
                    desc.axisIndex0         = i;
                    desc.dragType           = EGizmoDragType.Rotate;
                    desc.rotationCenter     = mAxisCaps[i].transform.position;
                    return mDrag.Start(desc, mGizmo, mGizmo.transform, camera) ? mDrag : null;            
                }
            }

            // Match view cap
            if (dragHandle == mViewCap)
            {
                // Start dragging
                desc.axis0              = mViewCap.transform.forward;   // Note: Tied to the internal representation of the cap.
                desc.axisIndex0         = 2;
                desc.dragType           = EGizmoDragType.Rotate;
                desc.rotationCenter     = mViewCap.transform.position;
                return mDrag.Start(desc, mGizmo, mGizmo.transform, camera) ? mDrag : null;   
            }

            // Match arc-ball
            if (dragHandle == mArcBall)
            {
                // Start dragging
                desc.axis0              = camera.transform.right; 
                desc.axis1              = camera.transform.up;
                desc.axisIndex0         = 0;
                desc.axisIndex1         = 1;
                desc.dragType           = EGizmoDragType.DblRotate;
                desc.rotationCenter     = mArcBall.transform.position;
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
                // Draw the drag angles above the view rotation cap
                Circle circle = mViewCap.ToCircle(camera, 0.0f);
                RTGizmos.get.skin.globalGizmoStyle.GUILabel_CircleTop(circle, camera, mDrag.totalDrag.ToString("F3"), EGizmoTextType.DragInfo);
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
            // Init arc-ball and view caps and store them
            mArcBall.tag                = EGizmoHandleTag.ArcBall;
            mArcBall.hoverPriority      = mBaseHoverPriority;
            mViewCap.tag                = EGizmoHandleTag.ViewRotation;
            mViewCap.alignFlatToCamera  = false;
            mViewCap.flatWireHoverMode  = EGizmoFlatHoverMode.Wire;
            mViewCap.cullSphereEnabled  = true;
            mViewCap.hoverPriority      = mBaseHoverPriority;
            mViewCap.hoverPaddingOffset += 0.7f;
            mRotationArc.tag            = EGizmoHandleTag.RotationArc;

            handles.Add(mArcBall);
            handles.Add(mViewCap);
            handles.Add(mRotationArc);

            // Init axis caps and store them
            int count = mAxisCaps.Length;
            for (int i = 0; i < count; ++i)
            {
                handles.Add(mAxisCaps[i]);
                mAxisCaps[i].tag                 = (EGizmoHandleTag)i;
                mAxisCaps[i].hoverPriority       = mBaseHoverPriority + 1;
                mAxisCaps[i].alignFlatToCamera   = false;
                mAxisCaps[i].flatWireHoverMode   = EGizmoFlatHoverMode.Extrude;
                mAxisCaps[i].cullSphereEnabled   = true;
                mAxisCaps[i].hoverPaddingOffset += 0.5f;
            }
        }
        #endregion
    }
    #endregion
}
