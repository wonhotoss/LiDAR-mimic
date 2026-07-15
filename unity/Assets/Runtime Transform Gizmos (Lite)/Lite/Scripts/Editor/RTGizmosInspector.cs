using UnityEditor;
using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTGizmosInspector (Public Class)
    // Desc: Implements the Inspector UI for the 'RTGizmos' Mono.
    //-----------------------------------------------------------------------------
    [CustomEditor(typeof(RTGizmos))]
    public class RTGizmosInspector : Editor
    {
        #region Private Fields
        RTGizmos mTarget;  // Target
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: OnInspectorGUI() (Public Function)
        // Desc: Implements the Inspector UI logic.
        //-----------------------------------------------------------------------------
        public override void OnInspectorGUI() 
        {
            var content = new GUIContent();

            // Skin
            content.text    = "Skin";
            content.tooltip = "Gizmos skin.";
            EditorGUI.BeginChangeCheck();
            RTGizmosSkin newSkin = EditorGUILayout.ObjectField(content, mTarget.skin, typeof(RTGizmosSkin), false) as RTGizmosSkin;
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.skin = newSkin;
            }

            // Gizmos sorting
            content.text    = "Sort gizmos";
            content.tooltip = "Should the gizmos be sorted when rendered? Due to the fact that gizmos are rendered with the Z-Test disabled this can provide better results in some cases.";
            EditorGUI.BeginChangeCheck();
            bool newBool    = EditorGUILayout.ToggleLeft(content, mTarget.sortGizmos);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.sortGizmos = newBool;
            }
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: OnEnable() (Private Function)
        // Desc: Called when the object is enabled.
        //-----------------------------------------------------------------------------
        void OnEnable()
        {
            mTarget = target as RTGizmos;
        }
        #endregion
    }
    #endregion
}

