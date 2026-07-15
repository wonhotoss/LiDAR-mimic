using UnityEngine;
using System;
using UnityEngine.Rendering.RenderGraphModule;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EGridUpdateAction (Public Enum)
    // Desc: Defines different actions the grid can perform during its update phase.
    //-----------------------------------------------------------------------------
    public enum EGridUpdateAction
    {
        None = 0,               // No action
        SnapToPickPoint,        // Snap grid to pick point
        SnapToPickPointExtents  // Snap grid to pick point extents
    }
    #endregion

    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: RTGridRayHit (Public Struct)
    // Desc: Stores information for a grid ray hit.
    //-----------------------------------------------------------------------------
    public struct RTGridRayHit
    {
        #region Public Fields
        public RTGrid   grid;   // The hit grid (null when no hit)
        public Vector3  normal; // The hit normal
        public Vector3  point;  // The hit point
        public float    t;      // The hit distance from the ray origin
        #endregion
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTGridSettings (Public Class)
    // Desc: Stores settings for the 'RTGrid' instance.
    //-----------------------------------------------------------------------------
    [Serializable] public class RTGridSettings
    {
        #region Private Fields
        [SerializeField] float      mLocalYOffset   = defaultLocalYOffset;  // Offset along the grid's local Y axis
        [SerializeField] Vector3    mCellSize       = defaultCellSize;      // Cell size
        [SerializeField] EPlane     mPlane          = defaultPlane;         // The grid plane identifier. Controls the orientation of the grid in the scene.
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: localYOffset (Public Property)
        // Desc: Returns or sets the grid's Y offset along its local Y axis.
        //-----------------------------------------------------------------------------
        public float    localYOffset    { get { return mLocalYOffset; } set { mLocalYOffset = value; } }

        //-----------------------------------------------------------------------------
        // Name: cellSize (Public Property)
        // Desc: Returns or sets the grid's cell size.
        //-----------------------------------------------------------------------------
        public Vector3  cellSize        { get { return mCellSize; } set { mCellSize = Vector3.Max(value, new Vector3(0.1f, 0.1f, 0.1f)); } }

        //-----------------------------------------------------------------------------
        // Name: plane (Public Property)
        // Desc: Returns or sets the grid's plane. This controls the orientation of the
        //       grid.
        //-----------------------------------------------------------------------------
        public EPlane   plane           { get { return mPlane; } set { mPlane = value; } }
        #endregion

        #region Public Static Properties
        public static float     defaultLocalYOffset { get { return 0.0f; } }
        public static Vector3   defaultCellSize     { get { return Vector3.one; } }
        public static EPlane    defaultPlane        { get { return EPlane.ZX; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: UseDefaults() (Public Function)
        // Desc: Use default settings.
        //-----------------------------------------------------------------------------
        public void UseDefaults()
        {
            localYOffset    = defaultLocalYOffset;
            cellSize        = defaultCellSize;
            plane           = defaultPlane;
        }
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: RTGridStyle (Public Class)
    // Desc: Stores style settings for the 'RTGrid' instance.
    //-----------------------------------------------------------------------------
    [Serializable] public class RTGridStyle
    {
        #region Private Fields
        [SerializeField] bool  mVisible         = defaultVisible;           // Grid visibility toggle
        [SerializeField] bool  mZoomFadeEnabled = defaultZoomFadeEnabled;   // Is zoom fading enabled?
        [SerializeField] Color mCellLineColor   = defaultCellLineColor;     // Cell line color
        [SerializeField] Color mXAxisColor      = defaultXAxisColor;        // The color of the grid's X axis
        [SerializeField] Color mYAxisColor      = defaultYAxisColor;        // The color of the grid's Y axis
        [SerializeField] Color mZAxisColor      = defaultZAxisColor;        // The color of the grid's Z axis
        [SerializeField] bool  mDrawGridAxes    = defaultDrawGridAxes;      // Should the grid axes be rendered?
        [SerializeField] bool  mFiniteXAxis     = defaultFiniteXAxis;       // Is the X axis finite?
        [SerializeField] float mXAxisLength     = defaultXAxisLength;       // X axis length when finite
        [SerializeField] bool  mFiniteYAxis     = defaultFiniteYAxis;       // Is the Y axis finite?
        [SerializeField] float mYAxisLength     = defaultYAxisLength;       // Y axis length when finite
        [SerializeField] bool  mFiniteZAxis     = defaultFiniteZAxis;       // Is the Z axis finite?
        [SerializeField] float mZAxisLength     = defaultZAxisLength;       // Z axis length when finite
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: visible (Public Property)
        // Desc: Returns or sets the grid visibility.
        //-----------------------------------------------------------------------------
        public bool  visible            { get { return mVisible; } set { mVisible = value; } }

        //-----------------------------------------------------------------------------
        // Name: zoomFadeEnabled (Public Property)
        // Desc: Returns or sets whether zoom fading is enabled. When enabled, as the
        //       camera moves away from the grid, smaller cells fade out making larger
        //       cells more prominent.
        //-----------------------------------------------------------------------------
        public bool  zoomFadeEnabled    { get { return mZoomFadeEnabled; } set { mZoomFadeEnabled = value; } }

        //-----------------------------------------------------------------------------
        // Name: cellLineColor (Public Property)
        // Desc: Returns or sets the cell line color.
        //-----------------------------------------------------------------------------
        public Color cellLineColor      { get { return mCellLineColor; } set { mCellLineColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: xAxisColor (Public Property)
        // Desc: Returns or sets the X axis color used when drawing the grid's coordinate
        //       system axes.
        //-----------------------------------------------------------------------------
        public Color xAxisColor         { get { return mXAxisColor; } set { mXAxisColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: yAxisColor (Public Property)
        // Desc: Returns or sets the Y axis color used when drawing the grid's coordinate
        //       system axes.
        //-----------------------------------------------------------------------------
        public Color yAxisColor         { get { return mYAxisColor; } set { mYAxisColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: zAxisColor (Public Property)
        // Desc: Returns or sets the Z axis color used when drawing the grid's coordinate
        //       system axes.
        //-----------------------------------------------------------------------------
        public Color zAxisColor         { get { return mZAxisColor; } set { mZAxisColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: drawGridAxes (Public Property)
        // Desc: Returns or sets whether the grid axes should be rendered.
        //-----------------------------------------------------------------------------
        public bool  drawGridAxes       { get { return mDrawGridAxes; } set { mDrawGridAxes = value; } }

        //-----------------------------------------------------------------------------
        // Name: finiteXAxis (Public Property)
        // Desc: Returns or sets whether the X axis is finite.
        //-----------------------------------------------------------------------------
        public bool  finiteXAxis        { get { return mFiniteXAxis; } set { mFiniteXAxis = value; } }

        //-----------------------------------------------------------------------------
        // Name: xAxisLength (Public Property)
        // Desc: Returns or sets the length of the X axis when 'finiteXAxis' is true.
        //-----------------------------------------------------------------------------
        public float xAxisLength        { get { return mXAxisLength; } set { mXAxisLength = Mathf.Max(value, 0.0f); } }

        //-----------------------------------------------------------------------------
        // Name: finiteYAxis (Public Property)
        // Desc: Returns or sets whether the Y axis is finite.
        //-----------------------------------------------------------------------------
        public bool  finiteYAxis        { get { return mFiniteYAxis; } set { mFiniteYAxis = value; } }

        //-----------------------------------------------------------------------------
        // Name: yAxisLength (Public Property)
        // Desc: Returns or sets the length of the Y axis when 'finiteYAxis' is true.
        //-----------------------------------------------------------------------------
        public float yAxisLength        { get { return mYAxisLength; } set { mYAxisLength = Mathf.Max(value, 0.0f); } }

        //-----------------------------------------------------------------------------
        // Name: finiteZAxis (Public Property)
        // Desc: Returns or sets whether the Z axis is finite.
        //-----------------------------------------------------------------------------
        public bool  finiteZAxis        { get { return mFiniteZAxis; } set { mFiniteZAxis = value; } }

        //-----------------------------------------------------------------------------
        // Name: zAxisLength (Public Property)
        // Desc: Returns or sets the length of the Z axis when 'finiteZAxis' is true.
        //-----------------------------------------------------------------------------
        public float zAxisLength        { get { return mZAxisLength; } set { mZAxisLength = Mathf.Max(value, 0.0f); } }
        #endregion

        #region Public Static Properties
        public static bool  defaultVisible          { get { return true; } }
        public static bool  defaultZoomFadeEnabled  { get { return true; } }
        public static Color defaultCellLineColor    { get { return ColorEx.FromRGBABytes(128, 128, 128, 102); } }
        public static Color defaultXAxisColor       { get { return Core.xAxisColor; } }
        public static Color defaultYAxisColor       { get { return Core.yAxisColor; } }
        public static Color defaultZAxisColor       { get { return Core.zAxisColor; } }
        public static bool  defaultDrawGridAxes     { get { return true; } }
        public static bool  defaultFiniteXAxis      { get { return false; } }
        public static float defaultXAxisLength      { get { return 10.0f; } }
        public static bool  defaultFiniteYAxis      { get { return true; } }
        public static float defaultYAxisLength      { get { return 10.0f; } }
        public static bool  defaultFiniteZAxis      { get { return false; } }
        public static float defaultZAxisLength      { get { return 10.0f; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: UseDefaults() (Public Function)
        // Desc: Use default settings.
        //-----------------------------------------------------------------------------
        public void UseDefaults()
        {
            visible         = defaultVisible;
            zoomFadeEnabled = defaultZoomFadeEnabled;
            cellLineColor   = defaultCellLineColor;
            xAxisColor      = defaultXAxisColor;
            yAxisColor      = defaultYAxisColor;
            zAxisColor      = defaultZAxisColor;
            drawGridAxes    = defaultDrawGridAxes;
            finiteXAxis     = defaultFiniteXAxis;
            xAxisLength     = defaultXAxisLength;
            finiteYAxis     = defaultFiniteYAxis;
            yAxisLength     = defaultYAxisLength;
            finiteZAxis     = defaultFiniteZAxis;
            zAxisLength     = defaultZAxisLength;
        }
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: RTGrid (Public Singleton Class)
    // Desc: Implements the scene grid functionality.
    //-----------------------------------------------------------------------------
    public class RTGrid : MonoSingleton<RTGrid>
    {
        #region Private Static Fields   
        static Quaternion[] sPlaneRotationMap = new Quaternion[3]
        { Quaternion.AngleAxis(-90.0f, Vector3.right), Quaternion.AngleAxis(-90.0f, Vector3.forward), Quaternion.identity }; // Maps a member of the 'EPlane' enum to a grid rotation
        #endregion

        #region Private Fields
        MaterialPropertyBlock mMtrlPropertyBlock;       // Used for rendering

        [SerializeField] RTGridSettings mSettings   = new RTGridSettings();     // Grid settings
        [SerializeField] RTGridStyle    mStyle      = new RTGridStyle();        // Grid style
                        
        // Object filter used during picking
        ObjectFilter    mPickFilter         = new ObjectFilter { objectTypes = EGameObjectType.Mesh | EGameObjectType.Sprite | EGameObjectType.Terrain };

        // Buffers used for memory allocations
        Vector3[]       mBoxCornerBuffer    = new Vector3[8];
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: settings (Public Property)
        // Desc: Returns the grid settings which can be used to configure grid behavior.
        //-----------------------------------------------------------------------------
        public RTGridSettings   settings            { get { return mSettings; } }

        //-----------------------------------------------------------------------------
        // Name: style (Public Property)
        // Desc: Returns the style which can be used to configure grid style properties.
        //-----------------------------------------------------------------------------
        public RTGridStyle      style               { get { return mStyle; } }

        //-----------------------------------------------------------------------------
        // Name: origin (Public Property)
        // Desc: Returns the grid origin.
        //-----------------------------------------------------------------------------
        public Vector3          origin              { get { return Vector3.zero + up * mSettings.localYOffset; } }

        //-----------------------------------------------------------------------------
        // Name: right (Public Property)
        // Desc: Returns the grid's right axis.
        //-----------------------------------------------------------------------------
        public Vector3          right               { get { return rotation * Vector3.right; } }

        //-----------------------------------------------------------------------------
        // Name: up (Public Property)
        // Desc: Returns the grid's up axis.
        //-----------------------------------------------------------------------------
        public Vector3          up                  { get { return rotation * Vector3.up; } }

        //-----------------------------------------------------------------------------
        // Name: forward (Public Property)
        // Desc: Returns the grid's forward axis.
        //-----------------------------------------------------------------------------
        public Vector3          forward             { get { return rotation * Vector3.forward; } }

        //-----------------------------------------------------------------------------
        // Name: plane (Public Property)
        // Desc: Returns the grid's plane.
        //-----------------------------------------------------------------------------
        public Plane            plane               { get { return new Plane(up, origin); } }

        //-----------------------------------------------------------------------------
        // Name: rotation (Public Property)
        // Desc: Returns the grid's rotation.
        //-----------------------------------------------------------------------------
        public Quaternion       rotation            { get { return sPlaneRotationMap[(int)mSettings.plane]; } }

        //-----------------------------------------------------------------------------
        // Name: actionModeEnabled (Public Property)
        // Desc: Returns or sets whether action mode is enabled. When enabled, the grid
        //       will use the 'updateAction' property to decide what kind of action to
        //       execute during its next update.
        //-----------------------------------------------------------------------------
        public bool             actionModeEnabled   { get; set; } = false;

        //-----------------------------------------------------------------------------
        // Name: updateAction (Public Property)
        // Desc: Returns or sets the action to execute during the next update. This is
        //       used in conjunction with 'actionModeEnabled'. An action is executed if
        //       'actionModeEnabled' is true and 'updateAction' is anything other than
        //       'None'.
        //-----------------------------------------------------------------------------
        public EGridUpdateAction    updateAction    { get; set; } = EGridUpdateAction.None;

        //-----------------------------------------------------------------------------
        // Name: snapDesc (Public Property)
        // Desc: Returns the snap descriptor which can be used to snap to the grid.
        //-----------------------------------------------------------------------------
        public GridSnapDesc snapDesc
        {
            get
            {
                // Fill grid snap descriptor
                var desc        = new GridSnapDesc();
                desc.cellSize   = settings.cellSize;
                desc.right      = right;
                desc.up         = up;
                desc.forward    = forward;
                desc.origin     = origin;

                // Return descriptor
                return desc;
            }
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Internal_Update() (Public Function)
        // Desc: Called by the system to allow the grid to update itself.
        //-----------------------------------------------------------------------------
        public void Internal_Update()
        {
            // Are we ready to perform an action?
            if (actionModeEnabled)
            {
                // Check if we need to perform an action
                switch (updateAction)
                {
                    case EGridUpdateAction.SnapToPickPoint:

                        SnapToPickPoint();
                        break;

                    case EGridUpdateAction.SnapToPickPointExtents:

                        SnapToPickPointExtents();
                        break;
                }

                // Reset action
                updateAction  = EGridUpdateAction.None;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_Render() (Public Function)
        // Desc: Called by the system to allow the grid to render itself.
        // Parm: camera     - Render camera.
        //       rasterCtx  - Raster graphics context.
        //-----------------------------------------------------------------------------
        public void Internal_Render(Camera camera, RasterGraphContext rasterCtx)
        {
            // No-op?
            if (!style.visible)
                return;

            // Setup rendering material
            Material material = MaterialManager.rtGrid;
            material.SetColor("_CellLineColor", style.cellLineColor);
            material.SetVector("_GridOrigin", origin);
            material.SetVector("_GridRight", right);
            material.SetVector("_GridForward", forward);
            material.SetFloat("_CellSizeX", settings.cellSize.x);
            material.SetFloat("_CellSizeZ", settings.cellSize.z);
            material.SetFloat("_FadeZoom", MathEx.FastAbs(plane.GetDistanceToPoint(camera.transform.position)) / 4.0f);
            material.SetInt("_ZoomFadeEnabled", style.zoomFadeEnabled ? 1 : 0);
           
            // Calculate grid transform data
            Vector3 gridPos     = plane.ProjectPoint(camera.transform.position);
            Vector3 gridSize    = new Vector3(camera.farClipPlane * 2.0f, 1.0f, camera.farClipPlane * 2.0f);
      
            // Draw axes?
            if (style.drawGridAxes)
            {
                // Prepare property block
                if (mMtrlPropertyBlock == null) mMtrlPropertyBlock = new MaterialPropertyBlock();
                mMtrlPropertyBlock.Clear();

                // Draw the grid's local axes. Start with the X axis.
                mMtrlPropertyBlock.SetColor("_GridAxisColor", style.xAxisColor);
                Vector3 axisScale = new Vector3(style.finiteXAxis ? style.xAxisLength : gridSize.x, 1.0f, 1.0f);
                rasterCtx.cmd.DrawMesh(MeshManager.unitXSegment, Matrix4x4.TRS(origin, rotation, axisScale), material, 0, 0, mMtrlPropertyBlock);
                rasterCtx.cmd.DrawMesh(MeshManager.unitXSegment, Matrix4x4.TRS(origin, rotation * Quaternion.AngleAxis(180.0f, Vector3.up), axisScale), material, 0, 0, mMtrlPropertyBlock);

                // Draw Y axis
                mMtrlPropertyBlock.SetColor("_GridAxisColor", style.yAxisColor);
                axisScale = new Vector3(style.finiteYAxis ? style.yAxisLength : gridSize.x, 1.0f, 1.0f);
                rasterCtx.cmd.DrawMesh(MeshManager.unitXSegment, Matrix4x4.TRS(origin, rotation * Quaternion.AngleAxis( 90.0f, Vector3.forward), axisScale), material, 0, 0, mMtrlPropertyBlock);
                rasterCtx.cmd.DrawMesh(MeshManager.unitXSegment, Matrix4x4.TRS(origin, rotation * Quaternion.AngleAxis(-90.0f, Vector3.forward), axisScale), material, 0, 0, mMtrlPropertyBlock);
            
                // Draw Z axis
                mMtrlPropertyBlock.SetColor("_GridAxisColor", style.zAxisColor);
                axisScale = new Vector3(style.finiteZAxis ? style.zAxisLength : gridSize.z, 1.0f, 1.0f);
                rasterCtx.cmd.DrawMesh(MeshManager.unitXSegment, Matrix4x4.TRS(origin, rotation * Quaternion.AngleAxis( 90.0f, Vector3.up), axisScale), material, 0, 0, mMtrlPropertyBlock);
                rasterCtx.cmd.DrawMesh(MeshManager.unitXSegment, Matrix4x4.TRS(origin, rotation * Quaternion.AngleAxis(-90.0f, Vector3.up), axisScale), material, 0, 0, mMtrlPropertyBlock);
            }

            // Draw grid plane
            rasterCtx.cmd.DrawMesh(MeshManager.unitZXQuad, Matrix4x4.TRS(gridPos, rotation, gridSize), material, 0, 1);
        }

        //-----------------------------------------------------------------------------
        // Name: Raycast() (Public Function)
        // Desc: Checks if the specified ray intersects the grid.
        // Parm: ray    - Query ray.
        //       rayHit - Returns the ray hit info. Should be ignored if the function
        //                returns false.
        // Rtrn: True if the ray hits the grid and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Raycast(Ray ray, out RTGridRayHit rayHit)
        {
            // Clear output
            rayHit = new RTGridRayHit();

            // Raycast plane
            if (plane.Raycast(ray, out float t))
            {
                // We have a hit
                rayHit.grid   = this;
                rayHit.normal = plane.normal;
                rayHit.point  = ray.GetPoint(t);
                rayHit.t      = t;
                return true;
            }

            // No hit
            return false;
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: SnapToPickPoint() (Private Function)
        // Desc: Snaps the grid to the world point picked by the mouse cursor.
        //-----------------------------------------------------------------------------
        void SnapToPickPoint()
        {
            // Check UI interaction
            if (RTScene.get.IsUIPointerBlocked())
                return;

            // Raycast closest object
            Ray ray = RTCamera.get.GetPickRay();
            if (RTScene.get.Raycast(ray, mPickFilter, false, out SceneRayHit sceneHit) && sceneHit.hasObjectHit)
                SnapToPoint(sceneHit.objectHit.point);
        }

        //-----------------------------------------------------------------------------
        // Name: SnapToPoint() (Private Function)
        // Desc: Snaps the grid to the specified world point.
        //-----------------------------------------------------------------------------
        void SnapToPoint(Vector3 pt)
        {
            // Calculate the distance between the point and the grid plane
            float d = plane.GetDistanceToPoint(pt);

            // This distance is the amount we need to add to the local Y offset to reposition 
            // the grid plane in such a way that the point sits on the plane.
            settings.localYOffset += d;
        }
                
        //-----------------------------------------------------------------------------
        // Name: SnapToPickPointExtents() (Private Function)
        // Desc: Snaps the grid to the extents of the object under the mouse cursor.
        //-----------------------------------------------------------------------------
        void SnapToPickPointExtents()
        {
            // Check UI interaction
            if (RTScene.get.IsUIPointerBlocked())
                return;

            // Raycast closest object
            Ray ray = RTCamera.get.GetPickRay();
            if (RTScene.get.Raycast(ray, mPickFilter, false, out SceneRayHit sceneHit) && sceneHit.hasObjectHit)
            {
                // Cache data
                var objectHit = sceneHit.objectHit;

                // Calculate the hit object's OBB
                var boundsQConfig = BoundsQueryConfig.defaultConfig;
                boundsQConfig.objectTypes = mPickFilter.objectTypes;
                OBox obb = objectHit.gameObject.CalculateWorldOBB(boundsQConfig);
                if (!obb.isValid) return;

                // If the hit point is in front of the plane snap to the furthest OBB
                // corner in front of the plane. If the hit point is behind the plane,
                // snap to the furthest corner behind the plane.
                obb.CalculateCorners(mBoxCornerBuffer);
                if (plane.GetDistanceToPoint(objectHit.point) >= 0.0f)
                    SnapToPoint(mBoxCornerBuffer[plane.FindFurthestPointInFront(mBoxCornerBuffer)]);
                else SnapToPoint(mBoxCornerBuffer[plane.FindFurthestPointBehind(mBoxCornerBuffer)]);
            }
        }
        #endregion
    }
    #endregion
}