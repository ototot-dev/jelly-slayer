using UniRx;
using UGUI.Rx;
using System.Linq;
using UnityEngine;
using ZLinq;
using UnityEngine.UI;

namespace Game
{
    public interface IInteractionKeyAttachable
    {
        Vector3 GetInteractionKeyAttachPoint() => Vector3.zero;
    }

    [Template(path: "UI/template/interaction-key")]
    public class InteractionKeyController : Controller
    {
        public string displayName;
        public string commandName;
        public float visibleRadius = -1f;
        BoolReactiveProperty visibilityFlag = new();
        bool IsVisible => visibilityFlag.Value;

        RectTransform __attachedRect;
        PawnBrainController __targetBrain;
        IInteractionKeyAttachable __attachableTarget;
        Transform __attachableTargetTrasform;

        public InteractionKeyController(string displayName, string commandName, PawnBrainController targetBrain, float visibleRadius = -1f)
        {
            this.displayName = displayName;
            this.commandName = commandName;
            this.visibleRadius = visibleRadius;
            __attachableTarget = __targetBrain = targetBrain;
        }

        public InteractionKeyController(string displayName, string commandName, Transform attachableTarget, float visibleRadius = -1f)
        {
            this.displayName = displayName;
            this.commandName = commandName;
            this.visibleRadius = visibleRadius;
            __attachableTarget = attachableTarget as IInteractionKeyAttachable;

            if (__attachableTarget == null)
            {
                __attachableTargetTrasform = attachableTarget.Children().Where(c => c.CompareTag("InteractionKeyAttachPoint")).FirstOrDefault();
                Debug.LogWarning($"InteractionKeyController.ctor(), InteractionKeyAttachPoint is not found => attachableTarget: {attachableTarget}");   
            }
        }

        public override void OnPreShow()
        {
            base.OnPreShow();

            if (__attachedRect == null) __attachedRect = template.transform as RectTransform;
            __attachedRect.anchorMin = __attachedRect.anchorMax = Vector2.zero;

            if (visibleRadius > 0f)
            {
                //* visibleRadius가 활성화 상태면 최초엔 보이지 않도록 강제 수정
                GetComponentById<CanvasGroup>("body").alpha = 0f;
            }
            else
            {
                visibilityFlag.Value = true;
            }

            visibilityFlag.Subscribe(v =>
            {
                if (v)
                {
                    var query = GetComponentById<ImageStyleSelector>("body").query;
                    query.activeStates.Remove("hide");
                    query.activeStates.Add("show");
                    query.Apply();
                }
                else
                {
                    var query = GetComponentById<ImageStyleSelector>("body").query;
                    query.activeStates.Remove("show");
                    query.activeStates.Add("hide");
                    query.Apply();
                }
            }).AddToHide(this);

            Observable.EveryLateUpdate().TakeWhile(_ => hideCount <= 0).Subscribe(_ =>
            {
                var attachPoint = __attachableTarget != null ? __attachableTarget.GetInteractionKeyAttachPoint() : __attachableTargetTrasform.position;
                __attachedRect.anchoredPosition = GameContext.Instance.cameraCtrler.gameCamera.WorldToScreenPoint(attachPoint);

                if (visibleRadius > 0f)
                    visibilityFlag.Value = attachPoint.Distance2D(GameContext.Instance.playerCtrler.possessedBrain.GetWorldPosition()) <= visibleRadius;
            }).AddToHide(this);
        }

        // public override void OnPostShow()
        // {
        //     base.OnPostShow();

        //     var query = GetComponentById<ImageStyleSelector>("body").query;
        //     query.activeStates.Add("heartbeat");
        //     query.Apply();
        // }

        public override void OnPreHide()
        {
            base.OnPreHide();

            var query = GetComponentById<ImageStyleSelector>("body").query;
            query.activeStates.Remove("heartbeat");
            query.Apply();
        }

        //* 키 입력 처리가 가능한지 판단하는 함수
        public bool PreprocessKeyDown() => IsVisible;
    }
}