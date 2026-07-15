#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: TextureManager (Public Static Class)
    // Desc: Provides shared texture resources used throughout the plugin.
    //-----------------------------------------------------------------------------
    public static class TextureManager
    {
        #region Private Static Fields
        static Texture2D    mWhite;                                 // White texture

        static Texture2D[]  mGizmoAxisLabels = new Texture2D[3];    // Stores a label texture for each of the X, Y and Z gizmo axes
        static Texture2D    mOrthoCamModeIcon;                      // Ortho camera mode icon
        static Texture2D    mPerspectiveCamModeIcon;                // Perspective camera mode icon
        static Texture2D    mGizmoLabelBg;                          // Gizmo label Bg texture
        static Texture2D    mGizmoLabelBgBorder;                    // Gizmo label Bg border texture

        static Texture2D    mWarningIcon;                           // Warning icon
        #endregion

        #region Public Static Properties
        //-----------------------------------------------------------------------------
        // Name: white (Public Static Property)
        // Desc: Returns a white texture.
        //-----------------------------------------------------------------------------
        public static Texture2D white                       { get { if (mWhite == null) mWhite = Texture2DEx.CreateFillTexture(Color.white, 1, 1); return mWhite; } }

        //-----------------------------------------------------------------------------
        // Name: orthoCamModeIcon (Public Static Property)
        // Desc: Returns the icon that indicates that a camera is in ortho mode.
        //-----------------------------------------------------------------------------
        public static Texture2D orthoCamModeIcon            { get { if (mOrthoCamModeIcon == null) mOrthoCamModeIcon = Resources.Load<Texture2D>("Gizmos/Textures/OrthoCameraMode"); return mOrthoCamModeIcon; } }

        //-----------------------------------------------------------------------------
        // Name: perspectiveCamModeIcon (Public Static Property)
        // Desc: Returns the icon that indicates that a camera is in perspective mode.
        //-----------------------------------------------------------------------------
        public static Texture2D perspectiveCamModeIcon      { get { if (mPerspectiveCamModeIcon == null) mPerspectiveCamModeIcon = Resources.Load<Texture2D>("Gizmos/Textures/PerspectiveCameraMode"); return mPerspectiveCamModeIcon; } }

        //-----------------------------------------------------------------------------
        // Name: gizmoLabelBg (Public Static Property)
        // Desc: Returns the texture that is used to draw gizmo label backgrounds.
        //-----------------------------------------------------------------------------
        public static Texture2D gizmoLabelBg                { get { if (mGizmoLabelBg == null) mGizmoLabelBg = Resources.Load<Texture2D>("Gizmos/Textures/GizmoLabelBg"); return mGizmoLabelBg; } }

        //-----------------------------------------------------------------------------
        // Name: gizmoLabelBgBorder (Public Static Property)
        // Desc: Returns the texture that is used to draw gizmo label background borders.
        //-----------------------------------------------------------------------------
        public static Texture2D gizmoLabelBgBorder          { get { if (mGizmoLabelBgBorder == null) mGizmoLabelBgBorder = Resources.Load<Texture2D>("Gizmos/Textures/GizmoLabelBgBorder"); return mGizmoLabelBgBorder; } }

        //-----------------------------------------------------------------------------
        // Name: warningIcon (Public Static Property)
        // Desc: Returns the warning icon texture.
        //-----------------------------------------------------------------------------
        public static Texture2D warningIcon                 { get { if (mWarningIcon == null) mWarningIcon = Resources.Load<Texture2D>("UI/Textures/Warning"); return mWarningIcon; } }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: GetGizmoAxisLabel() (Public Static Function)
        // Desc: Returns a gizmo axis label texture for the specified gizmo axis.
        // Parm: axis - The gizmo axis index: (0 = X, 1 = Y, 2 = Z).
        // Rtrn: The label texture for the specified gizmo axis.
        //-----------------------------------------------------------------------------
        public static Texture2D GetGizmoAxisLabel(int axis)
        {
            // Create the texture if necessary
            if (mGizmoAxisLabels[axis] == null)
            {
                // Check what texture we're dealing with
                switch (axis)
                {
                case 0: mGizmoAxisLabels[0] = Resources.Load<Texture2D>("Gizmos/Textures/XAxisLabel"); break;
                case 1: mGizmoAxisLabels[1] = Resources.Load<Texture2D>("Gizmos/Textures/YAxisLabel"); break;
                case 2: mGizmoAxisLabels[2] = Resources.Load<Texture2D>("Gizmos/Textures/ZAxisLabel"); break;
                }
            }

            // Return texture
            return mGizmoAxisLabels[axis];
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_Reset() (Public Static Function)
        // Desc: Resets all cached texture references.
        //-----------------------------------------------------------------------------
        public static void Internal_Reset()
        {
            // Reset generated textures
            mWhite                  = null;

            // Reset gizmo textures
            mGizmoAxisLabels        = new Texture2D[3];
            mOrthoCamModeIcon       = null;
            mPerspectiveCamModeIcon = null;
            mGizmoLabelBg           = null;
            mGizmoLabelBgBorder     = null;

            // Reset icons
            mWarningIcon            = null;
        }
        #endregion
    }
    #endregion
}