using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.NodeCanvasExtension.Therionide
{
    [Category("Therionide")]
    public class StartJump : ActionTask
    {
        public float jumpHeight;

        protected override void OnExecute()
        {
            if (agent.TryGetComponent<TherionideBrain>(out var brain))
                brain.Movement.StartJump(jumpHeight);

            EndAction(true);
        }
    }
}