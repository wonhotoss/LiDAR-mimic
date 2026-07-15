using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: QuaternionEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'Quaternion' struct.
    //-----------------------------------------------------------------------------
    public static class QuaternionEx
    {
        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: LookRotationEx() (Public Static Function)
        // Desc: Builds a 'Quaternion' that looks in a specified direction and handles
        //       edge cases where forward and up are nearly aligned.
        // Parm: forward - Desired forward direction. The function normalizes this.
        //       up      - Desired up direction (doesn't have to be orthogonal). Doesn't
        //                 have to be normalized.
        // Rtrn: A valid Quaternion, or identity if no valid orientation exists.
        //-----------------------------------------------------------------------------
        public static Quaternion LookRotationEx(Vector3 forward, Vector3 up)
        {
            // Ensure the forward vector is normalized. 'up' doesn't need to be.
            forward = forward.normalized;

            // Generate an up vector that is perpendicular to the forward vector
            up = up - Vector3.Dot(forward, up) * forward;

            // If the 2 vectors were perfectly aligned, the up vector is now 0, so we need to correct
            if (up.magnitude < 1e-5f)
            {
                // Create an up vector that is perpendicular to the forward vector.
                // Note: The X/Y/Z components are picked so that if we performed a
                //       dot product with 'forward', the result would be 0 (i.e. the
                //       vectors are perpendicular).
                up.x =      - forward.y * forward.x;
                up.y = 1.0f - forward.y * forward.y;
                up.z =      - forward.y * forward.z;

                // The above trick can fail if 'forward' is <0, 1, 0>. So validate again.
                if (up.magnitude < 1e-5f)
                {
                    // Try favoring the Z axis
                    up.x =      - forward.z * forward.x;
                    up.y =      - forward.z * forward.y;
                    up.z = 1.0f - forward.z * forward.z;

                    // If it's still not valid, just exit
                    if (up.magnitude < 1e-5f)
                        return Quaternion.identity;
                }
            }

            // Revert to Unity's default implementation now that we have a valid up vector
            return Quaternion.LookRotation(forward, up);
        }
        #endregion
    }
    #endregion
}