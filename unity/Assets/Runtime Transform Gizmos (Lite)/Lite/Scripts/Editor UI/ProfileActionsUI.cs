#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ProfileActionsUI (Public Class)
    // Desc: Implements the creation and management of user interfaces that allow
    //       the user to add/delete/edit profiles.
    // Parm: TProfile        - Profile type. Must derive from 'Profile'.
    //       TProfileManager - Profile manager type. Must derive from 'IProfileManager<TProfile>'.
    //-----------------------------------------------------------------------------
    public class ProfileActionsUI<TProfile, TProfileManager> : VisualElement
        where TProfile          : Profile, new()
        where TProfileManager   : IProfileManager<TProfile>
    {
        #region Private Fields
        string          mProfileCategory        = string.Empty; // The name of the category to which the profile belongs
        ToolbarMenu     mActionDropDown;                        // Action dropdown menu
        float           mActionDropDownWidth    = 100.0f;       // Action dropdown width
        TProfileManager mProfileManager;                        // The profile manager used to create, remove and edit profiles

        // Buffers used to avoid memory allocations
        List<string>    mNameBuffer     = new List<string>();
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: actionDropDownWidth (Public Property)
        // Desc: Returns or sets the width of the action dropdown menu. This is the menu
        //       which displays available profiles and a few more items which enable the
        //       user to perform different actions such as create, rename and delete
        //       profiles.
        //-----------------------------------------------------------------------------
        public float actionDropDownWidth { get { return mActionDropDownWidth; } set { mActionDropDownWidth = value; if (mActionDropDown != null) mActionDropDown.style.width = mActionDropDownWidth; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Create() (Public Function)
        // Desc: Creates the UI using the specified parameters.
        // Parm: profileCategory - The name of the category to which the profiles belong
        //                         (e.g. "shortcut", "prefab library"). If null or empty,
        //                         an empty string will be used.
        //       profileManager  - Profile manager used to create, remove and edit profiles.
        //       parent          - Parent element.
        //-----------------------------------------------------------------------------
        public void Create(string profileCategory, TProfileManager profileManager, VisualElement parent)
        {
            // Set profile category
            mProfileCategory = profileCategory;
            if (string.IsNullOrEmpty(mProfileCategory))
                mProfileCategory = string.Empty;

            // Set profile manager
            mProfileManager = profileManager;

            // Add to parent
            parent.Add(this);

            // Create the action dropdown menu
            mActionDropDown = EditorUI.CreateToolbarMenu(mActionDropDownWidth, this);

            // Add menu items
            RefreshDropDownMenuItems();
            
            // When a new profile is selected in the dropdown, we need to enable/disable
            // menu items depending on the currently active profile. But it seems there
            // is no API to do this so we will just rebuild the dropdown menu items list.
            mActionDropDown.RegisterValueChangedCallback(e => 
            { RefreshDropDownMenuItems(); });
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: RefreshDropDownMenuItems() (Private Function)
        // Desc: Clears the dropdown menu's items and reinserts them while also updating
        //       their action status.
        //-----------------------------------------------------------------------------
        void RefreshDropDownMenuItems()
        {
            // Clear all items
            mActionDropDown.menu.ClearItems();

            // Add profile items to the dropdown menu
            mActionDropDown.text = mProfileManager.activeProfile.name;
            int profileCount = mProfileManager.profileCount;
            for (int i = 0; i < profileCount; ++i)
                InsertProfileItem(i, mProfileManager[i]);

            // Add actions
            mActionDropDown.menu.AppendSeparator();
            mActionDropDown.menu.AppendAction("Create new profile...",
            (p) =>
            {
                // Init window properties
                CreateEntityWindow.entityTag            = $"{mProfileCategory} profile";
                CreateEntityWindow.initialEntityName    = "New Profile";
                CreateEntityWindow.createEntity         = (name) =>
                {
                    // Create a new profile and add it to the dropdown
                    var newProfile = mProfileManager.CreateProfile(name);
                    InsertProfileItem(mProfileManager.profileCount - 1, newProfile);

                    // Make this the new active profile
                    Action_SetActiveProfile(newProfile.name);
                };

                // Show the window
                PluginEditorWindow.ShowModal<CreateEntityWindow>(true, "Create profile");
            });
            mActionDropDown.menu.AppendAction("Rename profile...", (p) =>
            {
                // Init window properties
                RenameEntityWindow.entityTag            = $"{mProfileCategory} profile";
                RenameEntityWindow.initialEntityName    = "New Profile Name";
                RenameEntityWindow.renameEntity         = (name) =>
                {
                    // Rename profile and change the dropdown text
                    mProfileManager.RenameProfile(mProfileManager.activeProfile, name);
                    mActionDropDown.text = mProfileManager.activeProfile.name;

                    // Now we need to remove the item and reinsert it with the new name
                    int itemIndex = mProfileManager.IndexOf(mProfileManager.activeProfile);
                    mActionDropDown.menu.RemoveItemAt(itemIndex);
                    InsertProfileItem(itemIndex, mProfileManager.activeProfile);
                };

                // Show the window
                PluginEditorWindow.ShowModal<RenameEntityWindow>(true, "Rename profile");

            }, mProfileManager.isDefaultProfileActive ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);
            mActionDropDown.menu.AppendAction("Delete profile...", (p) =>
            {
                // Init window properties
                DeleteEntityWindow.entityTag        = $"{mProfileCategory} profile";
                DeleteEntityWindow.entityName       = mProfileManager.activeProfile.name;
                DeleteEntityWindow.deleteEntity     = (name) =>
                {
                    // Store the index of this profile. We need it to remove the menu item.
                    int itemIndex = mProfileManager.IndexOf(mProfileManager.activeProfile);

                    // Delete the profile
                    mProfileManager.DeleteProfile(mProfileManager.activeProfile);

                    // Remove this item and check the item which is mapped to the new active profile
                    mActionDropDown.menu.RemoveItemAt(itemIndex);
                    mActionDropDown.text = mProfileManager.activeProfile.name;
                };

                // Show the window
                PluginEditorWindow.ShowModal<DeleteEntityWindow>(true, "Delete profile");

            }, mProfileManager.isDefaultProfileActive ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);
        }

        //-----------------------------------------------------------------------------
        // Name: InsertProfileItem() (Private Function)
        // Desc: Inserts a profile item in the dropdown menu at the specified index.
        // Parm: index   - Insertion index.
        //       profile - The profile associated with the item.
        //-----------------------------------------------------------------------------
        void InsertProfileItem(int index, TProfile profile)
        {
            mActionDropDown.menu.InsertAction(index, profile.name, (p) => { Action_SetActiveProfile(p.name); },
                (p) => { return p.name == mProfileManager.activeProfile.name ? DropdownMenuAction.Status.Checked : DropdownMenuAction.Status.Normal; });
        }

        //-----------------------------------------------------------------------------
        // Name: Action_SetActiveProfile() (Private Function)
        // Desc: Executes an action which set the active profile.
        //-----------------------------------------------------------------------------
        void Action_SetActiveProfile(string profileName)
        {
            mProfileManager.SetActiveProfile(profileName);
            mActionDropDown.text = profileName;
        }
        #endregion
    }
    #endregion
}
#endif