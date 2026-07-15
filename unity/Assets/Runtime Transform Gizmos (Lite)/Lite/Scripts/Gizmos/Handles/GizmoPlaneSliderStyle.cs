using System;
using UnityEngine;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EGizmoPlaneSliderBorderType (Public Enum)
    // Desc: Defines different plane slider border types.
    //-----------------------------------------------------------------------------
    public enum EGizmoPlaneSliderBorderType
    {
        Thin = 0,
        Thick,
        WireThick
    }

    //-----------------------------------------------------------------------------
    // Name: EGizmoPlaneSliderType (Public Enum)
    // Desc: Defines different types of gizmo plane sliders.
    //-----------------------------------------------------------------------------
    public enum EGizmoPlaneSliderType
    {
        Quad = 0,   // Quad slider
        RATriangle  // Right-angled triangle
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GizmoPlaneSliderStyle (Public Class)
    // Desc: Stores style properties for a gizmo plane slider.
    //-----------------------------------------------------------------------------
    [Serializable]
    public class GizmoPlaneSliderStyle : GizmoHandleStyle
    {
        #region Private Fields
        [SerializeField] EGizmoPlaneSliderType          mSliderType         = defaultSliderType;        // Slider type
        [SerializeField] EGizmoPlaneSliderBorderType    mBorderType         = defaultBorderType;        // Border type

        [SerializeField] bool   mPlaneVisible       = defaultPlaneVisible;      // Is the plane visible?
        [SerializeField] bool   mBorderVisible      = defaultBorderVisible;     // Is the border visible?
        [SerializeField] Color  mColor              = defaultColor;             // Color
        [SerializeField] Color  mBorderColor        = defaultBorderColor;       // Border color
        [SerializeField] float  mQuadWidth          = defaultQuadSize;          // Quad width
        [SerializeField] float  mQuadHeight         = defaultQuadSize;          // Quad height
        [SerializeField] float  mQuadBorderWidth    = defaultBorderSize;        // Quad border width
        [SerializeField] float  mQuadBorderHeight   = defaultBorderSize;        // Quad border height
        [SerializeField] float  mRATriangleSize     = defaultRATriangleSize;    // The size of the right-angled triangle adjacent edges
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: sliderType (Public Property)
        // Desc: Returns or sets the slider type.
        //-----------------------------------------------------------------------------
        public EGizmoPlaneSliderType        sliderType  { get { return mSliderType; } set { mSliderType = value; } } 

        //-----------------------------------------------------------------------------
        // Name: borderType (Public Property)
        // Desc: Returns or sets the border type.
        //-----------------------------------------------------------------------------
        public EGizmoPlaneSliderBorderType  borderType  { get { return mBorderType; } set { mBorderType = value; } }

        //-----------------------------------------------------------------------------
        // Name: planeVisible (Public Property)
        // Desc: Returns or sets the plane visibility.
        //-----------------------------------------------------------------------------
        public bool     planeVisible        { get { return mPlaneVisible; } set { mPlaneVisible = value; } }

        //-----------------------------------------------------------------------------
        // Name: borderVisible (Public Property)
        // Desc: Returns or sets the border visibility.
        //-----------------------------------------------------------------------------
        public bool     borderVisible       { get { return mBorderVisible; } set { mBorderVisible = value; } }

        //-----------------------------------------------------------------------------
        // Name: color (Public Property)
        // Desc: Returns or sets the slider color.
        //-----------------------------------------------------------------------------
        public Color    color           { get { return mColor; } set { mColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: borderColor (Public Property)
        // Desc: Returns or sets the border color.
        //-----------------------------------------------------------------------------
        public Color    borderColor     { get { return mBorderColor; } set { mBorderColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: quadWidth (Public Property)
        // Desc: Returns or sets the quad width.
        //-----------------------------------------------------------------------------
        public float    quadWidth       { get { return mQuadWidth; } set { mQuadWidth = Mathf.Max(1e-3f, value); } }

        //-----------------------------------------------------------------------------
        // Name: quadHeight (Public Property)
        // Desc: Returns or sets the quad height.
        //-----------------------------------------------------------------------------
        public float    quadHeight      { get { return mQuadHeight; } set { mQuadHeight = Mathf.Max(1e-3f, value); } }

        //-----------------------------------------------------------------------------
        // Name: quadBorderWidth (Public Property)
        // Desc: Returns or sets the quad border width.
        //-----------------------------------------------------------------------------
        public float    quadBorderWidth     { get { return mQuadBorderWidth; } set { mQuadBorderWidth = Mathf.Max(1e-3f, value); } }

        //-----------------------------------------------------------------------------
        // Name: quadBorderHeight (Public Property)
        // Desc: Returns or sets the quad border height.
        //-----------------------------------------------------------------------------
        public float    quadBorderHeight    { get { return mQuadBorderHeight; } set { mQuadBorderHeight = Mathf.Max(1e-3f, value); } }

        //-----------------------------------------------------------------------------
        // Name: raTriangleSize (Public Property)
        // Desc: Returns or sets the size of the right-angled triangle adjacent edges.
        //-----------------------------------------------------------------------------
        public float    raTriangleSize      { get { return mRATriangleSize; } set { mRATriangleSize = Mathf.Max(1e-3f, value); } }
        #endregion

        #region Public Static Properties
        public static EGizmoPlaneSliderType         defaultSliderType       { get { return EGizmoPlaneSliderType.Quad; } }
        public static EGizmoPlaneSliderBorderType   defaultBorderType       { get { return EGizmoPlaneSliderBorderType.Thin; } }
        public static bool                          defaultPlaneVisible     { get { return true; } }
        public static bool                          defaultBorderVisible    { get { return true; } }
        public static Color                         defaultColor            { get { return Color.white; } }
        public static Color                         defaultBorderColor      { get { return Color.black; } }
        public static float                         defaultQuadSize         { get { return 1.0f; } }
        public static float                         defaultBorderSize       { get { return 0.1f; } }
        public static float                         defaultRATriangleSize   { get { return 1.0f; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: CloneStyle() (Public Function)
        // Desc: Clones the style.
        // Rtrn: The cloned style.
        //-----------------------------------------------------------------------------
        public GizmoPlaneSliderStyle CloneStyle()
        {
            return MemberwiseClone() as GizmoPlaneSliderStyle;
        }
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: OnUseDefaults() (Protected Function)
        // Desc: Called in order to set the style properties to default values.
        //-----------------------------------------------------------------------------
        protected override void OnUseDefaults()
        {
            sliderType          = defaultSliderType;
            borderType          = defaultBorderType;
            planeVisible        = defaultPlaneVisible;
            borderVisible       = defaultBorderVisible;
            color               = defaultColor;
            borderColor         = defaultBorderColor;
            quadWidth           = defaultQuadSize;
            quadHeight          = defaultQuadSize;
            quadBorderWidth     = defaultBorderSize;
            quadBorderHeight    = defaultBorderSize;
            raTriangleSize      = defaultRATriangleSize;
        }
        #endregion
    }
    #endregion
}
