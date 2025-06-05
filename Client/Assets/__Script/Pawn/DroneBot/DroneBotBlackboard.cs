using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class DroneBotBlackboard : PawnBlackboard
    {
        public DroneBotBrain.Decisions CurrDecision => decision.currDecision.Value;
        public SlayerBrain HostBrain => decision.hostBrain.Value;
        public PawnColliderHelper HostColliderHelper => HostBrain != null ? decision.hostBrain.Value.coreColliderHelper : null;
        public Transform HostCore => HostBrain != null ? decision.hostBrain.Value.coreColliderHelper.transform : null;
        public Transform FormationSpot => decision.formationSpot.Value;
        public bool IsHanging => HostBrain != null && HostBrain.BB.body.hangingBrain.Value == __pawnBrain;
        public bool IsInCombat => decision.aggressiveLevel.Value >= 0f;
        public float AggressiveLevel => decision.aggressiveLevel.Value;
        public float HookingDistance => body.hookingDistance;
        public float SpacingInDistance => body.spacingInDistance;
        public float SpacingOutDistance => body.spacingOutDistance;
        public float MinSpacingDistance => body.minSpacingDistance;
        public float MaxSpacingDistance => body.maxSpacingDistance;
        public float MinApproachDistance => body.minApproachDistance;
        public float HoldPositionRate => body.holdPositionRate;
        public float MoveAroundRate => body.moveAroundRate;

        [Serializable]
        public class Body
        {
            public float normalSpeed = 1f;
            public float boostSpeed = 1f;
            public float flyHeight = 1f;
            public float flyHeightAdjustSpeed = 1f;
            public float hookingDistance = 1f;
            public float spacingInDistance = 1f;
            public float spacingOutDistance = 1f;
            public float minSpacingDistance = 1f;
            public float maxSpacingDistance = 1f;
            public float minApproachDistance = 1f;
            public float holdPositionRate = 1f;
            public float moveAroundRate = 1f;
        }

        public Body body = new();

        [Serializable]
        public class Decision
        {
            public ReactiveProperty<DroneBotBrain.Decisions> currDecision = new(DroneBotBrain.Decisions.None);
            public FloatReactiveProperty aggressiveLevel = new(0);
            public ReactiveProperty<SlayerBrain> hostBrain = new();
            public ReactiveProperty<Transform> formationSpot = new();
        }

        public Decision decision = new();

        [Serializable]
        public class Resource
        {
            public GameObject protonExplosionFx;
            public ParticleSystem jetBoostFx;
            public ParticleSystem orbBlueFx;
        }

        public Resource resource = new();

        [Serializable]
        public class Children
        {
            public SphereCollider visibilityChecker;
        }

        public Children children = new();
 
    }
}