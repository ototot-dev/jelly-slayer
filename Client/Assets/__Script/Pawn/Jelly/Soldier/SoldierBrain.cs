using System;
using System.Collections;
using Game.NodeCanvasExtension;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(SoldierMovement))]
    [RequireComponent(typeof(SoldierBlackboard))]
    [RequireComponent(typeof(SoldierAnimController))]
    [RequireComponent(typeof(SoldierActionController))]
    public class SoldierBrain : JellyHumanoidBrain, IPawnTargetable
    {
        #region IPawnTargetable 구현
        PawnColliderHelper IPawnTargetable.StartTargeting() => bodyHitColliderHelper;
        PawnColliderHelper IPawnTargetable.NextTarget() => null;
        PawnColliderHelper IPawnTargetable.CurrTarget() => bodyHitColliderHelper;
        void IPawnTargetable.StopTargeting() { }
        #endregion

        #region IPawnSpawnable 재정의
        public override void OnDeadHandler()
        {
            base.OnDeadHandler();

            var roboDogBrain = roboDogFormationCtrler.PickRoboDog();
            while (roboDogBrain != null)
            {
                roboDogFormationCtrler.ReleaseRoboDog(roboDogBrain);
                roboDogBrain.BB.common.isDead.Value = true;
                roboDogBrain = roboDogFormationCtrler.PickRoboDog();
            }
        }
        #endregion

        [Header("Component")]
        public RoboDogFormationController roboDogFormationCtrler;

        public override Vector3 GetSpecialKeyPosition() => BB.attachment.specialKeyAttachPoint.transform.position;
        public SoldierBlackboard BB { get; private set; }
        public SoldierMovement Movement { get; private set; }
        public SoldierAnimController AnimCtrler { get; private set; }
        public SoldierActionController ActionCtrler { get; private set; }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BB = GetComponent<SoldierBlackboard>();
            Movement = GetComponent<SoldierMovement>();
            AnimCtrler = GetComponent<SoldierAnimController>();
            ActionCtrler = GetComponent<SoldierActionController>();
        }

        public enum ActionPatterns : int
        {
            None = -1,
            JumpAttackA,
            JumpAttackB,
            ShieldAttackA,
            ShieldAttackB,
            CounterA,
            CounterB,
            MissileA,
            MissileB,
            ComboAttackA,
            ComboAttackB,
            ComboAttackC,
            Leap,
            Max,
        }

        IEnumerator SpawningCoroutine()
        {
            PawnEventManager.Instance.SendPawnSpawningEvent(this, PawnSpawnStates.SpawnStart);
            Movement.gravity = Vector3.down;
            Movement.StartJump(0f);

            yield return new WaitForSeconds(1f);

            AnimCtrler.ragdollAnimator.Handler.AnimatingMode = FIMSpace.FProceduralAnimation.RagdollHandler.EAnimatingMode.Off;
            Movement.gravity = 20f * Vector3.down;
            Movement.StartFalling();

            yield return new WaitUntil(() => Movement.IsOnGround);

            GameContext.Instance.cameraCtrler.Shake(0.2f, 0.5f);

            yield return new WaitForSeconds(1f);

            BB.common.isSpawnFinished.Value = true;
            PawnEventManager.Instance.SendPawnSpawningEvent(this, PawnSpawnStates.SpawnFinished);
        }

        IDisposable __spawningDisposable;

        protected override void StartInternal()
        {
            base.StartInternal();

            //* Spawning 연출 시작
            Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(_ =>
            {
                __spawningDisposable = Observable.FromCoroutine(SpawningCoroutine)
                    .DoOnCompleted(() => __spawningDisposable = null)
                    .Subscribe().AddTo(this);
            }).AddTo(this);

            //* 액션 패턴 등록
            ActionDataSelector.ReserveSequence(ActionPatterns.JumpAttackA, "JumpAttack").BeginCoolTime(BB.action.jumpAttackCoolTime);
            ActionDataSelector.ReserveSequence(ActionPatterns.JumpAttackB, "Missile", 1f, "JumpAttack").BeginCoolTime(BB.action.jumpAttackCoolTime);
            ActionDataSelector.ReserveSequence(ActionPatterns.ShieldAttackA, "ShieldAttack", "Counter", "Counter", 0.1f, "Attack#3");
            ActionDataSelector.ReserveSequence(ActionPatterns.ShieldAttackB, "ShieldAttack", "Attack#1", "Attack#2", 0.1f, "Attack#3");
            ActionDataSelector.ReserveSequence(ActionPatterns.CounterA, "Counter", "Counter", 0.1f, "Attack#3");
            ActionDataSelector.ReserveSequence(ActionPatterns.CounterB, "Counter", 0.1f, "Attack#1", "Attack#2", 0.1f, "Attack#3");
            ActionDataSelector.ReserveSequence(ActionPatterns.MissileA, "Missile").BeginCoolTime(BB.action.missileCoolTime);
            ActionDataSelector.ReserveSequence(ActionPatterns.MissileB, "Backstep", 0.5f, "Missile").BeginCoolTime(BB.action.missileCoolTime);
            ActionDataSelector.ReserveSequence(ActionPatterns.ComboAttackA, "Attack#1", "Attack#2", 0.1f, "Attack#3");
            ActionDataSelector.ReserveSequence(ActionPatterns.ComboAttackB, "Counter", "Counter", 0.1f, "Attack#3");
            ActionDataSelector.ReserveSequence(ActionPatterns.ComboAttackC, "Counter", "Counter", 0.1f, "Attack#1", "Attack#2", 0.1f, "Attack#3");
            ActionDataSelector.ReserveSequence(ActionPatterns.Leap, "Backstep", 0.5f, "Missile", 0.2f, "Missile", 0.2f, "Leap").BeginCoolTime(BB.action.leapCoolTime);

            ActionDataSelector.ResetProbability(ActionPatterns.MissileA, 1f);
            ActionDataSelector.ResetProbability(ActionPatterns.MissileB, 1f);
            ActionDataSelector.ResetProbability(ActionPatterns.Leap, 1f);

            ActionCtrler.onActionStart += (actionContext, __) =>
            {
                if (actionContext.actionName == "Backstep")
                {
                    var dashVec = GetWorldTransform().InverseTransformDirection( -GetWorldPosition()).Vector2D().normalized;
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
                    jellyMeshCtrler.FadeOut(0.5f);
                    // jellyMeshCtrler.Die(5f);
                    jellyMeshCtrler.FinishHook();

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
            BB.action.shieldTouchSensor.OnTriggerEnterAsObservable().Subscribe(c =>
            {
                if (!BB.IsSpawnFinished || !BB.IsInCombat || BB.IsDead || BB.IsDown || BB.IsGroggy)
                    return;
                if (ActionCtrler.CheckActionPending() || (ActionCtrler.CheckActionRunning() && !ActionCtrler.CanInterruptAction()))
                    return;
                if (StatusCtrler.CheckStatus(PawnStatus.CanNotAction) || StatusCtrler.CheckStatus(PawnStatus.Staggered))
                    return;

                if (ActionDataSelector.EvaluateSequence(ActionPatterns.ShieldAttackA) && c.TryGetComponent<PawnColliderHelper>(out var colliderHelper) && colliderHelper.pawnBrain.PawnBB.common.pawnName == "Hero")
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
                if (ActionDataSelector.EvaluateSequence(ActionPatterns.Leap))
                {
                    ActionDataSelector.EnqueueSequence(ActionPatterns.Leap).BeginCoolTime(BB.action.leapCoolTime);

                }
                else if (ActionDataSelector.EvaluateSequence(ActionPatterns.MissileA))
                {
                    ActionDataSelector.EnqueueSequence(GetWorldPosition().Magnitude2D() < BB.action.backstepTriggerDistance ? ActionPatterns.MissileA : ActionPatterns.MissileB);
                    ActionDataSelector.BeginCoolTime(ActionPatterns.MissileA, BB.action.missileCoolTime);
                    ActionDataSelector.BeginCoolTime(ActionPatterns.MissileB, BB.action.missileCoolTime);
                }
                else
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
                    
                        if (CheckTargetVisibility() && ActionDataSelector.EvaluateSequence(comboAttackPick))
                        {
                            ActionDataSelector.EnqueueSequence(comboAttackPick);
                            ActionDataSelector.BeginCoolTime(ActionPatterns.ComboAttackA, BB.action.comboAttackCoolTime);
                            ActionDataSelector.BeginCoolTime(ActionPatterns.ComboAttackB, BB.action.comboAttackCoolTime);
                            ActionDataSelector.BeginCoolTime(ActionPatterns.ComboAttackC, BB.action.comboAttackCoolTime);
                        }
                    }
                    else
                    {
                        var jumpAttackPick = Rand.Dice(2) == 1 ? ActionPatterns.JumpAttackA : ActionPatterns.JumpAttackB;

                        //* 미사일 패턴이 중복되지 않도록 미사일 쿨타임이 존재하면 'JumpAttackB'는 선택하지 않음
                        if (jumpAttackPick == ActionPatterns.JumpAttackB && Mathf.Max(ActionDataSelector.GetRemainCoolTime(ActionPatterns.MissileA), ActionDataSelector.GetRemainCoolTime(ActionPatterns.MissileB)) > 0f)
                            jumpAttackPick = ActionPatterns.JumpAttackA;


                        if (CheckTargetVisibility() && ActionDataSelector.EvaluateSequence(jumpAttackPick))
                        {
                            ActionDataSelector.EnqueueSequence(jumpAttackPick);
                            ActionDataSelector.BeginCoolTime(ActionPatterns.JumpAttackA, BB.action.jumpAttackCoolTime);
                            ActionDataSelector.BeginCoolTime(ActionPatterns.JumpAttackB, BB.action.jumpAttackCoolTime);
                        }
                    }
                }
            }
        }

        protected override void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            base.DamageReceiverHandler(ref damageContext);

            if (damageContext.actionResult == ActionResults.Damaged)
            {
                GameContext.Instance.damageTextManager.Create(damageContext.finalDamage.ToString("0"), damageContext.hitPoint, 1f, Color.white);
            }
            else if (damageContext.actionResult == ActionResults.Blocked)
            {
                if (ActionDataSelector.CurrSequence() == null && ActionDataSelector.EvaluateSequence(ActionPatterns.CounterA))
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
