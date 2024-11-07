#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Retween.Rx
{
    [CustomEditor(typeof(TweenName))]
    public class TweenNameEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var tweenName = serializedObject.targetObject as TweenName;

            if (tweenName.hasValidName)
            {
                EditorGUILayout.TextField("Tag", tweenName.tagName);
                EditorGUILayout.TextField("Class", tweenName.className);
                EditorGUILayout.TextField("State", tweenName.stateName);
            }
            else
            {
                var invalid = "Invalid";
                EditorGUILayout.TextField("Tag", invalid);
                EditorGUILayout.TextField("Class", invalid);
                EditorGUILayout.TextField("State", invalid);
            }

            if (GUILayout.Button("Refresh Tag, Class, State"))
                (serializedObject.targetObject as TweenName).Parse();

            // Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif