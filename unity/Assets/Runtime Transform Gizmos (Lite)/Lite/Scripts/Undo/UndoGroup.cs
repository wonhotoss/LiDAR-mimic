using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EUndoInputBinding (Public Enum)
    // Desc: Describes what kind of input created and currently owns the active group.
    //-----------------------------------------------------------------------------
    public enum EUndoInputBinding
    {
        None = 0,       // No binding (group should commit at end-of-frame)
        Mouse,          // Bound to a mouse button (drag semantics)
        Keyboard        // Bound to a key press (single command semantics)
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: UndoGroup (Public Class)
    // Desc: Represents an undo group. An undo group is a collection of operations
    //       which should be treated as a unit. All operations belonging to the same
    //       group are undone/redone at the same time.
    //-----------------------------------------------------------------------------
    public class UndoGroup
    {
        #region Private Fields
        Dictionary<object, RecordUndoOp>    mObjectToRecordOpMap    = new Dictionary<object, RecordUndoOp>();   // Maps an object to its record operation. Helps avoid having more than one record op for a single object.
        List<IUndoOperation>                mOperations             = new List<IUndoOperation>();               // Operations belonging to this group
        
        ulong                               mCreatedFrameIndex;     // Frame index when this group was created
        EUndoInputBinding                   mInputBinding;          // Input binding type for this group
        EMouseButtons                       mMouseButtonMask;       // Mouse button mask captured when the group was created (valid only if mouse-bound)
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: createdFrameIndex (Public Property)
        // Desc: Returns the 64-bit frame index at which this group was created.
        //-----------------------------------------------------------------------------
        public ulong createdFrameIndex { get { return mCreatedFrameIndex; } }

        //-----------------------------------------------------------------------------
        // Name: inputBinding (Public Property)
        // Desc: Returns the input binding type for this group. The binding describes
        //       what kind of user input created the group (None, Mouse, Keyboard).
        //-----------------------------------------------------------------------------
        public EUndoInputBinding inputBinding { get { return mInputBinding; } }

        //-----------------------------------------------------------------------------
        // Name: mouseButtonMask (Public Property)
        // Desc: Returns the mouse button bitmask captured when the group was created.
        //       This is only meaningful when 'inputBinding' is Mouse.
        //-----------------------------------------------------------------------------
        public EMouseButtons mouseButtonMask { get { return mMouseButtonMask; } }

        //-----------------------------------------------------------------------------
        // Name: operationCount (Public Property)
        // Desc: Returns the number of undo operations stored in this group.
        //-----------------------------------------------------------------------------
        public int operationCount { get { return mOperations.Count; } }

        //-----------------------------------------------------------------------------
        // Name: operations (Public Property)
        // Desc: Returns a read-only view of the undo operations stored in this group.
        //-----------------------------------------------------------------------------
        public IReadOnlyList<IUndoOperation> operations { get { return mOperations; } }
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------
        // Name: UndoGroup() (Public Constructor)
        // Desc: Creates a new undo group with the specified metadata.
        // Parm: createdFrameIndex - Frame index when the group was created.
        //       inputBinding      - Input binding type for this group.
        //       mouseButtonMask   - Mouse button mask captured at group creation time.
        //                           Only meaningful when inputBinding is Mouse.
        //-----------------------------------------------------------------------------
        public UndoGroup(ulong createdFrameIndex, EUndoInputBinding inputBinding, EMouseButtons mouseButtonMask)
        {
            mCreatedFrameIndex = createdFrameIndex;
            mInputBinding      = inputBinding;
            mMouseButtonMask   = mouseButtonMask;
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Undo() (Public Function)
        // Desc: Reverts the effects of all operations in the group.
        //-----------------------------------------------------------------------------
        public void Undo()
        {
            int count = mOperations.Count;
            for (int i = count - 1; i >= 0; --i)
                mOperations[i].Undo();
        }

        //-----------------------------------------------------------------------------
        // Name: Redo() (Public Function)
        // Desc: Restores the effects of all operations in the group.
        //-----------------------------------------------------------------------------
        public void Redo()
        {
            int count = mOperations.Count;
            for (int i = 0; i < count; ++i)
                mOperations[i].Redo();
        }

        //-----------------------------------------------------------------------------
        // Name: Flush() (Public Function)
        // Desc: Called when the group is about to be removed from the undo stack
        //       because it is no longer needed. This has the effect of flushing all 
        //       operations assigned to the group.
        //-----------------------------------------------------------------------------
        public void Flush()
        {
            int count = mOperations.Count;
            for (int i = 0; i < count; ++i)
                mOperations[i].Flush();
        }

        //-----------------------------------------------------------------------------
        // Name: AddOperation() (Public Function)
        // Desc: Adds the specified operation to the group. The function doesn't validate
        //       the operation. It assumes it points to a valid instance and that it hasn't
        //       already been assigned to the group.
        // Parm: op - The operation to add to the group.
        //-----------------------------------------------------------------------------
        public void AddOperation(IUndoOperation op)
        {
            // If this is a record op, check if we already have a record op for this object
            RecordUndoOp recOp = op as RecordUndoOp;
            if (recOp != null)
            {
                // If we are already recording this object, exit
                if (mObjectToRecordOpMap.ContainsKey(recOp.target))
                    return;

                // Store this object for next time
                mObjectToRecordOpMap.Add(recOp.target, recOp);
            }

            // Add operation
            mOperations.Add(op);
        }

        //-----------------------------------------------------------------------------
        // Name: Commit() (Public Function)
        // Desc: Called before the undo group is pushed onto the undo stack in order to
        //       perform a post-process step on the undo operations. For example, record
        //       operations whose before and after states register no diffs, are removed.
        // Rtrn: True if the undo group contains at least one undo operation and false
        //       otherwise.
        //-----------------------------------------------------------------------------
        public bool Commit()
        {
            // Loop through each operation
            for (int i = 0; i < mOperations.Count; )
            {
                // Is this a record op?
                RecordUndoOp recordOp = mOperations[i] as RecordUndoOp;
                if (recordOp != null)
                {
                    // Prune if necessary?
                    if (recordOp.ShouldBePruned(this))
                    {
                        mOperations.RemoveAt(i);
                        continue;
                    }
                }

                // Next op
                ++i;
            }

            // Do we have any operations left?
            return mOperations.Count != 0;
        }
        #endregion
    }
    #endregion
}