using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: TRSGizmoStyle (Public Class)
    // Desc: Stores style properties for TRS gizmos.
    //-----------------------------------------------------------------------------
    [Serializable] public class TRSGizmoStyle : GizmoStyle
    {
        #region Private Enumerations
        //-----------------------------------------------------------------------------
        // Name: EStyleCategory (Private Enum)
        // Desc: Defines different TRS gizmo style categories. This can be used to
        //       differentiate between move, rotate and scale properties in the UI for
        //       example.
        //-----------------------------------------------------------------------------
        enum EStyleCategory
        {
            Move = 0,
            Rotate,
            Scale,
        }
        #endregion

        #region Private Fields
        #if UNITY_EDITOR
        [SerializeField] EStyleCategory     mStyleCategory  = EStyleCategory.Move;      // The currently selected style category that is visible in the UI
        #endif
        [SerializeField] MoveGizmoStyle     mMoveStyle      = new MoveGizmoStyle();     // Style used by the move handles
        [SerializeField] RotateGizmoStyle   mRotateStyle    = new RotateGizmoStyle();   // Style used by the rotate handles
        [SerializeField] ScaleGizmoStyle    mScaleStyle     = new ScaleGizmoStyle();    // Style used by the scale handles
        #endregion

        #region Public Static Properties
        public static float defaultMoveDblAxisSize      { get { return 1.4f; } }
        public static float defaultScaleAxisLength      { get { return 1.0f; } }
        public static float defaultScaleAxisOffset      { get { return 5.0f; } }
        public static float defaultScaleDblAxisOffset   { get { return 1.5f; } }
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: moveStyle() (Public Property)
        // Desc: Returns the style used by the move handles.
        //-----------------------------------------------------------------------------
        public MoveGizmoStyle   moveStyle   { get { return mMoveStyle; } }

        //-----------------------------------------------------------------------------
        // Name: rotateStyle() (Public Property)
        // Desc: Returns the style used by the rotate handles.
        //-----------------------------------------------------------------------------
        public RotateGizmoStyle rotateStyle { get { return mRotateStyle; } }

        //-----------------------------------------------------------------------------
        // Name: scaleStyle() (Public Property)
        // Desc: Returns the style used by the scale handles.
        //-----------------------------------------------------------------------------
        public ScaleGizmoStyle  scaleStyle  { get { return mScaleStyle; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: CloneStyle() (Public Function)
        // Desc: Clones the style.
        // Rtrn: The cloned style.
        //-----------------------------------------------------------------------------
        public TRSGizmoStyle CloneStyle()
        {
            var clone           = MemberwiseClone() as TRSGizmoStyle;
            clone.mMoveStyle    = mMoveStyle.CloneStyle();
            clone.mRotateStyle  = mRotateStyle.CloneStyle();
            clone.mScaleStyle   = mScaleStyle.CloneStyle();
            return clone;
        }
        #endregion

        #region Protected Functions
        //-----------------------------------------------------------------------------
        // Name: OnUseDefaults() (Protected Function)
        // Desc: Called in order to set the style properties to default values.
        // Parm: parentObject - The serializable parent object used to record property
        //                      changes.
        //-----------------------------------------------------------------------------
        protected override void OnUseDefaults(UnityEngine.Object parentObject)
        {
            moveStyle.UseDefaults(parentObject);
            rotateStyle.UseDefaults(parentObject);
            scaleStyle.UseDefaults(parentObject);
            scaleStyle.axisOffset       = defaultScaleAxisOffset;
            scaleStyle.dblAxisOffset    = defaultScaleDblAxisOffset;

            for (int i = 0; i < 3; ++i)
            {
                moveStyle.GetDblAxisSliderStyle((EPlane)i).quadWidth = defaultMoveDblAxisSize;
                moveStyle.GetDblAxisSliderStyle((EPlane)i).quadHeight = defaultMoveDblAxisSize;

                scaleStyle.GetAxisSliderStyle(i, true).length  = defaultScaleAxisLength;
                scaleStyle.GetAxisSliderStyle(i, false).length = defaultScaleAxisLength;
            }
        }

        #if UNITY_EDITOR
        //-----------------------------------------------------------------------------
        // Name: OnEditorGUI() (Protected Function)
        // Desc: Called when the style's GUI must be rendered inside the Unity Editor.
        // Parm: parentObject - The serializable parent object used to record property
        //                      changes.
        //-----------------------------------------------------------------------------
        protected override void OnEditorGUI(UnityEngine.Object parentObject)
        {
            // Style category selection
            EditorGUILayout.Separator();
            EditorGUI.BeginChangeCheck();
            var newStyleCategory = (EStyleCategory)GUILayout.Toolbar((int)mStyleCategory, 
                new string[] { "Move", "Rotate", "Scale" });
            if (EditorGUI.EndChangeCheck())
            {
                parentObject.OnWillChangeInEditor();
                mStyleCategory = newStyleCategory;
            }

            // Draw UI based on the selected style category
            switch (mStyleCategory)
            {
                case EStyleCategory.Move:

                    moveStyle.DrawEditorGUI(parentObject, string.Empty, false, false);
                    break;

                case EStyleCategory.Rotate:

                    rotateStyle.DrawEditorGUI(parentObject, string.Empty, false, false);
                    break;

                case EStyleCategory.Scale:

                    scaleStyle.DrawEditorGUI(parentObject, string.Empty, false, false);
                    break;
            }
        }
        #endif
        #endregion
    }
    #endregion
}