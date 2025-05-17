using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

#if ENABLE_DOTWEEN_SEQUENCE
using DG.Tweening;
#endif

namespace Retween.Rx
{
    public class TweenPlayer : MonoBehaviour
    {
        public bool clearAnimClips = true;

        /// <summary>
        /// The minimum value of animation elapsed time.
        /// </summary>
        public const float MIN_ELAPSED_TIME = 0.001f;

        /// <summary>
        /// The counter value to generate new run number.
        /// </summary>
        int __runNumCounter = 0;
        int ReserveRunNumber() { return ++__runNumCounter; }

        void Awake()
        {
            if (!TryGetComponent<Animation>(out __anim))
            {
                __anim = gameObject.AddComponent<Animation>();
            }
            else if (clearAnimClips)
            {
                DestroyImmediate(__anim);
                __anim = gameObject.AddComponent<Animation>();
            }

            __anim.playAutomatically = false;
        }

        Animation __anim;
        public Dictionary<TweenAnim, TweenAnimState> tweenStates = new();

        public IObservable<TweenAnimState> Run(TweenAnim tweenAnim, bool resetElapsed = false)
        {
            if (!tweenStates.TryGetValue(tweenAnim, out var currState))
            {
                currState = new TweenAnimState(tweenAnim);
                tweenStates.Add(tweenAnim, currState);
            }

            if (currState.IsRunning)
            {
                var currNum = currState.runNum;

                // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Run() observable is animState. returns empty Run() observable. runNum is {currRunNum}.");

                return Observable.EveryUpdate()
                    .TakeWhile(_ => (tweenAnim.transition.IsLooping || currState.elapsed < currState.duration) && currState.runNum == currNum)
                    .Select(_ => currState);
            }

            var reservedNum = ReserveRunNumber();

            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Run() observable reserved. reservedRunNum to {reservedRunNum}.");

            currState.runNum = reservedNum;
            currState.duration = tweenAnim.transition.duration;
            currState.elapsed = (resetElapsed || currState.IsRollBack) ? MIN_ELAPSED_TIME : Mathf.Max(currState.elapsed, MIN_ELAPSED_TIME);
            currState.IsRewinding = false;
            currState.IsRollBack = false;
            currState.togglePingPong = false;

            SetAnimation(currState);

            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Run() observable started. runNum is {reservedRunNum}.");

            return Observable.NextFrame().ContinueWith(
                Observable.EveryUpdate()
                    .TakeWhile(_ => (tweenAnim.transition.IsLooping || currState.elapsed < currState.duration) && currState.runNum == reservedNum)
                    .Do(_ =>
                    {
                        AdvanceElapsed(currState, Time.deltaTime);
                        UpdateAnimation(currState);
                    })
                    .DoOnCancel(() =>
                    {
                        if (currState.runNum != reservedNum)
                        {
                            if (currState.source.transition.loop != TweenAnim.Loopings.None)
                                UnsetAnimation(currState);

                            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Run() observable cancelled. runNum is {reservedRunNum}.");

                            return;
                        }

                        if (!tweenAnim.transition.IsLooping && tweenAnim.rewindOnCancelled)
                        {
                            currState.IsRewinding = true;

                            Observable.EveryUpdate()
                                .TakeWhile(_ => currState.elapsed > MIN_ELAPSED_TIME && currState.runNum == reservedNum)
                                .DoOnCompleted(() =>
                                {
                                    if (currState.runNum == reservedNum)
                                    {
                                        UnsetAnimation(currState);

                                        currState.elapsed = MIN_ELAPSED_TIME;
                                        currState.IsRewinding = false;
                                    }

                                    ForceToRepaint();
                                })
                                .Subscribe(_ =>
                                {
                                    AdvanceElapsed(currState, Time.deltaTime);
                                    UpdateAnimation(currState);
                                }).AddTo(this);

                            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rewind() observable started via rewindOnCancelled. runNum is {reservedRunNum}.");
                        }
                        else
                        {
                            UnsetAnimation(currState);
                            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Run() observable cancelled. runNum is {reservedRunNum}.");
                        }

                        ForceToRepaint();
                    })
                    .DoOnCompleted(() =>
                    {
                        if (currState.runNum == reservedNum)
                        {
                            UnsetAnimation(currState);
                            currState.elapsed = currState.duration;

                            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Run() observable completed. runNum is {reservedRunNum}.");
                        }
                        else
                        {
                            if (currState.source.transition.loop != TweenAnim.Loopings.None)
                                UnsetAnimation(currState);

                            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Run()  observable cancelled. runNum is {reservedRunNum}.");
                        }

                        ForceToRepaint();
                    })
                    .Select(_ => currState)
                );
        }

