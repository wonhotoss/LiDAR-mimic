using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ShortcutProfile (Public Class)
    // Desc: Represents a shortcut profile. A shortcut profile stores a collection
    //       of shortcut categories and their shortcuts.
    //-----------------------------------------------------------------------------
    [Serializable] public class ShortcutProfile : Profile
    {
        #region Private Fields
        [SerializeField] List<ShortcutCategory> mShortcutCategories = new List<ShortcutCategory>(); // Shortcut categories for this profile

        // Buffers used to avoid memory allocations
        List<string>    mNameBuffer     = new List<string>();
        List<Shortcut>  mShortcutBuffer = new List<Shortcut>();
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: shortcutCategoryCount (Public Property)
        // Desc: Returns the number of shortcut categories that exist inside the profile.
        //-----------------------------------------------------------------------------
        public int              shortcutCategoryCount   { get { return mShortcutCategories.Count; } }

        //-----------------------------------------------------------------------------
        // Name: this[int index] (Public Property)
        // Desc: Indexer which returns the shortcut category with the specified index.
        //-----------------------------------------------------------------------------
        public ShortcutCategory this[int index]         { get { return mShortcutCategories[index]; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: UseDefaults() (Public Function)
        // Desc: Reset all shortcuts to default bindings.
        //-----------------------------------------------------------------------------
        public void UseDefaults()
        {
            // Loop through each shortcut in the active profile and make it use the default binding
            CollectShortcuts(mShortcutBuffer);
            int shortcutCount = mShortcutBuffer.Count;
            for (int i = 0; i < shortcutCount; ++i)
                mShortcutBuffer[i].binding = mShortcutBuffer[i].defaultBinding;

            // Detect conflicts. This essentially removes all conflicts assuming the defaults are ok.
            DetectShortcutConflicts();
        }

        //-----------------------------------------------------------------------------
        // Name: SetShortcutEnabled() (Public Function)
        // Desc: Sets the enabled state of the shortcut with the specified name.
        // Parm: shortcutName   - Shortcut name.
        //       categoryName   - Name of the category that contains the shortcut.
        //       enabled        - Shortcut enabled state.
        //-----------------------------------------------------------------------------
        public void SetShortcutEnabled(string shortcutName, string categoryName, bool enabled)
        {
            // Find the shortcut category which contains this shortcut and change the shortcut's active state
            int count = shortcutCategoryCount;
            for (int i = 0; i < count; ++i)
            {
                // Match category
                var category = mShortcutCategories[i];
                if (category.name == categoryName)
                {
                    // Find shortcut
                    var shortcut = category.FindShortcut(shortcutName);
                    if (shortcut != null)
                    {
                        shortcut.enabled = enabled;
                        break;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------
        // Name: DetectShortcutConflicts() (Public Function)
        // Desc: Detects all conflicts that exist between all shortcuts in this profile.
        //       When this function returns, each shortcut will have its conflict list
        //       updated accordingly.
        // Rtrn: True if at least one conflict was detected and false otherwise.
        //-----------------------------------------------------------------------------
        public bool DetectShortcutConflicts()
        {
            // First, clear all existing conflicts
            CollectShortcuts(mShortcutBuffer);
            int shortcutCount = mShortcutBuffer.Count;
            for (int i = 0; i < shortcutCount; ++i)
                mShortcutBuffer[i].ClearConflicts();

            // Now we need to loop through each shortcut and check for conflicts with other shortcuts
            bool foundConflicts = false;
            for (int i = 0; i < shortcutCount; ++i)
            {
                // Loop through other shortcuts
                for (int j = i + 1; j < shortcutCount; ++j)
                {
                    // Check for conflict
                    if (mShortcutBuffer[i].ConflictsWith(mShortcutBuffer[j]))
                    {
                        // Register conflicts
                        mShortcutBuffer[i].AddConflict(mShortcutBuffer[j].name);
                        mShortcutBuffer[j].AddConflict(mShortcutBuffer[i].name);
                        foundConflicts = true;
                    }
                }
            }

            // Return result
            return foundConflicts;
        }

        //-----------------------------------------------------------------------------
        // Name: FindOrCreateShortcutCategory() (Public Function)
        // Desc: Finds a shortcut category with the specified name and returns it if it
        //       exists. Otherwise, it creates a new category.
        // Parm: name - Category name.
        // Rtrn: The shortcut category (existing one or newly created).
        //-----------------------------------------------------------------------------
        public ShortcutCategory FindOrCreateShortcutCategory(string name)
        {
            // Find a category with the same name and return it if it exists
            var category = FindShortcutCategory(name);
            if (category != null) return category;

            // Create a new one
            return CreateShortcutCategory(name);
        }

        //-----------------------------------------------------------------------------
        // Name: CreateShortcutCategory() (Public Function)
        // Desc: Creates a new shortcut category with the specified name. 
        // Parm: name - Category name. If a category with the same name already exists,
        //              the name will be modified slightly to make it unique.
        // Rtrn: The created shortcut category or null if the name is invalid.
        //-----------------------------------------------------------------------------
        public ShortcutCategory CreateShortcutCategory(string name)
        {
            // Validate call
            if (string.IsNullOrEmpty(name))
                return null;

            // Generate a unique name
            CollectShortcutCategoryNames(mNameBuffer);
            name = NameGenerator.GenerateUnique(name, mNameBuffer);

            // Create the shortcut category and add it
            ShortcutCategory sc = new ShortcutCategory(name);
            mShortcutCategories.Add(sc);

            // Return the created category
            return sc;
        }

        //-----------------------------------------------------------------------------
        // Name: FindShortcutCategory() (Public Function)
        // Desc: Finds and returns the shortcut category with the specified name.
        // Parm: name - Name of shortcut category to search for.
        // Rtrn: The shortcut category with the specified name or null if no such shortcut
        //       category exists.
        //-----------------------------------------------------------------------------
        public ShortcutCategory FindShortcutCategory(string name)
        {
            // Loop through each category
            int categoryCount = shortcutCategoryCount;
            for (int i = 0; i < categoryCount; ++i)
            {
                // Match?
                if (mShortcutCategories[i].name == name)
                    return mShortcutCategories[i];
            }

            // Not found
            return null;
        }

        //-----------------------------------------------------------------------------
        // Name: CollectShortcutCategoryNames() (Public Function)
        // Desc: Collects all shortcut category names and stores them in 'names'.
        // Parm: names - Returns the names of all shortcut categories.
        //-----------------------------------------------------------------------------
        public void CollectShortcutCategoryNames(List<string> names)
        {
            // Clear old content
            names.Clear();

            // Loop through each shortcut category and store its name
            int count = shortcutCategoryCount;
            for (int i = 0; i < count; ++i)
                names.Add(mShortcutCategories[i].name);
        }

        //-----------------------------------------------------------------------------
        // Name: CollectShortcuts() (Public Function)
        // Desc: Collects all shortcuts that exist inside the profile and stores them
        //       inside 'shortcuts'.
        // Parm: shortcuts - Returns the shortcuts that exist inside the profile.
        //-----------------------------------------------------------------------------
        public void CollectShortcuts(List<Shortcut> shortcuts)
        {
            // Clear list
            shortcuts.Clear();

            // Loop through each shortcut category and add shortcuts
            int categoryCount = shortcutCategoryCount;
            for (int i = 0; i < categoryCount; ++i)
                mShortcutCategories[i].CollectShortcuts(true, shortcuts);
        }
        
        //-----------------------------------------------------------------------------
        // Name: RefreshShortcuts() (Public Function)
        // Desc: Creates/refreshes the shortcut categories and shortcut lists. Called when
        //       the profile is initialized. It can also be useful to call this each time
        //       the scripts are recompiled in order to ensure that any code added to this
        //       function is executed and all categories and shortcuts are properly created.
        //-----------------------------------------------------------------------------
        public void RefreshShortcuts()
        {
            // Store contexts here so that they can be reused
            Context_Global                  ctx_Global          = new Context_Global();
            Context_SceneView               ctx_SceneView       = new Context_SceneView();
            Context_SceneView_Navigation    ctx_SceneViewNav    = new Context_SceneView_Navigation();
            Context_Camera_FlyMode          ctx_Camera_FlyMode  = new Context_Camera_FlyMode();
            Context_Grid_ActionMode         ctx_Grid_ActionMode = new Context_Grid_ActionMode();

            // Create shortcut categories and populate them
            //-----------------------------------------------------------------------------
            // Camera
            //-----------------------------------------------------------------------------
            ShortcutCategory    sc = null;
            sc = FindOrCreateShortcutCategory(ShortcutCategoryNames.camera);
            sc.FindOrCreateShortcut(CameraShortcutNames.panMode,         new Command_Camera_PanMode(),       ctx_SceneViewNav, "mmb");
            sc.FindOrCreateShortcut(CameraShortcutNames.orbitMode,       new Command_Camera_OrbitMode(),     ctx_SceneViewNav, "alt, lmb");
            sc.FindOrCreateShortcut(CameraShortcutNames.flyMode,         new Command_Camera_FlyMode(),       ctx_SceneViewNav, "rmb").strictModifierCheck = false;
            sc.FindOrCreateShortcut(CameraShortcutNames.zoomMode,        new Command_Camera_ZoomMode(),      ctx_SceneViewNav, "alt, rmb");
            sc.FindOrCreateShortcut(CameraShortcutNames.flyLeft,         new Command_Camera_FlyLeft(),       ctx_Camera_FlyMode, "a").strictModifierCheck = false;
            sc.FindOrCreateShortcut(CameraShortcutNames.flyRight,        new Command_Camera_FlyRight(),      ctx_Camera_FlyMode, "d").strictModifierCheck = false;
            sc.FindOrCreateShortcut(CameraShortcutNames.flyDown,         new Command_Camera_FlyDown(),       ctx_Camera_FlyMode, "q").strictModifierCheck = false;                                        
            sc.FindOrCreateShortcut(CameraShortcutNames.flyUp,           new Command_Camera_FlyUp(),         ctx_Camera_FlyMode, "e").strictModifierCheck = false;                                                                
            sc.FindOrCreateShortcut(CameraShortcutNames.flyBackward,     new Command_Camera_FlyBackward(),   ctx_Camera_FlyMode, "s").strictModifierCheck = false;                                          
            sc.FindOrCreateShortcut(CameraShortcutNames.flyForward,      new Command_Camera_FlyForward(),    ctx_Camera_FlyMode, "w").strictModifierCheck = false;
            sc.FindOrCreateShortcut(CameraShortcutNames.speedModifier,   new Command_Camera_SpeedModifier(), ctx_Camera_FlyMode, "shift");

            //-----------------------------------------------------------------------------
            // Grid
            //-----------------------------------------------------------------------------
            sc = FindOrCreateShortcutCategory(ShortcutCategoryNames.grid);
            sc.FindOrCreateShortcut(GridShortcutNames.stepMoveDown,              new Command_Grid_StepMoveDown(),            ctx_SceneView, "[");
            sc.FindOrCreateShortcut(GridShortcutNames.stepMoveUp,                new Command_Grid_StepMoveUp(),              ctx_SceneView, "]");
            sc.FindOrCreateShortcut(GridShortcutNames.actionMode,                new Command_Grid_ActionMode(),              ctx_SceneView, "g");
            sc.FindOrCreateShortcut(GridShortcutNames.snapToPickPoint,           new Command_Grid_SnapToPickPoint(),         ctx_Grid_ActionMode, "lmb");
            sc.FindOrCreateShortcut(GridShortcutNames.snapToPickPointExtents,    new Command_Grid_SnapToPickPointExtents(),  ctx_Grid_ActionMode, "rmb");

            //-----------------------------------------------------------------------------
            // Gizmos
            //-----------------------------------------------------------------------------
            sc = FindOrCreateShortcutCategory(ShortcutCategoryNames.gizmos);
            sc.FindOrCreateShortcut(GizmosShortcutNames.snap,        new Command_Gizmos_Snap(),          ctx_SceneView, "ctrl").strictModifierCheck = false;
            sc.FindOrCreateShortcut(GizmosShortcutNames.vertexSnap,  new Command_Gizmos_VertexSnap(),    ctx_SceneView, "v");
            sc.FindOrCreateShortcut(GizmosShortcutNames.altMode,     new Command_Gizmos_AltMode(),       ctx_SceneView, "shift").strictModifierCheck = false;
          
            //-----------------------------------------------------------------------------
            // Undo/Redo
            //-----------------------------------------------------------------------------
            sc = FindOrCreateShortcutCategory(ShortcutCategoryNames.undoRedo);
            sc.FindOrCreateShortcut(UndoRedoShortcutNames.undo,      new Command_Undo(),    ctx_Global, "ctrl, z");
            sc.FindOrCreateShortcut(UndoRedoShortcutNames.redo,      new Command_Redo(),    ctx_Global, "ctrl, y");
        }
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: OnInit() (Protected Function)
        // Desc: Called by the base class to allow the profile to initialize itself.
        //-----------------------------------------------------------------------------
        protected override void OnInit()
        {
            RefreshShortcuts();
        }
        #endregion
    }
    #endregion
}
