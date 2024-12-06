using System;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Game
{
    public class JellyManBlackboard : PawnBlackboard
    {
        public JellyManBrain.Decisions CurrDecision => decision.currDecision.Value;
        public PawnBrainController TargetBrain => decision.targetPawnHP.Value != null ? decision.targetPawnHP.Value.PawnBrain : null;
        public GameObject TargetPawn => decision.targetPawnHP.Value != null ? decision.targetPawnHP.Value.gameObject : null;
        public Transform TargetCore => TargetBrain !=  null ? TargetBrain.coreColliderHelper.transform : null;
        public bool IsInCombat => decision.isInCombat.Value;
        public virtual bool IsGuarding => decision.isGuarding.Value;
        public float SpacingInDistance => decision.spacingInDistance;
        public float SpacingOutDistance => decision.spacingOutDistance;
        public float MinSpacingDistance => decision.minSpacingDistance;
        public float MaxSpacingDistance => decision.maxSpacingDistance;
        public float HoldPositionWeight => decision.holdPositionWeight;
        public float MoveAroundWeight => decision.moveAroundWeight;

        [Serializable]
        public class Body
        {
            public float walkSpeed = 1f;
            public float runSpeed = 1f;
            public float sprintSpeed = 1f;
        }

        public Body body = new();

        [Serializable]
        public class Decision
        {
            public ReactiveProperty<PawnHeartPointDispatcher> targetPawnHP = new();
            public ReactiveProperty<JellyManBrain.Decisions> currDecision = new(JellyManBrain.Decisions.None);
            public FloatReactiveProperty aggressiveLevel = new(0);
            public BoolReactiveProperty isInCombat = new();
            public BoolReactiveProperty isGuarding = new();
            public float spacingInDistance = 1;
            public float spacingOutDistance = 1;
            public float minSpacingDistance = 1;
            public float maxSpacingDistance = 1;
            public float holdPositionWeight = 1;
            public float moveAroundWeight = 1;
            public float minApproachDistance = 1;
        }

        public Decision decision = new();
        
        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            pawnData_Movement = MainTable.PawnData_Movement.PawnData_MovementList.First(d => d.pawnId == common.pawnId);
            
            // body.moveSpeed = pawnData_Movement.moveSpeed;
            // body.walkSpeed = pawnData_Movement.walkSpeed;
            
            var movement = GetComponent<PawnMovementEx>();
            // movement.moveSpeed = body.moveSpeed;
            // movement.moveAccel = body.moveAccel;
            // movement.moveBrake = body.moveBrake;
            // movement.rotateSpeed = body.rotateSpeed;
        }
        
        public MainTable.PawnData_Movement pawnData_Movement;
    }
}