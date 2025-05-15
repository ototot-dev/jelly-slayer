// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 03.10.2019 : 14:17
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 03.10.2019 : 14:18
// // ***********************************************************************
// // <copyright file="Singleton.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

#if UNITY_5_3_OR_NEWER

using System;
using JetBrains.Annotations;
using UnityEngine;

namespace FinalFactory.Behaviours
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        [CanBeNull] protected static T _instance;
        [NotNull] protected static readonly string Name = typeof(T).Name;

        [CanBeNull]
        [PublicAPI]
        public static T Instance
        {
            get
            {
                if (!_instance)
                {
                    Initialize();
                    
                    if (!_instance)
                    {
                        Debug.LogError("There is no Instance of " + Name + " available!");
                    }
                }

                return _instance;
            }
        }

        protected static void Initialize()
        {
            if (!_instance)
            {
                _instance = FindObjectOfType<T>();
            }
        }

        [PublicAPI]
        public static bool InstanceExists => _instance != null;

        [PublicAPI]
        protected virtual void Awake()
        {
            T[] instances = FindObjectsOfType<T>();
            if (instances.Length > 1 || instances.Length > 0 && instances[0] != this)
            {
                Debug.LogError("Duplicate Singleton of Type:  " + Name + Environment.NewLine +
                               "The old instance will be overwritten.");
            }
            _instance = (T) this;
        }

        [PublicAPI]
        protected virtual void OnDestroy()
        {
            if (this == _instance)
            {
                _instance = null;
            }
            else if (_instance != null)
            {
                Debug.LogWarning("An old discarded singleton instance was deleted of the type: " + Name);
            }
        }
    }
}

#endif