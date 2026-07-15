using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: Quad (Public Struct)
    // Desc: Provides storage for quad primitives and implements relevant functions.
    //-----------------------------------------------------------------------------
    public struct Quad
    {
        #region Private Fields
        Vector3     mCenter;        // Center position
        float       mWidth;         // Width
        float       mHeight;        // Height
        Vector3     mNormal;        // Normal vector
        Vector3     mWidthAxis;     // The width axis
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: center (Public Property)
        // Desc: Returns or sets the quad center.
        //-----------------------------------------------------------------------------
        public Vector3  center      { get { return mCenter; } set { mCenter = value; } }

        //-----------------------------------------------------------------------------
        // Name: width (Public Property)
        // Desc: Returns or sets the quad width.
        //-----------------------------------------------------------------------------
        public float    width       { get { return mWidth; } set { mWidth = Mathf.Max(value, 0.0f); } }

        //-----------------------------------------------------------------------------
        // Name: height (Public Property)
        // Desc: Returns or sets the quad height.
        //-----------------------------------------------------------------------------
        public float    height      { get { return mHeight; } set { mHeight = Mathf.Max(value, 0.0f); } }

        //-----------------------------------------------------------------------------
        // Name: normal (Public Property)
        // Desc: Returns or sets the quad normal. The setter normalizes the input.
        //-----------------------------------------------------------------------------
        public Vector3  normal      { get { return mNormal; } set { mNormal = value.normalized; } }

        //-----------------------------------------------------------------------------
        // Name: widthAxis (Public Property)
        // Desc: Returns or sets the width axis. The setter normalizes the input.
        //-----------------------------------------------------------------------------
        public Vector3  widthAxis   { get { return mWidthAxis; } set { mWidthAxis = value.normalized; } }

        //-----------------------------------------------------------------------------
        // Name: heightAxis (Public Property)
        // Desc: Calculates and returns the quad's height axis.
        //-----------------------------------------------------------------------------
        public Vector3  heightAxis  { get { return Vector3.Cross(mNormal, mWidthAxis).normalized; } }

        //-----------------------------------------------------------------------------
        // Name: plane (Public Property)
        // Desc: Returns the quad plane.
        //-----------------------------------------------------------------------------
        public Plane    plane       { get { return new Plane(mNormal, mCenter); } }

        //-----------------------------------------------------------------------------
        // Name: xyTransform (Public Property)
        // Desc: Calculates and returns a transform matrix that can be used to draw the 
        //       quad as a mesh that defines the quad in the XY plane in model space.
        //-----------------------------------------------------------------------------
        public Matrix4x4 xyTransform { get { return Matrix4x4.TRS(mCenter, Quaternion.LookRotation(normal, heightAxis), new Vector3(mWidth, mHeight, 1.0f)); } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Set (Public Function)
        // Desc: Sets the quad data. The function normalizes the input axes.
        // Parm: center     - Quad center.
        //       width      - Quad width.
        //       height     - Quad height.
        //       normal     - Quad normal.
        //       widthAxis  - Quad width axis.
        //-----------------------------------------------------------------------------
        public void Set(Vector3 center, float width, float height, Vector3 normal, Vector3 widthAxis)
        {
            this.center     = center;
            this.width      = width;
            this.height     = height;
            this.normal     = normal;
            this.widthAxis  = widthAxis;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateAABB() (Public Function)
        // Desc: Calculates the quad's AABB.
        // Rtrn: The quad's AABB.
        //-----------------------------------------------------------------------------
        public Box CalculateAABB()
        {
            // Cache data
            Vector3 hAxis   = heightAxis;
            float hw        = mWidth / 2.0f;
            float hh        = mHeight / 2.0f;

            // Enclose corner points
            Box aabb = new Box(mCenter - mWidthAxis * hw - hAxis * hh, Vector3.zero);
            aabb.EnclosePoint(mCenter - mWidthAxis * hw + hAxis * hh);
            aabb.EnclosePoint(mCenter + mWidthAxis * hw + hAxis * hh);
            aabb.EnclosePoint(mCenter + mWidthAxis * hw - hAxis * hh);

            // Return AABB
            return aabb;
        }

        //-----------------------------------------------------------------------------
        // Name: Inflate() (Public Function)
        // Desc: Inflates the quad by the specified amount.
        // Parm: amount - The amount to inflate. If negative, a deflation will be performed
        //                instead.
        //-----------------------------------------------------------------------------
        public void Inflate(float amount)
        {
            // Inflate/deflate
            mWidth  += amount;
            mHeight += amount;

            // Clip size
            if (mWidth < 0.0f) mWidth = 0.0f;
            if (mHeight < 0.0f) mHeight = 0.0f;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateEdges() (Public Function)
        // Desc: Calculates the quad's edge segments and stores them inside 'edges'.
        // Parm: edges - Returns the quad's edge segments in the following order:
        //               left, top, right, bottom when looking down along the quad normal.
        //-----------------------------------------------------------------------------
        public void CalculateEdges(List<Segment> edges)
        {
            // Precompute data
            float hw = mWidth / 2.0f;
            float hh = mHeight / 2.0f;

            // Calculate corner points
            Vector3 heightAxis = Vector3.Cross(mWidthAxis, mNormal).normalized;
            Vector3 p0 = mCenter - mWidthAxis * hw - heightAxis * hh;
            Vector3 p1 = mCenter - mWidthAxis * hw + heightAxis * hh;
            Vector3 p2 = mCenter + mWidthAxis * hw + heightAxis * hh;
            Vector3 p3 = mCenter + mWidthAxis * hw - heightAxis * hh;

            // Create the segments
            edges.Clear();
            edges.Add(new Segment(p0, p1));
            edges.Add(new Segment(p1, p2));
            edges.Add(new Segment(p2, p3));
            edges.Add(new Segment(p3, p0));
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

            // If the ray doesn't intersect the plane, it can't possibly intersect the quad
            if (!plane.Raycast(ray, out t))
                return false;

            // The ray intersects the plane. Check if is within the area.
            Vector3 iPt = ray.GetPoint(t);
            Vector3 v   = iPt - mCenter;
            if (MathEx.FastAbs(Vector3.Dot(v, mWidthAxis)) > (mWidth / 2.0f)) return false;

            // Check if it is within area along the height axis
            if (MathEx.FastAbs(Vector3.Dot(v, heightAxis)) > (mHeight / 2.0f)) return false;

            // We have an intersection
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: RaycastWire() (Public Function)
        // Desc: Checks if the specified ray intersects the wireframe representation
        //       of the quad (i.e. the quad border).
        // Parm: ray      - Query ray.
        //       wireSize - Wire size. Only intersections whose distance from the
        //                  quad border is wireSize / 2 are taken into account.
        //       t        - Returns the distance from the ray origin where the intersection
        //                  happens. If the ray doesn't intersect the quad, this value should
        //                  be ignored.
        // Rtrn: True if the ray intersects the quad wireframe and false otherwise.
        //-----------------------------------------------------------------------------
        public bool RaycastWire(Ray ray, float wireSize, out float t)
        {
            // Clear output
            t = 0.0f;

            // If the ray doesn't intersect the plane, it can't possibly intersect the quad
            if (!plane.Raycast(ray, out t))
                return false;

            // The ray intersects the plane. Check if is close enough to the wire along both axes.
            Vector3 iPt = ray.GetPoint(t);
            Vector3 v   = iPt - mCenter;
            float eps   = wireSize / 2.0f;
            float dw = MathEx.FastAbs(Vector3.Dot(v, mWidthAxis));
            float dh = MathEx.FastAbs(Vector3.Dot(v, heightAxis));
            if (dw >= (mWidth  / 2.0f - eps) && dw <= (mWidth  / 2.0f + eps) && dh <= mHeight / 2.0f + eps) return true;     
            if (dh >= (mHeight / 2.0f - eps) && dh <= (mHeight / 2.0f + eps) && dw <= mWidth  / 2.0f + eps) return true;

            // No intersection
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: ContainsPoint() (Public Function)
        // Desc: Checks if the quad contains the specified point.
        // Parm: pt - Query point.
        // Rtrn: True if the point lies inside the quad and false otherwise. The function
        //       doesn't check if the point is on the quad plane. It checks if the point
        //       lies within the quad's area defined by its width and height.
        //-----------------------------------------------------------------------------
        public bool ContainsPoint(Vector3 pt)
        {
            // Check width axis
            Vector3 v = pt - mCenter;
            if (MathEx.FastAbs(Vector3.Dot(v, mWidthAxis)) > (mWidth / 2.0f)) return false;

            // Check height axis
            Vector3 heightAxis = Vector3.Cross(mWidthAxis, mNormal).normalized;
            if (MathEx.FastAbs(Vector3.Dot(v, heightAxis)) > (mHeight / 2.0f)) return false;

            // The point is inside the quad
            return true;
        }
        #endregion
    }
    #endregion
}
