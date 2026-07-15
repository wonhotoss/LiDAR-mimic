using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: Box (Public Struct)
    // Desc: Provides storage for axis-aligned bounding boxes and implements relevant
    //       functions.
    //-----------------------------------------------------------------------------
    public struct Box
    {
        #region Private Static Fields
        // Used to avoid memory allocations
        static List<Vector3> sCornerBuffer          = new List<Vector3>();
        static List<Vector3> sScreenCornerBuffer    = new List<Vector3>();
        #endregion

        #region Private Fields
        Vector3 mMin;   // Min corner
        Vector3 mMax;   // Max corner
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: isValid (Public Property)
        // Desc: Returns true if the box is valid and false otherwise.
        //-----------------------------------------------------------------------------
        public bool     isValid { get { return mMin.x != float.MaxValue; } }

        //-----------------------------------------------------------------------------
        // Name: center (Public Property)
        // Desc: Returns or sets the box's center.
        //-----------------------------------------------------------------------------
        public Vector3  center        
        { 
            get { return (mMax + mMin) / 2.0f; } 
            set 
            {
                // Calculate extents
                float ex = (mMax.x - mMin.x) / 2.0f;
                float ey = (mMax.y - mMin.y) / 2.0f;
                float ez = (mMax.z - mMin.z) / 2.0f;

                // Update min point
                mMin.x = value.x - ex;
                mMin.y = value.y - ey;
                mMin.z = value.z - ez;

                // Update max point
                mMax.x = value.x + ex;
                mMax.y = value.y + ey;
                mMax.z = value.z + ez;
            } 
        }  

        //-----------------------------------------------------------------------------
        // Name: size (Public Property)
        // Desc: Returns or sets the box's size. When setting the size, the absolute 
        //       value of the vector will be used in order to ensure the box always has
        //       a positive size.
        //-----------------------------------------------------------------------------
        public Vector3  size        
        {
            get { return mMax - mMin; }
            set 
            {
                // Calculate new extents
                float ex = MathEx.FastAbs(value.x / 2.0f);
                float ey = MathEx.FastAbs(value.y / 2.0f);
                float ez = MathEx.FastAbs(value.z / 2.0f);

                // Calculate current center
                float cx = (mMax.x + mMin.x) / 2.0f;
                float cy = (mMax.y + mMin.y) / 2.0f;
                float cz = (mMax.z + mMin.z) / 2.0f;

                // Update min point
                mMin.x = cx - ex;
                mMin.y = cy - ey;
                mMin.z = cz - ez;

                // Update max point
                mMax.x = cx + ex;
                mMax.y = cy + ey;
                mMax.z = cz + ez;
            } 
        }

        //-----------------------------------------------------------------------------
        // Name: extents (Public Property)
        // Desc: Returns the box's extents (i.e. half the size).
        //-----------------------------------------------------------------------------
        public Vector3  extents { get { return (mMax - mMin) / 2.0f; } }

        //-----------------------------------------------------------------------------
        // Name: min (Public Property)
        // Desc: Returns or sets the box's minimum corner.
        //-----------------------------------------------------------------------------
        public Vector3  min     { get { return mMin; } set { mMin = value; } }

        //-----------------------------------------------------------------------------
        // Name: max (Public Property)
        // Desc: Returns or sets the box's maximum corner.
        //-----------------------------------------------------------------------------
        public Vector3  max     { get { return mMax; } set { mMax = value; } }
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------
        // Name: Box() (Public Constructor)
        // Desc: Creates a box from the specified center and size values.
        // Parm: center - Box center.
        //       size   - Box size.
        //-----------------------------------------------------------------------------
        public Box(Vector3 center, Vector3 size)
        {
            // Calculate extents
            float ex = size.x / 2.0f;
            float ey = size.y / 2.0f;
            float ez = size.z / 2.0f;

            // Init min corner
            mMin.x = center.x - ex;
            mMin.y = center.y - ey;
            mMin.z = center.z - ez;

            // Init max corner
            mMax.x = center.x + ex;
            mMax.y = center.y + ey;
            mMax.z = center.z + ez;
        }

        //-----------------------------------------------------------------------------
        // Name: Box() (Public Constructor)
        // Desc: Creates a box from a 'Bounds' instance.
        // Parm: bounds - The 'Bounds' instance.
        //-----------------------------------------------------------------------------
        public Box(Bounds bounds)
        {
            // Store min and max corners
            mMin = bounds.min;
            mMax = bounds.max;
        }

        //-----------------------------------------------------------------------------
        // Name: Box() (Public Constructor)
        // Desc: Creates a box from a collection of 3D points.
        // Parm: points - Collection of 3D points enclosed by the box.
        //-----------------------------------------------------------------------------
        public Box(IEnumerable<Vector3> points)
        {
            // Calculate point cloud min/max values
            mMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            mMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (Vector3 pt in points)
            {
                mMin = Vector3.Min(pt, mMin);
                mMax = Vector3.Max(pt, mMax);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: Box() (Public Constructor)
        // Desc: Creates a box from a collection of 2D points.
        // Parm: points - Collection of 2D points enclosed by the box.
        //-----------------------------------------------------------------------------
        public Box(IEnumerable<Vector2> points)
        {
            // Calculate point cloud min/max values
            mMin = new Vector2(float.MaxValue, float.MaxValue);
            mMax = new Vector2(float.MinValue, float.MinValue);
            foreach (Vector2 pt in points)
            {
                mMin = Vector2.Min(pt, mMin);
                mMax = Vector2.Max(pt, mMax);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: Box() (Public Constructor)
        // Desc: Creates a box from the specified oriented box. The created box will be
        //       large enough to enclose the specified oriented box.
        // Parm: obb - The oriented box which enclosed by 'this' box.
        //-----------------------------------------------------------------------------
        public Box(OBox obb)
        {
            // Init box to default model space values
            mMin.x = mMin.y = mMin.z = -0.5f;
            mMax.x = mMax.y = mMax.z =  0.5f;

            // Transform using the OBB's transform matrix
            Transform(obb.transform);
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: GetInvalid() (Public Static Function)
        // Desc: Returns an invalid box.
        // Rtrn: An invalid box.
        //-----------------------------------------------------------------------------
        public static Box GetInvalid()
        {
            Box box = new Box();
            box.mMin.x = float.MaxValue;
            return box;
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: CalculateClosestPoint() (Public Function)
        // Desc: Calculates the box point which is closest to 'pt'.
        // Parm: pt - Query point.
        // Rtrn: The box point which is closest to 'pt'.
        //-----------------------------------------------------------------------------
        public Vector3 CalculateClosestPoint(Vector3 pt)
        {
            // Precompute data
            Vector3 c       = center;
            Vector3 toPt    = pt - c;       // Vector from box center to query point
            Vector3 ex      = extents;      // Box extents

            // Initialize closest point to box center 
            Vector3 closestPt = c;

            // For each box axis, project the 'toPt' vector, clip projection to box extents and offset the closest point
            Vector3 axis    = Vector3.right;
            float   dot     = Mathf.Clamp(toPt[0], -extents[0], extents[0]);
            closestPt       += axis * dot;

            axis            = Vector3.up;
            dot             = Mathf.Clamp(toPt[1], -extents[1], extents[1]);
            closestPt       += axis * dot;

            axis            = Vector3.forward;
            dot             = Mathf.Clamp(toPt[2], -extents[2], extents[2]);
            closestPt       += axis * dot;

            // Return closest point
            return closestPt;
        }

        //-----------------------------------------------------------------------------
        // Name: ToBounds() (Public Function)
        // Desc: Converts the box to a 'Bounds' instance and returns it.
        // Rtrn: The 'Bounds' instance which describes the box.
        //-----------------------------------------------------------------------------
        public Bounds ToBounds()
        {
            return new Bounds(center, size);
        }

        //-----------------------------------------------------------------------------
        // Name: EnclosePoint() (Public Function)
        // Desc: Encloses the specified point.
        // Parm: point - The point to be enclosed.
        //-----------------------------------------------------------------------------
        public void EnclosePoint(Vector3 point)
        {
            // Update min/max to enclose point
            if (point.x < mMin.x) mMin.x = point.x;
            if (point.y < mMin.y) mMin.y = point.y;
            if (point.z < mMin.z) mMin.z = point.z;
            if (point.x > mMax.x) mMax.x = point.x;
            if (point.y > mMax.y) mMax.y = point.y;
            if (point.z > mMax.z) mMax.z = point.z;
        }

        //-----------------------------------------------------------------------------
        // Name: EncloseBox() (Public Function)
        // Desc: Encloses the specified box.
        // Parm: box - The box to be enclosed. Assumed to be valid.
        //-----------------------------------------------------------------------------
        public void EncloseBox(Box box)
        {
            EnclosePoint(box.mMin);
            EnclosePoint(box.mMax);
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateCorners() (Public Function)
        // Desc: Calculates the box corners and stores them inside the 'corners' list.
        // Parm: corners - List of 'Vector3' which returns the corner positions.
        //-----------------------------------------------------------------------------
        public void CalculateCorners(List<Vector3> corners)
        {
            // Clear output
            corners.Clear();

            // Cache data
            Vector3 e = extents;

            // Calculate front face points
            corners.Add(mMin);
            corners.Add(new Vector3(mMin.x, mMax.y, mMin.z));
            corners.Add(new Vector3(mMax.x, mMax.y, mMin.z));
            corners.Add(new Vector3(mMax.x, mMin.y, mMin.z));

            // Calculate back face points
            corners.Add(mMax);
            corners.Add(new Vector3(mMin.x, mMax.y, mMax.z));
            corners.Add(new Vector3(mMin.x, mMin.y, mMax.z));
            corners.Add(new Vector3(mMax.x, mMin.y, mMax.z));
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateScreenRect() (Public Function)
        // Desc: Calculates the box's screen rectangle.
        // Parm: camera - The camera which interacts with or renders the box.
        // Rtrn: The rectangle which encloses the box in screen space or a rectangle with
        //       negative width and height if the box corners are behind the camera.
        //-----------------------------------------------------------------------------
        public Rect CalculateScreenRect(Camera camera)
        {
            // Convert OBB corners to screen world coords
            CalculateCorners(sCornerBuffer);
            camera.WorldToScreenPoints(sCornerBuffer, sScreenCornerBuffer);

            // Create a rectangle from the screen points and return it
            return RectEx.FromPoints(sScreenCornerBuffer);
        }

        //-----------------------------------------------------------------------------
        // Name: Transform() (Public Function)
        // Desc: Transforms the box with the specified transform matrix.
        // Parm: mtx - The transform matrix used to transform the box.
        //-----------------------------------------------------------------------------
        public void Transform(Matrix4x4 mtx)
        {
            // Store matrix local axes
            Vector3 right   = mtx.GetColumn(0);
            Vector3 up      = mtx.GetColumn(1);
            Vector3 forward = mtx.GetColumn(2);

            // Calculate extent vectors
            Vector3 extents     = (mMax - mMin) / 2.0f;
            Vector3 exRight     = right * extents.x;
            Vector3 exUp        = up * extents.y;
            Vector3 exForward   = forward * extents.z;

            // Calculate the new extents
            float exX = MathEx.FastAbs(exRight.x) + MathEx.FastAbs(exUp.x) + MathEx.FastAbs(exForward.x);
            float exY = MathEx.FastAbs(exRight.y) + MathEx.FastAbs(exUp.y) + MathEx.FastAbs(exForward.y);
            float exZ = MathEx.FastAbs(exRight.z) + MathEx.FastAbs(exUp.z) + MathEx.FastAbs(exForward.z);

            // Update min and max corners
            Vector3 center = mtx.MultiplyPoint3x4((mMax + mMin) / 2.0f);
            mMin.x = center.x - exX;
            mMin.y = center.y - exY;
            mMin.z = center.z - exZ;
            mMax.x = center.x + exX;
            mMax.y = center.y + exY;
            mMax.z = center.z + exZ;      
        }

        //-----------------------------------------------------------------------------
        // Name: ContainsPoint() (Public Function)
        // Desc: Checks if the box contains the specified point.
        // Parm: point - The query point.
        // Rtrn: True if the box contains the point and false otherwise.
        //-----------------------------------------------------------------------------
        public bool ContainsPoint(Vector3 point)
        {
            // Return result
            return point.x >= mMin.x && point.x <= mMax.x &&
                   point.y >= mMin.y && point.y <= mMax.y &&
                   point.z >= mMin.z && point.z <= mMax.z;
        }

        //-----------------------------------------------------------------------------
        // Name: TestSphere() (Public Function)
        // Desc: Determines if the specified sphere intersects or is fully contained
        //       within the box.
        // Parm: center - Sphere center.
        //       radius - Sphere radius.
        // Rtrn: True if the specified sphere intersects or is fully contained within
        //       the box and false otherwise.
        //-----------------------------------------------------------------------------
        public bool TestSphere(Vector3 center, float radius)
        {
            float distSq = 0.0f;

            // For each axis, add squared distance to box if center is outside
            float minX = mMin.x, maxX = mMax.x;
            if (center.x < minX) distSq += (minX - center.x) * (minX - center.x);
            else if (center.x > maxX) distSq += (center.x - maxX) * (center.x - maxX);

            float minY = mMin.y, maxY = mMax.y;
            if (center.y < minY) distSq += (minY - center.y) * (minY - center.y);
            else if (center.y > maxY) distSq += (center.y - maxY) * (center.y - maxY);

            float minZ = mMin.z, maxZ = mMax.z;
            if (center.z < minZ) distSq += (minZ - center.z) * (minZ - center.z);
            else if (center.z > maxZ) distSq += (center.z - maxZ) * (center.z - maxZ);

            return distSq <= radius * radius;
        }
        #endregion
    }
    #endregion
}