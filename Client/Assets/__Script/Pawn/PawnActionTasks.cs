using System;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Retween.Rx;
using UniRx;
using UnityEngine;
using XftWeapon;

namespace Game.NodeCanvasExtension
{
    [Category("Pawn")]
    public class CheckActionRunning : ConditionTask
    {
        protected override string info => "CheckActionRunning() == " + (invert ? "False" : "True");
        protected override bool OnCheck() 
        {
            return agent.GetComponent<PawnActionController>().CheckActionRunning();
        }
    }

    [Category("Pawn")]
    public class CheckActionPending : ConditionTask
    {
        protected override string info => "CheckActionPending() == " + (invert ? "False" : "True");
        protected override bool OnCheck() 
        {
            return agent.GetComponent<PawnActionController>().CheckActionPending();
        }
    }

    [Category("Pawn")]
    public class CanStartPendingAction : ConditionTask
    {
        protected override string info => "CanStartPendingAction() == " + (invert ? "False" : "True");
        protected override bool OnCheck() 
        {
            return agent.GetComponent<PawnActionController>().CanStartPendingAction();
        }
    }

    [Category("Pawn")]
    public class CheckActionHasPreMotion : ConditionTask
    {
        protected override string info => "CheckActionHasPreMotion() == " + (invert ? "False" : "True");
        protected override bool OnCheck() 
        {
            return agent.GetComponent<PawnActionController>().CheckPendingActionHasPreMotion();
        }
    }

    [Category("Pawn")]
    public class FreePosition : ActionTask
    {
        public BBParameter<float> duration = -1;
        public bool notifyDecisionFinished = false;
        PawnBrainController __pawnBrain;
        float __executeTimeStamp;

        protected override void OnExecute()
        {
            __pawnBrain = agent.GetComponent<PawnBrainController>();
            Debug.Assert(__pawnBrain != null);
            
            __executeTimeStamp = Time.time;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (duration.value > 0 && (Time.time - __executeTimeStamp) > duration.value)
                EndAction(true);
        }

        protected override void OnStop(bool interrupted)
        {
            base.OnStop(interrupted);

            if (!interrupted && notifyDecisionFinished)
                __pawnBrain.OnDecisionFinishedHandler();
        }
    }

    [Category("Pawn")]
    public class HoldPosition : ActionTask
    {
        public BBParameter<Transform> lookAt;
        public BBParameter<float> duration = -1;
        public bool notifyDecisionFinished = false;
        PawnBrainController __pawnBrain;
        IPawnMovable __pawnMovable;
        float __executeTimeStamp;

        protected override void OnExecute()
        {
            __pawnBrain = agent.GetComponent<PawnBrainController>();
            Debug.Assert(__pawnBrain != null);

            __pawnMovable = __pawnBrain as IPawnMovable;
            Debug.Assert(__pawnMovable != null);
            
            __pawnMovable.Stop();
            __executeTimeStamp = Time.time;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            if (!lookAt.isNoneOrNull)
                __pawnMovable.SetDestination(lookAt.value.position);

            if (duration.value > 0 && (Time.time - __executeTimeStamp) > duration.value)
                EndAction(true);
        }

        protected override void OnStop(bool interrupted)
        {
            base.OnStop(interrupted);

            if (!interrupted && notifyDecisionFinished)
                __pawnBrain.OnDecisionFinishedHandler();
        }
    }

    [Category("Pawn")]
    public class MoveTo : ActionTask
    {
        public BBParameter<Vector3> destination;
        IDisposable __moveDisposable;

        protected override void OnExecute()
        {
            var pawnMovable = agent.GetComponent<PawnBrainController>() as IPawnMovable;
            Debug.Assert(pawnMovable != null);

            pawnMovable.MoveTo(destination.value);
            __moveDisposable = Observable.EveryUpdate()
                .TakeWhile(_ => !pawnMovable.CheckReachToDestination())
                .TakeUntil(Observable.Timer(TimeSpan.FromSeconds(pawnMovable.GetEstimateTimeToDestination() + 1)))
                .TakeLast(1)
                .DoOnCancel(() =>
                {
                    pawnMovable.Stop();
                    EndAction(true);
                })
                .DoOnCompleted(() =>
                {
                    pawnMovable.Stop();
                    EndAction(true);
                })
                .Subscribe().AddTo(agent);
        }

        protected override void OnStop(bool interrupted)
        {
            base.OnStop(interrupted);

            if (interrupted && __moveDisposable != null)
                __moveDisposable.Dispose();
        }
    }

    [Category("Pawn")]
    public class MoveAround : ActionTask
    {
        public BBParameter<float> maxTurnAngle = 90;
        public BBParameter<float> moveDistance = 1;
        IDisposable __moveDisposable;

        protected override void OnExecute()
        {
            var pawnBrain = agent.GetComponent<PawnBrainController>();
            Debug.Assert(pawnBrain != null);

            var pawnMovable = pawnBrain as IPawnMovable;
            Debug.Assert(pawnMovable != null);

            var targetPoint = agent.GetComponent<PawnSensorController>().GetRandomPoint(pawnBrain.coreColliderHelper.transform.forward, maxTurnAngle.value, moveDistance.value);
            pawnMovable.SetDestination(targetPoint);

            __moveDisposable = Observable.EveryUpdate()
                .TakeWhile(_ => !pawnMovable.CheckReachToDestination())
                .TakeUntil(Observable.Timer(TimeSpan.FromSeconds(pawnMovable.GetEstimateTimeToDestination() + 1)))
                .TakeLast(1)
                .DoOnCancel(() =>
                {
                    pawnMovable.Stop();
                    EndAction(true);
                })
                .DoOnCompleted(() =>
                {
                    pawnMovable.Stop();
                    EndAction(true);
                })
                .Subscribe().AddTo(agent);
        }

        protected override void OnStop(bool interrupted)
        {
            base.OnStop(interrupted);

            if (interrupted && __moveDisposable != null)
                __moveDisposable.Dispose();
        }
    }

    [Category("Pawn")]
    public class Approach : ActionTask
    {
        public BBParameter<Transform> target;
        public BBParameter<float> approachDistance;
        public BBParameter<float> duration = -1;
        public bool shouldRotateToTarget = true;
        public bool ignoreTargetRadius;
        public bool stopOnReachToDestination;
        public bool notifyDecisionFinished;

        PawnBrainController __targetBrain;
        PawnBrainController __pawnBrain;
        PawnActionController __pawnActionCtrler;
        IPawnMovable __pawnMovable;
        float __targetCapsuleRadius;
        float __executeTimeStamp;

