// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   FinalEditorBasicMethods.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using FinalFactory.Utilities;
using UnityEditor;
using UnityEngine;

[assembly:InternalsVisibleTo("FinalFactory.PlayTools.Editor")]
[assembly:InternalsVisibleTo("FinalFactory.Pool.Editor")]

namespace FinalFactory.Editor
{
    internal class FinalEditorBasicMethods
    {
        public const BindingFlags InvokeAttr = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
        public const BindingFlags TypeAttr = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        public static readonly Type ToolbarType;
        public static readonly EditorGUIToolbarSearchFieldDelegate EditorGUIToolbarSearchField;

        private static Type EditorGUIUtilityEditorHeaderItemsMethodsDelegateType;
        //public static readonly EditorDrawHeaderGUIDelegate EditorDrawHeaderGUI;
        //public static readonly Type CustomEditorAttributesType;
        //public static readonly object CustomEditorAttributesInstance;
        //public static readonly CustomEditorAttributesFindCustomEditorTypeByTypeDelegate CustomEditorAttributesFindCustomEditorTypeByType;

        public static IList EditorGUIUtilityEditorHeaderItemsMethods { get; private set; }
        
        static FinalEditorBasicMethods()
        {
            var editorType = typeof(UnityEditor.Editor);
            var editorGUIType = typeof(EditorGUI);
            var editorGUIUtilityType = typeof(EditorGUIUtility);
            
            var editorAssembly = editorType.Assembly;
            
            var editorTypeMethods = editorType.GetMethods(InvokeAttr);
            var editorGUIMethods = editorGUIType.GetMethods(InvokeAttr);
            
            var methodInfo = editorGUIMethods.First(x =>
            {
                var paras = x.GetParameters();
                return x.Name == "ToolbarSearchField" && paras.Length == 4 && paras[0].ParameterType == typeof(Rect) &&
                       paras[1].ParameterType == typeof(string[]);
            });
            
            EditorGUIToolbarSearchField =
                (EditorGUIToolbarSearchFieldDelegate) Delegate.CreateDelegate(typeof(EditorGUIToolbarSearchFieldDelegate), methodInfo);
            
            // methodInfo = editorTypeMethods.First(x =>
            // {
            //     var paras = x.GetParameters();
            //     return x.Name == "DrawHeaderGUI" && paras.Length == 3 && paras[0].ParameterType == editorType &&
            //            paras[1].ParameterType == typeof(string) && paras[2].ParameterType == typeof(float);
            // });
            
            //EditorDrawHeaderGUI = (EditorDrawHeaderGUIDelegate) Delegate.CreateDelegate(typeof(EditorDrawHeaderGUIDelegate), methodInfo);

            ToolbarType = editorAssembly.GetType("UnityEditor.Toolbar");
            
            //CustomEditorAttributesInstance = editorAssembly.CreateInstance("UnityEditor.CustomEditorAttributes");
            
            //Check.NotNull(CustomEditorAttributesInstance, "Creating UnityEditor.CustomEditorAttributes failed.");
            
            //CustomEditorAttributesType = CustomEditorAttributesInstance.GetType();
            
            //methodInfo = CustomEditorAttributesType.GetMethod("FindCustomEditorType", InvokeAttr);
            
            //CustomEditorAttributesFindCustomEditorTypeByType = methodInfo.CreateDelegate<CustomEditorAttributesFindCustomEditorTypeByTypeDelegate>();

        }

        /// <summary>
        /// Adds a new item to the editor header.
        /// Removing is currently not supported.
        /// Return false until an inspector is opened.
        /// </summary>
        /// <param name="func"></param>
        public static bool TryAddEditorHeaderItem(EditorGUIUtilityHeaderItemDelegate func)
        {
            if (EditorGUIUtilityEditorHeaderItemsMethods == null)
            {
                var editorGUIUtilityType = typeof(EditorGUIUtility);
                var fieldInfo = editorGUIUtilityType.GetField("s_EditorHeaderItemsMethods", InvokeAttr);
            
                Check.NotNull(fieldInfo, "Getting EditorGUIUtility.s_EditorHeaderItemsMethods failed.");
            
                EditorGUIUtilityEditorHeaderItemsMethods = (IList)fieldInfo.GetValue(null);

                if (EditorGUIUtilityEditorHeaderItemsMethods == null)
                {
                    return false;
                }
            
                EditorGUIUtilityEditorHeaderItemsMethodsDelegateType = EditorGUIUtilityEditorHeaderItemsMethods.GetType().GetGenericArguments()[0];
            }
            
            EditorGUIUtilityEditorHeaderItemsMethods.Add(Delegate.CreateDelegate(EditorGUIUtilityEditorHeaderItemsMethodsDelegateType, func.Method));

            return true;
        }
        
        internal delegate string EditorGUIToolbarSearchFieldDelegate(Rect position, string[] searchModes, ref int searchMode, string text);

        //internal delegate Rect EditorDrawHeaderGUIDelegate(UnityEditor.Editor editor, string header, float marginLeft);
        
        //internal delegate Type CustomEditorAttributesFindCustomEditorTypeByTypeDelegate(Type type, bool multiEdit);
        
        internal delegate bool EditorGUIUtilityHeaderItemDelegate(Rect rectangle, UnityEngine.Object[] targets);
    }
}