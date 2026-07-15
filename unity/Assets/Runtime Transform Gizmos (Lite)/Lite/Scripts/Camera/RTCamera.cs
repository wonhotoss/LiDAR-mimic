using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.RenderGraphModule;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: ECameraFocusMode (Public Enum)
    // Desc: Defines different camera focus modes.
    //-----------------------------------------------------------------------------
    public enum ECameraFocusMode
    {
        Instant = 0,    // Instant focus
        Smooth          // Smooth focus using exponential lerp
    }

    //-----------------------------------------------------------------------------
    // Name: ECameraProjectionSwitchMode (Public Enum)
    // Desc: Defines different modes in which a camera projection switch can be
    //       performed. A projection switch happens when the camera switches from
    //       perspective to ortho or vice versa.
    //-----------------------------------------------------------------------------
    public enum ECameraProjectionSwitchMode
    {
        Instant = 0,    // Instant switch
        Linear          // The switch happens as a linear transition in a specified time interval
    }

    //-----------------------------------------------------------------------------
    // Name: ECameraRotationSwitchMode (Public Enum)
    // Desc: Defines different modes in which a camera rotation switch can be
    //       performed. A rotation switch happens when the camera switches from
    //       one rotation to another.
    //-----------------------------------------------------------------------------
    public enum ECameraRotationSwitchMode
    {
        Instant = 0,    // Instant switch
        Smooth          // Smooth switch using exponential lerp
    }

    //-----------------------------------------------------------------------------
    // Name: ECameraRenderFlags (Public Flags Enum)
    // Desc: Defines different camera render flags which can be combined to control
    //       what a camera can render.
    //-----------------------------------------------------------------------------
    [Flags] public enum ECameraRenderFlags
    {
        None            = 0,    // Don't render any of the following ...
        SceneGrid       = 1,    // If set, the camera can render the scene grid
        CameraBg        = 2,    // If set, the camera can render the camera background
        Gizmos          = 4,    // If set, the camera can render gizmos

        All             = SceneGrid | CameraBg | Gizmos // Render all of the above ...
    }

    //-----------------------------------------------------------------------------
    // Name: ECameraMoveDirections (Public Flags Enum)
    // Desc: Defines different camera move directions which can be combined to
    //       control the direction in which the camera is moving.
    //-----------------------------------------------------------------------------
    [Flags] public enum ECameraMoveDirections
    {
        None        = 0,
        Left        = 1,
        Right       = 2,
        Down        = 4,
        Up          = 8,
        Backward    = 16,
        Forward     = 32
    }

    //-----------------------------------------------------------------------------
    // Name: ECameraNavigationMode (Public Enum)
    // Desc: Defines different camera navigation modes.
    //-----------------------------------------------------------------------------
    public enum ECameraNavigationMode
    {
        None = 0,   // No navigation
        Pan,        // Camera panning
        Orbit,      // Camera orbiting
        Fly,        // Camera flying
        Zoom,       // Camera zooming
    }
    #endregion

    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: RTCameraRenderConfig (Public Struct)
    // Desc: Stores camera render configuration data which can be used to configure
    //       the way in which cameras behave when rendering different kinds of entities.
    //-----------------------------------------------------------------------------
    public struct RTCameraRenderConfig
    {
        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: renderFlags (Public Property)
        // Desc: Returns or sets the render flags which control what the camera can render.
        //-----------------------------------------------------------------------------
        public ECameraRenderFlags renderFlags { get; set; }
        #endregion

        #region Public Static Readonly Fields
        public static readonly RTCameraRenderConfig defaultConfig = new RTCameraRenderConfig()    // Default render config
        {
            renderFlags     = ECameraRenderFlags.All,   // Render everything
        };
        #endregion
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTCameraSettings (Public Class)
    // Desc: Stores settings for the 'RTCamera' instance. 
    //-----------------------------------------------------------------------------
    [Serializable] public class RTCameraSettings
    {
        #region Private Fields
        [SerializeField] Camera mTargetCamera           = defaultTargetCamera;          // The Unity camera being controlled by the 'RTCamera' instance
        [SerializeField] bool   mUsePhysics             = defaultUsePhysics;            // Apply simple physics?
        [SerializeField] float  mAcceleration           = defaultAcceleration;          // Acceleration     (used only if 'mUsePhysics' is true)
        [SerializeField] float  mSpeedModifier          = defaultSpeedModifier;         // Speed modifier used when the speed modifier is active
        [SerializeField] float  mFriction               = defaultFriction;              // Friction         (used only if 'mUsePhysics' is true)
        [SerializeField] float  mMaxMoveSpeed           = defaultMaxMoveSpeed;          // Maximum speed    (used only if 'mUsePhysics' is true)
        [SerializeField] float  mPanSensitivity         = defaultPanSensitivity;        // Mouse sensitivity when panning the camera
        [SerializeField] float  mRotationSensitivity    = defaultRotationSensitivity;   // Mouse sensitivity when rotating the camera
        [SerializeField] float  mOrbitSnesitivity       = defaultOrbitSensitivity;      // Mouse sensitivity when orbiting the camera
        [SerializeField] float  mMoveSpeed              = defaultMoveSpeed;             // Camera move speed (used only if 'mUsePhysics' is false) 

        [SerializeField] ECameraFocusMode               mFocusMode                      = defaultFocusMode;                     // Focus mode used when the camera must focus on a 3D entity or a group of entities           
        [SerializeField] float                          mFocusSmoothSpeed               = defaultFocusSmoothSpeed;              // Focus smooth speed

        [SerializeField] ECameraProjectionSwitchMode    mProjectionSwitchMode           = defaultProjectionSwitchMode;          // Projection switch mode
        [SerializeField] float                          mProjectionTransitionDuration   = defaultProjectionTransitionDuration;  // The time in seconds in takes to complete a projection transition. Used when the switch mode is set to 'Linear'.

        [SerializeField] ECameraRotationSwitchMode      mRotationSwitchMode             = defaultRotationSwitchMode;            // Rotation switch mode
        [SerializeField] float                          mRotationSwitchSmoothSpeed      = defaultRotationSwitchSmoothSpeed;     // Rotation switch smooth speed
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: targetCamera (Public Property)
        // Desc: Returns or sets the target camera. This is the camera whose transform
        //       is manipulated by the 'RTCamera' instance.
        //-----------------------------------------------------------------------------
        public Camera   targetCamera        
        { 
            get { return mTargetCamera; }
            set
            {
                if (value != null && !value.gameObject.IsSceneObject()) return;
                mTargetCamera = value;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: usePhysics (Public Property)
        // Desc: Returns or sets whether simple physics should be used when moving the
        //       camera. When true, the camera will accelerate and decelerate when flying
        //       around.
        //-----------------------------------------------------------------------------
        public bool     usePhysics          { get { return mUsePhysics; } set { mUsePhysics = value; } }

        //-----------------------------------------------------------------------------
        // Name: acceleration (Public Property)
        // Desc: Returns or sets the acceleration value used to update the camera speed.
        //       This is only used when 'usePhysics' is true.
        //-----------------------------------------------------------------------------
        public float    acceleration        { get { return mAcceleration; } set { mAcceleration = Mathf.Max(1.0f, value); } }

        //-----------------------------------------------------------------------------
        // Name: speedModifier (Public Property)
        // Desc: Speed modifier value used to scale the camera speed when the speed modifier
        //       is active.
        //-----------------------------------------------------------------------------
        public float    speedModifier       { get { return mSpeedModifier; } set { mSpeedModifier = Mathf.Max(0.0f, value); } }

        //-----------------------------------------------------------------------------
        // Name: friction (Public Property)
        // Desc: Returns or sets the friction value used to slow down the camera when not
        //       instructed to move. This is only used when 'usePhysics' is true.
        //-----------------------------------------------------------------------------
        public float    friction            { get { return mFriction; } set { mFriction = Mathf.Max(1.0f, value); } }

        //-----------------------------------------------------------------------------
        // Name: maxMoveSpeed (Public Property)
        // Desc: Returns or sets the maximum allowed speed. This is only used when
        //       'usePhysics' is true.
        //-----------------------------------------------------------------------------
        public float    maxMoveSpeed        { get { return mMaxMoveSpeed; } set { mMaxMoveSpeed = Mathf.Max(1.0f, value); if (mMoveSpeed > mMaxMoveSpeed) mMoveSpeed = mMaxMoveSpeed; } }
        
        //-----------------------------------------------------------------------------
        // Name: panSensitivity (Public Property)
        // Desc: Returns or sets how sensitive is the camera to mouse movements when
        //       panning.
        //-----------------------------------------------------------------------------
        public float    panSensitivity      { get { return mPanSensitivity; } set { mPanSensitivity = Mathf.Clamp01(value); } }

        //-----------------------------------------------------------------------------
        // Name: rotationSensitivity (Public Property)
        // Desc: Returns or sets how sensitive is the camera to mouse movements when
        //       rotating to look around.
        //-----------------------------------------------------------------------------
        public float    rotationSensitivity { get { return mRotationSensitivity; } set { mRotationSensitivity = Mathf.Clamp01(value); } }
        
        //-----------------------------------------------------------------------------
        // Name: orbitSensitivity (Public Property)
        // Desc: Returns or sets how sensitive is the camera to mouse movements when
        //       orbiting.
        //-----------------------------------------------------------------------------
        public float    orbitSensitivity    { get { return mOrbitSnesitivity; } set { mOrbitSnesitivity = Mathf.Clamp01(value); } }

        //-----------------------------------------------------------------------------
        // Name: moveSpeed (Public Property)
        // Desc: Returns or sets the movement speed in units/second. This is only used
        //       when 'usePhysics' is false.
        //-----------------------------------------------------------------------------
        public float    moveSpeed           { get { return mMoveSpeed; } set { mMoveSpeed = Mathf.Clamp(value, 1.0f, mMaxMoveSpeed); } }

        //-----------------------------------------------------------------------------
        // Name: focusMode (Public Property)
        // Desc: Returns or sets the camera focus mode.
        //-----------------------------------------------------------------------------
        public ECameraFocusMode focusMode           { get { return mFocusMode; } set { mFocusMode = value; } }

        //-----------------------------------------------------------------------------
        // Name: focusSmoothSpeed (Public Property)
        // Desc: Returns or sets the focus smooth speed.
        //-----------------------------------------------------------------------------
        public float    focusSmoothSpeed    { get { return mFocusSmoothSpeed; } set { mFocusSmoothSpeed = Mathf.Max(value, 1e-1f); } }

        //-----------------------------------------------------------------------------
        // Name: projectionSwitchMode (Public Property)
        // Desc: Returns or sets the camera projection switch mode.
        //-----------------------------------------------------------------------------
        public ECameraProjectionSwitchMode  projectionSwitchMode    { get { return mProjectionSwitchMode; } set { mProjectionSwitchMode = value; } }

        //-----------------------------------------------------------------------------
        // Name: projectionTransitionDuration (Public Property)
        // Desc: Returns or sets the projection transition duration.
        //-----------------------------------------------------------------------------
        public float    projectionTransitionDuration    { get { return mProjectionTransitionDuration; } set { mProjectionTransitionDuration = Math.Max(0.01f, value); } }
        
        //-----------------------------------------------------------------------------
        // Name: rotationSwitchMode (Public Property)
        // Desc: Returns or sets the camera rotation switch mode.
        //-----------------------------------------------------------------------------
        public ECameraRotationSwitchMode    rotationSwitchMode      { get { return mRotationSwitchMode; } set { mRotationSwitchMode = value; } }
        
        //-----------------------------------------------------------------------------
        // Name: rotationSwitchSmoothSpeed (Public Property)
        // Desc: Returns or sets the rotation switch smooth speed.
        //-----------------------------------------------------------------------------
        public float    rotationSwitchSmoothSpeed       { get { return mRotationSwitchSmoothSpeed; } set { mRotationSwitchSmoothSpeed = Mathf.Max(value, 1e-1f); } }
        #endregion

        #region Public Static Properties
        public static Camera    defaultTargetCamera             { get { return null; } }
        public static bool      defaultUsePhysics               { get { return true; } }
        public static float     defaultAcceleration             { get { return 180.0f; } }
        public static float     defaultFriction                 { get { return 120.0f; } }
        public static float     defaultMaxMoveSpeed             { get { return 60.0f; } }
        public static float     defaultPanSensitivity           { get { return 0.05f; } }
        public static float     defaultRotationSensitivity      { get { return 0.5f; } }
        public static float     defaultOrbitSensitivity         { get { return 0.5f; } }
        public static float     defaultMoveSpeed                { get { return 60.0f; } }
        public static float     defaultSpeedModifier            { get { return 2.0f; }}

        public static ECameraFocusMode              defaultFocusMode                    { get { return ECameraFocusMode.Smooth; } }
        public static float                         defaultFocusSmoothSpeed             { get { return 15.0f; } }

        public static ECameraProjectionSwitchMode   defaultProjectionSwitchMode         { get { return ECameraProjectionSwitchMode.Linear; } }
        public static float                         defaultProjectionTransitionDuration { get { return 0.23f; } }

        public static ECameraRotationSwitchMode     defaultRotationSwitchMode           { get { return ECameraRotationSwitchMode.Smooth; } }
        public static float                         defaultRotationSwitchSmoothSpeed    { get { return 15.0f; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: UseDefaults() (Public Function)
        // Desc: Use default settings.
        //-----------------------------------------------------------------------------
        public void UseDefaults()
        {
            usePhysics                      = defaultUsePhysics;
            acceleration                    = defaultAcceleration;
            speedModifier                   = defaultSpeedModifier;
            friction                        = defaultFriction;
            maxMoveSpeed                    = defaultMaxMoveSpeed;
            panSensitivity                  = defaultPanSensitivity; 
            rotationSensitivity             = defaultRotationSensitivity;
            orbitSensitivity                = defaultOrbitSensitivity;
            moveSpeed                       = defaultMoveSpeed;
            focusMode                       = defaultFocusMode;
            focusSmoothSpeed                = defaultFocusSmoothSpeed;
            projectionSwitchMode            = defaultProjectionSwitchMode;
            projectionTransitionDuration    = defaultProjectionTransitionDuration;
            rotationSwitchMode              = defaultRotationSwitchMode;
            rotationSwitchSmoothSpeed       = defaultRotationSwitchSmoothSpeed;
        }
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: RTCameraBgStyle (Public Class)
    // Desc: Stores camera background style settings. 
    //-----------------------------------------------------------------------------
    [Serializable] public class RTCameraBgStyle
    {
        #region Private Fields
        [SerializeField] bool   mVisible            = defaultVisible;           // Is the camera background visible?
        [SerializeField] Color  mTopColor           = defaultTopColor;          // The top color
        [SerializeField] Color  mBottomColor        = defaultBottomColor;       // The bottom color
        [SerializeField] float  mGradientOffset     = defaultGradientOffset;    // Gradient offset
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: visible (Public Property)
        // Desc: Returns or sets the visibility state of the camera background. Use this
        //       to toggle the background's visibility.
        //-----------------------------------------------------------------------------
        public bool     visible         { get { return mVisible; } set { mVisible = value; } }

        //-----------------------------------------------------------------------------
        // Name: topColor (Public Property)
        // Desc: Returns or sets the background's top gradient color.
        //-----------------------------------------------------------------------------
        public Color    topColor        { get { return mTopColor; } set { mTopColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: bottomColor (Public Property)
        // Desc: Returns or sets the background's bottom gradient color.
        //-----------------------------------------------------------------------------
        public Color    bottomColor     { get { return mBottomColor; } set { mBottomColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: gradientOffset (Public Property)
        // Desc: Returns or sets the gradient offset. Positive values move the bottom
        //       color closer to the top. Negative values move the top color closer to
        //       the bottom.
        //-----------------------------------------------------------------------------
        public float    gradientOffset  { get { return mGradientOffset; } set { mGradientOffset = Mathf.Clamp(value, -1.0f, 1.0f); } }
        #endregion

        #region Public Static Properties
        public static bool  defaultVisible          { get { return true; } }
        public static Color defaultTopColor         { get { return ColorEx.FromRGBBytes(35, 35, 35); } }
        public static Color defaultBottomColor      { get { return ColorEx.FromRGBBytes(10, 10, 10); } }
        public static float defaultGradientOffset   { get { return 0.0f; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: UseDefaults() (Public Function)
        // Desc: Use default settings.
        //-----------------------------------------------------------------------------
        public void UseDefaults()
        {
            visible       = defaultVisible;
            topColor        = defaultTopColor;
            bottomColor     = defaultBottomColor;
            gradientOffset  = defaultGradientOffset;
        }
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: RTCamera (Public Singleton Class)
    // Desc: Implements the scene navigation camera.        
    //-----------------------------------------------------------------------------
    public class RTCamera : MonoSingleton<RTCamera>
    {
        #region Private Classes
        //-----------------------------------------------------------------------------
        // Name: CameraNavState (Private Class)
        // Desc: Stores camera navigation state for a Unity camera.
        //-----------------------------------------------------------------------------
        class CameraNavState
        {
            #region Public Fields
            public float zoom = 5.0f;  // Current camera zoom
            #endregion
        }
        #endregion

        #region Private Static Readonly Fields
        static readonly float sMinNearPlane = 0.01f;    // Minimum valid near plane for a perspective camera
        #endregion

        #region Private Fields
        [SerializeField] RTCameraSettings   mSettings   = new RTCameraSettings();   // Camera settings
        [SerializeField] RTCameraBgStyle    mBgStyle    = new RTCameraBgStyle();    // Background style

        // Maps a camera to a render config. Used to configure rendering for different cameras in the scene.
        Dictionary<int, RTCameraRenderConfig>       mRenderConfigMap    = new Dictionary<int, RTCameraRenderConfig>();

        // Maps a camera instance Id to its navigation state
        Dictionary<int, CameraNavState>             mNavStateMap        = new Dictionary<int, CameraNavState>();
        CameraNavState                              mNavState           = null;     // Navigation state for the currently active camera

        Transform   mTransform      = null;         // Cached camera transform. This is the transform of the target camera, not the transform of the object the script is attached to.
        Vector3     mVelocity       = Vector3.zero; // Current velocity (only if physics is enabled)
        float       mFOV;                           // The camera's 'fieldOfView' property is used for projection switches so we need to keep track of the original value

        CameraFocusData         mFocusData;         // Focus data used when focusing the camera
        Coroutine               mFocusCrtn;         // Focus coroutine
        CameraProjectionSwitch  mProjectionSwitch
                    = new CameraProjectionSwitch(); // Projection switch used to perform projection switch transitions
        CameraRotationSwitch    mRotationSwitch
                    = new CameraRotationSwitch();   // Rotation switch used to perform rotation switch transitions
        Camera                  mOldTargetCamera;   // Used to detect when the target camera changes

        bool                    mUseFocusRotation;  // Does the current focus operation also animate rotation?
        Quaternion              mFocusRotation;     // Target rotation used by the current focus operation

        // Buffers used to avoid memory allocations
        List<GameObject>    mParentBuffer       = new List<GameObject>();
        List<GameObject>    mObjectBuffer       = new List<GameObject>();
        Vector3[]           mQuadCornerBuffer   = new Vector3[4];
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: settings (Public Property)
        // Desc: Returns the camera settings which can be used to configure camera behavior.
        //-----------------------------------------------------------------------------
        public RTCameraSettings         settings        { get { return mSettings; } }       
        
        //-----------------------------------------------------------------------------
        // Name: bgStyle (Public Property)
        // Desc: Returns the camera background style which can be used to configure background
        //       style properties.
        //-----------------------------------------------------------------------------
        public RTCameraBgStyle          bgStyle         { get { return mBgStyle; } }   

        //-----------------------------------------------------------------------------
        // Name: zoomPoint (Public Property)
        // Desc: Returns the camera zoom point. This is the point the camera zooms in or
        //       out from.
        //-----------------------------------------------------------------------------
        public Vector3                  zoomPoint       { get { return mTransform.position + mTransform.forward * (mNavState != null ? mNavState.zoom : 0.0f); } }

        //-----------------------------------------------------------------------------
        // Name: navigationMode (Public Property)
        // Desc: Returns or sets the camera navigation mode. Based on what value is
        //       set, the camera is allowed to fly, rotate, pan, orbit etc.
        //-----------------------------------------------------------------------------
        public ECameraNavigationMode    navigationMode  { get; set; } = ECameraNavigationMode.None; 
        
        //-----------------------------------------------------------------------------
        // Name: moveDirections (Public Property)
        // Desc: Returns or sets the camera move direction flags. When 'navigationMode'
        //       is set to 'Fly', the move direction flags control the direction in which
        //       the camera is flying/moving.
        //-----------------------------------------------------------------------------
        public ECameraMoveDirections    moveDirections  { get; set; } = ECameraMoveDirections.None;

        //-----------------------------------------------------------------------------
        // Name: fov (Public Property)
        // Desc: Returns the camera field of view. Use this instead of the camera's
        //       'fieldOfView' property.
        //-----------------------------------------------------------------------------
        public float fov { get { return mFOV; } }

        //-----------------------------------------------------------------------------
        // Name: isFocusing (Public Property)
        // Desc: Returns whether or not the camera is currently engaged in a focus operation.
        //-----------------------------------------------------------------------------
        public bool isFocusing  { get { return mFocusCrtn != null; } }

        //-----------------------------------------------------------------------------
        // Name: isSwitchingProjection (Public Property)
        // Desc: Returns whether or not the camera is currently engaged in a projection
        //       switch.
        //-----------------------------------------------------------------------------
        public bool isSwitchingProjection       { get { return mProjectionSwitch.active; } }

        //-----------------------------------------------------------------------------
        // Name: isSwitchingToPerspective (Public Property)
        // Desc: Returns whether or not the camera is currently switching to perspective
        //       mode.
        //-----------------------------------------------------------------------------
        public bool isSwitchingToPerspective    { get { return mProjectionSwitch.active && !mProjectionSwitch.toOrtho; } }

        //-----------------------------------------------------------------------------
        // Name: isSwitchingToOrtho (Public Property)
        // Desc: Returns whether or not the camera is currently switching to orthographic
        //       mode.
        //-----------------------------------------------------------------------------
        public bool isSwitchingToOrtho          { get { return mProjectionSwitch.active && mProjectionSwitch.toOrtho; } }

        //-----------------------------------------------------------------------------
        // Name: projectionSwitchProgress (Public Property)
        // Desc: Returns the projection switch progress.
        //-----------------------------------------------------------------------------
        public float projectionSwitchProgress   { get { return mProjectionSwitch.progress; } }

        //-----------------------------------------------------------------------------
        // Name: isSwitchingRotation (Public Property)
        // Desc: Returns whether or not the camera is currently engaged in a rotation
        //       switch.
        //-----------------------------------------------------------------------------
        public bool isSwitchingRotation     { get { return mRotationSwitch.active; } }

        //-----------------------------------------------------------------------------
        // Name: canFocus (Public Property)
        // Desc: Returns whether or not the camera is allowed to focus.
        //-----------------------------------------------------------------------------
        public bool canFocus                { get { return !isFocusing && !isSwitchingProjection && !isSwitchingRotation; } }

        //-----------------------------------------------------------------------------
        // Name: canSwitchProjection (Public Property)
        // Desc: Returns whether or not the camera is allowed to do a projection switch.
        //-----------------------------------------------------------------------------
        public bool canSwitchProjection     { get { return !isFocusing && navigationMode == ECameraNavigationMode.None && !isSwitchingRotation; } }

        //-----------------------------------------------------------------------------
        // Name: canSwitchRotation (Public Property)
        // Desc: Returns whether or not the camera is allowed to do a rotation switch.
        //-----------------------------------------------------------------------------
        public bool canSwitchRotation       { get { return !isFocusing && navigationMode == ECameraNavigationMode.None && !isSwitchingProjection && !isSwitchingRotation; } }

        //-----------------------------------------------------------------------------
        // Name: canNavigate (Public Property)
        // Desc: Returns whether or not the camera is allowed to navigate the scene.
        //-----------------------------------------------------------------------------
        public bool canNavigate             { get { return !isFocusing && !isSwitchingProjection && !isSwitchingRotation; } }

        //-----------------------------------------------------------------------------
        // Name: canChangeFOV (Public Property)
        // Desc: Returns whether or not the camera is allowed to change its FOV.
        //-----------------------------------------------------------------------------
        public bool canChangeFOV            { get { return !isSwitchingProjection && !isSwitchingRotation; } }

        //-----------------------------------------------------------------------------
        // Name: speedModifierEnabled (Public Property)
        // Desc: Returns or sets whether the speed modifier should be applied.
        //-----------------------------------------------------------------------------
        public bool speedModifierEnabled    { get; set; } = false;
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Internal_Init() (Public Function)
        // Desc: Called by the system to allow the camera to initialize itself.
        //-----------------------------------------------------------------------------
        public void Internal_Init()
        {
            // Do we have a target camera?
            if (mSettings.targetCamera == null)
            {
                // If not, use he main camera. If none exists, show error and stop the app.
                mSettings.targetCamera = Camera.main;
                if (mSettings.targetCamera == null)
                {
                    Debug.LogError("RTCamera: Target camera was null and no Main camera exists in the scene.");
                    Debug.DebugBreak();
                }
            }

            // Create navigation state for the current camera
            mNavState = new CameraNavState();
            mNavStateMap.Add(mSettings.targetCamera.GetInstanceID(), mNavState);

            // Init data
            mOldTargetCamera    = mSettings.targetCamera;
            mTransform          = mSettings.targetCamera.transform;
            mFOV                = settings.targetCamera.fieldOfView;

            // Init ortho size
            UpdateOrthoSize();
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_Update() (Public Function)
        // Desc: Called by the system to allow the camera to update itself.
        //-----------------------------------------------------------------------------
        public void Internal_Update()
        {
            // Validate call
            if (mSettings.targetCamera == null || !canNavigate) 
                return;

            // Has the target camera changed?
            if (mOldTargetCamera != settings.targetCamera)
            {
                // Create a new camera navigation state if necessary
                int camId = settings.targetCamera.GetInstanceID();
                if (!mNavStateMap.TryGetValue(camId, out mNavState))
                {
                    mNavState = new CameraNavState();
                    mNavStateMap.Add(camId, mNavState);
                }

                // Update camera data
                mOldTargetCamera    = settings.targetCamera;
                mFOV                = settings.targetCamera.fieldOfView;
                mTransform          = mSettings.targetCamera.transform;
             
                // Update camera details
                UpdateOrthoSize();
                UpdateNearPlane();
            }
  
            // Let's always keep the near plane updated
            UpdateNearPlane();

            // Navigate
            Navigate();
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_Render() (Public Function)
        // Desc: Called by the system to allow the camera to perform any necessary 
        //       rendering (e.g. camera background).
        // Parm: camera    - Render camera.
        //       rasterCtx - Raster graph context.
        //-----------------------------------------------------------------------------
        public void Internal_Render(Camera camera, RasterGraphContext rasterCtx)
        {
            // No-op?
            if (!bgStyle.visible) return;

            // Can't render background for this camera?
	        GetCameraRenderConfig(camera, out RTCameraRenderConfig renderConfig);
	        if ((renderConfig.renderFlags & ECameraRenderFlags.CameraBg) == 0)
		        return;

            // Cache render camera transform
            Transform   camTransform    = camera.transform;

            // Calculate the camera far plane height
            float tan               = Mathf.Tan(Mathf.Deg2Rad * camera.fieldOfView * 0.5f);
            float farPlaneHeight    = tan * camera.farClipPlane * 2.0f;

            // Setup background material
            Material material = MaterialManager.rtCameraBg;
            material.SetColor("_FirstColor", bgStyle.topColor);
            material.SetColor("_SecondColor", bgStyle.bottomColor);
            material.SetFloat("_FarPlaneHeight", farPlaneHeight);
            material.SetFloat("_GradientOffset", bgStyle.gradientOffset);

            // Calculate the camera far plane world matrix. We will draw the background as a quad sitting
            // on top of the camera far plane.
            // Note: We scale the camera's far plane by 0.988 to avoid rendering artifacts which can
            //       occur due to floating point rounding errors pushing the plane mesh outside of the frustum.
            // Note: We also add a small value to the frustum height (and implicitly width) in order to
            //       ensure that it really does cover the whole far plane area.
            Matrix4x4 worldMtx      = Matrix4x4.TRS(camTransform.position + camTransform.forward * camera.farClipPlane * 0.988f, 
                camTransform.rotation, new Vector3((farPlaneHeight + 0.01f) * 
                camera.aspect, (farPlaneHeight + 0.01f), 1.0f));

            // Draw background
            rasterCtx.cmd.DrawMesh(MeshManager.unitXYQuad, worldMtx, material);
        }

        //-----------------------------------------------------------------------------
        // Name: GetPickRay() (Public Function)
        // Desc: Returns a ray from the current mouse cursor position.
        // Rtrn: A ray from the current mouse cursor position. This ray can be used for
        //       picking tasks (i.e. raycasting objects in the scene etc).
        //-----------------------------------------------------------------------------
        public Ray GetPickRay()
        {
            return RTInput.get.pointingInputDevice.GetPickRay(settings.targetCamera);
        }

        //-----------------------------------------------------------------------------
        // Name: GetCameraRenderConfig() (Public Function)
        // Desc: Returns a render config for the specified camera. If no entry exists 
        //       for the camera, one will be added by default and returned to the caller.
        // Parm: camera         - The camera whose render config must be returned.
        //       renderConfig   - Returns the render config for the specified camera.
        //-----------------------------------------------------------------------------
        public void GetCameraRenderConfig(Camera camera, out RTCameraRenderConfig renderConfig)
        {
            // Try getting the render config from the map
            int camId = camera.GetInstanceID();
            if (!mRenderConfigMap.TryGetValue(camId, out renderConfig))
            {
                // Couldn't find an entry for this camera. Add a default config.
                mRenderConfigMap.Add(camId, RTCameraRenderConfig.defaultConfig);
                renderConfig = RTCameraRenderConfig.defaultConfig;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: SetCameraRenderConfig() (Public Function)
        // Desc: Sets the render config for the specified camera.
        // Parm: camera         - The camera whose render config must be set.
        //       renderConfig   - The new render config for the specified camera.
        //-----------------------------------------------------------------------------
        public void SetCameraRenderConfig(Camera camera, RTCameraRenderConfig renderConfig)
        {
            // Add a record one doesn't exist for this camera, or simply update
            int camId = camera.GetInstanceID();
            if (!mRenderConfigMap.TryAdd(camId, renderConfig))       
                mRenderConfigMap[camId] = renderConfig;
        }

        //-----------------------------------------------------------------------------
        // Name: Focus() (Public Function)
        // Desc: Focuses the camera on the specified game object. The current focus
        //       settings are used to control the focus behaviour.
        // Parm: gameObject - The object the camera must focus on.
        //-----------------------------------------------------------------------------
        public void Focus(GameObject gameObject)
        {
            // Validate args
            if (gameObject == null)
                return;

            // No-op?
            if (!canFocus)
                return;

            // Call override with object list
            mObjectBuffer.Clear();
            mObjectBuffer.Add(gameObject);
            Focus(mObjectBuffer);
        }

        //-----------------------------------------------------------------------------
        // Name: Focus() (Public Function)
        // Desc: Focuses the camera on the specified game object and rotates the camera
        //       to the specified rotation. The current focus settings are used to
        //       control the focus behaviour.
        // Parm: gameObject    - The object the camera must focus on.
        //       focusRotation - The rotation the camera will have when focus completes.
        //-----------------------------------------------------------------------------
        public void Focus(GameObject gameObject, Quaternion focusRotation)
        {
            // Validate args
            if (gameObject == null)
                return;

            // No-op?
            if (!canFocus)
                return;

            // Call override with object list
            mObjectBuffer.Clear();
            mObjectBuffer.Add(gameObject);
            Focus(mObjectBuffer, focusRotation);
        }

        //-----------------------------------------------------------------------------
        // Name: Focus() (Public Function)
        // Desc: Focuses the camera on the specified collection of objects. The current
        //       focus settings are used to control the focus behaviour.
        // Parm: gameObjects - The list of objects the camera will focus on. If this
        //                     list is null or empty, the function returns immediately.
        //-----------------------------------------------------------------------------
        public void Focus(List<GameObject> gameObjects)
        {
            // Validate args
            if (gameObjects == null || gameObjects.Count == 0)
                return;

            // No-op?
            if (!canFocus)
                return;

            // We will first build an AABB that surrounds all the objects stored in the object list
            var boundsConfig = BoundsQueryConfig.defaultConfig;
            boundsConfig.objectTypes = EGameObjectType.All;
            Box aabb = GameObjectEx.CalculateObjectsWorldAABB(gameObjects, boundsConfig);

            // If the AABB is invalid, it means that we're dealing with objects that
            // don't have a volume. In this case, we will create an AABB that surrounds
            // their positions.
            if (!aabb.isValid)
            {
                // Init min and max
                aabb.max = Vector3Ex.FromValue(float.MinValue);
                aabb.min = Vector3Ex.FromValue(float.MaxValue);

                // Loop through each object
                int objectCount = gameObjects.Count;
                for (int i = 0; i < objectCount; ++i)
                {
                    // Get object position
                    Vector3 pos = gameObjects[i].transform.position;

                    // Update AABB
                    aabb.min = Vector3.Min(aabb.min, pos);
                    aabb.max = Vector3.Max(aabb.max, pos);
                }
            }

            // Focus the camera on the calculated AABB
            Focus(aabb);
        }

        //-----------------------------------------------------------------------------
        // Name: Focus() (Public Function)
        // Desc: Focuses the camera on the specified collection of objects and rotates
        //       the camera to the specified rotation. The current focus settings are
        //       used to control the focus behaviour.
        // Parm: gameObjects   - The list of objects the camera will focus on. If this
        //                       list is null or empty, the function returns immediately.
        //       focusRotation - The rotation the camera will have when focus completes.
        //-----------------------------------------------------------------------------
        public void Focus(List<GameObject> gameObjects, Quaternion focusRotation)
        {
            // Validate args
            if (gameObjects == null || gameObjects.Count == 0)
                return;

            // No-op?
            if (!canFocus)
                return;

            // We will first build an AABB that surrounds all the objects stored in the object list
            var boundsConfig = BoundsQueryConfig.defaultConfig;
            boundsConfig.objectTypes = EGameObjectType.All;
            Box aabb = GameObjectEx.CalculateObjectsWorldAABB(gameObjects, boundsConfig);

            // If the AABB is invalid, it means that we're dealing with objects that
            // don't have a volume. In this case, we will create an AABB that surrounds
            // their positions.
            if (!aabb.isValid)
            {
                // Init min and max
                aabb.max = Vector3Ex.FromValue(float.MinValue);
                aabb.min = Vector3Ex.FromValue(float.MaxValue);

                // Loop through each object
                int objectCount = gameObjects.Count;
                for (int i = 0; i < objectCount; ++i)
                {
                    // Get object position
                    Vector3 pos = gameObjects[i].transform.position;

                    // Update AABB
                    aabb.min = Vector3.Min(aabb.min, pos);
                    aabb.max = Vector3.Max(aabb.max, pos);
                }
            }

            // Focus the camera on the calculated AABB
            Focus(aabb, focusRotation);
        }

        //-----------------------------------------------------------------------------
        // Name: Focus() (Public Function)
        // Desc: Focuses the camera on the specified AABB. The current focus settings
        //       are used to control the focus behaviour.
        // Parm: aabb - The AABB the camera will focus on.
        //-----------------------------------------------------------------------------
        public void Focus(Box aabb)
        {
            // No-op?
            if (!canFocus)
                return;

            // Focus the camera on the sphere that encloses the specified AABB
            Focus(new Sphere(aabb));
        }

        //-----------------------------------------------------------------------------
        // Name: Focus() (Public Function)
        // Desc: Focuses the camera on the specified AABB and rotates the camera to the
        //       specified rotation. The current focus settings are used to control the
        //       focus behaviour.
        // Parm: aabb          - The AABB the camera will focus on.
        //       focusRotation - The rotation the camera will have when focus completes.
        //-----------------------------------------------------------------------------
        public void Focus(Box aabb, Quaternion focusRotation)
        {
            // No-op?
            if (!canFocus)
                return;

            // Focus the camera on the sphere that encloses the specified AABB
            Focus(new Sphere(aabb), focusRotation);
        }

        //-----------------------------------------------------------------------------
        // Name: Focus() (Public Function)
        // Desc: Focuses the camera on the specified sphere. The current focus settings
        //       are used to control the focus behaviour.
        // Parm: sphere - The sphere the camera will focus on.
        //-----------------------------------------------------------------------------
        public void Focus(Sphere sphere)
        {
            Focus(sphere, false, Quaternion.identity);
        }

        //-----------------------------------------------------------------------------
        // Name: Focus() (Public Function)
        // Desc: Focuses the camera on the specified sphere and rotates the camera to the
        //       specified rotation. The current focus settings are used to control the
        //       focus behaviour.
        // Parm: sphere        - The sphere the camera will focus on.
        //       focusRotation - The rotation the camera will have when focus completes.
        //-----------------------------------------------------------------------------
        public void Focus(Sphere sphere, Quaternion focusRotation)
        {
            Focus(sphere, true, focusRotation);
        }

        //-----------------------------------------------------------------------------
        // Name: SwitchProjection() (Public Function)
        // Desc: Performs a projection switch using the current projection switch settings.
        // Rtrn: True if the projection switch can be performed and false otherwise.
        //-----------------------------------------------------------------------------
        public bool SwitchProjection()
        {
            // Can we switch?
            if (!canSwitchProjection)
                return false;

            // Instant switch?
            if (settings.projectionSwitchMode == ECameraProjectionSwitchMode.Instant)
            {
                settings.targetCamera.orthographic = !settings.targetCamera.orthographic;
                UpdateOrthoSize();
                UpdateNearPlane();
            }
            else
            {
                // Start a projection switch transition.
                // Note: Use the 'mFOV' field to specify the camera field of view. The camera may already
                //       be involved in a switch so the 'fieldOfView' property can't be used here.
                mProjectionSwitch.Switch(settings.targetCamera, mFOV, mNavState.zoom, settings.projectionTransitionDuration);

                // If we're switching to a perspective camera, the near plane could be negative (it's allowed with
                // an ortho camera) so we need to fix this. We will just update the near plane to ensure it's proper.
                UpdateNearPlane();
            }

            // Success!
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: SwitchRotation() (Public Function)
        // Desc: Performs a rotation switch using the current rotation switch settings.
        // Parm: targetRotation - The rotation to switch to.
        // Rtrn: True if the rotation switch can be performed and false otherwise.
        //-----------------------------------------------------------------------------
        public bool SwitchRotation(Quaternion targetRotation)
        {
            // Can we switch?
            if (!canSwitchRotation)
                return false;

            // Instant switch?
            if (settings.rotationSwitchMode == ECameraRotationSwitchMode.Instant)
            {
                // Instant switch
                mRotationSwitch.InstantSwitch(settings.targetCamera, targetRotation, mNavState.zoom);
            }
            else
            {
                // Start a rotation switch transition
                mRotationSwitch.StartSwitch(settings.targetCamera, targetRotation, mNavState.zoom, settings.rotationSwitchSmoothSpeed);
            }

            // Success!
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: Zoom() (Public Function)
        // Desc: Zooms in or out based on the specified zoom value.
        // Parm: zoom       - The amount to zoom. Positive values zoom in, negative values
        //                    zoom out. 
        //       scrollZoom - Should be set to true when zooming in response to the mouse
        //                    scroll wheel.
        //-----------------------------------------------------------------------------
        public void Zoom(float zoom, bool scrollZoom)
        {
            // When 'scrollZoom' is true:
            //  1. The zoom speed increases as the distance from the zoom point increases.
            //  2. The zoom speed decreases as the distance from the zoom point decreases.
            float zoomAmount = scrollZoom ? (zoom * 2.5f) * mNavState.zoom : zoom;

            // If we're scroll-zooming and the zoom point offset is < 1, don't scale by the zoom point offset.
            // Otherwise, the camera zooms out really slow when it's really close to the zoom point.
            if (scrollZoom && mNavState.zoom < 1.0f)
                zoomAmount = zoom * 2.5f;

            // If we're zooming in, make sure we don't overshoot
            if (zoom > 0.0f)
            {
                if (zoomAmount > mNavState.zoom)
                    zoomAmount = mNavState.zoom - 1e-3f;
            }
            else
            // If we're zooming out, and we're very close to the zoom point, scale up a notch to move faster
            if (zoom < 0.0f)
            {
                if (zoomAmount > -1.0f && scrollZoom)
                    zoomAmount *= 2.89f;
            }

            // Zoom and update zoom point offset
            mTransform.position += mTransform.forward * zoomAmount;
            mNavState.zoom -= zoomAmount;

            // Update the ortho size
            UpdateOrthoSize();
        }

        //-----------------------------------------------------------------------------
        // Name: SetFieldOfView() (Public Function)
        // Desc: Call this in order to set the camera field of view. Do not use the
        //       camera's 'fieldOfView' property because the camera may be involved in
        //       a projection switch.
        // Parm: fov - Field of view.
        // Rtrn: True if the field of view can be changed and false otherwise.
        //-----------------------------------------------------------------------------
        public bool SetFieldOfView(float fov)
        {
            // Can we change the FOV?
            if (canChangeFOV)
                return false;

            // Set FOV and return success!
            mFOV = Mathf.Clamp(fov, 1e-5f, 179.0f);
            return true;
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: Navigate() (Private Function)
        // Desc: Implements camera navigation such as movement, rotation, orbit, physics
        //       etc.
        //-----------------------------------------------------------------------------
        void Navigate()
        {
            // Can we navigate?
            if (!canNavigate)
                return;

            // Store mouse deltas
            RTInput rtInput = RTInput.get;
            float mouseX    = rtInput.mouseDeltaX;
            float mouseY    = rtInput.mouseDeltaY;

            // Take speed multiplier into account
            float maxMoveSpeed  = mSettings.maxMoveSpeed;
            float speedScale    = 1.0f;
            if (speedModifierEnabled)
            {
                maxMoveSpeed    *= mSettings.speedModifier;
                speedScale      = mSettings.speedModifier;
            }

            // Are we allowed to fly?
            if (navigationMode == ECameraNavigationMode.Fly)
            {
                // Are we ortho?
                bool ortho = settings.targetCamera.orthographic;

                // Check the move direction flags to decide in which direction we're moving.
                // Note: If we're ortho, don't move forward or backward.
                Vector3 moveDirection = Vector3.zero;

                if ((moveDirections & ECameraMoveDirections.Forward) != 0 && !ortho)
                    moveDirection += mTransform.forward;
                else
                if ((moveDirections & ECameraMoveDirections.Backward) != 0 && !ortho)
                    moveDirection -= mTransform.forward;

                if ((moveDirections & ECameraMoveDirections.Left) != 0)
                    moveDirection -= mTransform.right;
                else
                if ((moveDirections & ECameraMoveDirections.Right) != 0)
                    moveDirection += mTransform.right;

                if ((moveDirections & ECameraMoveDirections.Down) != 0)
                    moveDirection -= mTransform.up;
                else
                if ((moveDirections & ECameraMoveDirections.Up) != 0)
                    moveDirection += mTransform.up;

                // Do we need to move the camera?
                if (moveDirection.magnitude > 0.0f)
                {
                    // If we are not using physics, update on the spot
                    if (!mSettings.usePhysics)
                        mTransform.position += moveDirection.normalized * mSettings.moveSpeed * Time.smoothDeltaTime * speedScale;
                    else
                    {
                        // Apply acceleration when we're using physics
                        mVelocity += moveDirection.normalized * mSettings.acceleration * Time.smoothDeltaTime * speedScale;
                    }
                }

                // Rotate the camera
                if (mouseY != 0.0f) mTransform.Rotate(mTransform.right, mSettings.rotationSensitivity * -mouseY, Space.World);
                if (mouseX != 0.0f) mTransform.Rotate(Vector3.up, mSettings.rotationSensitivity * mouseX, Space.World);
            }
            else
            if (navigationMode == ECameraNavigationMode.Pan)
            {
                // Pan camera
                if (mouseX != 0.0f) mTransform.position += -mouseX * mSettings.panSensitivity * mTransform.right;
                if (mouseY != 0.0f) mTransform.position += -mouseY * mSettings.panSensitivity * mTransform.up;
            }

            // Is physics enabled?
            float l = mVelocity.magnitude;
            if (mSettings.usePhysics && l > 1e-5f)
            {
                // Normalize velocity vector and clip velocity based on max speed
                Vector3 normVelocity = mVelocity.normalized;
                if (l >= maxMoveSpeed) { mVelocity = normVelocity * maxMoveSpeed; l = maxMoveSpeed; }

                // Move camera using the current velocity
                mTransform.transform.position += mVelocity * Time.smoothDeltaTime;

                // Apply friction
                l -= mSettings.friction * Time.smoothDeltaTime;             // Reduce velocity length using the friction value
                if (l <= 1e-5f) { l = 0.0f; mVelocity = Vector3.zero; }     // If length is small enough, kill velocity 
                else mVelocity = normVelocity * l;                          // Otherwise, update the velocity vector with the new length
            }
            else mVelocity = Vector3.zero;  // Make sure this is 0. If camera physics is disabled and then enabled again,
                                            // this can cause the camera to start moving by itself.

            // Orbit?
            if (navigationMode == ECameraNavigationMode.Orbit)
            {
                // Mouse X orbits around world X axis. Mouse Y orbits around camera local X.
                Vector3 zoomPt = zoomPoint;
                if (mouseX != 0.0f) mTransform.RotateAround(zoomPt, Vector3.up, mouseX * settings.orbitSensitivity);
                if (mouseY != 0.0f) mTransform.RotateAround(zoomPt, mTransform.right, -mouseY * settings.orbitSensitivity);
            }

            // Zoom using the mouse?
            if (navigationMode == ECameraNavigationMode.Zoom)
            {
                if (rtInput.mouseDeltaY != 0.0f)
                    Zoom(-rtInput.mouseDeltaY * 0.05f, false);
            }

            // When not navigating, we can zoom with the mouse scroll wheel
            if (navigationMode == ECameraNavigationMode.None)
            {
                // Have we scrolled?
                Vector2 scrollDelta = rtInput.scrollDelta;
                if (scrollDelta.y != 0.0f && !RTScene.get.IsUIPointerBlocked())
                    Zoom(scrollDelta.y, true);
            }
        }

        //-----------------------------------------------------------------------------
        // Name: Focus() (Private Function)
        // Desc: Focuses the camera on the specified sphere and optionally rotates the
        //       camera to the specified rotation.
        // Parm: sphere           - The sphere the camera will focus on.
        //       useFocusRotation - True if the camera should also rotate.
        //       focusRotation    - The rotation the camera will have when focus completes.
        //-----------------------------------------------------------------------------
        void Focus(Sphere sphere, bool useFocusRotation, Quaternion focusRotation)
        {
            // No-op?
            if (!canFocus)
                return;

            // Store rotation focus state
            mUseFocusRotation = useFocusRotation;
            mFocusRotation    = focusRotation;

            // Calculate focus data
            mFocusData = settings.targetCamera.CalculateFocusData(sphere);

            // If we are also rotating the camera, the final focus position has to be
            // calculated using the target rotation's forward axis.
            if (mUseFocusRotation)
                mFocusData.position = sphere.center - (mFocusRotation * Vector3.forward) * mFocusData.zoom;

            // We're now looking at a point which is 'zoom' units away from the camera.
            mNavState.zoom = mFocusData.zoom;

            // Instant focus?
            if (settings.focusMode == ECameraFocusMode.Instant)
            {
                if (mUseFocusRotation)
                    mTransform.rotation = mFocusRotation;

                mTransform.position = mFocusData.position;
                SetOrthoSize(mFocusData.orthoSize);
                UpdateNearPlane();
            }
            else
            {
                // Stop the old coroutine
                if (mFocusCrtn != null)
                    StopCoroutine(mFocusCrtn);

                // Start focus coroutine
                mFocusCrtn = StartCoroutine(Coroutine_Focus());
            }

            // Kill velocity
            mVelocity = Vector3.zero;
        }

        //-----------------------------------------------------------------------------
        // Name: UpdateNearPlane() (Private Function)
        // Desc: Updates the camera's near plane to ensure it doesn't cut through the
        //       scene grid.
        //-----------------------------------------------------------------------------
        void UpdateNearPlane()
        {
            // Cache data
            Camera camera       = settings.targetCamera;
            Vector3 nearMidPt   = mTransform.position + camera.nearClipPlane * mTransform.forward;

            // We have to calculate the near plane corner points and push the near
            // plane along the reverse of the camera look vector until the plane is
            // no longer spanning the grid plane. We only need to do this if the near
            // plane is cutting through the grid plane. We start by calculating the 
            // frustum width and height at a distance of 'nearClipPlane' units away
            // from the camera.
            float halfWidth;
            float halfHeight;
            if (camera.orthographic)
            {
                // Calculate width and height.Width has to be calculated using the aspect
                // ratio which can be > 1 (width > height) or < 1 (width < height).
                halfWidth   = camera.orthographicSize * camera.aspect;
                halfHeight  = camera.orthographicSize;
            }
            else
            {
                // Calculate tangent which tells us how many Y units correspond to one Z unit
                float tan   = Mathf.Tan(camera.fieldOfView * Mathf.Deg2Rad * 0.5f);

                // Calculate width and height. Width has to be calculated using the aspect
                // ratio which can be > 1 (width > height) or < 1 (width < height).
                halfWidth   = tan * camera.aspect * camera.nearClipPlane;
                halfHeight  = tan * camera.nearClipPlane;
            }

            // Calculate corner points
            // Note: We will store the points in the following order, seen from the
            //       perspective of the camera: bottom left, top left, top right,
            //       bottom right.
            mQuadCornerBuffer[0] = nearMidPt - mTransform.right * halfWidth - mTransform.up * halfHeight;
            mQuadCornerBuffer[1] = nearMidPt - mTransform.right * halfWidth + mTransform.up * halfHeight;
            mQuadCornerBuffer[2] = nearMidPt + mTransform.right * halfWidth + mTransform.up * halfHeight;
            mQuadCornerBuffer[3] = nearMidPt + mTransform.right * halfWidth - mTransform.up * halfHeight;

            // Check if the near plane quad is spanning the scene grid plane
            Plane gridPlane = RTGrid.get.plane;
            if (Polygon.VsPlane(mQuadCornerBuffer, gridPlane) == EPlaneClassify.Spanning)
            {
                // If the camera look vector runs parallel to the grid plane, we can't really do anything
                // except set the near plane to a really small value.
                // Note: Don't set to 0. It throws error messages when using 'ScreenPointToRay'.
                float dot   = Vector3.Dot(mTransform.forward, gridPlane.normal);
                float angle = MathEx.SafeAcos(dot) * Mathf.Rad2Deg;
                if (Mathf.Abs(angle - 90.0f) < 1e-3f) camera.nearClipPlane = sMinNearPlane;
                else
                {
                    // The camera is looking at the grid plane from a certain angle. Depending on which
                    // way it's looking we will find the furthest near plane corner point from the grid
                    // and cast a ray along the reverse of the camera's look vector. The t value of intersection
                    // is the distance we need to move the near plane back so that it no longer intersects
                    // the grid plane.
                    int ptIndex = -1;
                    if (dot < 0.0f) ptIndex = gridPlane.FindFurthestPointBehind(mQuadCornerBuffer);
                    else ptIndex = gridPlane.FindFurthestPointInFront(mQuadCornerBuffer);
                    
                    // Create the ray and intersect it with the grid plane
                    Ray ray = new Ray(mQuadCornerBuffer[ptIndex], -mTransform.forward);
                    if (gridPlane.Raycast(ray, out float t))
                    {
                        // Move the near plane back by a distance of 't'. 
                        // Note: We have to clip because if the camera is looking from a shallow angle, the calculated
                        //       distance could be very large and we could get in trouble.
                        if (t > 80.0f) t = 80.0f;
                        camera.nearClipPlane -= t;
                    }
                }
            }

            // Note: We also take care to ensure that the near clip plane is valid if a
            //       perspective camera is used.
            if (!settings.targetCamera.orthographic && settings.targetCamera.nearClipPlane < sMinNearPlane)
                settings.targetCamera.nearClipPlane = sMinNearPlane;
        }

        //-----------------------------------------------------------------------------
        // Name: UpdateOrthoSize() (Private Function)
        // Desc: Updates the camera's ortho size based on the current zoom point.
        //-----------------------------------------------------------------------------
        void UpdateOrthoSize()
        {
            // Update the ortho size. We want to update the size such that the objects that reside
            // at 'zoom' distance from the camera appear to have the same size in both perspective
            // and ortho modes. This is simple enough to do, but probably a bit more difficult to
            // explain why it works. The ortho size is the distance between the zoom point and the
            // camera focus plane. If we imagine the perspective camera frustum and the focus plane
            // to be the top plane for example and rotating the top plane such that it becomes the
            // top plane of an ortho camera frustum, the distance between the zoom point and the top
            // plane becomes the ortho size (half of ortho size actually).
            Camera camera = settings.targetCamera;
            float orthoSize = camera.CalculateFocusFrustumPlane().GetDistanceToPoint(zoomPoint);
            if (camera.aspect < 1.0f) orthoSize /= camera.aspect;
            SetOrthoSize(orthoSize);
        }

        //-----------------------------------------------------------------------------
        // Name: SetOrthoSize() (Private Function)
        // Desc: Sets the orthographic size to the specified value.
        // Parm: size - The new ortho size. The function applies any necessary clipping.
        //-----------------------------------------------------------------------------
        void SetOrthoSize(float size)
        {
            settings.targetCamera.orthographicSize = Mathf.Max(size, 1e-2f);
        }
        #endregion

        #region Coroutines
        //-----------------------------------------------------------------------------
        // Name: Coroutine_Focus() (Private Coroutine)
        // Desc: Coroutine which implements camera focus.
        //-----------------------------------------------------------------------------
        IEnumerator Coroutine_Focus()
        {
            // Keep going until the camera is in its final position and rotation
            while ((mFocusData.position - mTransform.position).magnitude > 1e-3f ||
                   (mUseFocusRotation && Quaternion.Angle(mTransform.rotation, mFocusRotation) > 1e-2f))
            {
                // Interpolate position, rotation and ortho size
                float t = Time.smoothDeltaTime * settings.focusSmoothSpeed;
                mTransform.position = Vector3.Lerp(mTransform.position, mFocusData.position, t);

                if (mUseFocusRotation)
                    mTransform.rotation = Quaternion.Slerp(mTransform.rotation, mFocusRotation, t);

                SetOrthoSize(Mathf.Lerp(settings.targetCamera.orthographicSize, mFocusData.orthoSize, t));

                // Update camera near plane
                UpdateNearPlane();

                // Wait for next turn
                yield return null;
            }

            // Snap to target values
            if (mUseFocusRotation)
                mTransform.rotation = mFocusRotation;

            mTransform.position = mFocusData.position;
            SetOrthoSize(mFocusData.orthoSize);
            UpdateNearPlane();

            // Clear rotation focus state
            mUseFocusRotation = false;
            mFocusRotation    = Quaternion.identity;

            // No longer focusing
            mFocusCrtn = null;
        }
        #endregion
    }
    #endregion
}