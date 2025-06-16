using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UGUI.Rx;
using Unity.Linq;
using System.Linq;
using GoogleSheet.Type;

namespace Game.UI
{
    [Template(path: "UI/template/status-bar")]
    public class StatusBarController : Controller
    {
        float __prevHeartPoint;
        float __prevStamina;
        float __prevBurst;

        public override void OnPreShow()
        {
            base.OnPreShow();

            __prevHeartPoint = GameContext.Instance.playerCtrler.possessedBrain.BB.stat.heartPoint.Value;
            __prevStamina = GameContext.Instance.playerCtrler.possessedBrain.BB.stat.stamina.Value;
            __prevBurst = GameContext.Instance.playerCtrler.possessedBrain.BB.stat.burst.Value;

            GameContext.Instance.playerCtrler.possessedBrain.BB.stat.heartPoint.Subscribe(_ => OnHeartPointChanged()).AddToHide(this);
            GameContext.Instance.playerCtrler.possessedBrain.BB.stat.stamina.Subscribe(_ => OnStaminaChangedHandler()).AddToHide(this);
            GameContext.Instance.playerCtrler.possessedBrain.BB.stat.burst.Subscribe(_ => OnBurstChangedHandler()).AddToHide(this);
        }

        public override void OnPostShow()
        {
            base.OnPostShow();

            var query = GetComponentById<StyleSelector>("hp-blur").query;

            //* HP 바에 자연스럽게 밝아졌다 어두워지는 효과 추가
            query.activeStates.Add("flow");
            query.Execute();
        }

        IDisposable __heartPointSizeDeltaDisposable;
        Vector2 __heartPointStartSizeDelta;
        Vector2 __heartPointEndSizeDelta;
        float __heartPointSizeDeltaTimeStamp;
        float __startHeartPoint;
        RectTransform __heartPointBarRect;
        RectTransform __heartPointFillRect;
        RectTransform __heartPointIndicatorRect;

        void OnHeartPointChanged()
        {
            __heartPointBarRect ??= GetComponentById<RectTransform>("hp");
            __heartPointFillRect ??= GetComponentById<RectTransform>("hp-fill");
            __heartPointIndicatorRect ??= GetComponentById<RectTransform>("hp-indicator");

            var heartPointRatio = GameContext.Instance.playerCtrler.possessedBrain.BB.stat.heartPoint.Value / GameContext.Instance.playerCtrler.possessedBrain.BB.stat.maxHeartPoint.Value;

            __startHeartPoint = __prevHeartPoint;
            __prevHeartPoint = GameContext.Instance.playerCtrler.possessedBrain.BB.stat.heartPoint.Value;

            __heartPointSizeDeltaTimeStamp = Time.time;

            if (__heartPointSizeDeltaDisposable == null)
            {
                __heartPointStartSizeDelta = __heartPointFillRect.sizeDelta;
                __heartPointEndSizeDelta = new Vector2(__heartPointBarRect.sizeDelta.x * heartPointRatio, __heartPointFillRect.sizeDelta.y);

                __heartPointSizeDeltaDisposable = Observable.EveryUpdate().TakeWhile(_ => !Mathf.Approximately(__heartPointFillRect.sizeDelta.x, __heartPointEndSizeDelta.x))
                    .DoOnCancel(() =>
                    {
                        __heartPointFillRect.sizeDelta = __heartPointEndSizeDelta;
                        __heartPointSizeDeltaDisposable = null;
                    })
                    .DoOnCompleted(() =>
                    {
                        __heartPointFillRect.sizeDelta = __heartPointEndSizeDelta;
                        __heartPointSizeDeltaDisposable = null;
                    })
                    .Subscribe(_ =>
                    {
                        var lerpAlpha = Mathf.Clamp01((Time.time - __heartPointSizeDeltaTimeStamp) / 0.2f);
                        __heartPointFillRect.sizeDelta = Vector2.Lerp(__heartPointStartSizeDelta, __heartPointEndSizeDelta, lerpAlpha);

                        var tempHeartPoint = Mathf.Lerp(__startHeartPoint, GameContext.Instance.playerCtrler.possessedBrain.BB.stat.heartPoint.Value, lerpAlpha);
                        GetComponentById<TMPro.TextMeshProUGUI>("hp-num").text = $"{Mathf.FloorToInt(tempHeartPoint)} / {GameContext.Instance.playerCtrler.possessedBrain.BB.stat.maxHeartPoint.Value}";
                    }).AddToHide(this);
            }
            else
            {
                __heartPointStartSizeDelta = __heartPointEndSizeDelta;
                __heartPointEndSizeDelta = new Vector2(__heartPointBarRect.sizeDelta.x * heartPointRatio, __heartPointFillRect.sizeDelta.y);
            }

            if (__heartPointEndSizeDelta.x < __heartPointStartSizeDelta.x) //* 데미지인 경우
            {
                __heartPointIndicatorRect.anchoredPosition = new(__heartPointEndSizeDelta.x, __heartPointIndicatorRect.anchoredPosition.y);
                __heartPointIndicatorRect.sizeDelta = new(Mathf.Abs(__heartPointEndSizeDelta.x - __heartPointStartSizeDelta.x), __heartPointIndicatorRect.sizeDelta.y);
            }
            else
            {
                __heartPointIndicatorRect.anchoredPosition = new(__heartPointEndSizeDelta.x, __heartPointIndicatorRect.anchoredPosition.y);
                __heartPointIndicatorRect.sizeDelta = new(5f, __heartPointIndicatorRect.sizeDelta.y);
            }

            var blurQuery = GetComponentById<StyleSelector>("hp-blur").query;
            var indicatorQuery = GetComponentById<ImageStyleSelector>("hp-indicator").query;

            blurQuery.activeStates.Remove("flash-hp");
            blurQuery.Execute();

            indicatorQuery.activeStates.Clear();
            indicatorQuery.Execute();

            Observable.NextFrame().Subscribe(_ =>
            {
                indicatorQuery.activeStates.Add(__heartPointEndSizeDelta.x < __heartPointStartSizeDelta.x ? "flash" : "cursor");
                indicatorQuery.Execute();

                blurQuery.activeStates.Add("flash-hp");
                blurQuery.Execute();
            }).AddToHide(this);
        }

