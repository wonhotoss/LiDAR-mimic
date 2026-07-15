using UnityEditor;
using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTGizmosSkinInspector (Public Class)
    // Desc: Implements the Inspector UI for the 'RTGizmosSkin' ScriptableObject.
    //-----------------------------------------------------------------------------
    [CustomEditor(typeof(RTGizmosSkin))]
    public class RTGizmosSkinInspector : Editor
    {
        #region Private Fields
        RTGizmosSkin mTarget;  // Target
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: OnInspectorGUI() (Public Function)
        // Desc: Implements the Inspector UI logic.
        //-----------------------------------------------------------------------------
        public override void OnInspectorGUI() 
        {
            mTarget.OnEditorGUI();
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: OnEnable() (Private Function)
        // Desc: Called when the object is enabled.
        //-----------------------------------------------------------------------------
        void OnEnable()
        {
            mTarget = target as RTGizmosSkin;
        }
        #endregion
    }
    #endregion
}

