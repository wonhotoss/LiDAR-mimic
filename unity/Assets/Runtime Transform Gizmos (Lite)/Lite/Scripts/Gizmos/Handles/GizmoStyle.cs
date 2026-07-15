using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GizmoStyle (Public Abstract Class)
    // Desc: Stores common style properties for all gizmo style classes and implements
    //       relevant functions.
    //-----------------------------------------------------------------------------
    [Serializable] public abstract class GizmoStyle
    {
        #region Private Fields
        [SerializeField] bool   mVisible    = defaultVisible;   // Visibility toggle
        [SerializeField] bool   mHoverable  = defaultHoverable; // Can handles that use this style be hovered?
        [SerializeField] float  mScale      = defaultScale;     // Scale value
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: visible (Public Property)
        // Desc: Returns or sets the visibility of the entity the style is attached to.
        //-----------------------------------------------------------------------------
        public bool visible     { get { return mVisible; } set { mVisible = value; } }

        //-----------------------------------------------------------------------------
        // Name: hoverable (Public Property)
        // Desc: Returns or sets whether gizmos that use this style can be hovered with
        //       the mouse cursor. When setting this to false, any kind of mouse interaction
        //       is disabled for all gizmos that use this style.
        //-----------------------------------------------------------------------------
        public bool hoverable   { get { return mHoverable; } set { mHoverable = value; } }

        //-----------------------------------------------------------------------------
        // Name: scale (Public Property)
        // Desc: Returns or sets the scale value for all gizmos that use this style.
        //-----------------------------------------------------------------------------
        public float scale      { get { return mScale; } set { mScale = Mathf.Max(value, 1e-2f); } }
        #endregion

        #region Public Static Properties
        public static bool  defaultVisible      { get { return true; } }
        public static bool  defaultHoverable    { get { return true; } }
        public static float defaultScale        { get { return 1.0f; } }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: Create() (Public Static Function)
        // Desc: Creates a style of the specified type. The difference between calling
        //       this function to create a style and allocating with 'new' is that this
        //       function also calls 'UseDefaults' to initialize the style to default
        //       values. This is done as a separate step because some defaults require
        //       loading resources such as textures and this can't be done in the class
        //       constructor. Therefore, you should not call this inside a constructor.
        // Parm: T - Style type. Must derive from 'GizmoStyle'.
        // Rtrn: The created style.
        //-----------------------------------------------------------------------------
        public static T Create<T>() where T : GizmoStyle, new()
        {
            T style = new T();
            style.UseDefaults(null);

            return style;
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: UseDefaults() (Public Function)
        // Desc: Use default settings.
        // Parm: parentObject - The serializable parent object used to record property
        //                      changes. If null, the operation won't support undo/redo.
        //-----------------------------------------------------------------------------
        public void UseDefaults(UnityEngine.Object parentObject)
        {
            #if UNITY_EDITOR
            if (parentObject) parentObject.OnWillChangeInEditor();
            #endif

            visible     = defaultVisible;
            hoverable   = defaultHoverable;
            scale       = defaultScale;

            OnUseDefaults(parentObject);
        }
        #endregion

        #region Public Functions
        #if UNITY_EDITOR
        //-----------------------------------------------------------------------------
        // Name: DrawEditorGUI() (Public Function)
        // Desc: Draws the Editor UI which allows the user to change the style properties.
        // Parm: parentObject       - The serializable parent object used to record property
        //                            changes.
        //       styleName          - The name of the style used for display purposes.
        //       drawDefaultsButton - If true, the use defaults button will be rendered.
        //       drawBaseProperties - If true, the style properties belonging to the
        //                            base class will be drawn in the UI.
        //-----------------------------------------------------------------------------
        public void DrawEditorGUI(UnityEngine.Object parentObject, string styleName, bool drawDefaultsButton, bool drawBaseProperties = true)
        {
            // Create a section label for the style name
            if (!string.IsNullOrEmpty(styleName))
                EditorUI.SectionTitleLabel(styleName);

            // Visibility
            if (drawBaseProperties)
            {
                var content = new GUIContent();
                content.text = "Visible";
                content.tooltip = "Gizmo visibility toggle.";
                EditorGUI.BeginChangeCheck();
                bool newBool = EditorGUILayout.ToggleLeft(content, visible);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    visible = newBool;
                }

                // Hoverable
                content.text    = "Hoverable";
                content.tooltip = "If checked, gizmos that use this style can be hovered by the mouse cursor. If unchecked, " +
                                  "any kind of interaction with the mouse cursor is disabled.";
                EditorGUI.BeginChangeCheck();
                newBool = EditorGUILayout.ToggleLeft(content, hoverable);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    hoverable = newBool;
                }

                // Scale
                content.text    = "Scale";
                content.tooltip = "Scale value for all gizmos that use this style.";
                EditorGUI.BeginChangeCheck();
                float newFloat  = EditorGUILayout.FloatField(content, scale);
                if (EditorGUI.EndChangeCheck())
                {
                    parentObject.OnWillChangeInEditor();
                    scale = newFloat;
                }
            }

            // Draw GUI
            OnEditorGUI(parentObject);

            // Use defaults button
            if (drawDefaultsButton && EditorUI.UseDefaultsButton())
                UseDefaults(parentObject);
        }
        #endif
        #endregion

        #region Protected Abstract Functions
        //-----------------------------------------------------------------------------
        // Name: OnUseDefaults() (Protected Abstract Function)
        // Desc: Called in order to set the style properties to default values.
        // Parm: parentObject - The serializable parent object used to record property
        //                      changes.
        //-----------------------------------------------------------------------------
        protected abstract void OnUseDefaults(UnityEngine.Object parentObject);

        #if UNITY_EDITOR
        //-----------------------------------------------------------------------------
        // Name: OnEditorGUI() (Protected Abstract Function)
        // Desc: Called when the style's GUI must be rendered inside the Unity Editor.
        // Parm: parentObject - The serializable parent object used to record property
        //                      changes.
        //-----------------------------------------------------------------------------
        protected abstract void OnEditorGUI(UnityEngine.Object parentObject);
        #endif
        #endregion
    }
    #endregion
}
