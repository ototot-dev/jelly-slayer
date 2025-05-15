// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   FinalTaggerEditorUI.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using System;
using System.Collections.Generic;
using System.Linq;
using FinalFactory.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Finalfactory.Tagger.Editor
{
    public static class FinalTaggerEditorUI
    {
        public enum TagStyle
        {
            Tag,

            Group
        }

        private const int XOffsetTagButton = -2;
        private const int YOffsetTagButton = -1;
        private const int YOffsetTagLabel = -3;
        private const int YOffsetTagTextField = -2;
        private const int WidthTagButton = 20;
        private const int LineHeightTagLabel = 20;
        private const float PaddingTagXMin = 8;
        private const float PaddingTagXMax = 8;
        private const float PaddingTagYMin = 2;
        private const float PaddingTagYMax = 2;
        private const float AddButtonWidth = 14f;
        private const float AddButtonXOffset = RemoveButtonWidth / 2f;
        private const float RemoveButtonWidth = 14f;
        private const float RemoveButtonOffsetX = RemoveButtonWidth / 2f;

        /// <summary>
        ///     The tag add button style.
        /// </summary>
        private static GUIStyle _addstyle;

        /// <summary>
        ///     The background style.
        /// </summary>
        private static GUIStyle _backgroundStyle;

        /// <summary>
        ///     The blend background style.
        /// </summary>
        private static GUIStyle _blendBackgroundStyle;

        /// <summary>
        ///     The header style.
        /// </summary>
        private static GUIStyle _headerStyle;

        /// <summary>
        ///     The tag style.
        /// </summary>
        private static GUIStyle _tagStyle;

        /// <summary>
        ///     The tag text style.
        /// </summary>
        private static GUIStyle _textStyleTag;

        /// <summary>
        ///     The group text style
        /// </summary>
        private static GUIStyle _textStyleGroup;

        private static GUILayoutOption _guiLayoutOption;

        private static float _height;

        private static bool _initialized;

        internal static UnityEditor.Editor AnyEditorWindowReference;

        public static void DrawGroups(TaggerGroup[] groups, string header = null, Action<string> onItemClick = null,
            Action<string> onDeleteClick = null,
            bool allowToAdd = false, bool allowToDelete = false, bool allowToDeleteGroup = false,
            HashSet<string> excludeList = null)
        {
            InitStyle();
            DrawHeader(header, GetNewRect(), Color.white);

            EditorGUILayout.Space();

            var xMax = EditorGUIUtility.currentViewWidth - 12;
            EditorGUILayout.BeginHorizontal();
            const string newGroup = "New Group: ";
            GUILayout.Label(newGroup);
            var addGroupRect = NewLine(newGroup);
            var r = addGroupRect;
            addGroupRect.xMin += xMax - 150f;
            addGroupRect.xMax = xMax;
            DrawAddTag(addGroupRect, TagStyle.Group, s => TaggerSystem.Data.AddGroup(s),
                ref TaggerDataEditor.NewGroupTxt);
            EditorGUILayout.EndHorizontal();


            r.y += 30f;

            var height = 0f;
            foreach (var taggerGroup in groups)
            {
                EditorGUILayout.Space();
                var groupName = taggerGroup.GroupName;
                var allowToDeleteThisGroup = groupName != "Ungrouped" && allowToDeleteGroup;
                var alphabeticalOrder = FinalTaggerPreferences.AlphabeticalOrder &&
                                        FinalTaggerPreferences.AlphabeticalOrderOfTagsInGroups;
                var extraHeight = 0f;
                if (allowToAdd)
                {
                    taggerGroup.EditorGUIAddText ??= string.Empty;
                    extraHeight = DrawTags(r, taggerGroup.GetTags(), ref taggerGroup.EditorGUIAddText,
                        groupName, onItemClick, onDeleteClick, allowToDelete, allowToDeleteThisGroup,
                        excludeList, false, alphabeticalOrder, true);
                }
                else
                {
                    string addText = null;
                    extraHeight = DrawTags(r, taggerGroup.GetTags(), ref addText, groupName,
                        onItemClick, onDeleteClick, allowToDelete, allowToDeleteThisGroup,
                        excludeList, false, alphabeticalOrder, true);
                }

                extraHeight += LineHeightTagLabel;
                height += extraHeight;
                r.y += extraHeight;
            }

            EditorGUILayout.GetControlRect(false, height);
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static float DrawTags(Rect rect, string[] list, ref string addText, string group = null,
            Action<string> onItemClick = null, Action<string> onDeleteClick = null,
            bool allowToDelete = false, bool allowToDeleteGroup = false, HashSet<string> excludeList = null,
            bool displayGroupInTagName = false, bool alphabeticalOrder = false,
            bool alphabeticalOrderIgnoreGroups = false)
        {
            InitStyle();

            var hasGroup = !string.IsNullOrEmpty(group);

            string[] tagsArray;
            if (alphabeticalOrder)
            {
                if (alphabeticalOrderIgnoreGroups)
                    tagsArray = list.OrderByDescending(s => s).ToArray();
                else
                    tagsArray = list.OrderByDescending(delegate(string tag)
                    {
                        var groupOfTag = TaggerSystem.Data.GetGroupOfTag(tag);
                        return groupOfTag.GroupName + "." + tag;
                    }).ToArray();
            }
            else
            {
                tagsArray = list.ToArray();
            }

            var guicolor = GUI.color;
            float height = 0;
            var xMax = EditorGUIUtility.currentViewWidth - 12;
            var backgroundRect = rect;
            if (hasGroup)
            {
                var rHeaderLine = rect;
                rHeaderLine.y -= 6;
                rHeaderLine.height = 2;

                GUI.Box(rHeaderLine, GUIContent.none, _blendBackgroundStyle);

                var rHeader = rect;

                DrawGroupHeader(rHeader, group, null, () => { DrawDeleteTag(list, group); }, group, guicolor,
                    allowToDeleteGroup, ref addText);


                backgroundRect.x = rect.x;
                backgroundRect.y += _height + 2f;
                height += _height + 2f;
            }
            else
            {
                backgroundRect.xMin += AddButtonWidth + AddButtonXOffset;
                backgroundRect.height = _height;
            }

            if (tagsArray.Length > 0) height += LineHeightTagLabel;

            var newLineX = backgroundRect.xMin;
            for (var i = tagsArray.Length - 1; i >= 0; i--)
            {
                var item = tagsArray[i];
                if (excludeList != null && excludeList.Contains(item)) continue;

                if (DrawTag(onItemClick, () => { onDeleteClick?.Invoke(item); }, item, newLineX, ref backgroundRect,
                        xMax, guicolor, allowToDelete, displayGroupInTagName))
                    height += _height;
            }

            return height;
        }

        public static void DrawDeleteTag(string[] list, string group)
        {
            var choice = 0;
            if (list.Length > 0)
                choice = EditorUtility.DisplayDialogComplex(
                    "Remove Group " + group,
                    "Move tags inside the group to Ungrouped or delete it?",
                    "Move",
                    "Delete",
                    "Cancel");

            if (choice < 2) TaggerSystem.Data.DeleteGroup(group, choice == 1);
        }

        private static SerializedProperty FindNeighborProperty(SerializedProperty property, string neighborName = null)
        {
            var path = property.propertyPath;
            var index = path.LastIndexOf(".", StringComparison.InvariantCulture);
            if (index >= 0)
            {
                path = path.Substring(0, index + 1);
                path += neighborName;
            }
            else
            {
                path = neighborName;
            }

            return property.serializedObject.FindProperty(path);
        }

        public static float DrawUI(SerializedProperty property, Rect rect2, string excludePropertyName = null)
        {
            var array = TaggerSystem.Data.GetTagArrayOfId(property.intValue) ?? new TagArray();

            InitStyle();

            var labelWidth = rect2.width - 20;
            var rect = rect2;
            rect.width = AddButtonWidth;
            rect.height = 17;

            if (GUI.Button(rect, GUIContent.none, "OL Plus"))
            {
                var menu = new FinalTaggerPopupAddMenu(property, array, EditorGUIUtility.currentViewWidth - 2,
                    FindNeighborProperty(property, excludePropertyName));
                var popupRect = GUILayoutUtility.GetLastRect();
                PopupWindow.Show(popupRect, menu);
            }

            rect2.yMax += rect.height;

            var tags = array.HasAny()
                ? TaggerSystem.Data.GetTagsOfId(TaggerSystem.Data.TagsToId(array))
                : new HashSet<string>();
            float height = 17;
            if (tags != null && tags.Count != 0)
            {
                string addText = null;
                height += DrawTags(rect2, tags.ToArray(), ref addText, null, null,
                    s => { array.Set(TaggerSystem.Data.IndexOfTag(s), false); }, true, false, null, true,
                    FinalTaggerPreferences.AlphabeticalOrder &&
                    FinalTaggerPreferences.AlphabeticalOrderOfTagsOnGameObjects,
                    FinalTaggerPreferences.ShowGroupInTagsLabel == FinalTaggerGroupShowStyle.None ||
                    FinalTaggerPreferences.AlphabeticalOrderOfTagsOnGameObjectsIgnoreGroups);
            }
            else
            {
                rect.xMin += 30;
                rect.width = labelWidth;
                rect.yMin += 0;
                rect.yMax += 0;
                GUI.Label(rect, "No tags are assigned.");
            }

            if (GUI.changed)
            {
                if (array.HasAny())
                    property.intValue = TaggerSystem.Data.TagsToId(array);
                else
                    property.intValue = -1;
                property.serializedObject.ApplyModifiedProperties();
            }

            return height;
        }

        public static void DrawInspectorUI(GameObject gO, Rect r = default)
        {
            InitStyle();

            if (r == default) r = EditorGUILayout.GetControlRect(false, 16f);

            var rButton = new Rect(r);
            var labelWidth = r.width - 20;
            rButton.width = AddButtonWidth;
            rButton.height = 17;
            if (GUI.Button(rButton, GUIContent.none, "OL Plus"))
            {
                var menu = new FinalTaggerPopupAddMenu(gO, EditorGUIUtility.currentViewWidth - 2);
                var popupRect = GUILayoutUtility.GetLastRect();
                PopupWindow.Show(popupRect, menu);
            }

            var taggerGroupShowStyle = FinalTaggerPreferences.ShowGroupInTagsLabel;
            var showGroupInTagsLabel = taggerGroupShowStyle == FinalTaggerGroupShowStyle.AsPrefix;

            var rTags = new Rect(r);
            if (taggerGroupShowStyle == FinalTaggerGroupShowStyle.GroupTags)
                rTags.y += 30;
            else
                rTags.x += rButton.width + 5f;

            var tags = gO.GetTags();
            r.y = 2;
            if (tags != null && tags.Count != 0)
            {
                string addText = null;
                var alphabeticalOrderIgnoreGroups = !showGroupInTagsLabel ||
                                                    FinalTaggerPreferences
                                                        .AlphabeticalOrderOfTagsOnGameObjectsIgnoreGroups;
                var alphabeticalOrder = FinalTaggerPreferences.AlphabeticalOrder &&
                                        FinalTaggerPreferences.AlphabeticalOrderOfTagsOnGameObjects;
                if (taggerGroupShowStyle == FinalTaggerGroupShowStyle.GroupTags)
                {
                    var height = 0f;
                    var groups = tags.Select(tag => TaggerSystem.Data.GetGroupOfTag(tag)).Distinct().ToArray();
                    foreach (var group in groups)
                    {
                        var extraHeight = DrawTags(rTags,
                            tags.Where(x => TaggerSystem.GetGroupOfTag(x) == group).ToArray(), ref addText,
                            group.GroupName, null, gO.RemoveTag, true, false, null, false,
                            alphabeticalOrder, alphabeticalOrderIgnoreGroups);
                        extraHeight += 10f;
                        height += extraHeight;
                        rTags.y += extraHeight;
                    }

                    EditorGUILayout.GetControlRect(false, height);
                }
                else
                {
                    var height = DrawTags(rTags, tags.ToArray(), ref addText, null, null, gO.RemoveTag, true, false,
                        null, true,
                        alphabeticalOrder, alphabeticalOrderIgnoreGroups);
                    //height += 10f;
                    EditorGUILayout.GetControlRect(false, height);
                }
            }
            else
            {
                rButton.xMin += 30;
                rButton.width = labelWidth;
                rButton.yMin += 0;
                rButton.yMax += 0;
                GUI.Label(rButton, "No tags are assigned to the gameobject.");
            }

            if (GUI.changed) EditorUtility.SetDirty(gO);
        }

        public static void Repaint()
        {
            AnyEditorWindowReference.Repaint();
        }

        private static void DrawHeader(string header, Rect bgRect, Color guicolor)
        {
            var gc = new GUIContent(header);
            var w = _headerStyle.CalcSize(gc).x + 4;
            bgRect.width = w;
            GUI.color = new Color(0, 0, 0, 0);
            GUI.Box(bgRect, GUIContent.none, _backgroundStyle);

            GUI.color = guicolor;
            var rect = new Rect(bgRect);
            rect.position += new Vector2(1, 1);
            GUI.Label(rect, gc, _headerStyle);
        }


        private static void DrawAddTag(Rect r, TagStyle style, Action<string> onAdd, ref string addText)
        {
            DrawTagBackground(ref r);

            var width = r.width;
            var rButton = new Rect(r);
            rButton.x += width - RemoveButtonOffsetX;
            rButton.y += -3f;
            rButton.width = RemoveButtonWidth;

            var rField = new Rect(r);
            rField.x += 1f;
            rField.y += YOffsetTagTextField;
            rField.xMax -= rButton.width;

            var isInvalid = style == TagStyle.Tag
                ? TaggerSystem.Data.ContainsTag(addText)
                : TaggerSystem.Data.ContainsGroup(addText);
            isInvalid = isInvalid || !FinalTaggerUtilities.IsValidName(addText);
            if (isInvalid && !string.IsNullOrEmpty(addText)) GUI.color = Color.red;

            addText = GUI.TextField(rField, addText);
            GUI.color = Color.white;

            GUI.enabled = !isInvalid;
            if ((GUI.Button(rButton, GUIContent.none, _addstyle) || Event.current.keyCode == KeyCode.Return) &&
                !string.IsNullOrEmpty(addText) && !isInvalid)
            {
                onAdd(addText);
                addText = string.Empty;
            }

            GUI.enabled = true;
        }

        private static void DrawTagBackground(ref Rect r)
        {
            GUI.Box(r, GUIContent.none, _backgroundStyle);

            r.xMin += PaddingTagXMin;
            r.xMax -= PaddingTagXMax;
            r.yMin += PaddingTagYMin;
            r.yMax -= PaddingTagYMax;
        }

        private static void DrawGroupHeader(Rect r, string group, Action<string> onItemClick, Action onDeleteClick,
            string tag, Color guiColor, bool canGroupDelete, ref string addText)
        {
            var bgRect = r;
            var textStyle = _textStyleGroup;
            var gc = new GUIContent(tag);

            float deleteButtonWidth = 0;
            var textWidth = textStyle.CalcSize(gc).x + 4;
            if (canGroupDelete)
                deleteButtonWidth = WidthTagButton;
            else
                textWidth += 8;
            bgRect.width = textWidth + deleteButtonWidth;

            var rTagButton = new Rect(bgRect);
            rTagButton.position += new Vector2(1, YOffsetTagLabel);
            if (canGroupDelete)
                rTagButton.xMin += deleteButtonWidth - 4;
            else
                rTagButton.xMin += 4;

            rTagButton.width = textWidth;
            if (GUI.Button(rTagButton, gc, textStyle))
                switch (Event.current.button)
                {
                    case 0:
                        onItemClick?.Invoke(tag);
                        break;
                    case 1:
                        if (tag != "Ungrouped")
                            PopupWindow.Show(rTagButton, new FinalTaggerPopupMenu(tag, TagStyle.Group));
                        break;
                }

            //Draw icon after text
            if (TaggerSystem.Data.IsGroupSingleton(group))
            {
                var gcLength = textStyle.CalcSize(gc).x;
                var rIcon = new Rect(rTagButton);
                rIcon.x += gcLength + 4;
                rIcon.width = 20;
                rIcon.height = 20;
                GUI.DrawTexture(rIcon, FinalTaggerEditorTextures.SingletonIcon);
            }

            if (canGroupDelete)
            {
                var rDelete = new Rect(bgRect);
                rDelete.position += new Vector2(XOffsetTagButton, YOffsetTagButton);
                rDelete.width = deleteButtonWidth;
                var delete = GUI.Button(rDelete, GUIContent.none, _tagStyle);
                if (delete) onDeleteClick();
            }

            if (addText != null)
            {
                var rAdd = new Rect(r);
                var rAddWidth = 200f.Min(r.width - bgRect.width);
                rAdd.x = r.xMax - rAddWidth;
                rAdd.width = rAddWidth;
                DrawAddTag(rAdd, TagStyle.Tag, delegate(string s) { TaggerSystem.Data.AddTag(s, group); }, ref addText);
            }

            GUI.backgroundColor = guiColor;
        }

        private static bool DrawTag(Action<string> onItemClick, Action onDeleteClick, string tag, float newLineX,
            ref Rect bgRect, float xMax, Color guiColor, bool canBeDeleted, bool displayGroupInTagName)
        {
            var textStyle = _textStyleTag;
            GUIContent gc;
            if (displayGroupInTagName &&
                FinalTaggerPreferences.ShowGroupInTagsLabel == FinalTaggerGroupShowStyle.AsPrefix)
            {
                var groupOfTag = TaggerSystem.Data.GetGroupOfTag(tag);
                gc = new GUIContent(groupOfTag.GroupName + "." + tag);
            }
            else
            {
                gc = new GUIContent(tag);
            }

            float deleteButtonWidth = 0;
            var textWidth = textStyle.CalcSize(gc).x + 4;
            if (canBeDeleted) deleteButtonWidth = RemoveButtonWidth;

            var newLine = false;
            if (bgRect.xMin + textWidth + deleteButtonWidth + PaddingTagXMin > xMax)
            {
                newLine = true;
                bgRect.y += _height;
                bgRect.x = newLineX;
            }

            bgRect.width = textWidth + deleteButtonWidth + PaddingTagXMin;

            var tagColor = TaggerSystem.Data.GetColor(TaggerSystem.Data.IndexOfTag(tag));

            GUI.backgroundColor = tagColor;
            var r = new Rect(bgRect);
            DrawTagBackground(ref r);

            var rTagButton = new Rect(r);
            rTagButton.y += YOffsetTagLabel;

            rTagButton.width = textWidth;
            textStyle.normal.textColor = GetReadableFontColor(tagColor);
            if (GUI.Button(rTagButton, gc, textStyle))
                switch (Event.current.button)
                {
                    case 0:
                        onItemClick?.Invoke(tag);
                        break;
                    case 1:
                        PopupWindow.Show(rTagButton, new FinalTaggerPopupMenu(tag, TagStyle.Tag));
                        break;
                }

            GUI.backgroundColor = guiColor;
            if (canBeDeleted)
            {
                var rDelete = new Rect(r);
                rDelete.x += r.width - RemoveButtonOffsetX;
                rDelete.y -= 3f;
                rDelete.width = RemoveButtonWidth;
                var delete = GUI.Button(rDelete, GUIContent.none, _tagStyle);
                if (delete) onDeleteClick();
            }

            bgRect.x += bgRect.width;
            return newLine;
        }

        private static Rect GetNewRect()
        {
            return new Rect(EditorGUILayout.GetControlRect(_guiLayoutOption)) { xMin = 16 };
        }

        public static Color GetReadableFontColor(Color background)
        {
            if (EditorGUIUtility.isProSkin) return Color.white * 0.85f;

            var brightness = PerceivedBrightness(background);
            var readableFontColor = 1f - brightness;
            return readableFontColor < 0.3F ? Color.black : Color.white;
        }

        private static void InitStyle()
        {
            if (_initialized) return;

            _initialized = false;

            _addstyle = new GUIStyle("OL Plus");

            _headerStyle = new GUIStyle("label") { font = EditorStyles.boldFont };

            _tagStyle = new GUIStyle("OL Minus"); // {normal = {textColor = Color.white}, font = EditorStyles.boldFont};

            _textStyleTag = new GUIStyle(EditorStyles.label);
            _textStyleGroup = new GUIStyle(EditorStyles.boldLabel);

            _height = EditorGUIUtility.singleLineHeight + 2;
            _guiLayoutOption = GUILayout.Height(_height);
            _blendBackgroundStyle = "MeBlendBackground";
            _blendBackgroundStyle.stretchHeight = true;
            _blendBackgroundStyle.fixedHeight = 0f;
            _backgroundStyle = "CN CountBadge";
        }

        private static Rect NewLine(string header)
        {
            return new Rect(EditorGUILayout.GetControlRect(_guiLayoutOption))
                { xMin = string.IsNullOrEmpty(header) ? 16 : 30 };
        }

        private static float PerceivedBrightness(Color c)
        {
            return Mathf.Sqrt(c.r * c.r * .299f + c.g * c.g * .587f + c.b * c.b * .114f);
        }
    }
}