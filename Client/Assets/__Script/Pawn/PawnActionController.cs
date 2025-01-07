using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class PawnActionController : MonoBehaviour, IStatusContainer
    {   
        public struct ActionContext
        {
            public MainTable.ActionData actionData;
            public int actionInstanceId;
            public string preMotionName;
            public string actionName;
            public float actionSpeed;
            public float rootMotionMultiplier;
            public int rootMotionConstraint;
            public AnimationCurve rootMotionCurve;
            public bool insufficientStamina;
            public bool actionCanceled;
            public bool movementEnabled;
            public bool rootMotionEnabled;
            public bool legAnimGlueEnabled;
            public bool interruptEnabled;
            public bool activeParryEnabled;
            public bool isTraceRunning;
            public bool manualAdvanceEnabled;
            public float manualAdvanceTime;
            public float manualAdvanceSpeed;
            public float animClipLength;
            public int animClipFps;
            public float startTimeStamp;
            public float preMotionTimeStamp;
            public float finishTimeStamp;
            public float waitTimeStamp;
            public IDisposable impulseRootMotionDisposable;
            public IDisposable homingRotationDisposable;
            public IDisposable actionDisposable;
            static int __actionInstanceIdCounter;

            //* 일반적인 Action 초기화
            public ActionContext(MainTable.ActionData actionData, string preMotionName, float actionSpeedMultiplier, float rootMotionMultiplier, int rootMotionConstraint, AnimationCurve rootMotionCurve, bool manualAdvacneEnabled, float startTimeStamp)
            {
                Debug.Assert(actionData != null);
                this.actionData = actionData;
                actionInstanceId = ++__actionInstanceIdCounter;
                this.preMotionName = preMotionName;
                actionName = actionData.actionName;
                actionSpeed = actionData.actionSpeed * actionSpeedMultiplier;
                this.rootMotionMultiplier = rootMotionMultiplier;
                this.rootMotionConstraint = rootMotionConstraint;
                this.rootMotionCurve = rootMotionCurve;
                insufficientStamina = false;
                actionCanceled = false;
                movementEnabled = false;
                rootMotionEnabled = true;
                legAnimGlueEnabled = true;
                interruptEnabled = false;
                activeParryEnabled = false;
                isTraceRunning = false;
                this.manualAdvanceEnabled = manualAdvacneEnabled;
                manualAdvanceTime = 0f;
                manualAdvanceSpeed = actionSpeed;
                animClipLength = -1f;
                animClipFps = -1;
                this.startTimeStamp = startTimeStamp;
                preMotionTimeStamp = 0f;
                finishTimeStamp = 0f;
                waitTimeStamp = 0f;
                impulseRootMotionDisposable = null;
                homingRotationDisposable = null;
                actionDisposable = null;
            }

            //* 리액션 및 Addictivie 액션 초기화
            public ActionContext(string actionName, float actionSpeedMultiplier, float startTimeStamp)
            {
                actionData = null;
                actionInstanceId = ++__actionInstanceIdCounter;
                preMotionName = string.Empty;
                this.actionName = actionName;
                actionSpeed = actionSpeedMultiplier;
                rootMotionConstraint = 0;
                rootMotionMultiplier = 1;
                rootMotionCurve = null;
                insufficientStamina = false;
                actionCanceled = false;
                movementEnabled = false;
                rootMotionEnabled = true;
                legAnimGlueEnabled = true;
                interruptEnabled = false;
                activeParryEnabled = false;
                isTraceRunning = false;
                manualAdvanceEnabled = false;
                manualAdvanceTime = 0f;
                manualAdvanceSpeed = actionSpeed;
                animClipLength = -1f;
                animClipFps = -1;
                this.startTimeStamp = startTimeStamp;
                preMotionTimeStamp = 0f;
                finishTimeStamp = 0f;
                waitTimeStamp = 0f;
                impulseRootMotionDisposable = null;
                homingRotationDisposable = null;
                actionDisposable = null;
            }
        }

        public ActionContext prevActionContext = new(string.Empty, 1f, 0f);
        public ActionContext currActionContext = new(string.Empty, 1f, 0f);
        public bool CheckActionPending() => !string.IsNullOrEmpty(PendingActionData.Item1);
        public bool CheckActionRunning() => currActionContext.actionData != null || currActionContext.actionDisposable != null;
        public bool CheckActionCanceled() { Debug.Assert(CheckActionRunning()); return currActionContext.actionCanceled; }
        public bool CheckMovementEnabled() { Debug.Assert(CheckActionRunning()); return currActionContext.movementEnabled; }
        public bool CanInterruptAction() { Debug.Assert(CheckActionRunning()); return currActionContext.interruptEnabled; }
        public SuperArmorLevels GetSuperArmorLevel() => (SuperArmorLevels)(currActionContext.actionData?.superArmorLevel?? 0);

        public bool CheckSuperArmorLevel(SuperArmorLevels compareLevel) { return (currActionContext.actionData?.superArmorLevel?? 0) >= (int)compareLevel; }
        public bool CheckPendingActionHasPreMotion() => !string.IsNullOrEmpty(PendingActionData.Item2);
        public float LastActionTimeStamp => CheckActionRunning() ? Time.time : prevActionContext.finishTimeStamp;
        public string PreMotionName => currActionContext.preMotionName;
        public string CurrActionName => currActionContext.actionName;
        public string PrevActionName => prevActionContext.actionName;
        public string PendingActionName => PendingActionData.Item1;
        public string PendingPreMotionName => PendingActionData.Item2;

        //* Item1: actionName, Item2: preMotionName, Item3: pendingTimeStamp
        public  Tuple<string, string, float> PendingActionData { get; protected set; } = new(string.Empty, string.Empty, 0);

        //* Item1: Strength, Item2: Duration
        protected Dictionary<PawnStatus, Tuple<float, float>> __statusContainer = new();

        public Action<ActionContext, PawnHeartPointDispatcher.DamageContext> onActionStart;
        public Action<ActionContext, PawnHeartPointDispatcher.DamageContext> onAddictiveActionStart;
        public Action<ActionContext> onActiveParryEnabled;
        public Action<ActionContext> onActionFinished;
        public Action<ActionContext, float> onActionCanceled;

        //* 프로젝타일 방출
        public Action<ActionContext, ProjectileMovement, Transform, int> onEmitProjectile;

#if UNITY_EDITOR
        Vector3 __actionStartPosition;
        Vector3 __actionFinishPosition;
#endif

#region IBuffContainer
        Dictionary<PawnStatus, Tuple<float, float>> IStatusContainer.GetStatusTable() => __statusContainer;

        bool IStatusContainer.AddStatus(PawnStatus buff, float strength, float duration)
        {
            Debug.Assert(__pawnBrain.PawnStatusCtrler != null && CheckActionRunning());

            __pawnBrain.PawnStatusCtrler.AddExternStatus(this, buff, strength, duration);
            return true;
        }

        void IStatusContainer.RemoveStatus(PawnStatus buff)
        {
            Debug.Assert(__pawnBrain.PawnStatusCtrler != null && CheckActionRunning());
            __pawnBrain.PawnStatusCtrler.RemoveExternStatus(this, buff);
        }

        bool IStatusContainer.CheckStatus(PawnStatus buff)
        {
            Debug.Assert(__pawnBrain.PawnStatusCtrler != null && CheckActionRunning());
            return __pawnBrain.PawnStatusCtrler.CheckStatus(buff);
        }

        float IStatusContainer.GetStatusStrength(PawnStatus buff)
        {
            Debug.Assert(__pawnBrain.PawnStatusCtrler != null && CheckActionRunning());
            return __pawnBrain.PawnStatusCtrler.GetStrength(buff);
        }

#if UNITY_EDITOR
        Dictionary<PawnStatus, Tuple<float, float>>.Enumerator IStatusContainer.GetStatusEnumerator() => __statusContainer.GetEnumerator();
#endif
#endregion

        public virtual float GetRootMotionMultiplier()
        {
            if (currActionContext.rootMotionCurve == null)
                return currActionContext.rootMotionMultiplier;
            else if (currActionContext.manualAdvanceEnabled)
                return currActionContext.rootMotionMultiplier * currActionContext.rootMotionCurve.Evaluate(currActionContext.manualAdvanceTime);
            else
                return currActionContext.rootMotionMultiplier * currActionContext.rootMotionCurve.Evaluate(Time.time - (currActionContext.preMotionTimeStamp > 0f ? currActionContext.preMotionTimeStamp : currActionContext.startTimeStamp));
        }

        public virtual bool CheckRootMotionConstraint(params RootMotionConstraints[] constraints) 
        {
            foreach (var c in constraints)
            {
                if ((currActionContext.rootMotionConstraint & (int)c) > 0)
                    return true;
            }

            return false;
        }

        public virtual bool CheckKnockBackRunning() { return false; }
        public virtual bool CanRootMotion(Vector3 rootMotionVec) { return currActionContext.rootMotionEnabled; }
        public virtual bool CanBlockAction(ref PawnHeartPointDispatcher.DamageContext damageContext) { return false; }
        public virtual bool CanParryAction(ref PawnHeartPointDispatcher.DamageContext damageContext) { return false; }
        public virtual IDisposable StartOnHitAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false) { return null; }
        public virtual IDisposable StartOnBlockedAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false) { return null; }
        public virtual IDisposable StartOnParriedAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false) { return null; }
        public virtual IDisposable StartOnKnockDownAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false) { return null; }
        public virtual IDisposable StartOnGroogyAction(ref PawnHeartPointDispatcher.DamageContext damageContext, bool isAddictiveAction = false) { return null; }
        public virtual IDisposable StartActionDisposable(ref PawnHeartPointDispatcher.DamageContext damageContext, string actionName) { return null;}

        public void SetPendingAction(string actionName)
        {
            SetPendingAction(actionName, string.Empty);
        }

        public void SetPendingAction(string actionName, string preMotionName)
        {
            if (!string.IsNullOrEmpty(PendingActionData.Item1))
                __Logger.WarningF(gameObject, nameof(PendingActionData), "__pendingAction is not empty value!!", "actionName", PendingActionData.Item1);

            PendingActionData = new(actionName, preMotionName, Time.time);
        }

        public void ClearPendingAction()
        {
            PendingActionData = new(string.Empty, string.Empty, 0);
        }

        void Awake()
        {
            AwakeInternal();
        }

        protected virtual void AwakeInternal()
        {
            __pawnBrain = GetComponent<PawnBrainController>();
            __pawnMovement = GetComponent<PawnMovement>();
            __pawnAnimCtrler = GetComponent<PawnAnimController>();
            __pawnActionSelector = GetComponent<PawnActionDataSelector>();
        }

        protected PawnBrainController __pawnBrain;
        protected PawnMovement __pawnMovement;
        protected PawnAnimController __pawnAnimCtrler;
        protected PawnActionDataSelector __pawnActionSelector;

        void Start()
        {
            StartInternal();
        }

        protected virtual void StartInternal() 
        {
            __pawnBrain.PawnStatusCtrler.RegisterExternContainer(this);
            __pawnBrain.onUpdate += () =>
            {
                if (CheckActionRunning() && currActionContext.manualAdvanceEnabled && __pawnAnimCtrler != null && __pawnAnimCtrler.mainAnimator != null)
                {
                    if (currActionContext.manualAdvanceSpeed > 0)
                        currActionContext.manualAdvanceTime += currActionContext.manualAdvanceSpeed * Time.deltaTime;
                    if (currActionContext.animClipLength > 0)
                        __pawnAnimCtrler.mainAnimator.SetFloat("AnimAdvance", currActionContext.manualAdvanceTime / currActionContext.animClipLength);
                }
            };

            __pawnBrain.onLateUpdate += () =>
            {
                if (__traceCollider == null)
                    return;
                if (__traceCount > 0 && (Time.time - __prevTraceTimeStamp) < __traceSampleInterval)
                    return;

                var hitCount = 0;

                if (__traceBoxCollider != null)
                {
                    var halfExtent = 0.5f * __traceBoxCollider.size;
                    var interpRotation = Quaternion.Lerp(__prevTraceRotation, __traceCollider.transform.rotation, 0.5f);
                    var currPosition = __traceBoxCollider.transform.localToWorldMatrix.MultiplyPoint(__traceSphereCollider.center);
                    var deltaVec = currPosition - __prevTracePosition;

                    if (__traceCount == 0)
                        hitCount = Physics.BoxCastNonAlloc(__prevTracePosition, halfExtent, __traceBoxCollider.transform.right, __traceTempHits, __traceBoxCollider.transform.rotation, 0.1f, __traceLayerMask);
                    else
                        hitCount = Physics.BoxCastNonAlloc(__prevTracePosition, halfExtent, deltaVec.normalized, __traceTempHits, interpRotation, deltaVec.magnitude, __traceLayerMask);

                    if (__traceDrawGizmosEnabled)
                    {
                            var drawRotation = interpRotation;
                            var drawPosition0 = currPosition;
                            var drawPosition1 = __prevTracePosition;
                            var drawHalfExtent = 0.5f * __traceBoxCollider.size;

                            GizmosDrawer.Instance.Draw(__traceDrawGizmosDuration, () =>
                            {
                                Gizmos.color = Color.yellow;
                                GizmosDrawExtension.DrawBox(drawPosition0, drawRotation, drawHalfExtent);
                                GizmosDrawExtension.DrawBox(drawPosition1, drawRotation, drawHalfExtent);
                                GizmosDrawExtension.DrawBox(0.5f * (drawPosition0 + drawPosition1), drawRotation, drawHalfExtent);
                            });
                    }

                    __prevTracePosition = currPosition;
                    __prevTraceRotation = __traceBoxCollider.transform.rotation;
                }
                else if (__traceSphereCollider != null)
                {
                    var currPosition = __traceSphereCollider.transform.localToWorldMatrix.MultiplyPoint(__traceSphereCollider.center);
                    var deltaVec = currPosition - __prevTracePosition;

                    if (__traceCount == 0)
                        hitCount = Physics.SphereCastNonAlloc(__prevTracePosition, __traceSphereCollider.radius, __traceSphereCollider.transform.forward, __traceTempHits, 0.1f, __traceLayerMask);
                    else
                        hitCount = Physics.SphereCastNonAlloc(__prevTracePosition, __traceSphereCollider.radius, deltaVec.normalized, __traceTempHits, deltaVec.magnitude, __traceLayerMask);

                    if (__traceDrawGizmosEnabled)
                    {
                            var drawPosition0 = currPosition;
                            var drawPosition1 = __prevTracePosition;

                            GizmosDrawer.Instance.Draw(__traceDrawGizmosDuration, () =>
                            {
                                Gizmos.color = Color.yellow;
                                Gizmos.DrawSphere(currPosition, __traceSphereCollider.radius);
                                Gizmos.DrawSphere(__prevTracePosition, __traceSphereCollider.radius);
                                Gizmos.DrawSphere(0.5f * (currPosition + __prevTracePosition), __traceSphereCollider.radius);
                            });
                    }

                    __prevTracePosition = currPosition;
                    __prevTraceRotation = __traceSphereCollider.transform.rotation;
                }
                else if (__traceCapsuleCollider != null)
                {
                    var currPosition = __traceCapsuleCollider.transform.localToWorldMatrix.MultiplyPoint(__traceCapsuleCollider.center);
                    var heightWithoutCap = Mathf.Max(0f, __traceCapsuleCollider.height - __traceCapsuleCollider.radius * 2f);
                    var point1 = currPosition + heightWithoutCap * 0.5f * __traceCapsuleCollider.transform.up;
                    var point2 = currPosition - heightWithoutCap * 0.5f * __traceCapsuleCollider.transform.up;
                    var deltaVec = currPosition - __prevTracePosition;

                    if (__traceCount == 0)
                        hitCount = Physics.CapsuleCastNonAlloc(point1, point2, __traceCapsuleCollider.radius, __traceCapsuleCollider.transform.up, __traceTempHits, 0.1f, __traceLayerMask);
                    else
                        hitCount = Physics.CapsuleCastNonAlloc(point1, point2, __traceCapsuleCollider.radius, deltaVec.normalized, __traceTempHits, deltaVec.magnitude, __traceLayerMask);

                    if (__traceDrawGizmosEnabled)
                    {
                        var drawPosition0 = point1;
                        var drawPosition1 = point2;
                        var drawRadius = __traceCapsuleCollider.radius;

                        GizmosDrawer.Instance.Draw(__traceDrawGizmosDuration, () =>
                        {
                            Gizmos.color = Color.yellow;
                            Gizmos.DrawWireSphere(drawPosition0, drawRadius);
                            Gizmos.DrawWireSphere(drawPosition1, drawRadius);
                            Gizmos.DrawLine(drawPosition0, drawPosition1);
                        });
                    }

                    __prevTracePosition = currPosition;
                    __prevTraceRotation = __traceCapsuleCollider.transform.rotation;
                }

                __prevTraceTimeStamp = Time.time;
                __traceCount++;

                for (int i = 0; i < hitCount; i++)
                {
                    if (!__traceTempHits[i].collider.TryGetComponent<PawnColliderHelper>(out var hitColliderHelper) || hitColliderHelper.pawnBrain == null || hitColliderHelper.pawnBrain == __pawnBrain)
                        continue;
                    if (__tracePawnNames.Count > 0 && !__tracePawnNames.Contains(hitColliderHelper.pawnBrain.PawnBB.common.pawnName))
                        continue;

                    if (__multiHitEnabled || !__tracedPawnBrains.Contains(hitColliderHelper.pawnBrain))
                    {
                        if (!__multiHitEnabled)
                            __tracedPawnBrains.Add(hitColliderHelper.pawnBrain);

                        if (__sendDamageOnTrace)
                        {
                            hitColliderHelper.pawnBrain.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(__pawnBrain, hitColliderHelper.pawnBrain, currActionContext.actionData, hitColliderHelper.pawnCollider, currActionContext.insufficientStamina));

                            //* Debuff 할당
                            if (__debuffParams != null && __debuffParams.Length > 0)
                            {
                                foreach (var p in __debuffParams)
                                {
                                    if (p.isExtern && hitColliderHelper.pawnBrain.TryGetComponent<PawnActionController>(out var receiverActionCtrler))
                                        hitColliderHelper.pawnBrain.PawnStatusCtrler.AddExternStatus(receiverActionCtrler, p);
                                    else
                                        hitColliderHelper.pawnBrain.PawnStatusCtrler.AddStatus(p);
                                }
                            }
                        }
                    }
                }

                Array.Clear(__traceTempHits, 0, __traceTempHits.Length);
            };
        }

        public bool StartAction(string actionName, string preMotionName, float actionSpeedMultiplier = 1, float rootMotionMultiplier = 1, int rootMotionConstraint = 0, AnimationCurve rootMotionCurve = null, bool manualAdvacneEnabled = false)
        {
            return StartAction(new PawnHeartPointDispatcher.DamageContext(), actionName, preMotionName, actionSpeedMultiplier, rootMotionMultiplier, rootMotionConstraint, rootMotionCurve, manualAdvacneEnabled);
        }

        public bool StartAction(PawnHeartPointDispatcher.DamageContext damageContext, string actionName, string preMotionName, float actionSpeedMultiplier = 1, float rootMotionMultiplier = 1, int rootMotionConstraint = 0, AnimationCurve rootMotionCurve = null, bool manualAdvacneEnabled = false)
        {
            if (CheckActionRunning())
            {
                __Logger.LogR(gameObject, "CheckActionRunning() returns false", nameof(actionName), actionName, nameof(CurrActionName), CurrActionName);
                return false;
            }

            if (actionName.StartsWith('!')) //* '!'로 시작하는 이름은 리액션
            {
                currActionContext = new(actionName, actionSpeedMultiplier, Time.time);
                onActionStart?.Invoke(currActionContext, damageContext);

                __Logger.LogR(gameObject, "onActionStart", "actionName", currActionContext.actionName, "actionInstanceId", currActionContext.actionInstanceId);

                if (__pawnAnimCtrler != null && __pawnAnimCtrler.mainAnimator != null)
                {
                    __pawnAnimCtrler.mainAnimator.SetFloat("AnimSpeed", actionSpeedMultiplier);
                    __pawnAnimCtrler.mainAnimator.SetFloat("AnimAdvance", 0);
                }

                switch (actionName)
                {
                    case "!OnHit": currActionContext.actionDisposable = StartOnHitAction(ref damageContext); break;
                    case "!OnGroggy": currActionContext.actionDisposable = StartOnGroogyAction(ref damageContext); break;
                    case "!OnKnockDown": currActionContext.actionDisposable = StartOnKnockDownAction(ref damageContext); break;
                    case "!OnBlocked": currActionContext.actionDisposable = StartOnBlockedAction(ref damageContext); break;
                    case "!OnParried": currActionContext.actionDisposable = StartOnParriedAction(ref damageContext); break;
                }

                //* 수행할 액션이 없다면 다음 프레임에 종료시켜준다.
                if (currActionContext.actionDisposable == null)
                    Observable.NextFrame().Subscribe(_ => FinishAction()).AddTo(this);
            }
            else
            {
                var actionData = DatasheetManager.Instance.GetActionData(__pawnBrain.PawnBB.common.pawnId, actionName);
                if (actionData == null)
                {
                    __Logger.LogR(gameObject, "GetActionData() returns null", nameof(actionName), actionName, nameof(__pawnBrain.PawnBB.common.pawnId), __pawnBrain.PawnBB.common.pawnId);
                    return false;
                }

                currActionContext = new(actionData, preMotionName, actionSpeedMultiplier, rootMotionMultiplier, rootMotionConstraint, rootMotionCurve, manualAdvacneEnabled, Time.time);
                if (actionData.staminaCost > 0)
                {
                    if (__pawnBrain.PawnBB.stat.stamina.Value < actionData.staminaCost)
                    {
                        __Logger.LogR(gameObject, "Stamina is too low.", "stat.stamina", __pawnBrain.PawnBB.stat.stamina.Value, "staminaCost", actionData.staminaCost);
                        currActionContext.insufficientStamina = true;
                    }
                    else
                    {
                        __pawnBrain.PawnBB.stat.ReduceStamina(actionData.staminaCost);
                    }
                }

                onActionStart?.Invoke(currActionContext, damageContext);

                __Logger.LogR(gameObject, "onActionStart", nameof(currActionContext.actionName), currActionContext.actionName, "actionInstanceId", currActionContext.actionInstanceId);

                if (__pawnAnimCtrler != null && __pawnAnimCtrler.mainAnimator != null)
                {
                    __pawnAnimCtrler.mainAnimator.SetFloat("AnimSpeed", actionData.actionSpeed * actionSpeedMultiplier);
                    __pawnAnimCtrler.mainAnimator.SetFloat("AnimAdvance", 0);
                }

                currActionContext.actionDisposable = StartActionDisposable(ref damageContext, actionName);
            }

#if UNITY_EDITOR
            __actionStartPosition = __pawnBrain.coreColliderHelper.transform.position;
#endif

            return true;
        }

        public bool StartAddictiveAction(PawnHeartPointDispatcher.DamageContext damageContext, string actionName, float actionSpeedMultiplier = 1, float rootMotionMultiplier = 1)
        {
            if (!actionName.StartsWith('!'))
            {
                __Logger.WarningF(gameObject, nameof(StartAddictiveAction), "actionName.StartsWith('!') is false.", "actionName", actionName);
                return false;
            }

            __Logger.LogF(gameObject, nameof(StartAddictiveAction), "onAddictiveActionStart is invoked.", "actionName", actionName);
            onAddictiveActionStart?.Invoke(new ActionContext(actionName, 1f, Time.time), damageContext);

            if (__pawnAnimCtrler != null && __pawnAnimCtrler.mainAnimator != null)
            {
                __pawnAnimCtrler.mainAnimator.SetFloat("AnimSpeed", actionSpeedMultiplier);
                __pawnAnimCtrler.mainAnimator.SetFloat("AnimAdvance", 0);
            }

            switch (actionName)
            {
                case "!OnHit": StartOnHitAction(ref damageContext, true); break;
                case "!OnGroggy": StartOnGroogyAction(ref damageContext, true); break;
                case "!OnKnockDown": StartOnKnockDownAction(ref damageContext, true); break;
                case "!OnBlocked": StartOnBlockedAction(ref damageContext, true); break;
                case "!OnParried": StartOnParriedAction(ref damageContext, true); break;
            }

            return true;
        }

        public void WaitAction()
        {
            currActionContext.waitTimeStamp = Time.time;
        }

        public bool CheckWaitAction(float duration, float baseTimeStamp = -1)
        {
            return Time.time - (baseTimeStamp > 0 ? baseTimeStamp : currActionContext.waitTimeStamp) < duration;
        }

        public void CancelAction(bool rewindAction, float rewindSpeed = 1, float rewindDuration = 1)
        {
            __Logger.LogR(gameObject, nameof(CancelAction), "actionName", currActionContext.actionName);

            currActionContext.actionCanceled = true;
            currActionContext.actionDisposable?.Dispose();
            currActionContext.homingRotationDisposable?.Dispose();
            currActionContext.impulseRootMotionDisposable?.Dispose();
            currActionContext.actionDisposable = null;
            currActionContext.homingRotationDisposable = null;
            currActionContext.impulseRootMotionDisposable = null;
            onActionCanceled?.Invoke(currActionContext, rewindAction ? rewindSpeed : 1);

            if (rewindAction)
            {
                if (rewindSpeed > 0 && __pawnAnimCtrler != null && __pawnAnimCtrler.mainAnimator != null)
                    __pawnAnimCtrler.mainAnimator.SetFloat("AnimSpeed", -rewindSpeed);
                if (rewindDuration > 0)
                    Observable.Timer(TimeSpan.FromSeconds(rewindDuration)).Subscribe(_ => FinishAction()).AddTo(this);
            }
            else
            {
                FinishAction();
            }
        }

        public void FinishAction()
        {
            __Logger.LogR(gameObject, nameof(FinishAction), "actionName", currActionContext.actionName);

            if (string.IsNullOrEmpty(currActionContext.actionName))
                return;

            __statusContainer.Clear();

            if (__traceCollider != null)
                FinishTraceActionTargets();

            if (__pawnAnimCtrler != null && __pawnAnimCtrler.mainAnimator != null)
            {
                __pawnAnimCtrler.mainAnimator.SetFloat("AnimSpeed", 1);
                __pawnAnimCtrler.mainAnimator.SetFloat("AnimAdvance", 0);
            }

            currActionContext.actionDisposable?.Dispose();
            currActionContext.homingRotationDisposable?.Dispose();
            currActionContext.impulseRootMotionDisposable?.Dispose();
            currActionContext.actionDisposable = null;
            currActionContext.homingRotationDisposable = null;
            currActionContext.impulseRootMotionDisposable = null;
            currActionContext.finishTimeStamp = Time.time;

            prevActionContext = currActionContext;
            prevActionContext.finishTimeStamp = Time.time;
            currActionContext = new(string.Empty, 1, 0);
            onActionFinished?.Invoke(prevActionContext);

#if UNITY_EDITOR
            __actionFinishPosition = __pawnBrain.coreColliderHelper.transform.position;
            __Logger.LogR(gameObject, nameof(FinishAction), "distance", (__actionFinishPosition - __actionStartPosition).Vector2D().magnitude);
#endif
        }

        public void SetMovementEnabled(bool newValue)
        {
            if (!CheckActionRunning())
            {
                __Logger.WarningF(gameObject, nameof(SetMovementEnabled), "CheckActionRunning() return false.");
                return;
            }

            currActionContext.movementEnabled = newValue;
        }

        public void SetRootMotionEnabled(bool newValue)
        {
            if (!CheckActionRunning())
            {
                __Logger.WarningF(gameObject, nameof(SetRootMotionEnabled), "CheckActionRunning() return false.");
                return;
            }

            currActionContext.rootMotionEnabled = newValue;
        }

        public void SetLegAnimGlueEnabled(bool newValue)
        {
            if (!CheckActionRunning())
            {
                __Logger.WarningF(gameObject, nameof(SetLegAnimGlueEnabled), "CheckActionRunning() return false.");
                return;
            }

            currActionContext.legAnimGlueEnabled = newValue;
        }

        public void SetInterruptEnabled(bool newValue)
        {
            if (!CheckActionRunning())
            {
                __Logger.WarningF(gameObject, nameof(SetInterruptEnabled), "CheckActionRunning() return false.");
                return;
            }

            currActionContext.interruptEnabled = newValue;
        }

        public void SetSuperArmorEnabled(bool newValue)
        {
            if (!CheckActionRunning())
            {
                __Logger.WarningF(gameObject, nameof(SetSuperArmorEnabled), "CheckActionRunning() return false.");
                return;
            }

            // currActionContext. = newValue;
        }

        public void SetActiveParryingEnabled(bool newValue)
        {
            if (!CheckActionRunning())
            {
                __Logger.WarningF(gameObject, nameof(SetActiveParryingEnabled), "CheckActionRunning() return false.");
                return;
            }

            if (currActionContext.activeParryEnabled != newValue)
            {
                currActionContext.activeParryEnabled = newValue;
                onActiveParryEnabled?.Invoke(currActionContext);
            }
        }

        public void SetTraceRunning(bool newValue)
        {
            if (!CheckActionRunning())
            {
                __Logger.WarningF(gameObject, nameof(SetTraceRunning), "CheckActionRunning() return false.");
                return;
            }

            currActionContext.isTraceRunning = newValue;
        }
        
        static readonly RaycastHit[] __traceTempHits = new RaycastHit[16];
        static readonly Collider[] __traceTempColliders = new Collider[16];
        int __traceLayerMask;
        int __traceCount = 0;
        float __traceSampleInterval = 0;
        bool __traceDrawGizmosEnabled;
        float __traceDrawGizmosDuration;
        float __prevTraceTimeStamp;
        protected Vector3 __prevTracePosition;
        protected Quaternion __prevTraceRotation;
        Collider __traceCollider;
        protected BoxCollider __traceBoxCollider;
        protected SphereCollider __traceSphereCollider;
        protected CapsuleCollider __traceCapsuleCollider;
        readonly HashSet<string> __traceLayerNames = new();
        readonly HashSet<string> __tracePawnNames = new();
        readonly HashSet<PawnBrainController> __tracedPawnBrains = new();
        PawnStatusController.StatusParam[] __debuffParams;
        bool __multiHitEnabled;
        bool __sendDamageOnTrace;

        public void StartTraceActionTargets(Collider traceCollider, int samplingRate = 60, bool multiHitEnabled = false, PawnStatusController.StatusParam[] debuffParams = null, bool sendDamageImmediately = false, bool drawGizmosEnabled = false, float drawGizmosDuration = 0)
        {
            SetTraceRunning(true);

            __traceLayerMask = LayerMask.GetMask(__traceLayerNames.ToArray());
            __traceCount = 0;
            __traceSampleInterval = 1f / (float)samplingRate;
            __multiHitEnabled = multiHitEnabled;
            __debuffParams = debuffParams;
            __sendDamageOnTrace = sendDamageImmediately;
            __traceDrawGizmosEnabled = drawGizmosEnabled;
            __traceDrawGizmosDuration = drawGizmosDuration;
            __prevTraceTimeStamp = Time.time;

            __traceCollider = traceCollider;
            __traceBoxCollider = __traceCollider as BoxCollider;
            if (__traceBoxCollider != null)
            {
                __prevTracePosition = __traceBoxCollider.transform.localToWorldMatrix.MultiplyPoint(__traceBoxCollider.center);
                __prevTraceRotation = __traceBoxCollider.transform.rotation;
                return;
            }
            __traceSphereCollider = __traceCollider as SphereCollider;
            if (__traceSphereCollider != null)
            {
                __prevTracePosition = __traceSphereCollider.transform.localToWorldMatrix.MultiplyPoint(__traceSphereCollider.center);
                __prevTraceRotation = __traceSphereCollider.transform.rotation;
                return;
            }
            __traceCapsuleCollider = __traceCollider as CapsuleCollider;
            if (__traceCapsuleCollider != null)
            {
                __prevTracePosition = __traceCapsuleCollider.transform.localToWorldMatrix.MultiplyPoint(__traceCapsuleCollider.center);
                __prevTraceRotation = __traceCapsuleCollider.transform.rotation;
                return;
            }
        }

        public int FinishTraceActionTargets()
        {
            SetTraceRunning(false);

            __traceCollider = null;
            __traceBoxCollider = null;
            __traceSphereCollider = null;
            __traceCapsuleCollider = null;
            __debuffParams = null;
            __sendDamageOnTrace = false;

            var ret = __tracedPawnBrains.Count;
            __tracedPawnBrains.Clear();

            return ret;
        }

        public List<PawnColliderHelper> TraceActionTargets(Vector3 offset, Vector3 pitchYawRoll, float fanRadius, float fanAngle, float fanHeight, float minRadius, int maxTargetNum = -1, PawnStatusController.StatusParam[] debuffParams = null, bool sendDamageImmediately = false, bool drawGizmosEnabled = false, float drawGizmosDuration = 0)
        {
            return TraceActionTargets(Matrix4x4.TRS(offset + __pawnBrain.coreColliderHelper.pawnCollider.bounds.center - __pawnBrain.coreColliderHelper.transform.position, Quaternion.Euler(pitchYawRoll), Vector3.one), fanRadius, fanAngle, fanHeight, minRadius, maxTargetNum, debuffParams, sendDamageImmediately, drawGizmosEnabled, drawGizmosDuration);
        }

        public List<PawnColliderHelper> TraceActionTargets(Matrix4x4 fanMatrix, float fanRadius, float fanAngle, float fanHeight, float minRadius, int maxTargetNum = -1, PawnStatusController.StatusParam[] debuffParams = null, bool sendDamageImmediately = false, bool drawGizmosEnabled = false, float drawGizmosDuration = 0)
        {
            Debug.Assert(CheckActionRunning() && currActionContext.isTraceRunning, $"Brain: {gameObject}, CheckActionRunning(): {CheckActionRunning()}, currActionContext.traceRunning: {currActionContext.isTraceRunning}");

            var fanCenter = __pawnBrain.coreColliderHelper.pawnCollider.bounds.center + __pawnBrain.coreColliderHelper.transform.rotation * fanMatrix.GetPosition();
            var overlappedCount = __traceLayerNames != null ?
                Physics.OverlapCapsuleNonAlloc(fanCenter + 0.5f * fanHeight * Vector3.up, fanCenter + 0.5f * fanHeight * Vector3.down, fanRadius, __traceTempColliders, LayerMask.GetMask(__traceLayerNames.ToArray())) :
                Physics.OverlapCapsuleNonAlloc(fanCenter + 0.5f * fanHeight * Vector3.up, fanCenter + 0.5f * fanHeight * Vector3.down, fanRadius, __traceTempColliders);

            var localToWorld = __pawnBrain.coreColliderHelper.transform.localToWorldMatrix * fanMatrix;
            var worldToLocal = localToWorld.inverse;
            var compareDot = -1f;
            var compareSqrDistance = -1f;
            var compareColliderRadius = 0f;
            var results = new List<PawnColliderHelper>();

            if (overlappedCount > 0)
            {
                foreach (var c in __traceTempColliders)
                {
                    if (c == null)
                        continue;
                    if (!c.TryGetComponent<PawnColliderHelper>(out var tracedColliderHelper) || tracedColliderHelper.pawnBrain == null || tracedColliderHelper.pawnBrain == __pawnBrain)
                        continue;
                    if (__tracePawnNames.Count > 0 && !__tracePawnNames.Contains(tracedColliderHelper.pawnBrain.PawnBB.common.pawnName))
                        continue;

                    if (fanAngle < 360f)
                    {
                        var currDot = Vector3.Dot(__pawnBrain.coreColliderHelper.transform.forward.Vector2D().normalized, 
                            (c.transform.position - __pawnBrain.coreColliderHelper.transform.position).Vector2D().normalized);

                        //* 정면 방향에서 'minRadius'만큼 가까이 붙어있는지 체크 (minRadius보다 안쪽에 있는 대상에 대해선 정면 180도 시야각 안에만 존재하면 히트한 것으로 판정함)
                        var finalOverlapped = currDot >= 0 && tracedColliderHelper.GetDistanceBetween(__pawnBrain.coreColliderHelper) <= minRadius;
                        if (!finalOverlapped)
                        {
                            if (c as SphereCollider != null)
                                finalOverlapped = (c as SphereCollider).CheckOverlappedWithFan(fanAngle, fanRadius, fanHeight, worldToLocal);
                            else if (c as CapsuleCollider != null)
                                finalOverlapped = (c as CapsuleCollider).CheckOverlappedWithFan(fanAngle, fanRadius, fanHeight, worldToLocal);
                        }

                        if (finalOverlapped)
                        {
                            var deltaDot = compareDot - currDot;
                            
                            //* 각도 차가 거의 없으면 Radius가 큰 Collider를 압쪽에 추가함
                            if (Mathf.Abs(deltaDot) < 0.01f && tracedColliderHelper.GetRadius() > compareColliderRadius)
                            {
                                results.Insert(0, tracedColliderHelper);
                                compareDot = currDot;
                                compareColliderRadius = tracedColliderHelper.GetRadius();
                            }
                            else if (deltaDot < 0) //* 각도 차가 작은 타겟은 앞쪽에 추가함
                            {
                                results.Insert(0, tracedColliderHelper);
                                compareDot = currDot;
                                compareColliderRadius = tracedColliderHelper.GetRadius();
                            }
                            else
                            {
                                results.Add(tracedColliderHelper);
                            }
                        }
                    }
                    else
                    {
                        var finalOverlapped = false;                        
                        if (c as SphereCollider != null)
                            finalOverlapped = (c as SphereCollider).CheckOverlappedWithFan(fanAngle, fanRadius, fanHeight, worldToLocal);
                        else if (c as CapsuleCollider != null)
                            finalOverlapped = (c as CapsuleCollider).CheckOverlappedWithFan(fanAngle, fanRadius, fanHeight, worldToLocal);

                        if (finalOverlapped)
                        {
                            //* 거리값이 작은 타겟은 앞쪽에 추가함
                            var distanceVec = (c.transform.position - __pawnBrain.coreColliderHelper.transform.position).Vector2D();
                            if (distanceVec.sqrMagnitude > compareSqrDistance)
                            {
                                results.Insert(0, tracedColliderHelper);
                                compareSqrDistance = distanceVec.sqrMagnitude;
                            }
                            else
                            {
                                results.Add(tracedColliderHelper);
                            }
                        }
                    }
                }
            }

            if (drawGizmosEnabled)
            {
                GizmosDrawer.Instance.Draw(drawGizmosDuration, () =>
                {
                    Gizmos.color = Color.red;
                    foreach (var r in results)
                        if (r != null) Gizmos.DrawWireCube(r.pawnCollider.bounds.center, r.pawnCollider.bounds.extents);

                    Gizmos.color = Color.yellow;
                    GizmosDrawExtension.DrawFanCylinder(localToWorld, fanRadius, fanAngle, fanHeight, 12);
                });
            }

            if (maxTargetNum > 0 && results.Count > maxTargetNum)
                results = results.GetRange(0, maxTargetNum);

            if (sendDamageImmediately)
            {
                foreach (var r in results)
                {
                    __pawnBrain.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(__pawnBrain, r.pawnBrain, currActionContext.actionData, r.pawnCollider, currActionContext.insufficientStamina));

                    //* Debuff 할당
                    if (debuffParams != null && debuffParams.Length > 0)
                    {
                        foreach (var p in debuffParams)
                        {
                            if (p.isExtern && r.pawnBrain.TryGetComponent<PawnActionController>(out var receiverActionCtrler))
                                r.pawnBrain.PawnStatusCtrler.AddExternStatus(receiverActionCtrler, p);
                            else
                                r.pawnBrain.PawnStatusCtrler.AddStatus(p);
                        }
                    }
                }
            }

            Array.Clear(__traceTempColliders, 0, __traceTempColliders.Length);
            return results;
        }

        public void AddTraceLayerNames(params string[] layerNames)
        {
            Debug.Assert(layerNames != null && layerNames.Length > 0);
            foreach (var n in layerNames)
                __traceLayerNames.Add(n);
        }

        public void AddTracePawnNames(params string[] pawnNames)
        {
            Debug.Assert(pawnNames != null && pawnNames.Length > 0);
            foreach (var n in pawnNames)
                __tracePawnNames.Add(n);
        }

        public void ClearTraceNames()
        {
            __traceLayerNames.Clear();
            __tracePawnNames.Clear();
        }

        public virtual void EmitProjectile(GameObject emitSource, Transform emitPoint, int emitNum)
        {
            if (emitSource == null) 
            {
                Debug.Log("EmitProjectile is Null");
                return;
            }

            onEmitProjectile?.Invoke(currActionContext, emitSource.GetComponent<ProjectileMovement>(), emitPoint, emitNum);
        }
    }
}