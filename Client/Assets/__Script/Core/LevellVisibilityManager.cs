using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class LevelVisibilityManager : MonoSingleton<LevelVisibilityManager>
    {
        HashSet<SphereCollider> __visibilityChecker = new();
        HashSet<MeshRenderer> __visibilityBlockerRenderers = new();
        static readonly RaycastHit[] __tempHitsNonAlloc = new RaycastHit[16];
        static readonly HashSet<MeshRenderer> __tempHitRenderers = new();
        static readonly HashSet<MeshRenderer> __tempNoHitRenderers = new();

        public bool RegisterChecker(SphereCollider collider)
        {
            return false;
            return __visibilityChecker.Add(collider);
        }

        public void UnregisterChecker(SphereCollider collider)
        {
            __visibilityChecker.Remove(collider);
        }

        void Update()
        {
            if (__visibilityChecker.Count == 0)
                return;

            foreach (var c in __visibilityChecker)
            {
                var origin = c.transform.position + c.center;
                var distanceVec = GameContext.Instance.cameraCtrler.gameCamera.transform.position - origin;
                var hitCount = Physics.SphereCastNonAlloc(c.transform.position + c.center, c.radius, distanceVec.normalized, __tempHitsNonAlloc, distanceVec.magnitude, LayerMask.GetMask("Obstacle"));

                if (hitCount > 0)
                {
                    for (int i = 0; i < hitCount; i++)
                    {
                        if (__tempHitsNonAlloc[i].collider.CompareTag("VisibilityBlocker") && __tempHitsNonAlloc[i].collider.TryGetComponent<MeshRenderer>(out var renderer))
                            __tempHitRenderers.Add(renderer);
                    }
                }
            }

            foreach (var c in __visibilityBlockerRenderers)
            {
                if (!__tempHitRenderers.Contains(c))
                    __tempNoHitRenderers.Add(c);
            }

            foreach (var r in __tempHitRenderers)
            {
                __visibilityBlockerRenderers.Add(r);
                foreach (var m in r.materials)
                {
                    m.SetColor("_BaseColor", Color.white.AdjustAlpha(0.9f));
                    m.SetFloat("_Alpha", 0.9f);
                }
            }

            foreach (var r in __tempNoHitRenderers)
            {
                __visibilityBlockerRenderers.Remove(r);
                foreach (var m in r.materials)
                {
                    m.SetColor("_BaseColor", Color.white.AdjustAlpha(1f));
                    m.SetFloat("_Alpha", 1f);
                }
            }

            __tempHitRenderers.Clear();
            __tempNoHitRenderers.Clear();
        }
    }
}
