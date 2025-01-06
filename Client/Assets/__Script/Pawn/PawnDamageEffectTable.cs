using UnityEngine;

namespace Game
{
    [CreateAssetMenu(fileName = "NewDamageEffectData", menuName = "Game/ScriptableObjects/DamageEffectData", order = 1)]
    public class PawnDamageEffectData : ScriptableObject
    {
        public GameObject onHit;
        public GameObject onCriticalHit;
        public GameObject onMissed;
        public GameObject onBlocked;
        public GameObject onGuardBreak;
        public GameObject onParried;
    }
}