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
            {
                if ((target as JellyMeshController).hostBrain != null)
                    (target as JellyMeshController).springMassSystem.core.position = (target as JellyMeshController).hostBrain.coreColliderHelper.GetWorldCenter();
                
                (target as JellyMeshController).FadeIn(0.5f);
            }

            if (GUILayout.Button("Fade Out"))
                (target as JellyMeshController).FadeOut(0.2f);

            if (GUILayout.Button("Die"))
                (target as JellyMeshController).Die(10f);

            // if (GUILayout.Button("Hook"))
            //     (target as JellyMeshController).StartHook();

            // if (GUILayout.Button("Unhook"))
            //     (target as JellyMeshController).FinishHook();

            if (GUILayout.Button("Impulse"))
                (target as JellyMeshController).springMassSystem.AddImpulseRandom((target as JellyMeshController).hitImpulseStrength);

            if (GUILayout.Button("Hit"))
            {
                (target as JellyMeshController).springMassSystem.AddImpulseRandom((target as JellyMeshController).hitImpulseStrength);
                (target as JellyMeshController).ShowHitColor(0.3f);
                (target as JellyMeshController).PopRandomCube(8);
                // (target as JellyMeshController).ShowHitColor(2f);

                // EffectManager.Instance.Show((target as JellyMeshController).onHitFx, (target as JellyMeshController).springMassSystem.core.position, Quaternion.identity, Vector3.one);
            }
        }
    }

}