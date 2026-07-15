using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ScreenEx (Public Static Class)
    // Desc: Implements useful extensions and utility functions for the 'Screen' class.
    //-----------------------------------------------------------------------------
    public static class ScreenEx
    {
        #region Public Static Properties
        //-----------------------------------------------------------------------------
        // Name: rect (Public Static Property)
        // Desc: Returns the screen rectangle.
        //-----------------------------------------------------------------------------
        public static Rect rect { get { return new Rect(0.0f, 0.0f, Screen.width, Screen.height); } }
        #endregion
    }
    #endregion
}
