using System;
using NodeCanvas.Framework.Internal;
using UniRx;
using UnityEngine;

namespace Game
{
    public class DroneBotActionController : PawnActionController
    {
        [Header("Component")]
        public PawnColliderHelper hitColliderHelper;
        public PawnColliderHelper assaultActionColliderHelper;
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

            //* RootMotion이 켜져 있다면 movementEnabled은 반드시 꺼져 있어야 함 (RootMotion 벡터값이 이동 벡터값에 영향을 줌)
            Debug.Assert(!currActionContext.movementEnabled);

            if (__brain.BB.TargetBrain != null && __brain.SensorCtrler.TouchingColliders.Contains(__brain.BB.TargetBrain.coreColliderHelper.pawnCollider))
            {
                //* RootMotion으로 목표물을 밀지 않도록 목표물의 TouchingColliders와 접축할 정도로 가깝다면 rootMotionVec가 목표물에서 멀어지는 방향일때만 적용해준다.
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
                EffectManager.Instance.Show("FX/@Hit 23 cube", damageContext.hitPoint, Quaternion.identity, Vector3.one, 1);
                EffectManager.Instance.Show("FX/@BloodFX_impact_col", damageContext.hitPoint, Quaternion.identity, 1.5f * Vector3.one, 3);
            }

            var knockBackDisposable = Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                .Subscribe(_ => __brain.Movement.AddRootMotion(knockBackVec, Quaternion.identity, Time.deltaTime))
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
                .Subscribe(_ => __brain.Movement.AddRootMotion(knockBackVec, Quaternion.identity, Time.deltaTime))
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

        public override IDisposable StartCustomAction(ref PawnHeartPointDispatcher.DamageContext damageContext, string actionName, bool isAddictiveAction = false)
        {
            Debug.Assert(!isAddictiveAction);

            if (actionName == "Hook")
            {
                Debug.Assert(__brain.BB.HostBrain.BB.TargetBrain != null);

                var targetColliderHelper = GameContext.Instance.playerCtrler.possessedBrain.BB.TargetBrain.GetHookingColliderHelper();
                if (ropeHookCtrler.LaunchHook(targetColliderHelper.pawnCollider))
                {
                    __brain.BB.target.targetPawnHP.Value = targetColliderHelper.pawnBrain.PawnHP;
                    __brain.BB.decision.currDecision.Value = DroneBotBrain.Decisions.Hooking;
                }

                __hookingPointFx = EffectManager.Instance.ShowLooping(orbSmallYellowFx, ropeHookCtrler.obiTargetCollider.transform.position, Quaternion.identity, Vector3.one);
                __hookingPointFxUpdateDisposable = Observable.EveryUpdate()
                    .DoOnCancel(() =>
                    {
                        __hookingPointFx.Stop();
                        __hookingPointFx = null;
                    })
                    .DoOnCompleted(() =>
                    {
                        __hookingPointFx.Stop();
                        __hookingPointFx = null;
                    })
                    .Subscribe(_ =>
                    {
                        Debug.Assert(ropeHookCtrler.obiTargetCollider != null);

                        // __hookingPointFx.transform.position = ropeHookCtrler.hookingCollider.transform.position;
                        __hookingPointFx.transform.position = ropeHookCtrler.GetLastParticlePosition();
                        //* 카메라 쪽으로 위치를 당겨서 겹쳐서 안보이게 되는 경우를 완화함
                        __hookingPointFx.transform.position -= GameContext.Instance.cameraCtrler.gameCamera.transform.forward;
                    }).AddTo(this);

                return null;
            }
            else if (actionName == "Unhook")
            {
                ropeHookCtrler.DetachHook();
                __brain.BB.target.targetPawnHP.Value = null;
                __brain.InvalidateDecision(0.1f);

                return null;
            }
            else if (actionName == "Assault")
            {
                Debug.Assert(__brain.BB.HostBrain.BB.TargetBrain != null);

                __assaultActionTargetBrain = __brain.BB.HostBrain.BB.TargetBrain;

                if (__brain.BB.IsHanging)
                {
                    __assaultActionLayerMaskCached = __brain.BB.HostBrain.Movement.GetCharacterMovement().collisionLayers;

                    //* Assault 액션 중에는 뚫는 것을 허용하도록 레이어값 변경
                    __brain.BB.HostBrain.Movement.capsule.gameObject.layer = LayerMask.NameToLayer("Default");
                    __brain.BB.HostBrain.Movement.GetCharacterMovement().collisionLayers = LayerMask.GetMask("Floor", "Obstacle");

                    __brain.BB.resource.jetBoostFx.Play(true);
                    __brain.BB.resource.orbBlueFx.Play(true);

                    //* Hanging 상태라면 바로 Assault 액션을 수행함
                    AssaultActionObservable(__brain.GetWorldPosition(), Time.time, 0.2f).Subscribe().AddTo(this);
                }
                else
                {
                    Debug.Assert(!__brain.BB.HostBrain.BB.IsJumping);

                    //* Hanging 상태가 아니라면 강제로 Hanging 상태로 변경시킴 
                    __brain.BB.HostBrain.Movement.StartJump(1f);
                    __brain.BB.HostBrain.Movement.PrepareHanging(__brain, 0.1f);

                    __assaultActionLayerMaskCached = __brain.BB.HostBrain.Movement.GetCharacterMovement().collisionLayers;

                    //* Assault 액션 중에는 뚫는 것을 허용하도록 레이어값 변경
                    __brain.BB.HostBrain.Movement.capsule.gameObject.layer = LayerMask.NameToLayer("Default");
                    __brain.BB.HostBrain.Movement.GetCharacterMovement().collisionLayers = LayerMask.GetMask("Floor", "Obstacle");

                    var startPosition = __brain.BB.HostBrain.GetWorldPosition().AdjustY(__brain.Movement.GetHangingPointOffsetVector().y + 0.5f);
                    var teleportFx = EffectManager.Instance.Show(__brain.BB.resource.protonExplosionFx, __brain.GetWorldPosition(), __brain.GetWorldRotation(), 0.5f * Vector3.one, 0.1f);

                    return Observable.Timer(TimeSpan.FromSeconds(0.1f)).Do(_ =>
                        {
                            __brain.BB.resource.jetBoostFx.Play(true);
                            __brain.BB.resource.orbBlueFx.Play(true);
                        })
                        .ContinueWith(AssaultActionObservable(startPosition, Time.time + 0.1f, 0.2f)).Subscribe().AddTo(this);
                }
            }
            else if (actionName == "Heal")
            {
                __brain.InvalidateDecision();
                __brain.BB.decision.currDecision.Value = DroneBotBrain.Decisions.Heal;

                return HealActionObservable().Subscribe().AddTo(this);
            }

            return base.StartCustomAction(ref damageContext, actionName);
        }

