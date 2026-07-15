#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace RTGLite
{
    #pragma warning disable CS0618 // Unity TreeView API is obsolete but still required for editor UI
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ShortcutCategoryList (Public Class)
    // Desc: Implements the shortcut category list which displays the shortcut
    //       categories in the active shortcut profile inside the shortcut window.
    //-----------------------------------------------------------------------------
    public class ShortcutCategoryList : TreeView
    {
        //-----------------------------------------------------------------------------
        // Name: SelectionChangedHandler() (Public Delegate)
        // Desc: Handler for selection changed event.
        // Parm: selectedCategory - The new selected category.
        //-----------------------------------------------------------------------------
        public delegate void    SelectionChangedHandler(ShortcutCategory selectedCategory);
        public event            SelectionChangedHandler selectionChanged;

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: selectedCategory() (Public Property)
        // Desc: Returns the selected shortcut category.
        //-----------------------------------------------------------------------------
        public ShortcutCategory selectedCategory 
        { 
            get 
            { 
                IList<int> selectedIds = GetSelection();
                if (selectedIds == null || selectedIds.Count == 0)
                    return null;

                return GetCategoryByIndex(selectedIds[0]);
            } 
        }
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------
        // Name: ShortcutCategoryList() (Public Constructor)
        // Desc: Creates the shortcut category list.
        // Parm: listState      - List state object.
        //       columnHeade    - Column header.
        //-----------------------------------------------------------------------------
        public ShortcutCategoryList(TreeViewState listState, MultiColumnHeader columnHeader)
            : base(listState, columnHeader)
        {
            showBorder          = true;
            columnHeader.height = EditorUI.defaultHeaderHeight;
            Reload();
        }
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: BuildRoot() (Protected Function)
        // Desc: Creates the items in the list.
        //-----------------------------------------------------------------------------
        protected override TreeViewItem BuildRoot()
        {
            // Create the root item
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };

            // Loop through each category in the active profile
            var profile = RTInput.get.shortcutProfileManager.activeProfile;
            int count   = profile.shortcutCategoryCount;
            for (int i = 0; i < count; ++i)
            {
                // Create a tree view item for this shortcut category
                var item = new TreeViewItem { id = i, depth = 0, displayName = profile[i].name };
                root.AddChild(item);
            }

            // Calculate depths and return the root
            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        //-----------------------------------------------------------------------------
        // Name: CanMultiSelect() (Protected Function)
        // Desc: Returns whether or no the specified item can be part of a multiselection.
        // Parm: item - Query item.
        //-----------------------------------------------------------------------------
        protected override bool CanMultiSelect(TreeViewItem item)
        {
            // Don't allow multiselection
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: SelectionChanged() (Protected Function)
        // Desc: Event handler for the selection changed event.
        //-----------------------------------------------------------------------------
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null || selectedIds.Count == 0)
                return;

            ShortcutCategory category = GetCategoryByIndex(selectedIds[0]);
            if (selectionChanged != null && category != null)
                selectionChanged(category);
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: GetCategoryByIndex() (Private Function)
        // Desc: Returns the shortcut category with the specified index.
        // Parm: categoryIndex - Shortcut category index.
        // Rtrn: The shortcut category with the specified index or null if not found.
        //-----------------------------------------------------------------------------
        ShortcutCategory GetCategoryByIndex(int categoryIndex)
        {
            var profile = RTInput.get.shortcutProfileManager.activeProfile;
            if (profile == null || profile.shortcutCategoryCount == 0)
                return null;

            if (categoryIndex < 0 || categoryIndex >= profile.shortcutCategoryCount)
                return null;

            return profile[categoryIndex];
        }
        #endregion
    }
        #endregion
#pragma warning restore CS0618
}
#endif