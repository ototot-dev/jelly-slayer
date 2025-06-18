using UnityEngine;
using UniRx;
using UGUI.Rx;
using UnityEngine.UI;

namespace Game.UI
{
    [Template(path: "UI/template/floating-status-bar")]
    public class FloatingStatusBarController : StatusBarController
    {
        public FloatingStatusBarController(PawnBrainController hostBrain)
        {
            __hostBrain = hostBrain;
        }

        PawnBrainController __hostBrain;
        StatusBarData __heartPointBarData = new();
        FloatingStatusBarTemplate __template;
        RectTransform __templateRect;

        public override void OnPreShow()
        {
            base.OnPreShow();

            __template = template as FloatingStatusBarTemplate;
            __templateRect = template.transform as RectTransform;

            __heartPointBarData.barType = BarTypes.HeartPoint;
            __heartPointBarData.barRect = GetComponentById<RectTransform>("hp");
            __heartPointBarData.fillRect = GetComponentById<RectTransform>("hp-fill");
            __heartPointBarData.indicatorRect = GetComponentById<RectTransform>("hp-indicator");
            __heartPointBarData.indicatorStyleSelector = GetComponentById<ImageStyleSelector>("hp-indicator");
            __heartPointBarData.blurCanvanGroup = GetComponentById<CanvasGroup>("hp-blur");
            __heartPointBarData.numTextMesh = GetComponentById<TMPro.TextMeshProUGUI>("hp-num");
            __heartPointBarData.prevValue = GameContext.Instance.playerCtrler.possessedBrain.BB.stat.heartPoint.Value;

            __hostBrain.PawnBB.stat.heartPoint.Subscribe(_ => OnHeartPointChanged()).AddToHide(this);

            Observable.EveryLateUpdate().Subscribe(_ =>
            {
                __templateRect.anchoredPosition = WorldToAnchoredPosition(__templateRect, __hostBrain.GetStatusBarAttachPoint());

                if (__heartPointBarData.blurFlashTween == null)
                    __heartPointBarData.blurCanvanGroup.alpha = Mathf.PerlinNoise1D(Time.time) * __template.heartPointBlurIntensity;
            }).AddToHide(this);
        }

        protected override float GetBlurIntensity(BarTypes barType)
        {
            return barType switch
            {
                BarTypes.HeartPoint => __template.heartPointBlurIntensity,
                _ => 0f,
            };
        }

        void OnHeartPointChanged()
        {
            var stat = __hostBrain.PawnBB.stat;
            var heartPointRatio = stat.heartPoint.Value / stat.maxHeartPoint.Value;

            __heartPointBarData.startSizeDelta = __heartPointBarData.endSizeDelta;
            __heartPointBarData.endSizeDelta = new Vector2(__heartPointBarData.barRect.sizeDelta.x * heartPointRatio, __heartPointBarData.fillRect.sizeDelta.y);

            if (stat.heartPoint.Value < __heartPointBarData.prevValue)
            {
                StartFlashTransition(__heartPointBarData, 0.2f);
                ShowIndicator(__heartPointBarData, "flash");
            }
            else
            {
                ShowIndicator(__heartPointBarData, "cursor");
            }

            __heartPointBarData.startValue = __heartPointBarData.prevValue;
            __heartPointBarData.prevValue = stat.heartPoint.Value;
            __heartPointBarData.sizeDeltaTimeStamp = Time.time;

            if (__heartPointBarData.sizeDeltaDisposable == null)
                StartSizeDeltaTransition(__heartPointBarData, heartPointRatio);
        }
        
        CanvasScaler __canvasScaler;

        /// <summary>Calculates where to put dialogue bubble based on worldPosition and any desired screen margins. 
        /// Ensure "constrainToViewportMargin" is between 0.0f-1.0f (% of screen) to constrain to screen, or value of -1 lets bubble go off-screen.</summary>
        Vector2 WorldToAnchoredPosition(RectTransform rect, Vector3 worldPos, float constrainToViewportMargin = -1f)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle( 
                rect.parent.GetComponent<RectTransform>(), // calculate local point inside parent... NOT inside the dialogue bubble itself
                GameContext.Instance.cameraCtrler.gameCamera.WorldToScreenPoint(worldPos), 
                null, 
                out Vector2 screenPos
            );

            __canvasScaler ??= GameContext.Instance.canvasManager.GetComponent<CanvasScaler>();

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
                var halfBubbleWidth = rect.rect.width / 2;
                var halfBubbleHeight = rect.rect.height / 2;

                // to calculate margin in UI-space pixels, use a % of the smaller screen dimension
                var margin = screenSize.x < screenSize.y ? screenSize.x * constrainToViewportMargin : screenSize.y * constrainToViewportMargin;

                // finally, clamp the screenPos fully within the screen bounds, while accounting for the bubble's rectTransform anchors
                screenPos.x = Mathf.Clamp(
                    screenPos.x,
                    margin + halfBubbleWidth - rect.anchorMin.x * screenSize.x,
                    -(margin + halfBubbleWidth) - rect.anchorMax.x * screenSize.x + screenSize.x
                );

                screenPos.y = Mathf.Clamp(
                    screenPos.y,
                    margin + halfBubbleHeight - rect.anchorMin.y * screenSize.y,
                    -(margin + halfBubbleHeight) - rect.anchorMax.y * screenSize.y + screenSize.y
                );
            }

            return screenPos;
        }
    }
}