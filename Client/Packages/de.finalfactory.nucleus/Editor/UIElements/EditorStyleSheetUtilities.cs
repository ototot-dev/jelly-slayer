using UnityEditor;
using UnityEditor.iOS;
using UnityEngine;
using UnityEngine.UIElements;

namespace FinalFactory.Editor.UIElements
{
    public class EditorStyleSheetUtilities
    {
        public static void ApplyDefaultStyleSheet(VisualElement element)
        {
            var path = EditorGUIUtility.isProSkin ? "FinalFactory/EditorDarkStyle" : "FinalFactory/EditorLightStyle";
            element.styleSheets.Add(Resources.Load<StyleSheet>(path));
        }
        
        public static void ApplyEditorStyleSheet(VisualElement element, string path)
        {
            if (path.EndsWith(".uss"))
            {
                var pathWithoutExtension = path.Substring(0, path.Length - 4);
                //check if a light and dark version of the stylesheet exists
                var lightPath = pathWithoutExtension + "Light.uss";
                var darkPath = pathWithoutExtension + "Dark.uss";
                if (EditorGUIUtility.isProSkin && darkPath != path)
                {
                    element.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(darkPath));
                }
                else if (!EditorGUIUtility.isProSkin && lightPath != path)
                {
                    element.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(lightPath));
                }
            }
            element.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(path));
        }
    }
}