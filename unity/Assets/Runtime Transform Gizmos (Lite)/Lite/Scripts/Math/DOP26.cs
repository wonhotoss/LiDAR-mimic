using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: DOP26 (Public Class)
    // Desc: Provides storage for discrete oriented polytopes bounded by 26 planes and
    //       implements relevant functions.
    //-----------------------------------------------------------------------------
    public class DOP26 : IKDOP
    {
        #region Private Static Readonly Fields
        static readonly int[]       sPlaneIntersectCombos;      // Stores all possible combinations of 3 planes that must be tested for intersection when calculating the DOP verts  
        static readonly Vector3[]   sNormals = new Vector3[]    // The first pair of 13 normals used to calculate the min and max offsets
        {
            // AABB corners
            new Vector3( 1.0f,  1.0f,  1.0f).normalized,
            new Vector3( 1.0f,  1.0f, -1.0f).normalized,
            new Vector3( 1.0f, -1.0f,  1.0f).normalized,
            new Vector3(-1.0f,  1.0f,  1.0f).normalized,

            // AABB top edges
            new Vector3(-1.0f,  1.0f,  0.0f).normalized,
            new Vector3( 0.0f,  1.0f,  1.0f).normalized,
            new Vector3( 1.0f,  1.0f,  0.0f).normalized,
            new Vector3( 0.0f,  1.0f, -1.0f).normalized,

            // AABB vertical edges
            new Vector3( 1.0f,  0.0f,  1.0f).normalized,
            new Vector3( 1.0f,  0.0f, -1.0f).normalized,

            // AABB faces
            Vector3.right,
            Vector3.up,
            Vector3.forward
        };
        #endregion

        #region Static Constructors
        //-----------------------------------------------------------------------------
        // Name: DOP26() (Static Constructor)
        // Desc: Static constructor responsible for initializing static data.
        //-----------------------------------------------------------------------------
        static DOP26()
        {
            // Generate plane index combinations (N = 26, K = 3)
            int currentIndex = 0;
            sPlaneIntersectCombos = new int[2600 * 3];
            for (int i = 0; i <= 23; ++i)
            {
                for (int j = i + 1; j <= 24; ++j)
                {
                    for (int k = j + 1; k <= 25; ++k)
                    {
                        // Store current combo
                        sPlaneIntersectCombos[currentIndex++] = i;
                        sPlaneIntersectCombos[currentIndex++] = j;
                        sPlaneIntersectCombos[currentIndex++] = k;
                    }
                }
            }
        }
        #endregion

        #region Private Fields
        float[]         mMinOffsets         = new float[13];        // Min offsets from origin along each plane
        float[]         mMaxOffsets         = new float[13];        // Max offsets from origin along each plane
        Plane[]         mBoundaryPlanes     = new Plane[26];        // The boundary planes pointing outwards
        List<Vector3>   mVerts              = new List<Vector3>();  // DOP vertices
        bool            mIsValid;                                   // Is the DOP valid?
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: isValid (Public Property)
        // Desc: Returns true if the DOP is valid and false otherwise.
        //-----------------------------------------------------------------------------
        public bool isValid     { get { return mIsValid; } }

        //-----------------------------------------------------------------------------
        // Name: vertexCount (Public Property)
        // Desc: Returns the number of vertices.
        //-----------------------------------------------------------------------------
        public int  vertexCount { get { return mVerts.Count; } }

        //-----------------------------------------------------------------------------
        // Name: planeCount (Public Property)
        // Desc: Returns the number of boundary planes.
        //-----------------------------------------------------------------------------
        public int  planeCount  { get { return mBoundaryPlanes.Length; } }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: GetInvalid() (Public Static Function)
        // Desc: Returns an invalid DOP26.
        //-----------------------------------------------------------------------------
        public static DOP26 GetInvalid()
        {
            return new DOP26();
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: FromPoints() (Public Function)
        // Desc: Calculates the DOP from the specified array of points.
        // Parm: points - The array of points used to calculate the DOP.
        //-----------------------------------------------------------------------------
        public void FromPoints(Vector3[] points)
        {
            // Init min and max
            int valCount = mMinOffsets.Length;
            for (int i = 0; i < valCount; ++i)
            {
                mMinOffsets[i] = float.MaxValue;
                mMaxOffsets[i] = float.MinValue;
            }

            // Loop through each point and calculate min and max offsets along each normal
            int pointCount  = points.Length;
            int normalCount = sNormals.Length;
            for (int i = 0; i < pointCount; ++i)
            {
                // Update min and max for each plane
                for (int n = 0; n < normalCount; ++n)
                {
                    // Update min and max offsets along current normal
                    float d = Vector3.Dot(points[i], sNormals[n]);
                    if (d < mMinOffsets[n]) mMinOffsets[n] = d;
                    if (d > mMaxOffsets[n]) mMaxOffsets[n] = d;
                }
            }

            // Update remaining info
            CalculateBoundaryPlanes();
            CalculateVerts();

            // We're now a valid DOP
            mIsValid = true;
        }

        //-----------------------------------------------------------------------------
        // Name: GetVertex() (Public Function)
        // Desc: Returns the vertex with the specified index.
        // Parm: index - The index of the vertex to be returned.
        // Rtrn: The DOP's vertex with the specified index.
        //-----------------------------------------------------------------------------
        public Vector3 GetVertex(int index)
        {
            return mVerts[index];
        }

        //-----------------------------------------------------------------------------
        // Name: CollectVerts() (Public Function)
        // Desc: Collects the DOP's vertices and stores them inside 'verts'.
        // Parm: verts - Returns the DOP's vertices.
        //-----------------------------------------------------------------------------
        public void CollectVerts(List<Vector3> verts)
        {
            // Clear output vert list and copy DOP verts
            verts.Clear();
            verts.AddRange(mVerts);
        }

        //-----------------------------------------------------------------------------
        // Name: CollectVerts() (Public Function)
        // Desc: Collects the DOP's vertices and stores them inside 'verts'.
        // Parm: transformMtx - Transform matrix used to transform each vertex before
        //                      being stored in the output list.
        //       verts        - Returns the DOP's vertices transformed by 'transformMtx'.
        //-----------------------------------------------------------------------------
        public void CollectVerts(Matrix4x4 transformMtx, List<Vector3> verts)
        {
            // Clear output vert list
            verts.Clear();

            // Loop through each vertex, transform it and store it
            int vertexCount = mVerts.Count;
            for (int i = 0; i < vertexCount; ++i)
                verts.Add(transformMtx.MultiplyPoint(mVerts[i]));
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: CalculateBoundaryPlanes() (Private Function)
        // Desc: Calculates the planes that bound the DOP volume.
        //-----------------------------------------------------------------------------
        void CalculateBoundaryPlanes()
        {
            // Keep track of current plane
            int currentPlane = 0;

            // Calculate planes
            int normalCount = sNormals.Length;
            for (int n = 0; n < normalCount; ++n)
            {
                mBoundaryPlanes[currentPlane++] = new Plane(-sNormals[n], -mMinOffsets[n] * -sNormals[n]);
                mBoundaryPlanes[currentPlane++] = new Plane( sNormals[n],  mMaxOffsets[n] *  sNormals[n]);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateVerts() (Private Function)
        // Desc: Calculates the DOP vertices. Must be called after 'CalculateBoundaryPlanes'.
        //-----------------------------------------------------------------------------
        void CalculateVerts()
        {
            // Clear verts
            mVerts.Clear();

            // Loop through each possible plane combo
            int comboCount = sPlaneIntersectCombos.Length / 3;
            for (int i = 0; i < comboCount; ++i)
            {
                // Store plane indices
                int p0 = sPlaneIntersectCombos[i * 3];
                int p1 = sPlaneIntersectCombos[i * 3 + 1];
                int p2 = sPlaneIntersectCombos[i * 3 + 2];

                // Attempt to intersect these 3 planes
                if (PlaneEx.IntersectPlanes(mBoundaryPlanes[p0], mBoundaryPlanes[p1], mBoundaryPlanes[p2], out Vector3 pt))
                {
                    // Reject the point if it lies in front of at least one of the planes
                    bool rejected = false;
                    for (int j = 0; j < 26; ++j)
                    {
                        // Check point distance from plane
                        if (mBoundaryPlanes[j].GetDistanceToPoint(pt) > 1e-5f)
                        {
                            // The point is in front, so reject it
                            rejected = true;
                            break;
                        }
                    }

                    // Store the point if it wasn't rejected
                    if (!rejected) mVerts.Add(pt);
                }
            }

            // It is possible to have duplicate verts, so remove them
            for (int i = 0; i < mVerts.Count; ++i)
            {
                // Check for duplicates
                for (int j = i + 1; j < mVerts.Count; )
                {
                    // Are they the same?
                    if (mVerts[i].FuzzyEquals(mVerts[j]))
                    {
                        // Remove this element
                        mVerts.RemoveAt(j);
                    }
                    else ++j;   // Just move on to the next vector
                }
            }
        }
        #endregion
    }
    #endregion
}
