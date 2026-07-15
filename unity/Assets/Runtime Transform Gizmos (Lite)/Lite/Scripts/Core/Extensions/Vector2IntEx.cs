using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: Vector2IntEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'Vector2Int'
    //       struct.
    //-----------------------------------------------------------------------------
    public static class Vector2IntEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: Abs() (Public Extension)
        // Desc: Returns a 'Vector2Int' whose components are set to the absolute value of
        //       'this' vector's components.
        // Rtrn: A 'Vector2Int' whose components are set to the absolute value of 'this'
        //       vector's components
        //-----------------------------------------------------------------------------
        public static Vector2Int Abs(this Vector2Int v)
        {
            return new Vector2Int(MathEx.FastAbs(v.x), MathEx.FastAbs(v.y));
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: FromValue() (Public Static Function)
        // Desc: Returns a 'Vector2Int' whose components are equal to 'value'.
        // Parm: value - The returned vector's components will have this value.
        // Rtrn: A 'Vector2Int' whose components are all set to 'value'.
        //-----------------------------------------------------------------------------
        public static Vector2Int FromValue(int value)
        {
            return new Vector2Int(value, value);
        }
        #endregion
    }
    #endregion
}