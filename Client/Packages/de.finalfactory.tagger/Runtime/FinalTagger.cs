// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   FinalTagger.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using UnityEditor;
using UnityEngine;

namespace Finalfactory.Tagger
{
    /// <summary>
    ///     This <see cref="MonoBehaviour" /> is added to every <see cref="GameObject" /> that need the features of
    ///     <see cref="TaggerSystem" />.
    /// </summary>
    [DisallowMultipleComponent]
    public class FinalTagger : MonoBehaviour
    {
        [SerializeField] private int _tagId = -1;

        /// <summary>
        ///     Gets or sets the special Identification number for the tags on this <see cref="GameObject" />
        /// </summary>
        public int TagId
        {
            get { return _tagId; }

            set
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                    TaggerManager.Instance.INTERNAL_ModifyGameObject(gameObject, _tagId, value);

                _tagId = value;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();
                }
#endif
            }
        }

        private void Awake()
        {
            TaggerSystem.Data.INTERNAL_CheckForOldID(ref _tagId);
            TaggerManager.Instance.INTERNAL_RegisterGameObject(gameObject, TagId);
        }

        private void OnDestroy()
        {
            if (TaggerManager.Exist) TaggerManager.Instance.INTERNAL_UnRegisterGameObject(gameObject, TagId);
        }
    }
}