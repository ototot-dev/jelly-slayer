using UnityEngine;
using UnityEditor;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    [CustomEditor(typeof(RockStamp))]
    public class RockStampEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Generate"))
            {
                (target as RockStamp).Reset();
                (target as RockStamp).Generate();
            }
        }
    }

}