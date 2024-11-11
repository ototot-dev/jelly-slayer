using UnityEngine;

namespace Game
{
    public static class PawnBlackboardHelper
    {
        public static void ReduceStamina(this PawnBlackboard.Stat stat, float delta)
        {   
            stat.stamina.Value = Mathf.Max(0f, stat.stamina.Value -delta);
        }

        public static void RecoverStamina(this PawnBlackboard.Stat stat, float conditionTimeStamp, float deltaTime)
        {   
            if (Time.time - conditionTimeStamp > stat.staminaRecoverTimeThreshold)
                stat.stamina.Value = Mathf.Min(stat.maxStamina.Value, stat.stamina.Value + stat.staminaRecoverSpeed * deltaTime);
        }

        public static void ReduceStance(this PawnBlackboard.Stat stat, float conditionTimeStamp, float deltaTime)
        {   
            if (Time.time - conditionTimeStamp > stat.stanceReduceTimeThreshold)
                stat.stance.Value = Mathf.Max(0f, stat.stance.Value - stat.stanceReduceSpeed * deltaTime);
        }
    }
}