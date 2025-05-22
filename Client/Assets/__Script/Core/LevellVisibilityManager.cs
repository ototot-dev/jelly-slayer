using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class LevelVisibilityManager : MonoSingleton<LevelVisibilityManager>
    {
        HashSet<SphereCollider> __visibilityChecker = new();
        HashSet<MeshRenderer> __visibilityOffRenderers = new();
        static readonly RaycastHit[] __tempHitsNonAlloc = new RaycastHit[16];
        static readonly HashSet<MeshRenderer> __hitRenderers = new();
        static readonly HashSet<MeshRenderer> __noHitRenderers = new();

        public bool RegisterChecker(SphereCollider collider)
        {
            return __visibilityChecker.Add(collider);
        }

        public void UnregisterChecker(SphereCollider collider)
        {
            __visibilityChecker.Remove(collider);
        }

        void Update()
        {
            foreach (var c in __visibilityChecker)
            {
                var origin = c.transform.position + c.center;
                var distanceVec = GameContext.Instance.cameraCtrler.gameCamera.transform.position - origin;
                var hitCount = Physics.SphereCastNonAlloc(c.transform.position + c.center, c.radius, distanceVec.normalized, __tempHitsNonAlloc, distanceVec.magnitude, LayerMask.GetMask("Ceil"));

                if (hitCount > 0)
                {
                    for (int i = 0; i < hitCount; i++)
                    {
                        if (__tempHitsNonAlloc[i].collider.TryGetComponent<MeshRenderer>(out var renderer))
                            __hitRenderers.Add(renderer);
                    }
                }
            }

            foreach (var c in __visibilityOffRenderers)
            {
                if (!__hitRenderers.Contains(c))
                {
                    __noHitRenderers.Add(c);
                    c.enabled = true;
                }
            }

            foreach (var r in __hitRenderers)
            {
                if (__visibilityOffRenderers.Add(r))
                    r.enabled = false;
            }

            foreach (var r in __noHitRenderers)
                __visibilityOffRenderers.Remove(r);

            __hitRenderers.Clear();
            __noHitRenderers.Clear();
        }
    }
}
