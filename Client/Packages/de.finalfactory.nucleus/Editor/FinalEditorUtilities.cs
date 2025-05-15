using System;
using UnityEditor;
using UnityEngine;

namespace FinalFactory.Editor
{
    public static class FinalEditorUtilities
    {
        // public static Type FindCustomEditorType(UnityEngine.Object o, bool multiEdit = false)
        // {
        //     return FindCustomEditorTypeByType(o.GetType(), multiEdit);
        // }
        //
        // public static Type FindCustomEditorTypeByType(Type type, bool multiEdit = false)
        // {
        //     return FinalEditorBasicMethods.CustomEditorAttributesFindCustomEditorTypeByType(type, false);
        // }
        
        public static Texture2D LoadIcon(string path, string name, bool hasProSkin = true)
        {
            if (!path.EndsWith("/"))
            {
                path += "/";
            }
            if (EditorGUIUtility.isProSkin && hasProSkin)
            {
                path += "d_";
            }
            path += name;
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}