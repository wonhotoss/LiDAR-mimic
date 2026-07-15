using UnityEngine;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: InsetOBox (Public Struct)
    // Desc: Provides storage for oriented bounding boxes that have an inset and
    //       implements relevant functions. 
    // Note: The inset is aligned with the box's Y axis.
    //-----------------------------------------------------------------------------
    public struct InsetOBox
    {
        #region Private Fields
        Vector3     mCenter;    // Center
        Vector3     mSize;      // Size
        Quaternion  mRotation;  // Rotation
        float       mThickness; // Thickness
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: isValid (Public Property)
        // Desc: Returns true if the box is valid and false otherwise.
        //-----------------------------------------------------------------------------
        public bool         isValid         { get { return mSize.x >= 0.0f; } }

        //-----------------------------------------------------------------------------
        // Name: center (Public Property)
        // Desc: Returns or sets the box's center.
        //-----------------------------------------------------------------------------
        public Vector3      center          { get { return mCenter; } set { mCenter = value; } }

        //-----------------------------------------------------------------------------
        // Name: size (Public Property)
        // Desc: Returns or sets the box's size. When setting the size, the absolute 
        //       value of the vector will be used in order to ensure the box always has
        //       a positive size.
        //-----------------------------------------------------------------------------
        public Vector3      size            { get { return mSize; } set { mSize = value.Abs(); } }

        //-----------------------------------------------------------------------------
        // Name: extents (Public Property)
        // Desc: Returns the box's extents (i.e. half the size).
        //-----------------------------------------------------------------------------
        public Vector3      extents         { get { return new Vector3(mSize.x * 0.5f, mSize.y * 0.5f, mSize.z * 0.5f); } }

        //-----------------------------------------------------------------------------
        // Name: rotation (Public Property)
        // Desc: Returns the box's rotation.
        //-----------------------------------------------------------------------------
        public Quaternion   rotation        { get { return mRotation; } set { mRotation = value; } }

        //-----------------------------------------------------------------------------
        // Name: transform (Public Property)
        // Desc: Returns the box's transform matrix. This is the matrix that transforms
        //       a unit box centered at <0, 0, 0> with an identity rotation to a box that
        //       has 'this' center, rotation and size.
        //-----------------------------------------------------------------------------
        public Matrix4x4    transform       { get { return Matrix4x4.TRS(center, mRotation, size); } }

        //-----------------------------------------------------------------------------
        // Name: thickness (Public Property)
        // Desc: Returns or sets the box thickness.
        //-----------------------------------------------------------------------------
        public float        thickness       { get { return mThickness; } set { mThickness = Mathf.Max(value, 0.0f); } }

        //-----------------------------------------------------------------------------
        // Name: right (Public Property)
        // Desc: Returns the box's right axis.
        //-----------------------------------------------------------------------------
        public Vector3      right           { get { return mRotation * Vector3.right; } }

        //-----------------------------------------------------------------------------
        // Name: up (Public Property)
        // Desc: Returns the box's up axis.
        //-----------------------------------------------------------------------------
        public Vector3      up              { get { return mRotation * Vector3.up; } }

        //-----------------------------------------------------------------------------
        // Name: look (Public Property)
        // Desc: Returns the box's forward axis.
        //-----------------------------------------------------------------------------
        public Vector3      forward         { get { return mRotation * Vector3.forward; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: CalculateAABB() (Public Function)
        // Desc: Calculates the box's AABB.
        // Rtrn: The box's AABB.
        //-----------------------------------------------------------------------------
        public Box CalculateAABB()
        {
            // Cache data
            Vector3 ex = extents;                           // Extents
            Vector3 boxRight    = right     * extents.x;    // Box right vector scaled by X extent
            Vector3 boxUp       = up        * extents.y;    // Box up vector scaled by Y extent
            Vector3 boxForward  = forward   * extents.z;    // Box forward vector scaled by Z extent

            // Calculate the extents of the AABB
            float aabbX = MathEx.FastAbs(boxRight.x) + MathEx.FastAbs(boxUp.x) + MathEx.FastAbs(boxForward.x);
            float aabbY = MathEx.FastAbs(boxRight.y) + MathEx.FastAbs(boxUp.y) + MathEx.FastAbs(boxForward.y);
            float aabbZ = MathEx.FastAbs(boxRight.z) + MathEx.FastAbs(boxUp.z) + MathEx.FastAbs(boxForward.z);

            // Return the AABB
            return new Box(mCenter, new Vector3(aabbX * 2.0f, aabbY * 2.0f, aabbZ * 2.0f));
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the box.
        // Parm: ray - Query ray.
        // Rtrn: True if the ray intersects the box and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray)
        {
            return Raycast(ray, out float t);
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the box.
        // Parm: ray - Query ray.
        //       t   - Returns the distance from the ray origin where the intersection happens.
        //             If the ray doesn't intersect the box, this value should be ignored.
        // Rtrn: True if the ray intersects the box and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray, out float t)
        {
            // Clear output
            t = 0.0f;

            // Transform the ray into box model space
	        Ray r       = new Ray();
	        r.origin	= ray.origin - mCenter;
	        r.origin	= new Vector3(Vector3.Dot(right, r.origin), Vector3.Dot(up, r.origin), Vector3.Dot(forward, r.origin));
	        r.direction	= new Vector3(Vector3.Dot(right, ray.direction), Vector3.Dot(up, ray.direction), Vector3.Dot(forward, ray.direction)).normalized;

            // Raycast box
            Bounds box = new Bounds(Vector3.zero, mSize);
            if (box.IntersectRay(r, out t))
            {
                // Cache data
                Vector3 iPt = r.GetPoint(t);
                Vector3 minPt = -mSize / 2.0f;
                Vector3 maxPt =  mSize / 2.0f;

                // If we hit the top or bottom, check if we hit the inset area
                const float eps = 1e-4f;
		        if (MathEx.FastAbs(iPt.y - maxPt.y) <= eps || 
                    MathEx.FastAbs(iPt.y - minPt.y) <= eps)
		        {
			        // Are we in the inset area?
			        float hix = (mSize.x - mThickness * 2.0f) / 2.0f;	// Half inset size along X
			        float hiz = (mSize.z - mThickness * 2.0f) / 2.0f;	// half inset size along Z
			        if (MathEx.FastAbs(iPt.x) < hix &&
				        MathEx.FastAbs(iPt.z) < hiz) 
			        {
                        // We're in the inset area. We need to raycast again to see if the ray hits the inner faces.
                        // Divide the box into 4 sections, raycast all of them and pick smallest t.
                        float tAux;
				        float tMin = float.MaxValue;

				        // Front face <0, 0, -1>
                        Bounds b = new Bounds();
				        b.min = new Vector3(minPt.x + mThickness, minPt.y, maxPt.z - mThickness);
				        b.max = new Vector3(maxPt.x - mThickness, maxPt.y, maxPt.z);
				        if (b.IntersectRay(r, out tAux) && tAux < tMin) { tMin = tAux; };

				        // Back face <0, 0, 1>
				        b.min = new Vector3(minPt.x + mThickness, minPt.y, minPt.z);
				        b.max = new Vector3(maxPt.x - mThickness, maxPt.y, minPt.z + mThickness);
				        if (b.IntersectRay(r, out tAux) && tAux < tMin) { tMin = tAux; };

				        // Left face <0, 0, 1>
				        b.min = new Vector3(minPt.x, minPt.y, minPt.z + mThickness);
				        b.max = new Vector3(minPt.x + mThickness, maxPt.y, maxPt.z - mThickness);
				        if (b.IntersectRay(r, out tAux) && tAux < tMin) { tMin = tAux; };

				        // Right face <0, 0, -1>
				        b.min = new Vector3(maxPt.x - mThickness, minPt.y, minPt.z + mThickness);
				        b.max = new Vector3(maxPt.x, maxPt.y, maxPt.z - mThickness);
				        if (b.IntersectRay(r, out tAux) && tAux < tMin) { tMin = tAux; };

				        // Have we hit anything?
				        if (tMin == float.MaxValue) return false;

                        // Store hit distance
                        t = tMin;
			        }
		        }

		        // We have an intersection
		        return true;
            }

            // No intersection
            return false;
        }
        #endregion
    }
    #endregion
}