        public IObservable<TweenAnimState> Rewind(TweenAnim tweenAnim)
        {
            if (!tweenStates.TryGetValue(tweenAnim, out var currState))
            {
                currState = new TweenAnimState(tweenAnim);
                tweenStates.Add(tweenAnim, currState);
            }

            if (tweenAnim.transition.IsLooping)
            {
                currState.runNum = ReserveRunNumber();

                // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' transition.IsLooping is true. returns empty Rewind() observable. runNum is {animState.runNum}.");

                return Observable.Return(currState);
            }
            else if (currState.IsRewinding)
            {
                var currRunNum = currState.runNum;

                //Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rewind() observable is animState. returns empty Rewind() observable. runNum is {currRunNum}.");

                return Observable.EveryUpdate()
                    .TakeWhile(_ => currState.elapsed > MIN_ELAPSED_TIME && currState.runNum == currRunNum)
                    .Select(_ => currState);
            }
            else if (currState.IsRollBack)
            {
                currState = tweenStates[tweenAnim];

                var currRunNum = currState.runNum;

                //Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rollback() observable is animState. returns empty Rewind() observable. runNum is {currRunNum}.");

                return Observable.EveryUpdate()
                    .TakeWhile(_ => currState.elapsed < currState.duration && currState.runNum == currRunNum)
                    .Select(_ => currState);
            }

            var reservedRunNum = ReserveRunNumber();

            //Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rewind() observable reserved. reservedRunNum to {reservedRunNum}.");

            currState.runNum = reservedRunNum;
            currState.duration = tweenAnim.transition.duration;
            currState.elapsed = Mathf.Min(currState.elapsed, currState.duration);
            currState.IsRewinding = true;
            currState.IsRollBack = false;
            currState.togglePingPong = false;

            SetAnimation(currState);

            //Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rewind() observable started. runNum is {reservedRunNum}.");

            return Observable.NextFrame().ContinueWith(
                Observable.EveryUpdate()
                    .TakeWhile(_ => currState.elapsed > MIN_ELAPSED_TIME && currState.runNum == reservedRunNum)
                    .Do(_ =>
                    {
                        AdvanceElapsed(currState, Time.deltaTime);
                        UpdateAnimation(currState);
                    })
                    .DoOnCancel(() =>
                    {
                        if (currState.runNum == reservedRunNum)
                        {
                            UnsetAnimation(currState);
                            currState.IsRewinding = false;

                            //Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rewind() observable cancelled. runNum is {reservedRunNum}.");
                        }

                        ForceToRepaint();
                    })
                    .DoOnCompleted(() =>
                    {
                        if (currState.runNum == reservedRunNum)
                        {
                            UnsetAnimation(currState);

                            currState.elapsed = MIN_ELAPSED_TIME;
                            currState.IsRewinding = false;

                            //Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rewind() observable completed. runNum is {reservedRunNum}.");
                        }
                        else
                        {
                            //Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rewind() observable cancelled. runNum is {reservedRunNum}.");
                        }

                        ForceToRepaint();
                    })
                    .Select(_ => currState)
                );
        }

