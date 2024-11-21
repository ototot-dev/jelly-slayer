using System;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.BehaviourTrees;
using NodeCanvas.Framework;
using Retween.Rx;
using UniRx;
using UniRx.Triggers.Extension;
using Unity.Linq;
using UnityEngine;
using UnityEngine.UIElements.Experimental;
using XftWeapon;

namespace Game.NodeCanvasExtension
{
    public class CheckActionRunning : ConditionTask
    {
        [BlackboardOnly]
        public BBParameter<PawnActionController> actionCtrler;
        protected override string info => "CheckActionRunning() == " + (invert ? "False" : "True");

        protected override bool OnCheck() 
        {
            return actionCtrler.value.CheckActionRunning();
        }
    }

    public class CheckActionHasPreMotion : ConditionTask
    {
        [BlackboardOnly]
        public BBParameter<PawnActionController> actionCtrler;
        protected override string info => "CheckActionHasPreMotion() == " + (invert ? "False" : "True");

        protected override bool OnCheck() 
        {
            return actionCtrler.value.CheckPendingActionHasPreMotion();
        }
    }

    public class ModifyTransform : ActionTask
    {
        public BBParameter<Transform> target;
        public BBParameter<Vector3> position;
        public BBParameter<Vector3> pitchYawRoll;
        public bool changePosition = true;
        public bool changeRotation = true;

        protected override void OnExecute()
        {
            if (changePosition && changeRotation)
                target.value.SetLocalPositionAndRotation(position.value, Quaternion.Euler(pitchYawRoll.value.x, pitchYawRoll.value.y, pitchYawRoll.value.z));
            else if (changePosition)
                target.value.position = position.value;
            else    
                target.value.rotation = Quaternion.Euler(pitchYawRoll.value.x, pitchYawRoll.value.y, pitchYawRoll.value.z);

            EndAction(true);
        }
    }

    public class HoldPosition : ActionTask
    {
        public BBParameter<Transform> lookAt;
        public BBParameter<float> duration = -1;
        public bool notifyDecisionFinished = false;
        PawnBrainController __pawnBrain;
        IMovable __pawnMovable;
        float __executeTimeStamp;

        protected override void OnExecute()
        {
            __pawnBrain = agent.GetComponent<PawnBrainController>();
            Debug.Assert(__pawnBrain != null);

            __pawnMovable = __pawnBrain as IMovable;
            Debug.Assert(__pawnMovable != null);
            
            __pawnMovable.Stop();
            __executeTimeStamp = Time.time;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (!lookAt.isNoneOrNull)
                __pawnMovable.SetDestination(lookAt.value.position);

            if (duration.value > 0 && Time.time - __executeTimeStamp > duration.value)
                EndAction(true);
        }

        protected override void OnStop(bool interrupted)
        {
            base.OnStop(interrupted);

            if (!interrupted && notifyDecisionFinished)
                __pawnBrain.OnDecisionFinishedHandler();
        }
    }

    public class MoveTo : ActionTask
    {
        public BBParameter<Vector3> destination;
        IMovable __pawnMovable;
        IDisposable __moveDisposable;

        protected override void OnExecute()
        {
            __pawnMovable = agent.GetComponent<PawnBrainController>() as IMovable;
            __pawnMovable.MoveTo(destination.value);
            __moveDisposable = Observable.EveryUpdate()
                .TakeWhile(_ => !__pawnMovable.CheckReachToDestination())
                .TakeUntil(Observable.Timer(TimeSpan.FromSeconds(__pawnMovable.GetEstimateTimeToDestination() + 1)))
                .TakeLast(1)
                .DoOnCancel(() =>
                {
                    __pawnMovable.Stop();
                    EndAction(true);
                })
                .DoOnCompleted(() =>
                {
                    __pawnMovable.Stop();
                    EndAction(true);
                })
                .Subscribe().AddTo(agent);
        }

        protected override void OnStop(bool interrupted)
        {
            if (interrupted && __moveDisposable != null)
                __moveDisposable.Dispose();
        }
    }

    public class MoveAround : ActionTask
    {
        public BBParameter<float> maxTurnAngle = 90;
        public BBParameter<float> moveDistanceMultiplier = 1;
        PawnBrainController __pawnBrain;
        IMovable __pawnMovable;
        IDisposable __moveDisposable;

        protected override void OnExecute()
        {
            __pawnBrain = agent.GetComponent<PawnBrainController>();
            __pawnMovable = __pawnBrain as IMovable;

            var targetPoint = agent.GetComponent<PawnSensorController>().GetRandomPointInListeningArea(__pawnBrain.coreColliderHelper.transform.forward, maxTurnAngle.value, moveDistanceMultiplier.value);
            var distance = __pawnMovable.SetDestination(targetPoint);

            __moveDisposable = Observable.EveryUpdate()
                .TakeWhile(_ => !__pawnMovable.CheckReachToDestination())
                .TakeUntil(Observable.Timer(TimeSpan.FromSeconds(__pawnMovable.GetEstimateTimeToDestination() + 1)))
                .TakeLast(1)
                .DoOnCancel(() =>
                {
                    __pawnMovable.Stop();
                    EndAction(true);
                })
                .DoOnCompleted(() =>
                {
                    __pawnMovable.Stop();
                    EndAction(true);
                })
                .Subscribe().AddTo(agent);
        }

        protected override void OnStop(bool interrupted)
        {
            if (interrupted && __moveDisposable != null)
                __moveDisposable.Dispose();
        }
    }

    public class Approach : ActionTask
    {
        public BBParameter<Transform> target;
        public BBParameter<float> approachDistance;
        public BBParameter<float> duration = -1;
        public bool stopOnReachToDestination;
        public bool notifyDecisionFinished;

        PawnBrainController __targetBrain;
        PawnBrainController __pawnBrain;
        PawnActionController __pawnActionCtrler;
        IMovable __pawnMovable;
        float __targetCapsuleRadius;
        float __executeTimeStamp;

        protected override void OnExecute()
        {
            __targetBrain = target.value.GetComponent<PawnBrainController>();
            __targetCapsuleRadius = __targetBrain != null && __targetBrain.coreColliderHelper.GetCapsuleCollider() != null ? __targetBrain.coreColliderHelper.GetCapsuleCollider().radius : 0f;
            __pawnBrain = agent.GetComponent<PawnBrainController>();
            __pawnActionCtrler = __pawnBrain.GetComponent<PawnActionController>();
            __pawnMovable = __pawnBrain as IMovable;
            __executeTimeStamp = Time.time;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            __pawnMovable.SetMinApproachDistance(approachDistance.value + __targetCapsuleRadius);
            if (__pawnActionCtrler == null || !__pawnActionCtrler.CheckActionRunning())
                __pawnMovable.SetDestination(__targetBrain != null ? __targetBrain.coreColliderHelper.transform.position : target.value.position);

            if (duration.value > 0 && Time.time - __executeTimeStamp > duration.value)
                EndAction(true);
            else if (__targetBrain.PawnBB.IsDead || (stopOnReachToDestination && __pawnMovable.CheckReachToDestination()))
                EndAction(true);
        }

