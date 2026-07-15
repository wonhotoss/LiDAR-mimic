using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EGizmoCapType (Public Enum)
    // Desc: Defines different types of gizmo caps.
    //-----------------------------------------------------------------------------
    public enum EGizmoCapType
    {
        Cone = 0,           // Cone cap
        Box,                // Box cap
        Quad,               // Quad cap
        Sphere,             // Sphere cap
        Circle,             // Circle cap
        InsetCircle,        // Inset circle cap
        InsetCylinder,      // Inset cylinder cap
        Torus,              // Torus cap
        Pyramid,            // Pyramid cap

        // Note: Add wire shapes below to make it easy to distinguish between the 2 categories.
        WireBox,            // Wire box cap
        WirePyramid,        // Wire pyramid cap
        WireQuad,           // Wire quad cap
        WireCircle          // Wire circle cap
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: EGizmoCapTypeEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'EGizmoCapType' enum.
    //-----------------------------------------------------------------------------
    public static class EGizmoCapTypeEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: IsWire (Public Enum)
        // Desc: Checks if the cap type belongs to the wire category.
        // Rtrn: True if the cap type belongs to the wire category and false otherwise.
        //-----------------------------------------------------------------------------
        public static bool IsWire(this EGizmoCapType capType)
        {
            return (int)capType >= (int)EGizmoCapType.WireBox;
        }

        //-----------------------------------------------------------------------------
        // Name: IsFlat (Public Enum)
        // Desc: Checks if the cap type represents a flat cap like a circle or quad for
        //       example.
        // Rtrn: True if the cap represents a flat cap like a circle or quad for example.
        //-----------------------------------------------------------------------------
        public static bool IsFlat(this EGizmoCapType capType)
        {
            return capType == EGizmoCapType.Circle || capType == EGizmoCapType.WireCircle ||
                   capType == EGizmoCapType.Quad || capType == EGizmoCapType.WireQuad ||
                   capType == EGizmoCapType.InsetCircle;
        }
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: GizmoCapStyle (Public Class)
    // Desc: Stores style properties for a gizmo cap.
    //-----------------------------------------------------------------------------
    [Serializable] public class GizmoCapStyle : GizmoHandleStyle
    {
        #region Private Fields
        [SerializeField] EGizmoCapType      mCapType                = defaultCapType;                   // Cap type
        [SerializeField] Color              mColor                  = defaultColor;                     // Color
        [SerializeField] Color              mColorHovered           = defaultColorHovered;              // Hovered color when not using the global hovered color
        [SerializeField] Color              mSphereBorderColor      = defaultSphereBorderColor;         // Sphere border color used when the cap is a sphere
        [SerializeField] bool               mUseGlobalHoveredColor  = defaultUseGlobalHoveredColor;     // Use the global hovered color?
        [SerializeField] bool               mSphereBorderVisible    = defaultSphereBorderVisible;       // Is the sphere border visible?
        [SerializeField] float              mConeRadius             = defaultConeRadius;                // Cone cap radius
        [SerializeField] float              mConeLength             = defaultConeLength;                // Cone cap length
        [SerializeField] float              mPyramidBaseSize        = defaultPyramidBaseSize;           // Pyramid base size
        [SerializeField] float              mPyramidLength          = defaultPyramidLength;             // Pyramid length  
        [SerializeField] float              mBoxWidth               = defaultBoxSize;                   // Box cap width
        [SerializeField] float              mBoxHeight              = defaultBoxSize;                   // Box cap height
        [SerializeField] float              mBoxDepth               = defaultBoxSize;                   // Box cap depth
        [SerializeField] float              mQuadWidth              = defaultQuadSize;                  // Quad width
        [SerializeField] float              mQuadHeight             = defaultQuadSize;                  // Quad height
        [SerializeField] float              mSphereRadius           = defaultSphereRadius;              // Sphere cap radius
        [SerializeField] float              mCircleRadius           = defaultCircleRadius;              // Circle radius
        [SerializeField] float              mInsetCircleThickness   = defaultInsetCircleThickness;      // Inset circle thickness
        [SerializeField] float              mCylinderRadius         = defaultCylinderRadius;            // Cylinder radius
        [SerializeField] float              mCylinderLength         = defaultCylinderLength;            // Cylinder length
        [SerializeField] float              mInsetCylinderThickness = defaultInsetCylinderThickness;    // Inset cylinder thickness
        [SerializeField] float              mTorusRadius            = defaultTorusRadius;               // Torus radius
        [SerializeField] float              mTorusTubeRadius        = defaultTorusTubeRadius;           // Torus tube radius
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: capType (Public Property)
        // Desc: Returns or sets the cap type.
        //-----------------------------------------------------------------------------
        public EGizmoCapType   capType  { get { return mCapType; } set { mCapType = value; } } 

        //-----------------------------------------------------------------------------
        // Name: color (Public Property)
        // Desc: Returns or sets the cap color.
        //-----------------------------------------------------------------------------
        public Color color              { get { return mColor; } set { mColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: colorHovered (Public Property)
        // Desc: Returns or sets the cap hovered color. Only used when 'useGlobalHoveredColor'
        //       is false.
        //-----------------------------------------------------------------------------
        public Color colorHovered       { get { return mColorHovered; } set { mColorHovered = value; } }

        //-----------------------------------------------------------------------------
        // Name: sphereBorderColor (Public Property)
        // Desc: Returns or sets the sphere border color used when the cap is a sphere
        //       and 'sphereBorderVisible' is true.
        //-----------------------------------------------------------------------------
        public Color sphereBorderColor  { get { return mSphereBorderColor; } set { mSphereBorderColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: useGlobalHoveredColor (Public Property)
        // Desc: Returns or sets whether or not the cap should use the global hovered
        //       color. When this is false, the 'colorHovered' property can be used to
        //       control the hovered color.
        //-----------------------------------------------------------------------------
        public bool  useGlobalHoveredColor  { get { return mUseGlobalHoveredColor; } set { mUseGlobalHoveredColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: sphereBorderVisible (Public Property)
        // Desc: Returns or sets whether the sphere border is visible. Used when the cap
        //       is a sphere.
        //-----------------------------------------------------------------------------
        public bool  sphereBorderVisible    { get { return mSphereBorderVisible; } set { mSphereBorderVisible = value; } }

        //-----------------------------------------------------------------------------
        // Name: coneRadius (Public Property)
        // Desc: Returns or sets the cone cap radius.
        //-----------------------------------------------------------------------------
        public float coneRadius         { get { return mConeRadius; } set { mConeRadius = Mathf.Max(1e-3f, value); } }

        //-----------------------------------------------------------------------------
        // Name: coneLength (Public Property)
        // Desc: Returns or sets the cone cap length.
        //-----------------------------------------------------------------------------
        public float coneLength         { get { return mConeLength; } set { mConeLength = Mathf.Max(1e-3f, value); } }

        //-----------------------------------------------------------------------------
        // Name: pyramidBaseSize (Public Property)
        // Desc: Returns or sets the pyramid base size.
        //-----------------------------------------------------------------------------
        public float pyramidBaseSize    { get { return mPyramidBaseSize; } set { mPyramidBaseSize = Mathf.Max(1e-3f, value); } }

        //-----------------------------------------------------------------------------
        // Name: pyramidLength (Public Property)
        // Desc: Returns or sets the pyramid length.
        //-----------------------------------------------------------------------------
        public float pyramidLength      { get { return mPyramidLength; } set { mPyramidLength = Mathf.Max(1e-3f, value); } }

        //-----------------------------------------------------------------------------
        // Name: boxWidth (Public Property)
        // Desc: Returns or sets the box cap width.
        //-----------------------------------------------------------------------------
        public float    boxWidth        { get { return mBoxWidth; } set { mBoxWidth = Mathf.Max(1e-3f, value); } }

        //-----------------------------------------------------------------------------
        // Name: boxHeight (Public Property)
        // Desc: Returns or sets the box cap height.
        //-----------------------------------------------------------------------------
        public float    boxHeight       { get { return mBoxHeight; } set { mBoxHeight = Mathf.Max(1e-3f, value); } }

        //-----------------------------------------------------------------------------
        // Name: boxDepth (Public Property)
        // Desc: Returns or sets the box cap depth.
        //-----------------------------------------------------------------------------
        public float    boxDepth        { get { return mBoxDepth; } set { mBoxDepth = Mathf.Max(1e-3f, value); } }
        
        //-----------------------------------------------------------------------------
        // Name: boxSize (Public Property)
        // Desc: Returns or sets the box size as a 3D vector.
        //-----------------------------------------------------------------------------
        public Vector3  boxSize         { get { return new Vector3(mBoxWidth, mBoxHeight, mBoxDepth); } set { boxWidth = value.x; boxHeight = value.y; boxDepth = value.z; } }

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
        // Name: quadSize (Public Property)
        // Desc: Returns or sets the quad size as a 2D vector.
        //-----------------------------------------------------------------------------
        public Vector2  quadSize        { get { return new Vector2(mQuadWidth, mQuadHeight); } set { mQuadWidth = value.x; mQuadHeight = value.y; } }

        //-----------------------------------------------------------------------------
        // Name: sphereRadius (Public Property)
        // Desc: Returns or sets the sphere radius.
        //-----------------------------------------------------------------------------
        public float sphereRadius   { get { return mSphereRadius; } set { mSphereRadius = Mathf.Max(1e-3f, value); } }

        //-----------------------------------------------------------------------------
        // Name: circleRadius (Public Property)
        // Desc: Returns or sets the circle radius.
        //-----------------------------------------------------------------------------
        public float circleRadius   { get { return mCircleRadius; } set { mCircleRadius = Mathf.Max(1e-3f, value); } }

        //-----------------------------------------------------------------------------
        // Name: insetCircleThickness (Public Property)
        // Desc: Returns or sets the inset circle thickness.
        //-----------------------------------------------------------------------------
        public float insetCircleThickness   { get { return mInsetCircleThickness; } set { mInsetCircleThickness = Mathf.Max(1e-3f, value); } }

        //-----------------------------------------------------------------------------
        // Name: cylinderRadius (Public Property)
        // Desc: Returns or sets the cylinder radius.
        //-----------------------------------------------------------------------------
        public float cylinderRadius { get { return mCylinderRadius; } set { mCylinderRadius = Mathf.Max(1e-3f, value); } }

        //-----------------------------------------------------------------------------
        // Name: cylinderLength (Public Property)
        // Desc: Returns or sets the cylinder length.
        //-----------------------------------------------------------------------------
        public float cylinderLength { get { return mCylinderLength; } set { mCylinderLength = Mathf.Max(1e-3f, value); } }

        //-----------------------------------------------------------------------------
        // Name: insetCylinderThickness (Public Property)
        // Desc: Returns or sets the inset cylinder thickness.
        //-----------------------------------------------------------------------------
        public float insetCylinderThickness { get { return mInsetCylinderThickness; } set { mInsetCylinderThickness = Mathf.Max(1e-3f, value); } }
        
        //-----------------------------------------------------------------------------
        // Name: torusRadius (Public Property)
        // Desc: Returns or sets the torus radius. This is the distance from the center
        //       of the torus to the center of the cross section.
        //-----------------------------------------------------------------------------
        public float torusRadius            { get { return mTorusRadius; } set { mTorusRadius = Mathf.Max(1e-3f, value); } }

        //-----------------------------------------------------------------------------
        // Name: torusTubeRadius (Public Property)
        // Desc: Returns or sets the torus tube radius.
        //-----------------------------------------------------------------------------
        public float torusTubeRadius        { get { return mTorusTubeRadius; } set { mTorusTubeRadius = Mathf.Max(1e-3f, value); } }
        #endregion

        #region Public Static Properties
        public static EGizmoCapType     defaultCapType                  { get { return EGizmoCapType.Cone; } }
        public static Color             defaultColor                    { get { return Color.white; } }
        public static Color             defaultColorHovered             { get { return GlobalGizmoStyle.defaultColorHovered; } }
        public static Color             defaultSphereBorderColor        { get { return Color.black; } }
        public static bool              defaultUseGlobalHoveredColor    { get { return true; } }
        public static bool              defaultSphereBorderVisible      { get { return false; } }
        public static float             defaultConeRadius               { get { return 0.35f; } }
        public static float             defaultConeLength               { get { return 1.2f; } }
        public static float             defaultPyramidBaseSize          { get { return 0.7f; } }
        public static float             defaultPyramidLength            { get { return 1.2f; } }
        public static float             defaultBoxSize                  { get { return 0.5f; } }
        public static float             defaultQuadSize                 { get { return 0.5f; } }
        public static float             defaultSphereRadius             { get { return 0.35f; } }
        public static float             defaultCircleRadius             { get { return 0.25f; } }
        public static float             defaultInsetCircleThickness     { get { return 0.1f; } }
        public static float             defaultCylinderRadius           { get { return 0.25f; } }
        public static float             defaultCylinderLength           { get { return 0.5f; } }
        public static float             defaultInsetCylinderThickness   { get { return 0.1f; } }
        public static float             defaultTorusRadius              { get { return 1.0f; } }
        public static float             defaultTorusTubeRadius          { get { return 0.1f; } }
        #endregion

        #region Public Static Functions
        #if UNITY_EDITOR
        //-----------------------------------------------------------------------------
        // Name: DrawNonDirectionalCapUI() (Protected Function)
        // Desc: Draws the cap style UI for non-directional caps. These are the caps that
        //       are used for collider gizmos, lights etc to change the radius, size
        //       height etc.
        // Parm: capStyle      - The cap style whose UI must be drawn.
        //       styleName     - The name of the style. This is displayed as a section label.
        //       capName       - Cap name used for display purposes. Used as a suffix for
        //                       the cap type selection field
        //       drawColorUI   - If true, the cap color UI is drawn to allow the user to
        //                       change the cap's color.
        //       drawVisToggle - If true, the user will be able to toggle the cap's visibility.
        //       parentObject  - The serializable parent object used to record property
        //                       changes.
        //-----------------------------------------------------------------------------
        public static void DrawNonDirectionalCapUI(GizmoCapStyle capStyle, string styleName, string capName, 
            bool drawColorUI, bool drawVisToggle, UnityEngine.Object parentObject)
        {
            EGizmoCapType   newCapType;
            float           newFloat;
            Color           newColor;
            bool            newBool;
            var content = new GUIContent();

            // Draw section label
            if (!string.IsNullOrEmpty(styleName))
                EditorUI.SectionTitleLabel(styleName);

            // Cap type
            bool hasCapName = !string.IsNullOrEmpty(capName);
            content.text    = hasCapName ? capName + " type" : "Cap type";
            content.tooltip = hasCapName ? capName + " type." : "Cap type.";
            EditorGUI.BeginChangeCheck();
            newCapType = (EGizmoCapType)EditorGUILayout.EnumPopup(content, capStyle.capType, (e) =>
            {
                EGizmoCapType capType = (EGizmoCapType)e;
                return capType == EGizmoCapType.Box         || capType == EGizmoCapType.WireBox     || capType == EGizmoCapType.Sphere ||
                        capType == EGizmoCapType.Quad       || capType == EGizmoCapType.InsetCircle ||
                        capType == EGizmoCapType.WireQuad   || capType == EGizmoCapType.Circle      || capType == EGizmoCapType.WireCircle;
            }, false);
            if (EditorGUI.EndChangeCheck())
            {
                parentObject.OnWillChangeInEditor();
                capStyle.capType = newCapType;
            }

            // Check cap type
            if (newCapType == EGizmoCapType.Box ||
                newCapType == EGizmoCapType.WireBox)
            {
                // Box size
                content.text    = "Size";
                content.tooltip = "The box size.";
                EditorGUI.BeginChangeCheck();
                newFloat = EditorGUILayout.FloatField(content, capStyle.boxWidth);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    capStyle.boxSize = Vector3Ex.FromValue(newFloat);
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
                newFloat = EditorGUILayout.FloatField(content, capStyle.quadWidth);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    capStyle.quadSize = Vector2Ex.FromValue(newFloat);
                }
            }
            else
            if (newCapType == EGizmoCapType.Sphere)
            {
                // Sphere radius
                content.text    = "Radius";
                content.tooltip = "The sphere radius.";
                EditorGUI.BeginChangeCheck();
                newFloat = EditorGUILayout.FloatField(content, capStyle.sphereRadius);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    capStyle.sphereRadius = newFloat;
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
                newFloat = EditorGUILayout.FloatField(content, capStyle.circleRadius);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    capStyle.circleRadius = newFloat;
                }

                // Inset circle?
                if (newCapType == EGizmoCapType.InsetCircle)
                {
                    // Circle thickness
                    content.text    = "Thickness";
                    content.tooltip = "The circle thickness.";
                    EditorGUI.BeginChangeCheck();
                    newFloat = EditorGUILayout.FloatField(content, capStyle.insetCircleThickness);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        capStyle.insetCircleThickness = newFloat;
                    }
                }
            }

            if (drawColorUI)
            {
                // Cap color
                content.text    = "Color";
                content.tooltip = "Cap color.";
                EditorGUI.BeginChangeCheck();
                newColor = EditorGUILayout.ColorField(content, capStyle.color);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    capStyle.color = newColor;
                }
            }

            if (drawVisToggle)
            {
                // Visibility
                EditorGUILayout.Separator();
                content.text    = "Visible";
                content.tooltip = "Is the cap visible?";
                EditorGUI.BeginChangeCheck();
                newBool = EditorGUILayout.ToggleLeft(content, capStyle.visible);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    capStyle.visible = newBool;
                }
            }
        }
        #endif
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: CloneStyle() (Public Function)
        // Desc: Clones the style.
        // Rtrn: The cloned style.
        //-----------------------------------------------------------------------------
        public GizmoCapStyle CloneStyle()
        {
            return MemberwiseClone() as GizmoCapStyle;
        }
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: OnUseDefaults() (Protected Function)
        // Desc: Called in order to set the style properties to default values.
        //-----------------------------------------------------------------------------
        protected override void OnUseDefaults()
        {
            capType                 = defaultCapType;
            color                   = defaultColor;
            colorHovered            = defaultColorHovered;
            sphereBorderColor       = defaultSphereBorderColor;
            useGlobalHoveredColor   = defaultUseGlobalHoveredColor;
            sphereBorderVisible     = defaultSphereBorderVisible;
            coneRadius              = defaultConeRadius;
            coneLength              = defaultConeLength;
            pyramidBaseSize         = defaultPyramidBaseSize;
            pyramidLength           = defaultPyramidLength;
            boxWidth                = defaultBoxSize;
            boxHeight               = defaultBoxSize;
            boxDepth                = defaultBoxSize;
            quadWidth               = defaultQuadSize;
            quadHeight              = defaultQuadSize;
            sphereRadius            = defaultSphereRadius;
            circleRadius            = defaultCircleRadius;
            insetCircleThickness    = defaultInsetCircleThickness;
            cylinderRadius          = defaultCylinderRadius;
            cylinderLength          = defaultCylinderLength;
            insetCylinderThickness  = defaultInsetCylinderThickness;
            torusRadius             = defaultTorusRadius;
            torusTubeRadius         = defaultTorusTubeRadius;
        }
        #endregion
    }
    #endregion
}