        public IObservable<TweenAnimState> Rollback(TweenAnim tweenAnim, bool acceptRewinding = true)
        {
            if (!tweenStates.TryGetValue(tweenAnim, out var currState))
            {
                currState = new TweenAnimState(tweenAnim);
                tweenStates.Add(tweenAnim, currState);
            }

            if (tweenAnim.transition.IsLooping || !tweenAnim.runRollback)
            {
                currState.runNum = ReserveRunNumber();
                currState.elapsed = MIN_ELAPSED_TIME;
                currState.IsRewinding = false;

                UnsetAnimation(currState);

                // if (tweenAnim.transition.IsLooping)
                //     Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' transition.IsLooping is true. returns empty Rollback() observable. runNum is {animState.runNum}.");
                // else if (!tweenAnim.runRollback)
                //     Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' ransition.runRollback is false. returns empty Rollback() observable. runNum is {animState.runNum}.");

                return Observable.Return(currState);
            }
            else if (currState.IsRewinding)
            {
                var currRunNum = currState.runNum;

                // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rewind() observable is animState. returns empty Rollback() TweenAnim observable. runNum is {currRunNum}.");

                return Observable.EveryUpdate()
                    .TakeWhile(_ => currState.elapsed > MIN_ELAPSED_TIME && currState.runNum == currRunNum)
                    .Select(_ => currState);
            }
            else if (currState.IsRollBack)
            {
                currState = tweenStates[tweenAnim];

                var currRunNum = currState.runNum;

                // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rollback() observable is animState. returns empty Rollback() TweenAnim observable. runNum is {currRunNum}.");

                return Observable.EveryUpdate()
                    .TakeWhile(_ => currState.elapsed < currState.duration && currState.runNum == currRunNum)
                    .Select(_ => currState);
            }

            var reservedRunNum = ReserveRunNumber();

            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rollback() observable reserved. reservedRunNum to {reservedRunNum}.");

            currState.runNum = reservedRunNum;

            if (currState.source.rewindOnRollback || (currState.elapsed < currState.duration && acceptRewinding))
            {
                currState.duration = tweenAnim.transition.duration;
                currState.elapsed = Mathf.Min(currState.elapsed, currState.duration);
                currState.IsRewinding = true;
                currState.IsRollBack = false;
                currState.togglePingPong = false;

                // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' allowRewinding is true. executes Rewind() observable. runNum is {reservedRunNum}.");
            }
            else
            {
                currState.duration = tweenAnim.rollbackTransition.duration;
                currState.elapsed = MIN_ELAPSED_TIME;
                currState.IsRewinding = false;
                currState.IsRollBack = true;
                currState.togglePingPong = false;

                // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rollback() observable started. runNum is {reservedRunNum}.");
            }

            SetAnimation(currState);

            return Observable.NextFrame().ContinueWith(
                Observable.EveryUpdate()
                    .TakeWhile(_ =>
                    {
                        if (currState.IsRewinding)
                            return currState.elapsed > MIN_ELAPSED_TIME && reservedRunNum == currState.runNum;
                        else
                            return currState.elapsed < currState.duration && reservedRunNum == currState.runNum;
                    })
                    .Do(_ =>
                    {
                        AdvanceElapsed(currState, Time.deltaTime);
                        UpdateAnimation(currState);
                    })
                    .DoOnCancel(() =>
                    {
                        if (currState.runNum == reservedRunNum)
                        {
                            UnsetAnimation(currState);

                            if (currState.IsRollBack)
                            {
                                currState.duration = tweenAnim.transition.duration;
                                currState.elapsed = MIN_ELAPSED_TIME;
                            }

                            currState.IsRewinding = false;
                            currState.IsRollBack = false;

                            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rollback() observable cancelled. runNum is {reservedRunNum}.");
                        }

                        ForceToRepaint();
                    })
                    .DoOnCompleted(() =>
                    {
                        if (currState.runNum == reservedRunNum)
                        {
                            UnsetAnimation(currState);

                            if (currState.IsRollBack)
                            {
                                currState.duration = tweenAnim.transition.duration;
                                currState.elapsed = MIN_ELAPSED_TIME;
                            }

                            currState.elapsed = MIN_ELAPSED_TIME;
                            currState.IsRewinding = false;
                            currState.IsRollBack = false;

                            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rollback() observable completed. runNum is {reservedRunNum}.");
                        }
                        else
                        {
                            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rollback() observable cancelled. runNum is {reservedRunNum}.");
                        }

                        ForceToRepaint();
                    })
                    .Select(_ => currState)
                );
        }

        public void AdvanceElapsed(TweenAnimState animState, float deltaTime)
        {
            switch (animState.source.transition.loop)
            {
                case TweenAnim.Loopings.None:
                    if (animState.IsRewinding)
                    {
                        animState.elapsed -= deltaTime;
                    }
                    else
                    {
                        // 딜레이로 인한 초기값 튐 방지
                        animState.elapsed = Mathf.Max(animState.elapsed, MIN_ELAPSED_TIME);
                        animState.elapsed += deltaTime;
                    }

                    break;

                case TweenAnim.Loopings.Loop:
                    animState.elapsed += deltaTime;

                    if (animState.elapsed >= animState.source.transition.duration)
                        animState.elapsed -= animState.source.transition.duration;

                    break;

                case TweenAnim.Loopings.PingPong:
                    if (animState.togglePingPong)
                    {
                        animState.elapsed -= deltaTime;

                        if (animState.elapsed <= MIN_ELAPSED_TIME)
                        {
                            animState.elapsed = -animState.elapsed;
                            animState.togglePingPong = false;
                        }
                    }
                    else
                    {
                        animState.elapsed += deltaTime;

                        if (animState.elapsed >= animState.source.transition.duration)
                        {
                            animState.elapsed = animState.source.transition.duration + animState.source.transition.duration - animState.elapsed;
                            animState.togglePingPong = true;
                        }
                    }

                    break;
            }
        }

