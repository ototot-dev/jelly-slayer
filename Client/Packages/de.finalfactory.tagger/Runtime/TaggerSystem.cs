// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   TaggerSystem.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FinalFactory;
using FinalFactory.Logging;
using Finalfactory.Tagger;
using FinalFactory.Utilities;

[assembly: InternalsVisibleTo("HeiKyu.Tagger.Editor")]
[assembly: InternalsVisibleTo("FinalFactory.Tagger.Tests")]

// ReSharper disable once CheckNamespace
namespace UnityEngine
{
    public static class TaggerSystem
    {
        internal static readonly Log Logger = LogManager.GetLogger("TAGGER");
        /// <summary>
        ///     DO N_O_T USE THIS FIELD. IT COULD BREAK THE TAGGERSYSTEM.
        ///     If you need access, contact me. So I can figure out why
        ///     it makes sense to make this field public.
        /// </summary>
        // ReSharper disable once InconsistentNaming
#pragma warning disable 414
        internal static TaggerData INTERNAL_Data;
#pragma warning restore 414

        private static ITaggerDataLoader _dataLoader;

        /// <summary>
        ///     Returns the current Instance of <see cref="TaggerData" /> or if there is no instance then loads a new one from
        ///     current <see cref="ITaggerDataLoader" />.
        /// </summary>
        public static TaggerData Data
        {
            get
            {
#if UNITY_EDITOR
                //No caching in editor. Because the original data could be deleted.
                return DataLoader.GetData();
#else
                return INTERNAL_Data ?? (INTERNAL_Data = DataLoader.GetData());
#endif
            }
        }

        /// <summary>
        ///     Returns the current Instance of <see cref="ITaggerDataLoader" /> or if there is no instance then creates a new
        ///     <see cref="TaggerDataLoaderDefault" />.
        /// </summary>
        public static ITaggerDataLoader DataLoader
        {
            get => _dataLoader ??= new TaggerDataLoaderDefault();

            set
            {
                INTERNAL_Data = null;
                _dataLoader = value;
            }
        }

        #region Accessing GameObject Tags

        #region Add

        /// <summary>
        ///     Adds a tag to a GameObject.
        ///     If the tag does not exist, it will be created and added to the "Ungrouped" group.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag"></param>
        public static void AddTag(this GameObject gameObject, string tag)
        {
            var data = Data;

            data.AddTag(tag);

            var finalTagger = gameObject.GetComponent<FinalTagger>();
            if (!finalTagger) finalTagger = gameObject.AddComponent<FinalTagger>();

            var tagArray = data.GetTagArrayOfId(finalTagger.TagId) ?? new TagArray();
            var group = data.GetGroupOfTag(tag);
            if (group.IsSingleton) RemoveAllTagsOfGroupFromTagArray(data, tagArray, group);
            tagArray.Set(data.IndexOfTag(tag), true);

            var tagId = data.TagsToId(tagArray);

            finalTagger.TagId = tagId;
        }
        
        /// <summary>
        ///     Adds multiple tags to a GameObject.
        ///     If the tag does not exist, it will be created and added to the "Ungrouped" group.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tags"></param>
        public static void AddTags(this GameObject gameObject, params string[] tags)
        {
            var data = Data;

            data.AddTags(tags);

            var finalTagger = gameObject.GetComponent<FinalTagger>();
            if (!finalTagger) finalTagger = gameObject.AddComponent<FinalTagger>();

            var tagArray = data.GetTagArrayOfId(finalTagger.TagId) ?? new TagArray();

            for (var i = 0; i < tags.Length; i++)
            {
                var tag = tags[i];
                var group = data.GetGroupOfTag(tag);
                if (group.IsSingleton) RemoveAllTagsOfGroupFromTagArray(data, tagArray, group);

                tagArray.Set(data.IndexOfTag(tag), true);
            }

            var tagId = data.TagsToId(tagArray);

            finalTagger.TagId = tagId;
        }
       
