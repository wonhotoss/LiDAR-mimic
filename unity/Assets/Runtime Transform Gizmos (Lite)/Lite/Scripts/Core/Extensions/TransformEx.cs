using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: TransformEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'Transform' class.
    //-----------------------------------------------------------------------------
    public static class TransformEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: GetAxis() (Public Extension)
        // Desc: Returns the transform axis with the specified index.
        // Parm: index - Axis index: (0 = X, 1 = Y, 2 = Z).
        // Rtrn: The transform axis with the specified index.
        //-----------------------------------------------------------------------------
        public static Vector3 GetAxis(this Transform transform, int index)
        {
            switch (index)
            {
                case 0:     return transform.right;
                case 1:     return transform.up;
                case 2:     return transform.forward;
                default:    return Vector3.zero;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: RotateAroundPivot() (Public Extension)
        // Desc: Rotates the transform around the specified pivot. This function affects
        //       the transform's position and rotation.
        // Parm: rotation - Rotation to apply.
        //       pivot    - Pivot to rotate around.
        //-----------------------------------------------------------------------------
        public static void RotateAroundPivot(this Transform transform, Quaternion rotation, Vector3 pivot)
        {
            Vector3 v = transform.position - pivot;                 // Position anchor
            transform.rotation = rotation * transform.rotation;     // Apply rotation (this changes the rotation)
            v = rotation * v;                                       // Rotate anchor
            transform.position = pivot + v;                         // Rotate position
        }

        //-----------------------------------------------------------------------------
        // Name: SetScale() (Public Extension)
        // Desc: Sets the object's global/lossy scale.
        // Parm: scale - The object's global/lossy scale.
        //-----------------------------------------------------------------------------
        public static void SetScale(this Transform transform, Vector3 scale)
        {
            // If we don't have a parent set the local scale
            Transform parent = transform.parent;
            if (parent == null) transform.localScale = scale;
            else
            {
                // Otherwise, update the local scale based on the relationship between the desired scale and its parent scale
                Vector3 parentScale = parent.lossyScale;
                transform.localScale = new Vector3(
                    scale.x / parentScale.x,
                    scale.y / parentScale.y,
                    scale.z / parentScale.z);
            }
        }
    
        //-----------------------------------------------------------------------------
        // Name: AlignAxis() (Public Extension)
        // Desc: Aligns the specified transform axis to 'destAxis'.
        // Parm: axis      - Index of the axis to align: (0 = X, 1 = Y, 2 = Z).
        //       destAxis  - Destination axis.
        // Rtrn: The rotation that had to be applied to achieve the alignment.
        //-----------------------------------------------------------------------------
        public static Quaternion AlignAxis(this Transform transform, int axis, Vector3 destAxis)
        {
            // Get the axis that has to be aligned
            Vector3 vAxis              = transform.right;
            if (axis == 1) vAxis       = transform.up;
            else if (axis == 2) vAxis  = transform.forward;

            // Already aligned?
            float dot = Vector3.Dot(vAxis, destAxis);
            if (1.0f - dot < 1e-6f) return Quaternion.identity;

            // Aligned in reverse?
            if (dot + 1.0f < 1e-6f)
            {
                // If the axis is aligned to the destination axis in reverse,
                // we can pick one of the other transform axes as a rotation
                // axis and rotate 180 degrees around it.
                Vector3 rotationAxis                = transform.forward;
                if (axis == 0) rotationAxis         = transform.up;
                else if (axis == 2) rotationAxis    = transform.right;

                // Rotate 180 degrees around the rotation axis and return rotation
                transform.Rotate(rotationAxis, 180.0f, Space.World);
                return Quaternion.AngleAxis(180.0f, rotationAxis);
            }
            else
            {
                // Calculate the rotation axis and rotation angle
                Vector3 rotationAxis    = Vector3.Cross(vAxis, destAxis).normalized;
                float rotationAngle     = Vector3.SignedAngle(vAxis, destAxis, rotationAxis);

                // Rotate and return rotation
                transform.Rotate(rotationAxis, rotationAngle, Space.World);
                return Quaternion.AngleAxis(rotationAngle, rotationAxis);
            }
        }
        #endregion
    }
    #endregion
}