// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   TaggerGroup.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Finalfactory.Tagger
{
    public class TaggerGroup : ICollection<string>
    {
        private readonly HashSet<string> _tagList;
        private int _tagId;
        private string[] _tags;

#if UNITY_EDITOR

        /// <summary>
        ///     Used for Editor GUI Only.
        /// </summary>
        public string EditorGUIAddText;

#endif

        public TaggerGroup(string groupName)
        {
            GroupName = groupName;
            _tagList = new HashSet<string>();
            _tags = Array.Empty<string>();
            _tagId = -1;
        }

        /// <summary>
        ///     Returns the Group name
        /// </summary>
        public string GroupName { get; private set; }

        /// <summary>
        ///     Returns the tag of the specific index.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string this[int index] => _tags[index];

        /// <summary>
        ///     Returns if this group is a singleton group.
        ///     A GameObject can only have one tag from a singleton group.
        ///     This rule will only on add/set tags enforced.
        /// </summary>
        public bool IsSingleton { get; set; }

        /// <summary>
        ///     Returns the TagId of this group.
        /// </summary>
        public int TagId
        {
            get
            {
                if (_tagId < -1) _tagId = TaggerSystem.Data.TagsToId(_tags);
                return _tagId;
            }
        }

        /// <summary>
        ///     Returns the number of tags in this group.
        /// </summary>
        public int Count => _tags.Length;

        public bool IsReadOnly => false;

        /// <summary>
        ///     Add the given tag to this Group and remove the tag from his current group.
        ///     If the tag does not exist, it will be created.
        /// </summary>
        /// <param name="tag"></param>
        public void Add(string tag)
        {
            if (!TaggerSystem.Data.AddTag(tag, this)) TaggerSystem.Data.MoveTag(tag, this);
        }

        /// <summary>
        ///     Remove all tags from this group and move it to "Ungrouped"
        ///     Group "Ungrouped" can not be cleared.
        /// </summary>
        /// <exception cref="T:System.InvalidOperationException">If this group is "Ungrouped"</exception>
        public void Clear()
        {
            if (GroupName == "Ungrouped")
                throw new InvalidOperationException("The group \"Ungrouped\" can not be cleared.");
            for (var i = _tags.Length - 1; i >= 0; i--) Remove(_tags[i]);
        }

        /// <summary>
        ///     Returns if the specific tag are in this group.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool Contains(string tag)
        {
            return _tagList.Contains(tag);
        }

        /// <summary>
        ///     Copies all the tags of the current array to the specified array starting at the specified destination array index.
        /// </summary>
        /// <param name="array"></param>
        /// <param name="arrayIndex"></param>
        public void CopyTo(string[] array, int arrayIndex)
        {
            _tags.CopyTo(array, arrayIndex);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _tagList.GetEnumerator();
        }

        /// <summary>
        ///     Remove the given tag from this group and move it to "Ungrouped"
        /// </summary>
        /// <param name="tag"></param>
        public bool Remove(string tag)
        {
            TaggerSystem.Data.MoveTag(tag, "Ungrouped");
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _tags.GetEnumerator();
        }

        /// <summary>
        ///     Returns an new array of all tags in this group.
        /// </summary>
        /// <returns></returns>
        public string[] GetTags()
        {
            return _tags.ToArray();
        }

        /// <summary>
        ///     Rename this TaggerGroup
        /// </summary>
        /// <param name="newName"></param>
        public void Rename(string newName)
        {
            TaggerSystem.Data.RenameGroup(this, newName);
        }

        /// <summary>
        ///     DO N_O_T USE THIS METHOD. IT COULD BREAK THE TAGGERSYSTEM.
        ///     If you need access, contact me. So I can figure out why
        ///     it makes sense to make this method public.
        /// </summary>
        internal void INTERNAL_AddTag(string tag)
        {
            _tagList.Add(tag);
            Array.Resize(ref _tags, _tags.Length + 1);
            _tags[^1] = tag;
            _tagId = -2;
        }

        /// <summary>
        ///     DO N_O_T USE THIS METHOD. IT COULD BREAK THE TAGGERSYSTEM.
        ///     If you need access, contact me. So I can figure out why
        ///     it makes sense to make this method public.
        /// </summary>
        internal void INTERNAL_RemoveTag(string tag)
        {
            _tagList.Remove(tag);
            _tags = _tagList.ToArray();
            _tagId = -2;
        }

        /// <summary>
        ///     DO N_O_T USE THIS METHOD. IT COULD BREAK THE TAGGERSYSTEM.
        ///     If you need access, contact me. So I can figure out why
        ///     it makes sense to make this method public.
        /// </summary>
        internal void INTERNAL_SetName(string name)
        {
            GroupName = name;
        }

        internal void INTERNAL_RenameTag(string tag, string newName)
        {
            _tagList.Remove(tag);
            _tagList.Add(newName);
            _tags = _tagList.ToArray();
            _tagId = -2;
        }
    }
}