        /// <summary>
        ///     Adds all tags that are associated with the given tagId.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag"></param>
        public static void AddTagId(this GameObject gameObject, int tagId)
        {
            var data = Data;

            var tags = data.GetTagsOfId(tagId);
            if (tags == null)
            {
                Logger.Error($"Could not add tag with id {tagId}. TagId is not valid.");
                return;
            }

            if (tags.Count == 0)
            {
                Logger.Error($"Could not add tag with id {tagId}. There are no tags in the TagId.");
                return;
            }

            var finalTagger = gameObject.GetComponent<FinalTagger>();
            if (!finalTagger) finalTagger = gameObject.AddComponent<FinalTagger>();

            var tagArray = data.GetTagArrayOfId(finalTagger.TagId) ?? new TagArray();
            
            foreach (var tag in tags)
            {
                var group = data.GetGroupOfTag(tag);
                if (group.IsSingleton) RemoveAllTagsOfGroupFromTagArray(data, tagArray, group);

                tagArray.Set(data.IndexOfTag(tag), true);
            }

            var newTagId = data.TagsToId(tagArray);

            finalTagger.TagId = newTagId;
        }

        
        #endregion

        #region Remove
        
        /// <summary>
        ///     Removes a tag from a GameObject
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag"></param>
        public static void RemoveTag(this GameObject gameObject, string tag)
        {
            var finalTagger = gameObject.GetComponent<FinalTagger>();
            if (!finalTagger) return;

            var data = Data;

            var tagArray = data.GetTagArrayOfId(finalTagger.TagId) ?? new TagArray();

            tagArray.Set(data.IndexOfTag(tag), false);

            var tagId = data.TagsToId(tagArray);

            finalTagger.TagId = tagId;
        }
        
        /// <summary>
        ///     Removes all specified tags from a GameObject
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tags"></param>
        public static void RemoveTags(this GameObject gameObject, params string[] tags)
        {
            var finalTagger = gameObject.GetComponent<FinalTagger>();
            if (!finalTagger) return;

            var data = Data;

            var tagArray = data.GetTagArrayOfId(finalTagger.TagId) ?? new TagArray();

            foreach (var tag in tags) tagArray.Set(data.IndexOfTag(tag), false);

            var tagId = data.TagsToId(tagArray);

            finalTagger.TagId = tagId;
        }
        
        /// <summary>
        ///     Removes all specified tags from a GameObject
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tags"></param>
        public static void RemoveTagId(this GameObject gameObject, int tagId)
        {
            var finalTagger = gameObject.GetComponent<FinalTagger>();
            if (!finalTagger) return;

            var data = Data;

            var tagArray = data.GetTagArrayOfId(finalTagger.TagId) ?? new TagArray();
            var removeArray = data.GetTagArrayOfId(tagId);

            tagArray.SetMask(removeArray);

            var newTagId = data.TagsToId(tagArray);

            finalTagger.TagId = newTagId;
        }
        
        #endregion

        #region Set
    

        /// <summary>
        ///     Replaces the tags from a GameObject by the specified tag
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag"></param>
        public static void SetTag(this GameObject gameObject, string tag)
        {
            var data = Data;
            data.AddTag(tag);

            var tags = new List<string> { tag };

            var tagId = data.TagsToId(tags.ToArray());

            if (!gameObject.TryGetComponent<FinalTagger>(out var tagger))
            {
                Logger.Error($"Unable to set tagId {tagId}. FinalTagger component not found on GameObject {StringUtilities.NameOf(gameObject)}");
                return;
            }
            
            tagger.TagId = tagId;
        }

        /// <summary>
        ///     Replaces the tags from a GameObject by the specified tags
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tags"></param>
        public static void SetTags(this GameObject gameObject, params string[] tags)
        {
            var data = Data;
            data.AddTags(tags);

            // Clear temp array to play with it.
            data.TempTagArray.SetAll(false);

            for (var i = 0; i < tags.Length; i++)
            {
                var tag = tags[i];
                var group = data.GetGroupOfTag(tag);
                if (group.IsSingleton) RemoveAllTagsOfGroupFromTagArray(data, data.TempTagArray, group);
                data.TempTagArray.Set(data.IndexOfTag(tag), true);
            }

            var tagId = data.TagsToId(data.TempTagArray);

            if (!gameObject.TryGetComponent<FinalTagger>(out var tagger))
            {
                Logger.Error($"Unable to set tags {string.Join(", ", tags)}. FinalTagger component not found on GameObject {StringUtilities.NameOf(gameObject)}");
                return;
            }
            
            tagger.TagId = tagId;
        }

        /// <summary>
        ///     Replaces the tags from a GameObject by the specified tags of the tagId
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tagId"></param>
        public static void SetTagId(this GameObject gameObject, int tagId)
        {
            if (!gameObject.TryGetComponent<FinalTagger>(out var tagger))
            {
                Logger.Error($"Unable to set tagId {tagId}. FinalTagger component not found on GameObject {StringUtilities.NameOf(gameObject)}");
                return;
            }
            
            tagger.TagId = tagId;
        }
        
