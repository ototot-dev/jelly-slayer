// #region License
// // // --------------------------------------------------------------------------------------------------------------------
// // // <summary>
// // //   © 2024 Final Factory Florian Schmidt. All rights reserved.
// // //   TaggerDataEditor.cs is part of an asset of Final Factory distributed on the Unity Asset Store.
// // //   Usage or distribution of this file is subject to the Unity Asset Store Terms of Service.
// // // </summary>
// // // --------------------------------------------------------------------------------------------------------------------
// #endregion

using UnityEditor;
using UnityEngine;

namespace Finalfactory.Tagger.Editor
{
    [CustomEditor(typeof(TaggerData))]

    // ReSharper disable once UnusedMember.Global
    public class TaggerDataEditor : UnityEditor.Editor
    {
        public static string NewGroupTxt = string.Empty;

        public override void OnInspectorGUI()
        {
            var taggerData = (TaggerData)target;
            var groups = taggerData.GetAllGroups(FinalTaggerPreferences.AlphabeticalOrder &&
                                                 FinalTaggerPreferences.AlphabeticalOrderOfGroups);
            FinalTaggerEditorUI.AnyEditorWindowReference = this;
            FinalTaggerEditorUI.DrawGroups(groups, "All known tags", null, TaggerSystem.Data.DeleteTag, true, true,
                true);

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "WARNING:\n" + "Do NOT DELETE this Resources. \n" +
                "If you do, the tags on your prefabs are lost or are totally messed up.",
                MessageType.Warning);

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Static Class Generation", EditorStyles.boldLabel);
            if (FinalTaggerUtilities.AutoGenScheduled)
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Auto Generation Delay: " +
                                           FinalTaggerUtilities.AutoGenRemainingTime.ToString("##") + "s");
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Generate")) FinalTaggerUtilities.GenerateStaticClasses(true);
        }
    }
}