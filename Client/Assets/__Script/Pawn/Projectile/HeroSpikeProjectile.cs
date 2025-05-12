using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using Unity.Linq;
using UnityEngine;

namespace Game
{

    /// <summary>
    /// 
    /// </summary>
    public class HeroSpikeProjectile : HeroProjectile
    {
        
        /// <summary>
        /// 
        /// </summary>
        public int maxHitCount = 1;

        /// <summary>
        /// 
        /// </summary>
        public float spikeDistance = 1;

        /// <summary>
        /// 
        /// </summary>
        public float spikeDuration = 1;

        /// <summary>
        /// 
        /// </summary>
        public float actionPowerAttenRate;

        /// <summary>
        /// 
        /// </summary>
        public float actionPowerAttenTime;

        /// <summary>
        /// 
        /// </summary>
        public float checkDamageDeltaTime = 1;

        /// <summary>
        /// 
        /// </summary>
        public bool executeBurst;

        /// <summary>
        /// 
        /// </summary>
        public MeshRenderer spikeMeshRenderer;

        /// <summary>
        /// 
        /// </summary>
        protected override void StartInternal()
        {
            base.StartInternal();

            emitterBrain.Subscribe(v =>
            {
                heroBrain = emitterBrain.Value.GetComponent<SlayerBrain>();
                __startPoint = transform.position;
                __endPoint = transform.position + transform.forward.Vector2D().normalized * spikeDistance;
            });

            __startPoint = transform.position;
            __endPoint = transform.position + transform.forward.Vector2D().normalized * spikeDistance;

            onHitSomething += (obj) =>
            {
                if (obj == emitterBrain.Value || __currHitCount >= maxHitCount)
                    return;

                // var hitBrain = obj.GetComponent<JellySlimeBrain>();

                // if (hitBrain != null && !hitBrain.PawnHP.CheckDamageHistory(EmitterBrain.gameObject, checkDamageDeltaTime, "Attack"))
                // {
                //     //! 데미지 디스패칭 전에 어그로를 먼저 처리해야함
                //     if (!__aggroIncreased)
                //     {
                //         foreach (var s in EmitterBrain.SensorCtrler.ListeningObjects)
                //         {
                //             if (s != null && s.TryGetComponent<JellySlimeBrain>(out var jellyBrain))
                //                 jellyBrain.IncreaseAggro(EmitterBrain.gameObject, 1, 0.5f);
                //         }
                        
                //         __aggroIncreased = true;
                //     }

                //     hitBrain.PawnHP.Send(new PawnHeartPointDispatcherEx.DamageContext(this, obj, executeBurst ? "Attack" : "Burst", checkDamageDeltaTime, (hitBrain.transform.position - transform.position).Vector2D().normalized));

                //     __currHitCount++;
                // }
            };
        }

        int __currHitCount;

        /// <summary>
        /// 
        /// </summary>
        public bool CheckActionPownerAttenuated()
        {
            return __elapsedTime > actionPowerAttenTime;
        }

        bool __aggroIncreased;
        float __elapsedTime;
        Vector3 __startPoint;
        Vector3 __endPoint;

        /// <summary>
        /// 
        /// </summary>
        protected override void OnUpdateHandler()
        {
            base.OnUpdateHandler();

            __elapsedTime += Time.deltaTime;

            // if (emitter.Value != null)
            // {
            //     var normTime = __elapsedTime < spikeDuration * 0.5f ? __elapsedTime : spikeDuration - __elapsedTime;
            //     var t = EaseOutExpo(0, 1, Mathf.Clamp01(normTime / (spikeDuration * 0.5f)));

            //     transform.position = Vector3.Lerp(EmitterBrain?.WeaponCtrler.SpikeMesh.transform.position ?? __startPoint, __endPoint, t);
            //     transform.position = transform.position.AdjustY(TerrainManager.GetTerrainPoint(transform.position).y) + Vector3.up * 0.4f;

            //     spikeMeshRenderer.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1, Mathf.Clamp01(5 * normTime / (spikeDuration * 0.5f)));
            //     spikeMeshRenderer.transform.localRotation *= Quaternion.Euler(360 * Time.deltaTime, 180 * Time.deltaTime, 90 * Time.deltaTime);
            // }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        float EaseOutExpo(float start, float end, float value)
        {
            end -= start;
            return end * (-Mathf.Pow(2, -10 * value / 1) + 1) + start;
        }

    }

}
