using NodeCanvas.Framework;
using ParadoxNotion.Design;

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
}