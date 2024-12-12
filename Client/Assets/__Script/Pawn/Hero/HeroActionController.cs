using System;
using System.Linq;
using UniRx;
using UniRx.Triggers.Extension;
using UnityEngine;

namespace Game
{
    public class HeroActionController : PawnActionController
    {
        [Header("Component")]
        public GameObject staggerFx;
        public Transform fxAttachPoint;
        public MeshRenderer shieldMesh;
        public Collider kickActionCollider;
        public Collider swordActionCollider;

        public override bool CanRootMotion(Vector3 rootMotionVec)
        {
            if (!base.CanRootMotion(rootMotionVec) || __brain.BB.IsRolling)
                return false;

            if (__brain.BB.TargetBrain != null && __brain.SensorCtrler.TouchingColliders.Contains(__brain.BB.TargetBrain.coreColliderHelper.pawnCollider))
            {
                var currDistance = (__brain.BB.TargetBrain.coreColliderHelper.transform.position - __brain.coreColliderHelper.transform.position).Vector2D().sqrMagnitude;
                var newDistance = (__brain.BB.TargetBrain.coreColliderHelper.transform.position - (__brain.coreColliderHelper.transform.position + rootMotionVec)).Vector2D().sqrMagnitude;

                //* RootMotion으로 목표물을 밀지 않도록 목묘물의 TouchingColliders와 접축할 정도로 가깝다면 rootMotionVec가 목표물에서 멀어지는 방향일때만 적용해준다.
                return newDistance > currDistance;
            }
            else
            {
                return true;
            }
        }

        public override bool CanParryAction(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            return currActionContext.activeParryEnabled && damageContext.receiverBrain.coreColliderHelper.GetDistanceBetween(damageContext.senderBrain.coreColliderHelper) < 1f || 
                __brain.StatusCtrler.CheckStatus(PawnStatus.GuardParrying);
        }

        public override bool CanBlockAction(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if ((!__brain.BB.IsGuarding && !__brain.BB.IsAutoGuardEnabled) || __brain.BB.IsJumping == true)
                return false;
            if (__brain.ActionCtrler.CheckActionRunning() || __brain.StatusCtrler.CheckStatus(PawnStatus.Staggered) || __brain.StatusCtrler.CheckStatus(PawnStatus.CanNotGuard))
                return false;
            if (__brain.SensorCtrler.WatchingColliders.Contains(damageContext.senderBrain.coreColliderHelper.pawnCollider) == false)
                return false;
            if (damageContext.receiverBrain.PawnBB.stat.stamina.Value < damageContext.senderActionData.guardStaminaDamage)
                return false;

            return true;
        }

        public override IDisposable StartOnHitAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);

