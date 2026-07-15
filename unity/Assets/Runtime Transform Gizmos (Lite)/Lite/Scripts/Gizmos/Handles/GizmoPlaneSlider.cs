using UnityEngine;
using UnityEngine.Rendering;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GizmoPlaneSlider (Public Class)
    // Desc: Implements a gizmo plane slider which can be used to drag along 2 axes
    //       at once. In its default pose, the slider is aligned with the XY plane.
    //-----------------------------------------------------------------------------
    public class GizmoPlaneSlider : GizmoHandle
    {
        #region Private Fields    
        GizmoPlaneSliderStyle mSliderStyle  = new GizmoPlaneSliderStyle();  // Default style
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: normal (Public Property)
        // Desc: Returns the slider plane's normal.
        //-----------------------------------------------------------------------------
        public Vector3 normal { get { return -transform.forward; } }

        //-----------------------------------------------------------------------------
        // Name: slideDirection0 (Public Property)
        // Desc: Returns the first slide direction. This is aligned with one of the edges
        //       of the plane and perpendicular to 'slideDirection1'. These 2 can be
        //       used to slide/drag entities along the slider plane.
        //-----------------------------------------------------------------------------
        public Vector3 slideDirection0 { get { return transform.right; } }

        //-----------------------------------------------------------------------------
        // Name: slideDirection1 (Public Property)
        // Desc: Returns the second slide direction. This is aligned with one of the edges
        //       of the plane and perpendicular to 'slideDirection0'. These 2 can be
        //       used to slide/drag entities along the slider plane.
        //-----------------------------------------------------------------------------
        public Vector3 slideDirection1 { get { return transform.up; } }

        //-----------------------------------------------------------------------------
        // Name: sliderStyle (Public Property)
        // Desc: Returns or sets the slider style.
        //-----------------------------------------------------------------------------
        public GizmoPlaneSliderStyle      sliderStyle { get { return mSliderStyle; } set { if (value != null) mSliderStyle = value; } }

        //-----------------------------------------------------------------------------
        // Name: handleStyle (Public Property)
        // Desc: Returns the current style assigned to the handle.
        //-----------------------------------------------------------------------------
        public override GizmoHandleStyle  handleStyle { get { return mSliderStyle; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: SetNormal() (Public Function)
        // Desc: Sets the slider normal.
        // Parm: normal - The slider normal.
        //-----------------------------------------------------------------------------
        public void SetNormal(Vector3 normal)
        {
            transform.rotation = Quaternion.FromToRotation(this.normal, normal) * transform.rotation;
        }

        //-----------------------------------------------------------------------------
        // Name: ToQuad() (Public Function)
        // Desc: Creates a quad that describes the slider given the current style.
        // Parm: camera        - The camera that interacts with or renders the handle.
        //       inflateAmount - Used to inflate the quad.
        // Rtrn: Quad primitive that describes the slider given the current style.
        //-----------------------------------------------------------------------------
        public Quad ToQuad(Camera camera, float inflateAmount)
        {
            // Create the quad
            var quad = new Quad();
            quad.Set(transform.position, 
                FVal(sliderStyle.quadWidth, camera), 
                FVal(sliderStyle.quadHeight, camera),
                normal, transform.right);

            // Inflate and return
            quad.Inflate(inflateAmount * GetZoomScale(camera));
            return quad;
        }

        //-----------------------------------------------------------------------------
        // Name: ToRATriangle() (Public Function)
        // Desc: Creates a right-angled triangle that describes the slider given the
        //       current style.
        // Parm: camera        - The camera that interacts with or renders the handle.
        //       inflateAmount - Used to inflate the quad.
        // Rtrn: Quad right-angled triangle primitive that describes the slider given
        //       the current style.
        //-----------------------------------------------------------------------------
        public RATriangle ToRATriangle(Camera camera, float inflateAmount)
        {
            // Create the triangle
            var triangle = new RATriangle();
            float edgeSize = FVal(sliderStyle.raTriangleSize, camera);
            triangle.Set(transform.position, transform.right, transform.up, edgeSize, edgeSize);

            // Inflate and return
            triangle.Inflate(inflateAmount * GetZoomScale(camera));
            return triangle;
        }

        //-----------------------------------------------------------------------------
        // Name: ToBorderInsetOBox() (Public Function)
        // Desc: Creates an oriented inset box that describes the slider border given
        //       the current style.
        // Parm: camera        - The camera that interacts with or renders the handle.
        //       inflateAmount - Used to inflate the box.
        // Rtrn: Inset oriented box primitive that describes the slider border given
        //       the current style.
        //-----------------------------------------------------------------------------
        public InsetOBox ToBorderInsetOBox(Camera camera, float inflateAmount)
        {
            // Calculate box size
            var quad        = ToQuad(camera, 0.0f);
            Vector3 boxSize = new Vector3(quad.width, FVal(sliderStyle.quadBorderHeight, camera), quad.height);

            // Create the box
            var insetBox = new InsetOBox()
            {
                center      = transform.position,
                size        = boxSize,
                rotation    = Quaternion.LookRotation(transform.up, -normal),
                thickness   = FVal(sliderStyle.quadBorderWidth, camera),
            };

            // Inflate and return.
            // Note: Don't inflate the height. Inflate only along the slider's quad plane.
            //       The thickness needs to be inflated too. Otherwise, the inset is moved
            //       from where it visually appears and it makes it look like as if the ray
            //       doesn't hit the box.
            float zoomScale = GetZoomScale(camera);
            insetBox.size       += new Vector3(inflateAmount * zoomScale, 0.0f, inflateAmount * zoomScale);
            insetBox.thickness  += inflateAmount * zoomScale;
            return insetBox;
        }

        //-----------------------------------------------------------------------------
        // Name: SitRightAngle() (Public Function)
        // Desc: Updates the position and rotation of the plane slider so that it sits
        //       in the right angle formed by the 2 line sliders which are assumed to
        //       be perpendicular and having the same position.
        // Parm: cornerPosition - The right-angled corner position.
        //       s0             - First line slider.
        //       s1             - Second line slider.
        //       cornerOffset   - How much offset to apply to the slider position to move
        //                        it away from the right angle corner.
        //       camera         - The camera that interacts with or renders the slider.
        //       bestVisibility - If true, the function will take into account the slider's
        //                        relationship to the camera and will calculate it position
        //                        such that it is as close as possible to the camera without
        //                        being obscured by the line sliders.
        //-----------------------------------------------------------------------------
        public void SitRightAngle(Vector3 cornerPosition, GizmoLineSlider s0, GizmoLineSlider s1, float cornerOffset, Camera camera, bool bestVisibility = true)
        {
            // Calculate slider directions
            Vector3 dir0 = s0.direction;
            Vector3 dir1 = s1.direction;

            // Update for best visibility?
            if (bestVisibility)
            {
                // Treat the sliders as plane normals and check if the camera position lies behind the planes
                Plane plane = new Plane(s0.direction, cornerPosition);
                if (plane.GetDistanceToPoint(camera.transform.position) < 0.0f) dir0 = -s0.direction;
                plane = new Plane(s1.direction, cornerPosition);
                if (plane.GetDistanceToPoint(camera.transform.position) < 0.0f) dir1 = -s1.direction;
            }

            // Set the plane slider rotation
            Vector3 forwardAxis = Vector3.Cross(dir0, dir1).normalized;
            transform.rotation  = Quaternion.LookRotation(forwardAxis, dir1);

            // Offset the position so that the plane slider sits right between the 2 sliders without intersecting them
            if (sliderStyle.sliderType == EGizmoPlaneSliderType.Quad)
            {
                Quad quad = ToQuad(camera, 0.0f);
                transform.position = cornerPosition + dir0 * quad.width / 2.0f;
                transform.position += dir1 * quad.height / 2.0f;
            }
            else transform.position = cornerPosition;

            // Offset if any of the sliders are thick
            if (s0.sliderStyle.sliderType != EGizmoLineSliderType.Thin)
                transform.position += dir1 * FVal(s0.sliderStyle.thickness, camera) * 0.5f;
            if (s1.sliderStyle.sliderType != EGizmoLineSliderType.Thin)
                transform.position += dir0 * FVal(s1.sliderStyle.thickness, camera) * 0.5f;

            // Apply corner offset
            if (cornerOffset != 0.0f)
                transform.position += (dir0 + dir1) * FVal(cornerOffset, camera);
        }

        //-----------------------------------------------------------------------------
        // Name: SitRightAngle() (Public Function)
        // Desc: Updates the position and rotation of the plane slider so that it sits
        //       in the right angle formed by the 2 axes which are assumed to be perpendicular.
        // Parm: cornerPosition - The right-angled corner position.
        //       axis0          - First axis direction.
        //       axis1          - Second axis direction.
        //       cornerOffset   - How much offset to apply to the slider position to move
        //                        it away from the right angle corner.
        //       camera         - The camera that interacts with or renders the slider.
        //       bestVisibility - If true, the function will take into account the slider's
        //                        relationship to the camera and will calculate it position
        //                        such that it is as close as possible to the camera without
        //                        being obscured by the line sliders.
        //-----------------------------------------------------------------------------
        public void SitRightAngle(Vector3 cornerPosition, Vector3 axis0, Vector3 axis1, float cornerOffset, Camera camera, bool bestVisibility = true)
        {
            // Update for best visibility?
            if (bestVisibility)
            {
                // Treat the sliders as plane normals and check if the camera position lies behind the planes
                Plane plane = new Plane(axis0, cornerPosition);
                if (plane.GetDistanceToPoint(camera.transform.position) < 0.0f) axis0 = -axis0;
                plane = new Plane(axis1, cornerPosition);
                if (plane.GetDistanceToPoint(camera.transform.position) < 0.0f) axis1 = -axis1;
            }

            // Set the plane slider rotation
            Vector3 forwardAxis = Vector3.Cross(axis0, axis1).normalized;
            transform.rotation  = Quaternion.LookRotation(forwardAxis, axis1);

            // Offset the position so that the plane slider sits right between the 2 axes (originating from the corner) without intersecting them
            if (sliderStyle.sliderType == EGizmoPlaneSliderType.Quad)
            {
                Quad quad = ToQuad(camera, 0.0f);
                transform.position = cornerPosition + axis0 * quad.width / 2.0f;
                transform.position += axis1 * quad.height / 2.0f;
            }
            else transform.position = cornerPosition;

            // Apply corner offset
            if (cornerOffset != 0.0f)
                transform.position += (axis0 + axis1) * FVal(cornerOffset, camera);
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
                case EGizmoPlaneSliderType.Quad:

                    return ToQuad(camera, 0.0f).CalculateAABB();

                case EGizmoPlaneSliderType.RATriangle:

                    return ToRATriangle(camera, 0.0f).CalculateAABB();

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

            // Check slider type
            float hoverPadding = GetHoverPadding();
            switch (sliderStyle.sliderType)
            {
                case EGizmoPlaneSliderType.Quad:

                    // If we're using a thin border, just raycast against the slider quad
                    if (sliderStyle.borderType == EGizmoPlaneSliderBorderType.Thin)
                    {
                        // Raycast quad
                        Quad quad = ToQuad(args.camera, hoverPadding);
                        if (quad.Raycast(args.ray, out t))
                            return true;
                    }
                    else
                    {
                        // Otherwise, we need to raycast against the quad and the thick border
                        float tQuad, tBorder;
                        Quad quad = ToQuad(args.camera, hoverPadding);
                        if (!quad.Raycast(args.ray, out tQuad))
                            tQuad = float.MaxValue;

                        if (!sliderStyle.planeVisible)
                            tQuad = float.MaxValue;

                        InsetOBox insetBox = ToBorderInsetOBox(args.camera, hoverPadding);
                        if (!insetBox.Raycast(args.ray, out tBorder))
                            tBorder = float.MaxValue;

                        // Pick smallest t
                        t = tQuad < tBorder ? tQuad : tBorder;

                        // Return result
                        return t != float.MaxValue;
                    }
                    break;

                case EGizmoPlaneSliderType.RATriangle:

                    RATriangle triangle = ToRATriangle(args.camera, hoverPadding);
                    return triangle.Raycast(args.ray, out t);
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
            RTGizmos.DrawPlaneSlider(this, args);
        }
        #endregion
    }
    #endregion
}
