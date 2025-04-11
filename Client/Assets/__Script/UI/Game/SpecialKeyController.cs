using UniRx;
using UGUI.Rx;
using System.Linq;
using UnityEngine;
using TMPro;

namespace Game
{
    [Template(path: "UI/template/special-key")]
    public class SpecialKeyController : Controller
    {
        public string actionName;
        public string specialTag;
        public int actionCount;
        PawnBrainController __targetBrain;
        JellyMeshController __targetJellyMesh;
        RectTransform __bodyRect;

        public SpecialKeyController(PawnBrainController targetBrain, string actionName, string specialTag)
        {
            this.actionName = actionName;
            this.specialTag = specialTag;
            __targetBrain = targetBrain;
            if (targetBrain is JellyBrain jellyBrain) __targetJellyMesh = jellyBrain.jellyMeshCtrler;
        }

        public override void OnPreShow()
        {
            base.OnPreShow();

            if (__bodyRect == null) __bodyRect = template.transform as RectTransform;
            __bodyRect.anchorMin = __bodyRect.anchorMax = Vector2.zero;

            var attachToJellyMesh = specialTag == "Groggy";

            Observable.EveryLateUpdate().TakeWhile(_ => hideCount <= 0).Subscribe(_ =>
            {
                __bodyRect.anchoredPosition = attachToJellyMesh ?
                    GameContext.Instance.cameraCtrler.viewCamera.WorldToScreenPoint(__targetJellyMesh.springMassSystem.core.position + Vector3.up) :
                    GameContext.Instance.cameraCtrler.viewCamera.WorldToScreenPoint(__targetBrain.GetSpecialKeyPosition());
            }).AddTo(template);
        }

        public override void OnPostShow()
        {
            base.OnPostShow();

            var bodyQuery = GetComponentById<ImageStyleSelector>("body").query;
            bodyQuery.activeStates.Add("heartbeat");
            bodyQuery.Apply();
        }

        public override void OnPreHide()
        {
            base.OnPreHide();

            var bodyQuery = GetComponentById<ImageStyleSelector>("body").query;
            bodyQuery.activeStates.Remove("heartbeat");
            bodyQuery.Apply();
        }
    }
}