            if (damageContext.actionResult == ActionResults.Damaged || damageContext.actionResult == ActionResults.GuardBreak)
            {
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

                //* 경직 지속 시간과 맞춰주기 위해서 'AnimSpeed' 값을 조정함
                __brain.AnimCtrler.mainAnimator.SetFloat("AnimSpeed", 1f / damageContext.receiverPenalty.Item2);

                if (damageContext.actionResult == ActionResults.Damaged)
                {
                    SoundManager.Instance.Play(SoundID.HIT_FLESH);
                    EffectManager.Instance.Show("@Hit 23 cube", damageContext.hitPoint, Quaternion.identity, Vector3.one, 1f);
                    EffectManager.Instance.Show("@BloodFX_impact_col", damageContext.hitPoint, Quaternion.identity, 1.5f * Vector3.one, 3f);
                }
                else
                {
                    SoundManager.Instance.Play(SoundID.HIT_BLOCK);
                    EffectManager.Instance.Show("@Hit 4 yellow arrow", __brain.AnimCtrler.shieldMeshSlot.position, Quaternion.identity, Vector3.one, 1f);
                }

                var knockBackVec = __brain.BB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.senderActionData.knockBackDistance / __brain.BB.pawnData_Movement.knockBackSpeed)))
                    .DoOnCancel(() => __brain.Movement.Freeze())
                    .DoOnCompleted(() => __brain.Movement.Freeze())
                    .Subscribe(_ => __brain.Movement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity))
                    .AddTo(this);
            }
            else //* Sender의 액션을 파훼된 경우
            {
                if (damageContext.actionResult == ActionResults.Blocked)
                {
                    __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 1);
                    __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");
                    __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", true);

                    Observable.Timer(TimeSpan.FromSeconds(0.5f)).Subscribe(_ => 
                    {
                        if (!__brain.BB.IsGuarding)
                            __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", false);
                    }).AddTo(this);

                    SoundManager.Instance.Play(SoundID.HIT_BLOCK);
                    EffectManager.Instance.Show("@Hit 4 yellow arrow", __brain.AnimCtrler.shieldMeshSlot.position, Quaternion.identity, Vector3.one, 1f);
                    EffectManager.Instance.Show("BlockAttack", __brain.AnimCtrler.shieldMeshSlot.position, Quaternion.identity, Vector3.one, 1f);
                }
                else if (damageContext.actionResult == ActionResults.GuardParried)
                {
                    __brain.AnimCtrler.mainAnimator.SetTrigger("OnGuardParry");

                    var senderHelper = damageContext.senderBrain.coreColliderHelper;
                    var hitPoint = senderHelper.GetCenter() + senderHelper.GetRadius() * 
                        (__brain.coreColliderHelper.GetCenter() - senderHelper.GetCenter()).Vector2D().normalized;

                    EffectManager.Instance.Show("Hit 26 blue crystal", hitPoint, Quaternion.identity, 3f * Vector3.one, 1f);
                    EffectManager.Instance.Show("BasicSparkExplosion", hitPoint, Quaternion.identity, 1f * Vector3.one, 1f);
                    SoundManager.Instance.Play(SoundID.HIT_PARRYING);
                }
                else if (damageContext.actionResult == ActionResults.KickParried)
                {
                    var hitPoint = damageContext.senderBrain.coreColliderHelper.GetCenter() + 
                        damageContext.senderBrain.coreColliderHelper.GetRadius() * (__brain.coreColliderHelper.GetCenter() - damageContext.senderBrain.coreColliderHelper.GetCenter()).Vector2D().normalized;

                    EffectManager.Instance.Show("Hit 26 blue crystal", hitPoint, Quaternion.identity, 2f * Vector3.one, 1f);
                    SoundManager.Instance.Play(SoundID.HIT_PARRYING);
                }

                if (damageContext.actionResult == ActionResults.Blocked || damageContext.actionResult == ActionResults.GuardParried)
                {
                    var knockBackVec = __brain.BB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                    Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.5f / __brain.BB.pawnData_Movement.knockBackSpeed)))
                        .DoOnCancel(() => __brain.Movement.Freeze())
                        .DoOnCompleted(() => __brain.Movement.Freeze())
                        .Subscribe(_ => __brain.Movement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity))
                        .AddTo(this);
                }
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

            var knockBackVec = __brain.BB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.GetWorldTransform().forward.Vector2D().normalized;
            Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverActionData.knockBackDistance / __brain.BB.pawnData_Movement.knockBackSpeed)))
                .DoOnCancel(() =>
                {
                    __brain.Movement.Freeze();
                    if (CurrActionName == "!OnBlocked")
                        FinishAction();
                })
                .DoOnCompleted(() =>
                {
                    __brain.Movement.Freeze();
                    if (CurrActionName == "!OnBlocked")
                        FinishAction();
                })
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity)).AddTo(this);

            return null;
        }

        public override IDisposable StartOnKnockDownAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);
            Debug.Assert(!isAddictiveAction);

            var knockBackVec = __brain.BB.pawnData_Movement.knockBackSpeed * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            Observable.EveryFixedUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(damageContext.senderActionData.knockBackDistance / __brain.BB.pawnData_Movement.knockBackSpeed)))
                .DoOnCancel(() => __brain.Movement.Freeze())
                .DoOnCompleted(() => __brain.Movement.Freeze())
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.fixedDeltaTime * knockBackVec, Quaternion.identity))
                .AddTo(this);

            return null;
        }

        HeroBrain __brain;
        EffectInstance __staggerFxInstance;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<HeroBrain>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            __brain.BB.common.isDown.Subscribe(v =>
            {
                if (v)
                {
                    __brain.AnimCtrler.mainAnimator.SetBool("IsDown", true);
                    __brain.AnimCtrler.mainAnimator.SetTrigger("OnDown");

                    //* Down (Loop) 스테이트에서 애님 클립이 진행되지 않고 강제로 멈춰있도록 함
                    __brain.AnimCtrler.mainAnimator.SetFloat("AnimSpeed", 1);
                    __brain.AnimCtrler.mainAnimator.SetFloat("AnimAdvance", 99f);
                }
                else
                {
                    //* 일어나는 모션동안은 무적
                    __brain.PawnStatusCtrler.AddStatus(PawnStatus.Invincible, 1f, 1f);
                    __brain.AnimCtrler.mainAnimator.SetBool("IsDown", false);
                }
            }).AddTo(this);

            __brain.BB.action.isJumping.Skip(1).Subscribe(v =>
            {
                if (v)
                {
                    EffectManager.Instance.Show("FX_Cartoony_Jump_Up_01", __brain.GetWorldPosition(), 
                        Quaternion.identity, Vector3.one, 1f);
                    SoundManager.Instance.Play(SoundID.JUMP);
                }
                else
                {
                    EffectManager.Instance.Show("JumpCloudSmall", __brain.GetWorldPosition() + 
                        Time.deltaTime * __brain.Movement.moveSpeed * __brain.Movement.moveVec + 
                        0.1f * Vector3.up, Quaternion.identity, 0.8f * Vector3.one, 1f);
                    SoundManager.Instance.Play(SoundID.LAND);
                }
            }).AddTo(this);

            __brain.PawnStatusCtrler.onStatusActive += (status) =>
            {
                // if (status == PawnStatus.Staggered && __staggerFxInstance == null)
                // {
                //     __staggerFxInstance = EffectManager.Instance.ShowLooping(staggerFx, fxAttachPoint.position, Quaternion.identity, Vector3.one);
                //     __staggerFxInstance.transform.SetParent(fxAttachPoint);
                // }
            };

            __brain.PawnStatusCtrler.onStatusDeactive += (status) =>
            {
                //* 경직 종료 후에 짧은 시간 동안 가드 불가 부여 
                if (status == PawnStatus.Staggered)
                {
                    __brain.PawnStatusCtrler.AddStatus(PawnStatus.CanNotGuard, 1f, MainTable.PlayerData.GetList().First().canNotGuardStaggerDuration);
                    // __staggerFxInstance?.Stop();
                    // __staggerFxInstance = null;
                }
            };

            onActiveParryEnabled += (_) => parryHitColliderHelper.pawnCollider.enabled = currActionContext.activeParryEnabled;
            onActionCanceled += (_, __) => parryHitColliderHelper.pawnCollider.enabled = false;
            onActionFinished += (_) => parryHitColliderHelper.pawnCollider.enabled = false;

            onEmitProjectile += OnEmitProjectile;
        }

        void OnEmitProjectile(ActionContext context, ProjectileMovement proj, Transform point, int num) 
        {
            var obj = GameObject.Instantiate(proj);

            var trRoot = __brain.GetWorldTransform();

            var chainProj = obj.GetComponent<HeroChainShotProjectile>();
            var pos = (trRoot.position + Vector3.up) + trRoot.forward;
            chainProj.emitterBrain = __brain;
            chainProj.Go(null, pos, 10.0f * trRoot.forward, Vector3.one);
        }

        void OnDrawGizmos()
        {
            // if (swordCollider != null)
            //     GizmosDrawExtension.DrawBox(swordCollider.transform.position + swordCollider.transform.rotation * (swordCollider as BoxCollider).center, swordCollider.transform.rotation , 0.5f * (swordCollider as BoxCollider).size);
        }
    }
}

// void RollingEffect() 
// {
//     // 롤링 이펙트
//     var trRoot = __brain.coreColliderHelper.transform;
//     var pos = trRoot.position + (0.2f * Vector3.up);
//     EffectManager.Instance.Show("JumpCloudSmall", pos, Quaternion.identity, 0.8f * Vector3.one, 0.4f);

//     SoundManager.Instance.Play(SoundID.ROLL);
// }
