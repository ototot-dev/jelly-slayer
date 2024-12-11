using System;
using System.Linq;
using UniRx;
using UnityEngine;
using XftWeapon;

namespace Game
{
    public class SoldierActionController : PawnActionController
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

            if (__brain.StatusCtrler.CheckStatus(PawnStatus.Staggered))
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
                if (damageContext.receiverBrain.PawnBB.IsGroggy)
                {
                    __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 3);
                    __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");
                }
                else
                {
                    var hitVec = damageContext.receiverBrain.CoreTransform.position - damageContext.senderBrain.CoreTransform.position;
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
            else if (damageContext.actionResult == ActionResults.Missed)
            {
                SoundManager.Instance.Play(SoundID.HIT_BLOCK);
                EffectManager.Instance.Show("@Hit 4 yellow arrow", __brain.AnimCtrler.shieldMeshSlot.position, Quaternion.identity, Vector3.one, 1f);
            }
            else if (damageContext.actionResult == ActionResults.Blocked)
            {
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");
                __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 1);

                SoundManager.Instance.Play(SoundID.HIT_BLOCK);
                EffectManager.Instance.Show("@Hit 4 yellow arrow", __brain.AnimCtrler.shieldMeshSlot.position, Quaternion.identity, Vector3.one, 1f);
                EffectManager.Instance.Show("BlockAttack", __brain.AnimCtrler.shieldMeshSlot.position, Quaternion.identity, Vector3.one, 1f);
            }
            else if (damageContext.actionResult == ActionResults.GuardBreak) 
            {
                __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 2);
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");
                __brain.PawnStatusCtrler.AddStatus(PawnStatus.Guardbreak, duration: 1.0f);

                SoundManager.Instance.Play(SoundID.GUARD_BREAK);
                EffectManager.Instance.Show("SwordHitRed", __brain.AnimCtrler.shieldMeshSlot.position, Quaternion.identity, Vector3.one, 1f);
            }

            var knockBackVec = __brain.BB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            if (damageContext.actionResult == ActionResults.Missed || damageContext.actionResult == ActionResults.Blocked)
            {
                Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f / __brain.BB.pawnData_Movement.knockBackSpeed)))
                    .DoOnCancel(() => __brain.Movement.Freeze())
                    .DoOnCompleted(() => __brain.Movement.Freeze())
                    .Subscribe(_ => __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity)).AddTo(this);
            }
            else
            {            
                Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.senderActionData.knockBackDistance / __brain.BB.pawnData_Movement.knockBackSpeed)))
                    .DoOnCancel(() => __brain.Movement.Freeze())
                    .DoOnCompleted(() => __brain.Movement.Freeze())
                    .Subscribe(_ => __brain.Movement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity)).AddTo(this);
            }

            return null;
        }

        public override IDisposable StartOnBlockedAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.senderBrain == __brain);
            Debug.Assert(damageContext.receiverActionData != null);
            Debug.Assert(!isAddictiveAction);

            __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");
            __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 3);

            var knockBackVec = __brain.BB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverActionData.knockBackDistance / __brain.BB.pawnData_Movement.knockBackSpeed)))
                .DoOnCancel(() => __brain.Movement.Freeze())
                .DoOnCompleted(() => __brain.Movement.Freeze())
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity)).AddTo(this);

            return null;
        }

        public override IDisposable StartOnParriedAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.senderBrain == __brain);

            __Logger.LogF(gameObject, nameof(StartOnParriedAction), "-", "Distance", damageContext.senderBrain.coreColliderHelper.GetDistanceBetween(damageContext.receiverBrain.coreColliderHelper));
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnParried");

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
            Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(knockBackDistance / __brain.BB.pawnData_Movement.knockBackSpeed)))
                .DoOnCancel(() => __brain.Movement.Freeze())
                .DoOnCompleted(() => __brain.Movement.Freeze())
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity)).AddTo(this);

            return null;
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

                var knockBackVec = __brain.BB.pawnData_Movement.knockBackSpeed * damageContext.receiverBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(knockBackDistance / __brain.BB.pawnData_Movement.knockBackSpeed)))
                    .DoOnCancel(() =>
                    {
                        __brain.Movement.Freeze();
                        //* KnockBack 연출 후에 Groogy 모션 진입
                        __brain.AnimCtrler.mainAnimator.SetBool("IsGroggy", true);
                        __brain.AnimCtrler.mainAnimator.SetTrigger("OnGroggy");
                    })
                    .DoOnCompleted(() =>
                    {
                        __brain.Movement.Freeze();
                        //* KnockBack 연출 후에 Groogy 모션 진입
                        __brain.AnimCtrler.mainAnimator.SetBool("IsGroggy", true);
                        __brain.AnimCtrler.mainAnimator.SetTrigger("OnGroggy");
                    })
                    .Subscribe(_ => __brain.Movement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity))
                    .AddTo(this);
            }
            else
            {
                __brain.AnimCtrler.mainAnimator.SetBool("IsGroggy", true);
                __brain.AnimCtrler.mainAnimator.SetTrigger("OnGroggy");
            }

            return null;
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

            __brain.BB.decision.isGuarding.Subscribe(v =>
            {
                __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", v);
            }).AddTo(this);

            __brain.BB.common.isDown.Subscribe(v =>
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
                    __brain.InvalidateDecision(2f);
                }
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