        protected override void OnExecute()
        {
            if (target.value == null) 
            {
                EndAction(true);
                return;
            }

            __targetBrain = target.value.GetComponent<PawnBrainController>();
            __targetCapsuleRadius = __targetBrain != null && __targetBrain.coreColliderHelper.GetCapsuleCollider() != null ? __targetBrain.coreColliderHelper.GetCapsuleCollider().radius : 0f;

            __pawnBrain = agent.GetComponent<PawnBrainController>();
            Debug.Assert(__pawnBrain != null);

            __pawnActionCtrler = __pawnBrain.GetComponent<PawnActionController>();
            Debug.Assert(__pawnActionCtrler != null);

            __pawnMovable = __pawnBrain as IPawnMovable;
            Debug.Assert(__pawnMovable != null);
            
            __pawnMovable.FreezeRotation(!shouldRotateToTarget);
            __executeTimeStamp = Time.time;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            __pawnMovable.SetMinApproachDistance(approachDistance.value + (ignoreTargetRadius ? 0f : __targetCapsuleRadius));
            if (__pawnActionCtrler == null || !__pawnActionCtrler.CheckActionRunning())
                __pawnMovable.SetDestination(__targetBrain != null ? __targetBrain.coreColliderHelper.transform.position : target.value.position);

            if (duration.value > 0 && Time.time - __executeTimeStamp > duration.value)
                EndAction(true);
            else if (__targetBrain != null && __targetBrain.PawnBB.IsDead)
                EndAction(true);
            else if (stopOnReachToDestination && __pawnMovable.CheckReachToDestination())
                EndAction(true);
        }

        protected override void OnStop(bool interrupted)
        {
            base.OnStop(interrupted);
            
            if (__pawnMovable != null)
            {
                __pawnMovable.Stop();
                __pawnMovable.FreezeRotation(false);
            }

            if (!interrupted && notifyDecisionFinished && __pawnBrain != null)
                __pawnBrain.OnDecisionFinishedHandler();
        }
    }

    [Category("Pawn")]
    public class Spacing : ActionTask
    {
        public BBParameter<Transform> target;
        public BBParameter<float> minDistance = 1;
        public BBParameter<float> maxDistance = 1;
        public BBParameter<float> outDistance = 1;
        public BBParameter<float> duration = -1;
        public bool shouldRotateToTarget = true;
        public bool notifyDecisionFinished = false;
        PawnBrainController __targetBrain;
        PawnBrainController __pawnBrain;
        IPawnMovable __pawnMovable;
        IDisposable __moveStrafeDisposable;
        Vector3 __strafeMoveVec;
        float __strafeDuration;
        float __targetCapsuleRadius;
        float __minApproachDistance;
        float __executeTimeStamp;

        protected override void OnExecute()
        {
            __targetBrain = target.value.GetComponent<PawnBrainController>();
            __targetCapsuleRadius = (__targetBrain != null && __targetBrain.coreColliderHelper.GetCapsuleCollider() != null) ? __targetBrain.coreColliderHelper.GetCapsuleCollider().radius : 0f;
            
            __pawnBrain = agent.GetComponent<PawnBrainController>();
            Debug.Assert(__pawnBrain != null);
             
            __pawnMovable = __pawnBrain as IPawnMovable;
            Debug.Assert(__pawnMovable != null);
            
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
            else if (__targetBrain != null && __targetBrain.PawnBB.IsDead)
                EndAction(true);
            else if ((__targetBrain != null ? __pawnBrain.coreColliderHelper.GetDistanceBetween(__targetBrain.coreColliderHelper) : __pawnBrain.GetWorldPosition().Distance2D(target.value.position))  > outDistance.value)
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
                        var newDestination = __pawnBrain.GetWorldPosition() + __minApproachDistance * 2f * (__pawnBrain.GetWorldRotation() * __strafeMoveVec).Vector2D().normalized;
                        if (__targetBrain != null)
                            newDestination = __targetBrain.GetWorldPosition() + (spacingDistance + __targetCapsuleRadius) * (newDestination - __targetBrain.GetWorldPosition()).Vector2D().normalized;
                        else
                            newDestination = target.value.position + spacingDistance * (newDestination - target.value.position).Vector2D().normalized;

                        __pawnMovable.SetDestination(newDestination);
                    }
                    
                    if (shouldRotateToTarget)
                        __pawnMovable.SetFaceVector(((__targetBrain != null ? __targetBrain.GetWorldPosition() : target.value.position) - __pawnBrain.GetWorldPosition()).Vector2D().normalized);
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


    [Category("Pawn")]
    public class Away : ActionTask
    {
        public BBParameter<Transform> target;
        public BBParameter<float> minDistance = 1;
        public BBParameter<float> maxDistance = 1;
        public BBParameter<float> outDistance = 1;
        public BBParameter<float> duration = -1;
        public bool shouldRotateToTarget = true;
        public bool notifyDecisionFinished = false;
        PawnBrainController __targetBrain;
        PawnBrainController __pawnBrain;
        IPawnMovable __pawnMovable;
        IDisposable __moveStrafeDisposable;
        Vector3 __strafeMoveVec;
        float __strafeDuration;
        float __targetCapsuleRadius;
        float __minApproachDistance;
        float __executeTimeStamp;

        protected override void OnExecute()
        {
            __targetBrain = target.value.GetComponent<PawnBrainController>();
            __targetCapsuleRadius = (__targetBrain != null && __targetBrain.coreColliderHelper.GetCapsuleCollider() != null) ? __targetBrain.coreColliderHelper.GetCapsuleCollider().radius : 0f;
            
            __pawnBrain = agent.GetComponent<PawnBrainController>();
            Debug.Assert(__pawnBrain != null);
             
            __pawnMovable = __pawnBrain as IPawnMovable;
            Debug.Assert(__pawnMovable != null);
            
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
            else if (__targetBrain != null && __targetBrain.PawnBB.IsDead)
                EndAction(true);
            else if ((__targetBrain != null ? __pawnBrain.coreColliderHelper.GetDistanceBetween(__targetBrain.coreColliderHelper) : __pawnBrain.GetWorldPosition().Distance2D(target.value.position))  > outDistance.value)
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
                        var newDestination = __pawnBrain.GetWorldPosition() + __minApproachDistance * 2f * (__pawnBrain.GetWorldRotation() * __strafeMoveVec).Vector2D().normalized;
                        if (__targetBrain != null)
                            newDestination = __targetBrain.GetWorldPosition() + (spacingDistance + __targetCapsuleRadius) * (newDestination - __targetBrain.GetWorldPosition()).Vector2D().normalized;
                        else
                            newDestination = target.value.position + spacingDistance * (newDestination - target.value.position).Vector2D().normalized;

                        __pawnMovable.SetDestination(newDestination);
                    }
                    
                    if (shouldRotateToTarget)
                        __pawnMovable.SetFaceVector(((__targetBrain != null ? __targetBrain.GetWorldPosition() : target.value.position) - __pawnBrain.GetWorldPosition()).Vector2D().normalized);
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

