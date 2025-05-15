// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   TaggerData.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

namespace Finalfactory.Tagger
{
    public class TaggerData : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField] private List<string> _allTags;

        [SerializeField] private List<SerializedGroup> _serializedGroups;

        [SerializeField] private List<int> _serializedIDs;

        [SerializeField] private List<int> _serializedLinkIdKeys;

        [SerializeField] private List<int> _serializedLinkIdValues;

        [SerializeField] private List<TagArray> _serializedTagArrays;

        [SerializeField] private List<Color> _tagColors;

        [SerializeField] private int _version;
        private readonly Dictionary<TagArray, int> _arrayToId = new(new TagArray.TagArrayEqualityComparer());

        private readonly Dictionary<string, int> _fastTagIndex = new();

        private readonly Dictionary<string, TaggerGroup> _fastTagToGroup = new();

        private readonly Dictionary<string, TaggerGroup> _groups = new();

        private readonly Dictionary<int, TagArray> _idToArray = new();

        private readonly Dictionary<int, int> _oldIDs = new();

        internal TagArray TempTagArray;

        /// <summary>
        ///     Returns the current Version if this Data.
        ///     The version increases with each change.
        /// </summary>
        public int Version => _version;

        private void OnEnable()
        {
            if (_allTags == null) _allTags = new List<string>();

            if (_tagColors == null)
                _tagColors = new List<Color>();
            else
                while (_tagColors.Count < _allTags.Count)
                    _tagColors.Add(Color.white);

            if (_groups.Count == 0) GetOrAddGroup("Ungrouped");

            if (_serializedIDs == null) _serializedIDs = new List<int>();

            if (_serializedLinkIdKeys == null) _serializedLinkIdKeys = new List<int>();

            if (_serializedLinkIdValues == null) _serializedLinkIdValues = new List<int>();

            if (_serializedTagArrays == null) _serializedTagArrays = new List<TagArray>();

            if (_serializedGroups == null) _serializedGroups = new List<SerializedGroup>();

            TempTagArray = new TagArray();
            TempTagArray.Persistent = true;
        }

        private void OnDestroy()
        {
            TaggerSystem.INTERNAL_Data = null;
        }

        /// <summary>
        ///     Adds a new tag to the specified group.
        ///     If the tag already exists, the tag will not be added and not moved to the group.
        /// </summary>
        /// <param name="tag">The tag</param>
        /// <param name="group">the group</param>
        /// <returns>Returns true if the tag was added. Returns false if the tag already exists.</returns>
        public bool AddTag(string tag, string group)
        {
            return !_fastTagIndex.ContainsKey(tag) && AddTag(tag, GetOrAddGroup(group));
        }


        /// <summary>
        ///     Adds a new tag to the specified group.
        ///     If the tag already exists, the tag will not be added and not moved to the group.
        /// </summary>
        /// <param name="tag">The tag</param>
        /// <param name="group">the group</param>
        /// <returns>Returns true if the tag was added. Returns false if the tag already exists.</returns>
        public bool AddTag(string tag, TaggerGroup group = null)
        {
            if (!_fastTagIndex.ContainsKey(tag))
            {
                group ??= GetOrAddGroup("Ungrouped");
                _allTags.Add(tag);
                _tagColors.Add(Color.white);
                group.INTERNAL_AddTag(tag);
                _fastTagToGroup.Add(tag, group);
                RebuildFastIndex();
                TagArray.INTERNAL_Add();
                TaggerSystem.DataLoader.RequestSave(this);
                _version++;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Adds new tags to the specified group.
        /// </summary>
        /// <param name="tags">The tag</param>
        /// <param name="group">the group</param>
        public void AddTags(string[] tags, string group = "Ungrouped")
        {
            var version = _version;
            foreach (var tag in tags)
                if (!_fastTagIndex.ContainsKey(tag))
                {
                    _allTags.Add(tag);
                    _tagColors.Add(Color.white);
                    var taggerGroup = GetOrAddGroup(group);
                    taggerGroup.INTERNAL_AddTag(tag);
                    _fastTagToGroup.Add(tag, taggerGroup);
                    RebuildFastIndex();
                    TagArray.INTERNAL_Add();
                    _version++;
                }

            if (_version != version) TaggerSystem.DataLoader.RequestSave(this);
        }


        /// <summary>
        ///     Returns true if this tag is already known
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool ContainsTag(string tag)
        {
            return _fastTagIndex.ContainsKey(tag);
        }


        /// <summary>
        ///     Removes one day.
        ///     The tag is automatically removed from all Gameobject and Prefabs.
        /// </summary>
        /// <param name="tag">The tag</param>
        public void DeleteTag(string tag)
        {
            if (_fastTagIndex.ContainsKey(tag))
            {
                var index = IndexOfTag(tag);
                _allTags.Remove(tag);
                _tagColors.RemoveAt(index);
                foreach (var keyValuePair in _groups) keyValuePair.Value.INTERNAL_RemoveTag(tag);

                _fastTagToGroup.Remove(tag);
                RebuildFastIndex();
                TagArray.INTERNAL_RemoveAt(index);

                var idTagArrays = _idToArray.ToArray();
                var comparer = new TagArray.TagArrayEqualityComparer();
                for (var i = 0; i < idTagArrays.Length; i++)
                for (var j = 0; j < idTagArrays.Length; j++)
                    if (i < idTagArrays.Length)
                    {
                        var idTagArrayPair = idTagArrays[i];
                        var otherIdTagArrayPair = idTagArrays[j];

                        if (i != j && comparer.Equals(idTagArrayPair.Value, otherIdTagArrayPair.Value))
                        {
                            otherIdTagArrayPair.Value.Persistent = false;
                            _idToArray.Remove(otherIdTagArrayPair.Key);
                            _arrayToId.Remove(otherIdTagArrayPair.Value);
                            _oldIDs.Add(otherIdTagArrayPair.Key, idTagArrayPair.Key);
                            i = -1;
                            idTagArrays = _idToArray.ToArray();
                            break;
                        }
                    }

                TaggerSystem.DataLoader.RequestSave(this);
                _version++;
            }
        }


        /// <summary>
        ///     Returns all known tags.
        /// </summary>
        /// <returns></returns>
        public string[] GetAllTags()
        {
            return _allTags.ToArray();
        }


        /// <summary>
        ///     Returns the identification number of the given <see cref="TagArray" />
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public int GetIdOfTagArray(TagArray array)
        {
            return _arrayToId.GetValueOrDefault(array, -1);
        }

        /// <summary>
        ///     Returns a list of identification number of <see cref="TagArray" />s that matched to the given
        ///     <see cref="TagArray" /> over the Function <see cref="TagArray.ContainsWithMask" />
        /// </summary>
        /// <param name="array"></param>
        /// <returns></returns>
        public List<int> GetMatchedIDs(TagArray array)
        {
            var matchedIDs = new List<int>();
            foreach (var keyValuePair in _arrayToId)
                if (keyValuePair.Key.ContainsWithMask(array))
                    matchedIDs.Add(keyValuePair.Value);

            return matchedIDs;
        }

        /// <summary>
        ///     Returns the <see cref="TaggerGroup" /> with the specified name.
        /// </summary>
        /// <param name="group">The group name</param>
        /// <param name="createGroup">If this is true, a non-existent group is created.</param>
        /// <returns></returns>
        public TaggerGroup GetOrAddGroup(string group, bool createGroup = true)
        {
            if (!_groups.TryGetValue(group, out var taggerGroup) && createGroup)
            {
                taggerGroup = new TaggerGroup(group);
                _groups.Add(group, taggerGroup);
                TaggerSystem.DataLoader.RequestSave(this);
                _version++;
            }

            return taggerGroup;
        }

        /// <summary>
        ///     Returns the <see cref="TagArray" /> list from an identification number.
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        public TagArray GetTagArrayOfId(int tagId)
        {
            if (_idToArray.TryGetValue(tagId, out var array)) return new TagArray(array);

            return null;
        }
        
        /// <summary>
        ///     Returns a tag list from an identification number.
        /// </summary>
        /// <param name="tagId"></param>
        /// <returns></returns>
        public HashSet<string> GetTagsOfId(int tagId)
        {
            if (_idToArray.TryGetValue(tagId, out var array))
            {
                var tags = new HashSet<string>();
                for (var i = 0; i < array.Count; i++)
                    if (array[i])
                        tags.Add(_allTags[i]);

                return tags;
            }

            return null;
        }

        /// <summary>
        ///     Returns true if the given tag exists.
        /// </summary>
        /// <param name="tag">The Tag</param>
        /// <returns></returns>
        public bool TagExists(string tag)
        {
            return IndexOfTag(tag) >= 0;
        }

        /// <summary>
        ///     Returns the index of the given tag.
        /// </summary>
        /// <param name="tag">The Tag</param>
        /// <returns>The index of the tag</returns>
        public int IndexOfTag(string tag)
        {
            if (!_fastTagIndex.TryGetValue(tag, out var index)) index = -1;

            return index;
        }

        /// <summary>
        ///     Move Tag to the <see cref="TaggerGroup" />
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="taggerGroup"></param>
        public void MoveTag(string tag, TaggerGroup taggerGroup)
        {
            if (_fastTagIndex.ContainsKey(tag))
            {
                foreach (var keyValuePair in _groups) keyValuePair.Value.INTERNAL_RemoveTag(tag);

                taggerGroup.INTERNAL_AddTag(tag);
                _fastTagToGroup[tag] = taggerGroup;
                TaggerSystem.DataLoader.RequestSave(this);
                _version++;
            }
        }

        /// <summary>
        ///     Move a tag to the specificated <see cref="TaggerGroup" />
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="group"></param>
        public void MoveTag(string tag, string group)
        {
            MoveTag(tag, GetOrAddGroup(group));
        }

        /// <summary>
        ///     Rename the <see cref="TaggerGroup" />.
        /// </summary>
        /// <param name="group">The <see cref="TaggerGroup" /></param>
        /// <param name="newName">New Name</param>
        public void RenameGroup(TaggerGroup group, string newName)
        {
            _groups.Remove(group.GroupName);
            group.INTERNAL_SetName(newName);
            _groups.Add(group.GroupName, group);
            TaggerSystem.DataLoader.RequestSave(this);
        }

        /// <summary>
        ///     Rename the TaggerGroup.
        /// </summary>
        /// <param name="group">The Group</param>
        /// <param name="newName">New Name</param>
        public void RenameGroup(string group, string newName)
        {
            RenameGroup(GetOrAddGroup(group, false), newName);
        }

        /// <summary>
        ///     Rename a tag.
        /// </summary>
        /// <param name="tag">The name of the tag</param>
        /// <param name="newName">The new name of the tag</param>
        public void RenameTag(string tag, string newName)
        {
            var index = _fastTagIndex[tag];
            _fastTagIndex.Remove(tag);
            _fastTagIndex.Add(newName, index);
            var group = _fastTagToGroup[tag];
            group.INTERNAL_RenameTag(tag, newName);
            _fastTagToGroup.Remove(tag);
            _fastTagToGroup.Add(newName, group);
            _allTags[_allTags.IndexOf(tag)] = newName;
            TaggerSystem.DataLoader.RequestSave(this);
        }

        /// <summary>
        ///     Get the Identification Number of the given tag collection.
        /// </summary>
        /// <param name="tags">The Tag Collection</param>
        /// <returns>The Identification Number</returns>
        public int TagsToId(string[] tags)
        {
            // Of corse there sould be Tags in the array...
            if (tags.Length == 0) return -1;

            // Clear temp array to play with it.
            TempTagArray.SetAll(false);

            // Set all bits for the tags for comparing
            for (var i = 0; i < tags.Length; i++)
            {
                var tag = tags[i];
                if (_fastTagIndex.TryGetValue(tag, out var value))
                    TempTagArray.Set(value, true);
                else
                    TaggerSystem.Logger.Error("Unknown Tag " + tag);
            }

            return TagsToId(TempTagArray);
        }

        /// <summary>
        ///     Get the Identification Number of the given <see cref="TagArray" />.
        /// </summary>
        /// <param name="tagArray">The <see cref="TagArray" /></param>
        /// <returns>The Identification Number</returns>
        public int TagsToId(TagArray tagArray)
        {
            // Trys to find the matched tag array.
            if (!_arrayToId.TryGetValue(tagArray, out var id))
            {
                // Create new id for the new, unknown tag collection
                id = NextId();
                var array = (TagArray)tagArray.Clone();
                array.Persistent = true;
                _arrayToId.Add(array, id);
                _idToArray.Add(id, array);
                TaggerSystem.DataLoader.RequestSave(this);
            }

            return id;
        }

        /// <summary>
        ///     Returns a <see cref="TagArray" /> created from tags.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public TagArray GetTagArrayOfTags(string[] tags)
        {
            var array = new TagArray();
            foreach (var tag in tags)
                if (_fastTagIndex.TryGetValue(tag, out var value))
                    array.Set(value, true);
                else
                    TaggerSystem.Logger.Error("Unknown Tag " + tag);

            return array;
        }

        /// <summary>
        ///     Returns a <see cref="TagArray" /> created from the tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public TagArray GetTagArrayOfTag(string tag)
        {
            var array = new TagArray();

            if (_fastTagIndex.ContainsKey(tag))
                array.Set(_fastTagIndex[tag], true);
            else
                TaggerSystem.Logger.Error("Unknown Tag " + tag);

            return array;
        }

        /// <summary>
        /// Returns true, if the tag id is valid.
        /// </summary>
        public bool IsTagIdValid(int tagId) => _idToArray.ContainsKey(tagId);

        /// <summary>
        ///     DO N_O_T USE THIS METHOD. IT COULD BREAK THE TAGGERSYSTEM.
        ///     If you need access, contact me. So I can figure out why
        ///     it makes sense to make this method public.
        /// </summary>
        internal void INTERNAL_CheckForOldID(ref int tagId)
        {
            if (_oldIDs.ContainsKey(tagId)) _oldIDs.TryGetValue(tagId, out tagId);
        }

        private int NextId()
        {
            var id = 0;
            while (_idToArray.ContainsKey(id) || _oldIDs.ContainsKey(id)) id++;

            return id;
        }

        private void RebuildFastIndex()
        {
            _fastTagIndex.Clear();
            for (var i = 0; i < _allTags.Count; i++)
            {
                var allTag = _allTags[i];
                _fastTagIndex.Add(allTag, i);
            }
        }

        #region Groups

        /// <summary>
        ///     Create the <see cref="TaggerGroup" /> with the specified name.
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>the <see cref="TaggerGroup" /></returns>
        public TaggerGroup AddGroup(string groupName)
        {
            return GetOrAddGroup(groupName);
        }

        /// <summary>
        ///     Returns the <see cref="TaggerGroup" /> with the specified name.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public TaggerGroup GetGroupOfTag(string tag)
        {
            _fastTagToGroup.TryGetValue(tag, out var group);
            return group;
        }

        /// <summary>
        ///     Returns true if this group already exists.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool ContainsGroup(string group)
        {
            return _groups.ContainsKey(group);
        }

        /// <summary>
        ///     Deletes a <see cref="TaggerGroup" />.
        ///     The <see cref="TaggerGroup" /> "Ungrouped" can not be deleted.
        /// </summary>
        /// <param name="group">The <see cref="TaggerGroup" /> name to delete</param>
        /// <param name="deleteTags">
        ///     If this is true, all tags will be deleted too. If this is false, all tags are moved to
        ///     "Ungrouped"
        /// </param>
        public void DeleteGroup(string group, bool deleteTags)
        {
            if (group == "Ungrouped") return;

            var taggerGroup = GetOrAddGroup(group, false);
            if (taggerGroup != null)
            {
                for (var i = taggerGroup.Count - 1; i >= 0; i--)
                {
                    var tag = taggerGroup[i];
                    if (deleteTags)
                        DeleteTag(tag);
                    else
                        MoveTag(tag, GetOrAddGroup("Ungrouped"));
                }

                _groups.Remove(group);
                TaggerSystem.DataLoader.RequestSave(this);
                _version++;
            }
        }

        /// <summary>
        ///     Returns all <see cref="TaggerGroup" />
        /// </summary>
        /// <returns></returns>
        public TaggerGroup[] GetAllGroups(bool orderAlphabetical = false)
        {
            if (orderAlphabetical) return _groups.Values.OrderBy(x => x.GroupName).ToArray();
            return _groups.Values.ToArray();
        }

        /// <summary>
        ///     Returns true if the group is a singleton group.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool IsGroupSingleton(string group)
        {
            return GetOrAddGroup(group).IsSingleton;
        }

        /// <summary>
        ///     Set the group as a singleton group.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="singleton"></param>
        public void SetGroupSingleton(string group, bool singleton)
        {
            GetOrAddGroup(group).IsSingleton = singleton;
            TaggerSystem.DataLoader.RequestSave(this);
        }

        #endregion

        #region Color

        /// <summary>
        ///     Returns the Color of the tag slot.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Color GetColor(int index)
        {
            if (index < 0 || index >= _tagColors.Count) return Color.white;

            return _tagColors[index];
        }

        /// <summary>
        ///     Returns the color of the specified tag.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public Color GetColor(string tag)
        {
            return GetColor(IndexOfTag(tag));
        }

        /// <summary>
        ///     Set the Color of the tag slot
        /// </summary>
        /// <param name="index"></param>
        /// <param name="color"></param>
        public void SetColor(int index, Color color)
        {
            _tagColors[index] = color;
            TaggerSystem.DataLoader.RequestSave(this);
        }

        /// <summary>
        ///     Set the Color of the specified tag
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="color"></param>
        public void SetColor(string tag, Color color)
        {
            SetColor(IndexOfTag(tag), color);
        }

        #endregion

        #region Serialization

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            TagArray.INTERNAL_Clear();
            TagArray.INTERNAL_SetSize(_allTags.Count);

            foreach (var tagArray in _idToArray) tagArray.Value.Dispose();
            _idToArray.Clear();
            _arrayToId.Clear();
            _groups.Clear();
            _oldIDs.Clear();
            _fastTagToGroup.Clear();
            if (_serializedIDs.Count != _serializedTagArrays.Count)
                throw new Exception(
                    $"There are {_serializedIDs.Count} keys and {_serializedTagArrays.Count} values after deserialization.");

            if (_serializedLinkIdKeys.Count != _serializedLinkIdValues.Count)
                throw new Exception(
                    $"There are {_serializedLinkIdKeys.Count} keys and {_serializedLinkIdValues.Count} values after deserialization.");


            for (var i = 0; i < _serializedIDs.Count; i++)
            {
                var array = _serializedTagArrays[i];
                array.SetPersistentState(true, true);
                _idToArray.Add(_serializedIDs[i], array);
                _arrayToId.Add(array, _serializedIDs[i]);
            }

            foreach (var serializedGroup in _serializedGroups)
            {
                var group = new TaggerGroup(serializedGroup.Name);
                group.IsSingleton = serializedGroup.IsSingleton;
                foreach (var serializedGroupTag in serializedGroup.Tags) group.INTERNAL_AddTag(serializedGroupTag);

                _groups.Add(serializedGroup.Name, group);
                foreach (var serializedGroupTag in group) _fastTagToGroup.Add(serializedGroupTag, group);
            }

            for (var i = 0; i < _serializedLinkIdKeys.Count; i++)
                _oldIDs.Add(_serializedLinkIdKeys[i], _serializedLinkIdValues[i]);

            RebuildFastIndex();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (_serializedIDs == null) OnEnable();

            _serializedIDs.Clear();
            _serializedTagArrays.Clear();
            _serializedGroups.Clear();
            _serializedLinkIdKeys.Clear();
            _serializedLinkIdValues.Clear();
            foreach (var pair in _idToArray)
            {
                _serializedIDs.Add(pair.Key);
                _serializedTagArrays.Add(pair.Value);
            }

            foreach (var pair in _groups) _serializedGroups.Add(new SerializedGroup(pair.Value));

            foreach (var keyValuePair in _oldIDs)
            {
                _serializedLinkIdKeys.Add(keyValuePair.Key);
                _serializedLinkIdValues.Add(keyValuePair.Value);
            }
        }

        [Serializable]
        [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Local")]
        private struct SerializedGroup
        {
            public SerializedGroup(TaggerGroup group)
            {
                _name = group.GroupName;
                _tags = group.GetTags();
                _isSingleton = group.IsSingleton;
            }

#pragma warning disable IDE0044
            [SerializeField] private string _name;
            [SerializeField] private string[] _tags;
            [SerializeField] private bool _isSingleton;
#pragma warning restore IDE0044

            public string Name => _name;

            public string[] Tags => _tags;

            public bool IsSingleton => _isSingleton;
        }

        #endregion
    }
}