        void SetAnimation(TweenAnimState animState)
        {
            if (__anim.clip != null)
                Debug.Log($"@@ clip => {__anim.clip}");

            foreach (var c in animState.source.animClips)
            {
                if (__anim.GetClip(c.name) != null)
                    continue;

                __anim.AddClip(c, c.name);

                var t = Mathf.Clamp01(animState.elapsed / (animState.IsRollBack ? animState.source.rollbackTransition.duration : animState.source.transition.duration));

                if (animState.IsRollBack)
                    __anim[c.name].normalizedTime = TweenFunc.GetValue(animState.source.rollbackTransition.easing, 1f, 0f, t);
                else
                    __anim[c.name].normalizedTime = TweenFunc.GetValue(animState.source.transition.easing, 0f, 1f, t);

                __anim[c.name].normalizedSpeed = 0f;
                __anim[c.name].weight = 1f;
                __anim[c.name].enabled = true;
            }

            animState.animEnabled = true;
        }

        void UnsetAnimation(TweenAnimState animState)
        {
            foreach (var c in animState.source.animClips)
            {
                if (__anim.GetClip(c.name) != null)
                    __anim.RemoveClip(c.name);
            }

            animState.animEnabled = false;
        }

        public void UpdateAnimation(TweenAnimState animState)
        {
            var t = Mathf.Clamp01(animState.elapsed / (animState.IsRollBack ? animState.source.rollbackTransition.duration : animState.source.transition.duration));

            foreach (var c in animState.source.animClips)
            {
                try
                {
                    if (animState.IsRollBack)
                        __anim[c.name].normalizedTime = TweenFunc.GetValue(animState.source.rollbackTransition.easing, 1f, 0f, t);
                    else
                        __anim[c.name].normalizedTime = TweenFunc.GetValue(animState.source.transition.easing, 0f, 1f, t);
                }
                catch (NullReferenceException e)
                {
                    Debug.LogWarning($"TweenPlayer => __anim[c.name] returns {e}. {c.name} in {animState.source.name} is not a valid animation clip!!");
                }
            }
        }

#if ENABLE_DOTWEEN_SEQUENCE
    public Dictionary<TweenSequence, TweenSequenceRunning> dotweeenSeqRunnings = new();

    public IObservable<TweenSequenceRunning> Run(TweenSequence tweenSeq, bool resetPosition = false) 
    {
        if (!dotweeenSeqRunnings.ContainsKey(tweenSeq))
            dotweeenSeqRunnings.Add(tweenSeq, new TweenSequenceRunning(tweenSeq));

        var animState = dotweeenSeqRunnings[tweenSeq];

        if (animState.dotweenSeq != null && animState.dotweenSeq.IsPlaying()) {
            var currRunNum = animState.runNum;

            //Debug.Log($"TweenPlayer => TweenSequence '{TweenSequence.gameObject.name}' Run() observable is animState. returns empty Run() observable. runNum is {currRunNum}.");

            return Observable.EveryUpdate()
                .TakeWhile(_ => animState.dotweenSeq.IsPlaying() && animState.runNum == currRunNum)
                .Select(_ => animState);
        }

        var reservedRunNum = ReserveRunNumber();

        //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Run() observable reserved. reservedRunNum to {reservedRunNum}.");

        animState.runNum = reservedRunNum;
        animState.isRewinding = false;

        if (animState.dotweenSeq == null || animState.source.reinitializeOnRun) {
            if (animState.dotweenSeq != null)
                animState.dotweenSeq.Kill();
                
            animState.dotweenSeq = animState.source.dotweenSeqMaker.Invoke(gameObject);
            animState.dotweenSeq.SetAutoKill(false);
        }

        if (resetPosition)
            animState.dotweenSeq.Restart(false);
        else
            animState.dotweenSeq.PlayForward();
            
        //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Run() observable started. runNum is {reservedRunNum}.");

        return Observable.NextFrame().ContinueWith(
            Observable.EveryUpdate()
                .TakeWhile(_ => animState.dotweenSeq.IsPlaying() && animState.runNum == reservedRunNum)
                .DoOnCancel(() => {
                    if (animState.runNum != reservedRunNum)
                        return;

                    if (tweenSeq.rewindOnCancelled) {
                        animState.dotweenSeq.PlayBackwards();
                        animState.isRewinding = true;

                        Observable.EveryUpdate()
                            .TakeWhile(_ => animState.dotweenSeq.IsPlaying() && animState.runNum == reservedRunNum)
                            .DoOnCompleted(() => {
                                if (animState.runNum == reservedRunNum) {
                                    animState.dotweenSeq.Pause();
                                    animState.isRewinding = false;
                                }
                            })
                            .Subscribe()
                            .AddTo(this);

                        //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Rewind() observable started via rewindOnCancelled . runNum is {reservedRunNum}.");
                    }
                    else {
                        animState.dotweenSeq.Pause();
                    }

                    ForceToRepaint();
                })
                .DoOnCancel(() => {
                    if (animState.runNum == reservedRunNum) {
                        animState.dotweenSeq.Pause();
                        //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Run() observable cancelled. runNum is {reservedRunNum}.");
                    }

                    ForceToRepaint();
                })
                .DoOnCompleted(() => {
                    if (animState.runNum == reservedRunNum) {
                        animState.dotweenSeq.Pause();
                        //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Run() observable completed. runNum is {reservedRunNum}.");
                    }
                    else {
                        //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Run() observable cancelled. runNum is {reservedRunNum}.");
                    }

                    ForceToRepaint();
                })
                .Select(_ => animState)
            );
    }