    [Category("Pawn")]
    public class StartPendingAction : ActionTask
    {
        protected override string info => $"Start Action <b>{actionName.value}</b>";
        public BBParameter<string> actionName;
        public BBParameter<bool> manualAdvanceEnabled;
        public BBParameter<float> animSpeedMultiplier = 1f;
        public BBParameter<float> animBlendSpeed = 1f;
        public BBParameter<float> animClipLength = -1f;
        public BBParameter<int> animClipFps = -1;
        public BBParameter<float> rootMotionMultiplier = 1f;
        public BBParameter<AnimationCurve> rootMotionCurve;
        public BBParameter<RootMotionConstraints[]> rootMotionConstraints;

        protected override void OnExecute()
        {
            var actionCtrler = agent.GetComponent<PawnActionController>();
            Debug.Assert(actionCtrler != null);

            if (actionCtrler.PendingActionData.Item1 == actionName.value)
            {
                var rootMotionConstrainSum = 0;
                if (!rootMotionConstraints.isNoneOrNull && rootMotionConstraints.value.Length > 0)
                {
                    foreach (var c in rootMotionConstraints.value)
                        rootMotionConstrainSum |= (int)c;
                }

                EndAction(actionCtrler.StartAction(actionName.value, string.Empty, animBlendSpeed.value, animSpeedMultiplier.value, rootMotionMultiplier.value, rootMotionConstrainSum, rootMotionCurve.value, manualAdvanceEnabled.value));
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

    [Category("Pawn")]
    public class StartJumping : ActionTask
    {
        public BBParameter<float> jumpHeight;

        protected override void OnExecute()
        {
            if (agent.TryGetComponent<HeroMovement>(out var movement))
                movement.StartJump(jumpHeight.value);
            else
                Debug.Assert(false);

            EndAction(true);
        }
    }

    [Category("Pawn")]
    public class FinishJumping : ActionTask
    {
        protected override void OnExecute()
        {
            if (agent.TryGetComponent<HeroMovement>(out var movement))
                movement.FinishJump();
            else
                Debug.Assert(false);

            EndAction(true);
        }
    }

    [Category("Pawn")]
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

    [Category("Pawn")]
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
            if (!__pawnActionCtrler.CheckActionRunning() || __capturedActionInstanceId != __pawnActionCtrler.currActionContext.actionInstanceId)
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

    [Category("Pawn")]
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
            if (!__pawnActionCtrler.CheckActionRunning() || __capturedActionInstanceId != __pawnActionCtrler.currActionContext.actionInstanceId)
            {
                EndAction(false);
                return;
            }

            if (__pawnActionCtrler.currActionContext.manualAdvanceEnabled)
            {
                var baseTimeStamp = Time.time - __pawnActionCtrler.currActionContext.manualAdvanceTime;
                var waitTime = waitFrame.value > 0 ? 1f / __pawnActionCtrler.currActionContext.animClipFps * Mathf.Max(0, waitFrame.value - 1) : __pawnActionCtrler.currActionContext.animClipLength;
                if (!__pawnActionCtrler.CheckWaitAction(waitTime, baseTimeStamp))
                    EndAction(true);
            }
            else
            {
                //* 'preMotionTimeStamp'값이 있으면. 실제 액션 시작 시간은 preMotionTimeStamp으로 간주함
                var baseTimeStamp = __pawnActionCtrler.currActionContext.preMotionTimeStamp > 0f ? __pawnActionCtrler.currActionContext.preMotionTimeStamp : __pawnActionCtrler.currActionContext.startTimeStamp;
                var waitTime = waitFrame.value > 0 ? 1f / __pawnActionCtrler.currActionContext.animClipFps * Mathf.Max(0, waitFrame.value - 1) : __pawnActionCtrler.currActionContext.animClipLength;
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

    [Category("Pawn")]
    public class WaitActionDisposable : ActionTask
    {
        PawnActionController __pawnActionCtrler;
        int __capturedActionInstanceId;

        protected override void OnExecute()
        {
            __pawnActionCtrler = agent.GetComponent<PawnActionController>();
            __capturedActionInstanceId =  __pawnActionCtrler.currActionContext.actionInstanceId;
        }

        protected override void OnUpdate()
        {
            if (!__pawnActionCtrler.CheckActionRunning() || __capturedActionInstanceId != __pawnActionCtrler.currActionContext.actionInstanceId)
                EndAction(false);
            if (__pawnActionCtrler.currActionContext.actionDisposable == null)
                EndAction(true);
        }
    }

    [Category("Pawn")]
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

            if (__pawnActionCtrler.currActionContext.manualAdvanceEnabled)
                __manualAdvanceSpeedCached = __pawnActionCtrler.currActionContext.manualAdvanceSpeed;

            //* animStateName 값이 비어 있으면 PreMotion은 없는 것으로 간주함
            if (stateName.isNoneOrNull || string.IsNullOrEmpty(stateName.value))
                EndAction(true);

            var stateBehaviour =__pawnAnimCtrler.FindObservableStateMachineTriggerEx(stateName.value);
            Debug.Assert(stateBehaviour != null);

            __pawnActionCtrler.currActionContext.manualAdvanceSpeed = 0;
            __stateExitDisposable = stateBehaviour.OnStateExitAsObservable().Subscribe(_ => 
            {
                if (__pawnActionCtrler.currActionContext.manualAdvanceEnabled)
                    __pawnActionCtrler.currActionContext.manualAdvanceSpeed = __manualAdvanceSpeedCached;
                __pawnActionCtrler.currActionContext.preMotionTimeStamp = Time.time;
                EndAction(true);
            }).AddTo(agent);
        }

        protected override void OnUpdate()
        {
            if (!__pawnActionCtrler.CheckActionRunning() || __capturedActionInstanceId != __pawnActionCtrler.currActionContext.actionInstanceId)
                EndAction(false);
        }

        protected override void OnStop(bool interrupted)
        {
            base.OnStop(interrupted);
            
            __stateExitDisposable?.Dispose();
            __stateExitDisposable = null;
        }
    }

    [Category("Pawn")]
    public class WaitGroundHit : ActionTask
    {
        PawnMovement __pawnMovement;
        PawnActionController __pawnActionCtrler;
        int __capturedActionInstanceId;

        protected override void OnExecute()
        {
            __pawnMovement = agent.GetComponent<PawnMovement>();
            __pawnActionCtrler = agent.GetComponent<PawnActionController>();
            __capturedActionInstanceId =  __pawnActionCtrler.currActionContext.actionInstanceId;
        }

        protected override void OnUpdate()
        {
            if (!__pawnActionCtrler.CheckActionRunning() || __capturedActionInstanceId != __pawnActionCtrler.currActionContext.actionInstanceId)
                EndAction(false);
            else if (__pawnMovement.IsOnGround)
                EndAction(true);
        }
    }

    [Category("Pawn")]
    public class DelayAction : ActionTask
    {
        protected override string info => $"Delay {delayTime} secs";
        public BBParameter<float> delayTime = 1f;
        public BBParameter<float> randomRangeMin = -0.1f;
        public BBParameter<float> randomRangeMax = 0.1f;
        public BBParameter<float> animAdvanceSinusoidalAmplitude = 0.1f;
        public BBParameter<float> animAdvanceSinusoidalFrequence = 1;
        public bool resetActionStartTime;
        PawnActionController __pawnActionCtrler;
        int __capturedActionInstanceId;
        float __waitDuration;
        float __animAdvanceSinusoidal;
        float __manualAdvanceTimeCached;
        float __manualAdvanceSpeedCached;

        protected override void OnExecute()
        {
            __pawnActionCtrler = agent.GetComponent<PawnActionController>();
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

            if (!__pawnActionCtrler.CheckActionRunning() || __capturedActionInstanceId != __pawnActionCtrler.currActionContext.actionInstanceId)
            {
                EndAction(false);
            }
            else if (!__pawnActionCtrler.CheckWaitAction(__waitDuration))
            {   
                if (resetActionStartTime)
                    __pawnActionCtrler.currActionContext.startTimeStamp = Time.time;

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

    [Category("Pawn")]
    public class ImpluseRootMotion : ActionTask
    {
        public BBParameter<float> impulseStrength;
        public BBParameter<float> impulseSpeed;
        public BBParameter<float> minApproachDistance = 0.1f;
        public BBParameter<float> duration;
        public BBParameter<ParadoxNotion.Animation.EaseType> accelEase;
        public BBParameter<float> accelDuration = -1f;
        public BBParameter<ParadoxNotion.Animation.EaseType> breakEase;
        public BBParameter<float> breakDuration = -1f;
        public BBParameter<PawnColliderHelper> targetColliderHelper;
        public bool endActionWhenReachToTarget;
        IDisposable __rootMotionDisposable;
        int __capturedActionInstanceId;
        float __manualAdvanceSpeedCached;
        
        protected override void OnExecute()
        {
            var pawnBrain = agent.GetComponent<PawnBrainController>();
            Debug.Assert(pawnBrain != null);

            var actionCtrler = pawnBrain.GetComponent<PawnActionController>();
            Debug.Assert(actionCtrler != null);

            var pawnMovable = pawnBrain as IPawnMovable;
            Debug.Assert(pawnMovable != null);

            __capturedActionInstanceId =  actionCtrler.currActionContext.actionInstanceId;
            if (endActionWhenReachToTarget)
            {
                actionCtrler.WaitAction();
                __manualAdvanceSpeedCached = actionCtrler.currActionContext.manualAdvanceSpeed;
                actionCtrler.currActionContext.manualAdvanceSpeed = 0;
            }

            var executeTimeStamp = Time.time;

            actionCtrler.currActionContext.rootMotionDisposable?.Dispose();
            actionCtrler.currActionContext.rootMotionDisposable = __rootMotionDisposable = Observable.EveryUpdate()
                .TakeUntil(Observable.Timer(TimeSpan.FromSeconds(duration.value + Mathf.Max(0f, accelDuration.value) + Mathf.Max(0f, breakDuration.value))))
                .DoOnCancel(() => 
                {
                    __rootMotionDisposable = actionCtrler.currActionContext.rootMotionDisposable = null;
                    if (endActionWhenReachToTarget)
                    {
                        actionCtrler.currActionContext.manualAdvanceSpeed = __manualAdvanceSpeedCached;
                        EndAction(true);
                    }
                })
                .DoOnCompleted(() =>
                {
                    __rootMotionDisposable = actionCtrler.currActionContext.rootMotionDisposable = null;
                    if (endActionWhenReachToTarget)
                    {
                        actionCtrler.currActionContext.manualAdvanceSpeed = __manualAdvanceSpeedCached;
                        EndAction(true);
                    }
                })
                .Subscribe(_ =>
                {
                    if (endActionWhenReachToTarget)
                        actionCtrler.currActionContext.manualAdvanceSpeed = 0;

                    if (!actionCtrler.CheckActionRunning() || __capturedActionInstanceId != actionCtrler.currActionContext.actionInstanceId)
                    {
                        Debug.Assert(__rootMotionDisposable != null && __rootMotionDisposable == actionCtrler.currActionContext.rootMotionDisposable);
                        __rootMotionDisposable.Dispose();
                        __rootMotionDisposable = actionCtrler.currActionContext.rootMotionDisposable = null;

                        return;
                    }
                    else if (endActionWhenReachToTarget)
                    {
                        //* Target과 최소 접근 거리에 도달헀으면 RootMotion 적용 안함
                        if (!targetColliderHelper.isNoneOrNull && targetColliderHelper.value.GetDistanceBetween(pawnBrain.coreColliderHelper) <= minApproachDistance.value)
                        {
                            Debug.Assert(__rootMotionDisposable != null && __rootMotionDisposable == actionCtrler.currActionContext.rootMotionDisposable);
                            __rootMotionDisposable.Dispose();
                            __rootMotionDisposable = actionCtrler.currActionContext.rootMotionDisposable = null;

                            return;
                        }
                    }

                    var rootMotionSpeed = impulseSpeed.value;
                    var deltaTime = Time.time - executeTimeStamp;
                    if (deltaTime < accelDuration.value)
                        rootMotionSpeed = impulseSpeed.value * ParadoxNotion.Animation.Easing.Ease(accelEase.value, 0f, 1f, deltaTime / accelDuration.value);
                    else if (deltaTime > duration.value + Mathf.Max(0f, breakDuration.value))
                        rootMotionSpeed = impulseSpeed.value * ParadoxNotion.Animation.Easing.Ease(breakEase.value, 0, 1f, 1f - (deltaTime - duration.value - Mathf.Max(0f, accelDuration.value)) / breakDuration.value);

                    var rootMotionVec = rootMotionSpeed * pawnBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                    if (actionCtrler.CanRootMotion(rootMotionVec))
                        pawnMovable.AddRootMotion(Time.deltaTime * rootMotionVec, Quaternion.identity, Time.deltaTime);
                }).AddTo(agent);

            if (!endActionWhenReachToTarget)
                EndAction(true);
        }
    }

    [Category("Pawn")]
    public class StartHomingRotation : ActionTask
    {
        public BBParameter<Transform> target;
        public BBParameter<float> rotateSpeed = 1f;
        public BBParameter<float> duration = -1f;
        const float __MIN_DELTA_ANGLE = 1f;

        protected override void OnExecute()
        {
            var pawnBrain = agent.GetComponent<PawnBrainController>();
            Debug.Assert(pawnBrain != null);

            var actionCtrler = agent.GetComponent<PawnActionController>();
            Debug.Assert(actionCtrler != null);

            var pawnMovable = pawnBrain as IPawnMovable;
            Debug.Assert(pawnMovable != null);

            var executeTimeStamp = Time.time;
            actionCtrler.currActionContext.homingRotationDisposable?.Dispose();
            actionCtrler.currActionContext.homingRotationDisposable = Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds((duration.value > 0f ? duration.value : 3600f))))
                .DoOnCancel(() => actionCtrler.currActionContext.homingRotationDisposable = null)
                .DoOnCompleted(() => actionCtrler.currActionContext.homingRotationDisposable = null)
                .Subscribe(_ =>
                {
                    var forwardVec = pawnBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                    var deltaAngle = target.isNoneOrNull ? 0f : Vector3.SignedAngle(forwardVec, (target.value.position - pawnBrain.coreColliderHelper.transform.position).Vector2D().normalized, Vector3.up);
                    if (Mathf.Abs(deltaAngle) > __MIN_DELTA_ANGLE)
                    {
                        var rotateAngle = (deltaAngle > 0f ? 1f : -1f) * Mathf.Min(Mathf.Abs(deltaAngle) * Mathf.Rad2Deg, rotateSpeed.value * Time.deltaTime);
                        pawnMovable.FaceTo(Quaternion.Euler(0f, rotateAngle, 0f) * forwardVec);
                    }
                }).AddTo(agent);

            EndAction(true);
        }
    }

    [Category("Pawn")]
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

    [Category("Pawn")]
    public class StartHomingRootMoion : ActionTask
    {
        public BBParameter<float> homingSpeed = 1f;
        public BBParameter<float> duration = -1f;
        public BBParameter<ParadoxNotion.Animation.EaseType> accelEase;
        public BBParameter<float> accelDuration = -1f;
        public BBParameter<ParadoxNotion.Animation.EaseType> breakEase;
        public BBParameter<float> breakDuration = -1f;
        public BBParameter<float> minApproachDistance = 0.1f;
        public BBParameter<PawnColliderHelper> targetColliderHelper;
        public bool endActionWhenReachToTarget;
        IDisposable __rootMotionDisposable;
        int __capturedActionInstanceId;
        float __manualAdvanceSpeedCached;
        
        protected override void OnExecute()
        {
            var pawnBrain = agent.GetComponent<PawnBrainController>();
            Debug.Assert(pawnBrain != null);

            var pawnActionCtrler = pawnBrain.GetComponent<PawnActionController>();
            Debug.Assert(pawnActionCtrler != null);

            var pawnMovable = pawnBrain as IPawnMovable;
            Debug.Assert(pawnMovable != null);

            __capturedActionInstanceId =  pawnActionCtrler.currActionContext.actionInstanceId;
            if (endActionWhenReachToTarget)
            {
                pawnActionCtrler.WaitAction();
                __manualAdvanceSpeedCached = pawnActionCtrler.currActionContext.manualAdvanceSpeed;
                pawnActionCtrler.currActionContext.manualAdvanceSpeed = 0;
            }

            var executeTimeStamp = Time.time;

            pawnActionCtrler.currActionContext.rootMotionDisposable?.Dispose();
            pawnActionCtrler.currActionContext.rootMotionDisposable = __rootMotionDisposable = Observable.EveryUpdate()
                .TakeUntil(Observable.Timer(TimeSpan.FromSeconds((duration.value > 0f ? duration.value : 3600f) + Mathf.Max(0f, accelDuration.value) + Mathf.Max(0f, breakDuration.value))))
                .DoOnCancel(() => 
                {
                    pawnActionCtrler.currActionContext.rootMotionDisposable = __rootMotionDisposable = null;
                    if (endActionWhenReachToTarget)
                    {
                        pawnActionCtrler.currActionContext.manualAdvanceSpeed = __manualAdvanceSpeedCached;
                        EndAction(true);
                    }
                })
                .DoOnCompleted(() =>
                {
                    pawnActionCtrler.currActionContext.rootMotionDisposable = __rootMotionDisposable = null;
                    if (endActionWhenReachToTarget)
                    {
                        pawnActionCtrler.currActionContext.manualAdvanceSpeed = __manualAdvanceSpeedCached;
                        EndAction(true);
                    }
                })
                .Subscribe(_ =>
                {
                    if (endActionWhenReachToTarget)
                        pawnActionCtrler.currActionContext.manualAdvanceSpeed = 0;

                    //* Action이 취소되었다면 즉시 중지
                    if (!pawnActionCtrler.CheckActionRunning() || __capturedActionInstanceId != pawnActionCtrler.currActionContext.actionInstanceId)
                    {
                        Debug.Assert(__rootMotionDisposable != null && __rootMotionDisposable == pawnActionCtrler.currActionContext.rootMotionDisposable);
                        __rootMotionDisposable.Dispose();
                        __rootMotionDisposable = pawnActionCtrler.currActionContext.rootMotionDisposable = null;

                        return;
                    }

                    if (!targetColliderHelper.isNoneOrNull && minApproachDistance.value > 0f && targetColliderHelper.value.GetDistanceBetween(pawnBrain.coreColliderHelper) <= minApproachDistance.value)
                    {
                        if (endActionWhenReachToTarget)
                        {
                            Debug.Assert(__rootMotionDisposable != null && __rootMotionDisposable == pawnActionCtrler.currActionContext.rootMotionDisposable);
                            __rootMotionDisposable.Dispose();
                            __rootMotionDisposable = pawnActionCtrler.currActionContext.rootMotionDisposable = null;
                        }

                        //* 타겟에 도달했다면 RootMotion은 적용하지 않도록 리턴함
                        return;
                    }

                    var rootMotionSpeed = homingSpeed.value;
                    var deltaTime = Time.time - executeTimeStamp;
                    if (deltaTime < accelDuration.value)
                        rootMotionSpeed = homingSpeed.value * ParadoxNotion.Animation.Easing.Ease(accelEase.value, 0f, 1f, deltaTime / accelDuration.value);
                    else if (deltaTime > duration.value + Mathf.Max(0f, breakDuration.value))
                        rootMotionSpeed = homingSpeed.value * ParadoxNotion.Animation.Easing.Ease(breakEase.value, 0, 1f, 1f - (deltaTime - duration.value - Mathf.Max(0f, accelDuration.value)) / breakDuration.value);

                    var rootMotionVec = rootMotionSpeed * pawnBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                    pawnMovable.AddRootMotion(Time.deltaTime * rootMotionVec, Quaternion.identity, Time.deltaTime);
                }).AddTo(agent);

            if (!endActionWhenReachToTarget)
                EndAction(true);
        }
    }

    [Category("Pawn")]
    public class VerticalImpulseRootMotion : ActionTask
    {
        public BBParameter<float> verticalImpulse = 1f;
        protected override void OnExecute()
        {
            if (agent.TryGetComponent<PawnMovement>(out var pawnMovement) && pawnMovement.GetCharacterMovement() != null)
            {
                pawnMovement.GetCharacterMovement().velocity.y = verticalImpulse.value;
                pawnMovement.GetCharacterMovement().PauseGroundConstraint();
            }

            EndAction(true);
        }
    }

    [Category("Pawn")]
    public class FallingRootMotion : ActionTask
    {
        public BBParameter<float> fallingSpeed;
        public BBParameter<float> duration;
        IDisposable __fallingDisposable;
        float __falllingTimeStamp;
        int __capturedActionInstanceId;
        
        protected override void OnExecute()
        {
            var actionCtrler = agent.GetComponent<PawnActionController>();
            Debug.Assert(actionCtrler != null);
            
            var pawnMovable = agent.GetComponent<PawnBrainController>() as IPawnMovable;
            Debug.Assert(pawnMovable != null);

            __capturedActionInstanceId =  actionCtrler.currActionContext.actionInstanceId;
            __falllingTimeStamp = Time.time;
            __fallingDisposable = Observable.EveryUpdate().Subscribe(_ =>
            {   
                if ((Time.time - __falllingTimeStamp) > duration.value || pawnMovable.IsOnGround())
                {
                    __fallingDisposable.Dispose();
                    __fallingDisposable = null;
                    return;
                }
                else if (!actionCtrler.CheckActionRunning() || __capturedActionInstanceId != actionCtrler.currActionContext.actionInstanceId)
                {
                    __fallingDisposable.Dispose();
                    __fallingDisposable = null;
                    return;
                }

                pawnMovable.AddRootMotion(fallingSpeed.value * Time.deltaTime * Vector3.down, Quaternion.identity, Time.deltaTime);
            }).AddTo(agent);

            EndAction(true);
        }
    }

    [Category("Pawn")]
    public class StartTraceActionTargets : ActionTask
    {
        public BBParameter<Collider> traceCollider;
        public BBParameter<string[]> traceLayerNames;
        public BBParameter<string[]> tracePawnNames;
        public BBParameter<PawnStatusController.StatusParam[]> debuffParams;
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

            actionCtrler.StartTraceActionTargets(traceCollider.value, traceSamplingRate.value, multiHitEnabled.value, debuffParams.value, true, drawGizmos, drawGizmosDuration);
            EndAction(true);
        }
    }

    [Category("Pawn")]
    public class FinishTraceActionTargets : ActionTask
    {
        public bool cancelActionWhenTraceFailed;

        protected override void OnExecute()
        {
            EndAction(agent.GetComponent<PawnActionController>().FinishTraceActionTargets() > 0 || !cancelActionWhenTraceFailed);
        }
    }

    [Category("Pawn")]
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
        public BBParameter<PawnStatusController.StatusParam[]> debuffParams;
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

        [Serializable]
        public struct StatusInjectionData
        {
            public PawnStatus status;
            public float strength;
            public float duration;
        }

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

            if (!__pawnActionCtrler.CheckActionRunning() || __capturedActionInstanceId != __pawnActionCtrler.currActionContext.actionInstanceId)
                EndAction(false);
            else if (__sampleInterval <= Time.time - __lastSampleTimeStamp && TraceSampleInternal() >= __sampleNum)
                EndAction(true);
        }

