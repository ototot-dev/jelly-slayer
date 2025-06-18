using System;
using UnityEngine;
using UniRx;
using UGUI.Rx;
using System.Linq;
using DG.Tweening;
using ZLinq;

namespace Game.UI
{
    [Template(path: "UI/template/player-status-bar")]
    public class PlayerStatusBarController : StatusBarController
    {
        StatusBarData __heartPointBarData = new();
        StatusBarData __staminaBarData = new();
        StatusBarData __burstBarData = new();
        PlayerStatusBarTemplate __template;

        public override void OnPreShow()
        {
            base.OnPreShow();

            __template = template as PlayerStatusBarTemplate;

            __heartPointBarData.barType = BarTypes.HeartPoint;
            __heartPointBarData.barRect = GetComponentById<RectTransform>("hp");
            __heartPointBarData.fillRect = GetComponentById<RectTransform>("hp-fill");
            __heartPointBarData.indicatorRect = GetComponentById<RectTransform>("hp-indicator");
            __heartPointBarData.indicatorStyleSelector = GetComponentById<ImageStyleSelector>("hp-indicator");
            __heartPointBarData.blurCanvanGroup = GetComponentById<CanvasGroup>("hp-blur");
            __heartPointBarData.numTextMesh = GetComponentById<TMPro.TextMeshProUGUI>("hp-num");

            __staminaBarData.barType = BarTypes.Stamina;
            __staminaBarData.barRect = GetComponentById<RectTransform>("stamina");
            __staminaBarData.fillRect = GetComponentById<RectTransform>("stamina-fill");
            __staminaBarData.indicatorRect = GetComponentById<RectTransform>("stamina-indicator");
            __staminaBarData.indicatorStyleSelector = GetComponentById<ImageStyleSelector>("stamina-indicator");
            __staminaBarData.blurCanvanGroup = GetComponentById<CanvasGroup>("stamina-blur");

            __burstBarData.barType = BarTypes.Burst;
            __burstBarData.barRect = GetComponentById<RectTransform>("burst");
            __burstBarData.fillRect = GetComponentById<RectTransform>("burst-fill");
            __burstBarData.indicatorRect = GetComponentById<RectTransform>("burst-indicator");
            __burstBarData.indicatorStyleSelector = GetComponentById<ImageStyleSelector>("burst-indicator");
            __burstBarData.blurCanvanGroup = GetComponentById<CanvasGroup>("burst-blur");

            __heartPointBarData.prevValue = GameContext.Instance.playerCtrler.possessedBrain.BB.stat.heartPoint.Value;
            __staminaBarData.prevValue = GameContext.Instance.playerCtrler.possessedBrain.BB.stat.stamina.Value;
            __burstBarData.prevValue = GameContext.Instance.playerCtrler.possessedBrain.BB.stat.burst.Value;

            GameContext.Instance.playerCtrler.possessedBrain.BB.stat.heartPoint.Subscribe(_ => OnHeartPointChanged()).AddToHide(this);
            GameContext.Instance.playerCtrler.possessedBrain.BB.stat.stamina.Subscribe(_ => OnStaminaChangedHandler()).AddToHide(this);
            GameContext.Instance.playerCtrler.possessedBrain.BB.stat.burst.Subscribe(_ => OnBurstChangedHandler()).AddToHide(this);
        }

        public override void OnPostShow()
        {
            base.OnPostShow();

            Observable.EveryUpdate().Subscribe(_ =>
            {
                if (__heartPointBarData.blurFlashTween == null)
                    __heartPointBarData.blurCanvanGroup.alpha = Mathf.PerlinNoise1D(Time.time) * __template.heartPointBlurIntensity;
                if (__staminaBarData.blurFlashTween == null)
                    __staminaBarData.blurCanvanGroup.alpha = Mathf.PerlinNoise1D(Time.time * 0.5f) * __template.staminaBlurIntensity;
                if (__burstBarData.blurFlashTween == null)
                    __burstBarData.blurCanvanGroup.alpha = Mathf.PerlinNoise1D(Time.time * 0.8f) * __template.burstBlurIntensity;
            }).AddToHide(this);            
        }

        protected override float GetBlurIntensity(BarTypes barType)
        {
            return barType switch
            {
                BarTypes.HeartPoint => __template.heartPointBlurIntensity,
                BarTypes.Stamina => __template.staminaBlurIntensity,
                BarTypes.Burst => __template.burstBlurIntensity,
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

        void OnStaminaChangedHandler()
        {
            var stat = GameContext.Instance.playerCtrler.possessedBrain.BB.stat;
            var staminaRatio = stat.stamina.Value / stat.maxStamina.Value;

            __staminaBarData.startSizeDelta = __staminaBarData.endSizeDelta;
            __staminaBarData.endSizeDelta = new Vector2(__staminaBarData.barRect.sizeDelta.x * staminaRatio, __staminaBarData.fillRect.sizeDelta.y);

            ShowIndicator(__staminaBarData, "cursor");

            __staminaBarData.startValue = __staminaBarData.prevValue;
            __staminaBarData.prevValue = stat.stamina.Value;
            __staminaBarData.sizeDeltaTimeStamp = Time.time;

            if (__staminaBarData.sizeDeltaDisposable == null)
                StartSizeDeltaTransition(__staminaBarData, staminaRatio);
        }

        void OnBurstChangedHandler()
        {
            var stat = GameContext.Instance.playerCtrler.possessedBrain.BB.stat;
            var burstRatio = stat.burst.Value / stat.maxBurst.Value;

            __burstBarData.startSizeDelta = __burstBarData.endSizeDelta;
            __burstBarData.endSizeDelta = new Vector2(__burstBarData.barRect.sizeDelta.x * burstRatio, __burstBarData.fillRect.sizeDelta.y);

            if (stat.burst.Value > __burstBarData.prevValue)
            {
                ShowIndicator(__burstBarData, "flash");
                StartFlashTransition(__burstBarData, 0.2f);
            }

            __burstBarData.startValue = __burstBarData.prevValue;
            __burstBarData.prevValue = stat.burst.Value;
            __burstBarData.sizeDeltaTimeStamp = Time.time;

            if (__burstBarData.sizeDeltaDisposable == null)
                StartSizeDeltaTransition(__burstBarData, burstRatio);
        }
    }
}