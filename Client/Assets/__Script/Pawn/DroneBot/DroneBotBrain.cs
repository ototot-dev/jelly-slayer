using System.Linq;
using Packets;
using UniRx;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(DroneBotMovement))]
    [RequireComponent(typeof(DroneBotBlackboard))]
    [RequireComponent(typeof(DroneBotAnimController))]
    [RequireComponent(typeof(DroneBotActionController))]
    public class DroneBotBrain : PawnBrainController, IPawnSpawnable, IPawnMovable
    {
        [Header("Debug")]
        public bool debugActionDisabled;

#region ISpawnable/IMovable 구현
        Vector3 IPawnSpawnable.GetSpawnPosition() => transform.position;
        void IPawnSpawnable.OnStartSpawnHandler() { }
        void IPawnSpawnable.OnFinishSpawnHandler() { }
        void IPawnSpawnable.OnDespawnedHandler() { }
        void IPawnSpawnable.OnDeadHandler() 
        { 
            AnimCtrler.mainAnimator.SetTrigger("OnDead"); 
            Movement.SetMovementEnabled(false);
        }
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
#endregion

        public enum Decisions
        {
            None = -1,
            Idle,
            Catch,
            Hanging,
            Spacing,
            Approach,
            Max
        }

        public DroneBotBlackboard BB { get; private set; }
        public DroneBotMovement Movement { get; private set; }
        public DroneBotAnimController AnimCtrler { get; private set; }
        public DroneBotActionController ActionCtrler { get; private set; }
        public PawnStatusController StatusCtrler { get; private set; }
        public PawnSensorController SensorCtrler { get; private set; }
        public PawnActionDataSelector ActionDataSelector { get; private set; }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BB = GetComponent<DroneBotBlackboard>();
            Movement = GetComponent<DroneBotMovement>();
            AnimCtrler = GetComponent<DroneBotAnimController>();
            ActionCtrler = GetComponent<DroneBotActionController>();
            StatusCtrler = GetComponent<PawnStatusController>();
            SensorCtrler = GetComponent<PawnSensorController>();
            ActionDataSelector = GetComponent<PawnActionDataSelector>();
        }

        protected float __decisionCoolTime;


        protected override void StartInternal()
        {
            base.StartInternal();

            SensorCtrler.onListenSomething += (s) =>
            {
                if (BB.IsSpawnFinished && !BB.IsDead && BB.TargetPawn == null && ValidateTargetCollider(s.collider))
                    OnWatchSomethingOrDamagedHandler(s.collider.GetComponent<PawnColliderHelper>().pawnBrain, 0.5f);
            };

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
            
            PawnStatusCtrler.onStatusActive += (status) =>
            {
                if (status == PawnStatus.KnockDown || status == PawnStatus.Groggy)
                    InvalidateDecision(0.2f);
            };

            ActionCtrler.onActionStart += (actionContext, _) =>
            {
                if ((actionContext.actionData?.staminaCost ?? 0) > 0)
                    BB.stat.ReduceStamina(actionContext.actionData.staminaCost);

                //* 액션 수행 중에 현재 이동 제어를 끔
                InvalidateDecision(0.2f);
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
                {
                    BB.stat.RecoverStamina(Mathf.Max(ActionCtrler.prevActionContext.startTimeStamp, ActionCtrler.prevActionContext.finishTimeStamp), Time.deltaTime);

                    //* 스테미너 회복 후 액션 수행 가능으로 변경
                    if (BB.stat.stamina.Value == BB.stat.maxStamina.Value && PawnStatusCtrler.CheckStatus(Game.PawnStatus.CanNotAction))
                        PawnStatusCtrler.RemoveStatus(Game.PawnStatus.CanNotAction);
                    
                    //* Catch 단계가 완료됨을 확인 후에 Hanging 시작
                    if (BB.CurrDecision == Decisions.Catch && Movement.CheckCatchingDurationExpired())
                        GameContext.Instance.playerCtrler.heroBrain.Movement.StartHanging();
                }

                BB.stat.ReduceStance(PawnHP.LastDamageTimeStamp, Time.deltaTime);
                ActionDataSelector.UpdateSelection(Time.deltaTime);
            };
        }

        void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (damageContext.receiverBrain.PawnBB.IsDead)
                return;
            
            if (damageContext.receiverPenalty.Item1 == Game.PawnStatus.None)
            {
                //* receiverPenalty가 없다면 Sender 액션을 'Blocked' 혹은 'GuardParried'등으로 파훼한 것으로 판정하여, Sender 공격에 대한 반동 연출을 한다.
                //* 이 때, "!OnHit" 액션을 Addictive 모드로 실행하여 실제 Action이 실행되지는 않도록 한다.
                ActionCtrler.StartAddictiveAction(damageContext, "!OnHit");
            }
            else
            {
                if (ActionCtrler.CheckActionRunning())
                    ActionCtrler.CancelAction(false);

                switch (damageContext.receiverPenalty.Item1)
                {
                    case Game.PawnStatus.Staggered: ActionCtrler.StartAction(damageContext, "!OnHit", string.Empty); break;
                    case Game.PawnStatus.KnockDown: ActionCtrler.StartAction(damageContext, "!OnKnockDown", string.Empty); break;
                    case Game.PawnStatus.Groggy: ActionCtrler.StartAction(damageContext, "!OnGroggy", string.Empty); break;
                }
            }
        }
        void DamageSenderHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (damageContext.senderBrain.PawnBB.IsDead)
                return;

            if (damageContext.senderPenalty.Item1 != Game.PawnStatus.None && ActionCtrler.CheckActionRunning())
                ActionCtrler.CancelAction(false);

            switch (damageContext.actionResult)
            {
                case ActionResults.Blocked: 
                    ActionCtrler.StartAction(damageContext, "!OnBlocked", string.Empty);
                    break;

                case ActionResults.KickParried:
                case ActionResults.GuardParried:
                    ActionCtrler.StartAction(damageContext, damageContext.senderPenalty.Item1 == Game.PawnStatus.Groggy ? "!OnGroggy" : "!OnParried", string.Empty); 
                    break;
            }
        }

        public override void OnWatchSomethingOrDamagedHandler(PawnBrainController otherBrain, float reservedDecisionCoolTime)
        {
            BB.target.targetPawnHP.Value = otherBrain.PawnHP;
            BB.decision.aggressiveLevel.Value = 0f;
            InvalidateDecision(reservedDecisionCoolTime);

            //* 어그로를 다른 Jelly들에게 전파함
            foreach (var b in NpcSpawnManager.Instance.spawnedBrains)
            {
                if (b != this)
                    b.OnWatchSomethingOrDamagedHandler(otherBrain, reservedDecisionCoolTime);
            }
        }

        public override void InvalidateDecision(float decisionCoolTime = 0)
        {
            __decisionCoolTime = decisionCoolTime;
            BB.decision.currDecision.Value = Decisions.None;
        }

        public override void OnDecisionFinishedHandler() 
        { 
            InvalidateDecision(0.2f); 
        }

        protected override void OnTickInternal(float interval)
        {
            base.OnTickInternal(interval);

            __decisionCoolTime = Mathf.Max(0, __decisionCoolTime - tickInterval);

            if (BB.IsInCombat)
            {
                if (BB.IsDead)
                {
                    BB.target.targetPawnHP.Value = null;
                    BB.decision.aggressiveLevel.Value = -1f;
                    InvalidateDecision(1f);
                }
                else if (BB.IsDown || BB.IsGroggy)
                {
                    //* Down이나 Groogy 상태라면 Decision 갱신이 안되도록 공회전시킴
                    if (__decisionCoolTime <= 0f)
                        InvalidateDecision(0.2f);
                }
                else if (!ValidateTargetBrain(BB.TargetBrain))
                {
                    var nextTarget = NextTargetBrain();
                    if (nextTarget != null)
                    {
                        OnWatchSomethingOrDamagedHandler(nextTarget.GetComponent<PawnBrainController>(), 0.5f);
                    }
                    else
                    {
                        BB.target.targetPawnHP.Value = null;
                        BB.decision.aggressiveLevel.Value = -1f;
                        InvalidateDecision(0.5f);
                    }
                }
                else if (RefreshAggresiveLevel())
                {
                    InvalidateDecision(0.1f);
                }
                else if (BB.CurrDecision == Decisions.Catch)
                {   
                    ;
                }
                else if (BB.CurrDecision == Decisions.Hanging)
                {   
                    ;
                }
                else
                {
                    if (BB.CurrDecision == Decisions.Spacing || BB.CurrDecision == Decisions.Approach)
                    {
                        var targetCapsuleRadius = BB.TargetBrain.coreColliderHelper.GetCapsuleCollider() != null ? BB.TargetBrain.coreColliderHelper.GetCapsuleCollider().radius : 0f;
                        var distanceVec = (BB.TargetBrain.coreColliderHelper.transform.position - coreColliderHelper.transform.position).Vector2D();
                        var distance = Mathf.Max(0f, distanceVec.magnitude - targetCapsuleRadius);
                        
                        if (BB.CurrDecision == Decisions.Spacing && distance > BB.SpacingOutDistance)
                            InvalidateDecision(0.1f);
                        else if (BB.CurrDecision == Decisions.Approach && distance <= BB.SpacingInDistance)
                            InvalidateDecision(0.1f);
                    }
                    else
                    {
                        //* 액션 수행 중에는 Decision 갱신은 하지 않음
                        if (__decisionCoolTime <= 0f && !ActionCtrler.CheckActionRunning())
                            BB.decision.currDecision.Value = MakeDecision();
                    }
                }
            }
            else if (BB.CurrDecision == Decisions.None)
            {
                if (__decisionCoolTime <= 0f)
                    BB.decision.currDecision.Value = Decisions.Idle;
            }
        }

        protected virtual bool RefreshAggresiveLevel()
        {
            if (BB.decision.aggressiveLevel.Value == 0f && BB.stat.stamina.Value == BB.stat.maxStamina.Value)
            {
                BB.decision.aggressiveLevel.Value = 1f;
                return true;
            }
            else if (BB.decision.aggressiveLevel.Value == 1f && BB.stat.stamina.Value == 0f)
            {
                BB.decision.aggressiveLevel.Value = 0f;
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual Decisions MakeDecision()
        {
            Debug.Assert(BB.TargetPawn != null);
            //* Catch, Hanging 2개의 상태는 MakeDecision() 함수에 의해서 갱신될 수 없음
            Debug.Assert(BB.CurrDecision != Decisions.Catch && BB.CurrDecision != Decisions.Hanging);

            return BB.TargetBrain.coreColliderHelper.GetApproachDistance(coreColliderHelper.transform.position) < BB.SpacingInDistance ? Decisions.Spacing : Decisions.Approach;
        }
        
        protected virtual bool ValidateTargetCollider(Collider otherCollider)
        {
            if (otherCollider.TryGetComponent<PawnColliderHelper>(out var colliderHelper) && colliderHelper.pawnBrain != null)
                return colliderHelper.pawnBrain.PawnBB.common.pawnId == PawnId.Hero && colliderHelper.pawnBrain.PawnBB.IsSpawnFinished && !colliderHelper.pawnBrain.PawnBB.IsDead;
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

            var origin = Movement.capsule.TransformPoint(Movement.capsuleCollider.center);
            var direction = (BB.TargetCore.transform.position - coreColliderHelper.transform.position).Vector2D().normalized;
            if (Physics.SphereCast(origin, Movement.capsuleCollider.radius, direction, out var hit, SensorCtrler.visionLen, LayerMask.GetMask("HitBox")) && hit.collider.TryGetComponent<PawnColliderHelper>(out var colliderHelper))
                return colliderHelper.pawnBrain == BB.TargetBrain;
            else
                return true;
        }
    }
}