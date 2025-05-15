#region License
// // --------------------------------------------------------------------------------------------------------------------
// // <summary>
// //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// //   FinalEditorGUILayout.Stuff.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // </summary>
// // --------------------------------------------------------------------------------------------------------------------
#endregion

using UnityEditor;
using UnityEngine;

namespace FinalFactory.Editor
{
    public partial class FinalEditorGUILayout
    {
        private static GUIContent _TempContent = new GUIContent();
        private static GUIStyle _HelpBoxStyle;
        public static void HelpBox(string message, Texture2D icon, int fontSize = 14)
        {
            _HelpBoxStyle ??= new GUIStyle(EditorStyles.helpBox);
            _HelpBoxStyle.fontStyle = FontStyle.Bold;
            _HelpBoxStyle.fontSize = fontSize;
            EditorGUILayout.LabelField(GUIContent.none, TempContent(message, icon), _HelpBoxStyle);
        }
        
        private static GUIContent TempContent(string message, Texture2D icon)
        {
            _TempContent.text = message;
            _TempContent.image = icon;
            return _TempContent;
        }
    }
}