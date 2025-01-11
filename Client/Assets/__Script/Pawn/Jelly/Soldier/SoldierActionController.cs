using System;
using UniRx;
using UnityEngine;
using XftWeapon;

namespace Game
{
    public class SoldierActionController : JellyHumanoidActionController
    {
        [Header("Component")]
        public PawnColliderHelper hookingPointColliderHelper;
        public PawnColliderHelper counterActionColliderHelper;
        public BoxCollider shieldCollider;
        public XWeaponTrail sworldWeaponTrailA;
        public XWeaponTrail sworldWeaponTrailB;

        [Header("Parameter")]
        public float leapRootMotionDistance = 7f;
        public float leapRootMotionMultiplier = 1f;
        public CapsuleCollider CounterActionCollider => counterActionColliderHelper.pawnCollider as CapsuleCollider;

        public override bool CanBlockAction(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (__brain.BB.IsGroggy)
                return false;
            else if (__brain.ActionCtrler.CheckActionRunning() || __brain.StatusCtrler.CheckStatus(PawnStatus.Staggered) || __brain.StatusCtrler.CheckStatus(PawnStatus.CanNotGuard))
                return false;
            else if (__brain.SensorCtrler.WatchingColliders.Contains(damageContext.senderBrain.coreColliderHelper.pawnCollider) == false)
                return false;

            return true;
        }

        public override IDisposable StartOnHitAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                if (damageContext.senderActionData.actionName.StartsWith("Kick"))
                    EffectManager.Instance.Show(__brain.BB.graphics.onKickHitFx, __brain.hitColliderHelper.GetCenter(), Quaternion.identity, Vector3.one, 1f);
                else if (damageContext.senderActionData.actionName.StartsWith("Heavy"))
                    EffectManager.Instance.Show(__brain.BB.graphics.onBigHitFx, __brain.hitColliderHelper.GetCenter(), Quaternion.LookRotation(damageContext.hitPoint - __brain.hitColliderHelper.GetCenter()) * Quaternion.Euler(90f, 0f, 0f), Vector3.one, 1f);
                else
                    EffectManager.Instance.Show(__brain.BB.graphics.onHitFx, __brain.hitColliderHelper.GetCenter(), Quaternion.LookRotation(damageContext.hitPoint - __brain.hitColliderHelper.GetCenter()) * Quaternion.Euler(90f, 0f, 0f), Vector3.one, 1f);

                SoundManager.Instance.Play(SoundID.HIT_FLESH);
            }
            else if (damageContext.actionResult == ActionResults.Missed)
            {
                SoundManager.Instance.Play(SoundID.HIT_BLOCK);
                EffectManager.Instance.Show("@Hit 4 yellow arrow", __brain.AnimCtrler.shieldMeshSlot.position, Quaternion.identity, Vector3.one, 1f);
            }
            else if (damageContext.actionResult == ActionResults.Blocked)
            {
                __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", true);
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnGuard");
                
                Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ => __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", false)).AddTo(this);
                Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ => EffectManager.Instance.Show(__brain.BB.graphics.onBlockedFx, __brain.BB.graphics.BlockingFxAttachPoint.transform.position, Quaternion.identity, 0.8f * Vector3.one, 1f)).AddTo(this);
                SoundManager.Instance.Play(SoundID.HIT_BLOCK);

            }
            else if (damageContext.actionResult == ActionResults.GuardBreak) 
            {
                Observable.NextFrame(FrameCountType.EndOfFrame).Subscribe(_ => EffectManager.Instance.Show(__brain.BB.graphics.onGuardBreakFx, __brain.BB.graphics.BlockingFxAttachPoint.transform.position, Quaternion.identity, Vector3.one, 1f)).AddTo(this);
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

        SoldierBrain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<SoldierBrain>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            __brain.BB.action.isGuarding.Subscribe(v =>
            {
                __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", v);
            }).AddTo(this);
        }
    }
}