        IDisposable __staminaSizeDeltaDisposable;
        Vector2 __staminaStartSizeDelta;
        Vector2 __staminaEndSizeDelta;
        float __staminaSizeDeltaTimeStamp;
        RectTransform __staminaBarRect;
        RectTransform __staminaFillRect;
        RectTransform __staminaIndicatorRect;

        void OnStaminaChangedHandler()
        {
            __staminaBarRect ??= GetComponentById<RectTransform>("stamina");
            __staminaFillRect ??= GetComponentById<RectTransform>("stamina-fill");
            __staminaIndicatorRect ??= GetComponentById<RectTransform>("stamina-indicator");

            var staminaRatio = GameContext.Instance.playerCtrler.possessedBrain.BB.stat.stamina.Value / GameContext.Instance.playerCtrler.possessedBrain.BB.stat.maxStamina.Value;

            __staminaSizeDeltaTimeStamp = Time.time;

            if (__staminaSizeDeltaDisposable == null)
            {
                __staminaStartSizeDelta = __staminaFillRect.sizeDelta;
                __staminaEndSizeDelta = new Vector2(__staminaBarRect.sizeDelta.x * staminaRatio, __staminaFillRect.sizeDelta.y);

                __staminaSizeDeltaDisposable = Observable.EveryUpdate().TakeWhile(_ => !Mathf.Approximately(__staminaFillRect.sizeDelta.x, __staminaEndSizeDelta.x))
                    .DoOnCancel(() =>
                    {
                        __staminaFillRect.sizeDelta = __staminaEndSizeDelta;
                        __staminaSizeDeltaDisposable = null;
                    })
                    .DoOnCompleted(() =>
                    {
                        __staminaFillRect.sizeDelta = __staminaEndSizeDelta;
                        __staminaSizeDeltaDisposable = null;
                    })
                    .Subscribe(_ =>
                    {
                        var lerpAlpha = Mathf.Clamp01((Time.time - __staminaSizeDeltaTimeStamp) / 0.2f);
                        __staminaFillRect.sizeDelta = Vector2.Lerp(__staminaStartSizeDelta, __staminaEndSizeDelta, lerpAlpha);
                    }).AddToHide(this);
            }
            else
            {
                __staminaStartSizeDelta = __staminaEndSizeDelta;
                __staminaEndSizeDelta = new Vector2(__staminaBarRect.sizeDelta.x * staminaRatio, __staminaFillRect.sizeDelta.y);
            }

            __staminaIndicatorRect.anchoredPosition = new(__staminaEndSizeDelta.x, __staminaIndicatorRect.anchoredPosition.y);
            __staminaIndicatorRect.sizeDelta = new(5f, __staminaIndicatorRect.sizeDelta.y);

            var blurQuery = GetComponentById<StyleSelector>("stamina-blur").query;
            var indicatorQuery = GetComponentById<ImageStyleSelector>("stamina-indicator").query;

            blurQuery.activeStates.Clear();
            blurQuery.Execute();

            indicatorQuery.activeStates.Clear();
            indicatorQuery.Execute();

            Observable.NextFrame().Subscribe(_ =>
            {
                blurQuery.activeStates.Add("flash-stamina");
                blurQuery.Execute();

                indicatorQuery.activeStates.Add("cursor");
                indicatorQuery.Execute();
            }).AddToHide(this);
        }
        