        #endregion

        #region Get

        /// <summary>
        ///     Returns all tags on this GameObject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static HashSet<string> GetTags(this GameObject gameObject)
        {
            if (!gameObject)
            {
                Logger.Error("GameObject is null. Unable to get TagId.");
                return null;
            }

            if (!gameObject.TryGetComponent<FinalTagger>(out var finalTagger))
            {
                Logger.Error($"Unable to get tags. FinalTagger component not found on GameObject {StringUtilities.NameOf(gameObject)}");
                return null;
            }
            
            return Data.GetTagsOfId(finalTagger.TagId);
        }

        /// <summary>
        /// Returns the tagId of the GameObject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static int GetTagId(this GameObject gameObject)
        {
            if (!gameObject)
            {
                Logger.Error("GameObject is null. Unable to get TagId.");
                return -1;
            }

            if (!gameObject.TryGetComponent<FinalTagger>(out var finalTagger))
            {
                Logger.Error($"Unable to get TagId. FinalTagger component not found on GameObject {StringUtilities.NameOf(gameObject)}");
                return -1;
            }

            return finalTagger.TagId;
        }

        #endregion

        #region Has

        /// <summary>
        ///     Returns true if this tag is on this GameObject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasTag(this GameObject gameObject, string tag)
        {
            var hashSet = gameObject.GetTags();
            return hashSet != null && hashSet.Contains(tag);
        }

        /// <summary>
        ///     Returns true if all tags are on this GameObject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasTag(this GameObject gameObject, params string[] tag)
        {
            var hashSet = gameObject.GetTags();
            return hashSet != null && tag.All(s => hashSet.Contains(s));
        }
        
        /// <summary>
        /// Returns true if all tags of the tagId are on this GameObject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasTagId(this GameObject gameObject, int tagId)
        {
            var goTagId = gameObject.GetTagId();
            if (goTagId == -1) return tagId == -1;
            if (goTagId == tagId) return true;
            
            
            var data = Data;
            var goTagArray = data.GetTagArrayOfId(goTagId);
            var tagArray = data.GetTagArrayOfId(tagId);

            return goTagArray.Contains(tagArray);
        }
        
        /// <summary>
        /// Returns true if any of the tags are on this GameObject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAnyTag(this GameObject gameObject, params string[] tag)
        {
            var hashSet = gameObject.GetTags();
            return hashSet != null && tag.Any(s => hashSet.Contains(s));
        }

        /// <summary>
        /// Returns true if all tags of the tagId are on this GameObject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAnyTagId(this GameObject gameObject, int tagId)
        {
            var goTagId = gameObject.GetTagId();
            if (goTagId == -1) return tagId == -1;
            if (goTagId == tagId) return true;
            
            var data = Data;
            var goTagArray = data.GetTagArrayOfId(goTagId);
            var tagArray = data.GetTagArrayOfId(tagId);

            return goTagArray.Any(tagArray);
        }
        
        #endregion
        
        #endregion

        #region Search
        
        /// <summary>
        ///     Creates <see cref="TaggerAdvancedSearch" /> from pattern.
        ///     Available operators & | ! for And Or and Not Mode.
        /// </summary>
        /// <param name="pattern">
        ///     The Pattern must begin with a Tag.
        ///     Allowed pattern is:
        ///     TAG OPERATOR TAG
        ///     TAG,TAG,... OPERATOR TAG
        /// </param>
        /// <returns></returns>
        public static TaggerAdvancedSearch CreateAdvancedSearchFromPattern(string pattern)
        {
            var tag = string.Empty;
            var tags = new List<string>();
            var baseSearch = new TaggerAdvancedSearch();
            var currentSearch = baseSearch;
            foreach (var c in pattern)
            {
                var operatorAnd = c == '&';
                var operatorOr = c == '|';
                var operatorNot = c == '!';
                var operatorAny = operatorAnd || operatorOr || operatorNot;
                if (operatorAny)
                {
                    tags.Add(tag.Replace(" ", ""));
                    tag = string.Empty;
                    currentSearch.SetTags(tags.ToArray());
                    tags.Clear();
                    if (operatorAnd)
                        currentSearch = currentSearch.And();
                    else if (operatorOr)
                        currentSearch = currentSearch.Or();
                    else
                        currentSearch = currentSearch.Not();
                }
                else if (c == ',')
                {
                    tags.Add(tag.Replace(" ", ""));
                    tag = string.Empty;
                }
                else
                {
                    if (c == '(' || c == ')' || c == ' ') continue;

                    tag += c;
                }
            }

            tags.Add(tag.Replace(" ", ""));
            currentSearch.SetTags(tags.ToArray());
            return baseSearch;
        }

