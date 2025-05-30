using UniRx;
using UGUI.Rx;
using UnityEngine;

namespace Game
{
    public interface IInteractionKeyAttachable
    {
        bool GetEnanbled() => false;
        float GetVisibleRadius() => -1f;
        Vector3 GetInteractionKeyAttachPoint() => Vector3.zero;
    }

    [Template(path: "UI/template/interaction-key")]
    public class InteractionKeyController : Controller
    {
        public string displayName;
        public string commandName;
        public IntReactiveProperty keyPressedCount = new();
        public bool IsInteractableFinished => hideCount > 0;
        public bool IsInteractableEnabled => __attachable.GetEnanbled();
        public bool IsVisible => __attachable.GetEnanbled() && __visibilityFlag.Value;
        BoolReactiveProperty __visibilityFlag = new();

        RectTransform __templateRect;
        PawnBrainController __attachableBrain;
        InteractableHandler __attachableHandler;
        IInteractionKeyAttachable __attachable;

        public InteractionKeyController(string displayName, string commandName, PawnBrainController attachableBrain)
        {
            this.displayName = displayName;
            this.commandName = commandName;
            __attachable = __attachableBrain = attachableBrain;
        }

        public InteractionKeyController(string displayName, string commandName, InteractableHandler attachableHandler)
        {
            this.displayName = displayName;
            this.commandName = commandName;
            __attachable = __attachableHandler = attachableHandler;
        }

        //* 키 입력 처리가 가능한지 판단하는 함수
        public bool PreprocessKeyDown() => IsVisible;
        public void PostProcessKeyDown(bool finishInteractable = false)
        {
            keyPressedCount.Value++;
            if (finishInteractable)
                this.HideAsObservable().Subscribe(_ => this.Unload());
        }

        public override void OnPreShow()
        {
            base.OnPreShow();

            //* PlayerController에 직접 등록함
            GameContext.Instance.playerCtrler.interactionKeyCtrlers.Add(this);

            if (__templateRect == null) __templateRect = template.transform as RectTransform;
            __templateRect.anchorMin = __templateRect.anchorMax = Vector2.zero;

            if (__attachable.GetVisibleRadius() > 0f)
            {
                //* visibleRadius가 활성화 상태면 최초엔 보이지 않도록 강제 수정
                GetComponentById<CanvasGroup>("body").alpha = 0f;
            }
            else
            {
                __visibilityFlag.Value = true;
            }

            __visibilityFlag.Subscribe(v =>
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

            Observable.EveryLateUpdate().Subscribe(_ =>
            {
                if (!__attachable.GetEnanbled())
                {
                    if (__visibilityFlag.Value) __visibilityFlag.Value = false;
                    return;
                }

                if (__attachable.GetVisibleRadius() > 0f)
                    __visibilityFlag.Value = __attachable.GetInteractionKeyAttachPoint().Distance2D(GameContext.Instance.playerCtrler.possessedBrain.GetWorldPosition()) <= __attachable.GetVisibleRadius();
                if (__visibilityFlag.Value)
                    __templateRect.anchoredPosition = GameContext.Instance.cameraCtrler.gameCamera.WorldToScreenPoint(__attachable.GetInteractionKeyAttachPoint());
            }).AddToHide(this);
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

            //* PlayerController에서 해제도 직접 함
            GameContext.Instance.playerCtrler.interactionKeyCtrlers.Remove(this);
        }
    }
}