using UnityEngine;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: SquarePyramid (Public Struct)
    // Desc: Provides storage for square pyramid primitives and implements relevant
    //       functions.
    //-----------------------------------------------------------------------------
    public struct SquarePyramid
    {
        #region Private Fields
        float       mLength;        // Pyramid length (a.k.a height)
        float       mBaseSize;      // Pyramid base size
        Vector3     mBaseCenter;    // Base center position
        Vector3     mLengthAxis;    // Normalized length axis
        Vector3     mBaseAxis;      // Normalized base axis. If we move from the base center along this axis for
                                    // a distance of 'mBaseSize' / 2, we reach a point on the edge of the base.
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: length (Public Property)
        // Desc: Returns or sets the pyramid length.
        //-----------------------------------------------------------------------------
        public float    length      { get { return mLength; } set { mLength = Mathf.Max(0.0f, value); } }

        //-----------------------------------------------------------------------------
        // Name: baseSize (Public Property)
        // Desc: Returns or sets the pyramid base size.
        //-----------------------------------------------------------------------------
        public float    baseSize    { get { return mBaseSize; } set { mBaseSize = Mathf.Max(0.0f, value); } }

        //-----------------------------------------------------------------------------
        // Name: baseCenter (Public Property)
        // Desc: Returns or sets the pyramid base center position. The pyramid length
        //       extends out from this position in the direction of 'lengthAxis'.
        //-----------------------------------------------------------------------------
        public Vector3  baseCenter  { get { return mBaseCenter; } set { mBaseCenter = value; } }

        //-----------------------------------------------------------------------------
        // Name: tip (Public Property)
        // Desc: Returns the pyramid's tip position.
        //-----------------------------------------------------------------------------
        public Vector3  tip         { get { return mBaseCenter + mLengthAxis * mLength; } }
        
        //-----------------------------------------------------------------------------
        // Name: lengthAxis (Public Property)
        // Desc: Returns or sets the pyramid length axis. The setter normalizes the input.
        //-----------------------------------------------------------------------------
        public Vector3  lengthAxis  { get { return mLengthAxis; } set { mLengthAxis = value.normalized; } }

        //-----------------------------------------------------------------------------
        // Name: baseAxis (Public Property)
        // Desc: Returns or sets the pyramid base axis. If we move from the base center
        //       along this axis for a distance of 'baseSize' / 2, we reach a point on
        //       the edge of the base. The setter normalizes the input.
        //-----------------------------------------------------------------------------
        public Vector3  baseAxis   { get { return mBaseAxis; } set { mBaseAxis = value.normalized; } }

        //-----------------------------------------------------------------------------
        // Name: xCapTransform (Public Property)
        // Desc: Calculates and returns a transform matrix that can be used to draw the 
        //       pyramid as a mesh that defines the pyramid as the cap of the X axis
        //       (i.e. the length is aligned with the X axis) in model space.
        //-----------------------------------------------------------------------------
        public Matrix4x4    xCapTransform   { get { return Matrix4x4.TRS(mBaseCenter, Quaternion.LookRotation(Vector3.Cross(mLengthAxis, mBaseAxis).normalized, mBaseAxis), new Vector3(mLength, mBaseSize, mBaseSize)); } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Set (Public Function)
        // Desc: Sets the pyramid data. The function normalizes the input axes.
        // Parm: baseCenter - Pyramid base center.
        //       baseSize   - Pyramid base size.
        //       length     - Pyramid length.
        //       baseAxis   - Pyramid base axis. If we move from the base center along
        //                    this axis for a distance of 'baseSize' / 2, we reach a point
        //                    on the edge of the base.
        //       lengthAxis - Pyramid length axis.
        //-----------------------------------------------------------------------------
        public void Set(Vector3 baseCenter, float baseSize, float length, Vector3 baseAxis, Vector3 lengthAxis)
        {
            this.baseCenter = baseCenter;
            this.baseSize   = baseSize;
            this.length     = length;
            this.baseAxis   = baseAxis;
            this.lengthAxis = lengthAxis;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateAABB() (Public Function)
        // Desc: Calculates the pyramid's AABB.
        // Rtrn: The pyramid's AABB.
        //-----------------------------------------------------------------------------
        public Box CalculateAABB()
        {
            // Enclose base and tip
            Box aabb = new Box(mBaseCenter, Vector3.zero);
            aabb.EnclosePoint(tip);

            // Enclose base corners
            float hb = mBaseSize / 2.0f;
            Vector3 a = Vector3.Cross(mBaseAxis, mLengthAxis).normalized;
            aabb.EnclosePoint(mBaseCenter - mBaseAxis * hb - a * hb);
            aabb.EnclosePoint(mBaseCenter - mBaseAxis * hb + a * hb);
            aabb.EnclosePoint(mBaseCenter + mBaseAxis * hb + a * hb);
            aabb.EnclosePoint(mBaseCenter + mBaseAxis * hb - a * hb);

            // Return AABB
            return aabb;
        }

        //-----------------------------------------------------------------------------
        // Name: Inflate() (Public Function)
        // Desc: Inflates the pyramid by the specified amount.
        // Parm: amount - The amount to inflate. If negative, a deflation will be performed
        //                instead.
        //-----------------------------------------------------------------------------
        public void Inflate(float amount)
        {
            mLength     += amount;
            mBaseSize   += amount;
            mBaseCenter -= mLengthAxis * amount / 2.0f;
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the pyramid.
        // Parm: ray - Query ray.
        // Rtrn: True if the ray intersects the pyramid and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray)
        {
            return Raycast(ray, out float t);
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the pyramid.
        // Parm: ray - Query ray.
        //       t   - Returns the distance from the ray origin where the intersection happens.
        //             If the ray doesn't intersect the pyramid, this value should be ignored.
        // Rtrn: True if the ray intersects the pyramid and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray, out float t)
        {
            // Calculate forward axis
            Vector3 forwardAxis = Vector3.Cross(mBaseAxis, mLengthAxis).normalized;

            // Test base plane
            Plane basePlane = new Plane(-mLengthAxis, mBaseCenter);
            if (basePlane.Raycast(ray, out t))
            {
                // The ray intersects the plane, but we must also check if the intersection point is inside the base area
                Vector3 vec = ray.GetPoint(t) - mBaseCenter;
                if (MathEx.FastAbs(Vector3.Dot(vec, mBaseAxis)) <= mBaseSize &&
                    MathEx.FastAbs(Vector3.Dot(vec, forwardAxis)) <= mBaseSize) return true;           
            }

            // We need to check against the 4 triangles that make up the pyramid. Calculate the
            // pyramid base corner points needed to construct the triangles.
            float halfBSize = mBaseSize / 2.0f;
            Vector3 p0      = mBaseCenter - halfBSize * (mBaseAxis + forwardAxis);
            Vector3 p1      = mBaseCenter - halfBSize * (mBaseAxis - forwardAxis);
            Vector3 p2      = mBaseCenter + halfBSize * (mBaseAxis + forwardAxis);
            Vector3 p3      = mBaseCenter + halfBSize * (mBaseAxis - forwardAxis);
            Vector3 pTip    = tip;

            // Check if the ray intersects any of the 4 triangles and pick the closest intersection
            float triT;
            t = float.MaxValue;
            bool result = false;
            Triangle tri = new Triangle();
            tri.Set(p0, p1, pTip);
            if (tri.Raycast(ray, out triT) && triT < t) { t = triT; result = true; }
            tri.Set(p1, p2, pTip);
            if (tri.Raycast(ray, out triT) && triT < t) { t = triT; result = true; }
            tri.Set(p2, p3, pTip);
            if (tri.Raycast(ray, out triT) && triT < t) { t = triT; result = true; }
            tri.Set(p3, p0, pTip);
            if (tri.Raycast(ray, out triT) && triT < t) { t = triT; result = true; }

            // Return result
            return result;
        }
        #endregion
    }
    #endregion
}