using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: Vector3Ex (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'Vector3' struct.
    //-----------------------------------------------------------------------------
    public static class Vector3Ex
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name : SyncComponents() (Public Extension)
        // Desc : Syncs all components of 'this' Vector3 with the value of the 
        //        component at the specified axis index.
        // Parm : axis - Source component index (0 = X, 1 = Y, 2 = Z).
        // Rtrn : A new 'Vector3' with the synced components.
        //-----------------------------------------------------------------------------
        public static Vector3 SyncComponents(this Vector3 v, int axis)
        {
            switch (axis)
            {
                case 0: v.y = v.z = v.x; return v;
                case 1: v.x = v.z = v.y; return v;
                case 2: v.x = v.y = v.z; return v;
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
        public static Vector3 FixFloatError(this Vector3 v, float eps = 1e-5f)
        {
            return new Vector3(MathEx.FixFloatError(v.x, eps), MathEx.FixFloatError(v.y, eps), MathEx.FixFloatError(v.z, eps));
        }

        //-----------------------------------------------------------------------------
        // Name: Abs() (Public Extension)
        // Desc: Returns a 3D vector whose components are set to the absolute value of
        //       'this' vector's components.
        // Rtrn: A 3D vector whose components are set to the absolute value of 'this'
        //       vector's components
        //-----------------------------------------------------------------------------
        public static Vector3 Abs(this Vector3 v)
        {
            return new Vector3(MathEx.FastAbs(v.x), MathEx.FastAbs(v.y), MathEx.FastAbs(v.z));
        }

        //-----------------------------------------------------------------------------
        // Name: Inverse() (Public Extension)
        // Desc: Returns the inverse of the vector.
        // Rtrn: The vector inverse: <1.0f / v.x, 1.0f / v.y, 1.0f / v.z).
        //-----------------------------------------------------------------------------
        public static Vector3 Inverse(this Vector3 v)
        {
            return new Vector3(1.0f / v.x, 1.0f / v.y, 1.0f / v.z);
        }
        
        //-----------------------------------------------------------------------------
        // Name: SafeInverse() (Public Extension)
        // Desc: Returns the safe inverse of the vector.
        // Rtrn: The vector inverse: <1.0f / v.x, 1.0f / v.y, 1.0f / v.z>. Components
        //       with a value of 0.0f in 'v' will yield 0.0f in the output.
        //-----------------------------------------------------------------------------
        public static Vector3 SafeInverse(this Vector3 v)
        {
            // Compute safe inverse
            Vector3 inv = v;
            inv.x = (inv.x == 0.0f) ? 0.0f : 1.0f / inv.x;
            inv.y = (inv.y == 0.0f) ? 0.0f : 1.0f / inv.y;
            inv.z = (inv.z == 0.0f) ? 0.0f : 1.0f / inv.z;

            // Return safe inverse
            return inv;
        }

        //-----------------------------------------------------------------------------
        // Name: FuzzyEquals() (Public Extension)
        // Desc: Checks if 'this' vector is equal to 'other'. The check is done per
        //       component using the specified epsilon value.
        // Parm: other - The other vector compared with 'this' vector.
        //       eps   - Epsilon used for comparisons.
        // Rtrn: True if the vectors are equal and false otherwise.
        //-----------------------------------------------------------------------------
        public static bool FuzzyEquals(this Vector3 v, Vector3 other, float eps = 1e-5f)
        {
            // Check if components differ
            if (MathEx.FastAbs(v.x - other.x) > eps) return false; 
            if (MathEx.FastAbs(v.y - other.y) > eps) return false; 
            if (MathEx.FastAbs(v.z - other.z) > eps) return false;

            // Vectors are equal
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: FindClosestPoint() (Public Extension)
        // Desc: Finds the closest point to 'this' point.
        // Parm: points - List of query points. 
        // Rtrn: Index of the closest point to 'v' or -1 if no such point exists.
        //-----------------------------------------------------------------------------
        public static int FindClosestPoint(this Vector3 v, IList<Vector3> points)
        {
            // Keep track of closest point
            float dMin = float.MaxValue;
            int closestPt = -1;

            // Loop through each point
            int count = points.Count;
            for (int i = 0; i < count; ++i)
            {
                // Is this point closer?
                float d = (points[i] - v).magnitude;
                if (d < dMin)
                {
                    dMin = d;
                    closestPt = i;
                }
            }

            // Return point index
            return closestPt;
        }

        //-----------------------------------------------------------------------------
        // Name: Count0() (Public Static Function)
        // Desc: Counts the number of components that have a value of 0.
        // Parm: eps - Epsilon used for comparison with 0.
        // Rtrn: The number of 0 components.
        //-----------------------------------------------------------------------------
        public static int Count0(this Vector3 v, float eps = 1e-5f)
        {
            int count = 0;

            // Count
            if (MathEx.FastAbs(v.x) < eps) ++count;
            if (MathEx.FastAbs(v.y) < eps) ++count;
            if (MathEx.FastAbs(v.z) < eps) ++count;

            // Return count
            return count;
        }

        //-----------------------------------------------------------------------------
        // Name: Replace0() (Public Static Function)
        // Desc: Replaces 0 value components with 'val'.
        // Parm: val - Replacement value.
        //       eps - Epsilon used for comparison with 0.
        // Rtrn: The updated vector where each 0 value component is replaced with 'val'.
        //-----------------------------------------------------------------------------
        public static Vector3 Replace0(this Vector3 v, float val, float eps = 1e-5f)
        {
            // Replace and return new vector
            if (MathEx.FastAbs(v.x) < eps) v.x = val;
            if (MathEx.FastAbs(v.y) < eps) v.y = val;
            if (MathEx.FastAbs(v.z) < eps) v.z = val;
            return v;
        }

        //-----------------------------------------------------------------------------
        // Name: ReplaceInfinity() (Public Static Function)
        // Desc: Replaces infinity value components with 'val'.
        // Parm: val - Replacement value.
        // Rtrn: The updated vector where each infinity value component is replaced with
        //       'val'.
        //-----------------------------------------------------------------------------
        public static Vector3 ReplaceInfinity(this Vector3 v, float val)
        {
            // Replace and return new vector
            if (float.IsInfinity(v.x)) v.x = val;
            if (float.IsInfinity(v.y)) v.y = val;
            if (float.IsInfinity(v.z)) v.z = val;
            return v;
        }

        //-----------------------------------------------------------------------------
        // Name: ReplaceNaN() (Public Static Function)
        // Desc: Replaces NaN value components with 'val'.
        // Parm: val - Replacement value.
        // Rtrn: The updated vector where each NaN value component is replaced with 'val'.
        //-----------------------------------------------------------------------------
        public static Vector3 ReplaceNaN(this Vector3 v, float val)
        {
            // Replace and return new vector
            if (float.IsNaN(v.x)) v.x = val;
            if (float.IsNaN(v.y)) v.y = val;
            if (float.IsNaN(v.z)) v.z = val;
            return v;
        }

        //-----------------------------------------------------------------------------
        // Name: MaxComp() (Public Static Function)
        // Desc: Returns the maximum component.
        // Rtrn: The maximum component.
        //-----------------------------------------------------------------------------
        public static float MaxComp(this Vector3 v)
        {
            // Find max component
            float max = v.x;
            if (max < v.y) max = v.y;
            if (max < v.z) max = v.z;

            // Return max component
            return max;
        }

        //-----------------------------------------------------------------------------
        // Name: MaxAbsComp() (Public Static Function)
        // Desc: Returns the component with the maximum absolute value.
        // Rtrn: The component with the maximum absolute value.
        //-----------------------------------------------------------------------------
        public static float MaxAbsComp(this Vector3 v)
        {
            // Store abs components
            float absX = MathEx.FastAbs(v.x);
            float absY = MathEx.FastAbs(v.y);
            float absZ = MathEx.FastAbs(v.z);

            // Find max component
            float max = absX;
            if (max < absY) max = absY;
            if (max < absZ) max = absZ;

            // Return max component
            return max;
        }

        //-----------------------------------------------------------------------------
        // Name: MaxAbsComp() (Public Static Function)
        // Desc: Returns the component with the maximum absolute value, ignoring the
        //       specified axis.
        // Parm: ignoreAxis - The axis to ignore. For example, if this is set to 1, the
        //                    maximum between the X and Z components is returned. The Y
        //                    component is ignored.
        // Rtrn: The component with the maximum absolute value ignoring the specified axis.
        //-----------------------------------------------------------------------------
        public static float MaxAbsComp(this Vector3 v, int ignoreAxis)
        {
            // Store abs components
            float abs0 = MathEx.FastAbs(v[(ignoreAxis + 1) % 3]);
            float abs1 = MathEx.FastAbs(v[(ignoreAxis + 2) % 3]);

            // Return max component
            return abs0 > abs1 ? abs0 : abs1;
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: FromValue() (Public Static Function)
        // Desc: Returns a 3D vector whose components are equal to 'value'.
        // Parm: value - The returned vector's components will have this value.
        // Rtrn: A 3D vector whose components are all set to 'value'.
        //-----------------------------------------------------------------------------
        public static Vector3 FromValue(float value)
        {
            return new Vector3(value, value, value);
        }

        //-----------------------------------------------------------------------------
        // Name: AbsDot() (Public Static Function)
        // Desc: Returns the absolute value of the dot product between 2 vectors.
        // Parm: v0, v1 - Vectors whose dot product is calculated.
        // Rtrn: The absolute value of the dot product between 'v0' and 'v1'.
        //-----------------------------------------------------------------------------
        public static float AbsDot(Vector3 v0, Vector3 v1)
        {
            return MathEx.FastAbs(Vector3.Dot(v0, v1));
        }
        #endregion
    }
    #endregion
}