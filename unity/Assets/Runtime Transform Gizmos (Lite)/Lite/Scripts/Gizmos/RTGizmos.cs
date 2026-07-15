using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EGizmoRenderShape (Public Enum)
    // Desc: Defines different gizmo render shapes.
    //-----------------------------------------------------------------------------
    public enum EGizmoRenderShape
    {
        InputGeometry   = 0,    // Nothing special, just use the input geometry
        InsetBox,               // Inset box
        InsetCircle,            // Inset circle
        InsetCylinder,          // Inset cylinder
        Torus,                  // Torus
        Arc,                    // Arc
        SphereBorder            // Sphere border
    }

    //-----------------------------------------------------------------------------
    // Name: EGizmoMoveSnapMode (Public Enum)
    // Desc: Defines different move snap modes.
    //-----------------------------------------------------------------------------
    public enum EGizmoMoveSnapMode
    {
        Absolute = 0,   // Absolute snapping to the world grid
        Relative        // Snap relative to the drag origin
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTGizmos (Public Singleton Class)
    // Desc: This is the gizmo engine which handles gizmo creation, management and
    //       rendering tasks.
    //-----------------------------------------------------------------------------
    public class RTGizmos : MonoSingleton<RTGizmos>
    {
        #region Public Delegates & Events
        //-----------------------------------------------------------------------------
        // Name: GizmoHoverEnabledHandler() (Public Delegate)
        // Desc: Handler for the event which is fired when the gizmo engine needs to
        //       know if gizmo hovering is enabled.
        // Parm: answer - Listeners can use this to answer 'yes' or 'no'. Gizmo can be
        //                hovered only when all listeners answer 'yes'.
        //-----------------------------------------------------------------------------
        public delegate void    GizmoHoverEnabledHandler (BinaryAnswer answer);
        public event            GizmoHoverEnabledHandler hoverEnabledQuery;

        //-----------------------------------------------------------------------------
        // Name: RenderGizmosHandler() (Public Delegate)
        // Desc: Handler for the event which is fired to allow the client code to draw
        //       custom gizmos/shapes etc.
        // Parm: camera     - The render camera.
        //       rasterCtx  - The 'RasterGraphContext' used for rendering.
        //-----------------------------------------------------------------------------
        public delegate void    RenderGizmosHandler(Camera camera, RasterGraphContext rasterCtx);
        public event            RenderGizmosHandler preRenderGizmos;            // Fired before gizmos are rendered
        public event            RenderGizmosHandler postRenderGizmos;           // Fired after all gizmos are rendered

        //-----------------------------------------------------------------------------
        // Name: OnGUIHandle() (Public Delegate)
        // Desc: Handler for the 'OnGUI' event.
        // Parm: camera         - The render camera.
        //       guiViewRect    - The camera viewport in GUI space coordinates where
        //                        the Y axis increases downwards.
        //-----------------------------------------------------------------------------
        public delegate void    OnGUIHandle(Camera camera, Rect guiViewRect);
        public event            OnGUIHandle onGUI;
        #endregion

        #region Private Static Fields
        static MaterialPropertyBlock    sMtrlPropertyBlock;                 // Used for rendering tasks
        static Material                 sMaterial;                          // The selected material used for drawing
        static GizmoRenderStates        sGRS = new GizmoRenderStates();     // Gizmo render states used for drawing
        static MaterialRenderStates     sDefaultMtrlRenderStates = 
            new MaterialRenderStates    // Default material render states used for rendering 
        {
            zWrite      = false,
            zTest       = CompareFunction.Always,
            cullMode    = CullMode.Back
        };
        #endregion

        #region Private Fields
        [SerializeField] RTGizmosSkin   mSkin;                              // The current gizmos skin
        [SerializeField] bool           mSortGizmos = true;                 // Should gizmos be sorted back-to-front when rendered?

        List<Gizmo>     mGizmos                 = new List<Gizmo>();        // Stores all created gizmos
        List<ViewGizmo> mViewGizmos             = new List<ViewGizmo>();    // Stores all view gizmos
        List<Camera>    mViewGizmoRenderCameras = new List<Camera>();       // List of all view gizmo render cameras. There is one such camera for each view gizmo.
        List<Gizmo>     mSortedGizmos           = new List<Gizmo>();        // If gizmo sorted is enabled, this list stores the gizmos in back-to-front sorted order
        Gizmo           mHoveredGizmo;                                      // The hovered gizmo
        Gizmo           mDraggedGizmo;                                      // The gizmo currently being dragged
        bool            mDragEndedThisFrame;                                // Did a gizmo drag end this frame?

        BinaryAnswer    mHoverEnabledAnswer     = new BinaryAnswer();       // Used with the hover enabled handler
        List<Camera>    mFrameRenderCameras     = new List<Camera>();       // Stores all cameras that rendered gizmos during the current frame
        ulong           mFrameIndex             = ulong.MaxValue;           // Current frame index
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: skin (Public Property)
        // Desc: Returns or sets the gizmos skin.
        //-----------------------------------------------------------------------------
        public RTGizmosSkin skin
        {
            get
            {   // Resolve skin
                if (mSkin == null)
                {
                    // Attempt to load the default skin. If we can't load, create a skin.
                    mSkin = RTGizmosSkinManager.get.defaultSkin;
                    if (mSkin == null) mSkin = ScriptableObject.CreateInstance<RTGizmosSkin>();
                }
                return mSkin;
            }
            set { if (value != null) mSkin = value; }
        }

        //-----------------------------------------------------------------------------
        // Name: sortGizmos (Public Property)
        // Desc: Returns or sets whether or not gizmos should be back-to-front sorted
        //       when rendered. Due to the fact that gizmos are rendered with the Z-Test
        //       disabled this can provide better results in some cases.
        //-----------------------------------------------------------------------------
        public bool                 sortGizmos      { get { return mSortGizmos; } set { mSortGizmos = value; } }

        //-----------------------------------------------------------------------------
        // Name: hoveredGizmo (Public Property)
        // Desc: Returns the hovered gizmo or null if no gizmo is hovered.
        //-----------------------------------------------------------------------------
        public Gizmo                hoveredGizmo    { get { return mHoveredGizmo; } }

        //-----------------------------------------------------------------------------
        // Name: hoveredHandle (Public Property)
        // Desc: Returns the hovered gizmo handle or null if no gizmo is hovered.
        //-----------------------------------------------------------------------------
        public GizmoHandle          hoveredHandle   { get { return mHoveredGizmo != null ? mHoveredGizmo.hoverData.handle : null; } }

        //-----------------------------------------------------------------------------
        // Name: hoverData (Public Property)
        // Desc: Returns the gizmo hover data. If no gizmo is hovered, the 'handle'
        //       field inside the 'GizmoHoverData' is null.
        //-----------------------------------------------------------------------------
        public GizmoHoverData       hoverData       { get { return mHoveredGizmo != null ? mHoveredGizmo.hoverData : new GizmoHoverData(); } }

        //-----------------------------------------------------------------------------
        // Name: draggedGizmo (Public Property)
        // Desc: Returns the gizmo which is currently being dragged or null if no gizmo
        //       is being dragged.
        //-----------------------------------------------------------------------------
        public Gizmo                draggedGizmo    { get { return mDraggedGizmo; } }

        //-----------------------------------------------------------------------------
        // Name: draggedHandle (Public Property)
        // Desc: Returns the dragged gizmo handle or null if no gizmo is being dragged.
        //-----------------------------------------------------------------------------
        public GizmoHandle          draggedHandle   { get { return mDraggedGizmo != null ? mDraggedGizmo.dragData.handle : null; } }

        //-----------------------------------------------------------------------------
        // Name: dragData (Public Property)
        // Desc: Returns the gizmo drag data. If no gizmo is being dragged, the 'handle'
        //       field inside the 'GizmoDragData' is null.
        //-----------------------------------------------------------------------------
        public GizmoDragData        dragData            { get { return mDraggedGizmo != null ? mDraggedGizmo.dragData : new GizmoDragData(); } }

        //-----------------------------------------------------------------------------
        // Name: dragEndedThisFrame (Public Property)
        // Desc: Returns whether a gizmo drag ended this frame.
        //-----------------------------------------------------------------------------
        public bool                 dragEndedThisFrame  { get { return mDragEndedThisFrame; } }

        //-----------------------------------------------------------------------------
        // Name: moveSnapMode (Public Property)
        // Desc: Returns or sets the move snap mode.
        //-----------------------------------------------------------------------------
        public EGizmoMoveSnapMode   moveSnapMode    { get; set; } = EGizmoMoveSnapMode.Relative;

        //-----------------------------------------------------------------------------
        // Name: snapEnabled (Public Property)
        // Desc: Returns or sets the snap enabled state. Set this to true to allow gizmos
        //       to snap. Set this to false to turn off snapping.
        //-----------------------------------------------------------------------------
        public bool     snapEnabled     { get; set; } = false;

        //-----------------------------------------------------------------------------
        // Name: gizmoCount (Public Property)
        // Desc: Returns the number of gizmos.
        //-----------------------------------------------------------------------------
        public int      gizmoCount      { get { return mGizmos.Count; } }

        //-----------------------------------------------------------------------------
        // Name: viewGizmoCount (Public Property)
        // Desc: Returns the number of view gizmos.
        //-----------------------------------------------------------------------------
        public int      viewGizmoCount  { get { return mViewGizmos.Count; } }

        //-----------------------------------------------------------------------------
        // Name: this[int index] (Public Property)
        // Desc: Indexer which returns the gizmo with the specified index.
        //-----------------------------------------------------------------------------
        public Gizmo    this[int index] { get { return mGizmos[index]; } }
        #endregion

        #region Public Static Properties
        //-----------------------------------------------------------------------------
        // Name: rasterCtx (Public Static Property)
        // Desc: Returns or sets the 'RasterGraphContext' used with 'Draw###' calls. This
        //       happens automatically when rendering gizmos.
        //-----------------------------------------------------------------------------
        public static RasterGraphContext    rasterCtx   { get; set; }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: CalculateZoomScale() (Public Static Function)
        // Desc: Calculates a zoom scale value for the specified position. A zoom scale
        //       is used to scale gizmo handles by the distance from camera in such a way
        //       as to maintain a constant screen size.
        // Parm: position - Reference position (e.g. handle position).
        //       camera   - The camera that renders the handle.
        // Rtrn: Zoom scale.
        //-----------------------------------------------------------------------------
        public static float CalculateZoomScale(Vector3 position, Camera camera)
        {
            // Note: The magic numbers were picked to produce decent sized gizmos with decent
            //       default values (e.g. default slider length etc).
            //       We also need to factor in the field of view for perspective cameras. This
            //       allows the gizmos to maintain a constant scale during projection switches.
            if (camera.orthographic) return (camera.orthographicSize * 2.0f) / (0.052f * camera.pixelHeight);
            else return (position - camera.transform.position).magnitude / (0.046f * camera.pixelHeight * 65.0f / camera.fieldOfView);
        }

        //-----------------------------------------------------------------------------
        // Name: DrawLineSlider() (Public Static Function)
        // Desc: Draws the specified line slider.
        // Parm: slider - The slider to draw.
        //       args   - Render arguments.
        //-----------------------------------------------------------------------------
        public static void DrawLineSlider(GizmoLineSlider slider, GizmoHandleRenderArgs args)
        {
            // Can render?
            if (!slider.canRender)
                return;

            // Describe the slider as a segment and calculate its scale
            var     segment     = slider.ToSegment(args.camera);
            Vector3 scale       = new Vector3(segment.length, 1.0f, 1.0f);

            // Establish details based on the slider type
            Mesh    mesh        = null;
            float   fval        = 0.0f;
            var     sliderStyle = slider.sliderStyle;
            switch (slider.sliderStyle.sliderType)
            {
                case EGizmoLineSliderType.Thin:

                    mesh    = MeshManager.unitXSegment;
                    break;

                case EGizmoLineSliderType.Box:

                    mesh    = MeshManager.unitBoxXSegment;
                    fval    = slider.FVal(sliderStyle.thickness, args.camera);
                    scale.y = fval;
                    scale.z = fval;
                    break;

                case EGizmoLineSliderType.Cylinder:

                    mesh    = MeshManager.unitCylinderXCap;
                    fval    = slider.FVal(sliderStyle.thickness / 2.0f, args.camera);  // Divide by 2 because a unit cylinder has a radius of 1.
                    scale.y = fval;    
                    scale.z = fval;
                    break;

                case EGizmoLineSliderType.WireBox:

                    mesh    = MeshManager.unitWireBoxXSegment;
                    fval    = slider.FVal(sliderStyle.thickness, args.camera);
                    scale.y = fval;
                    scale.z = fval;
                    break;
            }

            // Setup render states
            var globalStyle  = RTGizmos.get.skin.globalGizmoStyle;
            ExtractHandleRenderStates(slider, args, sGRS);
            sGRS.lit                    = globalStyle.shadeMode == EGizmoShadeMode.Lit;
            sGRS.color                  = slider.IsRenderHovered() ? RTGizmos.get.skin.globalGizmoStyle.colorHovered : sliderStyle.color;
            sGRS.renderShape            = EGizmoRenderShape.InputGeometry;
            sGRS.materialRenderStates   = sDefaultMtrlRenderStates;
            if (sliderStyle.sliderType == EGizmoLineSliderType.Thin || sliderStyle.sliderType.IsWire())
                sGRS.lit = false;

            // Bind render states
            BindRenderStates(sGRS);

            // Draw slider.
            // Note: If the slider had its direction inverted by the length offset (i.e. length is negative),
            //       we have to rotate 180 degrees so that it's facing in the other direction. Can't use scale
            //       because if the slider is a box or a cylinder (i.e. actual triangle geometry), rendering 
            //       is incorrect.
            Quaternion rotation = slider.transform.rotation.normalized; // Note: For some reason we need to normalize. Not sure why. Causes issues with the final Demo when switching between Single and Multi-Viewport modes.
            if ((sliderStyle.length + slider.lengthOffset) < 0.0f) rotation = (Quaternion.AngleAxis(180.0f, slider.transform.up) * rotation).normalized;
            args.rasterCtx.cmd.DrawMesh(mesh, Matrix4x4.TRS(segment.start, rotation, scale), sMaterial, 0, 0, sMtrlPropertyBlock);
        }

        //-----------------------------------------------------------------------------
        // Name: DrawPlaneSlider() (Public Static Function)
        // Desc: Draws the specified plane slider.
        // Parm: slider - The slider to draw.
        //       args   - Render arguments.
        //-----------------------------------------------------------------------------
        public static void DrawPlaneSlider(GizmoPlaneSlider slider, GizmoHandleRenderArgs args)
        {
            // Can render?
            if (!slider.canRender)
                return;

            // Cache data
            var     globalStyle     = RTGizmos.get.skin.globalGizmoStyle;
            bool    isRenderHovered = slider.IsRenderHovered();
            var     sliderStyle     = slider.sliderStyle;

            // Copy common render state data
            ExtractHandleRenderStates(slider, args, sGRS);

            // Is the slider plane visible?
            if (sliderStyle.planeVisible)
            {
                // Setup render states
                sGRS.lit            = false;
                sGRS.color          = sliderStyle.color.WithAlpha(globalStyle.planeSliderAlpha);
                if (isRenderHovered) sGRS.color = globalStyle.colorHovered.WithAlpha(globalStyle.planeSliderHoveredAlpha);
                sGRS.renderShape    = EGizmoRenderShape.InputGeometry;
                sGRS.materialRenderStates = new MaterialRenderStates
                {
                    zWrite      = false,
                    zTest       = CompareFunction.Always,
                    cullMode    = CullMode.Off
                };

                // Bind render states
                BindRenderStates(sGRS);

                // Check slider type and draw
                if (sliderStyle.sliderType == EGizmoPlaneSliderType.Quad)
                {
                    // Quad
                    args.rasterCtx.cmd.DrawMesh(MeshManager.unitXYQuad, 
                        slider.ToQuad(args.camera, 0.0f).xyTransform, sMaterial, 0, 0, sMtrlPropertyBlock);
                }
                else
                {
                    // Right-angled triangle
                    args.rasterCtx.cmd.DrawMesh(MeshManager.unitXYRATriangle, 
                        slider.ToRATriangle(args.camera, 0.0f).xyTransform, sMaterial, 0, 0, sMtrlPropertyBlock);
                }
            }

            // Is the border visible?
            sGRS.materialRenderStates   = sDefaultMtrlRenderStates;
            if (sliderStyle.borderVisible)
            {
                // Check border type
                if (sliderStyle.borderType == EGizmoPlaneSliderBorderType.Thin ||
                    sliderStyle.sliderType == EGizmoPlaneSliderType.RATriangle)
                {
                    // Setup render states
                    sGRS.lit                    = false;
                    sGRS.color                  = isRenderHovered ? globalStyle.colorHovered : sliderStyle.borderColor;
                    sGRS.renderShape            = EGizmoRenderShape.InputGeometry;

                    // Bind render states
                    BindRenderStates(sGRS);

                    // Check slider type
                    if (sliderStyle.sliderType == EGizmoPlaneSliderType.Quad)
                    {
                        // Wire quad                     
                        args.rasterCtx.cmd.DrawMesh(MeshManager.unitWireXYQuad, 
                            slider.ToQuad(args.camera, 0.0f).xyTransform, sMaterial, 0, 0, sMtrlPropertyBlock);
                    }
                    else
                    {
                        // Wire right-angled triangle
                        args.rasterCtx.cmd.DrawMesh(MeshManager.unitWireXYRATriangle, 
                            slider.ToRATriangle(args.camera, 0.0f).xyTransform, sMaterial, 0, 0, sMtrlPropertyBlock);
                    }
                }
                else         
                {
                    // Calculate border inset box
                    var borderBox = slider.ToBorderInsetOBox(args.camera, 0.0f);
                    var boxSize   = borderBox.size;

                    // Thick or wire border?
                    if (sliderStyle.borderType == EGizmoPlaneSliderBorderType.Thick)
                    {
                        // Setup render states
                        sGRS.color              = isRenderHovered ? globalStyle.colorHovered : sliderStyle.borderColor;
                        sGRS.lit                = globalStyle.shadeMode == EGizmoShadeMode.Lit;
                        sGRS.renderShape        = EGizmoRenderShape.InsetBox;
                        sGRS.renderShapeArgs    = 
                            new Vector4(boxSize.x, boxSize.y, boxSize.z, slider.FVal(sliderStyle.quadBorderWidth, args.camera));                                       
                    
                        // Bind render states
                        BindRenderStates(sGRS);

                        // Inset box                
                        args.rasterCtx.cmd.DrawMesh(MeshManager.unitBox, borderBox.transform, sMaterial, 0, 0, sMtrlPropertyBlock);
                    }
                    else
                    {
                        // Setup render states
                        sGRS.color          = isRenderHovered ? globalStyle.colorHovered : sliderStyle.borderColor;
                        sGRS.lit            = false;
                        sGRS.renderShape    = EGizmoRenderShape.InputGeometry;

                        // Bind render states
                        BindRenderStates(sGRS);

                        // Large box
                        Quaternion rotation = Quaternion.LookRotation(slider.transform.up, -slider.normal);
                        args.rasterCtx.cmd.DrawMesh(MeshManager.unitWireBox, 
                            borderBox.transform, sMaterial, 0, 0, sMtrlPropertyBlock);

                        // Draw smaller box inside the larger box to create the inset
                        float insetX = borderBox.size.x - slider.FVal(sliderStyle.quadBorderWidth, args.camera);
                        float insetZ = borderBox.size.z - slider.FVal(sliderStyle.quadBorderWidth, args.camera);
                        borderBox.size = new Vector3(insetX, borderBox.size.y, insetZ);
                        args.rasterCtx.cmd.DrawMesh(MeshManager.unitWireBox, 
                            borderBox.transform, sMaterial, 0, 0, sMtrlPropertyBlock);
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------
        // Name: DrawCap() (Public Static Function)
        // Desc: Draws the specified cap.
        // Parm: cap    - The cap to draw.
        //       args   - Render arguments.
        //-----------------------------------------------------------------------------
        public static void DrawCap(GizmoCap cap, GizmoHandleRenderArgs args)
        {
            // Can render?
            if (!cap.canRender)
                return;

            // Establish draw data based on the cap type
            Matrix4x4       mtx             = Matrix4x4.identity;
            Mesh            mesh            = null;
            InsetCircle     insetCircle     = new InsetCircle();
            InsetCylinder   insetCylinder   = new InsetCylinder();
            Torus           torus           = new Torus();
            var             capStyle        = cap.capStyle;
            switch (capStyle.capType)
            {
                case EGizmoCapType.Box:

                    mtx  = cap.ToOBox(args.camera, 0.0f).transform;
                    mesh = MeshManager.unitBox;
                    break;

                case EGizmoCapType.Quad:

                    mtx  = cap.ToQuad(args.camera, 0.0f).xyTransform;
                    mesh = MeshManager.unitXYQuad;
                    break;

                case EGizmoCapType.Cone:

                    mtx  = cap.ToCone(args.camera, 0.0f).xCapTransform;
                    mesh = MeshManager.unitConeXCap;
                    break;

                case EGizmoCapType.Sphere:

                    mtx  = cap.ToSphere(args.camera, 0.0f).transform;
                    mesh = MeshManager.unitSphere;
                    break;

                case EGizmoCapType.Circle:

                    mtx  = cap.ToCircle(args.camera, 0.0f).xyTransform;
                    mesh = MeshManager.unitXYCircle;
                    break;

                case EGizmoCapType.InsetCircle:

                    insetCircle = cap.ToInsetCircle(args.camera, 0.0f);
                    mtx         = insetCircle.xyTransform;
                    mesh        = MeshManager.unitXYCircle;
                    break;

                case EGizmoCapType.InsetCylinder:

                    insetCylinder   = cap.ToInsetCylinder(args.camera, 0.0f);
                    mtx             = insetCylinder.GetZTransform(cap.transform.up);
                    mesh            = MeshManager.unitZCylinder;
                    break;

                case EGizmoCapType.Pyramid:

                    mtx  = cap.ToPyramid(args.camera, 0.0f).xCapTransform;
                    mesh = MeshManager.unitSPyramidXCap;
                    break;

                case EGizmoCapType.WireBox:

                    mtx  = cap.ToOBox(args.camera, 0.0f).transform;
                    mesh = MeshManager.unitWireBox;
                    break;

                case EGizmoCapType.WirePyramid:

                    mtx  = cap.ToPyramid(args.camera, 0.0f).xCapTransform;
                    mesh = MeshManager.unitWireSPyramidXCap;
                    break;

                case EGizmoCapType.WireQuad:

                    mtx  = cap.ToQuad(args.camera, 0.0f).xyTransform;
                    mesh = MeshManager.unitWireXYQuad;
                    break;

                case EGizmoCapType.WireCircle:

                    mtx  = cap.ToCircle(args.camera, 0.0f).xyTransform;
                    mesh = MeshManager.unitWireXYCircle;
                    break;

                case EGizmoCapType.Torus:

                    torus   = cap.ToTorus(args.camera, 0.0f);
                    mtx     = Matrix4x4.TRS(cap.transform.position, Quaternion.LookRotation(torus.mainAxis, torus.radiusAxis), Vector3.one);    // Note: Scale is one. Radius is handled inside the shader.
                    mesh    = MeshManager.unitZTorus;
                    break;
            }

            // Setup render states
            var globalStyle = RTGizmos.get.skin.globalGizmoStyle;
            sGRS.materialRenderStates = sDefaultMtrlRenderStates;
            ExtractHandleRenderStates(cap, args, sGRS);
            sGRS.lit = globalStyle.shadeMode == EGizmoShadeMode.Lit;
            if (capStyle.capType.IsFlat() || capStyle.capType.IsWire())
            {
                sGRS.lit = false;
                sGRS.materialRenderStates = new MaterialRenderStates
                {
                    zWrite      = false,
                    zTest       = CompareFunction.Always,
                    cullMode    = CullMode.Off
                };
            }

            sGRS.color      = capStyle.color;
            if (cap.IsRenderHovered())
                sGRS.color  = capStyle.useGlobalHoveredColor ? RTGizmos.get.skin.globalGizmoStyle.colorHovered : capStyle.colorHovered;

            // Setup additional states based on the cap type
            if (capStyle.capType == EGizmoCapType.InsetCircle)
            {
                sGRS.renderShape        = EGizmoRenderShape.InsetCircle;
                sGRS.renderShapeArgs    = new Vector4(insetCircle.radius, insetCircle.thickness, 0.0f, 0.0f);
            }
            else
            if (capStyle.capType == EGizmoCapType.InsetCylinder)
            {
                sGRS.renderShape        = EGizmoRenderShape.InsetCylinder;
                sGRS.renderShapeArgs    = new Vector4(insetCylinder.radius, insetCylinder.length, insetCylinder.thickness, 0.0f);
            }
            else if (capStyle.capType == EGizmoCapType.Torus)
            {
                sGRS.renderShape        = EGizmoRenderShape.Torus;
                sGRS.renderShapeArgs    = new Vector4(torus.radius, torus.tubeRadius, 0.0f, 0.0f);
            }
            else sGRS.renderShape = EGizmoRenderShape.InputGeometry;

            // Bind render states
            BindRenderStates(sGRS);

            // Draw
            args.rasterCtx.cmd.DrawMesh(mesh, mtx, sMaterial, 0, 0, sMtrlPropertyBlock);

            // If we are a sphere, we may need to draw a border
            if (capStyle.capType == EGizmoCapType.Sphere && capStyle.sphereBorderVisible)
            {
                // Calculate sphere
                Sphere sphere   = cap.ToSphere(args.camera, 0.0f);
                Vector3 center  = sphere.center;

                // Setup render states
                sGRS.color              = capStyle.sphereBorderColor;
                sGRS.lit                = false;
                sGRS.renderShape        = EGizmoRenderShape.SphereBorder;
                sGRS.renderShapeArgs    = new Vector4(center.x, center.y, center.z, sphere.radius);

                // Bind render states
                BindRenderStates(sGRS);

                // Draw
                args.rasterCtx.cmd.DrawMesh(MeshManager.unitWireXYCircle,
                    Matrix4x4.TRS(sphere.center, 
                    Quaternion.LookRotation(args.camera.transform.forward, args.camera.transform.up), new Vector3(sphere.radius, sphere.radius, 1.0f)), 
                    sMaterial, 0, 0, sMtrlPropertyBlock);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: DrawArc() (Public Static Function)
        // Desc: Draws the specified arc.
        // Parm: arc    - The arc to draw.
        //       args   - Render arguments.
        //-----------------------------------------------------------------------------
        public static void DrawArc(GizmoArc arc, GizmoHandleRenderArgs args)
        {
            // Can we render?
            if (!arc.canRender)
                return;

            // Cache data
            Vector3 arcNormal   = arc.arcNormal;
            Matrix4x4 mtx       = Matrix4x4.TRS(arc.transform.position, arc.transform.rotation, new Vector3(arc.arcRadius, arc.arcRadius, 1.0f));

            // Setup render states
            ExtractHandleRenderStates(arc, args, sGRS);
            sGRS.lit = false;
            sGRS.materialRenderStates   = new MaterialRenderStates()
            {
                zWrite      = false,
                zTest       = CompareFunction.Always,
                cullMode    = CullMode.Off
            };

            //-----------------------------------------------------------------------------
            // Full Circles
            //-----------------------------------------------------------------------------
            sGRS.color          = arc.arcStyle.color;
            sGRS.renderShape    = EGizmoRenderShape.InputGeometry;
            BindRenderStates(sGRS);
            int circleCount = (int)(MathEx.FastAbs(arc.arcAngle) / 360.0f);
            for (int i = 0; i < circleCount; ++i)
                args.rasterCtx.cmd.DrawMesh(MeshManager.unitXYCircle, mtx, sMaterial, 0, 0, sMtrlPropertyBlock);

            //-----------------------------------------------------------------------------
            // Arc
            //-----------------------------------------------------------------------------
            Vector3 arcStart        = arc.arcStart;
            sGRS.renderShape        = EGizmoRenderShape.Arc;
            sGRS.renderShapeArgs    = new Vector4(arcStart.x, arcStart.y, arcStart.z, arc.arcAngle);
            sGRS.arcNormal          = arcNormal;   
            BindRenderStates(sGRS);
            args.rasterCtx.cmd.DrawMesh(MeshManager.unitXYCircle, mtx, sMaterial, 0, 0, sMtrlPropertyBlock);

            //-----------------------------------------------------------------------------
            // Border
            //-----------------------------------------------------------------------------
            // Calculate arc end point by rotating the start point           
            Quaternion rotation = Quaternion.AngleAxis(arc.arcAngle, arcNormal);
            Vector3 v           = rotation * (arc.arcStart - arc.transform.position);
            Vector3 arcEnd      = arc.transform.position + v;

            // Draw the border
            sGRS.renderShape    = EGizmoRenderShape.InputGeometry;
            sGRS.color          = arc.arcStyle.borderColor;
            BindRenderStates(sGRS);
            args.rasterCtx.cmd.DrawMesh(MeshManager.unitXSegment, 
                Matrix4x4.TRS(arc.transform.position, 
                Quaternion.LookRotation(Vector3.Cross(arc.arcStart - arc.transform.position, arcNormal).normalized, arcNormal), 
                new Vector3(arc.arcRadius, 1.0f, 1.0f)), sMaterial, 0, 0, sMtrlPropertyBlock);
            args.rasterCtx.cmd.DrawMesh(MeshManager.unitXSegment, 
                Matrix4x4.TRS(arc.transform.position, 
                Quaternion.LookRotation(Vector3.Cross(arcEnd - arc.transform.position, arcNormal).normalized, arcNormal), 
                new Vector3(arc.arcRadius, 1.0f, 1.0f)), sMaterial, 0, 0, sMtrlPropertyBlock);
        }

        //-----------------------------------------------------------------------------
        // Name: DrawSegment() (Public Static Function)
        // Desc: Draws a segment from 'start' to 'end' with the specified color.
        // Parm: start - Segment start.
        //       end   - Segment end.
        //       color - Segment color.
        //-----------------------------------------------------------------------------
        public static void DrawSegment(Vector3 start, Vector3 end, Color color)
        {
            // Cache data
            Vector3 segmentDir = end - start;

            // Draw
            sMtrlPropertyBlock.Clear();
            sMtrlPropertyBlock.SetColor("_Color", color);
            Matrix4x4 mtx = Matrix4x4.TRS(start, QuaternionEx.LookRotationEx(segmentDir, Vector3.up), new Vector3(1.0f, 1.0f, segmentDir.magnitude));
            rasterCtx.cmd.DrawMesh(MeshManager.unitZSegment, mtx, MaterialManager.rtUnlit, 0, 0, sMtrlPropertyBlock);
        }

        //-----------------------------------------------------------------------------
        // Name: DrawMesh() (Public Static Function)
        // Desc: Draws a mesh with the specified transform and color.
        // Parm: mesh           - The mesh to draw.
        //       transformMtx   - Transform matrix.
        //       color          - Mesh color.
        //-----------------------------------------------------------------------------
        public static void DrawMesh(Mesh mesh, Matrix4x4 transformMtx, Color color)
        {
            // Draw
            sMtrlPropertyBlock.Clear();
            sMtrlPropertyBlock.SetColor("_Color", color);
            rasterCtx.cmd.DrawMesh(mesh, transformMtx, MaterialManager.rtUnlit, 0, 0, sMtrlPropertyBlock);
        }

        //-----------------------------------------------------------------------------
        // Name: DrawWireCapsule() (Public Static Function)
        // Desc: Draws a wire capsule with the specified parameters.
        // Parm: center     - Capsule world center.
        //       radius     - Capsule world radius.
        //       height     - Capsule world height.
        //       heightAxis - Index of the height axis: (0 = X, 1 = Y, 2 = Z). If an invalid
        //                    index is passed, the Y axis is used by default.
        //       color      - Capsule color.
        //-----------------------------------------------------------------------------
        public static void DrawWireCapsule(Vector3 center, float radius, float height, Quaternion rotation, int heightAxis, Color color)
        {
            // Clip radius and height
            if (radius < 0.0f) radius = 0.0f;
            if (height < 2.0f * radius) height = 2.0f * radius;

            // We have to create a rotation quaternion that aligns the capsule height axis in model space
            Quaternion r = Quaternion.identity;     // Default to 'heightAxis' = 1
            if (heightAxis == 0) r = Quaternion.AngleAxis(-90.0f, Vector3.forward);
            else if (heightAxis == 2) r = Quaternion.AngleAxis(90.0f, Vector3.right);

            // Concatenate rotations and calculate local axes
            r               = rotation * r;
            Vector3 right   = r * Vector3.right;
            Vector3 up      = r * Vector3.up;
            Vector3 forward = r * Vector3.forward;

            // Calculate segment height
            float segmentHeight = height - 2.0f * radius;

            // Setup render states
            sGRS.UseDefaults();
            sGRS.color      = color;
            sGRS.lit        = false;
            
            // Binds render states and draw
            BindRenderStates(sGRS);
            Vector3  scale      = new Vector3(1.0f, segmentHeight, 1.0f);
            rasterCtx.cmd.DrawMesh(MeshManager.unitYSegment, 
                Matrix4x4.TRS(center - up * segmentHeight / 2.0f - right * radius, r, scale),
                sMaterial, 0, 0, sMtrlPropertyBlock);     // Left segment
            rasterCtx.cmd.DrawMesh(MeshManager.unitYSegment, 
                Matrix4x4.TRS(center - up * segmentHeight / 2.0f + right * radius, r, scale),
                sMaterial, 0, 0, sMtrlPropertyBlock);     // Right segment
            rasterCtx.cmd.DrawMesh(MeshManager.unitYSegment, 
                Matrix4x4.TRS(center - up * segmentHeight / 2.0f + forward * radius, r, scale),
                sMaterial, 0, 0, sMtrlPropertyBlock);     // Back segment
            rasterCtx.cmd.DrawMesh(MeshManager.unitYSegment, 
                Matrix4x4.TRS(center - up * segmentHeight / 2.0f - forward * radius, r, scale),
                sMaterial, 0, 0, sMtrlPropertyBlock);     // Front segment

            // Draw circle caps
            scale = new Vector3(radius, 1.0f, radius);
            rasterCtx.cmd.DrawMesh(MeshManager.unitWireZXCircle, 
                Matrix4x4.TRS(center - up * segmentHeight / 2.0f, r, scale),
                sMaterial, 0, 0, sMtrlPropertyBlock);     // Bottom circle
            rasterCtx.cmd.DrawMesh(MeshManager.unitWireZXCircle, 
                Matrix4x4.TRS(center + up * segmentHeight / 2.0f, r, scale),
                sMaterial, 0, 0, sMtrlPropertyBlock);     // Top circle

            // Draw semicircle caps aligned with the capsule's XY plane
            Plane bottomClipPlane   = new Plane(-up, center - up * segmentHeight / 2.0f);
            Plane topClipPlane      = new Plane( up, center + up * segmentHeight / 2.0f);
            sGRS.SetClipPlaneEnabled(0, true);
            sGRS.SetClipPlane(0, bottomClipPlane);
            BindRenderStates(sGRS); 
            scale = new Vector3(radius, radius, 1.0f);
            rasterCtx.cmd.DrawMesh(MeshManager.unitWireXYCircle, 
                Matrix4x4.TRS(center - up * segmentHeight / 2.0f, r, scale),
                sMaterial, 0, 0, sMtrlPropertyBlock);     // Bottom semicircle
            sGRS.SetClipPlane(0, topClipPlane);
            BindRenderStates(sGRS); 
            rasterCtx.cmd.DrawMesh(MeshManager.unitWireXYCircle, 
                Matrix4x4.TRS(center + up * segmentHeight / 2.0f, r, scale),
                sMaterial, 0, 0, sMtrlPropertyBlock);     // Top semicircle

            // Draw semicircle caps aligned with the capsule's YZ plane
            scale = new Vector3(1.0f, radius, radius);
            sGRS.SetClipPlane(0, bottomClipPlane);
            BindRenderStates(sGRS);
            rasterCtx.cmd.DrawMesh(MeshManager.unitWireYZCircle, 
                Matrix4x4.TRS(center - up * segmentHeight / 2.0f, r, scale),
                sMaterial, 0, 0, sMtrlPropertyBlock);     // Bottom semicircle
            sGRS.SetClipPlane(0, topClipPlane);
            BindRenderStates(sGRS);
            rasterCtx.cmd.DrawMesh(MeshManager.unitWireYZCircle, 
                Matrix4x4.TRS(center + up * segmentHeight / 2.0f, r, scale),
                sMaterial, 0, 0, sMtrlPropertyBlock);     // Top semicircle
        }

        //-----------------------------------------------------------------------------
        // Name: DrawWireCapsule2D() (Public Static Function)
        // Desc: Draws a 2D wire capsule with the specified parameters.
        // Parm: center     - Capsule world center.
        //       radius     - Capsule world radius.
        //       height     - Capsule world height.
        //       heightAxis - Index of the height axis: (0 = X, 1 = Y, 2 = Z). If an invalid
        //                    index is passed, the Y axis is used by default.
        //       color      - Capsule color.
        //-----------------------------------------------------------------------------
        public static void DrawWireCapsule2D(Vector3 center, float radius, float height, Quaternion rotation, int heightAxis, Color color)
        {
            // Clip radius and height
            if (radius < 0.0f) radius = 0.0f;
            if (height < 2.0f * radius) height = 2.0f * radius;

            // We have to create a rotation quaternion that aligns the capsule height axis in model space
            Quaternion r = Quaternion.identity;     // Default to 'heightAxis' = 1
            if (heightAxis == 0) r = Quaternion.AngleAxis(-90.0f, Vector3.forward);
            else if (heightAxis == 2) r = Quaternion.AngleAxis(90.0f, Vector3.right);

            // Concatenate rotations and calculate local axes
            r               = rotation * r;
            Vector3 right   = r * Vector3.right;
            Vector3 up      = r * Vector3.up;
            Vector3 forward = r * Vector3.forward;

            // Calculate segment height
            float segmentHeight = height - 2.0f * radius;

            // Setup render states
            sGRS.UseDefaults();
            sGRS.color      = color;
            sGRS.lit        = false;
            
            // Binds render states and draw
            BindRenderStates(sGRS);
            Vector3  scale      = new Vector3(1.0f, segmentHeight, 1.0f);
            rasterCtx.cmd.DrawMesh(MeshManager.unitYSegment, 
                Matrix4x4.TRS(center - up * segmentHeight / 2.0f - right * radius, r, scale),
                sMaterial, 0, 0, sMtrlPropertyBlock);     // Left segment
            rasterCtx.cmd.DrawMesh(MeshManager.unitYSegment, 
                Matrix4x4.TRS(center - up * segmentHeight / 2.0f + right * radius, r, scale),
                sMaterial, 0, 0, sMtrlPropertyBlock);     // Right segment

            // Draw semicircle caps aligned with the capsule's XY plane
            Plane bottomClipPlane   = new Plane(-up, center - up * segmentHeight / 2.0f);
            Plane topClipPlane      = new Plane( up, center + up * segmentHeight / 2.0f);
            sGRS.SetClipPlaneEnabled(0, true);
            sGRS.SetClipPlane(0, bottomClipPlane);
            BindRenderStates(sGRS); 
            scale = new Vector3(radius, radius, 1.0f);
            rasterCtx.cmd.DrawMesh(MeshManager.unitWireXYCircle, 
                Matrix4x4.TRS(center - up * segmentHeight / 2.0f, r, scale),
                sMaterial, 0, 0, sMtrlPropertyBlock);     // Bottom semicircle
            sGRS.SetClipPlane(0, topClipPlane);
            BindRenderStates(sGRS); 
            rasterCtx.cmd.DrawMesh(MeshManager.unitWireXYCircle, 
                Matrix4x4.TRS(center + up * segmentHeight / 2.0f, r, scale),
                sMaterial, 0, 0, sMtrlPropertyBlock);     // Top semicircle
        }
        #endregion

        #region Public Functions        
        //-----------------------------------------------------------------------------
        // Name: Internal_CreateViewGizmoRenderCamera (Public Function)
        // Desc: Called by the system to create a view gizmo render camera.
        // Rtrn: A camera that can be used to render a view gizmo. 
        //-----------------------------------------------------------------------------
        public Camera Internal_CreateViewGizmoRenderCamera()
        {
            // Create the camera object and store it
            GameObject camObject        = new GameObject("RTViewGizmoRenderCamera");
            camObject.transform.parent  = RTGizmos.get.gameObject.transform;
            Camera camera               = camObject.AddComponent<Camera>();
            mViewGizmoRenderCameras.Add(camera);

            // Setup camera properties
            camera.cullingMask      = 0;
            camera.clearFlags       = CameraClearFlags.Depth;   // Standard pipeline only
            camera.renderingPath    = RenderingPath.Forward;
            camera.fieldOfView      = 60.0f;
            camera.orthographicSize = 5.0f;
            camera.allowHDR         = false;                                    // Probably needs to be off
            camera.backgroundColor  = camera.backgroundColor.WithAlpha(0.0f);   // Use a transparent clear color
            camera.nearClipPlane    = 0.01f;

            // Setup URP camera data
            var camData = camera.GetUniversalAdditionalCameraData();
            camData.renderPostProcessing = false;       // Needs to be off to render transparent backgrounds
            
            // This camera can only render gizmos
            RTCamera.get.SetCameraRenderConfig(camera, new RTCameraRenderConfig { renderFlags = ECameraRenderFlags.Gizmos });

            // Return camera
            return camera;
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_Clear (Public Function)
        // Desc: Called by the system to clear the gizmos module. This function destroys
        //       all gizmos.
        //-----------------------------------------------------------------------------
        public void Internal_Clear()
        {
            // Clear all gizmos
            mGizmos.Clear();
            mViewGizmos.Clear();

            // Destroy view gizmo render cameras
            int count = mViewGizmoRenderCameras.Count;
            for (int i = 0; i < count; ++i)
            {
                if (mViewGizmoRenderCameras[i] != null)
                    GameObject.DestroyImmediate(mViewGizmoRenderCameras[i].gameObject);
            }
            mViewGizmoRenderCameras.Clear();

            // Reset data
            mHoveredGizmo   = null;
            mDraggedGizmo   = null;
            mFrameRenderCameras.Clear();
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_Update (Public Function)
        // Desc: Called by the system to allow the gizmos to update themselves.
        //-----------------------------------------------------------------------------
        public void Internal_Update()
        {
            // Can we hover gizmos?
            mHoverEnabledAnswer.Clear();
            if (hoverEnabledQuery != null) hoverEnabledQuery(mHoverEnabledAnswer);
            bool canHoverGizmos = !RTScene.get.IsUIPointerBlocked() && mHoverEnabledAnswer.noCount == 0 && skin.globalGizmoStyle.hoverable;

            // Reset drag ended this frame and update if necessary
            mDragEndedThisFrame = false;

            // Do we have to end dragging?
            if (mDraggedGizmo != null)
            {
                if (RTInput.get.pointingInputDevice.pickButtonWentUp)
                {
                    mDragEndedThisFrame = true;
                    mDraggedGizmo.Internal_EndDrag();
                    mDraggedGizmo = null;
                }
            }

            // If we're not dragging, reset the hovered gizmo
            if (mDraggedGizmo == null)
                mHoveredGizmo = null;

            // Loop through each gizmo and update it. Also update the hovered gizmo as we go.
            int gizmoCount = mGizmos.Count;
            float tMin = float.MaxValue;
            for (int i = 0; i < gizmoCount; ++i)
            {
                // Update gizmo
                mGizmos[i].Internal_Update();

                // Raycast to update the hovered gizmo, but only if we are not dragging.
                // While dragging, we want the same hovered gizmo until the end of the
                // drag operation.
                if (mDraggedGizmo == null && canHoverGizmos)
                {
                    if (mGizmos[i].Internal_HoverRaycast(out GizmoRayHit rayHit))
                    {
                        // Is this hit closer or is the hit gizmo a view gizmo?
                        if (rayHit.t < tMin || mGizmos[i] is ViewGizmo)
                        {
                            // Update hover data
                            mHoveredGizmo = mGizmos[i];
                            tMin = rayHit.t;
                        }
                    }
                }
            }

            // Do we have to start dragging?
            if (mHoveredGizmo != null && mDraggedGizmo == null)
            {
                // Did we press the pick button?
                if (RTInput.get.pointingInputDevice.pickButtonWentDown)
                {
                    // Notify the hovered gizmo and start dragging
                    mHoveredGizmo.Internal_OnClickedHoveredHandle();
                    GizmoDrag drag = mHoveredGizmo.Internal_StartDrag();
                    if (drag != null) mDraggedGizmo = mHoveredGizmo;
                }
            }
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_Render (Public Function)
        // Desc: Called by the system to allow the gizmos to render themselves.
        // Parm: camera    - The camera that renders the gizmos.
        //       rasterCtx - The raster graph context.
        //-----------------------------------------------------------------------------
        public void Internal_Render(Camera camera, RasterGraphContext rasterCtx)
        {
            // Clear render camera list for this frame if a new frame has started
            if (mFrameIndex != RTFrame.index)
            {
                mFrameIndex = RTFrame.index;
                mFrameRenderCameras.Clear();
            }

            // Store the render camera
            mFrameRenderCameras.Add(camera);

            // Create property block if not already created
            if (sMtrlPropertyBlock == null)
                sMtrlPropertyBlock = new MaterialPropertyBlock();

            // Set current raster command context
            RTGizmos.rasterCtx = rasterCtx;

            // No-op?
            if (!skin.globalGizmoStyle.visible)
                return;

            // Fire pre-render event
            if (preRenderGizmos != null)
                preRenderGizmos(camera, rasterCtx);

            // Sort?
            // Note: We don't need to sort if the render camera is a view gizmo render camera. The view gizmo
            //       render camera only renders a single gizmo and no sorting is needed. 
            if (mSortGizmos && !IsViewGizmoRenderCamera(camera))
            {
                // Move gizmos in the sorted list
                mSortedGizmos.Clear();
                mSortedGizmos.AddRange(mGizmos);

                // Sort
                Vector3 camPos      = camera.transform.position;
                Vector3 camForward  = camera.transform.forward;
                mSortedGizmos.Sort((g0, g1) => 
                {
                    float d0 = Vector3.Dot(g0.transform.position - camPos, camForward);
                    float d1 = Vector3.Dot(g1.transform.position - camPos, camForward);
                    return d1.CompareTo(d0);
                });

                // Loop through each gizmo and render it
                int gizmoCount = mSortedGizmos.Count;
                for (int i = 0; i < gizmoCount; ++i)
                    mSortedGizmos[i].Internal_Render(camera, rasterCtx);
            }
            else
            {
                // Loop through each gizmo and render it
                int gizmoCount = mGizmos.Count;
                for (int i = 0; i < gizmoCount; ++i)
                    mGizmos[i].Internal_Render(camera, rasterCtx);
            }

            // Fire post-render event
            if (postRenderGizmos != null)
                postRenderGizmos(camera, rasterCtx);
        }

        //-----------------------------------------------------------------------------
        // Name: IsGizmoGUIHovered() (Public Function)
        // Desc: Checks if the GUI of any gizmo is hovered.
        // Rtrn: True if at least one gizmo has its GUI hovered and false otherwise.
        //-----------------------------------------------------------------------------
        public bool IsGizmoGUIHovered()
        {
            // Loop through each gizmo
            int count = mGizmos.Count;
            for (int i = 0; i < count; ++i)
            {
                // Hovered?
                if (mGizmos[i].IsGUIHovered())
                    return true;
            }

            // Not hovered
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateGizmo() (Public Function)
        // Desc: Creates a gizmo of the specified type.
        // Parm: T - Gizmo type. Must derive from 'Gizmo'.
        // Rtrn: The created gizmo.
        //-----------------------------------------------------------------------------
        public T CreateGizmo<T>() where T : Gizmo, new()
        {
            // Create the gizmo and initialize it
            T gizmo = new T();
            gizmo.Internal_Init();
            gizmo.enabledStateChanged += OnGizmoEnabledStateChanged;

            // Store gizmo
            mGizmos.Add(gizmo);

            // Return the gizmo
            return gizmo;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateViewGizmo() (Public Function)
        // Desc: Creates a view gizmo for the specified view camera.
        // Parm: viewCamera - The view camera. The gizmo is displayed inside this camera's
        //                    viewport.
        // Rtrn: The created gizmo.
        //-----------------------------------------------------------------------------
        public ViewGizmo CreateViewGizmo(Camera viewCamera)
        {
            // Create the gizmo and store it
            var gizmo = CreateGizmo<ViewGizmo>();
            gizmo.viewCamera = viewCamera;
            mViewGizmos.Add(gizmo);

            // Return gizmo
            return gizmo;
        }

        //-----------------------------------------------------------------------------
        // Name: IsViewGizmoRenderCamera (Public Function)
        // Desc: Checks if the specified camera is a view gizmo render camera.
        // Parm: camera - Query camera.
        // Rtrn: True if 'camera' is a view gizmo render camera and false otherwise.
        //-----------------------------------------------------------------------------
        public bool IsViewGizmoRenderCamera(Camera camera)
        {
            return mViewGizmoRenderCameras.Contains(camera);
        }

        //-----------------------------------------------------------------------------
        // Name: GetViewGizmo (Public Function)
        // Desc: Returns the view gizmo with the specified index. 
        // Parm: index - View gizmo index.
        // Rtrn: The view gizmo with the specified index.
        //-----------------------------------------------------------------------------
        public ViewGizmo GetViewGizmo(int index)
        {
            return mViewGizmos[index];
        }

        //-----------------------------------------------------------------------------
        // Name: CreateObjectMoveGizmo() (Public Function)
        // Desc: Creates a move gizmo that can be used to move objects.
        // Rtrn: The created gizmo.
        //-----------------------------------------------------------------------------
        public MoveGizmo CreateObjectMoveGizmo()
        {
            // Create the gizmo and attach the 'ObjectTransformGizmo' behaviour
            var gizmo = CreateGizmo<MoveGizmo>();
            gizmo.CreateBehaviour<ObjectTransformGizmo>();

            // Return gizmo
            return gizmo;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateObjectRotateGizmo() (Public Function)
        // Desc: Creates a rotate gizmo that can be used to rotate objects.
        // Rtrn: The created gizmo.
        //-----------------------------------------------------------------------------
        public RotateGizmo CreateObjectRotateGizmo()
        {
            // Create the gizmo and attach the 'ObjectTransformGizmo' behaviour
            var gizmo = CreateGizmo<RotateGizmo>();
            gizmo.CreateBehaviour<ObjectTransformGizmo>();

            // Return gizmo
            return gizmo;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateObjectScaleGizmo() (Public Function)
        // Desc: Creates a scale gizmo that can be used to scale objects.
        // Rtrn: The created gizmo.
        //-----------------------------------------------------------------------------
        public ScaleGizmo CreateObjectScaleGizmo()
        {
            // Create the gizmo and attach the 'ObjectTransformGizmo' behaviour
            var gizmo = CreateGizmo<ScaleGizmo>();
            gizmo.CreateBehaviour<ObjectTransformGizmo>();

            // Return gizmo
            return gizmo;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateObjectTRSGizmo() (Public Function)
        // Desc: Creates a TRS gizmo that can be used to move, rotate and scale objects.
        // Rtrn: The created gizmo.
        //-----------------------------------------------------------------------------
        public TRSGizmo CreateObjectTRSGizmo()
        {
            // Create the gizmo and attach the 'ObjectTransformGizmo' behaviour
            var gizmo = CreateGizmo<TRSGizmo>();
            gizmo.CreateBehaviour<ObjectTransformGizmo>();

            // Return gizmo
            return gizmo;
        }

        //-----------------------------------------------------------------------------
        // Name: CollectObjectTransformGizmos() (Public Function)
        // Desc: Collects all object transform gizmos and stores them inside the specified
        //       list.
        // Parm: transformGizmos - Returns the collected object transform gizmos.
        //-----------------------------------------------------------------------------
        public void CollectObjectTransformGizmos(List<ObjectTransformGizmo> transformGizmos)
        {
            // Clear output
            transformGizmos.Clear();

            // Loop through each gizmo
            int count = mGizmos.Count;
            for (int i = 0; i < count; ++i)
            {
                // If the gizmo has an object transform gizmo, store it
                if (mGizmos[i].objectTransformGizmo != null)
                    transformGizmos.Add(mGizmos[i].objectTransformGizmo);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: SetMoveGizmosMoveMode() (Public Function)
        // Desc: Sets the move mode for all move gizmos. Also affects TRS gizmos.
        // Parm: moveMode - Move mode.
        //-----------------------------------------------------------------------------
        public void SetMoveGizmosMoveMode(EGizmoMoveMode moveMode)
        {
            // Loop through each gizmo
            int count = mGizmos.Count;
            for (int i = 0; i < count; ++i)
            {
                // Try move gizmo
                MoveGizmo moveGizmo = mGizmos[i] as MoveGizmo;
                if (moveGizmo != null)
                {
                    moveGizmo.moveMode = moveMode;
                    continue;
                }

                // Try TRS gizmo
                TRSGizmo trsGizmo = mGizmos[i] as TRSGizmo;
                if (trsGizmo != null)
                {
                    trsGizmo.moveMode = moveMode;
                    continue;
                }
            }
        }
        
        //-----------------------------------------------------------------------------
        // Name: SetAltModeEnabled() (Public Function)
        // Desc: Sets the enabled state of the gizmos' alternative mode. The alternative
        //       mode depends on the gizmo type. For example, for a move gizmo, the alternative
        //       mode is 'EGizmoMoveMode.Screen'.
        // Parm: enabled - The alt mode enabled state.
        //-----------------------------------------------------------------------------
        public void SetAltModeEnabled(bool enabled)
        {
            // Cache data
            EGizmoMoveMode moveMode = enabled ? EGizmoMoveMode.Screen : EGizmoMoveMode.Normal;

            // Loop through each gizmo
            int count = mGizmos.Count;
            for (int i = 0; i < count; ++i)
            {
                // Try move gizmo
                MoveGizmo moveGizmo = mGizmos[i] as MoveGizmo;
                if (moveGizmo != null)
                {
                    moveGizmo.moveMode = moveMode;
                    continue;
                }

                // Try TRS gizmo
                TRSGizmo trsGizmo = mGizmos[i] as TRSGizmo;
                if (trsGizmo != null)
                {
                    trsGizmo.moveMode = moveMode;
                    continue;
                }
            }
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: Start() (Public Function)
        // Desc: Called by Unity to allow the object to initialize itself.
        //-----------------------------------------------------------------------------
        void Start()
        {
            // No need for layout events
            useGUILayout = false;
        }

        //-----------------------------------------------------------------------------
        // Name: OnGUI() (Public Function)
        // Desc: Called during the 'OnGUI' event to allow the gizmos to draw their GUI.
        //-----------------------------------------------------------------------------
        void OnGUI()
        {
            // No-op?
            int cameraCount = mFrameRenderCameras.Count;
            if (!skin.globalGizmoStyle.visible || cameraCount == 0)
                return;

            // Loop through each camera and notify gizmos
            int gizmoCount = mGizmos.Count;
            for (int c = 0; c < cameraCount; ++c)
            {
                // Notify gizmos
                Camera camera = mFrameRenderCameras[c];
                for (int g = 0; g < gizmoCount; ++g)
                    mGizmos[g].Internal_OnGUI(camera);

                // Fire 'onGUI' event for this camera
                if (onGUI != null)
                    onGUI(camera, camera.pixelRect.ScreenToGUIRect());
            }

            //-----------------------------------------------------------------------------
            // Clear the list of render cameras after each Repaint pass consumes the
            // currently collected cameras.
            //
            // The ordering between camera rendering and OnGUI execution is not stable
            // across Unity versions, render pipelines, or runtime contexts. Camera
            // rendering and OnGUI may be interleaved (e.g. CameraA render -> Repaint ->
            // CameraB render -> Repaint), or multiple cameras may render before a single
            // Repaint pass occurs.
            //
            // Because OnGUI does not provide a reliable direct camera context here, we
            // collect cameras as they render and consume the accumulated list during
            // Repaint. The list is then cleared so that the same camera set is not drawn
            // again by subsequent OnGUI passes.
            //-----------------------------------------------------------------------------
            if (Event.current.type == EventType.Repaint)
                mFrameRenderCameras.Clear();
        }

        //-----------------------------------------------------------------------------
        // Name: OnGizmoEnabledStateChanged() (Public Function)
        // Desc: Event handler for the gizmo enabled state changed event.
        // Parm: gizmo - The gizmo that fired the event.
        //-----------------------------------------------------------------------------
        void OnGizmoEnabledStateChanged(Gizmo gizmo)
        {
            // Was the gizmo disabled?
            if (!gizmo.enabled)
            {
                // Clear data
                if (mHoveredGizmo == gizmo)
                    mHoveredGizmo = null;
                if (mDraggedGizmo == gizmo)
                    mDraggedGizmo = null;
            }
        }
        #endregion

        #region Private Static Functions
        //-----------------------------------------------------------------------------
        // Name: BindRenderStates() (Private Static Function)
        // Desc: Binds the specified 'GizmoRenderStates' before drawing. This function
        //       also updates the 'sMaterial' field with the material that should be
        //       used for drawing.
        // Parm: grs - 'GizmoRenderStates' instance.
        //-----------------------------------------------------------------------------
        static void BindRenderStates(GizmoRenderStates grs)
        {
            // Get material for the selected render states
            sMaterial = MaterialManager.GetRTGizmoHandleMaterial(grs.materialRenderStates);

            // Lighting states
            sMtrlPropertyBlock.Clear();
            if (grs.lit)
            {
                sMtrlPropertyBlock.SetInt("_Lit", 1);
                sMtrlPropertyBlock.SetVector("_LightDirection", grs.lightDirection);
            }
            else sMtrlPropertyBlock.SetInt("_Lit", 0);

            // Misc
            sMtrlPropertyBlock.SetColor("_Color",       grs.color);
            sMtrlPropertyBlock.SetFloat("_AlphaScale",  grs.alphaScale);
            sMtrlPropertyBlock.SetFloat("_ZoomScale",   grs.zoomScale);

            // Set shape args
            sMtrlPropertyBlock.SetInt("_HandleShape",           (int)grs.renderShape);
            sMtrlPropertyBlock.SetVector("_HandleShapeArgs",    grs.renderShapeArgs);
            sMtrlPropertyBlock.SetVector("_ArcNormal",          grs.arcNormal);

            // Sphere culling
            if (grs.cullSphereEnabled)
            {
                var cullSphere = grs.cullSphere;
                var cullCenter = grs.cullSphere.center;

                sMtrlPropertyBlock.SetInt("_CullSphereEnabled",     1);
                sMtrlPropertyBlock.SetInt("_CullSphereMode",        (int)grs.cullSphereMode);
                sMtrlPropertyBlock.SetVector("_CullSphere",         new Vector4(cullCenter.x, cullCenter.y, cullCenter.z, cullSphere.radius));
                sMtrlPropertyBlock.SetFloat("_CullAlphaScale",      RTGizmos.get.skin.globalGizmoStyle.cullAlphaScale);
            }
            else sMtrlPropertyBlock.SetInt("_CullSphereEnabled",    0);

            // Clip planes
            var clipPlane = grs.GetClipPlane(0);
            if (clipPlane.enabled)
            {
                Vector3 normal = clipPlane.plane.normal;
                sMtrlPropertyBlock.SetInt("_ClipPlaneEnabled",  1);
                sMtrlPropertyBlock.SetVector("_ClipPlane",      new Vector4(normal.x, normal.y, normal.z, clipPlane.plane.distance));
            }
            else sMtrlPropertyBlock.SetInt("_ClipPlaneEnabled", 0);
        }

        //-----------------------------------------------------------------------------
        // Name: ExtractHandleRenderStates() (Private Static Function)
        // Desc: Extract common handle render states and store them in 'grs' before
        //       rendering a handle.
        // Parm: handle - The handle which is about to be rendered.
        //       args   - Handle render args.
        //       grs    - Gizmo render states which receive state data from the handle.
        //-----------------------------------------------------------------------------
        static void ExtractHandleRenderStates(GizmoHandle handle, GizmoHandleRenderArgs args, GizmoRenderStates grs)
        {
            // Set default render states
            grs.lightDirection  = args.camera.transform.forward;
            grs.zoomScale       = handle.GetZoomScale(args.camera);
            grs.alphaScale      = handle.alphaScale;

            // Are we using a cull sphere?
            if ((handle.cullSphereTargets & EGizmoHandleCullSphereTargets.Render) != 0)
            {
                // Note: Don't cull if we're hovered and hovered pixel culling is disabled.
                grs.cullSphereEnabled   = handle.cullSphereEnabled && (handle.cullHoveredPixels || RTGizmos.get.hoveredHandle != handle);
                grs.cullSphereMode      = handle.cullSphereMode;
                grs.cullSphere          = handle.cullSphere;
            }
            else grs.cullSphereEnabled = false;

            // Clip planes
            var clipPlane = handle.GetClipPlane(0);
            grs.SetClipPlane(0, clipPlane.plane);
            grs.SetClipPlaneEnabled(0, clipPlane.enabled);
        }
        #endregion
    }
    #endregion
}
