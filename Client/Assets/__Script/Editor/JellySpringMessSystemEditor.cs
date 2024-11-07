using UnityEngine;
using UnityEditor;
using Unity.Linq;
using System.Linq;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    [CustomEditor(typeof(JellySpringMassSystem))]
    public class JellySpringMassSystemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            if (GUILayout.Button("Build"))
                (target as JellySpringMassSystem).Build();
        }
    }

}