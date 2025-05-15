// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   TaggerManager.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

//#define FINALFACTORY_TAGGER_TRACE

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly: InternalsVisibleTo("Finalfactory.Tagger.Editor")]

namespace Finalfactory.Tagger
{
    /// <summary>
    ///     The Tagger Manager.
    ///     There must be one per Scene.
    ///     All tagable GameObjects in the Scene are registered here, for the tag search.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TaggerManager : MonoBehaviour
    {
        private static TaggerManager _instance;

        private readonly Dictionary<int, HashSet<GameObject>> _gOs = new();

        /// <summary>
        ///     Returns if the TaggerManager is already existing. Does not create any Gameobject.
        /// </summary>
        public static bool Exist => _instance != null;

        /// <summary>
        ///     Returns true if the TaggerManager was initialized.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public static bool Initialized { get; private set; }

        /// <summary>
        ///     The current Instance of the TaggerManager.
        ///     There can only be one TaggerManager per Scene.
        ///     Note: If there is no TaggerManager Instance, calling this property creates a new GameObject with the TaggerManager
        ///     Script attached.
        /// </summary>
        public static TaggerManager Instance
        {
            get
            {
                if (_instance == null) new GameObject("Tag Manager").AddComponent<TaggerManager>();

                return _instance;
            }
        }

        protected virtual void Awake()
        {
            _instance = this;
        }

        protected virtual void Start()
        {
            Initialize();
        }

        protected virtual void OnDestroy()
        {
            _instance = null;
        }

        /// <summary>
        ///     Returns a new array of GameObjects that have this Identification Number
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GameObject[] Get(int id)
        {
            if (_gOs.TryGetValue(id, out var list)) return list.ToArray();

            return null;
        }

        /// <summary>
        ///     Returns a new array of GameObjects that have this Identification Number
        /// </summary>
        /// <param name="id"></param>
        /// <param name="result"></param>
        public void Get(int id, out IReadOnlyCollection<GameObject> result)
        {
            result = _gOs.GetValueOrDefault(id);
        }

        /// <summary>
        ///     Returns a new array of GameObjects that have this Identification Number
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public HashSet<GameObject> GetAsHashSet(int id)
        {
            if (_gOs.TryGetValue(id, out var list)) return list.ToHashSet();

            return null;
        }
        
        /// <summary>
        ///     Returns a single (first) GameObjects that have this Identification Number
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public GameObject GetSingle(int id)
        {
            if (_gOs.TryGetValue(id, out var list)) return list.FirstOrDefault();

            return null;
        }
        
        /// <summary>
        /// Returns all GameObjects that are tagged.
        /// </summary>
        /// <returns></returns>
        public HashSet<GameObject> GetAllTaggedGameObjects()
        {
            var result = new HashSet<GameObject>();
            foreach (var go in _gOs.Values)
            {
                result.UnionWith(go);
            }
            return result;
        }

        /// <summary>
        ///     DO N_O_T USE THIS METHOD. IT COULD BREAK THE TAGGERSYSTEM.
        ///     If you need access, contact me. So I can figure out why
        ///     it makes sense to make this method public.
        /// </summary>
        internal void INTERNAL_ModifyGameObject(GameObject go, int oldValue, int newValue)
        {
            if (!_gOs[oldValue].Remove(go))
            {
                TaggerSystem.Logger.Fatal($"Cloud not move GameObject {go.name} from TagID {oldValue} to {newValue}");
            }
            if (!_gOs.ContainsKey(newValue)) _gOs.Add(newValue, new HashSet<GameObject>());

            _gOs[newValue].Add(go);
#if FINALFACTORY_TAGGER_TRACE
            TaggerSystem.Logger.Trace($"Moved GameObject {go.name} from TagID {oldValue} to {newValue}");
#endif
        }

        /// <summary>
        ///     DO N_O_T USE THIS METHOD. IT COULD BREAK THE TAGGERSYSTEM.
        ///     If you need access, contact me. So I can figure out why
        ///     it makes sense to make this method public.
        /// </summary>
        internal void INTERNAL_UnRegisterGameObject(GameObject go, int tagID)
        {
            if(!_gOs[tagID].Remove(go))
            {
                TaggerSystem.Logger.Fatal($"Cloud not remove GameObject {go.name} from TagID {tagID}");
            }
#if FINALFACTORY_TAGGER_TRACE
            TaggerSystem.Logger.Trace($"Removed GameObject {go.name} from TagID {tagID}");
#endif
        }

        /// <summary>
        ///     DO N_O_T USE THIS METHOD. IT COULD BREAK THE TAGGERSYSTEM.
        ///     If you need access, contact me. So I can figure out why
        ///     it makes sense to make this method public.
        /// </summary>
        internal void INTERNAL_RegisterGameObject(GameObject go, int tagID)
        {
            if (!_gOs.ContainsKey(tagID)) _gOs.Add(tagID, new HashSet<GameObject>());

            _gOs[tagID].Add(go);
#if FINALFACTORY_TAGGER_TRACE
            TaggerSystem.Logger.Trace($"Registered GameObject {go.name} with TagID {tagID}");
#endif
        }
        
        protected virtual void Initialize()
        {
            Initialized = true;
        }
    }
}