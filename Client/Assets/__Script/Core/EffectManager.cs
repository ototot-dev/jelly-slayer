using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{
    public class EffectInstance : MonoBehaviour
    {
        public string effectName;
        public GameObject sourcePrefab;

        void OnParticleCollision(GameObject other)
        {
            __collisionEvents ??= new();

            int eventNum = __particleSystem.GetCollisionEvents(other, __collisionEvents);
            for (int i = 0; i < eventNum; i++)
                onCollisionEvent?.Invoke(__collisionEvents[i]);
        }

        ParticleSystemScalingMode __defaultScaleMode;
        List<ParticleCollisionEvent> __collisionEvents;
        public Action<ParticleCollisionEvent> onCollisionEvent;

        public void PlayLooping()
        {
            Debug.Assert(__stopDiposable == null);
            
            if (__particleSystem != null || TryGetComponent<ParticleSystem>(out __particleSystem))
            {
                var mainModule = __particleSystem.main; mainModule.loop = true;
                __particleSystem.Play();
            }
        }

        public void Play(float duration, bool releaseInstance = true)
        {
            Debug.Assert(__stopDiposable == null);

            if (__particleSystem != null || TryGetComponent<ParticleSystem>(out __particleSystem))
            {
                var mainModule = __particleSystem.main; 
                mainModule.loop = false;
                __particleSystem.Play();

                duration = duration > 0 ? duration : Mathf.Max(0.01f, mainModule.duration * 0.99f);
                Observable.Timer(TimeSpan.FromSeconds(duration)).Subscribe(_ => Stop(releaseInstance)).AddTo(this);
            }
        }

        ParticleSystem __particleSystem;
        IDisposable __stopDiposable;

        public void Stop(bool releaseInstance = true)
        {
            if (__particleSystem != null || TryGetComponent<ParticleSystem>(out __particleSystem))
            {
                __particleSystem.Stop();

                if (releaseInstance)
                {
                    Debug.Assert(__stopDiposable == null);

                    __stopDiposable = Observable.Timer(TimeSpan.FromSeconds(4)).Subscribe(_ =>
                    {
                        EffectManager.Instance.ReleaseInstance(this);
                        __stopDiposable = null;
                    }).AddTo(this);
                }
            }
        }

        public void SetScale(Vector3 newScale, ParticleSystemScalingMode scalingMode = ParticleSystemScalingMode.Local)
        {
            if (__particleSystem != null || TryGetComponent(out __particleSystem) && __particleSystem.main.scalingMode != scalingMode)
            { 
                __defaultScaleMode = __particleSystem.main.scalingMode;
                var mainModule = __particleSystem.main;
                mainModule.scalingMode = scalingMode;
            }

            transform.localScale = newScale;
        }

        public void ResetScale()
        {
            if (__particleSystem != null || TryGetComponent<ParticleSystem>(out __particleSystem))
            {
                var mainModule = __particleSystem.main; 
                mainModule.scalingMode = __defaultScaleMode;
            }

            transform.localScale = Vector3.one;
        }
    }

    public class EffectManager : MonoSingleton<EffectManager>
    {
        /// <summary>
        /// Pooling을 이용하지 않는 경우 (한번 쓰고 버림)
        /// </summary>
        public GameObject ShowAndForget(string effectName, Vector3 position, Quaternion rotation, Vector3 scale, ParticleSystemScalingMode scalingMode = ParticleSystemScalingMode.Local)
        {
            var instance = Instantiate(Resources.Load<GameObject>($"FX/{effectName}"), position, rotation);
            if (scale != Vector3.zero && instance.TryGetComponent<ParticleSystem>(out var particleSystem))
            {
                if (particleSystem.main.scalingMode != scalingMode)
                {
                    var mainModule = particleSystem.main;
                    mainModule.scalingMode = scalingMode;
                }

                instance.transform.localScale = scale;
            }

            return instance;
        }

        public EffectInstance ShowLooping(string effectName, Vector3 position, Quaternion rotation, Vector3 scale, float waitingTime = 0f, ParticleSystemScalingMode scalingMode = ParticleSystemScalingMode.Local)
        {
            var instance = GetInstance(effectName, position, rotation, scale, scalingMode);
            if (waitingTime > 0)
            {
                instance.gameObject.SetActive(false);
                Observable.Timer(TimeSpan.FromSeconds(waitingTime)).Subscribe(_ =>
                {
                    instance.gameObject.SetActive(true);
                    instance.PlayLooping();
                }).AddTo(instance);
            }
            else
            {
                instance.PlayLooping();
            }

            return instance;
        }

        public EffectInstance Show(string effectName, Vector3 position, Quaternion rotation, Vector3 scale, float duration = -1f, float waitingTime = 0f, ParticleSystemScalingMode scalingMode = ParticleSystemScalingMode.Local)
        {
            var instance = GetInstance(effectName, position, rotation, scale, scalingMode);
            if (waitingTime > 0f)
            {
                instance.gameObject.SetActive(false);
                
                Observable.Timer(TimeSpan.FromSeconds(waitingTime)).Subscribe(_ => { 
                    instance.gameObject.SetActive(true); 
                    instance.Play(duration); 
                }).AddTo(instance);
            }
            else
            {
                instance.Play(duration);
            }

            return instance;
        }

        public EffectInstance ShowLooping(GameObject sourcePrefab, Vector3 position, Quaternion rotation, Vector3 scale, float waitingTime = 0f, ParticleSystemScalingMode scalingMode = ParticleSystemScalingMode.Local)
        {
            Debug.Assert(sourcePrefab != null);

            var instance = GetInstance(sourcePrefab, position, rotation, scale, scalingMode);
            if (waitingTime > 0f)
                Observable.Timer(TimeSpan.FromSeconds(waitingTime)).Subscribe(_ => instance.PlayLooping()).AddTo(instance);
            else
                instance.PlayLooping();

            return instance;
        }

        public EffectInstance Show(GameObject sourcePrefab, Vector3 position, Quaternion rotation, Vector3 scale, float duration = -1f, float waitingTime = 0f, ParticleSystemScalingMode scalingMode = ParticleSystemScalingMode.Local)
        {
            Debug.Assert(sourcePrefab != null);

            var instance = GetInstance(sourcePrefab, position, rotation, scale, scalingMode);
            if (waitingTime > 0f)
            {
                instance.gameObject.SetActive(false);
                Observable.Timer(TimeSpan.FromSeconds(waitingTime)).Subscribe(_ =>
                {
                    instance.gameObject.SetActive(true);
                    instance.Play(duration);
                }).AddTo(instance);
            }
            else
            {
                instance.Play(duration);
            }

            return instance;
        }

        EffectInstance GetInstance(string effectName, Vector3 position, Quaternion rotation, Vector3 scale, ParticleSystemScalingMode scalingMode)
        {
            if (!__instancePoolA.ContainsKey(effectName))
                __instancePoolA.Add(effectName, new());

            EffectInstance instance;
            if (__instancePoolA[effectName].Count > 0)
            {
                instance = __instancePoolA[effectName].First();
                __instancePoolA[effectName].Remove(instance);
            }
            else
            {
                instance = Instantiate(Resources.Load<GameObject>($"FX/{effectName}"), position, rotation).AddComponent<EffectInstance>();
                instance.effectName = effectName;

                //* (성능 이슈로 인해서..) Light는 기본적으로 끔
                foreach (var d in instance.gameObject.DescendantsAndSelf())
                {
                    if (d.TryGetComponent<Light>(out var light))
                        light.gameObject.SetActive(false);
                }
            }

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetScale(scale, scalingMode);
            instance.gameObject.SetActive(true);

            return instance;
        }

        EffectInstance GetInstance(GameObject sourcePrefab, Vector3 position, Quaternion rotation, Vector3 scale, ParticleSystemScalingMode scalingMode)
        {
            if (!__instancePoolB.ContainsKey(sourcePrefab))
                __instancePoolB.Add(sourcePrefab, new());

            EffectInstance instance;
            if (__instancePoolB[sourcePrefab].Count > 0)
            {
                instance = __instancePoolB[sourcePrefab].First();
                __instancePoolB[sourcePrefab].Remove(instance);
            }
            else
            {
                instance = Instantiate(sourcePrefab, position, rotation).AddComponent<EffectInstance>();
                instance.effectName = string.Empty;
                instance.sourcePrefab = sourcePrefab;

                //* (성능 이슈로 인해서..) Light는 기본적으로 끔
                foreach (var d in instance.gameObject.DescendantsAndSelf())
                {
                    if (d.TryGetComponent<Light>(out var light))
                        light.gameObject.SetActive(false);
                }
            }

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.SetScale(scale, scalingMode);
            instance.gameObject.SetActive(true);

            return instance;
        }

        public void ReleaseInstance(EffectInstance effectInstance)
        {
            effectInstance.gameObject.SetActive(false);
            effectInstance.transform.SetParent(transform);
            effectInstance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            effectInstance.ResetScale();

            if (effectInstance.sourcePrefab != null)
            {
                __instancePoolB[effectInstance.sourcePrefab].Add(effectInstance);
            }
            else
            {
                Debug.Assert(effectInstance.effectName != string.Empty);
                __instancePoolA[effectInstance.effectName].Add(effectInstance);
            }
        }

        readonly Dictionary<string, HashSet<EffectInstance>> __instancePoolA = new();
        readonly Dictionary<GameObject, HashSet<EffectInstance>> __instancePoolB = new();
    }
}
