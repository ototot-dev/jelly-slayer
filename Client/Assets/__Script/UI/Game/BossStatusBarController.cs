using UnityEngine;
using UniRx;
using UGUI.Rx;

namespace Game.UI
{
    [Template(path: "UI/template/boss-status-bar")]
    public class BossStatusBarController : StatusBarController
    {
        StatusBarData __heartPointBarData = new();
        StatusBarData __stanceBarData = new();
        BossStatusBarTemplate __template;

        public override void OnPreShow()
        {
            base.OnPreShow();

            __template = template as BossStatusBarTemplate;

            __heartPointBarData.barType = BarTypes.HeartPoint;
            __heartPointBarData.barRect = GetComponentById<RectTransform>("hp");
            __heartPointBarData.fillRect = GetComponentById<RectTransform>("hp-fill");
            __heartPointBarData.indicatorRect = GetComponentById<RectTransform>("hp-indicator");
            __heartPointBarData.indicatorStyleSelector = GetComponentById<ImageStyleSelector>("hp-indicator");
            __heartPointBarData.blurCanvanGroup = GetComponentById<CanvasGroup>("hp-blur");
            __heartPointBarData.numTextMesh = GetComponentById<TMPro.TextMeshProUGUI>("hp-num");

            __stanceBarData.barType = BarTypes.Stance;
            __stanceBarData.barRect = GetComponentById<RectTransform>("stance");
            __stanceBarData.fillRect = GetComponentById<RectTransform>("stance-fill");
            __stanceBarData.indicatorRect = GetComponentById<RectTransform>("stance-indicator");
            __stanceBarData.indicatorStyleSelector = GetComponentById<ImageStyleSelector>("stance-indicator");
            __stanceBarData.blurCanvanGroup = GetComponentById<CanvasGroup>("stance-blur");

            __heartPointBarData.prevValue = GameContext.Instance.playerCtrler.possessedBrain.BB.stat.heartPoint.Value;
            __stanceBarData.prevValue = GameContext.Instance.playerCtrler.possessedBrain.BB.stat.stance.Value;

            GameContext.Instance.playerCtrler.possessedBrain.BB.stat.heartPoint.Subscribe(_ => OnHeartPointChanged()).AddToHide(this);
            GameContext.Instance.playerCtrler.possessedBrain.BB.stat.stance.Subscribe(_ => OnStanceChangedHandler()).AddToHide(this);
        }

        public override void OnPostShow()
        {
            base.OnPostShow();

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (__heartPointBarData.blurFlashTween == null)
                    __heartPointBarData.blurCanvanGroup.alpha = Mathf.PerlinNoise1D(Time.time) * __template.heartPointBlurIntensity;
                if (__stanceBarData.blurFlashTween == null)
                    __stanceBarData.blurCanvanGroup.alpha = Mathf.PerlinNoise1D(Time.time * 0.8f) * __template.stanceBlurIntensity;
            }).AddToHide(this);            
        }

        protected override float GetBlurIntensity(BarTypes barType)
        {
            return barType switch
            {
                BarTypes.HeartPoint => __template.heartPointBlurIntensity,
                BarTypes.Stance => __template.stanceBlurIntensity,
                _ => 0f,
            };
        }

        void OnHeartPointChanged()
        {
            var stat = GameContext.Instance.playerCtrler.possessedBrain.BB.stat;
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

        void OnStanceChangedHandler()
        {
            var stat = GameContext.Instance.playerCtrler.possessedBrain.BB.stat;
            var stanceRatio = stat.stance.Value / stat.maxStance.Value;

            __stanceBarData.startSizeDelta = __stanceBarData.endSizeDelta;
            __stanceBarData.endSizeDelta = new Vector2(__stanceBarData.barRect.sizeDelta.x * stanceRatio, __stanceBarData.fillRect.sizeDelta.y);

            if (stat.stance.Value > __stanceBarData.prevValue)
            {
                ShowIndicator(__stanceBarData, "flash");
                StartFlashTransition(__stanceBarData, 0.2f);
            }

            __stanceBarData.startValue = __stanceBarData.prevValue;
            __stanceBarData.prevValue = stat.stance.Value;
            __stanceBarData.sizeDeltaTimeStamp = Time.time;

            if (__stanceBarData.sizeDeltaDisposable == null)
                StartSizeDeltaTransition(__stanceBarData, stanceRatio);
        }
    }
}