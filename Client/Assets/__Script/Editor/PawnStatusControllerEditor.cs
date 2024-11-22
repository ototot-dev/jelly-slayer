using UnityEngine;
using UnityEditor;
using System.Linq;
using FlowCanvas.Nodes;

namespace Game
{
    [CustomEditor(typeof(PawnStatusController))]
    public class PawnStatusControllerEditor : Editor
    {
        bool __stakableFoldOut = true;
        bool __uniqueFoldOut = true;
        bool __externFoldOut = true;
        GUIStyle __labelStyle;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            __labelStyle ??= new("U2D.createRect");
            __stakableFoldOut = EditorGUILayout.Foldout(__stakableFoldOut, "Stakable Status");
            if (__stakableFoldOut)
            {
                GUILayout.BeginVertical();
                {
                    foreach (var t in (target as PawnStatusController).StackableTable)
                        GUILayout.Button($"{t.Key} | count: {t.Value.Count}", __labelStyle);
                }
                GUILayout.EndHorizontal();
            }

            __uniqueFoldOut = EditorGUILayout.Foldout(__uniqueFoldOut, "Unique Status");
            if (__uniqueFoldOut)
            {
                GUILayout.BeginVertical();
                {
                    foreach (var t in (target as PawnStatusController).UniqueTable)
                        GUILayout.Button(t.Value.Item2 < 0f ? $"{t.Key}" : $"{t.Key} | duration: {(t.Value.Item2 - Time.time):F1}", __labelStyle);
                }
                GUILayout.EndHorizontal();
            }

            __externFoldOut = EditorGUILayout.Foldout(__externFoldOut, "Extern Status");
            if (__externFoldOut)
            {
                GUILayout.BeginVertical();
                {
                    foreach (var c in (target as PawnStatusController).externContainer)
                    {
                        var enumerator = c.GetStatusEnumerator();
                        while (enumerator.MoveNext())
                            GUILayout.Button(enumerator.Current.Value.Item2 < 0f ? $"{enumerator.Current.Key}" :  $"{enumerator.Current.Key} | duration: {(enumerator.Current.Value.Item2 - Time.time):F1}", __labelStyle);

                        enumerator.Dispose();
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}