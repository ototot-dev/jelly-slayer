using System;
using System.Collections.Generic;
using NodeCanvas.Framework.Internal;
using Obi;
using UniRx;
using UnityEngine;
using XftWeapon;

namespace Game
{
    public class Etasphera42_ActionController : JellyQuadWalkActionController
    {
        [Header("Component")]
        public PawnColliderHelper hookingPointColliderHelper;

        [Header("Parameter")]
        public float leapRootMotionDistance = 7f;
        public float leapRootMotionMultiplier = 1f;

        public override IDisposable StartOnHitAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                var hitColliderHelper = damageContext.hitCollider.GetComponent<PawnColliderHelper>();
                ShowHitColor(hitColliderHelper);

                if (damageContext.senderActionData.actionName.StartsWith("Kick"))
                    EffectManager.Instance.Show(__brain.BB.graphics.onKickHitFx, hitColliderHelper.GetWorldCenter(), Quaternion.identity, Vector3.one, 1f);
                else if (damageContext.senderActionData.actionName.StartsWith("Heavy"))
                    EffectManager.Instance.Show(__brain.BB.graphics.onBigHitFx, hitColliderHelper.GetWorldCenter(), Quaternion.LookRotation(damageContext.hitPoint - __brain.bodyHitColliderHelper.GetWorldCenter()) * Quaternion.Euler(90f, 0f, 0f), Vector3.one, 1f);
                else
                    EffectManager.Instance.Show(__brain.BB.graphics.onHitFx, hitColliderHelper.GetWorldCenter(), Quaternion.LookRotation(damageContext.hitPoint - __brain.bodyHitColliderHelper.GetWorldCenter()) * Quaternion.Euler(90f, 0f, 0f), Vector3.one, 1f);

                SoundManager.Instance.Play(SoundID.HIT_FLESH);
            }
            else if (damageContext.actionResult == ActionResults.Missed)
            {
                SoundManager.Instance.Play(SoundID.HIT_BLOCK);
                // EffectManager.Instance.Show("@Hit 4 yellow arrow", __brain.AnimCtrler.shieldMeshSlot.position, Quaternion.identity, Vector3.one, 1f);
            }
            else if (damageContext.actionResult == ActionResults.Blocked)
            {
                __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", true);
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnGuard");
                
                Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ => __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", false)).AddTo(this);
                // Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ => EffectManager.Instance.Show(__brain.BB.graphics.onBlockedFx, GetShieldCenter(), Quaternion.identity, Vector3.one, 1f)).AddTo(this);
                SoundManager.Instance.Play(SoundID.HIT_BLOCK);

            }
            else if (damageContext.actionResult == ActionResults.GuardBreak) 
            {
                // Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ => EffectManager.Instance.Show(__brain.BB.graphics.onGuardBreakFx, GetShieldCenter(), Quaternion.identity, Vector3.one, 1f)).AddTo(this);
                SoundManager.Instance.Play(SoundID.GUARD_BREAK);
            }

