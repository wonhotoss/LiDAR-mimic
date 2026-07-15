#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RTGLite
{
    #pragma warning disable CS0618 // Unity TreeView API is obsolete but still required for editor UI
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: ShortcutList (Public Class)
    // Desc: Implements the shortcut list which displays shortcuts in a specified
    //       shortcut category.
    //-----------------------------------------------------------------------------
    public class ShortcutList : TreeView
    {
        #region Private Fields
        ShortcutCategory    mShortcutCategory;  // The list displays the shortcuts inside this category
        Shortcut            mEditShortcut;      // Current edit shortcut. When the user double-clicks on the binding text, they are allowed to edit the shortcut binding.
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: shortcutCategory (Public Property)
        // Desc: Returns or sets the shortcut category whose shortcuts are displayed inside 
        //       the list. Setting the shortcut category also refreshes the list. The setter
        //       has no effect if the new category is the same as the current one.
        //-----------------------------------------------------------------------------
        public ShortcutCategory shortcutCategory 
        { 
            get 
            { 
                return mShortcutCategory; 
            } 

            set 
            { 
                if (value == null)
                    return;

                mEditShortcut     = null;
                mShortcutCategory = value;
                Reload();
            } 
        }
        #endregion

        #region Public Constructors
        //-----------------------------------------------------------------------------
        // Name: ShortcutList() (Public Constructor)
        // Desc: Creates the shortcut list.
        // Parm: listState          - List state object.
        //       columnHeade        - Column header.
        //       shortcutCategory   - The shortcut category whose shortcuts are displayed 
        //                            inside the list.
        //-----------------------------------------------------------------------------
        public ShortcutList(TreeViewState listState, MultiColumnHeader columnHeader, ShortcutCategory shortcutCategory)
            : base(listState, columnHeader)
        {
            // Set shortcut category and validate
            mShortcutCategory   = shortcutCategory;
            if (mShortcutCategory == null)
                RTG.Exception(nameof(ShortcutList), nameof(ShortcutList), "Shortcut category was null.");

            // Init
            showBorder          = true;
            columnHeader.height = EditorUI.defaultHeaderHeight;
            Reload();
        }
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: BuildRoot() (Protected Function)
        // Desc: Creates the items in the list.
        //-----------------------------------------------------------------------------
        protected override TreeViewItem BuildRoot()
        {
            // Create the root item
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };

            // Loop through each shortcut in the shortcut category          
            int count   = shortcutCategory.shortcutCount;
            for (int i = 0; i < count; ++i)
            {
                // Create a tree view item for this shortcut
                var item = new TreeViewItem { id = i, depth = 0, displayName = shortcutCategory[i].name };
                root.AddChild(item);
            }

            // Return root
            return root;
        }

        //-----------------------------------------------------------------------------
        // Name: CanMultiSelect() (Protected Function)
        // Desc: Returns whether or no the specified item can be part of a multiselection.
        // Parm: item - Query item.
        //-----------------------------------------------------------------------------
        protected override bool CanMultiSelect(TreeViewItem item)
        {
            // Don't allow multiselection
            return false;
        }

        //-----------------------------------------------------------------------------
        // Name: SelectionChanged() (Protected Function)
        // Desc: Event handler for the selection changed event.
        //-----------------------------------------------------------------------------
        protected override void SelectionChanged(IList<int> selectedIds)
        {
            // Exit edit mode when the selection changes
            mEditShortcut = null;
        }

        //-----------------------------------------------------------------------------
        // Name: RowGUI() (Protected Function)
        // Desc: Called when a row has to be rendered.
        // Parm: args - Arguments needed to render the row GUI.
        //-----------------------------------------------------------------------------
        protected override void RowGUI(RowGUIArgs args)
        {
            bool newBool;
            var s = shortcutCategory[args.row]; // The shortcut associated with this row

            // Loop through each visible column
            int count = args.GetNumVisibleColumns();
            for (int i = 0; i < count; ++i)
            {
                // Cache data
                GUIRect cellRect = (GUIRect)args.GetCellRect(i);

                // What column are we dealing with?
                switch (i)
                {
                    case 0:

                        // Enabled toggle
                        EditorGUI.BeginChangeCheck();
                        newBool = GUI.Toggle(cellRect.WithWidth(15.0f).AlignRight(cellRect), s.enabled, string.Empty);
                        if (EditorGUI.EndChangeCheck())
                        {
                            RTInput.get.OnWillChangeInEditor();
                            s.enabled = newBool;
                        }
                        break;

                    case 1:

                        // Name
                        GUI.Label(cellRect, s.name);
                        break;

                    case 2:

                        // Context
                        GUI.Label(cellRect, s.contextName);
                        break;

                    case 3:

                        // Type
                        GUI.Label(cellRect, s.commandType.ToString());
                        break;

                    case 4:

                        // Enable edit mode?
                        Event e = Event.current;
                        if (e.type == EventType.MouseDown && e.button == 0 && e.clickCount == 2)
                        {
                            // If we clicked inside the binding column, start editing this shortcut.
                            // Otherwise, exit edit mode.
                            if (cellRect.Contains(e.mousePosition))
                            {
                                mEditShortcut = s;
                                e.Use();
                            }
                            else
                            if (mEditShortcut != null)
                            {
                                mEditShortcut = null;
                                e.Use();
                            }
                        }

                        // Are we editing this shortcut?
                        if (mEditShortcut == s)
                        {
                            // Display a message informing the user to press a key
                            EditorUI.PushGUIColor(Color.green);
                            GUI.Label(cellRect, "[Enter binding]");
                            EditorUI.PopGUIColor();

                            // Capture key presses
                            if (e.type == EventType.KeyUp)
                            {
                                // Create a new binding
                                ShortcutBinding binding = new ShortcutBinding();

                                // Set keycode but filter modifiers. Modifiers are treated differently.
                                Key key = KeyEx.KeyFromKeyCode(e.keyCode);
                                bool isModifier = Shortcut.IsModifierKey(key);
                                if (key != Key.None && !isModifier)
                                    binding.key = KeyEx.KeyFromKeyCode(e.keyCode);

                                // Check for any modifiers that accompany this key
                                if (RTInput.get.altPresed)      binding.alt     = true;
                                if (RTInput.get.shiftPressed)   binding.shift   = true;
                                if (RTInput.get.ctrlPressed)    binding.control = true;
                                if (RTInput.get.cmdPressed)     binding.command = true;

                                // Set the new binding and detect conflicts
                                s.binding = binding;
                                RTInput.get.shortcutProfileManager.activeProfile.DetectShortcutConflicts();

                                // Exit edit mode
                                mEditShortcut = null;
                            }
                            else
                            // Capture mouse button presses
                            if (e.type == EventType.MouseDown &&
                                cellRect.Contains(e.mousePosition))
                            { 
                                // Create a new binding
                                ShortcutBinding binding = new ShortcutBinding();

                                // Set mouse button
                                if (e.button == 0) binding.leftMB = true;
                                else if (e.button == 1) binding.rightMB = true;
                                else if (e.button == 2) binding.middleMB = true;

                                // Check for any modifiers that accompany this button
                                if (RTInput.get.altPresed)      binding.alt     = true;
                                if (RTInput.get.shiftPressed)   binding.shift   = true;
                                if (RTInput.get.ctrlPressed)    binding.control = true;
                                if (RTInput.get.cmdPressed)     binding.command = true;

                                // Check for any non-modifier key that accompanies this button
                                if (TryGetPressedNonModifierKey(out Key key))
                                    binding.key = key;

                                // Set the new binding and detect conflicts
                                s.binding = binding;
                                RTInput.get.shortcutProfileManager.activeProfile.DetectShortcutConflicts();

                                // Exit edit mode
                                mEditShortcut = null;
                            }
                        }
                        else
                        {
                            // Display the shortcut binding
                            GUI.Label(cellRect, s.bindingDisplayText);

                            // If we have any conflicts, we also need to draw the warning icon
                            int conflictCount = s.conflictCount;
                            if (conflictCount != 0)
                                GUI.DrawTexture(cellRect.WithSize(16.0f, 16.0f).AlignRight(cellRect).OffsetX(-2.0f).CenterYOf(cellRect), TextureManager.warningIcon);

                            break;
                        }
                        break;
                }
            }
        }
        #endregion

        #region Private Static Functions
        //-----------------------------------------------------------------------------
        // Name: TryGetPressedNonModifierKey() (Private Static Function)
        // Desc: Returns the first pressed non-modifier key.
        // Parm: key - The pressed non-modifier key.
        // Rtrn: True if a pressed non-modifier key was found and false otherwise.
        //-----------------------------------------------------------------------------
        static bool TryGetPressedNonModifierKey(out Key key)
        {
            key = Key.None;

            // Must have valid keyboard
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
                return false;

            // Loop through each key
            foreach (var keyControl in keyboard.allKeys)
            {
                // Filter out modifier keys
                Key candidateKey = keyControl.keyCode;
                if (candidateKey == Key.None || Shortcut.IsModifierKey(candidateKey))
                    continue;

                // Ignore if not pressed
                if (!keyControl.isPressed)
                    continue;

                // Found key
                key = candidateKey;
                return true;
            }

            // No key found
            return false;
        }
        #endregion
    }
    #endregion
    #pragma warning restore CS0618
}
#endif