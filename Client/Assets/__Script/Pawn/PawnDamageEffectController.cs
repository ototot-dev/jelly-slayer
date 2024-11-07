using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Retween.Rx;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{

    /// <summary>
    /// 
    /// </summary>
    public class PawnDamageEffectController : MonoBehaviour
    {

        /// <summary>
        /// 
        /// </summary>
        public TMPro.TextMeshPro damageText;

        /// <summary>
        /// 
        /// </summary>
        public TMPro.TextMeshPro[] otherTexts;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="duration"></param>
        /// <param name="prefix"></param>
        /// <param name="textColor"></param>
        /// <param name="hideNameTag"></param>
        public void ShowDamageText(int damage, float duration, string prefix, Color textColor, bool hideNameTag = false)
        {
            if (__showDamageTextDisposables.Count == 0)
            {
                __showDamageTextDisposables.Add(damageText, null);
                __damageTextPositionCached = damageText.transform.localPosition;
            }

            if (!__showDamageTextDisposables.Any(d => d.Value == null))
            {
                var newText = Instantiate(damageText.gameObject, damageText.transform.position, damageText.transform.rotation, damageText.transform.parent).GetComponent<TMPro.TextMeshPro>();
                __showDamageTextDisposables.Add(newText, null);
            }

            var showText = __showDamageTextDisposables.First(p => p.Value == null).Key;

            showText.transform.localPosition = __damageTextPositionCached +
                Vector3.forward * UnityEngine.Random.Range(0.1f, 0.2f) * (UnityEngine.Random.Range(0, 2) == 0 ? -1f : 1f) +
                Vector3.right * UnityEngine.Random.Range(0.1f, 0.2f) * (UnityEngine.Random.Range(0, 2) == 0 ? -1f : 1f) +
                Vector3.up * UnityEngine.Random.Range(-0.1f, 0.1f);

            showText.transform.DOShakePosition(0.5f, 0.2f);
            showText.text = $"{prefix}{damage}";
            showText.color = textColor;

            showText.GetComponent<MeshRenderer>().enabled = true;

            if (hideNameTag && ++__nameTagHiddenCount > 0)
            {
                foreach (var t in otherTexts)
                    t.GetComponent<MeshRenderer>().enabled = false;
            }

            __showDamageTextDisposables[showText] = Observable.Timer(TimeSpan.FromSeconds(duration)).Subscribe(_ =>
            {
                showText.GetComponent<MeshRenderer>().enabled = false;

                if (hideNameTag && --__nameTagHiddenCount == 0)
                {
                    foreach (var t in otherTexts)
                        t.GetComponent<MeshRenderer>().enabled = true;
                }

                __showDamageTextDisposables[showText] = null;
            }).AddTo(this);
        }

        int __nameTagHiddenCount;
        Vector3 __damageTextPositionCached;
        Dictionary<TMPro.TextMeshPro, IDisposable> __showDamageTextDisposables = new Dictionary<TMPro.TextMeshPro, IDisposable>();

        /// <summary>
        /// 
        /// </summary>
        public void HideDamageText()
        {
            foreach (var k in __showDamageTextDisposables.Keys)
            {
                if (__showDamageTextDisposables[k] != null)
                {
                    __showDamageTextDisposables[k].Dispose();
                    __showDamageTextDisposables[k] = null;
                }

                k.GetComponent<MeshRenderer>().enabled = false;
            }

            foreach (var t in otherTexts)
                t.GetComponent<MeshRenderer>().enabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual Renderer[] CollectRenderers()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration"></param>
        /// <param name="domainColor"></param>
        public void PlayDamaged(float duration, Color domainColor)
        {
            if (__damagedDisposable != null)
            {
                __damagedDisposable.Dispose();
                __damagedDisposable = null;
            }

            __currImpactColorTable[0] = domainColor;
            __currImpactColorTable[1] = Color.Lerp(__defaultImpactColorTable[1], domainColor, 0.5f);

            var colorIndex = UnityEngine.Random.Range(0, __currImpactColorTable.Length);
            var renderers = CollectRenderers();
            var elapsedTime = 0f;

            foreach (var r in renderers)
            {
                r.materials = new Material[] { r.material, Resources.Load<Material>("Material/HitImpact") };
                r.materials[1].SetVector("_ImpactSize", new Vector4(UnityEngine.Random.Range(1.05f, 1.1f), UnityEngine.Random.Range(1.02f, 1.04f), UnityEngine.Random.Range(1.03f, 1.06f), 1));
                r.materials[1].SetColor("_ImpactColor", __currImpactColorTable[colorIndex]);
            }

            __damagedDisposable = Observable.Interval(TimeSpan.FromMilliseconds(50))
                .TakeWhile(_ => elapsedTime <= duration)
                .DoOnCompleted(() =>
                {
                    foreach (var r in renderers)
                        r.materials = new Material[] { r.material };

                    __damagedDisposable.Dispose();
                    __damagedDisposable = null;
                })
                .Subscribe(_ =>
                {
                    elapsedTime += 0.05f;

                    if (++colorIndex >= __currImpactColorTable.Length)
                        colorIndex = 0;

                    foreach (var r in renderers)
                    {
                        r.materials[1].SetVector("_ImpactSize", new Vector4(UnityEngine.Random.Range(1.05f, 1.1f), UnityEngine.Random.Range(1.02f, 1.04f), UnityEngine.Random.Range(1.03f, 1.06f), 1));
                        r.materials[1].SetColor("_ImpactColor", __currImpactColorTable[colorIndex]);
                    }
                }).AddTo(this);
        }

        Color[] __defaultImpactColorTable = new Color[]
        {
            Color.red,
            Color.yellow,
            Color.black,
        };

        Color[] __currImpactColorTable = new Color[]
        {
            Color.red,
            Color.yellow,
            Color.black,
        };

        IDisposable __damagedDisposable;
        IDisposable __invincibleDisposable;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pawn"></param>
        /// <param name="duration"></param>
        public void PlayInvincible(float duration)
        {
            if (__invincibleDisposable != null)
            {
                __invincibleDisposable.Dispose();
                __invincibleDisposable = null;
            }

            var renderers = CollectRenderers();

            foreach (var r in renderers)
            {
                // r.material.SetFloat("_DitherAlpha", 0.5f);
                r.material.SetFloat("_DitherBlink", 1);
                r.material.SetFloat("_DitherBlinkTimeStamp", Time.timeSinceLevelLoad);
            }

            __invincibleDisposable = Observable.Timer(TimeSpan.FromSeconds(duration)).Subscribe(_ =>
            {
                foreach (var r in renderers)
                {
                    // r.material.SetFloat("_DitherAlpha", 1);
                    r.material.SetFloat("_DitherBlink", 0);
                }

                __invincibleDisposable = null;
            }).AddTo(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration"></param>
        public void PlayDead(float duration)
        {
            if (__invincibleDisposable != null)
            {
                __invincibleDisposable.Dispose();
                __invincibleDisposable = null;
            }

            var renderers = CollectRenderers();
            var elapsedTime = 0f;

            Observable.EveryUpdate().TakeWhile(_ => elapsedTime <= duration)
                .Subscribe(_ =>
                {
                    elapsedTime += Time.deltaTime;

                    foreach (var r in renderers)
                        r.material.SetFloat("_DitherAlpha", Mathf.Clamp01(1 - elapsedTime / duration));
                }).AddTo(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="duration"></param>
        public void PlayBorn(float duration)
        {
            if (__invincibleDisposable != null)
            {
                __invincibleDisposable.Dispose();
                __invincibleDisposable = null;
            }

            var renderers = CollectRenderers();
            var elapsedTime = 0f;

            Observable.EveryUpdate().TakeWhile(_ => elapsedTime <= duration)
                .Subscribe(_ =>
                {
                    elapsedTime += Time.deltaTime;

                    foreach (var r in renderers)
                        r.material.SetFloat("_DitherAlpha", Mathf.Clamp01(elapsedTime / duration));
                }).AddTo(this);
        }

    }

}
