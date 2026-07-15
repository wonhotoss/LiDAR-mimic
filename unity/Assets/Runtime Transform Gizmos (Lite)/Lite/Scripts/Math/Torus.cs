using System.IO.MemoryMappedFiles;
using UnityEngine;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: Torus (Public Struct)
    // Desc: Provides storage for torus primitives and implements relevant functions.
    //-----------------------------------------------------------------------------
    public struct Torus
    {
        #region Private Fields
        float   mTubeRadius;        // Tube radius
        float   mRadius;            // Torus radius which is the distance between the center of the torus to the center of the cross section
        Vector3 mCenter;            // Torus center
        Vector3 mMainAxis;          // The torus main axis. The cross sections rotate around this axis to create the torus.
        Vector3 mRadiusAxis;        // The torus radius axis. If we start from the torus center and move along this axis by a distance of 'mRadius' units, we end up at the center of a cross section.
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: tubeRadius (Public Property)
        // Desc: Returns or sets the tube radius.
        //-----------------------------------------------------------------------------
        public float    tubeRadius      { get { return mTubeRadius; } set { mTubeRadius = Mathf.Max(value, 0.0f); } }

        //-----------------------------------------------------------------------------
        // Name: radius (Public Property)
        // Desc: Returns or sets the torus radius. This is the distance between the center
        //       of the torus to the center of the cross section.
        //-----------------------------------------------------------------------------
        public float    radius          { get { return mRadius; } set { mRadius = Mathf.Max(value, 0.0f); } }

        //-----------------------------------------------------------------------------
        // Name: center (Public Property)
        // Desc: Returns or sets the torus center.
        //-----------------------------------------------------------------------------
        public Vector3  center          { get { return mCenter; } set { mCenter = value; } }

        //-----------------------------------------------------------------------------
        // Name: mainAxis (Public Property)
        // Desc: Returns or sets the torus main axis. The cross sections rotate around
        //       this axis to create the torus. The setter normalizes the input.
        //-----------------------------------------------------------------------------
        public Vector3  mainAxis        { get { return mMainAxis; } set { mMainAxis = value.normalized; } }

        //-----------------------------------------------------------------------------
        // Name: radiusAxis (Public Property)
        // Desc: Returns or sets the torus radius axis. If we start from the torus center
        //       and move along this axis by a distance of 'radius' units, we end up at
        //       the center of a cross section.
        //-----------------------------------------------------------------------------
        public Vector3  radiusAxis      { get { return mRadiusAxis; } set { mRadiusAxis = value.normalized; } }       
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Set (Public Function)
        // Desc: Sets the torus data. The function normalizes the input axes.
        // Parm: center     - Torus center.
        //       radius     - Torus radius. This is the distance between the center of
        //                    the torus to the center of the cross section.
        //       tubeRadius - Torus tube radius.
        //       radiusAxis - Torus radius axis. If we start from the torus center and move
        //                    along this axis by a distance of 'radius' units, we end up at
        //                    the center of a cross section.
        //       mainAxis   - Torus main axis. The cross sections rotate around this axis
        //                    to create the torus.
        //-----------------------------------------------------------------------------
        public void Set(Vector3 center, float radius, float tubeRadius, Vector3 radiusAxis, Vector3 mainAxis)
        {
            this.center         = center;
            this.radius         = radius;
            this.tubeRadius     = tubeRadius;
            this.radiusAxis     = radiusAxis;
            this.mainAxis       = mainAxis;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateAABB() (Public Function)
        // Desc: Calculates the torus' AABB.
        // Rtrn: The torus' AABB.
        //-----------------------------------------------------------------------------
        public Box CalculateAABB()
        {
            // Enclose corners of OBB that encloses the torus
            Vector3 a   = Vector3.Cross(mRadiusAxis, mMainAxis).normalized;
            float r     = mTubeRadius + mRadius;
            Box aabb    = new Box(mCenter - mRadiusAxis * r - mMainAxis * r - a * r, Vector3.zero);
            aabb.EnclosePoint(mCenter - mRadiusAxis * r + mMainAxis * r - a * r);
            aabb.EnclosePoint(mCenter - mRadiusAxis * r - mMainAxis * r + a * r);
            aabb.EnclosePoint(mCenter - mRadiusAxis * r + mMainAxis * r + a * r);
            aabb.EnclosePoint(mCenter + mRadiusAxis * r - mMainAxis * r - a * r);
            aabb.EnclosePoint(mCenter + mRadiusAxis * r + mMainAxis * r - a * r);
            aabb.EnclosePoint(mCenter + mRadiusAxis * r - mMainAxis * r + a * r);
            aabb.EnclosePoint(mCenter + mRadiusAxis * r + mMainAxis * r + a * r);

            // Return AABB
            return aabb;
        }

        //-----------------------------------------------------------------------------
        // Name: Inflate() (Public Function)
        // Desc: Inflates the torus by the specified amount.
        // Parm: amount - The amount to inflate. If negative, a deflation will be performed
        //                instead.
        //-----------------------------------------------------------------------------
        public void Inflate(float amount)
        {
            // Inflate/deflate and clip
            mTubeRadius += amount;
            if (mTubeRadius < 0.0f) mTubeRadius = 0.0f;
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the torus. This function approximates
        //       the torus using an inset cylinder.
        // Parm: ray - Query ray.
        // Rtrn: True if the ray intersects the torus and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray)
        {
            return Raycast(ray, out float t);
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the torus. This function approximates
        //       the torus using an inset cylinder.
        // Parm: ray - Query ray.
        //       t   - Returns the distance from the ray origin where the intersection happens.
        //             If the ray doesn't intersect the torus, this value should be ignored.
        // Rtrn: True if the ray intersects the torus and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray, out float t)
        {
            // Clear output
            t = 0.0f;

            // Create an inset cylinder
            var insetCylinder = new InsetCylinder();
            insetCylinder.Set(mCenter - mMainAxis * mTubeRadius, mRadius + mTubeRadius, 2.0f * mTubeRadius, mRadiusAxis, mMainAxis, 2.0f * mTubeRadius);

            // Raycast
            return insetCylinder.Raycast(ray, false, out t);
        }
        #endregion
    }
    #endregion
}
