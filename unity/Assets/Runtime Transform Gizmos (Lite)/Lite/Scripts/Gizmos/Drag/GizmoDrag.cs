using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EGizmoDragType (Public Enum)
    // Desc: Defines different gizmo drag operation types.
    //-----------------------------------------------------------------------------
    public enum EGizmoDragType
    {
        Move = 0,       // A drag operation that moves along a single axis
        DblMove,        // A drag operation that moves along 2 axes
        VertexSnap,     // A drag operation that performs vertex snapping
        SurfaceSnap,    // A drag operation that snaps to the surface of scene entities such as objects or grid
        Rotate,         // A drag operation that rotates around a single axis
        DblRotate,      // A drag operation that rotates around 2 axes
        Scale,          // A drag operation that scales along a single axis
        DblScale,       // A drag operation that scales along 2 axes
        UniformScale    // A drag operation that scales along all axes
    }

    //-----------------------------------------------------------------------------
    // Name: EGizmoDragSettingSource (Public Enum)
    // Desc: Defines different gizmo drag setting sources which control the way in
    //       which drag-related settings can be specified.
    //-----------------------------------------------------------------------------
    public enum EGizmoDragSettingSource
    {
        UseGlobal = 0,      // Use global settings
        Custom              // Use a custom value
    }

    //-----------------------------------------------------------------------------
    // Name: EGizmoDragSnapMode (Public Enum)
    // Desc: Defines how a drag operation's snap state is determined.
    //-----------------------------------------------------------------------------
    public enum EGizmoDragSnapMode
    {
        UseGlobal = 0,      // Use global snapping settings
        Enabled,            // Snapping is explicitly enabled
        Disabled            // Snapping is explicitly disabled
    }

    //-----------------------------------------------------------------------------
    // Name: EGizmoDragChannel (Public Enum)
    // Desc: Defines different gizmo drag channels. An drag channel is a way of
    //       specifying how the drag values will be consumed. When a drag operation
    //       is updated and produces a drag delta for example, that delta could be
    //       used to update position, rotation or scale.
    //-----------------------------------------------------------------------------
    public enum EGizmoDragChannel
    {
        Position = 0,   // Drag values are used to change position
        Rotation,       // Drag values are used to change rotation
        Scale           // Drag values are used to change scale
    }
    #endregion

    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: GizmoDragDesc (Public Struct)
    // Desc: Describes a gizmo drag operation.
    //-----------------------------------------------------------------------------
    public struct GizmoDragDesc
    {
        #region Public Fields
        public EGizmoDragType               dragType;               // Drag type
        public Vector3                      axis0;                  // First drag axis for a double-axis drag and the drag axis for a single-axis drag
        public Vector3                      axis1;                  // Second drag axis for a double-axis drag. Ignored for single-axis drag ops.
        public int                          axisIndex0;             // First drag axis index
        public int                          axisIndex1;             // Second drag axis index
        public IReadOnlyList<GameObject>    vSnapPivotObjects;      // Used with vertex snapping and it stores all objects that are going to be snapped along with the snap pivot  
        public Vector3                      rotationCenter;         // The center of the rotation circle/sphere etc
        #endregion
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GizmoDrag (Public Class)
    // Desc: Represents a gizmo drag operation. A drag operation starts when the input
    //       device is pressed over a gizmo and is updated every time the device is
    //       moved to produce drag values.
    //-----------------------------------------------------------------------------
    public class GizmoDrag
    {
        #region Private Fields
        bool                mActive;            // Is the drag operation active (i.e. is the user dragging the gizmo handles)?
        Camera              mCamera;            // The camera that interacts with the gizmo handles
        Gizmo               mGizmo;             // The gizmo involved in the drag operation
        GizmoTransform      mGizmoTransform;    // The gizmo transform. This could be the transform of a gizmo or a gizmo handle.
        GizmoDragDesc       mDesc;              // Drag descriptor
        EGizmoDragChannel   mDragChannel;       // Drag channel established based on the drag type

        GizmoPlaneDrag      mPlaneDrag      = new GizmoPlaneDrag();     // Plane drag operation used to generate drag values
        Vector3             mTotalDrag      = Vector3.zero;             // Total drag
        Vector3             mDragDelta      = Vector3.zero;             // The drag delta produced during the last update step

        // Move
        Vector3             mCurrentPosition;           // Keeps track of the current position. Useful when snapping is enabled.

        // Rotation
        Plane               mRotationPlane      = new Plane();          // Rotation plane used for single axis rotation ops
        Quaternion          mRotationDelta      = Quaternion.identity;  // The rotation generated during the last update step
        Vector3[]           mRotationDragAxes   = new Vector3[3];       // Drag axes used to produce rotation values for each rotation axis
        float[]             mAccumRotations     = new float[3];         // Keeps track of accumulated rotation for each rotation axis. Useful when snapping is enabled.

        // Scale
        float               mCurrentScale       = 1.0f; // The current scale that has been produced
        float               mAccumScale         = 0.0f; // Accumulated scale. Useful when snapping is enabled.
        float               mScale              = 1.0f; // Same as 'mCurrentScale' but can be different when snapping is used.
        float               mScaleSensitivity   = 1.0f; // Mouse sensitivity used for scale drags

        // Surface snap
        ObjectFilter        mSurfaceSnapFilter  = new ObjectFilter()    // Object filter used during surface snapping
        {
            objectTypes = EGameObjectType.Mesh | EGameObjectType.Sprite | EGameObjectType.Terrain
        };
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: active (Public Property)
        // Desc: Returns whether or not the drag operation is active. This is true as
        //       long as the user is dragging the gizmo handles.
        //-----------------------------------------------------------------------------
        public bool         active          { get { return mActive; } }

        //-----------------------------------------------------------------------------
        // Name: totalDrag (Public Property)
        // Desc: Returns the total drag. This is the sum of all drag deltas that have 
        //       been produced. The way in which this value is interpreted is the same
        //       as 'dragDelta'.
        //-----------------------------------------------------------------------------
        public Vector3      totalDrag       { get { return mTotalDrag; } }

        //-----------------------------------------------------------------------------
        // Name: dragDelta (Public Property)
        // Desc: Returns the drag delta. This is the amount of drag produced during the
        //       last drag update. The way in which this value is interpreted depends
        //       on the drag type:
        //          1. Move, DblMove, VertexSnap     - move offset.
        //          2. Rotate, DblRotate             - angle deltas in degrees.
        //          3. Scale, DblScale, UniformScale - scale delta.
        //-----------------------------------------------------------------------------
        public Vector3      dragDelta       { get { return mDragDelta; } }

        //-----------------------------------------------------------------------------
        // Name: rotationDelta (Public Property)
        // Desc: Returns the rotation delta. This is the amount of rotation produced during
        //       the last drag update. This rotation corresponds to the angle deltas stored
        //       in 'dragDelta'.
        //-----------------------------------------------------------------------------
        public Quaternion   rotationDelta   { get { return mRotationDelta; } }

        //-----------------------------------------------------------------------------
        // Name: moveGridSnapDescSource (Public Property)
        // Desc: Returns or sets the move grid snap descriptor source.
        //-----------------------------------------------------------------------------
        public EGizmoDragSettingSource  moveGridSnapDescSource  { get; set; } = EGizmoDragSettingSource.UseGlobal;
            
        //-----------------------------------------------------------------------------
        // Name: moveGridSnapDesc (Public Property)
        // Desc: Returns or sets the move grid snap descriptor. Used when 'moveGridSnapDescSource'
        //       is 'Custom'.
        //-----------------------------------------------------------------------------
        public GridSnapDesc             moveGridSnapDesc        { get; set; } = new GridSnapDesc();           

        //-----------------------------------------------------------------------------
        // Name: scaleSnapStepSource (Public Property)
        // Desc: Returns or sets the scale snap step source.
        //-----------------------------------------------------------------------------
        public EGizmoDragSettingSource  scaleSnapStepSource     { get; set; } = EGizmoDragSettingSource.UseGlobal;

        //-----------------------------------------------------------------------------
        // Name: scaleSnap (Public Property)
        // Desc: Returns or sets the scale snap step used when 'scaleSnapStepSource'
        //       is 'Custom'.
        //-----------------------------------------------------------------------------
        public float                    scaleSnap               { get; set; } = 0.1f;

        //-----------------------------------------------------------------------------
        // Name: scaleSensitivitySource (Public Property)
        // Desc: Returns or sets the scale sensitivity source.
        //-----------------------------------------------------------------------------
        public EGizmoDragSettingSource  scaleSensitivitySource  { get; set; } = EGizmoDragSettingSource.UseGlobal;

        //-----------------------------------------------------------------------------
        // Name: scaleSensitivity (Public Property)
        // Desc: Returns or sets the mouse sensitivity used for scale drag operations.
        //       Used when 'scaleSensitivitySource' is 'Custom'.
        //-----------------------------------------------------------------------------
        public float                    scaleSensitivity        { get { return mScaleSensitivity; } set { mScaleSensitivity = Mathf.Clamp01(value); } }

        //-----------------------------------------------------------------------------
        // Name: snapMode (Public Property)
        // Desc: Returns or sets the snap mode.
        //-----------------------------------------------------------------------------
        public EGizmoDragSnapMode       snapMode                { get; set; } = EGizmoDragSnapMode.UseGlobal;

        //-----------------------------------------------------------------------------
        // Name: ignoreZoomScale (Public Property)
        // Desc: Returns or sets whether the drag operation should ignore zoom scale. 
        //       Some operations use the zoom scale to scale the drag values to produce
        //       constant offsets regardless of the distance between the camera and the
        //       drag point. However, this can be undesirable in some situations, and
        //       a good example is gizmos which don't use the zoom scale to scale their
        //       handles (e.g. point light gizmo). 
        //-----------------------------------------------------------------------------
        public bool             ignoreZoomScale         { get; set; } = false;

        //-----------------------------------------------------------------------------
        // Name: surfaceSnapObjectTypes (Public Property)
        // Desc: Returns or sets the object types which can be used as a snap surface.
        //       Used when the drag type is  'SurfaceSnap'.
        //-----------------------------------------------------------------------------
        public EGameObjectType  surfaceSnapObjectTypes  { get { return mSurfaceSnapFilter.objectTypes; } set { mSurfaceSnapFilter.objectTypes = value; } }

        //-----------------------------------------------------------------------------
        // Name: gridSurfaceSnapEnabled (Public Property)
        // Desc: Returns or sets whether the grid can be used as a snap surface. Used when
        //       the drag type is 'SurfaceSnap'.
        //-----------------------------------------------------------------------------
        public bool             gridSurfaceSnapEnabled  { get; set; } = true;

        //-----------------------------------------------------------------------------
        // Name: desc (Public Property)
        // Desc: Returns the drag operation descriptor.
        //-----------------------------------------------------------------------------
        public GizmoDragDesc        desc            { get { return mDesc; } }

        //-----------------------------------------------------------------------------
        // Name: dragChannel (Public Property)
        // Desc: Returns the drag channel.
        //-----------------------------------------------------------------------------
        public EGizmoDragChannel    dragChannel     { get { return mDragChannel; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: IsSnapEnabled() (Public Function)
        // Desc: Returns whether or not snapping is enabled by taking into account the
        //       current snap mode.
        // Rtrn: True if snapping is enabled and false otherwise.
        //-----------------------------------------------------------------------------
        public bool IsSnapEnabled()
        {
            switch (snapMode)
            {
                case EGizmoDragSnapMode.UseGlobal:  return RTGizmos.get.snapEnabled;
                case EGizmoDragSnapMode.Enabled:    return true;
                case EGizmoDragSnapMode.Disabled:   return false;
                default: return false;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: Start() (Public Function)
        // Desc: Starts a gizmo drag operation. The function has no effect if the drag
        //       operation is already active.
        // Parm: desc       - Drag operation descriptor.
        //       gizmo      - The gizmo that starts the drag operation.
        //       transform  - The transform that is affected by the drag. This could be
        //                    the gizmo transform or the transform of one of the gizmo handles.
        //                    Only position and rotation drags affect the transform.
        //       camera     - The camera that interacts with the gizmo handles. 
        // Rtrn: True if the gizmo drag operation started and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Start(GizmoDragDesc desc, Gizmo gizmo, GizmoTransform transform, Camera camera)
        {
            // No-op?
            if (mActive)
                return false;

            // Validate call
            if (gizmo == null)
                return false;
            if (mDesc.dragType == EGizmoDragType.VertexSnap &&
                (mDesc.vSnapPivotObjects == null || mDesc.vSnapPivotObjects.Count == 0))
                return false;

            // Start dragging
            mGizmo              = gizmo;
            mGizmoTransform     = transform;
            mCurrentPosition    = mGizmoTransform.position;
            mAccumRotations[0]  = 0.0f;
            mAccumRotations[1]  = 0.0f;
            mAccumRotations[2]  = 0.0f;
            mDesc               = desc;
            mActive             = true;
            mCamera             = camera;
            mTotalDrag          = Vector3.zero;
            mDragDelta          = Vector3.zero;
            mRotationDelta      = Quaternion.identity;
            mCurrentScale       = 1.0f;
            mScale              = 1.0f;

            // Normalize axes
            mDesc.axis0.Normalize();
            mDesc.axis1.Normalize();

            // Initialize based on the drag type
            switch (mDesc.dragType)
            {
                case EGizmoDragType.Move:
                case EGizmoDragType.DblMove:

                    mDragChannel = EGizmoDragChannel.Position;
                    mPlaneDrag.Start(CreateDragPlane(), mGizmo.dragData.dragStartHoverPoint, mCamera);
                    break;

                case EGizmoDragType.VertexSnap:
                case EGizmoDragType.SurfaceSnap:

                    mDragChannel = EGizmoDragChannel.Position;
                    break;

                case EGizmoDragType.Rotate:

                    mDragChannel                        = EGizmoDragChannel.Rotation;
                    mRotationPlane                      = new Plane(mDesc.axis0, mDesc.rotationCenter);
                    mRotationDragAxes[mDesc.axisIndex0] = CalculateRotationDragAxis();
                    break;

                case EGizmoDragType.DblRotate:

                    mDragChannel    = EGizmoDragChannel.Rotation;
                
                    // Calculate the rotation drag axes. These are simply the rotation axes rotated 90 degrees in screen space.
                    Vector3 p0      = mDesc.rotationCenter;
                    Vector3 p1      = mDesc.rotationCenter + mDesc.axis0;
                    mRotationDragAxes[mDesc.axisIndex0] = (mCamera.WorldToScreenPoint(p1) - mCamera.WorldToScreenPoint(p0)).normalized;
                    mRotationDragAxes[mDesc.axisIndex0] = new Vector3(-mRotationDragAxes[mDesc.axisIndex0].y, mRotationDragAxes[mDesc.axisIndex0].x, 0.0f);
                    p1              = mDesc.rotationCenter + mDesc.axis1;
                    mRotationDragAxes[mDesc.axisIndex1] = (mCamera.WorldToScreenPoint(p1) - mCamera.WorldToScreenPoint(p0)).normalized;
                    mRotationDragAxes[mDesc.axisIndex1] = new Vector3(-mRotationDragAxes[mDesc.axisIndex1].y, mRotationDragAxes[mDesc.axisIndex1].x, 0.0f);
                    break;

                case EGizmoDragType.Scale:
                case EGizmoDragType.DblScale:
                case EGizmoDragType.UniformScale:

                    mDragChannel = EGizmoDragChannel.Scale;
                    mPlaneDrag.Start(CreateDragPlane(), mGizmo.dragData.dragStartHoverPoint, mCamera);
                    break;
            }

            // Success!
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: Update() (Public Function)
        // Desc: Allows the drag operation to update itself and produce drag values.
        //       Should be called once per update step. The function has no effect if the
        //       drag operation is not active.
        // Rtrn: True if the drag operation was updated and drag values were produced and
        //       false otherwise.
        //-----------------------------------------------------------------------------
        public bool Update()
        {
            // Locals
            Vector3 vMove;
            float scaleOffset, zoomScale,
                  offset0, offset1;

            // No-op?
            if (!mActive)
                return false;

            // Reset values
            mDragDelta      = Vector3.zero;
            mRotationDelta  = Quaternion.identity;
            if (!RTInput.get.pointingInputDevice.moved)
                return false;

            // Cache data
            Vector3 oldTransformPos = mGizmoTransform.position;
            var     gridSnapDesc    = RTGrid.get.snapDesc;

            // If we are using absolute snapping and we are move-snapping along a single axis,
            // we have force a relative snap if the axis is not aligned with the corresponding
            // world axis. Otherwise, it looks weird.
            bool forceRelativeSnap = false;
            if (mDesc.dragType == EGizmoDragType.Move)
            {
                if (MathEx.FastAbs(1.0f - Vector3Ex.AbsDot(mDesc.axis0, Core.axes[mDesc.axisIndex0])) > 1e-5f)
                    forceRelativeSnap = true;
            }

            // If relative snapping is enabled and we are producing move values, the gizmo transform
            // represents the grid coordinate system, so we have to update the grid snap config.
            if (forceRelativeSnap || ((mDesc.dragType == EGizmoDragType.Move || mDesc.dragType == EGizmoDragType.DblMove) && 
                RTGizmos.get.moveSnapMode == EGizmoMoveSnapMode.Relative))
            {
                gridSnapDesc.origin     = mGizmoTransform.position;
                gridSnapDesc.right      = mGizmoTransform.right;
                gridSnapDesc.up         = mGizmoTransform.up;
                gridSnapDesc.forward    = mGizmoTransform.forward;
            }

            // Are we using custom move grid snap?
            if (moveGridSnapDescSource == EGizmoDragSettingSource.Custom)
                gridSnapDesc = moveGridSnapDesc;

            // Update plane drag here. Although not all types of drag ops require a plane drag,
            // most of them do and if it's not needed, the call is simply ignored.
            mPlaneDrag.Update();

            // Produce drag values based on the drag type
            switch (mDesc.dragType)
            {
                case EGizmoDragType.Move:

                    // Calculate the move vector and update the current position
                    vMove = mDesc.axis0 * Vector3.Dot(mDesc.axis0, mPlaneDrag.dragDelta);
                    mCurrentPosition += vMove;

                    // Snap?
                    if (IsSnapEnabled())
                        mGizmoTransform.position = Snap.GridSnapAxis(mCurrentPosition, gridSnapDesc, mDesc.axisIndex0);
                    else mGizmoTransform.position = mCurrentPosition;

                    // Update drag
                    mDragDelta = mGizmoTransform.position - oldTransformPos;
                    mTotalDrag += mDragDelta;
                    break;

                case EGizmoDragType.DblMove:

                    // Apply the drag delta to the current position
                    mCurrentPosition += mPlaneDrag.dragDelta;

                    // Snap?
                    if (IsSnapEnabled())
                    {
                        var axisMask = new Vector3Int();
                        axisMask[mDesc.axisIndex0] = 1;
                        axisMask[mDesc.axisIndex1] = 1;
                        mGizmoTransform.position = Snap.GridSnapAxes(mCurrentPosition, gridSnapDesc, axisMask);
                    }
                    else mGizmoTransform.position = mCurrentPosition;

                    // Update drag
                    mDragDelta = mGizmoTransform.position - oldTransformPos;
                    mTotalDrag += mDragDelta;
                    break;

                case EGizmoDragType.VertexSnap:

                    // Snap the pivot
                    if (GizmoVertexSnap.SnapPivot(mDesc.vSnapPivotObjects, mCamera, mGizmoTransform.position, out Vector3 snappedPivot))
                    {
                        // Calculate drag and update position
                        mDragDelta = snappedPivot - mGizmoTransform.position;
                        mTotalDrag += mDragDelta;
                        mGizmoTransform.position = snappedPivot.FixFloatError();
                    }
                    break;

                case EGizmoDragType.SurfaceSnap:

                    // Raycast scene
                    if (RTScene.get.Raycast(RTInput.get.pointingInputDevice.GetPickRay(mCamera), 
                        mSurfaceSnapFilter, gridSurfaceSnapEnabled, out SceneRayHit rayHit))
                    {
                        // Snap?
                        Vector3 newPos = rayHit.closestHit;
                        if (IsSnapEnabled())
                            newPos = Snap.GridSnapAllAxes(newPos, gridSnapDesc);

                        // Update drag data and snap position to pick point
                        mDragDelta = newPos - mGizmoTransform.position;
                        mTotalDrag += mDragDelta;
                        mGizmoTransform.position = newPos;
                    }
                    break;

                case EGizmoDragType.Rotate:

                    // Do rotation drag and apply rotation
                    mRotationDelta = DoRotationDrag(mDesc.axisIndex0, mDesc.axis0);
                    mGizmoTransform.rotation = mRotationDelta * mGizmoTransform.rotation;
                    break;

                case EGizmoDragType.DblRotate:

                    // Do rotation drag and apply rotation
                    Quaternion r0 = DoRotationDrag(mDesc.axisIndex0, mDesc.axis0);
                    Quaternion r1 = DoRotationDrag(mDesc.axisIndex1, mDesc.axis1);
                    mRotationDelta = r1 * r0;
                    mGizmoTransform.rotation = mRotationDelta * mGizmoTransform.rotation;
                    break;

                case EGizmoDragType.Scale:

                    // Update current scale. This is done by calculating how much we have
                    // moved along the drag axis and adding that to the current scale.
                    // Note: Scale the offset by the inverse of the zoom scale the cancel the perspective effect. If
                    //       we don't do this, the further the camera is from the drag plane the bigger the drag deltas
                    //       will get because the cursor delta in screen space is mapped to a larger world space distance.
                    zoomScale   = ignoreZoomScale ? 1.0f : RTGizmos.CalculateZoomScale(mGizmo.dragData.dragStartHoverPoint, mCamera);
                    scaleOffset = Vector3.Dot(mPlaneDrag.dragDelta * GetScaleSensitivity() / zoomScale, mDesc.axis0);
                    mCurrentScale += scaleOffset;
                 
                    // Is snapping enabled?
                    if (IsSnapEnabled())
                    {
                        // Accumulate scale and snap if necessary
                        mAccumScale += scaleOffset;
                        float scaleSnap = GetScaleSnap();
                        if (MathEx.FastAbs(mAccumScale) >= scaleSnap)
                        {
                            // Snap the scale using the scale snap
                            float prevScale                 = mScale;
                            mScale                          = Snap.SnapValue(mCurrentScale, scaleSnap);
                            mDragDelta[mDesc.axisIndex0]    = Snap.SnapValue(mScale - prevScale, scaleSnap);
                            mAccumScale                     = 0.0f;
                        }
                    }
                    else
                    {
                        // Update scale and drag delta
                        mScale                          = mCurrentScale;
                        mDragDelta[mDesc.axisIndex0]    = scaleOffset;
                        mAccumScale                     = 0.0f;
                    }

                    // Update total drag and scale ratio
                    mTotalDrag[mDesc.axisIndex0] += mDragDelta[mDesc.axisIndex0];
                    break;

                case EGizmoDragType.DblScale:

                    // Calculate drag offsets along the 2 axes. Sum the up to get the total scale offset.
                    // Note: Scale the offsets by the inverse of the zoom scale the cancel the perspective effect. If
                    //       we don't do this, the further the camera is from the drag plane the bigger the drag deltas
                    //       will get because the cursor delta in screen space is mapped to a larger world space distance.
                    zoomScale   = ignoreZoomScale ? 1.0f : RTGizmos.CalculateZoomScale(mGizmo.dragData.dragStartHoverPoint, mCamera);
                    offset0     = Vector3.Dot(mPlaneDrag.dragDelta * GetScaleSensitivity() / zoomScale, mDesc.axis0);
                    offset1     = Vector3.Dot(mPlaneDrag.dragDelta * GetScaleSensitivity() / zoomScale, mDesc.axis1);
                    scaleOffset = offset0 + offset1;

                    // Update current scale
                    mCurrentScale += scaleOffset;

                    // Is snapping enabled?
                    if (IsSnapEnabled())
                    {
                        // Accumulate scale and snap if necessary
                        mAccumScale += scaleOffset;
                        float scaleSnap = GetScaleSnap();
                        if (MathEx.FastAbs(mAccumScale) >= scaleSnap)
                        {
                            // Snap the scale using the scale snap
                            float prevScale                 = mScale;
                            mScale                          = Snap.SnapValue(mCurrentScale, scaleSnap);
                            mDragDelta[mDesc.axisIndex0]    = Snap.SnapValue(mScale - prevScale, scaleSnap);
                            mDragDelta[mDesc.axisIndex1]    = mDragDelta[mDesc.axisIndex0];
                            mAccumScale                     = 0.0f;
                        }
                    }
                    else
                    {
                        // Update scale and drag delta
                        mScale                    = mCurrentScale;
                        mDragDelta[mDesc.axisIndex0]    = scaleOffset;
                        mDragDelta[mDesc.axisIndex1]    = mDragDelta[mDesc.axisIndex0];
                        mAccumScale                     = 0.0f;
                    }

                    // Update total drag and scale ratio
                    mTotalDrag += mDragDelta;;
                    break;

                case EGizmoDragType.UniformScale:

                    // Calculate drag offsets along the camera X and Y axes. Sum the up to get the total scale offset.
                    // Note: Scale the offsets by the inverse of the zoom scale the cancel the perspective effect. If
                    //       we don't do this, the further the camera is from the drag plane the bigger the drag deltas
                    //       will get because the cursor delta in screen space is mapped to a larger world space distance.
                    zoomScale   = ignoreZoomScale ? 1.0f : RTGizmos.CalculateZoomScale(mGizmo.dragData.dragStartHoverPoint, mCamera);
                    offset0     = Vector3.Dot(mPlaneDrag.dragDelta * GetScaleSensitivity() / zoomScale, mCamera.transform.right);
                    offset1     = Vector3.Dot(mPlaneDrag.dragDelta * GetScaleSensitivity() / zoomScale, mCamera.transform.up);
                    scaleOffset = offset0 + offset1;

                    // Update current scale
                    mCurrentScale += scaleOffset;

                    // Is snapping enabled?
                    if (IsSnapEnabled())
                    {
                        // Accumulate scale and snap if necessary
                        mAccumScale += scaleOffset;;
                        float scaleSnap = GetScaleSnap();
                        if (MathEx.FastAbs(mAccumScale) >= scaleSnap)
                        {
                            // Snap the scale using the scale snap
                            float prevScale     = mScale;
                            mScale              = Snap.SnapValue(mCurrentScale, scaleSnap);
                            mDragDelta          = Vector3Ex.FromValue(Snap.SnapValue(mScale - prevScale, scaleSnap));
                            mAccumScale         = 0.0f;
                        }
                    }
                    else
                    {
                        // Update scale and drag delta
                        mScale      = mCurrentScale;
                        mDragDelta  = Vector3Ex.FromValue(scaleOffset);
                        mAccumScale = 0.0f;
                    }

                    // Update total drag and scale ratio
                    mTotalDrag += mDragDelta;
                    break;
            }

            // Drag updated
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: End() (Public Function)
        // Desc: Ends the gizmo drag operation. The function has no effect if the drag
        //       operation is not active.
        // Rtrn: True if the gizmo drag operation ended and false otherwise.
        //-----------------------------------------------------------------------------
        public bool End()
        {
            // No-op?
            if (!mActive)
                return false;

            // Stop dragging
            mGizmo              = null;
            mGizmoTransform     = null;
            mActive             = false;
            mTotalDrag          = Vector3.zero;
            mDragDelta          = Vector3.zero;
            mRotationDelta      = Quaternion.identity;
            mScale              = 1.0f;
            mPlaneDrag.End();

            // Success!
            return true;
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: DoRotationDrag() (Private Function)
        // Desc: Performs a rotation drag.
        // Parm: axisIndex      - Rotation axis index (0 = X, 1 = Y, 2 = Z).
        //       rotationAxis   - Rotation axis.
        // Rtrn: A quaternion that represents the rotation delta that was produced.
        //-----------------------------------------------------------------------------
        Quaternion DoRotationDrag(int axisIndex, Vector3 rotationAxis)
        {
            // Calculate rotation
            float rotation = Vector3.Dot(mRotationDragAxes[axisIndex], RTInput.get.pointingInputDevice.delta) *
                    RTGizmos.get.skin.globalGizmoStyle.rotationSensitivity;
                    
            // Snap?
            if (IsSnapEnabled())
            {
                // Update accumulated rotation and snap if enough rotation has been accumulated
                mAccumRotations[axisIndex] += rotation;
                float rotationSnap = RTGizmos.get.skin.globalGizmoStyle.rotationSnap;
                if (MathEx.FastAbs(mAccumRotations[axisIndex]) >= rotationSnap)
                {
                    // Calculate drag values and rotation delta
                    mDragDelta[axisIndex]       = Snap.SnapValue(mAccumRotations[axisIndex], rotationSnap);
                    mTotalDrag[axisIndex]       += mDragDelta[axisIndex];
                    mAccumRotations[axisIndex]  = 0.0f;

                    // Return rotation delta
                    return Quaternion.AngleAxis(mDragDelta[axisIndex], rotationAxis);
                }
            }
            else
            {
                // Calculate drag values and rotation delta
                mDragDelta[axisIndex]       = rotation;
                mTotalDrag[axisIndex]       += mDragDelta[axisIndex];
                mRotationDelta              = Quaternion.AngleAxis(mDragDelta[axisIndex], rotationAxis);
                mAccumRotations[axisIndex]  = 0.0f;

                // Return rotation delta
                return Quaternion.AngleAxis(mDragDelta[axisIndex], rotationAxis);
            }

            // If we reach this point, it means no rotation has been produced
            return Quaternion.identity;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateDragPlane() (Private Function)
        // Desc: Creates and returns a drag plane required for different kinds of drag
        //       operations.
        // Rtrn: The drag plane.
        //-----------------------------------------------------------------------------
        Plane CreateDragPlane()
        {
            // Check operation type
            switch (mDesc.dragType)
            {
                case EGizmoDragType.Scale:
                case EGizmoDragType.Move:

                    // The ideal plane must satisfy the following conditions:
                    //  1. it cuts through the move axis (i.e. the move axis exists on the plane)
                    //  2. it is aligned with the camera look vector as much as possible.
                    // Cross the camera forward vector with the move axis.
                    Vector3 vCross = mCamera.transform.forward;
                    Vector3 vec    = Vector3.Cross(mDesc.axis0, vCross);                    
                    if (vec.magnitude < 1e-3f)      // If the forward vector is aligned with the move axis, we need to try again with another vector
                    {
                        vCross = mCamera.transform.right;
                        vec    = Vector3.Cross(mDesc.axis0, vCross);
                    }

                    // At this point we have a a vector 'vec' which is perpendicular to the move axis.
                    // We can now cross 'vec' with the move axis to get the normal of the drag plane.
                    return new Plane(Vector3.Cross(vec, mDesc.axis0).normalized, mGizmo.dragData.dragStartHoverPoint);

                case EGizmoDragType.DblScale:
                case EGizmoDragType.DblMove:

                    // We have 2 axes we can use to generate the plane normal
                    return new Plane(Vector3.Cross(mDesc.axis0, mDesc.axis1).normalized, mGizmo.dragData.dragStartHoverPoint);

                case EGizmoDragType.UniformScale:

                    // Create a plane that is aligned with the camera XY plane
                    return new Plane(mCamera.transform.forward, mGizmo.dragData.dragStartHoverPoint);

                default: return new Plane();
            }
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateRotationDragAxis() (Private Function)
        // Desc: Calculates a screen axis used to produce rotation values. The more aligned
        //       the cursor movement is to this axis, the stronger the rotation.
        // Rtrn: The rotation drag axis in screen space.
        //-----------------------------------------------------------------------------
        Vector3 CalculateRotationDragAxis()
        {
            // Raycast rotation plane and calculate the ideal screen drag vector. When the mouse
            // is moved along this direction in screen space, it will generate rotation values.
            // The more aligned the mouse is with this vector, the stronger the rotation.
            Vector2 dragAxis = Vector3.zero;
            Ray ray = RTInput.get.pointingInputDevice.GetPickRay(mCamera);
            if (mRotationPlane.Raycast(ray, out float t))
            {
                // Convert the intersection point to a vector tangent to the rotation circle
                Vector3 iPt     = ray.GetPoint(t);
                Vector3 v       = (iPt - mDesc.rotationCenter);
                Vector3 tangent = Vector3.Cross(mDesc.axis0, v);

                // Convert the tangent to the ideal screen space vector
                Vector2 tangentStart    = mCamera.WorldToScreenPoint(iPt);
                Vector2 tangentEnd      = mCamera.WorldToScreenPoint(iPt + tangent);
                dragAxis                = (tangentEnd - tangentStart).normalized;
            }
            else
            {
                // If we reach this point, it most likely means that the camera view vector is
                // aligned to the plane. In this case, the ideal vector can be obtained by rotating
                // the rotation axis by 90 degrees in screen space.
                Vector2 p0 = mCamera.WorldToScreenPoint(mDesc.rotationCenter);
                Vector2 p1 = mCamera.WorldToScreenPoint(mDesc.rotationCenter + mDesc.axis0);
                dragAxis = (p1 - p0).normalized;
                dragAxis = new Vector2(-dragAxis.y, dragAxis.x);
            }
              
            // Return rotation screen drag axis
            return dragAxis;
        }
        
        //-----------------------------------------------------------------------------
        // Name: GetScaleSnap() (Private Function)
        // Desc: Returns the scale snap step used for scale drags.
        // Rtrn: The scale snap step used for scale drags.
        //-----------------------------------------------------------------------------
        float GetScaleSnap()
        {
            return scaleSnapStepSource == EGizmoDragSettingSource.UseGlobal ? 
                RTGizmos.get.skin.globalGizmoStyle.scaleSnap : scaleSnap;
        }

        //-----------------------------------------------------------------------------
        // Name: GetScaleSensitivity() (Private Function)
        // Desc: Returns the scale sensitivity used for scale drags.
        // Rtrn: The scale sensitivity used for scale drags.
        //-----------------------------------------------------------------------------
        float GetScaleSensitivity()
        {
            return scaleSensitivitySource == EGizmoDragSettingSource.UseGlobal ?
                RTGizmos.get.skin.globalGizmoStyle.scaleSensitivity : scaleSensitivity;
        }
        #endregion
    }
    #endregion
}