using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: OBox (Public Struct)
    // Desc: Provides storage for oriented bounding boxes and implements relevant
    //       functions.
    //-----------------------------------------------------------------------------
    public struct OBox
    {
        #region Private Static Fields
        // Used to avoid memory allocations
        static List<Vector3> sCornerBuffer          = new List<Vector3>();
        static List<Vector3> sScreenCornerBuffer    = new List<Vector3>();
        #endregion

        #region Private Fields
        Vector3     mCenter;    // Center
        Vector3     mSize;      // Size
        Quaternion  mRotation;  // Rotation
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

        #region Public Constructors
        //-----------------------------------------------------------------------------
        // Name: OBox() (Public Constructor)
        // Desc: Creates a box from the specified 'Bounds' instance.
        // Parm: bounds - The 'Bounds' instance.
        //-----------------------------------------------------------------------------
        public OBox(Bounds bounds)
        {
            // Store data
            mCenter     = bounds.center;
            mSize       = bounds.size;
            mRotation   = Quaternion.identity;
        }

        //-----------------------------------------------------------------------------
        // Name: OBox() (Public Constructor)
        // Desc: Creates a box from the specified 'Box' instance.
        // Parm: box - The 'Box' instance.
        //-----------------------------------------------------------------------------
        public OBox(Box box)
        {
            // Store data
            mCenter     = box.center;
            mSize       = box.size;
            mRotation   = Quaternion.identity;
        }

        //-----------------------------------------------------------------------------
        // Name: OBox() (Public Constructor)
        // Desc: Creates a box from the specified center. Default size is 0 and default
        //       rotation is identity.
        // Parm: center - Box center.
        //-----------------------------------------------------------------------------
        public OBox(Vector3 center)
        {
            // Store data
            mCenter     = center;
            mSize       = Vector3.zero;
            mRotation   = Quaternion.identity;
        }

        //-----------------------------------------------------------------------------
        // Name: OBox() (Public Constructor)
        // Desc: Creates a box from the specified center and size. Default rotation is
        //       identity.
        // Parm: center - Box center;
        //       size   - Box size.
        //-----------------------------------------------------------------------------
        public OBox(Vector3 center, Vector3 size)
        {
            // Store data
            mCenter     = center;
            mSize       = size;
            mRotation   = Quaternion.identity;
        }

        //-----------------------------------------------------------------------------
        // Name: OBox() (Public Constructor)
        // Desc: Creates a box from the specified center, size and rotation.
        // Parm: center     - Box center.
        //       size       - Box size.
        //       rotation   - Box rotation.
        //-----------------------------------------------------------------------------
        public OBox(Vector3 center, Vector3 size, Quaternion rotation)
        {
            // Store data
            mCenter     = center;
            mSize       = size;
            mRotation   = rotation;
        }

        //-----------------------------------------------------------------------------
        // Name: OBox() (Public Constructor)
        // Desc: Creates a box from the specified center and rotation. Default size 
        //       is 0.
        // Parm: center   - Box center.
        //       rotation - Box rotation.
        //-----------------------------------------------------------------------------
        public OBox(Vector3 center, Quaternion rotation)
        {
            // Store data
            mCenter     = center;
            mSize       = Vector3.zero;
            mRotation   = rotation;
        }

        //-----------------------------------------------------------------------------
        // Name: OBox() (Public Constructor)
        // Desc: Creates a box from the specified rotation. Default center and size are 0.
        // Parm: rotation - Box rotation.
        //-----------------------------------------------------------------------------
        public OBox(Quaternion rotation)
        {
            // Store data
            mCenter     = Vector3.zero;
            mSize       = Vector3.zero;
            mRotation   = rotation;
        }

        //-----------------------------------------------------------------------------
        // Name: OBox() (Public Constructor)
        // Desc: Creates a box from the specified 'Bounds' instance and rotation.
        // Parm: bounds   - The 'Bounds instance'.
        //       rotation - Box rotation.
        //-----------------------------------------------------------------------------
        public OBox(Bounds bounds, Quaternion rotation)
        {
            // Store data
            mCenter     = bounds.center;
            mSize       = bounds.size;
            mRotation   = rotation;
        }

        //-----------------------------------------------------------------------------
        // Name: OBox() (Public Constructor)
        // Desc: Creates a box from the specified 'Box' instance and rotation.
        // Parm: box      - The 'Box' instance.
        //       rotation - Box rotation.
        //-----------------------------------------------------------------------------
        public OBox(Box box, Quaternion rotation)
        {
            // Store data
            mCenter     = box.center;
            mSize       = box.size;
            mRotation   = rotation;
        }

        //-----------------------------------------------------------------------------
        // Name: OBox() (Public Constructor)
        // Desc: Creates a box from the specified 'Box' and 'Transform'.
        // Parm: box        - The 'Box' instance.
        //       transform  - The transform used to position, scale and rotate the box.
        //-----------------------------------------------------------------------------
        public OBox(Box box, Transform transform)
        {
            // Store data
            mSize       = Vector3.Scale(box.size.Abs(), transform.lossyScale.Abs());
            mCenter     = transform.TransformPoint(box.center);
            mRotation   = transform.rotation;
        }

        //-----------------------------------------------------------------------------
        // Name: OBox() (Public Constructor)
        // Desc: Creates a box from the specified 'Bounds' and 'Transform'.
        // Parm: bounds     - The 'Bounds' instance.
        //       transform  - The transform used to position, scale and rotate the box.
        //-----------------------------------------------------------------------------
        public OBox(Bounds bounds, Transform transform)
        {
            // Store data
            Vector3 lossyAbs = transform.lossyScale.Abs();
            mSize = new Vector3(
                bounds.size.x * lossyAbs.x,
                bounds.size.y * lossyAbs.y,
                bounds.size.z * lossyAbs.z
            );

            mCenter   = transform.TransformPoint(bounds.center);
            mRotation = transform.rotation;
        }

        //-----------------------------------------------------------------------------
        // Name: OBox() (Copy Constructor)
        // Desc: Creates a box from the specified 'OBox'.
        // Parm: src - The source 'OBox' whose data will be copied.
        //-----------------------------------------------------------------------------
        public OBox(OBox src)
        {
            // Copy data
            mSize       = src.mSize;
            mCenter     = src.mCenter;
            mRotation   = src.mRotation;
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: GetInvalid() (Public Static Function)
        // Desc: Returns an invalid box.
        // Rtrn: An invalid box.
        //-----------------------------------------------------------------------------
        public static OBox GetInvalid()
        {
            OBox box = new OBox();
            box.mSize.x = -1.0f;
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
            Vector3 toPt    = pt - mCenter;     // Vector from box center to query point
            Vector3 ex      = extents;          // Box extents

            // Initialize closest point to box center 
            Vector3 closestPt = mCenter;

            // For each box axis, project the 'toPt' vector, clip projection to box extents and offset the closest point
            Vector3 axis    = mRotation * Vector3.right;
            float   dot     = Mathf.Clamp(Vector3.Dot(axis, toPt), -extents[0], extents[0]);
            closestPt       += axis * dot;

            axis            = mRotation * Vector3.up;
            dot             = Mathf.Clamp(Vector3.Dot(axis, toPt), -extents[1], extents[1]);
            closestPt       += axis * dot;

            axis            = mRotation * Vector3.forward;
            dot             = Mathf.Clamp(Vector3.Dot(axis, toPt), -extents[2], extents[2]);
            closestPt       += axis * dot;

            // Return closest point
            return closestPt;
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

            // Convert the ray in box model space so we can use a regular AABB/ray intersection test
            Matrix4x4 boxMtx = transform;
            Ray modelRay = ray.Transform(boxMtx.inverse);
            if (modelRay.direction.sqrMagnitude == 0.0f) return false;

            // Cast the ray against the model unit box
            Bounds unitBox = new Bounds(Vector3.zero, Vector3.one);
            if (unitBox.IntersectRay(modelRay, out t))
            {
                // Convert the intersection point to the original space and calculate the intersection distance
                Vector3 iPt = boxMtx.MultiplyPoint(modelRay.GetPoint(t));
                t = (iPt - ray.origin).magnitude;
                return true;
            }

            // No intersection
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: EnclosePoint() (Public Function)
        // Desc: Encloses the specified point.
        // Parm: point - The point to be enclosed.
        //-----------------------------------------------------------------------------
        public void EnclosePoint(Vector3 point)
        {
            // X axis
            float dot = Vector3.Dot((point - mCenter), right);
            if (dot > extents.x)
            {
                float delta = dot - extents.x;
                mSize.x += delta;
                mCenter += right * delta * 0.5f;
            }
            else
            if (dot < -extents.x)
            {
                float delta = dot + extents.x;
                mSize.x += MathEx.FastAbs(delta);
                mCenter += right * delta * 0.5f;
            }

            // Y axis
            dot = Vector3.Dot((point - mCenter), up);
            if (dot > extents.y)
            {
                float delta = dot - extents.y;
                mSize.y += delta;
                mCenter += up * delta * 0.5f;
            }
            else
            if (dot < -extents.y)
            {
                float delta = dot + extents.y;
                mSize.y += MathEx.FastAbs(delta);
                mCenter += up * delta * 0.5f;
            }

            // Z axis
            dot = Vector3.Dot((point - mCenter), forward);
            if (dot > extents.z)
            {
                float delta = dot - extents.z;
                mSize.z += delta;
                mCenter += forward * delta * 0.5f;
            }
            else
            if (dot < -extents.z)
            {
                float delta = dot + extents.z;
                mSize.z += MathEx.FastAbs(delta);
                mCenter += forward * delta * 0.5f;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: EncloseBox() (Public Function)
        // Desc: Encloses the specified box.
        // Parm: box - The box to be enclosed. Assumed to be valid.
        //-----------------------------------------------------------------------------
        public void EncloseBox(OBox box)
        {
            // Store local axes of the other OBB
            Vector3 otherRightAxis = box.right;
            Vector3 otherUpAxis = box.up;
            Vector3 otherExtents = box.extents;

            // Enclose front points
            Vector3 otherFaceCenter = box.center - box.forward * otherExtents.z;
            EnclosePoint(otherFaceCenter - otherRightAxis * otherExtents.x + otherUpAxis * otherExtents.y);
            EnclosePoint(otherFaceCenter + otherRightAxis * otherExtents.x + otherUpAxis * otherExtents.y);
            EnclosePoint(otherFaceCenter + otherRightAxis * otherExtents.x - otherUpAxis * otherExtents.y);
            EnclosePoint(otherFaceCenter - otherRightAxis * otherExtents.x - otherUpAxis * otherExtents.y);

            // Enclose back points
            otherFaceCenter = box.center + box.forward * otherExtents.z;
            EnclosePoint(otherFaceCenter + otherRightAxis * otherExtents.x + otherUpAxis * otherExtents.y);
            EnclosePoint(otherFaceCenter - otherRightAxis * otherExtents.x + otherUpAxis * otherExtents.y);
            EnclosePoint(otherFaceCenter - otherRightAxis * otherExtents.x - otherUpAxis * otherExtents.y);
            EnclosePoint(otherFaceCenter + otherRightAxis * otherExtents.x - otherUpAxis * otherExtents.y);
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateCorners() (Public Function)
        // Desc: Calculates the box corners and stores them inside the 'corners' array.
        // Parm: corners - A 'Vector3' array large enough to store at least 8 items.
        //-----------------------------------------------------------------------------
        public void CalculateCorners(Vector3[] corners)
        {
            // Cache data
            Vector3 e       = extents;
            Vector3 right   = this.right;
            Vector3 up      = this.up;
            Vector3 forward = this.forward;

            // Calculate front face points
            corners[0] = mCenter - right * e.x - up * e.y - forward * e.z;
            corners[1] = mCenter - right * e.x + up * e.y - forward * e.z;
            corners[2] = mCenter + right * e.x + up * e.y - forward * e.z;
            corners[3] = mCenter + right * e.x - up * e.y - forward * e.z;

            // Calculate back face points
            corners[4] = mCenter + right * e.x - up * e.y + forward * e.z;
            corners[5] = mCenter + right * e.x + up * e.y + forward * e.z;
            corners[6] = mCenter - right * e.x + up * e.y + forward * e.z;
            corners[7] = mCenter - right * e.x - up * e.y + forward * e.z;
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
            Vector3 e       = extents;
            Vector3 right   = this.right;
            Vector3 up      = this.up;
            Vector3 forward = this.forward;

            // Calculate front face points
            corners.Add(mCenter - right * e.x - up * e.y - forward * e.z);
            corners.Add(mCenter - right * e.x + up * e.y - forward * e.z);
            corners.Add(mCenter + right * e.x + up * e.y - forward * e.z);
            corners.Add(mCenter + right * e.x - up * e.y - forward * e.z);

            // Calculate back face points
            corners.Add(mCenter + right * e.x - up * e.y + forward * e.z);
            corners.Add(mCenter + right * e.x + up * e.y + forward * e.z);
            corners.Add(mCenter - right * e.x + up * e.y + forward * e.z);
            corners.Add(mCenter - right * e.x - up * e.y + forward * e.z);
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateInwardFaceQuads() (Public Function)
        // Desc: Calculates the quads that represent the inward box faces and stores
        //       them in 'quads'.
        // Parm: quads - Returns the box face quads.
        //-----------------------------------------------------------------------------
        public void CalculateInwardFaceQuads(List<Quad> quads)
        {
            // Clear output
            quads.Clear();

            // Cache data
            Vector3 e       = extents;
            Vector3 right   = this.right;
            Vector3 up      = this.up;
            Vector3 forward = this.forward;

            // Front & Back faces
            quads.Add(new Quad { center = mCenter - forward * e.z, width = mSize.x, height = mSize.y, normal =  forward, widthAxis = right });
            quads.Add(new Quad { center = mCenter + forward * e.z, width = mSize.x, height = mSize.y, normal = -forward, widthAxis = right });

            // Left & Right faces
            quads.Add(new Quad { center = mCenter - right * e.x, width = mSize.z, height = mSize.y, normal =  right, widthAxis = forward });
            quads.Add(new Quad { center = mCenter + right * e.x, width = mSize.z, height = mSize.y, normal = -right, widthAxis = forward });

            // Bottom & Top faces
            quads.Add(new Quad { center = mCenter - up * e.y, width = mSize.x, height = mSize.z, normal =  up, widthAxis = right });
            quads.Add(new Quad { center = mCenter + up * e.y, width = mSize.x, height = mSize.z, normal = -up, widthAxis = right });
        }

        //-----------------------------------------------------------------------------
        // Name: VsPlane() (Public Function)
        // Desc: Classifies the box against the specified plane.
        // Parm: plane      - Query plane.
        //       onPlaneEps - An epsilon value which represents the maximum distance from
        //                    the plane where points are considered to be on the plane.
        // Rtrn: A member of 'EPlaneClassify'.
        //-----------------------------------------------------------------------------
        public EPlaneClassify VsPlane(Plane plane, float onPlaneEps = 1e-5f)
        {
            // Counter variables
            int frontCount      = 0;
            int behindCount     = 0;
            int onPlaneCount    = 0;

            // Loop through each corner point
            CalculateCorners(sCornerBuffer);
            int ptCount = sCornerBuffer.Count;
            for (int i = 0; i < ptCount; ++i)
            {
                // Get the distance between the point and the plane
                float d = plane.GetDistanceToPoint(sCornerBuffer[i]);

                // Increase counters
                if (d < -onPlaneEps)        ++behindCount;
                else if (d > onPlaneEps)    ++frontCount;
                else
                {
                    // Note: We the point is on the plane, we increase all counters. Makes things easier later.
                    ++onPlaneCount;
                    ++behindCount;
                    ++frontCount;
                }
            }

            // Establish the result
            if (frontCount == ptCount)      return EPlaneClassify.InFront;
            if (behindCount == ptCount)     return EPlaneClassify.Behind;
            if (onPlaneCount == ptCount)    return EPlaneClassify.OnPlane;

            // Spanning
            return EPlaneClassify.Spanning;
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
            // Transform to local OBB space
            Vector3 d = center - mCenter;
            Quaternion qInv = Quaternion.Inverse(mRotation);
            float dx = qInv.x * d.z - qInv.z * d.y + qInv.w * d.x + qInv.y * d.y;
            float dy = qInv.y * d.z - qInv.z * d.x + qInv.w * d.y + qInv.x * d.x;
            float dz = qInv.z * d.y - qInv.y * d.x + qInv.w * d.z + qInv.x * d.x;
            float qw = qInv.w * d.z - qInv.z * d.z + qInv.x * d.y + qInv.y * d.x;

            float x = 2.0f * (qw * qInv.x + dx * qInv.w + dy * qInv.z - dz * qInv.y);
            float y = 2.0f * (qw * qInv.y - dx * qInv.z + dy * qInv.w + dz * qInv.x);
            float z = 2.0f * (qw * qInv.z + dx * qInv.y - dy * qInv.x + dz * qInv.w);

            float ex = extents.x;
            float ey = extents.y;
            float ez = extents.z;

            float cx = x < -ex ? -ex : (x > ex ? ex : x);
            float cy = y < -ey ? -ey : (y > ey ? ey : y);
            float cz = z < -ez ? -ez : (z > ez ? ez : z);

            float dx2 = x - cx;
            float dy2 = y - cy;
            float dz2 = z - cz;

            return (dx2 * dx2 + dy2 * dy2 + dz2 * dz2) <= radius * radius;
        }

        //-----------------------------------------------------------------------------
        // Name: TestBox() (Public Function)
        // Desc: Determines if 'box' intersects or is fully contained within 'this' box.
        // Parm: box - Query box.
        // Rtrn: True if 'box' intersects or is fully contained within 'this' box and
        //       false otherwise.
        //-----------------------------------------------------------------------------
        public bool TestBox(OBox box)
        {
            // Axes of both boxes
            Vector3 A0 = mRotation * Vector3.right;
            Vector3 A1 = mRotation * Vector3.up;
            Vector3 A2 = mRotation * Vector3.forward;

            Vector3 B0 = box.mRotation * Vector3.right;
            Vector3 B1 = box.mRotation * Vector3.up;
            Vector3 B2 = box.mRotation * Vector3.forward;

            // Rotation matrix
            float R00 = A0.x * B0.x + A0.y * B0.y + A0.z * B0.z;
            float R01 = A0.x * B1.x + A0.y * B1.y + A0.z * B1.z;
            float R02 = A0.x * B2.x + A0.y * B2.y + A0.z * B2.z;
            float R10 = A1.x * B0.x + A1.y * B0.y + A1.z * B0.z;
            float R11 = A1.x * B1.x + A1.y * B1.y + A1.z * B1.z;
            float R12 = A1.x * B2.x + A1.y * B2.y + A1.z * B2.z;
            float R20 = A2.x * B0.x + A2.y * B0.y + A2.z * B0.z;
            float R21 = A2.x * B1.x + A2.y * B1.y + A2.z * B1.z;
            float R22 = A2.x * B2.x + A2.y * B2.y + A2.z * B2.z;

            // AbsR (w/ epsilon)
            const float eps = 1e-4f;
            float AR00 = (R00 < 0.0f ? -R00 : R00) + eps;
            float AR01 = (R01 < 0.0f ? -R01 : R01) + eps;
            float AR02 = (R02 < 0.0f ? -R02 : R02) + eps;
            float AR10 = (R10 < 0.0f ? -R10 : R10) + eps;
            float AR11 = (R11 < 0.0f ? -R11 : R11) + eps;
            float AR12 = (R12 < 0.0f ? -R12 : R12) + eps;
            float AR20 = (R20 < 0.0f ? -R20 : R20) + eps;
            float AR21 = (R21 < 0.0f ? -R21 : R21) + eps;
            float AR22 = (R22 < 0.0f ? -R22 : R22) + eps;

            // Translation
            Vector3 d = box.mCenter - mCenter;
            float t0 = d.x * A0.x + d.y * A0.y + d.z * A0.z;
            float t1 = d.x * A1.x + d.y * A1.y + d.z * A1.z;
            float t2 = d.x * A2.x + d.y * A2.y + d.z * A2.z;

            Vector3 AEx = extents;
            Vector3 BEx = box.extents;

            float r;

            // A’s axes
            r = BEx.x * AR00 + BEx.y * AR01 + BEx.z * AR02;
            if ((t0 < 0.0f ? -t0 : t0) > AEx.x + r) return false;

            r = BEx.x * AR10 + BEx.y * AR11 + BEx.z * AR12;
            if ((t1 < 0.0f ? -t1 : t1) > AEx.y + r) return false;

            r = BEx.x * AR20 + BEx.y * AR21 + BEx.z * AR22;
            if ((t2 < 0.0f ? -t2 : t2) > AEx.z + r) return false;

            // B’s axes
            float a = t0 * R00 + t1 * R10 + t2 * R20;
            r = AEx.x * AR00 + AEx.y * AR10 + AEx.z * AR20;
            if ((a < 0.0f ? -a : a) > r + BEx.x) return false;

            a = t0 * R01 + t1 * R11 + t2 * R21;
            r = AEx.x * AR01 + AEx.y * AR11 + AEx.z * AR21;
            if ((a < 0.0f ? -a : a) > r + BEx.y) return false;

            a = t0 * R02 + t1 * R12 + t2 * R22;
            r = AEx.x * AR02 + AEx.y * AR12 + AEx.z * AR22;
            if ((a < 0.0f ? -a : a) > r + BEx.z) return false;

            // Cross products A x B
            a = t2 * R10 - t1 * R20;
            r = AEx.y * AR20 + AEx.z * AR10;
            if ((a < 0.0f ? -a : a) > r + (BEx.y * AR02 + BEx.z * AR01)) return false;

            a = t2 * R11 - t1 * R21;
            r = AEx.y * AR21 + AEx.z * AR11;
            if ((a < 0.0f ? -a : a) > r + (BEx.x * AR02 + BEx.z * AR00)) return false;

            a = t2 * R12 - t1 * R22;
            r = AEx.y * AR22 + AEx.z * AR12;
            if ((a < 0.0f ? -a : a) > r + (BEx.x * AR01 + BEx.y * AR00)) return false;

            a = t0 * R20 - t2 * R00;
            r = AEx.x * AR20 + AEx.z * AR00;
            if ((a < 0.0f ? -a : a) > r + (BEx.y * AR12 + BEx.z * AR11)) return false;

            a = t0 * R21 - t2 * R01;
            r = AEx.x * AR21 + AEx.z * AR01;
            if ((a < 0.0f ? -a : a) > r + (BEx.x * AR12 + BEx.z * AR10)) return false;

            a = t0 * R22 - t2 * R02;
            r = AEx.x * AR22 + AEx.z * AR02;
            if ((a < 0.0f ? -a : a) > r + (BEx.x * AR11 + BEx.y * AR10)) return false;

            a = t1 * R00 - t0 * R10;
            r = AEx.x * AR10 + AEx.y * AR00;
            if ((a < 0.0f ? -a : a) > r + (BEx.y * AR22 + BEx.z * AR21)) return false;

            a = t1 * R01 - t0 * R11;
            r = AEx.x * AR11 + AEx.y * AR01;
            if ((a < 0.0f ? -a : a) > r + (BEx.x * AR22 + BEx.z * AR20)) return false;

            a = t1 * R02 - t0 * R12;
            r = AEx.x * AR12 + AEx.y * AR02;
            if ((a < 0.0f ? -a : a) > r + (BEx.x * AR21 + BEx.y * AR20)) return false;

            // No separating axis found — we have an intersection
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: TestBox() (Public Function)
        // Desc: Determines if 'bounds' intersects or is fully contained within 'this'
        //       oriented box.
        // Parm: bounds - Query AABB.
        // Rtrn: True if intersection occurs and false otherwise.
        //-----------------------------------------------------------------------------
        public bool TestBox(Bounds bounds)
        {
            // Local axes of this OBB
            Vector3 a0 = mRotation * Vector3.right;
            Vector3 a1 = mRotation * Vector3.up;
            Vector3 a2 = mRotation * Vector3.forward;

            Vector3 aEx = extents;
            Vector3 bEx = bounds.extents;

            // Vector from this center to bounds center
            Vector3 d = bounds.center - mCenter;

            // Project d onto OBB axes
            float t0 = d.x * a0.x + d.y * a0.y + d.z * a0.z;
            float t1 = d.x * a1.x + d.y * a1.y + d.z * a1.z;
            float t2 = d.x * a2.x + d.y * a2.y + d.z * a2.z;

            // Test axes of this box (A0, A1, A2)
            float rb =
                bEx.x * (a0.x >= 0.0f ? a0.x : -a0.x) +
                bEx.y * (a0.y >= 0.0f ? a0.y : -a0.y) +
                bEx.z * (a0.z >= 0.0f ? a0.z : -a0.z);
            if ((t0 >= 0.0f ? t0 : -t0) > aEx.x + rb) return false;

            rb =
                bEx.x * (a1.x >= 0.0f ? a1.x : -a1.x) +
                bEx.y * (a1.y >= 0.0f ? a1.y : -a1.y) +
                bEx.z * (a1.z >= 0.0f ? a1.z : -a1.z);
            if ((t1 >= 0.0f ? t1 : -t1) > aEx.y + rb) return false;

            rb =
                bEx.x * (a2.x >= 0.0f ? a2.x : -a2.x) +
                bEx.y * (a2.y >= 0.0f ? a2.y : -a2.y) +
                bEx.z * (a2.z >= 0.0f ? a2.z : -a2.z);
            if ((t2 >= 0.0f ? t2 : -t2) > aEx.z + rb) return false;

            // Test world X axis
            float ra =
                aEx.x * (a0.x >= 0.0f ? a0.x : -a0.x) +
                aEx.y * (a1.x >= 0.0f ? a1.x : -a1.x) +
                aEx.z * (a2.x >= 0.0f ? a2.x : -a2.x);
            if ((d.x >= 0.0f ? d.x : -d.x) > ra + bEx.x) return false;

            // Test world Y axis
            ra =
                aEx.x * (a0.y >= 0.0f ? a0.y : -a0.y) +
                aEx.y * (a1.y >= 0.0f ? a1.y : -a1.y) +
                aEx.z * (a2.y >= 0.0f ? a2.y : -a2.y);
            if ((d.y >= 0.0f ? d.y : -d.y) > ra + bEx.y) return false;

            // Test world Z axis
            ra =
                aEx.x * (a0.z >= 0.0f ? a0.z : -a0.z) +
                aEx.y * (a1.z >= 0.0f ? a1.z : -a1.z) +
                aEx.z * (a2.z >= 0.0f ? a2.z : -a2.z);
            if ((d.z >= 0.0f ? d.z : -d.z) > ra + bEx.z) return false;

            // No separating axis found — we have an intersection
            return true;
        }
        #endregion
    }
    #endregion
}