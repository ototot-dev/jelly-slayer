// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   TaggerDataLoaderDefault.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using System;
using FinalFactory.Logging;
using FinalFactory.Utilities;
using UnityEditor;
using UnityEngine;

namespace Finalfactory.Tagger
{
#if UNITY_EDITOR
#endif

    /// <summary>
    ///     Default implimentation of the <see cref="ITaggerDataLoader" />.
    ///     Loads in Editor and Runtime the <see cref="TaggerData" /> from the Resources Folder.
    ///     Saves only in Editortime.
    ///     If you need saving in Runtime, please implimentated your own soluiton with the <see cref="ITaggerDataLoader" />
    ///     interface and
    ///     set the Property <see cref="TaggerSystem._dataLoader" /> with your loader.
    /// </summary>
    public class TaggerDataLoaderDefault : ITaggerDataLoader
    {
        internal static Action OnDataChanged;
        private static readonly Log Log = TaggerSystem.Logger;
        private TaggerData _data;

        public TaggerData GetData()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
#endif
                if (_data != null)
                    return _data;

            return SingletonScriptableLoader.Load<TaggerData>("Assets/Resources/", "TaggerData", null, o =>
            {
#if UNITY_EDITOR
                o.GetOrAddGroup("Ungrouped");
#else
                Log.Error("Data not found!");
#endif
            });
        }

        public void RequestSave(TaggerData data)
        {
            if (!Application.isPlaying)
            {
#if UNITY_EDITOR
                EditorUtility.SetDirty(data);
                AssetDatabase.SaveAssetIfDirty(data);
                OnDataChanged?.Invoke();
#endif
            }
        }
    }
}