        /// <summary>
        ///     Returns a single (first) GameObject that match the tag with the search mode.
        /// </summary>
        /// <param name="mode">
        /// <br/>EXACT: Returns only GameObjects that have all given tags and no other tags.
        /// <br/>AND: Returns only GameObjects that have all given tags.
        /// <br/>OR: Returns all GameObjects that have any of the given tags.
        /// <br/>NOT: Returns all GameObjects that do not have any of the given tags.
        /// </param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static GameObject FindGameObjectWithTag(string tag, TaggerSearchMode mode = TaggerSearchMode.And)
        {
            var data = Data;
            if (mode == TaggerSearchMode.And)
            {
                var array = data.GetTagArrayOfTag(tag);
                var matchedId = data.GetIdOfTagArray(array);
                return TaggerManager.Instance.GetSingle(matchedId);
            }

            if (mode == TaggerSearchMode.Or)
            {
                var array = data.GetTagArrayOfTag(tag);
                var matchedIDs = data.GetMatchedIDs(array);
                foreach (var matchedId in matchedIDs)
                {
                    var gameObjects = TaggerManager.Instance.Get(matchedId);
                    if (gameObjects != null)
                        foreach (var gameObject in gameObjects)
                            return gameObject;
                }
            }
            else
            {
                var result = new TagArray();
                var array = data.GetTagArrayOfTag(tag);
                result.SetMask(array);
                var matchedIDs = data.GetMatchedIDs(result);
                foreach (var matchedId in matchedIDs)
                {
                    var gameObjects = TaggerManager.Instance.Get(matchedId);
                    if (gameObjects != null)
                        foreach (var gameObject in gameObjects)
                            return gameObject;
                }
            }

            return null;
        }
        
        /// <summary>
        ///     Returns a single (first) GameObject that match the tagId with the search mode.
        /// </summary>
        /// <param name="mode">
        /// <br/>EXACT: Returns only GameObjects that have all given tags and no other tags.
        /// <br/>AND: Returns only GameObjects that have all given tags.
        /// <br/>OR: Returns all GameObjects that have any of the given tags.
        /// <br/>NOT: Returns all GameObjects that do not have any of the given tags.
        /// </param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static GameObject FindGameObjectWithId(int tag, TaggerSearchMode mode = TaggerSearchMode.And)
        {
            var data = Data;
            if (mode == TaggerSearchMode.And)
            {
                var array = data.GetTagArrayOfId(tag);
                var matchedId = data.GetIdOfTagArray(array);
                return TaggerManager.Instance.GetSingle(matchedId);
            }

            if (mode == TaggerSearchMode.Or)
            {
                var array = data.GetTagArrayOfId(tag);
                var matchedIDs = data.GetMatchedIDs(array);
                foreach (var matchedId in matchedIDs)
                {
                    var gameObjects = TaggerManager.Instance.Get(matchedId);
                    if (gameObjects != null)
                        foreach (var gameObject in gameObjects)
                            return gameObject;
                }
            }
            else
            {
                var result = new TagArray();
                var array = data.GetTagArrayOfId(tag);
                result.SetMask(array);
                var matchedIDs = data.GetMatchedIDs(result);
                foreach (var matchedId in matchedIDs)
                {
                    var gameObjects = TaggerManager.Instance.Get(matchedId);
                    if (gameObjects != null)
                        foreach (var gameObject in gameObjects)
                            return gameObject;
                }
            }

            return null;
        }
        
