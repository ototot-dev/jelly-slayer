using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.NodeCanvasExtension.Soldier
{
    [Category("Soldier")]
    public class StartGliding : ActionTask
    {
        protected override void OnExecute()
        {
            if (agent.TryGetComponent<SoldierBrain>(out var brain))
                brain.Movement.StartJump(brain.BB.body.jumpHeight);

            EndAction(true);
        }
    }

    [Category("Soldier")]
    public class FinishGliding : ActionTask
    {
        protected override void OnExecute()
        {
            if (agent.TryGetComponent<SoldierBrain>(out var brain))
                brain.Movement.StartFalling();

            EndAction(true);
        }
    }

    [Category("Soldier")]
    public class AdjustRootMotionMultiplier : ActionTask
    {
        public BBParameter<Transform> target;
        public BBParameter<float> desireDistance;
        public BBParameter<float> rootMotionDistance;

        protected override void OnExecute()
        {
            if (agent.TryGetComponent<SoldierBrain>(out var brain))
            {
                Debug.Assert(brain.ActionCtrler.CheckActionRunning());
                Debug.Assert(rootMotionDistance.value > 0f);

                var currDistance = (target.value.position - brain.GetWorldPosition()).Magnitude2D();
                var deltaDistance = Mathf.Max(1f, desireDistance.value - currDistance);
                brain.ActionCtrler.currActionContext.rootMotionMultiplier = Mathf.Clamp01(deltaDistance / rootMotionDistance.value);
            }

            EndAction(true);
        }
    }
}