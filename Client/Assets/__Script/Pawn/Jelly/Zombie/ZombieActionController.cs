using System;
using UniRx;
using UnityEngine;
using static FIMSpace.FProceduralAnimation.LegsAnimator;

namespace Game
{
    public class ZombieActionController : PawnActionController
    {
        public override bool CanRootMotion(Vector3 rootMotionVec)
        {
            Debug.Assert(CheckActionRunning());

            if (!base.CanRootMotion(rootMotionVec))
                return false;

            if (__brain.BB.TargetBrain != null && __brain.SensorCtrler.TouchingColliders.Contains(__brain.BB.TargetBrain.coreColliderHelper.pawnCollider))
            {
                //* Target과 접촉한 상태라면, rootMotionVec가 목표점에서 멀어지는 경우에만 적용함
                return __brain.coreColliderHelper.GetDistanceDelta(__brain.BB.TargetBrain.coreColliderHelper, rootMotionVec) > 0f;
            }
            else
            {
                return true;
            }
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

            var hitVec = damageContext.senderBrain.coreColliderHelper.transform.position - damageContext.receiverBrain.coreColliderHelper.transform.position;
            hitVec = damageContext.receiverBrain.coreColliderHelper.transform.InverseTransformDirection(hitVec).Vector2D().normalized;
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");
            __brain.AnimCtrler.mainAnimator.SetFloat("HitParam", hitVec.z);

            var knockBackVec = damageContext.senderActionData.knockBackDistance / 0.2f * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            var knockBackDisposable = Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity, Time.deltaTime))
                .AddTo(this);

            if (isAddictiveAction)
            {
                return knockBackDisposable;
            }
            else
            {
                return Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverPenalty.Item2))
                    .DoOnCancel(() =>
                    {
                        if (CurrActionName == "!OnHit")
                            FinishAction();
                    })
                    .Subscribe(_ =>
                    {
                        if (CurrActionName == "!OnHit")
                            FinishAction();
                    }).AddTo(this);
            }
        }

        public override IDisposable StartOnKnockDownAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);
            Debug.Assert(!isAddictiveAction);

            __brain.AnimCtrler.mainAnimator.SetBool("IsDown", true);
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnDown");

            var knockBackVec = damageContext.senderActionData.knockBackDistance / 0.2f * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.5f)))
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity, Time.fixedDeltaTime))
                .AddTo(this);

            return Observable.EveryFixedUpdate().TakeWhile(_ => __brain.BB.IsDown)
                .DoOnCancel(() =>
                {
                    __brain.AnimCtrler.mainAnimator.SetBool("IsDown", false);
                    if (CurrActionName == "!OnKnockDown")
                        FinishAction();
                })
                .DoOnCompleted(() =>
                {
                    __brain.AnimCtrler.mainAnimator.SetBool("IsDown", false);
                    if (CurrActionName == "!OnKnockDown")
                        FinishAction();
                })
                .Subscribe().AddTo(this);
        }
        
        public override IDisposable StartOnGroogyAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);
            Debug.Assert(!isAddictiveAction);

            __brain.AnimCtrler.mainAnimator.SetBool("IsStunned", true);
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnStunned");

            return Observable.EveryFixedUpdate().TakeWhile(_ => __brain.BB.IsGroggy)
                .DoOnCancel(() =>
                {
                    __brain.AnimCtrler.mainAnimator.SetBool("IsStunned", false);
                    if (CurrActionName == "!OnGroggy")
                        FinishAction();
                })
                .DoOnCompleted(() =>
                {
                    __brain.AnimCtrler.mainAnimator.SetBool("IsStunned", false);
                    if (CurrActionName == "!OnGroggy")
                        FinishAction();
                })
                .Subscribe().AddTo(this);
        }

        public override IDisposable StartOnBlockedAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.senderBrain == __brain);
            Debug.Assert(!isAddictiveAction);
    
            __brain.AnimCtrler.legAnimator.User_AddImpulse(new ImpulseExecutor(new Vector3(0, 0.2f, -0.1f), Vector3.zero, 0.2f));

            var knockBackDistance = DatasheetManager.Instance.GetActionData(damageContext.receiverBrain.PawnBB.common.pawnId, "!OnBlock")?.knockBackDistance ?? 0f;
            if (knockBackDistance <= 0f)
                __Logger.WarningR2(gameObject, nameof(StartOnBlockedAction), "knockBackDistance is invalid.", "knockBackDistance", knockBackDistance);

            var knockBackVec = knockBackDistance / 0.2f * -damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity, Time.deltaTime))
                .AddTo(this);
                
            return Observable.EveryFixedUpdate().TakeWhile(_ => __brain.BB.IsGroggy)
                .DoOnCancel(() =>
                {
                    __brain.AnimCtrler.mainAnimator.SetBool("IsStunned", false);
                    if (CurrActionName == "!OnBlocked")
                        FinishAction();
                })
                .DoOnCompleted(() =>
                {
                    __brain.AnimCtrler.mainAnimator.SetBool("IsStunned", false);
                    if (CurrActionName == "!OnBlocked")
                        FinishAction();
                })
                .Subscribe().AddTo(this);
        }

        public override IDisposable StartOnParriedAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.senderBrain == __brain);
            Debug.Assert(!isAddictiveAction);
    
            __brain.AnimCtrler.legAnimator.User_AddImpulse(new ImpulseExecutor(new Vector3(0, 0.2f, -0.1f), Vector3.zero, 0.2f));

            var parryActionName = damageContext.actionResult == ActionResults.KickParried ? "Kick" : "GuardParry";
            var knockBackDistance = DatasheetManager.Instance.GetActionData(damageContext.receiverBrain.PawnBB.common.pawnId, parryActionName)?.knockBackDistance ?? 0f;
            if (knockBackDistance <= 0f)
                __Logger.WarningR2(gameObject, nameof(StartOnBlockedAction), "knockBackDistance is invalid.", "knockBackDistance", knockBackDistance);

            var knockBackVec = knockBackDistance / 0.2f * -damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity, Time.deltaTime))
                .AddTo(this);
                
            return Observable.EveryFixedUpdate().TakeWhile(_ => __brain.BB.IsGroggy)
                .DoOnCancel(() =>
                {
                    __brain.AnimCtrler.mainAnimator.SetBool("IsStunned", false);
                    if (CurrActionName == "!OnParried")
                        FinishAction();
                })
                .DoOnCompleted(() =>
                {
                    __brain.AnimCtrler.mainAnimator.SetBool("IsStunned", false);
                    if (CurrActionName == "!OnParried")
                        FinishAction();
                })
                .Subscribe().AddTo(this);

        }
        ZombieBrain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<ZombieBrain>();
        }
    }
}