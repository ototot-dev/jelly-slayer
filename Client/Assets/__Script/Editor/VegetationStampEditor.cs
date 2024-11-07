using UnityEngine;
using UnityEditor;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    [CustomEditor(typeof(VegetationStamp))]
    public class VegetationStampEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate"))
            {
                (target as VegetationStamp).Reset();
                (target as VegetationStamp).Generate();
            }
        }
    }

}