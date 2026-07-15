using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GlobalGizmoStyle (Public Class)
    // Desc: Stores global gizmo style properties.
    //-----------------------------------------------------------------------------
    [Serializable] public class GlobalGizmoStyle : GizmoStyle
    {
        #region Private Fields
        [SerializeField] EGizmoShadeMode    mShadeMode                      = defaultShadeMode;                     // Shade mode for solid handles
        [SerializeField] Color[]            mAxisColors                     = new Color[3];                         // XYZ axis colors
        [SerializeField] Color              mColorHovered                   = defaultColorHovered;                  // Hovered color
        [SerializeField] float              mPlaneSliderAlpha               = defaultPlaneSliderAlpha;              // Plane slider alpha
        [SerializeField] float              mPlaneSliderHoveredAlpha        = defaultPlaneSliderHoveredAlpha;       // Plane slider hovered alpha
        [SerializeField] float              mCullAlphaScale                 = defaultCullAlphaScale;                // Alpha scale value used for culled pixels (e.g. rotate gizmo circles culled by the arc-ball)
        [SerializeField] GizmoArcStyle      mRotationArcStyle               = new GizmoArcStyle();                  // Style used to draw rotation arcs

        [SerializeField] Color              mLightColor                     = defaultLightColor;                    // Color used for light gizmos
        [SerializeField] Color              mLightSnapRayColor              = defaultLightSnapRayColor;             // Snap ray color used to snap a light's direction vector to look at the cursor pick point
        [SerializeField] GizmoCapStyle      mLightSnapCapStyle              = new GizmoCapStyle();                  // Light snap cap style
        [SerializeField] float              mLightRangeSnap                 = defaultLightRangeSnap;                // Light range increment used when snapping enabled

        [SerializeField] Color              mColliderColor                  = defaultColliderColor;                 // Color used for collider gizmos
        [SerializeField] float              mColliderSnap                   = defaultColliderSnap;                  // Collider size increment when snapping is enabled

        [SerializeField] Color              mCharacterControllerColor       = defaultCharacterControllerColor;      // Color used for character controller gizmos
        [SerializeField] float              mCharacterControllerSnap        = defaultCharacterControllerSnap;       // Character controller size increment when snapping is enabled

        [SerializeField] float              mAudioSnap                      = defaultAudioSnap;                     // Size increment used for audio gizmos when snapping is enabled

        [SerializeField] GizmoTextStyle     mDragInfoTextStyle              = new GizmoTextStyle();                 // Text style used to display drag information to the user

        [SerializeField] float              mHoverPadding                   = defaultHoverPadding;                  // Hover padding used to inflate the handle primitives when checking if they are hovered by the cursor
        [SerializeField] float              mRotationSensitivity            = defaultRotationSensitivity;           // Mouse rotation sensitivity
        [SerializeField] float              mScaleSensitivity               = defaultScaleSensitivity;              // Mouse scale sensitivity
        [SerializeField] float              mRotationSnap                   = defaultRotationSnap;                  // Rotation increment used when snapping is enabled
        [SerializeField] float              mScaleSnap                      = defaultScaleSnap;                     // Scale increment used when snapping is enabled

        // Buffers used to avoid memory allocations
        List<Vector3>   mVec3Buffer = new List<Vector3>();

        // Used to avoid allocations of GUI content and GUI style instances
        GUIContent      mGUIContent         = new GUIContent();
        GUIStyle        mLabelBorderStyle   = new GUIStyle();
        #endregion

        #region Public Static Properties
        public static EGizmoShadeMode   defaultShadeMode                { get { return EGizmoShadeMode.Lit; } }
        public static Color             defaultXAxisColor               { get { return Core.xAxisColor; } }
        public static Color             defaultYAxisColor               { get { return Core.yAxisColor; } }
        public static Color             defaultZAxisColor               { get { return Core.zAxisColor; } }
        public static Color             defaultColorHovered             { get { return ColorEx.FromRGBBytes(241, 241, 73); } }
        public static float             defaultPlaneSliderAlpha         { get { return 0.2f; } }
        public static float             defaultPlaneSliderHoveredAlpha  { get { return 0.4f; } }
        public static float             defaultCullAlphaScale           { get { return 0.035f; } }

        public static Color             defaultLightColor               { get { return ColorEx.FromRGBBytes(210, 210, 138); } }
        public static Color             defaultLightSnapRayColor        { get { return ColorEx.FromRGBBytes(221, 144, 60); } }
        public static Color             defaultLightSnapCapColor        { get { return Color.magenta; } }
        public static EGizmoCapType     defaultLightSnapCapType         { get { return EGizmoCapType.Quad; } }
        public static float             defaultLightRangeSnap           { get { return 0.1f; } }
        public static float             defaultLightSpotSnap            { get { return 1.0f; } }

        public static Color             defaultColliderColor            { get { return ColorEx.FromRGBBytes(153, 232, 144); } }
        public static float             defaultColliderSnap             { get { return 0.1f; } }

        public static Color             defaultCharacterControllerColor { get { return defaultColliderColor; } }
        public static float             defaultCharacterControllerSnap  { get { return 0.1f; } }

        public static float             defaultAudioSnap                { get { return 0.1f; } }

        public static float             defaultHoverPadding             { get { return 0.4f; } }
        public static float             defaultRotationSensitivity      { get { return 0.45f; } }
        public static float             defaultScaleSensitivity         { get { return 0.3f; } }
        public static float             defaultRotationSnap             { get { return 15.0f; } }
        public static float             defaultScaleSnap                { get { return 0.1f; } }
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: shadeMode (Public Property)
        // Desc: Returns or sets the shade mode used for solid gizmo handles.
        //-----------------------------------------------------------------------------
        public EGizmoShadeMode  shadeMode       { get { return mShadeMode; } set { mShadeMode = value; } }

        //-----------------------------------------------------------------------------
        // Name: colorHovered (Public Property)
        // Desc: Returns or sets the hovered color used by hovered gizmo handles.
        //-----------------------------------------------------------------------------
        public Color    colorHovered            { get { return mColorHovered; } set { mColorHovered = value; } }

        //-----------------------------------------------------------------------------
        // Name: planeSliderAlpha (Public Property)
        // Desc: Returns or sets the plane slider alpha value.
        //-----------------------------------------------------------------------------
        public float    planeSliderAlpha        { get { return mPlaneSliderAlpha; } set { mPlaneSliderAlpha = Mathf.Clamp01(value); } }

        //-----------------------------------------------------------------------------
        // Name: planeSliderHoveredAlpha (Public Property)
        // Desc: Returns or sets the plane slider hovered alpha value.
        //-----------------------------------------------------------------------------
        public float    planeSliderHoveredAlpha { get { return mPlaneSliderHoveredAlpha; } set { mPlaneSliderHoveredAlpha = Mathf.Clamp01(value); } }

        //-----------------------------------------------------------------------------
        // Name: cullAlphaScale (Public Property)
        // Desc: Returns or sets the cull alpha scale. This is the alpha scale value used
        //       for culled pixels (e.g. rotate gizmo circles culled by the arc-ball).
        //-----------------------------------------------------------------------------
        public float            cullAlphaScale      { get { return mCullAlphaScale; } set { mCullAlphaScale = Mathf.Clamp01(value); } }

        //-----------------------------------------------------------------------------
        // Name: rotationArcStyle (Public Property)
        // Desc: Returns the style used to draw rotation arcs.
        //-----------------------------------------------------------------------------
        public GizmoArcStyle    rotationArcStyle    { get { return mRotationArcStyle; } }

        //-----------------------------------------------------------------------------
        // Name: lightColor (Public Property)
        // Desc: Returns or sets the color used for light gizmos.
        //-----------------------------------------------------------------------------
        public Color            lightColor          { get { return mLightColor; } set { mLightColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: lightSnapRayColor (Public Property)
        // Desc: Returns or sets the light snap ray color. This ray is capped by a handle
        //       that can be used to snap a light's direction to look at the cursor
        //       pick point.
        //-----------------------------------------------------------------------------
        public Color            lightSnapRayColor   { get { return mLightSnapRayColor; } set { mLightSnapRayColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: lightSnapCapStyle (Public Property)
        // Desc: Returns the light snap cap style.
        //-----------------------------------------------------------------------------
        public GizmoCapStyle    lightSnapCapStyle   { get { return mLightSnapCapStyle; } set { mLightSnapCapStyle = value; } }

        //-----------------------------------------------------------------------------
        // Name: lightRangeSnap (Public Property)
        // Desc: Returns or sets the light range increment used when snapping is enabled.
        //-----------------------------------------------------------------------------
        public float            lightRangeSnap      { get { return mLightRangeSnap; } set { mLightRangeSnap = Mathf.Max(value, 1e-4f); } }

        //-----------------------------------------------------------------------------
        // Name: colliderColor (Public Property)
        // Desc: Returns or sets the color used for collider gizmos.
        //-----------------------------------------------------------------------------
        public Color            colliderColor       { get { return mColliderColor; } set { mColliderColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: colliderSnap (Public Property)
        // Desc: Returns or sets the collider size increment used when snapping is enabled.
        //-----------------------------------------------------------------------------
        public float            colliderSnap        { get { return mColliderSnap; } set { mColliderSnap = Mathf.Max(value, 1e-4f); } }      

        //-----------------------------------------------------------------------------
        // Name: characterControllerColor (Public Property)
        // Desc: Returns or sets the color used for character controller gizmos.
        //-----------------------------------------------------------------------------
        public Color            characterControllerColor        { get { return mCharacterControllerColor; } set { mCharacterControllerColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: characterControllerSnap (Public Property)
        // Desc: Returns or sets the character controller size increment used when snapping
        //       is enabled.
        //-----------------------------------------------------------------------------
        public float            characterControllerSnap         { get { return mCharacterControllerSnap; } set { mCharacterControllerSnap = Mathf.Max(value, 1e-4f); } }

        //-----------------------------------------------------------------------------
        // Name: audioSnap (Public Property)
        // Desc: Returns or sets the size increment used for audio gizmos when snapping
        //       is enabled.
        //-----------------------------------------------------------------------------
        public float            audioSnap                       { get { return mAudioSnap; } set { mAudioSnap = Mathf.Max(value, 1e-4f); } }

        //-----------------------------------------------------------------------------
        // Name: dragInfoTextStyle (Public Property)
        // Desc: Returns the text style used to display drag information to the user.
        //-----------------------------------------------------------------------------
        public GizmoTextStyle   dragInfoTextStyle   { get { return mDragInfoTextStyle; } }

        //-----------------------------------------------------------------------------
        // Name: hoverPadding (Public Property)
        // Desc: Returns or sets the hover padding used to inflate the handle primitives
        //       when checking if they are hovered by the cursor.
        //-----------------------------------------------------------------------------
        public float hoverPadding           { get { return mHoverPadding; } set { mHoverPadding = Mathf.Max(value, 0.0f); } }

        //-----------------------------------------------------------------------------
        // Name: rotationSensitivity (Public Property)
        // Desc: Returns or sets the mouse rotation sensitivity. This is used in conjunction
        //       with rotation gizmos for example.
        //-----------------------------------------------------------------------------
        public float rotationSensitivity    { get { return mRotationSensitivity; } set { mRotationSensitivity = Mathf.Clamp01(value); } }

        //-----------------------------------------------------------------------------
        // Name: scaleSensitivity (Public Property)
        // Desc: Returns or sets the mouse scale sensitivity. This is used in conjunction
        //       with scale gizmos for example.
        //-----------------------------------------------------------------------------
        public float scaleSensitivity       { get { return mScaleSensitivity; } set { mScaleSensitivity = Mathf.Clamp01(value); } }

        //-----------------------------------------------------------------------------
        // Name: rotationSnap (Public Property)
        // Desc: Returns or sets the rotation increment used when snapping is enabled.
        //-----------------------------------------------------------------------------
        public float rotationSnap           { get { return mRotationSnap; } set { mRotationSnap = Mathf.Max(value, 1e-4f); } }

        //-----------------------------------------------------------------------------
        // Name: scaleSnap (Public Property)
        // Desc: Returns or sets the scale increment used when snapping is enabled.
        //-----------------------------------------------------------------------------
        public float scaleSnap              { get { return mScaleSnap; } set { mScaleSnap = Mathf.Max(value, 1e-4f); } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: CloneStyle() (Public Function)
        // Desc: Clones the style.
        // Rtrn: The cloned style.
        //-----------------------------------------------------------------------------
        public GlobalGizmoStyle CloneStyle()
        {
            // Clone and return
            var clone = MemberwiseClone() as GlobalGizmoStyle;
            clone.mRotationArcStyle  = mRotationArcStyle.CloneStyle();
            clone.mLightSnapCapStyle = mLightSnapCapStyle.CloneStyle();
            clone.mDragInfoTextStyle = mDragInfoTextStyle.CloneStyle();
            return clone;
        }

        //-----------------------------------------------------------------------------
        // Name: GetAxisColor() (Public Function)
        // Desc: Returns the color for the specified axis.
        // Parm: axis - Axis index: (0 = X, 1 = Y, 2 = Z).
        // Rtrn: The axis color.
        //-----------------------------------------------------------------------------
        public Color GetAxisColor(int axis)
        {
            return mAxisColors[axis];
        }

        //-----------------------------------------------------------------------------
        // Name: SetAxisColor() (Public Function)
        // Desc: Sets the color for the specified axis.
        // Parm: axis - Axis index: (0 = X, 1 = Y, 2 = Z).
        // Rtrn: The axis color.
        //-----------------------------------------------------------------------------
        public void SetAxisColor(int axis, Color color)
        {
            mAxisColors[axis] = color;
        }

        //-----------------------------------------------------------------------------
        // Name: GUILabel_HandlesTop() (Public Function)
        // Desc: Draws a text label at the top of the specified handle collection from
        //       the perspective of the specified camera. Must be called from 'OnGUI'.
        // Parm: handles    - The label will be drawn at the top of this handle collection
        //                    from the perspective of 'camera'. Only visible handles are
        //                    taken into account.
        //       camera     - Camera which renders or interacts with the handles.
        //       text       - Label text.
        //       textType   - The gizmo text type.
        // Rtrn: The rectangle that describes the GUI space position and size of the label.
        //-----------------------------------------------------------------------------
        public Rect GUILabel_HandlesTop(List<GizmoHandle> handles, Camera camera, string text, EGizmoTextType textType)
        {
            float yMax      = float.MinValue;
            Box bestAABB    = Box.GetInvalid();

            // Find the AABB with the highest Y extents in screen space
            int count = handles.Count;
            for (int i = 0; i < count; ++i)
            {
                // Is the handle visible?
                if (handles[i].canRender)
                {
                    // Calculate rectangle and check if it's maximum Y is larger than the maximum
                    Box aabb  = handles[i].CalculateAABB(camera);
                    Rect rect = aabb.CalculateScreenRect(camera);
                    if (rect.HasPositiveSize() && rect.yMax > yMax)
                    {
                        yMax = rect.yMax;
                        bestAABB = aabb;
                    }
                }
            }

            // Did we find a good AABB?
            if (!bestAABB.isValid)
                return new Rect();

            // Draw label at the top of the AABB
            return GUILabel_AABBTop(bestAABB, camera, text, textType);
        }

        //-----------------------------------------------------------------------------
        // Name: GUILabel_AABBTop() (Public Function)
        // Desc: Draws a text label at the top of the specified AABB from the perspective
        //       of the specified camera. Must be called from 'OnGUI'.
        // Parm: aabb       - The label will be drawn at the top of this AABB from the perspective
        //                    of 'camera'. If invalid, the function exits immediately.
        //       camera     - Camera which renders or interacts with the aabb.
        //       text       - Label text.
        //       textType   - The gizmo text type.
        // Rtrn: The rectangle that describes the GUI space position and size of the label.
        //-----------------------------------------------------------------------------
        public Rect GUILabel_AABBTop(Box aabb, Camera camera, string text, EGizmoTextType textType)
        {
            // No-op?
            if (!aabb.isValid)
                return new Rect();

            // Calculate the AABB screen rectangle
            Rect screenRect = aabb.CalculateScreenRect(camera);
            if (!screenRect.HasPositiveSize()) return new Rect();

            // Draw label
            return GUILabel(new Vector2(screenRect.center.x, screenRect.yMax), camera, text, textType);
        }

        //-----------------------------------------------------------------------------
        // Name: GUILabel_CircleTop() (Public Function)
        // Desc: Draws a text label at the top of the specified circle from the perspective
        //       of the specified camera. Must be called from 'OnGUI'.
        // Parm: circle     - The label will be drawn at the top of this circle from the
        //                    perspective of 'camera'.
        //       camera     - Camera which interacts with or renders the circle.
        //       text       - Label text.
        //       textType   - The gizmo text type.
        // Rtrn: The rectangle that describes the GUI space position and size of the label.
        //-----------------------------------------------------------------------------
        public Rect GUILabel_CircleTop(Circle circle, Camera camera, string text, EGizmoTextType textType)
        {
            // Project the camera up vector on the circle plane. Then use this vector to calculate
            // the circle top from the perspective of the camera.
            Plane circlePlane   = new Plane(circle.normal, 0.0f);
            Vector3 up          = circlePlane.ProjectPoint(camera.transform.up).normalized;
            Vector3 top         = circle.center + up * circle.radius;

            // Draw label
            return GUILabel_World(top, camera, text, textType);
        }

        //-----------------------------------------------------------------------------
        // Name: GUILabel_World() (Public Function)
        // Desc: Draws a text label at the specified world position. Must be called from
        //       'OnGUI'.
        // Parm: worldPos - The label world position.
        //       camera   - Camera needed to convert the label position to screen space.
        //       text     - Label text.
        //       textType - The gizmo text type.
        // Rtrn: The rectangle that describes the GUI space position and size of the label.
        //-----------------------------------------------------------------------------
        public Rect GUILabel_World(Vector3 worldPos, Camera camera, string text, EGizmoTextType textType)
        {
            // Calculate label position
            Vector3 labelPos    = camera.WorldToScreenPoint(worldPos);
            if (labelPos.z <= 0.0f) return new Rect(); // Behind camera?
 
            // Draw label
            return GUILabel(labelPos, camera, text, textType);
        }

        //-----------------------------------------------------------------------------
        // Name: GUILabel() (Public Function)
        // Desc: Draws a text label at the specified screen space position. Must be called
        //       from 'OnGUI'.
        // Parm: screenPos  - The label screen space position.
        //       camera     - The camera which renders the label.
        //       text       - Label text.
        //       textType   - The gizmo text type.
        // Rtrn: The rectangle that describes the GUI space position and size of the label.
        //-----------------------------------------------------------------------------
        public Rect GUILabel(Vector2 screenPos, Camera camera, string text, EGizmoTextType textType)
        {
            // Get style. If the text is invisible, exit.
            var textStyle = GetGizmoTextStyle(textType);
            if (!textStyle.visible) return new Rect();

            // Convert position to GUI space
            var guiPos = CameraEx.ScreenToGUIPoint(screenPos);

            // Calculate text size
            mGUIContent.text    = text;
            mGUIContent.tooltip = null;
            mGUIContent.image   = null;
            Vector2 textSize    = textStyle.guiStyle.CalcSize(mGUIContent);

            // Init label border style
            mLabelBorderStyle.border.left = 4;
            mLabelBorderStyle.border.top = 4;
            mLabelBorderStyle.border.right = 4;
            mLabelBorderStyle.border.bottom = 4;
            mLabelBorderStyle.normal.background = TextureManager.gizmoLabelBgBorder;

            // Clip to camera viewport
            Rect viewRect = camera.pixelRect.ScreenToGUIRect();
            GUI.BeginClip(viewRect);

            // Draw background and label
            Rect labelRect      = new Rect(guiPos.x - textSize.x / 2.0f, guiPos.y - textSize.y - 5.0f, textSize.x, textSize.y).RelativeTo(viewRect);
            Rect bgBorderRect   = labelRect.Inflate(10.0f);
            GUI.DrawTexture(bgBorderRect, TextureManager.gizmoLabelBg, 
                ScaleMode.StretchToFill, true, 0.0f, 
                textStyle.bgColor, Vector4.zero, 4);                    // Background
            var oldColor = GUI.color;
            GUI.color = textStyle.bgBorderColor;
            GUI.Box(bgBorderRect, string.Empty, mLabelBorderStyle);     // Background border
            GUI.color = oldColor;
            GUI.Label(labelRect, text, textStyle.guiStyle);             // Label

            // End clipping
            GUI.EndClip();

            // Return label rect
            return labelRect;
        }

        //-----------------------------------------------------------------------------
        // Name: GetGizmoTextStyle() (Public Function)
        // Desc: Returns the 'GizmoTextStyle' used to draw gizmo text of the specified
        //       type.
        // Parm: textType - Gizmo text type.
        // Rtrn: A 'GizmoTextStyle' instance that can be used to draw gizmo text of the
        //       specified type.
        //-----------------------------------------------------------------------------
        public GizmoTextStyle GetGizmoTextStyle(EGizmoTextType textType)
        {
            switch (textType)
            {
                case EGizmoTextType.DragInfo: return mDragInfoTextStyle;
                default: return null;
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
            // Colors
            shadeMode                   = defaultShadeMode;
            mAxisColors[0]              = defaultXAxisColor;
            mAxisColors[1]              = defaultYAxisColor;
            mAxisColors[2]              = defaultZAxisColor;
            colorHovered                = defaultColorHovered;
            planeSliderAlpha            = defaultPlaneSliderAlpha;
            planeSliderHoveredAlpha     = defaultPlaneSliderHoveredAlpha;
            cullAlphaScale              = defaultCullAlphaScale;
            rotationArcStyle.UseDefaults();

            // Lights
            lightColor                  = defaultLightColor;
            lightSnapRayColor           = defaultLightSnapRayColor;
            lightRangeSnap              = defaultLightRangeSnap;
            mLightSnapCapStyle.UseDefaults();
            mLightSnapCapStyle.color    = defaultLightSnapCapColor;
            mLightSnapCapStyle.capType  = defaultLightSnapCapType;

            // Colliders
            colliderColor               = defaultColliderColor;
            colliderSnap                = defaultColliderSnap;

            // Character controllers
            characterControllerColor    = defaultCharacterControllerColor;
            characterControllerSnap     = defaultCharacterControllerSnap;

            // Audio
            audioSnap                   = defaultAudioSnap;

            // Text
            dragInfoTextStyle.UseDefaults();

            // Misc
            hoverPadding            = defaultHoverPadding;
            rotationSensitivity     = defaultRotationSensitivity;
            scaleSensitivity        = defaultScaleSensitivity;
            rotationSnap            = defaultRotationSnap;
            scaleSnap               = defaultScaleSnap;
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
            Color newColor; float newFloat;
            EGizmoShadeMode newShadeMode;
            var content = new GUIContent();

            // Colors
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Colors");
            {
                // Shade mode
                content.text    = "Shade mode";
                content.tooltip = "Shade mode used for solid gizmo handles.";
                EditorGUI.BeginChangeCheck();
                newShadeMode    = (EGizmoShadeMode)EditorGUILayout.EnumPopup(content, mShadeMode);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    shadeMode = newShadeMode;
                }

                // Axis colors
                int count = mAxisColors.Length;
                for (int i = 0; i < count; ++i)
                {
                    content.text    = Core.axisNames[i] + " color";
                    content.tooltip = Core.axisNames[i] + " axis color.";
                    EditorGUI.BeginChangeCheck();
                    newColor        = EditorGUILayout.ColorField(content, mAxisColors[i]);
                    if (EditorGUI.EndChangeCheck())
                    {
                        parentObject.OnWillChangeInEditor();
                        mAxisColors[i] = newColor;
                    }
                }

                // Hovered color
                content.text    = "Hovered color";
                content.tooltip = "Color used for gizmo handles which are hovered by the mouse cursor.";
                EditorGUI.BeginChangeCheck();
                newColor        = EditorGUILayout.ColorField(content, colorHovered);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    colorHovered = newColor;
                }

                // Plane slider alpha
                content.text    = "Plane slider alpha";
                content.tooltip = "The alpha value used when drawing plane sliders like the ones the move gizmo uses to move along 2 axes at once.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, planeSliderAlpha);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    planeSliderAlpha = newFloat;
                }

                // Plane slider hovered alpha
                content.text    = "Plane slider hovered alpha";
                content.tooltip = "The alpha value used when drawing hovered plane sliders like the ones the move gizmo uses to move along 2 axes at once.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, planeSliderHoveredAlpha);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    planeSliderHoveredAlpha = newFloat;
                }

                // Cull alpha scale
                content.text    = "Cull alpha scale";
                content.tooltip = "Used to scale the alpha of culled pixels (e.g. rotate gizmo circles culled by the arc-ball).";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, cullAlphaScale);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    cullAlphaScale = newFloat;
                }

                // Rotation arc color
                content.text    = "Rotation arc color";
                content.tooltip = "The color used to draw rotation arcs.";
                EditorGUI.BeginChangeCheck();
                newColor        = EditorGUILayout.ColorField(content, mRotationArcStyle.color);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mRotationArcStyle.color = newColor;
                }

                // Rotation arc border color
                content.text    = "Rotation arc border color";
                content.tooltip = "The color used to draw rotation arc borders.";
                EditorGUI.BeginChangeCheck();
                newColor        = EditorGUILayout.ColorField(content, mRotationArcStyle.borderColor);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    mRotationArcStyle.borderColor = newColor;
                }
            }

            // Lights
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Lights");
            {
                // Light color
                content.text    = "Color";
                content.tooltip = "Color used for light gizmos.";
                EditorGUI.BeginChangeCheck();
                newColor        = EditorGUILayout.ColorField(content, lightColor);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    lightColor = newColor;
                }

                // Snap ray color
                content.text    = "Snap ray color";
                content.tooltip = "The color of the snap ray. This ray is capped by a handle " +
                                  "that can be used to snap a light's direction to look at the cursor pick point.";
                EditorGUI.BeginChangeCheck();
                newColor        = EditorGUILayout.ColorField(content, lightSnapRayColor);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    lightSnapRayColor = newColor;
                }

                // Snap cap
                GizmoCapStyle.DrawNonDirectionalCapUI(mLightSnapCapStyle, string.Empty, "Snap cap", false, false, parentObject);

                // Range snap
                content.text    = "Range snap";
                content.tooltip = "Light range increment used when snapping is enabled.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, lightRangeSnap);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    lightRangeSnap = newFloat;
                }
            }
            
            // Colliders
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Colliders");
            {
                // Color
                content.text    = "Color";
                content.tooltip = "Color used for collider gizmos.";
                EditorGUI.BeginChangeCheck();
                newColor        = EditorGUILayout.ColorField(content, colliderColor);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    colliderColor = newColor;
                }

                // Snap
                content.text    = "Snap";
                content.tooltip = "Collider size increment used when snapping is enabled.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, colliderSnap);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    colliderSnap = newFloat;
                }
            }

            // Character controllers
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Character Controllers");
            {
                // Color
                content.text    = "Color";
                content.tooltip = "Color used for character controller gizmos.";
                EditorGUI.BeginChangeCheck();
                newColor        = EditorGUILayout.ColorField(content, characterControllerColor);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    characterControllerColor = newColor;
                }

                // Snap
                content.text    = "Snap";
                content.tooltip = "Character controller size increment used when snapping is enabled.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, characterControllerSnap);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    characterControllerSnap = newFloat;
                }
            }

            // Audio
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Audio");
            {
                // Snap
                content.text    = "Snap";
                content.tooltip = "Size increment used for audio gizmos when snapping is enabled.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, audioSnap);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    audioSnap = newFloat;
                }
            }

            // Drag info text
            EditorGUILayout.Separator();
            dragInfoTextStyle.DrawEditorGUI(parentObject, "Drag Info Text", false);

            // Misc
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Misc");
            {
                // Hover padding
                content.text    = "Hover padding";
                content.tooltip = "Used to inflate the handle primitives when checking if they are hovered by the cursor.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, hoverPadding);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    hoverPadding = newFloat;
                }

                // Rotation sensitivity
                content.text    = "Rotation sensitivity";
                content.tooltip = "Mouse rotation sensitivity. Used in conjunction with rotation gizmos.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, rotationSensitivity);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    rotationSensitivity = newFloat;
                }

                // Scale sensitivity
                content.text    = "Scale sensitivity";
                content.tooltip = "Mouse scale sensitivity. Used in conjunction with scale gizmos.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, scaleSensitivity);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    scaleSensitivity = newFloat;
                }

                // Rotation snap
                content.text    = "Rotation snap";
                content.tooltip = "Rotation increment used when snapping is enabled.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, rotationSnap);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    rotationSnap = newFloat;
                }

                // Scale snap
                content.text    = "Scale snap";
                content.tooltip = "Scale increment used when snapping is enabled.";
                EditorGUI.BeginChangeCheck();
                newFloat        = EditorGUILayout.FloatField(content, scaleSnap);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    scaleSnap = newFloat;
                }
            }
        }
        #endif
        #endregion

    }
    #endregion
}
