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

            if (GUILayout.Button("Hook"))
                (target as JellyMeshController).StartHook();

            if (GUILayout.Button("Unhook"))
                (target as JellyMeshController).FinishHook();

            if (GUILayout.Button("Impulse"))
                (target as JellyMeshController).springMassSystem.AddImpulseRandom((target as JellyMeshController).impulseStrength);

            if (GUILayout.Button("Hit"))
            {
                (target as JellyMeshController).springMassSystem.AddImpulseRandom((target as JellyMeshController).impulseStrength);
                (target as JellyMeshController).ShowHitColor(0.2f);

                EffectManager.Instance.Show((target as JellyMeshController).onHitFx, (target as JellyMeshController).springMassSystem.core.position, Quaternion.identity, Vector3.one);
            }
        }
    }

}