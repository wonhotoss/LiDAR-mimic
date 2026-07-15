using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using System;
using System.Collections.Generic;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EMouseButtons (Public Flags Enum)
    // Desc: Bitmask representing mouse buttons. Allows combination of multiple
    //       buttons using bitwise operations.
    //-----------------------------------------------------------------------------
    [Flags] public enum EMouseButtons
    {
        None   = 0,
        Left   = 1 << 0,
        Right  = 1 << 1,
        Middle = 1 << 2
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTInput (Public Singleton Class)
    // Desc: Wraps input specific functionality and hides the underlying Input API
    //       details.
    //-----------------------------------------------------------------------------
    [ExecuteInEditMode] public class RTInput : MonoSingleton<RTInput>
    {
        #region Private Fields
        [SerializeField] ShortcutProfileManager mShortcutProfileManager = new ShortcutProfileManager();    // Manages a collection of shortcut profiles

        // The pointing input device
        PointingInputDevice mPointingInputDevice;

        // Buffers used to avoid memory allocations
        List<Shortcut> mClutchShortcutBuffer    = new List<Shortcut>();
        List<Shortcut> mActionShortcutBuffer    = new List<Shortcut>();
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: pointingInputDevice (Public Property)
        // Desc: Returns or sets the pointing input device. If you are using your own
        //       pointing input device, you can set it with this property.
        //-----------------------------------------------------------------------------
        public PointingInputDevice      pointingInputDevice     { get { return mPointingInputDevice; } set { if (value != null) mPointingInputDevice = value; } }

        //-----------------------------------------------------------------------------
        // Name: shortcutProfileManager (Public Property)
        // Desc: Returns the shortcut profile manager.
        //-----------------------------------------------------------------------------
        public ShortcutProfileManager   shortcutProfileManager  { get { return mShortcutProfileManager; } }

        //-----------------------------------------------------------------------------
        // Name: mousePosition (Public Property)
        // Desc: Returns the mouse position.
        //-----------------------------------------------------------------------------
        public Vector3  mousePosition   { get { return Mouse.current.position.ReadValue(); } }

        //-----------------------------------------------------------------------------
        // Name: hasMouse (Public Property)
        // Desc: Returns whether or not a mouse is present.
        //-----------------------------------------------------------------------------
        public bool     hasMouse        { get { return Mouse.current != null; } }

        //-----------------------------------------------------------------------------
        // Name: scrollDelta (Public Property)
        // Desc: Returns the mouse scroll delta.
        //-----------------------------------------------------------------------------
        public Vector2  scrollDelta
        {
            get
            {
                // Note: When using windows, we need to divide by 120
                #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                return Mouse.current.scroll.ReadValue() / new Vector2(120.0f, 120.0f);
                #else
                return Mouse.current.scroll.ReadValue();
                #endif
            }
        }

        //-----------------------------------------------------------------------------
        // Name: anyKeyPressed (Public Property)
        // Desc: Returns true if any key is pressed.
        //-----------------------------------------------------------------------------
        public bool     anyKeyPressed       { get { return Keyboard.current.anyKey.isPressed; } }

        //-----------------------------------------------------------------------------
        // Name: anyKeyWentDown (Public Property)
        // Desc: Returns true if any key was pressed during the current frame.
        //-----------------------------------------------------------------------------
        public bool     anyKeyWentDown      { get { return Keyboard.current.anyKey.wasPressedThisFrame; } }

        //-----------------------------------------------------------------------------
        // Name: anyNonModifierKeyWentDown (Public Property)
        // Desc: Returns true if any non-modifier key was pressed during the current frame.
        //-----------------------------------------------------------------------------
        public bool     anyNonModifierKeyWentDown
        {
            get
            {
                var kb = Keyboard.current;

                // No key went down?
                if (!kb.anyKey.wasPressedThisFrame)
                    return false;

                // Is the key a modifier?
                if (kb.leftShiftKey.wasPressedThisFrame)        return false;
                if (kb.rightShiftKey.wasPressedThisFrame)       return false;
                if (kb.leftCtrlKey.wasPressedThisFrame)         return false;
                if (kb.rightCtrlKey.wasPressedThisFrame)        return false;
                if (kb.leftAltKey.wasPressedThisFrame)          return false;
                if (kb.rightAltKey.wasPressedThisFrame)         return false;
                if (kb.leftCommandKey.wasPressedThisFrame)      return false;
                if (kb.rightCommandKey.wasPressedThisFrame)     return false;

                // A non-modifier key went down
                return true;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: anyKeyWentUp (Public Property)
        // Desc: Returns true if any key was released during the current frame.
        //-----------------------------------------------------------------------------
        public bool     anyKeyWentUp        { get { return Keyboard.current.anyKey.wasReleasedThisFrame; } }

        //-----------------------------------------------------------------------------
        // Name: altPresed (Public Property)
        // Desc: Returns true if any Alt key is pressed.
        //-----------------------------------------------------------------------------
        public bool     altPresed           { get { return KeyPressed(Key.LeftAlt) || KeyPressed(Key.RightAlt); } }

        //-----------------------------------------------------------------------------
        // Name: ctrlPressed (Public Property)
        // Desc: Returns true if any Control key is pressed.
        //-----------------------------------------------------------------------------
        public bool     ctrlPressed         { get { return KeyPressed(Key.LeftCtrl) || KeyPressed(Key.RightCtrl); } }

        //-----------------------------------------------------------------------------
        // Name: cmdPressed (Public Property)
        // Desc: Returns true if any Command key is pressed.
        //-----------------------------------------------------------------------------
        public bool     cmdPressed         { get { return KeyPressed(Key.LeftCommand) || KeyPressed(Key.RightCommand); } }

        //-----------------------------------------------------------------------------
        // Name: shiftPressed (Public Property)
        // Desc: Returns true if any Shift key is pressed.
        //-----------------------------------------------------------------------------
        public bool     shiftPressed        { get { return KeyPressed(Key.LeftShift) || KeyPressed(Key.RightShift); } }

        //-----------------------------------------------------------------------------
        // Name: leftMBWentDown (Public Property)
        // Desc: Return true if the left mouse button was pressed during the current
        //       frame and false otherwise.
        //-----------------------------------------------------------------------------
        public bool     leftMBWentDown      { get { return Mouse.current.leftButton.wasPressedThisFrame; } }

        //-----------------------------------------------------------------------------
        // Name: rightMBWentDown (Public Property)
        // Desc: Return true if the right mouse button was pressed during the current
        //       frame and false otherwise.
        //-----------------------------------------------------------------------------
        public bool     rightMBWentDown     { get { return Mouse.current.rightButton.wasPressedThisFrame; } }

        //-----------------------------------------------------------------------------
        // Name: middleMBWentDown (Public Property)
        // Desc: Return true if the middle mouse button was pressed during the current
        //       frame and false otherwise.
        //-----------------------------------------------------------------------------
        public bool     middleMBWentDown    { get { return Mouse.current.middleButton.wasPressedThisFrame; } }

        //-----------------------------------------------------------------------------
        // Name: leftMBWentUp (Public Property)
        // Desc: Return true if the left mouse button was released during the current
        //       frame and false otherwise.
        //-----------------------------------------------------------------------------
        public bool     leftMBWentUp        { get { return Mouse.current.leftButton.wasReleasedThisFrame; } }

        //-----------------------------------------------------------------------------
        // Name: rightMBWentUp (Public Property)
        // Desc: Return true if the right mouse button was released during the current
        //       frame and false otherwise.
        //-----------------------------------------------------------------------------
        public bool     rightMBWentUp       { get { return Mouse.current.rightButton.wasReleasedThisFrame; } }

        //-----------------------------------------------------------------------------
        // Name: middleMBWentUp (Public Property)
        // Desc: Return true if the middle mouse button was released during the current
        //       frame and false otherwise.
        //-----------------------------------------------------------------------------
        public bool     middleMBWentUp      { get { return Mouse.current.middleButton.wasReleasedThisFrame; } }

        //-----------------------------------------------------------------------------
        // Name: leftMBPressed (Public Property)
        // Desc: Return true if the left mouse button is pressed and false otherwise.
        //-----------------------------------------------------------------------------
        public bool     leftMBPressed       { get { return Mouse.current.leftButton.isPressed; } }

        //-----------------------------------------------------------------------------
        // Name: rightMBPressed (Public Property)
        // Desc: Return true if the right mouse button is pressed and false otherwise.
        //-----------------------------------------------------------------------------
        public bool     rightMBPressed      { get { return Mouse.current.rightButton.isPressed; } }

        //-----------------------------------------------------------------------------
        // Name: middleMBPressed (Public Property)
        // Desc: Return true if the middle mouse button is pressed and false otherwise.
        //-----------------------------------------------------------------------------
        public bool     middleMBPressed     { get { return Mouse.current.middleButton.isPressed; } }

        //-----------------------------------------------------------------------------
        // Name: anyMBPressed (Public Property)
        // Desc: Returns true if any mouse button is pressed.
        //-----------------------------------------------------------------------------
        public bool     anyMBPressed        { get { return leftMBPressed || rightMBPressed || middleMBPressed; } }

        //-----------------------------------------------------------------------------
        // Name: anyMBWentDown (Public Property)
        // Desc: Returns true if any mouse button was pressed during the current frame.
        //-----------------------------------------------------------------------------
        public bool     anyMBWentDown       { get { return leftMBWentDown || rightMBWentDown || middleMBWentDown; } }

        //-----------------------------------------------------------------------------
        // Name: anyMBWentUp (Public Property)
        // Desc: Returns true if any mouse button was released during the current frame.
        //-----------------------------------------------------------------------------
        public bool     anyMBWentUp         { get { return leftMBWentUp || rightMBWentUp || middleMBWentUp; } }

        //-----------------------------------------------------------------------------
        // Name: mbPressedMask (Public Property)
        // Desc: Returns a bitmask describing which mouse buttons are currently held
        //       down during this frame.
        //-----------------------------------------------------------------------------
        public EMouseButtons mbPressedMask
        {
            get
            {
                EMouseButtons mask = EMouseButtons.None;

                if (leftMBPressed)   mask |= EMouseButtons.Left;
                if (rightMBPressed)  mask |= EMouseButtons.Right;
                if (middleMBPressed) mask |= EMouseButtons.Middle;

                return mask;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: mbWentDownMask (Public Property)
        // Desc: Returns a bitmask describing which mouse buttons were pressed during
        //       the current frame. Multiple buttons may be combined using flags.
        //-----------------------------------------------------------------------------
        public EMouseButtons mbWentDownMask
        {
            get
            {
                EMouseButtons mask = EMouseButtons.None;

                if (leftMBWentDown)   mask |= EMouseButtons.Left;
                if (rightMBWentDown)  mask |= EMouseButtons.Right;
                if (middleMBWentDown) mask |= EMouseButtons.Middle;

                return mask;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: mouseMoved (Public Property)
        // Desc: Return true if the mouse was moved since the last frame and false otherwise.
        //-----------------------------------------------------------------------------
        public bool     mouseMoved          { get { return Mouse.current.delta.ReadValue().sqrMagnitude != 0.0f; } }

        //-----------------------------------------------------------------------------
        // Name: mouseDeltaX (Public Property)
        // Desc: Return the horizontal mouse delta.
        //-----------------------------------------------------------------------------
        public float    mouseDeltaX         { get { return Mouse.current.delta.x.ReadValue(); } }

        //-----------------------------------------------------------------------------
        // Name: mouseDeltaY (Public Property)
        // Desc: Return the vertical mouse delta.
        //-----------------------------------------------------------------------------
        public float    mouseDeltaY         { get { return Mouse.current.delta.y.ReadValue(); } }
        
        //-----------------------------------------------------------------------------
        // Name: mouseDelta (Public Property)
        // Desc: Return the mouse delta.
        //-----------------------------------------------------------------------------
        public Vector2  mouseDelta          { get { return Mouse.current.delta.value; } }

        //-----------------------------------------------------------------------------
        // Name: touchCount (Public Property)
        // Desc: Return the number of touches.
        //-----------------------------------------------------------------------------
        public int      touchCount          { get { return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Internal_ProcessShortcuts() (Public Function)
        // Desc: Evaluates the active shortcut profile and triggers any associated
        //       clutch or action commands.
        //
        //       Clutch shortcuts are evaluated first in order to update transient
        //       input state (e.g. modifier-driven modes). If any clutch shortcut
        //       deactivates during this frame, action shortcuts are suppressed to
        //       prevent unintended command execution (e.g. release-triggered actions).
        //
        // Note: If the 'RTInput' MonoBehaviour script is disabled, this function
        //       has no effect.
        //-----------------------------------------------------------------------------
        public void Internal_ProcessShortcuts()
        {
            int shortcutCount;

            // No-op?
            if (!enabled)
                return;

            // Reset data
            mClutchShortcutBuffer.Clear();
            mActionShortcutBuffer.Clear();

            // Loop through each shortcut category and separate the Clutch from the Action commands
            int shortcutCategoryCount = shortcutProfileManager.activeProfile.shortcutCategoryCount;
            for (int c = 0; c < shortcutCategoryCount; ++c)
            {
                // Store category for easy access
                var category = shortcutProfileManager.activeProfile[c];

                // Loop through each shortcut in this category
                shortcutCount = category.shortcutCount;
                for (int s = 0; s < shortcutCount; ++s)
                {
                    // Store shortcut in the corresponding list
                    Shortcut shortcut = category[s];
                    if (shortcut.commandType == ECommandType.Clutch) mClutchShortcutBuffer.Add(shortcut);
                    else mActionShortcutBuffer.Add(shortcut);
                }
            }

            // Handle 'Clutch' shortcuts first. If we find a 'Clutch' command that was just deactivated,
            // we will no longer evaluate the 'Action' commands. For example, holding ALT + left MB to orbit
            // the camera will cause a pick-selection to occur when the left mouse button is released if
            // the mouse cursor happens to be hovering an object at that time.
            bool ignoreActionShortcuts = false;
            shortcutCount = mClutchShortcutBuffer.Count;
            for (int s = 0; s < shortcutCount; ++s)
            {
                Shortcut shortcut = mClutchShortcutBuffer[s];

                // If a more specific clutch shortcut is active in the same context, the
                // generic shortcut must not compete with it. Example: F + RMB beats RMB.
                if (IsClutchShortcutBlocked(shortcut, mClutchShortcutBuffer))
                {
                    if (shortcut.isCommandActive)
                    {
                        var deactivateResult = shortcut.ForceDeactivate();
                        if (deactivateResult == EShortcutEvalAction.Deactivated)
                            ignoreActionShortcuts = true;
                    }

                    continue;
                }

                // Evaluate and if the shortcut has been deactivated, ignore 'Action' shortcuts.
                var result = shortcut.Evaluate();
                if (result == EShortcutEvalAction.Deactivated)
                    ignoreActionShortcuts = true;
            }

            // Evaluate action commands
            if (!ignoreActionShortcuts)
            {
                shortcutCount = mActionShortcutBuffer.Count;
                for (int s = 0; s < shortcutCount; ++s)
                    mActionShortcutBuffer[s].Evaluate();
            }
        }
        
        //-----------------------------------------------------------------------------
        // Name: SyncShortcutEnabledStates() (Public Function)
        // Desc: Syncs the shortcut enabled states across profiles.
        // Parm: syncProfile - All shortcut profiles are synced with this profile. If
        //                     null, the active profile is used instead.
        //-----------------------------------------------------------------------------
        public void SyncShortcutEnabledStates(ShortcutProfile syncProfile = null)
        {
            // Default to the active profile if no sync profile was specified
            if (syncProfile == null)
                syncProfile = mShortcutProfileManager.activeProfile;

            // Loop through each shortcut in the sync profile
            int categoryCount = syncProfile.shortcutCategoryCount;
            for (int c = 0; c < categoryCount; ++c)
            {
                var category = syncProfile[c];
                int shCount  = category.shortcutCount;
                for (int s = 0; s < shCount; ++s)
                {
                    // Sync enabled states
                    SetShortcutEnabled(category[s].name, category.name, category[s].enabled);
                }
            }
        }

        //-----------------------------------------------------------------------------
        // Name: SetShortcutEnabled() (Public Function)
        // Desc: Sets the enabled state of the shortcut with the specified name.
        // Parm: shortcutName   - Shortcut name.
        //       categoryName   - Name of the category that contains the shortcut.
        //       enabled        - Shortcut enabled state.
        //-----------------------------------------------------------------------------
        public void SetShortcutEnabled(string shortcutName, string categoryName, bool enabled)
        {
            // Loop through each profile and set shortcut enabled state
            int count = mShortcutProfileManager.profileCount;
            for (int i = 0; i < count; ++i)
                mShortcutProfileManager[i].SetShortcutEnabled(shortcutName, categoryName, enabled);
        }

        //-----------------------------------------------------------------------------
        // Name: GetTouch() (Public Function)
        // Desc: Returns the touch with the specified index.
        // Parm: index - Touch index.
        // Rtrn: An instance of 'Touch' which stores touch data.
        //-----------------------------------------------------------------------------
        public UnityEngine.InputSystem.EnhancedTouch.Touch GetTouch(int index)
        {
            if (index >= touchCount) return new UnityEngine.InputSystem.EnhancedTouch.Touch();
            return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[index];
        }

        //-----------------------------------------------------------------------------
        // Name: MBWentDown() (Public Function)
        // Desc: Checks if the specified mouse button was pressed this frame.
        // Parm: mouseButton - The mouse button to check for.
        // Rtrn: True if the specified mouse button was pressed this frame and false otherwise.
        //-----------------------------------------------------------------------------
        public bool MBWentDown(EMouseButton mouseButton)
        {
            // Check button state
            if (mouseButton == EMouseButton.Left) return Mouse.current.leftButton.wasPressedThisFrame;
            else if (mouseButton == EMouseButton.Right) return Mouse.current.rightButton.wasPressedThisFrame;
            else if (mouseButton == EMouseButton.Middle) return Mouse.current.middleButton.wasPressedThisFrame;
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: MBWentUp() (Public Function)
        // Desc: Checks if the specified mouse button was released this frame.
        // Parm: mouseButton - The mouse button to check for.
        // Rtrn: True if the specified mouse button was released this frame and false otherwise.
        //-----------------------------------------------------------------------------
        public bool MBWentUp(EMouseButton mouseButton)
        {
            // Check button state
            if (mouseButton == EMouseButton.Left) return Mouse.current.leftButton.wasReleasedThisFrame;
            else if (mouseButton == EMouseButton.Right) return Mouse.current.rightButton.wasReleasedThisFrame;
            else if (mouseButton == EMouseButton.Middle) return Mouse.current.middleButton.wasReleasedThisFrame;
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: MBPressed() (Public Function)
        // Desc: Checks if the specified mouse button is pressed.
        // Parm: mouseButton - The mouse button to check for.
        // Rtrn: True if the specified mouse button is pressed and false otherwise.
        //-----------------------------------------------------------------------------
        public bool MBPressed(EMouseButton mouseButton)
        {
            // Check button state
            if (mouseButton == EMouseButton.Left) return Mouse.current.leftButton.isPressed;
            else if (mouseButton == EMouseButton.Right) return Mouse.current.rightButton.isPressed;
            else if (mouseButton == EMouseButton.Middle) return Mouse.current.middleButton.isPressed;
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: KeyWentDown() (Public Function)
        // Desc: Checks if the specified key was pressed this frame.
        // Parm: keyCode - Query key.
        // Rtrn: True if the key was pressed this frame and false otherwise.
        //-----------------------------------------------------------------------------
        public bool KeyWentDown(Key keyCode)
        {
            return Keyboard.current[keyCode].wasPressedThisFrame;
        }

        //-----------------------------------------------------------------------------
        // Name: KeyWentUp() (Public Function)
        // Desc: Checks if the specified key was released this frame.
        // Parm: keyCode - Query key.
        // Rtrn: True if the key was released this frame and false otherwise.
        //-----------------------------------------------------------------------------
        public bool KeyWentUp(Key keyCode)
        {
            return Keyboard.current[keyCode].wasReleasedThisFrame;
        }

        //-----------------------------------------------------------------------------
        // Name: KeyPressed() (Public Function)
        // Desc: Checks if the specified key is pressed.
        // Parm: keyCode - Query key.
        // Rtrn: True if the key is pressed and false otherwise.
        //-----------------------------------------------------------------------------
        public bool KeyPressed(Key keyCode)
        {
            return Keyboard.current[keyCode].isPressed;
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: Start() (Private Function)
        // Desc: Called by Unity to allow the object to initialize itself.
        //-----------------------------------------------------------------------------
        void Start()
        {
            // Enable enhanced touch
            EnhancedTouchSupport.Enable();
            
            // Create the pointing input device
            #if !UNITY_EDITOR && (UNITY_IOS || UNITY_ANDROID || UNITY_WP_8_1)
            mPointingInputDevice = new TouchInputDevice();
            #else
            mPointingInputDevice = new MouseInputDevice();
            #endif
        }

        //-----------------------------------------------------------------------------
        // Name: OnEnable() (Private Function)
        // Desc: Called by Unity when the object is enabled.
        //-----------------------------------------------------------------------------
        void OnEnable()
        {
            // Init data
            mShortcutProfileManager.Init();

            // Refresh shortcut definitions in editor to ensure that any
            // code-added shortcuts are created after domain reload or recompilation.
            #if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                int count = mShortcutProfileManager.profileCount;
                for (int i = 0; i < count; ++i)
                {
                    mShortcutProfileManager[i].RefreshShortcuts();
                    mShortcutProfileManager[i].DetectShortcutConflicts();
                }
            }
            #endif
        }

        //-----------------------------------------------------------------------------
        // Name: IsClutchShortcutBlocked() (Private Function)
        // Desc: Checks if the specified clutch shortcut is blocked by a more specific
        //       active clutch shortcut from the specified list.
        // Parm: shortcut        - Shortcut to check.
        //       clutchShortcuts - List of clutch shortcuts to test against.
        // Rtrn: True if the shortcut is blocked and false otherwise.
        //-----------------------------------------------------------------------------
        bool IsClutchShortcutBlocked(Shortcut shortcut, List<Shortcut> clutchShortcuts)
        {
            int shortcutCount = clutchShortcuts.Count;
            for (int s = 0; s < shortcutCount; ++s)
            {
                Shortcut other = clutchShortcuts[s];
                if (other == shortcut)
                    continue;

                if (!other.CanActivate())
                    continue;

                if (other.IsMoreSpecificThan(shortcut))
                    return true;
            }

            return false;
        }
        #endregion
    }
    #endregion
}