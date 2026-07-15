using UnityEngine;

namespace RTGLite
{ 
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: CameraShortcutNames (Public Static Class)
    // Desc: Implements accessors for different camera related shortcut names.
    //-----------------------------------------------------------------------------
    public static class CameraShortcutNames
    {
        #region Public Static Properties
        public static string panMode        { get { return "Pan Mode"; } }
        public static string orbitMode      { get { return "Orbit Mode"; } }
        public static string flyMode        { get { return "Fly Mode"; } }
        public static string zoomMode       { get { return "Zoom Mode"; } }
        public static string flyLeft        { get { return "Fly Left"; } }
        public static string flyRight       { get { return "Fly Right"; } }
        public static string flyDown        { get { return "Fly Down"; } }
        public static string flyUp          { get { return "Fly Up"; } }
        public static string flyBackward    { get { return "Fly Backward"; } }
        public static string flyForward     { get { return "Fly Forward"; } }
        public static string speedModifier  { get { return "Speed Modifier"; } }
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: GridShortcutNames (Public Static Class)
    // Desc: Implements accessors for different grid related shortcut names.
    //-----------------------------------------------------------------------------
    public static class GridShortcutNames
    {
        #region Public Static Properties
        public static string stepMoveDown               { get { return "Step Move Down"; } }
        public static string stepMoveUp                 { get { return "Step Move Up"; } }
        public static string actionMode                 { get { return "Action Mode"; } }
        public static string snapToPickPoint            { get { return "Snap to Pick-Point"; } }
        public static string snapToPickPointExtents     { get { return "Snap to Pick-Point Extents"; } }
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: GizmosShortcutNames (Public Static Class)
    // Desc: Implements accessors for different gizmos related shortcut names.
    //-----------------------------------------------------------------------------
    public static class GizmosShortcutNames
    {
        #region Public Static Properties
        public static string snap       { get { return "Snap"; } }
        public static string vertexSnap { get { return "Vertex Snap"; } }
        public static string altMode    { get { return "Alt Mode"; } }
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: UndoRedoShortcutNames (Public Static Class)
    // Desc: Implements accessors for different undo/redo related shortcut names.
    //-----------------------------------------------------------------------------
    public static class UndoRedoShortcutNames
    {
        #region Public Static Properties
        public static string undo { get { return "Undo"; } }
        public static string redo { get { return "Redo"; } }
        #endregion
    }
    #endregion
}

