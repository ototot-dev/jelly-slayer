using System;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Framework;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Game.SpiderActionTask
{
    // [ParadoxNotion.Design.Name("Hold Position (Spider)")]
    // public class HoldPosition : ActionTask
    // {
    //     public BBParameter<float> duration = 1;
    //     public bool notifyDecisionFinished = false;

    //     protected override void OnExecute()
    //     {
    //         agent.GetComponent<SpiderMovementController>().Stop();

    //         Observable.Timer(TimeSpan.FromSeconds(duration.value))
    //             .Subscribe(_ =>
    //             {
    //                 if (notifyDecisionFinished)
    //                     agent.GetComponent<SpiderBrain>()?.OnDecisionFinishedHandler();

    //                 EndAction(true);
    //             })
    //             .AddTo(agent);
    //     }
    // }

    // [ParadoxNotion.Design.Name("Move To Target (Spider)")]
    // public class MoveToTarget : ActionTask
    // {
    //     protected override void OnExecute()
    //     {
    //         __brain = agent.GetComponent<SpiderBrain>();

    //         var distance = __brain.MoveCtrler.Go();
            
    //         __moveDisposable = Observable.EveryUpdate()
    //             .TakeWhile(_ => !__brain.MoveCtrler.CheckReachToTargetPoint())
    //             .TakeLast(1)
    //             .Timeout(TimeSpan.FromSeconds(__brain.MoveCtrler.EstimateTimeToReachTargetPoint() + 1))
    //             .DoOnError(_ =>
    //             {
    //                 __brain.MoveCtrler.Stop();
    //                 EndAction(true);
    //             })
    //             .DoOnCancel(() =>
    //             {
    //                 __brain.MoveCtrler.Stop();
    //                 EndAction(true);
    //             })
    //             .DoOnCompleted(() =>
    //             {
    //                 __brain.MoveCtrler.Stop();
    //                 EndAction(true);
    //             })
    //             .Subscribe(_ => { }, e => { }).AddTo(agent);
    //     }

    //     SpiderBrain __brain;
    //     IDisposable __moveDisposable;

    //     protected override void OnStop(bool interrupted)
    //     {
    //         if (interrupted && __moveDisposable != null)
    //             __moveDisposable.Dispose();
    //     }
    // }

    // // [ParadoxNotion.Design.Name("Approach (Spider)")]
    // // public class Approach : ActionTask
    // // {
    // //     /// <summary>
    // //     /// 
    // //     /// </summary>
    // //     public BBParameter<GameObject> targetObj;

    // //     /// <summary>
    // //     /// 
    // //     /// </summary>
    // //     public BBParameter<bool> adjustMinApproachDistance;

    // //     protected override void OnExecute()
    // //     {
    // //         __brain = agent.GetComponent<JellySlimeBrain>();
    // //         __moveCtrler = agent.GetComponent<JellySlimeMovementController>();

    // //         __moveCtrler.minApproachDistance = JellySlimeMovementController.DEFAULT_MIN_APPROACH_DISTANCE;

    // //         if (adjustMinApproachDistance.value)
    // //         {
    // //             __moveCtrler.minApproachDistance += __brain.GetComponent<CapsuleCollider>().radius;
    // //             __moveCtrler.minApproachDistance += targetObj.value.GetComponent<CapsuleCollider>().radius;
    // //         }
            
    // //         __moveCtrler.GoTo(targetObj.value.transform.position);

    // //         __moveDisposable = Observable.Interval(TimeSpan.FromSeconds(0.2f))
    // //             .TakeWhile(_ => targetObj.value != null)
    // //             .Do(_ =>
    // //             {
    // //                 if (!__brain.ActionCtrler.CheckActionRunning())
    // //                     __moveCtrler.GoTo(targetObj.value.transform.position);
    // //             })
    // //             .DoOnCancel(() =>
    // //             {
    // //                 __moveCtrler.Stop();
    // //                 EndAction(true);
    // //             })
    // //             .Subscribe().AddTo(agent);
    // //     }

    // //     JellySlimeBrain __brain;
    // //     JellySlimeMovementController __moveCtrler;
    // //     IDisposable __moveDisposable;

    // //     protected override void OnStop(bool interrupted)
    // //     {
    // //         if (interrupted && __moveDisposable != null)
    // //             __moveDisposable.Dispose();
    // //     }
    // // }

    // /// <summary>
    // /// 
    // /// </summary>
    // [ParadoxNotion.Design.Name("Move Around (Spider)")]
    // public class MoveAround : ActionTask
    // {
    //     /// <summary>
    //     /// 
    //     /// </summary>
    //     public BBParameter<float> maxTurnAngle = 90;

    //     /// <summary>
    //     /// 
    //     /// </summary>
    //     public BBParameter<float> moveDistanceMultiplier = 1;

    //     protected override void OnExecute()
    //     {
    //         __brain = agent.GetComponent<SpiderBrain>();
    //         __moveCtrler = agent.GetComponent<SpiderMovementController>();

    //         var targetPoint = agent.GetComponent<PawnSensorController>().GetRandomPointInListeningArea(__moveCtrler.transform.forward, maxTurnAngle.value, moveDistanceMultiplier.value);
    //         // var targetPoint = __moveCtrler.target.position;
    //         var distance = __moveCtrler.GoTo(targetPoint);

    //         __moveDisposable = Observable.EveryUpdate()
    //             .TakeWhile(_ => !__moveCtrler.CheckReachToTargetPoint())
    //             .TakeLast(1)
    //             .Timeout(TimeSpan.FromSeconds(__moveCtrler.EstimateTimeToReachTargetPoint() + 1))
    //             .DoOnError(_ =>
    //             {
    //                 __moveCtrler.Stop();
    //                 EndAction(true);
    //             })
    //             .DoOnCancel(() =>
    //             {
    //                 __moveCtrler.Stop();
    //                 EndAction(true);
    //             })
    //             .DoOnCompleted(() =>
    //             {
    //                 __moveCtrler.Stop();
    //                 EndAction(true);
    //             })
    //             .Subscribe(_ => { }, e => { }).AddTo(agent);
    //     }

    //     SpiderBrain __brain;
    //     SpiderMovementController __moveCtrler;
    //     IDisposable __moveDisposable;

    //     protected override void OnStop(bool interrupted)
    //     {
    //         if (interrupted && __moveDisposable != null)
    //             __moveDisposable.Dispose();
    //     }
    // }

    // /// <summary>
    // /// 
    // /// </summary>
    // [ParadoxNotion.Design.Name("Patrol (Spider)")]
    // public class Patrol : ActionTask
    // {
    //     protected override void OnExecute()
    //     {
    //         __brain = agent.GetComponent<JellySlimeBrain>();
    //         __moveCtrler = agent.GetComponent<JellySlimeMovementController>();

    //         if (__moveCtrler.prevTargetPoints.Count != PawnMovement.PREV_TARGET_POINTS_COUNT)
    //         {
    //             EndAction(true);
    //             return;
    //         }

    //         var point2pointTable = new List<Tuple<Vector3, Vector3, float>>();

    //         foreach (var p in __moveCtrler.prevTargetPoints)
    //         {
    //             foreach (var pp in __moveCtrler.prevTargetPoints)
    //             {
    //                 point2pointTable.Add(new Tuple<Vector3, Vector3, float>(p, pp, 0));
    //             }
    //         }

    //         var findDistance = point2pointTable.Max(t => (t.Item1 - t.Item2).SqrMagnitude2D());
    //         var found = point2pointTable.Find(t => Mathf.Approximately((t.Item1 - t.Item2).SqrMagnitude2D(), findDistance));

    //         var patrolLoopCount = UnityEngine.Random.Range(1, 5);

    //         for (int i = 0; i < patrolLoopCount; i++)
    //         {
    //             __partrolPoints.Add(found.Item1);
    //             __partrolPoints.Add(found.Item2);
    //         }
    //     }

    //     List<Vector3> __partrolPoints = new List<Vector3>();
    //     int __patrolNumLeft;
    //     JellySlimeBrain __brain;
    //     JellySlimeMovementController __moveCtrler;
    //     IDisposable __moveDisposable;

    //     protected override void OnUpdate()
    //     {
    //         if (__partrolPoints.Count == 0)
    //         {
    //             EndAction(true);
    //             return;
    //         }

    //         if (!__moveCtrler.IsMoving)
    //         {
    //             __moveDisposable = MoveToPatrolPoint(__partrolPoints.First());
    //             __partrolPoints.RemoveAt(0);
    //         }
    //     }

    //     IDisposable MoveToPatrolPoint(Vector3 patrolPoint)
    //     {
    //         var distance = __moveCtrler.GoTo(patrolPoint, false);

    //         return Observable.EveryUpdate()
    //             .TakeWhile(_ => !__moveCtrler.CheckReachToTargetPoint())
    //             .TakeLast(1)
    //             .Timeout(TimeSpan.FromSeconds(distance / __brain.BB.decision.moveSpeed + 2))
    //             .DoOnError(_ =>
    //             {
    //                 __moveCtrler.Stop();
    //                 EndAction(true);
    //             })
    //             .DoOnCancel(() =>
    //             {
    //                 __moveCtrler.Stop();
    //                 EndAction(true);
    //             })
    //             .DoOnCompleted(() =>
    //             {
    //                 __moveCtrler.Stop();
    //             })
    //             .Subscribe(_ => { }, e => { }).AddTo(agent);
    //     }

    //     protected override void OnStop(bool interrupted)
    //     {
    //         if (interrupted && __moveDisposable != null)
    //             __moveDisposable.Dispose();
    //     }
    // }

}