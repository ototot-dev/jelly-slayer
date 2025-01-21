using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class JellyQuadWalkBrain : JellyBrain
    {    
        public override void OnWatchSomethingOrDamagedHandler(PawnBrainController otherBrain, float reservedDecisionCoolTime)
        {
            JellyBB.target.targetPawnHP.Value = otherBrain.PawnHP;
            JellyBB.decision.aggressiveLevel.Value = 0f;
            InvalidateDecision(reservedDecisionCoolTime);

            //* 어그로를 다른 Jelly들에게 전파함
            foreach (var b in NpcSpawnManager.Instance.spawnedBrains)
            {
                if (b != this)
                    b.OnWatchSomethingOrDamagedHandler(otherBrain, reservedDecisionCoolTime);
            }
        }

        public override void OnDecisionFinishedHandler() 
        { 
            InvalidateDecision(0.2f); 
        }

        public override void InvalidateDecision(float decisionCoolTime = 0)
        {
            __decisionCoolTime = decisionCoolTime;
            JellyBB.decision.currDecision.Value = Decisions.None;
        }

        public JellyQuadWalkBlackboard JellyQuadWalkBB { get; private set; }
        protected float __decisionCoolTime;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            JellyQuadWalkBB = GetComponent<JellyQuadWalkBlackboard>();
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            SensorCtrler.onListenSomething += (s) =>
            {
                if (JellyBB.IsSpawnFinished && !JellyBB.IsDead && JellyBB.TargetPawn == null && ValidateTargetCollider(s.collider))
                    OnWatchSomethingOrDamagedHandler(s.collider.GetComponent<PawnColliderHelper>().pawnBrain, 0.5f);
            };

            SensorCtrler.onWatchSomething += (s) =>
            {
                if (JellyBB.IsSpawnFinished && !JellyBB.IsDead && JellyBB.TargetPawn == null && ValidateTargetCollider(s))
                    OnWatchSomethingOrDamagedHandler(s.GetComponent<PawnColliderHelper>().pawnBrain, 0.5f);
            };

            SensorCtrler.onTouchSomething += (s) =>
            {
                if (JellyBB.IsSpawnFinished && !JellyBB.IsDead && JellyBB.TargetPawn == null && ValidateTargetCollider(s))
                    OnWatchSomethingOrDamagedHandler(s.GetComponent<PawnColliderHelper>().pawnBrain, 0.1f);
            };

            PawnStatusCtrler.onStatusActive += (status) =>
            {
                if (status == PawnStatus.KnockDown || status == PawnStatus.Groggy)
                    InvalidateDecision(0.2f);
            };

            __pawnActionCtrler.onActionStart += (actionContext, damageContext) =>
            {
                if ((actionContext.actionData?.staminaCost ?? 0) > 0)
                    JellyBB.stat.ReduceStamina(actionContext.actionData.staminaCost);

                //* 리액션 수행 중에 현재 이동 제어를 끔
                if (actionContext.actionName.StartsWith("!") && damageContext.receiverPenalty.Item1 != PawnStatus.None)
                    InvalidateDecision(damageContext.receiverPenalty.Item2);
            };

            onUpdate += () => 
            {   
                if (!JellyBB.IsSpawnFinished)
                    return;

                if (!__pawnActionCtrler.CheckActionRunning())
                {
                    JellyBB.stat.RecoverStamina(Mathf.Max(__pawnActionCtrler.prevActionContext.startTimeStamp, __pawnActionCtrler.prevActionContext.finishTimeStamp), Time.deltaTime);

                    //* 스테미너 회복 후 액션 수행 가능으로 변경
                    if (JellyBB.stat.stamina.Value == JellyBB.stat.maxStamina.Value && PawnStatusCtrler.CheckStatus(Game.PawnStatus.CanNotAction))
                        PawnStatusCtrler.RemoveStatus(Game.PawnStatus.CanNotAction);
                }

                JellyBB.stat.ReduceStance(PawnHP.LastDamageTimeStamp, Time.deltaTime);
                ActionDataSelector.UpdateSelection(Time.deltaTime);
            };
        }

        protected override void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (damageContext.receiverBrain.PawnBB.IsDead)
                return;
            
            if (damageContext.receiverPenalty.Item1 == Game.PawnStatus.None)
            {
                //* receiverPenalty가 없다면 Sender 액션을 'Blocked' 혹은 'GuardParried'등으로 파훼한 것으로 판정하여, Sender 공격에 대한 반동 연출을 한다.
                //* 이 때, "!OnHit" 액션을 Addictive 모드로 실행하여 실제 Action이 실행되지는 않도록 한다.
                __pawnActionCtrler.StartAddictiveAction(damageContext, "!OnHit");
            }
            else
            {
                if (__pawnActionCtrler.CheckActionRunning())
                    __pawnActionCtrler.CancelAction(false);

                switch (damageContext.receiverPenalty.Item1)
                {
                    case Game.PawnStatus.Staggered: __pawnActionCtrler.StartAction(damageContext, "!OnHit", string.Empty); break;
                    case Game.PawnStatus.KnockDown: __pawnActionCtrler.StartAction(damageContext, "!OnKnockDown", string.Empty); break;
                    case Game.PawnStatus.Groggy: __pawnActionCtrler.StartAction(damageContext, "!OnGroggy", string.Empty); break;
                }
            }
            
            // OnPawnDamaged
            GameManager.Instance.PawnDamaged(ref damageContext);
        }

        protected override void DamageSenderHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            if (damageContext.senderBrain.PawnBB.IsDead)
                return;

            if (damageContext.senderPenalty.Item1 != Game.PawnStatus.None && __pawnActionCtrler.CheckActionRunning())
                __pawnActionCtrler.CancelAction(false);

            switch (damageContext.actionResult)
            {
                case ActionResults.Blocked: 
                    __pawnActionCtrler.StartAction(damageContext, "!OnBlocked", string.Empty);
                    break;

                case ActionResults.KickParried:
                case ActionResults.GuardParried:
                    __pawnActionCtrler.StartAction(damageContext, damageContext.senderPenalty.Item1 == Game.PawnStatus.Groggy ? "!OnGroggy" : "!OnParried", string.Empty); 
                    break;
            }
        }

        protected override void OnTickInternal(float interval)
        {
            base.OnTickInternal(interval);

            __decisionCoolTime = Mathf.Max(0, __decisionCoolTime - tickInterval);

            if (JellyBB.IsInCombat)
            {
                if (JellyBB.IsDead)
                {
                    JellyBB.target.targetPawnHP.Value = null;
                    JellyBB.decision.aggressiveLevel.Value = -1f;
                    InvalidateDecision(1f);
                }
                else if (JellyBB.IsDown || JellyBB.IsGroggy)
                {
                    //* Down이나 Groogy 상태라면 Decision 갱신이 안되도록 공회전시킴
                    if (__decisionCoolTime <= 0f)
                        InvalidateDecision(0.2f);
                }
                else if (!ValidateTargetBrain(JellyBB.TargetBrain))
                {
                    var nextTarget = NextTargetBrain();
                    if (nextTarget != null)
                    {
                        OnWatchSomethingOrDamagedHandler(nextTarget.GetComponent<PawnBrainController>(), 0.5f);
                    }
                    else
                    {
                        JellyBB.target.targetPawnHP.Value = null;
                        JellyBB.decision.aggressiveLevel.Value = -1f;
                        InvalidateDecision(0.5f);
                    }
                }
                else if (RefreshAggresiveLevel())
                {
                    InvalidateDecision(0.1f);
                }
                else
                {
                    if (JellyBB.CurrDecision == Decisions.Spacing || JellyBB.CurrDecision == Decisions.Approach)
                    {
                        var targetCapsuleRadius = JellyBB.TargetBrain.coreColliderHelper.GetCapsuleCollider() != null ? JellyBB.TargetBrain.coreColliderHelper.GetCapsuleCollider().radius : 0f;
                        var distanceVec = (JellyBB.TargetBrain.coreColliderHelper.transform.position - coreColliderHelper.transform.position).Vector2D();
                        var distance = Mathf.Max(0f, distanceVec.magnitude - targetCapsuleRadius);
                        
                        if (JellyBB.CurrDecision == Decisions.Spacing && distance > JellyQuadWalkBB.SpacingOutDistance)
                            InvalidateDecision(1f);
                        else if (JellyBB.CurrDecision == Decisions.Approach && distance <= JellyQuadWalkBB.SpacingInDistance)
                            InvalidateDecision(1f);
                    }
                    else
                    {
                        //* 액션 수행 중에는 Decision 갱신은 하지 않음
                        if (__decisionCoolTime <= 0f && !__pawnActionCtrler.CheckActionRunning())
                            JellyBB.decision.currDecision.Value = MakeDecision();
                    }
                }
            }
            else if (JellyBB.CurrDecision == Decisions.None)
            {
                if (__decisionCoolTime <= 0f)
                    JellyBB.decision.currDecision.Value = Decisions.Idle;
            }
        }

        protected virtual bool RefreshAggresiveLevel()
        {
            if (JellyBB.decision.aggressiveLevel.Value == 0f && JellyBB.stat.stamina.Value == JellyBB.stat.maxStamina.Value)
            {
                JellyBB.decision.aggressiveLevel.Value = 1f;
                return true;
            }
            else if (JellyBB.decision.aggressiveLevel.Value == 1f && JellyBB.stat.stamina.Value == 0f)
            {
                JellyBB.decision.aggressiveLevel.Value = 0f;
                return true;
            }
            else
            {
                return false;
            }
        }

        protected virtual Decisions MakeDecision()
        {
            Debug.Assert(JellyBB.TargetPawn != null);
            return JellyBB.TargetBrain.coreColliderHelper.GetApproachDistance(coreColliderHelper.transform.position) < JellyQuadWalkBB.SpacingInDistance ? Decisions.Spacing : Decisions.Approach;
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
            Debug.Assert(JellyBB.TargetPawn != null);

            if (!SensorCtrler.WatchingColliders.Contains(JellyBB.TargetBrain.coreColliderHelper.pawnCollider))
                return false;

            var origin = __pawnMovement.capsule.TransformPoint(__pawnMovement.capsuleCollider.center);
            var direction = (JellyBB.TargetCore.transform.position - coreColliderHelper.transform.position).Vector2D().normalized;
            if (Physics.SphereCast(origin, __pawnMovement.capsuleCollider.radius, direction, out var hit, SensorCtrler.visionLen, LayerMask.GetMask("HitBox")) && hit.collider.TryGetComponent<PawnColliderHelper>(out var colliderHelper))
                return colliderHelper.pawnBrain == JellyBB.TargetBrain;
            else
                return true;
        }
    }
}
