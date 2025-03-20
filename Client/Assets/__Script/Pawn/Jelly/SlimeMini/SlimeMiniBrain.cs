using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class SlimeMiniBrain : PawnBrainController, IPawnSpawnable, IPawnMovable
    {
        [Header("Parameter")]
        public float emitPowerOnSpawned = 1f;
        
#region ISpawnable/IMovable 구현
        Vector3 IPawnSpawnable.GetSpawnPosition() => transform.position;
        void IPawnSpawnable.OnStartSpawnHandler() 
        {
            if (AnimCtrler.springMassCore.TryGetComponent<Rigidbody>(out var rigidBody))
            {   
                rigidBody.gameObject.layer = LayerMask.NameToLayer("Projectile");
                rigidBody.isKinematic = false;
                rigidBody.angularDamping = 0.2f;
                rigidBody.linearDamping = 0.1f;
                rigidBody.mass = 1f;

                //* 방출 효과
                Observable.NextFrame(FrameCountType.FixedUpdate).Subscribe(_ => 
                {
                    rigidBody.AddForce(emitPowerOnSpawned * (new Vector3(1f, 0f, 1f).Random() + Vector3.up), ForceMode.VelocityChange);
                    rigidBody.AddTorque(10f * emitPowerOnSpawned * Vector3.one.Random());
                }).AddTo(this);

                var impulseTimeStamp = Time.time;
                
                Observable.EveryFixedUpdate().TakeWhile(_ => !BB.IsSpawnFinished)
                    .TakeUntil(Observable.Timer(TimeSpan.FromSeconds(10f)))
                    .DoOnCancel(() => 
                    {
                        if (!BB.IsSpawnFinished)
                        {
                            BB.common.isSpawnFinished.Value = true;
                            (this as IPawnSpawnable).OnFinishSpawnHandler();
                        }
                    })
                    .DoOnCompleted(() =>
                    {
                        if (!BB.IsSpawnFinished)
                        {
                            BB.common.isSpawnFinished.Value = true;
                            (this as IPawnSpawnable).OnFinishSpawnHandler();
                        }
                    })
                    .Subscribe(_ =>
                    {
                        if (Time.time - impulseTimeStamp > 0.2f && rigidBody.linearVelocity.sqrMagnitude < 0.1f)
                        {
                            BB.common.isSpawnFinished.Value = true;
                            (this as IPawnSpawnable).OnFinishSpawnHandler();
                        }
                    }).AddTo(this);
            }
        }
        void IPawnSpawnable.OnFinishSpawnHandler() 
        { 
            if (AnimCtrler.springMassCore.TryGetComponent<Rigidbody>(out var rigidBody))
            {
                rigidBody.gameObject.layer = LayerMask.NameToLayer("Default");
                rigidBody.isKinematic = true;

                //* Capsule 위치를 rigidBody 위치로 맞춰줌
                coreColliderHelper.transform.position = rigidBody.transform.position;
            }
        }
        void IPawnSpawnable.OnDespawnedHandler() { }
        void IPawnSpawnable.OnDeadHandler() { AnimCtrler.mainAnimator.SetTrigger("OnDead"); Movement.SetMovementEnabled(false); }
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
            Approach,
            Max
        }

        public PawnStatusController BuffCtrler { get; private set; }
        public PawnSensorController SensorCtrler { get; private set; }
        public PawnActionDataSelector ActionDataSelector { get; private set; }
        public SlimeMiniBlackboard BB { get; private set; }
        public SlimeMiniMovement Movement { get; private set; }
        public SlimeMiniAnimController AnimCtrler { get; private set; }
        public SlimeMiniActionController ActionCtrler { get; private set; }
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

        public override void InvalidateDecision(float decisionCoolTime = 0f)
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
            BB = GetComponent<SlimeMiniBlackboard>();
            Movement = GetComponent<SlimeMiniMovement>();
            AnimCtrler = GetComponent<SlimeMiniAnimController>();
            ActionCtrler = GetComponent<SlimeMiniActionController>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

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
                if (damageContext.receiverBrain == this)
                    OnReceiveDamageHandler(ref damageContext);
            };

            ActionCtrler.onActionStart += (actionContext, _) =>
            {
                if ((actionContext.actionData?.staminaCost ?? 0) > 0)
                    BB.stat.ReduceStamina(actionContext.actionData.staminaCost);
            };

            ActionCtrler.onActionFinished += (actionContext) =>
            {
                if (!actionContext.actionCanceled && actionContext.actionName == "Swelling")
                    ActionCtrler.SetPendingAction("Pop");
            };
        }

        protected virtual void OnReceiveDamageHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
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
            else if (damageContext.finalDamage > 0)
            {
                ActionCtrler.StartAddictiveAction(damageContext, "!OnHit");
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
                    
                    //* 공격 시작
                    if (string.IsNullOrEmpty(ActionCtrler.PendingActionData.Item1) && !BB.IsJumping && !ActionCtrler.CheckActionRunning() && !BuffCtrler.CheckStatus(Game.PawnStatus.Staggered) && CheckTargetVisibility())
                    {
                        // var selection = ActionDataSelector.RandomPick(0f, BB.stat.stamina.Value);
                        // if (selection != null)
                        //     ActionCtrler.SetPendingAction(selection.actionName);
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