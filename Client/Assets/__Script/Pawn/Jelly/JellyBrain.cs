using System.Linq;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{
    public class JellyBrain : PawnBrainController, IPawnSpawnable, IPawnMovable
    {
        
#region ISpawnable/IMovable 구현
        Vector3 IPawnSpawnable.GetSpawnPosition() => transform.position;
        void IPawnSpawnable.OnStartSpawnHandler() { __pawnMovement.capsule.gameObject.layer = LayerMask.NameToLayer("PawnOverlapped"); }
        void IPawnSpawnable.OnFinishSpawnHandler() { __pawnMovement.capsule.gameObject.layer = LayerMask.NameToLayer("Pawn"); }
        void IPawnSpawnable.OnDespawnedHandler() { }
        void IPawnSpawnable.OnDeadHandler() 
        { 
            __pawnAnimCtrler.mainAnimator.SetTrigger("OnDead"); 
            __pawnMovement.SetMovementEnabled(false);
        }
        void IPawnSpawnable.OnLifeTimeOutHandler() { PawnHP.Die("TimeOut"); }
        bool IPawnMovable.CheckReachToDestination() { return __pawnMovement.CheckReachToDestination(); }
        bool IPawnMovable.IsJumping() { return false; }
        bool IPawnMovable.IsRolling() { return false; }
        bool IPawnMovable.IsOnGround() { return __pawnMovement.IsOnGround; }
        Vector3 IPawnMovable.GetDestination() { return __pawnMovement.destination; }
        float IPawnMovable.GetEstimateTimeToDestination() { return __pawnMovement.EstimateTimeToDestination(); }
        float IPawnMovable.GetDefaultMinApproachDistance() { return __pawnMovement.GetDefaultMinApproachDistance(); }
        bool IPawnMovable.GetFreezeMovement() { return __pawnMovement.freezeMovement; }
        bool IPawnMovable.GetFreezeRotation() { return __pawnMovement.freezeRotation; }
        void IPawnMovable.ReserveDestination(Vector3 destination) { __pawnMovement.ReserveDestination(destination); }
        float IPawnMovable.SetDestination(Vector3 destination) { return __pawnMovement.SetDestination(destination); }
        void IPawnMovable.SetMinApproachDistance(float distance) { __pawnMovement.minApproachDistance = distance; }
        void IPawnMovable.SetFaceVector(Vector3 faceVec) { __pawnMovement.faceVec = faceVec; }
        void IPawnMovable.FreezeMovement(bool newValue) { __pawnMovement.freezeMovement = newValue; }
        void IPawnMovable.FreezeRotation(bool newValue) { __pawnMovement.freezeRotation = newValue; }
        void IPawnMovable.AddRootMotion(Vector3 deltaPosition, Quaternion deltaRotation) { __pawnMovement.AddRootMotion(deltaPosition, deltaRotation); }
        void IPawnMovable.StartJump(float jumpHeight) {}
        void IPawnMovable.FinishJump() {}
        void IPawnMovable.Teleport(Vector3 destination) { __pawnMovement.Teleport(destination); }
        void IPawnMovable.MoveTo(Vector3 destination) { __pawnMovement.destination = destination; }
        void IPawnMovable.FaceTo(Vector3 direction) { __pawnMovement.FaceTo(direction); }
        void IPawnMovable.Stop() { __pawnMovement.Stop(); }
#endregion

        public enum Decisions
        {
            None = -1,
            Idle,
            Spacing,
            Approach,
            Max
        }

        public JellyBlackboard JellyBB { get; private set; }
        public PawnStatusController StatusCtrler { get; private set; }
        public PawnSensorController SensorCtrler { get; private set; }
        public PawnActionDataSelector ActionDataSelector { get; private set; }
        protected PawnMovementEx __pawnMovement;
        protected PawnAnimController __pawnAnimCtrler;
        protected PawnActionController __pawnActionCtrler;
        
        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            
            JellyBB = GetComponent<JellyBlackboard>();
            StatusCtrler = GetComponent<PawnStatusController>();
            SensorCtrler = GetComponent<PawnSensorController>();
            ActionDataSelector = GetComponent<PawnActionDataSelector>();
            __pawnMovement = GetComponent<PawnMovementEx>();
            __pawnAnimCtrler = GetComponent<PawnAnimController>();
            __pawnActionCtrler = GetComponent<PawnActionController>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();
            
            PawnHP.onDamaged += (damageContext) =>
            {
                //! Sender와 Recevier가 동일할 수 있기 때문에 반드시 'receiverBrain'을 먼저 체크해야 함
                if (damageContext.receiverBrain == this)
                    DamageReceiverHandler(ref damageContext);
                else if (damageContext.senderBrain == this)
                    DamageSenderHandler(ref damageContext);
            };
        }

        protected virtual void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext) {}
        protected virtual void DamageSenderHandler(ref PawnHeartPointDispatcher.DamageContext damageContext) {}
    }
}
