// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   TaggerIdDecoratorDrawer.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Finalfactory.Tagger.Editor
{
    [CustomPropertyDrawer(typeof(TaggerIdAttribute))]
    internal sealed class TaggerIdDecoratorDrawer : PropertyDrawer
    {
        private static readonly Dictionary<string, float> Heights = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var excludePropertyName = ((TaggerIdAttribute)attribute).ExcludePropertyName;
            var r = position;
            r.height = EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(r, label);
            var labelWidth = EditorGUIUtility.labelWidth;
            position.xMin += labelWidth;
            var height = FinalTaggerEditorUI.DrawUI(property, position, excludePropertyName);
            var path = GetPath(property);
            Heights[path] = height;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return Heights.GetValueOrDefault(GetPath(property), 18f);
        }

        private static string GetPath(SerializedProperty property)
        {
            return property.serializedObject.targetObject.GetInstanceID() + property.propertyPath;
        }
    }
}