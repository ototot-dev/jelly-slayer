using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class FootmanBrain : PawnBrainController, IPawnSpawnable, IPawnMovable
    {
        [Header("Component")]
        public Transform headBone;
        public Transform eyeTarget;
        public Transform leftClavicle;
        public Transform rightClavicle;
        public Animator bodyAnimator;
        public FIMSpace.FProceduralAnimation.LegsAnimator legAnimator;
        public FIMSpace.FEyes.FEyesAnimator eyeAnimator;
        public JellySpringMassSystem springMassSystem;

        public float clavicleOffset = 0;

        Vector3 IPawnSpawnable.GetSpawnPosition() => Vector3.zero;
        public virtual void OnStartSpawnHandler() { }
        void IPawnSpawnable.OnFinishSpawnHandler() { }
        void IPawnSpawnable.OnDespawnedHandler() { }
        void IPawnSpawnable.OnDeadHandler() { bodyAnimator.SetTrigger("OnDead"); Movement.SetMovementEnabled(false); }
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

        public class ActionData
        {
            public string actionId;         // 유니크한 액션 ID
            public string name;             // 액션 이름
            public int damage;              // 데미지 수치
            public float range;             // 공격 범위 (미터)
            public float animSpeed;         // 애니메이션 속도
            public float staggerDuration;   // 경직 지속 시간 (초)
            public float staminaCost;        // 스태미나 소모량

            public ActionData(string actionId, string name, int damage, float range, float animSpeed, float staggerDuration, float staminaCost)
            {
                this.actionId = actionId;
                this.name = name;
                this.damage = damage;
                this.range = range;
                this.animSpeed = animSpeed;
                this.staggerDuration = staggerDuration;
                this.staminaCost = staminaCost;
            }
        }

        public readonly Dictionary<string, ActionData> actionDataTable = new();

        public FootmanBlackboard BB { get; private set; }
        public PawnMovementEx Movement { get; private set; }
        public PawnStatusController BuffCtrler { get; private set; }
        public PawnActionController ActionCtrler { get; private set; }
        public PawnSensorController SensorCtrler { get; private set; }

        void LateUpdate()
        {
            // leftClavicle.localPosition += clavicleOffset * Vector3.right;
            // rightClavicle.localPosition -= clavicleOffset * Vector3.right;
        }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BB = GetComponent<FootmanBlackboard>();
            Movement = GetComponent<PawnMovementEx>();
            BuffCtrler = GetComponent<PawnStatusController>();
            ActionCtrler = GetComponent<PawnActionController>();
            SensorCtrler = GetComponent<PawnSensorController>();

            PawnHP.heartPoint.Value = BB.stat.maxHeartPoint.Value;
            Movement.moveSpeed = BB.body.moveSpeed;
            Movement.moveBrake = BB.body.moveBrake;
            Movement.rotateSpeed = BB.body.rotateSpeed;
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            actionDataTable.Add("Combo#1", new ActionData("Combo#1", "Combo#1", 1, 2, 1, 0.2f, 0));
            actionDataTable.Add("Combo#2", new ActionData("Combo#2", "Combo#1", 1, 2, 1, 0.2f, 0));
            actionDataTable.Add("Combo#3", new ActionData("Combo#3", "Combo#1", 1, 2, 1, 0.2f, 0));
            actionDataTable.Add("Combo#4", new ActionData("Combo#4", "Combo#1", 1, 2, 1, 0.2f, 0));
            actionDataTable.Add("Dodge", new ActionData("Dodge", "Dodge", 1, 4, 1, 0.2f, 0));
            actionDataTable.Add("Heavy", new ActionData("Heavy", "Heavy", 1, 3, 1, 0.2f, 0));

            onUpdate += () =>
            {
                if (ActionCtrler.CheckActionRunning())
                    Movement.AddRootMotion(SensorCtrler.TouchingColliders.Count > 0 ? Vector3.zero : bodyAnimator.deltaPosition, bodyAnimator.deltaRotation);
                bodyAnimator.transform.SetPositionAndRotation(coreColliderHelper.transform.position, coreColliderHelper.transform.rotation);
                bodyAnimator.SetLayerWeight(1, Mathf.Clamp01(bodyAnimator.GetLayerWeight(1) + (ActionCtrler.CheckActionRunning() ? 10 : -1) * Time.deltaTime));
                bodyAnimator.SetBool("IsMoving", Movement.moveVec != Vector3.zero);
                bodyAnimator.SetFloat("MoveSpeed", Movement.CurrVelocity.magnitude / 4);
                legAnimator.User_SetIsMoving(Movement.moveVec != Vector3.zero);
                legAnimator.User_SetIsGrounded(Movement.IsOnGround);
                // characterAnimator.SetLayerWeight(1, Mathf.Clamp01(characterAnimator.GetLayerWeight(1) + (ActionCtrler.CheckActionRunning() ? 5 : -5) * Time.deltaTime));

                if (BB.TargetBrain != null)
                    eyeTarget.position = BB.TargetBrain.coreColliderHelper.transform.position + Vector3.up;
                else
                    eyeTarget.position = coreColliderHelper.transform.position + coreColliderHelper.transform.forward + Vector3.up;
            };

            onFixedUpdate += () =>
            {
                springMassSystem.core.SetPositionAndRotation(headBone.position, headBone.rotation);
            };

            PawnHP.onDamaged += (damageContext) =>
            {
                if (!BB.IsSpawnFinished || BB.IsDead || damageContext.receiverBrain != this)
                    return;

                if (damageContext.receiverBrain == this)
                {
                    if (ActionCtrler.CheckActionRunning())
                        ActionCtrler.CancelAction(false);

                    // ActionCtrler.StartAction("!OnHit", 1);
                    // bodyAnimator.SetTrigger("OnHit");
                    // bodyAnimator.SetFloat("HitX", 0);
                    // bodyAnimator.SetFloat("HitY", 1);

                    var pushBackVec = damageContext.senderBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                    Observable.EveryFixedUpdate()
                        .TakeUntil(Observable.Timer(TimeSpan.FromSeconds(0.2f)))
                        .Subscribe(_ => Movement.AddRootMotion(4 * Time.fixedDeltaTime * pushBackVec, Quaternion.identity)).AddTo(this);

                    // Hit Effect
                    EffectCenter.CreateEffect(30, damageContext.hitPoint);

                    // OnPawnDamaged
                    GameManager.Instance.PawnDamaged(ref damageContext);
                }
            };

            ActionCtrler.onActionStart += (actionContext, _) =>
            {
                if (BB.TargetBrain != null)
                    Movement.FaceAt(BB.TargetBrain.coreColliderHelper.transform.position);
            };

            ActionCtrler.onActionCanceled += (_, _) => bodyAnimator.speed = 1;
            ActionCtrler.onActionFinished += (_) => bodyAnimator.speed = 1;

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
                else
                {
                    var distanceVec = (BB.TargetBrain.coreColliderHelper.transform.position - coreColliderHelper.transform.position).Vector2D();

                    if (BB.CurrDecision == Decisions.Spacing || BB.CurrDecision == Decisions.Approach)
                    {
                        if (BB.CurrDecision == Decisions.Spacing && distanceVec.magnitude > BB.decision.spacingOutDistance)
                            InvalidateDecision(0.1f);
                        else if (BB.CurrDecision == Decisions.Approach && distanceVec.magnitude <= BB.decision.spacingInDistance)
                            InvalidateDecision(0);

                        // if (!BuffCtrler.CheckBuff(PawnBuffController.Buffs.Staggered) && (!ActionCtrler.CheckActionRunning() || ActionCtrler.CanInterruptAction()))
                        // {
                        //     if (ActionCtrler.currActionContext.actionName == "Combo#1" || (ActionCtrler.prevActionContext.actionName == "Combo#1" && Time.time - ActionCtrler.prevActionContext.finishTimeStamp < 0.2f))
                        //     {
                        //         ActionCtrler.CancelAction(false);
                        //         ActionCtrler.pendingAction = new("Combo#2", Time.time);
                        //     }
                        //     else
                        //     {
                        //         var distance = distanceVec.magnitude;
                        //         if (ActionCtrler.currActionContext.actionName == "Combo#2" || (ActionCtrler.prevActionContext.actionName == "Combo#2" && Time.time - ActionCtrler.prevActionContext.finishTimeStamp < 0.2f))
                        //         {
                        //             ActionCtrler.CancelAction(false);
                        //             ActionCtrler.pendingAction = new("Sprint", Time.time);
                        //         }
                        //         else if (SensorCtrler.WatchingColliders.Contains(BB.TargetBrain.core.pawnCollider))
                        //         {    
                        //             if (distance <= 3)
                        //             {
                        //                 ActionCtrler.pendingAction = new("Combo#1", Time.time);
                        //                 Movement.FaceTo(BB.TargetBrain.core.transform.position);
                        //             }
                        //             else if (distance > 4 && distance <= 6 && UnityEngine.Random.Range(0, 2) == 0)
                        //             {
                        //                 ActionCtrler.pendingAction = new("Dodge", Time.time);
                        //                 Movement.FaceTo(BB.TargetBrain.core.transform.position);
                        //             }
                        //         }
                        //     }
                        // }
                    }
                    else if (BB.CurrDecision == Decisions.None)
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

            var distance = (BB.TargetBrain.coreColliderHelper.transform.position - coreColliderHelper.transform.position).Vector2D().magnitude;
            return distance > BB.decision.spacingOutDistance ? Decisions.Approach : Decisions.Spacing;
        }

        float __decisionCoolTime;

        void InvalidateDecision(float decisionCoolTime = 0)
        {
            __decisionCoolTime = decisionCoolTime;
            BB.decision.currDecision.Value = Decisions.None;
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
