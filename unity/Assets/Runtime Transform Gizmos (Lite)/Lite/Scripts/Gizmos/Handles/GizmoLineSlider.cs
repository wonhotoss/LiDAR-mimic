using UnityEngine;
using UnityEngine.Rendering;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GizmoLineSlider (Public Class)
    // Desc: Implements a gizmo line slider which can be used to drag along a single
    //       axis. In its default pose, the slider is aligned with the X axis.
    //-----------------------------------------------------------------------------
    public class GizmoLineSlider : GizmoHandle
    {
        #region Private Fields
        GizmoLineSliderStyle mSliderStyle = new GizmoLineSliderStyle();     // Default style
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: direction (Public Property)
        // Desc: Returns the slider's direction.
        //-----------------------------------------------------------------------------
        public Vector3  direction   { get { return transform.right; } }

        //-----------------------------------------------------------------------------
        // Name: trackedCap (Public Property)
        // Desc: Returns or sets the tracked slider cap. This does not affect the slider's 
        //       or cap's behaviour in any way. It is simply a way of tracking what cap
        //       belongs to what slider. This is useful in situations where we are dealing
        //       with lists of sliders which have to be sorted or reordered in some way.
        //       The reordering ruins the logical mapping between the sliders and the caps
        //       and this property can be used to track which cap belongs to which slider.
        //-----------------------------------------------------------------------------
        public GizmoCap     trackedCap      { get; set; } = null;

        //-----------------------------------------------------------------------------
        // Name: lengthOffset (Public Property)
        // Desc: Returns or sets the length offset. This is a value to add to the style's
        //       length property to make the slider shorter or larger. Useful for changing
        //       the length of the slider without affecting its style (e.g. scale gizmo
        //       sliders change their length when scaling).
        //-----------------------------------------------------------------------------
        public float        lengthOffset    { get; set; } = 0.0f;

        //-----------------------------------------------------------------------------
        // Name: sliderStyle (Public Property)
        // Desc: Returns or sets the slider style.
        //-----------------------------------------------------------------------------
        public GizmoLineSliderStyle       sliderStyle { get { return mSliderStyle; } set { if (value != null) mSliderStyle = value; } }

        //-----------------------------------------------------------------------------
        // Name: handleStyle (Public Property)
        // Desc: Returns the current style assigned to the handle.
        //-----------------------------------------------------------------------------
        public override GizmoHandleStyle  handleStyle { get { return mSliderStyle; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: SetDirection() (Public Function)
        // Desc: Sets the slider direction to the specified vector.
        // Parm: direction - The slider direction.
        //-----------------------------------------------------------------------------
        public void SetDirection(Vector3 direction)
        {
            transform.rotation = Quaternion.FromToRotation(this.direction, direction) * transform.rotation;
        }

        //-----------------------------------------------------------------------------
        // Name: IsFacingCamera() (Public Function)
        // Desc: Checks if the slider is facing the camera.
        // Parm: camera     - The query camera. This camera is either interacting with
        //                    the slider or is rendering it.
        //       unitLength - If true, the function will treat the slider as having a
        //                    length of 1. This is only important when dealing with perspective
        //                    cameras.
        // Rtrn: True if the slider is facing the camera (i.e. looking at the camera)
        //       and false otherwise.
        //-----------------------------------------------------------------------------
        public bool IsFacingCamera(Camera camera, bool unitLength = false)
        {
            // Are we an ortho or perspective camera?
            if (camera.orthographic)
                return Vector3.Dot(camera.transform.forward, direction) < 0.0f;
            else
            {
                // For a perspective camera, we need to check if it can see the slider's end point
                var segment = ToSegment(camera);
                if (unitLength) segment.Normalize();
                Vector3 ray = (segment.end - camera.transform.position).normalized;
                return Vector3.Dot(ray, direction) < 0.0f;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: ToSegment() (Public Function)
        // Desc: Creates a segment that describes the slider given the current style.
        // Parm: camera - The camera that interacts with or renders the handle.
        // Rtrn: Segment primitive that describes the slider given the current style.
        //-----------------------------------------------------------------------------
        public Segment ToSegment(Camera camera)
        {
            // If no cutoff is needed, we default to no position offset and the style length
            Vector3 offset      = Vector3.zero;
            float length        = MathEx.FastAbs(sliderStyle.length + lengthOffset);
            float finalLength   = length;

            // Do we need to cut off from the start?
            if (sliderStyle.startCutoff != 0.0f)
            {
                // The position is associated with the segment start, so we have to offset it by how much we've cut off
                offset          += direction * FVal(sliderStyle.startCutoff * length, camera);
                finalLength     -= sliderStyle.startCutoff * length;
            }

            // Do we need to cut off from the end?
            if (sliderStyle.endCutoff != 0.0f)
            {
                finalLength    -= sliderStyle.endCutoff * length;
                if (finalLength < 0.0f) finalLength = 0.0f;
            }

            // Return segment
            finalLength = FVal(finalLength, camera);
            float sign = Mathf.Sign(sliderStyle.length + lengthOffset);
            return new Segment { start = transform.position + offset * sign, end = transform.position + offset * sign + direction * finalLength * sign };
        }

        //-----------------------------------------------------------------------------
        // Name: ToThinLine() (Public Function)
        // Desc: Creates a cylinder that describes the slider as a thin line given the
        //       current style.
        // Parm: camera        - The camera that interacts with or renders the handle.
        //       inflateAmount - Used to inflate the cylinder.
        // Rtrn: Cylinder primitive that describes the slider as a thin line given the
        //       current style.
        //-----------------------------------------------------------------------------
        public Cylinder ToThinLine(Camera camera, float inflateAmount)
        {
            // Convert to segment initially to handle cutoff
            var segment = ToSegment(camera);

            // Create the cylinder
            var cylinder = new Cylinder();
            cylinder.Set(segment.start, 0.0f, segment.length, transform.up, direction);

            // Inflate and return
            cylinder.Inflate(inflateAmount * GetZoomScale(camera));
            return cylinder;
        }

        //-----------------------------------------------------------------------------
        // Name: ToOBox() (Public Function)
        // Desc: Creates an oriented box that describes the slider given the current style.
        // Parm: camera        - The camera that interacts with or renders the handle.
        //       inflateAmount - Used to inflate the box.
        // Rtrn: Oriented box primitive that describes the slider given the current style.
        //-----------------------------------------------------------------------------
        public OBox ToOBox(Camera camera, float inflateAmount)
        {
            // Convert to segment initially to handle cutoff
            var segment = ToSegment(camera);

            // Create the box
            float thickness = FVal(sliderStyle.thickness, camera);
            var obox = new OBox()
            {
                center   = segment.start + direction * segment.length * 0.5f,
                size     = new Vector3(segment.length, thickness, thickness),
                rotation = transform.rotation
            };

            // Inflate and return
            obox.size += Vector3Ex.FromValue(inflateAmount * GetZoomScale(camera));
            return obox;
        }

        //-----------------------------------------------------------------------------
        // Name: ToCylinder() (Public Function)
        // Desc: Creates a cylinder that describes the slider given the current style.
        // Parm: camera        - The camera that interacts with or renders the handle.
        //       inflateAmount - Used to inflate the cylinder.
        // Rtrn: Cylinder primitive that describes the slider given the current style.
        //-----------------------------------------------------------------------------
        public Cylinder ToCylinder(Camera camera, float inflateAmount)
        {
            // Convert to segment initially to handle cutoff
            var segment = ToSegment(camera);

            // Create the cylinder
            var cylinder = new Cylinder();
            cylinder.Set(segment.start, FVal(sliderStyle.thickness / 2.0f, camera),
                         segment.length, transform.up, direction);

            // Inflate and return
            cylinder.Inflate(inflateAmount * GetZoomScale(camera));
            return cylinder;
        }       

        //-----------------------------------------------------------------------------
        // Name: CalculateAABB (Public Function)
        // Desc: Calculates and returns the handle's AABB.
        // Parm: camera - The camera that interacts with or renders the handle.
        // Rtrn: The handle's AABB.
        //-----------------------------------------------------------------------------
        public override Box CalculateAABB(Camera camera)
        {
            // Check slider type
            switch (sliderStyle.sliderType)
            {
                case EGizmoLineSliderType.Thin:

                    return ToThinLine(camera, 0.0f).CalculateAABB();

                case EGizmoLineSliderType.WireBox:
                case EGizmoLineSliderType.Box:

                    return ToOBox(camera, 0.0f).CalculateAABB();

                case EGizmoLineSliderType.Cylinder:

                    return ToCylinder(camera, 0.0f).CalculateAABB();

                default: return Box.GetInvalid();
            }
        }
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: Raycast (Protected Function)
        // Desc: Performs a raycast check against the gizmo handle.
        // Parm: args   - Raycast arguments.
        //       t      - Returns the distance from the ray origin where the intersection
        //                happens.
        // Rtrn: True if the ray intersects the handle and false otherwise.
        //-----------------------------------------------------------------------------
        protected override bool Raycast(GizmoHoverRaycastArgs args, out float t)
        {
            // Clear output
            t = 0.0f;

            // Raycast based on the slider type
            float hoverPadding  = GetHoverPadding();
            switch (sliderStyle.sliderType)
            {
                case EGizmoLineSliderType.Thin:

                    return ToThinLine(args.camera, hoverPadding).Raycast(args.ray, false, out t);

                case EGizmoLineSliderType.WireBox:
                case EGizmoLineSliderType.Box:

                    return ToOBox(args.camera, hoverPadding).Raycast(args.ray, out t);

                case EGizmoLineSliderType.Cylinder:

                    return ToCylinder(args.camera, hoverPadding).Raycast(args.ray, false, out t);
            }

            // No hit
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: OnRender (Protected Function)
        // Desc: Called when the handle must render itself.
        // Parm: args - Render arguments.
        //-----------------------------------------------------------------------------
        protected override void OnRender(GizmoHandleRenderArgs args)
        {
            RTGizmos.DrawLineSlider(this, args);
        }
        #endregion
    }
    #endregion
}
