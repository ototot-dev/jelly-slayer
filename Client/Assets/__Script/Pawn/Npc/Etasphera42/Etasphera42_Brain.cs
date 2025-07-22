using UniRx;
using UnityEngine;

namespace Game
{
    [RequireComponent(typeof(Etasphera42_Movement))]
    [RequireComponent(typeof(Etasphera42_Blackboard))]
    [RequireComponent(typeof(Etasphera42_AnimController))]
    [RequireComponent(typeof(Etasphera42_ActionController))]
    public class Etasphera42_Brain : NpcQuadWalkBrain, IPawnTargetable, IPawnEventListener
    {
#region IPawnTargetable 구현
        public enum HitBoxIndices
        {
            Body = 0,
            LeftLeg1,
            LeftLeg2,
            RightLeg1,
            RightLeg2,
            Max,
        }

        PawnColliderHelper IPawnTargetable.StartTargeting()
        {   
            __currTargetingIndex = HitBoxIndices.Body;
            return GetCurrentTarget();
        }

        PawnColliderHelper IPawnTargetable.NextTarget()
        {
            return ++__currTargetingIndex == HitBoxIndices.Max ? null : GetCurrentTarget();
        }

        PawnColliderHelper IPawnTargetable.PrevTarget()
        {
            return --__currTargetingIndex < 0 ? null : GetCurrentTarget();
        }

        PawnColliderHelper IPawnTargetable.CurrTarget()
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

#region IPlayerActionListener 구현
        void IPawnEventListener.OnReceivePawnActionStart(PawnBrainController sender, string eventName)
        {
            switch (eventName)
            {
                case "OnJump": break;
                case "OnHanging": ActionCtrler.StartAddictiveAction(new PawnHeartPointDispatcher.DamageContext(),  "LaserB", 1f); break;
            }
        }

        void IPawnEventListener.OnReceivePawnStatusChanged(PawnBrainController sender, PawnStatus status, float strength, float duration) {}
        void IPawnEventListener.OnReceivePawnDamageContext(PawnBrainController sender, PawnHeartPointDispatcher.DamageContext damageContext) {}
        void IPawnEventListener.OnReceivePawnSpawningStateChanged(PawnBrainController sender, PawnSpawnStates state) {}
#endregion

        [Header("Component")]
        public PawnColliderHelper leftLeg1_colliderHelper;
        public PawnColliderHelper leftLeg2_colliderHelper;
        public PawnColliderHelper rightLeg1_colliderHelper;
        public PawnColliderHelper rightLeg2_colliderHelper;

        public override PawnColliderHelper GetHookingColliderHelper() => BB.children.hookingPointColliderHelper;
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
        MainTable.ActionData __torchFireActionData;
        MainTable.ActionData __rushActionData;

        protected override void StartInternal()
        {
            base.StartInternal();
            
            __muzzleFireActionData ??= ActionDataSelector.GetActionData("MuzzleFire");
            __torchFireActionData ??= ActionDataSelector.GetActionData("TorchFire");
            __rushActionData ??= ActionDataSelector.GetActionData("Rush");

            onTick += (deltaTick) =>
            {
                if (!BB.IsSpawnFinished || BB.IsDead || BB.IsGroggy || BB.IsDown || !BB.IsInCombat || BB.TargetPawn == null)
                    return;
                    
                // ActionDataSelector.UpdateExcutables(deltaTick);

                // if (debugActionDisabled)
                //     return;
                
                // if (!ActionCtrler.CheckActionRunning() && !ActionCtrler.CheckActionPending())
                // {
                //     var distanceConstraint = BB.TargetBrain != null ? coreColliderHelper.GetApproachDistance(BB.TargetBrain.GetWorldPosition()) : -1f;
                //     if (ActionDataSelector.CheckCoolTime(__muzzleFireActionData) && ActionDataSelector.EvaluateSelection(__muzzleFireActionData, 0f, 1f) && CheckTargetVisibility())
                //     {
                //         ActionDataSelector.ResetCoolTime(__muzzleFireActionData);
                //         ActionCtrler.SetPendingAction(__muzzleFireActionData.actionName);
                //     }
                //     else if (ActionDataSelector.CheckCoolTime(__torchFireActionData) && ActionDataSelector.EvaluateSelection(__torchFireActionData, 0f, 1f) && CheckTargetVisibility())
                //     {
                //         ActionDataSelector.ResetCoolTime(__torchFireActionData);
                //         ActionCtrler.SetPendingAction(__torchFireActionData.actionName);
                //     }
                //     else if (ActionDataSelector.CheckExecutable(__rushActionData) && ActionDataSelector.EvaluateSelection(__rushActionData, distanceConstraint, 1f) && CheckTargetVisibility())
                //     {
                //         ActionDataSelector.ResetSelection(__rushActionData);
                //         ActionCtrler.SetPendingAction(__rushActionData.actionName);
                //     }
                // }
            };

            // BB.action.isFalling.Skip(1).Subscribe(v =>
            // {
            //     //* 착지 동작 완료까지 이동을 금지함
            //     if (!v) PawnStatusCtrler.AddStatus(PawnStatus.CanNotMove, 1f, 0.5f);
            // }).AddTo(this);
        }

        protected override void DamageReceiverHandler(ref PawnHeartPointDispatcher.DamageContext damageContext)
        {
            base.DamageReceiverHandler(ref damageContext);

            if (damageContext.actionResult == ActionResults.Blocked)
            {   
    
                // if (string.IsNullOrEmpty(ActionCtrler.GetPendingActionData().ActionName) && ActionDataSelector.EvaluateSelection(__counterActionData, -1f, 1f) && CheckTargetVisibility())
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
            // Movement.StartFalling();
        }
    }
}