        /// <summary>
        ///     Returns GameObjects that match the tag with the search mode.
        /// </summary>
        /// <param name="mode">
        /// <br/>EXACT: Returns only GameObjects that have all given tags and no other tags.
        /// <br/>AND: Returns only GameObjects that have all given tags.
        /// <br/>OR: Returns all GameObjects that have any of the given tags.
        /// <br/>NOT: Returns all GameObjects that do not have any of the given tags.
        /// </param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static HashSet<GameObject> FindGameObjectsWithTag(string tag, TaggerSearchMode mode = TaggerSearchMode.And)
        {
            var data = Data;

            if (mode == TaggerSearchMode.Exact)
            {
                var array = data.GetTagArrayOfTag(tag);
                var matchedId = data.GetIdOfTagArray(array);
                return TaggerManager.Instance.GetAsHashSet(matchedId);
            }

            var resultGameObjects = new HashSet<GameObject>();
            if (mode is TaggerSearchMode.Or or TaggerSearchMode.And)
            {
                var array = data.GetTagArrayOfTag(tag);
                var matchedIDs = data.GetMatchedIDs(array);
                foreach (var matchedId in matchedIDs)
                {
                    var gameObjects = TaggerManager.Instance.Get(matchedId);
                    if (gameObjects != null)
                        foreach (var gameObject in gameObjects)
                            resultGameObjects.Add(gameObject);
                }
            }
            else
            {
                var result = new TagArray();
                var array = data.GetTagArrayOfTag(tag);
                result.SetMask(array);
                var matchedIDs = data.GetMatchedIDs(result);
                foreach (var matchedId in matchedIDs)
                {
                    var gameObjects = TaggerManager.Instance.Get(matchedId);
                    if (gameObjects != null)
                        foreach (var gameObject in gameObjects)
                            resultGameObjects.Add(gameObject);
                }
            }

            return resultGameObjects;
        }
        
        /// <summary>
        ///     Returns GameObjects that match the tagId with the search mode.
        /// </summary>
        /// <param name="mode">
        /// <br/>EXACT: Returns only GameObjects that have all given tags and no other tags.
        /// <br/>AND: Returns only GameObjects that have all given tags.
        /// <br/>OR: Returns all GameObjects that have any of the given tags.
        /// <br/>NOT: Returns all GameObjects that do not have any of the given tags.
        /// </param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static HashSet<GameObject> FindGameObjectsWithId(int tag, TaggerSearchMode mode = TaggerSearchMode.And)
        {
            var data = Data;
            if (mode == TaggerSearchMode.Exact)
            {
                var array = data.GetTagArrayOfId(tag);
                var matchedId = data.GetIdOfTagArray(array);
                return TaggerManager.Instance.GetAsHashSet(matchedId);
            }

            var resultGameObjects = new HashSet<GameObject>();

            if (mode == TaggerSearchMode.And)
            {
                var array = data.GetTagArrayOfId(tag);
                var matchedIDs = data.GetMatchedIDs(array);
                foreach (var matchedId in matchedIDs)
                {
                    var gameObjects = TaggerManager.Instance.Get(matchedId);
                    if (gameObjects != null)
                        foreach (var gameObject in gameObjects)
                            resultGameObjects.Add(gameObject);
                }
            }
            else if (mode == TaggerSearchMode.Or)
            {
                var result = new TagArray();
                var array = data.GetTagArrayOfId(tag);
                for (var i = 0; i < array.Count; i++)
                {
                    var bit = array[i];
                    if (bit)
                    {
                        result.Set(i, true);
                        var matchedIDs = data.GetMatchedIDs(result);
                        foreach (var matchedId in matchedIDs)
                        {
                            var gameObjects = TaggerManager.Instance.Get(matchedId);
                            if (gameObjects != null)
                                foreach (var gameObject in gameObjects)
                                    resultGameObjects.Add(gameObject);
                        }
                        result.Set(i, false);
                    }
                }
            }
            else
            {
                var result = new TagArray();
                var array = data.GetTagArrayOfId(tag);
                result.SetMask(array);
                var matchedIDs = data.GetMatchedIDs(result);
                foreach (var matchedId in matchedIDs)
                {
                    var gameObjects = TaggerManager.Instance.Get(matchedId);
                    if (gameObjects != null)
                        foreach (var gameObject in gameObjects)
                            resultGameObjects.Add(gameObject);
                }
            }

            return resultGameObjects;
        }

