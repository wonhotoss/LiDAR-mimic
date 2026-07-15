using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: Vector2Ex (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'Vector2' struct.
    //-----------------------------------------------------------------------------
    public static class Vector2Ex
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name : SyncComponents() (Public Extension)
        // Desc : Syncs all components of 'this' Vector2 with the value of the 
        //        component at the specified axis index.
        // Parm : axis - Source component index (0 = X, 1 = Y).
        // Rtrn : A new 'Vector2' with the synced components.
        //-----------------------------------------------------------------------------
        public static Vector2 SyncComponents(this Vector2 v, int axis)
        {
            switch (axis)
            {
                case 0: v.y = v.x; return v;
                case 1: v.x = v.y; return v;
                default: return v;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: FixFloatError() (Public Extension)
        // Desc: Calls 'FixFloatError' for each vector component and returns the updated
        //       vector.
        // Parm: eps - Round epsilon.
        // Rtrn: The updated vector.
        //-----------------------------------------------------------------------------
        public static Vector2 FixFloatError(this Vector2 v, float eps = 1e-5f)
        {
            return new Vector2(MathEx.FixFloatError(v.x, eps), MathEx.FixFloatError(v.y, eps));
        }

        //-----------------------------------------------------------------------------
        // Name: MaxAbsComp() (Public Static Function)
        // Desc: Returns the component with the maximum absolute value.
        // Rtrn: The component with the maximum absolute value.
        //-----------------------------------------------------------------------------
        public static float MaxAbsComp(this Vector2 v)
        {
            // Store abs components
            float absX = MathEx.FastAbs(v.x);
            float absY = MathEx.FastAbs(v.y);

            // Find max component
            float max = absX;
            if (max < absY) max = absY;

            // Return max component
            return max;
        }

        //-----------------------------------------------------------------------------
        // Name: MaxAbsComp() (Public Static Function)
        // Desc: Returns the component with the maximum absolute value, ignoring the
        //       specified axis.
        // Parm: ignoreAxis - The axis to ignore. For example, if this is set to 1, the
        //                    absolute value of X is returned. If set to 0, the absolute
        //                    value of Y is returned.
        // Rtrn: The component with the maximum absolute value ignoring the specified axis.
        //-----------------------------------------------------------------------------
        public static float MaxAbsComp(this Vector2 v, int ignoreAxis)
        {
            // Store abs components
            float abs0 = MathEx.FastAbs(v[(ignoreAxis + 1) % 2]);
            float abs1 = MathEx.FastAbs(v[(ignoreAxis + 2) % 2]);

            // Return max component
            return abs0 > abs1 ? abs0 : abs1;
        }

        //-----------------------------------------------------------------------------
        // Name: Abs() (Public Extension)
        // Desc: Returns a 2D vector whose components are set to the absolute value of
        //       'this' vector's components.
        // Rtrn: A 2D vector whose components are set to the absolute value of 'this'
        //       vector's components
        //-----------------------------------------------------------------------------
        public static Vector2 Abs(this Vector2 v)
        {
            return new Vector2(MathEx.FastAbs(v.x), MathEx.FastAbs(v.y));
        }

        //-----------------------------------------------------------------------------
        // Name: Inverse() (Public Extension)
        // Desc: Returns the inverse of the vector.
        // Rtrn: The vector inverse: <1.0f / v.x, 1.0f / v.y).
        //-----------------------------------------------------------------------------
        public static Vector2 Inverse(this Vector2 v)
        {
            return new Vector2(1.0f / v.x, 1.0f / v.y);
        }

        //-----------------------------------------------------------------------------
        // Name: FuzzyEquals() (Public Extension)
        // Desc: Checks if 'this' vector is equal to 'other'. The check is done per
        //       component using the specified epsilon value.
        // Parm: other - The other vector compared with 'this' vector.
        //       eps   - Epsilon used for comparisons.
        // Rtrn: True if the vectors are equal and false otherwise.
        //-----------------------------------------------------------------------------
        public static bool FuzzyEquals(this Vector2 v, Vector2 other, float eps = 1e-5f)
        {
            // Check if components differ
            if (MathEx.FastAbs(v.x - other.x) > eps) return false; 
            if (MathEx.FastAbs(v.y - other.y) > eps) return false; 

            // Vectors are equal
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: Round() (Public Extension)
        // Desc: Round the X and Y components to the nearest integer.
        // Rtrn: A new vector with rounded X and Y components.
        //-----------------------------------------------------------------------------
        public static Vector2 Round(this Vector2 v)
        {
            return new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: FromValue() (Public Static Function)
        // Desc: Returns a 2D vector whose components are equal to 'value'.
        // Parm: value - The returned vector's components will have this value.
        // Rtrn: A 2D vector whose components are all set to 'value'.
        //-----------------------------------------------------------------------------
        public static Vector2 FromValue(float value)
        {
            return new Vector2(value, value);
        }
        #endregion
    }
    #endregion
}