            return base.StartOnHitAction(ref damageContext, isAddictiveAction);
        }

        public override IDisposable StartOnKnockDownAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                SoundManager.Instance.Play(SoundID.HIT_FLESH);
                EffectManager.Instance.Show("@Hit 23 cube", damageContext.hitPoint, Quaternion.identity, Vector3.one, 1);
                EffectManager.Instance.Show("@BloodFX_impact_col", damageContext.hitPoint, Quaternion.identity, 1.5f * Vector3.one, 3);
            }

            return base.StartOnKnockDownAction(ref damageContext, isAddictiveAction);
        }

        void ShowHitColor(PawnColliderHelper colliderHelper)
        {
            var hitBoxIndex = Etasphera42_Brain.HitBoxIndices.Max;
            if (colliderHelper.name.EndsWith("_pelves") || colliderHelper.name.EndsWith("_body"))
            {
                hitBoxIndex = Etasphera42_Brain.HitBoxIndices.Body;
                if (!__hitColorRenderers.ContainsKey(Etasphera42_Brain.HitBoxIndices.Body))
                {
                    __hitColorRenderers.Add(Etasphera42_Brain.HitBoxIndices.Body, __brain.BB.graphics.body_meshRenderers);
                    __hitColorDisposables.Add(Etasphera42_Brain.HitBoxIndices.Body, null);
                }
            }
            else if (colliderHelper.name.EndsWith("_1_l"))
            {
                hitBoxIndex = Etasphera42_Brain.HitBoxIndices.LeftLeg1;
                if (!__hitColorRenderers.ContainsKey(Etasphera42_Brain.HitBoxIndices.LeftLeg1))
                {
                    __hitColorRenderers.Add(Etasphera42_Brain.HitBoxIndices.LeftLeg1, __brain.BB.graphics.leftLeg1_meshRenderes);
                    __hitColorDisposables.Add(Etasphera42_Brain.HitBoxIndices.LeftLeg1, null);
                }
            }
            else if (colliderHelper.name.EndsWith("_2_l"))
            {
                hitBoxIndex = Etasphera42_Brain.HitBoxIndices.LeftLeg2;
                if (!__hitColorRenderers.ContainsKey(Etasphera42_Brain.HitBoxIndices.LeftLeg2))
                {
                    __hitColorRenderers.Add(Etasphera42_Brain.HitBoxIndices.LeftLeg2, __brain.BB.graphics.leftLeg2_meshRenderes);
                    __hitColorDisposables.Add(Etasphera42_Brain.HitBoxIndices.LeftLeg2, null);
                }
            }
            else if (colliderHelper.name.EndsWith("_1_r"))
            {
                hitBoxIndex = Etasphera42_Brain.HitBoxIndices.RightLeg1;
                if (!__hitColorRenderers.ContainsKey(Etasphera42_Brain.HitBoxIndices.RightLeg1))
                {
                    __hitColorRenderers.Add(Etasphera42_Brain.HitBoxIndices.RightLeg1, __brain.BB.graphics.rightLeg1_meshRenderes);
                    __hitColorDisposables.Add(Etasphera42_Brain.HitBoxIndices.RightLeg1, null);
                }
            }
            else if (colliderHelper.name.EndsWith("_2_r"))
            {
                hitBoxIndex = Etasphera42_Brain.HitBoxIndices.RightLeg2;
                if (!__hitColorRenderers.ContainsKey(Etasphera42_Brain.HitBoxIndices.RightLeg2))
                {
                    __hitColorRenderers.Add(Etasphera42_Brain.HitBoxIndices.RightLeg2, __brain.BB.graphics.rightLeg2_meshRenderes);
                    __hitColorDisposables.Add(Etasphera42_Brain.HitBoxIndices.RightLeg2, null);
                }
            }
            else
            {
                Debug.Assert(false);
            }

            foreach (var r in __hitColorRenderers[hitBoxIndex])
                r.materials = new Material[] { r.material, new(__brain.BB.graphics.hitColor) };

            __hitColorDisposables[hitBoxIndex]?.Dispose();
            __hitColorDisposables[hitBoxIndex] = Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(_ => 
            {
                __hitColorDisposables[hitBoxIndex] = null;
                foreach (var r in __hitColorRenderers[hitBoxIndex])
                    r.materials = new Material[] { r.materials[0] };
            }).AddTo(this);
        }

        readonly Dictionary<Etasphera42_Brain.HitBoxIndices, SkinnedMeshRenderer[]> __hitColorRenderers = new();
        readonly Dictionary<Etasphera42_Brain.HitBoxIndices, IDisposable> __hitColorDisposables = new();
        Etasphera42_Brain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<Etasphera42_Brain>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            __brain.BB.action.isGuarding.Subscribe(v =>
            {
                __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", v);
            }).AddTo(this);
        }

        public override void EmitProjectile(GameObject sourcePrefab, Transform emitPoint, int emitNum)
        {
            if (sourcePrefab == __brain.BB.action.bulletPrefab)
                ProjectilePoolingSystem.Instance.GetProjectile<Etasphera42_Bullet>(sourcePrefab, emitPoint.position + UnityEngine.Random.Range(-0.2f, 0.2f) * Vector3.right, emitPoint.rotation).Go(__brain, 20f, 0.5f);
        }
    }
}