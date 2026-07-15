using UnityEditor;
using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTCameraInspector (Public Class)
    // Desc: Implements the Inspector UI for the 'RTCamera' Mono.
    //-----------------------------------------------------------------------------
    [CustomEditor(typeof(RTCamera))]
    public class RTCameraInspector : Editor
    {
        #region Private Fields
        RTCamera mTarget;   // Target
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: OnInspectorGUI() (Public Function)
        // Desc: Implements the Inspector UI logic.
        //-----------------------------------------------------------------------------
        public override void OnInspectorGUI() 
        {
            // Settings
            General();
            EditorGUILayout.Separator();
            Navigation();

            // Use defaults button
            if (EditorUI.UseDefaultsButton())
            {
                UndoEx.Record(mTarget);
                mTarget.settings.UseDefaults();
            }

            // Style
            EditorGUILayout.Separator();
            BackgroundStyle();

            // Use defaults button
            if (EditorUI.UseDefaultsButton())
            {
                UndoEx.Record(mTarget);
                mTarget.bgStyle.UseDefaults();
            }
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: General() (Private Function)
        // Desc: Implements the Inspector UI logic for general settings.
        //-----------------------------------------------------------------------------
        void General()
        {
            EditorUI.SectionTitleLabel("General");

            // Target camera
            EditorGUI.BeginChangeCheck();
            Camera newCamera = EditorGUILayout.ObjectField(new GUIContent("Target camera", "The camera which will be manipulated by the user. " +
                "If this field is empty, the Main camera will be used by default. This is the camera with the 'MainCamera' tag assigned to it."), 
                mTarget.settings.targetCamera, typeof(Camera), true) as Camera;
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.settings.targetCamera = newCamera;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: Navigation() (Private Function)
        // Desc: Implements the Inspector UI logic for navigation settings.
        //-----------------------------------------------------------------------------
        void Navigation()
        {
            float newFloat;
            GUIContent content = new GUIContent();

            // Use physics?
            EditorUI.SectionTitleLabel("Movement");
            content.text    = "Physics";
            content.tooltip = "If checked, a very simple physics model will be used to move the camera.";
            EditorGUI.BeginChangeCheck();
            bool newBool = EditorGUILayout.Toggle(content, mTarget.settings.usePhysics);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.settings.usePhysics = newBool;
            }

            // Are we using physics?
            if (mTarget.settings.usePhysics)
            {
                // Acceleration
                content.text    = "Acceleration";
                content.tooltip = "Acceleration applied when moving the camera. Used when 'Physics' is checked.";
                EditorGUI.BeginChangeCheck();
                newFloat = EditorGUILayout.FloatField(content, mTarget.settings.acceleration);
                if (EditorGUI.EndChangeCheck())
                {
                    UndoEx.Record(mTarget);
                    mTarget.settings.acceleration = newFloat;
                }

                // Friction
                content.text    = "Friction";
                content.tooltip = "Friction value used to gradually bring the camera to a halt. Used when 'Physics' is checked.";
                EditorGUI.BeginChangeCheck();
                newFloat = EditorGUILayout.FloatField(content, mTarget.settings.friction);
                if (EditorGUI.EndChangeCheck())
                {
                    UndoEx.Record(mTarget);
                    mTarget.settings.friction = newFloat;
                }

                // Max move speed
                content.text    = "Max move speed";
                content.tooltip = "The maximum speed the camera can have while moving. Used when 'Physics' is checked.";
                EditorGUI.BeginChangeCheck();
                newFloat = EditorGUILayout.FloatField(content, mTarget.settings.maxMoveSpeed);
                if (EditorGUI.EndChangeCheck())
                {
                    UndoEx.Record(mTarget);
                    mTarget.settings.maxMoveSpeed = newFloat;
                }
            }
            else
            {
                // Move speed
                content.text    = "Move speed";
                content.tooltip = "Camera move speed. Used when 'Physics' is unchecked.";
                EditorGUI.BeginChangeCheck();
                newFloat = EditorGUILayout.FloatField(content, mTarget.settings.moveSpeed);
                if (EditorGUI.EndChangeCheck())
                {
                    UndoEx.Record(mTarget);
                    mTarget.settings.moveSpeed = newFloat;
                }
            }

            // Speed modifier
            content.text    = "Speed modifier";
            content.tooltip = "Speed modifier value used when the speed modifier is active.";
            EditorGUI.BeginChangeCheck();
            newFloat = EditorGUILayout.FloatField(content, mTarget.settings.speedModifier);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.settings.speedModifier = newFloat;
            }

            // Focus mode
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Focus");
            content.text    = "Focus mode";
            content.tooltip = "Controls the camera focus mode.";
            EditorGUI.BeginChangeCheck();
            ECameraFocusMode newFocusMode = (ECameraFocusMode)EditorGUILayout.EnumPopup(content, mTarget.settings.focusMode);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.settings.focusMode = newFocusMode;
            }

            // Focus smooth speed
            content.text    = "Focus smooth speed";
            content.tooltip = "Focus speed when doing a smooth focus.";
            EditorGUI.BeginChangeCheck();
            newFloat = EditorGUILayout.FloatField(content, mTarget.settings.focusSmoothSpeed);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.settings.focusSmoothSpeed = newFloat;
            }

            // Projection switch mode
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Projection Switch");
            content.text    = "Switch mode";
            content.tooltip = "Controls the camera projection switch mode.";
            EditorGUI.BeginChangeCheck();
            ECameraProjectionSwitchMode newPSwitchMode = (ECameraProjectionSwitchMode)EditorGUILayout.EnumPopup(content, mTarget.settings.projectionSwitchMode);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.settings.projectionSwitchMode = newPSwitchMode;
            }

            // Projection switch transition duration
            content.text    = "Switch duration";
            content.tooltip = "The amount of time it takes to complete a projection switch transition. Used when the switch mode is set to 'Linear'.";
            EditorGUI.BeginChangeCheck();
            newFloat = EditorGUILayout.FloatField(content, mTarget.settings.projectionTransitionDuration);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.settings.projectionTransitionDuration = newFloat;
            }

            // Rotation switch mode
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Rotation Switch");
            content.text    = "Switch mode";
            content.tooltip = "Controls the camera rotation switch mode.";
            EditorGUI.BeginChangeCheck();
            ECameraRotationSwitchMode newRSwitchMode = (ECameraRotationSwitchMode)EditorGUILayout.EnumPopup(content, mTarget.settings.rotationSwitchMode);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.settings.rotationSwitchMode = newRSwitchMode;
            }

            // Rotation switch smooth speed
            content.text    = "Switch smooth speed";
            content.tooltip = "Rotation switch speed when doing a smooth rotation switch.";
            EditorGUI.BeginChangeCheck();
            newFloat = EditorGUILayout.FloatField(content, mTarget.settings.rotationSwitchSmoothSpeed);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.settings.rotationSwitchSmoothSpeed = newFloat;
            }

            // Mouse sensitivity
            EditorGUILayout.Separator();
            EditorUI.SectionTitleLabel("Mouse sensitivity");
            content.text    = "Pan sensitivity";
            content.tooltip = "Mouse sensitivity when panning the camera.";
            EditorGUI.BeginChangeCheck();
            newFloat = EditorGUILayout.FloatField(content, mTarget.settings.panSensitivity);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.settings.panSensitivity = newFloat;
            }
            content.text    = "Rotation sensitivity";
            content.tooltip = "Mouse sensitivity when rotating the camera.";
            EditorGUI.BeginChangeCheck();
            newFloat = EditorGUILayout.FloatField(content, mTarget.settings.rotationSensitivity);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.settings.rotationSensitivity = newFloat;
            }
            content.text    = "Orbit sensitivity";
            content.tooltip = "Mouse sensitivity when orbiting the camera.";
            EditorGUI.BeginChangeCheck();
            newFloat = EditorGUILayout.FloatField(content, mTarget.settings.orbitSensitivity);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.settings.orbitSensitivity = newFloat;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: BackgroundStyle() (Private Function)
        // Desc: Implements the Inspector UI logic for background style settings.
        //-----------------------------------------------------------------------------
        void BackgroundStyle()
        {
            GUIContent content = new GUIContent();
            EditorUI.SectionTitleLabel("Background Style");

            // Visible?
            content.text    = "Visible";
            content.tooltip = "If checked, the camera background will be rendered.";
            EditorGUI.BeginChangeCheck();
            bool newBool = EditorGUILayout.ToggleLeft(content, mTarget.bgStyle.visible);
            if (EditorGUI.EndChangeCheck()) 
            {
                UndoEx.Record(mTarget);
                mTarget.bgStyle.visible = newBool;
            }

            // Top color
            content.text    = "Top color";
            content.tooltip = "Top background gradient color.";
            EditorGUI.BeginChangeCheck();
            Color newColor = EditorGUILayout.ColorField(content, mTarget.bgStyle.topColor);
            if (EditorGUI.EndChangeCheck()) 
            {
                UndoEx.Record(mTarget);
                mTarget.bgStyle.topColor = newColor;
            }
            
            // Bottom color
            content.text    = "Bottom color";
            content.tooltip = "Bottom background gradient color.";
            EditorGUI.BeginChangeCheck();
            newColor = EditorGUILayout.ColorField(content, mTarget.bgStyle.bottomColor);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.bgStyle.bottomColor = newColor;
            }

            // Gradient offset
            content.text    = "Gradient offset";
            content.tooltip = "Positive values move the bottom color closer to the top color. Negative values move the top color closer to the bottom color.";
            EditorGUI.BeginChangeCheck();
            float newFloat = EditorGUILayout.FloatField(content, mTarget.bgStyle.gradientOffset);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.bgStyle.gradientOffset = newFloat;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: OnEnable() (Private Function)
        // Desc: Called when the object is enabled.
        //-----------------------------------------------------------------------------
        void OnEnable()
        {
            mTarget = target as RTCamera;
        }
        #endregion
    }
    #endregion
}