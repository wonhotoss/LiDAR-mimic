#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: DeleteEntityWindow (Public Class)
    // Desc: Implements the functionality for an editor window with a simple interface
    //       that allows the user to delete entities.
    //-----------------------------------------------------------------------------
    public class DeleteEntityWindow : PluginEditorWindow
    {
        #region Private Fields
        Label mInfoLabel;       // The label which informs the user what the window allows them to do
        Label mQuestionLabel;   // The label which asks the user if they're sure they want to delete
        #endregion

        #region Public Static Properties
        //-----------------------------------------------------------------------------
        // Name: entityTag (Public Static Property)
        // Desc: Returns or sets the entity tag (e.g. "shortcut profile"). This is used
        //       for display purposes inside the UI to inform the user about the kind of
        //       entities the window allows them to delete.
        //-----------------------------------------------------------------------------
        public static string            entityTag       { get; set; } = "entity";

        //-----------------------------------------------------------------------------
        // Name: entityName (Public Static Property)
        // Desc: Returns or sets the name of the entity to delete. This is passed as
        //       argument to the 'deleteEntity' action.
        //-----------------------------------------------------------------------------
        public static string            entityName      { get; set; } = null;

        //-----------------------------------------------------------------------------
        // Name: deleteEntity (Public Static Property)
        // Desc: Returns or sets the action to be executed when the entity with name
        //       'entityName' has to be deleted. The action function accepts a string
        //       argument which represents the name of the entity to be created. This
        //       argument is set to the value of the 'entityName' property.
        //-----------------------------------------------------------------------------
        public static Action<string>    deleteEntity    { get; set; } = null;
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: OnCreateUI() (Protected Function)
        // Desc: Called when the window UI must be created.
        // Parm: parent - Parent visual element where all controls will be added.
        //-----------------------------------------------------------------------------
        protected override void OnCreateUI(VisualElement parent)
        {
            // Validate call
            if (string.IsNullOrEmpty(entityName))
            {
                Close();
                return;
            }

            // Init window properties
            minSize = maxSize = new Vector2(400.0f, 100.0f);

            // Create the content container
            VisualElement container = new VisualElement();
            container.style.SetMargin(EditorUI.windowMargin);
            parent.Add(container);

            // Create the info label 
            mInfoLabel = new Label();
            mInfoLabel.text = entityTag.IsPrecededByAn() ? $"Delete an {entityTag}" : $"Delete a {entityTag}";
            mInfoLabel.style.marginBottom = 5.0f;
            mInfoLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            container.Add(mInfoLabel);

            // Create the question label
            mInfoLabel = new Label();
            mInfoLabel.text = $"Are you sure you want to delete the {entityTag} '{entityName}'?";
            container.Add(mInfoLabel);

            // Create buttons container
            var buttons = new VisualElement();
            container.Add(buttons);
            buttons.style.marginTop = 15.0f;
            buttons.style.flexDirection = FlexDirection.Row;
            buttons.style.flexShrink = 0.0f;
            buttons.style.justifyContent = Justify.FlexEnd;

            // Create the button which deletes the entity
            var deleteBtn = new Button();
            buttons.Add(deleteBtn);
            deleteBtn.text = "Delete";
            deleteBtn.style.width = 100.0f;
            deleteBtn.clicked += () => { if (deleteEntity != null) { deleteEntity(entityName); Close(); } };

            // Create the cancel button
            var cancelBtn = new Button();
            buttons.Add(cancelBtn);
            cancelBtn.text = "Cancel";
            cancelBtn.style.width = 100.0f;
            cancelBtn.clicked += () => { Close(); };
        }
        #endregion
    }
    #endregion
}
#endif