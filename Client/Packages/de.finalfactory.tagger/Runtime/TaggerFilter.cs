// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   TaggerFilter.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Finalfactory.Tagger
{
    [Serializable]
    public class TaggerFilter
    {
        [TaggerId(nameof(_excludeTagId))] [SerializeField]
        private int _includeTagId = -1;

        [TaggerId(nameof(_includeTagId))] [SerializeField]
        private int _excludeTagId = -1;

        public TaggerFilter()
        {
        }

        public TaggerFilter(string[] includeTags, string[] excludeTags)
        {
            SetIncludeTags(includeTags);
            SetExcludeTags(excludeTags);
        }

        /// <summary>
        ///     Returns a list of the include tags
        /// </summary>
        /// <returns></returns>
        public HashSet<string> GetIncludeTags() => TaggerSystem.Data.GetTagsOfId(_includeTagId);

        /// <summary>
        ///     Returns a list of the exclude tags
        /// </summary>
        public HashSet<string> GetExcludeTags() => TaggerSystem.Data.GetTagsOfId(_excludeTagId);

        /// <summary>
        ///     Set the include tags
        /// </summary>
        public void SetIncludeTags(params string[] tags)
        {
            if (tags == null || tags.Length == 0)
                _includeTagId = -1;
            else
                _includeTagId = TaggerSystem.Data.TagsToId(tags);
        }
        
        /// <summary>
        /// Set the include tags using the tagger id
        /// </summary>
        public void SetIncludeTags(int taggerId) => _includeTagId = taggerId;

        /// <summary>
        ///     Set the exclude tags
        /// </summary>
        public void SetExcludeTags(params string[] tags)
        {
            if (tags == null || tags.Length == 0)
                _excludeTagId = -1;
            else
                _excludeTagId = TaggerSystem.Data.TagsToId(tags);
        }

        /// <summary>
        ///     Set the exclude tags using the tagger id
        /// </summary>
        public void SetExcludeTags(int taggerId) => _excludeTagId = taggerId;

        public bool Match(GameObject gO)
        {
            var finalTagger = gO.GetComponent<FinalTagger>();
            if (finalTagger == null) return false;
            var include = TaggerSystem.Data.GetTagArrayOfId(_includeTagId);
            var exclude = TaggerSystem.Data.GetTagArrayOfId(_excludeTagId);
            var gameObject = TaggerSystem.Data.GetTagArrayOfId(finalTagger.TagId);

            if (include != null && !gameObject.Contains(include)) return false;

            if (exclude != null && gameObject.Any(exclude)) return false;

            return true;
        }
    }
}