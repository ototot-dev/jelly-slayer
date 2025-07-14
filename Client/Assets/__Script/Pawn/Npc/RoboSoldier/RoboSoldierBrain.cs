using System;
using System.Collections;
using DG.Tweening;
using UGUI.Rx;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(RoboSoldierMovement))]
    [RequireComponent(typeof(RoboSoldierBlackboard))]
    [RequireComponent(typeof(RoboSoldierAnimController))]
    [RequireComponent(typeof(RoboSoldierActionController))]
    public class RoboSoldierBrain : NpcHumanoidBrain, IPawnTargetable
    {
        #region IPawnTargetable 구현
        PawnColliderHelper IPawnTargetable.StartTargeting() => bodyHitColliderHelper;
        PawnColliderHelper IPawnTargetable.NextTarget() => null;
        PawnColliderHelper IPawnTargetable.PrevTarget() => null;
        PawnColliderHelper IPawnTargetable.CurrTarget() => bodyHitColliderHelper;
        void IPawnTargetable.StopTargeting() { }
        #endregion

        public override Vector3 GetInteractionKeyAttachPoint() => BB.children.specialKeyAttachPoint.transform.position;
        public override float GetInteractionVisibleRadius() => 2f;
        public RoboSoldierBlackboard BB { get; private set; }
        public RoboSoldierMovement Movement { get; private set; }
        public RoboSoldierAnimController AnimCtrler { get; private set; }
        public RoboSoldierActionController ActionCtrler { get; private set; }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BB = GetComponent<RoboSoldierBlackboard>();
            Movement = GetComponent<RoboSoldierMovement>();
            AnimCtrler = GetComponent<RoboSoldierAnimController>();
            ActionCtrler = GetComponent<RoboSoldierActionController>();
        }

        public enum ActionPatterns : int
        {
            None = -1,
            JumpAttack,
            ShieldAttackA,
            ShieldAttackB,
            CounterA,
            CounterB,
            ComboAttackA,
            ComboAttackB,
            ComboAttackC,
            Max,
        }

        IEnumerator SpawningCoroutine()
        {
            PawnEventManager.Instance.SendPawnSpawningEvent(this, PawnSpawnStates.SpawnStart);

            foreach (var r in BB.children.jetFlameRenderers)
            {
                if (!r.enabled) r.enabled = true;
                r.transform.localScale = 2f * Vector3.one;
                r.transform.GetChild(0).GetComponent<ParticleSystem>().Play();
            }

            AnimCtrler.mainAnimator.SetBool("IsFalling", true);

            var dropVelocity = Vector3.zero;
            var oldCollisionLayers = Movement.GetCharacterMovement().collisionLayers;

            //* 강하 중에 벽에 걸리지 않도록 충돌 레이어를 임시로 변경함
            Movement.GetCharacterMovement().collisionLayers = LayerMask.GetMask("Floor");
            Observable.EveryFixedUpdate().TakeWhile(_ => !Movement.IsOnGround).Subscribe(_ =>
            {
                dropVelocity += Time.fixedDeltaTime * BB.body.spawnDropAccel * BB.body.spawnDropDirection;
                Movement.GetCharacterMovement().SimpleMove(dropVelocity, BB.body.spawnDropSpeed, 999f, 999f, 1f, 1f, Vector3.zero, false, Time.fixedDeltaTime);
            }).AddTo(this);

            yield return new WaitUntil(() => Movement.IsOnGround);

            foreach (var r in BB.children.jetFlameRenderers)
            {
                r.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() => r.enabled = false);
                r.transform.GetChild(0).GetComponent<ParticleSystem>().Stop();
            }

            AnimCtrler.mainAnimator.SetBool("IsFalling", false);
            GameContext.Instance.cameraCtrler.Shake(0.5f, 2f, 0.5f);

            yield return new WaitForSeconds(2f);

            Movement.GetCharacterMovement().velocity = Vector3.zero;
            Movement.GetCharacterMovement().collisionLayers = LayerMask.GetMask("Floor");

            BB.common.isSpawnFinished.Value = true;
            PawnEventManager.Instance.SendPawnSpawningEvent(this, PawnSpawnStates.SpawnFinished);
        }

        IDisposable __spawningDisposable;

        protected override void StartInternal()
        {
            base.StartInternal();

            Movement.Teleport(GetWorldPosition() - BB.body.spawnDropDistance * BB.body.spawnDropDirection.normalized, false);

            //* Spawning 연출 시작
            __spawningDisposable = Observable.FromCoroutine(SpawningCoroutine)
                .DoOnCompleted(() => __spawningDisposable = null)
                .Subscribe().AddTo(this);

            //* 액션 패턴 등록
            ActionDataSelector.ReserveSequence(ActionPatterns.JumpAttack, "JumpAttack").BeginCoolTime(BB.action.jumpAttackCoolTime).ResetProbability(1f);
            ActionDataSelector.ReserveSequence(ActionPatterns.ShieldAttackA, "ShieldAttack", "Counter", "Counter", 0.1f, "Attack#3").ResetProbability(1f);
            ActionDataSelector.ReserveSequence(ActionPatterns.ShieldAttackB, "ShieldAttack", "Attack#1", "Attack#2", 0.1f, "Attack#3").ResetProbability(1f);
            ActionDataSelector.ReserveSequence(ActionPatterns.CounterA, "Counter", "Counter", 0.1f, "Attack#3").ResetProbability(1f);
            ActionDataSelector.ReserveSequence(ActionPatterns.CounterB, "Counter", 0.1f, "Attack#1", "Attack#2", 0.1f, "Attack#3").ResetProbability(1f);
            ActionDataSelector.ReserveSequence(ActionPatterns.ComboAttackA, "Attack#1", "Attack#2", 0.1f, "Attack#3").ResetProbability(1f);
            ActionDataSelector.ReserveSequence(ActionPatterns.ComboAttackB, "Counter", "Counter", 0.1f, "Attack#3").ResetProbability(1f);
            ActionDataSelector.ReserveSequence(ActionPatterns.ComboAttackC, "Counter", "Counter", 0.1f, "Attack#1", "Attack#2", 0.1f, "Attack#3").ResetProbability(1f);

            ActionCtrler.onActionStart += (actionContext, __) =>
            {
                if (actionContext.actionName == "Backstep")
                {
                    var dashVec = GetWorldTransform().InverseTransformDirection(-GetWorldPosition()).Vector2D().normalized;
                    __pawnAnimCtrler.mainAnimator.SetFloat("DashX", dashVec.x);
                    __pawnAnimCtrler.mainAnimator.SetFloat("DashY", dashVec.z);
                }
            };

            PawnStatusCtrler.onStatusActive += (status) =>
            {
                if (status == PawnStatus.Groggy || status == PawnStatus.Staggered || status == PawnStatus.KnockDown)
                    ActionDataSelector.CancelSequences();

                //* 그로기 시작 이벤트 
                if (status == PawnStatus.Groggy)
                    PawnEventManager.Instance.SendPawnStatusEvent(this, PawnStatus.Groggy, 1f, PawnStatusCtrler.GetDuration(PawnStatus.Groggy));
            };

            PawnStatusCtrler.onStatusDeactive += (status) =>
            {
                if (status == PawnStatus.Groggy)
                {
                    //* 막타 Hit 애님이 온전 출력되는 시간 딜레이 
                    Observable.Timer(TimeSpan.FromSeconds(1.5f)).Subscribe(_ => __pawnAnimCtrler.mainAnimator.SetBool("IsGroggy", false)).AddTo(this);

                    //* 리커버 동작 동안은 무적 및 이동, 액션 금지
                    PawnStatusCtrler.AddStatus(PawnStatus.Invincible, 1f, 2f);
                    PawnStatusCtrler.AddStatus(PawnStatus.CanNotMove, 1f, 3f);
                    PawnStatusCtrler.AddStatus(PawnStatus.CanNotAction, 1f, 3f);

                    ActionDataSelector.BeginCoolTime(ActionPatterns.ShieldAttackA, BB.action.shieldAttackCoolTime);
                    ActionDataSelector.BeginCoolTime(ActionPatterns.ShieldAttackB, BB.action.shieldAttackCoolTime);

                    InvalidateDecision(3f);
                }
            };

            AnimCtrler.FindStateMachineTriggerObservable("OnGroggy (Wait)").OnStateEnterAsObservable().Subscribe(_ =>
            {
                new InteractionKeyController("E", "Chainsaw", 2f, this).Load().Show(GameContext.Instance.canvasManager.body.transform as RectTransform);
                bodyHitColliderHelper.gameObject.layer = LayerMask.NameToLayer("HitBox");
            }).AddTo(this);

            AnimCtrler.FindStateMachineTriggerObservable("OnGroggy (Wait)").OnStateExitAsObservable().Subscribe(_ =>
            {
                bodyHitColliderHelper.gameObject.layer = LayerMask.NameToLayer("HitBoxBlocking");
            }).AddTo(this);

            BB.common.isDown.Skip(1).Subscribe(v =>
            {
                if (v)
                {
                    AnimCtrler.mainAnimator.SetBool("IsDown", true);
                    AnimCtrler.mainAnimator.SetTrigger("OnDown");
                }
                else
                {
                    AnimCtrler.mainAnimator.SetBool("IsDown", false);

                    //* 일어나는 모션 동안은 무적
                    PawnStatusCtrler.AddStatus(PawnStatus.Invincible, 1f, 2f);
                    PawnStatusCtrler.AddStatus(PawnStatus.CanNotMove, 1f, 3f);
                    PawnStatusCtrler.AddStatus(PawnStatus.CanNotAction, 1f, 3f);

                    InvalidateDecision(3f);
                }
            }).AddTo(this);

            BB.body.isFalling.Skip(1).Subscribe(v =>
            {
                //* 착지 동작 완료까지 이동을 금지함
                if (!v) PawnStatusCtrler.AddStatus(PawnStatus.CanNotMove, 1f, 0.5f);
            }).AddTo(this);

            //* 방패 터치 시에 ShieldAttack 발동 조건
            BB.children.shieldTouchSensor.OnTriggerEnterAsObservable().Subscribe(c =>
            {
                if (!BB.IsSpawnFinished || !BB.IsInCombat || BB.IsDead || BB.IsDown || BB.IsGroggy)
                    return;
                if (ActionCtrler.CheckActionPending() || (ActionCtrler.CheckActionRunning() && !ActionCtrler.CanInterruptAction()))
                    return;
                if (StatusCtrler.CheckStatus(PawnStatus.CanNotAction) || StatusCtrler.CheckStatus(PawnStatus.Staggered))
                    return;

                if (ActionDataSelector.EvaluateSequence(ActionPatterns.ShieldAttackA, 1f) && c.TryGetComponent<PawnColliderHelper>(out var colliderHelper) && colliderHelper.pawnBrain.PawnBB.common.pawnName == "Hero")
                {
                    ActionDataSelector.EnqueueSequence(UnityEngine.Random.Range(0, 2) > 0 ? ActionPatterns.ShieldAttackA : ActionPatterns.ShieldAttackB);
                    ActionDataSelector.BeginCoolTime(ActionPatterns.ShieldAttackA, 1f);
                    ActionDataSelector.BeginCoolTime(ActionPatterns.ShieldAttackB, 1f);
                }
            }).AddTo(this);

            onUpdate += () =>
            {
                if (!BB.IsSpawnFinished || !BB.IsInCombat || BB.IsDead || BB.IsDown || BB.IsGroggy)
                    return;

                if (StatusCtrler.CheckStatus(PawnStatus.CanNotAction) || StatusCtrler.CheckStatus(PawnStatus.Staggered))
                    return;

                if (!ActionCtrler.CheckActionPending() && (!ActionCtrler.CheckActionRunning() || ActionCtrler.CanInterruptAction()))
                {
                    var nextActionData = ActionDataSelector.AdvanceSequence();
                    if (nextActionData != null)
                    {
                        if (ActionCtrler.CheckActionRunning()) ActionCtrler.CancelAction(false);
                        ActionCtrler.SetPendingAction(nextActionData.actionName, string.Empty, string.Empty, ActionDataSelector.CurrSequence().GetPaddingTime());
                    }
                }
            };
        }

        protected override void OnTickInternal(float interval)
        {
            base.OnTickInternal(interval);

            if (!BB.IsSpawnFinished || !BB.IsInCombat || BB.IsDead || BB.IsDown || BB.IsGroggy)
                return;

            if (StatusCtrler.CheckStatus(PawnStatus.CanNotAction) || StatusCtrler.CheckStatus(PawnStatus.Staggered))
                return;

#if UNITY_EDITOR
            if (ActionDataSelector.debugActionSelectDisabled)
                return;
#endif

            if (ActionDataSelector.CurrSequence() == null)
            {
                var distanceToTarget = coreColliderHelper.GetDistanceSimple(BB.TargetBrain.coreColliderHelper);
                if (distanceToTarget < BB.action.comboAttackDistance)
                {
                    var comboAttackPick = ActionPatterns.None;
                    switch (UnityEngine.Random.Range(0, 3))
                    {
                        case 0: comboAttackPick = ActionPatterns.ComboAttackA; break;
                        case 1: comboAttackPick = ActionPatterns.ComboAttackB; break;
                        case 2: comboAttackPick = ActionPatterns.ComboAttackC; break;
                    }

                    if (CheckTargetVisibility() && ActionDataSelector.EvaluateSequence(comboAttackPick, 1f))
                    {
                        ActionDataSelector.EnqueueSequence(comboAttackPick);
                        ActionDataSelector.BeginCoolTime(ActionPatterns.ComboAttackA, BB.action.comboAttackCoolTime);
                        ActionDataSelector.BeginCoolTime(ActionPatterns.ComboAttackB, BB.action.comboAttackCoolTime);
                        ActionDataSelector.BeginCoolTime(ActionPatterns.ComboAttackC, BB.action.comboAttackCoolTime);
                    }
                }
                else
                {
                    if (CheckTargetVisibility() && ActionDataSelector.EvaluateSequence(ActionPatterns.JumpAttack, 1f))
                    {
                        ActionDataSelector.EnqueueSequence(ActionPatterns.JumpAttack);
                        ActionDataSelector.BeginCoolTime(ActionPatterns.JumpAttack, BB.action.jumpAttackCoolTime);
                    }
                }
            }
        }

        protected override void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            base.DamageReceiverHandler(ref damageContext);

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                CreateDamageText(ref damageContext);
            }
            else if (damageContext.actionResult == ActionResults.Blocked)
            {
                if (ActionDataSelector.CurrSequence() == null && ActionDataSelector.EvaluateSequence(ActionPatterns.CounterA, 1f))
                {
                    var counterPick = UnityEngine.Random.Range(0, 2) > 0 ? ActionPatterns.CounterA : ActionPatterns.CounterB;
                    ActionDataSelector.EnqueueSequence(UnityEngine.Random.Range(0, 2) > 0 ? ActionPatterns.CounterA : ActionPatterns.CounterB);
                    ActionDataSelector.BeginCoolTime(ActionPatterns.CounterA, BB.action.counterCoolTime);
                    ActionDataSelector.BeginCoolTime(ActionPatterns.CounterA, BB.action.counterCoolTime);
                }
            }
        }

        protected override void StartJumpInternal(float jumpHeight)
        {
            Movement.StartJump(BB.body.jumpHeight);
        }

        protected override void FinishJumpInternal()
        {
            Movement.StartFalling();
        }
    }
}
