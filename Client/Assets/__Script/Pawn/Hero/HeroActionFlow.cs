using System;
using Game;
using UniRx;
using UnityEngine;

namespace FlowCanvas.Nodes
{
    /// <summary>
    /// 
    /// </summary>
    [ParadoxNotion.Design.Category("Game/Hero")]
    public class OnAttackAction : EventNode<Transform>
    {
        FlowOutput __raised;

        string __actionName;

        float __actionSpeed;

        public override void OnGraphStarted()
        {
            base.OnGraphStarted();

            target.value.GetComponent<Game.HeroActionController>().onActionStart += OnActionStartHandler;
        }

        void OnActionStartHandler(PawnActionController.ActionContext actionContext, Game.PawnHeartPointDispatcher.DamageContext damageContext)
        {
            this.__actionName = actionContext.actionName;
            this.__actionSpeed = actionContext.actionData.actionSpeed;

            __raised.Call(new Flow());
        }

        public override void OnGraphStoped()
        {
            base.OnGraphStoped();

            try 
            {
                if (target.value != null)
                    target.value.GetComponent<Game.HeroActionController>().onActionStart -= OnActionStartHandler;
            }
            catch (MissingReferenceException e)
            {
                // Debug.Log("1?? Application is quitting.");
            }
        }

        protected override void RegisterPorts()
        {
            __raised = AddFlowOutput("Out");

            AddValueOutput<string>("ActionName", () => { return __actionName; });
            AddValueOutput<float>("ActionSpeed", () => { return __actionSpeed; });
        }
    }
}