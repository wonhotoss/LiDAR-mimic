using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTMeshManager (Public Static Class)
    // Desc: Manages 'RTMesh' instances. Clients can map 'Mesh' instances to the
    //       corresponding 'RTMesh' instance.
    //-----------------------------------------------------------------------------
    public static class RTMeshManager
    {
        #region Private Static Fields
        static Dictionary<Mesh, RTMesh>  mMeshMap            = new Dictionary<Mesh, RTMesh>();  // Maps 'Mesh' to 'RTMesh'
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: Internal_Reset() (Public Static Function)
        // Desc: Resets all cached mesh mappings.
        //-----------------------------------------------------------------------------
        public static void Internal_Reset()
        {
            // Reset mesh data
            mMeshMap.Clear();
        }

        //-----------------------------------------------------------------------------
        // Name: RegisterSceneMeshes() (Public Static Function)
        // Desc: Registers all readable meshes in the active scene. If 'warmUp' is true,
        //       the function also forces internal BVH trees to be built to avoid runtime
        //       cost during raycasts.
        // Parm: warmUp - If true, forces each mesh's internal acceleration structure
        //                to be built immediately.
        //-----------------------------------------------------------------------------
        public static void RegisterSceneMeshes(bool warmUp)
        {
            // Find all MeshFilters in the active scene
            MeshFilter[] meshFilters = GameObjectEx.FindObjectsByType<MeshFilter>();

            // Loop through each filter and register its mesh
            int count = meshFilters.Length;
            for (int i = 0; i < count; ++i)
            {
                // Cache data
                MeshFilter meshFilter = meshFilters[i];
                Mesh mesh             = meshFilter.sharedMesh;
                
                // Skip invalid or missing meshes
                if (mesh == null)
                    continue;

                // Create the RTMesh for this mesh
                RTMesh rtMesh = GetRTMesh(mesh);
                if (rtMesh == null)
                    continue;

                // Warm up acceleration data
                if (warmUp)
                    rtMesh.WarmUp();
            }
        }

        //-----------------------------------------------------------------------------
        // Name: OnMeshDestroy() (Public Static Function)
        // Desc: Must be called when the specified mesh is about to be destroyed.
        // Parm: mesh - The mesh which is about to be destroyed.
        //-----------------------------------------------------------------------------
        public static void OnMeshDestroy(Mesh mesh)
        {
            // Nothing to do?
            if (ReferenceEquals(mesh, null))
                return;

            // Remove cached mesh data
            mMeshMap.Remove(mesh);
        }
        
        //-----------------------------------------------------------------------------
        // Name: OnMeshDataChanged() (Public Static Function)
        // Desc: Must be called when mesh vertex positions, indices, or sub-meshes
        //       have changed.
        // Parm: mesh - Mesh whose data has changed.
        //-----------------------------------------------------------------------------
        public static void OnMeshDataChanged(Mesh mesh)
        {
            // Notify RTMesh if registered
            if (mMeshMap.TryGetValue(mesh, out RTMesh rtMesh))
                rtMesh.Internal_OnMeshDataChanged();
        }

        //-----------------------------------------------------------------------------
        // Name: OnSubMeshTopologyChanged() (Public Static Function)
        // Desc: Must be called when the topology of a sub-mesh has changed.
        // Parm: mesh         - Mesh whose sub-mesh topology has changed.
        //       subMeshIndex - Index of sub-mesh whose topology has changed.
        //-----------------------------------------------------------------------------
        public static void OnSubMeshTopologyChanged(Mesh mesh, int subMeshIndex)
        {
            // Notify RTMesh if registered
            if (mMeshMap.TryGetValue(mesh, out RTMesh rtMesh))
                rtMesh.Internal_OnSubMeshTopologyChanged(subMeshIndex);
        }

        //-----------------------------------------------------------------------------
        // Name: GetRTMesh() (Public Static Function)
        // Desc: Returns the 'RTMesh' instance associated with the specified 'Mesh'.
        //       The function automatically registers the mesh if needed.
        // Parm: mesh - Query 'Mesh'.
        // Rtrn: The 'RTMesh' instance associated with the specified 'Mesh' or null if
        //       something goes wrong.
        //-----------------------------------------------------------------------------
        public static RTMesh GetRTMesh(Mesh mesh)
        {
            // Invalid mesh?
            if (!mesh)
                return null;

            // Return existing RTMesh
            if (mMeshMap.TryGetValue(mesh, out RTMesh rtMesh))
                return rtMesh;

            // Register new RTMesh
            return RegisterMesh(mesh);
        }
        #endregion

        #region Private Static Functions
        //-----------------------------------------------------------------------------
        // Name: RegisterMesh() (Private Static Function)
        // Desc: Registers the specified 'Mesh' instance and returns an 'RTMesh' for
        //       this mesh.
        // Parm: mesh - The 'Mesh' instance to register. Must have valid vertex and
        //              index count and must be readable.
        // Rtrn: An 'RTMesh' instance for the specified 'Mesh' or null if something
        //       goes wrong.
        //-----------------------------------------------------------------------------
        static RTMesh RegisterMesh(Mesh mesh)
        {
            // Validate mesh
            if (mesh.vertexCount == 0 || !mesh.isReadable)
                return null;

            // Check for a valid triangle sub-mesh
            bool foundValidSubMesh  = false;
            int subMeshCount        = mesh.subMeshCount;
            for (int i = 0; i < subMeshCount; ++i)
            {
                // Validate topology and index count
                uint indexCount = mesh.GetIndexCount(i);
                if (mesh.GetTopology(i) == MeshTopology.Triangles &&
                    indexCount != 0 && indexCount % 3 == 0)
                {
                    foundValidSubMesh = true;
                    break;
                }
            }

            // No valid sub-mesh found?
            if (!foundValidSubMesh)
                return null;

            // Create and register RTMesh
            RTMesh rtMesh = new RTMesh(mesh);
            mMeshMap.Add(mesh, rtMesh);

            // Return RTMesh
            return rtMesh;
        }
        #endregion
    }
    #endregion
}