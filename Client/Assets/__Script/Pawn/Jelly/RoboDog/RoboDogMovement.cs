using System;
using FIMSpace;
using Packets;
using UniRx;
using UnityEngine;

namespace Game
{
    public class RoboDogMovement : PawnMovementEx
    {
        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<RoboDogBrain>();
        }

        RoboDogBrain __brain;

        protected override void OnUpdateHandler()
        {
            var canRotate1 = __pawnBrain.PawnBB.IsSpawnFinished && !__pawnBrain.PawnBB.IsDead && !__pawnBrain.PawnBB.IsGroggy && !__pawnBrain.PawnBB.IsDown;
            var canRotate2 = canRotate1 && (__pawnActionCtrler == null || !__pawnActionCtrler.CheckActionRunning());

            if (canRotate2 && __brain.BB.CurrDecision == NpcBrain.Decisions.Spacing && __brain.BB.TargetBrain != null)
                faceVec = (0.5f * (__brain.BB.TargetBrain.GetWorldPosition() + __brain.BB.HostBrain.GetWorldPosition()) - __brain.GetWorldPosition()).Vector2D().normalized;

            base.OnUpdateHandler();
        }
    }
}