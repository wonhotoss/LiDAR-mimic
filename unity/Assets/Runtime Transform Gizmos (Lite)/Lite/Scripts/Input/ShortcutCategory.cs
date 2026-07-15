using UnityEngine;
using System;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ShortcutCategory (Public Class)
    // Desc: Represents a shortcut category. Shortcut categories can be used to organize
    //       shortcuts.
    //-----------------------------------------------------------------------------
    [Serializable] public class ShortcutCategory
    {
        #region Private Fields
        [SerializeField] string         mName       = string.Empty;             // Category name
        [SerializeField] List<Shortcut> mShortcuts  = new List<Shortcut>();     // The shortcuts assigned to this category
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: name (Public Property)
        // Desc: Returns the name of the shortcut category.
        //-----------------------------------------------------------------------------
        public string   name            { get { return mName; } }

        //-----------------------------------------------------------------------------
        // Name: shortcutCount (Public Property)
        // Desc: Returns the number of shortcuts that exist inside the category.
        //-----------------------------------------------------------------------------
        public int      shortcutCount   { get { return mShortcuts.Count; } }

        //-----------------------------------------------------------------------------
        // Name: this[int index] (Public Property)
        // Desc: Indexer which returns the shortcut with the specified index.
        //-----------------------------------------------------------------------------
        public Shortcut this[int index] { get { return mShortcuts[index]; } }
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------
        // Name: ShortcutCategory() (Public Constructor)
        // Desc: Creates a shortcut category with the specified name.
        // Parm: name - Category name. Must be valid.
        //-----------------------------------------------------------------------------
        public ShortcutCategory(string name)
        {
            // Assign name and validate it
            mName = name;
            if (string.IsNullOrEmpty(mName))
                RTG.Exception(nameof(ShortcutCategory), nameof(ShortcutCategory), "A shortcut category must have a valid name.");
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: FindOrCreateShortcut() (Public Function)
        // Desc: Finds a shortcut with the specified name and returns it if it exists.
        //       If found, the shortcut's structural definition (command and context)
        //       is synchronized with the specified instances while preserving its
        //       binding and enabled state.
        //       Otherwise, a new shortcut is created.
        // Parm: name           - Shortcut name. Must be valid.
        //       command        - Command which defines the shortcut behavior.
        //       context        - The context in which the shortcut can be triggered.
        //       bindingString  - Shortcut binding in text format (e.g. shift, lmb, w). If
        //                        null or invalid, the shortcut will be created without
        //                        a binding. Ignored when the shortcut already exists.
        // Rtrn: The shortcut (existing one or newly created).
        //-----------------------------------------------------------------------------
        public Shortcut FindOrCreateShortcut(string name, Command command, Context context, string bindingString)
        {
            // Find a shortcut with this name and return it if it exists
            Shortcut sh = FindShortcut(name);
            if (sh != null)
            {   
                sh.SyncDefinition(command, context);
                return sh;
            }

            // Create the shortcut
            return CreateShortcut(name, command, context, bindingString);
        }

        //-----------------------------------------------------------------------------
        // Name: CreateShortcut() (Public Function)
        // Desc: Creates a shortcut with the specified name and command.
        // Parm: name           - Shortcut name. Must be valid.
        //       command        - Command to be triggered when the shortcut is active.
        //                        Must be valid.
        //       context        - The context in which the shortcut's command can be triggered.
        //                        Must be valid.
        //       bindingString  - Shortcut binding in text format (e.g. shift,lmb,w). If
        //                        null or invalid, the shortcut will be created without a
        //                        binding.
        // Rtrn: The created shortcut or null if a shortcut with the same name already
        //       exists.
        //-----------------------------------------------------------------------------
        public Shortcut CreateShortcut(string name, Command command, Context context, string bindingString)
        {
            // Already exists?
            if (FindShortcut(name) != null)
                return null;

            // Create shortcut
            Shortcut s = new Shortcut(name, command, context, bindingString);
            mShortcuts.Add(s);

            // Return shortcut
            return s;
        }

        //-----------------------------------------------------------------------------
        // Name: FindShortcut() (Public Function)
        // Desc: Finds and returns the shortcut with the specified name.
        // Parm: name - Name of shortcut to search for.
        // Rtrn: The shortcut with the specified name or null if no such shortcut exists.
        //-----------------------------------------------------------------------------
        public Shortcut FindShortcut(string name)
        {
            // Loop through each shortcut and search for a match
            int shCount = shortcutCount;
            for (int i = 0; i < shCount; ++i)
            {
                // Match?
                if (mShortcuts[i].name == name)
                    return mShortcuts[i];
            }

            // Not found
            return null;
        }

        //-----------------------------------------------------------------------------
        // Name: CollectShortcutNames() (Public Function)
        // Desc: Collects all shortcut names and stores them in 'names'.
        // Parm: names - Returns the names of all shortcuts stored in the category.
        //-----------------------------------------------------------------------------
        public void CollectShortcutNames(List<string> names)
        {
            // Clear old content
            names.Clear();

            // Loop through each shortcut and store its name
            int count = shortcutCount;
            for (int i = 0; i < count; ++i)
                names.Add(mShortcuts[i].name);
        }

        //-----------------------------------------------------------------------------
        // Name: CollectShortcuts() (Public Function)
        // Desc: Collects all shortcuts that exist inside this category and stores them
        //       inside 'shortcuts'.
        // Parm: append    - If true, the category shortcuts will be added to existing
        //                   shortcuts inside the 'shortcuts' list. Otherwise, the 'shortcuts'
        //                   list will be cleared before the category shortcuts are added to it.
        //       shortcuts - Returns the collected shortcuts.
        //-----------------------------------------------------------------------------
        public void CollectShortcuts(bool append, List<Shortcut> shortcuts)
        {
            // Clear if necessary
            if (!append) shortcuts.Clear();

            // Add shortcuts
            shortcuts.AddRange(mShortcuts);
        }
        #endregion
    }
    #endregion
}
