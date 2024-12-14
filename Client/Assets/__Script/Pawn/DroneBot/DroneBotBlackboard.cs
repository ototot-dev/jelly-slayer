using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class  DroneBotBlackboard : PawnBlackboard
    {
        public DroneBotBrain.Decisions CurrDecision => decision.currDecision.Value;
        public float AggressiveLevel => decision.aggressiveLevel.Value;
        public bool IsInCombat => decision.aggressiveLevel.Value >= 0f;
        public float SpacingInDistance => action.spacingInDistance;
        public float SpacingOutDistance => action.spacingOutDistance;
        public float MinSpacingDistance => action.minSpacingDistance;
        public float MaxSpacingDistance => action.maxSpacingDistance;
        public float MinApproachDistance => action.minApproachDistance;
        public float HoldPositionRate => action.holdPositionRate;
        public float MoveAroundRate => action.moveAroundRate;

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