using UnityEditor;
using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTInputInspector (Public Class)
    // Desc: Implements the Inspector UI for the 'RTInput' Mono.
    //-----------------------------------------------------------------------------
    [CustomEditor(typeof(RTInput))]
    public class RTInputInspector : Editor
    {
        #region Private Fields
        RTInput mTarget;  // Target
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: OnInspectorGUI() (Public Function)
        // Desc: Implements the Inspector UI logic.
        //-----------------------------------------------------------------------------
        public override void OnInspectorGUI() 
        {
            // Notify user about how they can change shortcuts
            EditorGUILayout.HelpBox("You can change shortcuts by clicking on Tools/" + RTG.shortPluginName + "/Windows/Shortcuts... in the top menu or by clicking on the button below.", MessageType.Info);

            // Create the button which opens the shortcut window
            var content     = new GUIContent();
            content.text    = "Shortcuts...";
            content.tooltip = "Opens up the window which allows you to configure shortcuts.";
            if (GUILayout.Button(content, GUILayout.Width(100.0f)))
                PluginEditorWindow.Show<ShortcutManagerWindow>();
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: OnEnable() (Private Function)
        // Desc: Called when the object is enabled.
        //-----------------------------------------------------------------------------
        void OnEnable()
        {
            mTarget = target as RTInput;
        }
        #endregion
    }
    #endregion
}

