using UnityEngine;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: InsetCircle (Public Struct)
    // Desc: Provides storage for circles that have an inset and implements relevant
    //       functions. 
    //-----------------------------------------------------------------------------
    public struct InsetCircle
    {
        #region Private Fields
        Vector3     mCenter;        // Center position
        float       mRadius;        // Radius
        Vector3     mNormal;        // Normal vector
        Vector3     mRadiusAxis;    // The radius axis. Moving from the center of the circle along this
                                    // axis for a distance of 'mRadius' units, we reach a point on the
                                    // circle circumference.
        float       mThickness;     // Thickness
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: center (Public Property)
        // Desc: Returns or sets the circle center.
        //-----------------------------------------------------------------------------
        public Vector3  center      { get { return mCenter; } set { mCenter = value; } }

        //-----------------------------------------------------------------------------
        // Name: radius (Public Property)
        // Desc: Returns or sets the circle radius.
        //-----------------------------------------------------------------------------
        public float    radius      { get { return mRadius; } set { mRadius = Mathf.Max(value, 0.0f); } }

        //-----------------------------------------------------------------------------
        // Name: normal (Public Property)
        // Desc: Returns or sets the circle normal. The setter normalizes the input.
        //-----------------------------------------------------------------------------
        public Vector3  normal      { get { return mNormal; } set { mNormal = value.normalized; } }

        //-----------------------------------------------------------------------------
        // Name: radiusAxis (Public Property)
        // Desc: Returns or sets the radius axis. Moving from the center of the circle
        //       along this axis for a distance of 'radius' units, we reach a point on
        //       the circle circumference. The setter normalizes the input.
        //-----------------------------------------------------------------------------
        public Vector3  radiusAxis  { get { return mRadiusAxis; } set { mRadiusAxis = value.normalized; } }

        //-----------------------------------------------------------------------------
        // Name: upAxis (Public Property)
        // Desc: Calculates and returns the circle's up axis.
        //-----------------------------------------------------------------------------
        public Vector3  upAxis      { get { return Vector3.Cross(mNormal, mRadiusAxis).normalized; } }

        //-----------------------------------------------------------------------------
        // Name: plane (Public Property)
        // Desc: Returns the circle plane.
        //-----------------------------------------------------------------------------
        public Plane    plane       { get { return new Plane(mNormal, mCenter); } }

        //-----------------------------------------------------------------------------
        // Name: thickness (Public Property)
        // Desc: Returns or sets the circle thickness.
        //-----------------------------------------------------------------------------
        public float    thickness   { get { return mThickness; } set { mThickness = Mathf.Max(value, 0.0f); } }

        //-----------------------------------------------------------------------------
        // Name: xyTransform (Public Property)
        // Desc: Calculates and returns a transform matrix that can be used to draw the 
        //       circle as a mesh that defines the circle in the XY plane in model space.
        //-----------------------------------------------------------------------------
        public Matrix4x4 xyTransform { get { return Matrix4x4.TRS(mCenter, Quaternion.LookRotation(normal, upAxis), new Vector3(mRadius, mRadius, 1.0f)); } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Set (Public Function)
        // Desc: Sets the circle data. The function normalizes the input axes.
        // Parm: center     - Circle center.
        //       radius     - Circle radius.
        //       radiusAxis - Circle radius axis. Moving from the center of the circle
        //                    along this axis for a distance of 'radius' units, we reach
        //                    a point on the circle circumference.
        //       normal     - Circle normal.
        //       thickness  - Circle thickness.
        //-----------------------------------------------------------------------------
        public void Set(Vector3 center, float radius, Vector3 radiusAxis, Vector3 normal, float thickness = 0.0f)
        {
            this.center         = center;
            this.radius         = radius;
            this.radiusAxis     = radiusAxis;
            this.normal         = normal;
            this.thickness      = thickness;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateAABB() (Public Function)
        // Desc: Calculates the circle's AABB.
        // Rtrn: The circle's AABB.
        //-----------------------------------------------------------------------------
        public Box CalculateAABB()
        {
            // Cache data
            Vector3 a = upAxis;

            // Enclose extents along circle axes
            Box aabb = new Box(mCenter + mRadiusAxis * mRadius, Vector3.zero);
            aabb.EnclosePoint(mCenter - mRadiusAxis * mRadius);
            aabb.EnclosePoint(mCenter + a * mRadius);
            aabb.EnclosePoint(mCenter - a * mRadius);

            // Return AABB
            return aabb;
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the circle.
        // Parm: ray - Query ray.
        // Rtrn: True if the ray intersects the circle and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray)
        {
            return Raycast(ray, out float t);
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the circle.
        // Parm: ray - Query ray.
        //       t   - Returns the distance from the ray origin where the intersection happens.
        //             If the ray doesn't intersect the circle, this value should be ignored.
        // Rtrn: True if the ray intersects the circle and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray, out float t)
        {
            // Clear output
            t = 0.0f;

            // If the ray doesn't intersect the circle plane, it can't possibly intersect the circle
            if (!plane.Raycast(ray, out t))
                return false;

            // The ray intersects the plane. The intersection point must lie inside the circle, but outside the inset area.
            Vector3 iPt = ray.GetPoint(t);
            float d = (iPt - mCenter).magnitude;
            if (d <= mRadius && d > (mRadius - mThickness))
		        return true;

            // No intersection
            return false;
        }
        #endregion
    }
    #endregion
}
