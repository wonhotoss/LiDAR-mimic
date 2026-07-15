using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: PointingInputDevice (Public Abstract Class)
    // Desc: An abstract base class which implement common functionality for pointing
    //       input devices. A pointing device is a device whose interfaces requires a
    //       cursor/pointer of some kind. A mouse device is the best example, but a
    //       touchscreen device is also an example of a pointing device, where the
    //       cursor/pointer can be thought of as a finger touching the screen.
    //-----------------------------------------------------------------------------
    public abstract class PointingInputDevice
    {
        #region Public Asbtract Properties
        //-----------------------------------------------------------------------------
        // Name: hasPointer (Public Abstract Property)
        // Desc: Returns whether or not the device has a pointer.
        //-----------------------------------------------------------------------------
        public abstract bool    hasPointer          { get; }

        //-----------------------------------------------------------------------------
        // Name: position (Public Abstract Property)
        // Desc: Returns the position of the device's cursor/pointer.
        //-----------------------------------------------------------------------------
        public abstract Vector2 position            { get; }

        //-----------------------------------------------------------------------------
        // Name: pickButtonWentDown (Public Abstract Property)
        // Desc: Returns whether or not the button used to pick entities with the cursor
        //       has been pressed during the current frame.
        //-----------------------------------------------------------------------------
        public abstract bool    pickButtonWentDown  { get; }

        //-----------------------------------------------------------------------------
        // Name: pickButtonWentUp (Public Abstract Property)
        // Desc: Returns whether or not the button used to pick entities with the cursor
        //       has been released during the current frame.
        //-----------------------------------------------------------------------------
        public abstract bool    pickButtonWentUp    { get; }

        //-----------------------------------------------------------------------------
        // Name: pickButtonPressed (Public Abstract Property)
        // Desc: Returns whether or not the button used to pick entities with the cursor
        //       is currently pressed.
        //-----------------------------------------------------------------------------
        public abstract bool    pickButtonPressed   { get; }

        //-----------------------------------------------------------------------------
        // Name: moved (Public Abstract Property)
        // Desc: Returns whether or not the device was moved.
        //-----------------------------------------------------------------------------
        public abstract bool    moved               { get; }

        //-----------------------------------------------------------------------------
        // Name: delta (Public Abstract Property)
        // Desc: Returns the device delta since the last frame.
        //-----------------------------------------------------------------------------
        public abstract Vector2 delta               { get; }
        #endregion

        #region Public Abstract Functions
        //-----------------------------------------------------------------------------
        // Name: GetPickRay() (Public Abstract Function)
        // Desc: Returns the cursor's ray which can be used for picking.
        // Parm: camera - The camera which interacts with the cursor.
        // Rtrn: Ray which can be used to pick entities with the cursor.
        //-----------------------------------------------------------------------------
        public abstract Ray     GetPickRay(Camera camera);
        #endregion
    }
    #endregion
}

