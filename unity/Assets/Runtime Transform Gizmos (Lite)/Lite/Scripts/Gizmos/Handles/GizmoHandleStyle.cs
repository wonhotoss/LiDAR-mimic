using UnityEngine;
using System;

namespace RTGLite
{
    #region Public Enumerations
    //-----------------------------------------------------------------------------
    // Name: EGizmoShadeMode (Public Enum)
    // Desc: Defines different kinds of gizmo shade modes.
    //-----------------------------------------------------------------------------
    public enum EGizmoShadeMode
    {
        Lit = 0,    // Lit
        Flat        // Flat/Unlit
    }
    #endregion

    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GizmoHandleStyle (Public Abstract Class)
    // Desc: Stores common style properties for all gizmo handle style classes and
    //       implements relevant functions.
    //-----------------------------------------------------------------------------
    [Serializable] public abstract class GizmoHandleStyle
    {
        #region Private Fields
        [SerializeField] bool mVisible      = defaultVisible;       // Visibility toggle for handles that use this style
        [SerializeField] bool mHoverable    = defaultHoverable;     // Can handles that use this style be hovered?
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: visible (Public Property)
        // Desc: Returns or sets the visibility of handles that use this style.
        //-----------------------------------------------------------------------------
        public bool visible     { get { return mVisible; } set { mVisible = value; } }

        //-----------------------------------------------------------------------------
        // Name: hoverable (Public Property)
        // Desc: Returns or sets whether handles that use this style can be hovered with
        //       the mouse cursor. When setting this to false, any kind of mouse interaction
        //       is disabled for all handles that use this style.
        //-----------------------------------------------------------------------------
        public bool hoverable   { get { return mHoverable; } set { mHoverable = value; } }
        #endregion

        #region Public Static Properties
        public static bool defaultVisible   { get { return true; } }
        public static bool defaultHoverable { get { return true; } }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: Create() (Public Static Function)
        // Desc: Creates a style of the specified type. The difference between calling
        //       this function to create a style and allocating with 'new' is that this
        //       function also calls 'UseDefaults' to initialize the style to default
        //       values. This is done as a separate step because some defaults require
        //       loading resources such as textures and this can't be done in the class
        //       constructor. Therefore, you should not call this inside a constructor.
        // Parm: T - Style type. Must derive from 'GizmoStyle'.
        // Rtrn: The created style.
        //-----------------------------------------------------------------------------
        public static T Create<T>() where T : GizmoHandleStyle, new()
        {
            T style = new T();
            style.UseDefaults();

            return style;
        }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: UseDefaults() (Public Function)
        // Desc: Use default settings.
        //-----------------------------------------------------------------------------
        public void UseDefaults()
        {
            visible     = defaultVisible;
            hoverable   = defaultHoverable;
            OnUseDefaults();
        }
        #endregion

        #region Protected Abstract Functions
        //-----------------------------------------------------------------------------
        // Name: OnUseDefaults() (Protected Abstract Function)
        // Desc: Called in order to set the style properties to default values.
        //-----------------------------------------------------------------------------
        protected abstract void OnUseDefaults();
        #endregion
    }
    #endregion
}
