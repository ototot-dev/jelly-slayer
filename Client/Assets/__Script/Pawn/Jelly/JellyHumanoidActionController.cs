using System;
using System.Linq;
using UniRx;
using UnityEngine;
using XftWeapon;

namespace Game
{
    public class JellyHumanoidActionController : PawnActionController
    {
        public override bool CanRootMotion(Vector3 rootMotionVec)
        {
            Debug.Assert(CheckActionRunning());

            if (!base.CanRootMotion(rootMotionVec))
                return false;

            if (__brain.StatusCtrler.CheckStatus(PawnStatus.Staggered))
                return false;

            if (__brain.PawnBB.TargetBrain != null && __brain.SensorCtrler.TouchingColliders.Contains(__brain.PawnBB.TargetBrain.coreColliderHelper.pawnCollider))
            {
                //* RootMotion으로 목표물을 밀지 않도록 목묘물의 TouchingColliders와 접축할 정도로 가깝다면 rootMotionVec가 목표물에서 멀어지는 방향일때만 적용해준다.
                var newDistance = (__brain.PawnBB.TargetBrain.coreColliderHelper.transform.position - __brain.coreColliderHelper.transform.position + rootMotionVec).Vector2D().sqrMagnitude;
                return newDistance > (__brain.PawnBB.TargetBrain.coreColliderHelper.transform.position - __brain.coreColliderHelper.transform.position).Vector2D().sqrMagnitude;
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
                if (damageContext.receiverBrain.PawnBB.IsGroggy)
                {
                    __pawnAnimCtrler.mainAnimator.SetInteger("HitType", 3);
                    __pawnAnimCtrler.mainAnimator.SetTrigger("OnHit");
                }
                else
                {
                    var hitVec = damageContext.receiverBrain.GetWorldPosition() - damageContext.senderBrain.GetWorldPosition();
                    hitVec = damageContext.receiverBrain.GetWorldTransform().InverseTransformDirection(hitVec).Vector2D().normalized;

                    if (Mathf.Abs(hitVec.x) > Mathf.Abs(hitVec.z))
                    {
                        __pawnAnimCtrler.mainAnimator.SetFloat("HitX", hitVec.x > 0f ? 1f : -1f);
                        __pawnAnimCtrler.mainAnimator.SetFloat("HitY", 0f);
                    }
                    else
                    {
                        __pawnAnimCtrler.mainAnimator.SetFloat("HitX", 0f);
                        __pawnAnimCtrler.mainAnimator.SetFloat("HitY", hitVec.z > 0f ? 1f : -1f);
                    }
                    __pawnAnimCtrler.mainAnimator.SetInteger("HitType", 0);
                    __pawnAnimCtrler.mainAnimator.SetTrigger("OnHit");
                }
            }
            else if (damageContext.actionResult == ActionResults.Blocked)
            {
                __pawnAnimCtrler.mainAnimator.SetTrigger("OnHit");
                __pawnAnimCtrler.mainAnimator.SetInteger("HitType", 1);
            }
            else if (damageContext.actionResult == ActionResults.GuardBreak) 
            {
                __pawnAnimCtrler.mainAnimator.SetInteger("HitType", 2);
                __pawnAnimCtrler.mainAnimator.SetTrigger("OnHit");
                __brain.PawnStatusCtrler.AddStatus(PawnStatus.Guardbreak, duration: 1.0f);
            }

            var knockBackVec = __brain.JellyBB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            if (damageContext.actionResult == ActionResults.Missed || damageContext.actionResult == ActionResults.Blocked)
            {
                Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f / __brain.JellyBB.pawnData_Movement.knockBackSpeed)))
                    .DoOnCancel(() => __pawnMovement.Freeze())
                    .DoOnCompleted(() => __pawnMovement.Freeze())
                    .Subscribe(_ => __pawnMovement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity)).AddTo(this);
            }
            else
            {            
                Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.senderActionData.knockBackDistance / __brain.JellyBB.pawnData_Movement.knockBackSpeed)))
                    .DoOnCancel(() => __pawnMovement.Freeze())
                    .DoOnCompleted(() => __pawnMovement.Freeze())
                    .Subscribe(_ => __pawnMovement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity)).AddTo(this);
            }

            return null;
        }

        public override IDisposable StartOnBlockedAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.senderBrain == __brain);
            Debug.Assert(damageContext.receiverActionData != null);
            Debug.Assert(!isAddictiveAction);

            __pawnAnimCtrler.mainAnimator.SetTrigger("OnHit");
            __pawnAnimCtrler.mainAnimator.SetInteger("HitType", 3);

            var knockBackVec = __brain.JellyBB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverActionData.knockBackDistance / __brain.JellyBB.pawnData_Movement.knockBackSpeed)))
                .DoOnCancel(() => __pawnMovement.Freeze())
                .DoOnCompleted(() => __pawnMovement.Freeze())
                .Subscribe(_ => __pawnMovement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity)).AddTo(this);

            return null;
        }

        public override IDisposable StartOnParriedAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.senderBrain == __brain);

            __Logger.LogF(gameObject, nameof(StartOnParriedAction), "-", "Distance", damageContext.senderBrain.coreColliderHelper.GetDistanceBetween(damageContext.receiverBrain.coreColliderHelper));
            __pawnAnimCtrler.mainAnimator.SetTrigger("OnParried");

            var knockBackDistance = 0f;
            if (damageContext.actionResult == ActionResults.KickParried)
                knockBackDistance = damageContext.receiverActionData.knockBackDistance;
            else if (damageContext.actionResult == ActionResults.GuardParried)
                knockBackDistance = DatasheetManager.Instance.GetActionData(damageContext.receiverBrain.PawnBB.common.pawnId, "GuardParry")?.knockBackDistance ?? 0f;
            else
                Debug.Assert(false);

            if (knockBackDistance <= 0f)
                __Logger.WarningF(gameObject, nameof(StartOnParriedAction), "knockBackDistance is zero", "knockBackDistance", knockBackDistance);

            var knockBackVec = __brain.JellyBB.pawnData_Movement.knockBackSpeed * damageContext.receiverBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(knockBackDistance / __brain.JellyBB.pawnData_Movement.knockBackSpeed)))
                .DoOnCancel(() => __pawnMovement.Freeze())
                .DoOnCompleted(() => __pawnMovement.Freeze())
                .Subscribe(_ => __pawnMovement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity)).AddTo(this);

            return null;
        }

        public override IDisposable StartOnKnockDownAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);
            return null;
        }

        public override IDisposable StartOnGroogyAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            __Logger.LogF(gameObject, nameof(StartOnGroogyAction), "-", "Distance", damageContext.senderBrain.coreColliderHelper.GetDistanceBetween(damageContext.receiverBrain.coreColliderHelper));

            if (damageContext.actionResult == ActionResults.GuardParried || damageContext.actionResult == ActionResults.KickParried)
            {
                Debug.Assert(__brain == damageContext.senderBrain);

                var knockBackDistance = 0f;
                if (damageContext.actionResult == ActionResults.KickParried)
                    knockBackDistance = damageContext.receiverActionData.knockBackDistance;
                else if (damageContext.actionResult == ActionResults.GuardParried)
                    knockBackDistance = DatasheetManager.Instance.GetActionData(damageContext.receiverBrain.PawnBB.common.pawnId, "GuardParry")?.knockBackDistance ?? 0f;
                else
                    Debug.Assert(false);

                var knockBackVec = __brain.JellyBB.pawnData_Movement.knockBackSpeed * damageContext.receiverBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(knockBackDistance / __brain.JellyBB.pawnData_Movement.knockBackSpeed)))
                    .DoOnCancel(() =>
                    {
                        __pawnMovement.Freeze();
                        //* KnockBack 연출 후에 Groogy 모션 진입
                        __pawnAnimCtrler.mainAnimator.SetBool("IsGroggy", true);
                        __pawnAnimCtrler.mainAnimator.SetTrigger("OnGroggy");
                    })
                    .DoOnCompleted(() =>
                    {
                        __pawnMovement.Freeze();
                        //* KnockBack 연출 후에 Groogy 모션 진입
                        __pawnAnimCtrler.mainAnimator.SetBool("IsGroggy", true);
                        __pawnAnimCtrler.mainAnimator.SetTrigger("OnGroggy");
                    })
                    .Subscribe(_ => __pawnMovement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity))
                    .AddTo(this);
            }
            else
            {
                __pawnAnimCtrler.mainAnimator.SetBool("IsGroggy", true);
                __pawnAnimCtrler.mainAnimator.SetTrigger("OnGroggy");
            }

            return null;
        }

        JellyHumanoidBrain __brain;
        PawnMovementEx __pawnMovement;
        PawnAnimController __pawnAnimCtrler;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            
            __brain = GetComponent<JellyHumanoidBrain>();
            __pawnMovement = GetComponent<PawnMovementEx>();
            __pawnAnimCtrler = GetComponent<PawnAnimController>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            // __brain.JellyBB.decision.isGuarding.Subscribe(v =>
            // {
            //     __pawnAnimCtrler.mainAnimator.SetBool("IsGuarding", v);
            // }).AddTo(this);

            __brain.JellyBB.common.isDown.Skip(1).Subscribe(v =>
            {
                if (v)
                {
                    __pawnAnimCtrler.mainAnimator.SetBool("IsDown", true);
                    __pawnAnimCtrler.mainAnimator.SetTrigger("OnDown");
                }
                else
                {
                    //* 일어나는 모션동안은 무적
                    __brain.PawnStatusCtrler.AddStatus(PawnStatus.Invincible, 1f, 1f);
                    __pawnAnimCtrler.mainAnimator.SetBool("IsDown", false);
                    __brain.InvalidateDecision(2f);
                }
            }).AddTo(this);

            __brain.JellyBB.common.isGroggy.Skip(1).Where(v => !v).Subscribe(v =>
            {
                if (!__brain.PawnStatusCtrler.CheckStatus(PawnStatus.KnockDown))
                {
                    //* 일어나는 모션동안은 무적
                    __brain.PawnStatusCtrler.AddStatus(PawnStatus.Invincible, 1f, 1f);
                    __pawnAnimCtrler.mainAnimator.SetBool("IsGroggy", false);
                    __brain.InvalidateDecision(1f);
                }
            }).AddTo(this);
        }
    }
}