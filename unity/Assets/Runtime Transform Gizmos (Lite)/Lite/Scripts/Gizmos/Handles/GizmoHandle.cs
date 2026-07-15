using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using System;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EGizmoHandleTag (Public Enum)
    // Desc: Defines different gizmo handle tags.
    //-----------------------------------------------------------------------------
    public enum EGizmoHandleTag
    {
        X = 0,              // X axis handle
        Y,                  // Y axis handle
        Z,                  // Z axis handle

        XY,                 // XY plane handle
        YZ,                 // YZ plane handle
        ZX,                 // ZX plane handle

        VertexSnap,         // Vertex snap handle
        ArcBall,            // Arc-ball handle
        ViewRotation,       // View rotation handle that rotates around the camera view vector
        RotationArc,        // Rotation arc used by rotation gizmos as a rotation indicator
        ProjectionSwitch,   // A handle which performs a camera projection switch (e.g. scene gizmo mid cap)

        None                // No tag
    }

    //-----------------------------------------------------------------------------
    // Name: EGizmoHandleZoomScaleMode (Public Enum)
    // Desc: Defines different gizmo handle zoom scale modes. A zoom scale mode
    //       controls the way in which handles calculate their zoom scale.
    //-----------------------------------------------------------------------------
    public enum EGizmoHandleZoomScaleMode
    {
        Auto = 0,       // The handle automatically calculates its own zoom scale
        Manual,         // The client has to manually specify a zoom scale value
        One,            // The handle uses a zoom scale of 1
        FromGizmo,      // The handle will use the zoom scale of specified gizmo
    }

    //-----------------------------------------------------------------------------
    // Name: EGizmoHandleTransformScaleMode (Public Enum)
    // Desc: Defines different gizmo handle transform scale modes. A transform
    //       scale mode controls the way in which a handle uses the transform scale.
    //-----------------------------------------------------------------------------
    public enum EGizmoHandleTransformScaleMode
    {
        Use = 0,    // Use the scale stored in the handle's transform
        One         // Ignore the scale stored in the handle's transform and use a scale of 1 instead
    }

    //-----------------------------------------------------------------------------
    // Name: EGizmoHandleCullSphereTargets (Public Flags Enum)
    // Desc: Defines different gizmo handle cull sphere targets. For example, 
    //       the cull sphere may be used only during rendering, or hovering, or both.
    //-----------------------------------------------------------------------------
    [Flags] public enum EGizmoHandleCullSphereTargets
    {
        None    = 0,                // Culling doesn't happen anywhere
        Render  = 1,                // Culling happens when rendering the handle
        Hover   = 2,                // Culling happens when hovering the handle
        All     = Render | Hover    // Culling happens everywhere
    }
    #endregion

    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: GizmoHandleRenderArgs (Public Struct)
    // Desc: Provides storage for gizmo handle render arguments.
    //-----------------------------------------------------------------------------
    public struct GizmoHandleRenderArgs
    {
        #region Public Fields
        public Camera               camera;     // Render camera
        public RasterGraphContext   rasterCtx;  // Raster context
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: GizmoHoverRaycastArgs (Public Struct)
    // Desc: Provides storage for gizmo hover raycast arguments.
    //-----------------------------------------------------------------------------
    public struct GizmoHoverRaycastArgs
    {
        #region Public Fields
        public Ray      ray;        // Hover ray (i.e. mouse cursor ray)
        public Camera   camera;     // The camera that interacts with the gizmo
        #endregion
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GizmoHandle (Public Abstract Class)
    // Desc: Gizmos are composed of handles. A handle represents an interactive part 
    //       of the gizmo, enabling user interaction. Some handles are non-interactive 
    //       and serve purely as visual indicators.
    //-----------------------------------------------------------------------------
    public abstract class GizmoHandle
    {
        #region Private Static Fields
        static int sId = 1;     // Used to generate sequential handle ids
        #endregion

        #region Private Fields
        int                 mId                 = sId++;                    // Handle Id
        GizmoTransform      mTransform          = new GizmoTransform();     // Handle transform       
        float               mHoverPaddingOffset = 0.0f;                     // Used to offset global hover padding. Useful when some handles are harder to pick with the global padding value.
        float               mManualZoomScale    = 1.0f;                     // Manual zoom scale
        float               mAlphaScale         = 1.0f;                     // Used to scale the original pixel alpha
        GizmoClipPlane[]    mClipPlanes         = new GizmoClipPlane[1];    // Array of clip planes

        // List of render hover connections. A render hover connection allows handles
        // to borrow the hovered state from each other when rendered. This is useful in
        // situations where the cursor hovers one handle, but other handles need to be
        // highlighted during rendering (e.g. move gizmo axis slider and cap).
        List<GizmoHandle>   mRenderHoverConnections = new List<GizmoHandle>();
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: id (Public Property)
        // Desc: Returns the handle Id.
        //-----------------------------------------------------------------------------
        public int              id          { get { return mId; } }

        //-----------------------------------------------------------------------------
        // Name: transform (Public Property)
        // Desc: Returns the handle transform.
        //-----------------------------------------------------------------------------
        public GizmoTransform   transform   { get { return mTransform; } }

        //-----------------------------------------------------------------------------
        // Name: hoverPriority (Public Property)
        // Desc: Returns or sets the hover priority. The higher the number, the higher
        //       the priority. For example, if the cursor hovers 3 handles at the same
        //       time with priorities: 0, -2, 7 the handle with priority 7 will be marked
        //       as hovered.
        //-----------------------------------------------------------------------------
        public int      hoverPriority       { get; set; } = 0;

        //-----------------------------------------------------------------------------
        // Name: visible (Public Property)
        // Desc: Returns or sets the handle visibility. This is in addition to the handle's
        //       style 'visible' property.
        //-----------------------------------------------------------------------------
        public bool     visible             { get; set; } = true;

        //-----------------------------------------------------------------------------
        // Name: canRender (Public Property)
        // Desc: Returns whether or not the handle can be rendered. This is true if the 
        //       following properties return true: 'visible', 'handleStyle.visible'.
        //-----------------------------------------------------------------------------
        public bool     canRender           { get { return visible && handleStyle.visible; } }

        //-----------------------------------------------------------------------------
        // Name: hoverable (Public Property)
        // Desc: Returns or sets whether the handle can be hovered. This is in addition
        //       to the handle's style 'hoverable' property.
        //-----------------------------------------------------------------------------
        public bool     hoverable           { get; set; } = true;

        //-----------------------------------------------------------------------------
        // Name: canHover (Public Property)
        // Desc: Returns whether or not the handle can be hovered. This is true if the 
        //       following properties return true: 'canRender', 'hoverable', 'handleStyle.hoverable'.
        //-----------------------------------------------------------------------------
        public bool     canHover            { get { return hoverable && handleStyle.hoverable && canRender; } }

        //-----------------------------------------------------------------------------
        // Name: alphaScale (Public Property)
        // Desc: Returns or sets the alpha scale value used to scale the handle alpha value.
        //-----------------------------------------------------------------------------
        public float    alphaScale          { get { return mAlphaScale; } set { mAlphaScale = Mathf.Max(0, value); } }

        //-----------------------------------------------------------------------------
        // Name: tag (Public Property)
        // Desc: Returns or sets the handle tag.
        //-----------------------------------------------------------------------------
        public EGizmoHandleTag  tag         { get; set; } = EGizmoHandleTag.None;

        //-----------------------------------------------------------------------------
        // Name: cullSphereEnabled (Public Property)
        // Desc: Returns or sets whether the cull sphere is enabled.
        //-----------------------------------------------------------------------------
        public bool     cullSphereEnabled   { get; set; }

        //-----------------------------------------------------------------------------
        // Name: cullSphere (Public Property)
        // Desc: Returns or sets the cull sphere. The cull sphere is used to cull the
        //       handle pixels that are occluded by the sphere from the perspective of
        //       the render camera. Culling can mean rejecting those pixels or simply
        //       changing their alpha value to indicate occlusion.
        // Note: The sphere radius is treated as a finalized value with zoom scale and
        //       any other scale values properly applied. For example, if the sphere is
        //       created from a sphere cap, you should call 'FVal' on the sphere cap and
        //       pass in the cap's style sphere radius.
        //-----------------------------------------------------------------------------
        public Sphere   cullSphere          { get; set; }

        //-----------------------------------------------------------------------------
        // Name: cullSphereMode (Public Property)
        // Desc: Returns or sets the cull sphere mode.
        //-----------------------------------------------------------------------------
        public EGizmoCullSphereMode             cullSphereMode      { get; set; } = EGizmoCullSphereMode.Behind;

        //-----------------------------------------------------------------------------
        // Name: cullSphereTargets (Public Property)
        // Desc: Returns or sets the cull sphere targets. For example, if you wish to
        //       use culling for rendering only, but allow the culled pixels to be
        //       hovered, you could set this to 'EGizmoHandleCullSphereTargets.Hover'.
        //-----------------------------------------------------------------------------
        public EGizmoHandleCullSphereTargets    cullSphereTargets   { get; set; } = EGizmoHandleCullSphereTargets.All;

        //-----------------------------------------------------------------------------
        // Name: cullHoveredPixels (Public Property)
        // Desc: Returns or sets whether pixels should be culled when the handle is
        //       hovered. This is used when 'cullSphereEnabled' is enabled and when the
        //       'cullSphereTargets' property has the 'Render' flag set.
        //-----------------------------------------------------------------------------
        public bool cullHoveredPixels   { get; set; } = true;  

        //-----------------------------------------------------------------------------
        // Name: zoomScaleMode (Public Property)
        // Desc: Returns or sets the handle's zoom scale mode. Sometimes it's useful to
        //       let the handle calculate its own zoom scale or simply force it to use 
        //       a zoom scale of 1 and this property can be used for this purpose.
        //-----------------------------------------------------------------------------
        public EGizmoHandleZoomScaleMode zoomScaleMode  { get; set; } = EGizmoHandleZoomScaleMode.Auto;

        //-----------------------------------------------------------------------------
        // Name: manualZoomScale (Public Property)
        // Desc: Returns or sets the manual zoom scale used when the zoom scale mode is
        //       set to 'Manual'.
        //-----------------------------------------------------------------------------
        public float manualZoomScale    { get { return mManualZoomScale; } set { mManualZoomScale = Mathf.Max(value, 0.0f); } }

        //-----------------------------------------------------------------------------
        // Name: zoomScaleGizmo (Public Property)
        // Desc: Returns or sets the zoom scale gizmo used when the zoom scale mode is
        //       set to 'FromGizmo'.
        //-----------------------------------------------------------------------------
        public Gizmo zoomScaleGizmo     { get; set; }

        //-----------------------------------------------------------------------------
        // Name: transformScaleMode (Public Property)
        // Desc: Returns or sets the handle's transform scale mode which controls how
        //       the handle uses the scale stored in its transform. Sometimes it's useful
        //       to ignore the handle scale. For example, a point light gizmo has to have
        //       a radius that reflects the light range exactly.
        //-----------------------------------------------------------------------------
        public EGizmoHandleTransformScaleMode   transformScaleMode  { get; set; } = EGizmoHandleTransformScaleMode.Use;

        //-----------------------------------------------------------------------------
        // Name: hoverPaddingOffset (Public Property)
        // Desc: Returns or sets the hover padding offset. This is a value that is added
        //       to the global hover padding value and is useful when some handles are
        //       hard to pick with the global padding value.
        //-----------------------------------------------------------------------------
        public float hoverPaddingOffset  { get { return mHoverPaddingOffset; } set { mHoverPaddingOffset = value; } }
        #endregion

        #region Public Abstract Properties
        //-----------------------------------------------------------------------------
        // Name: handleStyle (Public Abstract Property)
        // Desc: Returns the current style assigned to the handle.
        //-----------------------------------------------------------------------------
        public abstract GizmoHandleStyle handleStyle { get; }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: ZSortHandles (Public Static Function)
        // Desc: Sorts the specified handles to ensure correct back-to-front rendering.
        //       Necessary because most of the times the Z test will be disabled when
        //       rendering gizmo handles.
        // Parm: sliders        - Input handles.
        //       renderArgs     - Render arguments.
        //       sortedSliders  - Returns the sorted handle list.
        //       T              - Gizmo handle type. Must derive from 'GizmoHandle'.
        //-----------------------------------------------------------------------------
        public static void ZSortHandles<T>(List<T> handles, GizmoHandleRenderArgs renderArgs, List<T> sortedHandles)
            where T : GizmoHandle
        {
            // Store unsorted handles
            sortedHandles.Clear();
            sortedHandles.AddRange(handles);

            // Sort
            Vector3 camPos     = renderArgs.camera.transform.position;
            Vector3 camForward = renderArgs.camera.transform.forward;
            sortedHandles.Sort((s0, s1) => 
            {
                float d0 = Vector3.Dot(s0.transform.position - camPos, camForward);
                float d1 = Vector3.Dot(s1.transform.position - camPos, camForward);
                return d1.CompareTo(d0);
            });
        }

        //-----------------------------------------------------------------------------
        // Name: ZSortSliders (Public Static Function)
        // Desc: Sorts the specified sliders to ensure correct back-to-front rendering.
        //       Necessary because most of the times the Z test will be disabled when
        //       rendering gizmo handles.
        // Parm: sliders        - Input sliders.
        //       renderArgs     - Render arguments.
        //       sortedSliders  - Returns the sorted slider list.
        //-----------------------------------------------------------------------------
        public static void ZSortSliders(List<GizmoLineSlider> sliders, GizmoHandleRenderArgs renderArgs, List<GizmoLineSlider> sortedSliders)
        {
            // Store unsorted sliders
            sortedSliders.Clear();
            sortedSliders.AddRange(sliders);

            // Sort
            Vector3 camPos     = renderArgs.camera.transform.position;
            Vector3 camForward = renderArgs.camera.transform.forward;
            sortedSliders.Sort((s0, s1) => 
            {
                float d0 = Vector3.Dot(s0.ToSegment(renderArgs.camera).end - camPos, camForward);
                float d1 = Vector3.Dot(s1.ToSegment(renderArgs.camera).end - camPos, camForward);
                return d1.CompareTo(d0);
            });
        }

        //-----------------------------------------------------------------------------
        // Name: ZSortRenderSliderAndCap (Public Static Function)
        // Desc: Sorts the specified slider and cap to ensure correct back-to-front rendering.
        //       Necessary because most of the times the Z test will be disabled when
        //       rendering gizmo handles.
        // Parm: slider     - The slider.
        //       cap        - The slider cap.
        //       renderArgs - Render arguments.
        //-----------------------------------------------------------------------------
        public static void ZSortRenderSliderAndCap(GizmoLineSlider slider, GizmoCap cap, GizmoHandleRenderArgs renderArgs)
        {
            // Store data
            Camera camera = renderArgs.camera;

            // If the cap is a quad or a circle or anything else that is flat, always render it on top. Looks better.
            if (cap.capStyle.capType.IsFlat())
            {
                slider.Render(renderArgs);
                cap.Render(renderArgs);
                return;
            }

            // If the slider is looking at the camera, render the slider first and then the cap.
            // Otherwise, reverse order.
            if (slider.IsFacingCamera(renderArgs.camera))
            {
                slider.Render(renderArgs);
                cap.Render(renderArgs);
            }
            else
            {
                cap.Render(renderArgs);
                slider.Render(renderArgs);
            }
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: GetZoomScale() (Public Function)
        // Desc: Returns the handle's zoom scale value.
        // Parm: camera - The camera that interacts with or renders the handle.
        // Rtrn: The zoom scale the handle should use for rendering or interaction based
        //       on the active zoom scale mode. If the zoom scale mode is set to 'FromGizmo'
        //       but no zoom scale gizmo was specified, the function returns 1.
        //-----------------------------------------------------------------------------
        public float GetZoomScale(Camera camera)
        {
            switch (zoomScaleMode)
            {
                case EGizmoHandleZoomScaleMode.Auto:        return RTGizmos.CalculateZoomScale(transform.position, camera);
                case EGizmoHandleZoomScaleMode.Manual:      return mManualZoomScale;
                case EGizmoHandleZoomScaleMode.One:         return 1.0f;
                case EGizmoHandleZoomScaleMode.FromGizmo:   return zoomScaleGizmo != null ? zoomScaleGizmo.internal_zoomScale : 1.0f;
                default:                                    return 1.0f;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: SetClipPlane() (Public Function)
        // Desc: Sets the clip plane with the specified index.
        // Parm: index     - Clip plane index. Currently unused. Reserved for future use.
        //       clipPlane - The clip plane.
        //-----------------------------------------------------------------------------
        public void SetClipPlane(int index, Plane clipPlane)
        {
            mClipPlanes[index].plane = clipPlane;
        }

        //-----------------------------------------------------------------------------
        // Name: SetClipPlaneEnabled() (Public Function)
        // Desc: Sets the enabled state of the clip plane with the specified index.
        // Parm: index     - Clip plane index. Currently unused. Reserved for future use.                  
        //       enabled   - The clip plane enabled state.
        //-----------------------------------------------------------------------------
        public void SetClipPlaneEnabled(int index, bool enabled)
        {
            mClipPlanes[index].enabled = enabled;
        }

        //-----------------------------------------------------------------------------
        // Name: GetClipPlane() (Public Function)
        // Desc: Returns the clip plane with the specified index.
        // Parm: index - Clip plane index. Currently unused. Reserved for future use.
        // Rtrn: The clip plane with the specified index.
        //-----------------------------------------------------------------------------
        public GizmoClipPlane GetClipPlane(int index)
        {
            return mClipPlanes[0];
        }

        //-----------------------------------------------------------------------------
        // Name: FVal() (Public Function)
        // Desc: Given a value that describes the handle in some way (radius, length, width,
        //       height etc), the function will return the actual value that should be used
        //       to render or interact with the handle based on zoom scale and other factors.
        // Note: 'FVal' stands for 'FinalizeValue'. It's more convenient to use the short
        //       version while writing code.
        // Parm: val    - The value to finalize.
        //       camera - The camera that interacts with or renders the handle.
        // Rtrn: The finalized value.
        //-----------------------------------------------------------------------------
        public float FVal(float val, Camera camera)
        {
            // Finalize the value
            if (transformScaleMode != EGizmoHandleTransformScaleMode.One)
                val *= transform.scale;
            if (zoomScaleMode != EGizmoHandleZoomScaleMode.One)
                val *= GetZoomScale(camera);

            // Return value
            return val;
        }

        //-----------------------------------------------------------------------------
        // Name: IsPointCulled() (Public Function)
        // Desc: Checks if the specified point is culled from the perspective of the
        //       specified camera. This is useful for rejecting hover intersection points
        //       when the cull sphere is enabled.
        // Parm: pt     - Query point.
        //       camera - Cull camera.
        // Rtrn: True if the point is culled and false otherwise.
        //-----------------------------------------------------------------------------
        public bool IsPointCulled(Vector3 pt, Camera camera)
        {
            // No-op?
            if (!cullSphereEnabled)
                return false;

            // Create the ray used for culling
            Ray ray = new Ray();
            if (camera.orthographic)
            {
                Vector3 cameraForward = camera.transform.forward;
                ray.origin = pt - cameraForward * Vector3.Dot(cameraForward, pt - camera.transform.position);
                ray.direction = cameraForward;
            }
            else
            {
                ray.origin = camera.transform.position;
                ray.direction = pt - ray.origin;
            }
           
            // The point is culled when the point normal is facing away from the camera. This essentially treats the
			// sphere as if it has an infinite radius. If this check fails, we will do a raycast to see if the point
			// is actually occluded.
            if (Vector3.Dot(pt - cullSphere.center, ray.direction) >= 0.0f) 
			{
                // Is the point really culled?
                if (cullSphere.Raycast(ray))
                    return true;
			}
			else
            if (cullSphereMode == EGizmoCullSphereMode.Full)  // Only do the inside check if we are allowed to do so
			{
				// The point normal is facing the camera but, the point may be inside the sphere
				if ((pt - cullSphere.center).magnitude - cullSphere.radius < -1e-2f * GetZoomScale(camera))
				    return true;
			}

            // Not culled
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: IsRenderHovered() (Public Function)
        // Desc: Checks if the handle is treated as hovered when rendering. 
        // Rtrn: True if the handle or one of its render hover connections is hovered
        //       by the mouse cursor.
        //-----------------------------------------------------------------------------
        public bool IsRenderHovered()
        {
            return this == RTGizmos.get.hoveredHandle ||
                mRenderHoverConnections.Contains(RTGizmos.get.hoveredHandle);
        }

        //-----------------------------------------------------------------------------
        // Name: AddRenderHoverConnection() (Public Function)
        // Desc: Adds a render hover connection. A render hover connection allows handles
        //       to borrow the hovered state from each other when rendered. This is useful in
        //       situations where the cursor hovers one handle, but other handles need to
        //       be highlighted during rendering (e.g. move gizmo axis slider and cap).
        // Parm: handle - The hover connection handle.
        //-----------------------------------------------------------------------------
        public void AddRenderHoverConnection(GizmoHandle handle)
        {
            // No-op?
            if (handle == null || handle == this || mRenderHoverConnections.Contains(handle))
                return;

            // Add connection
            mRenderHoverConnections.Add(handle);
            handle.mRenderHoverConnections.Add(this);
        }

        //-----------------------------------------------------------------------------
        // Name: RemoveRenderHoverConnection() (Public Function)
        // Desc: Removes a render hover connection.
        // Parm: handle - The hover connection handle to be removed.
        //-----------------------------------------------------------------------------
        public void RemoveRenderHoverConnection(GizmoHandle handle)
        {
            // No-op?
            if (handle == null || handle == this)
                return;

            // Remove connection
            if (mRenderHoverConnections.Remove(handle))
                handle.mRenderHoverConnections.Remove(this);
        }

        //-----------------------------------------------------------------------------
        // Name: HoverRaycast (Public Function)
        // Desc: Performs a hover raycast check against the gizmo handle.
        // Parm: args   - Raycast arguments.
        //       t      - Returns the distance from the ray origin where the intersection
        //                happens.
        // Rtrn: True if the ray intersects the handle and false otherwise.
        //-----------------------------------------------------------------------------
        public bool HoverRaycast(GizmoHoverRaycastArgs args, out float t)
        {
            // Clear output
            t = 0.0f;

            // No-op?
            if (!canHover)
                return false;

            // Raycast
            if (Raycast(args, out t))
            {
                // Is the hit point culled?
                if ((cullSphereTargets & EGizmoHandleCullSphereTargets.Hover) != 0)
                {
                    if (IsPointCulled(args.ray.GetPoint(t), args.camera))
                        return false;
                }

                // We have a hit!
                return true;
            }

            // No hit
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: Render (Public Function)
        // Desc: Renders the handle.
        // Parm: args - Render arguments.
        //-----------------------------------------------------------------------------
        public void Render(GizmoHandleRenderArgs args)
        {
            // No-op?
            if (!canRender)
                return;
      
            // Render
            OnRender(args);
        }
        #endregion

        #region Public Abstract Functions
        //-----------------------------------------------------------------------------
        // Name: CalculateAABB (Public Abstract Function)
        // Desc: Calculates and returns the handle's AABB.
        // Parm: camera - The camera that interacts with or renders the handle.
        // Rtrn: The handle's AABB.
        //-----------------------------------------------------------------------------
        public abstract Box CalculateAABB(Camera camera);
        #endregion

        #region Protected Abstract Functions
        //-----------------------------------------------------------------------------
        // Name: Raycast (Protected Abstract Function)
        // Desc: Performs a raycast check against the gizmo handle.
        // Parm: args   - Raycast arguments.
        //       t      - Returns the distance from the ray origin where the intersection
        //                happens.
        // Rtrn: True if the ray intersects the handle and false otherwise.
        //-----------------------------------------------------------------------------
        protected abstract bool Raycast(GizmoHoverRaycastArgs args, out float t);

        //-----------------------------------------------------------------------------
        // Name: OnRender (Protected Abstract Function)
        // Desc: Called when the handle must render itself.
        // Parm: args - Render arguments.
        //-----------------------------------------------------------------------------
        protected abstract void OnRender(GizmoHandleRenderArgs args);
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: GetHoverPadding (Protected Function)
        // Desc: Returns the hover padding used by this handle.
        //-----------------------------------------------------------------------------
        protected float GetHoverPadding()
        {
            return Mathf.Max(0.0f, RTGizmos.get.skin.globalGizmoStyle.hoverPadding + mHoverPaddingOffset);
        }
        #endregion
    }
    #endregion
}