        IDisposable __burstSizeDeltaDisposable;
        Vector2 __burstStartSizeDelta;
        Vector2 __burstEndSizeDelta;
        float __burstSizeDeltaTimeStamp;
        RectTransform __burstBarRect;
        RectTransform __burstFillRect;
        RectTransform __burstIndicatorRect;

        void OnBurstChangedHandler()
        {
            __burstBarRect ??= GetComponentById<RectTransform>("burst");
            __burstFillRect ??= GetComponentById<RectTransform>("burst-fill");
            __burstIndicatorRect ??= GetComponentById<RectTransform>("burst-indicator");

            var burstRatio = GameContext.Instance.playerCtrler.possessedBrain.BB.stat.burst.Value / GameContext.Instance.playerCtrler.possessedBrain.BB.stat.maxBurst.Value;

            __burstSizeDeltaTimeStamp = Time.time;

            if (__burstSizeDeltaDisposable == null)
            {
                __burstStartSizeDelta = __burstFillRect.sizeDelta;
                __burstEndSizeDelta = new Vector2(__burstBarRect.sizeDelta.x * burstRatio, __burstFillRect.sizeDelta.y);

                __burstSizeDeltaDisposable = Observable.EveryUpdate().TakeWhile(_ => !Mathf.Approximately(__burstFillRect.sizeDelta.x, __burstEndSizeDelta.x))
                    .DoOnCancel(() =>
                    {
                        __burstFillRect.sizeDelta = __burstEndSizeDelta;
                        __burstSizeDeltaDisposable = null;
                    })
                    .DoOnCompleted(() =>
                    {
                        __burstFillRect.sizeDelta = __burstEndSizeDelta;
                        __burstSizeDeltaDisposable = null;
                    })
                    .Subscribe(_ =>
                    {
                        var lerpAlpha = Mathf.Clamp01((Time.time - __burstSizeDeltaTimeStamp) / 0.2f);
                        __burstFillRect.sizeDelta = Vector2.Lerp(__burstStartSizeDelta, __burstEndSizeDelta, lerpAlpha);

                        if (__burstFillRect.sizeDelta.x <= 0f && __burstFillRect.gameObject.activeSelf)
                            __burstFillRect.gameObject.SetActive(false);
                        else if (__burstFillRect.sizeDelta.x > 0f && !__burstFillRect.gameObject.activeSelf)
                            __burstFillRect.gameObject.SetActive(true);
                    }).AddToHide(this);
            }
            else
            {
                __burstStartSizeDelta = __burstEndSizeDelta;
                __burstEndSizeDelta = new Vector2(__burstBarRect.sizeDelta.x * burstRatio, __burstFillRect.sizeDelta.y);
            }

            __burstIndicatorRect.anchoredPosition = new(__burstStartSizeDelta.x, __burstIndicatorRect.anchoredPosition.y);
            __burstIndicatorRect.sizeDelta = new(Mathf.Abs(__burstEndSizeDelta.x - __burstStartSizeDelta.x), __burstIndicatorRect.sizeDelta.y);

            var blurQuery = GetComponentById<StyleSelector>("burst-blur").query;
            var indicatorQuery = GetComponentById<ImageStyleSelector>("burst-indicator").query;

            blurQuery.activeStates.Clear();
            blurQuery.Execute();

            indicatorQuery.activeStates.Clear();
            indicatorQuery.Execute();

            var showIndicator = GameContext.Instance.playerCtrler.possessedBrain.BB.stat.burst.Value > __prevBurst;

            __prevBurst = GameContext.Instance.playerCtrler.possessedBrain.BB.stat.burst.Value;

            Observable.NextFrame().Subscribe(_ =>
            {
                if (GameContext.Instance.playerCtrler.possessedBrain.BB.stat.burst.Value >= GameContext.Instance.playerCtrler.possessedBrain.BB.stat.maxBurst.Value)
                    blurQuery.activeStates.Add("flow-burst");

                blurQuery.activeStates.Add("flash-burst");
                blurQuery.Execute();

                if (showIndicator)
                {
                    indicatorQuery.activeStates.Add("flash");
                    indicatorQuery.Execute();
                }
            }).AddToHide(this);
        }
    }
}