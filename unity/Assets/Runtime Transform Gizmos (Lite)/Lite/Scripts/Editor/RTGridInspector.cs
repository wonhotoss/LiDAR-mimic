using UnityEditor;
using UnityEngine;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: RTGridInspector (Public Class)
    // Desc: Implements the Inspector UI for the 'RTGrid' Mono.
    //-----------------------------------------------------------------------------
    [CustomEditor(typeof(RTGrid))]
    public class RTGridInspector : Editor
    {
        #region Private Fields
        RTGrid mTarget; // Target
        #endregion

        #region Public Functions
        //-----------------------------------------------------------------------------
        // Name: OnInspectorGUI() (Public Function)
        // Desc: Implements the Inspector UI logic.
        //-----------------------------------------------------------------------------
        public override void OnInspectorGUI() 
        {
            Settings();
            EditorGUILayout.Separator();
            Style();
        }
        #endregion

        #region Private Functions
        //-----------------------------------------------------------------------------
        // Name: Settings() (Private Function)
        // Desc: Implements the Inspector UI logic for grid settings.
        //-----------------------------------------------------------------------------
        void Settings()
        {
            GUIContent content = new GUIContent();
            EditorUI.SectionTitleLabel("Settings");

            // Local Y offset
            content.text    = "Local y offset";
            content.tooltip = "The grid offset along its local Y axis.";
            EditorGUI.BeginChangeCheck();
            float newFloat = EditorGUILayout.FloatField(content, mTarget.settings.localYOffset);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.settings.localYOffset = newFloat;
            }

            // Cell size
            content.text    = "Cell size";
            content.tooltip = "The grid cell size.";
            EditorGUI.BeginChangeCheck();
            Vector3 newVec3 = EditorGUILayout.Vector3Field(content, mTarget.settings.cellSize);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.settings.cellSize = newVec3;
            }

            // Grid plane
            content.text    = "Plane";
            content.tooltip = "The grid plane. Controls the grid's orientation.";
            EditorGUI.BeginChangeCheck();
            EPlane newPlaneId = (EPlane)EditorGUILayout.EnumPopup(content, mTarget.settings.plane);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.settings.plane = newPlaneId;
            }

            // Use defaults button
            if (EditorUI.UseDefaultsButton())
            {
                UndoEx.Record(mTarget);
                mTarget.settings.UseDefaults();
            }
        }

        //-----------------------------------------------------------------------------
        // Name: Style() (Private Function)
        // Desc: Implements the Inspector UI logic for grid style settings.
        //-----------------------------------------------------------------------------
        void Style()
        {
            GUIContent content = new GUIContent();
            EditorUI.SectionTitleLabel("Style");

            // Visibility
            content.text    = "Visible";
            content.tooltip = "If checked, the grid is visible and will be rendered.";
            EditorGUI.BeginChangeCheck();
            bool newBool    = EditorGUILayout.ToggleLeft(content, mTarget.style.visible);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.style.visible = newBool;
            }

            // Use zoom fade?
            content.text    = "Zoom fade";
            content.tooltip = "If checked, cell lines will fade in and out as the camera moves closer or further away from the grid.";
            EditorGUI.BeginChangeCheck();
            newBool         = EditorGUILayout.ToggleLeft(content, mTarget.style.zoomFadeEnabled);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.style.zoomFadeEnabled = newBool;
            }

            // Cell line color
            content.text    = "Cell line color";
            content.tooltip = "Cell line color.";
            EditorGUI.BeginChangeCheck();
            Color newColor = EditorGUILayout.ColorField(content, mTarget.style.cellLineColor);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.style.cellLineColor = newColor;
            }

            // Draw axes?
            EditorGUILayout.Separator();
            content.text    = "Draw axes";
            content.tooltip = "If checked, the grid's coordinate system axes will be rendered.";
            EditorGUI.BeginChangeCheck();
            newBool = EditorGUILayout.ToggleLeft(content, mTarget.style.drawGridAxes);
            if (EditorGUI.EndChangeCheck())
            {
                UndoEx.Record(mTarget);
                mTarget.style.drawGridAxes = newBool;
            }

            // Are we drawing the grid axes?
            if (mTarget.style.drawGridAxes)
            {
                // Axes colors
                content.text    = "X axis color";
                content.tooltip = "Color used when drawing the grid's X axis.";
                EditorGUI.BeginChangeCheck();
                newColor = EditorGUILayout.ColorField(content, mTarget.style.xAxisColor);
                if (EditorGUI.EndChangeCheck())
                {
                    UndoEx.Record(mTarget);
                    mTarget.style.xAxisColor = newColor;
                }
                content.text    = "Y axis color";
                content.tooltip = "Color used when drawing the grid's Y axis.";
                EditorGUI.BeginChangeCheck();
                newColor = EditorGUILayout.ColorField(content, mTarget.style.yAxisColor);
                if (EditorGUI.EndChangeCheck())
                {
                    UndoEx.Record(mTarget);
                    mTarget.style.yAxisColor = newColor;
                }
                content.text    = "Z axis color";
                content.tooltip = "Color used when drawing the grid's Z axis.";
                EditorGUI.BeginChangeCheck();
                newColor = EditorGUILayout.ColorField(content, mTarget.style.zAxisColor);
                if (EditorGUI.EndChangeCheck())
                {
                    UndoEx.Record(mTarget);
                    mTarget.style.zAxisColor = newColor;
                }

                // Finite axes toggles
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Finite: ");
                content.text    = "X";
                content.tooltip = "Is the X axis finite?";
                EditorGUI.BeginChangeCheck();
                newBool = EditorGUILayout.ToggleLeft(content, mTarget.style.finiteXAxis, GUILayout.Width(30.0f));
                if (EditorGUI.EndChangeCheck())
                {
                    UndoEx.Record(mTarget);
                    mTarget.style.finiteXAxis = newBool;
                }
                content.text    = "Y";
                content.tooltip = "Is the Y axis finite?";
                EditorGUI.BeginChangeCheck();
                newBool = EditorGUILayout.ToggleLeft(content, mTarget.style.finiteYAxis, GUILayout.Width(30.0f));
                if (EditorGUI.EndChangeCheck())
                {
                    UndoEx.Record(mTarget);
                    mTarget.style.finiteYAxis = newBool;
                }
                content.text    = "Z";
                content.tooltip = "Is the Z axis finite?";
                EditorGUI.BeginChangeCheck();
                newBool = EditorGUILayout.ToggleLeft(content, mTarget.style.finiteZAxis, GUILayout.Width(30.0f));
                if (EditorGUI.EndChangeCheck())
                {
                    UndoEx.Record(mTarget);
                    mTarget.style.finiteZAxis = newBool;
                }
                EditorGUILayout.EndHorizontal();

                // Axes finite length
                content.text    = "Finite length";
                content.tooltip = "Length value used to draw the finite grid axes.";
                EditorGUI.BeginChangeCheck();
                Vector3 newVec3 = EditorGUILayout.Vector3Field(content,
                    new Vector3(mTarget.style.xAxisLength, mTarget.style.yAxisLength, mTarget.style.zAxisLength));
                if (EditorGUI.EndChangeCheck())
                {
                    UndoEx.Record(mTarget);
                    mTarget.style.xAxisLength = newVec3.x;
                    mTarget.style.yAxisLength = newVec3.y;
                    mTarget.style.zAxisLength = newVec3.z;
                }
            }

            // Use defaults button
            if (EditorUI.UseDefaultsButton())
            {
                UndoEx.Record(mTarget);
                mTarget.style.UseDefaults();
            }
        }

        //-----------------------------------------------------------------------------
        // Name: OnEnable() (Private Function)
        // Desc: Called when the object is enabled.
        //-----------------------------------------------------------------------------
        void OnEnable()
        {
            mTarget = target as RTGrid;
        }
        #endregion
    }
    #endregion
}