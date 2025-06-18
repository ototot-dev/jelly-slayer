using System;
using UnityEngine;
using UniRx;
using UGUI.Rx;
using System.Linq;
using DG.Tweening;
using ZLinq;

namespace Game.UI
{
    public class StatusBarController : Controller
    {
        public enum BarTypes
        {
            None,
            HeartPoint,
            Stamina,
            Burst,
            Stance,
        }

        public class StatusBarData
        {
            public BarTypes barType;
            public float prevValue;
            public float startValue;
            public float sizeDeltaTimeStamp;
            public Vector2 startSizeDelta;
            public Vector2 endSizeDelta;
            public RectTransform barRect;
            public RectTransform fillRect;
            public RectTransform indicatorRect;
            public StyleSelector indicatorStyleSelector;
            public CanvasGroup blurCanvanGroup;
            public TMPro.TextMeshProUGUI numTextMesh;
            public IDisposable sizeDeltaDisposable;
            public DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> blurFlashTween;
        }

        protected virtual float GetBlurIntensity(BarTypes barType) => 1f;

        protected IDisposable StartSizeDeltaTransition(StatusBarData barData, float fillRatio = 1f)
        {
            Debug.Assert(barData.sizeDeltaDisposable == null);

            return barData.sizeDeltaDisposable = Observable.EveryUpdate().TakeWhile(_ => !Mathf.Approximately(barData.fillRect.sizeDelta.x, barData.endSizeDelta.x))
                .DoOnCancel(() =>
                {
                    barData.sizeDeltaDisposable = null;
                })
                .DoOnCompleted(() =>
                {
                    barData.fillRect.sizeDelta = barData.endSizeDelta;
                    barData.sizeDeltaDisposable = null;
                })
                .Subscribe(_ =>
                {
                    var lerpAlpha = Mathf.Clamp01((Time.time - barData.sizeDeltaTimeStamp) / 0.2f);
                    barData.fillRect.sizeDelta = Vector2.Lerp(barData.startSizeDelta, barData.endSizeDelta, lerpAlpha);

                    if (barData.fillRect.sizeDelta.x <= 0f && barData.fillRect.gameObject.activeSelf)
                        barData.fillRect.gameObject.SetActive(false);
                    else if (barData.fillRect.sizeDelta.x > 0f && !barData.fillRect.gameObject.activeSelf)
                        barData.fillRect.gameObject.SetActive(true);

                    if (barData.numTextMesh != null && barData.barType == BarTypes.HeartPoint)
                    {
                        var tempHeartPoint = Mathf.Lerp(barData.startValue, GameContext.Instance.playerCtrler.possessedBrain.BB.stat.heartPoint.Value, lerpAlpha);
                        barData.numTextMesh.text = $"{Mathf.FloorToInt(tempHeartPoint)} / {GameContext.Instance.playerCtrler.possessedBrain.BB.stat.maxHeartPoint.Value}";
                    }
                }).AddToHide(this);
        }

        protected DG.Tweening.Core.TweenerCore<float, float, DG.Tweening.Plugins.Options.FloatOptions> StartFlashTransition(StatusBarData barData, float duration)
        {
            barData.blurFlashTween?.Kill();

            return barData.blurFlashTween = DOTween.To(() => barData.blurCanvanGroup.alpha, v => barData.blurCanvanGroup.alpha = v, GetBlurIntensity(barData.barType), duration)
                .SetEase(Ease.OutQuart)
                .SetLoops(2, LoopType.Yoyo)
                .OnComplete(() => barData.blurFlashTween = null);
        }

        protected void ShowIndicator(StatusBarData barData, string activeState)
        {
            if (activeState == "flash")
            {
                if (barData.endSizeDelta.x > barData.startSizeDelta.x)
                {
                    barData.indicatorRect.anchoredPosition = new(barData.startSizeDelta.x, barData.indicatorRect.anchoredPosition.y);
                    barData.indicatorRect.sizeDelta = new(Mathf.Abs(barData.startSizeDelta.x - barData.endSizeDelta.x), barData.indicatorRect.sizeDelta.y);
                }
                else
                {
                    barData.indicatorRect.anchoredPosition = new(barData.endSizeDelta.x, barData.indicatorRect.anchoredPosition.y);
                    barData.indicatorRect.sizeDelta = new(Mathf.Abs(barData.endSizeDelta.x - barData.startSizeDelta.x), barData.indicatorRect.sizeDelta.y);
                }
            }
            else
            {
                barData.indicatorRect.anchoredPosition = new(barData.endSizeDelta.x, barData.indicatorRect.anchoredPosition.y);
                barData.indicatorRect.sizeDelta = new(5f, barData.indicatorRect.sizeDelta.y);
            }

            var indicatorQuery = barData.indicatorStyleSelector.query;

            indicatorQuery.activeStates.Remove(activeState == "flash" ? "cursor" : "flash");
            indicatorQuery.activeStates.Add(activeState == "flash" ? "flash" : "cursor");
            indicatorQuery.Execute();
        }
    }
}