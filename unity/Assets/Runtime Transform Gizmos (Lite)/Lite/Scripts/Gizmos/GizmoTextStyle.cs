using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EGizmoTextType (Public Enum)
    // Desc: Defines different types of gizmo text.
    //-----------------------------------------------------------------------------
    public enum EGizmoTextType
    {
        DragInfo = 0,   // Text used to display gizmo drag information to the user
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GizmoTextStyle (Public Class)
    // Desc: Stores gizmo text style properties.
    //-----------------------------------------------------------------------------
    [Serializable] public class GizmoTextStyle
    {
        #region Private Fields
        [SerializeField] Color      mColor          = defaultColor;         // Text color
        [SerializeField] Color      mBgColor        = defaultBgColor;       // Text Bg color
        [SerializeField] Color      mBgBorderColor  = defaultBgBorderColor; // Text background border color
        [SerializeField] int        mFontSize       = defaultFontSize;      // Font size
        [SerializeField] FontStyle  mFontStyle      = defaultFontStyle;     // Font style
        [SerializeField] bool       mVisible        = defaultVisible;       // Is the text visible?

        GUIStyle mGUIStyle; // The GUI style which describes this text style
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: color (Public Property)
        // Desc: Returns or sets the text color.
        //-----------------------------------------------------------------------------
        public Color        color           { get { return mColor; } set { mColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: bgColor (Public Property)
        // Desc: Returns or sets the text Bg color.
        //-----------------------------------------------------------------------------
        public Color        bgColor         { get { return mBgColor; } set { mBgColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: bgBorderColor (Public Property)
        // Desc: Returns or sets the text Bg border color.
        //-----------------------------------------------------------------------------
        public Color        bgBorderColor   { get { return mBgBorderColor; } set { mBgBorderColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: fontSize (Public Property)
        // Desc: Returns or sets the font size.
        //-----------------------------------------------------------------------------
        public int          fontSize        { get { return mFontSize; } set { mFontSize = Mathf.Max(6, value); } }

        //-----------------------------------------------------------------------------
        // Name: fontStyle (Public Property)
        // Desc: Returns or sets the font style.
        //-----------------------------------------------------------------------------
        public FontStyle    fontStyle       { get { return mFontStyle; } set { mFontStyle = value; } }
        
        //-----------------------------------------------------------------------------
        // Name: visible (Public Property)
        // Desc: Returns or sets whether the text is visible.
        //-----------------------------------------------------------------------------
        public bool         visible         { get { return mVisible; } set { mVisible = value; } }

        //-----------------------------------------------------------------------------
        // Name: guiStyle (Public Property)
        // Desc: Returns the 'GUIStyle' for this gizmo text style. This style can be
        //       used to render GUI labels inside 'OnGUI'.
        //-----------------------------------------------------------------------------
        public GUIStyle     guiStyle
        {
            get
            {
                // Create the GUI style if necessary and update style properties
                if (mGUIStyle == null) mGUIStyle = new GUIStyle("label");
                mGUIStyle.normal.textColor  = mColor;
                mGUIStyle.active.textColor  = mColor;
                mGUIStyle.hover.textColor   = mColor;
                mGUIStyle.fontSize          = mFontSize;
                mGUIStyle.fontStyle         = mFontStyle;
                mGUIStyle.wordWrap          = false;

                // Return style
                return mGUIStyle;
            }
        }
        #endregion

        #region Public Static Properties
        public static Color     defaultColor            { get { return Color.white; } }
        public static Color     defaultBgColor          { get { return new Color(0.1f, 0.1f, 0.1f, 1.0f); } }
        public static Color     defaultBgBorderColor    { get { return Color.white; } }
        public static int       defaultFontSize         { get { return 12; } }
        public static FontStyle defaultFontStyle        { get { return FontStyle.Normal; } }
        public static bool      defaultVisible          { get { return true; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: UseDefaults() (Public Function)
        // Desc: Use default settings.
        //-----------------------------------------------------------------------------
        public void UseDefaults()
        {
            color           = defaultColor;
            bgColor         = defaultBgColor;
            bgBorderColor   = defaultBgBorderColor;
            fontSize        = defaultFontSize;
            fontStyle       = defaultFontStyle;
            visible         = defaultVisible;
        }

        //-----------------------------------------------------------------------------
        // Name: CloneStyle() (Public Function)
        // Desc: Clones the style.
        // Rtrn: The cloned style.
        //-----------------------------------------------------------------------------
        public GizmoTextStyle CloneStyle()
        {
            return MemberwiseClone() as GizmoTextStyle;
        }

        #if UNITY_EDITOR
        //-----------------------------------------------------------------------------
        // Name: DrawEditorGUI() (Public Function)
        // Desc: Draws the Editor UI which allows the user to change the style properties.
        // Parm: parentObject       - The serializable parent object used to record property
        //                            changes.
        //       styleName          - The name of the style used for display purposes.
        //       useDefaultsButton  - If true, the use defaults button will be rendered.
        //-----------------------------------------------------------------------------
        public void DrawEditorGUI(UnityEngine.Object parentObject, string styleName, bool useDefaultsButton)
        {
            var content = new GUIContent();

            // Create a section label for the style name
            EditorUI.SectionTitleLabel(styleName);

            // Color
            content.text    = "Color";
            content.tooltip = "Text color.";
            EditorGUI.BeginChangeCheck();
            Color newColor  = EditorGUILayout.ColorField(content, color);
            if (EditorGUI.EndChangeCheck())
            {
                parentObject.OnWillChangeInEditor();
                color = newColor;
            }

            // Bg color
            content.text    = "Bg color";
            content.tooltip = "Text Bg color.";
            EditorGUI.BeginChangeCheck();
            newColor        = EditorGUILayout.ColorField(content, bgColor);
            if (EditorGUI.EndChangeCheck())
            {
                parentObject.OnWillChangeInEditor();
                bgColor = newColor;
            }

            // Bg border color
            content.text    = "Bg border color";
            content.tooltip = "Text Bg border color.";
            EditorGUI.BeginChangeCheck();
            newColor        = EditorGUILayout.ColorField(content, bgBorderColor);
            if (EditorGUI.EndChangeCheck())
            {
                parentObject.OnWillChangeInEditor();
                bgBorderColor = newColor;
            }

            // Font size
            content.text    = "Font size";
            content.tooltip = "Text font size.";
            EditorGUI.BeginChangeCheck();
            int newInt      = EditorGUILayout.IntField(content, fontSize);
            if (EditorGUI.EndChangeCheck())
            {
                parentObject.OnWillChangeInEditor();
                fontSize = newInt;
            }

            // Font style
            content.text        = "Font style";
            content.tooltip     = "Text font style.";
            EditorGUI.BeginChangeCheck();
            FontStyle newStyle  = (FontStyle)EditorGUILayout.EnumPopup(content, fontStyle);
            if (EditorGUI.EndChangeCheck())
            {
                parentObject.OnWillChangeInEditor();
                fontStyle = newStyle;
            }

            // Visibility
            content.text        = "Visible";
            content.tooltip     = "Is the text visible?";
            EditorGUI.BeginChangeCheck();
            bool newBool        = EditorGUILayout.ToggleLeft(content, visible);
            if (EditorGUI.EndChangeCheck())
            {
                parentObject.OnWillChangeInEditor();
                visible = newBool;
            }

            // Use defaults button
            if (useDefaultsButton && EditorUI.UseDefaultsButton())
                UseDefaults();
        }
        #endif
        #endregion
    }
    #endregion
}