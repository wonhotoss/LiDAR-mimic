using System;
using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: Profile (Public Abstract Class)
    // Desc: A profile wraps a collection of data/settings/etc and enables the quick
    //       changing of said data without having to manually specify each piece of
    //       information. A good example is shortcut profiles. It is up to the derived
    //       class and the clients to establish how this happens. This class simply
    //       provides storage for the most common profile properties and functionality.
    //-----------------------------------------------------------------------------
    [Serializable] public abstract class Profile
    {
        #region Private Fields
        [SerializeField] string mName = string.Empty;   // Profile name
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: name (Public Property)
        // Desc: Returns or sets the profile name. Only non-null and non-empty names
        //       are allowed.
        //-----------------------------------------------------------------------------
        public string name { get { return mName; } set { if (!string.IsNullOrEmpty(value)) mName = value; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Init() (Public Function)
        // Desc: Initializes the profile.
        // Parm: name - Profile name. Must be valid.
        //-----------------------------------------------------------------------------
        public void Init(string name)
        {
            // Assign name and validate it
            mName = name;
            if (string.IsNullOrEmpty(mName))
                RTG.Exception(nameof(Profile), "Init", "A profile must have a valid name.");

            // Call derived implementation
            OnInit();
        }
        #endregion

        #region Protected Virtual Functions
        //-----------------------------------------------------------------------------
        // Name: OnInit() (Protected Virtual Function)
        // Desc: Derived classes can implement this in order to allow the profile to
        //       perform custom initialization tasks.
        //-----------------------------------------------------------------------------
        protected virtual void OnInit() { }
        #endregion
    }
    #endregion
}