        int TraceSampleInternal()
        {   
            __Logger.LogR1(__pawnBrain.gameObject, nameof(TraceSampleInternal), "__sampleNum", __sampleIndex);

            if (traceDirection.value == 0 || __sampleNum == 1)
            {
                __traceResults = __pawnActionCtrler.TraceActionTargets(offset.value, pitchYawRoll.value, fanRadius.value, fanAngle.value, fanHeight.value, minRadius.value, maxTargetNum.value, null, false, drawGizmos, drawGizmosDuration);
            }
            else
            {
                var sampleYaw =  (traceDirection.value > 0 ? 1f : -1f) * ((__sampleIndex + 0.5f) * __stepFanAngle - __halfFanAngle);
                var fanMatrix = Matrix4x4.TRS(offset.value + __pawnBrain.coreColliderHelper.pawnCollider.bounds.center - __pawnBrain.coreColliderHelper.transform.position, Quaternion.Euler(pitchYawRoll.value) * Quaternion.Euler(0f, sampleYaw, 0f), Vector3.one);
                __traceResults = __pawnActionCtrler.TraceActionTargets(fanMatrix, fanRadius.value, __stepFanAngle, fanHeight.value, minRadius.value, maxTargetNum.value, null, false, drawGizmos, drawGizmosDuration);
            }

            if (__sampleNum == 1)
            {
                foreach (var r in __traceResults)
                    __pawnBrain.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(__pawnBrain, r.pawnBrain, __actionData, r.pawnCollider, __pawnActionCtrler.currActionContext.insufficientStamina));

                return 1;
            }
            else
            {
                __lastSampleTimeStamp = Time.time;

                foreach (var r in __traceResults)
                {
                    if (!__sentDamageBrains.Contains(r.pawnBrain))
                    {
                        __sentDamageBrains.Add(r.pawnBrain);
                        __pawnBrain.PawnHP.Send(new PawnHeartPointDispatcher.DamageContext(__pawnBrain, r.pawnBrain, __actionData, r.pawnCollider, __pawnActionCtrler.currActionContext.insufficientStamina));

                        //* Debuff 할당
                        if (!debuffParams.isNoneOrNull && debuffParams.value.Length > 0)
                        {
                            foreach (var p in debuffParams.value)
                            {
                                if (p.isExtern && r.pawnBrain.TryGetComponent<PawnActionController>(out var receiverActionCtrler))
                                    r.pawnBrain.PawnStatusCtrler.AddExternStatus(receiverActionCtrler, p);
                                else
                                    r.pawnBrain.PawnStatusCtrler.AddStatus(p);
                            }
                        }
                    }
                }
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

    [Category("Pawn")]
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

    [Category("Pawn")]
    public class EmitProjectile : ActionTask
    {
        protected override string info => emitPrefab.isNoneOrNull && emitPrefab.varRef == null ? base.info : $"Emit '{emitPrefab.name}' x <b>{emitNum.value}</b>";
        public BBParameter<GameObject> emitPrefab;
        public BBParameter<Transform> emitPoint;
        public BBParameter<int> emitNum = 1;
        public BBParameter<float> emitInterval = -1f;
        public bool runInParallel;

        PawnActionController __pawnActionCtrler;
        int __capturedActionInstanceId;
        int __emitCount;

        protected override void OnExecute()
        {
            __pawnActionCtrler = agent.GetComponent<PawnActionController>();
            Debug.Assert(__pawnActionCtrler != null);

            __capturedActionInstanceId = __pawnActionCtrler.currActionContext.actionInstanceId;
            __pawnActionCtrler.EmitActionHandler(emitPrefab.value, emitPoint.value, emitNum.value);
            __emitCount = 1;

            if (emitNum.value == 1 || emitInterval.value <= 0f)
            {
                EndAction(true);
            }
            else
            {
                Observable.Interval(TimeSpan.FromSeconds(emitInterval.value))
                    .TakeWhile(_ => __pawnActionCtrler.CheckActionRunning() && __pawnActionCtrler.currActionContext.actionInstanceId == __capturedActionInstanceId)
                    .Take(emitNum.value - 1)
                    .DoOnCancel(() =>
                    {
                        if (!runInParallel)
                            EndAction(false);
                    })
                    .DoOnCompleted(() =>
                    {
                        if (!runInParallel)
                            EndAction(true);
                    })
                    .Subscribe(_ => __pawnActionCtrler.EmitActionHandler(emitPrefab.value, emitPoint.value, __emitCount)).AddTo(agent);
                
                if (runInParallel)
                    EndAction(true);
            }
        }
    }

    [Category("Pawn")]
    public class SetActionLayerBlendSpeed : ActionTask
    {
        protected override string info => base.info + $" <b>{newValue}</b>";
        public float newValue;
        protected override void OnExecute()
        {
            if (agent.TryGetComponent<PawnActionController>(out var actionCtrler) && actionCtrler.CheckActionRunning())
                actionCtrler.currActionContext.animBlendSpeed = Mathf.Clamp01(newValue);

            EndAction(true);
        }
    }

    [Category("Pawn")]
    public class SetActionLayerBlendWeight : ActionTask
    {
        protected override string info => base.info + $" <b>{newValue}</b>";
        public float newValue;
        protected override void OnExecute()
        {
            if (agent.TryGetComponent<PawnActionController>(out var actionCtrler) && actionCtrler.CheckActionRunning())
                actionCtrler.currActionContext.animBlendWeight = Mathf.Clamp01(newValue);

            EndAction(true);
        }
    }

    [Category("Pawn")]
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

    [Category("Pawn")]
    public class SetAnimBool : ActionTask
    {
        protected override string info => string.IsNullOrEmpty(paramId.value) ? base.info : $"Set <b>{paramId.value}</b> Param <b>{newValue.value}</b>";
        public BBParameter<Animator> animator;
        public BBParameter<string> paramId;
        public BBParameter<bool> newValue;
        protected override void OnExecute()
        {
            animator.value.SetBool(paramId.value, newValue.value);
            EndAction(true);
        }
    }

    [Category("Pawn")]
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

    [Category("Pawn")]
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

    [Category("Pawn")]
    public class FadeAnimFloat : ActionTask
    {
        protected override string info => $"Fade <b>{paramId.value}</b> Param to <b>{targetValue.value}</b>";
        public BBParameter<Animator> animator;
        public BBParameter<string> paramId;
        public BBParameter<float> targetValue;
        public BBParameter<float> duration;
        float __currValue;
        float __fadeSpeed;
        
        protected override void OnExecute()
        {
            __currValue = animator.value.GetFloat(paramId.value);
            __fadeSpeed = (targetValue.value - __currValue) / duration.value;
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            __currValue += __fadeSpeed * Time.deltaTime;
            __currValue = __fadeSpeed > 0f ? Mathf.Min(targetValue.value, __currValue) : Mathf.Max(targetValue.value, __currValue);
            animator.value.SetFloat(paramId.value, __currValue);

            if (__currValue == targetValue.value)
                EndAction(true);
        }
    }

    [Category("Pawn")]
    public class SetAnimSpeedMultiplier : ActionTask
    {
        protected override string info => $"Set AnimSpeedMultiplier <b>{newValue.value}</b>";
        public BBParameter<float> newValue;
        
        protected override void OnExecute()
        {
            if (agent.TryGetComponent<PawnActionController>(out var actionCtrler) && actionCtrler.CheckActionRunning())
            {
                actionCtrler.currActionContext.actionSpeed = (actionCtrler.currActionContext.actionData?.actionSpeed ?? 1f) * newValue.value;

                if (agent.TryGetComponent<PawnAnimController>(out var animCtrler))
                    animCtrler.mainAnimator.SetFloat("AnimSpeed", actionCtrler.currActionContext.actionSpeed);
                else
                    Debug.Assert(false);
            }

            EndAction(true);
        }
    }

    [Category("Pawn")]
    public class SetRootMotionMultiplier : ActionTask
    {   
        protected override string info => $"Set RootMotion Multiplier <b>{newValue.value}</b>";
        public BBParameter<float> newValue;
        protected override void OnExecute()
        {
            if (agent.TryGetComponent<PawnActionController>(out var actionCtrler) && actionCtrler.currActionContext.actionData != null)
                actionCtrler.currActionContext.rootMotionMultiplier = newValue.value;
            EndAction(true);
        }
    }

    [Category("Pawn")]
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

    [Category("Pawn")]
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

    [Category("Pawn")]
    public class SetMovementEnabled : ActionTask
    {
        protected override string info => movementEnabled ? $"Movement <b>On</b>" : $"Movement <b>Off</b>";
        public bool movementEnabled;

        protected override void OnExecute()
        {
            if (agent.TryGetComponent<PawnActionController>(out var actionCtrler))
                actionCtrler.currActionContext.movementEnabled = movementEnabled;

            EndAction(true);
        }
    }

    [Category("Pawn")]
    public class SetLegGlueEnabled : ActionTask
    {
        protected override string info => enabled.value ? $"Leg Glue <b>On</b>" : $"Leg Glue <b>Off</b>";
        public BBParameter<bool> enabled;

        protected override void OnExecute()
        {
            if (agent.TryGetComponent<PawnActionController>(out var actionCtrler))
                actionCtrler.currActionContext.legAnimGlueEnabled = enabled.value;

            EndAction(true);
        }
    }

    [Category("Pawn")]
    public class SetInterruptEnabled : ActionTask
    {
        protected override string info => enabled.value ? $"Interrupt <b>Enabled</b>" : $"Interrupt <b>Disabled</b>";
        public BBParameter<bool> enabled;

        protected override void OnExecute()
        {
            if (agent.TryGetComponent<PawnActionController>(out var actionCtrler))
                actionCtrler.currActionContext.interruptEnabled = enabled.value;

            EndAction(true);
        }
    }

    [Category("Pawn")]
    public class SetParryingEnabled : ActionTask
    {   
        protected override string info => newValue.value ? "Parrying <b>On</b>" : "Parrying <b>Off</b>";
        public BBParameter<bool> newValue;
        protected override void OnExecute()
        {
            if (agent.TryGetComponent<PawnActionController>(out var actionCtrler) && actionCtrler.currActionContext.actionData != null)
                actionCtrler.SetActiveParryingEnabled(newValue.value);
            EndAction(true);
        }
    }

    [Category("Pawn")]
    public class AddStatus : ActionTask
    {
        protected override string info => duration.value < 0 ? $"<b>{status.value}</b> On" : $"<b>{status.value}</b> On for <b>{duration.value}</b> secs";
        public BBParameter<PawnStatus> status;
        public BBParameter<float> strength;
        public BBParameter<float> duration;
        public bool removeBuffWhenActionFinished = true;
        protected override void OnExecute()
        {
            Debug.Assert(status.value < PawnStatus.__DEBUFF__SEPERATOR__);

            if (removeBuffWhenActionFinished)
            {
                if (agent.TryGetComponent<PawnActionController>(out var actionCtrler))
                    (actionCtrler as IStatusContainer).AddStatus(status.value, strength.value, duration.value);
            }
            else
            {
                if (agent.TryGetComponent<PawnStatusController>(out var statusCtrler))
                    statusCtrler.AddStatus(status.value, strength.value, duration.value);
            }

            EndAction(true);
        }
    }

    [Category("Pawn")]
    public class RemoveStatus : ActionTask
    {
        protected override string info => $"<b>{status.value}</b> Off";
        public BBParameter<PawnStatus> status;
        protected override void OnExecute()
        {
            if (agent.TryGetComponent<PawnActionController>(out var actionCtrler))
                (actionCtrler as IStatusContainer).RemoveStatus(status.value);

            EndAction(true);
        }
    }

    [Category("Pawn")]
    public class ShowFX : ActionTask
    {
        protected override string info => $"Show FX <b>{(fxPrefab.isNoneOrNull && fxPrefab.varRef == null ? fxName.value : fxPrefab)}</b>";
        public BBParameter<string> fxName;
        public BBParameter<GameObject> fxPrefab;
        public BBParameter<Transform> localToWorld;
        public BBParameter<Vector3> position;
        public BBParameter<Vector3> pitchYawRoll;
        public BBParameter<Vector3> scale = Vector3.one;
        public BBParameter<ParticleSystemScalingMode> scalingMode = ParticleSystemScalingMode.Local;
        public BBParameter<float> duration = -1f;
        public BBParameter<float> playRate = 1f;
        public BBParameter<string[]> childNameToBeHidden;
        public bool attachToTransform = false;
        public bool stopWhenActionCanceled = true;
        PawnActionController __pawnActionCtrler;
        EffectInstance __fxInstance;
        IDisposable __showDisposable;
        float __showTimeStamp;
        int __capturedActionInstanceId;

        protected override void OnExecute()
        {
            __pawnActionCtrler = agent.GetComponent<PawnActionController>();
            Debug.Assert(__pawnActionCtrler != null);
            
            if (!localToWorld.isNoneOrNull)
            {
                var localToWorldMatrix = Matrix4x4.TRS(localToWorld.value.position, localToWorld.value.rotation, Vector3.one) * Matrix4x4.TRS(position.value, Quaternion.Euler(pitchYawRoll.value), Vector3.one);
                if (fxPrefab.isNoneOrNull)
                    __fxInstance = EffectManager.Instance.Show(fxName.value, localToWorldMatrix.GetPosition(), localToWorldMatrix.rotation, scale.value, duration.value, 0f, scalingMode.value, playRate.value);
                else
                    __fxInstance = EffectManager.Instance.Show(fxPrefab.value, localToWorldMatrix.GetPosition(), localToWorldMatrix.rotation, scale.value, duration.value, 0f, scalingMode.value, playRate.value);
            }
            else
            {
                if (fxPrefab.isNoneOrNull)
                    __fxInstance = EffectManager.Instance.Show(fxName.value, position.value, Quaternion.Euler(pitchYawRoll.value), scale.value, duration.value, 0f, scalingMode.value, playRate.value);
                else
                    __fxInstance = EffectManager.Instance.Show(fxPrefab.value, position.value, Quaternion.Euler(pitchYawRoll.value), scale.value, duration.value, 0f, scalingMode.value, playRate.value);
            }
            
            Debug.Assert(__fxInstance != null);

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
                __showDisposable = Observable.EveryUpdate().Subscribe(_ =>
                {
                    if (!__pawnActionCtrler.CheckActionRunning() || __pawnActionCtrler.currActionContext.actionInstanceId != __capturedActionInstanceId)
                    {
                        __fxInstance.gameObject.SetActive(false);
                        __showDisposable?.Dispose();
                        __showDisposable = null;
                    }
                }).AddTo(agent);
            }

            EndAction(true);
        }
    }

