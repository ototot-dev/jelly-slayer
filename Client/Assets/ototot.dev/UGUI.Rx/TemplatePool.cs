using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;


namespace UGUI.Rx
{
    /// <summary>
    /// TemplatePool is an object poll for Template instances.
    /// </summary>
    /// <typeparam name="TemplatePool"></typeparam>
    public class TemplatePool : ototot.dev.MonoSingleton<TemplatePool>
    {

        /// <summary>
        /// The container for Template object instances.
        /// </summary>
        /// <returns></returns>
        Dictionary<string, List<Timestamped<Template>>> _pool = new Dictionary<string, List<Timestamped<Template>>>();

        /// <summary>
        /// Get a Template instance from the pool.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Template Get(string path)
        {
            if (!_pool.ContainsKey(path))
            {
                _pool.Add(path, new List<Timestamped<Template>>());

                //* Starts GC
                MainThreadDispatcher.StartUpdateMicroCoroutine(CollectGarbages(_pool[path]));
            }

            var templates = _pool[path];

            if (templates.Count > 0)
            {
                var ret = templates[0].Value;
                templates.RemoveAt(0);

                return ret;

            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Return a Template instance to the pool.
        /// </summary>
        /// <param name="template"></param>
        public void Return(Template template)
        {
            if (template.gameObject.activeSelf)
                template.gameObject.SetActive(false);

            template.transform.SetParent(transform, false);

            if (_pool.ContainsKey(template.poolingName))
                _pool[template.poolingName].Add(new Timestamped<Template>(template, DateTimeOffset.Now));
            else
                Debug.LogWarningFormat("TemplatePool => _pool doesn't have '{0}' as a key :(", template.poolingName);
        }

        //! Any Template instance which is not accessed durning '_garbageLifeTime' will be destoryed.
        const int _garbageLifeTime = 60000;

        /// <summary>
        /// The MicroCoroutine which checks the TimeStamped value of each Template instance and if it's too old (Compared to '_garbageLifeTime' value) then destroy it.
        /// </summary>
        /// <param name="templates"></param>
        /// <returns></returns>
        IEnumerator CollectGarbages(List<Timestamped<Template>> templates)
        {
            var wait = DateTimeOffset.Now;

            while (!_isDestroyed)
            {
                yield return null;

                // templates이 비어있거나 60초간 wait가 안되었으면 스킵
                if (templates.Count == 0 || (DateTimeOffset.Now - wait).TotalMilliseconds < 60000)
                    continue;

                var anyDestroyed = false;

                foreach (var t in templates)
                {
                    if ((DateTimeOffset.Now - t.Timestamp).TotalMilliseconds > _garbageLifeTime)
                    {
                        Destroy(t.Value.gameObject);
                        anyDestroyed = true;
                    }
                }

                if (anyDestroyed)
                {
                    templates.RemoveAll(t => (DateTimeOffset.Now - t.Timestamp).TotalMilliseconds > _garbageLifeTime);
                    GC.Collect();
                }

                wait = DateTimeOffset.Now;
            }
        }

        void OnDestroy()
        {
            _isDestroyed = true;
        }

        bool _isDestroyed;

    }
}