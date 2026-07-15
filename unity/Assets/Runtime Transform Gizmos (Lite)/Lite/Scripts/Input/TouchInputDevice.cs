using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: TouchInputDevice (Public Class)
    // Desc: Implements 'PointingInputDevice' for a touchscreen device.
    //-----------------------------------------------------------------------------
    public class TouchInputDevice : PointingInputDevice
    {
        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: hasPointer (Public Property)
        // Desc: Returns whether or not the device has a pointer.
        //-----------------------------------------------------------------------------
        public override bool    hasPointer      { get { return RTInput.get.touchCount >= 1; } }

        //-----------------------------------------------------------------------------
        // Name: position (Public Property)
        // Desc: Returns the position of the device's cursor/pointer.
        //-----------------------------------------------------------------------------
        public override Vector2 position        { get { return RTInput.get.GetTouch(0).screenPosition; } }

        //-----------------------------------------------------------------------------
        // Name: pickButtonWentDown (Public Property)
        // Desc: Returns whether or not the button used to pick entities with the cursor
        //       has been pressed during the current frame.
        //-----------------------------------------------------------------------------
        public override bool pickButtonWentDown 
        { get { return RTInput.get.touchCount >= 1 && RTInput.get.GetTouch(0).phase == UnityEngine.InputSystem.TouchPhase.Began; } }

        //-----------------------------------------------------------------------------
        // Name: pickButtonWentUp (Public Property)
        // Desc: Returns whether or not the button used to pick entities with the cursor
        //       has been released during the current frame.
        //-----------------------------------------------------------------------------
        public override bool pickButtonWentUp  
        { get { return RTInput.get.touchCount >= 1 && RTInput.get.GetTouch(0).phase == UnityEngine.InputSystem.TouchPhase.Canceled || 
                    RTInput.get.GetTouch(0).phase == UnityEngine.InputSystem.TouchPhase.Ended; } }

        //-----------------------------------------------------------------------------
        // Name: pickButtonPressed (Public Property)
        // Desc: Returns whether or not the button used to pick entities with the cursor
        //       is currently pressed.
        //-----------------------------------------------------------------------------
        public override bool pickButtonPressed  { get { return RTInput.get.touchCount >= 1; } }

        //-----------------------------------------------------------------------------
        // Name: moved (Public Property)
        // Desc: Returns whether or not the device was moved.
        //-----------------------------------------------------------------------------
        public override bool moved              
        { get { return RTInput.get.touchCount >= 1 && RTInput.get.GetTouch(0).phase == UnityEngine.InputSystem.TouchPhase.Moved; } }

        //-----------------------------------------------------------------------------
        // Name: delta (Public Property)
        // Desc: Returns the device delta since the last frame.
        //-----------------------------------------------------------------------------
        public override Vector2 delta
        { get { return RTInput.get.touchCount >= 1 ? RTInput.get.GetTouch(0).delta : Vector2.zero; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: GetPickRay() (Public Function)
        // Desc: Returns the cursor's ray which can be used for picking.
        // Parm: camera - The camera which interacts with the cursor.
        // Rtrn: Ray which can be used to pick entities with the cursor.
        //-----------------------------------------------------------------------------
        public override Ray GetPickRay(Camera camera)
        {
            // No touches?
            if (RTInput.get.touchCount == 0)
                return new Ray();

            // Get the first touch and create a ray from it
            return camera.ScreenPointToRay(RTInput.get.GetTouch(0).screenPosition);
        }
        #endregion
    }
    #endregion
}