using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: PlaneEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'Plane' struct.
    //-----------------------------------------------------------------------------
    public static class PlaneEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: ProjectPoint() (Public Extension)
        // Desc: Projects the specified point on the plane and returns the result.
        // Parm: point - The point to be projected.
        // Rtrn: The projected point.
        //-----------------------------------------------------------------------------
        public static Vector3 ProjectPoint(this Plane plane, Vector3 point) 
        {
            // Start from the point and move along the reverse of the plane normal
            // by a distance equal to the point's distance to the plane
            return point - plane.normal * plane.GetDistanceToPoint(point);
        }

        //-----------------------------------------------------------------------------
        // Name: FindFurthestPointInFront() (Public Extension)
        // Desc: Given a collection of points, returns the index of the furthest point
        //       in front of the plane.
        // Parm: points     - Indexable collection of points.
        //       onPlaneEps - An epsilon value which represents the maximum distance from
        //                    the plane where points are considered to be on the plane.
        // Rtrn: The index of the furthest point in front of the plane or -1 if no such
        //       point exists (e.g. all points are behind or on plane).
        //-----------------------------------------------------------------------------
        public static int FindFurthestPointInFront(this Plane plane, IList<Vector3> points, float onPlaneEps = 1e-5f)
        {       
            int ptIndex         = 0;                // Keeps track of the current point index
            int bestIndex       = -1;               // Keeps track of the index of the furthest point in front of the plane
            float maxDistance   = float.MinValue;   // Keeps track of the maximum distance in front of the plane

            // Loop through each point
            foreach (var pt in points)
            {
                // Get distance to plane
                float d = plane.GetDistanceToPoint(pt);
                if (d > onPlaneEps && d > maxDistance)
                {
                    // We have a new best point
                    maxDistance = d;
                    bestIndex = ptIndex;
                }

                // Next index
                ++ptIndex;
            }

            // Return index
            return bestIndex;
        }

        //-----------------------------------------------------------------------------
        // Name: FindFurthestPointBehind() (Public Extension)
        // Desc: Given a collection of points, returns the index of the furthest point
        //       behind the plane.
        // Parm: points     - Indexable collection of points.
        //       onPlaneEps - An epsilon value which represents the maximum distance from
        //                    the plane where points are considered to be on the plane.
        // Rtrn: The index of the furthest point behind the plane or -1 if no such point
        //       exists (e.g. all points are in front or on plane).
        //-----------------------------------------------------------------------------
        public static int FindFurthestPointBehind(this Plane plane, IList<Vector3> points, float onPlaneEps = 1e-5f)
        {       
            int ptIndex         = 0;                // Keeps track of the current point index
            int bestIndex       = -1;               // Keeps track of the index of the furthest point behind the plane
            float maxDistance   = float.MaxValue;   // Keeps track of the maximum distance behind the plane

            // Loop through each point
            foreach (var pt in points)
            {
                // Get distance to plane
                float d = plane.GetDistanceToPoint(pt);
                if (d < -onPlaneEps && d < maxDistance)
                {
                    // We have a new best point
                    maxDistance = d;
                    bestIndex = ptIndex;
                }

                // Next index
                ++ptIndex;
            }

            // Return index
            return bestIndex;
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: IntersectPlanes() (Public Extension)
        // Desc: Calculates the intersection point between the specified 3 planes and
        //       returns the intersection point.
        // Parm: p0, p1, p2 - The 3 planes whose intersection point must be calculated.
        //       pt         - Returns the intersection point if an intersection exists.
        //                    Otherwise, it should be ignored.
        // Rtrn: True if an intersection exists and false otherwise. If the function returns
        //       false, the value of 'pt' should be ignored.
        // Cred: 'FGED 1' by Eric Lengyel.
        //-----------------------------------------------------------------------------
        public static bool IntersectPlanes(Plane p0, Plane p1, Plane p2, out Vector3 pt)
        {
            // Clear output
            pt = Vector3.zero;

            // Calculate denominator for intersection formula
            Vector3 u = Vector3.Cross(p0.normal, p1.normal);
            float denom = Vector3.Dot(p2.normal, u);
            if (MathEx.FastAbs(denom) < 1e-5f) return false;    // No intersection if absolute value is 0

            // Calculate intersection point
            pt = (Vector3.Cross(p2.normal, p1.normal) * p0.distance + Vector3.Cross(p0.normal, p2.normal) * p1.distance - u * p2.distance) / denom;

            // We have an intersection
            return true;
        }
        #endregion
    }
    #endregion
}