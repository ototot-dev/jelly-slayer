using System;
using System.Collections.Generic;
using System.Reflection;
using FinalFactory.Logging;
using FinalFactory.Tooling;
using FinalFactory.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FinalFactory.Editor
{
    internal delegate void ComponentDrawHeaderItemDelegate(Object target, Rect rect);
    
    public class FinalEditorComponentHeaderItemManager
    {
        private static readonly Log Log = LogManager.GetLogger(typeof(FinalEditorComponentHeaderItemManager));
        private static readonly Dictionary<Type, ComponentDrawHeaderItemDelegate> Types = new();

        [InitializeOnLoadMethod]
        private static void Initialize() => EditorApplication.update += TryInitialize;

        private static void TryInitialize()
        {
            if (!FinalEditorBasicMethods.TryAddEditorHeaderItem(DrawHeaderItem))
            {
                //Could not add the header item
                //Probably because the inspector is not open or not used.
                return;
            }

            var methods = TypeCache.GetMethodsWithAttribute<CustomHeaderItemAttribute>();
            foreach (var method in methods)
            {
                if (!method.IsStatic)
                {
                    Log.Error($"Method {method.Name} must be static");
                    continue;
                }
                
                //check parameter
                var parameterInfos = method.GetParameters();
                
                if (parameterInfos.Length != 2)
                {
                    Log.Error($"Method {method.Name} must have only one parameter");
                    continue;
                }
                
                if (parameterInfos[0].ParameterType != typeof(Object))
                {
                    Log.Error($"Method {method.Name} must have a parameter of type UnityEngine.Object");
                    continue;
                }
                
                if (parameterInfos[1].ParameterType != typeof(Rect))
                {
                    Log.Error($"Method {method.Name} must have a parameter of type UnityEngine.Rect");
                    continue;
                }
                
                //check return type
                if (method.ReturnType != typeof(void))
                {
                    Log.Error($"Method {method.Name} must have a return type of void");
                    continue;
                }

                var editorType = method.ReflectedType;
                
                //read attribute data from class type
                var attribute = editorType.GetCustomAttribute<CustomEditor>();

                if (attribute == null)
                {
                    Log.Error($"Method {method.Name} must be in a class with the CustomEditor attribute");
                    continue;
                }
                
                var inspectedType = attribute.GetValue<Type>("m_InspectedType");
                
                Types.TryAdd(inspectedType, method.CreateDelegate<ComponentDrawHeaderItemDelegate>());
            }

            EditorApplication.update -= TryInitialize;
        }

        private static bool DrawHeaderItem(Rect rect, Object[] targets)
        {
            var target = targets[0];

            if (Types.TryGetValue(target.GetType(), out var method))
            {
                method.Invoke(target, rect);
                return true;
            }
            return false;
        }
    }
}
