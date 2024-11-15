using System;
using System.Linq;
using UniRx;
using UniRx.Triggers.Extension;
using UnityEngine;

namespace Game
{
    public class HeroActionController : PawnActionController
    {
        public Collider swordCollider;
        public MeshRenderer shieldMesh;

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
            return IsActiveParryEnabled || __brain.BuffCtrler.CheckBuff(BuffTypes.PassiveParrying);
        }

        public override bool CanBlockAction(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if ((!__brain.BB.IsGuarding && !__brain.BB.IsAutoGuardEnabled) || __brain.BB.IsJumping == true)
                return false;
            if (!__brain.ActionCtrler.CheckActionRunning() == false)
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

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                var hitVec = damageContext.senderBrain.coreColliderHelper.transform.position - damageContext.receiverBrain.coreColliderHelper.transform.position;
                hitVec = damageContext.receiverBrain.coreColliderHelper.transform.InverseTransformDirection(hitVec).Vector2D().normalized;

                __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");
                __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 0);
                __brain.AnimCtrler.mainAnimator.SetFloat("HitX", hitVec.x);
                __brain.AnimCtrler.mainAnimator.SetFloat("HitY", hitVec.z);

                SoundManager.Instance.Play(SoundID.HIT_FLESH);
                EffectManager.Instance.Show("@Hit 23 cube", damageContext.hitPoint, Quaternion.identity, Vector3.one, 1f);
                EffectManager.Instance.Show("@BloodFX_impact_col", damageContext.hitPoint, Quaternion.identity, 1.5f * Vector3.one, 3f);
            }
            else if (damageContext.actionResult == ActionResults.GuardBreak)
            {
                __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", true);
                Observable.Timer(TimeSpan.FromSeconds(0.2f)).Subscribe(_ => 
                {
                    if (!__brain.BB.IsGuarding)
                        __brain.AnimCtrler.mainAnimator.SetBool("IsGuarding", false);
                }).AddTo(this);

                SoundManager.Instance.Play(SoundID.HIT_BLOCK);
                EffectManager.Instance.Show("@Hit 4 yellow arrow", __brain.AnimCtrler.shieldSocket.position, Quaternion.identity, Vector3.one, 1f);
            }
            else //* Sender의 액션을 파훼된 경우
            {
                if (damageContext.actionResult == ActionResults.ActiveParried)
                {
                    var hitPoint = damageContext.senderBrain.coreColliderHelper.GetCenter() + damageContext.senderBrain.coreColliderHelper.GetRadius() * (__brain.coreColliderHelper.GetCenter() - damageContext.senderBrain.coreColliderHelper.GetCenter()).Vector2D().normalized;
                    EffectManager.Instance.Show("Hit 26 blue crystal", hitPoint, Quaternion.identity, 2f * Vector3.one, 1f);
                    SoundManager.Instance.Play(SoundID.HIT_PARRYING);

                    return null;
                }
                else if (damageContext.actionResult == ActionResults.PassiveParried)
                {
                    __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 2);
                    __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");

                    var hitPoint = damageContext.senderBrain.coreColliderHelper.GetCenter() + damageContext.senderBrain.coreColliderHelper.GetRadius() * (__brain.coreColliderHelper.GetCenter() - damageContext.senderBrain.coreColliderHelper.GetCenter()).Vector2D().normalized;
                    EffectManager.Instance.Show("Hit 26 blue crystal", hitPoint, Quaternion.identity, 2f * Vector3.one, 1f);
                    SoundManager.Instance.Play(SoundID.HIT_PARRYING);
                }
                else if (damageContext.actionResult == ActionResults.Blocked)
                {
                    __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 1);
                    __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");

                    SoundManager.Instance.Play(SoundID.HIT_BLOCK);
                    EffectManager.Instance.Show("@Hit 4 yellow arrow", __brain.AnimCtrler.shieldSocket.position, Quaternion.identity, Vector3.one, 1f);
                }
            }

            var actionResult = damageContext.actionResult;
            var knockBackVec = damageContext.senderActionData.knockBackDistance / 0.2f * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            var knockBackDisposable = Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                .DoOnCancel(() => __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 0))
                .DoOnCompleted(() => __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 0))
                .Subscribe(_ =>
                {
                    if (actionResult != ActionResults.PassiveParried)
                        __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity);
                }).AddTo(this);

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

        public override IDisposable StartOnBlockedAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.senderBrain == __brain);
            Debug.Assert(damageContext.receiverActionData != null);
            Debug.Assert(!isAddictiveAction);

            __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");
            __brain.AnimCtrler.mainAnimator.SetInteger("HitType", 3);

            var knockBackVec = damageContext.receiverActionData.knockBackDistance / 0.2f * damageContext.receiverBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            var knockBackDisposable = Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                .Subscribe(_ =>
                {
                    __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity);
                }).AddTo(this);

            return null;
        }

        public override IDisposable StartOnKnockDownAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);
            Debug.Assert(!isAddictiveAction);

            __brain.AnimCtrler.mainAnimator.SetBool("IsDown", true);
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnDown");

            //* Down (Loop) 스테이트에서 애님 클립이 진행되지 않고 강제로 멈춰있도록 함
            __brain.AnimCtrler.mainAnimator.SetFloat("AnimSpeed", 1);
            __brain.AnimCtrler.mainAnimator.SetFloat("AnimAdvance", 99f);

            var knockBackVec = damageContext.senderActionData.knockBackDistance / 0.2f * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity))
                .AddTo(this);

            if (__downStateStartDisposable == null)
            {
                var downStartBehaviour = __brain.AnimCtrler.mainAnimator.GetBehaviours<ObservableStateMachineTriggerEx>().First(s => s.stateName == "Down (Start)");
                Debug.Assert(downStartBehaviour != null);

                __downStateStartDisposable = downStartBehaviour.OnStateExitAsObservable().Subscribe(s =>
                {
                    __brain.coreColliderHelper.gameObject.layer = LayerMask.NameToLayer("PawnOverlapped");
                }).AddTo(this);
            }

            return Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverPenalty.Item2))
                .DoOnCancel(() =>
                {
                    __brain.coreColliderHelper.gameObject.layer = LayerMask.NameToLayer("Pawn");
                    __brain.AnimCtrler.mainAnimator.SetBool("IsDown", false);
                    if (CurrActionName == "!OnKnockDown")
                        FinishAction();
                })
                .Subscribe(_ =>
                {
                    __brain.coreColliderHelper.gameObject.layer = LayerMask.NameToLayer("Pawn");
                    __brain.AnimCtrler.mainAnimator.SetBool("IsDown", false);
                    if (CurrActionName == "!OnKnockDown")
                        FinishAction();
                }).AddTo(this);
        }
        IDisposable __downStateStartDisposable;

        public override IDisposable StartOnGroogyAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);
            Debug.Assert(!isAddictiveAction);

            __brain.AnimCtrler.mainAnimator.SetBool("IsStunned", true);
            __brain.AnimCtrler.mainAnimator.SetTrigger("OnStunned");

            return Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverPenalty.Item2))
                .DoOnCancel(() =>
                {
                    __brain.AnimCtrler.mainAnimator.SetBool("IsStunned", false);
                    if (CurrActionName == "!OnGroggy")
                        FinishAction();
                })
                .Subscribe(_ =>
                {
                    __brain.AnimCtrler.mainAnimator.SetBool("IsStunned", false);
                    if (CurrActionName == "!OnGroggy")
                        FinishAction();
                }).AddTo(this);
        }

        HeroBrain __brain;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<HeroBrain>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            onActiveParryEnabled += (_) => parryHitColliderHelper.pawnCollider.enabled = currActionContext.activeParryEnabled;
            onActionCanceled += (_, __) => parryHitColliderHelper.pawnCollider.enabled = false;
            onActionFinished += (_) => parryHitColliderHelper.pawnCollider.enabled = false;

            onEmitProjectile += OnEmitProjectile;
        }

        void OnEmitProjectile(ActionContext context, ProjectileMovement proj, Transform point, int num) 
        {
            var obj = GameObject.Instantiate(proj);

            var trRoot = __brain.CoreTransform;

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