        IObservable<float> AssaultActionObservable(Vector3 startPosition, float startTimeStamp, float duration)
        {
            return Observable.EveryLateUpdate().Select(_ => (Time.time - startTimeStamp) / duration).TakeWhile(v => v < 1.2f)
                .DoOnCancel(() =>
                {
                    __brain.BB.resource.jetBoostFx.Stop(true);
                    __brain.BB.resource.orbBlueFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                    __brain.BB.HostBrain.Movement.FinishHanging();
                    __brain.BB.HostBrain.Movement.StartJump(1f);
                    __brain.BB.HostBrain.BB.body.isJumping.Take(2).Skip(1).Subscribe(v =>
                    {
                        Debug.Assert(!v);
                        //* Capsule의 Layer를 원래대로 복구함
                        __brain.BB.HostBrain.Movement.capsule.gameObject.layer = LayerMask.NameToLayer("Pawn");
                        __brain.BB.HostBrain.Movement.GetCharacterMovement().collisionLayers = __assaultActionLayerMaskCached;
                    }).AddTo(this);
                })
                .DoOnCompleted(() =>
                {
                    __brain.BB.resource.jetBoostFx.Stop(true);
                    __brain.BB.resource.orbBlueFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

                    var assaultActionData = DatasheetManager.Instance.GetActionData(__brain.BB.HostBrain.BB.common.pawnId, "Assault");
                    Debug.Assert(assaultActionData != null);

                    __brain.BB.HostBrain.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(__brain.BB.HostBrain, __assaultActionTargetBrain, assaultActionData, __assaultActionTargetBrain.bodyHitColliderHelper.pawnCollider, false));
                    __brain.BB.HostBrain.Movement.FinishHanging();
                    __brain.BB.HostBrain.Movement.StartJump(1f);
                    __brain.BB.HostBrain.BB.body.isJumping.Take(2).Skip(1).Subscribe(v =>
                    {
                        Debug.Assert(!v);
                        //* Capsule의 Layer를 원래대로 복구함
                        __brain.BB.HostBrain.Movement.capsule.gameObject.layer = LayerMask.NameToLayer("Pawn");
                        __brain.BB.HostBrain.Movement.GetCharacterMovement().collisionLayers = __assaultActionLayerMaskCached;
                    }).AddTo(this);
                })
                .Do(v =>
                {
                    if (__brain.BB.IsHanging)
                    {
                        var alpha = ParadoxNotion.Animation.Easing.Ease(ParadoxNotion.Animation.EaseType.ExponentialIn, 0, 1f, v);
                        var targetPosition = __assaultActionTargetBrain.GetWorldPosition() + __brain.BB.HostBrain.Movement.capsuleCollider.height * Vector3.up;

                        //* 겹친 상태에서 뚫고 가지않도록 접근 최소 거리를 추가함
                        targetPosition -= __assaultActionTargetBrain.bodyHitColliderHelper.GetCapsuleRadius() * __brain.coreColliderHelper.transform.forward.Vector2D().normalized;

                        __brain.Movement.GetCharacterMovement().SetPosition(Vector3.Lerp(startPosition, targetPosition, alpha));
                        __brain.Movement.GetCharacterMovement().SetRotation(Quaternion.LookRotation((targetPosition - __brain.GetWorldPosition()).Vector2D().normalized));
                    }
                });
        }

