using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class  DroneBotBlackboard : PawnBlackboard
    {
        public DroneBotBrain.Decisions CurrDecision => decision.currDecision.Value;
        public HeroBrain HostBrain => body.hostBrain.Value;
        public PawnColliderHelper HostColliderHelper => HostBrain != null ? body.hostBrain.Value.coreColliderHelper : null;
        public Transform HostCore => HostBrain != null ? body.hostBrain.Value.coreColliderHelper.transform : null;
        public Transform FormationSpot => body.formationSpot.Value;
        public bool IsHanging => HostBrain != null && HostBrain.BB.action.hangingBrain.Value == __pawnBrain;
        public float AggressiveLevel => decision.aggressiveLevel.Value;
        public bool IsInCombat => decision.aggressiveLevel.Value >= 0f;
        public float CatchingDuration => action.catchingDuration;
        public float HookingDistance => action.hookingDistance;
        public float SpacingInDistance => action.spacingInDistance;
        public float SpacingOutDistance => action.spacingOutDistance;
        public float MinSpacingDistance => action.minSpacingDistance;
        public float MaxSpacingDistance => action.maxSpacingDistance;
        public float MinApproachDistance => action.minApproachDistance;
        public float HoldPositionRate => action.holdPositionRate;
        public float MoveAroundRate => action.moveAroundRate;

        [Serializable]
        public class Body
        {
            public float normalSpeed = 1f;
            public float boostSpeed = 1f;
            public float flyHeight = 1f;
            public float flyHeightAdjustSpeed = 1f;
            public ReactiveProperty<HeroBrain> hostBrain = new();
            public ReactiveProperty<Transform> formationSpot = new();
        }

        public Body body = new();

        [Serializable]
        public class Decision
        {
            public ReactiveProperty<DroneBotBrain.Decisions> currDecision = new(DroneBotBrain.Decisions.None);
            public FloatReactiveProperty aggressiveLevel = new(0);
        }

        public Decision decision = new();
        
        [Serializable]
        public class Action
        {
            public float catchingDuration = 1f;
            public float hookingDistance = 1f;
            public float spacingInDistance = 1f;
            public float spacingOutDistance = 1f;
            public float minSpacingDistance = 1f;
            public float maxSpacingDistance = 1f;
            public float minApproachDistance = 1f;
            public float holdPositionRate = 1f;
            public float moveAroundRate = 1f;
        }

        public Action action = new();
    }
}