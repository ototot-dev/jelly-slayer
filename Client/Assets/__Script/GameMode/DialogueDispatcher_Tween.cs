using System;
using System.Collections;
using System.Linq;
using DG.Tweening;
using Finalfactory.Tagger;
using FinalFactory;
using UniRx;
using UnityEngine;
using Yarn.Unity;

namespace Game
{
    public partial class DialogueDispatcher : DialogueViewBase
    {
        public void TweenShake(string tag, float duration, int vibrato, bool fadeOut)
        {
            Debug.Log("YarnCommand TweenShake: " + tag);

            var obj = TaggerSystem.FindGameObjectWithTag(tag);
            if (obj != null)
            {
                Debug.Log("YarnCommand:  " + obj.name + ", " + obj.tag);
                var startPos = obj.transform.position;
                obj.transform.DOShakePosition(duration, strength: 5 * Vector3.one, 
                    vibrato: vibrato, fadeOut: fadeOut).OnComplete(() => {
                        obj.transform.position = startPos;
                    });
            }
        }
    }
}