        /// <summary>
        ///     Returns the matching GameObjects to the given tag collection.
        /// </summary>
        /// <param name="mode">
        /// <br/>EXACT: Returns only GameObjects that have all given tags and no other tags.
        /// <br/>AND: Returns only GameObjects that have all given tags.
        /// <br/>OR: Returns all GameObjects that have any of the given tags.
        /// <br/>NOT: Returns all GameObjects that do not have any of the given tags.
        /// </param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static GameObject FindGameObjectWithTags(string[] tags, TaggerSearchMode mode)
        {
            var data = Data;
            if (mode == TaggerSearchMode.Or)
            {
                foreach (var s in tags)
                {
                    var array = data.GetTagArrayOfTags(new[] { s });
                    var matchedIDs = data.GetMatchedIDs(array);
                    foreach (var matchedId in matchedIDs)
                    {
                        var gameObjects = TaggerManager.Instance.Get(matchedId);
                        if (gameObjects != null)
                            foreach (var gameObject in gameObjects)
                                return gameObject;
                    }
                }
            }
            else
            {
                var result = new TagArray();
                var array = data.GetTagArrayOfTags(tags);
                if (mode == TaggerSearchMode.And)
                    result.Or(array);
                else
                    result.SetMask(array);

                var matchedIDs = data.GetMatchedIDs(result);
                foreach (var matchedId in matchedIDs)
                {
                    var gameObjects = TaggerManager.Instance.Get(matchedId);
                    if (gameObjects != null)
                        foreach (var gameObject in gameObjects)
                            return gameObject;
                }
            }

            return null;
        }

        /// <summary>
        ///     Returns the matching GameObjects to the given tag collection.
        /// </summary>
        /// <param name="mode">
        /// <br/>EXACT: Returns only GameObjects that have all given tags and no other tags.
        /// <br/>AND: Returns only GameObjects that have all given tags.
        /// <br/>OR: Returns all GameObjects that have any of the given tags.
        /// <br/>NOT: Returns all GameObjects that do not have any of the given tags.
        /// </param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static GameObject FindGameObjectWithTags(TaggerSearchMode mode, params string[] tag) => FindGameObjectWithTags(tag, mode);


        /// <summary>
        ///     Returns the matching GameObjects to the given tag collection.
        /// </summary>
        /// <param name="mode">
        /// <br/>EXACT: Returns only GameObjects that have all given tags and no other tags.
        /// <br/>AND: Returns only GameObjects that have all given tags.
        /// <br/>OR: Returns all GameObjects that have any of the given tags.
        /// <br/>NOT: Returns all GameObjects that do not have any of the given tags.
        /// </param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static HashSet<GameObject> FindGameObjectsWithTags(string[] tag, TaggerSearchMode mode)
        {
            var resultGameObjects = new HashSet<GameObject>();
            var data = Data;

            switch (mode)
            {
                case TaggerSearchMode.Exact:
                {
                    var array = data.GetTagArrayOfTags(tag);
                    var matchedId = data.GetIdOfTagArray(array);
                    var gameObjects = TaggerManager.Instance.Get(matchedId);
                    if (gameObjects != null)
                        foreach (var gameObject in gameObjects)
                            resultGameObjects.Add(gameObject);
                    return resultGameObjects;
                }
                case TaggerSearchMode.Or:
                {
                    foreach (var s in tag)
                    {
                        var array = data.GetTagArrayOfTags(new[] { s });
                        var matchedIDs = data.GetMatchedIDs(array);
                        foreach (var matchedId in matchedIDs)
                        {
                            var gameObjects = TaggerManager.Instance.Get(matchedId);
                            if (gameObjects != null)
                                foreach (var gameObject in gameObjects)
                                    resultGameObjects.Add(gameObject);
                        }
                    }

                    break;
                }
                default:
                {
                    var result = new TagArray();
                    var array = data.GetTagArrayOfTags(tag);
                    if (mode == TaggerSearchMode.And)
                        result.Or(array);
                    else
                        result.SetMask(array);

                    var matchedIDs = data.GetMatchedIDs(result);
                    foreach (var matchedId in matchedIDs)
                    {
                        var gameObjects = TaggerManager.Instance.Get(matchedId);
                        if (gameObjects != null)
                            foreach (var gameObject in gameObjects)
                                resultGameObjects.Add(gameObject);
                    }

                    break;
                }
            }

            return resultGameObjects;
        }
        
        /// <summary>
        ///     Returns a single (first) matching GameObject to the given tagId. The tags must match exactly.
        /// </summary>
        /// <param name="mode">
        /// <br/>EXACT: Returns only GameObjects that have all given tags and no other tags.
        /// <br/>AND: Returns only GameObjects that have all given tags.
        /// <br/>OR: Returns all GameObjects that have any of the given tags.
        /// <br/>NOT: Returns all GameObjects that do not have any of the given tags.
        /// </param>
        /// <param name="tagId">The TaggerId used by the Attribute or FinalTagger Component</param>
        /// <returns></returns>
        public static GameObject FindGameObjectWithExactId(int tagId) => TaggerManager.Instance.GetSingle(tagId);

