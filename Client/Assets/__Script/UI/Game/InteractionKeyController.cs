using UniRx;
using UGUI.Rx;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 상호작용 키 UI가 부착될 수 있는 객체의 인터페이스
    /// </summary>
    public interface IInteractionKeyAttachable
    {
        /// <summary>
        /// 키 입력 처리 전에 호출되어 입력 가능 여부를 판단
        /// </summary>
        /// <returns>키 입력 처리가 가능하면 true</returns>
        bool PreprocessKeyDown() => true;
        
        /// <summary>
        /// 상호작용 키 UI가 표시될 월드 위치를 반환
        /// </summary>
        /// <returns>UI 부착 지점의 월드 좌표</returns>
        Vector3 GetInteractionKeyAttachPoint() => Vector3.zero;
        
        /// <summary>
        /// 상호작용 키 UI가 부착될 때 호출
        /// </summary>
        void OnInteractionKeyAttached() { }
        
        /// <summary>
        /// 상호작용 키 UI가 제거될 때 호출
        /// </summary>
        void OnInteractionKeyDetached() { }
    }

    /// <summary>
    /// 플레이어와 상호작용 가능한 객체에 표시되는 키 프롬프트 UI 컨트롤러
    /// </summary>
    [Template(path: "UI/template/interaction-key")]
    public class InteractionKeyController : Controller
    {
        // 공개 필드 및 속성
        public string displayName;
        public string commandName;
        public float visibleRadius = -1f;
        public IntReactiveProperty keyPressedCount = new();
        public bool IsInteractionFinished => hideCount > 0;
        public bool IsVisible => __visibilityFlag.Value;

        // 내부 필드
        BoolReactiveProperty __visibilityFlag = new();
        PawnBrainController __attachableBrain;
        InteractableHandler __attachableHandler;
        IInteractionKeyAttachable __attachable;
        RectTransform __templateRect;

        // 생성자 - PawnBrainController용
        public InteractionKeyController(string displayName, string commandName, float visibleRadius, PawnBrainController attachableBrain)
        {
            this.displayName = displayName;
            this.commandName = commandName;
            this.visibleRadius = visibleRadius;
            __attachable = __attachableBrain = attachableBrain;
        }

        // 생성자 - InteractableHandler용
        public InteractionKeyController(string displayName, string commandName, float visibleRadius, InteractableHandler attachableHandler)
        {
            this.displayName = displayName;
            this.commandName = commandName;
            this.visibleRadius = visibleRadius;
            __attachable = __attachableHandler = attachableHandler;
        }

        // 키 입력 처리 메서드
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

            // GameContext에 등록 및 부착 알림
            GameContext.Instance.interactionKeyCtrlers.Add(this);
            __attachable.OnInteractionKeyAttached();

            // UI 템플릿 초기화
            if (__templateRect == null)
                __templateRect = template.transform as RectTransform;

            __templateRect.anchorMin = __templateRect.anchorMax = 0.5f * Vector2.one;
            GetComponentById<CanvasGroup>("body").alpha = 0f;

            // 초기 가시성 설정
            if (visibleRadius > 0f) {

                Vector3 attachPoint = __attachable.GetInteractionKeyAttachPoint();
                __visibilityFlag.Value = attachPoint.Distance2D(GameContext.Instance.playerCtrler.possessedBrain.GetWorldPosition()) <= visibleRadius;
            }
            else
                __visibilityFlag.Value = true;

            // 가시성 변경 시 애니메이션 처리
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

            // 매 프레임 거리 체크 및 위치 업데이트
            Observable.EveryLateUpdate().Subscribe(_ =>
            {
                if (visibleRadius > 0f)
                    __visibilityFlag.Value = __attachable.GetInteractionKeyAttachPoint().Distance2D(GameContext.Instance.playerCtrler.possessedBrain.GetWorldPosition()) <= visibleRadius;

                Vector3 point = __attachable.GetInteractionKeyAttachPoint();
                Debug.Log("InteractionKeyController Pos : " + point);
                Vector3 screenPoint = GameContext.Instance.cameraCtrler.gameCamera.WorldToScreenPoint(point);
                Vector2 centerOffset = new Vector2(screenPoint.x - Screen.width * 0.5f, screenPoint.y - Screen.height * 0.5f);
                __templateRect.anchoredPosition = centerOffset;

            }).AddToHide(this);
        }

        public override void OnPostShow()
        {
            base.OnPostShow();

            // heartbeat 애니메이션 시작
            var query = GetComponentById<ImageStyleSelector>("body").query;
            query.activeStates.Add("heartbeat");
            query.Execute();
        }

        public override void OnPreHide()
        {
            base.OnPreHide();

            // 숨김 애니메이션 시작
            var query = GetComponentById<ImageStyleSelector>("body").query;
            query.activeStates.Clear();
            query.activeStates.Add("hide");
            query.Execute();

            // 부착 해제 알림 및 GameContext에서 제거
            __attachable.OnInteractionKeyDetached();
            GameContext.Instance.interactionKeyCtrlers.Remove(this);
        }
    }
}