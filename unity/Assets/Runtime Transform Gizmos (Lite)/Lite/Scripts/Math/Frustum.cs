using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: Frustum (Public Class)
    // Desc: Provides storage for frustums and implements relevant functions.
    //-----------------------------------------------------------------------------
    public class Frustum
    {
        #region Private Fields
        Plane[] mPlanes     = new Plane[6]; // Frustum planes pointing inwards. Ordering: [0] = Left, [1] = Right, [2] = Down, [3] = Up, [4] = Near, [5] = Far
        bool    mIsValid    = false;        // Is the frustum valid?
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: isValid (Public Property)
        // Desc: Returns true if the frustum is valid and false otherwise.
        //-----------------------------------------------------------------------------
        public bool isValid { get { return mIsValid; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: FromCamera() (Public Function)
        // Desc: Creates the frustum from the specified camera.
        // Parm: camera - The camera used to extract the frustum data.
        //-----------------------------------------------------------------------------
        public void FromCamera(Camera camera)
        {
            // Calculate frustum planes
            GeometryUtility.CalculateFrustumPlanes(camera, mPlanes);

            // Validate frustum
            mIsValid = ValidatePlanes();
        }

        //-----------------------------------------------------------------------------
        // Name: TestSphere() (Public Function)
        // Desc: Determines if the specified sphere intersects or is fully contained
        //       within the frustum.
        // Parm: center - Sphere center.
        //       radius - Sphere radius.
        // Rtrn: True if the specified sphere intersects or is fully contained within
        //       the frustum and false otherwise.
        //-----------------------------------------------------------------------------
        public bool TestSphere(Vector3 center, float radius)
        {
            // Validate call
            if (!isValid)
                return false;

            // If the sphere lies completely behind any of the planes, it can't possibly intersect the frustum.
            if (mPlanes[0].GetDistanceToPoint(center) < -radius) return false;
            if (mPlanes[1].GetDistanceToPoint(center) < -radius) return false;
            if (mPlanes[2].GetDistanceToPoint(center) < -radius) return false;
            if (mPlanes[3].GetDistanceToPoint(center) < -radius) return false;
            if (mPlanes[4].GetDistanceToPoint(center) < -radius) return false;
            if (mPlanes[5].GetDistanceToPoint(center) < -radius) return false;

            // We have an intersection
            return true;
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: ValidatePlanes() (Private Function)
        // Desc: Validates the frustum planes. Called when the planes are updated to ensure
        //       they were constructed from valid input data.
        // Rtrn: True if the planes are valid and false otherwise.
        //-----------------------------------------------------------------------------
        bool ValidatePlanes()
        {
            // Loop through each plane
            for (int i = 0; i < 6; ++i)
            {
                // Validate plane distance
                float d = mPlanes[i].distance;
                if (float.IsInfinity(d) || float.IsNaN(d)) return false;

                // Validate plane normal
                Vector3 normal = mPlanes[i].normal;            
                if (float.IsInfinity(normal.x) || float.IsInfinity(normal.y) || float.IsInfinity(normal.z)) return false;
                if (float.IsNaN(normal.x) || float.IsNaN(normal.y) || float.IsNaN(normal.z)) return false;
            }

            // All planes are valid
            return true;
        }
        #endregion
    }
    #endregion
}
