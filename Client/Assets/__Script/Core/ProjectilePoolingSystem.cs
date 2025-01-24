using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class ProjectilePoolingSystem : MonoSingleton<ProjectilePoolingSystem>
    {
        public T GetProjectile<T>(GameObject sourcePrefab, Vector3 position, Quaternion rotation) where T : ProjectileMovement
        {
            if (!__projectilePool.ContainsKey(sourcePrefab))
                __projectilePool.Add(sourcePrefab, new());

            T ret = null;
            if (__projectilePool[sourcePrefab].Count > 0)
            {
                ret = __projectilePool[sourcePrefab].First() as T;
                __projectilePool[sourcePrefab].Remove(ret);
            }
            else
            {
                ret = Instantiate(sourcePrefab).GetComponent<T>();
                ret.transform.SetParent(transform);
            }

            ret.transform.SetPositionAndRotation(position, rotation);
            ret.gameObject.SetActive(true);

            return ret;
        }

        public void ReturnProjectile(ProjectileMovement projectile, GameObject sourcePrefab)
        {
            projectile.emitterBrain.Value = null;
            projectile.gameObject.SetActive(false);
            projectile.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            __projectilePool[sourcePrefab].Add(projectile);
        }

        readonly Dictionary<GameObject, HashSet<ProjectileMovement>> __projectilePool = new();
    }
}