    [Category("Pawn")]
    public class ShowTrailFX : ActionTask
    {
        protected override string info => $"Show Trail-FX <b>{(trailFx.isNoneOrNull ? trailFx.value : string.Empty)}</b>";
        public BBParameter<XWeaponTrail> trailFx;
        public BBParameter<Transform> startPoint;
        public BBParameter<Transform> endPoint;
        public BBParameter<float> duration = -1f;
        public bool stopWhenActionCanceled = true;
        PawnActionController __pawnActionCtrler;
        IDisposable __showDisposable;
        float __showTimeStamp;
        int __capturedActionInstanceId;

        protected override void OnExecute()
        {
            if (trailFx.value == null) { EndAction(true); }
            __pawnActionCtrler = agent.GetComponent<PawnActionController>();

            if (!startPoint.isNoneOrNull)
                trailFx.value.PointStart = startPoint.value;
            if (!endPoint.isNoneOrNull)
                trailFx.value.PointEnd = endPoint.value;

            trailFx.value.Activate();

            __capturedActionInstanceId = __pawnActionCtrler.currActionContext.actionInstanceId;
            __showTimeStamp = Time.time;
            __showDisposable = Observable.EveryUpdate().Subscribe(_ =>
            {
                if ((Time.time - __showTimeStamp) > duration.value)
                {
                    trailFx.value.Deactivate();
                    __showDisposable.Dispose();
                    __showDisposable = null;
                }
                else if (stopWhenActionCanceled && __pawnActionCtrler != null && (!__pawnActionCtrler.CheckActionRunning() || __pawnActionCtrler.currActionContext.actionInstanceId != __capturedActionInstanceId))
                {
                    trailFx.value.Deactivate();
                    __showDisposable.Dispose();
                    __showDisposable = null;
                }
            }).AddTo(agent);

            EndAction(true);
        }
    }

    [Category("Pawn")]
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

    [Category("Pawn")]
    public class PlaySoundClip : ActionTask 
    {
        protected override string info => soundClip.isNoneOrNull ? base.info : $"Play SoundClip <b>{soundClip.name}</b>";
        public BBParameter<AudioClip> soundClip;
        public BBParameter<SoundType> soundType;
        public BBParameter<float> volumeRate;
        public BBParameter<bool> isLooping;

        protected override void OnExecute()
        {
            SoundManager.Instance.PlayWithClip(soundClip.value, isLooping.value, soundType.value == SoundType.SFX, volumeRate.value);
            EndAction(true);
        }
    }
    
    [Category("Pawn")]
    public class ZoomCamera : ActionTask
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

    [Category("Pawn")]
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

    [Category("Pawn")]
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

    [Category("Pawn")]
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

    [Category("Pawn")]
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

    [Category("Pawn")]
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

    [Category("Pawn")]
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
}