        protected override void OnStop(bool interrupted)
        {
            base.OnStop(interrupted);
            
            __pawnMovable.Stop();

            if (!interrupted && notifyDecisionFinished)
                __pawnBrain.OnDecisionFinishedHandler();
        }
    }

    public class Spacing : ActionTask
    {
        public BBParameter<Transform> target;
        public BBParameter<float> minDistance = 1;
        public BBParameter<float> maxDistance = 1;
        public BBParameter<float> outDistance = 1;
        public BBParameter<float> duration = -1;
        public bool notifyDecisionFinished = false;
        PawnBrainController __targetBrain;
        PawnBrainController __pawnBrain;
        IMovable __pawnMovable;
        IDisposable __moveStrafeDisposable;
        Vector3 __strafeMoveVec;
        float __strafeDuration;
        float __targetCapsuleRadius;
        float __minApproachDistance;
        float __executeTimeStamp;

        protected override void OnExecute()
        {
            __targetBrain = target.value.GetComponent<PawnBrainController>();
            __targetCapsuleRadius = __targetBrain.coreColliderHelper.GetCapsuleCollider() != null ? __targetBrain.coreColliderHelper.GetCapsuleCollider().radius : 0f;
            __pawnBrain = agent.GetComponent<PawnBrainController>();
            __pawnMovable = __pawnBrain as IMovable;
            __strafeMoveVec = Vector3.zero;
            __minApproachDistance = Mathf.Max(0.1f, __pawnBrain.coreColliderHelper.GetRadius() + __targetCapsuleRadius);
            __pawnMovable.SetMinApproachDistance(__minApproachDistance);
            __pawnMovable.FreezeRotation(true);
            __executeTimeStamp = Time.time;
        }

        protected override void OnUpdate()
        {
            var isDurationExpired = duration.value > 0 && Time.time - __executeTimeStamp > duration.value;
            if (__moveStrafeDisposable == null && !isDurationExpired)
                __moveStrafeDisposable = MoveStrafe();

            if (isDurationExpired && __moveStrafeDisposable == null)
                EndAction(true);
            else if (__targetBrain != null && (__targetBrain.PawnBB.IsDead || __targetBrain.coreColliderHelper.GetApproachDistance(__pawnBrain.coreColliderHelper.transform.position) > outDistance.value))
                EndAction(true);
        }

