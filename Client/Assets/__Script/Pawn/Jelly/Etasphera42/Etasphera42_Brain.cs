using UniRx;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Etasphera42_Movement))]
    [RequireComponent(typeof(Etasphera42_Blackboard))]
    [RequireComponent(typeof(Etasphera42_AnimController))]
    [RequireComponent(typeof(Etasphera42_ActionController))]
    public class Etasphera42_Brain : JellyQuadWalkBrain, IPawnTargetable
    {
        public enum HitBoxIndices
        {
            Body = 0,
            LeftLeg1,
            LeftLeg2,
            RightLeg1,
            RightLeg2,
            Max,
        }

#region IPawnTargetable 구현
        PawnColliderHelper IPawnTargetable.StartTargeting()
        {   
            __currTargetingIndex = HitBoxIndices.Body;
            return GetCurrentTarget();
        }

        PawnColliderHelper IPawnTargetable.NextTargeting()
        {
            if (++__currTargetingIndex == HitBoxIndices.Max)
                __currTargetingIndex = HitBoxIndices.Body;
            return GetCurrentTarget();
        }

        PawnColliderHelper IPawnTargetable.CurrTargeting()
        {
            return GetCurrentTarget();
        }

        void IPawnTargetable.StopTargeting()
        {
            __currTargetingIndex = HitBoxIndices.Max;
        }
        
        PawnColliderHelper GetCurrentTarget()
        {
            return __currTargetingIndex switch
            {
                HitBoxIndices.Body => bodyHitColliderHelper,
                HitBoxIndices.LeftLeg1 => leftLeg1_colliderHelper,
                HitBoxIndices.LeftLeg2 => leftLeg2_colliderHelper,
                HitBoxIndices.RightLeg1 => rightLeg1_colliderHelper,
                HitBoxIndices.RightLeg2 => rightLeg2_colliderHelper,
                _ => null,
            };
        }

        HitBoxIndices __currTargetingIndex = HitBoxIndices.Max;
#endregion

        [Header("Component")]
        public PawnColliderHelper leftLeg1_colliderHelper;
        public PawnColliderHelper leftLeg2_colliderHelper;
        public PawnColliderHelper rightLeg1_colliderHelper;
        public PawnColliderHelper rightLeg2_colliderHelper;

        [Header("Debug")]
        public bool debugActionDisabled;

        public override PawnColliderHelper GetHookingColliderHelper() => ActionCtrler.hookingPointColliderHelper;
        public Etasphera42_Blackboard BB { get; private set; }
        public Etasphera42_Movement Movement { get; private set; }
        public Etasphera42_AnimController AnimCtrler { get; private set; }
        public Etasphera42_ActionController ActionCtrler { get; private set; }

        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            BB = GetComponent<Etasphera42_Blackboard>();
            Movement = GetComponent<Etasphera42_Movement>();
            AnimCtrler = GetComponent<Etasphera42_AnimController>();
            ActionCtrler = GetComponent<Etasphera42_ActionController>();
        }

        MainTable.ActionData __muzzleFireActionData;

        protected override void StartInternal()
        {
            base.StartInternal();
            
            __muzzleFireActionData ??= ActionDataSelector.GetActionData("MuzzleFire");

            onTick += (deltaTick) =>
            {
                if (!BB.IsSpawnFinished || BB.IsDead || BB.IsGroggy || BB.IsDown || !BB.IsInCombat || BB.TargetPawn == null)
                    return;
                    
                ActionDataSelector.UpdateSelection(deltaTick);

                if (debugActionDisabled)
                    return;
                
                if (ActionDataSelector.CheckExecutable(__muzzleFireActionData))
                {
                    var distanceConstraint = BB.TargetBrain != null ? coreColliderHelper.GetApproachDistance(BB.TargetBrain.GetWorldPosition()) : -1f;
                    if (ActionDataSelector.EvaluateSelection(__muzzleFireActionData, distanceConstraint, 1f) && CheckTargetVisibility())
                    {
                        ActionDataSelector.ResetSelection(__muzzleFireActionData);
                        ActionCtrler.SetPendingAction(__muzzleFireActionData.actionName);
                    }
                }
            };

            BB.action.isFalling.Skip(1).Subscribe(v =>
            {
                //* 착지 동작 완료까지 이동을 금지함
                if (!v) PawnStatusCtrler.AddStatus(PawnStatus.CanNotMove, 1f, 0.5f);
            }).AddTo(this);
        }

        protected override void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            base.DamageReceiverHandler(ref damageContext);

            if (damageContext.actionResult == ActionResults.Blocked)
            {   
                if (debugActionDisabled)
                    return;
                    
                // if (string.IsNullOrEmpty(ActionCtrler.PendingActionData.Item1) && ActionDataSelector.EvaluateSelection(__counterActionData, -1f, 1f) && CheckTargetVisibility())
                // {
                //     ActionDataSelector.ResetSelection(__counterActionData);
                //     ActionCtrler.SetPendingAction("Counter");
                // }
                // else
                // {
                //     ActionDataSelector.BoostSelection(__counterActionData, BB.action.counterAttackRateStep);
                // }
            }
        }

        protected override void StartJumpInternal(float jumpHeight)
        {
            Movement.StartJump(BB.body.jumpHeight);
        }

        protected override void FinishJumpInternal()
        {
            Movement.StartFalling();
        }
    }
}
