using UnityEngine;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GizmoTransform (Public Class)
    // Desc: A gizmo transform stores position, rotation and scale data for gizmos
    //       and gizmo handles.
    // Note: When a 'GizmoTransform' is attached to a gizmo HANDLE, the local scale
    //       has a special meaning and is used to store the gizmo style's scale. This
    //       allows us to have a scale value per gizmo style and also be able to scale
    //       further by changing the scale of the gizmo that owns the handle. Moral of
    //       the story: a handle's local scale and scale values are only touched by the
    //       system.
    //-----------------------------------------------------------------------------
    public class GizmoTransform
    {
        #region Private Fields
        Vector3     mPosition       = Vector3.zero;             // Position
        Quaternion  mRotation       = Quaternion.identity;      // Rotation
        float       mScale          = 1.0f;                     // Scale (scalar to make raycasting simple with some shapes like cylinders for example)

        Vector3     mLocalPosition  = Vector3.zero;             // Local position
        Quaternion  mLocalRotation  = Quaternion.identity;      // Local rotation
        float       mLocalScale     = 1.0f;                     // Local scale

        Vector3     mRight          = Vector3.right;            // Right axis
        Vector3     mUp             = Vector3.up;               // Up axis
        Vector3     mForward        = Vector3.forward;          // Forward axis

        GizmoTransform        mParent;                                  // Parent transform
        List<GizmoTransform>  mChildren = new List<GizmoTransform>();   // Children
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: position (Public Property)
        // Desc: Returns or sets the position.
        //-----------------------------------------------------------------------------
        public Vector3      position        
        { 
            get { return mPosition; } 
            set 
            {
                // Set position
                mPosition = value; 

                // Update local position
                if (mParent == null) mLocalPosition = mPosition;
                else mLocalPosition = (Quaternion.Inverse(mParent.mRotation) * (mPosition - mParent.mPosition)) / mParent.mScale;

                // Propagate change
                PropagatePositionChange();
            } 
        }

        //-----------------------------------------------------------------------------
        // Name: rotation (Public Property)
        // Desc: Returns or sets the rotation.
        //-----------------------------------------------------------------------------
        public Quaternion   rotation        
        { 
            get { return mRotation; } 
            set 
            { 
                // Set rotation
                mRotation = value; 

                // Update local rotation
                if (mParent == null) mLocalRotation = mRotation;
                else mLocalRotation = Quaternion.Normalize(Quaternion.Inverse(mParent.mRotation) * mRotation);

                // Update axes
                UpdateAxes();

                // Propagate change
                PropagateRotationChange();
            } 
        }

        //-----------------------------------------------------------------------------
        // Name: scale (Public Property)
        // Desc: Returns or sets the scale.
        //-----------------------------------------------------------------------------
        public float      scale           
        { 
            get { return mScale; } 
            set 
            { 
                // Set scale
                mScale = value;

                // Update local scale
                if (mParent == null) mLocalScale = mScale;
                else mLocalScale = mScale / mParent.mScale;

                // Propagate change
                PropagateScaleChange();
            } 
        }

        //-----------------------------------------------------------------------------
        // Name: localPosition (Public Property)
        // Desc: Returns or sets the local position.
        //-----------------------------------------------------------------------------
        public Vector3      localPosition   
        { 
            get { return mLocalPosition; } 
            set 
            { 
                // Set local position
                mLocalPosition = value; 

                // Update position
                if (mParent == null) mPosition = mLocalPosition;
                else mPosition = mParent.mRotation * (mLocalPosition * mParent.mScale) + mParent.mPosition;

                // Propagate change
                PropagatePositionChange();
            } 
        }
        
        //-----------------------------------------------------------------------------
        // Name: localRotation (Public Property)
        // Desc: Returns or sets the local rotation.
        //-----------------------------------------------------------------------------
        public Quaternion   localRotation   
        { 
            get { return mLocalRotation; } 
            set 
            { 
                // Set local rotation
                mLocalRotation = value; 

                // Update rotation
                if (mParent == null) mRotation = mLocalRotation;
                else mRotation = Quaternion.Normalize(mParent.mRotation * mLocalRotation);

                // Update axes
                UpdateAxes();

                // Propagate change
                PropagateRotationChange();
            } 
        }

        //-----------------------------------------------------------------------------
        // Name: localScale (Public Property)
        // Desc: Returns or sets the local scale.
        //-----------------------------------------------------------------------------
        public float      localScale      
        { 
            get { return mLocalScale; } 
            set 
            { 
                // Set local scale
                mLocalScale = value;

                // Update scale
                if (mParent == null) mScale = mLocalScale;
                else mScale = mLocalScale * mParent.scale;

                // Propagate change
                PropagateScaleChange();
            } 
        }
        
        //-----------------------------------------------------------------------------
        // Name: parent (Public Property)
        // Desc: Returns or sets the transform parent.
        //-----------------------------------------------------------------------------
        public GizmoTransform parent      
        { 
            get { return mParent; } 
            set 
            {
                // No-op?
                if (mParent == value) return;

                // Remove the child from the old parent
                if (mParent != null)
                    mParent.mChildren.Remove(this);

                // Set the new parent
                mParent = value;
                if (mParent != null) mParent.mChildren.Add(this);
                OnParentChanged();
            } 
        }

        //-----------------------------------------------------------------------------
        // Name: right (Public Property)
        // Desc: Returns the right axis.
        //-----------------------------------------------------------------------------
        public Vector3 right    { get { return mRight; } }

        //-----------------------------------------------------------------------------
        // Name: up (Public Property)
        // Desc: Returns the up axis.
        //-----------------------------------------------------------------------------
        public Vector3 up       { get { return mUp; } }

        //-----------------------------------------------------------------------------
        // Name: forward (Public Property)
        // Desc: Returns the forward axis.
        //-----------------------------------------------------------------------------
        public Vector3 forward  { get { return mForward; } }

        //-----------------------------------------------------------------------------
        // Name: childCount (Public Property)
        // Desc: Returns the number of child transforms.
        //-----------------------------------------------------------------------------
        public int childCount { get { return mChildren.Count; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: GetAxis() (Public Function)
        // Desc: Returns the transform axis with the specified index.
        // Parm: index - Axis index: (0 = X, 1 = Y, 2 = Z).
        // Rtrn: The transform axis with the specified index.
        //-----------------------------------------------------------------------------
        public Vector3 GetAxis(int index)
        {
            switch (index)
            {
                case 0:     return right;
                case 1:     return up;
                case 2:     return forward;
                default:    return Vector3.zero;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: CollectAxes() (Public Function)
        // Desc: Collects the transform's local axes and stores them inside 'axes'.
        // Parm: axes - Returns the transform's local axes. Must have a size of at least 3.
        //-----------------------------------------------------------------------------
        public void CollectAxes(Vector3[] axes)
        {
            axes[0] = right;
            axes[1] = up;
            axes[2] = forward;
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: OnParentChanged() (Private Function)
        // Desc: Called when the transform's parent changes.
        //-----------------------------------------------------------------------------
        void OnParentChanged()
        {
            // Update local transform data
            if (mParent == null)
            {
                // When no parent is available, the local transform is the same as the absolute transform
                mLocalPosition  = mPosition;
                mLocalRotation  = mRotation;
                mLocalScale     = mScale;
            }
            else
            {
                // Calculate transform relative to parent
                mLocalPosition      = (Quaternion.Inverse(mParent.mRotation) * (mPosition - mParent.mPosition)) / mParent.mScale;
                mLocalRotation      = Quaternion.Normalize(Quaternion.Inverse(mParent.mRotation) * mRotation);
                mLocalScale         = mScale / mParent.mScale;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: PropagatePositionChange() (Private Function)
        // Desc: When the position of the transform changes, this function is called in
        //       order to update the child transforms.
        //-----------------------------------------------------------------------------
        void PropagatePositionChange()
        {
            // Loop through each child
            int count = childCount;
            for (int i = 0; i < childCount; ++i)
            {
                // Update child position
                var child = mChildren[i];
                child.mPosition = mRotation * (child.mLocalPosition * mScale) + mPosition;
                child.PropagatePositionChange();
            }
        }

        //-----------------------------------------------------------------------------
        // Name: PropagateRotationChange() (Private Function)
        // Desc: When the rotation of the transform changes, this function is called in
        //       order to update the child transforms.
        //-----------------------------------------------------------------------------
        void PropagateRotationChange()
        {
            // Loop through each child
            int count = childCount;
            for (int i = 0; i < childCount; ++i)
            {
                // Update child position and rotation
                var child = mChildren[i];
                child.mPosition = mRotation * (child.mLocalPosition * mScale) + mPosition;
                child.PropagatePositionChange();

                child.mRotation = Quaternion.Normalize(mRotation * child.mLocalRotation);
                child.UpdateAxes();
                child.PropagateRotationChange();
            }
        }

        //-----------------------------------------------------------------------------
        // Name: PropagateScaleChange() (Private Function)
        // Desc: When the scale of the transform changes, this function is called in
        //       order to update the child transforms.
        //-----------------------------------------------------------------------------
        void PropagateScaleChange()
        {
            // Loop through each child
            int count = childCount;
            for (int i = 0; i < childCount; ++i)
            {
                // Update child position and scale
                var child = mChildren[i];
                child.mPosition = mRotation * (child.mLocalPosition * mScale) + mPosition;
                child.PropagatePositionChange();

                child.mScale    = child.mLocalScale * mScale;
                child.PropagateScaleChange();
            }
        }

        //-----------------------------------------------------------------------------
        // Name: UpdateAxes() (Private Function)
        // Desc: Updates the transform's axes.
        //-----------------------------------------------------------------------------
        void UpdateAxes()
        {
            mRight      = mRotation * Vector3.right;
            mUp         = mRotation * Vector3.up;
            mForward    = mRotation * Vector3.forward;
        }
        #endregion
    }
    #endregion
}
