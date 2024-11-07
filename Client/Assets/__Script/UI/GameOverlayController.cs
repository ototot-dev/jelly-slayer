using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UGUI.Rx;
using Unity.Linq;
using System.Linq;
using DG.Tweening;

namespace Game
{

    /// <summary>
    /// 
    /// </summary>
    [Template(path: "UI/game-overlay")]
    public class GameOverlayController : Controller
    {
        public override void OnPreLoad(List<IObservable<Controller>> loader)
        {
            base.OnPreLoad(loader);

            // loader.Add(Resources.LoadAsync("BG").AsAsyncOperationObservable().Select(_ => this));
            // loader.Add(Resources.LoadAsync("Block Spritesheet - 1").AsAsyncOperationObservable().Select(_ => this));
        }

        GameObject[] __heartPoints;
        GameObject[] __smallHeartPoints;
        GameObject[] __actionPoints;
        GameObject[] __bombPoints;
        Image[] __bombPointFillImgs;
        TMPro.TextMeshProUGUI __coinPoint;

        public override void OnPreShow()
        {
            base.OnPreShow();

            __smallHeartPoints = GetComponentById<RectTransform>("small-heart-point").gameObject.Children().ToArray();
            __heartPoints = GetComponentById<RectTransform>("heart-point").gameObject.Children().ToArray();
            __actionPoints = GetComponentById<RectTransform>("action-point").gameObject.Children().ToArray();
            __bombPoints = GetComponentById<RectTransform>("bomb-point").gameObject.Children().ToArray();
            __bombPointFillImgs = __bombPoints.Select(b => b.Descendants().First(d => d.name == "fill").GetComponent<Image>()).ToArray();
            __coinPoint = GetComponentById<RectTransform>("coin-icon").gameObject.AfterSelf().First(d => d.name == "value").GetComponent<TMPro.TextMeshProUGUI>();

            // if (Context.Instance.bgCtrler == null)
            //     Context.Instance.bgCtrler = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("BG")).GetComponent<BgController>();

            GameContext.Instance.playerCtrler.onPossessed += (hero) =>
            {
                var heroBrain = hero.GetComponent<HeroBrain>();

                Debug.Assert(heroBrain != null);

                RefreshHeartPointCount(heroBrain);
                RefreshActionPoint(heroBrain);
                RefreshBombPoint(heroBrain);

                heroBrain.PawnHP.heartPoint.Subscribe(v =>
                {
                    // GetComponentById<TMPro.TextMeshProUGUI>("gold-value").text = $"{v} <i>g</i>";
                    // GetComponentById<TMPro.TextMeshProUGUI>("gold-value").enabled = false;
                }).AddTo(GameContext.Instance.playerCtrler);

                heroBrain.PawnHP.heartPoint.Skip(1).Subscribe(_ => RefreshHeartPointCount(heroBrain)).AddTo(hero);
                // heroBrain.BB.body.coinPoint.Subscribe(v => __coinPoint.text = $"<b>{v}</b> <i>G</i>");
                // heroBrain.BB.action.currActionPointNum.Skip(1).Subscribe(_ => RefreshActionPoint(heroBrain)).AddTo(hero);
                // heroBrain.BB.action.currBombNum.Skip(1).Subscribe(_ => RefreshBombPoint(heroBrain)).AddTo(hero);
                // heroBrain.BB.action.bombFillAlpha.Subscribe(_ => RefreshBombFillAlpha(heroBrain)).AddTo(hero);
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="heroBrain"></param>
        void RefreshHeartPointCount(HeroBrain heroBrain)
        {
            // var maxHeartPoint = Mathf.CeilToInt(heroBrain.PawnBB.HeartPoint);

            // var heartPointScale = 1;

            // if (heartPointScale == 1)
            // {
            //     for (int i = 0; i < __smallHeartPoints.Length; i++)
            //     {
            //         if (i < heartPointNum)
            //         {
            //             var query = __smallHeartPoints[i].Children().First().GetComponent<ImageStyleSelector>().query;

            //             if (query.activeStates.Contains("hide"))
            //             {
            //                 query.activeStates.Remove("hide");
            //                 query.activeStates.Add("show");
            //                 query.Apply();
            //             }
            //         }
            //         else
            //         {
            //             var query = __smallHeartPoints[i].Children().First().GetComponent<ImageStyleSelector>().query;

            //             if (query.activeStates.Contains("show"))
            //             {
            //                 query.activeStates.Remove("show");
            //                 query.activeStates.Add("hide");
            //                 query.Apply();
            //             }
            //         }
            //     }

            //     GetComponentById<RectTransform>("small-heart-point").gameObject.SetActive(true);
            //     GetComponentById<RectTransform>("heart-point").gameObject.SetActive(false);
            //     GetComponentById<RectTransform>("action-point").DOAnchorPos(new Vector2(25, -65), 0.5f);
            //     GetComponentById<RectTransform>("bomb-point").DOAnchorPos(new Vector2(25, -95), 0.5f);
            // }
            // else
            // {
            //     for (int i = 0; i < __heartPoints.Length; i++)
            //     {
            //         if (i < heartPointNum)
            //         {
            //             var fillImg = __heartPoints[i].Descendants().First(d => d.name == "fill").GetComponent<Image>();

            //             if (i == heartPointNum - 1)
            //             {
            //                 fillImg.fillAmount = (heartPointScale - (maxHeartPoint - heroBrain.PawnHP.currHeartPoint.Value)) / (float)heartPointScale;

            //                 if (fillImg.fillAmount < 1)
            //                 {
            //                     (fillImg.transform.parent as RectTransform).DOShakeAnchorPos(0.5f, 5)
            //                         .OnComplete(() => (fillImg.transform.parent as RectTransform).anchoredPosition = Vector2.zero);
            //                 }
            //             }
            //             else
            //             {
            //                 fillImg.fillAmount = 1;
            //             }

            //             var query = __heartPoints[i].Children().First().GetComponent<ImageStyleSelector>().query;

            //             if (query.activeStates.Contains("hide"))
            //             {
            //                 query.activeClasses.Clear();
            //                 query.activeClasses.Add(i == 5 ? "heart-point-row" : "heart-point-column");

            //                 query.activeStates.Clear();
            //                 query.activeStates.Add("show");
            //                 query.Apply();
            //             }
            //         }
            //         else
            //         {
            //             var query = __heartPoints[i].Children().First().GetComponent<ImageStyleSelector>().query;

            //             if (query.activeStates.Contains("show"))
            //             {
            //                 query.activeClasses.Clear();
            //                 query.activeClasses.Add("heart-point");

            //                 query.activeStates.Clear();
            //                 query.activeStates.Add("hide");
            //                 query.Apply();
            //             }
            //         }
            //     }

            //     GetComponentById<RectTransform>("small-heart-point").gameObject.SetActive(false);
            //     // GetComponentById<RectTransform>("heart-point").gameObject.SetActive(true);

            //     if (heartPointNum <= 5)
            //     {
            //         GetComponentById<RectTransform>("action-point").DOAnchorPos(new Vector2(25, -100), 0.5f);
            //         GetComponentById<RectTransform>("bomb-point").DOAnchorPos(new Vector2(15, -230), 0.5f);
            //     }
            //     else
            //     {
            //         GetComponentById<RectTransform>("action-point").DOAnchorPos(new Vector2(25, -165), 0.5f);
            //         GetComponentById<RectTransform>("bomb-point").DOAnchorPos(new Vector2(15, -305), 0.5f);
            //     }
            // }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="heroBrain"></param>
        void RefreshActionPoint(HeroBrain heroBrain)
        {
            // GetComponentById<RectTransform>("action-point").gameObject.SetActive(true);

            // for (int i = 0; i < __actionPoints.Length; i++)
            // {
            //     if (i < heroBrain.BB.action.maxActionPointNum.Value)
            //     {
            //         __actionPoints[i].SetActive(true);

            //         var imgSelector = __actionPoints[i].Children().First().GetComponent<ImageStyleSelector>();

            //         imgSelector.query.activeStates.Add("show");
            //         imgSelector.query.Apply();

            //         var fillImgSelector = __actionPoints[i].Children().First().Children().First().GetComponent<ImageStyleSelector>();

            //         if (i < heroBrain.BB.action.currActionPointNum.Value)
            //         {
            //             if (fillImgSelector.query.activeClasses.Contains("action-point-unfill"))
            //             {
            //                 (imgSelector.transform as RectTransform).DOShakeAnchorPos(0.5f, 5)
            //                     .OnComplete(() => (imgSelector.transform as RectTransform).anchoredPosition = Vector2.zero);
            //             }

            //             fillImgSelector.query.activeClasses.Remove("action-point-unfill");
            //             fillImgSelector.query.activeClasses.Add("action-point-fill");
            //             fillImgSelector.query.Apply();
            //         }
            //         else
            //         {
            //             fillImgSelector.query.activeClasses.Remove("action-point-fill");
            //             fillImgSelector.query.activeClasses.Add("action-point-unfill");
            //             fillImgSelector.query.Apply();
            //         }
            //     }
            //     else
            //     {
            //         __actionPoints[i].SetActive(false);
            //     }
            // }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="heroBrain"></param>
        void RefreshBombPoint(HeroBrain heroBrain)
        {
            // GetComponentById<RectTransform>("bomb-point").gameObject.SetActive(true);

            // for (int i = 0; i < __bombPoints.Length; i++)
            // {
            //     if (i < heroBrain.BB.action.maxBombNum.Value)
            //     {
            //         __bombPoints[i].SetActive(true);

            //         var imgSelector = __bombPoints[i].Children().First().GetComponent<ImageStyleSelector>();

            //         imgSelector.query.activeStates.Add("show");
            //         imgSelector.query.Apply();

            //         var bombPointFx = __bombPoints[i].Descendants().First(d => d.name == "sparkle-fx").GetComponent<ParticleSystem>();

            //         if (i + 1 <= heroBrain.BB.action.currBombNum.Value)
            //         {
            //             if (!bombPointFx.isPlaying)
            //             {
            //                 bombPointFx.Play();

            //                 (imgSelector.transform as RectTransform).DOShakeAnchorPos(0.5f, 5)
            //                     .OnComplete(() => (imgSelector.transform as RectTransform).anchoredPosition = Vector2.zero);
            //             }
            //         }
            //         else
            //         {   
            //             bombPointFx.Stop();
            //         }
            //     }
            //     else
            //     {
            //         __bombPoints[i].SetActive(false);
            //         __bombPoints[i].Descendants().First(d => d.name == "sparkle-fx").GetComponent<ParticleSystem>().Stop();
            //     }
            // }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="heroBrain"></param>
        void RefreshBombFillAlpha(HeroBrain heroBrain)
        {
            // if (heroBrain.BB.action.currBombNum.Value < heroBrain.BB.action.maxBombNum.Value)
            //     __bombPointFillImgs[heroBrain.BB.action.currBombNum.Value].fillAmount = Mathf.Clamp01(heroBrain.BB.action.bombFillAlpha.Value);
        }

        public override void OnPostShow()
        {
            base.OnPostShow();

            Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(_ =>
            {
                // var prefab = GameContext.Instance.playerCtrler.reservedPawnPrefab;
                // var spawnTM = GameContext.Instance.playerCtrler.transform;

                // GameContext.Instance.playerCtrler.PossessPawn(GameObject.Instantiate(prefab, spawnTM.position, spawnTM.rotation));
            }).AddTo(template);

            // GameContext.Instance.playerCtrler.gainHeartPoint.Subscribe(v =>
            // {
            //     GetComponentById<TMPro.TextMeshProUGUI>("heart-point-gain").text = v.ToString();
            // }).AddTo(GameContext.Instance.playerCtrler);

            // GameContext.Instance.playerCtrler.lostHeartPoint.Subscribe(v =>
            // {
            //     GetComponentById<TMPro.TextMeshProUGUI>("heart-point-gain").text = v.ToString();
            // }).AddTo(GameContext.Instance.playerCtrler);

            // GameContext.Instance.playerCtrler.onPossessed += (hero) =>
            // {

            //     var bpmValue = GetComponentById<TMPro.TextMeshProUGUI>("bpm-value");
            //     var bpmLineDrawer = GetComponentById<BpmLineDrawer>("bpm-line-drawer");

            //     __bpmDisposable = pawn.GetComponent<PawnBlackboard>().bpm.currBpm.Subscribe(v =>
            //     {
            //         var bpm = Mathf.FloorToInt(v);

            //         bpmValue.text = bpm.ToString();
            //         bpmLineDrawer.bpm = bpm;
            //     }).AddTo(pawn);

            //     var lineDrawer = GetComponentById<BpmLineDrawer>("bpm-line-drawer");

            //     Observable.EveryUpdate().Subscribe(_ =>
            //     {
            //         GetComponentById<BpmLineDrawer>("bpm-line-drawer")?.SetVerticesDirty();
            //     }).AddTo(pawn);

            //     pawn.GetComponent<PawnBlackboard>().spawn.lifeTime.Subscribe(v => __lifeTime.Value = Mathf.CeilToInt(v)).AddTo(pawn);

            //     var bpmCtrler = pawn.GetComponent<PawnBpmController>();

            //     if (bpmCtrler != null)
            //     {
            //         var imgStyleSelector = GetComponentById<ImageStyleSelector>("bpm-bg");

            //         bpmCtrler.onHeartBeat += () =>
            //         {
            //             imgStyleSelector.query.activeClasses.Clear();
            //             imgStyleSelector.query.Apply();

            //             imgStyleSelector.query.activeClasses.Add("bpm-heartbeat");
            //             imgStyleSelector.query.Apply();

            //             lineDrawer.pulseTimeStamp = Time.realtimeSinceStartup;
            //             lineDrawer.SetVerticesDirty();
            //         };
            //     }
            //     else
            //     {
            //         bpmValue.text = "-";
            //     }

            //     var unbalanceSlider = GetComponentById<Slider>("bpm-unbalance");

            //     pawn.GetComponent<PawnBlackboard>().bpm.unbalanceFactor.Subscribe(v => 
            //     {
            //         unbalanceSlider.value = v;
            //     }).AddTo(pawn);
            // };

            // __lifeTime.Subscribe(v =>
            // {
            //     GetComponentById<TMPro.TextMeshProUGUI>("life-time").text = string.Format("{0:D2}:{1:D2}", TimeSpan.FromSeconds(v).Minutes, TimeSpan.FromSeconds(v).Seconds);
            // }).AddTo(template);

            GameContext.Instance.playerCtrler.onUnpossessed += (_) =>
            {
            };
        }

        IntReactiveProperty __lifeTime = new();
    }
}