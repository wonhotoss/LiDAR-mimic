using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: MathEx (Public Static Class)
    // Desc: Implements useful extensions and utility functions for the 'Mathf' class.
    //-----------------------------------------------------------------------------
    public static class MathEx
    {
        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: FixFloatError() (Public Static Function)
        // Desc: Rounds 'value' to the closest integer or rounds its fractional part to
        //       common values such as 0.1, 0.2 etc
        // Parm: value - The value to be rounded.
        //       eps   - Epsilon value used to decide if 'value' or its fractional part
        //               is close enough to the round target (e.g. value = 3.49, eps = 0.01,
        //               the function will return 3.5).
        // Rtrn: Rounded value.
        //-----------------------------------------------------------------------------
        public static float FixFloatError(float value, float eps = 1e-5f)
        {
            // Round to whole
            float iVal  = Mathf.Floor(value);
            if (MathEx.FastAbs(value - iVal) < eps) return iVal;
            float up    = Mathf.Ceil(value);
            if (MathEx.FastAbs(value - up) < eps) return up;

            // Round to 0.75
            float sign  = Mathf.Sign(value);
            float frac  = (value - iVal);
            if (MathEx.FastAbs(frac - 0.75f * sign) < eps) return iVal + 0.75f * sign;
      
            // Round to 0.5
            if (MathEx.FastAbs(frac - 0.5f * sign) < eps) return iVal + 0.5f * sign;
       
            // Round to 0.25
            if (MathEx.FastAbs(frac - 0.25f * sign) < eps) return iVal + 0.25f * sign;

            // Round to 0.1, 0.2, 0.3 etc
            if (MathEx.FastAbs(frac - 0.1f * sign) < eps) return iVal + 0.1f * sign;
            if (MathEx.FastAbs(frac - 0.2f * sign) < eps) return iVal + 0.2f * sign;
            if (MathEx.FastAbs(frac - 0.3f * sign) < eps) return iVal + 0.3f * sign;
            if (MathEx.FastAbs(frac - 0.4f * sign) < eps) return iVal + 0.4f * sign;
            if (MathEx.FastAbs(frac - 0.6f * sign) < eps) return iVal + 0.6f * sign;
            if (MathEx.FastAbs(frac - 0.7f * sign) < eps) return iVal + 0.7f * sign;
            if (MathEx.FastAbs(frac - 0.8f * sign) < eps) return iVal + 0.8f * sign;
            if (MathEx.FastAbs(frac - 0.9f * sign) < eps) return iVal + 0.9f * sign;
            
            return value;
        }

        //-----------------------------------------------------------------------------
        // Name: SafeAcos() (Public Static Function)
        // Desc: Clips 'v' to [-1, 1] range and returns Acos(v).
        //-----------------------------------------------------------------------------
        public static float SafeAcos(float v)
        {
            // Clip
            if (v < -1.0f) v = -1.0f;
            else if (v > 1.0f) v = 1.0f;

            // Return result
            return Mathf.Acos(v);
        }

        //-----------------------------------------------------------------------------
        // Name: FastAbs() (Public Static Function)
        // Desc: Returns the absolute value of 'val' float. Faster than 'Mathf.Abs'.
        //-----------------------------------------------------------------------------
        public static float FastAbs(float val)
        {
            return val < 0.0f ? -val : val;
        }

        //-----------------------------------------------------------------------------
        // Name: FastAbs() (Public Static Function)
        // Desc: Returns the absolute value of 'val' integer. Faster than 'Mathf.Abs'.
        //-----------------------------------------------------------------------------
        public static int FastAbs(int val)
        {
            return val < 0 ? -val : val;
        }

        //-----------------------------------------------------------------------------
        // Name: DegreesToArcLength() (Public Static Function)
        // Desc: Returns the arc length for the specified angle in degrees.
        // Parm: angle  - The angle in degrees.
        //       radius - Circle radius.
        // Rtrn: The arc length.
        //-----------------------------------------------------------------------------
        public static float DegreesToArcLength(float angle, float radius)
        {
            return Mathf.Deg2Rad * angle * radius;
        }

        //-----------------------------------------------------------------------------
        // Name: ArcLengthToDegrees() (Public Static Function)
        // Desc: Returns an angle in degrees for the specified arc length.
        // Parm: arcLength  - The arc length.
        //       radius     - Circle radius.
        // Rtrn: The angle in degrees.
        //-----------------------------------------------------------------------------
        public static float ArcLengthToDegrees(float arcLength, float radius)
        {
            return (arcLength / radius) * Mathf.Rad2Deg;
        }

        //-----------------------------------------------------------------------------
        // Name: SolveQuadratic() (Public Static Function)
        // Desc: Solves the quadratic equation whose coefficients are a, b and c.
        // Parm: a, b, c    - Coefficients.
        //       t0         - Returns the first solution. 
        //       t1         - Returns the second solution.
        // Rtrn: True if the equation has a solution and false otherwise. If false is
        //       returned the values in t0 and t1 should be ignored. If true is returned,
        //       the 2 solutions will always be sorted (t0 <= t1).
        //-----------------------------------------------------------------------------
        public static bool SolveQuadratic(float a, float b, float c, out float t0, out float t1)
        {
            // Reset output values
            t0 = t1 = 0.0f;

            // Calculate delta = b^2 - 4ac
            float delta = b * b - 4.0f * a * c;
            if (delta < 0.0f) return false;     // If delta < 0, the equation doesn't have any real solutions

            // Store denominator used to calculate solutions. If 0, there are no solutions.
            float denom = 2.0f * a;
            if (denom == 0.0f) return false;

            // If delta is 0, simplify (both solutions have the same value)
            if (delta == 0.0f)
            {
                t0 = t1 = -b / denom;
                return true;
            }
            else
            {
                // t = (-b +/- sqrt(delta))/ (2 * a)
                float sqrtDelta = Mathf.Sqrt(delta);
                t0 = (-b + sqrtDelta) / denom;
                t1 = (-b - sqrtDelta) / denom;

                // Order solutions
                if (t0 > t1)
                {
                    float tSwap = t0;
                    t0 = t1;
                    t1 = tSwap;
                }

                // Success!
                return true;
            }
        }
        #endregion
    }
    #endregion
}
