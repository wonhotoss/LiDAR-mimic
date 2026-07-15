using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: Polyhedron (Public Class)
    // Desc: Provides storage for 3D polyhedrons and implements relevant functions.
    //-----------------------------------------------------------------------------
    public class Polyhedron
    {
        #region Private Fields
        Plane[] mPlanes;    // The planes that describe the space enclosed by the polyhedron. All planes point inwards.
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: isValid (Public Property)
        // Desc: Returns true if the polyhedron is valid and false otherwise.
        //-----------------------------------------------------------------------------
        public bool isValid { get { return mPlanes != null; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: FromScreenRect() (Public Function)
        // Desc: Creates a polyhedron that encloses the space between the near and far
        //       clip plane projections of the specified screen rectangle inside the
        //       specified camera's frustum. That is to say, the screen rectangle's corners
        //       are converted to world space on the near and far camera clip planes and
        //       rays are traced to connect the world corners.
        // Parm: camera     - camera whose frustum contains the resulting polyhedron.
        //       screenRect - a screen rectangle that must be converted to a polyhedron
        //                    by projecting its corners on the camera's near and far clip
        //                    planes. The function clips the rectangle to the camera's viewport.
        // Rtrn: True if the polyhedron is successfully created and false otherwise. For
        //       example, this function might return false if the specified screen rectangle
        //       is completely outside the camera's viewport.
        //-----------------------------------------------------------------------------
        public bool FromScreenRect(Rect screenRect, Camera camera)
        {
            // Clip screen rect to camera viewport rect
            if (!screenRect.Clip(camera.pixelRect, out Rect r))
                return false;

            // Project the rectangle corner points onto the camera near clip plane.
            // Order: Bottom-Left, Top-Left, Top-Right, Bottom-Right.
            Vector3 n0 = camera.ScreenToWorldPoint(new Vector3(r.xMin, r.yMin, camera.nearClipPlane));
            Vector3 n1 = camera.ScreenToWorldPoint(new Vector3(r.xMin, r.yMax, camera.nearClipPlane));
            Vector3 n2 = camera.ScreenToWorldPoint(new Vector3(r.xMax, r.yMax, camera.nearClipPlane));
            Vector3 n3 = camera.ScreenToWorldPoint(new Vector3(r.xMax, r.yMin, camera.nearClipPlane));

            // Project the rectangle corner points onto the camera far clip plane.
            // Order: Bottom-Left, Top-Left, Top-Right, Bottom-Right.
            Vector3 f0 = camera.ScreenToWorldPoint(new Vector3(r.xMin, r.yMin, camera.farClipPlane));
            Vector3 f1 = camera.ScreenToWorldPoint(new Vector3(r.xMin, r.yMax, camera.farClipPlane));
            Vector3 f2 = camera.ScreenToWorldPoint(new Vector3(r.xMax, r.yMax, camera.farClipPlane));
            Vector3 f3 = camera.ScreenToWorldPoint(new Vector3(r.xMax, r.yMin, camera.farClipPlane));

            //-----------------------------------------------------------------------------
            // Generate the planes that enclose the 3D space. Planes point inwards.
            // (CW ordering when looking at the plane from the inside)
            //-----------------------------------------------------------------------------
            mPlanes     = new Plane[6];
            mPlanes[0]  = new Plane(n0, n1, f1);    // Left plane
            mPlanes[1]  = new Plane(n3, f3, f2);    // Right plane
            mPlanes[2]  = new Plane(n0, f0, f3);    // Bottom plane
            mPlanes[3]  = new Plane(f1, n1, n2);    // Top plane
            mPlanes[4]  = new Plane(n0, n3, n2);    // Back plane
            mPlanes[5]  = new Plane(f0, f1, f2);    // Front plane

            // Validate planes
            if (!ValidatePlanes())
            {
                mPlanes = null;
                return false;
            }

            // Success!
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: TestSphere() (Public Function)
        // Desc: Determines if the specified sphere intersects or is fully contained
        //       within the polyhedron.
        // Parm: center - Sphere center.
        //       radius - Sphere radius.
        // Rtrn: True if the specified sphere intersects or is fully contained within
        //       the polyhedron and false otherwise.
        //-----------------------------------------------------------------------------
        public bool TestSphere(Vector3 center, float radius)
        {
            // Validate call
            if (!isValid)
                return false;

            // Loop through each plane
            int planeCount = mPlanes.Length;
            for (int i = 0; i < planeCount; ++i)
            {
                // If the distance from the plane is < -radius it means the sphere 
                // lies behind the plane and more than 'radius' units away from it.
                // If that is the case, it means the sphere lies completely behind
                // this plane and it can't possibly intersect the polyhedron.
                if (mPlanes[i].GetDistanceToPoint(center) < -radius) 
                    return false;
            }

            // We have an intersection
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: TestSphereInside() (Public Function)
        // Desc: Determines if the specified sphere is fully contained within the polyhedron.
        // Parm: sphere - Query sphere.
        // Rtrn: True if the specified sphere is fully contained within the polyhedron
        //       and false otherwise.
        //-----------------------------------------------------------------------------
        public bool TestSphereInside(Sphere sphere)
        {
            // Validate call
            if (!isValid)
                return false;

            // Cache sphere data
            Vector3 center  = sphere.center;
            float   radius  = sphere.radius;

            // Loop through each plane
            int planeCount = mPlanes.Length;
            for (int i = 0; i < planeCount; ++i)
            {
                // If the distance from the plane is < radius it means the sphere is either spanning
                // or completely behind the plane. In that case, it can't possibly be fully contained
                // by the polyhedron.
                if (mPlanes[i].GetDistanceToPoint(center) < radius)
                    return false;
            }

            // The sphere is fully contained
            return true;
        }
        
        //-----------------------------------------------------------------------------
        // Name: TestBox() (Public Function)
        // Desc: Determines if the specified box intersects or is fully contained
        //       within the polyhedron.
        // Parm: box - Query box.
        // Rtrn: True if the specified box intersects or is fully contained within
        //       the polyhedron and false otherwise.
        //-----------------------------------------------------------------------------
        public bool TestBox(Bounds box)
        {
            // Store box data
            Vector3 center  = box.center;
            Vector3 extents = box.extents;

            // Loop through each plane
            int planeCount = mPlanes.Length;
            for (int i = 0; i < planeCount; ++i)
            {
                // Calculate the box's size along the plane's normal
                float radius = MathEx.FastAbs(Vector3.Dot(extents, mPlanes[i].normal));

                // Calculate the distance between the box's center and the plane
                float d = mPlanes[i].GetDistanceToPoint(center);

                // If the entire box is behind the plane, there's no intersection
                if (d < -radius) return false;
            }

            // We have an intersection
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: TestOBoxInside() (Public Function)
        // Desc: Determines if the specified oriented box is fully contained within the
        //       polyhedron.
        // Parm: box - query oriented box.
        // Rtrn: True if the specified box is fully contained within the polyhedron and
        //       false otherwise.
        //-----------------------------------------------------------------------------
        public bool TestOBoxInside(OBox box)
        {
            // Validate call
            if (!box.isValid)
                return false;

            // Store box data
            Vector3 center  = box.center;
            Vector3 extents = box.rotation * box.extents;

            // Loop through each plane
            int planeCount = mPlanes.Length;
            for (int i = 0; i < planeCount; ++i)
            {
                // Calculate the box's size along the plane's normal
                float radius = MathEx.FastAbs(Vector3.Dot(extents, mPlanes[i].normal));

                // Calculate the distance between the box's center and the plane
                float d = mPlanes[i].GetDistanceToPoint(center);

                // The box must reside at least 'radius' units in front of the plane. 
                // Otherwise, it means the box is either spanning or is behind the plane.
                if (d < radius) return false;
            }

            // The box is inside
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: TestPointInside() (Public Function)
        // Desc: Determines if the specified point is inside the polyhedron.
        // Parm: pt - Query point.
        // Rtrn: True if the specified point is inside the polyhedron and false otherwise.
        //-----------------------------------------------------------------------------
        public bool TestPointInside(Vector3 pt)
        {
            // If the point is behind one of the planes, it can't be inside the polyhedron
            int planeCount = mPlanes.Length;
            for (int i = 0; i < planeCount; ++i)
            {
                // If the distance between the point and the plane is < 0, it means the point is
                // behind the plane and it can't possibly lie inside the polyhedron.
                if (mPlanes[i].GetDistanceToPoint(pt) < -1e-5f)
                    return false;
            }

            // The point is inside
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: TestKDOPInside() (Public Function)
        // Desc: Determines if the specified kdop is fully contained within the polyhedron.
        // Parm: kdop - Query kdop.
        // Rtrn: True if the specified kdop is fully contained within the polyhedron and
        //       false otherwise.
        //-----------------------------------------------------------------------------
        public bool TestKDOPInside(IKDOP kdop)
        {
            // All points must be inside
            int vertexCount = kdop.vertexCount;
            for (int i = 0; i < vertexCount; ++i)
            {
                // Is this vertex inside?
                if (!TestPointInside(kdop.GetVertex(i)))
                    return false;
            }

            // All points are inside
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: TestKDOPInside() (Public Function)
        // Desc: Determines if the specified kdop is fully contained within the polyhedron.
        // Parm: kdop         - Query kdop.
        //       transformMtx - Transforms the KDOP verts from model space to the same
        //                      space the polyhedron is in.
        // Rtrn: True if the specified kdop is fully contained within the polyhedron and
        //       false otherwise.
        //-----------------------------------------------------------------------------
        public bool TestKDOPInside(IKDOP kdop, Matrix4x4 transformMtx)
        {
            // All points must be inside
            int vertexCount = kdop.vertexCount;
            for (int i = 0; i < vertexCount; ++i)
            {
                // Is this vertex inside?
                if (!TestPointInside(transformMtx.MultiplyPoint(kdop.GetVertex(i))))
                    return false;
            }

            // All points are inside
            return true;
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: ValidatePlanes() (Private Function)
        // Desc: Validates the polyhedron planes. Called when the planes are updated to
        //       ensure they were constructed from valid input data.
        // Rtrn: True if the planes are valid and false otherwise.
        //-----------------------------------------------------------------------------
        bool ValidatePlanes()
        {
            // No planes?
            if (mPlanes == null)
                return false;

            // Loop through each world plane
            int planeCount = mPlanes.Length;
            for (int i = 0; i < planeCount; ++i)
            {
                // Validate plane distance
                float d = mPlanes[i].distance;
                if (float.IsInfinity(d) || float.IsNaN(d)) return false;

                // Validate plane normal
                Vector3 normal = mPlanes[i].normal;            
                if (float.IsInfinity(normal.x) || float.IsInfinity(normal.y) || float.IsInfinity(normal.z)) return false;
                if (float.IsNaN(normal.x) || float.IsNaN(normal.y) || float.IsNaN(normal.z)) return false;
            }

            // All planes are valid
            return true;
        }
        #endregion
    }
    #endregion
}
