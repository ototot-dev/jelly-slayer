// using UnityEngine;
// using UnityEditor;
// using Unity.Linq;
// using System.Linq;

// namespace Game
// {
//     [CustomEditor(typeof(SlayerAnimController))]
//     public class HeroAnimControllerEditor : Editor
//     {

//         public override void OnInspectorGUI()
//         {
//             base.OnInspectorGUI();

//             if (GUILayout.Button("Reset"))
//             {
//                 (target as SlayerAnimController).sourceTest.gameObject.DescendantsAndSelf().ForEach(s =>
//                 {
//                     if ((target as SlayerAnimController).targetTest.gameObject.DescendantsAndSelf().Any(t => t.name == s.name))
//                     {
//                     var found = (target as SlayerAnimController).targetTest.gameObject.DescendantsAndSelf().First(t => t.name == s.name).transform;
//                     s.transform.SetLocalPositionAndRotation(found.localPosition, found.localRotation);
//                     }
//                 });
//             }
//         }
//     }
// }