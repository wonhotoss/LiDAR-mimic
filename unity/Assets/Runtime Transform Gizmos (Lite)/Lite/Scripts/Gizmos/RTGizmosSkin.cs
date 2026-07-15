using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTGizmosSkin (Public Class)
    // Desc: Represents a gizmos skin which can be used by the gizmos engine when
    //       drawing gizmos. A skin is essentially a collection of style properties
    //       used to draw different kinds of gizmo handles.
    //-----------------------------------------------------------------------------
    [CreateAssetMenu(fileName = "RTGizmosSkin", menuName = "RTG/RTGizmosSkin", order = 1)]
    public class RTGizmosSkin : ScriptableObject
    {
        #region Private Enumerations
        //-----------------------------------------------------------------------------
        // Name: EGizmoType (Private Enum)
        // Desc: Defines different types of gizmos whose styles can be configured in the UI.
        //-----------------------------------------------------------------------------
        enum EGizmoType
        {
            Global = 0,
            View,
            Move,
            Rotate,
            Scale,
            TRS,
        }
        #endregion

        #region Private Fields
        #if UNITY_EDITOR
        [SerializeField] EGizmoType         mGizmoType = EGizmoType.Global;     // The currently selected gizmo type
        #endif
        [SerializeField] bool               mInitialized;                       // Has the skin been initialized?

        [SerializeField] GlobalGizmoStyle               mGlobalGizmoStyle               = new GlobalGizmoStyle();               // Global gizmo style
        [SerializeField] ViewGizmoStyle                 mViewGizmoStyle                 = new ViewGizmoStyle();                 // View gizmo style
        [SerializeField] MoveGizmoStyle                 mMoveGizmoStyle                 = new MoveGizmoStyle();                 // Move gizmo style
        [SerializeField] RotateGizmoStyle               mRotateGizmoStyle               = new RotateGizmoStyle();               // Rotate gizmo style
        [SerializeField] ScaleGizmoStyle                mScaleGizmoStyle                = new ScaleGizmoStyle();                // Scale gizmo style
        [SerializeField] TRSGizmoStyle                  mTRSGizmoStyle                  = new TRSGizmoStyle();                  // TRS gizmo style
        #endregion

        #region Public Properties
        //-----------------------------------------------------------------------------
        // Name: globalGizmoStyle (Public Property)
        // Desc: Returns the global gizmo style.
        //-----------------------------------------------------------------------------
        public GlobalGizmoStyle     globalGizmoStyle    { get { return mGlobalGizmoStyle; } }

        //-----------------------------------------------------------------------------
        // Name: viewGizmoStyle (Public Property)
        // Desc: Returns the view gizmo style.
        //-----------------------------------------------------------------------------
        public ViewGizmoStyle       viewGizmoStyle      { get { return mViewGizmoStyle; } }

        //-----------------------------------------------------------------------------
        // Name: moveGizmoStyle (Public Property)
        // Desc: Returns the move gizmo style.
        //-----------------------------------------------------------------------------
        public MoveGizmoStyle       moveGizmoStyle      { get { return mMoveGizmoStyle; } }

        //-----------------------------------------------------------------------------
        // Name: rotateGizmoStyle (Public Property)
        // Desc: Returns the rotate gizmo style.
        //-----------------------------------------------------------------------------
        public RotateGizmoStyle     rotateGizmoStyle    { get { return mRotateGizmoStyle; } }

        //-----------------------------------------------------------------------------
        // Name: scaleGizmoStyle (Public Property)
        // Desc: Returns the scale gizmo style.
        //-----------------------------------------------------------------------------
        public ScaleGizmoStyle      scaleGizmoStyle     { get { return mScaleGizmoStyle; } }
        
        //-----------------------------------------------------------------------------
        // Name: trsGizmoStyle (Public Property)
        // Desc: Returns the TRS gizmo style.
        //-----------------------------------------------------------------------------
        public TRSGizmoStyle        trsGizmoStyle       { get { return mTRSGizmoStyle; } }
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: UseDefaults() (Public Function)
        // Desc: Use default settings.
        //-----------------------------------------------------------------------------
        public void UseDefaults()
        {
            globalGizmoStyle.UseDefaults(this);
            viewGizmoStyle.UseDefaults(this);
            moveGizmoStyle.UseDefaults(this);
            rotateGizmoStyle.UseDefaults(this);
            scaleGizmoStyle.UseDefaults(this);
            trsGizmoStyle.UseDefaults(this);
        }

        #if UNITY_EDITOR
        //-----------------------------------------------------------------------------
        // Name: OnEditorGUI() (Public Function)
        // Desc: Draws the Editor UI which allows the user to change the skin properties.
        //-----------------------------------------------------------------------------
        public void OnEditorGUI()
        {
            // Toolbar button rows
            var buttonRows = new string[][]
            {
                new string[] { "Global", "View", "Move", "Rotate", "Scale", "TRS" },
            };

            // Draw multi-row toolbar
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorUI.MultiRowToolbar((int)mGizmoType, buttonRows);
            if (EditorGUI.EndChangeCheck())
            {
                this.OnWillChangeInEditor();
                mGizmoType = (EGizmoType)newIndex;
            }

            // Draw selected gizmo style UI
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                switch (mGizmoType)
                {
                    case EGizmoType.Global:

                        globalGizmoStyle.DrawEditorGUI(this, "Global Gizmo Style", true);
                        break;

                    case EGizmoType.View:

                        viewGizmoStyle.DrawEditorGUI(this, "View Gizmo Style", true);
                        break;

                    case EGizmoType.Move:

                        moveGizmoStyle.DrawEditorGUI(this, "Move Gizmo Style", true);
                        break;

                    case EGizmoType.Rotate:

                        rotateGizmoStyle.DrawEditorGUI(this, "Rotate Gizmo Style", true);
                        break;

                    case EGizmoType.Scale:

                        scaleGizmoStyle.DrawEditorGUI(this, "Scale Gizmo Style", true);
                        break;

                    case EGizmoType.TRS:

                        trsGizmoStyle.DrawEditorGUI(this, "TRS Gizmo Style", true);
                        break;
                }
            }
        }
        #endif
        #endregion

        #region Private Functions
        #if UNITY_EDITOR
        //-----------------------------------------------------------------------------
        // Name: OnEnable() (Public Function)
        // Desc: Called when the object is enabled.
        //-----------------------------------------------------------------------------
        void OnEnable()
        {
            // Initialize if necessary
            if (!mInitialized)
            {
                UseDefaults();
                mInitialized = true;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: Reset() (Public Function)
        // Desc: Called the the object is reset.
        //-----------------------------------------------------------------------------
        void Reset()
        {
            UseDefaults();
        }
        #endif
        #endregion
    }
    #endregion
}
