using System;
using System.Linq;
using UniRx;
using UniRx.Triggers.Extension;
using UnityEngine;
using UnityEngine.AI;
using XftWeapon;
using static FIMSpace.FProceduralAnimation.LegsAnimator;

namespace Game
{
    public class WorkerActionController : PawnActionController
    {
        [Header("Component")]
        public Transform counterActionCollider;
        public XWeaponTrail sworldWeaponTrailA;
        public XWeaponTrail sworldWeaponTrailB;

        public override bool CanRootMotion(Vector3 rootMotionVec)
        {
            Debug.Assert(CheckActionRunning());

            if (!base.CanRootMotion(rootMotionVec))
                return false;

            if (__brain.BB.IsDown || __brain.BB.IsGroggy)
                return true;

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

            var knockBackVec = __brain.BB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                if (damageContext.receiverBrain.PawnBB.IsGroggy)
                {
                    __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 3);
                    __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");
                }
                else
                {
                    var hitVec = damageContext.receiverBrain.coreColliderHelper.transform.position - damageContext.senderBrain.CoreTransform.position;
                    hitVec = damageContext.receiverBrain.CoreTransform.InverseTransformDirection(hitVec).Vector2D().normalized;

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
                }

                SoundManager.Instance.Play(SoundID.HIT_FLESH);
                EffectManager.Instance.Show("@Hit 23 cube", damageContext.hitPoint, Quaternion.identity, Vector3.one, 1);
                EffectManager.Instance.Show("@BloodFX_impact_col", damageContext.hitPoint, Quaternion.identity, 1.5f * Vector3.one, 3);
            }
            else if (damageContext.actionResult == ActionResults.ZeroDamaged)
            {
                SoundManager.Instance.Play(SoundID.HIT_BLOCK);
                EffectManager.Instance.Show("@Hit 4 yellow arrow", __brain.AnimCtrler.shieldMeshSlot.position, Quaternion.identity, Vector3.one, 1f);
            }
            else if (damageContext.actionResult == ActionResults.Blocked)
            {
                __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 1);
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");
                __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", true);
                Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverPenalty.Item2))
                    .Subscribe(_ => __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", false)).AddTo(this);

                SoundManager.Instance.Play(SoundID.HIT_BLOCK);
                EffectManager.Instance.Show("@Hit 4 yellow arrow", __brain.AnimCtrler.shieldMeshSlot.position, Quaternion.identity, Vector3.one, 1f);
            }
            else if (damageContext.actionResult == ActionResults.GuardBreak) 
            {
                __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 2);
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");
                __brain.PawnStatusCtrler.AddStatus(PawnStatus.Guardbreak, duration: 1.0f);

                SoundManager.Instance.Play(SoundID.GUARD_BREAK);
                EffectManager.Instance.Show("SwordHitRed", __brain.AnimCtrler.shieldMeshSlot.position, Quaternion.identity, Vector3.one, 1f);
            }

            IDisposable knockBackDisposable = null;
            if (damageContext.actionResult == ActionResults.ZeroDamaged || damageContext.actionResult == ActionResults.Blocked)
            {
                knockBackDisposable = Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f / __brain.BB.pawnData_Movement.knockBackSpeed)))
                    .DoOnCancel(() => __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 0))
                    .DoOnCompleted(() => __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 0))
                    .Subscribe(_ => __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity))
                    .AddTo(this);
            }
            else
            {            
                knockBackDisposable = Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.senderActionData.knockBackDistance / __brain.BB.pawnData_Movement.knockBackSpeed)))
                    .DoOnCancel(() => __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 0))
                    .DoOnCompleted(() => __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 0))
                    .Subscribe(_ => __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity))
                    .AddTo(this);
            }

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

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                SoundManager.Instance.Play(SoundID.HIT_FLESH);
                EffectManager.Instance.Show("@Hit 23 cube", damageContext.hitPoint, Quaternion.identity, Vector3.one, 1);
                EffectManager.Instance.Show("@BloodFX_impact_col", damageContext.hitPoint, Quaternion.identity, 1.5f * Vector3.one, 3);
            }

            __brain.AnimCtrler.mainAnimator.SetBool("IsDown", true);
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnDown");

            //? 임시: knockBackDistance를 rootMotionMultiplier값에 대입하여 이동거리를 늘려줌
            __brain.Movement.rootMotionMultiplier = damageContext.senderActionData.knockBackDistance;

            //* KnockDown 애님의 RootMotion 재생을 위해서 'penaltyDuration' 동안 Action을 유지시켜준다.
            return Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverPenalty.Item2))
                .DoOnCancel(() =>
                {
                    if (CurrActionName == "!OnKnockDown")
                        FinishAction();
                })
                .DoOnCompleted(() =>
                {
                    if (CurrActionName == "!OnKnockDown")
                        FinishAction();
                })
                .Subscribe().AddTo(this);
        }

        public override IDisposable StartOnGroogyAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            __Logger.LogF(gameObject, nameof(StartOnParriedAction), "-", "Distance", damageContext.senderBrain.coreColliderHelper.GetDistanceBetween(damageContext.receiverBrain.coreColliderHelper));

            __brain.AnimCtrler.mainAnimator.SetBool("IsGroggy", true);
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnGroggy");

            if (damageContext.actionResult == ActionResults.KickParried)
            {
                Debug.Assert(__brain == damageContext.senderBrain);

                //* Groogy 애님의 RootMotion 배율 짧게 조정
                currActionContext.rootMotionMultiplier = 0f;

                //* KnockBack 연출 후에 Groogy 모션 진입
                var knockBackVec = __brain.BB.pawnData_Movement.knockBackSpeed * damageContext.receiverBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverActionData.knockBackDistance / __brain.BB.pawnData_Movement.knockBackSpeed)))
                    .DoOnCancel(() =>
                    {
                        __brain.AnimCtrler.mainAnimator.SetBool("IsGroggy", true);
                        __brain.AnimCtrler.mainAnimator.SetTrigger("OnGroggy");
                    })
                    .DoOnCompleted(() =>
                    {
                        __brain.AnimCtrler.mainAnimator.SetBool("IsGroggy", true);
                        __brain.AnimCtrler.mainAnimator.SetTrigger("OnGroggy");
                    })
                    .Subscribe(_ => __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity))
                    .AddTo(this);

                return Observable.NextFrame()
                    .DoOnCancel(() =>
                    {
                        if (CurrActionName == "!OnGroggy")
                            FinishAction();
                    })
                    .DoOnCompleted(() =>
                    {
                        if (CurrActionName == "!OnGroggy")
                            FinishAction();
                    })
                    .Subscribe().AddTo(this);
            }
            else
            {
                __brain.AnimCtrler.mainAnimator.SetBool("IsGroggy", true);
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnGroggy");

                //* Groggy 진입 애님 Length가 1초라고 1초간 Groggy 액션 지속함
                return Observable.Timer(TimeSpan.FromSeconds(1f))
                    .DoOnCancel(() =>
                    {
                        if (CurrActionName == "!OnGroggy")
                            FinishAction();
                    })
                    .DoOnCompleted(() =>
                    {
                        if (CurrActionName == "!OnGroggy")
                            FinishAction();
                    })
                    .Subscribe().AddTo(this);
            }
        }

        public override IDisposable StartOnParriedAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.senderBrain == __brain);

            __Logger.LogF(gameObject, nameof(StartOnParriedAction), "-", "Distance", damageContext.senderBrain.coreColliderHelper.GetDistanceBetween(damageContext.receiverBrain.coreColliderHelper));

            __brain.AnimCtrler.mainAnimator.SetTrigger("OnParried");

            //* Groogy 애님의 RootMotion 배율 짧게 조정
            currActionContext.rootMotionMultiplier = 0f;

            var knockBackDistance = 0f;
            if (damageContext.actionResult == ActionResults.KickParried)
                knockBackDistance = damageContext.receiverActionData.knockBackDistance;
            else if (damageContext.actionResult == ActionResults.GuardParried)
                knockBackDistance = DatasheetManager.Instance.GetActionData(damageContext.receiverBrain.PawnBB.common.pawnId, "GuardParry")?.knockBackDistance ?? 0f;
            else
                Debug.Assert(false);

            if (knockBackDistance <= 0f)
                __Logger.WarningF(gameObject, nameof(StartOnParriedAction), "knockBackDistance is zero", "knockBackDistance", knockBackDistance);

            var knockBackVec = __brain.BB.pawnData_Movement.knockBackSpeed * damageContext.receiverBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            return Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(knockBackDistance / __brain.BB.pawnData_Movement.knockBackSpeed)))
                .DoOnCancel(() =>
                {
                    if (CurrActionName == "!OnParried")
                        FinishAction();
                })
                .DoOnCompleted(() =>
                {
                    if (CurrActionName == "!OnParried")
                        FinishAction();
                })
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity))
                .AddTo(this);
        }

        WorkerBrain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<WorkerBrain>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            __brain.BB.decision.isGuarding.Subscribe(v =>
            {
                __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", v);
            }).AddTo(this);

            __brain.BB.common.isDown.Where(v => !v).Subscribe(_ =>
            {
                //* 일어나는 모션동안은 무적
                __brain.PawnStatusCtrler.AddStatus(PawnStatus.Invincible, 1f, 1f);
                __brain.AnimCtrler.mainAnimator.SetBool("IsDown", false);
                __brain.InvalidateDecision(1f);
            }).AddTo(this);

            __brain.BB.common.isGroggy.Where(v => !v).Subscribe(v =>
            {
                if (!__brain.PawnStatusCtrler.CheckStatus(PawnStatus.KnockDown))
                {
                    //* 일어나는 모션동안은 무적
                    __brain.PawnStatusCtrler.AddStatus(PawnStatus.Invincible, 1f, 1f);
                    __brain.AnimCtrler.mainAnimator.SetBool("IsGroggy", false);
                    __brain.InvalidateDecision(1f);
                }
            }).AddTo(this);
        }
    }
}