using UnityEngine;

namespace RTGLite
{
    #region Public Interfaces
    //-----------------------------------------------------------------------------
    // Name: IRTObjectTransformGizmoTarget (Public Interface)
    // Desc: This interface can be implemented by a 'MonoBehaviour' and attached to
    //       game objects that are transformed by 'ObjectTransformGizmo'. It is useful
    //       for handling special requirements some objects might have. For example,
    //       some objects may require that they remain unaffected by the transform gizmo.
    //       Others, may require to customize the way in which they are transformed etc.
    //-----------------------------------------------------------------------------
    public interface IRTObjectTransformGizmoTarget
    {
        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: CanTransform() (Public Function)
        // Desc: Called by an 'ObjectTransformGizmo' before changing an object's transform
        //       in order to check if it is allowed to do so.
        // Parm: channel        - The gizmo drag channel. Specifies how the object's transform
        //                        will be affected (position, rotation or scale).
        //       transformGizmo - The transform gizmo which changes the object's transform.
        // Rtrn: True if the object can change its transform and false otherwise.
        //-----------------------------------------------------------------------------
        bool CanTransform(EGizmoDragChannel channel, ObjectTransformGizmo transformGizmo);

        //-----------------------------------------------------------------------------
        // Name: OnTransformed() (Public Function)
        // Desc: Called by an 'ObjectTransformGizmo' after changing an object's transform.
        // Parm: channel        - The gizmo drag channel. Specifies how the object's transform
        //                        has changed (position, rotation or scale).
        //       transformGizmo - The transform gizmo that changed the object's transform.
        //-----------------------------------------------------------------------------
        void OnTransformed(EGizmoDragChannel channel, ObjectTransformGizmo transformGizmo);
        
        //-----------------------------------------------------------------------------
        // Name: UpdateMoveOffset() (Public Function)
        // Desc: Called by an 'ObjectTransformGizmo' that wants to change the object position
        //       by applying a move offset. The function can be used to cancel movement along
        //       an axis. For example, if an object is not allowed to ever move along the Y axis,
        //       the function can set the move vector's Y component to 0 and return it back to
        //       the transform gizmo.
        // Parm: moveOffset     - The move offset to be applied.
        //       transformGizmo - The transform gizmo which changes the object's transform.
        // Rtrn: The updated move offset. If no changes are necessary, just return the original
        //       offset passed as input. Otherwise, the return value should reflect whatever is
        //       required for the object in question.
        //-----------------------------------------------------------------------------
        Vector3 UpdateMoveOffset(Vector3 moveOffset, ObjectTransformGizmo transformGizmo);
        #endregion
    }
    #endregion
}
