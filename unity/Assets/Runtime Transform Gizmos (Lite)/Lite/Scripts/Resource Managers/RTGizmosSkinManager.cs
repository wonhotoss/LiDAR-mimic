using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTGizmosSkinManager (Public Singleton Class)
    // Desc: Manages a collection of gizmo skins and implements relevant functions.
    //-----------------------------------------------------------------------------
    public class RTGizmosSkinManager : Singleton<RTGizmosSkinManager>
    {
        #region Private Fields
        RTGizmosSkin mDefaultSkin;      // Default gizmos skin
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: defaultSkin (Public Property)
        // Desc: Returns the default gizmos skin.
        //-----------------------------------------------------------------------------
        public RTGizmosSkin defaultSkin { get { if (mDefaultSkin == null) mDefaultSkin = LoadSkin("DefaultGizmosSkin"); return mDefaultSkin; } }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: LoadSkin() (Private Function)
        // Desc: Loads a gizmos skin with the specified name.
        // Parm: name - Gizmos skin resource name.
        // Rtrn: The loaded skin or null if the skin can't be loaded.
        //-----------------------------------------------------------------------------
        RTGizmosSkin LoadSkin(string name)
        {
            return Resources.Load("Gizmos/Skins/" + name) as RTGizmosSkin;
        }
        #endregion
    }
    #endregion
}
