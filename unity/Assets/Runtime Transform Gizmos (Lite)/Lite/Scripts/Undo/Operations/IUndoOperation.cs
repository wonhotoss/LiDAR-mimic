using UnityEngine;

namespace RTGLite
{
    #region Public Interfaces
    //-----------------------------------------------------------------------------
    // Name: IUndoOperation (Public Interface)
    // Desc: Interface which must be implemented by all undo operations. An undo
    //       operation must be able to Undo/Redo itself.
    //-----------------------------------------------------------------------------
    public interface IUndoOperation
    {
        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Undo() (Public Function)
        // Desc: Reverts the operation's effects.
        //-----------------------------------------------------------------------------
        void Undo();

        //-----------------------------------------------------------------------------
        // Name: Redo() (Public Function)
        // Desc: Restores the operation's effects.
        //-----------------------------------------------------------------------------
        void Redo();

        //-----------------------------------------------------------------------------
        // Name: Flush() (Public Function)
        // Desc: Called when the operation is about to be removed from the undo stack
        //       because it is no longer needed.
        //-----------------------------------------------------------------------------
        void Flush();
        #endregion
    }
    #endregion
}
