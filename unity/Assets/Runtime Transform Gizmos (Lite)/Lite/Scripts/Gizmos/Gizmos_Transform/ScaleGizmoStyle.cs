using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ScaleGizmoStyle (Public Class)
    // Desc: Stores style properties for scale gizmos.
    //-----------------------------------------------------------------------------
    [Serializable] public class ScaleGizmoStyle : GizmoStyle
    {
        #region Private Fields
        [SerializeField] bool                     mSliderScaleEnabled   = defaultSliderScaleEnabled;    // Should the gizmo sliders be scaled while the user interacts with the gizmo?
        [SerializeField] GizmoLineSliderStyle[]   mAxisSliderStylesP    = new GizmoLineSliderStyle[3]{ new GizmoLineSliderStyle(), new GizmoLineSliderStyle(), new GizmoLineSliderStyle() };        // Positive XYZ slider styles
        [SerializeField] GizmoLineSliderStyle[]   mAxisSliderStylesN    = new GizmoLineSliderStyle[3]{ new GizmoLineSliderStyle(), new GizmoLineSliderStyle(), new GizmoLineSliderStyle() };        // Negative XYZ slider styles
        [SerializeField] GizmoCapStyle[]          mAxisCapStylesP       = new GizmoCapStyle[3]{ new GizmoCapStyle(), new GizmoCapStyle(), new GizmoCapStyle() };                                    // Positive XYZ slider cap styles
        [SerializeField] GizmoCapStyle[]          mAxisCapStylesN       = new GizmoCapStyle[3]{ new GizmoCapStyle(), new GizmoCapStyle(), new GizmoCapStyle() };                                    // Negative XYZ slider cap styles
        [SerializeField] GizmoPlaneSliderStyle[]  mDblAxisSliderStyles  = new GizmoPlaneSliderStyle[3]{ new GizmoPlaneSliderStyle(), new GizmoPlaneSliderStyle(), new GizmoPlaneSliderStyle() };    // Double-axis slider styles
        [SerializeField] GizmoCapStyle            mUniformCapStyle      = new GizmoCapStyle();          // Uniform cap style   
        
        [SerializeField] float  mAxisOffset     = defaultAxisOffset;        // Single-axis slider offset from the gizmo origin
        [SerializeField] float  mDblAxisOffset  = defaultDblAxisOffset;     // Double-axis slider offset from the gizmo origin
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: sliderScaleEnabled (Public Property)
        // Desc: Returns or sets whether or not the gizmo sliders should be scaled while
        //       the user interacts with the gizmo.
        //-----------------------------------------------------------------------------
        public bool sliderScaleEnabled          { get { return mSliderScaleEnabled; } set { mSliderScaleEnabled = value; } }

        //-----------------------------------------------------------------------------
        // Name: uniformCapStyle (Public Property)
        // Desc: Returns the uniform scale cap style.
        //-----------------------------------------------------------------------------
        public GizmoCapStyle uniformCapStyle    { get { return mUniformCapStyle; } }

        //-----------------------------------------------------------------------------
        // Name: axisOffset (Public Property)
        // Desc: Returns or sets the single-axis slider offset from the gizmo origin.
        //-----------------------------------------------------------------------------
        public float axisOffset         { get { return mAxisOffset; } set { mAxisOffset = value; } }

        //-----------------------------------------------------------------------------
        // Name: dblAxisOffset (Public Property)
        // Desc: Returns or sets the double-axis slider offset from the gizmo origin.
        //-----------------------------------------------------------------------------
        public float dblAxisOffset      { get { return mDblAxisOffset; } set { mDblAxisOffset = value; } }
        #endregion

        #region Public Static Properties
        public static bool          defaultSliderScaleEnabled       { get { return true; } }

        public static bool          defaultNXVisible                { get { return false; } }
        public static bool          defaultNYVisible                { get { return false; } }
        public static bool          defaultNZVisible                { get { return false; } }

        public static float         defaultAxisOffset               { get { return 0.0f; } }
        public static float         defaultDblAxisOffset            { get { return 0.3f; } }
        public static float         defaultDblAxisSize              { get { return 1.5f; } }

        public static EGizmoCapType defaultAxisCapType              { get { return EGizmoCapType.Box; } }
        public static EGizmoCapType defaultUniformCapType           { get { return EGizmoCapType.Box; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: CloneStyle() (Public Function)
        // Desc: Clones the style.
        // Rtrn: The cloned style.
        //-----------------------------------------------------------------------------
        public ScaleGizmoStyle CloneStyle()
        {
            var clone = MemberwiseClone() as ScaleGizmoStyle;
          
            // Clone styles
            int count = mAxisSliderStylesP.Length;
            for (int i = 0; i < count; ++i)
            {
                clone.mAxisSliderStylesP[i] = mAxisSliderStylesP[i].CloneStyle();
                clone.mAxisSliderStylesN[i] = mAxisSliderStylesN[i].CloneStyle();

                clone.mAxisCapStylesP[i] = mAxisCapStylesP[i].CloneStyle();
                clone.mAxisCapStylesN[i] = mAxisCapStylesN[i].CloneStyle();
            }

            count = mDblAxisSliderStyles.Length;
            for (int i = 0; i < count; ++i)
                clone.mDblAxisSliderStyles[i] = mDblAxisSliderStyles[i].CloneStyle();

            clone.mUniformCapStyle = mUniformCapStyle.CloneStyle();

            // Return clone
            return clone;
        }

        //-----------------------------------------------------------------------------
        // Name: GetAxisSliderStyle() (Public Function)
        // Desc: Returns the axis slider style for the specified axis.
        // Parm: axis     - Axis index: (0 = X, 1 = Y, 2 = Z).
        //       positive - True if a positive axis slider should be returned and false
        //                  otherwise.
        // Rtrn: The slider style for the specified axis.
        //-----------------------------------------------------------------------------
        public GizmoLineSliderStyle GetAxisSliderStyle(int axis, bool positive)
        {
            return positive ? mAxisSliderStylesP[axis] : mAxisSliderStylesN[axis];
        }

        //-----------------------------------------------------------------------------
        // Name: GetAxisCapStyle() (Public Function)
        // Desc: Returns the axis cap style for the specified axis.
        // Parm: axis     - Axis index: (0 = X, 1 = Y, 2 = Z).
        //       positive - True if a positive axis cap should be returned and false
        //                  otherwise.
        // Rtrn: The cap style for the specified axis.
        //-----------------------------------------------------------------------------
        public GizmoCapStyle GetAxisCapStyle(int axis, bool positive)
        {
            return positive ? mAxisCapStylesP[axis] : mAxisCapStylesN[axis];
        }

        //-----------------------------------------------------------------------------
        // Name: GetDblAxisSliderStyle() (Public Function)
        // Desc: Returns the double-axis slider style for the specified plane.
        // Parm: plane - Slider plane.
        // Rtrn: The slider style for the specified plane.
        //-----------------------------------------------------------------------------
        public GizmoPlaneSliderStyle GetDblAxisSliderStyle(EPlane plane)
        {
            return mDblAxisSliderStyles[(int)plane];
        }
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: OnUseDefaults() (Protected Function)
        // Desc: Called in order to set the style properties to default values.
        // Parm: parentObject - The serializable parent object used to record property
        //                      changes.
        //-----------------------------------------------------------------------------
        protected override void OnUseDefaults(UnityEngine.Object parentObject)
        {
            sliderScaleEnabled = defaultSliderScaleEnabled;

            // Axis sliders & caps
            int axisSliderCount = mAxisSliderStylesP.Length;
            for (int i = 0; i < axisSliderCount; ++i)
            {
                mAxisSliderStylesP[i].UseDefaults();
                mAxisSliderStylesN[i].UseDefaults();

                mAxisCapStylesP[i].UseDefaults();
                mAxisCapStylesN[i].UseDefaults();

                mAxisCapStylesP[i].capType = defaultAxisCapType;
                mAxisCapStylesN[i].capType = defaultAxisCapType;
            }
            axisOffset = defaultAxisOffset;

            // Dbl-axis sliders
            for (int i = 0; i < mDblAxisSliderStyles.Length; ++i)
            {
                mDblAxisSliderStyles[i].UseDefaults();
                mDblAxisSliderStyles[i].raTriangleSize  = defaultDblAxisSize;
                mDblAxisSliderStyles[i].sliderType      = EGizmoPlaneSliderType.RATriangle;
            }
            dblAxisOffset = defaultDblAxisOffset;

            // Uniform cap
            mUniformCapStyle.UseDefaults();
            mUniformCapStyle.capType = defaultUniformCapType;

            // Visibility
            mAxisSliderStylesN[0].visible = defaultNXVisible;
            mAxisSliderStylesN[1].visible = defaultNYVisible;
            mAxisSliderStylesN[2].visible = defaultNZVisible;

            mAxisCapStylesN[0].visible = defaultNXVisible;
            mAxisCapStylesN[1].visible = defaultNYVisible;
            mAxisCapStylesN[2].visible = defaultNZVisible;
        }

        #if UNITY_EDITOR
        //-----------------------------------------------------------------------------
        // Name: OnEditorGUI() (Protected Function)
        // Desc: Called when the style's GUI must be rendered inside the Unity Editor.
        // Parm: parentObject - The serializable parent object used to record property
        //                      changes.
        //-----------------------------------------------------------------------------
        protected override void OnEditorGUI(UnityEngine.Object parentObject)
        {
            EGizmoCapType   newCapType;
            float           newFloat;         
            bool            newBool;
            Color           newColor;
            Vector3         newSize3;
            Vector2         newSize2;

            var content             = new GUIContent();
            int axisSliderCount     = mAxisSliderStylesP.Length;
            int dblAxisSliderCount  = mDblAxisSliderStyles.Length;

            // Slider scale
            EditorGUILayout.Separator();
            content.text    = "Scale sliders";
            content.tooltip = "Should the gizmo sliders be scaled while the user interacts with the gizmo?";
            EditorGUI.BeginChangeCheck();
            newBool         = EditorGUILayout.ToggleLeft(content, sliderScaleEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                parentObject.OnWillChangeInEditor();
                sliderScaleEnabled = newBool;
            }

            #region Sliders
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Axis Sliders");
            {
                // Length
                content.text    = "Length";
                content.tooltip = "Axis slider length.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, mAxisSliderStylesP[0].length);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    for (int i = 0; i < axisSliderCount; ++i)
                    {
                        mAxisSliderStylesP[i].length = newFloat;
                        mAxisSliderStylesN[i].length = newFloat;
                    }
                }

                // Offset
                content.text    = "Offset";
                content.tooltip = "How much offset to apply to the axis sliders to move them away from the gizmo origin.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, axisOffset);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    axisOffset = newFloat;
                }

                // Start cutoff
                content.text    = "Start cutoff";
                content.tooltip = "How much to cut off from the start of the axis sliders.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, mAxisSliderStylesP[0].startCutoff);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    for (int i = 0; i < axisSliderCount; ++i)
                    {
                        mAxisSliderStylesP[i].startCutoff = newFloat;
                        mAxisSliderStylesN[i].startCutoff = newFloat;
                    }
                }

                // End cutoff
                content.text    = "End cutoff";
                content.tooltip = "How much to cut off from the end of the axis sliders.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, mAxisSliderStylesP[0].endCutoff);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    for (int i = 0; i < axisSliderCount; ++i)
                    {
                        mAxisSliderStylesP[i].endCutoff = newFloat;
                        mAxisSliderStylesN[i].endCutoff = newFloat;
                    }
                }

                // Slider type
                content.text    = "Slider type";
                content.tooltip = "Slider type.";
                EditorGUI.BeginChangeCheck();
                EGizmoLineSliderType newLineType = (EGizmoLineSliderType)EditorGUILayout.EnumPopup(content, mAxisSliderStylesP[0].sliderType);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    for (int i = 0; i < axisSliderCount; ++i)
                    {
                        mAxisSliderStylesP[i].sliderType = newLineType;
                        mAxisSliderStylesN[i].sliderType = newLineType;
                    }
                }

                // Thickness
                if (newLineType != EGizmoLineSliderType.Thin)
                {
                    content.text    = "Thickness";
                    content.tooltip = "Slider thickness when the slider type is not 'Thin'.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mAxisSliderStylesP[0].thickness);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        for (int i = 0; i < axisSliderCount; ++i)
                        {
                            mAxisSliderStylesP[i].thickness = newFloat;
                            mAxisSliderStylesN[i].thickness = newFloat;
                        }
                    }
                }

                // Positive sliders visibility
                EditorGUILayout.Separator();
                EditorUI.PushLabelWidth(10.0f);
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < axisSliderCount; ++i)
                {
                    content.text    = "+" + Core.axisNames[i];
                    content.tooltip = "Is the positive " + Core.axisNames[i] + " slider visible?";
                    EditorGUI.BeginChangeCheck();
                    newBool         = EditorGUILayout.ToggleLeft(content, mAxisSliderStylesP[i].visible, GUILayout.ExpandWidth(false));
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        mAxisSliderStylesP[i].visible = newBool;
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Negative sliders visibility
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < axisSliderCount; ++i)
                {
                    content.text    = "-" + Core.axisNames[i];
                    content.tooltip = "Is the negative " + Core.axisNames[i] + " slider visible?";
                    EditorGUI.BeginChangeCheck();
                    newBool         = EditorGUILayout.ToggleLeft(content, mAxisSliderStylesN[i].visible, GUILayout.ExpandWidth(false));
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        mAxisSliderStylesN[i].visible = newBool;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorUI.PopLabelWidth();
            }
            #endregion

            #region Caps
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Axis Slider Caps");
            {
                // Cap type
                content.text    = "Cap type";
                content.tooltip = "Slider cap type.";
                EditorGUI.BeginChangeCheck();
                newCapType = (EGizmoCapType)EditorGUILayout.EnumPopup(content, mAxisCapStylesP[0].capType, (e) => 
                {
                    EGizmoCapType capType = (EGizmoCapType)e;
                    return capType != EGizmoCapType.Torus && capType != EGizmoCapType.InsetCylinder;
                }, false);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    for (int i = 0; i < axisSliderCount; ++i)
                    {
                        mAxisCapStylesP[i].capType = newCapType;
                        mAxisCapStylesN[i].capType = newCapType;
                    }
                }

                // Check cap type
                if (newCapType == EGizmoCapType.Cone)
                {
                    // Cone radius
                    content.text    = "Radius";
                    content.tooltip = "The cone radius.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mAxisCapStylesP[0].coneRadius);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        for (int i = 0; i < axisSliderCount; ++i)
                        {
                            mAxisCapStylesP[i].coneRadius = newFloat;
                            mAxisCapStylesN[i].coneRadius = newFloat;
                        }
                    }

                    // Cone length
                    content.text    = "Length";
                    content.tooltip = "The cone length.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mAxisCapStylesP[0].coneLength);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        for (int i = 0; i < axisSliderCount; ++i)
                        {
                            mAxisCapStylesP[i].coneLength = newFloat;
                            mAxisCapStylesN[i].coneLength = newFloat;
                        }
                    }
                }
                else
                if (newCapType == EGizmoCapType.Pyramid || 
                    newCapType == EGizmoCapType.WirePyramid)
                {
                    // Base size
                    content.text    = "Base size";
                    content.tooltip = "The pyramid base size.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mAxisCapStylesP[0].pyramidBaseSize);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        for (int i = 0; i < axisSliderCount; ++i)
                        {
                            mAxisCapStylesP[i].pyramidBaseSize = newFloat;
                            mAxisCapStylesN[i].pyramidBaseSize = newFloat;
                        }
                    }

                    // Length
                    content.text    = "Length";
                    content.tooltip = "The pyramid length (i.e. height).";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mAxisCapStylesP[0].pyramidLength);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        for (int i = 0; i < axisSliderCount; ++i)
                        {
                            mAxisCapStylesP[i].pyramidLength = newFloat;
                            mAxisCapStylesN[i].pyramidLength = newFloat;
                        }
                    }
                }
                else
                if (newCapType == EGizmoCapType.Box ||
                    newCapType == EGizmoCapType.WireBox)
                {
                    // Box size
                    content.text    = "Size";
                    content.tooltip = "The box size.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mAxisCapStylesP[0].boxWidth);
                    if (EditorGUI.EndChangeCheck())
                    {
                        newSize3 = Vector3Ex.FromValue(newFloat);
                        parentObject.OnWillChangeInEditor();
                        for (int i = 0; i < axisSliderCount; ++i)
                        {
                            mAxisCapStylesP[i].boxSize = newSize3;
                            mAxisCapStylesN[i].boxSize = newSize3;
                        }
                    }
                }
                else
                if (newCapType == EGizmoCapType.Quad ||
                    newCapType == EGizmoCapType.WireQuad)
                {
                    // Quad size
                    content.text    = "Size";
                    content.tooltip = "The quad size.";
                    EditorGUI.BeginChangeCheck();
                    newFloat = EditorGUILayout.FloatField(content, mAxisCapStylesP[0].quadWidth);
                    if (EditorGUI.EndChangeCheck())
                    {
                        newSize2 = Vector2Ex.FromValue(newFloat);
                        parentObject.OnWillChangeInEditor();
                        for (int i = 0; i < axisSliderCount; ++i)
                        {
                            mAxisCapStylesP[i].quadSize = newSize2;
                            mAxisCapStylesN[i].quadSize = newSize2;
                        }
                    }
                }
                else
                if (newCapType == EGizmoCapType.Sphere)
                {
                    // Sphere radius
                    content.text    = "Radius";
                    content.tooltip = "The sphere radius.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mAxisCapStylesP[0].sphereRadius);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        for (int i = 0; i < axisSliderCount; ++i)
                        {
                            mAxisCapStylesP[i].sphereRadius = newFloat;
                            mAxisCapStylesN[i].sphereRadius = newFloat;
                        }
                    }
                }
                else
                if (newCapType == EGizmoCapType.Circle || 
                    newCapType == EGizmoCapType.WireCircle ||
                    newCapType == EGizmoCapType.InsetCircle)
                {
                    // Circle radius
                    content.text    = "Radius";
                    content.tooltip = "The circle radius.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mAxisCapStylesP[0].circleRadius);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        for (int i = 0; i < axisSliderCount; ++i)
                        {
                            mAxisCapStylesP[i].circleRadius = newFloat;
                            mAxisCapStylesN[i].circleRadius = newFloat;
                        }
                    }

                    // Inset circle?
                    if (newCapType == EGizmoCapType.InsetCircle)
                    {
                        // Circle thickness
                        content.text    = "Thickness";
                        content.tooltip = "The circle thickness.";
                        EditorGUI.BeginChangeCheck();
                        newFloat        = EditorGUILayout.FloatField(content, mAxisCapStylesP[0].insetCircleThickness);
                        if (EditorGUI.EndChangeCheck())
                        {
                            parentObject.OnWillChangeInEditor();
                            for (int i = 0; i < axisSliderCount; ++i)
                            {
                                mAxisCapStylesP[i].insetCircleThickness = newFloat;
                                mAxisCapStylesN[i].insetCircleThickness = newFloat;
                            }
                        }
                    }
                }

                // Positive cap visibility
                EditorGUILayout.Separator();
                EditorUI.PushLabelWidth(10.0f);
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < axisSliderCount; ++i)
                {
                    content.text    = "+" + Core.axisNames[i];
                    content.tooltip = "Is the positive " + Core.axisNames[i] + " cap visible?";
                    EditorGUI.BeginChangeCheck();
                    newBool         = EditorGUILayout.ToggleLeft(content, mAxisCapStylesP[i].visible, GUILayout.ExpandWidth(false));
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        mAxisCapStylesP[i].visible = newBool;
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Negative cap visibility
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < axisSliderCount; ++i)
                {
                    content.text    = "-" + Core.axisNames[i];
                    content.tooltip = "Is the negative " + Core.axisNames[i] + " cap visible?";
                    EditorGUI.BeginChangeCheck();
                    newBool         = EditorGUILayout.ToggleLeft(content, mAxisCapStylesN[i].visible, GUILayout.ExpandWidth(false));
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        mAxisCapStylesN[i].visible = newBool;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorUI.PopLabelWidth();
            }
            #endregion

            #region Double-Axis Sliders
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Dbl-Axis Sliders");
            {               
                // Triangle size
                content.text    = "Size";
                content.tooltip = "Plane slider size.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, mDblAxisSliderStyles[0].raTriangleSize);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    for (int i = 0; i < dblAxisSliderCount; ++i)
                        mDblAxisSliderStyles[i].raTriangleSize  = newFloat;
                }

                // Offset
                content.text    = "Offset";
                content.tooltip = "How much offset to apply to the dbl-axis sliders to move them away from the gizmo origin.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, dblAxisOffset);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    dblAxisOffset = newFloat;
                }               

                // Plane visibility
                EditorGUILayout.Separator();
                content.text    = "Plane";
                content.tooltip = "Is the slider plane visible?";
                EditorGUI.BeginChangeCheck();
                newBool = EditorGUILayout.ToggleLeft(content, mDblAxisSliderStyles[0].planeVisible);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    for (int i = 0; i < dblAxisSliderCount; ++i)
                        mDblAxisSliderStyles[i].planeVisible = newBool;
                }

                // Border visibility
                content.text    = "Border";
                content.tooltip = "Is the slider border visible?";
                EditorGUI.BeginChangeCheck();
                newBool = EditorGUILayout.ToggleLeft(content, mDblAxisSliderStyles[0].borderVisible);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    for (int i = 0; i < dblAxisSliderCount; ++i)
                        mDblAxisSliderStyles[i].borderVisible = newBool;
                }

                // Dbl-axis visibility
                EditorUI.PushLabelWidth(10.0f);
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < axisSliderCount; ++i)
                {
                    content.text    = Core.planeNames[i];
                    content.tooltip = "Is the " + Core.planeNames[i] + " slider visible?";
                    EditorGUI.BeginChangeCheck();
                    newBool         = EditorGUILayout.ToggleLeft(content, mDblAxisSliderStyles[i].visible, GUILayout.ExpandWidth(false));
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        mDblAxisSliderStyles[i].visible = newBool;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorUI.PopLabelWidth();
            }
            #endregion

            #region Uniform Scale Cap
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Uniform Scale Cap");
            {
                // Cap type
                content.text    = "Cap type";
                content.tooltip = "Uniform scale cap type.";
                EditorGUI.BeginChangeCheck();
                newCapType = (EGizmoCapType)EditorGUILayout.EnumPopup(content, mUniformCapStyle.capType, (e) => 
                {
                    EGizmoCapType capType = (EGizmoCapType)e;
                    return  capType == EGizmoCapType.Box || capType == EGizmoCapType.WireBox || capType == EGizmoCapType.Sphere ||
                            capType == EGizmoCapType.Quad || capType == EGizmoCapType.WireQuad ||
                            capType == EGizmoCapType.Circle || capType == EGizmoCapType.WireCircle || capType == EGizmoCapType.InsetCircle;
                }, false);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mUniformCapStyle.capType = newCapType;
                }

                // Check cap type
                if (newCapType == EGizmoCapType.Box ||
                    newCapType == EGizmoCapType.WireBox)
                {
                    // Box size
                    content.text    = "Size";
                    content.tooltip = "The box size.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mUniformCapStyle.boxWidth);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        mUniformCapStyle.boxSize = Vector3Ex.FromValue(newFloat);
                    }
                }
                else
                if (newCapType == EGizmoCapType.Quad ||
                    newCapType == EGizmoCapType.WireQuad)
                {
                    // Quad size
                    content.text    = "Size";
                    content.tooltip = "The quad size.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mUniformCapStyle.quadWidth);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        mUniformCapStyle.quadSize = Vector2Ex.FromValue(newFloat);
                    }
                }
                else
                if (newCapType == EGizmoCapType.Sphere)
                {
                    // Sphere radius
                    content.text    = "Radius";
                    content.tooltip = "The sphere radius.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mUniformCapStyle.sphereRadius);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        mUniformCapStyle.sphereRadius = newFloat;
                    }
                }
                else
                if (newCapType == EGizmoCapType.Circle || 
                    newCapType == EGizmoCapType.WireCircle ||
                    newCapType == EGizmoCapType.InsetCircle)
                {
                    // Circle radius
                    content.text    = "Radius";
                    content.tooltip = "The circle radius.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mUniformCapStyle.circleRadius);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        mUniformCapStyle.circleRadius = newFloat;
                    }

                    // Inset circle?
                    if (newCapType == EGizmoCapType.InsetCircle)
                    {
                        // Circle thickness
                        content.text    = "Thickness";
                        content.tooltip = "The circle thickness.";
                        EditorGUI.BeginChangeCheck();
                        newFloat        = EditorGUILayout.FloatField(content, mUniformCapStyle.insetCircleThickness);
                        if (EditorGUI.EndChangeCheck())
                        {
                            parentObject.OnWillChangeInEditor();
                            mUniformCapStyle.insetCircleThickness = newFloat;
                        }
                    }
                }

                // Cap color
                content.text    = "Color";
                content.tooltip = "Uniform scale cap color.";
                EditorGUI.BeginChangeCheck();
                newColor        = EditorGUILayout.ColorField(content, mUniformCapStyle.color);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mUniformCapStyle.color = newColor;
                }

                // Cap visibility
                EditorGUILayout.Separator();
                content.text    = "Visible";
                content.tooltip = "Is the uniform scale cap visible?";
                EditorGUI.BeginChangeCheck();
                newBool = EditorGUILayout.ToggleLeft(content, mUniformCapStyle.visible);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mUniformCapStyle.visible = newBool;
                }
            }
            #endregion
        }
        #endif
        #endregion
    }
    #endregion
}
