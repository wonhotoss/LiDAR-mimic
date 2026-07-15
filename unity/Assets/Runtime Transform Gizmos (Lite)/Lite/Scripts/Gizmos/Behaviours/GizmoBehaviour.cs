using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GizmoBehaviour (Public Abstract Class)
    // Desc: Implements common functionality for a gizmo behaviour. A gizmo behaviour
    //       can be used to listen to gizmo events such as drag updates and allows
    //       the gizmo to be used for different purposes. For example a 'MoveGizmo'
    //       could have an 'ObjectTransformGizmo' behaviour attached to it in order
    //       to move objects around. Another behaviour might be used to move vertex
    //       positions inside a mesh and so on. So, the main idea behind a behaviour
    //       is to be able to consume the drag data coming in from the gizmo.
    //-----------------------------------------------------------------------------
    public abstract class GizmoBehaviour
    {
        #region Private Fields
        Gizmo mGizmo;   // Owner gizmo
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: gizmo (Public Property)
        // Desc: Returns the owner gizmo.
        //-----------------------------------------------------------------------------
        public Gizmo gizmo { get { return mGizmo; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Internal_Init() (Public Function)
        // Desc: Called when a behaviour is created and attached to its owner gizmo.
        // Parm: gizmo - Owner gizmo.
        //-----------------------------------------------------------------------------
        public void Internal_Init(Gizmo gizmo)
        {
            mGizmo = gizmo;
            OnInit();
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_OnDragStart() (Public Function)
        // Desc: Called by the owner gizmo when the gizmo's drag operation starts.
        // Parm: drag - The gizmo's drag operation.
        //-----------------------------------------------------------------------------
        public void Internal_OnDragStart(GizmoDrag drag)
        {
            OnDragStart(drag);
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_OnDrag() (Public Function)
        // Desc: Called by the owner gizmo when the gizmo is dragged.
        // Parm: drag - The gizmo drag operation.
        //-----------------------------------------------------------------------------
        public void Internal_OnDrag(GizmoDrag drag)
        {
            OnDrag(drag);
        }

        //-----------------------------------------------------------------------------
        // Name: Internal_OnDragEnd() (Public Function)
        // Desc: Called by the owner gizmo when the gizmo's drag operation ends.
        // Parm: drag - The drag operation that ended.
        //-----------------------------------------------------------------------------
        public void Internal_OnDragEnd(GizmoDrag drag)
        {
            OnDragEnd(drag);
        }
        #endregion

        #region Protected Virtual Functions
        //-----------------------------------------------------------------------------
        // Name: OnInit() (Protected Virtual Function)
        // Desc: Called when the behaviour must initialize itself.
        //-----------------------------------------------------------------------------
        protected virtual void OnInit() { }

        //-----------------------------------------------------------------------------
        // Name: OnDragStart() (Protected Virtual Function)
        // Desc: Called when the gizmo's drag operation starts.
        // Parm: drag - The gizmo's drag operation.
        //-----------------------------------------------------------------------------
        protected virtual void OnDragStart(GizmoDrag drag) {}

        //-----------------------------------------------------------------------------
        // Name: OnDrag() (Protected Virtual Function)
        // Desc: Called when the gizmo is dragged.
        // Parm: drag - The active drag operation.
        //-----------------------------------------------------------------------------
        protected virtual void OnDrag(GizmoDrag drag) {}

        //-----------------------------------------------------------------------------
        // Name: OnDragEnd() (Protected Virtual Function)
        // Desc: Called when the gizmo's drag operation ends.
        // Parm: drag - The drag operation that ended.
        //-----------------------------------------------------------------------------
        protected virtual void OnDragEnd(GizmoDrag drag) {}
        #endregion
    }
    #endregion
}