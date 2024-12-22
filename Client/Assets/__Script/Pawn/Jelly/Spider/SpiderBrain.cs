using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class SpiderBrain : PawnBrainController, IPawnSpawnable, IPawnMovable
    {
        Vector3 IPawnSpawnable.GetSpawnPosition() => Vector3.zero;
        void IPawnSpawnable.OnStartSpawnHandler() { }
        void IPawnSpawnable.OnFinishSpawnHandler() { }
        void IPawnSpawnable.OnDespawnedHandler() { }
        void IPawnSpawnable.OnDeadHandler() { AnimCtrler.bodyAnimator.SetTrigger("OnDead"); Movement.SetMovementEnabled(false); }
        void IPawnSpawnable.OnLifeTimeOutHandler() { PawnHP.Die("TimeOut"); }
        bool IPawnMovable.CheckReachToDestination() { return Movement.CheckReachToDestination(); }
        bool IPawnMovable.IsJumping() { return false; }
        bool IPawnMovable.IsRolling() { return false; }
        bool IPawnMovable.IsOnGround() { return Movement.IsOnGround; }
        Vector3 IPawnMovable.GetDestination() { return Movement.destination; }
        float IPawnMovable.GetEstimateTimeToDestination() { return Movement.EstimateTimeToDestination(); }
        float IPawnMovable.GetDefaultMinApproachDistance() { return Movement.GetDefaultMinApproachDistance(); }
        bool IPawnMovable.GetFreezeMovement() { return Movement.freezeMovement; }
        bool IPawnMovable.GetFreezeRotation() { return Movement.freezeRotation; }
        void IPawnMovable.ReserveDestination(Vector3 destination) { Movement.ReserveDestination(destination); }
        float IPawnMovable.SetDestination(Vector3 destination) { return Movement.SetDestination(destination); }
        void IPawnMovable.SetMinApproachDistance(float distance) { Movement.minApproachDistance = distance; }
        void IPawnMovable.SetFaceVector(Vector3 faceVec) { Movement.faceVec = faceVec; }
        void IPawnMovable.FreezeMovement(bool newValue) { Movement.freezeMovement = newValue; }
        void IPawnMovable.FreezeRotation(bool newValue) { Movement.freezeRotation = newValue; }
        void IPawnMovable.AddRootMotion(Vector3 deltaPosition, Quaternion deltaRotation) { Movement.AddRootMotion(deltaPosition, deltaRotation); }
        void IPawnMovable.Teleport(Vector3 destination) { Movement.Teleport(destination); }
        void IPawnMovable.MoveTo(Vector3 destination) { Movement.destination = destination; }
        void IPawnMovable.FaceTo(Vector3 direction) { Movement.FaceTo(direction); }
        void IPawnMovable.Stop() { Movement.Stop(); }

        public enum Decisions
        {
            None = -1,
            Idle,
            Spacing,
            Approach, //* (액션을 수행하기 위해) 타겟에게 다가가기
            Max
        }

        public SpiderBlackboard BB { get; private set; }
        public SpiderMovement Movement { get; private set; }
        public SpiderAnimController AnimCtrler { get; private set; }
        public SpiderActionController ActionCtrler { get; private set; }
        public PawnStatusController BuffCtrler { get; private set; }
        public PawnSensorController SensorCtrler { get; private set; }


        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BB = GetComponent<SpiderBlackboard>();
            Movement = GetComponent<SpiderMovement>();
            AnimCtrler = GetComponent<SpiderAnimController>();
            ActionCtrler = GetComponent<SpiderActionController>();
            BuffCtrler = GetComponent<PawnStatusController>();
            SensorCtrler = GetComponent<PawnSensorController>();

            // PawnHP.heartPoint.Value = BB.stat.maxHeartPoint.Value;
            // Movement.moveSpeed = BB.body.moveSpeed;
            // Movement.moveBrake = BB.body.moveBrake;
            // Movement.rotateSpeed = BB.body.rotateSpeed;
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            SensorCtrler.onWatchSomething += (s) =>
            {
                if (BB.IsSpawnFinished && !BB.IsDead && BB.TargetPawn == null && ValidateTargetCollider(s))
                    HandleWatchSomethingOrDamaged(s.GetComponent<PawnColliderHelper>().pawnBrain, 0.5f);
            };

            SensorCtrler.onTouchSomething += (s) =>
            {
                if (BB.IsSpawnFinished && !BB.IsDead && BB.TargetPawn == null && ValidateTargetCollider(s))
                    HandleWatchSomethingOrDamaged(s.GetComponent<PawnColliderHelper>().pawnBrain, 0.1f);
            };

            PawnHP.onDamaged += (damageContext) =>
            {
                //! Sender와 Recevier가 동일할 수 있기 때문에 반드시 'receiverBrain'을 먼저 체크해야 함
                if (damageContext.receiverBrain == this)
                    PostReceiveDamage(ref damageContext);
                else if (damageContext.senderBrain == this)
                    PostSendDamage(ref damageContext);
            };
        }

        void PostReceiveDamage(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (damageContext.actionResult == ActionResults.Damaged)
            {
                if (ActionCtrler.CheckActionRunning())
                    ActionCtrler.CancelAction(false);

                ActionCtrler.StartAction(damageContext, "!OnHit", string.Empty);
                // ActionCtrler.StartAction(damageContext, "!OnBigHit", 1, 1, 0);

                // OnPawnDamaged
                GameManager.Instance.PawnDamaged(ref damageContext);
            }

            HandleWatchSomethingOrDamaged(damageContext.senderBrain, 0.1f);
        }

        void PostSendDamage(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (damageContext.actionResult == ActionResults.Blocked || damageContext.actionResult == ActionResults.GuardParried)
            {
                if (ActionCtrler.CheckActionRunning())
                    ActionCtrler.CancelAction(false);

                ActionCtrler.currActionContext.actionDisposable = ActionCtrler.StartOnBlockedAction(ref damageContext, false);
                // AnimCtrler.legAnimator.User_AddImpulse(new ImpulseExecutor(new Vector3(0, 0.2f, -0.1f), Vector3.zero, 0.2f));
            }
        }

        void HandleWatchSomethingOrDamaged(PawnBrainController otherBrain, float reservedDecisionCoolTime)
        {
            BB.decision.targetPawnHP.Value = otherBrain.PawnHP;
            BB.decision.isInCombat.Value = true;
            InvalidateDecision(reservedDecisionCoolTime);
        }

        protected override void OnTickInternal(float interval)
        {
            base.OnTickInternal(interval);

            __decisionCoolTime = Mathf.Max(0, __decisionCoolTime - tickInterval);

            if (BB.IsInCombat)
            {
                if (BB.IsDead)
                {
                    BB.decision.targetPawnHP.Value = null;
                    BB.decision.isInCombat.Value = false;
                    InvalidateDecision(0.5f);
                }
                else if (!ValidateTargetBrain())
                {
                    var nextPawn = NextTargetBrain();
                    if (nextPawn != null)
                    {
                        HandleWatchSomethingOrDamaged(nextPawn.GetComponent<PawnBrainController>(), 0.5f);
                    }
                    else
                    {
                        BB.decision.targetPawnHP.Value = null;
                        BB.decision.isInCombat.Value = false;
                        InvalidateDecision(0.5f);
                    }
                }
                else if (RefreshAggresiveLevel())
                {
                    InvalidateDecision(0.1f);
                }
                else
                {
                    if (BB.CurrDecision == Decisions.Spacing || BB.CurrDecision == Decisions.Approach)
                    {
                        var distanceVec = (BB.TargetBrain.coreColliderHelper.transform.position - coreColliderHelper.transform.position).Vector2D();
                        var distanceLen = distanceVec.magnitude;
                        // if (BB.decision.aggressiveLevel.Value == 0)
                        // {
                        //     if (BB.CurrDecision == Decisions.Spacing && distanceLen > BB.decision.spacingOutDistance)
                        //         InvalidateDecision(0.1f);
                        //     else if (BB.CurrDecision == Decisions.Approach && distanceLen <= BB.decision.spacingInDistance)
                        //         InvalidateDecision(0);
                        // }

                        if (!BuffCtrler.CheckStatus(Game.PawnStatus.Staggered) && (!ActionCtrler.CheckActionRunning() || ActionCtrler.CanInterruptAction()))
                        {
                            var newActionName = string.Empty;
                            // if (ActionCtrler.CheckActionRunning() && ActionCtrler.CanInterruptAction() && ActionCtrler.CurrActionName == "Attack#1" && BB.stat.stamina.Value >= BB.temp.attack2_stamina)
                            // {
                            //     ActionCtrler.CancelAction(false);
                            //     ActionCtrler.pendingAction = new("Attack#2", Time.time);
                            // }
                            // else if (ActionCtrler.prevActionContext.actionName == "Attack#1" && Time.time - ActionCtrler.prevActionContext.finishTimeStamp < 1 && BB.stat.stamina.Value >= BB.temp.attack2_stamina)
                            // {
                            //     ActionCtrler.pendingAction = new("Attack#2", Time.time);
                            // }
                            // else if (distanceLen <= BB.temp.attack1_range && BB.stat.stamina.Value >= BB.temp.attack1_stamina)
                            // {
                            //     ActionCtrler.pendingAction = new("Attack#1", Time.time);
                            // }
                        }
                    }
                    else
                    {
                        if (__decisionCoolTime <= 0)
                            BB.decision.currDecision.Value = MakeDecision();
                    }
                }
            }
            else if (BB.CurrDecision == Decisions.None)
            {
                if (__decisionCoolTime <= 0)
                    BB.decision.currDecision.Value = Decisions.Idle;
            }
        }

        Decisions MakeDecision()
        {
            Debug.Assert(BB.TargetPawn != null);


                return Decisions.Approach;

            // if (BB.decision.aggressiveLevel.Value == 1)
            // {
            //     return Decisions.Approach;
            // }
            // else
            // {
            //     var distance = (BB.TargetBrain.core.transform.position - core.transform.position).Vector2D().magnitude;
            //     return distance > BB.decision.spacingOutDistance ? Decisions.Approach : Decisions.Spacing;
            // }
        }

        float __decisionCoolTime;

        void InvalidateDecision(float decisionCoolTime = 0)
        {
            __decisionCoolTime = decisionCoolTime;
            BB.decision.currDecision.Value = Decisions.None;
        }

        bool RefreshAggresiveLevel()
        {
            return false;
            // if (BB.decision.aggressiveLevel.Value == 0 && BB.stat.stamina.Value == BB.stat.maxStamina.Value)
            // {
            //     BB.decision.aggressiveLevel.Value = 1;
            //     return true;
            // }
            // else if (BB.decision.aggressiveLevel.Value == 1 && BB.stat.stamina.Value == 0)
            // {
            //     BB.decision.aggressiveLevel.Value = 0;
            //     return true;
            // }
            // else
            // {
            //     return false;
            // }
        }

        bool ValidateTargetCollider(Collider otherCollider)
        {
            var colliderHelper = otherCollider.GetComponent<PawnColliderHelper>();
            if (colliderHelper.pawnBrain != null && colliderHelper.pawnBrain.PawnBB.IsSpawnFinished && !colliderHelper.pawnBrain.PawnBB.IsDead)
                return true;
            else
                return false;
        }

        bool ValidateTargetBrain()
        {
            if (BB.TargetBrain == null || BB.TargetBrain.PawnBB.IsDead)
                return false;

            if (!BB.TargetBrain.PawnBB.IsSpawnFinished)
                __Logger.ErrorR(gameObject, "BB.WatchingPawnBrain.PawnBB.IsSpawnFinished is false", nameof(BB.TargetPawn), BB.TargetPawn);

            Debug.Assert(BB.TargetBrain.PawnBB.IsSpawnFinished);
            return SensorCtrler.ListeningColliders.Contains(BB.TargetBrain.coreColliderHelper.pawnCollider);
        }

        PawnBrainController NextTargetBrain()
        {
            if (SensorCtrler.WatchingColliders.Count > 0 && SensorCtrler.WatchingColliders.Any(w => w.GetComponent<PawnColliderHelper>() != null))
            {
                //* 시야 안에 있는 Hero 찾기 (중복이 있다면 가장 가까운 것 선택)
                var colliderHelper = SensorCtrler.WatchingColliders
                    .Where(w => ValidateTargetCollider(w))
                    .Select(w => w.GetComponent<PawnColliderHelper>()).Where(h => h.pawnBrain != null)
                    .OrderBy(h => (h.transform.position - transform.position).SqrMagnitude2D())
                    .FirstOrDefault();

                if (colliderHelper != null)
                {
                    __Logger.LogF(gameObject, nameof(NextTargetBrain), "return...", nameof(colliderHelper.pawnBrain), colliderHelper.pawnBrain);
                    return colliderHelper.pawnBrain;
                }
            }

            __Logger.LogF(gameObject, nameof(NextTargetBrain), "return... null");
            return null;
        }
    }
}
