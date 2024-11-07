using System;
using System.Linq;
using DG.Tweening;
using Retween.Rx;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace Game
{

    /// <summary>
    /// 
    /// </summary>
    public class CubeDropHandler : DropItemController
    {

    //     /// <summary>
    //     /// 
    //     /// </summary>
    //     public Sprite[] spriteTable;

    //     /// <summary>
    //     /// 
    //     /// </summary>
    //     public CubeController.CubeTypes cubeType;

    //     /// <summary>
    //     /// 
    //     /// </summary>
    //     public int coinPoint = 1;

    //     /// <summary>
    //     /// 
    //     /// </summary>
    //     public float shakingScale = 1;

    //     /// <summary>
    //     /// 
    //     /// </summary>
    //     /// <param name="focused"></param>
    //     public void SetFocused(bool focused)
    //     {
    //         meshRenderer.material.SetVector("_EmissiveColor", new Vector4(focused ? 1 : 0, 0, 0));
    //         meshRenderer.material.SetFloat("_DitherAlpha", focused ? 0.5f : 1);
    //         shakingScale = focused ? 2 : 1;
    //     }
        
    //     /// <summary>
    //     /// 
    //     /// </summary>
    //     /// <param name="picker"></param>
    //     /// <returns></returns>
    //     public override bool TryPick(GameObject picker)
    //     {
    //         if (owner.Value != null)
    //             return false;

    //         if (picker.TryGetComponent<HeroBrain>(out var heroBrain) && heroBrain.CubeStackCtrler.TryStackCube(this))
    //         {
    //             owner.Value = picker;
    //             return true;
    //         }
    //         else
    //         {
    //             return false;
    //         }
    //     }

    //     /// <summary>
    //     /// 
    //     /// </summary>
    //     public MeshRenderer meshRenderer;

    //     /// <summary>
    //     /// 
    //     /// </summary>
    //     public GameObject glowEffect;

    //     /// <summary>
    //     /// 
    //     /// </summary>
    //     protected override void StartInternal()
    //     {
    //         __defaultMeshScale = meshRenderer.transform.localScale.x;

    //         base.StartInternal();

    //         touchCollider.OnTriggerEnterAsObservable().Subscribe(c =>
    //         {
    //             if (c.gameObject == GameContext.Instance.playerCtrler.MyHero)
    //                 GameContext.Instance.playerCtrler.MyHeroBrain.CubeStackCtrler.ReserveCube(this);
    //         }).AddTo(this);

    //         touchCollider.OnTriggerExitAsObservable().Subscribe(c =>
    //         {
    //             if (c.gameObject == GameContext.Instance.playerCtrler.MyHero)
    //                 GameContext.Instance.playerCtrler.MyHeroBrain.CubeStackCtrler.CancelReserveCube(this);
    //         }).AddTo(this);

    //         var randOffset = Rand.Range11();

    //         Observable.EveryUpdate().Subscribe(_ =>
    //         {
    //             if (GameContext.Instance.cameraCtrler != null)
    //                 glowEffect.transform.rotation = GameContext.Instance.cameraCtrler.SpriteLookRotation;

    //             meshRenderer.transform.localRotation = Quaternion.Euler(0, 0, Perlin.Noise(Time.time * shakingScale + randOffset) * shakingScale * 10);
    //             meshRenderer.transform.localScale = Vector3.one * __defaultMeshScale * Mathf.Max(1, shakingScale * 0.6f);

    //             if (GameContext.Instance.playerCtrler.MyHeroBrain != null)
    //                 SetFocused(GameContext.Instance.playerCtrler.MyHeroBrain.BB.action.focusingCube.Value == this);
    //         }).AddTo(this);

    //         onPickUpEnabled += () => glowEffect.SetActive(true);
    //         onPickUp += (_) => rigidBody.GetComponent<SphereCollider>().enabled = false;
    //     }

    //     float __defaultMeshScale;

    }

}