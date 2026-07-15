#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RTGLite
{
    #region Public Classes
    //-----------------------------------------------------------------------------
    // Name: EditorUI (Public Static Class)
    // Desc: Contains utility functions for Editor UI creation/management tasks.
    //-----------------------------------------------------------------------------
    public static class EditorUI
    {
        #region Private Static Fields
        static GUIStyle     sSectionTitleLabelStyle;                    // Style used for section title labels
        static GUIStyle     sSubsectionTitleLabelStyle;                 // Style used for subsection title labels
        static Stack<float> sLabelWidthStack    = new Stack<float>();   // Used for label width push/pop operations
        static Stack<Color> sGUIColorStack      = new Stack<Color>();   // Used for GUI.color push/pop operations
        static Stack<Color> sGUIBgColorStack    = new Stack<Color>();   // used for GUI.backgroundColor push/pop operations

        // Buffers used to avoid memory allocations
        static List<string> sLayerNameBuffer    = new List<string>();
        #endregion

        #region Public Static Properties
        //-----------------------------------------------------------------------------
        // Name: isProSkin (Public Static Property)
        // Desc: Returns true if the Editor uses the Pro skin and false otherwise.
        //-----------------------------------------------------------------------------
        public static bool  isProSkin               { get { return EditorGUIUtility.isProSkin; } }

        //-----------------------------------------------------------------------------
        // Name: useDefaultsButtonWidth (Public Static Property)
        // Desc: Returns the preferred width of buttons used for 'Use Defaults' actions.
        //-----------------------------------------------------------------------------
        public static float useDefaultsButtonWidth  { get { return 100.0f; } }

        //-----------------------------------------------------------------------------
        // Name: defaultFixedItemHeight (Public Static Property)
        // Desc: Returns the default fixed height of list items.
        //-----------------------------------------------------------------------------
        public static float defaultFixedItemHeight  { get { return 20.0f; } }

        //-----------------------------------------------------------------------------
        // Name: defaultDropDownHeight (Public Static Property)
        // Desc: Returns the default height of dropdown menus.
        //-----------------------------------------------------------------------------
        public static float defaultDropDownHeight   { get { return 20.0f; } }

        //-----------------------------------------------------------------------------
        // Name: windowMargin (Public Static Property)
        // Desc: Returns the preferred window margin.
        //-----------------------------------------------------------------------------
        public static float windowMargin            { get { return 5.0f; } }

        //-----------------------------------------------------------------------------
        // Name: defaultLabelColor (Public Static Property)
        // Desc: Returns the default label color.
        // Cred: https://discussions.unity.com/t/editorguiutility-getbuiltinskin-not-working-correctly-in-unity-4-3/520370/3
        //-----------------------------------------------------------------------------
        public static Color defaultLabelColor       { get { return EditorGUIUtility.GetBuiltinSkin(isProSkin ? EditorSkin.Scene : EditorSkin.Inspector).label.normal.textColor; } }

        //-----------------------------------------------------------------------------
        // Name: defaultButtonHeight (Public Static Property)
        // Desc: Returns the default button height.
        // Cred: https://discussions.unity.com/t/editorguiutility-getbuiltinskin-not-working-correctly-in-unity-4-3/520370/3
        //-----------------------------------------------------------------------------
        public static float defaultButtonHeight     { get { return 19.0f; } }

        //-----------------------------------------------------------------------------
        // Name: defaultHeaderHeight (Public Static Property)
        // Desc: Returns the default header height used by list views, tree views etc.
        //-----------------------------------------------------------------------------
        public static float defaultHeaderHeight     { get { return 22.0f; } }

        //-----------------------------------------------------------------------------
        // Name: defaultBorderColor (Public Static Property)
        // Desc: Returns the default border color used by list views, tree views etc.
        //-----------------------------------------------------------------------------
        public static Color defaultBorderColor          { get { return EditorGUIUtility.isProSkin ? new Color32(31, 31, 31, 255) : new Color32(153, 153, 153, 255); } }

        //-----------------------------------------------------------------------------
        // Name: defaultSelectedItemColor (Public Static Property)
        // Desc: Returns the default color for focused selected items.
        //-----------------------------------------------------------------------------
        public static Color defaultSelectedItemColor            { get { return new Color(0.235f, 0.470f, 0.843f, 0.35f); } }

        //-----------------------------------------------------------------------------
        // Name: defaultUnfocusedSelectedItemColor (Public Static Property)
        // Desc: Returns the default color for unfocused selected items.
        //-----------------------------------------------------------------------------
        public static Color defaultUnfocusedSelectedItemColor   { get { return isProSkin ? new Color32(77, 77, 77, 255) : new Color32(174, 174, 174, 255); } }

        //-----------------------------------------------------------------------------
        // Name: sectionTitleLabelStyle (Public Static Property)
        // Desc: Returns the 'GUIStyle' used for labels that display UI section titles.
        //-----------------------------------------------------------------------------
        public static GUIStyle sectionTitleLabelStyle
        {
            get
            {
                // Create the style if not already created
                if (sSectionTitleLabelStyle == null)
                {
                    sSectionTitleLabelStyle = new GUIStyle("label");
                    sSectionTitleLabelStyle.fontStyle = FontStyle.Bold;
                }

                // Return style
                return sSectionTitleLabelStyle;
            }
        }

        //-----------------------------------------------------------------------------
        // Name: subsectionTitleLabelStyle (Public Static Property)
        // Desc: Returns the 'GUIStyle' used for labels that display UI subsection titles.
        //-----------------------------------------------------------------------------
        public static GUIStyle subsectionTitleLabelStyle
        {
            get
            {
                // Create the style if not already created
                if (sSubsectionTitleLabelStyle == null)
                {
                    sSubsectionTitleLabelStyle = new GUIStyle("label");
                    sSubsectionTitleLabelStyle.fontStyle = FontStyle.Bold;
                }

                // Return style
                return sSubsectionTitleLabelStyle;
            }
        }
        #endregion

        #region Public Static Functions
        //-----------------------------------------------------------------------------
        // Name: FocusProjectWindow() (Public Static Function)
        // Desc: Focuses the Project window inside the Unity Editor.
        //-----------------------------------------------------------------------------
        public static void FocusProjectWindow()
        {
            EditorApplication.ExecuteMenuItem("Window/General/Project");
        }
        #endregion

        #region Public Static Functions (IMGUI)
        //-----------------------------------------------------------------------------
        // Name: PushLabelWidth() (Public Static Function: IMGUI)
        // Desc: Pushes the current label width onto the stack and changes the width to
        //       the specified value. Must have a matching 'PopLabelWidth' call.
        // Parm: newWidth - The new label width.
        //-----------------------------------------------------------------------------
        public static void PushLabelWidth(float newWidth)
        {
            sLabelWidthStack.Push(EditorGUIUtility.labelWidth);
            EditorGUIUtility.labelWidth = newWidth;
        }

        //-----------------------------------------------------------------------------
        // Name: PopLabelWidth() (Public Static Function: IMGUI)
        // Desc: Pops the last label width off the stack. Must have been preceded by
        //       a matching 'PushLabelWidth' call.
        //-----------------------------------------------------------------------------
        public static void PopLabelWidth()
        {
            if (sLabelWidthStack.Count != 0) EditorGUIUtility.labelWidth = sLabelWidthStack.Pop();
        }

        //-----------------------------------------------------------------------------
        // Name: PushGUIColor() (Public Static Function: IMGUI)
        // Desc: Pushes the current GUI.color onto the stack and changes the GUI.color to
        //       the specified value. Should have a matching 'PopGUIColor' call.
        // Parm: newColor - The new 'GUI.color'.
        //-----------------------------------------------------------------------------
        public static void PushGUIColor(Color newColor)
        {
            sGUIColorStack.Push(GUI.color);
            GUI.color = newColor;
        }

        //-----------------------------------------------------------------------------
        // Name: PopGUIColor() (Public Static Function: IMGUI)
        // Desc: Pops the last GUI.color off the stack. Should be preceded by a matching
        //       'PushGUIColor' call.
        //-----------------------------------------------------------------------------
        public static void PopGUIColor()
        {
            if (sGUIColorStack.Count != 0) GUI.color = sGUIColorStack.Pop();
        }

        //-----------------------------------------------------------------------------
        // Name: PushGUIBgColor() (Public Static Function: IMGUI)
        // Desc: Pushes the current GUI.backgroundColor onto the stack and changes the
        //       GUI.backgroundColor to the specified value. Should have a matching
        //       'PopGUIBgColor' call.
        // Parm: newColor - The new 'GUI.backgroundColor'.
        //-----------------------------------------------------------------------------
        public static void PushGUIBgColor(Color newColor)
        {
            sGUIBgColorStack.Push(GUI.backgroundColor);
            GUI.backgroundColor = newColor;
        }

        //-----------------------------------------------------------------------------
        // Name: PopGUIBgColor() (Public Static Function: IMGUI)
        // Desc: Pops the last GUI.backgroundColor off the stack. Should be preceded by
        //       a matching 'PushGUIBgColor' call.
        //-----------------------------------------------------------------------------
        public static void PopGUIBgColor()
        {
            if (sGUIBgColorStack.Count != 0) GUI.backgroundColor = sGUIBgColorStack.Pop();
        }

        //-----------------------------------------------------------------------------
        // Name: UseDefaultsButton() (Public Static Function: IMGUI)
        // Desc: Draws a 'Use defaults' button.
        // Rtrn: True if the button was clicked and false otherwise.
        //-----------------------------------------------------------------------------
        public static bool UseDefaultsButton()
        {
            // Create content
            GUIContent content = new GUIContent();
            content.text    = "Use Defaults";
            content.tooltip = "Use default settings.";

            // Draw the button and return click result
            return GUILayout.Button(content, GUILayout.Width(useDefaultsButtonWidth));
        }
  
        //-----------------------------------------------------------------------------
        // Name : MultiRowToolbar() (Public Static Function: IMGUI)
        // Desc : Renders a multi-row toolbar using 'GUILayout.Toolbar'. This allows
        //        arranging buttons in rows while maintaining a flat index space
        //        across all rows. The function returns the index of the newly
        //        selected button, or the current selection if no changes occurred.
        // Parm : selectedButton - the index of the currently selected button.
        //        buttonRows     - a 2D array containing button labels for each row.
        // Rtrn : The index of the selected button after user interaction.
        //-----------------------------------------------------------------------------
        public static int MultiRowToolbar(int selectedButton, string[][] buttonRows)
        {
            // Loop through each button row
            int rowCount        = buttonRows.Length;
            int buttonsSoFar    = 0;    // The amount of buttons we've drawn so far
            for (int i = 0; i < rowCount; ++i)
            {
                // Get the buttons on this row
                string[] rowButtons = buttonRows[i];
                int rowButtonCount  = rowButtons.Length;

                // Calculate the selected button index for this row. This has to be -1
                // if the selected button belongs to a different row.
                int rowSelectedIndex = (selectedButton >= buttonsSoFar && selectedButton < (buttonsSoFar + rowButtonCount)) ?
                    selectedButton - buttonsSoFar : -1;

                // Draw the toolbar for this row
                int newSelected = GUILayout.Toolbar(rowSelectedIndex, rowButtons);
                if (newSelected != rowSelectedIndex)
                    return newSelected + buttonsSoFar;

                // More buttons have been processed
                buttonsSoFar += rowButtonCount;
            }

            // No change, return the same index
            return selectedButton;
        }

        //-----------------------------------------------------------------------------
        // Name: HorizontalLineSeparator() (Public Static Function: IMGUI)
        // Desc: Draws a horizontal line separator.
        //-----------------------------------------------------------------------------
        public static void HorizontalLineSeparator()
        {
            // Source: https://discussions.unity.com/t/horizontal-line-in-editor-window/694105/6
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        }

        //-----------------------------------------------------------------------------
        // Name: SectionTitleLabel() (Public Static Function: IMGUI)
        // Desc: Draws a label that serves as a title for a UI section. Keeps the UI
        //       organized.
        // Parm: text - Label text.
        //-----------------------------------------------------------------------------
        public static void SectionTitleLabel(string text)
        {
            EditorGUILayout.LabelField(text, sectionTitleLabelStyle);
        }

        //-----------------------------------------------------------------------------
        // Name: SubsectionTitleLabel() (Public Static Function: IMGUI)
        // Desc: Draws a label that serves as a subsection title. Keeps the UI organized.
        // Parm: text - Label text.
        //-----------------------------------------------------------------------------
        public static void SubsectionTitleLabel(string text)
        {
            EditorGUILayout.LabelField(text, subsectionTitleLabelStyle);
        }

        //-----------------------------------------------------------------------------
        // Name: DrawTexture() (Public Static Function: IMGUI)
        // Desc: Draws a texture inside the specified rectangle using IMGUI, applying the
        //       specified tint color.
        // Parm: rect       - Texture position and size.
        //       texture    - The texture to draw.
        //       tint       - Texture tint.
        //       scaleMode  - Texture scale mode.
        //-----------------------------------------------------------------------------
        public static void DrawTexture(Rect rect, Texture2D texture, Color tint, ScaleMode scaleMode = ScaleMode.ScaleToFit)
        {
            PushGUIColor(tint);
            GUI.DrawTexture(rect, texture, scaleMode, true);
            PopGUIColor();
        }

        //-----------------------------------------------------------------------------
        // Name: DrawBorder() (Public Static Function: IMGUI)
        // Desc: Draws a border around the specified rectangle.
        // Parm: rect   - The rectangle whose border must be drawn.
        //       width  - Border width. Values <= 0 result in no border being drawn.
        //       color  - Border color.
        //-----------------------------------------------------------------------------
        public static void DrawBorder(Rect rect, float width, Color color)
        {
            // Validate args
            if (width <= 0.0f)
                return;

            PushGUIColor(color);

            // Top
            GUI.DrawTexture(
                new Rect(rect.x, rect.y, rect.width, width),
                EditorGUIUtility.whiteTexture
            );

            // Bottom
            GUI.DrawTexture(
                new Rect(rect.x, rect.yMax - width, rect.width, width),
                EditorGUIUtility.whiteTexture
            );

            // Left
            GUI.DrawTexture(
                new Rect(rect.x, rect.y, width, rect.height),
                EditorGUIUtility.whiteTexture
            );

            // Right
            GUI.DrawTexture(
                new Rect(rect.xMax - width, rect.y, width, rect.height),
                EditorGUIUtility.whiteTexture
            );

            PopGUIColor();
        }

        //-----------------------------------------------------------------------------
        // Name: LayerMaskField (Public Static Function: IMGUI)
        // Desc: Creates a layer mask field which allows the users to set/clear layer bits.
        // Parm: content   - Label and tooltip.
        //       layerMask - Input layer mask.
        // Rtrn: The new layer mask.
        //-----------------------------------------------------------------------------
        public static int LayerMaskField(GUIContent content, int layerMask)
        {
            // Get all layer names
            LayerEx.CollectLayerNames(sLayerNameBuffer);
            int layerCount = sLayerNameBuffer.Count;

            // Build a layer mask which has a bit of 1 for each layer that exists and has
            // its bit set to 1 in the input 'layerMask'.
            int inputLayerMask = 0;
            for(int l = 0; l < layerCount; ++l)
            {
                // If the layer is set inside the layer mask, set the bit in the index mask also
                string layerName = sLayerNameBuffer[l];
                if (LayerEx.CheckLayerBit(LayerMask.NameToLayer(layerName), layerMask)) inputLayerMask |= (1 << l); 
            }

            // Show the mask field
            int newMask = EditorGUILayout.MaskField(content, inputLayerMask, sLayerNameBuffer.ToArray());

            // Convert back to Unity layer mask
            int outputMask = 0;
            for (int i = 0; i < layerCount; ++i)
            {
                if ((newMask & (1 << i)) != 0)
                {
                    int realLayer = LayerMask.NameToLayer(sLayerNameBuffer[i]);
                    outputMask |= (1 << realLayer);
                }
            }

            // Return real mask
            return outputMask;
        }

        //-----------------------------------------------------------------------------
        // Name: RectOffsetField (Public Static Function: IMGUI)
        // Desc: Creates a 'RectOffset' field.
        // Parm: content    - Label and tooltip.
        //       rectOffset - Current value.
        // Rtrn: The new 'RectOffset' value.
        //-----------------------------------------------------------------------------
        public static RectOffset RectOffsetField(GUIContent content, RectOffset rectOffset)
        {          
            // Integer fields will sit to the right of the label
            EditorGUILayout.BeginHorizontal();
            {
                // Label
                EditorGUILayout.LabelField(content);

                // Int fields will be arranged as 2 pairs of integers, each pair on its own row
                PushLabelWidth(30.0f);
                EditorGUILayout.BeginVertical();
                {
                    // First pair/row (Left and Top offset)
                    EditorGUILayout.BeginHorizontal();
                    {
                        rectOffset.left = EditorGUILayout.IntField("L", rectOffset.left);
                        rectOffset.top  = EditorGUILayout.IntField("T", rectOffset.top);
                    }

                    // Second pair/row (Right and Bottom offset)
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    {
                        rectOffset.right  = EditorGUILayout.IntField("R", rectOffset.right);
                        rectOffset.bottom = EditorGUILayout.IntField("B", rectOffset.bottom);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                PopLabelWidth();
            }
            EditorGUILayout.EndHorizontal();
        
            // Return the new rect offset
            return rectOffset;
        }
        #endregion

        #region Public Static Functions (UIElements)
        //-----------------------------------------------------------------------------
        // Name: CreateHorizontalSplitView() (Public Static Function: UIElements)
        // Desc: Creates a horizontal split view control.
        // Parm: fixedPaneIndex         - Index of the fixed pane.
        //       fixedPaneInitialSize   - Initial size of the fixed pane.
        //       fixedPaneResized       - Callback to invoke when then the fixed pane 
        //                                is resized.
        //       parent                 - Parent element.
        // Rtrn: An instance of 'TwoPaneSplitView'.
        //-----------------------------------------------------------------------------
        public static TwoPaneSplitView CreateHorizontalSplitView(int fixedPaneIndex, float fixedPaneInitialSize,
            Action<float> fixedPaneResized, VisualElement parent)
        {
            // Create the split view
            var splitView = new TwoPaneSplitView(fixedPaneIndex, fixedPaneInitialSize, TwoPaneSplitViewOrientation.Horizontal);
            splitView.Add(new VisualElement());
            splitView.Add(new VisualElement());

            // Register resized callback
            if (fixedPaneResized != null)
            {
                splitView[fixedPaneIndex].RegisterCallback<GeometryChangedEvent>(e =>
                { fixedPaneResized(splitView[fixedPaneIndex].resolvedStyle.width); });
            }

            // Set parent
            parent.Add(splitView);

            // Return split view
            return splitView;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateMultiColumnListView() (Public Static Function: UIElements)
        // Desc: Creates a multicolumn list view with the most commonly used styles and
        //       properties.
        // Parm: so             - Serialized objects used for data binding.
        //       bindingPath    - Binding path. Columns will bind relative to this path.
        //       parent         - Parent element.
        // Rtrn: An instance of 'MultiColumnListView'.
        //-----------------------------------------------------------------------------
        public static MultiColumnListView CreateMultiColumnListView(SerializedObject so, string bindingPath, VisualElement parent)
        {
            // Create list
            var list = new MultiColumnListView();
            list.style.flexGrow             = 1.0f;
            list.showBoundCollectionSize    = false;
            list.showBorder                 = true;
            list.fixedItemHeight            = defaultFixedItemHeight;
            list.bindingPath                = bindingPath;
            list.Bind(so);

            // Note: We need to add an empty item source so we can use 'SetSelection'. Otherwise,
            //       'SetSelection' won't work.
            list.itemsSource = new List<string> {};

            // Add to parent
            parent.Add(list);

            // Return list
            return list;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateButton() (Public Static Function: UIElements)
        // Desc: Creates a button.
        // Parm: onClickedAction    - Action to execute when the button is clicked.
        //       text               - Button text.
        //       tooltip            - Button tooltip.
        //       parent             - Parent element.
        // Rtrn: The button.
        //-----------------------------------------------------------------------------
        public static Button CreateButton(Action onClickedAction, string text, string tooltip, VisualElement parent)
        {
            // Create the button
            var button          = new Button();
            button.text         = text;
            button.tooltip      = tooltip;
            button.style.height = defaultButtonHeight;
            parent.Add(button);

            // Register handlers
            if (onClickedAction != null) button.clicked += onClickedAction;

            // Return button
            return button;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateUseDefaultsButton() (Public Static Function: UIElements)
        // Desc: Creates a 'Use defaults' button.
        // Parm: onClickedAction - Action to execute when the button is clicked.
        //       parent          - Parent element.
        // Rtrn: The button.
        //-----------------------------------------------------------------------------
        public static Button CreateUseDefaultsButton(Action onClickedAction, VisualElement parent)
        {
            // Create the button
            var button = new Button();
            parent.Add(button);
            button.text           = "Use Defaults";
            button.tooltip        = "Use default settings.";
            button.style.width    = useDefaultsButtonWidth;
            button.style.height   = defaultButtonHeight;
            button.style.unityFontStyleAndWeight = FontStyle.Normal;

            // Register handlers
            if (onClickedAction != null) button.clicked += onClickedAction;
            button.clicked += () => SceneView.RepaintAll();

            // Return button
            return button;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateSectionTitleLabel() (Public Static Function: UIElements)
        // Desc: Creates a label that serves as a title for a UI section. Keeps the UI
        //       organized.
        // Parm: text   - Label text.
        //       parent - Parent element.
        // Rtrn: The section label.
        //-----------------------------------------------------------------------------
        public static Label CreateSectionTitleLabel(string text, VisualElement parent)
        {
            // Create the label
            Label sectionLabel = new Label(text);
            parent.style.unityFontStyleAndWeight = FontStyle.BoldAndItalic;
            parent.Add(sectionLabel);

            // Return label
            return sectionLabel;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateToolbarMenu() (Public Static Function: UIElements)
        // Desc: Creates a toolbar dropdown menu.
        // Parm: width  - Menu width.
        //       parent - Parent element.
        // Rtrn: The 'ToolbarMenu' instance.
        //-----------------------------------------------------------------------------
        public static ToolbarMenu CreateToolbarMenu(float width, VisualElement parent)
        {
            // Create the menu
            var menu = new ToolbarMenu();
            menu.style.width     = width;
            menu.style.height    = defaultDropDownHeight;
            menu.style.EnableRoundBorder();

            // Add to parent
            parent.Add(menu);

            // Return menu
            return menu;
        }

        //-----------------------------------------------------------------------------
        // Name: CreateIcon() (Public Static Function: UIElements)
        // Desc: Creates an icon element with the specified icon image.
        // Parm: iconImage - Icon image.
        //       parent    - Icon parent.
        // Rtrn: The icon element.
        //-----------------------------------------------------------------------------
        public static VisualElement CreateIcon(Texture2D iconImage, VisualElement parent)
        {
            // Create icon
            var icon = new VisualElement();
            icon.style.SetBackgroundImage(iconImage);
            icon.style.flexShrink = 0.0f;
            icon.style.marginTop = 3.0f;

            // Add to parent
            parent.Add(icon);

            // Return icon
            return icon;
        }
        #endregion
    }
    #endregion
}
#endif