using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.NodeCanvasExtension.RoboSoldier
{
    [Category("Rapax")]
    public class StartJump : ActionTask
    {
        public float jumpHeight;

        protected override void OnExecute()
        {
            if (agent.TryGetComponent<RapaxBrain>(out var brain))
                brain.Movement.StartJump(jumpHeight);

            EndAction(true);
        }
    }
}