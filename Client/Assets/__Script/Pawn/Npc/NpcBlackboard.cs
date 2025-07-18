using System;
using System.Linq;
using UniRx;

namespace Game
{
    public class NpcBlackboard : PawnBlackboard
    {
        public NpcBrain.Decisions CurrDecision => decision.currDecision.Value;
        public float AggressiveLevel => decision.aggressiveLevel.Value;
        public bool IsInCombat => decision.aggressiveLevel.Value >= 0f;

        [Serializable]
        public class Decision
        {
            public ReactiveProperty<NpcBrain.Decisions> currDecision = new(NpcBrain.Decisions.None);
            public FloatReactiveProperty aggressiveLevel = new(0);
        }

        public Decision decision = new();
        
        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            pawnData_Movement = MainTable.PawnData_Movement.PawnData_MovementList.First(d => d.pawnId == common.pawnId);
            
        }
        
        public MainTable.PawnData_Movement pawnData_Movement;
    }
}