using UnityEngine;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: Polygon (Public Struct)
    // Desc: Provides storage for 3D polygons and implements relevant functions.
    //-----------------------------------------------------------------------------
    public struct Polygon
    {
        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: VsPlane (Public Static Function)
        // Desc: Classifies a polygon against the specified plane.
        // Parm: polyPoints - Polygon points.
        //       plane      - Classification plane.
        //       onPlaneEps - An epsilon value which represents the maximum distance from
        //                    the plane where points are considered to be on the plane.
        // Rtrn: A member of 'EPlaneClassify'.
        //-----------------------------------------------------------------------------
        public static EPlaneClassify VsPlane(Vector3[] polyPoints, Plane plane, float onPlaneEps = 1e-5f)
        {
            // Counter variables
            int frontCount      = 0;
            int behindCount     = 0;
            int onPlaneCount    = 0;

            // Loop through each point inside the polygon
            int ptCount = polyPoints.Length;
            for (int i = 0; i < ptCount; ++i)
            {
                // Get the distance between the point and the plane
                float d = plane.GetDistanceToPoint(polyPoints[i]);

                // Increase counters
                if (d < -onPlaneEps)        ++behindCount;
                else if (d > onPlaneEps)    ++frontCount;
                else
                {
                    // Note: We the point is on the plane, we increase all counters. Makes things easier later.
                    ++onPlaneCount;
                    ++behindCount;
                    ++frontCount;
                }
            }

            // Establish the result
            if (frontCount == ptCount)      return EPlaneClassify.InFront;
            if (behindCount == ptCount)     return EPlaneClassify.Behind;
            if (onPlaneCount == ptCount)    return EPlaneClassify.OnPlane;

            // Spanning
            return EPlaneClassify.Spanning;
        }
        #endregion
    }
    #endregion
}
