using UniRx;
using UGUI.Rx;
using System.Linq;
using UnityEngine;
using TMPro;

namespace Game
{
    public interface IInteractionKeyAttachable
    {
        Vector3 GetInteractionKeyAttachPoint() => Vector3.zero;
    }

    [Template(path: "UI/template/interaction-key")]
    public class InteractionKeyController : Controller
    {
        public string commandName;
        RectTransform __attachedRect;
        PawnBrainController __targetBrain;

        public InteractionKeyController(string commandName, PawnBrainController targetBrain)
        {
            this.commandName = commandName;
            __targetBrain = targetBrain;
            // if (targetBrain is JellyBrain jellyBrain) __targetJellyMesh = jellyBrain.jellyMeshCtrler;
        }

        public override void OnPreShow()
        {
            base.OnPreShow();

            if (__attachedRect == null) __attachedRect = template.transform as RectTransform;
            __attachedRect.anchorMin = __attachedRect.anchorMax = Vector2.zero;

            Observable.EveryLateUpdate().TakeWhile(_ => hideCount <= 0).Subscribe(_ =>
            {
                if (__targetBrain != null)
                {
                    __attachedRect.anchoredPosition = GameContext.Instance.cameraCtrler.gameCamera.WorldToScreenPoint(__targetBrain.GetInteractionKeyAttachPoint());
                }
                // else

                // __bodyRect.anchoredPosition = attachToJellyMesh ?
                //     GameContext.Instance.cameraCtrler.gameCamera.WorldToScreenPoint(__targetJellyMesh.springMassSystem.core.position + Vector3.up) :
                //     GameContext.Instance.cameraCtrler.gameCamera.WorldToScreenPoint(__followBrain.GetInteractionKeyAttachPoint());
            }).AddTo(template);
        }

        public override void OnPostShow()
        {
            base.OnPostShow();

            var query = GetComponentById<ImageStyleSelector>("body").query;
            query.activeStates.Add("heartbeat");
            query.Apply();
        }

        public override void OnPreHide()
        {
            base.OnPreHide();

            var query = GetComponentById<ImageStyleSelector>("body").query;
            query.activeStates.Remove("heartbeat");
            query.Apply();
        }
    }
}