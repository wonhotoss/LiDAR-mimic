using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
#if RTGUI_PRESENT
using RTGUIFramework;
#endif

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EMouseButton (Public Enum)
    // Desc: Defines different mouse buttons.
    //-----------------------------------------------------------------------------
    public enum EMouseButton
    {
        Left = 0,
        Right,
        Middle
    }

    //-----------------------------------------------------------------------------
    // Name: EShortcutEvalAction (Public Enum)
    // Desc: Defines different actions that can be performed by a shortcut when
    //       its binding is evaluated.
    //-----------------------------------------------------------------------------
    public enum EShortcutEvalAction
    {
        None = 0,       // Nothing happened
        Activated,      // The shortcut's command was activated
        Deactivated     // The shortcut's command was deactivated (Clutch commands only)
    }
    #endregion

    #region Public Structures
    //-----------------------------------------------------------------------------
    // Name: ShortcutBinding (Public Struct)
    // Desc: Provides storage for a shortcut's binding data such as modifiers,
    //       mouse buttons and a non-modifier key.
    //-----------------------------------------------------------------------------
    [Serializable] public struct ShortcutBinding
    { 
        #region Public Fields
        public bool   alt;      // Is the Alt key part of the binding?
        public bool   control;  // Is the Control key part of the binding?
        public bool   command;  // Is the Command key part of the binding?
        public bool   shift;    // Is the Shift key part of the binding?
        public bool   leftMB;   // Is the Left Mouse Button part of the binding?
        public bool   middleMB; // Is the Middle Mouse Button part of the binding?
        public bool   rightMB;  // Is the Right Mouse Button part of the binding?
        public Key    key;      // The non-modifier key which is part of the binding
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: operator == (Public Static Function)
        // Desc: Equality check operator overload.
        //-----------------------------------------------------------------------------
        public static bool operator == (ShortcutBinding b0, ShortcutBinding b1)
        {
            return b0.alt == b1.alt && b0.control == b1.control && b0.command == b1.command &&
                   b0.shift == b1.shift && b0.leftMB == b1.leftMB && b0.middleMB == b1.middleMB &&
                   b0.rightMB == b1.rightMB && b0.key == b1.key;
        }

        //-----------------------------------------------------------------------------
        // Name: operator != (Public Static Function)
        // Desc: Inequality check operator overload.
        //-----------------------------------------------------------------------------
        public static bool operator != (ShortcutBinding b0, ShortcutBinding b1)
        {
            return b0.alt != b1.alt || b0.control != b1.control || b0.command != b1.command ||
                   b0.shift != b1.shift || b0.leftMB != b1.leftMB || b0.middleMB != b1.middleMB ||
                   b0.rightMB != b1.rightMB || b0.key != b1.key;
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: Equals (Public Function)
        // Desc: Checks for equality between 2 'ShortcutBinding' instances.
        // Parm: other - The other 'ShortcutBinding' instance. 
        // Rtrn: True if 'this' 'ShortcutBinding' is equal to 'other'.
        //-----------------------------------------------------------------------------
        public override bool Equals(object other)
        {
            // Not of the required type?
            if (!(other is ShortcutBinding))
                return false;

            // Compare
            return this == (ShortcutBinding)other;
        }

        //-----------------------------------------------------------------------------
        // Name: GetHashCode (Public Function)
        // Desc: Returns the hash code.
        // Rtrn: The hash code.
        //-----------------------------------------------------------------------------
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: Shortcut (Public Class)
    // Desc: Represents a key binding that triggers a command in a specified context.
    //-----------------------------------------------------------------------------
    [Serializable] public class Shortcut
    {
        #region Private Fields
        [SerializeField]        ShortcutBinding     mDefaultBinding             = new ShortcutBinding();    // Default shortcut binding
        [SerializeField]        ShortcutBinding     mBinding                    = new ShortcutBinding();    // Shortcut binding
        [SerializeField]        bool                mEnabled                    = true;                     // Is the shortcut enabled?
        [SerializeField]        string              mName                       = string.Empty;             // Shortcut name
        [SerializeField]        string              mBindingDisplayText         = string.Empty;             // Textual representation of the shortcut's binding used for display purposes (e.g. UI)
        [SerializeField]        bool                mStrictModifierCheck        = true;                     // If true, strict checking is done on the modifiers (e.g. if the shortcut doesn't use the Shift key, but the Shift key is pressed, it won't be activated)
        [SerializeField]        bool                mIgnoreUIKeyboardCapture    = false;                    // If true, the shortcut can activate even when UI captures keyboard input (e.g. text field editing)
        [SerializeReference]    Command             mCommand;                                               // The command which is triggered when the shortcut's binding is activated
        [SerializeReference]    Context             mContext;                                               // The context in which the shortcut's command can be triggered  
        [SerializeField]        List<string>        mConflicts                  = new List<string>();       // List of names of conflicting shortcuts
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: defaultBinding (Public Property)
        // Desc: Returns or sets the shortcut's default binding. If the binding's 'key'
        //       field is a modifier key, it will be set to 'None'.
        //-----------------------------------------------------------------------------
        public ShortcutBinding  defaultBinding  { get { return mDefaultBinding; } set { mDefaultBinding = value; if (IsModifierKey(mDefaultBinding.key)) mDefaultBinding.key = Key.None; } }

        //-----------------------------------------------------------------------------
        // Name: binding (Public Property)
        // Desc: Returns or sets the shortcut's binding. If the binding's 'key' field
        //       is a modifier key, it will be set to 'None'.
        //-----------------------------------------------------------------------------
        public ShortcutBinding  binding         { get { return mBinding; } set { mBinding = value; if (IsModifierKey(mBinding.key)) mBinding.key = Key.None; UpdateBindingDisplayText(); } }

        //-----------------------------------------------------------------------------
        // Name: enabled (Public Property)
        // Desc: Returns or sets whether the shortcut is enabled. A disabled shortcut
        //       doesn't trigger it's attached command.
        //-----------------------------------------------------------------------------
        public bool     enabled     { get { return mEnabled; } set { mEnabled = value; } }

        //-----------------------------------------------------------------------------
        // Name: alt (Public Property)
        // Desc: Returns or sets whether the Alt key is part of the shortcut binding.
        //-----------------------------------------------------------------------------
        public bool     alt         { get { return mBinding.alt; } set { mBinding.alt = value; UpdateBindingDisplayText(); } } 
        
        //-----------------------------------------------------------------------------
        // Name: control (Public Property)
        // Desc: Returns or sets whether the Control key is part of the shortcut binding.
        //-----------------------------------------------------------------------------
        public bool     control     { get { return mBinding.control; } set { mBinding.control = value; UpdateBindingDisplayText(); } } 
        
        //-----------------------------------------------------------------------------
        // Name: cmd (Public Property)
        // Desc: Returns or sets whether the Command key is part of the shortcut binding.
        //-----------------------------------------------------------------------------
        public bool     cmd         { get { return mBinding.command; } set { mBinding.command = value; UpdateBindingDisplayText(); } } 
        
        //-----------------------------------------------------------------------------
        // Name: shift (Public Property)
        // Desc: Returns or sets whether the Shift key is part of the shortcut binding.
        //-----------------------------------------------------------------------------
        public bool     shift       { get { return mBinding.shift; } set { mBinding.shift = value; UpdateBindingDisplayText(); } } 
        
        //-----------------------------------------------------------------------------
        // Name: leftMB (Public Property)
        // Desc: Returns or sets whether the left mouse button is part of the shortcut
        //       binding.
        //-----------------------------------------------------------------------------
        public bool     leftMB      { get { return mBinding.leftMB; } set { mBinding.leftMB = value; UpdateBindingDisplayText(); } }    

        //-----------------------------------------------------------------------------
        // Name: middleMB (Public Property)
        // Desc: Returns or sets whether the middle mouse button is part of the shortcut
        //       binding.
        //-----------------------------------------------------------------------------
        public bool     middleMB    { get { return mBinding.middleMB; } set { mBinding.middleMB = value; UpdateBindingDisplayText(); } }

        //-----------------------------------------------------------------------------
        // Name: rightMB (Public Property)
        // Desc: Returns or sets whether the right mouse button is part of the shortcut
        //       binding.
        //-----------------------------------------------------------------------------
        public bool     rightMB     { get { return mBinding.rightMB; } set { mBinding.rightMB = value; UpdateBindingDisplayText(); } }  

        //-----------------------------------------------------------------------------
        // Name: key (Public Property)
        // Desc: Returns or sets the non-modifier key which is part of the binding. If 
        //       a modifier key is specified, the property will have no effect.
        //-----------------------------------------------------------------------------
        public Key      key         { get { return mBinding.key; } set { if (!IsModifierKey(value)) { mBinding.key = value; UpdateBindingDisplayText(); } } }       
        
        //-----------------------------------------------------------------------------
        // Name: name (Public Property)
        // Desc: Returns the shortcut name.
        //-----------------------------------------------------------------------------
        public string   name        { get { return mName; } }   
        
        //-----------------------------------------------------------------------------
        // Name: bindingDisplayText (Public Property)
        // Desc: Returns the textual representation of the shortcut binding used for
        //       display purposes.
        //-----------------------------------------------------------------------------
        public string       bindingDisplayText  { get { return mBindingDisplayText; } }

        //-----------------------------------------------------------------------------
        // Name: strictModifierCheck (Public Property)
        // Desc: Returns or sets whether strict modifier checking is enabled. If true,
        //       the shortcut will not be activated if there are extra modifier keys
        //       pressed compared to what the shortcut uses (e.g. if the shortcut doesn't
        //       use the Shift key, but the Shift key is pressed, it won't be activated).
        //-----------------------------------------------------------------------------
        public bool         strictModifierCheck { get { return mStrictModifierCheck; } set { mStrictModifierCheck = value; } }

        //-----------------------------------------------------------------------------
        // Name: ignoreUIKeyboardCapture (Public Property)
        // Desc: Returns or sets whether this shortcut is allowed to activate even when
        //       the UI currently captures keyboard typing input (e.g. while editing a
        //       text field). This is typically enabled for global commands such as
        //       Undo and Redo.
        //-----------------------------------------------------------------------------
        public bool ignoreUIKeyboardCapture
        {
            get { return mIgnoreUIKeyboardCapture; }
            set { mIgnoreUIKeyboardCapture = value; }
        }
        
        //-----------------------------------------------------------------------------
        // Name: commandType (Public Property)
        // Desc: Returns the shortcut's command type.
        //-----------------------------------------------------------------------------
        public ECommandType commandType         { get { return mCommand.commandType; } }   
        
        //-----------------------------------------------------------------------------
        // Name: conflictCount (Public Property)
        // Desc: Returns the number of conflicts that have been registered with this
        //       shortcut.
        //-----------------------------------------------------------------------------
        public int          conflictCount       { get { return mConflicts.Count; } }

        //-----------------------------------------------------------------------------
        // Name: contextName (Public Property)
        // Desc: Returns the name of the shortcut's context.
        //-----------------------------------------------------------------------------
        public string       contextName         { get { return mContext != null ? mContext.name : string.Empty; } }

        //-----------------------------------------------------------------------------
        // Name: isCommandActive (Public Property)
        // Desc: Returns true if the shortcut's command is active.
        //-----------------------------------------------------------------------------
        public bool         isCommandActive     { get { return mCommand.isActive; } }
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------  
        // Name: Shortcut() (Public Constructor)
        // Desc: Creates a shortcut with the specified name, command and context.
        // Parm: name          - Shortcut name.
        //       command       - The command which must be triggered when the shortcut is active.
        //       context       - The context in which the shortcut's command can be triggered.
        //       bindingString - Shortcut binding in text format. If valid, this will also
        //                       be used as the shortcut's default binding. If null or invalid,
        //                       the shortcut will be created without a binding.
        //-----------------------------------------------------------------------------  
        public Shortcut(string name, Command command, Context context, string bindingString)
        {
            // Assign name and validate it
            mName = name;
            if (string.IsNullOrEmpty(mName))
                RTG.Exception(nameof(Shortcut), nameof(Shortcut), "A shortcut must have a valid name.");

            // Assign command and validate it
            mCommand = command;
            if (mCommand == null)
                RTG.Exception(nameof(Shortcut), nameof(Shortcut), "The shortcut command was null.");

            // Assign context and validate it
            mContext = context;
            if (mContext == null)
                RTG.Exception(nameof(Shortcut), nameof(Shortcut), "The shortcut context was null.");

            // Ensure no key by default
            mBinding.key = Key.None;

            // If a binding text was specified, parse it. Otherwise, just update the binding display text.
            if (!string.IsNullOrEmpty(bindingString))
            {
                // Set the binding string and if successful, set the default binding
                if (SetBinding(bindingString))
                    mDefaultBinding = mBinding;
            }
            else UpdateBindingDisplayText();
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: IsModifierKey() (Public Static Function)
        // Desc: Checks if the specified key is a modifier key recognized by the 'Shortcut'
        //       class. Recognized modifier keys are the left and right Alt, Control, Command
        //       and Shift.
        // Parm: key - Query key.
        // Rtrn: True if the specified key is a modifier key and false otherwise.
        //-----------------------------------------------------------------------------
        public static bool IsModifierKey(Key key)
        {
            return key == Key.LeftAlt       || key == Key.RightAlt      ||
                   key == Key.LeftCtrl      || key == Key.RightCtrl     ||
                   key == Key.LeftShift     || key == Key.RightShift    ||
                   key == Key.LeftCommand   || key == Key.RightCommand;
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: SyncDefinition() (Public Function)
        // Desc: Synchronizes the shortcut's command and context with the specified
        //       instances. If the types differ, they are replaced. Binding and enabled
        //       state are preserved.
        // Parm: command - The command instance which defines the shortcut behavior.
        //       context - The context instance in which the shortcut is valid.
        //-----------------------------------------------------------------------------
        public void SyncDefinition(Command command, Context context)
        {
            // Validate input
            if (command == null || context == null)
                return;

            // Replace command if type differs
            if (mCommand == null || mCommand.GetType() != command.GetType())
                mCommand = command;

            // Replace context if type differs
            if (mContext == null || mContext.GetType() != context.GetType())
                mContext = context;
        }

        //-----------------------------------------------------------------------------
        // Name: ClearBinding() (Public Function)
        // Desc: Clears the shortcut's binding.
        //-----------------------------------------------------------------------------
        public void ClearBinding()
        {
            mBinding = new ShortcutBinding();
            UpdateBindingDisplayText();
        }

        //-----------------------------------------------------------------------------
        // Name: ClearConflicts() (Public Function)
        // Desc: Clears the list of conflicts.
        //-----------------------------------------------------------------------------
        public void ClearConflicts()
        {
            mConflicts.Clear();
        }

        //-----------------------------------------------------------------------------
        // Name: AddConflict() (Public Function)
        // Desc: Adds the specified shortcut name to the conflict list if it doesn't
        //       already exist.
        // Parm: conflictName - Name of the conflicting shortcut.
        //-----------------------------------------------------------------------------
        public void AddConflict(string conflictName)
        {
            // No-op?
            if (string.IsNullOrEmpty(conflictName) || conflictName == mName)
                return;

            // Already exists?
            if (mConflicts.Contains(conflictName))
                return;

            // Add conflict name
            mConflicts.Add(conflictName);
        }

        //-----------------------------------------------------------------------------
        // Name: GetConflictName() (Public Function)
        // Desc: Returns the shortcut conflict name with the specified index.
        // Parm: index - Index of conflict whose name must be returned.
        //-----------------------------------------------------------------------------
        public string GetConflictName(int index)
        {
            return mConflicts[index];
        }

        //-----------------------------------------------------------------------------
        // Name: ConflictsWith() (Public Function)
        // Desc: Checks if 'this' shortcut conflicts with 'shortcut'. A conflict exists 
        //       when 2 (or more) shortcuts are assigned to the same category and have
        //       the same binding.
        // Parm: shortcut - The other shortcut which may be in conflict.
        // Rtrn: True of 'this' shortcut conflicts with 'shortcut'.
        //-----------------------------------------------------------------------------
        public bool ConflictsWith(Shortcut shortcut)
        {
            // No-op?
            if (this == shortcut)
                return false;

            // If the shortcuts have different contexts, they can't collide
            if (mContext != shortcut.mContext)
                return false;

            // If we reach this point, it means the contexts are the same. We have a conflict
            // if the shortcuts have the same binding.
            return mBinding == shortcut.mBinding;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateConflictsDisplayText() (Public Function)
        // Desc: Creates a text string that can be used for display purposes.
        // Rtrn: The conflicts text or an empty string if there are no conflicts.
        //-----------------------------------------------------------------------------
        public string CreateConflictsDisplayText()
        {
            // No conflicts?
            if (conflictCount == 0)
                return string.Empty;

            // Build conflict display text
            string conflicts = string.Empty;
            for (int c = 0; c < conflictCount; ++c)
                conflicts += GetConflictName(c) + "\n";

            // Return conflicts string
            return conflicts;
        }

        //-----------------------------------------------------------------------------
        // Name: Evaluate() (Public Function)
        // Desc: Checks the state of the shortcut's binding and activates or deactivates
        //       the shortcut's command. 
        // Rtrn: EShortcutEvalAction.None        - if nothing happened.
        //       EShortcutEvalAction.Activated   - if the command was activated.
        //       EShortcutEvalAction.Deactivated - if the command was deactivated.
        //-----------------------------------------------------------------------------
        public EShortcutEvalAction Evaluate()
        {
            // Can this shortcut activate?
            bool canActivate = mEnabled && IsBindingActive() && mContext.IsActive();

            // If activation is allowed and this shortcut overrides UI keyboard capture,
            // make sure the UI releases keyboard input before the command is triggered.
            #if RTGUI_PRESENT
            if (canActivate && ignoreUIKeyboardCapture && RTUI.get.isKeyboardCaptured)
            {
                if (!RTUI.get.TryCancelKeyboardCapture())
                    return EShortcutEvalAction.None;
            }
            #endif

            // Activate or deactivate command
            if (canActivate)
                return mCommand.Activate() ? EShortcutEvalAction.Activated : EShortcutEvalAction.None;
            else
                return mCommand.Deactivate() ? EShortcutEvalAction.Deactivated : EShortcutEvalAction.None;
        }

        //-----------------------------------------------------------------------------
        // Name: CanActivate() (Public Function)
        // Desc: Checks if the shortcut can activate in the current input/context state.
        // Rtrn: True if the shortcut can activate and false otherwise.
        //-----------------------------------------------------------------------------
        public bool CanActivate()
        {
            return mEnabled && IsBindingActive() && mContext.IsActive();
        }

        //-----------------------------------------------------------------------------
        // Name: ForceDeactivate() (Public Function)
        // Desc: Forces the shortcut command to deactivate.
        // Rtrn: EShortcutEvalAction.Deactivated if the command was deactivated and
        //       EShortcutEvalAction.None otherwise.
        //-----------------------------------------------------------------------------
        public EShortcutEvalAction ForceDeactivate()
        {
            return mCommand.Deactivate() ? EShortcutEvalAction.Deactivated : EShortcutEvalAction.None;
        }

        //-----------------------------------------------------------------------------
        // Name: GetBindingSpecificity() (Public Function)
        // Desc: Returns how many input parts are used by the shortcut binding.
        // Rtrn: Binding specificity.
        //-----------------------------------------------------------------------------
        public int GetBindingSpecificity()
        {
            int specificity = 0;

            if (mBinding.alt)       ++specificity;
            if (mBinding.control)   ++specificity;
            if (mBinding.command)   ++specificity;
            if (mBinding.shift)     ++specificity;
            if (mBinding.leftMB)    ++specificity;
            if (mBinding.middleMB)  ++specificity;
            if (mBinding.rightMB)   ++specificity;
            if (mBinding.key != Key.None) ++specificity;

            return specificity;
        }

        //-----------------------------------------------------------------------------
        // Name: BindingContains() (Public Function)
        // Desc: Checks if this shortcut binding contains the specified shortcut binding.
        //       For example, F + RMB contains RMB, but RMB does not contain F + RMB.
        // Parm: shortcut - The shortcut whose binding must be contained.
        // Rtrn: True if this shortcut binding contains the specified shortcut binding.
        //-----------------------------------------------------------------------------
        public bool BindingContains(Shortcut shortcut)
        {
            ShortcutBinding other = shortcut.mBinding;

            if (other.alt       && !mBinding.alt)       return false;
            if (other.control   && !mBinding.control)   return false;
            if (other.command   && !mBinding.command)   return false;
            if (other.shift     && !mBinding.shift)     return false;
            if (other.leftMB    && !mBinding.leftMB)    return false;
            if (other.middleMB  && !mBinding.middleMB)  return false;
            if (other.rightMB   && !mBinding.rightMB)   return false;

            if (other.key != Key.None && mBinding.key != other.key)
                return false;

            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: IsMoreSpecificThan() (Public Function)
        // Desc: Checks if this shortcut is more specific than the specified shortcut.
        // Parm: shortcut - Shortcut to compare against.
        // Rtrn: True if this shortcut is more specific and false otherwise.
        //-----------------------------------------------------------------------------
        public bool IsMoreSpecificThan(Shortcut shortcut)
        {
            // Validate args
            if (shortcut == null || shortcut == this)
                return false;

            // Different context?
            if (mContext != shortcut.mContext)
                return false;

            // Does this shortcut contain the other shortcut's binding?
            if (!BindingContains(shortcut))
                return false;

            // Compare specificity
            return GetBindingSpecificity() > shortcut.GetBindingSpecificity();
        }

        //-----------------------------------------------------------------------------
        // Name: IsBindingActive() (Public Function)
        // Desc: Checks if the shortcut's binding is active.
        // Rtrn: True if the binding is active and false otherwise.
        //-----------------------------------------------------------------------------
        public bool IsBindingActive()
        {
            // If the keyboard is captured and this shortcut is not allowed to override
            // UI keyboard capture, the binding can't be active.
            #if RTGUI_PRESENT
            if (RTUI.get.isKeyboardCaptured && !ignoreUIKeyboardCapture)
                return false;
            #endif

            // Cache data
            RTInput rtInput = RTInput.get;

            // Check Alt modifier
            if (mBinding.alt)
            {
                if (!rtInput.KeyPressed(Key.LeftAlt) && !rtInput.KeyPressed(Key.RightAlt))
                    return false;
            }
            else
            if (mStrictModifierCheck)
            {
                if (rtInput.KeyPressed(Key.LeftAlt) || rtInput.KeyPressed(Key.RightAlt))
                    return false;
            }

            // Check Control modifier
            if (mBinding.control)
            {
                if (!rtInput.KeyPressed(Key.LeftCtrl) && !rtInput.KeyPressed(Key.RightCtrl))
                    return false;
            }
            else
            if (mStrictModifierCheck)
            {
                if (rtInput.KeyPressed(Key.LeftCtrl) || rtInput.KeyPressed(Key.RightCtrl))
                    return false;
            }

            // Check Command modifier
            if (mBinding.command)
            {
                if (!rtInput.KeyPressed(Key.LeftCommand) && !rtInput.KeyPressed(Key.RightCommand))
                    return false;
            }
            else
            if (mStrictModifierCheck)
            {
                if (rtInput.KeyPressed(Key.LeftCommand) || rtInput.KeyPressed(Key.RightCommand))
                    return false;
            }

            // Check Shift modifier
            if (mBinding.shift)
            {
                if (!rtInput.KeyPressed(Key.LeftShift) && !rtInput.KeyPressed(Key.RightShift))
                    return false;
            }
            else
            if (mStrictModifierCheck)
            {
                if (rtInput.KeyPressed(Key.LeftShift) || rtInput.KeyPressed(Key.RightShift))
                    return false;
            }

            // Check if the command is UI blocked
            bool isUIBlocked = !mCommand.isActive && RTScene.get.IsUIPointerBlocked();

            // Check mouse buttons
            #if RTGUI_PRESENT
            if (mBinding.leftMB     && (!rtInput.leftMBPressed      || isUIBlocked || RTGUI.IsMouseButtonSuppressed((int)EMouseButton.Left)))   return false;
            if (mBinding.middleMB   && (!rtInput.middleMBPressed    || isUIBlocked || RTGUI.IsMouseButtonSuppressed((int)EMouseButton.Middle))) return false;
            if (mBinding.rightMB    && (!rtInput.rightMBPressed     || isUIBlocked || RTGUI.IsMouseButtonSuppressed((int)EMouseButton.Right)))  return false;
            #else
            if (mBinding.leftMB     && (!rtInput.leftMBPressed      || isUIBlocked)) return false;
            if (mBinding.middleMB   && (!rtInput.middleMBPressed    || isUIBlocked)) return false;
            if (mBinding.rightMB    && (!rtInput.rightMBPressed     || isUIBlocked)) return false;
            #endif

            // Check key
            if (mBinding.key != Key.None && !rtInput.KeyPressed(mBinding.key))
                return false;

            // Active
            return true;
        }

        //-----------------------------------------------------------------------------
        // Name: SetBinding() (Public Function)
        // Desc: Parses the specified binding string and updates the shortcut binding.
        //       This allows the client to specify the shortcut binding in text format.
        // Parm: bindingString - Shortcut binding in text format. If the format is invalid,
        //                       the old binding will be preserved.
        // Rtrn: True if the binding is successfully updated and false otherwise.
        //-----------------------------------------------------------------------------
        bool SetBinding(string bindingString)
        {
            // Validate call
            if (string.IsNullOrEmpty(bindingString))
                return false;

            // This will be the new shortcut binding if everything goes well 
            ShortcutBinding newBinding = new ShortcutBinding();

            // Remove whitespaces and convert to lower case
            bindingString = bindingString.RemoveWhiteSpace().ToLower();

            // Extract tokens
            char[] delimiters = new char[] { ',' };
            string[] tokens = bindingString.Split(delimiters);

            // Loop through each token and check its type
            int tokenCount = tokens.Length;
            for (int i = 0; i < tokenCount; ++i)
            {
                // Identify token type
                if (tokens[i] == "alt")                 newBinding.alt          = true;
                else if (tokens[i] == "shift")          newBinding.shift        = true;
                else if (tokens[i] == "ctrl")           newBinding.control      = true;
                else if (tokens[i] == "cmd")            newBinding.command      = true;
                else if (tokens[i] == "lmb")            newBinding.leftMB       = true;
                else if (tokens[i] == "mmb")            newBinding.middleMB     = true;
                else if (tokens[i] == "rmb")            newBinding.rightMB      = true;
                else
                {
                    // This can only be a keycode
                    if (KeyEx.KeyFromText(tokens[i], out Key key))
                    {
                        // Only assign if this is not a modifier
                        if (!IsModifierKey(key))
                            newBinding.key = key;
                    }
                    else return false;
                }
            }

            // Set the new binding 
            mBinding        = newBinding;

            // Update display text
            UpdateBindingDisplayText();

            // Success!
            return true;
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: UpdateBindingDisplayText() (Private Function)
        // Desc: When the binding changes, this function is called to update the binding's
        //       textual representation which is used for display purposes.
        //-----------------------------------------------------------------------------
        void UpdateBindingDisplayText()
        {
            // Start out with an empty string
            mBindingDisplayText = string.Empty;

            // Check for modifiers
            if (mBinding.alt)       mBindingDisplayText += "Alt + ";
            if (mBinding.control)   mBindingDisplayText += "Control + ";
            if (mBinding.command)   mBindingDisplayText += "Command + ";
            if (mBinding.shift)     mBindingDisplayText += "Shift + ";

            // Include the key if one is present
            if (mBinding.key != Key.None) mBindingDisplayText += mBinding.key.KeyToText() + " + ";

            // Check for mouse buttons
            if (mBinding.leftMB)    mBindingDisplayText += "Left MB + ";
            if (mBinding.middleMB)  mBindingDisplayText += "Middle MB + ";
            if (mBinding.rightMB)   mBindingDisplayText += "Right MB + ";

            // Remove trailing " + " if present
            if (mBindingDisplayText.Length > 3 &&
                mBindingDisplayText[mBindingDisplayText.Length - 3] == ' ' &&
                mBindingDisplayText[mBindingDisplayText.Length - 2] == '+' &&
                mBindingDisplayText[mBindingDisplayText.Length - 1] == ' ') mBindingDisplayText = mBindingDisplayText.Remove(mBindingDisplayText.Length - 3, 3);
        }
        #endregion
    }
    #endregion
}
