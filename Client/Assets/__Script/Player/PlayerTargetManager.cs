using UnityEngine;
using System.Linq;

namespace Game
{
    public class PlayerTargetManager : MonoBehaviour
    {
        public float _inBound = 5;
        public float _outBound = 6;

        void Awake()
        {
            __playerCtrler = GetComponent<PlayerController>();
        }

        PlayerController __playerCtrler;

        public static PawnColliderHelper FindTarget(HeroBrain heroBrain) 
        {
            var newTarget = heroBrain.SensorCtrler.ListeningColliders
                .Select(l => l.GetComponent<PawnColliderHelper>())
                .Where(p => p != null && p.pawnBrain != null &&
                    p.pawnBrain.PawnBB.IsBind == false && p.pawnBrain.PawnBB.IsDead == false)
                .OrderBy(p => (p.transform.position - heroBrain.CoreTransform.position).sqrMagnitude).FirstOrDefault();

            return newTarget;
        }
        public static PawnColliderHelper FindStunnedTarget(HeroBrain heroBrain)
        {
            var newTarget = heroBrain.SensorCtrler.ListeningColliders
                .Select(l => l.GetComponent<PawnColliderHelper>())
                .Where(p => p != null && p.pawnBrain != null &&
                    p.pawnBrain.PawnBB.IsStunned == true)
                .OrderBy(p => (p.transform.position - heroBrain.CoreTransform.position).sqrMagnitude).FirstOrDefault();

            return newTarget;
        }
        public static PawnColliderHelper FindGuardbreakTarget(HeroBrain heroBrain)
        {
            var newTarget = heroBrain.SensorCtrler.ListeningColliders
                .Select(l => l.GetComponent<PawnColliderHelper>())
                .Where(p => p != null && p.pawnBrain != null &&
                    p.pawnBrain.PawnBB.IsGuardbreak == true)
                .OrderBy(p => (p.transform.position - heroBrain.CoreTransform.position).sqrMagnitude).FirstOrDefault();

            return newTarget;
        }
        public void UpdateTarget()
        {
            if (__playerCtrler.MyHeroBrain == null)
                return;

            var heroBrain = __playerCtrler.MyHeroBrain;
            var curTarget = heroBrain.BB.TargetBrain;

            // If Target is Dead....
            if (curTarget != null && (curTarget.PawnBB.IsBind == true || curTarget.PawnBB.IsDead == true))
            {
                heroBrain.BB.action.targetPawnHP.Value = null;
                heroBrain.Movement.freezeRotation = false;
                return;
            }
            var newTarget = FindTarget(__playerCtrler.MyHeroBrain);
            if (heroBrain.BB.TargetPawn == null)
            {
                if (newTarget != null)
                {
                    var vDist = heroBrain.CoreTransform.position - newTarget.transform.position;
                    if (vDist.magnitude <= _inBound)
                    {
                        heroBrain.BB.action.targetPawnHP.Value = newTarget.pawnBrain.PawnHP;
                        heroBrain.Movement.freezeRotation = true;
                    }
                }
            }
            else
            {
                if (newTarget != null) 
                {
                    if (newTarget.pawnBrain != curTarget)
                    {
                        var playerPos = heroBrain.CoreTransform.position;
                        var vDist = playerPos - curTarget.CoreTransform.position;
                        var vDist2 = playerPos - newTarget.transform.position;
                        if (vDist.magnitude > _inBound && vDist2.magnitude < _inBound)
                        {
                            heroBrain.BB.action.targetPawnHP.Value = newTarget.pawnBrain.PawnHP;
                            heroBrain.Movement.freezeRotation = true;
                        }
                    }
                }
                else
                {
                    var vDist = heroBrain.CoreTransform.position - curTarget.CoreTransform.position;
                    if (vDist.magnitude > _outBound)
                    {
                        heroBrain.BB.action.targetPawnHP.Value = null;
                        heroBrain.Movement.freezeRotation = false;
                    }
                }
            }
        }
    }
}