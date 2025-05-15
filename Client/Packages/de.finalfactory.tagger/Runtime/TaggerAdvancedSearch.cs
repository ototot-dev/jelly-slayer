// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   TaggerAdvancedSearch.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Finalfactory.Tagger
{
    public enum TaggerSearchMode
    {
        /// <summary>
        /// The search must match exactly the tags.
        /// </summary>
        Exact,
        
        /// <summary>
        /// The search must match all tags.
        /// </summary>
        And,

        /// <summary>
        /// The search must match one of the tags.
        /// </summary>
        Or,

        /// <summary>
        /// The search must not match the tags.
        /// </summary>
        Not
    }

    public class TaggerAdvancedSearch
    {
        private readonly TaggerData _data;

        private bool _dirty;

        private int _lastVersion;

        private TagArray _tagArray2;

        public TaggerAdvancedSearch(bool include, params string[] tags)
            : this()
        {
            if (!include) Mode = TaggerSearchMode.Not;

            Tags = new List<string>(tags);
        }

        public TaggerAdvancedSearch()
        {
            _data = TaggerSystem.Data;
            _dirty = true;
        }

        /// <summary>
        ///     Gets the created cases of this search. Cases are created by using OR Mode. Like Case 1 OR Case 2
        /// </summary>
        public List<TagArray> Cases { get; } = new();

        /// <summary>
        ///     Gets the child search of this search.
        /// </summary>
        public TaggerAdvancedSearch Child { get; private set; }

        // ReSharper disable once MemberCanBePrivate.Global
        /// <summary>
        ///     Define if the search is changed.
        /// </summary>
        public bool IsDirty => _dirty || (Child != null && Child.IsDirty);

        /// <summary>
        ///     Gets the mode of this search.
        /// </summary>
        public TaggerSearchMode Mode { get; private set; } = TaggerSearchMode.And;

        /// <summary>
        ///     Gets the created id list on this search.
        /// </summary>
        public HashSet<int> TagArrayIDs { get; private set; }

        /// <summary>
        ///     Get the specified tags on this search.
        /// </summary>
        public List<string> Tags { get; private set; }

        public static TaggerAdvancedSearch operator &(TaggerAdvancedSearch c1, TaggerAdvancedSearch c2)
        {
            c1.And(c2);
            return c1;
        }

        public static TaggerAdvancedSearch operator |(TaggerAdvancedSearch c1, TaggerAdvancedSearch c2)
        {
            c1.Or(c2);
            return c1;
        }

        /// <summary>
        ///     Set the specified <see cref="TaggerAdvancedSearch" /> in AND Mode to this existing.
        /// </summary>
        /// <param name="and"></param>
        /// <returns></returns>
        public TaggerAdvancedSearch And(TaggerAdvancedSearch and)
        {
            SetChild(TaggerSearchMode.And, and);
            _dirty = true;
            return this;
        }

        /// <summary>
        ///     Create a new <see cref="TaggerAdvancedSearch" /> set the specified tags to it and combine the created search to
        ///     this existing in AND Mode
        /// </summary>
        /// <param name="tags"></param>
        /// <returns>The new <see cref="TaggerAdvancedSearch" /></returns>
        public TaggerAdvancedSearch And(params string[] tags)
        {
            var search = new TaggerAdvancedSearch(true, tags);
            SetChild(TaggerSearchMode.And, search);
            return search;
        }

        /// <summary>
        ///     Set the specified <see cref="TaggerAdvancedSearch" /> in NOT Mode to this existing.
        /// </summary>
        /// <param name="not"></param>
        /// <returns></returns>
        public TaggerAdvancedSearch Not(TaggerAdvancedSearch not)
        {
            SetChild(TaggerSearchMode.Not, not);
            return this;
        }

        /// <summary>
        ///     Create a new <see cref="TaggerAdvancedSearch" /> set the specified tags to it and combine the created search to
        ///     this existing in NOT Mode
        /// </summary>
        /// <param name="tags"></param>
        /// <returns>The new <see cref="TaggerAdvancedSearch" /></returns>
        public TaggerAdvancedSearch Not(params string[] tags)
        {
            var search = new TaggerAdvancedSearch(true, tags);
            SetChild(TaggerSearchMode.Not, search);
            return search;
        }

        /// <summary>
        ///     Set the specified <see cref="TaggerAdvancedSearch" /> in OR Mode to this existing.
        /// </summary>
        /// <param name="or"></param>
        /// <returns></returns>
        public TaggerAdvancedSearch Or(TaggerAdvancedSearch or)
        {
            SetChild(TaggerSearchMode.Or, or);
            return this;
        }

        /// <summary>
        ///     Create a new <see cref="TaggerAdvancedSearch" /> set the specified tags to it and combine the created search to
        ///     this existing in OR Mode
        /// </summary>
        /// <param name="tags"></param>
        /// <returns>The new <see cref="TaggerAdvancedSearch" /></returns>
        public TaggerAdvancedSearch Or(params string[] tags)
        {
            var search = new TaggerAdvancedSearch(true, tags);
            SetChild(TaggerSearchMode.Or, search);
            return search;
        }

        /// <summary>
        ///     Returns the matched gameobjects with this search.
        /// </summary>
        /// <returns></returns>
        public void MatchedGameObjects(HashSet<GameObject> list)
        {
            if (IsDirty || _lastVersion != _data.Version) Research();
            list.Clear();
            foreach (var tagArrayId in TagArrayIDs)
            {
                TaggerManager.Instance.Get(tagArrayId, out var result);
                if (result == null) continue;
                foreach (var gameObject in result) list.Add(gameObject);
            }
        }

        /// <summary>
        ///     Returns the matched gameobjects with this search.
        /// </summary>
        /// <returns></returns>
        public HashSet<GameObject> MatchedGameObjects()
        {
            if (IsDirty || _lastVersion != _data.Version) Research();

            var list = new HashSet<GameObject>();
            foreach (var tagArrayId in TagArrayIDs)
            {
                TaggerManager.Instance.Get(tagArrayId, out var result);
                if (result == null) continue;
                foreach (var gameObject in result) list.Add(gameObject);
            }

            return list;
        }

        /// <summary>
        ///     Returns the matched gameobjects with this search.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GameObject> MatchedGameObjectsEnumerable()
        {
            if (IsDirty || _lastVersion != _data.Version) Research();

            foreach (var tagArrayId in TagArrayIDs)
            {
                TaggerManager.Instance.Get(tagArrayId, out var result);
                if (result != null)
                    foreach (var gameObject in result)
                    {
                        if (IsDirty || _lastVersion != _data.Version)
                            throw new InvalidOperationException(
                                "It is not allowed to modify this search or to add/modify/delete groups or add/modify/delete tags. (BUT, it is allowed to add or remove known tags to gameobjects)");
                        yield return gameObject;
                    }
            }
        }

        public HashSet<GameObject> MatchedPrefabs(string path)
        {
            if (IsDirty || _lastVersion != _data.Version) Research();

            var list = new HashSet<GameObject>();
            var allPrefabs = Resources.LoadAll<GameObject>(path);
            var allTaggedPrefabs =
                allPrefabs.Select(x => x.GetComponent<FinalTagger>()).Where(x => x != null).ToArray();
            foreach (var tagArrayId in TagArrayIDs)
            {
                var gameObjects = allTaggedPrefabs.Where(x => x.TagId == tagArrayId);
                foreach (var gameObject in gameObjects) list.Add(gameObject.gameObject);
            }

            return list;
        }


        /// <summary>
        ///     Buildup the internal id collection for the search. This must not be called.
        ///     It is called by <see cref="MatchedGameObjects" /> if this search is dirty.
        ///     But you can it call to save some ms before you use the search.
        /// </summary>
        public void Research()
        {
            Cases.Clear();
            var array = new TagArray();
            Cases.Add(array);
            Process(Cases, array, null);

            TagArrayIDs = new HashSet<int>();
            foreach (var t in Cases)
            {
                var tagArrayID = _data.GetMatchedIDs(t);
                foreach (var id in tagArrayID) TagArrayIDs.Add(id);
            }

            _dirty = false;
        }

        public void SetChild(TaggerSearchMode mode, TaggerAdvancedSearch other)
        {
            Child = other;
            if (Child != null) Child.Mode = mode;

            _dirty = true;
        }

        public TaggerAdvancedSearch SetTags(params string[] tags)
        {
            Tags = new List<string>(tags);
            _dirty = true;
            return this;
        }

        private void CreateTagArrays()
        {
            _lastVersion = _data.Version;
            _tagArray2 = _data.GetTagArrayOfTags(Tags.ToArray());
        }

        private void Process(List<TagArray> cases, TagArray baseTags, TagArray lastArray)
        {
            CreateTagArrays();
            TagArray last = null;
            if (Child != null && Child.Mode == TaggerSearchMode.Or)
            {
                if (lastArray != null)
                    last = (TagArray)lastArray.Clone();
                else
                    last = (TagArray)baseTags.Clone();
            }

            if (Tags.Count > 0)
                switch (Mode)
                {
                    case TaggerSearchMode.And:
                        baseTags.Or(_tagArray2);

                        Child?.Process(cases, baseTags, last);

                        break;
                    case TaggerSearchMode.Or:
                        // ReSharper disable once PossibleNullReferenceException
                        lastArray.Or(_tagArray2);
                        cases.Add(lastArray);
                        Child?.Process(cases, lastArray, last);

                        break;
                    case TaggerSearchMode.Not:
                        baseTags.SetMask(_tagArray2);

                        Child?.Process(cases, baseTags, last);

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(cases), Mode, null);
                }

            if (Child != null) Child._dirty = false;
        }
    }
}