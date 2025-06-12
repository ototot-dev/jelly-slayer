using UnityEngine;

namespace Game
{
    public static class PawnActionDataSelectorExtension
    {
        public static PawnActionDataSelector.ActionSequence BoostProbability(this PawnActionDataSelector.ActionSequence seq, float deltaProb, float maxProb = 1f)
        {
            seq.currProb = Mathf.Min(seq.currProb + deltaProb, maxProb);
            return seq;
        }
        public static PawnActionDataSelector.ActionSequence ResetProbability(this PawnActionDataSelector.ActionSequence seq, float resetProb)
        {
            seq.currProb = resetProb;
            return seq;
        }
        public static PawnActionDataSelector.ActionSequence BeginCoolTime(this PawnActionDataSelector.ActionSequence seq, float coolTime)
        {
            seq.BeginCoolTimeInternal(coolTime);
            return seq;
        }
        public static PawnActionDataSelector.ActionSequence ReduceCoolTime(this PawnActionDataSelector.ActionSequence seq, float deltaTime)
        {
            seq.maxCoolTime = Mathf.Max(0f, seq.maxCoolTime - deltaTime);
            return seq;
        }
        public static PawnActionDataSelector.ActionSequence CancelCoolTime(this PawnActionDataSelector.ActionSequence seq)
        {
            seq.CancelCoolTimeInternal();
            return seq;
        }
    }
}