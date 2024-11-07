using UnityEngine;
using UnityEditor;

namespace Game
{
    [CustomEditor(typeof(JellyMeshBuilder))]
    public class JellyMeshBuilderEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Build-Mesh"))
            {
                (target as JellyMeshBuilder).Reset();
                (target as JellyMeshBuilder).BuildMesh();
            }
            
            if (GUILayout.Button("Save-Mesh") && (target as JellyMeshBuilder).meshRenderer.sharedMesh != null)
            {
                AssetDatabase.CreateAsset((target as JellyMeshBuilder).meshRenderer.sharedMesh, "Assets/__Data/JellyMesh.asset");
                AssetDatabase.SaveAssets();
            }
        }
    }

}