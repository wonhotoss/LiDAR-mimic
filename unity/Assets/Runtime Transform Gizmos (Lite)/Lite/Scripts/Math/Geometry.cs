using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: Geometry (Public Static Class)
    // Desc: Implements useful geometry related functions.   
    //-----------------------------------------------------------------------------
    public static class Geometry
    {
        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: GenerateCirclePoints() (Public Static Function)
        // Desc: Generates a list of points that sit on a circle's circumference.
        // Parm: circle         - The circle primitive whose points are generated.
        //       pointCount     - Number of points that make up the circle. If < 3, a value
        //                        of 3 is used instead.
        //       points         - Returns the circle points.
        //       indices        - If null, it is ignored. Otherwise, it returns the indices
        //                        which allow the circle to be rendered with a 'LineStrip'
        //                        mesh topology.
        //       indexOffset    - If 'indices' is valid, this value will be added to the
        //                        generated indices.
        //-----------------------------------------------------------------------------
        public static void GenerateCirclePoints(Circle circle, int pointCount, List<Vector3> points, List<int> indices = null, int indexOffset = 0)
        {
            // Clear output
            points.Clear();
            if (indices != null) indices.Clear();

            // Update args if necessary
            if (circle.radius < 0.0f)  circle.radius    = 0.0f;
            if (pointCount < 3)         pointCount      = 3;

            // Generate the points around the circle's normal
            Vector3 right = circle.radiusAxis;
            Vector3 up    = circle.upAxis;
            for (int i = 0; i < pointCount; ++i)
            {
                // Generate the circle point
                float angle = i / (float)pointCount * Mathf.PI * 2.0f;
                Vector3 pt = right  * Mathf.Cos(angle) * circle.radius +
                             up     * Mathf.Sin(angle) * circle.radius;

                // Move center
                pt += circle.center;

                // Store point
                points.Add(pt);
            }

            // Generate the indices if necessary
            if (indices != null)
            {
                // Generate indices for a line strip
                for (int i = 0; i <= pointCount; ++i)
                    indices.Add((i % pointCount) + indexOffset);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: GenerateSphereBorderPoints() (Public Static Function)
        // Desc: Generates a list of points that represent the border of a sphere.
        // Parm: sphere         - The function generates points that resemble the border
        //                        of this sphere.
        //       camera         - The camera that renders the sphere border.
        //       pointCount     - Number of points that make up the sphere border. If < 3,
        //                        a value of 3 is used instead.
        //       points         - Returns the border points.
        //-----------------------------------------------------------------------------
        public static void GenerateSphereBorderPoints(Sphere sphere, Camera camera, int pointCount, List<Vector3> points)
        {
            // Clear output
            points.Clear();

            // Update args if necessary
            if (pointCount < 3) pointCount = 3;

            // Generate the points around the sphere using the camera right and up vectors
            Vector3 right   = camera.transform.right;
            Vector3 up      = camera.transform.up;
            Vector3 camPos  = camera.transform.position;
            for (int i = 0; i < pointCount; ++i)
            {
                // Generate the circle point
                float angle = i / (float)pointCount * Mathf.PI * 2.0f;
                Vector3 pt = right  * Mathf.Cos(angle) * sphere.radius +
                             up     * Mathf.Sin(angle) * sphere.radius;

                // Move center
                pt += sphere.center;

                // Update point to turn it into a border point
                pt = sphere.ExtentToTangentPoint(pt, camera);

                // Store point
                points.Add(pt);
            }
        }
        #endregion
    }
    #endregion
}
