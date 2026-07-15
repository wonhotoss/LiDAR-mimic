#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: CreateEntityWindow (Public Class)
    // Desc: Implements the functionality for an editor window with a simple interface
    //       that allows the user to create entities with a specified name.
    //-----------------------------------------------------------------------------
    public class CreateEntityWindow : PluginEditorWindow
    {
        #region Private Fields
        Label mInfoLabel;   // The label which informs the user what the window allows them to do
        #endregion

        #region Public Static Properties
        //-----------------------------------------------------------------------------
        // Name: entityTag (Public Static Property)
        // Desc: Returns or sets the entity tag (e.g. "shortcut profile"). This is used
        //       for display purposes inside the UI to inform the user about the kind of
        //       entities the window allows them to create.
        //-----------------------------------------------------------------------------
        public static string            entityTag           { get; set; } = "entity";

        //-----------------------------------------------------------------------------
        // Name: initialEntityName (Public Static Property)
        // Desc: Returns or sets the initial entity name. This will be displayed in the
        //       entity name text field when the window is shown. 
        //-----------------------------------------------------------------------------
        public static string            initialEntityName   { get; set; } = "Name";

        //-----------------------------------------------------------------------------
        // Name: createEntity (Public Static Property)
        // Desc: Returns or sets the action to be executed when a new entity has to be
        //       created. The action function accepts a string argument which represents
        //       the name of the entity to be created.
        //-----------------------------------------------------------------------------
        public static Action<string>    createEntity        { get; set; } = null;
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: OnCreateUI() (Protected Function)
        // Desc: Called when the window UI must be created.
        // Parm: parent - Parent visual element where all controls will be added.
        //-----------------------------------------------------------------------------
        protected override void OnCreateUI(VisualElement parent)
        {
            // Init window properties
            minSize = maxSize = new Vector2(400.0f, 100.0f);

            // Create the content container
            VisualElement container = new VisualElement();
            container.style.SetMargin(EditorUI.windowMargin);
            parent.Add(container);

            // Create the info label 
            mInfoLabel = new Label();
            mInfoLabel.text = entityTag.IsPrecededByAn() ? $"Create an {entityTag}" : $"Create a {entityTag}";
            mInfoLabel.style.marginBottom = 5.0f;
            mInfoLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            container.Add(mInfoLabel);

            // Create name field
            TextField nameField = new TextField();
            nameField.value = string.IsNullOrEmpty(initialEntityName) ? "Name" : initialEntityName;
            nameField.label = "Name";           
            container.Add(nameField);
            nameField.FocusEx();
            nameField.SelectAll();

            // Create the label which notifies the user that a valid name is required
            Label enterValidName = new Label();
            enterValidName.text = "You need to enter a valid name.";
            enterValidName.style.color = Color.red;
            enterValidName.visible = false;
            container.Add(enterValidName);

            // Create buttons container
            var buttons = new VisualElement();
            container.Add(buttons);
            buttons.style.marginTop = 15.0f;
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.flexShrink = 0.0f;
            buttons.style.justifyContent = Justify.FlexEnd;

            // Create the button which creates the entity
            var createBtn = new Button();
            buttons.Add(createBtn);
            createBtn.text = "Create";
            createBtn.style.width = 100.0f;
            createBtn.clicked += () => { if (createEntity != null) { createEntity(nameField.text); Close(); } };
            nameField.RegisterValueChangedCallback(p => { createBtn.SetEnabled(!string.IsNullOrEmpty(p.newValue)); });

            // Create the cancel button
            var cancelBtn = new Button();
            buttons.Add(cancelBtn);
            cancelBtn.text = "Cancel";
            cancelBtn.style.width = 100.0f;
            cancelBtn.clicked += () => { Close(); };

            // Register callbacks
            nameField.RegisterValueChangedCallback(e => 
            {
                // Notify user if they haven't entered a valid name and enable/disable the 'Create' button
                if (string.IsNullOrEmpty(e.newValue))
                {
                    enterValidName.visible = true;
                    createBtn.SetEnabled(false);
                }
                else
                {
                    enterValidName.visible = false;
                    createBtn.SetEnabled(true);
                }
            });
        }
        #endregion
    }
    #endregion
}
#endif