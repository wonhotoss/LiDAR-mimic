using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: UndoObjectState (Public Abstract Class)
    // Desc: Wraps object state management used by the Undo/Redo system's 'Record###'
    //       family of functions. An object state is used to store the original state
    //       of an object before it is modified and diff it with the state of the
    //       object after it was modified. Undo record operations use object states
    //       to implement Undo/Redo for object state changes (e.g. object transform).
    //-----------------------------------------------------------------------------
    public abstract class UndoObjectState
    {
        #region Private Fields
        object mTarget; // The object the state applies to. Derived classes decide what this object is and how its state is stored.
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: target (Public Property)
        // Desc: Returns the object the state applies to.
        //-----------------------------------------------------------------------------
        public object target { get { return mTarget; } }
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------
        // Name: UndoObjectState() (Public Constructor)
        // Desc: Creates a state for the specified object. 
        // Parm: target - The object the state applies to. Must be passed down from the
        //                derived class constructor and must be a valid object reference.
        //-----------------------------------------------------------------------------
        public UndoObjectState(object target)
        {
            // Validate args
            if (target == null)
                RTG.Exception(nameof(UndoObjectState), nameof(UndoObjectState), "Undo object state target was null.");

            // Set target
            mTarget = target;
        }
        #endregion

        #region Public Virtual Functions
        //-----------------------------------------------------------------------------
        // Name: CloneState() (Public Virtual Function)
        // Desc: Clones the state.
        // Rtrn: The cloned state.
        //-----------------------------------------------------------------------------
        public virtual UndoObjectState CloneState()
        {
            return MemberwiseClone() as UndoObjectState;
        }
        #endregion

        #region Public Abstract Functions
        //-----------------------------------------------------------------------------
        // Name: Extract() (Public Abstract Function)
        // Desc: Extracts the state from its attached object.
        //-----------------------------------------------------------------------------
        public abstract void Extract();

        //-----------------------------------------------------------------------------
        // Name: Apply() (Public Abstract Function)
        // Desc: Applies the state to its attached object.
        //-----------------------------------------------------------------------------
        public abstract void Apply();

        //-----------------------------------------------------------------------------
        // Name: Diff() (Public Abstract Function)
        // Desc: Checks if there is any difference between 'this' state and 'other'. The
        //       function assumes the 2 states have the same target object.
        // Parm: other - The other state.
        // Rtrn: True if the 2 states are different and false otherwise.
        //-----------------------------------------------------------------------------
        public abstract bool Diff(UndoObjectState other);
        #endregion
    }
    #endregion
}
