using System;
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
    public class HeroBombProjectile : HeroProjectile
    {

        /// <summary>
        /// 
        /// </summary>
        public float impulse = 1;

        /// <summary>
        /// 
        /// </summary>
        public float impulsePitch = -1;

        /// <summary>
        /// 
        /// </summary>
        public float brakeScale = 1;

        /// <summary>
        /// 
        /// </summary>
        public float brakingEnabledTime = 0.1f;

        /// <summary>
        /// 
        /// </summary>
        public float explosionRange = 1;

        /// <summary>
        /// 
        /// </summary>
        public int bombPower = 1;

        /// <summary>
        /// 
        /// </summary>
        public SpriteRenderer spriteRenderer;

        /// <summary>
        /// 
        /// </summary>
        protected override void StartInternal()
        {
            base.StartInternal();

            // meshRenderer.material.SetColor("__tintColor", Color.red);
            // meshRenderer.material.SetFloat("__tintStartTime", Time.timeSinceLevelLoad);
            // meshRenderer.material.SetFloat("__tintFrequency", 2);

            emitterBrain.Where(v => v != null).Subscribe(v => heroBrain = v.GetComponent<HeroBrain>()).AddTo(this);

            onHitSomething += (obj) =>
            {
            //     if (obj.TryGetComponent<JellySlimeBrain>(out var jellyBrain))
            //     {
            //         Explode();
            //         Stop(true);
            //     }
            };

            onLifeTimeOut += () =>
            {
                Explode();
            };
        }

        HashSet<GameObject> __explosionTargets = new HashSet<GameObject>();

        /// <summary>
        /// 
        /// </summary>
        void Explode()
        {
            Debug.Assert(emitterBrain.Value != null);

            foreach (var c in Physics.OverlapSphere(transform.position, explosionRange, LayerMask.GetMask("Jelly")))
            {
                if (c == null || c == emitterBrain.Value)
                    continue;

                // if (c.TryGetComponent<JellySlimeBrain>(out var jellyBrain))
                // {
                //     //* 1kg 이하는 분열되지 않음
                //     if (jellyBrain.PawnHP.heartPoint.Value >= 1000 && jellyBrain.BB.body.bondingStrength.Value > 0)
                //     {
                //         var delta = bombPower / (float)Mathf.Sqrt(jellyBrain.PawnHP.heartPoint.Value);

                //         jellyBrain.BB.body.bondingStrength.Value = Mathf.Max(0, jellyBrain.BB.body.bondingStrength.Value - delta);
                //         jellyBrain.BB.body.bondingStrength.Value = 0;

                //         if (jellyBrain.BB.body.bondingStrength.Value > 0)
                //         {
                //             jellyBrain.IncreaseAggro(emitter.Value, 1, 0.5f);
                //             jellyBrain.AnimCtrler.Shake(10);
                //         }
                //     }

                //     // __Logger.Log(gameObject, nameof(jellyBrain), jellyBrain, nameof(jellyBrain.BB.body.bondingStrength), jellyBrain.BB.body.bondingStrength, nameof(delta), delta);
                // }
            }

            EffectManager.Instance.Show("CFXR Explosion 2", transform.position + Vector3.up * 0.2f, Quaternion.identity, 0.5f * Vector3.one);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnUpdateHandler()
        {
            base.OnUpdateHandler();

            if ((Time.time - __moveStartTimeStamp) > sensorEnabledTime && !BodyCollider.enabled)
                BodyCollider.enabled = true;

            if (spriteRenderer != null && GameContext.Instance.cameraCtrler != null)
            {
                spriteRenderer.transform.rotation = GameContext.Instance.cameraCtrler.SpriteLookRotation;
                spriteRenderer.transform.rotation *= Quaternion.Euler(0, 0, Perlin.Noise(Time.time + __moveStartTimeStamp) * 30);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnFixedUpdateHandler()
        {
            base.OnFixedUpdateHandler();

            if (Time.time - __moveStartTimeStamp > brakingEnabledTime)
            {
                var velocity2D = __rigidBody.linearVelocity.Vector2D();
                __rigidBody.linearVelocity = velocity2D.normalized * Mathf.Max(0, velocity2D.magnitude - brakeScale * Time.deltaTime) + Vector3.up * __rigidBody.linearVelocity.y;
            }
        }

    }

}