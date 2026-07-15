using System;
using UnityEditor;
using UnityEngine;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EViewGizmoAlignment (Public Enum)
    // Desc: Defines different view gizmo alignments which can be used to control
    //       where the gizmo appears inside the camera's viewport.
    //-----------------------------------------------------------------------------
    public enum EViewGizmoAlignment
    {
        TopLeft = 0,    // Top left corner of the camera's viewport
        TopRight,       // Top right corner of the camera's viewport
        BottomLeft,     // Bottom left corner of the camera's viewport
        BottomRight     // Bottom right corner of the camera's viewport
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ViewGizmoStyle (Public Class)
    // Desc: Stores style properties for view gizmos.
    //-----------------------------------------------------------------------------
    [Serializable] public class ViewGizmoStyle : GizmoStyle
    {
        #region Private Fields
        [SerializeField] EViewGizmoAlignment    mAlignment              = defaultAlignment;             // Gizmo alignment inside the camera's viewport
        [SerializeField] int                    mScreenSize             = defaultScreenSize;            // The size of the rectangle that displays the gizmo on the screen
        [SerializeField] Vector2                mScreenPadding          = defaultScreenPadding;         // Screen rectangle padding

        [SerializeField] GizmoCapStyle[]        mAxisCapStylesP         = new GizmoCapStyle[3] { new GizmoCapStyle(), new GizmoCapStyle(), new GizmoCapStyle() };       // Positive axis cap styles
        [SerializeField] GizmoCapStyle[]        mAxisCapStylesN         = new GizmoCapStyle[3] { new GizmoCapStyle(), new GizmoCapStyle(), new GizmoCapStyle() };       // Negative axis cap styles
        [SerializeField] GizmoCapStyle          mCenterCapStyle         = new GizmoCapStyle();          // Center cap style. The center cap is the handle used to change the camera projection mode.

        [SerializeField] Color                  mAxisLabelColor         = defaultAxisLabelColor;        // Axis label color
        [SerializeField] Color                  mCamPModeLabelColor     = defaultCamPModeLabelColor;    // Camera projection mode label color
        [SerializeField] bool                   mCamPModeLabelVisible   = defaultCamPModeLabelVisible;  // Is the camera projection mode label visible?
        #endregion

        #region Public Static Properties
        public static EViewGizmoAlignment   defaultAlignment                { get { return EViewGizmoAlignment.TopRight; } }
        public static int                   defaultScreenSize               { get { return 100; } }
        public static Vector2               defaultScreenPadding            { get { return new Vector2(0.0f, 0.0f); } }

        public static EGizmoCapType         defaultCenterCapType            { get { return EGizmoCapType.Box; } }
        public static EGizmoCapType         defaultAxisCapType              { get { return EGizmoCapType.Cone; } }
        public static Color                 defaultNegativeAxisCapColor     { get { return new Color(0.8f, 0.8f, 0.8f, 1.0f); } }
        public static Color                 defaultCenterCapColor           { get { return new Color(0.8f, 0.8f, 0.8f, 1.0f); } }

        public static float                 defaultCenterCapBoxSize         { get { return 0.7f; } }
        public static float                 defaultCenterCapSphereRadius    { get { return 0.5f; } }

        public static Color                 defaultAxisLabelColor           { get { return Color.white; } }
        public static Color                 defaultCamPModeLabelColor       { get { return Color.white; } }
        public static bool                  defaultCamPModeLabelVisible     { get { return true; } }
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: alignment (Public Property)
        // Desc: Returns or sets the gizmo's alignment inside the camera's viewport.
        //-----------------------------------------------------------------------------
        public EViewGizmoAlignment alignment    { get { return mAlignment; } set { mAlignment = value; } }

        //-----------------------------------------------------------------------------
        // Name: screenSize (Public Property)
        // Desc: Returns or sets the gizmo's screen size. This is the size of the rectangle
        //       that displays the gizmo on the screen.
        //-----------------------------------------------------------------------------
        public int      screenSize      { get { return mScreenSize; } set { mScreenSize = Math.Max(32, value); } }

        //-----------------------------------------------------------------------------
        // Name: screenPadding (Public Property)
        // Desc: Returns or sets the gizmo's screen padding.
        //-----------------------------------------------------------------------------
        public Vector2  screenPadding   { get { return mScreenPadding; } set { mScreenPadding = value; } }

        //-----------------------------------------------------------------------------
        // Name: centerCapStyle (Public Property)
        // Desc: Returns the style of the center cap that can be used to perform camera
        //       projection switches.
        //-----------------------------------------------------------------------------
        public GizmoCapStyle    centerCapStyle  { get { return mCenterCapStyle; } }
        
        //-----------------------------------------------------------------------------
        // Name: axisLabelSize (Public Property)
        // Desc: Returns the screen size of the axis labels.
        //-----------------------------------------------------------------------------
        public int              axisLabelSize   { get { return 12; } }

        //-----------------------------------------------------------------------------
        // Name: axisLabelColor (Public Property)
        // Desc: Returns or sets the axis label color.
        //-----------------------------------------------------------------------------
        public Color            axisLabelColor      { get { return mAxisLabelColor; } set { mAxisLabelColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: camPModeLabelColor (Public Property)
        // Desc: Returns or sets the camera projection mode label color.
        //-----------------------------------------------------------------------------
        public Color            camPModeLabelColor      { get { return mCamPModeLabelColor; } set { mCamPModeLabelColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: camPModeLabelVisible (Public Property)
        // Desc: Returns or sets whether or not the camera projection mode label is visible.
        //-----------------------------------------------------------------------------
        public bool             camPModeLabelVisible    { get { return mCamPModeLabelVisible; } set { mCamPModeLabelVisible = value; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: CloneStyle() (Public Function)
        // Desc: Clones the style.
        // Rtrn: The cloned style.
        //-----------------------------------------------------------------------------
        public ViewGizmoStyle CloneStyle()
        {
            var clone = MemberwiseClone() as ViewGizmoStyle;

            // Clone styles
            int count = mAxisCapStylesP.Length;
            for (int i = 0; i < count; ++i)
            {
                clone.mAxisCapStylesP[i] = mAxisCapStylesP[i].CloneStyle();
                clone.mAxisCapStylesN[i] = mAxisCapStylesN[i].CloneStyle();
            }
            clone.mCenterCapStyle = mCenterCapStyle.CloneStyle();

            // Return clone
            return clone;
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
        // Name: GetAxisCapLength() (Public Function)
        // Desc: Returns the length of the axis cap.
        // Rtrn: The length of the axis cap.
        //-----------------------------------------------------------------------------
        public float GetAxisCapLength()
        {
            switch (mAxisCapStylesP[0].capType)
            {
                case EGizmoCapType.Cone:        return mAxisCapStylesP[0].coneLength;
                case EGizmoCapType.WirePyramid:
                case EGizmoCapType.Pyramid:     return mAxisCapStylesP[0].pyramidLength;
                default: return 0.0f;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: GetCenterCapSize() (Public Function)
        // Desc: Returns the size of the center cap that changes the camera projection
        //       mode.
        // Rtrn: The size of the center cap.
        //-----------------------------------------------------------------------------
        public float GetCenterCapSize()
        {
            switch (mCenterCapStyle.capType)
            {
                case EGizmoCapType.Sphere:      return mCenterCapStyle.sphereRadius * 2.0f;
                case EGizmoCapType.WireBox:
                case EGizmoCapType.Box:         return mCenterCapStyle.boxWidth;
                default: return 0.0f;
            }
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
            alignment       = defaultAlignment;
            screenSize      = defaultScreenSize;
            screenPadding   = defaultScreenPadding;

            // Axis caps
            int count = mAxisCapStylesP.Length;
            for (int i = 0; i < count; ++i)
            {
                mAxisCapStylesP[i].UseDefaults();
                mAxisCapStylesN[i].UseDefaults();

                mAxisCapStylesP[i].capType = defaultAxisCapType;
                mAxisCapStylesN[i].capType = defaultAxisCapType;

                mAxisCapStylesN[i].color = defaultNegativeAxisCapColor;
            }

            // Mid cap
            mCenterCapStyle.UseDefaults();
            mCenterCapStyle.capType        = defaultCenterCapType;
            mCenterCapStyle.boxSize        = Vector3Ex.FromValue(defaultCenterCapBoxSize);
            mCenterCapStyle.sphereRadius   = defaultCenterCapSphereRadius;
            mCenterCapStyle.color          = defaultCenterCapColor;

            // Labels
            axisLabelColor              = defaultAxisLabelColor;
            camPModeLabelColor          = defaultCamPModeLabelColor;
            camPModeLabelVisible        = defaultCamPModeLabelVisible;
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
            int             newInt;
            float           newFloat;
            Vector2         newVec2;
            Color           newColor;
            bool            newBool;
            EGizmoCapType   newCapType;

            var content = new GUIContent();
            int axisCapCount = mAxisCapStylesP.Length;

            #region Position & Size
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Position & Size");
            {
                // Alignment
                content.text        = "Alignment";
                content.tooltip     = "Controls the position of the view gizmo inside the camera's viewport.";
                EditorGUI.BeginChangeCheck();
                var newAlignment    = (EViewGizmoAlignment)EditorGUILayout.EnumPopup(content, alignment);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    alignment = newAlignment;
                }

                // Screen size
                content.text        = "Screen size";
                content.tooltip     = "The size of the screen rectangle where the gizmo is displayed.";
                EditorGUI.BeginChangeCheck();
                newInt              = EditorGUILayout.IntField(content, screenSize);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    screenSize = newInt;
                }

                // Screen offset
                content.text        = "Screen padding";
                content.tooltip     = "Screen rectangle padding.";
                EditorGUI.BeginChangeCheck();
                newVec2             = EditorGUILayout.Vector2Field(content, screenPadding);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    screenPadding = newVec2;
                }
            }
            #endregion

            #region Axis Caps
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Axis Caps");
            {
                // Cap type
                content.text    = "Cap type";
                content.tooltip = "Axis cap type.";
                EditorGUI.BeginChangeCheck();
                newCapType = (EGizmoCapType)EditorGUILayout.EnumPopup(content, mAxisCapStylesP[0].capType, (e) => 
                {
                    EGizmoCapType capType = (EGizmoCapType)e;
                    return capType == EGizmoCapType.Pyramid || capType == EGizmoCapType.WirePyramid || capType == EGizmoCapType.Cone;
                }, false);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    for (int i = 0; i < axisCapCount; ++i)
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
                        for (int i = 0; i < axisCapCount; ++i)
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
                        for (int i = 0; i < axisCapCount; ++i)
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
                        for (int i = 0; i < axisCapCount; ++i)
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
                        for (int i = 0; i < axisCapCount; ++i)
                        {
                            mAxisCapStylesP[i].pyramidLength = newFloat;
                            mAxisCapStylesN[i].pyramidLength = newFloat;
                        }
                    }
                } 
                
                // Negative axis cap color
                content.text    = "Negative cap color";
                content.tooltip = "The color of the negative axis caps.";
                EditorGUI.BeginChangeCheck();
                newColor        = EditorGUILayout.ColorField(content, mAxisCapStylesN[0].color);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    for (int i = 0; i < axisCapCount; ++i)
                        mAxisCapStylesN[i].color = newColor;
                }

                // Positive cap visibility
                EditorGUILayout.Separator();
                EditorUI.PushLabelWidth(10.0f);
                EditorGUILayout.BeginHorizontal();
                for (int i = 0; i < axisCapCount; ++i)
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
                for (int i = 0; i < axisCapCount; ++i)
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

            #region Labels
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Labels");
            {
                // Axis label color
                content.text    = "Axis color";
                content.tooltip = "The axis label color.";
                EditorGUI.BeginChangeCheck();
                newColor        = EditorGUILayout.ColorField(content, mAxisLabelColor);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    axisLabelColor = newColor;
                }

                // Camera projection mode label color
                content.text    = "Projection mode";
                content.tooltip = "The camera projection mode label color.";
                EditorGUI.BeginChangeCheck();
                newColor        = EditorGUILayout.ColorField(content, mCamPModeLabelColor);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    camPModeLabelColor = newColor;
                }

                // Camera projection mode label visible
                content.text    = "Projection mode visible";
                content.tooltip = "Is the camera projection mode label visible?";
                EditorGUI.BeginChangeCheck();
                newBool         = EditorGUILayout.ToggleLeft(content, mCamPModeLabelVisible);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    camPModeLabelVisible = newBool;
                }
            }
            #endregion

            #region Mid-Cap
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Mid-Cap");
            {
                // Cap type
                content.text    = "Cap type";
                content.tooltip = "Mid-cap type.";
                EditorGUI.BeginChangeCheck();
                newCapType = (EGizmoCapType)EditorGUILayout.EnumPopup(content, mCenterCapStyle.capType, (e) => 
                {
                    EGizmoCapType capType = (EGizmoCapType)e;
                    return capType == EGizmoCapType.Box || capType == EGizmoCapType.WireBox || capType == EGizmoCapType.Sphere;
                }, false);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mCenterCapStyle.capType = newCapType;
                }

                // Check cap type
                if (newCapType == EGizmoCapType.Box ||
                    newCapType == EGizmoCapType.WireBox)
                {
                    // Box size
                    content.text    = "Size";
                    content.tooltip = "The box size.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mCenterCapStyle.boxWidth);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        mCenterCapStyle.boxSize = Vector3Ex.FromValue(newFloat);
                    }
                }               
                else
                if (newCapType == EGizmoCapType.Sphere)
                {
                    // Sphere radius
                    content.text    = "Radius";
                    content.tooltip = "The sphere radius.";
                    EditorGUI.BeginChangeCheck();
                    newFloat        = EditorGUILayout.FloatField(content, mCenterCapStyle.sphereRadius);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        mCenterCapStyle.sphereRadius = newFloat;
                    }
                }

                // Cap color
                content.text    = "Color";
                content.tooltip = "Mid-cap color.";
                EditorGUI.BeginChangeCheck();
                newColor        = EditorGUILayout.ColorField(content, mCenterCapStyle.color);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mCenterCapStyle.color = newColor;
                }
            }
            #endregion
        }
        #endif
        #endregion
    }
    #endregion
}