        /// <summary>
        ///     Returns the matching GameObjects to the given tagId. The tags must match exactly.
        /// </summary>
        /// <param name="mode">
        /// <br/>EXACT: Returns only GameObjects that have all given tags and no other tags.
        /// <br/>AND: Returns only GameObjects that have all given tags.
        /// <br/>OR: Returns all GameObjects that have any of the given tags.
        /// <br/>NOT: Returns all GameObjects that do not have any of the given tags.
        /// </param>
        /// <param name="tagId">The TaggerId used by the Attribute or FinalTagger Component</param>
        /// <returns></returns>
        public static GameObject[] FindGameObjectsWithExactId(int tagId) => TaggerManager.Instance.Get(tagId);

        /// <summary>
        ///     Returns the matching GameObjects to the given tag collection.
        /// </summary>
        /// <param name="mode">
        /// <br/>EXACT: Returns only GameObjects that have all given tags and no other tags.
        /// <br/>AND: Returns only GameObjects that have all given tags.
        /// <br/>OR: Returns all GameObjects that have any of the given tags.
        /// <br/>NOT: Returns all GameObjects that do not have any of the given tags.
        /// </param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static HashSet<GameObject> FindGameObjectsWithTags(TaggerSearchMode mode, params string[] tag) => FindGameObjectsWithTags(tag, mode);


        /// <summary>
        ///     Returns the matching GameObjects to the given group.
        /// </summary>
        /// <param name="mode">
        /// <br/>EXACT: Returns only GameObjects that have all given tags and no other tags.
        /// <br/>AND: Returns only GameObjects that have all given tags.
        /// <br/>OR: Returns all GameObjects that have any of the given tags.
        /// <br/>NOT: Returns all GameObjects that do not have any of the given tags.
        /// </param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static HashSet<GameObject> FindGameObjectsWithGroup(string group)
        {
            return FindGameObjectsWithGroup(GetGroupOfTag(group));
        }

        /// <summary>
        ///     Returns the matching GameObjects to the given group.
        /// </summary>
        /// <param name="mode">
        /// <br/>EXACT: Returns only GameObjects that have all given tags and no other tags.
        /// <br/>AND: Returns only GameObjects that have all given tags.
        /// <br/>OR: Returns all GameObjects that have any of the given tags.
        /// <br/>NOT: Returns all GameObjects that do not have any of the given tags.
        /// </param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static HashSet<GameObject> FindGameObjectsWithGroup(TaggerGroup group)
        {
            if (group == null)
            {
                Logger.Error("Group not found or null.");
                return null;
            }

            return FindGameObjectsWithTags(TaggerSearchMode.Or, group.ToArray());
        }

        #endregion

        #region Conversion

        /// <summary>
        /// Returns all tags of the given tagId.
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static HashSet<string> GetTagsOfId(int tagId) => Data.GetTagsOfId(tagId);

        /// <summary>
        /// Returns the tagId of the given tags.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static int GetIdOfTags(params string[] tags) => Data.TagsToId(tags);
        
        #endregion
        
        /// <summary>
        /// Returns all known tags.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static string[] GetAllTags() => Data.GetAllTags();
        
        /// <summary>
        ///     Adds the tags of a group to a GameObject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="group"></param>
        public static void AddTagsOfGroup(this GameObject gameObject, string group)
        {
            gameObject.AddTagsOfGroup(Data.GetOrAddGroup(group));
        }

        /// <summary>
        ///     Adds the tags of a group to a GameObject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="group"></param>
        public static void AddTagsOfGroup(this GameObject gameObject, TaggerGroup group)
        {
            if (group.IsSingleton)
                throw new InvalidOperationException("You can not add all tags of a singleton group to a GameObject.");

            var data = Data;
            var finalTagger = gameObject.GetComponent<FinalTagger>();
            if (!finalTagger) finalTagger = gameObject.AddComponent<FinalTagger>();

            var tagArray = data.GetTagArrayOfId(finalTagger.TagId) ?? new TagArray();

            foreach (var tag in group) tagArray.Set(data.IndexOfTag(tag), true);

            var tagId = data.TagsToId(tagArray);

            finalTagger.TagId = tagId;
        }

        /// <summary>
        ///     Returns true if all tags of the given group are on this GameObject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static bool HasTagsOfGroup(this GameObject gameObject, string group)
        {
            var taggerGroup = Data.GetOrAddGroup(group, false);
            return gameObject.HasTagsOfGroup(taggerGroup);
        }

