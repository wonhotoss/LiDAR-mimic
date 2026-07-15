using UnityEngine;
using UnityEngine.Rendering;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EGizmoCullSphereMode (Public Enum)
    // Desc: Defines different gizmo cull sphere modes. A cull sphere mode controls
    //       the way in which the cull sphere culls pixels.
    //-----------------------------------------------------------------------------
    public enum EGizmoCullSphereMode
    {
        Full = 0,   // Pixels are culled if they are behind or inside the cull sphere from the camera's perspective
        Behind      // Pixel are culled when they are behind the sphere from the camera perspective
    }
    #endregion

    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: GizmoClipPlane (Public Struct)
    // Desc: Represents a gizmo clip plane.
    //-----------------------------------------------------------------------------
    public struct GizmoClipPlane
    {
        #region Public Fields
        public Plane plane;     // The clip plane
        public bool  enabled;   // Is the clip plane enabled?
        #endregion
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GizmoRenderStates (Public Class)
    // Desc: Provides storage for render state used when drawing gizmo shapes. Some
    //       render states are ignored for certain kinds of primitives. For example,
    //       the 'lit' render state is ignored when drawing wireframe primitives.
    //-----------------------------------------------------------------------------
    public class GizmoRenderStates
    {
        #region Private Fields
        Vector3                 mLightDirection     = defaultLightDirection;        // Light direction
        float                   mAlphaScale         = defaultAlphaScale;            // Alpha scale value used to scale the original pixel alpha
        float                   mZoomScale          = defaultZoomScale;             // Zoom scale

        Vector4                 mRenderShapeArgs    = defaultRenderShapeArgs;       // Render shape args
        Vector3                 mArcNormal          = defaultArcNormal;             // Arc normal

        GizmoClipPlane[]        mClipPlanes         = new GizmoClipPlane[1];        // Array of clip planes
        MaterialRenderStates    mMtrlRenderStates   = defaultMaterialRenderStates;  // Material states used when drawing
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: color (Public Property)
        // Desc: Returns or sets the color used for drawing.
        //-----------------------------------------------------------------------------
        public Color    color   { get; set; } = defaultColor;

        //-----------------------------------------------------------------------------
        // Name: lit (Public Property)
        // Desc: Returns or sets whether the gizmo shapes are lit.
        //-----------------------------------------------------------------------------
        public bool     lit     { get; set; } = defaultLit;

        //-----------------------------------------------------------------------------
        // Name: cullSphereEnabled (Public Property)
        // Desc: Returns or sets whether the cull sphere is enabled.
        //-----------------------------------------------------------------------------
        public bool     cullSphereEnabled   { get; set; } = defaultCullSphereEnabled;
        
        //-----------------------------------------------------------------------------
        // Name: cullSphere (Public Property)
        // Desc: Returns or sets the cull sphere.
        //-----------------------------------------------------------------------------
        public Sphere   cullSphere          { get; set; } = defaultCullSphere;

        //-----------------------------------------------------------------------------
        // Name: cullSphereMode (Public Property)
        // Desc: Returns or sets the cull sphere mode.
        //-----------------------------------------------------------------------------
        public EGizmoCullSphereMode cullSphereMode  { get; set; } = defaultCullSphereMode;

        //-----------------------------------------------------------------------------
        // Name: alphaScale (Public Property)
        // Desc: Returns or sets the alpha scale value which scales the original pixel
        //       alpha values.
        //-----------------------------------------------------------------------------
        public float    alphaScale  { get { return mAlphaScale; } set { mAlphaScale = Mathf.Clamp01(value); } }

        //-----------------------------------------------------------------------------
        // Name: zoomScale (Public Property)
        // Desc: Returns or sets the zoom scale value.
        //-----------------------------------------------------------------------------
        public float    zoomScale   { get { return mZoomScale; } set { mZoomScale = value; } }

        //-----------------------------------------------------------------------------
        // Name: materialRenderStates (Public Property)
        // Desc: Returns or sets the material render states used for drawing.
        //-----------------------------------------------------------------------------
        public MaterialRenderStates materialRenderStates    { get { return mMtrlRenderStates; } set { mMtrlRenderStates = value; } }
        
        //-----------------------------------------------------------------------------
        // Name: lightDirection (Public Property)
        // Desc: Returns or sets the light direction vector. The function normalizes
        //       the input value.
        //-----------------------------------------------------------------------------
        public Vector3              lightDirection          { get { return mLightDirection; } set { mLightDirection = value.normalized; } }

        //-----------------------------------------------------------------------------
        // Name: renderShape (Public Property)
        // Desc: Returns or sets the render shape.
        //-----------------------------------------------------------------------------
        public EGizmoRenderShape    renderShape             { get; set; } = defaultRenderShape;
    
        //-----------------------------------------------------------------------------
        // Name: renderShapeArgs (Public Property)
        // Desc: Returns or sets the render shape arguments. The way this vector is
        //       interpreted depends on the value of 'renderShape':
        //          InsetBox:		xyz - size,		w	- thickness
		//          InsetCircle:	x	- radius,	y	- thickness
		//          InsetCylinder:	x	- radius,	y	- length,		z - thickness
		//          Torus:			x	- radius,   y	- tube radius
		//          Arc:			xyz - start,    w	- angle		
        //       Note: The arc normal must be set with the 'arcNormal' property.
        //-----------------------------------------------------------------------------
        public Vector4              renderShapeArgs         { get { return mRenderShapeArgs; } set { mRenderShapeArgs = value; } }

        //-----------------------------------------------------------------------------
        // Name: arcNormal (Public Property)
        // Desc: Returns or sets the arc normal. Used when 'renderShape' is 'Arc'. The
        //       setter normalizes the input.
        //-----------------------------------------------------------------------------
        public Vector3              arcNormal               { get { return mArcNormal; } set { mArcNormal = value.normalized; } }
        #endregion

        #region Public Static Properties
        public static Color                 defaultColor                { get { return Color.white; } }
        public static bool                  defaultLit                  { get { return false; } }
        public static bool                  defaultCullSphereEnabled    { get { return false; } }
        public static Sphere                defaultCullSphere           { get { return new Sphere(); } }
        public static EGizmoCullSphereMode  defaultCullSphereMode       { get { return EGizmoCullSphereMode.Full; } }
        public static float                 defaultAlphaScale           { get { return 1.0f; } }
        public static float                 defaultZoomScale            { get { return 1.0f; } }
        public static MaterialRenderStates  defaultMaterialRenderStates { get { return new MaterialRenderStates() { zWrite = false, zTest = CompareFunction.Always, cullMode = CullMode.Back }; } }
        public static Vector3               defaultLightDirection       { get { return Vector3.forward; } }
        public static EGizmoRenderShape     defaultRenderShape          { get { return EGizmoRenderShape.InputGeometry; } }
        public static Vector4               defaultRenderShapeArgs      { get { return Vector4.zero; } }
        public static Vector3               defaultArcNormal            { get { return Vector3.up; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: UseDefaults() (Public Function)
        // Desc: Use default settings.
        //-----------------------------------------------------------------------------
        public void UseDefaults()
        {
            color                   = defaultColor;
            lit                     = defaultLit;
            cullSphereEnabled       = defaultCullSphereEnabled;
            cullSphere              = defaultCullSphere;
            cullSphereMode          = defaultCullSphereMode;
            alphaScale              = defaultAlphaScale;
            zoomScale               = defaultZoomScale;
            materialRenderStates    = defaultMaterialRenderStates;
            lightDirection          = defaultLightDirection;
            renderShape             = defaultRenderShape;
            renderShapeArgs         = defaultRenderShapeArgs;
            arcNormal               = defaultArcNormal;

            int count = mClipPlanes.Length;
            for (int i = 0; i < count; ++i)
                mClipPlanes[i] = new GizmoClipPlane();
        }

        //-----------------------------------------------------------------------------
        // Name: SetClipPlane() (Public Function)
        // Desc: Sets the clip plane with the specified index.
        // Parm: index     - Clip plane index. Currently unused. Reserved for future use.
        //       clipPlane - The clip plane.
        //-----------------------------------------------------------------------------
        public void SetClipPlane(int index, Plane clipPlane)
        {
            mClipPlanes[0].plane = clipPlane;
        }

        //-----------------------------------------------------------------------------
        // Name: SetClipPlaneEnabled() (Public Function)
        // Desc: Sets the enabled state of the clip plane with the specified index.
        // Parm: index     - Clip plane index. Currently unused. Reserved for future use.                  
        //       enabled   - The clip plane enabled state.
        //-----------------------------------------------------------------------------
        public void SetClipPlaneEnabled(int index, bool enabled)
        {
            mClipPlanes[0].enabled = enabled;
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
        #endregion
    }
    #endregion
}