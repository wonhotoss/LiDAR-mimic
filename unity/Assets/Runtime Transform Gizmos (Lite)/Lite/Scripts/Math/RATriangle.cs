using UnityEngine;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: RATriangle (Public Struct)
    // Desc: Provides storage for right-angled triangle primitives and implements
    //       relevant functions.
    //-----------------------------------------------------------------------------
    public struct RATriangle
    {
        #region Private Fields
        Vector3 mRACorner;      // Right-angled corner position
        Vector3 mAEAxis0;       // First adjacent edge axis
        Vector3 mAEAxis1;       // Second adjacent edge axis
        float   mAESize0;       // First adjacent edge size
        float   mAESize1;       // Second adjacent edge size
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: raCorner (Public Property)
        // Desc: Returns or sets the position of the right-angled corner.
        //-----------------------------------------------------------------------------
        public Vector3  raCorner    { get { return mRACorner; } set { mRACorner = value; } }

        //-----------------------------------------------------------------------------
        // Name: aeAxis0 (Public Property)
        // Desc: Returns or sets the first adjacent edge axis. The setter normalizes the
        //       input.
        //-----------------------------------------------------------------------------
        public Vector3  aeAxis0     { get { return mAEAxis0; } set { mAEAxis0 = value.normalized; } }

        //-----------------------------------------------------------------------------
        // Name: aeAxis1 (Public Property)
        // Desc: Returns the second adjacent edge axis. The setter normalizes the input.
        //-----------------------------------------------------------------------------
        public Vector3  aeAxis1     { get { return mAEAxis1; } set { mAEAxis1 = value.normalized; } }

        //-----------------------------------------------------------------------------
        // Name: normal (Public Property)
        // Desc: Returns the triangle normal.
        //-----------------------------------------------------------------------------
        public Vector3  normal      { get { return Vector3.Cross(mAEAxis0, mAEAxis1).normalized; } }

        //-----------------------------------------------------------------------------
        // Name: plane (Public Property)
        // Desc: Returns the triangle plane.
        //-----------------------------------------------------------------------------
        public Plane    plane       { get { return new Plane(normal, mRACorner); } }

        //-----------------------------------------------------------------------------
        // Name: aeSize0 (Public Property)
        // Desc: Returns or sets the size of the first adjacent edge.
        //-----------------------------------------------------------------------------
        public float    aeSize0     { get { return mAESize0; } set { mAESize0 = Mathf.Max(0.0f, value); } }

        //-----------------------------------------------------------------------------
        // Name: aeSize1 (Public Property)
        // Desc: Returns the size of the second adjacent edge.
        //-----------------------------------------------------------------------------
        public float    aeSize1     { get { return mAESize1; } set { mAESize1 = Mathf.Max(0.0f, value); } }

        //-----------------------------------------------------------------------------
        // Name: xyTransform (Public Property)
        // Desc: Calculates and returns a transform matrix that can be used to draw the 
        //       triangle as a mesh that defines the triangle in the XY plane in model space.
        //-----------------------------------------------------------------------------
        public Matrix4x4 xyTransform { get { return Matrix4x4.TRS(mRACorner, Quaternion.LookRotation(normal, mAEAxis1), new Vector3(mAESize0, mAESize1, 1.0f)); } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Set() (Public Function)
        // Desc: Sets the triangle's data.
        // Parm: raCorner - Right-angled corner position.
        //       aeAxis0  - The first adjacent edge axis. The function takes care of
        //                  normalizing this vector.
        //       aeAxis1  - The second adjacent edge axis. The function takes care of
        //                  normalizing this vector.
        //       aeSize0  - The size of the first adjacent edge.
        //       aeSize1  - The size of the second adjacent edge.
        //-----------------------------------------------------------------------------
        public void Set(Vector3 raCorner, Vector3 aeAxis0, Vector3 aeAxis1, float aeSize0, float aeSize1)
        {
            this.raCorner   = raCorner;
            this.aeAxis0    = aeAxis0.normalized;
            this.aeAxis1    = aeAxis1.normalized;
            this.aeSize0    = aeSize0;
            this.aeSize1    = aeSize1;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateAABB() (Public Function)
        // Desc: Calculates the triangle's AABB.
        // Rtrn: The triangle's AABB.
        //-----------------------------------------------------------------------------
        public Box CalculateAABB()
        {
            // Enclose triangle points
            Box aabb = new Box(mRACorner, Vector3.zero);
            aabb.EnclosePoint(mRACorner + mAEAxis0 * mAESize0);
            aabb.EnclosePoint(mRACorner + mAEAxis1 * mAESize1);

            // Return AABB
            return aabb;
        }

        //-----------------------------------------------------------------------------
        // Name: Inflate() (Public Function)
        // Desc: Inflates the triangle by the specified amount.
        // Parm: amount - The amount to inflate. If negative, a deflation will be performed
        //                instead.
        //-----------------------------------------------------------------------------
        public void Inflate(float amount)
        {
            // Offset RA corner position outwards
            mRACorner -= (mAEAxis0 + mAEAxis1) * amount;

            // Inflate edge lengths
            mAESize0 += 3.0f * amount;
            mAESize1 += 3.0f * amount;
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the quad.
        // Parm: ray - Query ray.
        // Rtrn: True if the ray intersects the quad and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray)
        {
            return Raycast(ray, out float t);
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the quad.
        // Parm: ray - Query ray.
        //       t   - Returns the distance from the ray origin where the intersection happens.
        //             If the ray doesn't intersect the quad, this value should be ignored.
        // Rtrn: True if the ray intersects the quad and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray, out float t)
        {
            // Clear output
            t = 0.0f;

            // Create a generic triangle primitive and raycast it
            Triangle triangle = new Triangle();
            triangle.Set(mRACorner, mRACorner + mAEAxis0 * mAESize0, mRACorner + mAEAxis1 * mAESize1);
            return triangle.Raycast(ray, out t);
        }
        #endregion
    }
    #endregion
}
