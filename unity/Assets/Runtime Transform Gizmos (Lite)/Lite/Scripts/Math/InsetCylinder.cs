using UnityEngine;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: InsetCylinder (Public Struct)
    // Desc: Provides storage for cylinder primitives that have an inset and implements
    //       relevant functions.
    //-----------------------------------------------------------------------------
    public struct InsetCylinder
    {
        #region Private Fields
        float       mLength;        // Cylinder length (a.k.a height)
        float       mRadius;        // Cylinder radius
        Vector3     mBaseCenter;    // Base center position
        Vector3     mLengthAxis;    // Normalized length axis
        Vector3     mRadiusAxis;    // Normalized radius axis. If we move from the center of the base along this 
                                    // axis for 'mRadius' units, we end up on the cylinder circumference.
        float       mThickness;     // Thickness
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

        //-----------------------------------------------------------------------------
        // Name: thickness (Public Property)
        // Desc: Returns or sets the cylinder thickness.
        //-----------------------------------------------------------------------------
        public float    thickness   { get { return mThickness; } set { mThickness = Mathf.Max(value, 0.0f); } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Set (Public Function)
        // Desc: Sets the cylinder data. The function normalizes the input axes.
        // Parm: baseCenter - Cylinder base center.
        //       radius     - Cylinder radius.
        //       length     - Cylinder length.
        //       radiusAxis - Cylinder radius axis. If we move from the center of the base
        //                    along this axis for 'mRadius' units, we end up on the cylinder
        //                    circumference.
        //       lengthAxis - Cylinder length axis.
        //       thickness  - Cylinder thickness.
        //-----------------------------------------------------------------------------
        public void Set(Vector3 baseCenter, float radius, float length, Vector3 radiusAxis, Vector3 lengthAxis, float thickness = 0.0f)
        {
            this.baseCenter = baseCenter;
            this.radius     = radius;
            this.length     = length;
            this.radiusAxis = radiusAxis;
            this.lengthAxis = lengthAxis;
            this.thickness  = thickness;
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
        // Name: GetZTransform (Public Function)
        // Desc: Calculates and returns a transform matrix that can be used to draw the 
        //       cylinder as a mesh that defines the cylinder with its length axis aligned
        //       with the Z axis.
        // Parm: radiusAxis - Cylinder radius axis perpendicular to the cylinder length axis.
        // Rtrn: The cylinder transform matrix.
        //-----------------------------------------------------------------------------
        public Matrix4x4 GetZTransform(Vector3 radiusAxis)
        {
            return Matrix4x4.TRS(mBaseCenter, Quaternion.LookRotation(mLengthAxis, radiusAxis.normalized), new Vector3(mRadius, mRadius, mLength));
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
            mThickness  += amount * 2.0f;   // Note: Multiply by 2 to account for the radius increase.
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

            // Raycast standard cylinder
            Cylinder cylinder = new Cylinder();
            cylinder.Set(mBaseCenter, mRadius, mLength, mRadiusAxis, mLengthAxis);
            if (!cylinder.Raycast(ray, false, out t))
                return false;

            // If we've hit one of the caps, we might have hit the inset area
            Vector3 iPt = ray.GetPoint(t);
            float d = Vector3.Dot(iPt - cylinder.baseCenter, cylinder.lengthAxis);
            if (d < 1e-4f || MathEx.FastAbs(d - cylinder.length) < 1e-4f)
            {
                // We've hit one of the caps, are we inside the inset area?
                float halfInset = mRadius - mThickness;
                iPt = iPt - mLengthAxis * d;        // Hit point projected into the base plane
                if ((iPt - mBaseCenter).magnitude < halfInset)
                {
                    // We're in the inset area. Raycast against the inner cylinder. If we don't hit it, it means the ray doesn't intersect the cylinder.
                    cylinder.radius  = halfInset;
                    if (!cylinder.Raycast(ray, true, out t))
                        return false;
                }
            }

            // We have a hit!
            return true;
        }
        #endregion
    }
    #endregion
}