using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: RTMeshRayHit (Public Struct)
    // Desc: Stores information for an 'RTMesh' ray hit.
    //-----------------------------------------------------------------------------
    public struct RTMeshRayHit
    {
        #region Public Fields
        public RTMesh   rtMesh;         // The 'RTMesh' which was hit
        public Vector3  normal;         // The hit normal
        public Vector3  point;          // The hit point
        public float    t;              // The hit distance from the ray origin
        public int      subMeshIndex;   // Index of the sub-mesh the triangle belongs to
        public int      triIndex;       // Index of hit triangle inside its parent sub-mesh
        #endregion
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTMesh (Public Class)
    // Desc: Wrapper around a Unity 'Mesh' which provides additional functionality
    //       such as being able to perform raycasts without colliders.
    //-----------------------------------------------------------------------------
    public class RTMesh : IBVHQueryCollector<RTMesh.Triangle>
    {
        #region Private Classes
        //-----------------------------------------------------------------------------
        // Name: Triangle (Private Class)
        // Desc: Stores information for a mesh triangle.
        //-----------------------------------------------------------------------------
        class Triangle
        {
            public int      i0, i1, i2;     // Indices of verts that make up the triangle
            public Vector3  v0, v1, v2;     // Vertex coordinates
            public Vector3  normal;         // Triangle normal
            public Plane    plane;          // Triangle plane
            public int      subMeshIndex;   // The sub-mesh this triangle belongs to
            public int      index;          // Triangle index inside its sub-mesh

            public RTMeshRayHit rtMeshHit;  // Used during raycast tests
        }

        //-----------------------------------------------------------------------------
        // Name: SubMesh (Private Class)
        // Desc: Stores information for a sub-mesh.
        //-----------------------------------------------------------------------------
        class SubMesh
        {
            public SubMeshDescriptor                    desc;               // Sub-mesh descriptor
            public List<BinaryAABBTreeNode<Triangle>>   triangleNodes
                = new List<BinaryAABBTreeNode<Triangle>>();                 // List of tree nodes that contain triangles in this sub-mesh
        }
        #endregion

        #region Private Fields
        Mesh                        mUnityMesh;                                                 // The Unity 'Mesh'
        Vector3[]                   mVertexPositions;                                           // Vertex positions
        BinaryAABBTree<Triangle>    mMeshTree       = new BinaryAABBTree<Triangle>(0.0f);       // The mesh tree used for fast queries
        SubMesh[]                   mSubMeshes;                                                 // Array of sub-meshes
        DOP26                       mDOP26          = new DOP26();                              // DOP bounds for better volume approximation
        bool                        mIsTreeReady    = false;                                    // Is the mesh tree ready?
        Bounds                      mBounds;                                                    // Mesh bounds
        bool                        mIgnoreBackfaces;                                           // Used during raycasts

        // Buffers used to avoid memory allocations.
        List<BinaryAABBTreeNode<Triangle>>  mNodeBuffer = new(4096);
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: kdop (Public Property)
        // Desc: Returns the kdop which bounds the mesh.
        //-----------------------------------------------------------------------------
        public IKDOP    kdop        { get { return mDOP26; } }

        //-----------------------------------------------------------------------------
        // Name: vertexCount (Public Property)
        // Desc: Returns the number of vertices.
        //-----------------------------------------------------------------------------
        public int      vertexCount { get { return mVertexPositions != null ? mVertexPositions.Length : 0; } }
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------
        // Name: RTMesh() (Public Constructor)
        // Desc: Creates an 'RTMesh' instance from the specified 'Mesh' instance.
        // Parm: unityMesh - The 'Mesh' instance.
        //-----------------------------------------------------------------------------
        public RTMesh(Mesh unityMesh)
        {
            // Store Unity mesh
            mUnityMesh = unityMesh;

            // Cache mesh data
            RefreshMeshData();
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Internal_OnMeshDataChanged() (Public Function)
        // Desc: Called by the system when the mesh data has changed. This is necessary
        //       when the mesh geometry changes. This means vertex positions, indices and
        //       sub-meshes.
        //-----------------------------------------------------------------------------
        public void Internal_OnMeshDataChanged()
        {
            // Clear acceleration data
            mMeshTree.Clear();
            mIsTreeReady    = false;
            mSubMeshes      = null;

            // Refresh cached mesh data
            RefreshMeshData();
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_OnSubMeshTopologyChanged() (Public Function)
        // Desc: Called by the system when the topology of the specified sub-mesh changes.
        // Parm: subMeshIndex - Index of the sub-mesh that changed its topology.
        //-----------------------------------------------------------------------------
        public void Internal_OnSubMeshTopologyChanged(int subMeshIndex)
        {
            // If the tree is not ready, no need to do anything
            if (!mIsTreeReady) return;

            // Cache data
            SubMeshDescriptor   smDesc      = mUnityMesh.GetSubMesh(subMeshIndex);
            SubMesh             subMesh     = mSubMeshes[subMeshIndex];

            // Has the topology changed?
            if (subMesh.desc.topology != smDesc.topology)
            {
                // If the current topology is set to 'Triangles', it means
                // that the sub-mesh has changed to something else. In this
                // case we have to destroy the leafs.
                if (subMesh.desc.topology == MeshTopology.Triangles)
                {
                    // Loop through each triangle and destroy leaf
                    int numTriangles = subMesh.triangleNodes.Count;
                    for (int i = 0; i < numTriangles; ++i)
                        mMeshTree.DestroyLeaf(subMesh.triangleNodes[i]);

                    // Clear triangle node list
                    subMesh.triangleNodes.Clear();
                }

                // Update topology. If the new topology is 'Triangles' register sub-mesh triangles with the tree.
                subMesh.desc.topology = smDesc.topology;
                if (subMesh.desc.topology == MeshTopology.Triangles)
                    RegisterSubMeshTriangles(subMeshIndex, mVertexPositions);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: GetVertexPosition() (Public Function)
        // Desc: Returns the position of the vertex with the specified index.
        //-----------------------------------------------------------------------------
        public Vector3 GetVertexPosition(int index)
        {
            return mVertexPositions[index];
        }

        //-----------------------------------------------------------------------------
        // Name: WarmUp() (Public Function)
        // Desc: Forces the immediate construction of the internal acceleration
        //       structure for this mesh. If this is not called manually, the structure
        //       will be lazily initialized during the first spatial query (e.g. raycast
        //       or overlap).
        //-----------------------------------------------------------------------------
        public void WarmUp()
        {
            // If the tree is already initialized, skip
            if (mIsTreeReady)
                return;

            // Trigger tree construction now, so that future queries run at full speed
            BuildMeshTree();
        }

        //-----------------------------------------------------------------------------
        // Name: CollectTriangleCorners() (Public Function)
        // Desc: Collects the corner positions of the specified triangle.
        // Parm: subMeshIndex - Index of the sub-mesh the triangle belongs to.
        //       triIndex     - Triangle index.
        //       corners      - Returns the triangle corners.
        //-----------------------------------------------------------------------------
        public void CollectTriangleCorners(int subMeshIndex, int triIndex, List<Vector3> corners)
        {
            // Clear output
            corners.Clear();

            // Store triangle corners
            var tri = mSubMeshes[subMeshIndex].triangleNodes[triIndex].data;
            corners.Add(tri.v0);
            corners.Add(tri.v1);
            corners.Add(tri.v2);
        }

        //-----------------------------------------------------------------------------
        // Name: CollectTriangleCorners() (Public Function)
        // Desc: Collects the corner positions of the specified triangle.
        // Parm: subMeshIndex   - Index of the sub-mesh the triangle belongs to.
        //       triIndex       - Triangle index.
        //       meshTransform  - The mesh transform which describes its position, rotation
        //                        and scale.
        //       corners        - Returns the triangle corners.
        //-----------------------------------------------------------------------------
        public void CollectTriangleCorners(int subMeshIndex, int triIndex, Matrix4x4 meshTransform, List<Vector3> corners)
        {
            // Clear output
            corners.Clear();

            // Store triangle corners
            var tri = mSubMeshes[subMeshIndex].triangleNodes[triIndex].data;
            corners.Add(meshTransform.MultiplyPoint(tri.v0));
            corners.Add(meshTransform.MultiplyPoint(tri.v1));
            corners.Add(meshTransform.MultiplyPoint(tri.v2));
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Performs a raycast against the mesh triangles and returns the closest hit.
        // Parm: ray             - Query ray.
        //       meshTransform   - The mesh transform which describes its position, rotation
        //                         and scale.
        //       ignoreBackFaces - If true, the ray can't hit back-faces (i.e. triangles which
        //                         are facing away from the ray direction are ignored).
        //       hit             - Returns the mesh ray hit.
        // Rtrn: True if there is a hit. False otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray, Transform meshTransform, bool ignoreBackFaces, out RTMeshRayHit hit)
        {
            // Set config
            mIgnoreBackfaces = ignoreBackFaces;

            // Build the tree if not ready
            if (!mIsTreeReady) BuildMeshTree();

            // Raycast in mesh local space
            hit                     = new RTMeshRayHit();
            Matrix4x4 worldInvMtx   = meshTransform.worldToLocalMatrix;
            Ray localRay            = ray.Transform(worldInvMtx);

            // Check bounds first
            if (!mBounds.IntersectRay(localRay))
                return false;

            // Raycast
            if (mMeshTree.Raycast(localRay, this, out Triangle triangle, out BVHRaycastHit bvhHit))
            {
                // Store hit
                hit = triangle.rtMeshHit;

                // Note: We need to multiply the normal with the world inverse transpose. This will
                //       handle negative scale properly.
                Matrix4x4 worldIT = worldInvMtx;
                worldIT.m03 = 0.0f;
                worldIT.m13 = 0.0f; 
                worldIT.m23 = 0.0f; 
                worldIT.m33 = 1.0f;
                worldIT = worldIT.transpose;

                // Convert hit data to world space
                hit.normal  = worldInvMtx.transpose.MultiplyVector(triangle.normal).normalized;
                hit.point   = meshTransform.TransformPoint(localRay.GetPoint(hit.t));
                hit.t       = (ray.origin - hit.point).magnitude;   // Compute in world space

                // We have a hit!
                return true;
            }

            // No hit
            return false;
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: RefreshMeshData() (Private Function)
        // Desc: Refreshes cached vertex, bounds, and DOP data from the Unity mesh.
        //-----------------------------------------------------------------------------
        void RefreshMeshData()
        {
            // Cache vertex positions
            mVertexPositions = mUnityMesh.vertices;

            // Cache mesh bounds
            mBounds = mUnityMesh.bounds;
            if (mBounds.size.magnitude < 1e-5f)
            {
                mUnityMesh.RecalculateBounds();
                mBounds = mUnityMesh.bounds;
            }

            // Refresh DOP data
            mDOP26.FromPoints(mVertexPositions);
        }

        //-----------------------------------------------------------------------------
        // Name: RaycastTriangle() (Private Function)
        // Desc: Performs a raycast against the specified triangle.
        // Parm: ray        - Query ray.
        //       triangle   - Query triangle.
        //       t          - Returns the hit distance from the ray origin if there is
        //                    a hit. Should be ignored if no hit occurs.
        // Rtrn: True if the ray hits the triangle and false otherwise.
        //-----------------------------------------------------------------------------
        bool RaycastTriangle(Ray ray, Triangle triangle, out float t)
        {
            // Intersect ray with triangle plane
            if (!triangle.plane.Raycast(ray, out t))
                return false;

            // Compute intersection point
            Vector3 pt = ray.origin + ray.direction * t;

            // Edge 0: v0 -> v1
            {
                float ex = triangle.v1.x - triangle.v0.x;
                float ey = triangle.v1.y - triangle.v0.y;
                float ez = triangle.v1.z - triangle.v0.z;

                float nx = ey * triangle.normal.z - ez * triangle.normal.y;
                float ny = ez * triangle.normal.x - ex * triangle.normal.z;
                float nz = ex * triangle.normal.y - ey * triangle.normal.x;

                float px = pt.x - triangle.v0.x;
                float py = pt.y - triangle.v0.y;
                float pz = pt.z - triangle.v0.z;

                if ((nx * px + ny * py + nz * pz) > 0.0f)
                    return false;
            }

            // Edge 1: v1 -> v2
            {
                float ex = triangle.v2.x - triangle.v1.x;
                float ey = triangle.v2.y - triangle.v1.y;
                float ez = triangle.v2.z - triangle.v1.z;

                float nx = ey * triangle.normal.z - ez * triangle.normal.y;
                float ny = ez * triangle.normal.x - ex * triangle.normal.z;
                float nz = ex * triangle.normal.y - ey * triangle.normal.x;

                float px = pt.x - triangle.v1.x;
                float py = pt.y - triangle.v1.y;
                float pz = pt.z - triangle.v1.z;

                if ((nx * px + ny * py + nz * pz) > 0.0f)
                    return false;
            }

            // Edge 2: v2 -> v0
            {
                float ex = triangle.v0.x - triangle.v2.x;
                float ey = triangle.v0.y - triangle.v2.y;
                float ez = triangle.v0.z - triangle.v2.z;

                float nx = ey * triangle.normal.z - ez * triangle.normal.y;
                float ny = ez * triangle.normal.x - ex * triangle.normal.z;
                float nz = ex * triangle.normal.y - ey * triangle.normal.x;

                float px = pt.x - triangle.v2.x;
                float py = pt.y - triangle.v2.y;
                float pz = pt.z - triangle.v2.z;

                if ((nx * px + ny * py + nz * pz) > 0.0f)
                    return false;
            }

            // We have a hit!
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: BuildMeshTree() (Private Function)
        // Desc: Builds the mesh tree which is used for fast queries.
        //-----------------------------------------------------------------------------
        void BuildMeshTree()
        {
            // Cache data
            Vector3[]   verts           = mVertexPositions;
            int         subMeshCount    = mUnityMesh.subMeshCount;

            // Create sub-mesh array
            mSubMeshes = new SubMesh[subMeshCount];

            // Loop through each sub-mesh
            for (int i = 0; i < subMeshCount; ++i)
            {
                // Store sub-mesh in sub-mesh array
                SubMeshDescriptor smDesc    = mUnityMesh.GetSubMesh(i);
                mSubMeshes[i]               = new SubMesh();
                mSubMeshes[i].desc          = smDesc;

                // If this is not a 'Triangles' topology sub-mesh, ignore it
                if (smDesc.topology != MeshTopology.Triangles) continue;

                // Invalid index count?
                if (smDesc.indexCount == 0 || smDesc.indexCount % 3 != 0) continue;

                // Register sub-mesh triangles with the tree
                RegisterSubMeshTriangles(i, verts);
            }

            // The tree is ready
            mIsTreeReady = true;
        }

        //-----------------------------------------------------------------------------
        // Name: RegisterSubMeshTriangles() (Private Function)
        // Desc: Registers all triangles in the specified sub-mesh with the mesh tree.
        // Parm: subMeshIndex - Index of sub-mesh whose triangles will be registered with
        //                      the mesh tree.
        //       meshVerts    - Array that contains all vertices in the mesh.
        //-----------------------------------------------------------------------------
        void RegisterSubMeshTriangles(int subMeshIndex, Vector3[] meshVerts)
        {
            // Get indices for this sub-mesh
            int[] indices       = mUnityMesh.GetIndices(subMeshIndex);

            // Loop through each triangle in this sub-mesh
            int numTriangles    = indices.Length / 3;
            for (int triangleIndex = 0; triangleIndex < numTriangles; ++triangleIndex)
            {
                // Create a new triangle
                Triangle triangle       = new Triangle();
                triangle.index          = triangleIndex;
                triangle.subMeshIndex   = subMeshIndex;

                // Store triangle indices
                triangle.i0 = indices[triangleIndex * 3];
                triangle.i1 = indices[triangleIndex * 3 + 1];
                triangle.i2 = indices[triangleIndex * 3 + 2];

                // Store triangle verts
                triangle.v0 = meshVerts[triangle.i0];
                triangle.v1 = meshVerts[triangle.i1];
                triangle.v2 = meshVerts[triangle.i2];

                // Calculate triangle plane and normal
                triangle.plane = new Plane(Vector3.Cross(triangle.v1 - triangle.v0, triangle.v2 - triangle.v0).normalized, triangle.v0);
                triangle.normal = triangle.plane.normal;

                // Calculate triangle AABB (needed for calculating the node's sphere)
                Vector3 aabbMin = triangle.v0;
                aabbMin = Vector3.Min(aabbMin, triangle.v1);
                aabbMin = Vector3.Min(aabbMin, triangle.v2);

                Vector3 aabbMax = triangle.v0;
                aabbMax = Vector3.Max(aabbMax, triangle.v1);
                aabbMax = Vector3.Max(aabbMax, triangle.v2);

                // Create the leaf node and store it inside the sub-mesh node list
                var node = mMeshTree.CreateLeaf(triangle, new Bounds((aabbMax + aabbMin) / 2.0f, aabbMax - aabbMin));
                mSubMeshes[subMeshIndex].triangleNodes.Add(node);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Private Function)
        // Desc: Determines whether the ray hits the node's data during a raycast query.
        // Parm: ray    - Query ray.
        //       data   - Node data.
        //       bvhHit - Returns the BVH hit data.
        // Rtrn: True if the ray hits the node's data; false otherwise.
        //-----------------------------------------------------------------------------
        bool IBVHQueryCollector<Triangle>.Raycast(Ray ray, Triangle data, out BVHRaycastHit bvhHit)
        {
            // Clear output
            bvhHit = new BVHRaycastHit();

            // Raycast triangle
            if (RaycastTriangle(ray, data, out bvhHit.t))
            {
                // Only take the triangle into account if we can hit back-faces or if
                // the dot product between the ray direction and the triangle normal is
                // negative (i.e. it's a front-facing triangle with respect to the ray
                // direction).
                if (!mIgnoreBackfaces || Vector3.Dot(ray.direction, data.normal) < 0.0f)
                {
                    // Update hit data
                    bvhHit.normal = data.normal;

                    data.rtMeshHit.rtMesh           = this;
                    data.rtMeshHit.t                = bvhHit.t;
                    data.rtMeshHit.triIndex         = data.index;
                    data.rtMeshHit.subMeshIndex     = data.subMeshIndex;

                    // We have a hit!
                    return true;
                }
            }

            // No hit
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: BoxOverlap() (Private Function)
        // Desc: Determines whether the box overlaps with the node's data.
        // Parm: box  - Query box.
        //       data - Node data.
        // Rtrn: True if the box overlaps with the node's data; false otherwise.
        //-----------------------------------------------------------------------------
        bool IBVHQueryCollector<Triangle>.BoxOverlap(OBox box, Triangle data)
        {
            throw new System.NotImplementedException();
        }

        //-----------------------------------------------------------------------------
        // Name: SphereOverlap() (Private Function)
        // Desc: Determines whether the sphere overlaps with the node's data.
        // Parm: sphere  - Query sphere.
        //       data    - Node data.
        // Rtrn: True if the sphere overlaps with the node's data; false otherwise.
        //-----------------------------------------------------------------------------
        bool IBVHQueryCollector<Triangle>.SphereOverlap(Sphere sphere, Triangle data)
        {
            throw new System.NotImplementedException();
        }

        //-----------------------------------------------------------------------------
        // Name: PolyhedronOverlap() (Private Function)
        // Desc: Determines whether the polyhedron overlaps with the node's data.
        // Parm: polyhedron  - Query polyhedron.
        //       data        - Node data.
        // Rtrn: True if the polyhedron overlaps with the node's data; false otherwise.
        //-----------------------------------------------------------------------------
        bool IBVHQueryCollector<Triangle>.PolyhedronOverlap(Polyhedron polyhedron, Triangle data)
        {
            throw new System.NotImplementedException();
        }
        #endregion
    }
    #endregion
}
