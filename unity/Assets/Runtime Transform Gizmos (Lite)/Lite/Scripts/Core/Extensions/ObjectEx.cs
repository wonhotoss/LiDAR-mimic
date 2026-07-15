using UnityEditor;
using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ObjectEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'UnityEnginr.Object'
    //       class.
    //-----------------------------------------------------------------------------
    public static class ObjectEx
    {
        #region Public Static Functions
        #if UNITY_EDITOR
        //-----------------------------------------------------------------------------
        // Name: OnWillChangeInEditor() (Public Static Function)
        // Desc: Prepares the object for upcoming changes made inside the Unity Editor
        //       so that Undo/Redo may be used and also marks the object as dirty to
        //       preserve changes between Unity Editor sessions. Useful when working with
        //       'ScriptableObject' assets for example.
        //-----------------------------------------------------------------------------
        public static void OnWillChangeInEditor(this UnityEngine.Object obj)
        {
            UndoEx.Record(obj);
            EditorUtility.SetDirty(obj);
        }
        #endif
        #endregion
    }
    #endregion
}
