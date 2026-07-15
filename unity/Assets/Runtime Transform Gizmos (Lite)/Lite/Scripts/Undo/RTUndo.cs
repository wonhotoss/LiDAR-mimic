using UnityEngine;
using System;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTUndo (Public Singleton Class)
    // Desc: Implements the Undo/Redo system.
    //-----------------------------------------------------------------------------
    public class RTUndo : MonoSingleton<RTUndo>
    {
        #region Public Delegates & Events
        //-----------------------------------------------------------------------------
        // Name: UndoRedoHandler() (Public Delegate)
        // Desc: Handler for the Undo/Redo event fired on Undo/Redo.
        // Parm: redo - True on Redo. False on Undo.
        //-----------------------------------------------------------------------------
        public delegate void    UndoRedoHandler(bool redo);
        public event            UndoRedoHandler undoRedo;
        #endregion

        #region Private Fields
        [SerializeField] int    mStackSize  = 50;                                   // Number of undo stack entries
        UndoGroup[]             mStack;                                             // The Undo stack
        int                     mStackTop   = -1;                                   // The top of the stack which points at the last group stored on the stack (i.e. mStackTop + 1 = number of groups on the stack).
        int                     mSP         = -1;                                   // Stack pointer used to implement the Undo/Redo mechanism. Points to the next group which can be undone.

        UndoGroup               mActiveGroup;                                       // The active group that receives undo operations
        ArrayStack<bool>        mEnabledStack = new ArrayStack<bool>();             // Allows nesting of enabled state changes

        bool                    mInsideUndoHandler;                                 // Are we inside the Undo handler?
        bool                    mInsideRedoHandler;                                 // Are we inside the Redo handler?

        // Cached input edges for the current frame. These are updated during
        // 'Internal_Update()' and consumed inside 'RegisterUndoOp()' in order to
        // make split decisions independent of the exact frame in which the
        // undo operation is registered.
        bool                    mMBWentDown;
        EMouseButtons           mMBWentDownMask;
        bool                    mKeyWentDown;
        ulong                   mInputFrameIndex;
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: undoRedoEnabled (Public Property)
        // Desc: Returns whether or not the Undo/Redo functionality is enabled.
        //-----------------------------------------------------------------------------
        public bool undoRedoEnabled     { get { return mEnabledStack.count == 0 || mEnabledStack.Peek(); } }

        //-----------------------------------------------------------------------------
        // Name: stackSize (Public Property)
        // Desc: Returns or sets the undo stack size. Changing the size has the effect
        //       of clearing the undo stack.
        //-----------------------------------------------------------------------------
        public int stackSize            { get { return mStackSize; } set { ClearUndo(); mStackSize = Mathf.Max(1, value); mStack = new UndoGroup[mStackSize]; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Internal_Update() (Public Function)
        // Desc: Called by the system to allow the undo/redo system to update itself.
        //-----------------------------------------------------------------------------
        public void Internal_Update()
        {
            // Cache input edge state for this frame. The cached values are later
            // consumed inside 'RegisterUndoOp' to decide whether an active group
            // must be split before recording a new undo operation.
            mMBWentDown        = RTInput.get.anyMBWentDown;
            mMBWentDownMask    = RTInput.get.mbWentDownMask;
            mKeyWentDown       = RTInput.get.anyNonModifierKeyWentDown;
            mInputFrameIndex   = RTFrame.index;
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_LateUpdate() (Public Function)
        // Desc: Called by the system to allow the undo/redo system to update itself
        //       during the late update phase.
        //-----------------------------------------------------------------------------
        public void Internal_LateUpdate()
        {
            // Do we have an active group?
            if (mActiveGroup != null)
            {
                // Check if we can commit this group
                bool canCommit = false;
                switch (mActiveGroup.inputBinding)
                {
                    case EUndoInputBinding.None:
                    case EUndoInputBinding.Keyboard:

                        canCommit = true;
                        break;

                    case EUndoInputBinding.Mouse:

                        // Commit only when originating mouse button(s) are no longer held
                        if ((RTInput.get.mbPressedMask & mActiveGroup.mouseButtonMask) == 0)
                            canCommit = true;
                        break;
                }
                
                // Commit active group
                if (canCommit)
                    CommitActiveGroup();
            }
        }

        //-----------------------------------------------------------------------------
        // Name: ClearUndo() (Public Function)
        // Desc: Clears the undo stack.
        //-----------------------------------------------------------------------------
        public void ClearUndo()
        {
            // Flush all undo groups
            for (int i = mStackTop; i >= 0; --i)
            {
                mStack[i].Flush();
                mStack[i] = null;
            }

            // Clear stack and data
            mStackTop       = -1;
            mSP             = -1;
            mActiveGroup    = null;
        }

        //-----------------------------------------------------------------------------
        // Name: Undo() (Public Function)
        // Desc: Undo the last group of operations.
        //-----------------------------------------------------------------------------
        public void Undo()
        {
            // No-op?
            if (mInsideUndoHandler ||
                mInsideRedoHandler || 
                mSP < 0) return;

            // Note: Ensure "No empty groups" invariant holds.
            Debug.Assert(mStack[mSP].operationCount != 0, "Empty group found on undo stack.");

            // Undo and move the pointer down
            var group = mStack[mSP];
            mStack[mSP].Undo();
            --mSP;

            // Handle group undo
            HandleGroupUndo(group);

            // Fire undo event
            mInsideUndoHandler = true;
            if (undoRedo != null) undoRedo(false);
            mInsideUndoHandler = false;
        }

        //-----------------------------------------------------------------------------
        // Name: Redo() (Public Function)
        // Desc: Redo the last group which was undone.
        //-----------------------------------------------------------------------------
        public void Redo()
        {
            // No-op?
            if (mInsideUndoHandler ||
                mInsideRedoHandler || 
                mSP >= mStackTop) return;

            // Move the pointer up and redo
            ++mSP;
            mStack[mSP].Redo();

            // Handle group redo
            HandleGroupRedo(mStack[mSP]);

            // Fire redo event
            mInsideRedoHandler = true;
            if (undoRedo != null) undoRedo(true);
            mInsideRedoHandler = false;
        }

        //-----------------------------------------------------------------------------
        // Name: Record() (Public Function)
        // Desc: Records the state of the specified Unity Object so that any subsequent
        //       changes may be undone. The function has no effect if Undo/Redo is disabled.
        // Parm: unityObject - The Unity Object whose state must be recorded.
        //-----------------------------------------------------------------------------
        public void Record(UnityEngine.Object unityObject)
        {
            // No-op?
            if (!unityObject)
                return;

            // Check object type
            if (unityObject is Component)
            {
                // Is this component a MonoBehaviour?
                if (unityObject is MonoBehaviour)
                {
                }
                else
                {
                    // Transform?
                    var transform = unityObject as Transform;
                    if (transform != null)
                       RegisterUndoOp(new RecordUndoOp(new TransformUndoState(transform)));
                }
            }
        }

        //-----------------------------------------------------------------------------
        // Name: PushEnabled() (Public Function)
        // Desc: Changes the enabled state of the Undo/Redo functionality to the specified
        //       value. Every call to 'PushEnabled' should be matched by a call to 'PopEnabled'.
        // Parm: enabled - The new enabled state of the Undo/Redo functionality.
        //-----------------------------------------------------------------------------
        public void PushEnabled(bool enabled)
        {
            mEnabledStack.Push(enabled);
        }

        //-----------------------------------------------------------------------------
        // Name: PopEnabled() (Public Function)
        // Desc: Restores the enabled state to what it was before the last call to 
        //       'PushEnabled'. Should be matched with a previous call to 'PushEnabled'.
        //-----------------------------------------------------------------------------
        public void PopEnabled()
        {
            // No-op?
            if (mEnabledStack.count == 0) return;

            // Pop
            mEnabledStack.Pop();
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: Awake() (Private Function)
        // Desc: Called by Unity to allow the object to initialize itself.
        //-----------------------------------------------------------------------------
        void Awake()
        {
            mStack = new UndoGroup[mStackSize];
        }

        //-----------------------------------------------------------------------------
        // Name: PushGroup() (Private Function)
        // Desc: Pushes a new undo group onto the stack and returns it. If the user has
        //       undone operations, redo history is discarded. If the stack is full,
        //       the bottom-most group is flushed to make room.
        // Rtrn: The newly pushed undo group.
        //-----------------------------------------------------------------------------
        UndoGroup PushGroup()
        {
            // 1. If we undid something, discard redo history
            if (mSP < mStackTop)
            {
                for (int i = mStackTop; i > mSP; --i)
                {
                    mStack[i].Flush();
                    mStack[i] = null;
                }

                // Sync stack top
                mStackTop = mSP;
            }

            // 2. If stack is full, remove bottom-most group
            if (mStackTop + 1 == mStackSize)
            {
                // Flush bottom
                mStack[0].Flush();

                // Shift all groups down by 1
                for (int i = 0; i < mStackSize - 1; ++i)
                    mStack[i] = mStack[i + 1];

                // Adjust pointers (logical shift)
                mStackTop--;
                mSP--;
            }

            // 3. Establish input binding based on interaction context
            var inputBinding = EUndoInputBinding.None;
            EMouseButtons mouseBtnMask = EMouseButtons.None;

            // Case 1: A new mouse interaction begins this frame
            if (mMBWentDown)
            {
                inputBinding = EUndoInputBinding.Mouse;
                mouseBtnMask = mMBWentDownMask;
            }
            // Case 2: A new keyboard interaction begins this frame
            else if (mKeyWentDown)
            {
                inputBinding = EUndoInputBinding.Keyboard;
            }
            // Case 3: We are inside an ongoing mouse interaction (drag continuation)
            else if (RTInput.get.mbPressedMask != EMouseButtons.None)
            {
                inputBinding = EUndoInputBinding.Mouse;
                mouseBtnMask = RTInput.get.mbPressedMask;
            }

            // If bound by mouse, get the mouse button mask
            if (inputBinding == EUndoInputBinding.Mouse)
            {
                // Prefer the exact button(s) that went down this frame. Otherwise, fall back
                // to the currently pressed button(s) (drag continuation case).
                mouseBtnMask = mMBWentDown ? mMBWentDownMask : RTInput.get.mbPressedMask;
            }

            // 4. Push new group
            UndoGroup group = new UndoGroup(RTFrame.index, inputBinding, mouseBtnMask);
            mStack[++mStackTop] = group;
            mSP = mStackTop;

            // Return new group
            return group;
        }

        //-----------------------------------------------------------------------------
        // Name: RemoveEmptyActiveGroup() (Private Function)
        // Desc: Removes the currently active group from the undo stack.
        //-----------------------------------------------------------------------------
        void RemoveEmptyActiveGroup()
        {
            // No active group?
            if (mActiveGroup == null)
                return;

            // Sanity: active group must always be the top-most group.
            Debug.Assert(mStackTop >= 0 && mStack[mStackTop] == mActiveGroup,
                "Active undo group is not aligned with stack top.");

            // Remove group from stack
            mStack[mStackTop] = null;

            // If the stack pointer targets the group we remove, move it down as well.
            if (mSP == mStackTop)
                --mSP;

            // Move stack top down
            --mStackTop;

            // Clear active group
            mActiveGroup = null;
        }

        //-----------------------------------------------------------------------------
        // Name: RegisterUndoOp() (Private Function)
        // Desc: Registers an undo operation with the current active group. If a new user
        //       input edge occurs (mouse button went down / non-modifier key went down),
        //       the current group may be split and committed first, then a new group is
        //       created and bound to the originating input.
        // Note: The split decision is binding-aware. A mouse-born group only splits
        //       on a new mouse button down edge, and a keyboard-born group only
        //       splits on a new non-modifier key down edge. This prevents unrelated
        //       input (e.g. pressing a key while a mouse-born group exists) from
        //       prematurely committing and fragmenting the undo history.
        //-----------------------------------------------------------------------------
        void RegisterUndoOp(IUndoOperation undoOp)
        {
            // No-op?
            if (!undoRedoEnabled)
                return;

            // If we have an active group already, check if we need to split by
            // cached input edge state gathered during 'Internal_Update'.
            if (mActiveGroup != null && (mMBWentDown || mKeyWentDown))
            {
                bool shouldSplit = false;
                switch (mActiveGroup.inputBinding)
                {
                    case EUndoInputBinding.Mouse:

                        // Split only if a different mouse button stream begins.
                        shouldSplit = (mMBWentDownMask & mActiveGroup.mouseButtonMask) == 0;
                        break;

                    case EUndoInputBinding.Keyboard:

                        // Keyboard groups split on new non-modifier key down edges,
                        // but never on the same frame the group was created.
                        shouldSplit = mKeyWentDown &&
                                      (mInputFrameIndex != mActiveGroup.createdFrameIndex);
                        break;

                    default:

                        // Groups without binding split on any new input edge.
                        shouldSplit = (mMBWentDown || mKeyWentDown);
                        break;
                }

                // Split?
                if (shouldSplit)
                    CommitActiveGroup();
            }

            // Create a new group if necessary
            if (mActiveGroup == null)
                mActiveGroup = PushGroup();

            // Register undo op with the active group
            mActiveGroup.AddOperation(undoOp);
        }

        //-----------------------------------------------------------------------------
        // Name: CommitActiveGroup() (Private Function)
        // Desc: Commits the current active undo group. If the group contains no recorded
        //       operations, it is removed from the undo stack to avoid polluting history.
        //       After commit, the active group pointer and its binding state are cleared.
        //-----------------------------------------------------------------------------
        void CommitActiveGroup()
        {
            // No-op?
            if (mActiveGroup == null)
                return;

            // Commit group. Remove from stack if it can't be committed.
            if (!mActiveGroup.Commit())
                RemoveEmptyActiveGroup();

            // Group no longer active
            mActiveGroup = null;
        }

        //-----------------------------------------------------------------------------
        // Name: HandleGroupUndo() (Private Function)
        // Desc: Performs semantic cleanup after a group has been undone.
        // Parm: group - The undo group which was just undone.
        //-----------------------------------------------------------------------------
        void HandleGroupUndo(UndoGroup group)
        {
        }

        //-----------------------------------------------------------------------------
        // Name: HandleGroupRedo() (Private Function)
        // Desc: Performs semantic processing after a group has been redone.
        // Parm: group - The undo group which was just redone.
        //-----------------------------------------------------------------------------
        void HandleGroupRedo(UndoGroup group)
        {
        }
        #endregion
    }
    #endregion
}