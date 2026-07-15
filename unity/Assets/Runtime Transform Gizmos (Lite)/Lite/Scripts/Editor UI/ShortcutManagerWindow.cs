#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RTGLite
{
    #pragma warning disable CS0618 // Unity TreeView API is obsolete but still required for editor UI
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ShortcutManagerWindow (Public Class)
    // Desc: Implements the functionality for the shortcut manager window.
    //-----------------------------------------------------------------------------
    public class ShortcutManagerWindow : PluginEditorWindow
    {
        #region Private Static Readonly Fields
        static readonly float sShortcutCategoryListWidth = 150.0f;                  // Shortcut category list width
        #endregion

        #region Private Fields
        [SerializeField] TreeViewState          mShortcutCategoryListState;         // Shortcut category list state
        [SerializeField] MultiColumnHeaderState mShortcutCategoryListHeaderState;   // Shortcut category list header state
                         ShortcutCategoryList   mShortcutCategoryList;              // Shortcut category list

        [SerializeField] TreeViewState          mShortcutListState;                 // Shortcut list state
        [SerializeField] MultiColumnHeaderState mShortcutListHeaderState;           // Shortcut list header state
                         ShortcutList           mShortcutList;                      // Shortcut list which displays the shortcuts in the selected category

        ProfileActionsUI<ShortcutProfile, ShortcutProfileManager> 
                                                mProfileActionsUI;                  // Profile actions UI used to create, delete, rename etc profiles

        [System.NonSerialized] ShortcutProfileManager 
                                                mShortcutProfileManager;            // Shortcut profile manager used by this window
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: OnInit() (Protected Function)
        // Desc: Called to allow the window to initialize itself. This function is called
        //       before 'OnCreateUI'.
        //-----------------------------------------------------------------------------
        protected override void OnInit()
        {
            // Init data
            minSize             = new Vector2(500, 300);
            titleContent.text   = "Shortcuts";

            // Create list states
            if (mShortcutCategoryListState == null)
            {
                mShortcutCategoryListState = new TreeViewState();
                mShortcutCategoryListState.selectedIDs.Add(0);      // Select the first shortcut category. Assumes shortcut ids are the indices inside the shortcut list.
            }
            if (mShortcutCategoryListHeaderState == null)
            {
                mShortcutCategoryListHeaderState = new MultiColumnHeaderState(new MultiColumnHeaderState.Column[1] 
                {
                    new MultiColumnHeaderState.Column()
                    {
                        headerContent           = new GUIContent("Categories"),
					    headerTextAlignment     = TextAlignment.Left,                        
					    width                   = sShortcutCategoryListWidth, 
					    minWidth                = sShortcutCategoryListWidth,
					    maxWidth                = sShortcutCategoryListWidth,
					    autoResize              = false,
                        canSort                 = false,
                    }
                });
            }
            if (mShortcutListState == null) mShortcutListState = new TreeViewState();
            if (mShortcutListHeaderState == null)
            {
                mShortcutListHeaderState = new MultiColumnHeaderState(new MultiColumnHeaderState.Column[] 
                {
                    new MultiColumnHeaderState.Column()
                    {
                        headerContent           = new GUIContent(null as Texture2D, "Toggle shortcut enabled state."),
					    headerTextAlignment     = TextAlignment.Center,                       
					    width                   = 16, 
					    minWidth                = 16,
					    maxWidth                = 16,
					    autoResize              = true,
                        canSort                 = false,
                    },
                    new MultiColumnHeaderState.Column()
                    {
                        headerContent           = new GUIContent("Name"),
					    headerTextAlignment     = TextAlignment.Left,                        
					    width                   = 100.0f, 
					    minWidth                = 100.0f,
					    maxWidth                = 350.0f,
					    autoResize              = true,
                        canSort                 = false,
                    },
                    new MultiColumnHeaderState.Column()
                    {
                        headerContent           = new GUIContent("Context"),
					    headerTextAlignment     = TextAlignment.Left,                        
					    width                   = 150.0f, 
					    minWidth                = 100.0f,
					    maxWidth                = 350.0f,
					    autoResize              = true,
                        canSort                 = false,
                    },
                    new MultiColumnHeaderState.Column()
                    {
                        headerContent           = new GUIContent("Type"),
					    headerTextAlignment     = TextAlignment.Left,                        
					    width                   = 100.0f, 
					    minWidth                = 100.0f,
					    maxWidth                = 100.0f,
					    autoResize              = true,
                        canSort                 = false,
                    },
                    new MultiColumnHeaderState.Column()
                    {
                        headerContent           = new GUIContent("Shortcut"),
					    headerTextAlignment     = TextAlignment.Left,                        
					    width                   = 130.0f, 
					    autoResize              = true,
                        canSort                 = false,
                    },
                });
            }

            // Store the profile manager used by this window.
            mShortcutProfileManager = RTInput.get.shortcutProfileManager;

            // Register handlers.
            mShortcutProfileManager.activeProfileChanged -= OnActiveProfileChanged;
            mShortcutProfileManager.activeProfileChanged += OnActiveProfileChanged;
        }

        //-----------------------------------------------------------------------------
        // Name: OnDispose() (Protected Function)
        // Desc: Called to allow the window to clean up after itself. This can happen
        //       when the window is disabled or destroyed.
        //-----------------------------------------------------------------------------
        protected override void OnDispose()
        {
            // Unregister from the exact profile manager this window was using
            if (mShortcutProfileManager != null)
                mShortcutProfileManager.activeProfileChanged -= OnActiveProfileChanged;

            mShortcutProfileManager = null;
        }

        //-----------------------------------------------------------------------------
        // Name: OnCreateUI() (Protected Function)
        // Desc: Called when the window UI must be created. This function is called after
        //       'OnInit'.
        // Parm: parent - Parent visual element where all controls will be added.
        //-----------------------------------------------------------------------------
        protected override void OnCreateUI(VisualElement parent)
        {
            // Create a container control
            VisualElement container = new VisualElement();
            container.style.SetMargin(EditorUI.windowMargin);
            container.style.flexDirection = FlexDirection.Column;
            container.style.flexGrow = 1.0f;
            parent.Add(container);

            // The parent which stores the controls sitting on top of the 2 lists
            VisualElement topRow        = new VisualElement();
            topRow.style.flexDirection  = FlexDirection.Row;
            topRow.style.justifyContent = Justify.FlexStart;
            container.Add(topRow);

            // Create profile actions UI
            mProfileActionsUI = new ProfileActionsUI<ShortcutProfile, ShortcutProfileManager>();
            mProfileActionsUI.Create("shortcut", mShortcutProfileManager, topRow);
            mProfileActionsUI.actionDropDownWidth = sShortcutCategoryListWidth;
            mProfileActionsUI.style.marginBottom = 5.0f;

            // Create the button that syncs the enabled states across all profiles
            var button = EditorUI.CreateButton(() =>
            {
                RTInput.get.SyncShortcutEnabledStates();
                mShortcutList.Reload();

            }, "Sync Enabled", "Sync the shortcut enabled states across all profiles with the sates in the currently active profile.", topRow);

            // Create use defaults button
            button = EditorUI.CreateUseDefaultsButton(() => 
            {
                // Use defaults
                mShortcutProfileManager.activeProfile.UseDefaults();
                mShortcutList.Reload();

            }, topRow);
            button.tooltip = "Use default shortcuts in the active profile. Doesn't affect the shortcut enabled states.";

            // Create the lists
            mShortcutCategoryList   = new ShortcutCategoryList(mShortcutCategoryListState, new MultiColumnHeader(mShortcutCategoryListHeaderState));
            mShortcutList           = new ShortcutList(mShortcutListState, new MultiColumnHeader(mShortcutListHeaderState), mShortcutCategoryList.selectedCategory);

            // Register list handlers
            mShortcutCategoryList.selectionChanged += (c) => { mShortcutList.shortcutCategory = c; };

            // Create the parent control for the 2 lists which sit side by side
            IMGUIContainer lists = new IMGUIContainer();
            lists.style.flexDirection = FlexDirection.Row;
            lists.style.flexGrow = 1.0f;
            container.Add(lists);
            lists.onGUIHandler += () => 
            {
                mShortcutCategoryList.OnGUI(new Rect(0.0f, 0.0f, sShortcutCategoryListWidth, position.height));
                mShortcutList.OnGUI(new Rect(sShortcutCategoryListWidth + 1.0f, 0.0f, position.width - sShortcutCategoryListWidth, position.height));
            };
        }

        //-----------------------------------------------------------------------------
        // Name: OnUpdate() (Protected Function)
        // Desc: Called once per frame to allow the window to update itself.
        //-----------------------------------------------------------------------------
        protected override void OnUpdate()
        {
            if (RTInput.get == null)
                return;

            ShortcutProfileManager currentProfileManager = RTInput.get.shortcutProfileManager;
            if (ReferenceEquals(mShortcutProfileManager, currentProfileManager))
                return;

            RecreateUI();
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: RecreateUI() (Private Function)
        // Desc: Recreates the window UI using the current shortcut profile manager.
        //-----------------------------------------------------------------------------
        void RecreateUI()
        {
            // Dispose old references
            OnDispose();

            // Clear old UI
            rootVisualElement.Clear();

            // Reinitialize using the current RTInput instance
            OnInit();

            // Rebuild UI
            OnCreateUI(rootVisualElement);

            // Refresh
            Repaint();
        }

        //-----------------------------------------------------------------------------
        // Name: OnActiveProfileChanged() (Private Function)
        // Desc: Event handler for the active shortcut profile change event.
        // Parm: activeProfile - The new active profile.
        //-----------------------------------------------------------------------------
        void OnActiveProfileChanged(ShortcutProfile activeProfile)
        {
            mShortcutCategoryList.Reload();
            mShortcutList.shortcutCategory = mShortcutCategoryList.selectedCategory;    // We now have a different category in the new profile
        }
        #endregion
    }
    #endregion
    #pragma warning restore CS0618
}
#endif