using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: MoveGizmoStyle (Public Class)
    // Desc: Stores style properties move gizmos.
    //-----------------------------------------------------------------------------
    [Serializable] public class MoveGizmoStyle : GizmoStyle
    {
        #region Private Fields
        [SerializeField] GizmoLineSliderStyle[]   mAxisSliderStylesP    = new GizmoLineSliderStyle[3]{ new GizmoLineSliderStyle(), new GizmoLineSliderStyle(), new GizmoLineSliderStyle() };        // Positive XYZ slider styles
        [SerializeField] GizmoLineSliderStyle[]   mAxisSliderStylesN    = new GizmoLineSliderStyle[3]{ new GizmoLineSliderStyle(), new GizmoLineSliderStyle(), new GizmoLineSliderStyle() };        // Negative XYZ slider styles
        [SerializeField] GizmoCapStyle[]          mAxisCapStylesP       = new GizmoCapStyle[3]{ new GizmoCapStyle(), new GizmoCapStyle(), new GizmoCapStyle() };                                    // Positive XYZ slider cap styles
        [SerializeField] GizmoCapStyle[]          mAxisCapStylesN       = new GizmoCapStyle[3]{ new GizmoCapStyle(), new GizmoCapStyle(), new GizmoCapStyle() };                                    // Negative XYZ slider cap styles
        [SerializeField] GizmoPlaneSliderStyle[]  mDblAxisSliderStyles  = new GizmoPlaneSliderStyle[3]{ new GizmoPlaneSliderStyle(), new GizmoPlaneSliderStyle(), new GizmoPlaneSliderStyle() };    // Double-axis slider styles

        [SerializeField] float          mAxisOffset         = defaultAxisOffset;        // Single-axis slider offset from the gizmo origin
        [SerializeField] float          mDblAxisOffset      = defaultDblAxisOffset;     // Double-axis slider offset from the gizmo origin
        [SerializeField] GizmoCapStyle  mVSnapCapStyle      = new GizmoCapStyle();      // Vertex snap cap style
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: axisOffset (Public Property)
        // Desc: Returns or sets the single-axis slider offset from the gizmo origin.
        //-----------------------------------------------------------------------------
        public float axisOffset     { get { return mAxisOffset; } set { mAxisOffset = value; } }

        //-----------------------------------------------------------------------------
        // Name: dblAxisOffset (Public Property)
        // Desc: Returns or sets the double-axis slider offset from the gizmo origin.
        //-----------------------------------------------------------------------------
        public float dblAxisOffset  { get { return mDblAxisOffset; } set { mDblAxisOffset = value; } }

        //-----------------------------------------------------------------------------
        // Name: vSnapCapStyle (Public Property)
        // Desc: Returns the vertex snap cap style.
        //-----------------------------------------------------------------------------
        public GizmoCapStyle    vSnapCapStyle   { get { return mVSnapCapStyle; } }
        #endregion

        #region Public Static Properties
        public static bool          defaultNXVisible            { get { return false; } }
        public static bool          defaultNYVisible            { get { return false; } }
        public static bool          defaultNZVisible            { get { return false; } }

        public static float         defaultAxisOffset           { get { return 0.0f; } }
        public static float         defaultDblAxisOffset        { get { return 0.0f; } }

        public static EGizmoCapType defaultVSnapCapType         { get { return EGizmoCapType.WireQuad; } }
        public static float         defaultVSnapQuadSize        { get { return 1.1f; } }
        public static float         defaultVSnapCircleRadius    { get { return 0.8f; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: CloneStyle() (Public Function)
        // Desc: Clones the style.
        // Rtrn: The cloned style.
        //-----------------------------------------------------------------------------
        public MoveGizmoStyle CloneStyle()
        {
            var clone = MemberwiseClone() as MoveGizmoStyle;
          
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

            clone.mVSnapCapStyle   = mVSnapCapStyle.CloneStyle();

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
            // Axis sliders & caps
            int axisSliderCount = mAxisSliderStylesP.Length;
            for (int i = 0; i < axisSliderCount; ++i)
            {
                mAxisSliderStylesP[i].UseDefaults();
                mAxisSliderStylesN[i].UseDefaults();

                mAxisCapStylesP[i].UseDefaults();
                mAxisCapStylesN[i].UseDefaults();
            }
            axisOffset = defaultAxisOffset;

            // Dbl-axis sliders
            for (int i = 0; i < mDblAxisSliderStyles.Length; ++i)
                mDblAxisSliderStyles[i].UseDefaults();

            dblAxisOffset = defaultDblAxisOffset;

            // Vertex snapping
            mVSnapCapStyle.UseDefaults();
            mVSnapCapStyle.capType      = defaultVSnapCapType;
            mVSnapCapStyle.quadSize     = Vector2Ex.FromValue(defaultVSnapQuadSize);
            mVSnapCapStyle.circleRadius = defaultVSnapCircleRadius;

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
            Vector3         newSize3;
            Vector2         newSize2;

            var content             = new GUIContent();
            int axisSliderCount     = mAxisSliderStylesP.Length;
            int dblAxisSliderCount  = mDblAxisSliderStyles.Length;

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
                // Quad size
                content.text    = "Size";
                content.tooltip = "Plane slider size.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, mDblAxisSliderStyles[0].quadWidth);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    for (int i = 0; i < dblAxisSliderCount; ++i)
                    {
                        mDblAxisSliderStyles[i].quadWidth  = newFloat;
                        mDblAxisSliderStyles[i].quadHeight = newFloat;
                    }
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

                // Border type
                content.text    = "Border type";
                content.tooltip = "Border type.";
                EditorGUI.BeginChangeCheck();
                EGizmoPlaneSliderBorderType newBorderType = (EGizmoPlaneSliderBorderType)EditorGUILayout.EnumPopup(content, mDblAxisSliderStyles[0].borderType);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    for (int i = 0; i < dblAxisSliderCount; ++i)
                        mDblAxisSliderStyles[i].borderType = newBorderType;
                }

                // Check border type
                if (newBorderType == EGizmoPlaneSliderBorderType.Thick ||
                    newBorderType == EGizmoPlaneSliderBorderType.WireThick)
                {
                    // Border width
                    content.text    = "Width";
                    content.tooltip = "The border width.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mDblAxisSliderStyles[0].quadBorderWidth);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        for (int i = 0; i < dblAxisSliderCount; ++i)
                            mDblAxisSliderStyles[i].quadBorderWidth = newFloat;
                    }

                    // Border height
                    content.text    = "Height";
                    content.tooltip = "The border height.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mDblAxisSliderStyles[0].quadBorderHeight);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        for (int i = 0; i < dblAxisSliderCount; ++i)
                            mDblAxisSliderStyles[i].quadBorderHeight = newFloat;
                    }
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
                for (int i = 0; i < dblAxisSliderCount; ++i)
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

            #region Vertex Snap
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Vertex Snap");
            {
                // Cap type
                content.text    = "Cap type";
                content.tooltip = "Cap type used when vertex snapping is enabled.";
                EditorGUI.BeginChangeCheck();
                newCapType = (EGizmoCapType)EditorGUILayout.EnumPopup(content, mVSnapCapStyle.capType, (e) => 
                {
                    EGizmoCapType capType = (EGizmoCapType)e;
                    return  capType == EGizmoCapType.Quad || capType == EGizmoCapType.Circle ||
                            capType == EGizmoCapType.WireQuad || capType == EGizmoCapType.WireCircle;
                }, false);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mVSnapCapStyle.capType = newCapType;
                }

                // Check cap type
                if (newCapType == EGizmoCapType.Quad || 
                    newCapType == EGizmoCapType.WireQuad)
                {
                    // Quad size
                    content.text    = "Size";
                    content.tooltip = "The quad size.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mVSnapCapStyle.quadWidth);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        mVSnapCapStyle.quadSize = Vector2Ex.FromValue(newFloat);
                    }
                }
                else
                {
                    // Circle radius
                    content.text    = "Radius";
                    content.tooltip = "The circle radius.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mVSnapCapStyle.circleRadius);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        mVSnapCapStyle.circleRadius  = newFloat;
                    }
                }

                // Vertex snap cap color
                content.text    = "Color";
                content.tooltip = "Vertex snap cap color.";
                EditorGUI.BeginChangeCheck();
                Color newColor  = EditorGUILayout.ColorField(content, mVSnapCapStyle.color);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mVSnapCapStyle.color  = newColor;
                }
            }
            #endregion
        }
        #endif
        #endregion
    }
    #endregion
}