// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : .. : 
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 29.04.2020 : 10:07
// // ***********************************************************************
// // <copyright file="PersistUniqueBehaviour.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

#if UNITY_5_3_OR_NEWER

using System;
using JetBrains.Annotations;

namespace FinalFactory.Behaviours
{
    public abstract class PersistUniqueBehaviour<T> : SingletonBehaviour<T> where T : PersistUniqueBehaviour<T>
    {
        [NotNull]
        [PublicAPI]
        public new static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    Initialize();
                    
                    if (_instance == null)
                    {
                        throw new NullReferenceException(Name);
                    }
                }

                return _instance;
            }
        }

        protected new static void Initialize()
        {
            SingletonBehaviour<T>.Initialize();
            if (!_instance)
            {
                _instance = PersistUniqueBehaviourStorage.AddComponent<T>();
            }
        }
    }
}

#endif