using System.Linq;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{
    public class JellyManBrain : PawnBrainController, ISpawnable, IMovable
    {
        [Header("Component")]
        public Transform fxAttachPoint;
        public Transform spawnAttachPoint;
        public Transform spawnAttachSource;
        protected Rigidbody __spawnAttachSourceRigidBody;
        
#region ISpawnable/IMovable 구현
        Vector3 ISpawnable.GetSpawnPosition() => transform.position;
        void ISpawnable.OnStartSpawnHandler() 
        {
            if (spawnAttachPoint != null && spawnAttachSource != null && spawnAttachSource.TryGetComponent<Rigidbody>(out __spawnAttachSourceRigidBody))
            {
                __jellyManBB.common.isRagdoll.Value = true;
                __spawnAttachSourceRigidBody.isKinematic = true;
                __pawnMovement.capsule.gameObject.layer = LayerMask.NameToLayer("PawnOverlapped");
            }
        }
        void ISpawnable.OnFinishSpawnHandler() 
        {
            __jellyManBB.common.isRagdoll.Value = false;
            __pawnMovement.capsule.gameObject.layer = LayerMask.NameToLayer("Pawn");
        }
        void ISpawnable.OnDespawnedHandler() { }
        void ISpawnable.OnDeadHandler() { __pawnAnimCtrler.mainAnimator.SetTrigger("OnDead"); __pawnMovement.SetMovementEnabled(false); }
        void ISpawnable.OnLifeTimeOutHandler() { PawnHP.Die("TimeOut"); }
        bool IMovable.CheckReachToDestination() { return __pawnMovement.CheckReachToDestination(); }
        bool IMovable.IsJumping() { return false; }
        bool IMovable.IsRolling() { return false; }
        Vector3 IMovable.GetDestination() { return __pawnMovement.destination; }
        float IMovable.GetEstimateTimeToDestination() { return __pawnMovement.EstimateTimeToDestination(); }
        float IMovable.GetDefaultMinApproachDistance() { return __pawnMovement.GetDefaultMinApproachDistance(); }
        bool IMovable.GetFreezeMovement() { return __pawnMovement.freezeMovement; }
        bool IMovable.GetFreezeRotation() { return __pawnMovement.freezeRotation; }
        void IMovable.ReserveDestination(Vector3 destination) { __pawnMovement.ReserveDestination(destination); }
        float IMovable.SetDestination(Vector3 destination) { return __pawnMovement.SetDestination(destination); }
        void IMovable.SetMinApproachDistance(float distance) { __pawnMovement.minApproachDistance = distance; }
        void IMovable.SetFaceVector(Vector3 faceVec) { __pawnMovement.faceVec = faceVec; }
        void IMovable.FreezeMovement(bool newValue) { __pawnMovement.freezeMovement = newValue; }
        void IMovable.FreezeRotation(bool newValue) { __pawnMovement.freezeRotation = newValue; }
        void IMovable.AddRootMotion(Vector3 deltaPosition, Quaternion deltaRotation) { __pawnMovement.AddRootMotion(deltaPosition, deltaRotation); }
        void IMovable.Teleport(Vector3 destination) { __pawnMovement.Teleport(destination); }
        void IMovable.MoveTo(Vector3 destination) { __pawnMovement.MoveTo(destination); }
        void IMovable.FaceTo(Vector3 direction) { __pawnMovement.FaceTo(direction); }
        void IMovable.Stop() { __pawnMovement.Stop(); }
#endregion

        public enum Decisions
        {
            None = -1,
            Idle,
            Spacing,
            Approach,
            Max
        }

        public PawnBuffController BuffCtrler { get; private set; }
        public PawnSensorController SensorCtrler { get; private set; }
        public PawnActionDataSelector ActionDataSelector { get; private set; }
        protected PawnMovementEx __pawnMovement;
        protected PawnAnimController __pawnAnimCtrler;
        protected PawnActionController __pawnActionCtrler;
        protected JellyManBlackboard __jellyManBB;
        protected float __decisionCoolTime;
        
        public override void OnDecisionFinishedHandler() 
        { 
            InvalidateDecision(0.2f); 
        }

        public override void OnWatchSomethingOrDamagedHandler(PawnBrainController otherBrain, float reservedDecisionCoolTime)
        {
            __jellyManBB.decision.targetPawnHP.Value = otherBrain.PawnHP;
            __jellyManBB.decision.isInCombat.Value = true;
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
            __jellyManBB.decision.currDecision.Value = Decisions.None;
        }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BuffCtrler = GetComponent<PawnBuffController>();
            SensorCtrler = GetComponent<PawnSensorController>();
            ActionDataSelector = GetComponent<PawnActionDataSelector>();
            __pawnMovement = GetComponent<PawnMovementEx>();
            __pawnAnimCtrler = GetComponent<PawnAnimController>();
            __pawnActionCtrler = GetComponent<PawnActionController>();
            __jellyManBB = GetComponent<JellyManBlackboard>();
        }

        protected override void StartInternal()
        {
            __jellyManBB.common.isRagdoll.Skip(1).Subscribe(v =>
            {
                if (v)
                    __pawnAnimCtrler.StartRagdoll(false, true);
                else
                    __pawnAnimCtrler.FinishRagdoll(1f);
            }).AddTo(this);

            base.StartInternal();

            __jellyManBB.common.isStunned.Skip(1).Subscribe(v =>
            {
                if (v)
                    EffectManager.Instance.ShowLooping("StunnedStars", fxAttachPoint.position, fxAttachPoint.rotation, Vector3.one).transform.SetParent(fxAttachPoint, true);
                else
                    fxAttachPoint.gameObject.Children().Select(c => c.GetComponent<EffectInstance>()).First(e => e != null && e.effectName == "StunnedStars").Stop();
            }).AddTo(this);

            __jellyManBB.decision.currDecision.Subscribe(v =>
            {
                __pawnMovement.moveSpeed = v == Decisions.Spacing ? __jellyManBB.body.walkSpeed : __jellyManBB.body.moveSpeed;
            }).AddTo(this);

            SensorCtrler.onListenSomething += (s) =>
            {
                if (__jellyManBB.IsSpawnFinished && !__jellyManBB.IsDead && __jellyManBB.TargetPawn == null && ValidateTargetCollider(s.collider))
                    OnWatchSomethingOrDamagedHandler(s.collider.GetComponent<PawnColliderHelper>().pawnBrain, 0.5f);
            };

            SensorCtrler.onWatchSomething += (s) =>
            {
                if (__jellyManBB.IsSpawnFinished && !__jellyManBB.IsDead && __jellyManBB.TargetPawn == null && ValidateTargetCollider(s))
                    OnWatchSomethingOrDamagedHandler(s.GetComponent<PawnColliderHelper>().pawnBrain, 0.5f);
            };

            SensorCtrler.onTouchSomething += (s) =>
            {
                if (__jellyManBB.IsSpawnFinished && !__jellyManBB.IsDead && __jellyManBB.TargetPawn == null && ValidateTargetCollider(s))
                    OnWatchSomethingOrDamagedHandler(s.GetComponent<PawnColliderHelper>().pawnBrain, 0.1f);
            };

            PawnHP.onDamaged += (damageContext) =>
            {
                //! Sender와 Recevier가 동일할 수 있기 때문에 반드시 'receiverBrain'을 먼저 체크해야 함
                if (damageContext.receiverBrain == this)
                    DamageReceiverHandler(ref damageContext);
                else if (damageContext.senderBrain == this)
                    DamageSenderHandler(ref damageContext);

                //* 데미지를 받은 후엔 바로 반격할 수 있도록 'BuffType.CanNotAction' 상태를 해제함
                PawnBuff.RemoveBuff(BuffTypes.CanNotAction);
            };

            __pawnActionCtrler.onActionStart += (actionContext, _) =>
            {
                if ((actionContext.actionData?.staminaCost ?? 0) > 0)
                    __jellyManBB.stat.stamina.Value = Mathf.Max(0f, __jellyManBB.stat.stamina.Value - actionContext.actionData.staminaCost);
            };

            __pawnActionCtrler.onActionFinished += (_) =>
            {
                //* 스테미너 회복 전까지는 액션 수행 불가
                if (__jellyManBB.stat.stamina.Value < 1f)
                    PawnBuff.AddBuff(BuffTypes.CanNotAction);
            };

            onUpdate += () => 
            {   
                if (!__jellyManBB.IsSpawnFinished)
                    return;

                if (!__pawnActionCtrler.CheckActionRunning())
                {
                    __jellyManBB.stat.RecoverStamina(Mathf.Max(__pawnActionCtrler.prevActionContext.startTimeStamp, __pawnActionCtrler.prevActionContext.finishTimeStamp), Time.deltaTime);

                    //* 스테니머 회복 후 액션 수행 가능으로 변경
                    if (__jellyManBB.stat.stamina.Value ==__jellyManBB.stat.maxStamina.Value && PawnBuff.CheckBuff(BuffTypes.CanNotAction))
                        PawnBuff.RemoveBuff(BuffTypes.CanNotAction);
                }

                __jellyManBB.stat.ReduceStance(PawnHP.LastDamageTimeStamp, Time.deltaTime);
                ActionDataSelector.UpdateSelection(Time.deltaTime);
            };

            onLateUpdate += () =>
            {
                //* __spawnAttachSourceRigidBody.isKinematic 값이 true여야만 드래깅이 가능한 상태임
                if (spawnAttachPoint != null && spawnAttachSource != null && __spawnAttachSourceRigidBody != null && __spawnAttachSourceRigidBody.isKinematic)
                    spawnAttachSource.SetPositionAndRotation(spawnAttachPoint.position, spawnAttachPoint.rotation);
            };
        }

        protected virtual void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (damageContext.receiverBrain.PawnBB.IsDead)
                return;
            
            if (damageContext.receiverPenalty.Item1 == BuffTypes.None)
            {
                //* receiverPenalty가 없다면 Sender 액션을 'Blocked' 혹은 'PassiveParried'등으로 파훼한 것으로 판정하여, Sender 공격에 대한 반동 연출을 한다.
                //* 이 때, "!OnHit" 액션을 Addictive 모드로 실행하여 실제 Action이 실행되지는 않도록 한다.
                __pawnActionCtrler.StartAddictiveAction(damageContext, "!OnHit");
            }
            else
            {
                if (__pawnActionCtrler.CheckActionRunning())
                    __pawnActionCtrler.CancelAction(false);

                switch (damageContext.receiverPenalty.Item1)
                {
                    case BuffTypes.Groggy: __pawnActionCtrler.StartAction(damageContext, "!OnGroggy"); break;
                    case BuffTypes.Staggered: __pawnActionCtrler.StartAction(damageContext, "!OnHit"); break;
                    case BuffTypes.KnockDown: __pawnActionCtrler.StartAction(damageContext, "!OnKnockDown"); break;
                }
            }
        }

        protected virtual void DamageSenderHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (damageContext.senderBrain.PawnBB.IsDead)
                return;

            if (damageContext.senderPenalty.Item1 != BuffTypes.None && __pawnActionCtrler.CheckActionRunning())
                __pawnActionCtrler.CancelAction(false);

            switch (damageContext.actionResult)
            {
                case ActionResults.Blocked: 
                    __pawnActionCtrler.StartAction(damageContext, "!OnBlocked"); break;

                case ActionResults.ActiveParried:
                case ActionResults.PassiveParried: 
                    __pawnActionCtrler.StartAction(damageContext, "!OnParried"); break;
            }
        }

        protected override void OnTickInternal(float interval)
        {
            base.OnTickInternal(interval);

            __decisionCoolTime = Mathf.Max(0, __decisionCoolTime - tickInterval);

            if (__jellyManBB.IsInCombat)
            {
                if (__jellyManBB.IsDead)
                {
                    __jellyManBB.decision.targetPawnHP.Value = null;
                    __jellyManBB.decision.isInCombat.Value = false;
                    InvalidateDecision(0.5f);
                }
                else if (!ValidateTargetBrain(__jellyManBB.TargetBrain))
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
                        __jellyManBB.decision.targetPawnHP.Value = null;
                        __jellyManBB.decision.isInCombat.Value = false;
                        InvalidateDecision(0.5f);
                    }
                }
                else if (RefreshAggresiveLevel())
                {
                    InvalidateDecision(0.1f);
                }
                else
                {
                    if (__jellyManBB.CurrDecision == Decisions.Spacing || __jellyManBB.CurrDecision == Decisions.Approach)
                    {
                        var targetCapsuleRadius = __jellyManBB.TargetBrain.coreColliderHelper.GetCapsuleCollider() != null ? __jellyManBB.TargetBrain.coreColliderHelper.GetCapsuleCollider().radius : 0f;
                        var distanceVec = (__jellyManBB.TargetBrain.coreColliderHelper.transform.position - coreColliderHelper.transform.position).Vector2D();
                        var distance = Mathf.Max(0f, distanceVec.magnitude - targetCapsuleRadius);
                        
                        if (__jellyManBB.CurrDecision == Decisions.Spacing && distance > __jellyManBB.decision.spacingOutDistance)
                            InvalidateDecision(1f);
                        else if (__jellyManBB.CurrDecision == Decisions.Approach && distance <= __jellyManBB.decision.spacingInDistance)
                            InvalidateDecision(0.2f);
                    }
                    else
                    {
                        if (__decisionCoolTime <= 0f)
                            __jellyManBB.decision.currDecision.Value = MakeDecision();
                    }
                }
            }
            else if (__jellyManBB.CurrDecision == Decisions.None)
            {
                if (__decisionCoolTime <= 0f)
                    __jellyManBB.decision.currDecision.Value = Decisions.Idle;
            }
        }

        protected virtual bool RefreshAggresiveLevel()
        {
            if (__jellyManBB.decision.aggressiveLevel.Value == 0 && __jellyManBB.stat.stamina.Value == __jellyManBB.stat.maxStamina.Value)
            {
                __jellyManBB.decision.aggressiveLevel.Value = 1;
                return true;
            }
            else if (__jellyManBB.decision.aggressiveLevel.Value == 1 && __jellyManBB.stat.stamina.Value == 0)
            {
                __jellyManBB.decision.aggressiveLevel.Value = 0;
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual Decisions MakeDecision()
        {
            Debug.Assert(__jellyManBB.TargetPawn != null);
            return __jellyManBB.TargetBrain.coreColliderHelper.GetApproachDistance(coreColliderHelper.transform.position) < __jellyManBB.decision.spacingInDistance ? Decisions.Spacing : Decisions.Approach;
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
            Debug.Assert(__jellyManBB.TargetPawn != null);

            if (!SensorCtrler.WatchingColliders.Contains(__jellyManBB.TargetBrain.coreColliderHelper.pawnCollider))
                return false;

            var origin = __pawnMovement.capsule.TransformPoint(__pawnMovement.capsuleCollider.center);
            var direction = (__jellyManBB.TargetCore.transform.position - coreColliderHelper.transform.position).Vector2D().normalized;
            if (Physics.SphereCast(origin, __pawnMovement.capsuleCollider.radius, direction, out var hit, SensorCtrler.visionLen, LayerMask.GetMask("HitBox")) && hit.collider.TryGetComponent<PawnColliderHelper>(out var colliderHelper))
                return colliderHelper.pawnBrain == __jellyManBB.TargetBrain;
            else
                return true;
        }

    }
}
