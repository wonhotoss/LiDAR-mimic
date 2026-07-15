using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ShortcutCategoryNames (Public Static Class)
    // Desc: Implements accessors for different shortcut category names.
    //-----------------------------------------------------------------------------
    public static class ShortcutCategoryNames
    {
        #region Public Static Properties
        public static string camera     { get { return "Camera"; } }
        public static string grid       { get { return "Grid"; } }
        public static string gizmos     { get { return "Gizmos"; } }
        public static string undoRedo   { get { return "Undo/Redo"; } }
        #endregion
    }
    #endregion
}
