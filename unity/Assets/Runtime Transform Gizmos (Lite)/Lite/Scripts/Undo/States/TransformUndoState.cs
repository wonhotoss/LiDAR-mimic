using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: TransformUndoState (Public Class)
    // Desc: Implements a 'Transform' Undo/Redo state.
    //-----------------------------------------------------------------------------
    public class TransformUndoState : UndoObjectState
    {
        #region Private Fields
        Transform   mParent;        // Transform parent
        Vector3     mLocalPosition; // Local position
        Quaternion  mLocalRotation; // Local rotation
        Vector3     mLocalScale;    // Local scale
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------
        // Name: TransformUndoState() (Public Constructor)
        // Desc: Creates a 'TransformUndoState' for the specified transform.
        // Parm: transform - Target transform.
        //-----------------------------------------------------------------------------
        public TransformUndoState(Transform transform) : base(transform) {}
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Extract() (Public Function)
        // Desc: Extracts the state from its attached object.
        //-----------------------------------------------------------------------------
        public override void Extract()
        {
            // Extract state
            var transform   = target as Transform;
            mParent         = transform.parent;
            mLocalPosition  = transform.localPosition;
            mLocalRotation  = transform.localRotation;
            mLocalScale     = transform.localScale;
        }

        //-----------------------------------------------------------------------------
        // Name: Apply() (Public Function)
        // Desc: Applies the state to its attached object.
        //-----------------------------------------------------------------------------
        public override void Apply()
        {
            // Apply state
            var transform           = target as Transform;
            transform.parent        = mParent;
            transform.localPosition = mLocalPosition;
            transform.localRotation = mLocalRotation;
            transform.localScale    = mLocalScale;

            // Notify scene
            RTScene.get.OnObjectTransformChanged(transform.gameObject);
        }

        //-----------------------------------------------------------------------------
        // Name: Diff() (Public Function)
        // Desc: Checks if there is any difference between 'this' state and 'other'. The
        //       function assumes the 2 states have the same target object.
        // Parm: other - The other state.
        // Rtrn: True if the 2 states are different and false otherwise.
        //-----------------------------------------------------------------------------
        public override bool Diff(UndoObjectState other)
        {
            // Check type
            var s = other as TransformUndoState;
            if (s == null) return false;

            // Check state diff
            if (mParent                     != s.mParent ||
                mLocalPosition              != s.mLocalPosition ||
                mLocalRotation.eulerAngles  != s.mLocalRotation.eulerAngles ||
                mLocalScale                 != s.mLocalScale) return true;

            // No diff
            return false;
        }
        #endregion
    }
    #endregion
}
