using UnityEngine;
using UnityEngine.UIElements;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: TextFieldEx (Public Static Class)
    // Desc: Contains useful extensions and utility functions for the 'TextField'
    //       class.
    //-----------------------------------------------------------------------------
    public static class TextFieldEx
    {
        #region Public Extensions
        //-----------------------------------------------------------------------------
        // Name: FocusEx() (Public Extension)
        // Desc: Focuses the specified text field. This function is a workaround for
        //       what seems to be a bug which prevents 'TextField.Focus()' from working
        //       correctly. Must be called after the text field is attached to a parent
        //       control. Otherwise the function has no effect.
        //-----------------------------------------------------------------------------
        public static void FocusEx(this TextField textField)
        {
            // Do we have a parent?
            if (textField.parent != null)
            {
                // Schedule a focus action
                textField.parent.schedule.Execute(() => 
                { textField.Focus();});
            }
        }
        #endregion
    }
    #endregion
}
