using UnityEngine;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: Cone (Public Struct)
    // Desc: Provides storage for cone primitives and implements relevant functions.
    //-----------------------------------------------------------------------------
    public struct Cone
    {
        #region Private Fields
        float       mLength;        // Cone length (a.k.a height)
        float       mRadius;        // Cone radius
        Vector3     mBaseCenter;    // Base center position
        Vector3     mLengthAxis;    // Normalized length axis
        Vector3     mRadiusAxis;    // Normalized radius axis. Moving from the center of the cone base along this
                                    // axis for a distance of 'mRadius' units, we reach a point on the base circumference.
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: length (Public Property)
        // Desc: Returns or sets the Cone length.
        //-----------------------------------------------------------------------------
        public float    length      { get { return mLength; } set { mLength = Mathf.Max(0.0f, value); } }

        //-----------------------------------------------------------------------------
        // Name: radius (Public Property)
        // Desc: Returns or sets the Cone radius.
        //-----------------------------------------------------------------------------
        public float    radius      { get { return mRadius; } set { mRadius = Mathf.Max(0.0f, value); } }

        //-----------------------------------------------------------------------------
        // Name: baseCenter (Public Property)
        // Desc: Returns or sets the Cone base center position. The Cone length
        //       extends out from this position in the direction of 'lengthAxis'.
        //-----------------------------------------------------------------------------
        public Vector3  baseCenter  { get { return mBaseCenter; } set { mBaseCenter = value; } }

        //-----------------------------------------------------------------------------
        // Name: tip (Public Property)
        // Desc: Returns the cone's tip position.
        //-----------------------------------------------------------------------------
        public Vector3  tip   { get { return mBaseCenter + mLengthAxis * mLength; } }
        
        //-----------------------------------------------------------------------------
        // Name: lengthAxis (Public Property)
        // Desc: Returns or sets the cone length axis. The setter normalizes the input.
        //-----------------------------------------------------------------------------
        public Vector3  lengthAxis  { get { return mLengthAxis; } set { mLengthAxis = value.normalized; } }

        //-----------------------------------------------------------------------------
        // Name: radiusAxis (Public Property)
        // Desc: Returns or sets the cone radius axis. Moving from the center of the cone
        //       base along this axis for a distance of 'radius' units, we reach a point
        //       on the base circumference. The setter normalizes the input.
        //-----------------------------------------------------------------------------
        public Vector3  radiusAxis   { get { return mRadiusAxis; } set { mRadiusAxis = value.normalized; } }

        //-----------------------------------------------------------------------------
        // Name: xCapTransform (Public Property)
        // Desc: Calculates and returns a transform matrix that can be used to draw the 
        //       cone as a mesh that defines the cone as the cap of the X axis (i.e. the
        //       length is aligned with the X axis) in model space.
        //-----------------------------------------------------------------------------
        public Matrix4x4    xCapTransform   { get { return Matrix4x4.TRS(mBaseCenter, Quaternion.LookRotation(Vector3.Cross(mLengthAxis, mRadiusAxis).normalized, mRadiusAxis), new Vector3(mLength, mRadius, mRadius)); } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Set (Public Function)
        // Desc: Sets the cone data. The function normalizes the input axes.
        // Parm: baseCenter - Cone base center.
        //       radius     - Cone radius.
        //       length     - Cone length.
        //       radiusAxis - Cone radius axis. Moving from the center of the cone base
        //                    along this axis for a distance of 'radius' units, we reach a
        //                    point on the base circumference.
        //       lengthAxis - Cone length axis.
        //-----------------------------------------------------------------------------
        public void Set(Vector3 baseCenter, float radius, float length, Vector3 radiusAxis, Vector3 lengthAxis)
        {
            this.baseCenter     = baseCenter;
            this.radius         = radius;
            this.length         = length;
            this.radiusAxis     = radiusAxis;
            this.lengthAxis     = lengthAxis;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateAABB() (Public Function)
        // Desc: Calculates the cone's AABB.
        // Rtrn: The cone's AABB.
        //-----------------------------------------------------------------------------
        public Box CalculateAABB()
        {
            // Cache data
            Vector3 a = Vector3.Cross(mLengthAxis, mRadiusAxis).normalized;

            // Enclose base tip and base extents
            Box aabb = new Box(tip, Vector3.zero);
            aabb.EnclosePoint(mBaseCenter + a * mRadius);
            aabb.EnclosePoint(mBaseCenter - a * mRadius);
            aabb.EnclosePoint(mBaseCenter + mRadiusAxis * mRadius);
            aabb.EnclosePoint(mBaseCenter - mRadiusAxis * mRadius);

            // Return AABB
            return aabb;
        }

        //-----------------------------------------------------------------------------
        // Name: Inflate() (Public Function)
        // Desc: Inflates the cone by the specified amount.
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
        // Desc: Checks if the specified ray intersects the cone.
        // Parm: ray - Query ray.
        // Rtrn: True if the ray intersects the cone and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray)
        {
            return Raycast(ray, out float t);
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the cone.
        // Parm: ray - Query ray.
        //       t   - Returns the distance from the ray origin where the intersection happens.
        //             If the ray doesn't intersect the cone, this value should be ignored.
        // Rtrn: True if the ray intersects the cone and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray, out float t)
        {
            // Clear output
            t = 0.0f;

            // We will use the cone equation, so we need to convert the ray in the cone's model space
            Vector3 forwardAxis = Vector3.Cross(mRadiusAxis, mLengthAxis).normalized;
            Matrix4x4 invTransform = Matrix4x4.TRS(mBaseCenter, Quaternion.LookRotation(forwardAxis, mLengthAxis), Vector3.one).inverse;
            Ray modelRay = ray.Transform(invTransform);

            // We will first perform a preliminary check to see if the ray intersects the bottom cap of the cone.
            // This is necessary because the cone equation views the cone as infinite (i.e. no bottom cap), and
            // if we didn't perform this check, we would never be able to tell when the bottom cap was hit.
            Plane capPlane = new Plane(-Vector3.up, Vector3.zero);
            if (capPlane.Raycast(modelRay, out t))
            {
                // If the ray intersects the bottom cap plane, we will calculate the intersection point
                // and if it lies inside the cone's bottom cap area, it means we have a valid intersection.
                Vector3 iPt = modelRay.GetPoint(t);
                if (iPt.sqrMagnitude <= (mRadius * mRadius))
                    return true;
            }

            // Calculate the coefficients.
            // Note: The cone equation which was used is: (X^2 + Z^2) / rSq = (Y - length)^2.
            //       Where X, Y and Z are the coordinates of the point along the ray: (Origin + Direction * t).xyz
            float rSq = mRadius / mLength; rSq *= rSq;
            float a = modelRay.direction.x * modelRay.direction.x + modelRay.direction.z * modelRay.direction.z - rSq * modelRay.direction.y * modelRay.direction.y;
            float b = 2.0f * (modelRay.origin.x * modelRay.direction.x + modelRay.origin.z * modelRay.direction.z - rSq * modelRay.direction.y * (modelRay.origin.y - mLength));
            float c = modelRay.origin.x * modelRay.origin.x + modelRay.origin.z * modelRay.origin.z - rSq * (modelRay.origin.y - mLength) * (modelRay.origin.y - mLength);

            // The intersection happens only if the quadratic equation has solutions
            float t0, t1;
            if (MathEx.SolveQuadratic(a, b, c, out t0, out t1))
            {
                // Make sure the ray does not intersect the cone only from behind
                if (t0 < 0.0f && t1 < 0.0f) return false;

                // Make sure we are using the smallest positive t value
                if (t0 < 0.0f)
                {
                    float temp = t0;
                    t0 = t1;
                    t1 = temp;
                }
                t = t0;

                // Make sure the intersection point does not sit below the cone's bottom cap or above the cone's tip
                Vector3 iPt = modelRay.origin + modelRay.direction * t;
                if (iPt.y < 0.0f || iPt.y > mLength)
                {
                    t = 0.0f;
                    return false;
                }

                // The intersection point is valid
                return true;
            }

            // No intersection
            return false;
        }
        #endregion
    }
    #endregion
}