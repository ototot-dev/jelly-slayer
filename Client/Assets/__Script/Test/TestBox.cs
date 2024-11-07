using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Linq;
using UniRx;
using UniRx.Triggers;
using DG.Tweening;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    public class TestBox : MonoBehaviour
    {
        void Start()
        {
            Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(_ => Destroy(gameObject, 5));
        }
    }
}