        IDisposable MoveStrafe()
        {
            var spacingDistance = UnityEngine.Random.Range(minDistance.value, maxDistance.value);
            
            __strafeMoveVec = __strafeMoveVec == Vector3.zero ? new Vector3(UnityEngine.Random.Range(-1, 2), 0f, 0f) : Vector3.zero;
            if (__strafeMoveVec == Vector3.zero)
            {
                __strafeDuration = UnityEngine.Random.Range(1f, 2f);
                __pawnMovable.Stop();
            }
            else
            {
                __strafeDuration = duration.value > 0 ? UnityEngine.Random.Range(duration.value * 0.5f, duration.value) : UnityEngine.Random.Range(3f, 4f);
            }

            return Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(__strafeDuration)))
                .DoOnCompleted(() =>
                {
                    if (__moveStrafeDisposable != null)
                    {
                        __moveStrafeDisposable.Dispose();
                        __moveStrafeDisposable = null;
                    }
                })
                .Subscribe(_ =>
                {
                    if (__strafeMoveVec != Vector3.zero)
                    {
                        var newDestination = __pawnBrain.coreColliderHelper.transform.position + __minApproachDistance * 2f * (__pawnBrain.coreColliderHelper.transform.rotation * __strafeMoveVec).Vector2D().normalized;
                        newDestination = __targetBrain.coreColliderHelper.transform.position + (spacingDistance + __targetBrain.coreColliderHelper.GetRadius()) * (newDestination - __targetBrain.coreColliderHelper.transform.position).Vector2D().normalized;
                        __pawnMovable.SetDestination(newDestination);
                    }
                    
                    if (!__pawnBrain.PawnBB.IsDown)
                        __pawnMovable.SetFaceVector((__targetBrain.coreColliderHelper.transform.position - __pawnBrain.coreColliderHelper.transform.position).Vector2D().normalized);
                }).AddTo(agent);
        }

        protected override void OnStop(bool interrupted)
        {
            base.OnStop(interrupted);

            if (__moveStrafeDisposable != null)
            {
                __moveStrafeDisposable.Dispose();
                __moveStrafeDisposable = null;
            }

            __pawnMovable.FreezeRotation(false);
            __pawnMovable.Stop();

            if (!interrupted && notifyDecisionFinished)
                __pawnBrain.OnDecisionFinishedHandler();
        }
    }

    public class ImpluseRootMotion : ActionTask
    {
        public BBParameter<float> impulseStrength;
        public BBParameter<float> duration;
        public BBParameter<ParadoxNotion.Animation.EaseType> accelEase;
        public BBParameter<float> accelDuration = -1f;
        public BBParameter<ParadoxNotion.Animation.EaseType> breakEase;
        public BBParameter<float> breakDuration = -1f;
        public BBParameter<PawnColliderHelper> targetColliderHelper;
        public bool endActionWhenReachToTarget;
        PawnBrainController __pawnBrain;
        PawnActionController __pawnActionCtrler;
        IMovable __pawnMovable;
        IDisposable __rootMotionDisposable;
        
        protected override void OnExecute()
        {
            __pawnBrain = agent.GetComponent<PawnBrainController>();
            __pawnActionCtrler = agent.GetComponent<PawnActionController>();
            __pawnMovable = __pawnBrain as IMovable;

            Debug.Assert(__pawnBrain != null && __pawnMovable as IMovable != null);
            Debug.Assert(__pawnActionCtrler != null);

            var executeTimeStamp = Time.time;

            __pawnActionCtrler.currActionContext.impulseRootMotionDisposable?.Dispose();
            __pawnActionCtrler.currActionContext.impulseRootMotionDisposable = __rootMotionDisposable = Observable.EveryUpdate()
                .TakeUntil(Observable.Timer(TimeSpan.FromSeconds(duration.value + Mathf.Max(0f, accelDuration.value) + Mathf.Max(0f, breakDuration.value))))
                .DoOnCancel(() => 
                {
                    __rootMotionDisposable = __pawnActionCtrler.currActionContext.impulseRootMotionDisposable = null;
                    if (endActionWhenReachToTarget)
                        EndAction(true);
                })
                .DoOnCompleted(() =>
                {
                    __rootMotionDisposable = __pawnActionCtrler.currActionContext.impulseRootMotionDisposable = null;
                    if (endActionWhenReachToTarget)
                        EndAction(true);
                })
                .Subscribe(_ =>
                {
                    //* Target과 최소 접근 거리에 도달헀으면 RootMotion 적용 안함
                    if (!targetColliderHelper.isNoneOrNull && targetColliderHelper.value.GetApproachDistance(__pawnBrain.coreColliderHelper.transform.position) <= __pawnMovable.GetDefaultMinApproachDistance())
                    {
                        if (endActionWhenReachToTarget)
                        {
                            Debug.Assert(__rootMotionDisposable != null && __rootMotionDisposable == __pawnActionCtrler.currActionContext.impulseRootMotionDisposable);
                            __rootMotionDisposable.Dispose();
                            __rootMotionDisposable = __pawnActionCtrler.currActionContext.impulseRootMotionDisposable = null;
                        }

                        return;
                    }

                    var impulse = impulseStrength.value;
                    var deltaTime = Time.time - executeTimeStamp;
                    if (deltaTime < accelDuration.value)
                        impulse = impulseStrength.value * ParadoxNotion.Animation.Easing.Ease(accelEase.value, 0f, 1f, deltaTime / accelDuration.value);
                    else if (deltaTime > duration.value + Mathf.Max(0f, accelDuration.value))
                        impulse = impulseStrength.value * ParadoxNotion.Animation.Easing.Ease(breakEase.value, 0, 1f, 1f - (deltaTime - duration.value - Mathf.Max(0f, accelDuration.value)) / breakDuration.value);

                    var rootMotionVec = Time.deltaTime * impulse * __pawnBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                    if (__pawnActionCtrler.CanRootMotion(rootMotionVec))
                        __pawnMovable.AddRootMotion(rootMotionVec, Quaternion.identity);
                }).AddTo(agent);

            if (!endActionWhenReachToTarget)
                EndAction(true);
        }
    }

    public class StartPendingAction : ActionTask
    {
        public BBParameter<string> actionName;
        public BBParameter<bool> manualAdvanceEnabled;
        public BBParameter<float> animSpeedMultiplier = 1;
        public BBParameter<float> animClipLength = -1;
        public BBParameter<int> animClipFps = -1;
        public BBParameter<float> rootMotionMultiplier = 1;
        public BBParameter<AnimationCurve> rootMotionCurve;
        protected override string info => $"Start Action <b>{actionName.value}</b>";

        protected override void OnExecute()
        {
            var actionCtrler = agent.GetComponent<PawnActionController>();
            Debug.Assert(actionCtrler != null);

            if (actionCtrler.PendingActionData.Item1 == actionName.value)
            {
                EndAction(actionCtrler.StartAction(actionName.value, string.Empty,animSpeedMultiplier.value, rootMotionMultiplier.value, rootMotionCurve.value, manualAdvanceEnabled.value));
                actionCtrler.ClearPendingAction();

                if (animClipLength.value > 0)
                    actionCtrler.currActionContext.animClipLength = animClipLength.value;
                if (animClipFps.value > 0)
                    actionCtrler.currActionContext.animClipFps = animClipFps.value;
            }
            else
            {
                EndAction(false);
            }
        }
    }

    public class StartAction : ActionTask
    {
        public BBParameter<string> actionName;
        public BBParameter<float> actionSpeed = new(1);
        public BBParameter<bool> manualAdvanceEnabled;
        public BBParameter<float> rootMotionMultiplier = new(1);
        public BBParameter<AnimationCurve> rootMotionCurve;
        public BBParameter<float> actionTimeOut = new(-1);
        protected override string info => $"Start Action <b>{actionName.value}</b>";

        protected override void OnExecute()
        {
            EndAction(agent.GetComponent<PawnActionController>().StartAction(actionName.value, string.Empty, actionSpeed.value, rootMotionMultiplier.value, rootMotionCurve.value, manualAdvanceEnabled.value));
        }
    }

    public class FinishAction : ActionTask
    {
        protected override string info => possibility.isNoneOrNull || possibility.value <= 0f ? base.info : $"Finish Action <b>{possibility.value * 100f}%</b>";
        public BBParameter<float> possibility;

        protected override void OnExecute()
        {
            if (possibility.isNoneOrNull || possibility.value <= 0f || possibility.value < UnityEngine.Random.Range(0f, 1f))
            {
                var actionCtrler = agent.GetComponent<PawnActionController>();
                Debug.Assert(actionCtrler != null);

                actionCtrler.ClearPendingAction();
                actionCtrler.FinishAction();

                //* possibility.value가 0보다 크다면 EndAction(false)를 실행시켜 현재 Action 시퀀스 전체를 취소시킴
                EndAction(possibility.value <= 0f);
            }
            else
            {
                //* FinishAction 조건을 만족하지 못함
                EndAction(true);
            }
        }
    }

    public class WaitAction : ActionTask
    {
        protected override string info => useCurrentTime.value ? $"Wait for {waitTime} secs" : $"Wait@ {waitTime} secs";
        public BBParameter<float> waitTime = 1f;
        public BBParameter<bool> useCurrentTime = true;
        public BBParameter<bool> interruptEnabled = false;
        PawnActionController __pawnActionCtrler;
        int __capturedActionInstanceId;

        protected override void OnExecute()
        {
            __pawnActionCtrler = agent.GetComponent<PawnActionController>();
            __pawnActionCtrler.WaitAction();
            __capturedActionInstanceId =  __pawnActionCtrler.currActionContext.actionInstanceId;

            //* 대기 모드이면 Wait 시작 시점에서 interruptEnabled 값을 변경해준다.
            if (useCurrentTime.value)
                __pawnActionCtrler.SetInterruptEnabled(interruptEnabled.value);
        }

        protected override void OnUpdate()
        {
            if (__pawnActionCtrler.currActionContext.actionData == null || __capturedActionInstanceId != __pawnActionCtrler.currActionContext.actionInstanceId)
            {
                EndAction(false);
                return;
            }
            
            // 'baseTimeStamp'은 기본적으로 WaitAction()이 호출된 시간으로 셋팅된다. 하지만 'useCurrentTime.value'값이 false라면 Action이 시작된 시간으로 셋팅된다.
            // 이 경우엔 다시 2가지 경우가 생기는데 'manualAdvanceEnabled' 값이 false라면 'startTimeStamp'값, 즉 Action이 시작된 시점을 그대로 사용한다. 만약
            //  'manualAdvanceEnabled' 값이 true라면 Action의 시작된 시간은 (Time.time - __pawnActionCtrler.currActionContext.manualAdvanceTime)가 된다.
            var baseTimeStamp = __pawnActionCtrler.currActionContext.waitTimeStamp;
            if (!useCurrentTime.value)
            {
                if (__pawnActionCtrler.currActionContext.manualAdvanceEnabled)
                {
                    baseTimeStamp = Time.time - __pawnActionCtrler.currActionContext.manualAdvanceTime;
                }
                else
                {
                    //* 'preMotionTimeStamp'값이 있으면. 실제 액션 시작 시간은 preMotionTimeStamp으로 간주함
                    baseTimeStamp = __pawnActionCtrler.currActionContext.preMotionTimeStamp > 0f ? __pawnActionCtrler.currActionContext.preMotionTimeStamp : __pawnActionCtrler.currActionContext.startTimeStamp;
                }
            }

            if (!__pawnActionCtrler.CheckWaitAction(waitTime.value, baseTimeStamp))
                EndAction(true);
        }

        protected override void OnStop(bool interrupted)
        {
            base.OnStop(interrupted);

            //* 대기 모드가 아니면, Wait 종료 시점에서 interruptEnabled 값을 변경해준다.
            if (!useCurrentTime.value)
                __pawnActionCtrler.SetInterruptEnabled(interruptEnabled.value);
        }
    }

    public class WaitFrame : ActionTask
    {
        protected override string info => waitFrame.value > 0 ? $"Frame<b>#{waitFrame.value}</b>" + (interruptEnabled.value ? " <b>!!</b>" : string.Empty) : $"Frame<b>#End</b>";
        public BBParameter<int> waitFrame = -1;
        public BBParameter<bool> interruptEnabled = false;
        PawnActionController __pawnActionCtrler;
        int __capturedActionInstanceId;

        protected override void OnExecute()
        {
            __pawnActionCtrler = agent.GetComponent<PawnActionController>();
            __pawnActionCtrler.WaitAction();
            __capturedActionInstanceId =  __pawnActionCtrler.currActionContext.actionInstanceId;
        }

        protected override void OnUpdate()
        {
            if (__pawnActionCtrler.currActionContext.actionData == null || __capturedActionInstanceId != __pawnActionCtrler.currActionContext.actionInstanceId)
            {
                EndAction(false);
                return;
            }

            if (__pawnActionCtrler.currActionContext.manualAdvanceEnabled)
            {
                var baseTimeStamp = Time.time - __pawnActionCtrler.currActionContext.manualAdvanceTime;
                var waitTime = waitFrame.value > 0 ? 1f / __pawnActionCtrler.currActionContext.animClipFps * waitFrame.value : __pawnActionCtrler.currActionContext.animClipLength;
                if (!__pawnActionCtrler.CheckWaitAction(waitTime, baseTimeStamp))
                    EndAction(true);
            }
            else
            {
                //* 'preMotionTimeStamp'값이 있으면. 실제 액션 시작 시간은 preMotionTimeStamp으로 간주함
                var baseTimeStamp = __pawnActionCtrler.currActionContext.preMotionTimeStamp > 0f ? __pawnActionCtrler.currActionContext.preMotionTimeStamp : __pawnActionCtrler.currActionContext.startTimeStamp;
                var waitTime = waitFrame.value > 0 ? 1f / __pawnActionCtrler.currActionContext.animClipFps * waitFrame.value : __pawnActionCtrler.currActionContext.animClipLength;
                waitTime /= __pawnActionCtrler.currActionContext.actionSpeed;
                if (!__pawnActionCtrler.CheckWaitAction(waitTime, baseTimeStamp))
                    EndAction(true);
            }
        }

        protected override void OnStop(bool interrupted)
        {
            base.OnStop(interrupted);

            //* Wait 종료 시점에서 interruptEnabled 값을 변경해준다.
            __pawnActionCtrler.SetInterruptEnabled(interruptEnabled.value);
        }
    }

    public class WaitPreMotion : ActionTask
    {
        protected override string info => string.IsNullOrEmpty(stateName.value) ? base.info : $"Wait PreMotion <b>{stateName.value}</b>";
        public BBParameter<string> stateName;
        PawnActionController __pawnActionCtrler;
        PawnAnimController __pawnAnimCtrler;
        IDisposable __stateExitDisposable;
        int __capturedActionInstanceId;
        float __manualAdvanceSpeedCached;

        protected override void OnExecute()
        {
            __pawnAnimCtrler = agent.GetComponent<PawnAnimController>();
            __pawnActionCtrler = agent.GetComponent<PawnActionController>();
            __capturedActionInstanceId = __pawnActionCtrler.currActionContext.actionInstanceId;
            __manualAdvanceSpeedCached = __pawnActionCtrler.currActionContext.manualAdvanceSpeed;

            //* animStateName 값이 비어 있으면 PreMotion은 없는 것으로 간주함
            if (stateName.isNoneOrNull || string.IsNullOrEmpty(stateName.value))
                EndAction(true);

            var stateBehaviour =__pawnAnimCtrler.FindObservableStateMachineTriggerEx(stateName.value);
            Debug.Assert(stateBehaviour != null);

            __pawnActionCtrler.currActionContext.manualAdvanceSpeed = 0;
            __stateExitDisposable = stateBehaviour.OnStateExitAsObservable().Subscribe(_ => 
            {
                __pawnActionCtrler.currActionContext.manualAdvanceSpeed = __manualAdvanceSpeedCached;
                __pawnActionCtrler.currActionContext.preMotionTimeStamp = Time.time;
                EndAction(true);
            }).AddTo(agent);
        }

        protected override void OnUpdate()
        {
            if (__pawnActionCtrler.currActionContext.actionData == null || __capturedActionInstanceId != __pawnActionCtrler.currActionContext.actionInstanceId)
                EndAction(false);
        }

        protected override void OnStop(bool interrupted)
        {
            base.OnStop(interrupted);
            
            __stateExitDisposable?.Dispose();
            __stateExitDisposable = null;
        }
    }

    public class DelayAction : ActionTask
    {
        protected override string info => endActionWhenImpluseRootMotionDisposed ? $"Delay {delayTime} secs" : $"Delay {delayTime} secs";
        public BBParameter<float> delayTime = 1f;
        public BBParameter<float> randomRangeMin = -0.1f;
        public BBParameter<float> randomRangeMax = 0.1f;
        public BBParameter<float> animAdvanceSinusoidalAmplitude = 0.1f;
        public BBParameter<float> animAdvanceSinusoidalFrequence = 1;
        public bool endActionWhenImpluseRootMotionDisposed;
        PawnActionController __pawnActionCtrler;
        int __capturedActionInstanceId;
        float __waitDuration;
        float __animAdvanceSinusoidal;
        float __manualAdvanceTimeCached;
        float __manualAdvanceSpeedCached;

        protected override void OnExecute()
        {
            __pawnActionCtrler = agent.GetComponent<PawnActionController>();

            if (endActionWhenImpluseRootMotionDisposed)
                Debug.Assert(__pawnActionCtrler.currActionContext.impulseRootMotionDisposable != null);

            __pawnActionCtrler.WaitAction();
            __capturedActionInstanceId =  __pawnActionCtrler.currActionContext.actionInstanceId;
            __waitDuration = delayTime.value + (randomRangeMax.value > randomRangeMin.value ? UnityEngine.Random.Range(randomRangeMin.value, randomRangeMax.value) : 0);
            __animAdvanceSinusoidal = 0;
            __manualAdvanceSpeedCached = __pawnActionCtrler.currActionContext.manualAdvanceSpeed;
            __manualAdvanceTimeCached =  __pawnActionCtrler.currActionContext.manualAdvanceTime;
            __pawnActionCtrler.currActionContext.manualAdvanceSpeed = 0;
        }

        protected override void OnUpdate()
        {
            __pawnActionCtrler.currActionContext.manualAdvanceSpeed = 0;

            if (__pawnActionCtrler.currActionContext.actionData == null || __capturedActionInstanceId != __pawnActionCtrler.currActionContext.actionInstanceId)
            {
                EndAction(false);
            }
            else if (!__pawnActionCtrler.CheckWaitAction(__waitDuration))
            {   
                EndAction(true);
            }
            else if (endActionWhenImpluseRootMotionDisposed && __pawnActionCtrler.currActionContext.impulseRootMotionDisposable == null)
            {
                EndAction(true);
            }
            else if (animAdvanceSinusoidalAmplitude.value > 0)
            {
                __animAdvanceSinusoidal += animAdvanceSinusoidalFrequence.value * 2f * Mathf.PI * Time.deltaTime;
                __pawnActionCtrler.currActionContext.manualAdvanceTime = __manualAdvanceTimeCached + animAdvanceSinusoidalAmplitude.value * Mathf.Sin(__animAdvanceSinusoidal);
            }
        }

        protected override void OnStop(bool interrupted)
        {
            base.OnStop(interrupted);
            __pawnActionCtrler.currActionContext.manualAdvanceSpeed = __manualAdvanceSpeedCached;
        }
    }

    public class StartHomingRotation : ActionTask
    {
        public BBParameter<Transform> target;
        public BBParameter<float> rotateSpeed = 1f;
        const float __MIN_DELTA_ANGLE = 5f;

        protected override void OnExecute()
        {
            var brain = agent.GetComponent<PawnBrainController>();
            var actionCtrler = agent.GetComponent<PawnActionController>();

            Debug.Assert(brain != null && brain as IMovable != null);
            Debug.Assert(actionCtrler != null);

            var executeTimeStamp = Time.time;
            var movable = brain as IMovable;

            actionCtrler.currActionContext.homingRotationDisposable?.Dispose();
            actionCtrler.currActionContext.homingRotationDisposable = Observable.EveryUpdate().Subscribe(_ =>
            {
                var forward = brain.coreColliderHelper.transform.forward.Vector2D().normalized;
                var deltaAngle = target.isNoneOrNull ? 0f : Vector3.SignedAngle(forward, (target.value.position - brain.coreColliderHelper.transform.position).Vector2D().normalized, Vector3.up);
                if (Mathf.Abs(deltaAngle) > __MIN_DELTA_ANGLE)
                {
                    var rotateAngle = (deltaAngle > 0f ? 1f : -1f) * Mathf.Min(Mathf.Abs(deltaAngle) * Mathf.Rad2Deg, rotateSpeed.value * Time.deltaTime);
                    movable.FaceTo(Quaternion.Euler(0f, rotateAngle, 0f) * forward);
                }
            }).AddTo(agent);

            EndAction(true);
        }
    }

    public class FinishHomingRotation : ActionTask
    {
        protected override void OnExecute()
        {
            var actionCtrler = agent.GetComponent<PawnActionController>();
            Debug.Assert(actionCtrler != null);

            actionCtrler.currActionContext.homingRotationDisposable?.Dispose();
            actionCtrler.currActionContext.homingRotationDisposable = null;

            EndAction(true);
        }
    }

    public class StartTraceActionTargets : ActionTask
    {
        public BBParameter<Collider> traceCollider;
        public BBParameter<string[]> traceLayerNames;
        public BBParameter<string[]> tracePawnNames;
        public BBParameter<bool> resetTraceNames;
        public BBParameter<int> traceSamplingRate = 60;
        public BBParameter<bool> multiHitEnabled;
        public bool drawGizmos;
        public float drawGizmosDuration;

        protected override void OnExecute()
        {
            var actionCtrler = agent.GetComponent<PawnActionController>();
            Debug.Assert(actionCtrler != null);

            if (resetTraceNames.value)
                actionCtrler.ClearTraceNames();
            if (!traceLayerNames.isNull && traceLayerNames.value.Length > 0)
                actionCtrler.AddTraceLayerNames(traceLayerNames.value);
            if (!tracePawnNames.isNull && tracePawnNames.value.Length > 0)
                actionCtrler.AddTracePawnNames(tracePawnNames.value);

            actionCtrler.StartTraceActionTargets(traceCollider.value, traceSamplingRate.value, multiHitEnabled.value, true, drawGizmos, drawGizmosDuration);
            EndAction(true);
        }
    }

    public class FinishTraceActionTargets : ActionTask
    {
        public bool cancelActionWhenTraceFailed;

        protected override void OnExecute()
        {
            EndAction(agent.GetComponent<PawnActionController>().FinishTraceActionTargets().Count > 0 || !cancelActionWhenTraceFailed);
        }
    }

    public class TraceActionTargets : ActionTask
    {
        protected override string info => traceSampleNum.value == 1 ? "Trace <b>One-Frame</b>" : (traceDuration.value > 0 ? $"Trace for <b>{traceDuration.value}</b> secs" : $"Trace for <b>{traceFrames.value}</b> frames");
        
        public BBParameter<string> actionDataName;
        public BBParameter<Vector3> offset;
        public BBParameter<Vector3> pitchYawRoll;
        public BBParameter<float> fanAngle = 180;
        public BBParameter<float> fanRadius = 1;
        public BBParameter<float> fanHeight = 1;
        public BBParameter<float> minRadius = 0.1f;
        public BBParameter<int> maxTargetNum = 1;
        public BBParameter<string[]> traceLayerNames;
        public BBParameter<string[]> tracePawnNames;
        public BBParameter<bool> resetTraceNames = true;
        public BBParameter<int> traceSampleNum = 1;
        public BBParameter<int> traceDirection = 1;
        public BBParameter<float> traceDuration = 0f;
        public BBParameter<int> traceFrames = 0;
        public bool cancelActionWhenTraceFailed;
        public bool drawGizmos;
        public float drawGizmosDuration;
        int __capturedActionInstanceId;
        int __sampleIndex;
        int __sampleNum;
        float __sampleInterval;
        float __traceDuration;
        float __halfFanAngle;
        float __stepFanAngle;
        float __lastSampleTimeStamp;
        MainTable.ActionData __actionData;
        PawnBrainController __pawnBrain;
        PawnActionController __pawnActionCtrler;
        List<PawnColliderHelper> __traceResults;
        readonly HashSet<PawnBrainController> __sentDamageBrains = new();

        protected override void OnExecute()
        {
            __pawnBrain = agent.GetComponent<PawnBrainController>();
            __pawnActionCtrler = agent.GetComponent<PawnActionController>();
            __capturedActionInstanceId = __pawnActionCtrler.currActionContext.actionInstanceId;

            if (!actionDataName.isNoneOrNull && string.IsNullOrEmpty(actionDataName.value))
                __actionData = DatasheetManager.Instance.GetActionData(__pawnBrain.PawnBB.common.pawnId, actionDataName.value);
            else
                __actionData = __pawnActionCtrler.currActionContext.actionData;

            Debug.Assert(__actionData != null);

            if (resetTraceNames.value)
                __pawnActionCtrler.ClearTraceNames();

            if (!traceLayerNames.isNull && traceLayerNames.value.Length > 0)
                __pawnActionCtrler.AddTraceLayerNames(traceLayerNames.value);
            if (!tracePawnNames.isNull && tracePawnNames.value.Length > 0)
                __pawnActionCtrler.AddTracePawnNames(tracePawnNames.value);

            //* Trace 활성화
            __pawnActionCtrler.SetTraceRunning(true);

            if (traceSampleNum.value > 1)
            {
                __traceDuration = traceDuration.value > 0f ? traceDuration.value : 1f / __pawnActionCtrler.currActionContext.animClipFps * traceFrames.value;
                __traceDuration /= __pawnActionCtrler.currActionContext.actionSpeed;
                Debug.Assert(__traceDuration > 0f);

                __sampleIndex = 0;
                __sampleNum = traceSampleNum.value;
                __sampleInterval = __traceDuration / (__sampleNum - 1);
                __halfFanAngle = 0.5f * fanAngle.value;
                __stepFanAngle = fanAngle.value / __sampleNum;
                __traceResults = null;
                __sentDamageBrains.Clear();

                if (TraceSampleInternal() >= __sampleNum)
                    EndAction(true);
            }
            else
            {
                //* Duration 없는 단발성 Trace인 경우엔 Trace 진행 방향은 필요없음
                traceDirection.value = 0;
                __sampleNum = 1;
                __traceResults = null;
                __sentDamageBrains.Clear();

                TraceSampleInternal();
                EndAction(true);
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (__pawnActionCtrler.currActionContext.actionData == null || __capturedActionInstanceId != __pawnActionCtrler.currActionContext.actionInstanceId)
                EndAction(false);
            else if (__sampleInterval <= Time.time - __lastSampleTimeStamp && TraceSampleInternal() >= __sampleNum)
                EndAction(true);
        }

        int TraceSampleInternal()
        {   
            if (traceDirection.value == 0 || __sampleNum == 1)
            {
                __traceResults = __pawnActionCtrler.TraceActionTargets(offset.value, pitchYawRoll.value, fanRadius.value, fanAngle.value, fanHeight.value, minRadius.value, maxTargetNum.value, false, drawGizmos, drawGizmosDuration);
            }
            else
            {
                var sampleYaw =  (traceDirection.value > 0 ? 1f : -1f) * ((__sampleIndex + 0.5f) * __stepFanAngle - __halfFanAngle);
                var fanMatrix = Matrix4x4.TRS(offset.value + __pawnBrain.coreColliderHelper.pawnCollider.bounds.center - __pawnBrain.coreColliderHelper.transform.position, Quaternion.Euler(pitchYawRoll.value) * Quaternion.Euler(0f, sampleYaw, 0f), Vector3.one);
                __traceResults = __pawnActionCtrler.TraceActionTargets(fanMatrix, fanRadius.value, __stepFanAngle, fanHeight.value, minRadius.value, maxTargetNum.value, false, drawGizmos, drawGizmosDuration);
            }

            if (__sampleNum == 1)
            {
                foreach (var r in __traceResults)
                    __pawnBrain.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(__pawnBrain, r.pawnBrain, __actionData, r.pawnCollider, __pawnActionCtrler.currActionContext.insufficientStamina));

                return 1;
            }
            else
            {
                foreach (var r in __traceResults)
                {
                    if (!__sentDamageBrains.Contains(r.pawnBrain))
                    {
                        __pawnBrain.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(__pawnBrain, r.pawnBrain, __actionData, r.pawnCollider, __pawnActionCtrler.currActionContext.insufficientStamina));
                        __sentDamageBrains.Add(r.pawnBrain);
                    }
                }

                __lastSampleTimeStamp = Time.time;
                return ++__sampleIndex;
            }
        }

        protected override void OnStop()
        {
            base.OnStop();

            //* Trace 비활성화
            __pawnActionCtrler.SetTraceRunning(false);

            //* GC 될수도 있으니 Task 종료시에 바로 해제해줌
            __sentDamageBrains.Clear();
            __traceResults.Clear();
            __traceResults = null;
        }
    }

    public class SendDamage : ActionTask
    {
        public BBParameter<string> actionDataName;
        public BBParameter<GameObject> actionTarget;

        protected override void OnExecute()
        {
            var pawnBrain = agent.GetComponent<PawnBrainController>();
            var pawnActionCtrler = agent.GetComponent<PawnActionController>();

            var actionData = pawnActionCtrler.currActionContext.actionData;
            if (!actionDataName.isNoneOrNull && !string.IsNullOrEmpty(actionDataName.value))
                actionData = DatasheetManager.Instance.GetActionData(pawnBrain.PawnBB.common.pawnId, actionDataName.value);

            Debug.Assert(actionData != null);
            Debug.Assert(!actionTarget.isNoneOrNull);

            var targetBrain = actionTarget.value.GetComponent<PawnBrainController>();
            Debug.Assert(targetBrain != null);

            pawnBrain.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(pawnBrain, targetBrain, actionData, null, false));
            
            EndAction(true);
        }
    }

    public class EmitProjectile : ActionTask
    {
        protected override string info => emitSource.isNoneOrNull ? base.info : $"Emit '{emitSource.name}' x <b>{emitNum.value}</b>";
        public BBParameter<GameObject> emitSource;
        public BBParameter<Transform> emitPoint;
        public BBParameter<int> emitNum = 1;

        protected override void OnExecute()
        {
            var actionCtrler = agent.GetComponent<PawnActionController>();
            Debug.Assert(actionCtrler != null);

            actionCtrler.EmitProjectile(emitSource.value, emitPoint.value, emitNum.value);
            EndAction(true);
        }
    }

    public class TriggerAnim : ActionTask
    {
        protected override string info => string.IsNullOrEmpty(triggerName.value) ? base.info : $"Trigger <b>{triggerName.value}</b>";
        public BBParameter<Animator> animator;
        public BBParameter<string> triggerName;
        protected override void OnExecute()
        {
            animator.value.SetTrigger(triggerName.value);
            EndAction(true);
        }
    }

    public class TriggerAnimAB : ActionTask
    {
        public BBParameter<Animator> animator;
        public BBParameter<bool> condition;
        public BBParameter<string> triggerNameA;
        public BBParameter<string> triggerNameB;
        protected override void OnExecute()
        {
            animator.value.SetTrigger(condition.value? triggerNameA.value : triggerNameB.value);
            EndAction(true);
        }
    }

    public class SetAnimBool : ActionTask
    {
        public BBParameter<Animator> animator;
        public BBParameter<string> paramId;
        public BBParameter<bool> newValue;
        protected override void OnExecute()
        {
            animator.value.SetBool(paramId.value, newValue.value);
            EndAction(true);
        }
    }

    public class SetAnimInteger : ActionTask
    {
        protected override string info => string.IsNullOrEmpty(paramId.value) ? base.info : $"Set <b>{paramId.value}</b> Param <b>{newValue.value}</b>";
        public BBParameter<Animator> animator;
        public BBParameter<string> paramId;
        public BBParameter<int> newValue;
        protected override void OnExecute()
        {
            animator.value.SetInteger(paramId.value, newValue.value);
            EndAction(true);
        }
    }

    public class SetAnimFloat : ActionTask
    {
        protected override string info => $"Set <b>{paramId.value}</b> Param <b>{newValue.value}</b>";
        public BBParameter<Animator> animator;
        public BBParameter<string> paramId;
        public BBParameter<float> newValue;
        
        protected override void OnExecute()
        {
            animator.value.SetFloat(paramId.value, newValue.value);
            EndAction(true);
        }
    }

    // 상대의 액션 셋팅
    public class TargetAction : ActionTask
    {
        protected override string info => $"Target Action";
        public BBParameter<bool> isKnockDown;

        protected override void OnExecute()
        {
            if (isKnockDown.value == true) 
            {
                var brain = agent.GetComponent<PawnBrainController>();
                brain.TargetAction(ActionType.Knockback);
            }
            EndAction(true);
        }
    }

    public class SetTweenSelector : ActionTask
    {
        protected override string info 
        {
            get 
            {
                if (classes.isNoneOrNull) 
                    return (resetClasses.value || resetStates.value) ? "Clear Tween" : base.info;

                if (states.isNoneOrNull)
                    return $"Set Tween .<b>{(classes.value.Length > 0 ? classes.value[0] : string.Empty)}</b>";
                else
                    return $"Set Tween .<b>{(classes.value.Length > 0 ? classes.value[0] : string.Empty)}</b>:<b>{(states.value.Length > 0 ? states.value[0] : string.Empty)}</b>";
            }
        }

        public BBParameter<TweenSelector> selector;
        public BBParameter<string[]> classes;
        public BBParameter<string[]> states;
        public BBParameter<bool> resetClasses;
        public BBParameter<bool> resetStates;

        protected override void OnExecute()
        {
            if (resetClasses.value)
                selector.value.query.activeClasses.Clear();
            if (resetStates.value)
                selector.value.query.activeStates.Clear();

            if (!classes.isNoneOrNull)
            {
                foreach (var c in classes.value)
                    selector.value.query.activeClasses.Add(c);
            }

            if (!states.isNoneOrNull)
            {
                foreach (var s in states.value)
                    selector.value.query.activeStates.Add(s);
            }

            selector.value.query.Apply();
            EndAction(true);
        }
    }

    public class SetBoolReactiveProperty : ActionTask
    {
        protected override string info => string.IsNullOrEmpty(property.name) ? base.info : $"Set '{property.name}' <b>{newValue.value}</b>";
        public BBParameter<BoolReactiveProperty> property;
        public BBParameter<bool> newValue;

        protected override void OnExecute()
        {
            property.value.Value = newValue.value;
            EndAction(true);
        }
    }

    public class SetIntReactiveProperty : ActionTask
    {
        protected override string info => string.IsNullOrEmpty(property.name) ? base.info : $"Set '{property.name}' <b>{newValue.value}</b>";
        public BBParameter<IntReactiveProperty> property;
        public BBParameter<int> newValue;

        protected override void OnExecute()
        {
            property.value.Value = newValue.value;
            EndAction(true);
        }
    }

    public class SetFloatReactiveProperty : ActionTask
    {
        protected override string info => string.IsNullOrEmpty(property.name) ? base.info : $"Set '{property.name}' <b>{newValue.value}</b>";
        public BBParameter<FloatReactiveProperty> property;
        public BBParameter<float> newValue;

        protected override void OnExecute()
        {
            property.value.Value = newValue.value;
            EndAction(true);
        }
    }

    public class WaitBoolReactiveProperty : ActionTask
    {
        protected override string info =>  $"Wait '{property.name}' being <b>{checkValue.value}</b>";
        public BBParameter<BoolReactiveProperty> property;
        public BBParameter<bool> checkValue;
        PawnActionController __pawnActionCtrler;

        protected override void OnExecute()
        {
            base.OnExecute();
            __pawnActionCtrler = agent.GetComponent<PawnActionController>();
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (__pawnActionCtrler.currActionContext.actionData == null)
                EndAction(false);
            else if (property.value.Value == checkValue.value)
                EndAction(true);
        }
    }

    public class WaitIntReactiveProperty : ActionTask
    {
        protected override string info =>  $"Wait '{property.name}' being <b>{checkValue.value}</b>";
        public BBParameter<IntReactiveProperty> property;
        public BBParameter<int> checkValue;

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (property.value.Value == checkValue.value)
                EndAction(true);
        }
    }

    public class WaitFloatReactiveProperty : ActionTask
    {
        protected override string info =>  $"Wait '{property.name}' being <b>{checkValue.value}</b>";
        public BBParameter<FloatReactiveProperty> property;
        public BBParameter<float> checkValue;

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (property.value.Value == checkValue.value)
                EndAction(true);
        }
    }
    
    public class SetJellyMeshScale : ActionTask
    {
        protected override string info =>  $"Set JellyMesh Scale <b>{newScale.value}</b>";
        public BBParameter<JellySpringMassSystem> springMassSystem;
        public BBParameter<Vector3> newScale;

        protected override void OnExecute()
        {
            springMassSystem.value.SetScale(newScale.value.x, newScale.value.y, newScale.value.z);
            EndAction(true);
        }
    }

    public class SetSuperArmor : ActionTask
    {   
        protected override string info => newValue.value ? "Set SuperArmor <b>On</b>" : "Set SuperArmor <b>Off</b>";
        public BBParameter<bool> newValue;
        protected override void OnExecute()
        {
            if (agent.TryGetComponent<PawnActionController>(out var actionCtrler) && actionCtrler.currActionContext.actionData != null)
                actionCtrler.SetSuperArmorEnabled(newValue.value);
            EndAction(true);
        }
    }

    public class SetParrying : ActionTask
    {   
        protected override string info => newValue.value ? "Set Parrying <b>On</b>" : "Set Parrying <b>Off</b>";
        public BBParameter<bool> newValue;
        protected override void OnExecute()
        {
            if (agent.TryGetComponent<PawnActionController>(out var actionCtrler) && actionCtrler.currActionContext.actionData != null)
                actionCtrler.SetActiveParryingEnabled(newValue.value);
            EndAction(true);
        }
    }

    public class AddBuff : ActionTask
    {
        protected override string info => duration.value < 0 ? $"<b>{buffType.value}</b> On" : $"<b>{buffType.value}</b> On for <b>{duration.value}</b> secs";
        public BBParameter<BuffTypes> buffType;
        public BBParameter<float> strength;
        public BBParameter<float> duration;
        protected override void OnExecute()
        {
            if (agent.TryGetComponent<PawnActionController>(out var actionCtrler))
                (actionCtrler as IBuffContainer).AddBuff(buffType.value, strength.value, duration.value);

            EndAction(true);
        }
    }

    public class RemoveBuff : ActionTask
    {
        protected override string info => $"<b>{buffType.value}</b> Off";
        public BBParameter<BuffTypes> buffType;
        protected override void OnExecute()
        {
            if (agent.TryGetComponent<PawnActionController>(out var actionCtrler))
                (actionCtrler as IBuffContainer).RemoveBuff(buffType.value);

            EndAction(true);
        }
    }
    
    public class ShowFX : ActionTask
    {
        protected override string info => $"Show FX <b>{(fxPrefab.isNoneOrNull ? fxName.value : fxPrefab)}</b>";
        public BBParameter<string> fxName;
        public BBParameter<GameObject> fxPrefab;
        public BBParameter<Transform> localToWorld;
        public BBParameter<Vector3> position;
        public BBParameter<Vector3> pitchYawRoll;
        public BBParameter<Vector3> scale = Vector3.one;
        public BBParameter<ParticleSystemScalingMode> scalingMode = ParticleSystemScalingMode.Local;
        public BBParameter<float> duration = -1f;
        public BBParameter<string[]> childNameToBeHidden;
        public bool attachToTransform = false;
        public bool stopWhenActionCanceled = true;
        PawnActionController __pawnActionCtrler;
        EffectInstance __fxInstance;
        int __capturedActionInstanceId;

        protected override void OnExecute()
        {
            if (agent.TryGetComponent<PawnActionController>(out var __pawnActionCtrler))
            {
                if (!localToWorld.isNoneOrNull)
                {
                    var localToWorldMatrix = Matrix4x4.TRS(localToWorld.value.position, localToWorld.value.rotation, Vector3.one) * Matrix4x4.TRS(position.value, Quaternion.Euler(pitchYawRoll.value), Vector3.one);
                    if (fxPrefab.isNoneOrNull)
                        __fxInstance = EffectManager.Instance.Show(fxName.value, localToWorldMatrix.GetPosition(), localToWorldMatrix.rotation, scale.value, duration.value, 0f, scalingMode.value);
                    else
                        __fxInstance = EffectManager.Instance.Show(fxPrefab.value, localToWorldMatrix.GetPosition(), localToWorldMatrix.rotation, scale.value, duration.value, 0f, scalingMode.value);
                }
                else
                {
                    if (fxPrefab.isNoneOrNull)
                        __fxInstance = EffectManager.Instance.Show(fxName.value, position.value, Quaternion.Euler(pitchYawRoll.value), scale.value, duration.value, 0f, scalingMode.value);
                    else
                        __fxInstance = EffectManager.Instance.Show(fxPrefab.value, position.value, Quaternion.Euler(pitchYawRoll.value), scale.value, duration.value, 0f, scalingMode.value);
                }

                if (attachToTransform)
                {
                    Debug.Assert(!localToWorld.isNoneOrNull);
                    __fxInstance.transform.SetParent(localToWorld.value, true);
                }

                if (!childNameToBeHidden.isNoneOrNull && childNameToBeHidden.value.Length > 0)
                {
                    for (int i = 0; i < __fxInstance.transform.childCount; i++)
                    {
                        var child = __fxInstance.transform.GetChild(i);
                        for (int j = 0; j < childNameToBeHidden.value.Length; j++)
                        {
                            if (childNameToBeHidden.value[j] == child.name)
                            {
                                child.gameObject.SetActive(false);
                                break;
                            }
                        }
                    }
                }

                __capturedActionInstanceId = __pawnActionCtrler.currActionContext.actionInstanceId;

                if (stopWhenActionCanceled)
                {
                    Observable.Timer(TimeSpan.FromSeconds(duration.value))
                        .TakeWhile(_ => __pawnActionCtrler.CheckActionRunning() && __pawnActionCtrler.currActionContext.actionInstanceId == __capturedActionInstanceId)
                        .DoOnCancel(() => __fxInstance?.gameObject.SetActive(false))
                        .Subscribe().AddTo(agent);
                }
            }

            EndAction(true);
        }
    }

    public class ShowTrailFX : ActionTask
    {
        protected override string info => $"Show Trail-FX <b>{(trailFx.isNoneOrNull ? trailFx.value : string.Empty)}</b>";
        public BBParameter<XWeaponTrail> trailFx;
        public BBParameter<Transform> startPoint;
        public BBParameter<Transform> endPoint;
        public BBParameter<float> duration = -1f;
        public bool stopWhenActionCanceled = true;
        PawnActionController __pawnActionCtrler;
        int __capturedActionInstanceId;

        protected override void OnExecute()
        {
            if (agent.TryGetComponent<PawnActionController>(out var __pawnActionCtrler))
            {
                if (!startPoint.isNoneOrNull)
                    trailFx.value.PointStart = startPoint.value;
                if (!endPoint.isNoneOrNull)
                    trailFx.value.PointEnd = endPoint.value;

                trailFx.value.Activate();

                __capturedActionInstanceId = __pawnActionCtrler.currActionContext.actionInstanceId;

                if (stopWhenActionCanceled)
                {
                    Observable.Timer(TimeSpan.FromSeconds(duration.value))
                        .TakeWhile(_ => __pawnActionCtrler.CheckActionRunning() && __pawnActionCtrler.currActionContext.actionInstanceId == __capturedActionInstanceId)
                        .DoOnCancel(() => trailFx.value.Deactivate())
                        .DoOnCompleted(() => trailFx.value.Deactivate())
                        .Subscribe().AddTo(agent);
                }
                else
                {
                    Observable.Timer(TimeSpan.FromSeconds(duration.value))
                        .Subscribe(_ => trailFx.value.Deactivate()).AddTo(agent);
                }
            }

            EndAction(true);
        }
    }

    public class ShowBladeTrail : ActionTask
    {
        protected override string info => $"ShowBladeTrail : {trailIndex}";
        public BBParameter<int> trailIndex;

        protected override void OnExecute()
        {
            var brain = agent.GetComponent<PawnBrainController>();
            brain.ShowTrail(true, trailIndex.value);

            EndAction(true);
        }
    }

    public class HideBladeTrail : ActionTask
    {
        protected override string info => $"HideBladeTrail : {trailIndex}";
        public BBParameter<int> trailIndex;

        protected override void OnExecute()
        {
            var brain = agent.GetComponent<PawnBrainController>();
            brain.ShowTrail(false, trailIndex.value);

            EndAction(true);
        }
    }

    public class PlaySound : ActionTask 
    {
        public BBParameter<SoundID> soundId;
        public BBParameter<string> soundPath;

        protected override void OnExecute()
        {
            SoundManager.Instance.Play(soundId.value);

            EndAction(true);
        }
    }
    
    public class CameraAction : ActionTask
    {
        public BBParameter<bool> isZoom;
        public BBParameter<float> zoomValue = 1.0f;
        public BBParameter<float> zoomDuration = 0.5f;

        protected override void OnExecute()
        {
            if (isZoom.value == true) 
            {
                float duration = zoomDuration.value;
                if (duration <= 0)
                    duration = 0.1f;

                GameContext.Instance.cameraCtrler.InterpolateZoom(zoomValue.value, duration);
            }
            EndAction(true);
        }
    }

    public class CallFunction : ActionTask
    {
        public BBParameter<string> funcName;

        protected override void OnExecute()
        {
            if (funcName.value != "")
                agent.SendMessage(funcName.value, SendMessageOptions.DontRequireReceiver);

            EndAction(true);
        }
    }
}