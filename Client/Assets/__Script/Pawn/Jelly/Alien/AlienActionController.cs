using System;
using UniRx;
using UnityEngine;
using XftWeapon;

namespace Game
{
    public class AlienActionController : JellyHumanoidActionController
    {
        [Header("Component")]
        public Transform counterActionCollider;
        public XWeaponTrail leftKnifeTrailA;
        public XWeaponTrail leftKnifeTrailB;
        public XWeaponTrail rightKnifeTrailA;
        public XWeaponTrail rightKnifeTrailB;

        public override bool CanBlockAction(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (__brain.BB.IsGroggy)
                return false;
            else if (__brain.ActionCtrler.CheckActionRunning())
                return false;
            else if (!__brain.SensorCtrler.WatchingColliders.Contains(damageContext.senderBrain.coreColliderHelper.pawnCollider))
                return false;

            return damageContext.insufficientStamina;
        }

        public override IDisposable StartOnHitAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                SoundManager.Instance.Play(SoundID.HIT_FLESH);
                EffectManager.Instance.Show("@Hit 23 cube", damageContext.hitPoint, Quaternion.identity, Vector3.one, 1);
                EffectManager.Instance.Show("@BloodFX_impact_col", damageContext.hitPoint, Quaternion.identity, 1.5f * Vector3.one, 3);
            }
            else if (damageContext.actionResult == ActionResults.Missed || damageContext.actionResult == ActionResults.Blocked)
            {
                SoundManager.Instance.Play(SoundID.HIT_BLOCK);
                EffectManager.Instance.Show("@Hit 4 yellow arrow", 0.5f * (__brain.AnimCtrler.leftWeaponSlot.position + __brain.AnimCtrler.rightWeaponSlot.position), Quaternion.identity, Vector3.one, 1f);
            }
            else if (damageContext.actionResult == ActionResults.GuardBreak) 
            {
                SoundManager.Instance.Play(SoundID.GUARD_BREAK);
                EffectManager.Instance.Show("SwordHitRed", 0.5f * (__brain.AnimCtrler.leftWeaponSlot.position + __brain.AnimCtrler.rightWeaponSlot.position), Quaternion.identity, Vector3.one, 1f);
            }

            return base.StartOnHitAction(ref damageContext, isAddictiveAction);
        }

        public override IDisposable StartOnKnockDownAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
#if UNITY_EDITOR
            if (damageContext.senderBrain == null && damageContext.receiverBrain == null)
            {
                __brain.PawnStatusCtrler.AddStatus(PawnStatus.KnockDown, 1f, __brain.BB.pawnData.knockDownDuration);

                var __knockDownTimeStamp = Time.time;
                var __knockBackVec = -__brain.coreColliderHelper.transform.forward.Vector2D().normalized;
                Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.1f))).Subscribe(_ =>
                {
                    __brain.Movement.AddRootMotion(Time.fixedDeltaTime * (__brain.BB.pawnData_Movement.knockBackSpeed * __knockBackVec), Quaternion.identity);
                }).AddTo(this);

                return null;
            }
#endif

            Debug.Assert(damageContext.receiverBrain == __brain);
            Debug.Assert(!isAddictiveAction);
            
            var hitVec = damageContext.receiverBrain.GetWorldPosition() - damageContext.senderBrain.GetWorldPosition();
            hitVec = damageContext.receiverBrain.GetWorldTransform().InverseTransformDirection(hitVec).Vector2D().normalized;
            if (Mathf.Abs(hitVec.x) > Mathf.Abs(hitVec.z))
            {
                __brain.AnimCtrler.mainAnimator.SetFloat("HitX", hitVec.x > 0f ? 1f : -1f);
                __brain.AnimCtrler.mainAnimator.SetFloat("HitY", 0f);
            }
            else
            {
                __brain.AnimCtrler.mainAnimator.SetFloat("HitX", 0f);
                __brain.AnimCtrler.mainAnimator.SetFloat("HitY", hitVec.z > 0f ? 1f : -1f);
            }
            __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 0);
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");

            var knockDownTimeStamp = Time.time;
            var knockBakVec = (damageContext.receiverBrain.GetWorldPosition() - damageContext.senderBrain.GetWorldPosition()).Vector2D().normalized;
            Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.1f))).Subscribe(_ =>
            {
                __brain.Movement.AddRootMotion(Time.fixedDeltaTime * (__brain.BB.pawnData_Movement.knockBackSpeed * knockBakVec), Quaternion.identity);
            }).AddTo(this);

            return null;
        }

        AlienBrain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<AlienBrain>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            __brain.BB.common.isDown.Skip(1).Subscribe(v =>
            {
                if (v)
                {
                    __brain.AnimCtrler.mainAnimator.SetBool("IsDown", true);
                    __brain.AnimCtrler.mainAnimator.SetTrigger("OnDown");
                }
                else
                {
                    //* 일어나는 모션동안은 무적
                    __brain.PawnStatusCtrler.AddStatus(PawnStatus.Invincible, 1f, 1f);
                    __brain.AnimCtrler.mainAnimator.SetBool("IsDown", false);
                }
            }).AddTo(this);

            __brain.BB.action.isGuarding.Subscribe(v =>
            {
                __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", v);
            }).AddTo(this);
        }
    }
}