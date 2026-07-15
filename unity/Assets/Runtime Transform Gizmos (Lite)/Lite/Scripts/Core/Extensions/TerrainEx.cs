using UnityEngine;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: TerrainVertexRange (Public Struct)
    // Desc: Represents a 2D vertex range expressed as minimum and maximum X & Z
    //       heightmap coordinates. The max coordinates are exclusive.
    //-----------------------------------------------------------------------------
    public struct TerrainVertexRange
    {
        #region Public Fields
        public int minX;    // Min vertex along X
        public int maxX;    // Max vertex along X (exclusive)
        public int minZ;    // Min vertex along Z
        public int maxZ;    // Max vertex along Z (exclusive)
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: width (Public Property)
        // Desc: Returns the range width (i.e. number of vertices along X).
        //-----------------------------------------------------------------------------
        public int width { get { return maxX - minX; } }

        //-----------------------------------------------------------------------------
        // Name: depth (Public Property)
        // Desc: Returns the range depth (i.e. number of vertices along Z).
        //-----------------------------------------------------------------------------
        public int depth { get { return maxZ - minZ; } }
        #endregion
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: TerrainEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'Terrain' class.
    //-----------------------------------------------------------------------------
    public static class TerrainEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: CalculateVertexRange() (Public Extension)
        // Desc: Calculates the vertex range for the specified center and radius. Useful
        //       for calculating the range of terrain vertices that enclose a circle.
        // Parm: center - Circle center.
        //       radius - Circle radius.
        // Rtrn: Terrain vertex range that encloses the specified circle.
        //-----------------------------------------------------------------------------
        public static TerrainVertexRange CalculateVertexRange(this Terrain terrain, Vector3 center, float radius)
        {
            // Calculate extents in terrain space
            Vector3 terrainPos = terrain.transform.position;
            Vector3 min = center - Vector3.right * radius - Vector3.forward * radius - terrainPos;
            Vector3 max = center + Vector3.right * radius + Vector3.forward * radius - terrainPos;

            // Calculate normalized extents
            TerrainData terrainData = terrain.terrainData;
            float uMin = min.x / terrainData.size.x;
            float uMax = max.x / terrainData.size.x;
            float vMin = min.z / terrainData.size.z;
            float vMax = max.z / terrainData.size.z;

            // Calculate min/max vert row & columns
            int minCol      = Mathf.Clamp(Mathf.FloorToInt(uMin * terrainData.heightmapResolution), 0, terrainData.heightmapResolution - 1);
            int maxCol      = Mathf.Clamp(Mathf.FloorToInt(uMax * terrainData.heightmapResolution), 0, terrainData.heightmapResolution - 1);
            int minDepth    = Mathf.Clamp(Mathf.FloorToInt(vMin * terrainData.heightmapResolution), 0, terrainData.heightmapResolution - 1);
            int maxDepth    = Mathf.Clamp(Mathf.FloorToInt(vMax * terrainData.heightmapResolution), 0, terrainData.heightmapResolution - 1);

            // Return range
            return new TerrainVertexRange
            {
                minX = minCol,
                maxX = maxCol + 1,
                minZ = minDepth,
                maxZ = maxDepth + 1
            };
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateModelAABB() (Public Extension)
        // Desc: Calculates and returns the terrain's model space AABB.
        // Rtrn: The terrain's model space AABB. If the terrain data is invalid, the
        //       function will return an invalid AABB.
        //-----------------------------------------------------------------------------
        public static Box CalculateModelAABB(this Terrain terrain)
        {
            // Validate terrain data
            TerrainData terrainData = terrain.terrainData;
            if (terrainData == null) return Box.GetInvalid();

            // Return terrain model AABB
            return new Box(terrainData.bounds.center, terrainData.bounds.size);
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateWorldOBB() (Public Extension)
        // Desc: Calculates and returns the terrain's world space OBB.
        // Rtrn: The terrain's world space OBB. If the terrain data is invalid, the
        //       function will return an invalid OBB.
        //-----------------------------------------------------------------------------
        public static OBox CalculateWorldOBB(this Terrain terrain)
        {
            // Calculate model space AABB
            var modelABB = terrain.CalculateModelAABB();
            if (!modelABB.isValid) return OBox.GetInvalid();

            // Return world space OBB
            return new OBox(modelABB.center + terrain.transform.position, modelABB.size);
        }

        //-----------------------------------------------------------------------------
        // Name: SampleWorldHeight() (Public Extension)
        // Desc: Samples the terrain world height at the specified point.
        // Parm: point - Sample point.
        // Rtrn: The terrain world height at the specified point.
        //-----------------------------------------------------------------------------
        public static float SampleWorldHeight(this Terrain terrain, Vector3 point)
        {
            return terrain.transform.position.y + terrain.SampleHeight(point);
        }

        //-----------------------------------------------------------------------------
        // Name: GetDistanceToPoint() (Public Extension)
        // Desc: Returns the distance from the terrain surface to the specified point
        //       along the world Y axis.
        // Parm: point - Query point.
        // Rtrn: The distance from the terrain surface to the specified point along the
        //       world Y axis. This is a positive value if the point is above a terrain
        //       and a negative value if the point is below the terrain surface.
        //-----------------------------------------------------------------------------
        public static float GetDistanceToPoint(this Terrain terrain, Vector3 point)
        {
            return (point.y - terrain.SampleWorldHeight(point));
        }

        //-----------------------------------------------------------------------------
        // Name: ProjectPoint() (Public Extension)
        // Desc: Projects the specified point onto the terrain surface and returns the
        //       resulting position.
        // Parm: point - Point to be projected.
        // Rtrn: The projected point.
        //-----------------------------------------------------------------------------
        public static Vector3 ProjectPoint(this Terrain terrain, Vector3 point)
        {
            return point - Vector3.up * terrain.GetDistanceToPoint(point);
        }
        #endregion
    }
    #endregion
}