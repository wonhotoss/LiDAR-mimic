using UnityEngine;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: Triangle (Public Struct)
    // Desc: Provides storage for triangle primitives and implements relevant functions.
    //-----------------------------------------------------------------------------
    public struct Triangle
    {
        #region Private Fields
        Vector3 mP0;    // Point p0
        Vector3 mP1;    // Point p1
        Vector3 mP2;    // Point p2
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: p0 (Public Property)
        // Desc: Returns or sets triangle point p0.
        //-----------------------------------------------------------------------------
        public Vector3 p0 { get { return mP0; } set { mP0 = value; } }

        //-----------------------------------------------------------------------------
        // Name: p1 (Public Property)
        // Desc: Returns or sets triangle point p1.
        //-----------------------------------------------------------------------------
        public Vector3 p1 { get { return mP1; } set { mP1 = value; } }

        //-----------------------------------------------------------------------------
        // Name: p2 (Public Property)
        // Desc: Returns or sets triangle point p2.
        //-----------------------------------------------------------------------------
        public Vector3 p2 { get { return mP2; } set { mP2 = value; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Set() (Public Function)
        // Desc: Sets the triangle's points.
        // Parm: p0, p1, p2 - Triangle points.
        //-----------------------------------------------------------------------------
        public void Set(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            mP0 = p0;
            mP1 = p1;
            mP2 = p2;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateAABB() (Public Function)
        // Desc: Calculates the triangle's AABB.
        // Rtrn: The triangle's AABB.
        //-----------------------------------------------------------------------------
        public Box CalculateAABB()
        {
            // Enclose triangle points
            Box aabb = new Box(mP0, Vector3.zero);
            aabb.EnclosePoint(mP1);
            aabb.EnclosePoint(mP2);

            // Return AABB
            return aabb;
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the triangle.
        // Parm: ray - Query ray.
        // Rtrn: True if the ray intersects the triangle and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray)
        {
            return Raycast(ray, out float t);
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the triangle.
        // Parm: ray - Query ray.
        //       t   - Returns the distance from the ray origin where the intersection happens.
        //             If the ray doesn't intersect the triangle, this value should be ignored.
        // Rtrn: True if the ray intersects the triangle and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray, out float t)
        {
            // Calculate triangle plane and check if the ray intersects the plane. If the
            // ray doesn't intersect the triangle plane, it can't possibly intersect the triangle.
            Plane plane = new Plane(mP0, mP1, mP2);
            if (!plane.Raycast(ray, out t))
                return false;

            // Store intersection point
            Vector3 iPt = ray.GetPoint(t);
      
            // The ray intersects the triangle plane. Check if it is inside the triangle.
            // Treat each edge as a plane pointing outward. If the intersection point lies
            // behind all planes, we have a hit.
            // First edge
            Vector3 e = p1 - p0;
            Vector3 n = Vector3.Cross(e, plane.normal).normalized;
            if (Vector3.Dot(iPt - p0, n) > 0) return false;

            // Second edge
            e = p2 - p1;
            n = Vector3.Cross(e, plane.normal).normalized;
            if (Vector3.Dot(iPt - p1, n) > 0) return false;

            // Third edge
            e = p0 - p2;
            n = Vector3.Cross(e, plane.normal).normalized;
            if (Vector3.Dot(iPt - p2, n) > 0) return false;

            // We have a hit
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: ContainsPoint() (Public Function)
        // Desc: Checks if the triangle contains the specified point.
        // Parm: pt - Query point.
        // Rtrn: True if the point lies inside the triangle and false otherwise. The function
        //       doesn't check if the point is on the triangle plane. It checks if the point
        //       lies within the triangle's edge planes.
        //-----------------------------------------------------------------------------
        public bool ContainsPoint(Vector3 pt)
        {
            // Calculate the triangle plane
            Plane plane = new Plane(mP0, mP1, mP2);

            // Treat each edge as a plane pointing outward. If the point lies behind all planes, it means it lies inside the triangle.
            // First edge
            Vector3 e = p1 - p0;
            Vector3 n = Vector3.Cross(e, plane.normal).normalized;
            if (Vector3.Dot(pt - p0, n) > 0) return false;

            // Second edge
            e = p2 - p1;
            n = Vector3.Cross(e, plane.normal).normalized;
            if (Vector3.Dot(pt - p1, n) > 0) return false;

            // Third edge
            e = p0 - p2;
            n = Vector3.Cross(e, plane.normal).normalized;
            if (Vector3.Dot(pt - p2, n) > 0) return false;

            // We have a hit
            return true;
        }
        #endregion
    }
    #endregion
}