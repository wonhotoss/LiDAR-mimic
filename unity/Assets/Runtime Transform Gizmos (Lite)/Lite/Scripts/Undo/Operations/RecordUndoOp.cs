using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RecordUndoOp (Public Class)
    // Desc: Represents an undo operation that captures an object's state before
    //       and after a change.
    //-----------------------------------------------------------------------------
    public class RecordUndoOp : IUndoOperation
    {
        #region Private Fields
        UndoObjectState mStateBefore;   // Object state before it was modified
        UndoObjectState mStateAfter;    // Object state after it was modified
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: target (Public Property)
        // Desc: Returns the record operation target object.
        //-----------------------------------------------------------------------------
        public object target { get { return mStateBefore.target; } }
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------
        // Name: RecordUndoOp() (Public Constructor)
        // Desc: Creates a record undo operation from the specified state.
        // Parm: stateBefore - Object state before it was modified. Must target the same
        //                     object as 'stateAfter'.
        //-----------------------------------------------------------------------------
        public RecordUndoOp(UndoObjectState stateBefore)
        {
            // Validate args
            if (stateBefore == null)
                RTG.Exception(nameof(RecordUndoOp), nameof(RecordUndoOp), "The Undo object state must be valid.");

            // Store state and clone
            mStateBefore    = stateBefore;
            mStateBefore.Extract();
            mStateAfter     = stateBefore.CloneState();
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: DiffStates() (Public Function)
        // Desc: Preforms a diff on the before and after states.
        // Rtrn: True if the 2 states are different and false otherwise.
        //-----------------------------------------------------------------------------
        public bool DiffStates()
        {
            // Extract the after state and diff
            mStateAfter.Extract();
            return mStateBefore.Diff(mStateAfter);
        }

        //-----------------------------------------------------------------------------
        // Name: Undo() (Public Function)
        // Desc: Revert the operation's effects.
        //-----------------------------------------------------------------------------
        public void Undo()
        {
            mStateBefore.Apply();
        }

        //-----------------------------------------------------------------------------
        // Name: Redo() (Public Function)
        // Desc: Restore the operation's effects.
        //-----------------------------------------------------------------------------
        public void Redo()
        {
            mStateAfter.Apply();
        }
        #endregion

        #region Public Virtual Functions
        //-----------------------------------------------------------------------------
        // Name: Flush() (Public Virtual Function)
        // Desc: Called when the operation is about to be removed from the undo stack
        //       because it is no longer needed.
        //-----------------------------------------------------------------------------
        public virtual void Flush(){}

        //-----------------------------------------------------------------------------
        // Name: ShouldBePruned() (Public Virtual Function)
        // Desc: Determines whether this record operation should be removed from its
        //       undo group during commit. The default behavior is to prune the
        //       operation if no differences are detected between the before and after
        //       states. However, certain states (e.g. selection) may depend on other
        //       operations inside the same group and must be preserved.
        // Parm: group - The undo group which owns this operation.
        // Rtrn: True if the operation should be pruned (removed) from the group
        //       and false otherwise.
        //-----------------------------------------------------------------------------
        public virtual bool ShouldBePruned(UndoGroup group)
        {
            // If the states differ, this operation represents a real change.
            // It must be kept.
            if (DiffStates())
                return false;

            // Safe to prune
            return true;
        }
        #endregion
    }
    #endregion
}