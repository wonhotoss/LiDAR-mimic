using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ShortcutProfileManager (Public Class)
    // Desc: Manages a collection of shortcut profiles.
    //-----------------------------------------------------------------------------
    [Serializable] public class ShortcutProfileManager : IProfileManager<ShortcutProfile>
    {
        #region Public Delegates & Events
        //-----------------------------------------------------------------------------
        // Name: ActiveProfileChangedHandler() (Public Delegate)
        // Desc: Handler for the event which is fired when the active profile changes.
        // Parm: activeProfile - The new active profile.
        //-----------------------------------------------------------------------------
        public delegate void    ActiveProfileChangedHandler (ShortcutProfile activeProfile);
        public event            ActiveProfileChangedHandler activeProfileChanged;
        #endregion

        #region Private Fields
        [SerializeField]        List<ShortcutProfile>   mProfiles       = new List<ShortcutProfile>();      // Available profiles
        [SerializeField]        int                     mActiveProfileIndex;                                // Active profile

        // Buffers used to avoid memory allocation
        List<string> mNameBuffer = new List<string>();
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: profileCount (Public Property)
        // Desc: Returns the number of profiles.
        //-----------------------------------------------------------------------------
        public int  profileCount    { get { return mProfiles.Count; } }

        //-----------------------------------------------------------------------------
        // Name: activeProfile (Public Property)
        // Desc: Returns or sets the active profile. Null profiles are not allowed.
        //-----------------------------------------------------------------------------
        public ShortcutProfile  activeProfile
        {
            get { return mProfiles[mActiveProfileIndex]; }
            set
            {
                if (value != null)
                {
                    mActiveProfileIndex = IndexOf(value);
                    if (activeProfileChanged != null)
                        activeProfileChanged(mProfiles[mActiveProfileIndex]);
                }
            }
        }

        //-----------------------------------------------------------------------------
        // Name: isDefaultProfileActive (Public Property)
        // Desc: Returns true if the default profile is the active profile.
        //-----------------------------------------------------------------------------
        public bool isDefaultProfileActive  { get { return IsDefaultProfile(activeProfile); } }

        //-----------------------------------------------------------------------------
        // Name: this[int index] (Public Property)
        // Desc: Indexer which returns the profile with the specified index.
        //-----------------------------------------------------------------------------
        public ShortcutProfile this[int index] { get { return mProfiles[index]; } }
        #endregion

        #region Public Static Properties
        public static string defaultProfileName { get { return "Default"; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Init() (Public Function)
        // Desc: Initializes the profile manager.
        //-----------------------------------------------------------------------------
        public void Init()
        {
            // Create default profile
            if (profileCount == 0)
                CreateProfile(defaultProfileName);
        }

        //-----------------------------------------------------------------------------
        // Name: IsDefaultProfile() (Public Function)
        // Desc: Checks if the specified profile is the default profile.
        // Parm: profile - The query profile.
        // Rtrn: True if the specified profile is the default profile and false otherwise.
        //-----------------------------------------------------------------------------
        public bool IsDefaultProfile(ShortcutProfile profile)
        {
            return profile != null && profile.name == defaultProfileName;
        }

        //-----------------------------------------------------------------------------
        // Name: SetActiveProfile() (Public Function)
        // Desc: Sets the active profile to the profile with the specified name.
        // Parm: name - Profile name. If a profile with this name doesn't exist, the
        //              function has no effect.
        //-----------------------------------------------------------------------------
        public void SetActiveProfile(string name)
        {
            // Loop through each profile
            int count = profileCount;
            for (int i = 0; i < count; ++i)
            {
                // Match?
                if (mProfiles[i].name == name)
                {
                    // Set the matching profile as active
                    activeProfile = mProfiles[i];
                    #if UNITY_EDITOR
                    EditorUtility.SetDirty(RTInput.get);
                    #endif
                    return;
                }
            }
        }

        //-----------------------------------------------------------------------------
        // Name: CreateProfile() (Public Function)
        // Desc: Creates a new profile with the specified name. 
        // Parm: name - Profile name. If a profile with the same name already exists,
        //              the name will be modified slightly to make it unique.
        // Rtrn: The created profile or null if the name is invalid.
        //-----------------------------------------------------------------------------
        public ShortcutProfile CreateProfile(string name)
        {
            // Validate call
            if (string.IsNullOrEmpty(name))
                return null;

            // Generate a unique name
            CollectProfileNames(mNameBuffer);
            name = NameGenerator.GenerateUnique(name, mNameBuffer);

            // Create the profile and add it
            ShortcutProfile profile = new ShortcutProfile();
            profile.Init(name);
            mProfiles.Add(profile);

            // If this is the first created profile, mark it as the active profile
            if (activeProfile == null)
                activeProfile = mProfiles[0];

            #if UNITY_EDITOR
            EditorUtility.SetDirty(RTInput.get);
            #endif

            // Return profile
            return profile;
        }

        //-----------------------------------------------------------------------------
        // Name: RenameProfile() (Public Function)
        // Desc: Renames the specified profile. If the profile already uses the specified
        //       name or if it is the default profile, the function has no effect.
        // Parm: profile - The profile which will be renamed.
        //       newName - Profile name. If a profile with the same name already exists,
        //                 the name will be modified slightly to make it unique.
        //-----------------------------------------------------------------------------
        public void RenameProfile(ShortcutProfile profile, string newName)
        {
            // If the profile already has this name, don't do anything
            if (profile.name == newName)
                return;

            // Generate a unique name
            CollectProfileNames(mNameBuffer);
            newName = NameGenerator.GenerateUnique(newName, mNameBuffer);

            // Rename profile
            profile.name = newName;

            #if UNITY_EDITOR
            EditorUtility.SetDirty(RTInput.get);
            #endif
        }

        //-----------------------------------------------------------------------------
        // Name: DeleteProfile() (Public Function)
        // Desc: Deletes the specified profile.
        // Parm: profile - The profile to delete. If this is the default profile, the
        //                 function has no effect.
        // Rtrn: True if the profile was deleted and false otherwise.
        //-----------------------------------------------------------------------------
        public bool DeleteProfile(ShortcutProfile profile)
        {
            // Can't delete the default profile
            if (IsDefaultProfile(profile)) return false;

            // Find the index of the profile we have to delete
            int profileIndex = IndexOf(profile);
            if (profileIndex >= 0)
            {
                // Remove profile
                mProfiles.RemoveAt(profileIndex);

                // Validate and clip the active profile index
                if (mActiveProfileIndex >= mProfiles.Count)
                    mActiveProfileIndex = mProfiles.Count - 1;

                #if UNITY_EDITOR
                EditorUtility.SetDirty(RTInput.get);
                #endif

                // Deleted
                return true;
            }

            // Couldn't delete the profile
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: DeleteProfile() (Public Function)
        // Desc: Deletes the profile with the specified name.
        // Parm: name - The name of the profile to delete. If this is the name of the
        //              default profile, the function has no effect.
        // Rtrn: True if the profile was deleted and false otherwise.
        //-----------------------------------------------------------------------------
        public bool DeleteProfile(string name)
        {
            return DeleteProfile(FindProfile(name));
        }

        //-----------------------------------------------------------------------------
        // Name: FindProfile() (Public Function)
        // Desc: Finds and returns the profile with the specified name.
        // Parm: name - Name of profile to search for.
        // Rtrn: The profile with the specified name or null if no such profile exists.
        //-----------------------------------------------------------------------------
        public ShortcutProfile FindProfile(string name)
        {
            // Loop through each profile
            int count = profileCount;
            for (int i = 0; i < count; ++i)
            {
                // Match?
                if (mProfiles[i].name == name)
                    return mProfiles[i];
            }

            // Not found
            return null;
        }

        //-----------------------------------------------------------------------------
        // Name: IndexOf() (Public Function)
        // Desc: Returns the index of the specified index.
        // Parm: profile - The profile whose index will be returned.
        // Rtrn: The index of the specified profile.
        //-----------------------------------------------------------------------------
        public int IndexOf(ShortcutProfile profile)
        {
            return mProfiles.IndexOf(profile);
        }

        //-----------------------------------------------------------------------------
        // Name: ContainsProfile() (Public Function)
        // Desc: Checks if a profile with the specified name exists.
        // Parm: name - Name of profile to search for.
        // Rtrn: True if a profile with the specified name exists and false otherwise.
        //-----------------------------------------------------------------------------
        public bool ContainsProfile(string name)
        {
            // Loop through each profile
            int count = profileCount;
            for (int i = 0; i < count; ++i)
            {
                // Match?
                if (mProfiles[i].name == name)
                    return true;
            }

            // Not found
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: CollectProfileNames() (Public Function)
        // Desc: Collects all profile names and stores them in 'names'.
        // Parm: names - Returns the names of all profiles.
        //-----------------------------------------------------------------------------
        public void CollectProfileNames(List<string> names)
        {
            // Clear old content
            names.Clear();

            // Loop through each profile and store its name
            int count = profileCount;
            for (int i = 0; i < count; ++i)
                names.Add(mProfiles[i].name);
        }
        #endregion
    }
    #endregion
}
