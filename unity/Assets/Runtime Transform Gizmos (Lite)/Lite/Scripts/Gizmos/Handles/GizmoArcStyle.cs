using UnityEngine;
using System;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: GizmoArcStyle (Public Class)
    // Desc: Stores style properties for a gizmo arc handle (e.g. rotation gizmo
    //       rotation indicator).
    //-----------------------------------------------------------------------------
    [Serializable] public class GizmoArcStyle : GizmoHandleStyle
    {
        #region Private Fields
        [SerializeField] Color mColor       = defaultColor;         // Color
        [SerializeField] Color mBorderColor = defaultBorderColor;   // Border color
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: color (Public Property)
        // Desc: Returns or sets the arc color.
        //-----------------------------------------------------------------------------
        public Color color          { get { return mColor; } set { mColor = value; } }

        //-----------------------------------------------------------------------------
        // Name: borderColor (Public Property)
        // Desc: Returns or sets the arc border color.
        //-----------------------------------------------------------------------------
        public Color borderColor    { get { return mBorderColor; } set { mBorderColor = value; } }
        #endregion

        #region Public Static Properties
        public static Color defaultColor        { get { return Color.yellow.WithAlpha(0.1f); } }
        public static Color defaultBorderColor  { get { return Color.yellow.WithAlpha(0.7f); } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: CloneStyle() (Public Function)
        // Desc: Clones the style.
        // Rtrn: The cloned style.
        //-----------------------------------------------------------------------------
        public GizmoArcStyle CloneStyle()
        {
            return MemberwiseClone() as GizmoArcStyle;
        }
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: OnUseDefaults() (Protected Function)
        // Desc: Called in order to set the style properties to default values.
        //-----------------------------------------------------------------------------
        protected override void OnUseDefaults()
        {
            color       = defaultColor;
            borderColor = defaultBorderColor;
        }
        #endregion
    }
    #endregion
}
