using UnityEngine;

namespace Game
{
    public static class PawnBlackboardHelper
    {
        public static float ReduceStamina(this PawnBlackboard.Stat stat, float delta)
        {   
            stat.stamina.Value = Mathf.Max(0f, stat.stamina.Value -delta);
            return stat.stamina.Value;
        }

        public static float RecoverStamina(this PawnBlackboard.Stat stat, float conditionTimeStamp, float deltaTime)
        {   
            if (Time.time - conditionTimeStamp > stat.staminaRecoverTimeThreshold)
                stat.stamina.Value = Mathf.Min(stat.maxStamina.Value, stat.stamina.Value + stat.staminaRecoverSpeed * deltaTime);
            return stat.stamina.Value;
        }

        public static float ReduceStance(this PawnBlackboard.Stat stat, float conditionTimeStamp, float deltaTime)
        {   
            if (Time.time - conditionTimeStamp > stat.stanceReduceTimeThreshold)
                stat.stance.Value = Mathf.Max(0f, stat.stance.Value - stat.stanceReduceSpeed * deltaTime);
            return stat.stance.Value;
        }
    }
}