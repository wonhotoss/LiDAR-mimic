using UnityEngine;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: Sphere (Public Struct)
    // Desc: Provides storage for sphere primitives and implements relevant functions.
    //-----------------------------------------------------------------------------
    public struct Sphere
    {
        #region Private Fields
        Vector3 mCenter;    // Sphere center
        float   mRadius;    // Sphere radius
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: center (Public Property)
        // Desc: Returns or sets the sphere center.
        //-----------------------------------------------------------------------------
        public Vector3 center { get { return mCenter; } set { mCenter = value; } }

        //-----------------------------------------------------------------------------
        // Name: radius (Public Property)
        // Desc: Returns or sets the sphere radius. The absolute value of the radius will 
        //       always be used to ensure the radius is always positive.
        //-----------------------------------------------------------------------------
        public float   radius { get { return mRadius; } set { mRadius = Mathf.Max(value, 0.0f); } }

        //-----------------------------------------------------------------------------
        // Name: transform (Public Property)
        // Desc: Returns the sphere's transform matrix. This is the matrix that transforms
        //       a unit sphere centered at <0, 0, 0> to a sphere that has 'this' center
        //       and radius.
        //-----------------------------------------------------------------------------
        public Matrix4x4    transform   { get { return Matrix4x4.TRS(mCenter, Quaternion.identity, Vector3Ex.FromValue(mRadius)); } }
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------
        // Name: Sphere() (Public Constructor)
        // Desc: Creates a sphere from the specified center and radius.
        // Parm: center - Sphere center.
        //       radius - Sphere radius. If < 0, the radius will be set to 0.
        //-----------------------------------------------------------------------------
        public Sphere(Vector3 center, float radius)
        {
            // Set sphere data
            mCenter = center;
            mRadius = Mathf.Max(radius, 0.0f);
        }

        //-----------------------------------------------------------------------------
        // Name: Sphere() (Public Constructor)
        // Desc: Creates a sphere with the specified radius. The sphere center is set 
        //       to the default value of <0, 0, 0>.
        // Parm: radius - Sphere radius. If < 0, the radius will be set to 0.
        //-----------------------------------------------------------------------------
        public Sphere(float radius)
        {
            // Set sphere data
            mCenter = Vector3.zero;
            mRadius = Mathf.Max(radius, 0.0f);
        }

        //-----------------------------------------------------------------------------
        // Name: Sphere() (Public Constructor)
        // Desc: Creates a sphere that encloses the specified box. If the box is invalid,
        //       the sphere center will be set to <0, 0, 0> and its radius to 0.
        //-----------------------------------------------------------------------------
        public Sphere(Box box)
        {
            // Is the box valid?
            if (box.isValid)
            {
                mCenter = box.center;
                mRadius = box.extents.magnitude;
            }
            else
            {
                mCenter = Vector3.zero;
                mRadius = 0.0f;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: Sphere() (Public Constructor)
        // Desc: Creates a sphere that encloses the specified box. If the box is invalid,
        //       the sphere center will be set to <0, 0, 0> and its radius to 0.
        //-----------------------------------------------------------------------------
        public Sphere(OBox box)
        {
            // Is the box valid?
            if (box.isValid)
            {
                mCenter = box.center;
                mRadius = box.extents.magnitude;
            }
            else
            {
                mCenter = Vector3.zero;
                mRadius = 0.0f;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: Sphere() (Public Constructor)
        // Desc: Creates a sphere that encloses the specified 'Bounds'.
        //-----------------------------------------------------------------------------
        public Sphere(Bounds bounds)
        {
            mCenter = bounds.center;
            mRadius = bounds.extents.magnitude;
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: ContainsPoint() (Public Function)
        // Desc: Checks if the sphere contains the specified point.
        // Parm: point - Query point.
        // Rtrn: True if the sphere contains the point and false otherwise.
        //-----------------------------------------------------------------------------
        public bool ContainsPoint(Vector3 point)
        {
            // Return result
            return (point - center).sqrMagnitude <= (mRadius * mRadius);
        }

        //-----------------------------------------------------------------------------
        // Name: ProjectPoint() (Public Function)
        // Desc: Projects the specified point onto the sphere surface.
        // Parm: point - The point to be projected.
        // Rtrn: The projected point.
        //-----------------------------------------------------------------------------
        public Vector3 ProjectPoint(Vector3 point)
        {
            return mCenter + (point - mCenter).normalized * mRadius;
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Static Function)
        // Desc: Performs a raycast check between the specified ray and sphere.
        // Parm: ray    - Query ray.
        //       center - Sphere center.
        //       radius - Sphere radius.
        // Rtrn: True if the ray hits the sphere and false otherwise.
        //-----------------------------------------------------------------------------
        public static bool Raycast(Ray ray, Vector3 center, float radius)
        {
            // Store values for easy and fast access
            Vector3 cToO    = ray.origin - center;
            Vector3 rayDir  = ray.direction;

            // Calculate coefficients
            float a = rayDir.x * rayDir.x + rayDir.y * rayDir.y + rayDir.z * rayDir.z;
            float b = 2.0f * (rayDir.x * cToO.x + rayDir.y * cToO.y + rayDir.z * cToO.z);
            float c = (cToO.x * cToO.x + cToO.y * cToO.y +
                cToO.z * cToO.z) - radius * radius;

            // Solve quadratic equation
            if (MathEx.SolveQuadratic(a, b, c, out float t0, out float t1))
            {
                // Only accept solutions which move us forward along the original ray direction
                if (t0 < 0.0f && t1 < 0.0f) return false;
                return true;
            }

            // No hit
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Static Function)
        // Desc: Performs a raycast check between the specified ray and sphere.
        // Parm: ray    - Query ray.
        //       center - Sphere center.
        //       radius - Sphere radius.
        //       t      - Returns the distance from the ray origin to the hit point.
        //                This will always be >= 0, but if the function returns false (no hit),
        //                it should be ignored.
        // Rtrn: True if the ray hits the sphere and false otherwise.
        //-----------------------------------------------------------------------------
        public static bool Raycast(Ray ray, Vector3 center, float radius, out float t)
        {
            // Reset output to sensitive defaults
            t = 0.0f;

            // Store values for easy and fast access
            Vector3 cToO    = ray.origin - center;
            Vector3 rayDir  = ray.direction;

            // Calculate coefficients
            float a = rayDir.x * rayDir.x + rayDir.y * rayDir.y + rayDir.z * rayDir.z;
            float b = 2.0f * (rayDir.x * cToO.x + rayDir.y * cToO.y + rayDir.z * cToO.z);
            float c = (cToO.x * cToO.x + cToO.y * cToO.y +
                cToO.z * cToO.z) - radius * radius;

            // Solve quadratic equation
            if (MathEx.SolveQuadratic(a, b, c, out float t0, out float t1))
            {
                // Only accept solutions which move us forward along the original ray direction
                if (t0 < 0.0f && t1 < 0.0f) return false;

                // Return the first non-negative solution
                t = t0 >= 0.0f ? t0 : t1;
                return true;
            }

            // No hit
            return false;
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: CalculateAABB() (Public Function)
        // Desc: Calculates the sphere's AABB.
        // Rtrn: The sphere's AABB.
        //-----------------------------------------------------------------------------
        public Box CalculateAABB()
        {
            return new Box(mCenter, Vector3Ex.FromValue(mRadius * 2.0f));
        }

        //-----------------------------------------------------------------------------
        // Name: ExtentToTangentPoint() (Public Function)
        // Desc: Calculates the point where a ray from the camera position, tangent to
        //       the sphere, intersects the sphere surface. The input point 'exPt' is
        //       one of the sphere's extent points, obtained by projecting the sphere's
        //       center along the camera's right, left, up, or down vectors.
        // Parm: exPt   - A sphere extent point projected from the sphere center along
        //                a camera axis (right, left, up, or down).
        //       camera - The camera used to compute the extent point.
        // Rtrn: The point of tangency where the ray from the camera position touches
        //       the sphere surface.
        //-----------------------------------------------------------------------------
        public Vector3 ExtentToTangentPoint(Vector3 exPt, Camera camera)
        {
            // Precompute data
            Vector3 normal  = (exPt - mCenter).normalized;                      // Sphere point normal
            Vector3 rayDir  = (exPt - camera.transform.position).normalized;    // Camera ray direction
            if (camera.orthographic) rayDir = camera.transform.forward;         // Use camera forward vector if we're dealing with an ortho camera

            // If the ray is shooting behind the camera, return the original point
            if (Vector3.Dot(camera.transform.forward, rayDir) < 0.0f)
                return exPt;

            // Is the point's normal perpendicular to the camera ray direction?
            float dot = Vector3.Dot(normal, rayDir);
            if (Mathf.Abs(dot) > 1e-5f)
            {
                // We need to rotate the point normal so that it becomes perpendicular to the camera ray
                float angle             = MathEx.SafeAcos(dot) * Mathf.Rad2Deg;
                Vector3 rotationAxis    = Vector3.Cross(rayDir, normal).normalized;
                Quaternion rotation     = Quaternion.AngleAxis(90.0f - angle, rotationAxis);

                // Rotate normal and recalculate point
                normal  = (rotation * normal).normalized;
                exPt    = mCenter + normal * mRadius;
            }

            // Return new point (or the original one if no adjustment was needed)
            return exPt;
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the sphere.
        // Parm: ray - Query ray.
        // Rtrn: True if the ray intersects the sphere and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray)
        {
            return Sphere.Raycast(ray, mCenter, mRadius);
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the sphere.
        // Parm: ray - Query ray.
        //       t   - Returns the distance from the ray origin where the intersection happens.
        //             If the ray doesn't intersect the sphere, this value should be ignored.
        // Rtrn: True if the ray intersects the sphere and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray, out float t)
        {
            return Sphere.Raycast(ray, mCenter, mRadius, out t);
        }

        //-----------------------------------------------------------------------------
        // Name: TestBox() (Public Function)
        // Desc: Determines if the specified box intersects or is fully contained
        //       within 'this' sphere.
        // Parm: box - Query box.
        // Rtrn: True if the specified box intersects or is fully contained within
        //       'this' sphere and false otherwise.
        //-----------------------------------------------------------------------------
        public bool TestBox(Bounds box)
        {
            float sqDist = 0.0f;

            Vector3 min = box.min;
            Vector3 max = box.max;

            // Clip each axis of the sphere center to the AABB
            if (center.x < min.x) sqDist += (min.x - center.x) * (min.x - center.x);
            else if (center.x > max.x) sqDist += (center.x - max.x) * (center.x - max.x);

            if (center.y < min.y) sqDist += (min.y - center.y) * (min.y - center.y);
            else if (center.y > max.y) sqDist += (center.y - max.y) * (center.y - max.y);

            if (center.z < min.z) sqDist += (min.z - center.z) * (min.z - center.z);
            else if (center.z > max.z) sqDist += (center.z - max.z) * (center.z - max.z);

            return sqDist <= radius * radius;
        }

        //-----------------------------------------------------------------------------
        // Name: TestBoxInside() (Public Function)
        // Desc: Determines if the specified box is fully contained within the sphere.
        // Parm: box - query oriented box.
        // Rtrn: True if the specified box is fully contained within the sphere and
        //       false otherwise.
        //-----------------------------------------------------------------------------
        public bool TestBoxInside(Bounds box)
        {
            // Sphere center and squared radius
            float cx = center.x;
            float cy = center.y;
            float cz = center.z;
            float r2 = radius * radius;

            // Extract box min/max for all axes
            float minX = box.min.x, maxX = box.max.x;
            float minY = box.min.y, maxY = box.max.y;
            float minZ = box.min.z, maxZ = box.max.z;

            // Test each of the 8 corners of the box
            // Each corner must lie within the sphere (distance^2 <= radius^2)

            // Corner 0: (min, min, min)
            if ((minX - cx) * (minX - cx) + (minY - cy) * (minY - cy) + (minZ - cz) * (minZ - cz) > r2) return false;

            // Corner 1: (max, min, min)
            if ((maxX - cx) * (maxX - cx) + (minY - cy) * (minY - cy) + (minZ - cz) * (minZ - cz) > r2) return false;

            // Corner 2: (min, max, min)
            if ((minX - cx) * (minX - cx) + (maxY - cy) * (maxY - cy) + (minZ - cz) * (minZ - cz) > r2) return false;

            // Corner 3: (min, min, max)
            if ((minX - cx) * (minX - cx) + (minY - cy) * (minY - cy) + (maxZ - cz) * (maxZ - cz) > r2) return false;

            // Corner 4: (max, max, min)
            if ((maxX - cx) * (maxX - cx) + (maxY - cy) * (maxY - cy) + (minZ - cz) * (minZ - cz) > r2) return false;

            // Corner 5: (min, max, max)
            if ((minX - cx) * (minX - cx) + (maxY - cy) * (maxY - cy) + (maxZ - cz) * (maxZ - cz) > r2) return false;

            // Corner 6: (max, min, max)
            if ((maxX - cx) * (maxX - cx) + (minY - cy) * (minY - cy) + (maxZ - cz) * (maxZ - cz) > r2) return false;

            // Corner 7: (max, max, max)
            if ((maxX - cx) * (maxX - cx) + (maxY - cy) * (maxY - cy) + (maxZ - cz) * (maxZ - cz) > r2) return false;

            // All corners are within the sphere
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: TestSphere() (Public Function)
        // Desc: Determines if the specified sphere intersects or is fully contained
        //       within 'this' sphere.
        // Parm: center - Sphere center.
        //       radius - Sphere radius.
        // Rtrn: True if the specified sphere intersects or is fully contained within
        //       'this' sphere and false otherwise.
        //-----------------------------------------------------------------------------
        public bool TestSphere(Vector3 center, float radius)
        {
            float radiusSum = (mRadius + radius);
            return (mCenter - center).sqrMagnitude <= (radiusSum * radiusSum);
        }
        #endregion
    }
    #endregion
}