        IObservable<long> HealActionObservable()
        {
            __brain.InvalidateDecision();
            __brain.BB.decision.currDecision.Value = DroneBotBrain.Decisions.Heal;

            //* 최초 1번은 즉시 회복시켜줌
            __brain.BB.HostBrain.PawnHP.Recover(__brain.BB.action.healAmount, string.Empty);

            //* RootMotion은 꺼줌
            currActionContext.rootMotionEnabled = false;

            return Observable.Interval(TimeSpan.FromSeconds(__brain.BB.action.healInterval)).Take(__brain.BB.action.healCount)
                .DoOnCompleted(() =>
                {
                    //* FSM 액션이 종료될 수 있도록, actionDisposable은 null로 셋팅 (WaitActionDisposable 태스크에서 액션이 종료됨) 
                    currActionContext.actionDisposable = null;
                    __brain.InvalidateDecision(0.1f);
                })
                .DoOnCancel(() =>
                {
                    __brain.InvalidateDecision(0.1f);
                })
                .Do(_ =>
                {
                    __brain.BB.HostBrain.PawnHP.Recover(__brain.BB.action.healAmount, string.Empty);
                });
        }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<DroneBotBrain>();
        }

        DroneBotBrain __brain;
        EffectInstance __hookingPointFx;
        IDisposable __hookingPointFxUpdateDisposable;
        PawnBrainController __assaultActionTargetBrain;
        LayerMask __assaultActionLayerMaskCached;

        protected override void StartInternal()
        {
            base.StartInternal();

            __brain.PawnHP.onDamaged += (damageContext) =>
            {
                if (damageContext.senderBrain == this && CheckActionRunning() && CurrActionName == "Bumping")
                    currActionContext.rootMotionEnabled = false;
            };

            ropeHookCtrler.onRopeReleased += (_) =>
            {
                if (__hookingPointFxUpdateDisposable != null)
                {
                    __hookingPointFxUpdateDisposable.Dispose();
                    __hookingPointFxUpdateDisposable = null;
                }
            };
        }

        public override void EmitActionHandler(GameObject emitSource, Transform emitPoint, int emitNum)
        {
            // if (Instantiate(emitSource, emitPoint.position, emitPoint.rotation).TryGetComponent<DroneBotBullet>(out var bullet))
            //     bullet.Go(__brain, 16f);

            // Observable.Interval(TimeSpan.FromSeconds(0.2888f)).Take(emitNum - 1).Subscribe(_ =>
            // {
            //     if (Instantiate(emitSource, emitPoint.position, emitPoint.rotation).TryGetComponent<DroneBotBullet>(out var bullet))
            //         bullet.Go(__brain, 16f);
            // }).AddTo(this);

            //* onEmitProjectile 호출은 제일 나중에 함
            base.EmitActionHandler(emitSource, emitPoint, emitNum);
        }
    }
}