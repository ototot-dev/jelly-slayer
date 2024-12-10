using System;
using System.Collections.Generic;
using System.Linq;
using Packets;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{
    public class SlimeBossBrain : PawnBrainController, ISpawnable, IMovable
    {
        [Header("Component")]
        public Transform fxAttachPoint;

        public void SpawnSlimeMini()
        {
            Instantiate(Resources.Load<GameObject>("Pawn/Jelly/SlimeMini")).transform.position = ActionCtrler.emitPoint.transform.position;
        }

        #region ISpawnable/IMovable 구현
        Vector3 ISpawnable.GetSpawnPosition() => Vector3.zero;
        void ISpawnable.OnStartSpawnHandler() { }
        void ISpawnable.OnFinishSpawnHandler() { }
        void ISpawnable.OnDespawnedHandler() { }
        void ISpawnable.OnDeadHandler() { AnimCtrler.mainAnimator.SetTrigger("OnDead"); Movement.SetMovementEnabled(false); }
        void ISpawnable.OnLifeTimeOutHandler() { PawnHP.Die("TimeOut"); }
        bool IMovable.IsJumping() { return false; }
        bool IMovable.IsRolling() { return false; }
        bool IMovable.CheckReachToDestination() { return Movement.CheckReachToDestination(); }
        Vector3 IMovable.GetDestination() { return Movement.destination; }
        float IMovable.GetEstimateTimeToDestination() { return Movement.EstimateTimeToDestination(); }
        float IMovable.GetDefaultMinApproachDistance() { return Movement.GetDefaultMinApproachDistance(); }
        bool IMovable.GetFreezeMovement() { return Movement.freezeMovement; }
        bool IMovable.GetFreezeRotation() { return Movement.freezeRotation; }
        void IMovable.ReserveDestination(Vector3 destination) { Movement.ReserveDestination(destination); }
        float IMovable.SetDestination(Vector3 destination) { return Movement.SetDestination(destination); }
        void IMovable.SetMinApproachDistance(float distance) { Movement.minApproachDistance = distance; }
        void IMovable.SetFaceVector(Vector3 faceVec) { Movement.faceVec = faceVec; }
        void IMovable.FreezeMovement(bool newValue) { Movement.freezeMovement = newValue; }
        void IMovable.FreezeRotation(bool newValue) { Movement.freezeRotation = newValue; }
        void IMovable.AddRootMotion(Vector3 deltaPosition, Quaternion deltaRotation) { Movement.AddRootMotion(deltaPosition, deltaRotation); }
        void IMovable.Teleport(Vector3 destination) { Movement.Teleport(destination); }
        void IMovable.MoveTo(Vector3 destination) { Movement.destination = destination; }
        void IMovable.FaceTo(Vector3 direction) { Movement.FaceTo(direction); }
        void IMovable.Stop() { Movement.Stop(); }
        #endregion

        public enum Decisions
        {
            None = -1,
            Idle,
            Approach,
            Max
        }

        public PawnStatusController BuffCtrler { get; private set; }
        public PawnSensorController SensorCtrler { get; private set; }
        public PawnActionDataSelector ActionDataSelector { get; private set; }
        public SlimeBossBlackboard BB { get; private set; }
        public SlimeBossMovement Movement { get; private set; }
        public SlimeBossAnimController AnimCtrler { get; private set; }
        public SlimeBossActionController ActionCtrler { get; private set; }
        protected float __decisionCoolTime;

        public override void OnDecisionFinishedHandler()
        {
            InvalidateDecision(0.2f);
        }

        public override void OnWatchSomethingOrDamagedHandler(PawnBrainController otherBrain, float reservedDecisionCoolTime)
        {
            BB.targetPawnHP.Value = otherBrain.PawnHP;
            BB.isInCombat.Value = true;
            InvalidateDecision(reservedDecisionCoolTime);

            //* 어그로를 다른 Jelly들에게 전파함
            foreach (var b in NpcSpawnManager.Instance.spawnedBrains)
            {
                if (b != this)
                    b.OnWatchSomethingOrDamagedHandler(otherBrain, reservedDecisionCoolTime);
            }
        }

        protected void InvalidateDecision(float decisionCoolTime = 0)
        {
            __decisionCoolTime = decisionCoolTime;
            BB.currDecision.Value = Decisions.None;
        }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BuffCtrler = GetComponent<PawnStatusController>();
            SensorCtrler = GetComponent<PawnSensorController>();
            ActionDataSelector = GetComponent<PawnActionDataSelector>();
            BB = GetComponent<SlimeBossBlackboard>();
            Movement = GetComponent<SlimeBossMovement>();
            AnimCtrler = GetComponent<SlimeBossAnimController>();
            ActionCtrler = GetComponent<SlimeBossActionController>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            BB.common.isGroggy.Skip(1).Subscribe(v =>
            {
                if (v)
                    EffectManager.Instance.ShowLooping("StunnedStars", fxAttachPoint.position, fxAttachPoint.rotation, Vector3.one).transform.SetParent(fxAttachPoint, true);
                else
                    fxAttachPoint.gameObject.Children().Select(c => c.GetComponent<EffectInstance>()).First(e => e != null && e.sourceName == "StunnedStars").Stop();
            }).AddTo(this);

            BB.currDecision.Subscribe(v =>
            {
                Movement.moveSpeed = BB.jumpSpeed;
                Movement.jumpHeight = BB.jumpHeight;
            }).AddTo(this);

            SensorCtrler.onWatchSomething += (s) =>
            {
                if (BB.IsSpawnFinished && !BB.IsDead && BB.TargetPawn == null && ValidateTargetCollider(s))
                    OnWatchSomethingOrDamagedHandler(s.GetComponent<PawnColliderHelper>().pawnBrain, 0.5f);
            };

            SensorCtrler.onTouchSomething += (s) =>
            {
                if (BB.IsSpawnFinished && !BB.IsDead && BB.TargetPawn == null && ValidateTargetCollider(s))
                    OnWatchSomethingOrDamagedHandler(s.GetComponent<PawnColliderHelper>().pawnBrain, 0.1f);
            };

            PawnHP.onDamaged += (damageContext) =>
            {
                //! Sender와 Recevier가 동일할 수 있기 때문에 반드시 'receiverBrain'을 먼저 체크해야 함
                if (damageContext.receiverBrain == this)
                    DamageReceiverHandler(ref damageContext);
                else if (damageContext.senderBrain == this)
                    DamageSenderHandler(ref damageContext);
            };

            onUpdate += () =>
            {
                if (!BB.IsSpawnFinished)
                    return;

                if (!ActionCtrler.CheckActionRunning())
                    BB.stat.RecoverStamina(Mathf.Max(ActionCtrler.prevActionContext.startTimeStamp, ActionCtrler.prevActionContext.finishTimeStamp), Time.deltaTime);
                BB.stat.ReduceStance(PawnHP.LastDamageTimeStamp, Time.deltaTime);

                ActionDataSelector.UpdateSelection(Time.deltaTime);
            };
        }

        protected virtual void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (damageContext.receiverBrain.PawnBB.IsDead)
                return;

            if (damageContext.receiverPenalty.Item1 != Game.PawnStatus.None)
            {
                if (ActionCtrler.CheckActionRunning())
                    ActionCtrler.CancelAction(false);

                switch (damageContext.receiverPenalty.Item1)
                {
                    case Game.PawnStatus.Groggy: ActionCtrler.StartAction(damageContext, "!OnGroggy", string.Empty); break;
                    case Game.PawnStatus.Staggered: ActionCtrler.StartAction(damageContext, "!OnHit", string.Empty); break;
                    case Game.PawnStatus.KnockDown: ActionCtrler.StartAction(damageContext, "!OnKnockDown", string.Empty); break;
                }
            }
            else if (damageContext.finalDamage > 0 || damageContext.actionResult == ActionResults.Blocked || damageContext.actionResult == ActionResults.GuardParried)
            {
                ActionCtrler.StartAddictiveAction(damageContext, "!OnHit");
            }
        }

        protected virtual void DamageSenderHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (damageContext.senderBrain.PawnBB.IsDead)
                return;

            if (damageContext.senderPenalty.Item1 != Game.PawnStatus.None && ActionCtrler.CheckActionRunning())
                ActionCtrler.CancelAction(false);

            switch (damageContext.actionResult)
            {
                case ActionResults.GuardParried: ActionCtrler.StartAction(damageContext, "!OnParried", string.Empty); break;
                case ActionResults.Blocked: ActionCtrler.StartAction(damageContext, "!OnBlocked", string.Empty); break;
            }
        }

        protected override void OnTickInternal(float interval)
        {
            base.OnTickInternal(interval);

            __decisionCoolTime = Mathf.Max(0, __decisionCoolTime - tickInterval);

            if (BB.IsInCombat)
            {
                if (BB.IsDead)
                {
                    BB.targetPawnHP.Value = null;
                    BB.isInCombat.Value = false;
                    InvalidateDecision(0.5f);
                }
                else if (!ValidateTargetBrain(BB.TargetBrain))
                {
                    var nextTarget = NextTargetBrain();
                    if (nextTarget != null)
                    {
                        __Logger.Log(gameObject, "nextTargetBrain", nextTarget);
                        OnWatchSomethingOrDamagedHandler(nextTarget.GetComponent<PawnBrainController>(), 0.5f);
                    }
                    else
                    {
                        __Logger.Log(gameObject, "nextTarget", "null");
                        BB.targetPawnHP.Value = null;
                        BB.isInCombat.Value = false;
                        InvalidateDecision(0.5f);
                    }
                }
                else if (RefreshAggresiveLevel())
                {
                    InvalidateDecision(0.1f);
                }
                else
                {
                    if (BB.CurrDecision != Decisions.Approach && __decisionCoolTime <= 0f)
                        BB.currDecision.Value = Decisions.Approach;

                    var canAction1 = !BB.IsGroggy && !BB.IsDown && !BB.IsJumping && !BB.IsBumping && !BB.IsSmashing;
                    var canAction2 = canAction1 && string.IsNullOrEmpty(ActionCtrler.PendingActionData.Item1) && !ActionCtrler.CheckActionRunning();
                    var canAction3 = canAction2 && !BuffCtrler.CheckStatus(Game.PawnStatus.Staggered) && CheckTargetVisibility();

                    //* 공격 시작
                    if (canAction3)
                    {
                        var selection = ActionDataSelector.RandomSelection(BB.TargetBrain.coreColliderHelper.GetApproachDistance(coreColliderHelper.transform.position), BB.stat.stamina.Value, true);
                        if (selection != null)
                            ActionCtrler.SetPendingAction(selection.actionName);
                    }
                }
            }
            else if (BB.CurrDecision == Decisions.None)
            {
                if (__decisionCoolTime <= 0f)
                    BB.currDecision.Value = Decisions.Idle;
            }
        }

        protected virtual bool RefreshAggresiveLevel()
        {
            if (BB.aggressiveLevel.Value == 0 && BB.stat.stamina.Value == BB.stat.maxStamina.Value)
            {
                BB.aggressiveLevel.Value = 1;
                return true;
            }
            else if (BB.aggressiveLevel.Value == 1 && BB.stat.stamina.Value == 0)
            {
                BB.aggressiveLevel.Value = 0;
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual bool ValidateTargetCollider(Collider otherCollider)
        {
            var colliderHelper = otherCollider.GetComponent<PawnColliderHelper>();
            if (colliderHelper != null && colliderHelper.pawnBrain != null && colliderHelper.pawnBrain.PawnBB.common.pawnId == PawnId.Hero && colliderHelper.pawnBrain.PawnBB.IsSpawnFinished && !colliderHelper.pawnBrain.PawnBB.IsDead)
                return true;
            else
                return false;
        }

        protected virtual bool ValidateTargetBrain(PawnBrainController targetBrain)
        {
            if (targetBrain != null && !targetBrain.PawnBB.IsDead)
            {
                Debug.Assert(targetBrain.PawnBB.IsSpawnFinished);
                // return SensorCtrler.ListeningColliders.Contains(targetBrain.coreColliderHelper.pawnCollider);
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual PawnBrainController NextTargetBrain()
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
                    return colliderHelper.pawnBrain;
            }

            return null;
        }
        protected virtual bool CheckTargetVisibility()
        {
            Debug.Assert(BB.TargetPawn != null);

            if (!SensorCtrler.WatchingColliders.Contains(BB.TargetBrain.coreColliderHelper.pawnCollider))
                return false;

            var center = Movement.capsule.TransformPoint(Movement.capsuleCollider.center);
            var direction = (BB.TargetCore.transform.position - coreColliderHelper.transform.position).Vector2D().normalized;
            if (Physics.SphereCast(center, Movement.capsuleCollider.radius, direction, out var hit, SensorCtrler.visionLen, LayerMask.GetMask("Pawn")) && hit.collider.TryGetComponent<PawnColliderHelper>(out var helper))
                return helper.pawnBrain == BB.TargetBrain;
            else
                return true;
        }

    }
}