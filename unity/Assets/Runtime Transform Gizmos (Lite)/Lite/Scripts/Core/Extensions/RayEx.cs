using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RayEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'Ray' struct.
    //-----------------------------------------------------------------------------
    public static class RayEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: Transform() (Public Extension)
        // Desc: Transforms the ray with the specified transform matrix.
        // Parm: transformMatrix - The transform matrix.
        // Rtrn: The transformed ray.
        //-----------------------------------------------------------------------------
        public static Ray Transform(this Ray ray, Matrix4x4 transformMatrix)
        {
            return new Ray(transformMatrix.MultiplyPoint(ray.origin), transformMatrix.MultiplyVector(ray.direction).normalized);
        }
        #endregion
    }
    #endregion
}