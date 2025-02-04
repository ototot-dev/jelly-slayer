using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class JellyQuadWalkActionController : PawnActionController
    {
        public override bool CheckKnockBackRunning() => __knockBackDisposable != null;
        IDisposable __knockBackDisposable;

        public override bool CanRootMotion(Vector3 rootMotionVec)
        {
            Debug.Assert(CheckActionRunning());

            if (!base.CanRootMotion(rootMotionVec))
                return false;

            if (__humanoidBrain.StatusCtrler.CheckStatus(PawnStatus.Staggered))
                return false;

            if (__humanoidBrain.PawnBB.TargetBrain != null && __humanoidBrain.SensorCtrler.TouchingColliders.Contains(__humanoidBrain.PawnBB.TargetBrain.coreColliderHelper.pawnCollider))
            {
                //* RootMotion으로 목표물을 밀지 않도록 목표물의 TouchingColliders와 접촉할 정도로 가깝다면 rootMotionVec가 목표물에서 멀어지는 방향일때만 적용해준다.
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
                __humanoidBrain.PawnStatusCtrler.AddStatus(PawnStatus.Guardbreak, duration: 1.0f);
            }

            var knockBackVec = __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            if (damageContext.actionResult == ActionResults.Missed || damageContext.actionResult == ActionResults.Blocked)
            {
                __knockBackDisposable?.Dispose();
                __knockBackDisposable = Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(1f / __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed)))
                    .DoOnCancel(() => 
                    {
                        __pawnMovement.FreezeForOneFrame();
                        __knockBackDisposable = null;
                    })
                    .DoOnCompleted(() => 
                    {
                        __pawnMovement.FreezeForOneFrame();
                        __knockBackDisposable = null;
                    })
                    .Subscribe(_ => __pawnMovement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity)).AddTo(this);
            }
            else
            {            
                __knockBackDisposable?.Dispose();
                __knockBackDisposable = Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.senderActionData.knockBackDistance / __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed)))
                    .DoOnCancel(() => 
                    {
                        __pawnBrain.InvalidateDecision(0.5f);
                        __pawnMovement.FreezeForOneFrame();
                        __knockBackDisposable = null;
                    })
                    .DoOnCompleted(() => 
                    {
                        __pawnBrain.InvalidateDecision(0.5f);
                        __pawnMovement.FreezeForOneFrame();
                        __knockBackDisposable = null;
                    })
                    .Subscribe(_ => __pawnMovement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity)).AddTo(this);
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
                    __pawnMovement.FreezeForOneFrame();
                    __knockBackDisposable = null;
                })
                .DoOnCompleted(() => 
                {
                    __pawnBrain.InvalidateDecision(0.5f);
                    __pawnMovement.FreezeForOneFrame();
                    __knockBackDisposable = null;
                })
                .Subscribe(_ => __pawnMovement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity)).AddTo(this);

            return null;
        }

        public override IDisposable StartOnParriedAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.senderBrain == __humanoidBrain);

            __Logger.LogR2(gameObject, nameof(StartOnParriedAction), "-", "Distance", damageContext.senderBrain.coreColliderHelper.GetDistanceBetween(damageContext.receiverBrain.coreColliderHelper));
            __pawnAnimCtrler.mainAnimator.SetTrigger("OnParried");

            var knockBackDistance = 0f;
            if (damageContext.actionResult == ActionResults.KickParried)
                knockBackDistance = damageContext.receiverActionData.knockBackDistance;
            else if (damageContext.actionResult == ActionResults.GuardParried)
                knockBackDistance = DatasheetManager.Instance.GetActionData(damageContext.receiverBrain.PawnBB.common.pawnId, "GuardParry")?.knockBackDistance ?? 0f;
            else
                Debug.Assert(false);

            if (knockBackDistance <= 0f)
                __Logger.WarningR2(gameObject, nameof(StartOnParriedAction), "knockBackDistance is zero", "knockBackDistance", knockBackDistance);

            var knockBackVec = __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed * damageContext.receiverBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            __knockBackDisposable?.Dispose();
            __knockBackDisposable = Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(knockBackDistance / __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed)))
                .DoOnCancel(() => 
                {
                    __pawnBrain.InvalidateDecision(0.5f);
                    __pawnMovement.FreezeForOneFrame();
                    __knockBackDisposable = null;
                })
                .DoOnCompleted(() => 
                {
                    __pawnBrain.InvalidateDecision(0.5f);
                    __pawnMovement.FreezeForOneFrame();
                    __knockBackDisposable = null;
                })
                .Subscribe(_ => __pawnMovement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity)).AddTo(this);

            return null;
        }

        public override IDisposable StartOnKnockDownAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __humanoidBrain);
            return null;
        }

        public override IDisposable StartOnGroogyAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            __Logger.LogR2(gameObject, nameof(StartOnGroogyAction), "-", "Distance", damageContext.senderBrain.coreColliderHelper.GetDistanceBetween(damageContext.receiverBrain.coreColliderHelper));

            if (damageContext.actionResult == ActionResults.GuardParried || damageContext.actionResult == ActionResults.KickParried)
            {
                Debug.Assert(__humanoidBrain == damageContext.senderBrain);

                var knockBackDistance = 0f;
                if (damageContext.actionResult == ActionResults.KickParried)
                    knockBackDistance = damageContext.receiverActionData.knockBackDistance;
                else if (damageContext.actionResult == ActionResults.GuardParried)
                    knockBackDistance = DatasheetManager.Instance.GetActionData(damageContext.receiverBrain.PawnBB.common.pawnId, "GuardParry")?.knockBackDistance ?? 0f;
                else
                    Debug.Assert(false);

                var knockBackVec = __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed * damageContext.receiverBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                __knockBackDisposable?.Dispose();
                __knockBackDisposable = Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(knockBackDistance / __humanoidBrain.JellyBB.pawnData_Movement.knockBackSpeed)))
                    .DoOnCancel(() =>
                    {
                        __pawnBrain.InvalidateDecision(0.5f);
                        __pawnMovement.FreezeForOneFrame();
                        __knockBackDisposable = null;

                        //* KnockBack 연출 후에 Groogy 모션 진입
                        __pawnAnimCtrler.mainAnimator.SetBool("IsGroggy", true);
                        __pawnAnimCtrler.mainAnimator.SetTrigger("OnGroggy");
                    })
                    .DoOnCompleted(() =>
                    {
                        __pawnBrain.InvalidateDecision(0.5f);
                        __pawnMovement.FreezeForOneFrame();
                        __knockBackDisposable = null;

                        //* KnockBack 연출 후에 Groogy 모션 진입
                        __pawnAnimCtrler.mainAnimator.SetBool("IsGroggy", true);
                        __pawnAnimCtrler.mainAnimator.SetTrigger("OnGroggy");
                    })
                    .Subscribe(_ => __pawnMovement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity))
                    .AddTo(this);
            }
            else
            {
                // __pawnAnimCtrler.mainAnimator.SetBool("IsGroggy", true);
                // __pawnAnimCtrler.mainAnimator.SetTrigger("OnGroggy");

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
                        .Subscribe(_ => __pawnMovement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity))
                        .AddTo(this);
                }
            }

            return null;
        }

        protected JellyQuadWalkBrain __humanoidBrain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            
            __humanoidBrain = GetComponent<JellyQuadWalkBrain>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            __humanoidBrain.JellyBB.common.isDown.Skip(1).Subscribe(v =>
            {
                if (v)
                {
                    __pawnAnimCtrler.mainAnimator.SetBool("IsDown", true);
                    __pawnAnimCtrler.mainAnimator.SetTrigger("OnDown");
                }
                else
                {
                    //* 일어나는 모션 동안은 무적
                    __humanoidBrain.PawnStatusCtrler.AddStatus(PawnStatus.Invincible, 1f, 1f);
                    __pawnAnimCtrler.mainAnimator.SetBool("IsDown", false);
                    __humanoidBrain.InvalidateDecision(2f);
                }
            }).AddTo(this);

            __humanoidBrain.JellyBB.common.isGroggy.Skip(1).Where(v => !v).Subscribe(v =>
            {
                if (!__humanoidBrain.PawnStatusCtrler.CheckStatus(PawnStatus.KnockDown))
                {
                    //* 일어나는 모션 동안은 무적
                    __humanoidBrain.PawnStatusCtrler.AddStatus(PawnStatus.Invincible, 1f, 1f);
                    __pawnAnimCtrler.mainAnimator.SetBool("IsGroggy", false);
                    __humanoidBrain.InvalidateDecision(1f);
                }
            }).AddTo(this);
        }
    }
}