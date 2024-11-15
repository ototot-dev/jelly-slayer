using UnityEngine;
using UnityEditor;
using System.Linq;
using FlowCanvas.Nodes;

namespace Game
{
    [CustomEditor(typeof(PawnBuffController))]
    public class PawnBuffControllerEditor : Editor
    {
        bool __stakableBuffFoldOut = true;
        bool __uniqueBuffFoldOut = true;
        bool __externBuffFoldOut = true;
        GUIStyle __buffLabelStyle;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            __buffLabelStyle ??= new("U2D.createRect");
            __stakableBuffFoldOut = EditorGUILayout.Foldout(__stakableBuffFoldOut, "Stakable Buff");
            if (__stakableBuffFoldOut)
            {
                GUILayout.BeginVertical();
                {
                    foreach (var b in (target as PawnBuffController).StackableBuffTable)
                        GUILayout.Button($"{b.Key} | count: {b.Value.Count}", __buffLabelStyle);
                }
                GUILayout.EndHorizontal();
            }

            __uniqueBuffFoldOut = EditorGUILayout.Foldout(__uniqueBuffFoldOut, "Unique Buff");
            if (__uniqueBuffFoldOut)
            {
                GUILayout.BeginVertical();
                {
                    foreach (var b in (target as PawnBuffController).UniqueBuffTable)
                        GUILayout.Button(b.Value.Item2 < 0f ? $"{b.Key}" : $"{b.Key} | duration: {(b.Value.Item2 - Time.time):F1}", __buffLabelStyle);
                }
                GUILayout.EndHorizontal();
            }

            __externBuffFoldOut = EditorGUILayout.Foldout(__externBuffFoldOut, "Extern Buff");
            if (__uniqueBuffFoldOut)
            {
                GUILayout.BeginVertical();
                {
                    foreach (var b in (target as PawnBuffController).externBuffContainer)
                    {
                        var enumerator = b.GetBuffEnumerator();
                        while (enumerator.MoveNext())
                            GUILayout.Button(enumerator.Current.Value.Item2 < 0f ? $"{enumerator.Current.Key}" :  $"{enumerator.Current.Key} | duration: {(enumerator.Current.Value.Item2 - Time.time):F1}", __buffLabelStyle);

                        enumerator.Dispose();
                    }
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}