        /// <summary>
        ///     Returns true if all tags of the given group are on this GameObject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static bool HasTagsOfGroup(this GameObject gameObject, TaggerGroup group)
        {
            var hashSet = gameObject.GetTags();
            return hashSet != null && group.All(s => hashSet.Contains(s));
        }


        /// <summary>
        ///     Returns true if any of the tags of the given group are on this GameObject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static bool HasAnyTagsOfGroup(this GameObject gameObject, string group)
        {
            var taggerGroup = Data.GetOrAddGroup(group, false);
            return gameObject.HasAnyTagsOfGroup(taggerGroup);
        }

        /// <summary>
        ///     Returns true if any of the tags of the given group are on this GameObject.
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="group"></param>
        /// <returns></returns>
        public static bool HasAnyTagsOfGroup(this GameObject gameObject, TaggerGroup group)
        {
            var hashSet = gameObject.GetTags();
            return hashSet != null && group.Any(s => hashSet.Contains(s));
        }

        /// <summary>
        ///     Removes all tags from the given group from a GameObject
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="group"></param>
        public static void RemoveAllTagsOfGroup(this GameObject gameObject, string group)
        {
            var taggerGroup = Data.GetOrAddGroup(group, false);
            gameObject.RemoveAllTagsOfGroup(taggerGroup);
        }

        /// <summary>
        ///     Removes all tags from the given group from a GameObject
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="group"></param>
        public static void RemoveAllTagsOfGroup(this GameObject gameObject, TaggerGroup group)
        {
            var finalTagger = gameObject.GetComponent<FinalTagger>();
            if (!finalTagger) return;

            var data = Data;

            var tagArray = data.GetTagArrayOfId(finalTagger.TagId) ?? new TagArray();

            foreach (var tag in group) tagArray.Set(data.IndexOfTag(tag), false);

            var tagId = data.TagsToId(tagArray);

            finalTagger.TagId = tagId;
        }


        [MethodImpl(GlobalConst.ImplOptions)]
        private static void RemoveAllTagsOfGroupFromTagArray(TaggerData data, TagArray array, TaggerGroup group)
        {
            for (var i = 0; i < group.Count; i++) array.Set(data.IndexOfTag(group[i]), false);
        }

        /// <summary>
        ///     Replaces the tags from a GameObject with the tags of the given group
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="group"></param>
        public static void SetTagsOfGroup(this GameObject gameObject, string group)
        {
            gameObject.SetTagsOfGroup(Data.GetOrAddGroup(group));
        }

        /// <summary>
        ///     Replaces the tags from a GameObject with the tags of the given group
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="group"></param>
        public static void SetTagsOfGroup(this GameObject gameObject, TaggerGroup group)
        {
            if (group.IsSingleton)
                throw new InvalidOperationException("You can not add all tags of a singleton group to a GameObject.");

            gameObject.GetComponent<FinalTagger>().TagId = group.TagId;
        }

        /// <summary>
        ///     Get the group of the given tag.
        /// </summary>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static TaggerGroup GetGroupOfTag(string tag) => Data.GetGroupOfTag(tag);

        #region Obsolete

        [Obsolete("Use AddTags instead", true)]
        public static void AddTag(this GameObject gameObject, params string[] tags) => throw new NotSupportedException("Use AddTags instead");

        [Obsolete("Use SetTags instead", true)]
        public static void SetTag(this GameObject gameObject, params string[] tags) => throw new NotSupportedException("Use SetTags instead");

        [Obsolete("Use RemoveTags instead")]
        public static void RemoveTag(this GameObject gameObject, params string[] tags) {RemoveTags(gameObject, tags);}
        
        /// <summary>
        ///     Returns GameObjects that have exactly this one given tag.
        ///     If a GameObject has more than one tag, it will not be returned.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        [Obsolete("Use FindGameObjectsWithTag with SearchMode Exact instead")]
        public static GameObject[] FindGameObjectsWithExactOneTag(string tag)
        {
            return FindGameObjectsWithTag(tag, TaggerSearchMode.Exact).ToArray();
        }

        /// <summary>
        ///     Returns GameObjects that have exactly this one given tag.
        ///     If a GameObject has more than one tag, it will not be returned.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        [Obsolete("Use FindGameObjectWithTag with SearchMode Exact instead")]
        public static GameObject FindGameObjectWithExactOneTag(string tag) => FindGameObjectWithTag(tag, TaggerSearchMode.Exact);

        #endregion
    }
}