using UniRx;
using UGUI.Rx;
using UnityEngine;

namespace Game
{
    public interface IInteractionKeyAttachable
    {
        bool PreprocessKeyDown() => true;
        Vector3 GetInteractionKeyAttachPoint() => Vector3.zero;
    }

    [Template(path: "UI/template/interaction-key")]
    public class InteractionKeyController : Controller
    {
        public string displayName;
        public string commandName;
        public float visibleRadius = -1f;
        public IntReactiveProperty keyPressedCount = new();
        public bool IsInteractionFinished => hideCount > 0;
        public bool IsVisible => __visibilityFlag.Value;

        BoolReactiveProperty __visibilityFlag = new();
        PawnBrainController __attachableBrain;
        InteractableHandler __attachableHandler;
        IInteractionKeyAttachable __attachable;
        RectTransform __templateRect;

        public InteractionKeyController(string displayName, string commandName, float visibleRadius, PawnBrainController attachableBrain)
        {
            this.displayName = displayName;
            this.commandName = commandName;
            this.visibleRadius = visibleRadius;
            __attachable = __attachableBrain = attachableBrain;
        }

        public InteractionKeyController(string displayName, string commandName, float visibleRadius, InteractableHandler attachableHandler)
        {
            this.displayName = displayName;
            this.commandName = commandName;
            this.visibleRadius = visibleRadius;
            __attachable = __attachableHandler = attachableHandler;
        }

        //* 키 입력 처리가 가능한지 판단하는 함수
        public bool PreprocessKeyDown() => IsVisible && __attachable.PreprocessKeyDown();
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

            if (__templateRect == null)
                __templateRect = template.transform as RectTransform;

            __templateRect.anchorMin = __templateRect.anchorMax = Vector2.zero;

            //* 최초 상태에선 보이지 않도록 Alpha값은 0으로 셋팅
            GetComponentById<CanvasGroup>("body").alpha = 0f;

            if (visibleRadius > 0f)
                __visibilityFlag.Value = __attachable.GetInteractionKeyAttachPoint().Distance2D(GameContext.Instance.playerCtrler.possessedBrain.GetWorldPosition()) <= visibleRadius;
            else
                __visibilityFlag.Value = true;

            __visibilityFlag.Subscribe(v =>
            {
                if (v)
                {
                    var query = GetComponentById<ImageStyleSelector>("body").query;
                    query.activeStates.Remove("hide");
                    query.activeStates.Add("show");
                    query.Execute();
                }
                else
                {
                    var query = GetComponentById<ImageStyleSelector>("body").query;
                    query.activeStates.Remove("show");
                    query.activeStates.Add("hide");
                    query.Execute();
                }
            }).AddToHide(this);

            Observable.EveryLateUpdate().Subscribe(_ =>
            {
                if (visibleRadius > 0f)
                    __visibilityFlag.Value = __attachable.GetInteractionKeyAttachPoint().Distance2D(GameContext.Instance.playerCtrler.possessedBrain.GetWorldPosition()) <= visibleRadius;

                __templateRect.anchoredPosition = GameContext.Instance.cameraCtrler.gameCamera.WorldToScreenPoint(__attachable.GetInteractionKeyAttachPoint());
            }).AddToHide(this);
        }

        public override void OnPostShow()
        {
            base.OnPostShow();

            var query = GetComponentById<ImageStyleSelector>("body").query;
            query.activeStates.Add("heartbeat");
            query.Execute();
        }

        public override void OnPreHide()
        {
            base.OnPreHide();

            var query = GetComponentById<ImageStyleSelector>("body").query;

            query.activeStates.Clear();
            query.activeStates.Add("hide");
            query.Execute();

            //* PlayerController에서 해제도 직접 함
            GameContext.Instance.playerCtrler.interactionKeyCtrlers.Remove(this);
        }
    }
}