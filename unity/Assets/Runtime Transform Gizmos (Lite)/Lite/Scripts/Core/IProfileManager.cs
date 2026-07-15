using System.Collections.Generic;

namespace RTGLite
{
    #region Public Interfaces
    //-----------------------------------------------------------------------------
    // Name: IProfileManager (Public Interface)
    // Desc: Interface implemented by all profile manager types. It provides access
    //       to a collection of profiles and exposes methods that allow clients to
    //       create, remove, and query profiles.
    // Parm: T - Profile type. Must derive from 'Profile'.
    //-----------------------------------------------------------------------------
    public interface IProfileManager<T> where T : Profile
    {
        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: profileCount (Public Property)
        // Desc: Returns the number of profiles.
        //-----------------------------------------------------------------------------
        int profileCount { get; }

        //-----------------------------------------------------------------------------
        // Name: activeProfile (Public Property)
        // Desc: Returns or sets the active profile. Null profiles are not allowed.
        //-----------------------------------------------------------------------------
        T activeProfile { get; set; }

        //-----------------------------------------------------------------------------
        // Name: isDefaultProfileActive (Public Property)
        // Desc: Returns true if the default profile is the active profile.
        //-----------------------------------------------------------------------------
        bool isDefaultProfileActive { get; }

        //-----------------------------------------------------------------------------
        // Name: this[int index] (Public Property)
        // Desc: Indexer which returns the profile with the specified index.
        //-----------------------------------------------------------------------------
        T this[int index] { get; }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: CreateProfile() (Public Function)
        // Desc: Creates a new profile with the specified name.
        // Parm: name - Profile name. If a profile with the same name already exists,
        //              the name will be modified slightly to make it unique.
        // Rtrn: The created profile or null if the name is invalid.
        //-----------------------------------------------------------------------------
        T CreateProfile(string name);

        //-----------------------------------------------------------------------------
        // Name: RenameProfile() (Public Function)
        // Desc: Renames the specified profile.
        // Parm: profile - The profile to rename.
        //       newName - The new profile name.
        //-----------------------------------------------------------------------------
        void RenameProfile(T profile, string newName);

        //-----------------------------------------------------------------------------
        // Name: DeleteProfile() (Public Function)
        // Desc: Deletes the specified profile.
        // Parm: profile - The profile to delete.
        // Rtrn: True if the profile was deleted and false otherwise.
        //-----------------------------------------------------------------------------
        bool DeleteProfile(T profile);

        //-----------------------------------------------------------------------------
        // Name: DeleteProfile() (Public Function)
        // Desc: Deletes the profile with the specified name.
        // Parm: name - The name of the profile to delete.
        // Rtrn: True if the profile was deleted and false otherwise.
        //-----------------------------------------------------------------------------
        bool DeleteProfile(string name);

        //-----------------------------------------------------------------------------
        // Name: FindProfile() (Public Function)
        // Desc: Finds and returns the profile with the specified name.
        // Parm: name - Name of the profile to search for.
        // Rtrn: The profile with the specified name or null if no such profile exists.
        //-----------------------------------------------------------------------------
        T FindProfile(string name);

        //-----------------------------------------------------------------------------
        // Name: IndexOf() (Public Function)
        // Desc: Returns the index of the specified profile.
        // Parm: profile - The profile whose index will be returned.
        // Rtrn: The index of the specified profile.
        //-----------------------------------------------------------------------------
        int IndexOf(T profile);

        //-----------------------------------------------------------------------------
        // Name: ContainsProfile() (Public Function)
        // Desc: Checks if a profile with the specified name exists.
        // Parm: name - Name of the profile to search for.
        // Rtrn: True if a profile with the specified name exists and false otherwise.
        //-----------------------------------------------------------------------------
        bool ContainsProfile(string name);

        //-----------------------------------------------------------------------------
        // Name: SetActiveProfile() (Public Function)
        // Desc: Sets the active profile to the profile with the specified name.
        // Parm: name - The name of the profile to activate.
        //-----------------------------------------------------------------------------
        void SetActiveProfile(string name);

        //-----------------------------------------------------------------------------
        // Name: CollectProfileNames() (Public Function)
        // Desc: Collects all profile names and stores them in 'names'.
        // Parm: names - Returns the names of all profiles.
        //-----------------------------------------------------------------------------
        void CollectProfileNames(List<string> names);

        //-----------------------------------------------------------------------------
        // Name: IsDefaultProfile() (Public Function)
        // Desc: Checks if the specified profile is the default profile.
        // Parm: profile - The profile to check.
        // Rtrn: True if the specified profile is the default profile and false otherwise.
        //-----------------------------------------------------------------------------
        bool IsDefaultProfile(T profile);
        #endregion
    }
    #endregion
}
