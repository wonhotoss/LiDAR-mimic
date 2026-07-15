using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RotateGizmoStyle (Public Class)
    // Desc: Stores style properties for rotate gizmos.
    //-----------------------------------------------------------------------------
    [Serializable] public class RotateGizmoStyle : GizmoStyle
    {
        #region Private Fields
        [SerializeField] GizmoCapStyle[]    mAxisCapStyles      = new GizmoCapStyle[] { new GizmoCapStyle(), new GizmoCapStyle(), new GizmoCapStyle() };    // XYZ rotation caps      
        [SerializeField] GizmoCapStyle      mArcBallStyle       = new GizmoCapStyle { capType = EGizmoCapType.Sphere, useGlobalHoveredColor = false };      // Arc-ball style
        [SerializeField] GizmoCapStyle      mViewCapStyle       = new GizmoCapStyle { capType = EGizmoCapType.WireCircle };                                 // View cap which rotates around the camera look vector
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: arcBallStyle (Public Property)
        // Desc: Returns the arc-ball style.
        //-----------------------------------------------------------------------------
        public GizmoCapStyle arcBallStyle       { get { return mArcBallStyle; } }

        //-----------------------------------------------------------------------------
        // Name: viewCapStyle (Public Property)
        // Desc: Returns the view cap style. The view cap is the circle that rotates 
        //       around the camera look vector.
        //-----------------------------------------------------------------------------
        public GizmoCapStyle viewCapStyle       { get { return mViewCapStyle; } }
        #endregion

        #region Public Static Properties
        public static float         defaultArcBallRadius        { get { return 4.0f; } }
        public static Color         defaultArcBallColor         { get { return new Color(0.3f, 0.3f, 0.3f, 0.12f); } }
        public static Color         defaultArcBallColorHovered  { get { return new Color(0.3f, 0.3f, 0.3f, 0.12f); } }
        public static Color         defaultArcBallBorderColor   { get { return Color.white; } }
        public static bool          defaultArcBallBorderVisible { get { return true; } }
        public static EGizmoCapType defaultViewCapType          { get { return EGizmoCapType.WireCircle; } }
        public static float         defaultViewCapRadius        { get { return defaultArcBallRadius + 0.5f; } }
        public static float         defaultViewCapThickness     { get { return 0.1f; } }
        public static Color         defaultViewCapColor         { get { return Color.white; } }
        public static EGizmoCapType defaultAxisCapType          { get { return EGizmoCapType.WireCircle; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: CloneStyle() (Public Function)
        // Desc: Clones the style.
        // Rtrn: The cloned style.
        //-----------------------------------------------------------------------------
        public RotateGizmoStyle CloneStyle()
        {
            var clone               = MemberwiseClone() as RotateGizmoStyle;                  
            clone.mArcBallStyle     = mArcBallStyle.CloneStyle();
            clone.mViewCapStyle     = mViewCapStyle.CloneStyle();

            int count = mAxisCapStyles.Length;
            for (int i = 0; i < count; ++i)
                clone.mAxisCapStyles[i] = mAxisCapStyles[i].CloneStyle();

            // Return clone
            return clone;
        }

        //-----------------------------------------------------------------------------
        // Name: GetAxisCapStyle() (Public Function)
        // Desc: Returns the axis cap style for the specified axis.
        // Parm: axis - Axis index: (0 = X, 1 = Y, 2 = Z).
        // Rtrn: The cap style for the specified axis.
        //-----------------------------------------------------------------------------
        public GizmoCapStyle GetAxisCapStyle(int axis)
        {
            return mAxisCapStyles[axis];
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
            // Axis caps
            const float radiusOffset = 0.05f;
            int count = mAxisCapStyles.Length;
            for (int i = 0; i < count; ++i)
            {
                mAxisCapStyles[i].UseDefaults();
                mAxisCapStyles[i].capType        = defaultAxisCapType;
                mAxisCapStyles[i].circleRadius   = defaultArcBallRadius + radiusOffset;
                mAxisCapStyles[i].cylinderRadius = defaultArcBallRadius + radiusOffset;
                mAxisCapStyles[i].torusRadius    = defaultArcBallRadius + radiusOffset;
            }

            // Arc-ball
            mArcBallStyle.UseDefaults();
            mArcBallStyle.capType               = EGizmoCapType.Sphere;
            mArcBallStyle.sphereRadius          = defaultArcBallRadius;
            mArcBallStyle.color                 = defaultArcBallColor;
            mArcBallStyle.colorHovered          = defaultArcBallColorHovered;
            mArcBallStyle.sphereBorderColor     = defaultArcBallBorderColor;
            mArcBallStyle.sphereBorderVisible   = defaultArcBallBorderVisible;
            mArcBallStyle.useGlobalHoveredColor = false;

            // View cap
            mViewCapStyle.UseDefaults();
            mViewCapStyle.capType               = defaultViewCapType;
            mViewCapStyle.circleRadius          = defaultViewCapRadius;
            mViewCapStyle.insetCircleThickness  = defaultViewCapThickness;
            mViewCapStyle.color                 = defaultViewCapColor;
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
            float           newFloat; 
            Color           newColor; 
            bool            newBool;
            EGizmoCapType   newCapType;

            var content = new GUIContent();
            int axisCapCount = mAxisCapStyles.Length;

            #region XYZ Caps
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Axis Caps");
            {
                // Cap type
                content.text    = "Cap type";
                content.tooltip = "Axis cap type.";
                EditorGUI.BeginChangeCheck();
                newCapType = (EGizmoCapType)EditorGUILayout.EnumPopup(content, mAxisCapStyles[0].capType, (e) => 
                {
                    EGizmoCapType capType = (EGizmoCapType)e;
                    return capType == EGizmoCapType.WireCircle || capType == EGizmoCapType.InsetCylinder || capType == EGizmoCapType.Torus;
                }, false);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    for (int i = 0; i < axisCapCount; ++i)
                        mAxisCapStyles[i].capType = newCapType;
                }

                // Check cap type
                if (newCapType == EGizmoCapType.WireCircle)
                {
                    // Circle radius
                    content.text    = "Radius";
                    content.tooltip = "Axis circle radius.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mAxisCapStyles[0].circleRadius);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        for (int i = 0; i < axisCapCount; ++i)
                            mAxisCapStyles[i].circleRadius = newFloat;
                    }
                }
                else
                if (newCapType == EGizmoCapType.InsetCylinder)
                {
                    // Cylinder radius
                    content.text    = "Radius";
                    content.tooltip = "Axis cylinder radius.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mAxisCapStyles[0].cylinderRadius);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        for (int i = 0; i < axisCapCount; ++i)
                            mAxisCapStyles[i].cylinderRadius = newFloat;
                    }

                    // Cylinder length
                    content.text    = "Length";
                    content.tooltip = "Axis cylinder length.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mAxisCapStyles[0].cylinderLength);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        for (int i = 0; i < axisCapCount; ++i)
                            mAxisCapStyles[i].cylinderLength = newFloat;
                    }

                    // Cylinder thickness
                    content.text    = "Thickness";
                    content.tooltip = "Axis cylinder thickness.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mAxisCapStyles[0].insetCylinderThickness);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        for (int i = 0; i < axisCapCount; ++i)
                            mAxisCapStyles[i].insetCylinderThickness = newFloat;
                    }
                }
                else
                if (newCapType == EGizmoCapType.Torus)
                {
                    // Torus radius
                    content.text    = "Radius";
                    content.tooltip = "The torus radius.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mAxisCapStyles[0].torusRadius);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        for (int i = 0; i < axisCapCount; ++i)
                            mAxisCapStyles[i].torusRadius = newFloat;
                    }

                    // Torus tube radius
                    content.text    = "Tube radius";
                    content.tooltip = "The torus tube radius.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mAxisCapStyles[0].torusTubeRadius);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        for (int i = 0; i < axisCapCount; ++i)
                            mAxisCapStyles[i].torusTubeRadius = newFloat;
                    }
                }

                // Axis caps visibility
                EditorGUILayout.Separator();
                EditorUI.PushLabelWidth(10.0f);
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < axisCapCount; ++i)
                {
                    content.text    = Core.axisNames[i];
                    content.tooltip = "Is the " + Core.axisNames[i] + " cap visible?";
                    EditorGUI.BeginChangeCheck();
                    newBool         = EditorGUILayout.ToggleLeft(content, mAxisCapStyles[i].visible, GUILayout.ExpandWidth(false));
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        mAxisCapStyles[i].visible = newBool;
                    }
                }
                EditorGUILayout.EndHorizontal();
                EditorUI.PopLabelWidth();
            }
            #endregion

            #region Arc-Ball
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Arc-Ball");
            {               
                // Length
                content.text    = "Radius";
                content.tooltip = "Axis-ball radius.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, mArcBallStyle.sphereRadius);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mArcBallStyle.sphereRadius = newFloat;
                }

                // Color
                content.text    = "Color";
                content.tooltip = "Arc-ball color.";
                EditorGUI.BeginChangeCheck();
                newColor        = EditorGUILayout.ColorField(content, mArcBallStyle.color);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mArcBallStyle.color = newColor;
                }

                // Hovered color
                content.text    = "Hovered color";
                content.tooltip = "Arc-ball hovered color.";
                EditorGUI.BeginChangeCheck();
                newColor        = EditorGUILayout.ColorField(content, mArcBallStyle.colorHovered);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mArcBallStyle.colorHovered = newColor;
                }

                // Border color
                content.text    = "Border color";
                content.tooltip = "Arc-ball border color.";
                EditorGUI.BeginChangeCheck();
                newColor        = EditorGUILayout.ColorField(content, mArcBallStyle.sphereBorderColor);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mArcBallStyle.sphereBorderColor = newColor;
                }

                // Arc-ball visibility
                EditorGUILayout.Separator();
                content.text    = "Visible";
                content.tooltip = "Is the arc-ball visible?";
                EditorGUI.BeginChangeCheck();
                newBool = EditorGUILayout.ToggleLeft(content, mArcBallStyle.visible);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mArcBallStyle.visible = newBool;
                }

                // Border visibility
                content.text = "Border";
                content.tooltip = "Is the arc-ball border visible?";
                EditorGUI.BeginChangeCheck();
                newBool = EditorGUILayout.ToggleLeft(content, mArcBallStyle.sphereBorderVisible);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mArcBallStyle.sphereBorderVisible = newBool;
                }
            }
            #endregion

            #region View Cap
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("View Cap");
            {                
                // Cap type
                content.text    = "Cap type";
                content.tooltip = "View cap type.";
                EditorGUI.BeginChangeCheck();
                newCapType = (EGizmoCapType)EditorGUILayout.EnumPopup(content, mViewCapStyle.capType, (e) => 
                {
                    EGizmoCapType capType = (EGizmoCapType)e;
                    return capType == EGizmoCapType.WireCircle || capType == EGizmoCapType.InsetCircle;
                }, false);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mViewCapStyle.capType = newCapType;
                }

                // Circle radius
                content.text    = "Radius";
                content.tooltip = "The view circle radius.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, mViewCapStyle.circleRadius);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mViewCapStyle.circleRadius  = newFloat;
                }

                // Check cap type
                if (newCapType == EGizmoCapType.InsetCircle)
                {
                    // Circle thickness
                    content.text    = "Thickness";
                    content.tooltip = "The view circle thickness.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mViewCapStyle.insetCircleThickness);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        mViewCapStyle.insetCircleThickness  = newFloat;
                    }
                }

                // Color
                content.text    = "Color";
                content.tooltip = "View cap color.";
                EditorGUI.BeginChangeCheck();
                newColor        = EditorGUILayout.ColorField(content, mViewCapStyle.color);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mViewCapStyle.color = newColor;
                }

                // Cap visibility
                EditorGUILayout.Separator();
                content.text    = "Visible";
                content.tooltip = "Is the view cap visible?";
                EditorGUI.BeginChangeCheck();
                newBool = EditorGUILayout.ToggleLeft(content, mViewCapStyle.visible);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mViewCapStyle.visible = newBool;
                }
            }
            #endregion
        }
        #endif
        #endregion
    }
    #endregion
}
