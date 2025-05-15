// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   FinalTaggerPopupMenu.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Finalfactory.Tagger.Editor
{
    public class FinalTaggerPopupMenu : PopupWindowContent
    {
        private static readonly float SingleLine = EditorGUIUtility.singleLineHeight + 4;
        private static readonly float HalfLine = SingleLine / 2f;

        private readonly int _index;
        private readonly FinalTaggerEditorUI.TagStyle _style;

        private readonly string _tag;

        private bool _changeGroup;

        private Color _color;

        private float _lastPopupHeight;

        private string _newGroup;
        private string _newName;
        private bool _rename;

        public FinalTaggerPopupMenu(string tag, FinalTaggerEditorUI.TagStyle style)
        {
            _tag = tag;
            _style = style;
            _index = TaggerSystem.Data.IndexOfTag(tag);
            _color = TaggerSystem.Data.GetColor(_index);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(120, Math.Max(_lastPopupHeight, SingleLine));
        }

        public override void OnGUI(Rect rect)
        {
            var r = new Rect(rect);
            var startY = rect.yMin;
            r.yMin += 4f;
            r.yMax += 4f;
            r.xMin += 4f;
            r.height = SingleLine;
            r.xMax -= 5f;

            var isTag = _style == FinalTaggerEditorUI.TagStyle.Tag;
            if (isTag)
            {
                var col = EditorGUI.ColorField(r, _color);
                if (_color != col)
                {
                    _color = col;
                    TaggerSystem.Data.SetColor(_index, _color);
                }

                r.y += SingleLine;

                if (!_rename && !_changeGroup && GUI.Button(r, "Change Group")) _changeGroup = true;

                r.y += SingleLine;
            }
            else
            {
                var isGroupSingleton = TaggerSystem.Data.IsGroupSingleton(_tag);
                var singleTon = EditorGUI.ToggleLeft(r, "Is Singleton", isGroupSingleton);
                if (singleTon != isGroupSingleton) TaggerSystem.Data.SetGroupSingleton(_tag, singleTon);
                r.y += SingleLine;
            }

            if (!_changeGroup && !_rename && GUI.Button(r, "Rename"))
            {
                _rename = true;
                _newName = _tag;
            }

            if (_changeGroup)
            {
                var allGroups = TaggerSystem.Data.GetAllGroups();

                GUI.Label(r, "New Group Name");

                r.y += SingleLine;

                _newGroup = EditorGUI.TextField(r, string.Empty, _newGroup);

                r.y += SingleLine;

                var text = "Create New";
                if (allGroups.Any(x => x.GroupName == _newGroup)) text = "Move to";

                if (GUI.Button(r, text) && !string.IsNullOrEmpty(_newGroup))
                {
                    _changeGroup = false;
                    TaggerSystem.Data.MoveTag(_tag, _newGroup);
                    editorWindow.Close();
                }

                if (allGroups.Length > 1)
                {
                    r.y += HalfLine + SingleLine;
                    GUI.Label(r, "Move to...");

                    foreach (var group in allGroups)
                    {
                        r.y += SingleLine;
                        var style = new GUIStyle(EditorStyles.boldLabel);
                        style.normal.textColor = FinalTaggerEditorUI.GetReadableFontColor(Color.white) *
                                                 (group.IsSingleton ? Color.green : Color.white);
                        if (GUI.Button(r, group.GroupName, style))
                        {
                            _changeGroup = false;
                            TaggerSystem.Data.MoveTag(_tag, group);
                            editorWindow.Close();
                        }
                    }
                }
            }

            if (_rename)
            {
                GUI.Label(r, "Enter new Name");

                r.y += SingleLine;

                _newName = EditorGUI.TextField(r, string.Empty, _newName);

                r.y += SingleLine;

                var isValid = isTag
                    ? TaggerSystem.Data.ContainsTag(_newName)
                    : TaggerSystem.Data.ContainsGroup(_newName);

                isValid = isValid && FinalTaggerUtilities.IsValidName(_tag);

                if (!isValid)
                {
                    GUI.color = Color.green;
                }
                else
                {
                    GUI.color = Color.red;
                    GUI.enabled = false;
                }

                if (GUI.Button(r, "OK") || (Event.current.keyCode == KeyCode.Return && !string.IsNullOrEmpty(_newName)))
                {
                    _changeGroup = false;
                    if (isTag)
                        TaggerSystem.Data.RenameTag(_tag, _newName);
                    else
                        TaggerSystem.Data.RenameGroup(_tag, _newName);
                    editorWindow.Close();
                }

                GUI.color = Color.white;
                GUI.enabled = true;
            }

            r.y += HalfLine + SingleLine;
            GUI.color = Color.red;
            var deleteText = "Delete " + (isTag ? "Tag" : "Group");

            if (GUI.Button(r, deleteText))
            {
                if (isTag)
                {
                    if (EditorUtility.DisplayDialog("Delete Tag " + _tag,
                            "Are you sure you want to delete '" + _tag +
                            "' ? This can not be undone! It is automatically removed from all tagged objects.",
                            "Delete",
                            "Cancel"))
                    {
                        TaggerSystem.Data.DeleteTag(_tag);
                        FinalTaggerEditorUI.Repaint();
                    }
                }
                else
                {
                    var strings = TaggerSystem.Data.GetOrAddGroup(_tag, false)?.GetTags();
                    if (strings != null) FinalTaggerEditorUI.DrawDeleteTag(strings, _tag);
                }

                editorWindow.Close();
            }

            GUI.color = Color.white;
            r.y += 4f;
            _lastPopupHeight = r.yMax - startY;
        }
    }
}