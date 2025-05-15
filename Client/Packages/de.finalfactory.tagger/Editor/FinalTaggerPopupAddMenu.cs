// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   FinalTaggerPopupAddMenu.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Finalfactory.Tagger.Editor
{
    public class FinalTaggerPopupAddMenu : PopupWindowContent
    {
        private readonly TagArray _array;
        private readonly float _currentViewWidth;
        private readonly SerializedProperty _excludeProperty;

        private readonly GameObject _go;
        private readonly SerializedProperty _serializedProperty;

        private Vector2 _scrollView;

        public FinalTaggerPopupAddMenu(GameObject go, float currentViewWidth)
        {
            _go = go;
            _currentViewWidth = currentViewWidth;
        }

        public FinalTaggerPopupAddMenu(SerializedProperty serializedProperty, TagArray array, float currentViewWidth,
            SerializedProperty excludeProperty)
        {
            _serializedProperty = serializedProperty;
            _array = array;
            _currentViewWidth = currentViewWidth;
            _excludeProperty = excludeProperty;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(_currentViewWidth, 500);
        }

        public override void OnGUI(Rect rect)
        {
            _scrollView = EditorGUILayout.BeginScrollView(_scrollView);
            HashSet<string> excludeList;
            if (_go != null)
            {
                excludeList = _go.GetTags();
            }
            else
            {
                excludeList = TaggerSystem.Data.GetTagsOfId(TaggerSystem.Data.TagsToId(_array));
                if (_excludeProperty != null)
                {
                    var array = TaggerSystem.Data.GetTagArrayOfId(_excludeProperty.intValue);
                    if (array != null)
                    {
                        var exclude = TaggerSystem.Data.GetTagsOfId(TaggerSystem.Data.TagsToId(array));
                        foreach (var s in exclude) excludeList.Add(s);
                    }
                }
            }

            FinalTaggerEditorUI.DrawGroups(
                TaggerSystem.Data.GetAllGroups(FinalTaggerPreferences.AlphabeticalOrder &&
                                               FinalTaggerPreferences.AlphabeticalOrderOfGroups),
                "All tags that have not already been added",
                s =>
                {
                    if (_go != null)
                    {
                        _go.AddTag(s);
                    }
                    else
                    {
                        _array.Set(TaggerSystem.Data.IndexOfTag(s), true);
                        _serializedProperty.intValue = TaggerSystem.Data.TagsToId(_array);
                        _serializedProperty.serializedObject.ApplyModifiedProperties();
                    }
                }, null, true, false, false, excludeList);
            EditorGUILayout.EndScrollView();
        }
    }
}