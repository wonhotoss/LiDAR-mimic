using System;
using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: Context (Public Abstract Class)
    // Desc: Represents a context in which something is allowed to happen. The class
    //       was designed with shortcuts in mind, but it can be used generically if
    //       needed.
    //-----------------------------------------------------------------------------
    [Serializable] public abstract class Context
    {
        #region Private Fields
        [SerializeField] string mName = string.Empty;   // Context name
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: name (Public Property)
        // Desc: Returns the context name.
        //-----------------------------------------------------------------------------
        public string name { get { return mName; } }
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------
        // Name: Context() (Public Constructor)
        // Desc: Creates a context with the specified name.
        // Parm: name - Context name. Must be valid.
        //-----------------------------------------------------------------------------
        public Context(string name)
        {
            // Assign name and validate it
            mName = name;
            if (string.IsNullOrEmpty(mName))
                RTG.Exception(nameof(Context), nameof(Context), "A context must have a valid name.");
        }
        #endregion

        #region Public Abstract Functions
        //-----------------------------------------------------------------------------
        // Name: IsActive() (Public Abstract Function)
        // Desc: Checks if the context is active.
        // Rtrn: True if the context is active and false otherwise.
        //-----------------------------------------------------------------------------
        public abstract bool IsActive();
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: Context_Global (Public Class)
    // Desc: Global context which is always active.
    //-----------------------------------------------------------------------------
    [Serializable] public class Context_Global : Context
    {
        public Context_Global() : base("Global") { }
        public override bool IsActive() { return true; }
    }

    //-----------------------------------------------------------------------------
    // Name: Context_SceneView (Public Class)
    // Desc: Scene view context in which the user can change what happens in the scene
    //       (e.g. camera navigation, grid etc).
    //-----------------------------------------------------------------------------
    [Serializable] public class Context_SceneView : Context
    {
        public Context_SceneView() : base("Scene View") { }
        public override bool IsActive() { return true; }
    }

    //-----------------------------------------------------------------------------
    // Name: Context_SceneView_Navigation (Public Class)
    // Desc: Scene view context in which the user can navigate the scene view with
    //       the camera.
    //-----------------------------------------------------------------------------
    [Serializable] public class Context_SceneView_Navigation : Context
    {
        public Context_SceneView_Navigation() : base("Scene View Navigation") { }
        public override bool IsActive() { return RTGizmos.get.draggedGizmo == null; }
    }

    //-----------------------------------------------------------------------------
    // Name: Context_Camera_FlyMode (Public Class)
    // Desc: Context in which the camera is allowed to fly in different directions.
    //-----------------------------------------------------------------------------
    [Serializable] public class Context_Camera_FlyMode : Context
    {
        public Context_Camera_FlyMode() : base("Camera Fly Mode") { }
        public override bool IsActive() { return RTCamera.get.navigationMode == ECameraNavigationMode.Fly; }
    }

    //-----------------------------------------------------------------------------
    // Name: Context_Grid_ActionMode (Public Class)
    // Desc: Context in which action mode is enabled for the scene grid.
    //-----------------------------------------------------------------------------
    [Serializable] public class Context_Grid_ActionMode : Context
    {
        public Context_Grid_ActionMode() : base("Grid Action Mode") { }
        public override bool IsActive() { return RTGrid.get.actionModeEnabled; }
    }
    #endregion
}
