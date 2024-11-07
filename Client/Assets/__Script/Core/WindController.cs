using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    public class WindController : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public GameObject[] windEffectPrefabs;

        /// <summary>
        /// 
        /// </summary>
        public HashSet<GameObject> windEffectZones = new HashSet<GameObject>();

        /// <summary>
        /// 
        /// </summary>
        public float windBlowInterval = 60;

        /// <summary>
        /// 
        /// </summary>
        public float windDuration = 10;

        /// <summary>
        /// 
        /// </summary>
        public float WindStrength { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public Vector3 WindVec { get; private set; } = Vector3.forward;

        /// <summary>
        /// 
        /// </summary>
        public Vector3 CurrWindForce { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        const float __spawnWindFxTime = 3;

        /// <summary>
        /// 
        /// </summary>
        const float __despawnWindFxTime = 2;

        void Start()
        {
            //? 바람 효과 일시 중지
            return;

            Debug.Assert(windDuration > __despawnWindFxTime);

            var windBlowEnabled = new BoolReactiveProperty();

            windBlowEnabled.Where(v => v).Subscribe(_ => 
            {
                // Zone 별로 Wind 이펙트가 적용될지 결정
                foreach (var g in GameContext.Instance.terrainManager.zoneMeshGenerators)
                {
                    if (UnityEngine.Random.Range(0, 6) != 0)
                        continue;

                    windEffectZones.Add(g.gameObject);

                    Debug.Log($"2?? windEffectZones.Add(g.gameObject) => {g.gameObject.name}");
                }

                //* Wind 방향'만' 셋팅한다. 
                //* 'windBlowTimeLeft' 값이 0이 되어야 'WindStrength' 값이 갱신되면서 비로서 Wind가 활성화된다.
                WindVec = Quaternion.Euler(0, UnityEngine.Random.Range(0f, 360f), 0) * WindVec;

                // Wind 이펙트를 출력
                Observable.Interval(TimeSpan.FromSeconds(0.1f))
                    .TakeWhile(_ => windBlowEnabled.Value)
                    .Subscribe(_ => SpawnWindEffect(2 + UnityEngine.Random.Range(0, 0.2f)))
                    .AddTo(this);

                // __despawnWindFxTime (2초) 정도 미리 중지해야 자연스럽다. 
                // 요렇게 하지 않으면 바람에 의한 밀림은 끝났는데 계속 바람이 부는 것처럼 느껴진다.
                Observable.Timer(TimeSpan.FromSeconds(windDuration - __despawnWindFxTime))
                    .Subscribe(_ => windBlowEnabled.Value = false)
                    .AddTo(this);
            }).AddTo(this);

            Debug.Assert(windBlowInterval > __spawnWindFxTime);

            var windBlowTimeLeft = new FloatReactiveProperty(windBlowInterval);

            windBlowTimeLeft.Subscribe(v =>
            {
                Func<float, float, bool> approximately = (float a, float b) => { return Mathf.Abs(a - b) < 0.001f; };

                // __spawnWindFxTime (3초) 정도 미리 Fx 출력을 시작해야 자연스럽다.
                if (approximately(windBlowTimeLeft.Value, __spawnWindFxTime))
                {
                    windBlowEnabled.Value = true;
                }
                else if (approximately(windBlowTimeLeft.Value, 0))
                {
                    WindStrength = UnityEngine.Random.Range(1f, 3f);

                    Observable.Timer(TimeSpan.FromSeconds(windDuration)).Subscribe(_ =>
                    {
                        WindStrength = 0;
                        windBlowTimeLeft.Value = windBlowInterval;
                        windEffectZones.Clear();
                    }).AddTo(this);
                }
            }).AddTo(this);

            Observable.Interval(TimeSpan.FromSeconds(0.2f)).Subscribe(_ =>
            {
                if (WindStrength == 0)
                {   
                    // __spawnWindFxTime 보다 작게되면 연출 싱크를 맞추기 위해 뒷 쪽의 랜덤 팩터는 무시한다.
                    if (windBlowTimeLeft.Value <= __spawnWindFxTime || UnityEngine.Random.Range(0, 6) == 0)
                        windBlowTimeLeft.Value -= 0.2f;
                }
            }).AddTo(this);

            Observable.EveryUpdate().Subscribe(_ =>
            {
                CurrWindForce = WindVec;
                CurrWindForce *= Mathf.Max(0.5f, WindStrength * Perlin.Noise(Time.realtimeSinceStartup * 1.3334f)) + Perlin.Noise(Time.realtimeSinceStartup * 3.3334f) * 0.1f;
            }).AddTo(this);
        }
        
        float __spawnWindFxTimeStamp;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lifeTime"></param>
        void SpawnWindEffect(float lifeTime)
        {
            if (Time.realtimeSinceStartup - __spawnWindFxTimeStamp < 0.5f)
                return;

            __spawnWindFxTimeStamp = Time.realtimeSinceStartup;

            Debug.Assert(GameContext.Instance.cameraCtrler != null);

            var spawnPosition = GameContext.Instance.cameraCtrler.GetTerrainHitPoint().point;

            spawnPosition += Vector3.right * UnityEngine.Random.Range(-5f, 5f);
            spawnPosition += Vector3.forward * UnityEngine.Random.Range(-5f, 5f);

            // Wind가 활성화된 Zone인지 체크
            if (!windEffectZones.Contains(GameContext.Instance.terrainManager.GetTerrainZone(spawnPosition)))
                return;

            // 바람의 길이를 고려해서 바람 방향의 반대쪽으로 위치를 이동시킨다.
            // 요렇게 하면 바람이 화면에 꽉차게 출력된다.
            spawnPosition -= WindVec * 2f;

            var hitPoint = TerrainManager.GetTerrainHitPoint(spawnPosition);

            if (hitPoint.collider != null)
                spawnPosition = hitPoint.point + Vector3.up;

            var windFx = GetWindEffectFromPool(spawnPosition, Quaternion.LookRotation(WindVec));

            __spawnedWindEffects.Add(windFx);

            Observable.Timer(TimeSpan.FromSeconds(lifeTime)).Subscribe(_ =>
            {
                var particle = windFx.GetComponent<ParticleSystem>();

                particle.Stop();

                Observable.Timer(TimeSpan.FromSeconds(particle.main.duration * 2)).Subscribe(_ =>
                {
                    __spawnedWindEffects.Remove(windFx);
                    __windEffectPool.Enqueue(windFx);
                }).AddTo(this);
            }).AddTo(this);
        }

        HashSet<GameObject> __spawnedWindEffects = new HashSet<GameObject>();
        Queue<GameObject> __windEffectPool = new Queue<GameObject>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        GameObject GetWindEffectFromPool(Vector3 position, Quaternion rotation)
        {
            if (__windEffectPool.Count == 0)
            {
                return Instantiate<GameObject>(windEffectPrefabs[0], position, rotation);
            }
            else
            {
                var ret = __windEffectPool.Dequeue();

                ret.transform.position = position;
                ret.transform.rotation = rotation;

                ret.GetComponent<ParticleSystem>().Play();

                return ret;
            }
        }
    }
}
