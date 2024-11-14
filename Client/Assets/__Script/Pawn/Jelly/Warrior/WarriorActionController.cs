using System;
using UniRx;
using UnityEngine;
using static FIMSpace.FProceduralAnimation.LegsAnimator;

namespace Game
{
    public class WarriorActionController : PawnActionController
    {
        public override bool CanRootMotion(Vector3 rootMotionVec)
        {
            Debug.Assert(CheckActionRunning());

            if (!base.CanRootMotion(rootMotionVec))
                return false;

            if (__brain.BB.TargetBrain != null && __brain.SensorCtrler.TouchingColliders.Contains(__brain.BB.TargetBrain.coreColliderHelper.pawnCollider))
            {
                //* RootMotion으로 목표물을 밀지 않도록 목묘물의 TouchingColliders와 접축할 정도로 가깝다면 rootMotionVec가 목표물에서 멀어지는 방향일때만 적용해준다.
                var newDistance = (__brain.BB.TargetBrain.coreColliderHelper.transform.position - __brain.coreColliderHelper.transform.position + rootMotionVec).Vector2D().sqrMagnitude;
                return newDistance > (__brain.BB.TargetBrain.coreColliderHelper.transform.position - __brain.coreColliderHelper.transform.position).Vector2D().sqrMagnitude;
            }
            else
            {
                return true;
            }
        }
        
        public override bool CanBlockAction(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (!__brain.ActionCtrler.CheckActionRunning() == false)
                return false;
            if (__brain.SensorCtrler.WatchingColliders.Contains(damageContext.senderBrain.coreColliderHelper.pawnCollider) == false)
                return false;
            // if (damageContext.receiverBrain.PawnBB.stat.stamina.Value < damageContext.actionData.guardDamage)
            //     return false;

            return true;
        }

        public override IDisposable StartOnHitAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);

            var knockBackVec = damageContext.senderActionData.knockBackDistance / 0.2f * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                var hitVec = damageContext.senderBrain.coreColliderHelper.transform.position - damageContext.receiverBrain.coreColliderHelper.transform.position;
                hitVec = damageContext.receiverBrain.coreColliderHelper.transform.InverseTransformDirection(hitVec).Vector2D().normalized;

                __brain.AnimCtrler.mainAnimator.SetFloat("HitX", hitVec.x);
                __brain.AnimCtrler.mainAnimator.SetFloat("HitY", hitVec.z);
                __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 0);
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");

                SoundManager.Instance.Play(SoundID.HIT_FLESH);
                EffectManager.Instance.Show("@Hit 23 cube", damageContext.hitPoint, Quaternion.identity, Vector3.one, 1);
                EffectManager.Instance.Show("@BloodFX_impact_col", damageContext.hitPoint, Quaternion.identity, 1.5f * Vector3.one, 3);
            }
            else if (damageContext.actionResult == ActionResults.Blocked)
            {
                knockBackVec *= 0.1f;

                __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", true);
                __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 1);
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");
                Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverPenalty.Item2))
                    .Subscribe(_ => __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", false)).AddTo(this);

                SoundManager.Instance.Play(SoundID.HIT_BLOCK);
                EffectManager.Instance.Show("@Hit 4 yellow arrow", __brain.AnimCtrler.shieldSocket.position, Quaternion.identity, Vector3.one, 1f);
            }
            else if (damageContext.actionResult == ActionResults.GuardBreak) 
            {
                __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 2);
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");

                SoundManager.Instance.Play(SoundID.GUARD_BREAK);
                EffectManager.Instance.Show("@Hit 4 yellow arrow", __brain.AnimCtrler.shieldSocket.position, Quaternion.identity, Vector3.one, 1f);
            }

            var knockBackDisposable = Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity))
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

            __brain.AnimCtrler.mainAnimator.SetBool("IsDown", true);
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnDown");

            var knockBackVec = damageContext.senderActionData.knockBackDistance / 0.2f * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity))
                .AddTo(this);

            return Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverPenalty.Item2))
                .DoOnCancel(() =>
                {
                    __brain.BB.common.isDown.Value = false;
                    __brain.AnimCtrler.mainAnimator.SetBool("IsDown", false);
                    if (CurrActionName == "!OnKnockDown")
                        FinishAction();
                })
                .DoOnCompleted(() =>
                {
                    __brain.BB.common.isDown.Value = false;
                    __brain.AnimCtrler.mainAnimator.SetBool("IsDown", false);
                    if (CurrActionName == "!OnKnockDown")
                        FinishAction();
                })
                .Subscribe().AddTo(this);
        }
        
        public override IDisposable StartOnGroogyAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);

            __brain.AnimCtrler.mainAnimator.SetBool("IsStunned", true);
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnStunned");

            Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverPenalty.Item2)).Subscribe(_ =>
            {
                __brain.BB.common.isStunned.Value = false;
                __brain.AnimCtrler.mainAnimator.SetBool("IsStunned", false);
                if (CurrActionName == "!OnGroggy")
                    FinishAction();
            }).AddTo(this);

            return Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverPenalty.Item2)).Subscribe().AddTo(this);
        }

        public override IDisposable StartOnBlockedAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.senderBrain == __brain);

            __brain.AnimCtrler.legAnimator.User_AddImpulse(new ImpulseExecutor(new Vector3(0, 0.2f, -0.1f), Vector3.zero, 0.2f));

            var knockBackDistance = DatasheetManager.Instance.GetActionData(damageContext.receiverBrain.PawnBB.common.pawnId, "!OnBlock")?.knockBackDistance ?? 0f;
            if (knockBackDistance <= 0f)
                __Logger.WarningF(gameObject, nameof(StartOnBlockedAction), "knockBackDistance is invalid.", "knockBackDistance", knockBackDistance);

            var knockBackVec = knockBackDistance / 0.2f * -damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            return Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                .DoOnCancel(() =>
                {
                    if (CurrActionName == "!OnBlocked")
                        FinishAction();
                })
                .DoOnCompleted(() =>
                {
                    if (CurrActionName == "!OnBlocked")
                        FinishAction();
                })
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity))
                .AddTo(this);
        }

        WarriorBrain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<WarriorBrain>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            __brain.BB.decision.isGuarding.Subscribe(v =>
            {
                __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", v);
            }).AddTo(this);
        }
    }
}