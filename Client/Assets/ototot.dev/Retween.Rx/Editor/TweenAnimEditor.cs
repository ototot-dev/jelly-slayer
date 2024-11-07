#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Retween.Rx
{
    [CustomEditor(typeof(TweenAnim))]
    public class TweenAnimEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var transitionProperty = serializedObject.FindProperty("transition");
            var rewindOnCancelledProperty = serializedObject.FindProperty("rewindOnCancelled");
            var rewindOnRollbackProperty = serializedObject.FindProperty("rewindOnRollback");
            var runRollbackProperty = serializedObject.FindProperty("runRollback");
            var rollbackTransitionProperty = serializedObject.FindProperty("rollbackTransition");
            var animClipsProperty = serializedObject.FindProperty("animClips");

            EditorGUILayout.PropertyField(animClipsProperty, true);

            EditorGUILayout.PropertyField(transitionProperty);
            EditorGUILayout.PropertyField(rewindOnCancelledProperty);

            if (runRollbackProperty.boolValue)
                EditorGUILayout.PropertyField(rewindOnRollbackProperty);

            EditorGUILayout.PropertyField(runRollbackProperty);

            if (runRollbackProperty.boolValue && !rewindOnRollbackProperty.boolValue)
                EditorGUILayout.PropertyField(rollbackTransitionProperty);

            // Apply changes to the serializedProperty - always do this at the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }

    }
}
#endif