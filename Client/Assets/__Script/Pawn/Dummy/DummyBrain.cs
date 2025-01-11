using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class DummyBrain : PawnBrainController, IPawnSpawnable, IPawnMovable
    {
        [Header("Component")]
        public Animator animator;
        public FIMSpace.FProceduralAnimation.LegsAnimator legAnimator;

        Vector3 IPawnSpawnable.GetSpawnPosition() => Vector3.zero;
        public virtual void OnStartSpawnHandler() {}
        void IPawnSpawnable.OnFinishSpawnHandler() {}
        void IPawnSpawnable.OnDespawnedHandler() {}
        void IPawnSpawnable.OnDeadHandler() { animator.SetTrigger("OnDead"); Movement.SetMovementEnabled(false); }
        void IPawnSpawnable.OnLifeTimeOutHandler() { PawnHP.Die("TimeOut"); }
        bool IPawnMovable.IsJumping() { return false; }
        bool IPawnMovable.IsRolling() { return false; }
        bool IPawnMovable.IsOnGround() { return Movement.IsOnGround; }
        bool IPawnMovable.CheckReachToDestination() { return Movement.CheckReachToDestination(); }
        Vector3 IPawnMovable.GetDestination() { return Movement.destination; }
        float IPawnMovable.GetEstimateTimeToDestination() { return Movement.EstimateTimeToDestination(); }
        float IPawnMovable.GetDefaultMinApproachDistance() { return Movement.GetDefaultMinApproachDistance(); }
        bool IPawnMovable.GetFreezeMovement() { return Movement.freezeMovement; }
        bool IPawnMovable.GetFreezeRotation() { return Movement.freezeRotation; }
        void IPawnMovable.ReserveDestination(Vector3 destination) { Movement.ReserveDestination(destination); }
        float IPawnMovable.SetDestination(UnityEngine.Vector3 destination) { return Movement.SetDestination(destination); }
        void IPawnMovable.SetMinApproachDistance(float distance) { Movement.minApproachDistance = distance; }
        void IPawnMovable.SetFaceVector(Vector3 faceVec) { Movement.faceVec = faceVec; }
        void IPawnMovable.FreezeMovement(bool newValue) { Movement.freezeMovement = newValue; }
        void IPawnMovable.FreezeRotation(bool newValue) { Movement.freezeRotation = newValue; }
        void IPawnMovable.AddRootMotion(Vector3 deltaPosition, Quaternion deltaRotation) { Movement.AddRootMotion(deltaPosition, deltaRotation); }
        void IPawnMovable.StartJump(float jumpHeight) {}
        void IPawnMovable.FinishJump() {}
        void IPawnMovable.Teleport(Vector3 destination) { Movement.Teleport(destination); }
        void IPawnMovable.MoveTo(Vector3 destination) { Movement.destination = destination; }
        void IPawnMovable.FaceTo(Vector3 direction) { Movement.FaceTo(direction); }
        void IPawnMovable.Stop() { Movement.Stop(); }
        
        /// <summary>
        /// 
        /// </summary>
        public enum Decisions
        {
            None = -1,
            Idle,
            Approach, //* (액션을 수행하기 위해) 타겟에게 다가가기
            Max
        }

        public DummyBlackboard BB { get; private set; }
        public PawnMovementEx Movement { get; private set; }
        public PawnActionController ActionCtrler { get; private set; }
        public PawnSensorController SensorCtrler { get; private set; }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BB = GetComponent<DummyBlackboard>();
            Movement = GetComponent<PawnMovementEx>();
            ActionCtrler = GetComponent<PawnActionController>();
            SensorCtrler = GetComponent<PawnSensorController>();

            PawnHP.heartPoint.Value = BB.stat.maxHeartPoint.Value;
            Movement.moveSpeed = BB.movement.moveSpeed;
            Movement.moveBrake = BB.movement.moveBrake;
            Movement.rotateSpeed = BB.movement.rotateSpeed;
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            onUpdate += () =>
            {
                animator.transform.SetPositionAndRotation(coreColliderHelper.transform.position, coreColliderHelper.transform.rotation);
                // animator.SetLayerWeight(1, Mathf.Clamp01(animator.GetLayerWeight(1) + (ActionCtrler.CheckActionRunning() ? 5 : -5) * Time.deltaTime));
            };

            // onFixedUpdate += () => 
            // {
            //     animator.transform.SetPositionAndRotation(coreColliderHelper.transform.position, coreColliderHelper.transform.rotation);
            // };

            PawnHP.onDamaged += (damageContext) =>
            {
                if (!BB.IsSpawnFinished || BB.IsDead || damageContext.receiverBrain != this)
                    return;

                if (damageContext.receiverBrain == this)
                {
                    animator.SetTrigger("OnDamaged");
                    Movement.FaceTo(damageContext.senderBrain.coreColliderHelper.transform.position);

                    var pushBackVec = damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                    Observable.EveryFixedUpdate()
                        .TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                        .Subscribe(_ => Movement.AddRootMotion(4 * Time.fixedDeltaTime * pushBackVec, Quaternion.identity)).AddTo(this);

                    // Hit Effect
                    var pos = damageContext.hitPoint;
                    EffectCenter.CreateEffect(30, pos);

                    // OnPawnDamaged
                    GameManager.Instance.PawnDamaged(ref damageContext);

                }
            };

            ActionCtrler.onActionCanceled += (_, _) => animator.speed = 1;
            ActionCtrler.onActionFinished += (_) => animator.speed = 1;

            SensorCtrler.onWatchSomething += (s) =>
            {
                if (ValidateTargetCollider(s))
                    HandleWatchSomethingOrDamaged(s.GetComponent<PawnColliderHelper>().pawnBrain, 0.5f);

                __Logger.Log(gameObject, nameof(SensorCtrler.onWatchSomething), s);
            };

            SensorCtrler.onTouchSomething += (s) =>
            {
                if (ValidateTargetCollider(s))
                    HandleWatchSomethingOrDamaged(s.GetComponent<PawnColliderHelper>().pawnBrain, 0.1f);
            };
        }
        
        void HandleWatchSomethingOrDamaged(PawnBrainController otherBrain, float reservedDecisionCoolTime)
        {
            if (BB.TargetPawn == null)
            {
                BB.decision.targetPawnHP.Value = otherBrain.PawnHP;
                BB.decision.isInCombat.Value = true;
                InvalidateDecision(reservedDecisionCoolTime);
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
                else
                {
                    if (BB.CurrDecision == Decisions.Approach)
                    {
                        // if (!ActionCtrler.CheckActionRunning() && (BB.TargetBrain.core.transform.position - core.transform.position).Magnitude2D() < 1)
                        //     ActionCtrler.pendingAction = new("Slash", Time.time);
                    }
                    else if (BB.CurrDecision == Decisions.None && __decisionCoolTime <= 0)
                    {
                        BB.decision.currDecision.Value = MakeDecision();
                    }
                }
            }
            else if (BB.CurrDecision == Decisions.None && __decisionCoolTime <= 0)
            {
                BB.decision.currDecision.Value = Decisions.Idle;
            }
        }

        Decisions MakeDecision()
        {
            Debug.Assert(BB.TargetPawn != null);
            return Decisions.Approach;
        }

        float __decisionCoolTime;

        void InvalidateDecision(float decisionCoolTime = 0)
        {
            BB.decision.currDecision.Value = Decisions.None;
            __decisionCoolTime = decisionCoolTime;
        }

        bool ValidateTargetCollider(Collider otherCollider)
        {
            if (otherCollider.TryGetComponent<PawnColliderHelper>(out var colliderHelper) && colliderHelper.pawnBrain != null)
                return colliderHelper.pawnBrain.PawnBB.IsSpawnFinished && !colliderHelper.pawnBrain.PawnBB.IsDead;
            else
                return false;
        }

        bool ValidateTargetBrain()
        {
            if (BB.TargetBrain == null || BB.TargetBrain.PawnBB.IsDead)
                return false;

            if (!BB.TargetBrain.PawnBB.IsSpawnFinished)
                __Logger.ErrorR1(gameObject, "BB.WatchingPawnBrain.PawnBB.IsSpawnFinished is false", nameof(BB.TargetPawn), BB.TargetPawn);

            Debug.Assert(BB.TargetBrain.PawnBB.IsSpawnFinished);

            if (BB.CurrDecision == Decisions.Approach)
                return SensorCtrler.ListeningColliders.Contains(BB.TargetBrain.coreColliderHelper.pawnCollider);
            else
                return SensorCtrler.WatchingColliders.Contains(BB.TargetBrain.coreColliderHelper.pawnCollider);
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
                    __Logger.LogR2(gameObject, nameof(NextTargetBrain), "return...", nameof(colliderHelper.pawnBrain), colliderHelper.pawnBrain);
                    return colliderHelper.pawnBrain;
                }
            }

            __Logger.LogR2(gameObject, nameof(NextTargetBrain), "return... null");
            return null;
        }
    }
}
