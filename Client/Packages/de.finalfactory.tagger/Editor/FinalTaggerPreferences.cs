// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   FinalTaggerPreferences.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using FinalFactory;
using FinalFactory.Preferences;
using FinalFactory.Preferences.Items;
using UnityEditor;
using UnityEngine;

namespace Finalfactory.Tagger.Editor
{
    public static class FinalTaggerPreferences
    {
        public static readonly PreferenceKeyGen KeyGen = new(Company.FinalFactory, "Tagger");

        private static readonly PreferenceItemEnum<FinalTaggerGroupShowStyle> ShowGroupInTagsLabelItem =
            new(KeyGen["Show_GroupInTagsLabelKey"], FinalTaggerGroupShowStyle.GroupTags);

        private static readonly PreferenceItemBool AlphabeticalOrderItem = new(KeyGen["AlphabeticalOrder"], true);

        private static readonly PreferenceItemBool AlphabeticalOrderOfTagsOnGameObjectsItem =
            new(KeyGen["AlphabeticalOrderOfTags_OnGameObjects"], true);

        private static readonly PreferenceItemBool AlphabeticalOrderOfTagsInGroupsItem =
            new(KeyGen["AlphabeticalOrderOfTags_InGroups"], true);

        private static readonly PreferenceItemBool AlphabeticalOrderOfGroupsItem =
            new(KeyGen["AlphabeticalOrderOfGroups"], true);

        private static readonly PreferenceItemBool AlphabeticalOrderOfTagsOnGameObjectsIgnoreGroupsItem =
            new(KeyGen["AlphabeticalOrderOfTags_OnGameObjects_IgnoreGroups"]);

        private static readonly PreferenceItemString StaticClassPathItem =
            new(KeyGen["StaticClassPath"], "Assets/Scripts/Tags.cs", false);

        private static readonly PreferenceItemBool AutoGenerateStaticClassesItem =
            new(KeyGen["AutoGenerateStaticClasses"], true);

        public static bool AlphabeticalOrder => AlphabeticalOrderItem.Value;

        public static bool AlphabeticalOrderOfTagsOnGameObjects => AlphabeticalOrderOfTagsOnGameObjectsItem.Value;

        public static bool AlphabeticalOrderOfTagsInGroups => AlphabeticalOrderOfTagsInGroupsItem.Value;

        public static bool AlphabeticalOrderOfGroups => AlphabeticalOrderOfGroupsItem.Value;

        public static bool AlphabeticalOrderOfTagsOnGameObjectsIgnoreGroups =>
            AlphabeticalOrderOfTagsOnGameObjectsIgnoreGroupsItem.Value;

        public static FinalTaggerGroupShowStyle ShowGroupInTagsLabel => ShowGroupInTagsLabelItem.Value;

        public static string StaticClassPath => StaticClassPathItem.Value;

        /// <summary>
        ///     The preferences GUI.
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider PreferencesGUI()
        {
            var provider = new SettingsProvider(KeyGen.SettingsProviderPath, SettingsScope.User)
            {
                label = KeyGen.SettingsLabel,
                guiHandler = delegate
                {
                    EditorGUIUtility.labelWidth = 300f;
                    // Preferences GUI

                    ShowGroupInTagsLabelItem.Value =
                        (FinalTaggerGroupShowStyle)EditorGUILayout.EnumPopup("Tag Group Display Style",
                            ShowGroupInTagsLabelItem.Value);
                    EditorGUILayout.Space();
                    AlphabeticalOrderItem.Value = EditorGUILayout.Toggle("Enable order tags alphabetically",
                        AlphabeticalOrderItem.Value);

                    if (AlphabeticalOrderItem.Value)
                    {
                        EditorGUI.indentLevel++;
                        AlphabeticalOrderOfTagsOnGameObjectsItem.Value = EditorGUILayout.Toggle(
                            "Order attached tags alphabetically", AlphabeticalOrderOfTagsOnGameObjectsItem.Value);
                        if (AlphabeticalOrderOfTagsOnGameObjectsItem.Value)
                        {
                            EditorGUI.indentLevel++;
                            AlphabeticalOrderOfTagsOnGameObjectsIgnoreGroupsItem.Value =
                                EditorGUILayout.Toggle("Ignore groups",
                                    AlphabeticalOrderOfTagsOnGameObjectsIgnoreGroupsItem.Value);
                            EditorGUI.indentLevel--;
                        }

                        AlphabeticalOrderOfTagsInGroupsItem.Value = EditorGUILayout.Toggle(
                            "Order the tags alphabetically in groups", AlphabeticalOrderOfTagsInGroupsItem.Value);
                        AlphabeticalOrderOfGroupsItem.Value = EditorGUILayout.Toggle("Order the groups alphabetically",
                            AlphabeticalOrderOfGroupsItem.Value);
                        EditorGUI.indentLevel--;
                    }

                    EditorGUILayout.Space();
                    //Title
                    EditorGUILayout.LabelField("Static Class Generation", EditorStyles.boldLabel);
                    StaticClassPathItem.Value = EditorGUILayout.TextField("Path", StaticClassPathItem.Value);
                    if (StaticClassPathItem.HasChanges)
                    {
                        var path = StaticClassPathItem.Value;
                        if (!path.EndsWith(".cs")) path += ".cs";

                        if (!path.StartsWith("Assets/"))
                            EditorGUILayout.HelpBox("The path must be in the Assets folder", MessageType.Error);
                        else
                            StaticClassPathItem.Save();
                    }

                    AutoGenerateStaticClassesItem.Value = EditorGUILayout.Toggle("Auto Generate Static Classes",
                        AutoGenerateStaticClassesItem.Value);

                    if (GUILayout.Button("Generate")) FinalTaggerUtilities.GenerateStaticClasses(true);
                },
                keywords = KeyGen.SettingsKeywords
            };
            return provider;
        }
    }
}