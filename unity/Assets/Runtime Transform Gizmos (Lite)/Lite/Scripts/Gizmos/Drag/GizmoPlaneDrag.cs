using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GizmoPlaneDrag (Public Class)
    // Desc: Represents a gizmo drag operation along a plane. Keeps track of the total
    //       and relative drag offsets.
    //-----------------------------------------------------------------------------
    public class GizmoPlaneDrag
    {
        #region Private Fields
        bool    mActive;            // Is the drag operation active?
        Camera  mCamera;            // The camera that interacts with the plane
        Plane   mDragPlane;         // The drag plane
        Vector3 mDragStartPoint;    // The original drag point when the drag operation started
        Vector3 mDragPoint;         // The intersection point between the plane and the input device
        Vector3 mDragDelta;         // How much have we dragged since the last drag update (i.e. drag offset)
        #endregion

        #region Public Properties    
        //-----------------------------------------------------------------------------
        // Name: active (Public Property)
        // Desc: Returns whether or not the drag operation is active.
        //-----------------------------------------------------------------------------
        public bool     active          { get { return mActive; } }

        //-----------------------------------------------------------------------------
        // Name: dragPlane (Public Property)
        // Desc: Returns the drag plane.
        //-----------------------------------------------------------------------------
        public Plane    dragPlane       { get { return mDragPlane; } }

        //-----------------------------------------------------------------------------
        // Name: drag (Public Property)
        // Desc: Returns the total amount of drag. This is a vector which goes from the
        //       start drag point to the current drag point.
        //-----------------------------------------------------------------------------
        public Vector3  drag            { get { return mDragPoint - mDragStartPoint; } }

        //-----------------------------------------------------------------------------
        // Name: dragPoint (Public Property)
        // Desc: Returns the drag point. This is the intersection between the input device
        //       and the drag plane.
        //-----------------------------------------------------------------------------
        public Vector3  dragPoint       { get { return mDragPoint; } }

        //-----------------------------------------------------------------------------
        // Name: dragStartPoint (Public Property)
        // Desc: Returns the drag start point. This is the intersection between the input
        //       device and the drag plane when the drag operation started.
        //-----------------------------------------------------------------------------
        public Vector3  dragStartPoint  { get { return mDragStartPoint; } }

        //-----------------------------------------------------------------------------
        // Name: dragDelta (Public Property)
        // Desc: Returns the drag delta. This is the amount of drag that occurred since
        //       the last drag update.
        //-----------------------------------------------------------------------------
        public Vector3  dragDelta       { get { return mDragDelta; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Start() (Public Function)
        // Desc: Starts a drag operation along the specified plane. The function has no
        //       effect if the drag operation is already active.
        // Parm: dragPlane  - Drag plane.
        //       dragPoint  - The point to drag. The function projects this point on the
        //                    drag plane.
        //       camera     - The camera that interacts with the plane.
        // Rtrn: True if the drag operation started and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Start(Plane dragPlane, Vector3 dragPoint, Camera camera)
        {
            // No-op?
            if (mActive)
                return false;

            // Store data
            mCamera         = camera;
            mDragPlane      = dragPlane;
            mDragStartPoint = dragPlane.ProjectPoint(dragPoint);
            mDragPoint      = mDragStartPoint;
            mDragDelta      = Vector3.zero;
            mActive         = true;

            // Success!
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: Update() (Public Function)
        // Desc: Allows the drag operation to update itself. The function has no effect
        //       if the drag operation is not active.
        //-----------------------------------------------------------------------------
        public void Update()
        {
            // Not active?
            if (!mActive)
                return;

            // Raycast the drag plane
            Ray ray = RTInput.get.pointingInputDevice.GetPickRay(mCamera);
            if (mDragPlane.Raycast(ray, out float t))
            {
                // Calculate the intersection point
                Vector3 iPt = ray.GetPoint(t);

                // Calculate drag delta and update drag point
                mDragDelta = iPt - mDragPoint;
                mDragPoint = iPt;
            }
            else mDragDelta = Vector3.zero;
        }

        //-----------------------------------------------------------------------------
        // Name: End() (Public Function)
        // Desc: Ends the drag operation. The function has no effect if the drag operation
        //       is not active.
        // Rtrn: True if the drag operation ended and false otherwise.
        //-----------------------------------------------------------------------------
        public bool End()
        {
            // No-op?
            if (!mActive)
                return false;

            // Clear data
            mDragPlane      = new Plane();
            mDragStartPoint = Vector3.zero;
            mDragPoint      = mDragStartPoint;
            mDragDelta      = Vector3.zero;
            mActive         = false;

            // Success!
            return true;
        }
        #endregion
    }
    #endregion
}