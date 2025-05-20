using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UGUI.Rx;
using Yarn.Unity;

namespace Game
{
    [Template(path: "UI/template/3d-bubble-dialoque")]
    public class BubbleDialoqueController : Controller
    {
        public PawnBrainController speakerBrain;
        DialogueRunnerDispatcher __dialogueRunnerDispatcher;

        public override void OnPostLoad()
        {
            base.OnPostLoad();

            //* 드로우 순서를 제일 앞단으로 변경
            template.transform.SetAsFirstSibling();
        }

        public override void OnPreShow()
        {
            base.OnPreShow();

            __dialogueRunnerDispatcher = GameContext.Instance.launcher.currGameMode.GetDialogueRunnerDispatcher();
            Debug.Assert(__dialogueRunnerDispatcher != null);

            __dialogueRunnerDispatcher.AddViews(GetComponentById<DialogueViewBase>("dialogue"), GetComponentById<DialogueViewBase>("options"));

            __dialogueRunnerDispatcher.onRunLine += l =>
            {
                speakerBrain = GameContext.Instance.playerCtrler.possessedBrain;
                // speakerBrain = TaggerSystem.FindGameObjectWithTag(l.CharacterName).GetComponent<PawnBrainController>();
            };

            __dialogueRunnerDispatcher.onDialoqueComplete += () =>
            {
                this.HideAsObservable().Subscribe(_ => this.Unload());
            };

            var dialogueBubbleRect = GetComponentById<RectTransform>("dialogue");
            var optionsBubbleRect = GetComponentById<RectTransform>("options");

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (dialogueBubbleRect.gameObject.activeInHierarchy && speakerBrain != null)
                {
                    dialogueBubbleRect.anchoredPosition = WorldToAnchoredPosition(dialogueBubbleRect, speakerBrain.GetDialoqueAnchorPosition(), 0.2f);
                }
                if (optionsBubbleRect.gameObject.activeInHierarchy && speakerBrain != null)
                {
                    optionsBubbleRect.anchoredPosition = WorldToAnchoredPosition(optionsBubbleRect, speakerBrain.GetDialoqueAnchorPosition(), 0.2f);
                }
            }).AddToHide(this);
        }

        public override void OnPreHide()
        {
            base.OnPreHide();

            __dialogueRunnerDispatcher.RemoveViews(GetComponentById<DialogueViewBase>("dialogue"), GetComponentById<DialogueViewBase>("options"));
        }
        
        CanvasScaler __canvasScaler;

        /// <summary>Calculates where to put dialogue bubble based on worldPosition and any desired screen margins. 
        /// Ensure "constrainToViewportMargin" is between 0.0f-1.0f (% of screen) to constrain to screen, or value of -1 lets bubble go off-screen.</summary>
        Vector2 WorldToAnchoredPosition(RectTransform bubble, Vector3 worldPos, float constrainToViewportMargin = -1f)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle( 
                bubble.parent.GetComponent<RectTransform>(), // calculate local point inside parent... NOT inside the dialogue bubble itself
                GameContext.Instance.cameraCtrler.viewCamera.WorldToScreenPoint(worldPos), 
                null, 
                out Vector2 screenPos
            );
            
            if (__canvasScaler == null)
                __canvasScaler = template.transform.parent.GetComponent<CanvasScaler>();

            // to force the dialogue bubble to be fully on screen, clamp the bubble rectangle within the screen bounds
            if (constrainToViewportMargin >= 0f)
            {
                // because ScreenPointToLocalPointInRectangle is relative to a Unity UI RectTransform,
                // it may not necessarily match the full screen resolution (i.e. CanvasScaler)

                // it's not really in world space or screen space, it's in a RectTransform "UI space"
                // so we must manually convert our desired screen bounds to this UI space

                bool useCanvasResolution = __canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ConstantPixelSize;
                Vector2 screenSize = Vector2.zero;
                screenSize.x = useCanvasResolution ? __canvasScaler.referenceResolution.x : Screen.width;
                screenSize.y = useCanvasResolution ? __canvasScaler.referenceResolution.y : Screen.height;

                // calculate "half" values because we are measuring margins based on the center, like a radius
                var halfBubbleWidth = bubble.rect.width / 2;
                var halfBubbleHeight = bubble.rect.height / 2;

                // to calculate margin in UI-space pixels, use a % of the smaller screen dimension
                var margin = screenSize.x < screenSize.y ? screenSize.x * constrainToViewportMargin : screenSize.y * constrainToViewportMargin;

                // finally, clamp the screenPos fully within the screen bounds, while accounting for the bubble's rectTransform anchors
                screenPos.x = Mathf.Clamp(
                    screenPos.x,
                    margin + halfBubbleWidth - bubble.anchorMin.x * screenSize.x,
                    -(margin + halfBubbleWidth) - bubble.anchorMax.x * screenSize.x + screenSize.x
                );

                screenPos.y = Mathf.Clamp(
                    screenPos.y,
                    margin + halfBubbleHeight - bubble.anchorMin.y * screenSize.y,
                    -(margin + halfBubbleHeight) - bubble.anchorMax.y * screenSize.y + screenSize.y
                );
            }

            return screenPos;
        }
    }
}