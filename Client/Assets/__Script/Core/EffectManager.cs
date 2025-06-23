using System;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{
    public class EffectInstance : MonoBehaviour
    {
        // void OnParticleCollision(GameObject other)
        // {
        //     __collisionEvents ??= new();

        //     int eventNum = __particleSystem.GetCollisionEvents(other, __collisionEvents);
        //     for (int i = 0; i < eventNum; i++)
        //         onCollisionEvent?.Invoke(__collisionEvents[i]);
        // }
        // List<ParticleCollisionEvent> __collisionEvents;
        // public Action<ParticleCollisionEvent> onCollisionEvent;
        
        ParticleSystemScalingMode __defaultScaleMode;

        public void PlayLooping(float playRate = 1f)
        {
            Debug.Assert(__stopDiposable == null);
            
            if (__particleSystem != null || TryGetComponent<ParticleSystem>(out __particleSystem))
            {
                var mainModule = __particleSystem.main;
                
                mainModule.loop = true;
                mainModule.simulationSpeed = playRate;
                __particleSystem.Play();

                //* 음수면 재생 시간은 무한대, 즉 Looping임을 뜻함
                PlayDuration = -1f;
            }
        }

        public void Play(float duration, float playRate = 1f, bool releaseInstance = true)
        {
            Debug.Assert(__stopDiposable == null);

            if (__particleSystem != null || TryGetComponent<ParticleSystem>(out __particleSystem))
            {
                var mainModule = __particleSystem.main;

                mainModule.loop = false;
                mainModule.simulationSpeed = playRate;
                __particleSystem.Play();

                PlayDuration = duration > 0 ? duration : Mathf.Max(0.1f, mainModule.duration * 0.99f);
                Observable.Timer(TimeSpan.FromSeconds(PlayDuration)).Subscribe(_ => Stop(releaseInstance)).AddTo(this);
            }
        }

        public float PlayDuration { get; private set; }
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
                    __stopDiposable = Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(_ =>
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
        public GameObject ShowAndForget(string effectName, Vector3 position, Quaternion rotation, Vector3 scale, ParticleSystemScalingMode scalingMode = ParticleSystemScalingMode.Local, float playRate = 1f)
        {
            return ShowAndForget(Resources.Load<GameObject>($"FX/{effectName}"), position, rotation, scale, scalingMode, playRate);
        }

        public GameObject ShowAndForget(GameObject sourcePrefab, Vector3 position, Quaternion rotation, Vector3 scale, ParticleSystemScalingMode scalingMode = ParticleSystemScalingMode.Local, float playRate = 1f)
        {
            var instance = Instantiate(sourcePrefab, position, rotation);

            if (scale != Vector3.zero && instance.TryGetComponent<ParticleSystem>(out var particleSystem))
            {
                if (particleSystem.main.scalingMode != scalingMode)
                {
                    var mainModule = particleSystem.main;
                    mainModule.scalingMode = scalingMode;
                    mainModule.simulationSpeed = playRate;
                }

                instance.transform.localScale = scale;
            }

            return instance;
        }

        public EffectInstance ShowLooping(string effectName, Vector3 position, Quaternion rotation, Vector3 scale, float waitingTime = 0f, ParticleSystemScalingMode scalingMode = ParticleSystemScalingMode.Local, float playRate = 1f)
        {
            var instance = GetInstance(effectName, position, rotation, scale, scalingMode);

            if (waitingTime > 0)
            {
                // instance.gameObject.SetActive(false);
                Observable.Timer(TimeSpan.FromSeconds(waitingTime)).Subscribe(_ =>
                {
                    instance.gameObject.SetActive(true);
                    instance.PlayLooping(playRate);
                }).AddTo(instance);
            }
            else
            {
                instance.gameObject.SetActive(true);
                instance.PlayLooping();
            }

            return instance;
        }

        public EffectInstance Show(string effectName, Vector3 position, Quaternion rotation, Vector3 scale, float duration = -1f, float waitingTime = 0f, ParticleSystemScalingMode scalingMode = ParticleSystemScalingMode.Local, float playRate = 1f)
        {
            var instance = GetInstance(effectName, position, rotation, scale, scalingMode);
            
            if (waitingTime > 0f)
            {
                Observable.Timer(TimeSpan.FromSeconds(waitingTime)).Subscribe(_ =>
                {
                    instance.gameObject.SetActive(true);
                    instance.Play(duration, playRate);
                }).AddTo(instance);
            }
            else
            {
                instance.gameObject.SetActive(true);
                instance.Play(duration);
            }

            return instance;
        }

        public EffectInstance ShowLooping(GameObject sourcePrefab, Vector3 position, Quaternion rotation, Vector3 scale, float waitingTime = 0f, ParticleSystemScalingMode scalingMode = ParticleSystemScalingMode.Local, float playRate = 1f)
        {
            Debug.Assert(sourcePrefab != null);

            var instance = GetInstance(sourcePrefab, position, rotation, scale, scalingMode);

            if (waitingTime > 0f)
                Observable.Timer(TimeSpan.FromSeconds(waitingTime)).Subscribe(_ => instance.PlayLooping()).AddTo(instance);
            else
                instance.PlayLooping(playRate);

            return instance;
        }

        public EffectInstance Show(GameObject sourcePrefab, Vector3 position, Quaternion rotation, Vector3 scale, float duration = -1f, float waitingTime = 0f, ParticleSystemScalingMode scalingMode = ParticleSystemScalingMode.Local, float playRate = 1f)
        {
            Debug.Assert(sourcePrefab != null);

            var instance = GetInstance(sourcePrefab, position, rotation, scale, scalingMode);

            if (waitingTime > 0f)
            {
                Observable.Timer(TimeSpan.FromSeconds(waitingTime)).Subscribe(_ =>
                {
                    instance.gameObject.SetActive(true);
                    instance.Play(duration, playRate);
                }).AddTo(instance);
            }
            else
            {
                instance.gameObject.SetActive(true);
                instance.Play(duration);
            }

            return instance;
        }

        EffectInstance GetInstance(string sourcePath, Vector3 position, Quaternion rotation, Vector3 scale, ParticleSystemScalingMode scalingMode)
        {
            var handler = ObjectPoolingSystem.Instance.GetObject<ObjectPoolableHandler>(sourcePath, position, rotation);

            if (!handler.TryGetComponent<EffectInstance>(out var ret))
                ret = handler.gameObject.AddComponent<EffectInstance>();

            //* (성능 이슈로 인해서..) Light는 기본적으로 끔
            foreach (var d in ret.gameObject.DescendantsAndSelf())
            {
                if (d.TryGetComponent<Light>(out var light))
                    light.gameObject.SetActive(false);
            }

            ret.transform.SetPositionAndRotation(position, rotation);
            ret.SetScale(scale, scalingMode);
            ret.gameObject.SetActive(true);

            return ret;
        }

        EffectInstance GetInstance(GameObject sourcePrefab, Vector3 position, Quaternion rotation, Vector3 scale, ParticleSystemScalingMode scalingMode)
        {
            var handler = ObjectPoolingSystem.Instance.GetObject<ObjectPoolableHandler>(sourcePrefab, position, rotation);

            if (!handler.TryGetComponent<EffectInstance>(out var ret))
                ret = handler.gameObject.AddComponent<EffectInstance>();

            //* (성능 이슈로 인해서..) Light는 기본적으로 끔
            foreach (var d in ret.gameObject.DescendantsAndSelf())
            {
                if (d.TryGetComponent<Light>(out var light))
                    light.gameObject.SetActive(false);
            }

            ret.transform.SetPositionAndRotation(position, rotation);
            ret.SetScale(scale, scalingMode);

            return ret;
        }

        public void ReleaseInstance(EffectInstance instance)
        {
            instance.gameObject.SetActive(false);
            instance.transform.SetParent(transform);
            instance.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            instance.ResetScale();

            ObjectPoolingSystem.Instance.ReturnObject(instance.gameObject);
        }
    }
}
