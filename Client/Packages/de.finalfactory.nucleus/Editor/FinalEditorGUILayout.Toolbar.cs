using System;
using System.Collections.Generic;
using System.Globalization;
using FinalFactory.Helpers;
using FinalFactory.Unity.Editor;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace FinalFactory.Editor
{
    [PublicAPI]
    public static partial class FinalEditorGUILayout
    {
        [PublicAPI]
        public static void BeginToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
        }

        [PublicAPI]
        public static T ToolbarEnum<T>(T selected, params GUILayoutOption[] options) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            T[] names = (T[]) Enum.GetValues(selected.GetType());
            foreach (T name in names)
            {
                bool isOn = EqualityComparer<T>.Default.Equals(selected, name);
                string strName = name.ToString(CultureInfo.InvariantCulture);

                if (ToolbarToggle(strName, isOn, options))
                {
                    selected = name;
                }
            }

            return selected;
        }

        [PublicAPI]
        public static T ToolbarEnum<T>(T selected, GUIContent[] content, params GUILayoutOption[] options) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            T[] names = (T[]) Enum.GetValues(selected.GetType());
            for (int i = 0; i < names.Length; i++)
            {
                T name = names[i];
                GUIContent c = content[i];
                bool isOn = EqualityComparer<T>.Default.Equals(selected, name);

                if (ToolbarToggle(c, isOn, options))
                {
                    selected = name;
                }
            }

            return selected;
        }

        [PublicAPI]
        public static bool ToolbarToggle(string name, bool isOn, params GUILayoutOption[] options)
        {
            return GUILayout.Toggle(isOn, name, EditorStyles.toolbarButton, options);
        }

        [PublicAPI]
        public static bool ToolbarToggle(GUIContent name, bool isOn, params GUILayoutOption[] options)
        {
            return GUILayout.Toggle(isOn, name, EditorStyles.toolbarButton, options);
        }

        [PublicAPI]
        public static void ToolbarToggleEnumFlag<T>(GUIContent content, ref T theEnum, T flag, params GUILayoutOption[] options)
            where T : struct, Enum
        {
            theEnum = ToolbarToggleEnumFlag(content, theEnum, flag, options);
        }

        [PublicAPI]
        public static T ToolbarToggleEnumFlag<T>(GUIContent content, T theEnum, T flag, params GUILayoutOption[] options) where T : struct, Enum
        {
            var (enumInt, flagInt) = EnumHelpers.EnumAndFlagToInt(theEnum, flag);

            bool hasFlag = (enumInt & flagInt) == flagInt;

            hasFlag = ToolbarToggle(content, hasFlag, options);

            return Flags.Set<T>(hasFlag, enumInt, flagInt);
        }

        [PublicAPI]
        public static T ToolbarPopup<T>(T selected, params GUILayoutOption[] options) where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            var names = (T[]) Enum.GetValues(selected.GetType());
            var displayedOptions = new GUIContent[names.Length];
            for (var i = 0; i < names.Length; i++)
            {
                displayedOptions[i] = new GUIContent(names[i].ToString(CultureInfo.InvariantCulture));
            }

            var selectedIndex = Array.IndexOf(names, selected);
            selectedIndex = ToolbarPopup(GUIContent.none, selectedIndex, displayedOptions, options);

            return names[selectedIndex];
        }
        
        [PublicAPI]
        public static int ToolbarPopup(GUIContent label, int selectedIndex, GUIContent[] displayedOptions, params GUILayoutOption[] option)
        {
            return EditorGUILayout.Popup(label, selectedIndex, displayedOptions, EditorStyles.toolbarPopup, option);
        }
        [PublicAPI]
        public static int ToolbarPopup(string label, int selectedIndex, string[] displayedOptions, params GUILayoutOption[] option)
        {
            return EditorGUILayout.Popup(label, selectedIndex, displayedOptions, EditorStyles.toolbarPopup, option);
        }

        [PublicAPI]
        public static bool ToolbarButton(GUIContent content, params GUILayoutOption[] options)
        {
            return GUILayout.Button(content, EditorStyles.toolbarButton, options);
        }
        [PublicAPI]
        public static bool ToolbarButton(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            content.image = style.normal.background;
            var rt = GUILayoutUtility.GetRect(content, style, options);
            if (rt.Contains(Event.current.mousePosition)) {
                content.image = style.hover.background;;
            }
            return GUI.Button(rt, content, EditorStyles.toolbarButton);
        }
        
        
        [PublicAPI]
        public static bool ToolbarSettingsButton()
        {
            return GUILayout.Button(EditorGUIUtility.IconContent("_Popup"), EditorStyles.toolbarButton, GUILayout.Width(30));
        }

        [PublicAPI]
        public static string ToolbarSearchField(string text, params GUILayoutOption[] options)
        {
            text = EditorGUILayout.TextField(text, FinalEditorStyles.searchField);
            if (GUILayout.Button("", FinalEditorStyles.searchFieldCancelButton))
            {
                text = "";
                GUI.FocusControl(null);
            }

            return text;
        }

        [PublicAPI]
        public static string ToolbarSearchField(string text, string[] searchModes, ref int searchMode, GUIStyle style, params GUILayoutOption[] options)
        {
            int controlId = GUIUtility.GetControlID(FocusType.Keyboard);
            GUIContent content = GUIUtility.keyboardControl == controlId ? new GUIContent(text + Input.compositionString) : new GUIContent(text);
            Rect rect = GUILayoutUtility.GetRect(content, style, options);
            return FinalEditorBasicMethods.EditorGUIToolbarSearchField(rect, searchModes, ref searchMode, text);
        }

        [PublicAPI]
        public static void EndToolbar()
        {
            GUILayout.EndHorizontal();
        }
    }
}