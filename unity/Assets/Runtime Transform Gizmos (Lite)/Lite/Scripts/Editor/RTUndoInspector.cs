using UnityEditor;
using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTUndoInspector (Public Class)
    // Desc: Implements the Inspector UI for the 'RTUndo' Mono.
    //-----------------------------------------------------------------------------
    [CustomEditor(typeof(RTUndo))]
    public class RTUndoInspector : Editor
    {
        #region Private Fields
        RTUndo mTarget;  // Target
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: OnInspectorGUI() (Public Function)
        // Desc: Implements the Inspector UI logic.
        //-----------------------------------------------------------------------------
        public override void OnInspectorGUI() 
        {
            // Stack size
            var content     = new GUIContent();
            content.text    = "Stack size";
            content.tooltip = "Allows you to control the undo stack size. The size represents the maximum number of undo groups that can be pushed onto the stack before old ones are flushed out.";
            EditorGUI.BeginChangeCheck();
            int newInt      = EditorGUILayout.IntField(content, mTarget.stackSize);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.stackSize = newInt;
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
            mTarget = target as RTUndo;
        }
        #endregion
    }
    #endregion
}

