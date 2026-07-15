using System;
using UnityEngine;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: ECommandType (Public Enum)
    // Desc: Defines different types of commands.
    // Note: From Unity Editor's shortcut window.
    //-----------------------------------------------------------------------------
    public enum ECommandType
    {
        Action = 0, // An 'Action' command executes once (e.g. activate move gizmo, delete selected objects etc)
        Clutch      // A  'Clutch' command has 2 modes: clutch and unclutch (e.g. enable/disable vertex snapping, enable/disable camera movement etc)
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: Command (Public Abstract Class)
    // Desc: Represents a command which can be executed in response to a shortcut
    //       being triggered. Although this class was designed to be used with
    //       shortcuts, commands can also be created and activated on the spot as 
    //       needed.
    //-----------------------------------------------------------------------------
    [Serializable] public abstract class Command
    {
        #region Private Fields
        bool mIsActive   = false;   // Is the command currently active?
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: isActive (Public Property)
        // Desc: Returns true if the command is active and false otherwise.
        //-----------------------------------------------------------------------------
        public bool   isActive  { get { return mIsActive; } }
        #endregion

        #region Public Abstract Functions
        //-----------------------------------------------------------------------------
        // Name: commandType (Public Abstract Property)
        // Desc: Returns the command type.
        //-----------------------------------------------------------------------------
        public abstract ECommandType commandType { get; }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Activate() (Public Function)
        // Desc: Activates the command. Meaning for each command type:
        //          a) Action - executes the command.
        //          b) Clutch - executes the command's clutch phase.
        // Rtrn: True if the command was activated and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Activate()
        {
            // If we're already active, we can't activate
            if (mIsActive) return false;

            // Activate
            mIsActive = true;
            OnActivate();

            // Activated
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: Deactivate() (Public Function)
        // Desc: Deactivates the command. Meaning for each command type:
        //          a) Action - N/A
        //          b) Clutch - executes the command's unclutch phase.
        // Rtrn: True if the command was deactivated and false otherwise.
        //-----------------------------------------------------------------------------
        public bool Deactivate()
        {
            // If we're already deactivated, we can't deactivate
            if (!mIsActive) return false;

            // Deactivate
            mIsActive = false;
            OnDeactivate();

            // Deactivated
            return true;
        }
        #endregion

        #region Protected Abstract Functions
        //-----------------------------------------------------------------------------
        // Name: OnActivate() (Protected Abstract Function)
        // Desc: Called when the command is activated.
        //-----------------------------------------------------------------------------
        protected abstract void OnActivate();

        //-----------------------------------------------------------------------------
        // Name: OnDeactivate() (Protected Abstract Function)
        // Desc: Called when the command is deactivated.
        //-----------------------------------------------------------------------------
        protected abstract void OnDeactivate();
        #endregion
    }

    //-----------------------------------------------------------------------------
    // Name: ActionCommand (Public Abstract Class)
    // Desc: Represents an action command. Derived classes must implement the 'OnActivate'
    //       function in order to establish the command's logic.
    //-----------------------------------------------------------------------------
    [Serializable] public abstract class ActionCommand : Command
    {
        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: commandType (Public Property)
        // Desc: Returns the command type.
        //-----------------------------------------------------------------------------
        public override ECommandType commandType { get { return ECommandType.Action; } }
        #endregion

        // Action commands can't implement 'OnDeactivate'.
        sealed protected override void OnDeactivate() { }
    }

    //-----------------------------------------------------------------------------
    // Name: ClutchCommand (Public Abstract Class)
    // Desc: Represents a clutch command. Derived classes must implement the 'OnActivate'
    //       and 'OnDeactivate' functions in order to establish the command's logic.
    //       Clutch commands are useful when implementing commands with a temporary effect.
    //       For example, activating vertex snapping for the move gizmo while the 'V'
    //       key is pressed. Or enabling the camera to move around while the RMB is
    //       pressed. Clutch commands allow something to happen WHILE/AS LONG AS a
    //       condition is true.
    //-----------------------------------------------------------------------------
    [Serializable] public abstract class ClutchCommand : Command
    {
        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: commandType (Public Property)
        // Desc: Returns the command type.
        //-----------------------------------------------------------------------------
        public override ECommandType commandType { get { return ECommandType.Clutch; } }
        #endregion

        #region Protected Abstract Functions
        //-----------------------------------------------------------------------------
        // Name: OnDeactivate() (Protected Abstract Function)
        // Desc: Executes the command's 'Unclutch' phase.
        //-----------------------------------------------------------------------------
        protected abstract override void OnDeactivate();
        #endregion
    }

    #region Camera
    //-----------------------------------------------------------------------------
    // Name: Command_Camera_SpeedModifier (Public Class)
    // Desc: Command which enables the camera speed modifier.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Camera_SpeedModifier : ClutchCommand
    {
        protected override void OnActivate  () { RTCamera.get.speedModifierEnabled = true; }
        protected override void OnDeactivate() { RTCamera.get.speedModifierEnabled = false; }
    }

    //-----------------------------------------------------------------------------
    // Name: Command_Camera_PanMode (Public Class)
    // Desc: Command which enables camera pan mode.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Camera_PanMode : ClutchCommand
    {
        protected override void OnActivate  () { RTCamera.get.navigationMode = ECameraNavigationMode.Pan; }
        protected override void OnDeactivate()
        {
            if (RTCamera.get.navigationMode == ECameraNavigationMode.Pan)
                RTCamera.get.navigationMode = ECameraNavigationMode.None;
        }
    }

    //-----------------------------------------------------------------------------
    // Name: Command_Camera_OrbitMode (Public Class)
    // Desc: Command which enables camera orbit mode.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Camera_OrbitMode : ClutchCommand
    {
        protected override void OnActivate  () { RTCamera.get.navigationMode = ECameraNavigationMode.Orbit; }
        protected override void OnDeactivate()
        {
            if (RTCamera.get.navigationMode == ECameraNavigationMode.Orbit)
                RTCamera.get.navigationMode = ECameraNavigationMode.None;
        }
    }

    //-----------------------------------------------------------------------------
    // Name: Command_Camera_FlyMode (Public Class)
    // Desc: Command which enables camera fly mode.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Camera_FlyMode : ClutchCommand
    {
        protected override void OnActivate  () { RTCamera.get.navigationMode = ECameraNavigationMode.Fly; }
        protected override void OnDeactivate()
        {
            if (RTCamera.get.navigationMode == ECameraNavigationMode.Fly)
                RTCamera.get.navigationMode = ECameraNavigationMode.None;
        }
    }

    //-----------------------------------------------------------------------------
    // Name: Command_Camera_ZoomMode (Public Class)
    // Desc: Command which enables camera zoom mode.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Camera_ZoomMode : ClutchCommand
    {
        protected override void OnActivate  () { RTCamera.get.navigationMode = ECameraNavigationMode.Zoom; }
        protected override void OnDeactivate()
        {
            if (RTCamera.get.navigationMode == ECameraNavigationMode.Zoom)
                RTCamera.get.navigationMode = ECameraNavigationMode.None;
        }
    }

    //-----------------------------------------------------------------------------
    // Name: Command_Camera_FlyLeft (Public Class)
    // Desc: Command which enables the camera to move left.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Camera_FlyLeft : ClutchCommand
    {
        protected override void OnActivate  () { RTCamera.get.moveDirections |=  ECameraMoveDirections.Left; }
        protected override void OnDeactivate() { RTCamera.get.moveDirections &= ~ECameraMoveDirections.Left; }
    }

    //-----------------------------------------------------------------------------
    // Name: Command_Camera_FlyRight (Public Class)
    // Desc: Command which enables the camera to move right.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Camera_FlyRight : ClutchCommand
    {
        protected override void OnActivate  () { RTCamera.get.moveDirections |=  ECameraMoveDirections.Right; }
        protected override void OnDeactivate() { RTCamera.get.moveDirections &= ~ECameraMoveDirections.Right; }
    }

    //-----------------------------------------------------------------------------
    // Name: Command_Camera_FlyDown (Public Class)
    // Desc: Command which enables the camera to move down.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Camera_FlyDown : ClutchCommand
    {
        protected override void OnActivate  () { RTCamera.get.moveDirections |=  ECameraMoveDirections.Down; }
        protected override void OnDeactivate() { RTCamera.get.moveDirections &= ~ECameraMoveDirections.Down; }
    }

    //-----------------------------------------------------------------------------
    // Name: Command_Camera_FlyUp (Public Class)
    // Desc: Command which enables the camera to move up.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Camera_FlyUp : ClutchCommand
    {
        protected override void OnActivate  () { RTCamera.get.moveDirections |=  ECameraMoveDirections.Up; }
        protected override void OnDeactivate() { RTCamera.get.moveDirections &= ~ECameraMoveDirections.Up; }
    }

    //-----------------------------------------------------------------------------
    // Name: Command_Camera_FlyBackward (Public Class)
    // Desc: Command which enables the camera to move backward.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Camera_FlyBackward : ClutchCommand
    {
        protected override void OnActivate  () { RTCamera.get.moveDirections |=  ECameraMoveDirections.Backward; }
        protected override void OnDeactivate() { RTCamera.get.moveDirections &= ~ECameraMoveDirections.Backward; }
    }

    //-----------------------------------------------------------------------------
    // Name: Command_Camera_FlyForward (Public Class)
    // Desc: Command which enables the camera to move forward.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Camera_FlyForward : ClutchCommand
    {
        protected override void OnActivate  () { RTCamera.get.moveDirections |=  ECameraMoveDirections.Forward; }
        protected override void OnDeactivate() { RTCamera.get.moveDirections &= ~ECameraMoveDirections.Forward; }
    }
    #endregion

    #region Grid
    //-----------------------------------------------------------------------------
    // Name: Command_Grid_StepMoveDown (Public Class)
    // Desc: Command which moves the grid down along its local Y axis in steps equal
    //       to the grid cell size along Y.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Grid_StepMoveDown : ActionCommand
    {
        protected override void OnActivate() { RTGrid.get.settings.localYOffset -= RTGrid.get.settings.cellSize.y; }
    }

    //-----------------------------------------------------------------------------
    // Name: Command_Grid_StepMoveUp (Public Class)
    // Desc: Command which moves the grid up along its local Y axis in steps equal
    //       to the grid cell size along Y.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Grid_StepMoveUp : ActionCommand
    {
        protected override void OnActivate() { RTGrid.get.settings.localYOffset += RTGrid.get.settings.cellSize.y; }
    }

    //-----------------------------------------------------------------------------
    // Name: Command_Grid_ActionMode (Public Class)
    // Desc: Command which places the grid in action mode.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Grid_ActionMode : ClutchCommand
    {
        protected override void OnActivate  () { RTGrid.get.actionModeEnabled = true; }
        protected override void OnDeactivate() { RTGrid.get.actionModeEnabled = false; }
    }

    //-----------------------------------------------------------------------------
    // Name: Command_Grid_SnapToPickPoint (Public Class)
    // Desc: Command which snaps the grid to the world point picked by the mouse cursor.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Grid_SnapToPickPoint : ActionCommand
    {
        protected override void OnActivate() { RTGrid.get.updateAction = EGridUpdateAction.SnapToPickPoint; }
    }

    //-----------------------------------------------------------------------------
    // Name: Command_Grid_SnapToPickPointExtents (Public Class)
    // Desc: Command which snaps the grid to the extents of the world point picked
    //       by the mouse cursor.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Grid_SnapToPickPointExtents : ActionCommand
    {
        protected override void OnActivate() { RTGrid.get.updateAction = EGridUpdateAction.SnapToPickPointExtents; }
    }
    #endregion

    #region Gizmos
    //-----------------------------------------------------------------------------
    // Name: Command_Gizmos_Snap (Public Class)
    // Desc: Command which toggles snapping on/off.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Gizmos_Snap : ClutchCommand
    {
        protected override void OnActivate() { RTGizmos.get.snapEnabled = true; }
        protected override void OnDeactivate() { RTGizmos.get.snapEnabled = false; }
    }

    //-----------------------------------------------------------------------------
    // Name: Command_Gizmos_VertexSnap (Public Class)
    // Desc: Command which toggles vertex snapping on/off.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Gizmos_VertexSnap : ClutchCommand
    {
        protected override void OnActivate() { RTGizmos.get.SetMoveGizmosMoveMode(EGizmoMoveMode.VertexSnap); }
        protected override void OnDeactivate() { RTGizmos.get.SetMoveGizmosMoveMode(EGizmoMoveMode.Normal); }
    }

    //-----------------------------------------------------------------------------
    // Name: Command_Gizmos_AltMode (Public Class)
    // Desc: Command which toggles gizmos alternative mode.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Gizmos_AltMode : ClutchCommand
    {
        protected override void OnActivate() { RTGizmos.get.SetAltModeEnabled(true); }
        protected override void OnDeactivate() { RTGizmos.get.SetAltModeEnabled(false); }
    }
    #endregion

    #region Undo/Redo
    //-----------------------------------------------------------------------------
    // Name: Command_Undo (Public Class)
    // Desc: Undo command.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Undo : ActionCommand
    {
        protected override void OnActivate() { RTUndo.get.Undo(); }
    }

    //-----------------------------------------------------------------------------
    // Name: Command_Redo (Public Class)
    // Desc: Redo command.
    //-----------------------------------------------------------------------------
    [Serializable] public class Command_Redo : ActionCommand
    {
        protected override void OnActivate() { RTUndo.get.Redo(); }
    }
    #endregion
    #endregion
}