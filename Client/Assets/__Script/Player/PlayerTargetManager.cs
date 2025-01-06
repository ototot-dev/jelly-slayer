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
                .Where(p => p != null && p.pawnBrain != null && p.pawnBrain.PawnBB.IsDead == false)
                    // p.pawnBrain.PawnBB.IsBind == false && p.pawnBrain.PawnBB.IsDead == false)
                .OrderBy(p => (p.transform.position - heroBrain.GetWorldPosition()).sqrMagnitude).FirstOrDefault();

            return newTarget;
        }

        public static PawnColliderHelper FindStunnedTarget(HeroBrain heroBrain)
        {
            var newTarget = heroBrain.SensorCtrler.ListeningColliders
                .Select(l => l.GetComponent<PawnColliderHelper>())
                .Where(p => p != null && p.pawnBrain != null &&
                    p.pawnBrain.PawnBB.IsGroggy == true)
                .OrderBy(p => (p.transform.position - heroBrain.GetWorldPosition()).sqrMagnitude).FirstOrDefault();

            return newTarget;
        }
        public static PawnColliderHelper FindGuardbreakTarget(HeroBrain heroBrain)
        {
            return null;
            // var newTarget = heroBrain.SensorCtrler.ListeningColliders
            //     .Select(l => l.GetComponent<PawnColliderHelper>())
            //     .Where(p => p != null && p.pawnBrain != null &&
            //         p.pawnBrain.PawnBB.IsGuardbreak == true)
            //     .OrderBy(p => (p.transform.position - heroBrain.CoreTransform.position).sqrMagnitude).FirstOrDefault();

            // return newTarget;
        }
        public void UpdateTarget()
        {
            if (__playerCtrler.possessedBrain == null)
                return;

            var heroBrain = __playerCtrler.possessedBrain;
            var curTarget = heroBrain.BB.TargetBrain;

            // If Target is Dead....
            // if (curTarget != null && (curTarget.PawnBB.IsBind == true || curTarget.PawnBB.IsDead == true))
            if (curTarget != null && curTarget.PawnBB.IsDead == true)
            {
                heroBrain.BB.target.targetPawnHP.Value = null;
                heroBrain.Movement.freezeRotation = false;
                return;
            }
            // If the target is null, find a target.
            var newTarget = FindTarget(__playerCtrler.possessedBrain);
            if (heroBrain.BB.TargetPawn == null)
            {
                if (newTarget != null)
                {
                    var vDist = heroBrain.GetWorldPosition()- newTarget.transform.position;
                    if (vDist.magnitude <= _inBound)
                    {
                        heroBrain.BB.target.targetPawnHP.Value = newTarget.pawnBrain.PawnHP;
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
                        var playerPos = heroBrain.GetWorldPosition();
                        var vDist = playerPos - curTarget.GetWorldPosition();
                        var vDist2 = playerPos - newTarget.transform.position;
                        if (vDist.magnitude > _inBound && vDist2.magnitude < _inBound)
                        {
                            heroBrain.BB.target.targetPawnHP.Value = newTarget.pawnBrain.PawnHP;
                            heroBrain.Movement.freezeRotation = true;
                        }
                    }
                }
                // Out of a certain range...
                else
                {
                    var vDist = heroBrain.GetWorldPosition() - curTarget.GetWorldPosition();
                    if (vDist.magnitude > _outBound)
                    {
                        heroBrain.BB.target.targetPawnHP.Value = null;
                        heroBrain.Movement.freezeRotation = false;
                    }
                }
            }
        }
    }
}