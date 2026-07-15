using UnityEngine;
using System;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EGizmoLineSliderType (Public Enum)
    // Desc: Defines different types of gizmo line sliders.
    //-----------------------------------------------------------------------------
    public enum EGizmoLineSliderType
    {
        Thin = 0,   // Thin line slider
        Box,        // Thick line slider represented by a box
        Cylinder,   // Thick line slider represented by a cylinder

        // Note: Add wire shapes below to make it easy to distinguish between the 2 categories.
        WireBox     // Thick line slider represented by a wire box
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: EGizmoLineSliderTypeEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the
    //       'EGizmoLineSliderType' enum.
    //-----------------------------------------------------------------------------
    public static class EGizmoLineSliderTypeEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: IsWire (Public Enum)
        // Desc: Checks if the slider type belongs to the wire category.
        // Rtrn: True if the slider type belongs to the wire category and false otherwise.
        //-----------------------------------------------------------------------------
        public static bool IsWire(this EGizmoLineSliderType sliderType)
        {
            return (int)sliderType >= (int)EGizmoLineSliderType.WireBox;
        }
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: GizmoLineSliderStyle (Public Class)
    // Desc: Stores style properties for a gizmo line slider.
    //-----------------------------------------------------------------------------
    [Serializable] public class GizmoLineSliderStyle : GizmoHandleStyle
    {
        #region Private Fields
        [SerializeField] EGizmoLineSliderType   mSliderTYpe     = defaultSliderType;    // Slider type
        [SerializeField] Color                  mColor          = defaultColor;         // Slider color
        [SerializeField] float                  mLength         = defaultLength;        // Line length
        [SerializeField] float                  mStartCutoff    = defaultStartCutoff;   // How much to cut off from the start of the line segment in the [0, 1] range
        [SerializeField] float                  mEndCutOff      = defaultEndCutoff;     // How much to cut off from the start of the line segment in the [0, 1] range
        [SerializeField] float                  mThickness      = defaultThickness;     // Line thickness
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: sliderType (Public Property)
        // Desc: Returns or sets the slider type.
        //-----------------------------------------------------------------------------
        public EGizmoLineSliderType   sliderType    { get { return mSliderTYpe; } set { mSliderTYpe = value; } }

        //-----------------------------------------------------------------------------
        // Name: color (Public Property)
        // Desc: Returns or sets the slider color.
        //-----------------------------------------------------------------------------
        public Color            color           { get { return mColor; } set { mColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: length (Public Property)
        // Desc: Returns or sets the slider length.
        //-----------------------------------------------------------------------------
        public float            length          { get { return mLength; } set { mLength = Mathf.Max(value, 0.0f); } }

        //-----------------------------------------------------------------------------
        // Name: startCutoff (Public Property)
        // Desc: Returns or sets the line segment start cutoff. This is a value in the
        //       [0, 1] range and it represents how much of the length should be cut off
        //       from the start of the line segment.
        //-----------------------------------------------------------------------------
        public float            startCutoff     { get { return mStartCutoff; } set { mStartCutoff = Mathf.Clamp01(value); } }

        //-----------------------------------------------------------------------------
        // Name: endCutoff (Public Property)
        // Desc: Returns or sets the line segment end cutoff. This is a value in the
        //       [0, 1] range and it represents how much of the length should be cut off
        //       from the end of the line segment.
        //-----------------------------------------------------------------------------
        public float            endCutoff       { get { return mEndCutOff; } set { mEndCutOff = Mathf.Clamp01(value); } }

        //-----------------------------------------------------------------------------
        // Name: thickness (Public Property)
        // Desc: Returns or sets the slider thickness used when the slider type is not 'Thin'.
        //-----------------------------------------------------------------------------
        public float            thickness       { get { return mThickness; } set { mThickness = Mathf.Max(value, 1e-3f); } }
        #endregion

        #region Public Static Properties
        public static EGizmoLineSliderType  defaultSliderType   { get { return EGizmoLineSliderType.Thin; } }
        public static Color                 defaultColor        { get { return Color.white; } }
        public static float                 defaultLength       { get { return 4.0f; } }
        public static float                 defaultStartCutoff  { get { return 0.0f; } }
        public static float                 defaultEndCutoff    { get { return 0.0f; } }
        public static float                 defaultThickness    { get { return 0.1f; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: CloneStyle() (Public Function)
        // Desc: Clones the style.
        // Rtrn: The cloned style.
        //-----------------------------------------------------------------------------
        public GizmoLineSliderStyle CloneStyle()
        {
            return MemberwiseClone() as GizmoLineSliderStyle;
        }
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: OnUseDefaults() (Protected Function)
        // Desc: Called in order to set the style properties to default values.
        //-----------------------------------------------------------------------------
        protected override void OnUseDefaults()
        {
            sliderType      = defaultSliderType;
            color           = defaultColor;
            length          = defaultLength;
            startCutoff     = defaultStartCutoff;
            endCutoff       = defaultEndCutoff;
            thickness       = defaultThickness;
        }
        #endregion
    }
    #endregion
}

