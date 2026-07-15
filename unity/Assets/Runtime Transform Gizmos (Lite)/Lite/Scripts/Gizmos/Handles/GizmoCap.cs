using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EGizmoFlatHoverMode (Public Enum)
    // Desc: Defines different hover modes for flat gizmo shapes such as circles
    //       and quads for example.
    //-----------------------------------------------------------------------------
    public enum EGizmoFlatHoverMode
    {
        Solid,      // Treat the shape as a solid and raycast against its whole area
        Wire,       // Treat the shape as a wireframe and raycast against its wireframe representation
        Extrude,    // Extrude the flat shape (quad > box, circle > cylinde etc)
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GizmoCap (Public Class)
    // Desc: Implements a gizmo cap. Caps can be used to cap sliders but can also be
    //       used as standalone entities.
    //-----------------------------------------------------------------------------
    public class GizmoCap : GizmoHandle
    {
        #region Private Fields
        GizmoCapStyle mCapStyle = new GizmoCapStyle();  // Default style
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: capStyle (Public Property)
        // Desc: Returns or sets the cap style.
        //-----------------------------------------------------------------------------
        public GizmoCapStyle              capStyle    { get { return mCapStyle; } set { if (value != null) mCapStyle = value; } }

        //-----------------------------------------------------------------------------
        // Name: handleStyle (Public Property)
        // Desc: Returns the current style assigned to the handle.
        //-----------------------------------------------------------------------------
        public override GizmoHandleStyle  handleStyle { get { return mCapStyle; } }

        //-----------------------------------------------------------------------------
        // Name: alignFlatToCamera (Public Property)
        // Desc: Returns whether or not flat caps such as circles, quads etc should always
        //       be aligned with the camera look vector. If this is true, the surface of
        //       such shapes will always face the camera. Otherwise, the cap's rotation
        //       is used to define their orientation.
        //-----------------------------------------------------------------------------
        public bool alignFlatToCamera   { get; set; } = true;

        //-----------------------------------------------------------------------------
        // Name: flatWireHoverMode (Public Property)
        // Desc: Returns or sets the flat wireframe hover mode. The flat wireframe hover
        //       mode controls how flat wireframe shapes like quads and circles are treated
        //       during hover detecting. Only affects caps which use one of the flat wire
        //       shapes like 'WireQuad' or 'WireCircle' for example.
        //-----------------------------------------------------------------------------
        public EGizmoFlatHoverMode flatWireHoverMode { get; set; } = EGizmoFlatHoverMode.Solid;
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: ToCone() (Public Function)
        // Desc: Creates a cone that describes the cap given the current style.
        // Parm: camera        - The camera that interacts with or renders the handle.
        //       inflateAmount - Used to inflate the cone.
        // Rtrn: Cone primitive that describes the cap given the current style.
        //-----------------------------------------------------------------------------
        public Cone ToCone(Camera camera, float inflateAmount)
        {
            // Create the cone
            var     cone    = new Cone();
            float   radius  = FVal(capStyle.coneRadius, camera);
            float   length  = FVal(capStyle.coneLength, camera);
            cone.Set(transform.position, radius, length, transform.up, transform.right);

            // Inflate and return
            cone.Inflate(inflateAmount * GetZoomScale(camera));
            return cone;
        }

        //-----------------------------------------------------------------------------
        // Name: ToOBox() (Public Function)
        // Desc: Creates an oriented box that describes the cap given the current style.
        // Parm: camera        - The camera that interacts with or renders the handle.
        //       inflateAmount - Used to inflate the box.
        // Rtrn: Oriented box primitive that describes the cap given the current style.
        //-----------------------------------------------------------------------------
        public OBox ToOBox(Camera camera, float inflateAmount)
        {
            // Calculate box size
            Vector3 boxSize = new Vector3(FVal(capStyle.boxWidth,   camera), 
                                          FVal(capStyle.boxHeight,  camera),
                                          FVal(capStyle.boxDepth,   camera));

            // Create the box
            var obox = new OBox()
            {
                center   = transform.position,
                size     = boxSize,
                rotation = transform.rotation
            };

            // Inflate and return
            obox.size += Vector3Ex.FromValue(inflateAmount * GetZoomScale(camera));
            return obox;
        }

        //-----------------------------------------------------------------------------
        // Name: ToSphere() (Public Function)
        // Desc: Creates a sphere that describes the cap given the current style.
        // Parm: camera        - The camera that interacts with or renders the handle.
        //       inflateAmount - Used to inflate the sphere.
        // Rtrn: Sphere primitive that describes the cap given the current style.
        //-----------------------------------------------------------------------------
        public Sphere ToSphere(Camera camera, float inflateAmount)
        {
            // Create the sphere and return it
            return new Sphere()
            {
                center = transform.position,
                radius = FVal(capStyle.sphereRadius, camera) + inflateAmount / 2.0f * GetZoomScale(camera)
            };
        }

        //-----------------------------------------------------------------------------
        // Name: ToCircle() (Public Function)
        // Desc: Creates a circle that describes the cap given the current style.
        // Parm: camera        - The camera that interacts with or renders the handle.
        //       inflateAmount - Used to inflate the circle.
        // Rtrn: Circle primitive that describes the cap given the current style.
        //-----------------------------------------------------------------------------
        public Circle ToCircle(Camera camera, float inflateAmount)
        {
            // Create the circle and return it
            var     circle = new Circle();
            float   radius = FVal(capStyle.circleRadius, camera);
            circle.Set( transform.position,
                        radius + inflateAmount / 2.0f * GetZoomScale(camera),
                        alignFlatToCamera ? camera.transform.right   : transform.right,
                        alignFlatToCamera ? camera.transform.forward : transform.forward);
            return circle;
        }

        //-----------------------------------------------------------------------------
        // Name: ExtrudeWireCircle() (Public Function)
        // Desc: Extrudes a wire circle cap given the current style and turns it into an
        //       inset cylinder.
        // Parm: camera        - The camera that interacts with or renders the handle.
        //       extrudeAmount - Extrude amount. Also represents the cylinder's thickness.
        // Rtrn: Inset cylinder primitive that describes the extruded circle cap.
        //-----------------------------------------------------------------------------
        public InsetCylinder ExtrudeWireCircle(Camera camera, float extrudeAmount)
        {
            // Create the circle
            var circle = ToCircle(camera, 0.0f);    // Don't inflate circle

            // Convert the circle to an inset cylinder and return it
            var insetCylinder   = new InsetCylinder();
            float zoomScale     = GetZoomScale(camera);
            float length        = extrudeAmount * zoomScale;
            float thickness     = extrudeAmount * zoomScale;
            insetCylinder.Set(circle.center - circle.normal * length / 2.0f,
                         circle.radius, length, circle.radiusAxis, circle.normal, thickness);

            return insetCylinder;
        }

        //-----------------------------------------------------------------------------
        // Name: ToInsetCircle() (Public Function)
        // Desc: Creates an inset circle that describes the cap given the current style.
        // Parm: camera        - The camera that interacts with or renders the handle.
        //       inflateAmount - Used to inflate the circle.
        // Rtrn: Inset circle primitive that describes the cap given the current style.
        //-----------------------------------------------------------------------------
        public InsetCircle ToInsetCircle(Camera camera, float inflateAmount)
        {
            // Create the inset circle and return it
            var     insetCircle = new InsetCircle();
            float   zoomScale   = GetZoomScale(camera);
            float   radius      = FVal(capStyle.circleRadius, camera);
            float   thickness   = FVal(capStyle.insetCircleThickness, camera);
            insetCircle.Set(transform.position, radius + inflateAmount / 2.0f * zoomScale,
                            alignFlatToCamera ? camera.transform.right : transform.right,
                            alignFlatToCamera ? camera.transform.forward : transform.forward,
                            thickness + inflateAmount * zoomScale);
            return insetCircle;
        }

        //-----------------------------------------------------------------------------
        // Name: ToInsetCylinder() (Public Function)
        // Desc: Creates an inset cylinder that describes the cap given the current style.
        // Parm: camera        - The camera that interacts with or renders the handle.
        //       inflateAmount - Used to inflate the cylinder.
        // Rtrn: Inset cylinder primitive that describes the cap given the current style.
        //-----------------------------------------------------------------------------
        public InsetCylinder ToInsetCylinder(Camera camera, float inflateAmount)
        {
            // Create the inset cylinder
            var insetCylinder = new InsetCylinder();
            float radius      = FVal(capStyle.cylinderRadius, camera);
            float length      = FVal(capStyle.cylinderLength, camera);
            float thickness   = FVal(capStyle.insetCylinderThickness, camera);
            insetCylinder.Set(  transform.position - transform.forward * length / 2.0f,
                                radius, length,
                                transform.up, transform.forward, thickness);

            // Inflate and return
            insetCylinder.Inflate(inflateAmount * GetZoomScale(camera));
            return insetCylinder;
        }

        //-----------------------------------------------------------------------------
        // Name: ToPyramid() (Public Function)
        // Desc: Creates a pyramid that describes the cap given the current style.
        // Parm: camera        - The camera that interacts with or renders the handle.
        //       inflateAmount - Used to inflate the pyramid.
        // Rtrn: Pyramid primitive that describes the cap given the current style.
        //-----------------------------------------------------------------------------
        public SquarePyramid ToPyramid(Camera camera, float inflateAmount)
        {
            // Create the pyramid
            var pyramid = new SquarePyramid();
            pyramid.Set(transform.position, FVal(capStyle.pyramidBaseSize, camera),
                        FVal(capStyle.pyramidLength, camera), transform.up, transform.right);

            // Inflate and return
            pyramid.Inflate(inflateAmount * GetZoomScale(camera));
            return pyramid;
        }

        //-----------------------------------------------------------------------------
        // Name: ToQuad() (Public Function)
        // Desc: Creates a quad that describes the cap given the current style.
        // Parm: camera        - The camera that interacts with or renders the handle.
        //       inflateAmount - Used to inflate the quad.
        // Rtrn: Quad primitive that describes the cap given the current style.
        //-----------------------------------------------------------------------------
        public Quad ToQuad(Camera camera, float inflateAmount)
        {
            // Create the quad
            var quad = new Quad();
            quad.Set(transform.position, 
                     FVal(capStyle.quadWidth, camera), 
                     FVal(capStyle.quadHeight, camera), 
                     alignFlatToCamera ? camera.transform.forward : transform.forward,
                     alignFlatToCamera ? camera.transform.right : transform.right);

            // Inflate and return
            quad.Inflate(inflateAmount * GetZoomScale(camera));
            return quad;
        }

        //-----------------------------------------------------------------------------
        // Name: ExtrudeWireQuad() (Public Function)
        // Desc: Extrudes a wire quad cap given the current style and turns it into an
        //       inset box.
        // Parm: camera        - The camera that interacts with or renders the handle.
        //       extrudeAmount - Extrude amount. Also represents the box's thickness.
        // Rtrn: Inset box primitive that describes the extruded quad cap.
        //-----------------------------------------------------------------------------
        public InsetOBox ExtrudeWireQuad(Camera camera, float extrudeAmount)
        {
            // Create the quad
            var quad            = ToQuad(camera, 0.0f);    // Don't inflate quad
            float zoomScale     = GetZoomScale(camera);

            // Convert the quad to a box and return it
            var insetBox         = new InsetOBox();
            insetBox.center      = quad.center;
            insetBox.size        = new Vector3(quad.width, quad.height, extrudeAmount * zoomScale);
            insetBox.rotation    = Quaternion.LookRotation(quad.normal, quad.heightAxis);
            insetBox.thickness   = extrudeAmount * zoomScale;
            return insetBox;
        }

        //-----------------------------------------------------------------------------
        // Name: ToTorus() (Public Function)
        // Desc: Creates a torus that describes the cap given the current style.
        // Parm: camera        - The camera that interacts with or renders the handle.
        //       inflateAmount - Used to inflate the torus.
        // Rtrn: Torus primitive that describes the cap given the current style.
        //-----------------------------------------------------------------------------
        public Torus ToTorus(Camera camera, float inflateAmount)
        {
            // Create the torus
            var torus = new Torus();
            torus.Set(transform.position, 
                    FVal(capStyle.torusRadius, camera),
                    FVal(capStyle.torusTubeRadius, camera), transform.right, transform.forward);

            // Inflate and return
            torus.Inflate(inflateAmount * GetZoomScale(camera));
            return torus;
        }

        //-----------------------------------------------------------------------------
        // Name: CapSlider (Public Function)
        // Desc: Caps the specified slider by updating the cap position so as to sit at
        //       the slider's end point.
        // Parm: slider - The slider which must be capped.
        //       camera - The camera that interacts with or renders the slider/cap pair.
        //-----------------------------------------------------------------------------
        public void CapSlider(GizmoLineSlider slider, Camera camera)
        {
            // Calculate position (common to all cap types)
            transform.position = slider.transform.position + slider.direction * slider.FVal(slider.sliderStyle.length + slider.lengthOffset, camera);

            // Some caps need to be offset slightly because they are centered around the model space origin
            if (capStyle.capType == EGizmoCapType.Box || 
                capStyle.capType == EGizmoCapType.WireBox)
            {
                transform.position += slider.direction * FVal(capStyle.boxWidth / 2.0f, camera);
            }
            else
            if (capStyle.capType == EGizmoCapType.Sphere)
            {
                transform.position += slider.direction * FVal(capStyle.sphereRadius, camera);
            }
            else
            if (capStyle.capType == EGizmoCapType.InsetCylinder)
            {
                transform.position += slider.direction * FVal(capStyle.cylinderRadius, camera);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: ApproximateSphereBorder() (Public Function)
        // Desc: Can be used with inset circles and wire circles only. It treats the
        //       circle as a sphere and then turns itself into a border for that sphere.
        //       Useful for example when implementing the rotate gizmo view cap that rotates
        //       around the camera view vector. The result of the function is an approximation.
        // Parm: sphereCenter   - Sphere center.
        //       camera         - The camera that interacts with or renders the handle.
        //-----------------------------------------------------------------------------
        public void ApproximateSphereBorder(Vector3 sphereCenter, Camera camera)
        {
            // Validate cap type
            if (capStyle.capType != EGizmoCapType.InsetCircle &&
                capStyle.capType != EGizmoCapType.WireCircle) return;

            // Calculate sphere in the same position and radius
            Sphere sphere = new Sphere(sphereCenter, FVal(capStyle.circleRadius, camera));

            // Make border circle
            Circle circle = new Circle();
            circle.ApproximateSphereBorder(sphere, camera);

            // Update transform
            transform.position = circle.center;
            transform.rotation = Quaternion.LookRotation(circle.normal, circle.upAxis);
        }    
        
        //-----------------------------------------------------------------------------
        // Name: CalculateAABB (Public Function)
        // Desc: Calculates and returns the handle's AABB.
        // Parm: camera - The camera that interacts with or renders the handle.
        // Rtrn: The handle's AABB.
        //-----------------------------------------------------------------------------
        public override Box CalculateAABB(Camera camera)
        {
            // Check cap type
            switch (capStyle.capType)
            {
                case EGizmoCapType.Cone:

                    return ToCone(camera, 0.0f).CalculateAABB();

                case EGizmoCapType.WireBox:
                case EGizmoCapType.Box:

                    return ToOBox(camera, 0.0f).CalculateAABB();

                case EGizmoCapType.WireQuad:

                    return ToQuad(camera, 0.0f).CalculateAABB();

                case EGizmoCapType.Quad:

                    return ToQuad(camera, 0.0f).CalculateAABB();

                case EGizmoCapType.Sphere:

                    return ToSphere(camera, 0.0f).CalculateAABB();

                case EGizmoCapType.WireCircle:
                case EGizmoCapType.Circle:

                    return ToCircle(camera, 0.0f).CalculateAABB();

                case EGizmoCapType.InsetCircle:

                    return ToInsetCircle(camera, 0.0f).CalculateAABB();

                case EGizmoCapType.InsetCylinder:

                    return ToInsetCylinder(camera, 0.0f).CalculateAABB();

                case EGizmoCapType.WirePyramid:
                case EGizmoCapType.Pyramid:

                    return ToPyramid(camera, 0.0f).CalculateAABB();

                case EGizmoCapType.Torus:

                    return ToTorus(camera, 0.0f).CalculateAABB();

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
            switch (capStyle.capType)
            {
                case EGizmoCapType.Cone:

                    return ToCone(args.camera, hoverPadding).Raycast(args.ray, out t);

                case EGizmoCapType.WireBox:
                case EGizmoCapType.Box:

                    return ToOBox(args.camera, hoverPadding).Raycast(args.ray, out t);

                case EGizmoCapType.WireQuad:

                    if (flatWireHoverMode == EGizmoFlatHoverMode.Wire) return ToQuad(args.camera, 0.0f).RaycastWire(args.ray, FVal(hoverPadding, args.camera), out t);
                    else if (flatWireHoverMode == EGizmoFlatHoverMode.Solid) return ToQuad(args.camera, hoverPadding).Raycast(args.ray, out t);
                    else return ExtrudeWireQuad(args.camera, hoverPadding).Raycast(args.ray, out t);

                case EGizmoCapType.Quad:

                    return ToQuad(args.camera, hoverPadding).Raycast(args.ray, out t);

                case EGizmoCapType.Sphere:

                    return ToSphere(args.camera, hoverPadding).Raycast(args.ray, out t);

                case EGizmoCapType.WireCircle:

                    if (flatWireHoverMode == EGizmoFlatHoverMode.Wire) return ToCircle(args.camera, 0.0f).RaycastWire(args.ray, FVal(hoverPadding, args.camera), out t);
                    else if (flatWireHoverMode == EGizmoFlatHoverMode.Solid) return ToCircle(args.camera, hoverPadding).Raycast(args.ray, out t);
                    else return ExtrudeWireCircle(args.camera, hoverPadding).Raycast(args.ray, false, out t);

                case EGizmoCapType.Circle:

                    return ToCircle(args.camera, hoverPadding).Raycast(args.ray, out t);

                case EGizmoCapType.InsetCircle:

                    return ToInsetCircle(args.camera, hoverPadding).Raycast(args.ray, out t);

                case EGizmoCapType.InsetCylinder:

                    return ToInsetCylinder(args.camera, hoverPadding).Raycast(args.ray, false, out t);

                case EGizmoCapType.WirePyramid:
                case EGizmoCapType.Pyramid:

                    return ToPyramid(args.camera, hoverPadding).Raycast(args.ray, out t);

                case EGizmoCapType.Torus:

                    return ToTorus(args.camera, hoverPadding).Raycast(args.ray, out t);
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
            RTGizmos.DrawCap(this, args);
        }
        #endregion
    }
    #endregion
}