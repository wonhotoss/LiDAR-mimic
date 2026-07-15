using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: CameraFocusData (Public Struct)
    // Desc: Provides storage for the kind of data needed to focus a camera.
    //-----------------------------------------------------------------------------
    public struct CameraFocusData
    {
        #region Public Fields
        public float    zoom;       // The distance from the camera to the focus target
        public Vector3  position;   // The camera focus position
        public float    orthoSize;  // The size of the ortho view volume
        #endregion
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: CameraEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'Camera' class.
    //-----------------------------------------------------------------------------
    public static class CameraEx
    {
        #region Public Static Fields
        // Buffers used to avoid memory allocations
        static Plane[]  sFrustumPlaneBuffer = new Plane[6]; 
        #endregion

        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: WorldToScreenPoints() (Public Extension)
        // Desc: Converts all world points in 'worldPoints' to screen coordinates and stores
        //       them in 'screenPoints'.
        // Parm: worldPoints  - List of world points to convert.
        //       screenPoints - Returns the converted points as 'Vector3'.
        //-----------------------------------------------------------------------------
        public static void WorldToScreenPoints(this Camera camera, List<Vector3> worldPoints, List<Vector3> screenPoints)
        {
            // Clear output
            screenPoints.Clear();

            // Loop through each world point and convert it
            int count = worldPoints.Count;
            for (int i = 0; i < count; ++i)
                screenPoints.Add(camera.WorldToScreenPoint(worldPoints[i]));
        }

        //-----------------------------------------------------------------------------
        // Name: WorldToScreenPoints() (Public Extension)
        // Desc: Converts all world points in 'worldPoints' to screen coordinates and stores
        //       them in 'screenPoints'.
        // Parm: worldPoints  - List of world points to convert.
        //       screenPoints - Returns the converted points as 'Vector2'.
        //-----------------------------------------------------------------------------
        public static void WorldToScreenPoints(this Camera camera, List<Vector3> worldPoints, List<Vector2> screenPoints)
        {
            // Clear output
            screenPoints.Clear();

            // Loop through each world point and convert it
            int count = worldPoints.Count;
            for (int i = 0; i < count; ++i)
                screenPoints.Add(camera.WorldToScreenPoint(worldPoints[i]));
        }

        //-----------------------------------------------------------------------------
        // Name: RenderTexture_GUIPointToRay() (Public Extension)
        // Desc: Converts the specified GUI point to a world ray which can be used for
        //       picking. This function should be used when the camera uses a render
        //       texture but the result needs to be output to a rectangle on the screen.
        // Parm: guiPt       - GUI point defined in the screen coordinate system where the 
        //                     Y axis grows downwards.
        //       guiViewRect - GUI space rectangle where the camera render texture is
        //                     displayed.
        // Rtrn: The world ray which can be used for picking.
        //-----------------------------------------------------------------------------
        public static Ray RenderTexture_GUIPointToRay(this Camera camera, Vector2 guiPt, Rect guiViewRect)
        {
            // Transform the GUI coords to normalized device coordinates. In the transformation
            // pipeline, the normalized coordinates are transformed to screen space using the
            // following formulas:
            //   sx =  nx * w / 2 + w / 2 + viewX;   // nx - normalized X coord, w - viewport width,  viewX - viewport starting X coordinate.
            //   sy = -ny * h / 2 + h / 2 + viewY;   // ny - normalized Y coord, h - viewport height, viewY - viewport starting Y coordinate.
            // We need to do the reverse. We have sx and sy and we need to solve for nx and ny.
            //-----------------------------------------------------------------------------
            //   nx * w / 2 = sx - w / 2 - viewX;
            //   nx = (sx - w / 2 - viewX) / (w / 2);
            //   nx = 2 * (sx - w / 2 - viewX) / w;
            //   nx = (2 * (sx - viewX)) / w - 1;
            //-----------------------------------------------------------------------------
            //  -ny * h / 2 = sy - h / 2 - viewY;
            //  -ny = (sy - h / 2 - viewY) / (h / 2);
            //  -ny =   2 * (sy - h / 2 - viewY) / h;
            //   ny =  -2 * (sy - h / 2 - viewY) / h;
            //   ny = (-2 * (sy - viewY)) / h + 1;
            //-----------------------------------------------------------------------------
            Vector2 s   = guiPt;  
            float nx    =  ( 2.0f * (s.x - guiViewRect.x)) / guiViewRect.width  - 1.0f;
            float ny    =  (-2.0f * (s.y - guiViewRect.y)) / guiViewRect.height + 1.0f;

            // Are we an ortho camera?
            Ray ray = new Ray();
            if (camera.orthographic)
            {
                // Create the ray in world space. This is much more simple to do in ortho mode.
                ray.origin      = camera.transform.position + 
                                  camera.transform.right * camera.orthographicSize * nx * camera.aspect + 
                                  camera.transform.up * camera.orthographicSize * ny;
                ray.direction   = camera.transform.forward;
            }
            else
            {
                // The next step would be to tackle the divide by Z. However, we can simply assume that
                // Z is 1. This works because a ray shooting through (nx, ny) is the same at Z = 1, Z = 2, etc.
                // So we can simply skip to applying the inverse transform of the projection matrix.
                Matrix4x4 projectionMtx = camera.projectionMatrix;
                float x = nx / projectionMtx.m00;
                float y = ny / projectionMtx.m11;

                // Create ray in view space
                ray = new Ray(Vector3.zero, new Vector3(x, y, -1.0f).normalized);

                // Convert the view space ray to a world space ray by applying the inverse transform of the view matrix
                Matrix4x4 invViewMtx = camera.cameraToWorldMatrix;
                ray.origin      = invViewMtx.MultiplyPoint(ray.origin);
                ray.direction   = invViewMtx.MultiplyVector(ray.direction).normalized;
            }
          
            // Return ray
            return ray;
        }

        //-----------------------------------------------------------------------------
        // Name: RenderTexture_WorldToGUIPoint() (Public Extension)
        // Desc: Converts the specified world point to GUI space where the Y axis grows
        //       downwards.
        // Parm: worldPt     - The world point to convert to a GUI point.
        //       guiViewRect - GUI space rectangle where the camera render texture is
        //                     displayed.
        // Rtrn: The GUI point.
        //-----------------------------------------------------------------------------
        public static Vector2 RenderTexture_WorldToGUIPoint(this Camera camera, Vector3 worldPt, Rect guiViewRect)
        {
            // Convert to view space
            Vector4 pt = camera.worldToCameraMatrix.MultiplyPoint(worldPt);

            // Convert to projection space
            pt.w = 1.0f;    // Needed for ortho camera to divide by 1
            pt = camera.projectionMatrix * pt;

            // Divide by Z to get the normalized coordinates
            float nx = pt.x / pt.w;
            float ny = pt.y / pt.w;

            // Convert to screen coordinates
            return new Vector2(guiViewRect.x + guiViewRect.width  / 2.0f *  nx + guiViewRect.width  / 2.0f,
                               guiViewRect.y + guiViewRect.height / 2.0f * -ny + guiViewRect.height / 2.0f);
        }

        //-----------------------------------------------------------------------------
        // Name: RenderTexture_GUIToWorldPoint() (Public Extension)
        // Desc: Converts the specified GUI point to a world position. This function should
        //       be used when the camera uses a render texture but the result needs to be
        //       output to a rectangle on the screen.
        // Parm: guiPt       - GUI point defined in the screen coordinate system where the 
        //                     Y axis grows downwards. The Z coordinate is the distance
        //                     form the camera in world units.
        //       guiViewRect - GUI space rectangle where the camera render texture is
        //                     displayed.
        // Rtrn: The world point.
        //-----------------------------------------------------------------------------
        public static Vector3 RenderTexture_GUIToWorldPoint(this Camera camera, Vector3 guiPt, Rect guiViewRect)
        {
            // Convert the GUI point to NDC coords
            Vector2 s   = guiPt;  
            float nx    =  ( 2.0f * (s.x - guiViewRect.x)) / guiViewRect.width  - 1.0f;
            float ny    =  (-2.0f * (s.y - guiViewRect.y)) / guiViewRect.height + 1.0f;

            // Are we an ortho camera?
            if (camera.orthographic)
            {
                // For an ortho camera, we can just offset along the camera axes using the ortho size to get the world point
                return  camera.transform.position   +
                        camera.transform.right      *   camera.orthographicSize * nx * camera.aspect +
                        camera.transform.up         *   camera.orthographicSize * ny + camera.transform.forward * guiPt.z;
            }
            else
            {
                // Cancel the divide by Z
                float x = nx * guiPt.z;
                float y = ny * guiPt.z;

                // Apply inverse projection matrix transform
                Matrix4x4 projectionMtx = camera.projectionMatrix;
                x = x / projectionMtx.m00;
                y = y / projectionMtx.m11;

                // We no have the X, Y and Z coords in view space. We can use these to offset from
                // the camera position along the camera axes the get the world point.
                return camera.transform.position + camera.transform.right * x + camera.transform.up * y + camera.transform.forward * guiPt.z;
            }        
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateFocusData() (Public Extension)
        // Desc: Calculates the focus data required to focus the camera on the specified
        //       sphere target.
        // Parm: sphere - The sphere focus target.
        // Rtrn: An instance of 'CameraFocusData' which can be used to focus the camera.
        //-----------------------------------------------------------------------------
        public static CameraFocusData CalculateFocusData(this Camera camera, Sphere sphere)
        {
            //-----------------------------------------------------------------------------
            // We need to solve the following equation:
            // (C + F * t) * N + D = R
            // Where: C - camera position; F - camera forward axis; N - plane normal; D - plane distance from origin; R - sphere radius.
            //        t - this is the unknown we need to solve for and it represents the distance along the
            //        camera forward vector that we have to move starting from the camera position to reach a
            //        point in front of the camera where the distance between the sphere center and the camera
            //        plane is equal to the sphere radius. In other words, t is the distance along the camera
            //        forward axis where the sphere rests nicely against the frustum plane.
            //-----------------------------------------------------------------------------
            // Calculate the plane we need to use in the above equation
            Plane focusPlane = camera.CalculateFocusFrustumPlane();

            // Calculate t
            float dot = Vector3.Dot(camera.transform.position, focusPlane.normal);
            float t = (sphere.radius - focusPlane.distance - dot) / Vector3.Dot(camera.transform.forward, focusPlane.normal);

            // At this point 't' contains the distance from the camera position where the sphere's nearest
            // extent can be placed such that its extents rest nicely against the frustum planes. However,
            // if the sphere extent in the direction of the camera near plane penetrates the near plane,
            // we need to offset 't'.
            float d = t - camera.nearClipPlane;
            if (d < sphere.radius) t += sphere.radius - d;
         
            // Calculate the focus data
            var focusData = new CameraFocusData();
            focusData.zoom          = t;
            focusData.orthoSize     = camera.aspect >= 1.0f ? sphere.radius : sphere.radius / camera.aspect;
            focusData.position      = sphere.center - camera.transform.forward * t;

            // Return focus data
            return focusData;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateFocusFrustumPlane() (Public Extension)
        // Desc: Calculates and returns the focus frustum plane. The focus frustum plane
        //       is the frustum plane that touches the volume of the entity the camera
        //       is focused on.
        // Rtrn: The focus frustum plane.
        //-----------------------------------------------------------------------------
        public static Plane CalculateFocusFrustumPlane(this Camera camera)
        {
            // Note: If the camera is orthographic, we need to switch to perspective and restore after we're done.
            bool isOrtho = camera.orthographic;
            if (isOrtho) camera.orthographic = false;

            // Calculate frustum plane. If the camera screen is wider than it is higher (i.e. aspect >= 1.0f),
            // this is the top plane. Otherwise it is the left plane. We want to pick the plane that corresponds
            // to the smallest screen dimension. This is because if we fit the focus volume inside this region,
            // it is guaranteed to be visible along the larger dimension also.
            GeometryUtility.CalculateFrustumPlanes(camera, sFrustumPlaneBuffer);
            Plane plane = camera.aspect >= 1.0f ? sFrustumPlaneBuffer[3] : sFrustumPlaneBuffer[0];

            // Restore camera ortho state
            if (isOrtho) camera.orthographic = true;

            // Return plane
            return plane;
        }

        //-----------------------------------------------------------------------------
        // Name: Focus() (Public Extension)
        // Desc: Focuses the camera using the specified focus data.
        // Parm: focusData - The focus data used to focus the camera.
        //-----------------------------------------------------------------------------
        public static void Focus(this Camera camera, CameraFocusData focusData)
        {
            camera.transform.position = focusData.position;
            camera.orthographicSize = focusData.orthoSize;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateFarMidPoint() (Public Extension)
        // Desc: Calculates and returns the point sitting in the middle of the far plane.
        // Rtrn: The point sitting in the middle of the far plane.
        //-----------------------------------------------------------------------------
        public static Vector3 CalculateFarMidPoint(this Camera camera)
        {
            return camera.transform.position + camera.transform.forward * camera.farClipPlane;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateOrthoFarMidTopPoint() (Public Extension)
        // Desc: Calculates and returns the point sitting in the middle top of the camera's
        //       orthographic view volume.
        // Rtrn: The point sitting in the middle top of the camera's orthographic view volume.
        //-----------------------------------------------------------------------------
        public static Vector3 CalculateOrthoFarMidTopPoint(this Camera camera)
        {
            return camera.CalculateFarMidPoint() + camera.transform.up * camera.orthographicSize;
        }

        //-----------------------------------------------------------------------------
        // Name: CalculateOrthoFOV() (Public Extension)
        // Desc: Treats the camera's ortho frustum as a perspective frustum and returns
        //       its FOV.
        // Rtrn: The FOV of the camera's ortho frustum as if it were a perspective frustum
        //       where the camera rays shoot out from the camera position.
        //-----------------------------------------------------------------------------
        public static float CalculateOrthoFOV(this Camera camera)
        {
            // Calculate 2 points:
            //  1. middle point in the camera's far plane.
            //  2. middle top point in the camera's ortho far plane.
            Vector3 camPos      = camera.transform.position;
            Vector3 vMidFar     = camera.CalculateFarMidPoint() - camPos;
            Vector3 vMidFarTop  = camera.CalculateOrthoFarMidTopPoint() - camPos;

            // Calculate the angle between these two vectors and multiply by 2 to get the FOV
            return Vector3.Angle(vMidFar, vMidFarTop) * 2.0f;
        }

        //-----------------------------------------------------------------------------
        // Name: FrustumHeightToZ() (Public Extension)
        // Desc: Returns the Z offset from the camera position along the camera view
        //       vector where the perspective frustum's height is 'height'.
        // Parm: height - The frustum height which must be converted to a Z offset.
        // Rtrn: The Z offset from the camera position along the camera view vector where
        //       the perspective frustum's height is 'height'.
        //-----------------------------------------------------------------------------
        public static float FrustumHeightToZ(this Camera camera, float height)
        {
            // Z = height / 2 * ctan(fov / 2)
            return (height / 2.0f) / Mathf.Tan(camera.fieldOfView / 2.0f * Mathf.Deg2Rad);
        }

        //-----------------------------------------------------------------------------
        // Name: ScreenToWorldSize() (Public Extension)
        // Desc: Converts a screen size value to a world size given the specified world
        //       position.
        // Parm: worldPos   - The world position where the world size is being calculated.
        //       screenSize - The screen size to convert to a world size at the specified
        //                    world position.
        // Rtrn: The world size that maps to the specified screen size at the specified
        //       world position.
        //-----------------------------------------------------------------------------
        public static float ScreenToWorldSize(this Camera camera, Vector3 worldPos, float screenSize)
        {
            // Ortho camera?
            if (camera.orthographic) return (screenSize / camera.pixelHeight) * (camera.orthographicSize * 2.0f);
            else
            {
                // Calculate the distance between the camera position and the world point along the camera forward axis
                // and then calculate the height of the frustum at that distance from the camera.
                float d = Vector3.Dot(camera.transform.forward, (worldPos - camera.transform.position));
                float frustumHeight = 2.0f * Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * d;

                // Return the world size
                return (screenSize / camera.pixelHeight) * frustumHeight;
            }
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: ScreenToGUIPoint() (Public Extension)
        // Desc: Converts the specified screen point to a GUI point. Useful when rendering
        //       UI inside 'OnGUI'.
        // Parm: screenPt - A point in screen space where the Y axis grows upwards and
        //                  the point <0, 0> is at the bottom left corner of the screen.
        // Rtrn: A point in GUI space where the Y axis grows downwards and the point <0, 0>
        //       is at the top left corner of the screen.
        //-----------------------------------------------------------------------------
        public static Vector3 ScreenToGUIPoint(Vector3 screenPt)
        {
            return new Vector3(screenPt.x, Screen.height - screenPt.y, screenPt.z);
        }

        //-----------------------------------------------------------------------------
        // Name: ScreenToGUIPoint() (Public Extension)
        // Desc: Converts the specified screen point to a GUI point. Useful when rendering
        //       UI inside 'OnGUI'.
        // Parm: screenPt - A point in screen space where the Y axis grows upwards and
        //                  the point <0, 0> is at the bottom left corner of the screen.
        // Rtrn: A point in GUI space where the Y axis grows downwards and the point <0, 0>
        //       is at the top left corner of the screen.
        //-----------------------------------------------------------------------------
        public static Vector2 ScreenToGUIPoint(Vector2 screenPt)
        {
            return new Vector2(screenPt.x, Screen.height - screenPt.y);
        }
        #endregion
    }
    #endregion
}
