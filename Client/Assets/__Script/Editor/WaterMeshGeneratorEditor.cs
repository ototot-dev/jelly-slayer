using UnityEngine;
using UnityEditor;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    [CustomEditor(typeof(WaterMeshGenerator))]
    public class WaterMeshGeneratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate"))
            {
                (target as WaterMeshGenerator).Init();
                (target as WaterMeshGenerator).ResetMesh();
                (target as WaterMeshGenerator).UpdateMesh();
            }
        }
    }

}