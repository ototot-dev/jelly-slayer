using UnityEngine;
using UnityEditor;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    [CustomEditor(typeof(TerrainManager))]
    public class TerrainManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate"))
                (target as TerrainManager).Generate();
        }
    }
}