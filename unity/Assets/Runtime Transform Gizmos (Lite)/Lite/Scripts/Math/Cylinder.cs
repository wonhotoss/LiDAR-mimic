using UnityEngine;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: Cylinder (Public Struct)
    // Desc: Provides storage for cylinder primitives and implements relevant functions.
    //-----------------------------------------------------------------------------
    public struct Cylinder
    {
        #region Private Fields
        float       mLength;        // Cylinder length (a.k.a height)
        float       mRadius;        // Cylinder radius
        Vector3     mBaseCenter;    // Base center position
        Vector3     mLengthAxis;    // Normalized length axis
        Vector3     mRadiusAxis;    // Normalized radius axis. If we move from the center of the base along this 
                                    // axis for 'mRadius' units, we end up on the cylinder circumference.
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: length (Public Property)
        // Desc: Returns or sets the cylinder length.
        //-----------------------------------------------------------------------------
        public float    length      { get { return mLength; } set { mLength = Mathf.Max(0.0f, value); } }

        //-----------------------------------------------------------------------------
        // Name: radius (Public Property)
        // Desc: Returns or sets the cylinder radius.
        //-----------------------------------------------------------------------------
        public float    radius      { get { return mRadius; } set { mRadius = Mathf.Max(0.0f, value); } }

        //-----------------------------------------------------------------------------
        // Name: baseCenter (Public Property)
        // Desc: Returns or sets the cylinder base center position. The cylinder length
        //       extends out from this position in the direction of 'lengthAxis'.
        //-----------------------------------------------------------------------------
        public Vector3  baseCenter  { get { return mBaseCenter; } set { mBaseCenter = value; } }

        //-----------------------------------------------------------------------------
        // Name: topCenter (Public Property)
        // Desc: Returns the cylinder top center position.
        //-----------------------------------------------------------------------------
        public Vector3  topCenter   { get { return mBaseCenter + mLengthAxis * mLength; } }
        
        //-----------------------------------------------------------------------------
        // Name: lengthAxis (Public Property)
        // Desc: Returns or sets the cylinder length axis. The setter normalizes the input.
        //-----------------------------------------------------------------------------
        public Vector3  lengthAxis  { get { return mLengthAxis; } set { mLengthAxis = value.normalized; } }

        //-----------------------------------------------------------------------------
        // Name: radiusAxis (Public Property)
        // Desc: Returns or sets the cylinder radius axis. The setter normalizes the input.
        //-----------------------------------------------------------------------------
        public Vector3  radiusAxis  { get { return mRadiusAxis; } set { mRadiusAxis = value.normalized; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Set (Public Function)
        // Desc: Sets the cylinder data. The function normalizes the input axes.
        // Parm: baseCenter - Cylinder base center.
        //       radius     - Cylinder radius.
        //       length     - Cylinder length.
        //       radiusAxis - Cylinder radius axis. If we move from the center of the base
        //                    along this axis for 'radius' units, we end up on the cylinder
        //                    circumference.
        //       lengthAxis - Cylinder length axis.
        //-----------------------------------------------------------------------------
        public void Set(Vector3 baseCenter, float radius, float length, Vector3 radiusAxis, Vector3 lengthAxis)
        {
            this.baseCenter = baseCenter;
            this.radius     = radius;
            this.length     = length;
            this.radiusAxis = radiusAxis;
            this.lengthAxis = lengthAxis;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateAABB() (Public Function)
        // Desc: Calculates the cylinder's AABB.
        // Rtrn: The cylinder's AABB.
        //-----------------------------------------------------------------------------
        public Box CalculateAABB()
        {
            // Enclose base and top centers
            Box aabb = new Box(mBaseCenter, Vector3.zero);
            Vector3 topCenterPos = topCenter;
            aabb.EnclosePoint(topCenterPos);

            // We also need to generate the cap extent points. For this, we need to generate
            // 2 perpendicular axes that exist in the cap planes. If the length axis is aligned
            // with the world Y axis, we can just use the world X and Z axes. If not, we will
            // use the cross product to generate the 2 axes.
            Vector3 right   = Vector3.right;
            Vector3 forward = Vector3.forward;
            if (Vector3Ex.AbsDot(Vector3.up, mLengthAxis) > 1e-5f)
            {
                // The length axis is not aligned with the world Y axis. We need to use
                // the cross product to generate the axes.
                right   = Vector3.Cross(mLengthAxis, Vector3.up).normalized;
                forward = Vector3.Cross(right, mLengthAxis).normalized;
            }

            // Enclose cap points
            aabb.EnclosePoint(mBaseCenter + right   * mRadius);
            aabb.EnclosePoint(mBaseCenter - right   * mRadius);
            aabb.EnclosePoint(mBaseCenter + forward * mRadius);
            aabb.EnclosePoint(mBaseCenter - forward * mRadius);
            aabb.EnclosePoint(topCenterPos + right   * mRadius);
            aabb.EnclosePoint(topCenterPos - right   * mRadius);
            aabb.EnclosePoint(topCenterPos + forward * mRadius);
            aabb.EnclosePoint(topCenterPos - forward * mRadius);

            // Return AABB
            return aabb;
        }

        //-----------------------------------------------------------------------------
        // Name: Inflate() (Public Function)
        // Desc: Inflates the cylinder by the specified amount.
        // Parm: amount - The amount to inflate. If negative, a deflation will be performed
        //                instead.
        //-----------------------------------------------------------------------------
        public void Inflate(float amount)
        {
            mLength     += amount;
            mRadius     += amount / 2.0f;
            mBaseCenter -= mLengthAxis * amount / 2.0f;
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the cylinder.
        // Parm: ray        - Query ray.
        //       ignoreCaps - If true, the ray can't hit the cylinder caps.
        // Rtrn: True if the ray intersects the cylinder and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray, bool ignoreCaps)
        {
            return Raycast(ray, ignoreCaps, out float t);
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the cylinder.
        // Parm: ray        - Query ray.
        //       ignoreCaps - If true, the ray can't hit the cylinder caps.
        //       t          - Returns the distance from the ray origin where the intersection
        //                    happens. If the ray doesn't intersect the cylinder, this value
        //                    should be ignored.
        // Rtrn: True if the ray intersects the cylinder and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray, bool ignoreCaps, out float t)
        {
            // Clear output
            t = 0.0f;

            // Precompute needed data
            Vector3 dCl = Vector3.Cross(ray.direction, mLengthAxis);
            Vector3 oCl = Vector3.Cross((ray.origin - mBaseCenter), mLengthAxis);

            // Calculate the quadratic coefficients
            float a = dCl.sqrMagnitude;
            float b = 2.0f * Vector3.Dot(dCl, oCl);
            float c = oCl.sqrMagnitude - mRadius * mRadius;
            
            // Solve the quadratic equation
            float t0, t1;
            bool wasHit = false;
            Vector3 iPt = new Vector3();
            if (MathEx.SolveQuadratic(a, b, c, out t0, out t1))
            {
                // Make sure the ray doesn't intersect the cylinder only from behind
                if (t0 >= 0.0f || t1 >= 0.0f)
                {
                    // Are we ignoring caps?
                    if (!ignoreCaps)
                    {
                        // Make sure we are using the smallest positive t value
                        if (t0 < 0.0f)
                        {
                            float temp = t0;
                            t0 = t1;
                            t1 = temp;
                        }
                        t = t0;

                        // Now make sure the intersection point lies between the cylinder's bottom and top points
                        iPt = ray.GetPoint(t);
                        float d = Vector3.Dot(mLengthAxis, (iPt - mBaseCenter));
                        if (d >= 0.0f && d <= mLength) wasHit = true;
                    }
                    else
                    {
                        // When ignoring caps, we could have 2 hit points. One on the exterior and one on the interior.
                        // Pick the closest point that lies between the cylinder caps.
                        bool valid0 = false, valid1 = false;
                        float d0 = Vector3.Dot(mLengthAxis, (ray.GetPoint(t0) - mBaseCenter));
                        float d1 = Vector3.Dot(mLengthAxis, (ray.GetPoint(t1) - mBaseCenter));
                        if (d0 >= 0.0f && d0 <= mLength) valid0 = true;
                        if (d1 >= 0.0f && d1 <= mLength) valid1 = true;
                        if (valid0 && valid1) { t = t0 < t1 ? t0 : t1; return true; }
                        else if (valid0) { t = t0; return true; }
                        else if (valid1) { t = t1; return true; }

                        // No hit
                        return false;
                    }
                }
            }

            // Test bottom and top caps?
            if (!ignoreCaps)
            {
                Plane capPlane = new Plane(-mLengthAxis, mBaseCenter);
                if (capPlane.Raycast(ray, out t0) && (!wasHit || t0 < t))
                {
                    // The ray intersects the plane, but we must also check if the intersection point is within radius
                    if ((ray.GetPoint(t0) - mBaseCenter).sqrMagnitude <= mRadius * mRadius)
                    {
                        t = t0;
                        wasHit = true;
                    }
                }
                Vector3 topCenterPt = topCenter;
                capPlane = new Plane(mLengthAxis, topCenterPt);
                if (capPlane.Raycast(ray, out t1) && (!wasHit || t1 < t))
                {
                    // The ray intersects the plane, but we must also check if the intersection point is within radius
                    if ((ray.GetPoint(t1) - topCenterPt).sqrMagnitude <= mRadius * mRadius)
                    {
                        t = t1;
                        wasHit = true;
                    }
                }
            }

            // Return result
            return wasHit;
        }
        #endregion
    }
    #endregion
}