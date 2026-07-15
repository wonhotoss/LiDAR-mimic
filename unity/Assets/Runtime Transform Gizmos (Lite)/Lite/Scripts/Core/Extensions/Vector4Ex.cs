using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: Vector4Ex (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'Vector4' struct.
    //-----------------------------------------------------------------------------
    public static class Vector4Ex
    {
        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: FromValue() (Public Static Function)
        // Desc: Returns a 4D vector whose components are equal to 'value'.
        // Parm: value - The returned vector's components will have this value.
        // Rtrn: A 4D vector whose components are all set to 'value'.
        //-----------------------------------------------------------------------------
        public static Vector4 FromValue(float value)
        {
            return new Vector4(value, value, value, value);
        }
        #endregion
    }
    #endregion
}