// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 17.07.2019 : 17:19
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 03.08.2019 : 14:32
// // ***********************************************************************
// // <copyright file="EditorStyles.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using System;
using UnityEditor;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace FinalFactory.Unity.Editor
{
    /// <summary>
    ///     <para>
    ///         Common GUIStyles used for EditorGUI controls.
    ///     </para>
    /// </summary>
    public sealed class FinalEditorStyles
    {
        public static GUIStyle m_Label;
        public static GUIStyle m_TextField;
        public static GUIStyle m_TextArea;
        public static Font m_StandardFont;
        public static Font m_BoldFont;
        public static Font m_MiniFont;
        public static Font m_MiniBoldFont;

        public static Texture2D ConsoleWarning;

        public static GUIStyle ArrowLeft;
        public static GUIStyle ArrowRight;

        private static GUIStyle _labelStyle;
        
        static FinalEditorStyles()
        {
            _labelStyle = new GUIStyle(EditorStyles.label);
            
            ConsoleWarning = EditorGUIUtility.IconContent("console.warnicon").image as Texture2D;
            
            ArrowLeft = GetStyle("AC LeftArrow");
            ArrowRight = GetStyle("AC RightArrow");
            
            colorPickerBox = GetStyle("ColorPickerBox");
            inspectorBig = GetStyle("In BigTitle");
            miniLabel = GetStyle("miniLabel");
            largeLabel = GetStyle("LargeLabel");
            boldLabel = GetStyle("BoldLabel");
            miniBoldLabel = GetStyle("MiniBoldLabel");
            wordWrappedLabel = GetStyle("WordWrappedLabel");
            wordWrappedMiniLabel = GetStyle("WordWrappedMiniLabel");
            whiteLabel = GetStyle("WhiteLabel");
            whiteMiniLabel = GetStyle("WhiteMiniLabel");
            whiteLargeLabel = GetStyle("WhiteLargeLabel");
            whiteBoldLabel = GetStyle("WhiteBoldLabel");
            miniTextField = GetStyle("MiniTextField");
            radioButton = GetStyle("Radio");
            miniButton = GetStyle("miniButton");
            miniButtonLeft = GetStyle("miniButtonLeft");
            miniButtonMid = GetStyle("miniButtonMid");
            miniButtonRight = GetStyle("miniButtonRight");
            toolbar = GetStyle("toolbar");
            toolbarButton = GetStyle("toolbarbutton");
            toolbarPopup = GetStyle("toolbarPopup");
            toolbarDropDown = GetStyle("toolbarDropDown");
            toolbarTextField = GetStyle("toolbarTextField");
            toolbarSearchField = GetStyle("ToolbarSearchTextField");
            toolbarSearchFieldPopup = GetStyle("ToolbarSearchTextFieldPopup");
            toolbarSearchFieldCancelButton = GetStyle("ToolbarSearchCancelButton");
            toolbarSearchFieldCancelButtonEmpty = GetStyle("ToolbarSearchCancelButtonEmpty");
            searchField = GetStyle("SearchTextField");
            searchFieldCancelButton = GetStyle("SearchCancelButton");
            searchFieldCancelButtonEmpty = GetStyle("SearchCancelButtonEmpty");
            helpBox = GetStyle("HelpBox");
            assetLabel = GetStyle("AssetLabel");
            assetLabelPartial = GetStyle("AssetLabel Partial");
            assetLabelIcon = GetStyle("AssetLabel Icon");
            selectionRect = GetStyle("selectionRect");
            minMaxHorizontalSliderThumb = GetStyle("MinMaxHorizontalSliderThumb");
            dropDownList = GetStyle("DropDownButton");
            m_BoldFont = GetStyle("BoldLabel").font;
            m_StandardFont = GetStyle("Label").font;
            m_MiniFont = GetStyle("MiniLabel").font;
            m_MiniBoldFont = GetStyle("MiniBoldLabel").font;
            progressBarBack = GetStyle("ProgressBarBack");
            progressBarBar = GetStyle("ProgressBarBar");
            progressBarText = GetStyle("ProgressBarText");
            foldoutPreDrop = GetStyle("FoldoutPreDrop");
            inspectorTitlebar = GetStyle("IN Title");
            inspectorTitlebarText = GetStyle("IN TitleText");
            toggleGroup = GetStyle("BoldToggle");
            tooltip = GetStyle("Tooltip");
            notificationText = GetStyle("NotificationText");
            notificationBackground = GetStyle("NotificationBackground");
            popup = layerMaskField = GetStyle("MiniPopup");
            m_TextField = numberField = GetStyle("textField");
            m_Label = GetStyle("ControlLabel");
            objectField = GetStyle("ObjectField");
            objectFieldThumb = GetStyle("ObjectFieldThumb");
            objectFieldMiniThumb = GetStyle("ObjectFieldMiniThumb");
            toggle = GetStyle("Toggle");
            toggleMixed = GetStyle("ToggleMixed");
            colorField = GetStyle("ColorField");
            foldout = GetStyle("Foldout");
            foldoutSelected = GUIStyle.none;
            textFieldDropDown = GetStyle("TextFieldDropDown");
            textFieldDropDownText = GetStyle("TextFieldDropDownText");
            linkLabel = new GUIStyle(m_Label);
            linkLabel.normal.textColor = new Color(0.25f, 0.5f, 0.9f, 1f);
            linkLabel.stretchWidth = false;
            m_TextArea = new GUIStyle(m_TextField);
            m_TextArea.wordWrap = true;
            inspectorDefaultMargins = new GUIStyle();
            inspectorDefaultMargins.padding = new RectOffset(14, 4, 0, 0);
            inspectorFullWidthMargins = new GUIStyle();
            inspectorFullWidthMargins.padding = new RectOffset(5, 4, 0, 0);
            DeleteButtonRed = GetStyle("OL Minus");
            DeleteButtonRed.onHover.textColor = Color.red;
            centeredGreyMiniLabel = new GUIStyle(miniLabel);
            centeredGreyMiniLabel.alignment = TextAnchor.MiddleCenter;
            centeredGreyMiniLabel.normal.textColor = Color.grey;

            Plus = GetStyle("OL Plus");
        }

        public static GUIStyle Plus { get; set; }

        /// <summary>
        ///     <para>
        ///         Style used for the labelled on all EditorGUI overloads that take a prefix label.
        ///     </para>
        /// </summary>
        public static GUIStyle label => m_Label;

        /// <summary>
        ///     <para>
        ///         Style for label with small font.
        ///     </para>
        /// </summary>
        public static GUIStyle miniLabel { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style for label with large font.
        ///     </para>
        /// </summary>
        public static GUIStyle largeLabel { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style for bold label.
        ///     </para>
        /// </summary>
        public static GUIStyle boldLabel { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style for mini bold label.
        ///     </para>
        /// </summary>
        public static GUIStyle miniBoldLabel { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style for label with small font which is centered and grey.
        ///     </para>
        /// </summary>
        public static GUIStyle centeredGreyMiniLabel { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style for word wrapped mini label.
        ///     </para>
        /// </summary>
        public static GUIStyle wordWrappedMiniLabel { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style for word wrapped label.
        ///     </para>
        /// </summary>
        public static GUIStyle wordWrappedLabel { get; private set; }

        public static GUIStyle linkLabel { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style for white label.
        ///     </para>
        /// </summary>
        public static GUIStyle whiteLabel { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style for white mini label.
        ///     </para>
        /// </summary>
        public static GUIStyle whiteMiniLabel { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style for white large label.
        ///     </para>
        /// </summary>
        public static GUIStyle whiteLargeLabel { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style for white bold label.
        ///     </para>
        /// </summary>
        public static GUIStyle whiteBoldLabel { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style used for a radio button.
        ///     </para>
        /// </summary>
        public static GUIStyle radioButton { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style used for a standalone small button.
        ///     </para>
        /// </summary>
        public static GUIStyle miniButton { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style used for the leftmost button in a horizontal button group.
        ///     </para>
        /// </summary>
        public static GUIStyle miniButtonLeft { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style used for the middle buttons in a horizontal group.
        ///     </para>
        /// </summary>
        public static GUIStyle miniButtonMid { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style used for the rightmost button in a horizontal group.
        ///     </para>
        /// </summary>
        public static GUIStyle miniButtonRight { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style used for EditorGUI.TextField.
        ///     </para>
        /// </summary>
        public static GUIStyle textField => m_TextField;

        /// <summary>
        ///     <para>
        ///         Style used for EditorGUI.TextArea.
        ///     </para>
        /// </summary>
        public static GUIStyle textArea => m_TextArea;

        /// <summary>
        ///     <para>
        ///         Smaller text field.
        ///     </para>
        /// </summary>
        public static GUIStyle miniTextField { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style used for field editors for numbers.
        ///     </para>
        /// </summary>
        public static GUIStyle numberField { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style used for EditorGUI.Popup, EditorGUI.EnumPopup,.
        ///     </para>
        /// </summary>
        public static GUIStyle popup { get; private set; }

        [Obsolete("structHeadingLabel is deprecated, use EditorStyles.label instead.")]
        public static GUIStyle structHeadingLabel => m_Label;

        /// <summary>
        ///     <para>
        ///         Style used for headings for object fields.
        ///     </para>
        /// </summary>
        public static GUIStyle objectField { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style used for headings for the Select button in object fields.
        ///     </para>
        /// </summary>
        public static GUIStyle objectFieldThumb { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style used for object fields that have a thumbnail (e.g Textures).
        ///     </para>
        /// </summary>
        public static GUIStyle objectFieldMiniThumb { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style used for headings for Color fields.
        ///     </para>
        /// </summary>
        public static GUIStyle colorField { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style used for headings for Layer masks.
        ///     </para>
        /// </summary>
        public static GUIStyle layerMaskField { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style used for headings for EditorGUI.Toggle.
        ///     </para>
        /// </summary>
        public static GUIStyle toggle { get; private set; }

        public static GUIStyle toggleMixed { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style used for headings for EditorGUI.Foldout.
        ///     </para>
        /// </summary>
        public static GUIStyle foldout { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style used for headings for EditorGUI.Foldout.
        ///     </para>
        /// </summary>
        public static GUIStyle foldoutPreDrop { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style used for headings for EditorGUILayout.BeginToggleGroup.
        ///     </para>
        /// </summary>
        public static GUIStyle toggleGroup { get; private set; }

        public static GUIStyle textFieldDropDown { get; private set; }

        public static GUIStyle textFieldDropDownText { get; private set; }

        /// <summary>
        ///     <para>
        ///         Standard font.
        ///     </para>
        /// </summary>
        public static Font standardFont => m_StandardFont;

        /// <summary>
        ///     <para>
        ///         Bold font.
        ///     </para>
        /// </summary>
        public static Font boldFont => m_BoldFont;

        /// <summary>
        ///     <para>
        ///         Mini font.
        ///     </para>
        /// </summary>
        public static Font miniFont => m_MiniFont;

        /// <summary>
        ///     <para>
        ///         Mini Bold font.
        ///     </para>
        /// </summary>
        public static Font miniBoldFont => m_MiniBoldFont;

        /// <summary>
        ///     <para>
        ///         Toolbar background from top of windows.
        ///     </para>
        /// </summary>
        public static GUIStyle toolbar { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style for Button and Toggles in toolbars.
        ///     </para>
        /// </summary>
        public static GUIStyle toolbarButton { get; private set; }

        /// <summary>
        ///     <para>
        ///         Toolbar Popup.
        ///     </para>
        /// </summary>
        public static GUIStyle toolbarPopup { get; private set; }

        /// <summary>
        ///     <para>
        ///         Toolbar Dropdown.
        ///     </para>
        /// </summary>
        public static GUIStyle toolbarDropDown { get; private set; }

        /// <summary>
        ///     <para>
        ///         Toolbar text field.
        ///     </para>
        /// </summary>
        public static GUIStyle toolbarTextField { get; private set; }

        /// <summary>
        ///     <para>
        ///         Wrap content in a vertical group with this style to get the default margins used in the Inspector.
        ///     </para>
        /// </summary>
        public static GUIStyle inspectorDefaultMargins { get; private set; }

        /// <summary>
        ///     <para>
        ///         Wrap content in a vertical group with this style to get full width margins in the Inspector.
        ///     </para>
        /// </summary>
        public static GUIStyle inspectorFullWidthMargins { get; private set; }

        /// <summary>
        ///     <para>
        ///         Style used for background box for EditorGUI.HelpBox.
        ///     </para>
        /// </summary>
        public static GUIStyle helpBox { get; private set; }

        public static GUIStyle toolbarSearchField { get; private set; }

        public static GUIStyle toolbarSearchFieldPopup { get; private set; }

        public static GUIStyle toolbarSearchFieldCancelButton { get; private set; }

        public static GUIStyle toolbarSearchFieldCancelButtonEmpty { get; private set; }

        public static GUIStyle colorPickerBox { get; private set; }

        public static GUIStyle inspectorBig { get; private set; }

        public static GUIStyle inspectorTitlebar { get; private set; }

        public static GUIStyle inspectorTitlebarText { get; private set; }

        public static GUIStyle foldoutSelected { get; private set; }

        public static GUIStyle tooltip { get; private set; }

        public static GUIStyle notificationText { get; private set; }

        public static GUIStyle notificationBackground { get; private set; }

        public static GUIStyle assetLabel { get; private set; }

        public static GUIStyle assetLabelPartial { get; private set; }

        public static GUIStyle assetLabelIcon { get; private set; }

        public static GUIStyle searchField { get; private set; }

        public static GUIStyle searchFieldCancelButton { get; private set; }

        public static GUIStyle searchFieldCancelButtonEmpty { get; private set; }

        public static GUIStyle selectionRect { get; private set; }

        public static GUIStyle minMaxHorizontalSliderThumb { get; private set; }

        public static GUIStyle dropDownList { get; private set; }

        public static GUIStyle progressBarBack { get; private set; }

        public static GUIStyle progressBarBar { get; private set; }

        public static GUIStyle progressBarText { get; private set; }

        public static Vector2 knobSize { get; } = new Vector2(40f, 40f);

        public static Vector2 miniKnobSize { get; } = new Vector2(29f, 29f);

        public static GUIStyle DeleteButton { get; } = GetStyle("OL Minus");
        public static GUIStyle DeleteButtonRed { get; }

        public static GUIStyle GetTempLabelStyle(Color color)
        {
            _labelStyle.normal.textColor = color;
            return _labelStyle;
        }
        
        public static GUIStyle GetTempLabelStyle(int fontSize, FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            return GetTempLabelStyle(Color.white, fontSize, style, alignment);
        }
        
        public static GUIStyle GetTempLabelStyle(Color color, int fontSize, FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            _labelStyle.normal.textColor = color;
            _labelStyle.fontSize = fontSize;
            _labelStyle.fontStyle = style;
            _labelStyle.alignment = alignment;
            return _labelStyle;
        }
        
        private static GUIStyle GetStyle(string styleName)
        {
            GUIStyle guiStyle = GUI.skin.FindStyle(styleName) ?? EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle(styleName);
            if (guiStyle == null)
            {
                Debug.LogError("Missing built-in guistyle " + styleName);
                guiStyle = new GUIStyle();
            }

            return guiStyle;
        }
    }
}