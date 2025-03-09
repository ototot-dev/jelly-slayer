using UnityEngine;
using UnityEditor;

namespace Game
{
    [CustomEditor(typeof(JellyMeshController))]
    public class JellyMeshControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Fade In"))
                (target as JellyMeshController).FadeIn(0.5f);

            if (GUILayout.Button("Fade Out"))
                (target as JellyMeshController).FadeOut(0.2f);
        }
    }

}