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
    public class HeroWandProjectile : HeroProjectile
    {

        /// <summary>
        /// 
        /// </summary>
        public float forwardSpeed = 1;

        /// <summary>
        /// 
        /// </summary>
        public float forwardDecel = 1;

        /// <summary>
        /// 
        /// </summary>
        public float floatingHeight = 0.1f;

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
        public int splitCount;

        /// <summary>
        /// 
        /// </summary>
        public Transform targetPoint;

        /// <summary>
        /// 
        /// </summary>
        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            __guideCollider = gameObject.Descendants().First(d => d.name == "GuideSensor").GetComponent<Collider>();
            __guideCollider.enabled = false;
        }

        Collider __guideCollider;

        /// <summary>
        /// 
        /// </summary>
        protected override void StartInternal()
        {
            base.StartInternal();

            emitterBrain.Where(v => v != null).Subscribe(v =>
            {
                heroBrain = v.GetComponent<HeroBrain>();
            }).AddTo(this);

            if (targetPoint == null)
            {
                __guideCollider.OnTriggerEnterAsObservable().TakeWhile(_ => targetPoint == null).Subscribe(c =>
                {
                    // var targetBrain = c.GetComponent<JellySlimeBrain>();

                    // if (targetBrain != null && !targetBrain.PawnHP.CheckDamageHistory(EmitterBrain.gameObject, checkDamageDeltaTime, "Attack"))
                    // {
                    //     targetPoint = targetBrain.transform;
                    //     __guideCollider.enabled = false;
                    // }
                }).AddTo(this);

                __guideCollider.enabled = true;
            }

            onHitSomething += (obj) =>
            {
                // var hitBrain = obj.GetComponent<JellySlimeBrain>();

                // if (hitBrain != null)
                // {
                //     //! 데미지 디스패칭 전에 어그로를 먼저 처리해야함
                //     foreach (var s in EmitterBrain.SensorCtrler.ListeningObjects)
                //     {
                //         if (s != null && s.TryGetComponent<JellySlimeBrain>(out var jellyBrain))
                //             jellyBrain.IncreaseAggro(EmitterBrain.gameObject, 1, 0.5f);
                //     }

                //     hitBrain.PawnHP.Send(new PawnHeartPointDispatcherEx.DamageContext(this, hitBrain.gameObject, executeBurst ? "Attack" : "Burst", 1, velocity.Vector2D().normalized));

                //     if (splitCount < 2)
                //     {
                //         var targets = EmitterBrain.ActionCtrler.CollectActionTargetBrains<JellySlimeBrain>(hitBrain.transform.position, Vector3.forward, 0, 2, 180, 1, 2, true)
                //             .Where(t => t.transform != hitBrain.transform && !t.PawnHP.CheckDamageHistory(EmitterBrain.gameObject, checkDamageDeltaTime, "Attack"))
                //             .ToArray();

                //         foreach (var t in targets)
                //             EmitterBrain.WeaponCtrler.SpawnWandProjectile(transform.position, (t.transform.position - transform.position).Vector2D().normalized, splitCount + 1, t.transform, executeBurst);
                //     }

                //     Stop(true);
                // }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnUpdateHandler()
        {
            base.OnUpdateHandler();

            if (targetPoint != null)
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation((targetPoint.position - transform.position).Vector2D().normalized), Time.deltaTime * 720);

            var velocity2D = transform.forward.Vector2D().normalized * Mathf.Max(0, velocity.Vector2D().magnitude - forwardDecel * Time.deltaTime);

            velocity = velocity.AdjustXZ(velocity2D.x, velocity2D.z);

            transform.position += velocity * Time.deltaTime;
            transform.position = transform.position.AdjustY(TerrainManager.GetTerrainPoint(transform.position).y + floatingHeight * (1 + Perlin.Noise((Time.time - __moveStartTimeStamp) * 1.2f) * 0.2f));
        }

    }

}