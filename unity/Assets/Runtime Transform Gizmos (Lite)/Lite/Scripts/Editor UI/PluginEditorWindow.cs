#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: PluginEditorWindow (Public Abstract Class)
    // Desc: Abstract editor window class which implements common editor window
    //       functionality. Must be derived by all editor windows used by the plugin.
    //-----------------------------------------------------------------------------
    public abstract class PluginEditorWindow : EditorWindow
    {
        #region Private Fields
        [NonSerialized] bool mInitialized   = false;    // Used for delayed initialization inside the 'Update' function
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: Show() (Public Static Function)
        // Desc: Shows the window.
        // Parm: T  - Window type. Must derive from 'PluginEditorWindow'.
        //-----------------------------------------------------------------------------
        public static T Show<T>() where T : PluginEditorWindow
        {
            // Create the window and show it
            T window = GetWindow<T>();  
            window.Show();

            // Return window
            return window;
        }

        //-----------------------------------------------------------------------------
        // Name: ShowModal() (Public Static Function)
        // Desc: Shows the window as a modal window.
        // Parm: T       - Window type. Must derive from 'PluginEditorWindow'.
        //       utility - True if a utility window is required.
        //       title   - Window title.
        //-----------------------------------------------------------------------------
        public static T ShowModal<T>(bool utility, string title) where T : PluginEditorWindow
        {
            // Create the window and show it.
            // Note: For modal windows, we need to call 'Update' manually to allow the window to initialize itself properly.
            T window = GetWindow<T>(utility, title);
            window.Update(); 
            window.ShowModal();

            // Return window
            return window;
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: OnPluginInit() (Public Function)
        // Desc: Called when the plugin is initialized. It allows the window to refresh
        //       itself with the new data. Otherwise, it will show stale data from a
        //       previous plugin instance which no longer exists.
        //-----------------------------------------------------------------------------
        public void OnPluginInit()
        {
            RebuildUI();
        }

        //-----------------------------------------------------------------------------
        // Name: RebuildUI() (Public Function)
        // Desc: Rebuilds the window's UI.
        //-----------------------------------------------------------------------------
        public void RebuildUI()
        {
            // Delete old UI and rebuild
            rootVisualElement.Clear();
            OnCreateUI(rootVisualElement);
        }
        #endregion

        #region Protected Virtual Functions
        //-----------------------------------------------------------------------------
        // Name: OnInit() (Protected Virtual Function)
        // Desc: Called to allow the window to initialize itself. This function is called
        //       before 'OnCreateUI'.
        //-----------------------------------------------------------------------------
        protected virtual void OnInit() {}

        //-----------------------------------------------------------------------------
        // Name: OnDispose() (Protected Virtual Function)
        // Desc: Called to allow the window to clean up after itself. This can happen
        //       when the window is disabled or destroyed.
        //-----------------------------------------------------------------------------
        protected virtual void OnDispose() {}

        //-----------------------------------------------------------------------------
        // Name: OnUpdate() (Protected Virtual Function)
        // Desc: Called once per frame to allow the window to update itself.
        //-----------------------------------------------------------------------------
        protected virtual void OnUpdate() {}

        //-----------------------------------------------------------------------------
        // Name: OnUndoRedo() (Protected Virtual Function)
        // Desc: Called when Undo/Redo is performed.
        //-----------------------------------------------------------------------------
        protected virtual void OnUndoRedo() {}
        #endregion

        #region Protected Abstract Functions
        //-----------------------------------------------------------------------------
        // Name: OnCreateUI() (Protected Abstract Function)
        // Desc: Called when the window UI must be created. This function is called after
        //       'OnInit'.
        // Parm: parent - Parent visual element where all controls will be added.
        //-----------------------------------------------------------------------------
        protected abstract void OnCreateUI(VisualElement parent);
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: Update() (Private Function)
        // Desc: Allows the window to update itself.
        //-----------------------------------------------------------------------------
        void Update()
        {
            // If there is no plugin instance, close the window
            if (RTG.get == null) { Close(); return; }

            // If we're not initialized, let's initialize
            if (!mInitialized)
            {
                OnInit();
                OnCreateUI(rootVisualElement);
                mInitialized = true;
            }

            // Called derived implementation
            OnUpdate();

            // Always repaint. It's silly to have to do this manually every time it's needed...
            Repaint();
        }

        //-----------------------------------------------------------------------------
        // Name: OnEnable() (Private Function)
        // Desc: Called when the window is enabled.
        //-----------------------------------------------------------------------------
        void OnEnable()
        {
            // Register handlers
            Undo.undoRedoPerformed += OnUndoRedo;
        }

        //-----------------------------------------------------------------------------
        // Name: OnDisable() (Private Function)
        // Desc: Called when the window is disabled.
        //-----------------------------------------------------------------------------
        void OnDisable()
        {
            // Call derived
            OnDispose();

            // Unregister handlers
            Undo.undoRedoPerformed -= OnUndoRedo;
        }
        #endregion
    }
    #endregion
}
#endif