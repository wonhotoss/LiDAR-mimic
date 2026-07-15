using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: Matrix4x4Ex (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'Matrix4x4'
    //       struct.
    //-----------------------------------------------------------------------------
    public static class Matrix4x4Ex
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: CalcRelativeTransform() (Public Extension)
        // Desc: Calculates and returns the transform relative to 'refTransform'.
        // Parm: refTransform - The returned matrix encodes a transform relative to 'refTransform'.
        // Rtrn: A transform matrix that encodes a transform that is relative to 'refTransform'.
        //-----------------------------------------------------------------------------
        public static Matrix4x4 CalcRelativeTransform(this Matrix4x4 matrix, Matrix4x4 refTransform)
        {
            return refTransform.inverse * matrix;
        }
        #endregion
    }
    #endregion
}