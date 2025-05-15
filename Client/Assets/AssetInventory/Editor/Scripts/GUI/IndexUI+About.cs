﻿using UnityEditor;
using UnityEngine;

namespace AssetInventory
{
    public partial class IndexUI
    {
        private void DrawAboutTab()
        {
            GUIStyle textColor = EditorGUIUtility.isProSkin ? UIStyles.whiteCenter : UIStyles.blackCenter;

            EditorGUILayout.Space(30);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box(Logo, EditorStyles.centeredGreyMiniLabel, GUILayout.MaxWidth(300), GUILayout.MaxHeight(300));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("A tool by Impossible Robert", textColor);
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Developer: Robert Wetzold", textColor);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Online Resources", UIStyles.centerLinkLabel)) Application.OpenURL(AI.HOME_LINK);
            EditorGUILayout.LabelField(" | ", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(6));
            if (GUILayout.Button("Join Discord!", UIStyles.centerLinkLabel)) Application.OpenURL(AI.DISCORD_LINK);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            EditorGUILayout.LabelField($"Version {AI.VERSION}", textColor);
            EditorGUILayout.Space(30);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox("If you like this asset please consider leaving a review on the Unity Asset Store. Thanks a million!", MessageType.Info);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Write Review")) Application.OpenURL(AI.ASSET_STORE_LINK);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (ShowAdvanced() && GUILayout.Button("Create Debug Support Report")) CreateDebugReport();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            if (AI.DEBUG_MODE && GUILayout.Button("Get Token", GUILayout.ExpandWidth(false))) Debug.Log(CloudProjectSettings.accessToken);
            if (AI.DEBUG_MODE && GUILayout.Button("Reload Lookups")) ReloadLookups();
            if (AI.DEBUG_MODE && GUILayout.Button("Free Memory")) Resources.UnloadUnusedAssets();
        }
    }
}