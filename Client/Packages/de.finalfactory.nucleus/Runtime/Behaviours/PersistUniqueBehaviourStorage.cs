// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : .. : 
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 29.04.2020 : 11:29
// // ***********************************************************************
// // <copyright file="PersistUniqueBehaviourStorage.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************
#if UNITY_5_3_OR_NEWER
using System.Diagnostics;
using UnityEngine;

namespace FinalFactory.Behaviours
{
    public static class PersistUniqueBehaviourStorage
    {
        private static GameObject _gameObject;

        public static T AddComponent<T>() where T : Component
        {
            if (_gameObject == null)
            {
                _gameObject = new GameObject();
                //_gameObject.hideFlags = HideFlags.DontSave;
                Object.DontDestroyOnLoad(_gameObject);
            }

            var component = _gameObject.GetComponent<T>();

            if (component == null)
            {
                component = _gameObject.AddComponent<T>();
            }

            UpdateName();
            return component;
        }

        [Conditional("UNITY_EDITOR")]
        private static void UpdateName()
        {
            Component[] components = _gameObject.GetComponents<Component>();
            _gameObject.name = $"Persist Static Managers[{components.Length - 1}]:";
            foreach (var component in components)
            {
                if (component is Transform or RectTransform)
                {
                    continue;
                }
                _gameObject.name += $" {component.GetType().Name}";
            }
        }
    }
}

#endif