using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UniRx.Triggers;
using Unity.Linq;
using UnityEngine;

namespace Game
{

    /// <summary>
    /// 
    /// </summary>
    public class HeroSwordProjectile : HeroProjectile
    {

        /// <summary>
        /// 
        /// </summary>
        public float actionPowerAttenRadius;

        /// <summary>
        /// 
        /// </summary>
        public float actonPowerAttenMin;

        /// <summary>
        /// 
        /// </summary>
        public float impactRadius;

        /// <summary>
        /// 
        /// </summary>
        public Transform impactPoint;

        /// <summary>
        /// 
        /// </summary>
        public bool executeBurst;

        /// <summary>
        /// 
        /// </summary>
        protected override void StartInternal()
        {
            base.StartInternal();

            emitterBrain.Where(v => v != null).Subscribe(v =>
            {
                heroBrain = v.GetComponent<SlayerBrain>();
            });
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnUpdateHandler()
        {
            base.OnUpdateHandler();

            if (emitterBrain.Value != null && !IsDespawnPending)
            {
                velocity += gravity * Time.deltaTime;
                transform.position += velocity * Time.deltaTime;

                //* 땅을 내리치는 순간을 체크
                if (impactPoint.position.y < TerrainManager.GetTerrainHitPoint(impactPoint.position).point.y)
                {
                    // var targets = EmitterBrain.ActionCtrler.TraceActionTargets(impactPoint.position, Vector3.forward, 0, impactRadius, 360, 1, -1, true);

                    // //! 데미지 디스패칭 전에 어그로를 먼저 처리해야함
                    // foreach (var s in EmitterBrain.SensorCtrler.ListeningObjects)
                    // {
                    //     if (s != null && s.TryGetComponent<JellySlimeBrain>(out var jellyBrain) && !jellyBrain.BB.IsDead)
                    //         jellyBrain.IncreaseAggro(EmitterBrain.gameObject, targets.Count > 0 ? 1 : 0.1f, 0.5f);
                    // }

                    // JellySlimeBrain prevBrain = null;

                    // foreach (var t in targets)
                    // {
                    //     if (t.TryGetComponent<JellySlimeBrain>(out var jellyBrain))
                    //     {
                    //         if (!jellyBrain.BB.IsDead)
                    //         {
                    //             var actionForward = (jellyBrain.transform.position - transform.position).Vector2D().normalized;

                    //             if (Vector3.Dot(actionForward, EmitterBrain.transform.forward.Vector2D().normalized) < 0)
                    //                 actionForward = (jellyBrain.transform.position - EmitterBrain.transform.position).Vector2D().normalized;

                    //             EmitterBrain.PawnHP.Send(new PawnHeartPointDispatcherEx.DamageContext(this, jellyBrain.gameObject, executeBurst ? "Burst" : "Attack", 1, actionForward));

                    //             prevBrain = jellyBrain;
                    //         }
                    //     }
                    //     else if (t.TryGetComponent<JellyFloatingProjectile>(out var projectile))
                    //     {
                    //         projectile.Hit((projectile.transform.position - EmitterBrain.transform.position).Vector2D().normalized * 15, 20, 0.2f);
                    //     }
                    //     else
                    //     {
                    //         __Logger.LogF(gameObject, nameof(OnUpdateHandler), "target is unknown", nameof(t), t);
                    //     }
                    // }

                    // var objRes = Resources.Load("ParticleFx/FX_white-yellow bolder");
                    // //var objEff = Instantiate(objRes, impactPoint.position + Vector3.up * 0.1f, Quaternion.identity);
                    // //EffectManager.Instance.Show("FX_white-yellow bolder", impactPoint.position + Vector3.up * 0.1f, Quaternion.identity, 0.5f);

                    Stop(true);
                }
            }
        }

    }

}
