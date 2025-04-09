using System;
using System.Linq;
using Retween.Rx;
using TMPro;
using UniRx;
using UnityEngine;
using Unity.Linq;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    public class PawnTweenHelper : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="fontSize"></param>
        /// <param name="fontColor"></param>
        /// <param name="duration"></param>
        public void ShowEmoticon(string text, float fontSize, Color fontColor, float duration)
        {   
            Debug.Assert(duration > 0); 

            if (__emoticonTextMesh == null)
            {
                Debug.LogWarning($"1?? (__emoticonTextMesh == null) => gameObject:{gameObject.name}");
                return;
            }

            __emoticonTextMesh.gameObject.SetActive(true);
            __emoticonTextMesh.fontSize = fontSize;
            __emoticonTextMesh.color = fontColor;
            __emoticonTextMesh.text = text;

            if (__showEmoticonDisposable != null)
                __showEmoticonDisposable.Dispose();
                
            if (__rotateEmoticonDisposable != null)
                __rotateEmoticonDisposable.Dispose();

            TweenSelector.query.Add(":show", true, true);

            __showEmoticonDisposable = Observable.Timer(TimeSpan.FromSeconds(duration))
                .Subscribe(_ => 
                {
                    TweenSelector.query.activeStates.Clear();
                    TweenSelector.query.Apply();

                    Debug.Assert(__rotateEmoticonDisposable != null);
                    
                    __rotateEmoticonDisposable.Dispose();
                    __rotateEmoticonDisposable = null;
                    __showEmoticonDisposable = null;
                    
                    __emoticonTextMesh.gameObject.SetActive(false);
                }).AddTo(this);

            __rotateEmoticonDisposable = Observable.EveryLateUpdate().Subscribe(_ =>
            {
                if (GameContext.Instance.cameraCtrler != null)
                {
                    __emoticonTextMesh.transform.LookAt(GameContext.Instance.cameraCtrler.viewCamera.transform, Vector3.up);
                    __emoticonTextMesh.transform.rotation = Quaternion.Euler(0, __emoticonTextMesh.transform.rotation.eulerAngles.y + 180, 0);
                }

                if (!Mathf.Approximately(__emoticonTextMesh.alpha, fontAlpha))
                    __emoticonTextMesh.alpha = fontAlpha;
            }).AddTo(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public float fontAlpha = 1;

        IDisposable __showEmoticonDisposable;
        IDisposable __rotateEmoticonDisposable;
        TextMeshPro __emoticonTextMesh;
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetEmoticon()
        {
            return __emoticonTextMesh?.text ?? string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        public TweenSelector TweenSelector { get; private set; }

        void Awake()
        {
            if (gameObject.Children().Any(c => c.name == "Emoticon"))
            {
                __emoticonTextMesh = gameObject.Children().Where(c => c.name == "Emoticon").First().GetComponent<TMPro.TextMeshPro>();
                __emoticonTextMesh.transform.localScale = Vector3.zero;
            }

            TweenSelector = GetComponent<TweenSelector>();
        }
    }
}
