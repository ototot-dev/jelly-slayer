using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public interface IObjectPoolable
    {
        void OnGetFromPool();
        void OnReturnedToPool();
    }

    public class ObjectPoolableHandler : MonoBehaviour
    {
        public string sourcePath;
        public GameObject sourcePrefab;
        public MonoBehaviour targetComponent;
        public IObjectPoolable poolable;
    }

    public class ObjectPoolingSystem : MonoSingleton<ObjectPoolingSystem>
    {
        public T GetObject<T>(string sourcePath, Vector3 position, Quaternion rotation) where T : MonoBehaviour
        {
            Debug.Assert(!string.IsNullOrEmpty(sourcePath));

            if (!__poolA.ContainsKey(sourcePath))
                __poolA.Add(sourcePath, new());

            ObjectPoolableHandler handler;
            if (__poolA[sourcePath].Count > 0)
            {
                handler = __poolA[sourcePath].First();
                __poolA[sourcePath].Remove(handler);
            }
            else
            {
                handler = Instantiate(Resources.Load<GameObject>(sourcePath), position, rotation).AddComponent<ObjectPoolableHandler>();
                handler.sourcePath = sourcePath;
                handler.targetComponent = handler.GetComponent<T>();
                handler.poolable = handler.targetComponent as IObjectPoolable;
                
#if UNITY_EDITOR
                handler.gameObject.name = handler.gameObject.name + $"-{++__instanceCount}";
#endif
            }

            handler.transform.SetPositionAndRotation(position, rotation);
            handler.gameObject.SetActive(true);
            handler.poolable?.OnGetFromPool();

            var ret = handler.targetComponent as T;
            Debug.Assert(ret != null);

            return ret;
        }

        public T GetObject<T>(GameObject sourcePrefab, Vector3 position, Quaternion rotation) where T : MonoBehaviour
        {
            if (!__poolB.ContainsKey(sourcePrefab))
                __poolB.Add(sourcePrefab, new());

            ObjectPoolableHandler handler;
            if (__poolB[sourcePrefab].Count > 0)
            {
                handler = __poolB[sourcePrefab].First();
                __poolB[sourcePrefab].Remove(handler);
            }
            else
            {
                handler = Instantiate(sourcePrefab, position, rotation).AddComponent<ObjectPoolableHandler>();
                handler.sourcePrefab = sourcePrefab;
                handler.targetComponent = handler.GetComponent<T>();
                handler.poolable = handler.targetComponent as IObjectPoolable;

#if UNITY_EDITOR
                handler.gameObject.name = handler.gameObject.name + $"-{++__instanceCount}";
#endif
            }

            handler.transform.SetPositionAndRotation(position, rotation);
            handler.gameObject.SetActive(true);
            handler.poolable?.OnGetFromPool();

            var ret = handler.targetComponent as T;
            Debug.Assert(ret != null);

            return ret;
        }

        public void ReturnObject(GameObject instantiated)
        {
            var handler = instantiated.GetComponent<ObjectPoolableHandler>();
            Debug.Assert(handler != null);

            handler.poolable?.OnReturnedToPool();
            handler.gameObject.SetActive(false);
            handler.transform.SetParent(transform);
            handler.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

            if (handler.sourcePrefab != null)
            {
                __poolB[handler.sourcePrefab].Add(handler);
            }
            else
            {
                Debug.Assert(handler.sourcePath != string.Empty);
                __poolA[handler.sourcePath].Add(handler);
            }
        }

        int __instanceCount;
        readonly Dictionary<string, HashSet<ObjectPoolableHandler>> __poolA = new();
        readonly Dictionary<GameObject, HashSet<ObjectPoolableHandler>> __poolB = new();
    }
}