using System.Linq;
using Packets;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(DroneBotMovement))]
    [RequireComponent(typeof(DroneBotBlackboard))]
    [RequireComponent(typeof(DroneBotAnimController))]
    [RequireComponent(typeof(DroneBotActionController))]
    public class DroneBotBrain : PawnBrainController, IPawnSpawnable, IPawnMovable
    {
#region ISpawnable/IMovable 구현
        Vector3 IPawnSpawnable.GetSpawnPosition() => transform.position;
        void IPawnSpawnable.OnStartSpawnHandler() { }
        void IPawnSpawnable.OnFinishSpawnHandler() 
        { 
            if (GameContext.Instance.HeroBrain.droneBotFormationCtrler.AssignDroneBot(this))
                BB.decision.hostBrain.Value = GameContext.Instance.HeroBrain;
        }
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
        void IPawnMovable.AddRootMotion(Vector3 deltaPosition, Quaternion deltaRotation, float deltaTime) { Movement.AddRootMotion(deltaPosition, deltaRotation, deltaTime); }
        void IPawnMovable.StartJump(float jumpHeight) {}
        void IPawnMovable.FinishJump() {}
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
            Strike,
            Hooking,
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

        protected override void StartInternal()
        {
            base.StartInternal();
            
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

            onUpdate += () => 
            {   
                if (!BB.IsSpawnFinished)
                    return;

                //* Catch 단계가 완료됨을 확인 후에 Hanging 시작
                if (!BB.IsHanging && BB.CurrDecision == Decisions.Catch && Movement.CheckPrepareHangingDone())
                    GameContext.Instance.playerCtrler.possessedBrain.Movement.StartHanging(Movement.prepareHangingDuration > 0.1f);
            };
        }

        protected float __decisionCoolTime;

        public override void InvalidateDecision(float decisionCoolTime = 0f)
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

            if (!BB.IsSpawnFinished)
                return;

            __decisionCoolTime = Mathf.Max(0, __decisionCoolTime - tickInterval);

            if (BB.IsInCombat)
            {
                if (BB.IsDead)
                {
                    BB.common.targetPawnHP.Value = null;
                    BB.decision.aggressiveLevel.Value = -1f;
                    InvalidateDecision(1f);
                }
                else if (BB.IsDown || BB.IsGroggy)
                {
                    //* Down이나 Groogy 상태라면 Decision 갱신이 안되도록 공회전시킴
                    if (__decisionCoolTime <= 0f)
                        InvalidateDecision(0.2f);
                }
                else if (BB.CurrDecision == Decisions.Catch)
                {   
                    ;
                }
                else if (BB.CurrDecision == Decisions.Hooking)
                {   
                    ;
                }
                else
                {
                    if (BB.CurrDecision == Decisions.None)
                    {
                        //* 액션 수행 중에는 Decision 갱신은 하지 않음
                        if (__decisionCoolTime <= 0f && !ActionCtrler.CheckActionRunning())
                            BB.decision.currDecision.Value = MakeDecision();
                    }
                    else if (BB.CurrDecision == Decisions.Spacing || BB.CurrDecision == Decisions.Approach)
                    {
                        var distance = (BB.FormationSpot.transform.position - coreColliderHelper.transform.position).Magnitude2D();
                        if (BB.CurrDecision == Decisions.Spacing && distance > BB.SpacingOutDistance)
                            InvalidateDecision();
                        else if (BB.CurrDecision == Decisions.Approach && distance <= BB.SpacingInDistance)
                            InvalidateDecision();
                    }
                }
            }
            else
            {
                if (BB.CurrDecision == Decisions.None && __decisionCoolTime <= 0f)
                    BB.decision.currDecision.Value = MakeDecision();
            }
        }

        protected virtual Decisions MakeDecision()
        {
            if (BB.IsInCombat)
            {
                Debug.Assert(BB.HostBrain != null);
                //* Catch, Hanging 2개의 상태는 MakeDecision() 함수에 의해서 갱신될 수 없음
                Debug.Assert(BB.CurrDecision != Decisions.Catch && BB.CurrDecision != Decisions.Hooking);

                return (BB.FormationSpot.transform.position - coreColliderHelper.transform.position).Magnitude2D() < BB.SpacingInDistance ? Decisions.Spacing : Decisions.Approach;
            }
            else
            {
                return Decisions.Idle;
            }
        }
    }
}