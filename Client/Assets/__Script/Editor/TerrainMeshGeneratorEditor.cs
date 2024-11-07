using UnityEngine;
using UnityEditor;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    [CustomEditor(typeof(TerrainMeshGenerator))]
    public class TerrainMeshGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate"))
            {
                (target as TerrainMeshGenerator).Init();
                (target as TerrainMeshGenerator).ResetMesh();
                (target as TerrainMeshGenerator).UpdateMesh();
            }
        }
    }

}