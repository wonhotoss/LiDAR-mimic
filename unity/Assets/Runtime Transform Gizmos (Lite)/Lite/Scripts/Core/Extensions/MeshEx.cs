using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EFlatMeshPlane (Public Enum)
    // Desc: Defines different planes used to specify flat mesh orientation
    //       (e.g. quad, circle etc)
    //-----------------------------------------------------------------------------
    public enum EFlatMeshPlane
    {
        XY = 0, // The quad is aligned with the XY plane
        YZ,     // The quad is aligned with the YZ plane
        ZX      // The quad is aligned with the ZX plane
    }
    #endregion

    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: TorusMeshDesc (Public Struct)
    // Desc: Describes a torus mesh.
    //-----------------------------------------------------------------------------
    public struct TorusMeshDesc
    {
        public Vector3  center;             // Torus center
        public float    radius;             // Torus radius (distance between torus center and cross section) (>= 0)
        public float    tubeRadius;         // Tube radius (>= 0)
        public int      sliceCount;         // Number of slices that make up the torus (>= 3)
        public int      crossSliceCount;    // Number of slices that make up a cross section (>= 3)
        public int      mainAxis;           // The index of the main axis (i.e. cross sections rotate around this axis to create the torus): (0 = X, 1 = Y, 2 = Z).
        public Color    color;              // Vertex color
    }

    //-----------------------------------------------------------------------------
    // Name: SphereMeshDesc (Public Struct)
    // Desc: Describes a sphere mesh.
    //-----------------------------------------------------------------------------
    public struct SphereMeshDesc
    {
        public Vector3  center;         // Sphere center
        public float    radius;         // Sphere radius (>= 0)
        public int      sliceCount;     // Number of slices (>= 3)
        public int      stackCount;     // Number of stacks (>= 3)
        public Color    color;          // Vertex color
    }

    //-----------------------------------------------------------------------------
    // Name: CircleMeshDesc (Public Struct)
    // Desc: Describes a circle mesh.
    //-----------------------------------------------------------------------------
    public struct CircleMeshDesc
    {
        public EFlatMeshPlane   circlePlane;    // Circle plane
        public Vector3          center;         // Circle center
        public float            radius;         // Circle radius (>= 0)
        public int              sliceCount;     // Number of slices (>= 3)
        public Color            color;          // Vertex color
    }

    //-----------------------------------------------------------------------------
    // Name: ConeMeshDesc (Public Struct)
    // Desc: Describes a cone mesh.
    //-----------------------------------------------------------------------------
    public struct ConeMeshDesc
    {
        public Vector3  baseCenter;     // Cone base center
        public float    radius;         // Cone radius (>= 0)
        public float    length;         // Cone length (>= 0)
        public int      sliceCount;     // Number of slices going around the length axis (>= 3)
        public int      stackCount;     // Number of stacks (>= 1)
        public int      capRingCount;   // Number of quad rings in the cone cap
        public int      lengthAxis;     // The index of the length axis: (0 = X, 1 = Y, 2 = Z).
        public Color    color;          // Vertex color
    }

    //-----------------------------------------------------------------------------
    // Name: CylinderMeshDesc (Public Struct)
    // Desc: Describes a cylinder mesh.
    //-----------------------------------------------------------------------------
    public struct CylinderMeshDesc
    {
        public Vector3  baseCenter;     // Cylinder base center
        public float    radius;         // Cylinder radius (>= 0)
        public float    length;         // Cylinder length (>= 0)
        public int      sliceCount;     // Number of slices going around the length axis (>= 3)
        public int      stackCount;     // Number of stacks (>= 1)
        public int      capRingCount0;  // Number of quad rings in the left, bottom or front cap, depending on the length axis
        public int      capRingCount1;  // Number of quad rings in the right, top or back cap, depending on the length axis
        public int      lengthAxis;     // The index of the length axis: (0 = X, 1 = Y, 2 = Z).
        public Color    color;          // Vertex color
    }

    //-----------------------------------------------------------------------------
    // Name: SquarePyramidMeshDesc (Public Struct)
    // Desc: Describes a square pyramid mesh.
    //-----------------------------------------------------------------------------
    public struct SquarePyramidMeshDesc
    {
        public Vector3  baseCenter;     // Pyramid base center
        public float    baseSize;       // The size of the pyramid base (>= 0)
        public float    length;         // Pyramid length (i.e. height) (>= 0)
        public int      lengthAxis;     // The index of the length axis: (0 = X, 1 = Y, 2 = Z).
        public Color    color;          // Vertex color
    }

    //-----------------------------------------------------------------------------
    // Name: BoxMeshDesc (Public Struct)
    // Desc: Describes a box mesh.
    //-----------------------------------------------------------------------------
    public struct BoxMeshDesc
    {
        public Vector3  center;     // Box center
        public float    width;      // Box width  (>= 0)
        public float    height;     // Box height (>= 0)
        public float    depth;      // Box depth  (>= 0)
        public Color    color;      // Vertex color
    }

    //-----------------------------------------------------------------------------
    // Name: QuadMeshDesc (Public Struct)
    // Desc: Describes a quad mesh.
    //-----------------------------------------------------------------------------
    public struct QuadMeshDesc
    {
        public EFlatMeshPlane   quadPlane;  // Quad plane
        public Vector3          center;     // Quad center
        public float            width;      // Quad width  (>= 0)
        public float            height;     // Quad height (>= 0)
        public Color            color;      // Vertex color
    }

    //-----------------------------------------------------------------------------
    // Name: RATriangleMeshDesc (Public Struct)
    // Desc: Describes a right-angled triangle mesh.
    //-----------------------------------------------------------------------------
    public struct RATriangleMeshDesc
    {
        public EFlatMeshPlane   trianglePlane;  // Triangle plane
        public float            aeSize0;        // Size of the first adjacent edge  (>= 0)
        public float            aeSize1;        // Size of the second adjacent edge (>= 0)
        public Color            color;          // Vertex color
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: EFlatMeshPlaneEx (Public Static Class)
    // Desc: Implements useful extensions and utility functions for the 'EFlatMeshPlane'
    //       enum.      
    //-----------------------------------------------------------------------------
    public static class EFlatMeshPlaneEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: ToNormal() (Public Extension)
        // Desc: Returns the normal that corresponds to the plane identifier.
        // Rtrn: The normal that corresponds to the plane identifier.
        //-----------------------------------------------------------------------------
        public static Vector3 ToNormal(this EFlatMeshPlane plane)
        {
            switch (plane)
            {
                case EFlatMeshPlane.XY: return Vector3.forward;
                case EFlatMeshPlane.YZ: return Vector3.right;
                case EFlatMeshPlane.ZX: return Vector3.up;
                default: return Vector3.zero;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: ToRightAxis() (Public Extension)
        // Desc: Returns the right axis that corresponds to the plane identifier.
        // Rtrn: The right axis that corresponds to the plane identifier. This function
        //       can be used in conjunction with 'ToNormal' to generate the axes of a
        //       coordinate system.
        //-----------------------------------------------------------------------------
        public static Vector3 ToRightAxis(this EFlatMeshPlane plane)
        {
            switch (plane)
            {
                case EFlatMeshPlane.XY: return Vector3.right;
                case EFlatMeshPlane.YZ: return Vector3.forward;
                case EFlatMeshPlane.ZX: return Vector3.right;
                default: return Vector3.zero;
            }
        }
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: MeshEx (Public Static Class)
    // Desc: Implements useful extensions and utility functions for the 'Mesh' class.      
    //-----------------------------------------------------------------------------
    public static class MeshEx
    {
        #region Public Static Functions
        #region Torus
        //-----------------------------------------------------------------------------
        // Name: CreateTorus() (Public Static Function)
        // Desc: Creates a torus mesh.
        // Parm: desc - Torus descriptor.
        // Rtrn: The torus mesh.
        //-----------------------------------------------------------------------------
        public static Mesh CreateTorus(TorusMeshDesc desc)
        {
            // Update args if necessary
            if (desc.radius     < 0.0f  )   desc.radius     = 0.0f;
            if (desc.tubeRadius < 0.0f  )   desc.tubeRadius = 0.0f;
            if (desc.sliceCount < 3     )   desc.sliceCount = 3;
            if (desc.crossSliceCount < 3)   desc.crossSliceCount = 3;

            // Precompute data
            int vertRingCount   = desc.sliceCount + 1;          // Number of vertex rings (i.e. cross sections)
            int vertRingRes     = (desc.crossSliceCount + 1);
            int numVerts        = vertRingRes * vertRingCount;
            Vector3[] positions = new Vector3[numVerts];
            Vector3[] normals   = new Vector3[numVerts];

            // Loop through each vertex ring (i.e. cross section)
            int vPtr = 0;
            for (int crossIndex = 0; crossIndex < vertRingCount; ++crossIndex)
            {
                // Precompute data
                float crossAngle    = 360.0f / (desc.sliceCount - 1) * crossIndex * Mathf.Deg2Rad;
                float cosCross      = Mathf.Cos(crossAngle);
                float sinCross      = Mathf.Sin(crossAngle);
                Vector3 crossCenter = new Vector3(cosCross * desc.radius, 0.0f, sinCross * desc.radius);

                // Loop through each vertex in this cross section
                for (int vIndex = 0; vIndex < vertRingRes; ++vIndex)
                {
                    // Precompute data
                    float vAngle    = 360.0f / (desc.crossSliceCount - 1) * vIndex * Mathf.Deg2Rad;
                    float cosVert   = Mathf.Cos(vAngle);
                    float sinVert   = Mathf.Sin(vAngle);

                    // Generate position and normal
                    positions[vPtr] = crossCenter;
                    positions[vPtr].x += cosCross * sinVert * desc.tubeRadius;
                    positions[vPtr].y += cosVert * desc.tubeRadius;
                    positions[vPtr].z += sinCross * sinVert * desc.tubeRadius;
                    normals[vPtr] = (positions[vPtr] - crossCenter).normalized;

                    // Next vertex
                    ++vPtr;
                }
            }

            // Transform vertices
            Quaternion rotation = Quaternion.identity;
            if (desc.mainAxis == 0) rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
            else if (desc.mainAxis == 2) rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
            int vertexCount = positions.Length;
            for (int i = 0; i < vertexCount; ++i)
            {
                // Rotate only if necessary
                if (desc.mainAxis != 1)
                {
                    normals[i]      = (rotation * normals[i]).normalized;
                    positions[i]    = rotation * positions[i];
                }

                // Translate
                positions[i] += desc.center;
            }

            // Generate indices. Loop through each slice.
            int indexPtr    = 0;
            int indexCount  = desc.sliceCount * desc.crossSliceCount * 6;
            int[] indices   = new int[indexCount];
            for (int s = 0; s < desc.sliceCount; ++s)
            {
                // Loop through each slice in this cross section
                for (int cs = 0; cs < desc.crossSliceCount; ++cs)
                {
                    int baseIndex = s * vertRingRes + cs;

                    indices[indexPtr++] = baseIndex;
                    indices[indexPtr++] = baseIndex + vertRingRes;
                    indices[indexPtr++] = baseIndex + 1;

                    indices[indexPtr++] = baseIndex + 1;
                    indices[indexPtr++] = baseIndex + vertRingRes;
                    indices[indexPtr++] = baseIndex + 1 + vertRingRes;
                }
            }

            // Create and initialize mesh
            Mesh mesh       = new Mesh();
            mesh.vertices   = positions;
            mesh.normals    = normals;
            mesh.colors     = ColorEx.CreateColorArray(desc.color, positions.Length);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.UploadMeshData(false);

            // Return mesh
            return mesh;
        }
        #endregion

        #region Sphere
        //-----------------------------------------------------------------------------
        // Name: CreateSphere() (Public Static Function)
        // Desc: Creates a sphere mesh.
        // Parm: desc - Sphere descriptor.
        // Rtrn: The sphere mesh.
        //-----------------------------------------------------------------------------
        public static Mesh CreateSphere(SphereMeshDesc desc)
        {
            // Update args if necessary
            if (desc.radius < 0.0f) desc.radius         = 0.0f;
            if (desc.sliceCount < 3) desc.sliceCount    = 3;
            if (desc.stackCount < 1) desc.stackCount    = 1;

            // Cache data for easy access
            int vertRingCount = desc.stackCount + 1;     // Number of vertex rings around the central axis
            int vertRingRes   = desc.sliceCount + 1;     // Number of vertices in one vertex ring

            // Loop through each vertex ring
            List<Vector3> positions     = new List<Vector3>();
            List<Vector3> normals       = new List<Vector3>();
            float angleStep = 360.0f / desc.sliceCount;
            for (int ringIndex = 0; ringIndex < vertRingCount; ++ringIndex)
            {
                // Precompute values
                float theta = Mathf.PI * (float)ringIndex / (vertRingCount - 1);
                float cosTheta = Mathf.Cos(theta);
                float sinTheta = Mathf.Sin(theta);

                // Loop through each vertex in this row
                for (int v = 0; v < vertRingRes; ++v)
                {
                    // Calculate normal
                    float a = angleStep * v * Mathf.Deg2Rad;
                    Vector3 normal = new Vector3(Mathf.Cos(a) * sinTheta, 
                                                 cosTheta, 
                                                 Mathf.Sin(a) * sinTheta).normalized;
          
                    // Store vertex and normal
                    positions.Add(normal * desc.radius + desc.center);
                    normals.Add(normal);
                }
            }

            // Generate indices. Loop through each vertex ring.
            int indexCount = desc.sliceCount * desc.stackCount * 6;
            List<int> indices = new List<int>(indexCount);
            for(int ringIndex = 0; ringIndex < vertRingCount - 1; ++ringIndex)
            {
                // Loop through each vertex in the ring
                for (int v = 0; v < vertRingRes - 1; ++v)
                {
                    // Calculate base index
                    int baseIndex = ringIndex * vertRingRes + v;

                    // First triangle
                    indices.Add(baseIndex);
                    indices.Add(baseIndex + 1);
                    indices.Add(baseIndex + vertRingRes);

                    // Second triangle
                    indices.Add(baseIndex + 1);
                    indices.Add(baseIndex + vertRingRes + 1);
                    indices.Add(baseIndex + vertRingRes);
                }
            }

            // Create and initialize mesh
            Mesh mesh       = new Mesh();
            mesh.vertices   = positions.ToArray();
            mesh.normals    = normals.ToArray();
            mesh.colors     = ColorEx.CreateColorArray(desc.color, positions.Count);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.UploadMeshData(false);

            // Return mesh
            return mesh;
        }
        #endregion

        #region Circle
        //-----------------------------------------------------------------------------
        // Name: CreateCircle() (Public Static Function)
        // Desc: Creates a circle mesh.
        // Parm: desc - Circle descriptor.
        // Rtrn: The circle mesh.
        //-----------------------------------------------------------------------------
        public static Mesh CreateCircle(CircleMeshDesc desc)
        {
            // Update args if necessary
            if (desc.sliceCount < 3)    desc.sliceCount = 3;
            if (desc.radius     < 0.0f) desc.radius     = 0.0f;

            // Generate circle points
            List<Vector3> positions = new List<Vector3>(desc.sliceCount + 1);
            var circle = new Circle();
            circle.Set(desc.center, desc.radius, desc.circlePlane.ToRightAxis(), desc.circlePlane.ToNormal());
            Geometry.GenerateCirclePoints(circle, desc.sliceCount, positions);
            positions.Add(desc.center);  // Add circle center
           
            // Generate normals
            int vertexCount = positions.Count;
            Vector3[] normals = new Vector3[vertexCount];
            for (int i = 0; i < vertexCount; ++i)
                normals[i] = circle.normal;

            // Generate indices
            int indexPtr = 0;
            int[] indices = new int[desc.sliceCount * 3];
            for (int t = 0; t < desc.sliceCount; ++t)
            {
                indices[indexPtr++] = positions.Count - 1;
                indices[indexPtr++] = (t + 1) % (vertexCount - 1);
                indices[indexPtr++] = t;
            }

            // Create and initialize mesh
            Mesh mesh       = new Mesh();
            mesh.vertices   = positions.ToArray();
            mesh.normals    = normals;
            mesh.colors     = ColorEx.CreateColorArray(desc.color, positions.Count);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.UploadMeshData(false);

            // Return mesh
            return mesh;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateWireCircle() (Public Static Function)
        // Desc: Creates a wire circle mesh.
        // Parm: desc - Circle descriptor.
        // Rtrn: The wire circle mesh.
        //-----------------------------------------------------------------------------
        public static Mesh CreateWireCircle(CircleMeshDesc desc)
        {
            // Update args if necessary
            if (desc.sliceCount < 3)        desc.sliceCount = 3;
            if (desc.radius     < 0.0f)     desc.radius     = 0.0f;

            // Generate circle points
            List<Vector3>   positions   = new List<Vector3>(desc.sliceCount + 1);
            List<int>       indices     = new List<int>();
            var             circle      = new Circle();
            circle.Set(desc.center, desc.radius, desc.circlePlane.ToRightAxis(), desc.circlePlane.ToNormal());
            Geometry.GenerateCirclePoints(circle, desc.sliceCount, positions, indices);

            // Create and initialize mesh
            Mesh mesh       = new Mesh();
            mesh.vertices   = positions.ToArray();
            mesh.colors     = ColorEx.CreateColorArray(desc.color, positions.Count);
            mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
            mesh.UploadMeshData(false);

            // Return mesh
            return mesh;
        }
        #endregion

        #region Cone
        //-----------------------------------------------------------------------------
        // Name: CreateCone() (Public Static Function)
        // Desc: Creates a cone mesh.
        // Parm: desc - Cone descriptor.
        // Rtrn: The cone mesh.
        //-----------------------------------------------------------------------------
        public static Mesh CreateCone(ConeMeshDesc desc)
        {
            // Update args if necessary
            if (desc.sliceCount < 3)    desc.sliceCount = 3;
            if (desc.stackCount < 1)    desc.stackCount = 1;
            if (desc.radius     < 0.0f) desc.radius = 0.0f;
            if (desc.length     < 0.0f) desc.length = 0.0f;
            if (desc.lengthAxis < 0 ||
                desc.lengthAxis > 2)    desc.lengthAxis = 1;

            // Cache data for easy access
            int vertRingCount = desc.stackCount + 1;     // Number of vertex rings around the length axis
            int vertRingRes   = desc.sliceCount + 1;     // Number of vertices in one vertex ring

            // Generate the vertices around the length axis. We will use the Y axis as the length
            // axis and then transform the vertices as post process based on the selected length 
            // axis to keep things simple.
            Vector3 coneAxis    = Vector3.up;
            Vector3 bottomPos   = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 topPos      = new Vector3(0.0f, desc.length, 0.0f);
            float stackHeight   = desc.length / desc.stackCount;
            float angleStep     = 360.0f / desc.sliceCount;

            // Loop through each vertex ring
            List<Vector3> positions     = new List<Vector3>();
            List<Vector3> normals       = new List<Vector3>();
            for(int ringIndex = 0; ringIndex < vertRingCount; ++ringIndex)
            {
                // Precompute values
                float y         = bottomPos.y + ringIndex * stackHeight;
                float radius    = Mathf.Lerp(desc.radius, 0.0f, ringIndex / (float)(vertRingCount - 1));

                // Loop through each vertex in this ring
                for(int v = 0; v < vertRingRes; ++v)
                {
                    // Calculate current angle and generate the vertex normal
                    float angle = v * angleStep;
                    Vector3 normal = (new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0.0f, Mathf.Sin(angle * Mathf.Deg2Rad))).normalized;
                  
                    // Store position and normal
                    positions.Add(bottomPos + ringIndex * stackHeight * coneAxis + normal * radius);
                    normal.x *= desc.length;
                    normal.y  = desc.radius;
                    normal.z *= desc.length;
                    normals.Add(normal);
                }
            }
     
            // Generate indices. Loop through each vertex ring.
            int indexCount = desc.sliceCount * desc.stackCount * 6;
            List<int> indices = new List<int>(indexCount);
            for(int ringIndex = 0; ringIndex < vertRingCount - 1; ++ringIndex)
            {
                // Loop through each vertex in the ring
                for (int v = 0; v < vertRingRes - 1; ++v)
                {
                    // Calculate base index
                    int baseIndex = ringIndex * vertRingRes + v;

                    // First triangle
                    indices.Add(baseIndex);
                    indices.Add(baseIndex + vertRingRes);
                    indices.Add(baseIndex + 1);

                    // Second triangle
                    indices.Add(baseIndex + vertRingRes);
                    indices.Add(baseIndex + vertRingRes + 1);
                    indices.Add(baseIndex + 1);
                }
            }

            // Generate cap?
            if (desc.capRingCount >= 1)
            {
                // Store current vertex count. Necessary when generating indices for this ring.
                int vCheckpoint = positions.Count;

                // Loop through each vertex ring in this cap
                vertRingCount = desc.capRingCount + 1;
                for (int ringIndex = 0; ringIndex < vertRingCount; ++ringIndex)
                {
                    // Calculate ring radius and loop through each vertex in this ring
                    float radius = Mathf.Lerp(desc.radius, 0.0f, ringIndex / (float)(vertRingCount - 1));
                    for(int v = 0; v < vertRingRes; ++v)
                    {
                        // Calculate vertex direction
                        float angle = v * angleStep;
                        Vector3 vDir = (new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0.0f, -Mathf.Sin(angle * Mathf.Deg2Rad))).normalized;

                        // Store position and normal
                        positions.Add(bottomPos + vDir * radius);
                        normals.Add(-coneAxis);
                    }
                }

                // Generate indices. Loop through each vertex ring in this cap.
                for (int ringIndex = 0; ringIndex < vertRingCount - 1; ++ringIndex)
                {
                    // Calculate ring radius and loop through each vertex in this ring
                    for (int v = 0; v < vertRingRes - 1; ++v)
                    {
                        // Calculate base index
                        int baseIndex = vCheckpoint + ringIndex * vertRingRes + v;

                        // First triangle
                        indices.Add(baseIndex);
                        indices.Add(baseIndex + vertRingRes);
                        indices.Add(baseIndex + 1);

                        // Second triangle
                        indices.Add(baseIndex + vertRingRes);
                        indices.Add(baseIndex + vertRingRes + 1);
                        indices.Add(baseIndex + 1);
                    }
                }
            }

            // Transform vertices
            Quaternion rotation = Quaternion.identity;
            if (desc.lengthAxis == 0) rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
            else if (desc.lengthAxis == 2) rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
            int vertexCount = positions.Count;
            for (int i = 0; i < vertexCount; ++i)
            {
                // Rotate only if necessary
                if (desc.lengthAxis != 1)
                {
                    normals[i]      = (rotation * normals[i]).normalized;
                    positions[i]    = rotation * positions[i];
                }

                // Translate
                positions[i] += desc.baseCenter;
            }

            // Create and initialize mesh
            Mesh mesh       = new Mesh();
            mesh.vertices   = positions.ToArray();
            mesh.normals    = normals.ToArray();
            mesh.colors     = ColorEx.CreateColorArray(desc.color, positions.Count);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.UploadMeshData(false);

            // Return mesh
            return mesh;
        }
        #endregion

        #region Cylinder
        //-----------------------------------------------------------------------------
        // Name: CreateCylinder() (Public Static Function)
        // Desc: Creates a cylinder mesh.
        // Parm: desc - Cylinder descriptor.
        // Rtrn: The cylinder mesh.
        //-----------------------------------------------------------------------------
        public static Mesh CreateCylinder(CylinderMeshDesc desc)
        {
            // Update args if necessary
            if (desc.sliceCount < 3)    desc.sliceCount = 3;
            if (desc.stackCount < 1)    desc.stackCount = 1;
            if (desc.radius     < 0.0f) desc.radius     = 0.0f;
            if (desc.length     < 0.0f) desc.length     = 0.0f;
            if (desc.lengthAxis < 0 ||
                desc.lengthAxis > 2)    desc.lengthAxis = 1;

            // Cache data for easy access
            int vertRingCount = desc.stackCount + 1;     // Number of vertex rings around the length axis
            int vertRingRes   = desc.sliceCount + 1;     // Number of vertices in one vertex ring

            // Generate the vertices around the length axis. We will use the Y axis as the length
            // axis and then transform the vertices as post process based on the selected length 
            // axis to keep things simple.
            Vector3 cylinderAxis    = Vector3.up;
            Vector3 bottomPos       = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 topPos          = new Vector3(0.0f, desc.length, 0.0f);
            float stackHeight       = desc.length / desc.stackCount;
            float angleStep         = 360.0f / desc.sliceCount;

            // Loop through each vertex ring
            List<Vector3> positions     = new List<Vector3>();
            List<Vector3> normals       = new List<Vector3>();
            for(int ringIndex = 0; ringIndex < vertRingCount; ++ringIndex)
            {
                // Precompute values
                float y = bottomPos.y + ringIndex * stackHeight;

                // Loop through each vertex in this ring
                for(int v = 0; v < vertRingRes; ++v)
                {
                    // Calculate current angle and generate the vertex normal
                    float angle = v * angleStep;
                    Vector3 normal = (new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0.0f, Mathf.Sin(angle * Mathf.Deg2Rad))).normalized;
            
                    // Store position and normal
                    positions.Add(bottomPos + ringIndex * stackHeight * cylinderAxis + normal * desc.radius);
                    normals.Add(normal);
                }
            }
     
            // Generate indices. Loop through each vertex ring.
            int indexCount = desc.sliceCount * desc.stackCount * 6;
            List<int> indices = new List<int>(indexCount);
            for(int ringIndex = 0; ringIndex < vertRingCount - 1; ++ringIndex)
            {
                // Loop through each vertex in the ring
                for (int v = 0; v < vertRingRes - 1; ++v)
                {
                    // Calculate base index
                    int baseIndex = ringIndex * vertRingRes + v;

                    // First triangle
                    indices.Add(baseIndex);
                    indices.Add(baseIndex + vertRingRes);
                    indices.Add(baseIndex + 1);

                    // Second triangle
                    indices.Add(baseIndex + vertRingRes);
                    indices.Add(baseIndex + vertRingRes + 1);
                    indices.Add(baseIndex + 1);
                }
            }

            // Check if caps must be generated
            const int minRingCount = 1;
            bool hasFirstCap    = desc.capRingCount0 >= minRingCount;
            bool hasSecondCap   = desc.capRingCount1 >= minRingCount;

            // Generate first cap?
            if (hasFirstCap)
            {
                // Store current vertex count. Necessary when generating indices for this ring.
                int vCheckpoint = positions.Count;

                // Loop through each vertex ring in this cap
                vertRingCount = desc.capRingCount0 + 1;
                for (int ringIndex = 0; ringIndex < vertRingCount; ++ringIndex)
                {
                    // Calculate ring radius and loop through each vertex in this ring
                    float radius = Mathf.Lerp(desc.radius, 0.0f, ringIndex / (float)(vertRingCount - 1));
                    for(int v = 0; v < vertRingRes; ++v)
                    {
                        // Calculate vertex direction
                        float angle = v * angleStep;
                        Vector3 vDir = (new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0.0f, -Mathf.Sin(angle * Mathf.Deg2Rad))).normalized;

                        // Store position and normal
                        positions.Add(bottomPos + vDir * radius);
                        normals.Add(-cylinderAxis);
                    }
                }

                // Generate indices. Loop through each vertex ring in this cap.
                for (int ringIndex = 0; ringIndex < vertRingCount - 1; ++ringIndex)
                {
                    // Calculate ring radius and loop through each vertex in this ring
                    for (int v = 0; v < vertRingRes - 1; ++v)
                    {
                        // Calculate base index
                        int baseIndex = vCheckpoint + ringIndex * vertRingRes + v;

                        // First triangle
                        indices.Add(baseIndex);
                        indices.Add(baseIndex + vertRingRes);
                        indices.Add(baseIndex + 1);

                        // Second triangle
                        indices.Add(baseIndex + vertRingRes);
                        indices.Add(baseIndex + vertRingRes + 1);
                        indices.Add(baseIndex + 1);
                    }
                }
            }

            // Generate second cap?
            if (hasSecondCap)
            {
                // Store current vertex count. Necessary when generating indices for this ring.
                int vCheckpoint = positions.Count;

                // Loop through each vertex ring in this cap
                vertRingCount = desc.capRingCount1 + 1;
                for (int ringIndex = 0; ringIndex < vertRingCount; ++ringIndex)
                {
                    // Calculate ring radius and loop through each vertex in this ring
                    float radius = Mathf.Lerp(desc.radius, 0.0f, ringIndex / (float)(vertRingCount - 1));
                    for(int v = 0; v < vertRingRes; ++v)
                    {
                        // Calculate vertex direction
                        float angle = v * angleStep;
                        Vector3 vDir = (new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0.0f, Mathf.Sin(angle * Mathf.Deg2Rad))).normalized;

                        // Store position and normal
                        positions.Add(topPos + vDir * radius);
                        normals.Add(cylinderAxis);
                    }
                }

                // Generate indices. Loop through each vertex ring in this cap.
                for (int ringIndex = 0; ringIndex < vertRingCount - 1; ++ringIndex)
                {
                    // Calculate ring radius and loop through each vertex in this ring
                    for (int v = 0; v < vertRingRes - 1; ++v)
                    {
                        // Calculate base index
                        int baseIndex = vCheckpoint + ringIndex * vertRingRes + v;

                        // First triangle
                        indices.Add(baseIndex);
                        indices.Add(baseIndex + vertRingRes);
                        indices.Add(baseIndex + 1);

                        // Second triangle
                        indices.Add(baseIndex + vertRingRes);
                        indices.Add(baseIndex + vertRingRes + 1);
                        indices.Add(baseIndex + 1);
                    }
                }
            }

            // Transform vertices
            Quaternion rotation = Quaternion.identity;
            if (desc.lengthAxis == 0) rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
            else if (desc.lengthAxis == 2) rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
            int vertexCount = positions.Count;
            for (int i = 0; i < vertexCount; ++i)
            {
                // Rotate only if necessary
                if (desc.lengthAxis != 1)
                {
                    normals[i] = (rotation * normals[i]).normalized;
                    positions[i]   = rotation * positions[i];
                }

                // Translate
                positions[i] += desc.baseCenter;
            }

            // Create and initialize mesh
            Mesh mesh       = new Mesh();
            mesh.vertices   = positions.ToArray();
            mesh.normals    = normals.ToArray();
            mesh.colors     = ColorEx.CreateColorArray(desc.color, positions.Count);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.UploadMeshData(false);

            // Return mesh
            return mesh;
        }
        #endregion

        #region Pyramid
        //-----------------------------------------------------------------------------
        // Name: CreateSquarePyramid() (Public Static Function)
        // Desc: Creates a square pyramid mesh.
        // Parm: desc - Pyramid descriptor.
        // Rtrn: The square pyramid mesh.
        //-----------------------------------------------------------------------------
        public static Mesh CreateSquarePyramid(SquarePyramidMeshDesc desc)
        {
            // Update args if necessary
            if (desc.baseSize   < 0.0f)    desc.baseSize   = 0.0f;
            if (desc.length     < 0.0f)    desc.length     = 0.0f;
            if (desc.lengthAxis < 0 || desc.lengthAxis > 2) desc.lengthAxis = 1;

            // Cache data
            float halfBSize = desc.baseSize * 0.5f;

            // Generate positions. We will generate the positions assuming a vertical height axis.
            // We will rotate the positions afterwards.
            Vector3 basePos     = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 tipPosition = basePos + Vector3.up * desc.length;
            Vector3[] positions = new Vector3[]
            {
                // Front face
                tipPosition,
                basePos + Vector3.right * halfBSize - Vector3.forward * halfBSize,
                basePos - Vector3.right * halfBSize - Vector3.forward * halfBSize,

                // Right face
                tipPosition,
                basePos + Vector3.right * halfBSize + Vector3.forward * halfBSize,
                basePos + Vector3.right * halfBSize - Vector3.forward * halfBSize,

                // Back face
                tipPosition,
                basePos - Vector3.right * halfBSize + Vector3.forward * halfBSize,
                basePos + Vector3.right * halfBSize + Vector3.forward * halfBSize,

                // Left face
                tipPosition,
                basePos - Vector3.right * halfBSize - Vector3.forward * halfBSize,
                basePos - Vector3.right * halfBSize + Vector3.forward * halfBSize,

                // Bottom face
                basePos - Vector3.right * halfBSize - Vector3.forward * halfBSize,
                basePos + Vector3.right * halfBSize - Vector3.forward * halfBSize,
                basePos + Vector3.right * halfBSize + Vector3.forward * halfBSize,
                basePos - Vector3.right * halfBSize + Vector3.forward * halfBSize
            };

            // Generate indices
            int[] indices = new int[]
            {
                0, 1, 2,
                3, 4, 5,
                6, 7, 8,
                9, 10, 11,
                12, 13, 14,
                12, 14, 15
            };

            // Generate normals
            Vector3[] normals = new Vector3[positions.Length];
            for (int triangleIndex = 0; triangleIndex < 4; ++triangleIndex)
            {
                // Store vertex indices for this triangle
                int index0 = indices[triangleIndex * 3];
                int index1 = indices[triangleIndex * 3 + 1];
                int index2 = indices[triangleIndex * 3 + 2];

                // Calculate triangle edges and normals
                Vector3 edge0 = positions[index1] - positions[index0];
                Vector3 edge1 = positions[index2] - positions[index0];
                Vector3 normal = Vector3.Cross(edge0, edge1).normalized;

                // Store normals
                normals[index0] = normal;
                normals[index1] = normal;
                normals[index2] = normal;
            }

            // Override bottom face normals
            normals[normals.Length - 4] = -Vector3.up;
            normals[normals.Length - 3] = -Vector3.up;
            normals[normals.Length - 2] = -Vector3.up;
            normals[normals.Length - 1] = -Vector3.up;

            // Transform vertices
            Quaternion rotation = Quaternion.identity;
            if (desc.lengthAxis == 0) rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
            else if (desc.lengthAxis == 2) rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
            int vertexCount = positions.Length;
            for (int i = 0; i < vertexCount; ++i)
            {
                // Rotate only if necessary
                if (desc.lengthAxis != 1)
                {
                    normals[i] = (rotation * normals[i]).normalized;
                    positions[i]   = rotation * positions[i];
                }

                // Translate
                positions[i] += desc.baseCenter;
            }

            // Create and initialize mesh
            Mesh mesh       = new Mesh();
            mesh.vertices   = positions;
            mesh.normals    = normals;
            mesh.colors     = ColorEx.CreateColorArray(desc.color, positions.Length);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.UploadMeshData(false);

            // Return mesh
            return mesh;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateWireSquarePyramid() (Public Static Function)
        // Desc: Creates a wire square pyramid mesh.
        // Parm: desc - Pyramid descriptor.
        // Rtrn: The wire square pyramid mesh.
        //-----------------------------------------------------------------------------
        public static Mesh CreateWireSquarePyramid(SquarePyramidMeshDesc desc)
        {
            // Update args if necessary
            if (desc.baseSize   < 0.0f)    desc.baseSize   = 0.0f;
            if (desc.length     < 0.0f)    desc.length     = 0.0f;
            if (desc.lengthAxis < 0 || desc.lengthAxis > 2) desc.lengthAxis = 1;

            // Cache data
            float halfBSize = desc.baseSize * 0.5f;

            // Generate positions. We will generate the positions assuming a vertical height axis.
            // We will rotate the positions afterwards.
            Vector3 basePos     = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 tipPosition = basePos + Vector3.up * desc.length;
            Vector3[] positions = new Vector3[]
            {
                // Bottom face
                basePos - Vector3.right * halfBSize - Vector3.forward * halfBSize,
                basePos + Vector3.right * halfBSize - Vector3.forward * halfBSize,
                basePos + Vector3.right * halfBSize + Vector3.forward * halfBSize,
                basePos - Vector3.right * halfBSize + Vector3.forward * halfBSize,

                // Tip
                tipPosition,
            };

            // Generate indices
            int[] indices = new int[]
            {
                0, 1, 1, 2, 2, 3, 3, 0,  // Base

                // Connect base to tip
                0, 4, 1, 4, 2, 4, 3, 4
            };

            // Transform vertices
            Quaternion rotation = Quaternion.identity;
            if (desc.lengthAxis == 0) rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
            else if (desc.lengthAxis == 2) rotation = Quaternion.Euler(90.0f, 0.0f, 0.0f);
            int vertexCount = positions.Length;
            for (int i = 0; i < vertexCount; ++i)
            {
                // Rotate only if necessary
                if (desc.lengthAxis != 1)
                    positions[i] = rotation * positions[i];

                // Translate
                positions[i] += desc.baseCenter;
            }

            // Create and initialize mesh
            Mesh mesh       = new Mesh();
            mesh.vertices   = positions;
            mesh.colors     = ColorEx.CreateColorArray(desc.color, positions.Length);
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
            mesh.UploadMeshData(false);

            // Return mesh
            return mesh;
        }
        #endregion

        #region Box
        //-----------------------------------------------------------------------------
        // Name: CreateBox() (Public Static Function)
        // Desc: Creates a box mesh.
        // Parm: desc - Box descriptor.
        // Rtrn: The box mesh.
        //-----------------------------------------------------------------------------
        public static Mesh CreateBox(BoxMeshDesc desc)
        {
            // Update args if necessary
            if (desc.width   < 0.0f) desc.width  = 0.0f;
            if (desc.height  < 0.0f) desc.height = 0.0f;
            if (desc.depth   < 0.0f) desc.depth  = 0.0f;

            // Cache data
            float halfWidth  = desc.width * 0.5f;
            float halfHeight = desc.height * 0.5f;
            float halfDepth  = desc.depth * 0.5f;

            // Generate positions
            Vector3[] positions = new Vector3[]
            {
                // Front face
                new Vector3(-halfWidth, -halfHeight, -halfDepth),
                new Vector3(-halfWidth,  halfHeight, -halfDepth),
                new Vector3( halfWidth,  halfHeight, -halfDepth),
                new Vector3( halfWidth, -halfHeight, -halfDepth),

                // Back face
                new Vector3( halfWidth, -halfHeight, halfDepth),
                new Vector3( halfWidth,  halfHeight, halfDepth),
                new Vector3(-halfWidth,  halfHeight, halfDepth),
                new Vector3(-halfWidth, -halfHeight, halfDepth),

                // Top face
                new Vector3(-halfWidth, halfHeight, -halfDepth),
                new Vector3(-halfWidth, halfHeight,  halfDepth),
                new Vector3( halfWidth, halfHeight,  halfDepth),
                new Vector3( halfWidth, halfHeight, -halfDepth),

                // Bottom face
                new Vector3(-halfWidth, -halfHeight,  halfDepth),
                new Vector3(-halfWidth, -halfHeight, -halfDepth),
                new Vector3( halfWidth, -halfHeight, -halfDepth),
                new Vector3( halfWidth, -halfHeight,  halfDepth),

                // Left face
                new Vector3(-halfWidth, -halfHeight,  halfDepth),
                new Vector3(-halfWidth,  halfHeight,  halfDepth),
                new Vector3(-halfWidth,  halfHeight, -halfDepth),
                new Vector3(-halfWidth, -halfHeight, -halfDepth),

                // Right face
                new Vector3(halfWidth, -halfHeight, -halfDepth),
                new Vector3(halfWidth,  halfHeight, -halfDepth),
                new Vector3(halfWidth,  halfHeight,  halfDepth),
                new Vector3(halfWidth, -halfHeight,  halfDepth)
            };

            // Apply center
            for (int i = 0; i < positions.Length; ++i)
                positions[i] += desc.center;

            // Generate normals
            Vector3[] normals = new Vector3[]
            {
                // Front face
                -Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward,
                
                // Back face
                Vector3.forward,  Vector3.forward, Vector3.forward, Vector3.forward,

                // Top face
                Vector3.up,  Vector3.up,  Vector3.up, Vector3.up,

                // Bottom face
                -Vector3.up, -Vector3.up, -Vector3.up, -Vector3.up,

                // Left face
                -Vector3.right, -Vector3.right,  -Vector3.right, -Vector3.right,

                // Right face
                Vector3.right, Vector3.right, Vector3.right, Vector3.right
            };

            // Generate indices
            int[] indices = new int[] 
            {
                // Front face
                0, 1, 2, 2, 3, 0,   

                // Back face
                4, 5, 6, 6, 7, 4,
                
                // Top face
                8, 9, 10, 8, 10, 11,

                // Bottom face
                12, 13, 14, 12, 14, 15,

                // Left face
                16, 17, 18, 16, 18, 19,

                // Right face
                20, 21, 22, 20, 22, 23
            };

            // Create and initialize mesh
            Mesh mesh       = new Mesh();
            mesh.vertices   = positions;
            mesh.normals    = normals;
            mesh.colors     = ColorEx.CreateColorArray(desc.color, positions.Length);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.UploadMeshData(false);

            // Return mesh
            return mesh;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateWireBox() (Public Static Function)
        // Desc: Creates a wire box mesh.
        // Parm: desc - Box descriptor.
        // Rtrn: The wire box mesh.
        //-----------------------------------------------------------------------------
        public static Mesh CreateWireBox(BoxMeshDesc desc)
        {
            // Update args if necessary
            if (desc.width   < 0.0f) desc.width  = 0.0f;
            if (desc.height  < 0.0f) desc.height = 0.0f;
            if (desc.depth   < 0.0f) desc.depth  = 0.0f;

            // Cache data
            float halfWidth  = desc.width * 0.5f;
            float halfHeight = desc.height * 0.5f;
            float halfDepth  = desc.depth * 0.5f;

            // Generate positions
            Vector3[] positions = new Vector3[]
            {
                // Front face
                new Vector3(-halfWidth, -halfHeight, -halfDepth),
                new Vector3(-halfWidth,  halfHeight, -halfDepth),
                new Vector3( halfWidth,  halfHeight, -halfDepth),
                new Vector3( halfWidth, -halfHeight, -halfDepth),

                // Back face
                new Vector3( halfWidth, -halfHeight, halfDepth),
                new Vector3( halfWidth,  halfHeight, halfDepth),
                new Vector3(-halfWidth,  halfHeight, halfDepth),
                new Vector3(-halfWidth, -halfHeight, halfDepth),
            };

            // Apply center
            for (int i = 0; i < positions.Length; ++i)
                positions[i] += desc.center;

            // Generate indices
            int[] indices = new int[] 
            {
                0, 1, 1, 2, 2, 3, 3, 0,     // Front face
                4, 5, 5, 6, 6, 7, 7, 4,     // Back face

                // Connect faces
                0, 7, 1, 6, 2, 5, 3, 4
            };

            // Create and initialize mesh
            Mesh mesh       = new Mesh();
            mesh.vertices   = positions;
            mesh.colors     = ColorEx.CreateColorArray(desc.color, positions.Length);
            mesh.SetIndices(indices, MeshTopology.Lines, 0);
            mesh.UploadMeshData(false);

            // Return mesh
            return mesh;
        }
        #endregion

        #region Segment
        //-----------------------------------------------------------------------------
        // Name: CreateSegment() (Public Static Function)
        // Desc: Creates a line segment mesh.
        // Parm: p0, p1 - Segment starting and ending points.
        //       color  - Segment vertex color.
        // Rtrn: The segment mesh.
        //-----------------------------------------------------------------------------
        public static Mesh CreateSegment(Vector3 p0, Vector3 p1, Color color)
        {
            // Create and initialize the mesh
            Mesh mesh       = new Mesh();
            mesh.vertices   = new Vector3[] { p0, p1 };
            mesh.colors     = new Color[] { color, color };
            mesh.SetIndices(new int[] { 0, 1 }, MeshTopology.Lines, 0);
            mesh.UploadMeshData(false);

            // Return mesh
            return mesh;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateTessellatedSegment() (Public Static Function)
        // Desc: Creates a tessellated line segment mesh.
        // Parm: p0, p1     - Segment starting and ending points.
        //       color      - Segment vertex color.
        //       pointCount - The number of tessellation points (>= 2).
        // Rtrn: The tessellated segment mesh.
        //-----------------------------------------------------------------------------
        public static Mesh CreateTessellatedSegment(Vector3 p0, Vector3 p1, Color color, int pointCount)
        {
            // Update args if necessary
            if (pointCount < 2) pointCount = 2;

            // Calculate segment direction and length
            Vector3 dir = (p1 - p0);
            float l = dir.magnitude;
            dir /= l;

            // Create the segment positions and indices
            Vector3[]   positions   = new Vector3[pointCount];
            int[]       indices     = new int[pointCount];      // We're using a line strip
            float step = l / (pointCount - 1);                  // Whole segment length / smaller segment count
            for (int i = 0; i < pointCount; ++i)
            {
                positions[i]    = p0 + dir * step * i;
                indices[i]      = i;
            }

            // Create and initialize the mesh
            Mesh mesh       = new Mesh();
            mesh.vertices   = positions;
            mesh.colors     = ColorEx.CreateColorArray(color, pointCount);
            mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
            mesh.UploadMeshData(false);

            // Return mesh
            return mesh;
        }
        #endregion

        #region Quad
        //-----------------------------------------------------------------------------
        // Name: CreateQuad() (Public Static Function)
        // Desc: Creates a quad mesh.
        // Parm: desc - Quad descriptor.
        // Rtrn: The quad mesh.
        //-----------------------------------------------------------------------------
        public static Mesh CreateQuad(QuadMeshDesc desc)
        {
            // Update args
            if (desc.width  < 0.0f) desc.width  = 0.0f;
            if (desc.height < 0.0f) desc.height = 0.0f;

            // Store half size
            float hw = desc.width  / 2.0f;
            float hh = desc.height / 2.0f;

            // Create the vertex array based on the specified plane
            Vector3[] positions     = null;
            Vector3[] normals       = null;
            switch (desc.quadPlane)
            {
                case EFlatMeshPlane.XY:

                    positions = new Vector3[]
                    {
                        new Vector3(-hw, -hh, 0.0f) + desc.center,
                        new Vector3(-hw,  hh, 0.0f) + desc.center,
                        new Vector3( hw,  hh, 0.0f) + desc.center,
                        new Vector3( hw, -hh, 0.0f) + desc.center
                    };
                    normals = new Vector3[] { -Vector3.forward, -Vector3.forward, -Vector3.forward, -Vector3.forward };
                    break;

                case EFlatMeshPlane.YZ:

                    positions = new Vector3[]
                    {
                        new Vector3(0.0f, -hh, -hw) + desc.center,
                        new Vector3(0.0f,  hh, -hw) + desc.center,
                        new Vector3(0.0f,  hh,  hw) + desc.center,
                        new Vector3(0.0f, -hh,  hw) + desc.center
                    };
                    normals = new Vector3[] { Vector3.right, Vector3.right, Vector3.right, Vector3.right };
                    break;

                case EFlatMeshPlane.ZX:

                    positions = new Vector3[]
                    {
                        new Vector3(-hw, 0.0f, -hh) + desc.center,
                        new Vector3(-hw, 0.0f,  hh) + desc.center,
                        new Vector3( hw, 0.0f,  hh) + desc.center,
                        new Vector3( hw, 0.0f, -hh) + desc.center
                    };
                    normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up, Vector3.up };
                    break;
            }

            // Create the index array
            ushort[] indices = new ushort[] { 0, 1, 2, 0, 2, 3 };

            // Create UVs
            Vector2[] uvs = new Vector2[]
            {
                new Vector2(0.0f, 0.0f), new Vector2(0.0f, 1.0f),
                new Vector2(1.0f, 1.0f), new Vector2(1.0f, 0.0f)
            };

            // Create and initialize the mesh
            Mesh mesh       = new Mesh();
            mesh.vertices   = positions;
            mesh.normals    = normals;
            mesh.uv         = uvs;
            mesh.colors     = ColorEx.CreateColorArray(desc.color, positions.Length);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.UploadMeshData(false);

            // Return mesh
            return mesh;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateWireQuad() (Public Static Function)
        // Desc: Creates a wire quad mesh.
        // Parm: desc - Quad descriptor.
        // Rtrn: The quad mesh.
        //-----------------------------------------------------------------------------
        public static Mesh CreateWireQuad(QuadMeshDesc desc)
        {
            // Update args
            if (desc.width  < 0.0f) desc.width  = 0.0f;
            if (desc.height < 0.0f) desc.height = 0.0f;

            // Store half size
            float hw = desc.width  / 2.0f;
            float hh = desc.height / 2.0f;

            // Create the vertex array based on the specified plane
            Vector3[] positions = null;
            switch (desc.quadPlane)
            {
                case EFlatMeshPlane.XY:

                    positions = new Vector3[]
                    {
                        new Vector3(-hw, -hh, 0.0f) + desc.center,
                        new Vector3(-hw,  hh, 0.0f) + desc.center,
                        new Vector3( hw,  hh, 0.0f) + desc.center,
                        new Vector3( hw, -hh, 0.0f) + desc.center,
                    };
                    break;

                case EFlatMeshPlane.YZ:

                    positions = new Vector3[]
                    {
                        new Vector3(0.0f, -hh, -hw) + desc.center,
                        new Vector3(0.0f,  hh, -hw) + desc.center,
                        new Vector3(0.0f,  hh,  hw) + desc.center,
                        new Vector3(0.0f, -hh,  hw) + desc.center,
                    };
                    break;

                case EFlatMeshPlane.ZX:

                    positions = new Vector3[]
                    {
                        new Vector3(-hw, 0.0f, -hh) + desc.center,
                        new Vector3(-hw, 0.0f,  hh) + desc.center,
                        new Vector3( hw, 0.0f,  hh) + desc.center,
                        new Vector3( hw, 0.0f, -hh) + desc.center,
                    };
                    break;
            }

            // Create the index array
            ushort[] indices = new ushort[] { 0, 1, 2, 3, 0 };

            // Create and initialize the mesh
            Mesh mesh       = new Mesh();
            mesh.vertices   = positions;
            mesh.colors     = ColorEx.CreateColorArray(desc.color, positions.Length);
            mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
            mesh.UploadMeshData(false);

            // Return mesh
            return mesh;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateRATriangle() (Public Static Function)
        // Desc: Creates a right-angled triangle mesh. The triangle's right-angled corner
        //       sits at the origin.   
        // Parm: desc - Triangle descriptor.
        // Rtrn: The triangle mesh.
        //-----------------------------------------------------------------------------
        public static Mesh CreateRATriangle(RATriangleMeshDesc desc)
        {
            // Update args
            if (desc.aeSize0 < 0.0f) desc.aeSize0 = 0.0f;
            if (desc.aeSize1 < 0.0f) desc.aeSize1 = 0.0f;

            // Create the vertex array based on the specified plane
            Vector3[] positions = null;
            Vector3[] normals   = null;
            switch (desc.trianglePlane)
            {
                case EFlatMeshPlane.XY:

                    positions = new Vector3[]
                    {
                        Vector3.zero,
                        Vector3.up * desc.aeSize0,
                        Vector3.right * desc.aeSize1,
                    };
                    normals = new Vector3[] { -Vector3.forward, -Vector3.forward, -Vector3.forward };
                    break;

                case EFlatMeshPlane.YZ:

                    positions = new Vector3[]
                    {
                        Vector3.zero,
                        Vector3.up * desc.aeSize0,
                        Vector3.forward * desc.aeSize1,
                    };
                    normals = new Vector3[] { Vector3.right, Vector3.right, Vector3.right };
                    break;

                case EFlatMeshPlane.ZX:

                    positions = new Vector3[]
                    {
                        Vector3.zero,
                        Vector3.forward * desc.aeSize0,
                        Vector3.right * desc.aeSize1,
                    };
                    normals = new Vector3[] { Vector3.up, Vector3.up, Vector3.up };
                    break;
            }

            // Create the index array
            ushort[] indices = new ushort[] { 0, 1, 2 };

            // Create and initialize the mesh
            Mesh mesh       = new Mesh();
            mesh.vertices   = positions;
            mesh.normals    = normals;
            mesh.colors     = ColorEx.CreateColorArray(desc.color, positions.Length);
            mesh.SetIndices(indices, MeshTopology.Triangles, 0);
            mesh.UploadMeshData(false);

            // Return mesh
            return mesh;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateWireRATriangle() (Public Static Function)
        // Desc: Creates a wire right-angled triangle mesh. The triangle's right-angled
        //       corner sits at the origin.   
        // Parm: desc - Triangle descriptor.
        // Rtrn: The triangle mesh.
        //-----------------------------------------------------------------------------
        public static Mesh CreateWireRATriangle(RATriangleMeshDesc desc)
        {
            // Update args
            if (desc.aeSize0 < 0.0f) desc.aeSize0 = 0.0f;
            if (desc.aeSize1 < 0.0f) desc.aeSize1 = 0.0f;

            // Create the vertex array based on the specified plane
            Vector3[] positions = null;
            switch (desc.trianglePlane)
            {
                case EFlatMeshPlane.XY:

                    positions = new Vector3[]
                    {
                        Vector3.zero,
                        Vector3.up * desc.aeSize0,
                        Vector3.right * desc.aeSize1,
                    };
                    break;

                case EFlatMeshPlane.YZ:

                    positions = new Vector3[]
                    {
                        Vector3.zero,
                        Vector3.up * desc.aeSize0,
                        Vector3.forward * desc.aeSize1,
                    };
                    break;

                case EFlatMeshPlane.ZX:

                    positions = new Vector3[]
                    {
                        Vector3.zero,
                        Vector3.forward * desc.aeSize0,
                        Vector3.right * desc.aeSize1,
                    };
                    break;
            }

            // Create the index array
            ushort[] indices = new ushort[] { 0, 1, 2, 0 };

            // Create and initialize the mesh
            Mesh mesh       = new Mesh();
            mesh.vertices   = positions;
            mesh.colors     = ColorEx.CreateColorArray(desc.color, positions.Length);
            mesh.SetIndices(indices, MeshTopology.LineStrip, 0);
            mesh.UploadMeshData(false);

            // Return mesh
            return mesh;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateBlitQuad() (Public Static Function)
        // Desc: Creates a quad mesh that can be used for blit operations. The quad uses
        //       NDC coordinates to store the vertex positions.
        // Rtrn: The blit quad mesh.
        //-----------------------------------------------------------------------------
        public static Mesh CreateBlitQuad()
        {
            // Create and initialize the mesh
            Mesh mesh       = new Mesh();
            mesh.vertices   = new Vector3[] { new Vector3(-1.0f, -1.0f), new Vector3(1.0f, -1.0f),   new Vector3(1.0f, 1.0f), new Vector3(-1.0f, 1.0f) };

            // It seems that in Unity, NDC looks like this (?):
            // (-1, -1) ------------- (1, -1)
            //     |                     |
            //     |                     |
            // (-1,  1) ------------- (1,  1)
            mesh.uv = new Vector2[] { new Vector2(0.0f, 1.0f), new Vector2(1.0f, 1.0f), new Vector2(1.0f, 0.0f), new Vector2(0.0f, 0.0f) };

            mesh.SetIndices(new int[] { 0, 1, 2, 0, 2, 3 }, MeshTopology.Triangles, 0);
            mesh.UploadMeshData(false);

            // Return mesh
            return mesh;
        }
        #endregion
        #endregion
    }
    #endregion
}
