using System;
using FIMSpace.Generating.Rules.Modelling;
using NodeCanvas.Framework.Internal;
using UniRx;
using UnityEngine;

namespace Game
{
    public class DroneBotActionController : PawnActionController
    {
        [Header("Component")]
        public CapsuleCollider hitBox;
        public Transform emitPointL;
        public Transform emitPointR;
        public ParticleSystem plasmaExplosionFx;
        public ParticleSystem smallExplisionFx;
        public ParticleSystem smokeFx;
        public GameObject orbSmallYellowFx;
        public RopeHookController ropeHookCtrler;

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
        
        public override IDisposable StartOnHitAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.receiverBrain == __brain);

            var knockBackVec = damageContext.senderActionData.knockBackDistance / 0.2f * damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                var hitVec = damageContext.senderBrain.coreColliderHelper.transform.position - damageContext.receiverBrain.coreColliderHelper.transform.position;
                hitVec = damageContext.receiverBrain.coreColliderHelper.transform.InverseTransformDirection(hitVec).Vector2D().normalized;

                __brain.AnimCtrler.mainAnimator.SetTrigger("OnHit");

                if (hitVec.x < 0f)
                {
                    __brain.AnimCtrler.mainAnimator.SetFloat("HitX", hitVec.x);
                }
                else
                {
                    var hitX = UnityEngine.Random.Range(0, 3);
                    __brain.AnimCtrler.mainAnimator.SetFloat("HitX", hitX);
                }

                SoundManager.Instance.Play(SoundID.HIT_FLESH);
                EffectManager.Instance.Show("@Hit 23 cube", damageContext.hitPoint, Quaternion.identity, Vector3.one, 1);
                EffectManager.Instance.Show("@BloodFX_impact_col", damageContext.hitPoint, Quaternion.identity, 1.5f * Vector3.one, 3);
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

        public override IDisposable StartOnParriedAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false)
        {
            Debug.Assert(damageContext.senderBrain == __brain);
            Debug.Assert(!isAddictiveAction);

            var knockBackVec = damageContext.senderActionData.knockBackDistance / 0.2f * -damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
            Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                .Subscribe(_ => __brain.Movement.AddRootMotion(Time.deltaTime * knockBackVec, Quaternion.identity))
                .AddTo(this);

            return Observable.Timer(TimeSpan.FromSeconds(damageContext.receiverPenalty.Item2))
                .DoOnCancel(() =>
                {
                    if (CurrActionName == "!OnParried")
                        FinishAction();
                })
                .Subscribe(_ =>
                {
                    if (CurrActionName == "!OnParried")
                        FinishAction();
                }).AddTo(this);
        }

        public override IDisposable StartActionDisposable(ref PawnHeartPointDispatcher.DamageContext damageContext, string actionName)
        {
            if (actionName == "RopeHook")
            {
                Debug.Assert(__brain.BB.HostBrain.BB.TargetBrain != null);

                var targetColliderHelper = GameContext.Instance.HeroBrain.BB.TargetBrain.GetHookingColliderHelper();
                if (ropeHookCtrler.LaunchHook(targetColliderHelper.pawnCollider))
                {
                    __brain.BB.target.targetPawnHP.Value = targetColliderHelper.pawnBrain.PawnHP;
                    __brain.BB.decision.currDecision.Value = DroneBotBrain.Decisions.Hooking;
                }

                return null;
            }
            else if (actionName == "Assault")
            {
                Debug.Assert(__brain.BB.HostBrain.BB.TargetBrain != null);
                
                Vector3 assaultStartPosition = __brain.GetWorldPosition();
                float assaultTimeStamp = Time.time;
                float assaultDuration = 0.2f;

                return Observable.EveryUpdate().Select(_ => (Time.time - assaultTimeStamp) / assaultDuration).TakeWhile(a => a < 1f)
                    .DoOnCancel(() =>
                    {
                        __brain.BB.HostBrain.Movement.FinishHanging();
                        __brain.BB.HostBrain.Movement.StartJump(1f);
                    })
                    .DoOnCompleted(() =>
                    {
                        __brain.BB.HostBrain.Movement.FinishHanging();
                        __brain.BB.HostBrain.Movement.StartJump(1f);
                    })
                    .Subscribe(a =>
                    {
                        var alpha = ParadoxNotion.Animation.Easing.Ease(ParadoxNotion.Animation.EaseType.ExponentialIn, 0, 1f, a);
                        var targetPosition = __brain.BB.HostBrain.BB.TargetBrain.GetWorldPosition() + __brain.BB.HostBrain.Movement.capsuleCollider.height * Vector3.up;
                        __brain.Movement.GetCharacterMovement().SetPosition(Vector3.Lerp(assaultStartPosition, targetPosition, alpha));
                        __brain.Movement.GetCharacterMovement().SetRotation(Quaternion.LookRotation((targetPosition - __brain.GetWorldPosition()).Vector2D().normalized));
                    }).AddTo(this);
            }
            
            return base.StartActionDisposable(ref damageContext, actionName);
        }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<DroneBotBrain>();
        }

        DroneBotBrain __brain;
        EffectInstance __hookingPointFx;

        protected override void StartInternal()
        {
            base.StartInternal();

            __brain.PawnHP.onDamaged += (damageContext) =>
            {
                if (damageContext.senderBrain == this && CheckActionRunning() && CurrActionName == "Bumping")
                    currActionContext.rootMotionEnabled = false;
            };

            ropeHookCtrler.onRopeHooked += (_) =>
            {
                __hookingPointFx = EffectManager.Instance.ShowLooping(orbSmallYellowFx, ropeHookCtrler.hookingCollider.transform.position, Quaternion.identity, Vector3.one);

                Observable.EveryLateUpdate().TakeWhile(_ => __hookingPointFx != null).Subscribe(_ =>
                {
                    Debug.Assert(ropeHookCtrler.hookingCollider != null);

                    __hookingPointFx.transform.position = ropeHookCtrler.hookingCollider.transform.position;
                    __hookingPointFx.transform.position += (ropeHookCtrler.SourceCollider.transform.position - ropeHookCtrler.hookingCollider.transform.position).normalized * ropeHookCtrler.hookingCollider.GetComponent<PawnColliderHelper>().GetRadius();
                    __hookingPointFx.transform.position -= GameContext.Instance.cameraCtrler.viewCamera.transform.forward;

                }).AddTo(this);
            };

            ropeHookCtrler.onRopeReleased += (_) =>
            {
                if (__hookingPointFx != null)
                {
                    __hookingPointFx.Stop();
                    __hookingPointFx = null;
                }
            };
        }

        public override void EmitProjectile(GameObject emitSource, Transform emitPoint, int emitNum)
        {
            if (Instantiate(emitSource, emitPoint.position, emitPoint.rotation).TryGetComponent<DroneBotBullet>(out var bullet))
                bullet.Go(__brain, 16f);

            Observable.Interval(TimeSpan.FromSeconds(0.2888f)).Take(emitNum - 1).Subscribe(_ =>
            {
                if (Instantiate(emitSource, emitPoint.position, emitPoint.rotation).TryGetComponent<DroneBotBullet>(out var bullet))
                    bullet.Go(__brain, 16f);
            }).AddTo(this);

            //* onEmitProjectile 호출은 제일 나중에 함
            base.EmitProjectile(emitSource, emitPoint, emitNum);
        }
    }
}  