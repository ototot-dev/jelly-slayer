using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class NpcHumanoidActionController : PawnActionController
    {
        public override bool CheckKnockBackRunning() => __knockBackDisposable != null;
        protected IDisposable __knockBackDisposable;

        public override bool CanRootMotion(Vector3 rootMotionVec)
        {
            Debug.Assert(CheckActionRunning());

            if (!base.CanRootMotion(rootMotionVec))
                return false;

            if (__humanoidBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered))
                return false;

            if (__humanoidBrain.PawnBB.TargetBrain != null && __humanoidBrain.SensorCtrler.TouchingColliders.Contains(__humanoidBrain.PawnBB.TargetBrain.coreColliderHelper.pawnCollider))
            {
                //* RootMotion으로 목표물을 밀지 않도록 목표물의 TouchingColliders와 접축할 정도로 가깝다면 rootMotionVec가 목표물에서 멀어지는 방향일때만 적용해준다.
                return __humanoidBrain.coreColliderHelper.GetDistanceDelta(__humanoidBrain.PawnBB.TargetBrain.coreColliderHelper, rootMotionVec) > 0f;
            }
            else
            {
                return true;
            }
        }

        public override IDisposable StartOnHitAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __humanoidBrain);

            if (damageContext.actionResult == ActionResults.Damaged)
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

                if (damageContext.receiverBrain.PawnBB.IsGroggy)
                    __pawnAnimCtrler.mainAnimator.SetInteger("HitType", damageContext.groggyBreakHit ? 4 : 3);
                else
                    __pawnAnimCtrler.mainAnimator.SetInteger("HitType", 0);
                __pawnAnimCtrler.mainAnimator.SetTrigger("OnHit");
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
                __humanoidBrain.PawnStatusCtrler.AddStatus(PawnStatus.Guardbreak, duration: 1.0f);
            }

            var knockBackVec = __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            if (damageContext.actionResult == ActionResults.Missed || damageContext.actionResult == ActionResults.Blocked)
            {
                __knockBackDisposable?.Dispose();
                __knockBackDisposable = Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(1f / __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed)))
                    .DoOnCancel(() => __knockBackDisposable = null)
                    .DoOnCompleted(() => __knockBackDisposable = null)
                    .Subscribe(_ => __pawnMovement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity, Time.fixedDeltaTime)).AddTo(this);
            }
            else
            {            
                __knockBackDisposable?.Dispose();
                __knockBackDisposable = Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.senderActionData.knockBackDistance / __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed)))
                    .DoOnCancel(() => 
                    {
                        __pawnBrain.InvalidateDecision(0.5f);
                        __knockBackDisposable = null;
                    })
                    .DoOnCompleted(() => 
                    {
                        __pawnBrain.InvalidateDecision(0.5f);
                        __knockBackDisposable = null;
                    })
                    .Subscribe(_ => __pawnMovement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity, Time.fixedDeltaTime)).AddTo(this);
            }

            return null;
        }

        public override IDisposable StartOnBlockedAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.senderBrain == __humanoidBrain);
            Debug.Assert(damageContext.receiverActionData != null);
            Debug.Assert(!isAddictiveAction);

            __pawnAnimCtrler.mainAnimator.SetTrigger("OnHit");
            __pawnAnimCtrler.mainAnimator.SetInteger("HitType", 3);

            var knockBackVec = __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            __knockBackDisposable?.Dispose();
            __knockBackDisposable = Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverActionData.knockBackDistance / __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed)))
                .DoOnCancel(() => 
                {
                    __pawnBrain.InvalidateDecision(0.5f);
                    __knockBackDisposable = null;
                })
                .DoOnCompleted(() => 
                {
                    __pawnBrain.InvalidateDecision(0.5f);
                    __knockBackDisposable = null;
                })
                .Subscribe(_ => __pawnMovement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity, Time.fixedDeltaTime)).AddTo(this);

            return null;
        }

        public override IDisposable StartOnParriedAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.senderBrain == __humanoidBrain);

            __pawnAnimCtrler.mainAnimator.SetTrigger("OnParried");

            var knockBackDistance = 0f;
            if (damageContext.actionResult == ActionResults.PunchParrying)
                knockBackDistance = damageContext.receiverActionData.knockBackDistance;
            else if (damageContext.actionResult == ActionResults.GuardParrying)
                knockBackDistance = DatasheetManager.Instance.GetActionData(damageContext.receiverBrain.PawnBB.common.pawnId, "GuardParry")?.knockBackDistance ?? 0f;
            else
                Debug.Assert(false);

            if (knockBackDistance <= 0f)
                __Logger.WarningR2(gameObject, nameof(StartOnParriedAction), "knockBackDistance is zero", "knockBackDistance", knockBackDistance);
            else
                __Logger.LogR1(gameObject, nameof(StartOnParriedAction), "knockBackDistance", knockBackDistance);

            var knockBackVec = __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed * damageContext.receiverBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            __knockBackDisposable?.Dispose();
            __knockBackDisposable = Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(knockBackDistance / __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed)))
                .DoOnCancel(() => 
                {
                    __pawnBrain.InvalidateDecision(0.5f);
                    __knockBackDisposable = null;
                })
                .DoOnCompleted(() => 
                {
                    __pawnBrain.InvalidateDecision(0.5f);
                    __knockBackDisposable = null;
                })
                .Subscribe(_ => __pawnMovement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity, Time.fixedDeltaTime)).AddTo(this);

            return null;
        }

        public override IDisposable StartOnKnockDownAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __humanoidBrain);
            return null;
        }

        public override IDisposable StartOnGroogyAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            if (damageContext.actionResult == ActionResults.PunchParrying || damageContext.actionResult == ActionResults.GuardParrying)
            {
                Debug.Assert(__humanoidBrain == damageContext.senderBrain);

                __pawnAnimCtrler.mainAnimator.SetBool("IsGroggy", true);
                __pawnAnimCtrler.mainAnimator.SetTrigger("OnGroggy");

                // var knockBackDistance = 0f;
                // if (damageContext.actionResult == ActionResults.PunchParried)
                //     knockBackDistance = damageContext.receiverActionData.knockBackDistance;
                // else if (damageContext.actionResult == ActionResults.GuardParried)
                //     knockBackDistance = DatasheetManager.Instance.GetActionData(damageContext.receiverBrain.PawnBB.common.pawnId, "GuardParry")?.knockBackDistance ?? 0f;
                // else
                //     Debug.Assert(false);

                // var knockBackVec = __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed * damageContext.receiverBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                // __knockBackDisposable?.Dispose();
                // __knockBackDisposable = Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(knockBackDistance / __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed)))
                //     .DoOnCancel(() =>
                //     {
                //         __pawnBrain.InvalidateDecision(0.5f);
                //         __knockBackDisposable = null;
                //     })
                //     .DoOnCompleted(() =>
                //     {
                //         __pawnBrain.InvalidateDecision(0.5f);
                //         __knockBackDisposable = null;
                //     })
                //     .Subscribe(_ => __pawnMovement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity, Time.fixedDeltaTime))
                //     .AddTo(this);
            }
            else
            {
                if (damageContext.senderActionData.knockBackDistance > 0f)
                {
                    var knockBackVec = damageContext.senderActionData.actionName == "SpecialKick" ?
                        __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed * (damageContext.receiverBrain.GetWorldPosition() - damageContext.senderBrain.GetWorldPosition()).Vector2D().normalized :
                        __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                        
                    __knockBackDisposable?.Dispose();
                    __knockBackDisposable = Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.senderActionData.knockBackDistance / __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed)))
                        .DoOnCancel(() =>
                        {
                            __pawnBrain.InvalidateDecision(0.5f);
                            __knockBackDisposable = null;

                            //* KnockBack 연출 후에 Groogy 모션 진입
                            __pawnAnimCtrler.mainAnimator.SetBool("IsGroggy", true);
                            __pawnAnimCtrler.mainAnimator.SetTrigger("OnGroggy");
                        })
                        .DoOnCompleted(() =>
                        {
                            __pawnBrain.InvalidateDecision(0.5f);
                            __knockBackDisposable = null;

                            //* KnockBack 연출 후에 Groogy 모션 진입
                            __pawnAnimCtrler.mainAnimator.SetBool("IsGroggy", true);
                            __pawnAnimCtrler.mainAnimator.SetTrigger("OnGroggy");
                        })
                        .Subscribe(_ => __pawnMovement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity, Time.fixedDeltaTime))
                        .AddTo(this);
                }
            }

            return null;
        }

        protected NpcHumanoidBrain __humanoidBrain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __humanoidBrain = GetComponent<NpcHumanoidBrain>();
        }
    }
}