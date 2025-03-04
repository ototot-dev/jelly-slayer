using UniRx;
using UGUI.Rx;
using System.Linq;
using UnityEngine;

namespace Game
{
    [Template(path: "UI/template/special-key")]
    public class SpecialKeyController : Controller
    {
        public string reservedActionName;
        PawnBrainController __targetBrain;
        RectTransform __bodyRect;

        public SpecialKeyController(PawnBrainController targetBrain, string reservedActionName)
        {
            this.reservedActionName = reservedActionName;
            __targetBrain = targetBrain;
        }

        public override void OnPreShow()
        {
            base.OnPreShow();

            if (__bodyRect == null) __bodyRect = template.transform as RectTransform;
            __bodyRect.anchorMin = __bodyRect.anchorMax = Vector2.zero;
            Observable.EveryLateUpdate().TakeWhile(_ => hideCount <= 0).Subscribe(_ =>
            {
                __bodyRect.anchoredPosition =  GameContext.Instance.MainCamera.WorldToScreenPoint(__targetBrain.GetSpecialKeyPosition());
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