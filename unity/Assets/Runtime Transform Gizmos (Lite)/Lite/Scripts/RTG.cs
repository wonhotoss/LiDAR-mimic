using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTG (Public Singleton Class)
    // Desc: Plugin singleton. App logic starts here.
    //-----------------------------------------------------------------------------
    #if UNITY_EDITOR
    [ExecuteInEditMode]
    #endif
    [DefaultExecutionOrder(-32000)] public class RTG : MonoSingleton<RTG>
    {
        #region Public Delegates & Events
        //-----------------------------------------------------------------------------
        // Name: InitializedHandler() (Public Delegate)
        // Desc: Handler for the plugin initialized event fired after the plugin is
        //       initialized and ready to go.
        //-----------------------------------------------------------------------------
        public delegate void    InitializedHandler();
        public event            InitializedHandler initialized;
        #endregion

        #region Public Static Properties
        //-----------------------------------------------------------------------------
        // Name: pluginName (Public Static Property)
        // Desc: Returns the plugin name.
        //-----------------------------------------------------------------------------
        public static string pluginName         { get { return "Runtime Transform Gizmos"; } }

        //-----------------------------------------------------------------------------
        // Name: shortPluginName (Public Static Property)
        // Desc: Returns the short plugin name.
        //-----------------------------------------------------------------------------
        public static string shortPluginName    { get { return "RTG"; } }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: Exception() (Public Static Function)
        // Desc: Throws and exception.
        // Parm: className - The name of the class that throws the exception.
        //       funcName  - The name of the function that throws the exception.
        //       message   - Exception message.
        //-----------------------------------------------------------------------------
        public static void Exception(string className, string funcName, string message)
        {
            throw new UnityException($"{className}.{funcName}(): {message}");
        }
        #endregion

        #region Private Functions       
        //-----------------------------------------------------------------------------
        // Name: Awake() (Private Function)
        // Desc: Called by Unity to allow the app to initialize itself.
        //-----------------------------------------------------------------------------
        void Awake()
        {
            // Not playing?
            if (!Application.isPlaying)
                return;

            // Register handlers
            SceneManager.sceneLoaded    += OnSceneLoaded;
            SceneManager.sceneUnloaded  += OnSceneUnloaded;
        }

        //-----------------------------------------------------------------------------
        // Name: Start() (Private Function)
        // Desc: Called by Unity to allow the app to initialize itself.
        //-----------------------------------------------------------------------------
        void Start()
        {
            // Not playing?
            if (!Application.isPlaying)
                return;

            // Init modules
            RTCamera.get.Internal_Init();

            // The UI camera doesn't render anything except the UI
            #if RTGUI_PRESENT
            RTCamera.get.SetCameraRenderConfig(RTUI.get.uiCamera, new RTCameraRenderConfig(){ renderFlags = ECameraRenderFlags.None });
            #endif

            // We're initialized
            if (initialized != null) 
                initialized();
        }
        
        //-----------------------------------------------------------------------------
        // Name: OnEnable() (Private Function)
        // Desc: Called by Unity to allow the object to initialize itself.
        //-----------------------------------------------------------------------------
        void OnEnable()
        {
#if UNITY_EDITOR
            // Loop through each renderer in the active render pipeline and add the required features
            var renderers = UniversalRenderPipeline.asset.rendererDataList;
            foreach (var r in renderers)
            {
                r.AddRenderFeature<RTMainSRF>();
            }
#endif
        }

        //-----------------------------------------------------------------------------
        // Name: OnDisable() (Private Function)
        // Desc: Called by Unity to allow the object to perform any necessary cleanup.
        //-----------------------------------------------------------------------------
        void OnDisable()
        {
            // Not playing?
            if (!Application.isPlaying)
                return;

            // Unregister handlers
            SceneManager.sceneLoaded    -= OnSceneLoaded;
            SceneManager.sceneUnloaded  -= OnSceneUnloaded;
        }

        //-----------------------------------------------------------------------------
        // Name: Update() (Private Function)
        // Desc: Called by Unity to allow the app to update itself.
        //-----------------------------------------------------------------------------
        void Update()
        {
            // Not playing?
            if (!Application.isPlaying)
                return;

            // Update modules
            RTUndo.get.Internal_Update();
            RTScene.get.Internal_Update();
            RTCamera.get.Internal_Update();
            RTGizmos.get.Internal_Update();
            RTGrid.get.Internal_Update();

            // Process shortcuts
            RTInput.get.Internal_ProcessShortcuts();
        }

        //-----------------------------------------------------------------------------
        // Name: LateUpdate() (Private Function)
        // Desc: Called by Unity to allow the app to update itself during the late update 
        //       phase.
        //-----------------------------------------------------------------------------
        void LateUpdate()
        {
            // Not playing?
            if (!Application.isPlaying)
                return;

            // Update
            RTUndo.get.Internal_LateUpdate();
        }

        //-----------------------------------------------------------------------------
        // Name: OnSceneLoaded() (Private Function)
        // Desc: Called when a new scene is loaded.
        // Parm: scene      - The loaded scene.
        //       loadMode   - The scene load mode.
        //-----------------------------------------------------------------------------
        void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
        {
            // Not playing?
            if (!Application.isPlaying)
                return;

            // Load current scene
            RTScene.get.Internal_LoadCurrent();
        }

        //-----------------------------------------------------------------------------
        // Name: OnSceneUnloaded() (Private Function)
        // Desc: Called when a scene is unloaded.
        // Parm: scene - The scene which was unloaded.
        //-----------------------------------------------------------------------------
        void OnSceneUnloaded(Scene scene)
        {
            // Not playing?
            if (!Application.isPlaying)
                return;

            // Unload
            RTScene.get.Internal_Unload();
            RTGizmos.get.Internal_Clear();
        }
#endregion

        #region Private Static Functions
        //-----------------------------------------------------------------------------
        // Name: Init() (Private Static Function)
        // Desc: Menu item which initializes the plugin.
        //-----------------------------------------------------------------------------
#if UNITY_EDITOR
        [MenuItem("Tools/RTG/Initialize", false, 0)]
        static void Init()
        {
            // Check if we already have an RTG object. If we do, show a confirmation dialog.
            if (FindObjectsByType<RTG>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length != 0)
            {
                const string caption = "RTG Already Exists";
                const string message = "An RTG object already exists in the scene. Creating a new one will discard all existing data. Do you want to continue?";
                if (!EditorUtility.DisplayDialog(caption, message, "Create New", "Cancel"))
                    return;
            }

            // Make sure we clean up and destroy any plugin modules that were created previously
            var moduleTypes = GetRTGModuleTypes();
            foreach (var moduleType in moduleTypes) 
            {
                // Find modules of this type and destroy them
                var modules = FindObjectsByType(moduleType, FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var module in modules)
                    DestroyImmediate(((MonoBehaviour)module).gameObject);
            }

            // Create the 'RTG' game object
            var rtgApp = new GameObject("RTG");
            rtgApp.AddComponent<RTG>();

            // Create modules
            CreateModuleObject<RTGizmos>("RTGizmos", rtgApp);
            CreateModuleObject<RTScene>("RTScene", rtgApp);
            CreateModuleObject<RTCamera>("RTCamera", rtgApp);
            CreateModuleObject<RTGrid>("RTGrid", rtgApp);
            CreateModuleObject<RTInput>("RTInput", rtgApp);
            CreateModuleObject<RTUndo>("RTUndo", rtgApp);
            EditorUtility.SetDirty(rtgApp);

            // Notify all plugin windows that the plugin has been initialized
            var pluginWindows = Resources.FindObjectsOfTypeAll<PluginEditorWindow>();
            foreach (var window in pluginWindows)
                window.OnPluginInit();
        }

        //-----------------------------------------------------------------------------
        // Name: OpenShorcutManagerWindow() (Private Static Function)
        // Desc: Menu item which opens the shortcut manager window.
        //-----------------------------------------------------------------------------
        [MenuItem("Tools/RTG/Windows/Shortcuts...", false, 10)]
        static void OpenShorcutManagerWindow()
        {
            // Open the window
            PluginEditorWindow.Show<ShortcutManagerWindow>();
        }

        //-----------------------------------------------------------------------------
        // Name: CreateModuleObject() (Private Static Function)
        // Desc: Creates a new plugin module object with the specified name and parent.
        // Parm: name         - Module name.
        //       parentModule - The parent module object.
        // Rtrn: The created module object.
        //-----------------------------------------------------------------------------
        static T CreateModuleObject<T>(string name, GameObject parentModule) where T : MonoBehaviour
        {
            // Create module object
            var moduleObject = new GameObject(name);
            moduleObject.transform.parent = parentModule.transform;
            return moduleObject.AddComponent<T>();
        }

        //-----------------------------------------------------------------------------
        // Name: GetRTGModuleTypes() (Private Static Function)
        // Desc: Returns an array of all module types recognized by the plugin.
        // Rtrn: An array that contains all module types recognized by the plugin.
        //-----------------------------------------------------------------------------
        static Type[] GetRTGModuleTypes()
        {
            return new Type[] 
            { 
                typeof(RTG),    typeof(RTGizmos),   typeof(RTCamera), 
                typeof(RTGrid), typeof(RTInput),    typeof(RTScene),
                typeof(RTUndo)
            };
        }
#endif
#endregion
    }
#endregion
}