    public IObservable<TweenSequenceRunning> Rewind(TweenSequence tweenSeq) 
    {
        if (!dotweeenSeqRunnings.ContainsKey(tweenSeq))
            dotweeenSeqRunnings.Add(tweenSeq, new TweenSequenceRunning(tweenSeq));

        var animState = dotweeenSeqRunnings[tweenSeq];

        if (!tweenSeq.rewindOnRollback || animState.dotweenSeq == null) {
            var animState.runNum = ReserveRunNumber();

            // if (!tweenSeq.rewindOnRollback)
            //     Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' rewindOnRollback is false. returns empty Rewind() observable. runNum is {animState.runNum}.");
            // else if (animState.dotweenSeq == null)
            //     Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' is never played. returns empty Rewind() observable. runNum is {animState.runNum}.");

            return Observable.Return(animState);
        }
        else if (animState.isRewinding) {
            var currRunNum = animState.runNum;

            //Debug.Log($"TweenPlayer => TweenSequence '{tweenAnim.gameObject.name}' Rewind() observable is animState. returns empty Rewind() observable. runNum is {currRunNum}.");

            return Observable.EveryUpdate()
                    .TakeWhile(_ => animState.dotweenSeq.IsPlaying() && animState.runNum == currRunNum)
                    .Select(_ => animState);
        }
        
        var reservedRunNum = ReserveRunNumber();

        //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Rewind() observable reserved. reservedRunNum to {reservedRunNum}.");
        
        animState.runNum = reservedRunNum;
        animState.dotweenSeq.PlayBackwards();
        animState.isRewinding = true;

        //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Rewind() observable started. runNum is {reservedRunNum}.");

        return Observable.NextFrame().ContinueWith(
            Observable.EveryUpdate()
                .TakeWhile(_ => animState.dotweenSeq.IsPlaying() && animState.runNum == reservedRunNum)
                .DoOnCancel(() => {
                    if (animState.runNum == reservedRunNum) {
                        animState.dotweenSeq.Pause();
                        animState.isRewinding = false;
                    }

                    //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Rewind() observable cancelled. runNum is {reservedRunNum}.");

                    ForceToRepaint();
                })
                .DoOnCompleted(() => {
                    if (animState.runNum == reservedRunNum) {
                        animState.dotweenSeq.Pause();
                        animState.isRewinding = false;
                    }

                    //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Rewind() observable completed. runNum is {reservedRunNum}.");

                    ForceToRepaint();
                })
                .Select(_ => animState)
            );
    }
#endif

        public void ForceToRepaint()
        {
#if UNITY_EDITOR
            if (TryGetComponent<TweenSelector>(out var tweenSelector))
                tweenSelector.ForceToRepaint();
#endif
        }

#if UNITY_EDITOR
        Dictionary<int, List<string>> __animStateHistory = new();
        public List<string> AnimStateHistory
        {
            get
            {
                var ret = new List<string>();
                foreach (var r in __animStateHistory.Keys.OrderBy(k => k))
                    ret.AddRange(__animStateHistory[r]);

                return ret;
            }
        }
#endif
    }
}