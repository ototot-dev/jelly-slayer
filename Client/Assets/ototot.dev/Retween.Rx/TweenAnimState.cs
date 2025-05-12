using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Retween.Rx
{
    public class TweenAnimState
    {
        public TweenAnimState(TweenAnim source)
        {
            this.source = source;
        }

        public TweenAnim source;
        public int runNum;
        public float duration;
        public float elapsed;
        public bool animEnabled;
        public bool togglePingPong;
        public bool IsRunning => animEnabled && !IsRewinding && !IsRollBack;
        public bool IsRewinding { get; set; }
        public bool IsRollBack { get; set; }
    }
}