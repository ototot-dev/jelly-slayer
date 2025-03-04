using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

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
            __anim = GetComponent<Animation>();

            if (__anim == null)
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

        public IObservable<TweenAnimRunning> Run(TweenAnim tweenAnim, bool resetElapsed = false)
        {
            if (!animRunnings.ContainsKey(tweenAnim))
                animRunnings.Add(tweenAnim, new TweenAnimRunning(tweenAnim));

            var running = animRunnings[tweenAnim];

            if (running.IsRunning)
            {
                var currRunNum = running.runNum;

                // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Run() observable is running. returns empty Run() observable. runNum is {currRunNum}.");

                return Observable.EveryUpdate()
                    .TakeWhile(_ => (tweenAnim.transition.IsLooping || running.elapsed < running.duration) && running.runNum == currRunNum)
                    .Select(_ => running);
            }

            var reservedRunNum = ReserveRunNumber();

            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Run() observable reserved. reservedRunNum to {reservedRunNum}.");

            running.runNum = reservedRunNum;
            running.duration = tweenAnim.transition.duration;
            running.elapsed = (resetElapsed || running.IsRollBack) ? MIN_ELAPSED_TIME : Mathf.Max(running.elapsed, MIN_ELAPSED_TIME);
            running.IsRewinding = false;
            running.IsRollBack = false;
            running.togglePingPong = false;

            SetAnimation(running);

            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Run() observable started. runNum is {reservedRunNum}.");

            return Observable.NextFrame().ContinueWith(
                Observable.EveryUpdate()
                    .TakeWhile(_ => (tweenAnim.transition.IsLooping || running.elapsed < running.duration) && running.runNum == reservedRunNum)
                    .Do(_ =>
                    {
                        AdvanceElapsed(running, Time.deltaTime);
                        UpdateAnimation(running);
                    })
                    .DoOnCancel(() =>
                    {
                        if (running.runNum != reservedRunNum)
                        {
                            if (running.source.transition.loop != TweenAnim.Loopings.None)
                                UnsetAnimation(running);

                            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Run() observable cancelled. runNum is {reservedRunNum}.");

                            return;
                        }

                        if (!tweenAnim.transition.IsLooping && tweenAnim.rewindOnCancelled)
                        {
                            running.IsRewinding = true;

                            Observable.EveryUpdate()
                                .TakeWhile(_ => running.elapsed > MIN_ELAPSED_TIME && running.runNum == reservedRunNum)
                                .DoOnCompleted(() =>
                                {
                                    if (running.runNum == reservedRunNum)
                                    {
                                        UnsetAnimation(running);

                                        running.elapsed = MIN_ELAPSED_TIME;
                                        running.IsRewinding = false;
                                    }

                                    ForceToRepaint();
                                })
                                .Subscribe(_ =>
                                {
                                    AdvanceElapsed(running, Time.deltaTime);
                                    UpdateAnimation(running);
                                }).AddTo(this);

                            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rewind() observable started via rewindOnCancelled. runNum is {reservedRunNum}.");
                        }
                        else
                        {
                            UnsetAnimation(running);
                            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Run() observable cancelled. runNum is {reservedRunNum}.");
                        }

                        ForceToRepaint();
                    })
                    .DoOnCompleted(() =>
                    {
                        if (running.runNum == reservedRunNum)
                        {
                            UnsetAnimation(running);
                            running.elapsed = running.duration;

                            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Run() observable completed. runNum is {reservedRunNum}.");
                        }
                        else
                        {
                            if (running.source.transition.loop != TweenAnim.Loopings.None)
                                UnsetAnimation(running);

                            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Run()  observable cancelled. runNum is {reservedRunNum}.");
                        }

                        ForceToRepaint();
                    })
                    .Select(_ => running)
                );
        }

        public IObservable<TweenAnimRunning> Rewind(TweenAnim tweenAnim)
        {
            if (!animRunnings.ContainsKey(tweenAnim))
                animRunnings.Add(tweenAnim, new TweenAnimRunning(tweenAnim));

            var running = animRunnings[tweenAnim];

            if (tweenAnim.transition.IsLooping)
            {
                running.runNum = ReserveRunNumber();

                // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' transition.IsLooping is true. returns empty Rewind() observable. runNum is {running.runNum}.");

                return Observable.Return(running);
            }
            else if (running.IsRewinding)
            {
                var currRunNum = running.runNum;

                //Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rewind() observable is running. returns empty Rewind() observable. runNum is {currRunNum}.");

                return Observable.EveryUpdate()
                    .TakeWhile(_ => running.elapsed > MIN_ELAPSED_TIME && running.runNum == currRunNum)
                    .Select(_ => running);
            }
            else if (running.IsRollBack)
            {
                running = animRunnings[tweenAnim];

                var currRunNum = running.runNum;

                //Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rollback() observable is running. returns empty Rewind() observable. runNum is {currRunNum}.");

                return Observable.EveryUpdate()
                    .TakeWhile(_ => running.elapsed < running.duration && running.runNum == currRunNum)
                    .Select(_ => running);
            }

            var reservedRunNum = ReserveRunNumber();

            //Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rewind() observable reserved. reservedRunNum to {reservedRunNum}.");

            running.runNum = reservedRunNum;
            running.duration = tweenAnim.transition.duration;
            running.elapsed = Mathf.Min(running.elapsed, running.duration);
            running.IsRewinding = true;
            running.IsRollBack = false;
            running.togglePingPong = false;

            SetAnimation(running);

            //Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rewind() observable started. runNum is {reservedRunNum}.");

            return Observable.NextFrame().ContinueWith(
                Observable.EveryUpdate()
                    .TakeWhile(_ => running.elapsed > MIN_ELAPSED_TIME && running.runNum == reservedRunNum)
                    .Do(_ =>
                    {
                        AdvanceElapsed(running, Time.deltaTime);
                        UpdateAnimation(running);
                    })
                    .DoOnCancel(() =>
                    {
                        if (running.runNum == reservedRunNum)
                        {
                            UnsetAnimation(running);
                            running.IsRewinding = false;

                            //Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rewind() observable cancelled. runNum is {reservedRunNum}.");
                        }

                        ForceToRepaint();
                    })
                    .DoOnCompleted(() =>
                    {
                        if (running.runNum == reservedRunNum)
                        {
                            UnsetAnimation(running);

                            running.elapsed = MIN_ELAPSED_TIME;
                            running.IsRewinding = false;

                            //Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rewind() observable completed. runNum is {reservedRunNum}.");
                        }
                        else
                        {
                            //Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rewind() observable cancelled. runNum is {reservedRunNum}.");
                        }

                        ForceToRepaint();
                    })
                    .Select(_ => running)
                );
        }

        public IObservable<TweenAnimRunning> Rollback(TweenAnim tweenAnim, bool acceptRewinding = true)
        {
            if (!animRunnings.ContainsKey(tweenAnim))
                animRunnings.Add(tweenAnim, new TweenAnimRunning(tweenAnim));

            var running = animRunnings[tweenAnim];

            if (tweenAnim.transition.IsLooping || !tweenAnim.runRollback)
            {
                running.runNum = ReserveRunNumber();
                running.elapsed = MIN_ELAPSED_TIME;
                running.IsRewinding = false;

                UnsetAnimation(running);

                // if (tweenAnim.transition.IsLooping)
                //     Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' transition.IsLooping is true. returns empty Rollback() observable. runNum is {running.runNum}.");
                // else if (!tweenAnim.runRollback)
                //     Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' ransition.runRollback is false. returns empty Rollback() observable. runNum is {running.runNum}.");

                return Observable.Return(running);
            }
            else if (running.IsRewinding)
            {
                var currRunNum = running.runNum;

                // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rewind() observable is running. returns empty Rollback() TweenAnim observable. runNum is {currRunNum}.");

                return Observable.EveryUpdate()
                    .TakeWhile(_ => running.elapsed > MIN_ELAPSED_TIME && running.runNum == currRunNum)
                    .Select(_ => running);
            }
            else if (running.IsRollBack)
            {
                running = animRunnings[tweenAnim];

                var currRunNum = running.runNum;

                // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rollback() observable is running. returns empty Rollback() TweenAnim observable. runNum is {currRunNum}.");

                return Observable.EveryUpdate()
                    .TakeWhile(_ => running.elapsed < running.duration && running.runNum == currRunNum)
                    .Select(_ => running);
            }

            var reservedRunNum = ReserveRunNumber();

            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rollback() observable reserved. reservedRunNum to {reservedRunNum}.");

            running.runNum = reservedRunNum;

            if (running.source.rewindOnRollback || (running.elapsed < running.duration && acceptRewinding))
            {
                running.duration = tweenAnim.transition.duration;
                running.elapsed = Mathf.Min(running.elapsed, running.duration);
                running.IsRewinding = true;
                running.IsRollBack = false;
                running.togglePingPong = false;

                // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' allowRewinding is true. executes Rewind() observable. runNum is {reservedRunNum}.");
            }
            else
            {
                running.duration = tweenAnim.rollbackTransition.duration;
                running.elapsed = MIN_ELAPSED_TIME;
                running.IsRewinding = false;
                running.IsRollBack = true;
                running.togglePingPong = false;

                // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rollback() observable started. runNum is {reservedRunNum}.");
            }

            SetAnimation(running);

            return Observable.NextFrame().ContinueWith(
                Observable.EveryUpdate()
                    .TakeWhile(_ =>
                    {
                        if (running.IsRewinding)
                            return running.elapsed > MIN_ELAPSED_TIME && reservedRunNum == running.runNum;
                        else
                            return running.elapsed < running.duration && reservedRunNum == running.runNum;
                    })
                    .Do(_ =>
                    {
                        AdvanceElapsed(running, Time.deltaTime);
                        UpdateAnimation(running);
                    })
                    .DoOnCancel(() =>
                    {
                        if (running.runNum == reservedRunNum)
                        {
                            UnsetAnimation(running);

                            if (running.IsRollBack)
                            {
                                running.duration = tweenAnim.transition.duration;
                                running.elapsed = MIN_ELAPSED_TIME;
                            }

                            running.IsRewinding = false;
                            running.IsRollBack = false;

                            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rollback() observable cancelled. runNum is {reservedRunNum}.");
                        }

                        ForceToRepaint();
                    })
                    .DoOnCompleted(() =>
                    {
                        if (running.runNum == reservedRunNum)
                        {
                            UnsetAnimation(running);

                            if (running.IsRollBack)
                            {
                                running.duration = tweenAnim.transition.duration;
                                running.elapsed = MIN_ELAPSED_TIME;
                            }

                            running.elapsed = MIN_ELAPSED_TIME;
                            running.IsRewinding = false;
                            running.IsRollBack = false;

                            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rollback() observable completed. runNum is {reservedRunNum}.");
                        }
                        else
                        {
                            // Debug.Log($"TweenPlayer => TweenAnim '{tweenAnim.gameObject.name}' Rollback() observable cancelled. runNum is {reservedRunNum}.");
                        }

                        ForceToRepaint();
                    })
                    .Select(_ => running)
                );
        }

        public void AdvanceElapsed(TweenAnimRunning running, float deltaTime)
        {
            switch (running.source.transition.loop)
            {
                case TweenAnim.Loopings.None:
                    if (running.IsRewinding)
                    {
                        running.elapsed -= deltaTime;
                    }
                    else
                    {
                        // 딜레이로 인한 초기값 튐 방지
                        running.elapsed = Mathf.Max(running.elapsed, MIN_ELAPSED_TIME);
                        running.elapsed += deltaTime;
                    }

                    break;

                case TweenAnim.Loopings.Loop:
                    running.elapsed += deltaTime;

                    if (running.elapsed >= running.source.transition.duration)
                        running.elapsed -= running.source.transition.duration;

                    break;

                case TweenAnim.Loopings.PingPong:
                    if (running.togglePingPong)
                    {
                        running.elapsed -= deltaTime;

                        if (running.elapsed <= MIN_ELAPSED_TIME)
                        {
                            running.elapsed = -running.elapsed;
                            running.togglePingPong = false;
                        }
                    }
                    else
                    {
                        running.elapsed += deltaTime;

                        if (running.elapsed >= running.source.transition.duration)
                        {
                            running.elapsed = running.source.transition.duration + running.source.transition.duration - running.elapsed;
                            running.togglePingPong = true;
                        }
                    }

                    break;
            }
        }

        void SetAnimation(TweenAnimRunning running)
        {
            if (__anim.clip != null)
                Debug.Log($"@@ clip => {__anim.clip}");

            foreach (var c in running.source.animClips)
            {
                if (__anim.GetClip(c.name) != null)
                    continue;

                __anim.AddClip(c, c.name);

                var t = Mathf.Clamp01(running.elapsed / (running.IsRollBack ? running.source.rollbackTransition.duration : running.source.transition.duration));

                if (running.IsRollBack)
                    __anim[c.name].normalizedTime = TweenFunc.GetValue(running.source.rollbackTransition.easing, 1f, 0f, t);
                else
                    __anim[c.name].normalizedTime = TweenFunc.GetValue(running.source.transition.easing, 0f, 1f, t);

                __anim[c.name].normalizedSpeed = 0f;
                __anim[c.name].weight = 1f;
                __anim[c.name].enabled = true;
            }

            running.animEnabled = true;
        }

        void UnsetAnimation(TweenAnimRunning running)
        {
            foreach (var c in running.source.animClips)
            {
                if (__anim.GetClip(c.name) != null)
                    __anim.RemoveClip(c.name);
            }

            running.animEnabled = false;
        }

        public void UpdateAnimation(TweenAnimRunning running)
        {
            var t = Mathf.Clamp01(running.elapsed / (running.IsRollBack ? running.source.rollbackTransition.duration : running.source.transition.duration));

            foreach (var c in running.source.animClips)
            {
                try
                {
                    if (running.IsRollBack)
                        __anim[c.name].normalizedTime = TweenFunc.GetValue(running.source.rollbackTransition.easing, 1f, 0f, t);
                    else
                        __anim[c.name].normalizedTime = TweenFunc.GetValue(running.source.transition.easing, 0f, 1f, t);
                }
                catch (NullReferenceException e)
                {
                    Debug.LogWarning($"TweenPlayer => __anim[c.name] returns {e}. {c.name} in {running.source.name} is not a valid animation clip!!");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TweenAnim"></typeparam>
        /// <typeparam name="TweenAnimInstance"></typeparam>
        /// <returns></returns>
        public Dictionary<TweenAnim, TweenAnimRunning> animRunnings = new Dictionary<TweenAnim, TweenAnimRunning>();

#if ENABLE_DOTWEEN_SEQUENCE
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tweenSeq"></param>
    /// <param name="resetPosition"></param>
    /// <returns></returns>
    public IObservable<TweenSequenceRunning> Run(TweenSequence tweenSeq, bool resetPosition = false) {
        if (!dotweeenSeqRunnings.ContainsKey(tweenSeq))
            dotweeenSeqRunnings.Add(tweenSeq, new TweenSequenceRunning(tweenSeq));

        var running = dotweeenSeqRunnings[tweenSeq];

        if (running.dotweenSeq != null && running.dotweenSeq.IsPlaying()) {
            var currRunNum = running.runNum;

            //Debug.Log($"TweenPlayer => TweenSequence '{TweenSequence.gameObject.name}' Run() observable is running. returns empty Run() observable. runNum is {currRunNum}.");

            return Observable.EveryUpdate()
                .TakeWhile(_ => running.dotweenSeq.IsPlaying() && running.runNum == currRunNum)
                .Select(_ => running);
        }

        var reservedRunNum = ReserveRunNumber();

        //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Run() observable reserved. reservedRunNum to {reservedRunNum}.");

        running.runNum = reservedRunNum;
        running.isRewinding = false;

        if (running.dotweenSeq == null || running.source.reinitializeOnRun) {
            if (running.dotweenSeq != null)
                running.dotweenSeq.Kill();
                
            running.dotweenSeq = running.source.dotweenSeqMaker.Invoke(gameObject);
            running.dotweenSeq.SetAutoKill(false);
        }

        if (resetPosition)
            running.dotweenSeq.Restart(false);
        else
            running.dotweenSeq.PlayForward();
            
        //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Run() observable started. runNum is {reservedRunNum}.");

        return Observable.NextFrame().ContinueWith(
            Observable.EveryUpdate()
                .TakeWhile(_ => running.dotweenSeq.IsPlaying() && running.runNum == reservedRunNum)
                .DoOnCancel(() => {
                    if (running.runNum != reservedRunNum)
                        return;

                    if (tweenSeq.rewindOnCancelled) {
                        running.dotweenSeq.PlayBackwards();
                        running.isRewinding = true;

                        Observable.EveryUpdate()
                            .TakeWhile(_ => running.dotweenSeq.IsPlaying() && running.runNum == reservedRunNum)
                            .DoOnCompleted(() => {
                                if (running.runNum == reservedRunNum) {
                                    running.dotweenSeq.Pause();
                                    running.isRewinding = false;
                                }
                            })
                            .Subscribe()
                            .AddTo(this);

                        //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Rewind() observable started via rewindOnCancelled . runNum is {reservedRunNum}.");
                    }
                    else {
                        running.dotweenSeq.Pause();
                    }

                    ForceToRepaint();
                })
                .DoOnCancel(() => {
                    if (running.runNum == reservedRunNum) {
                        running.dotweenSeq.Pause();
                        //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Run() observable cancelled. runNum is {reservedRunNum}.");
                    }

                    ForceToRepaint();
                })
                .DoOnCompleted(() => {
                    if (running.runNum == reservedRunNum) {
                        running.dotweenSeq.Pause();
                        //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Run() observable completed. runNum is {reservedRunNum}.");
                    }
                    else {
                        //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Run() observable cancelled. runNum is {reservedRunNum}.");
                    }

                    ForceToRepaint();
                })
                .Select(_ => running)
            );
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tweenSeq"></param>
    /// <returns></returns>
    public IObservable<TweenSequenceRunning> Rewind(TweenSequence tweenSeq) {
        if (!dotweeenSeqRunnings.ContainsKey(tweenSeq))
            dotweeenSeqRunnings.Add(tweenSeq, new TweenSequenceRunning(tweenSeq));

        var running = dotweeenSeqRunnings[tweenSeq];

        if (!tweenSeq.rewindOnRollback || running.dotweenSeq == null) {
            var running.runNum = ReserveRunNumber();

            // if (!tweenSeq.rewindOnRollback)
            //     Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' rewindOnRollback is false. returns empty Rewind() observable. runNum is {running.runNum}.");
            // else if (running.dotweenSeq == null)
            //     Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' is never played. returns empty Rewind() observable. runNum is {running.runNum}.");

            return Observable.Return(running);
        }
        else if (running.isRewinding) {
            var currRunNum = running.runNum;

            //Debug.Log($"TweenPlayer => TweenSequence '{tweenAnim.gameObject.name}' Rewind() observable is running. returns empty Rewind() observable. runNum is {currRunNum}.");

            return Observable.EveryUpdate()
                    .TakeWhile(_ => running.dotweenSeq.IsPlaying() && running.runNum == currRunNum)
                    .Select(_ => running);
        }
        
        var reservedRunNum = ReserveRunNumber();

        //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Rewind() observable reserved. reservedRunNum to {reservedRunNum}.");
        
        running.runNum = reservedRunNum;
        running.dotweenSeq.PlayBackwards();
        running.isRewinding = true;

        //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Rewind() observable started. runNum is {reservedRunNum}.");

        return Observable.NextFrame().ContinueWith(
            Observable.EveryUpdate()
                .TakeWhile(_ => running.dotweenSeq.IsPlaying() && running.runNum == reservedRunNum)
                .DoOnCancel(() => {
                    if (running.runNum == reservedRunNum) {
                        running.dotweenSeq.Pause();
                        running.isRewinding = false;
                    }

                    //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Rewind() observable cancelled. runNum is {reservedRunNum}.");

                    ForceToRepaint();
                })
                .DoOnCompleted(() => {
                    if (running.runNum == reservedRunNum) {
                        running.dotweenSeq.Pause();
                        running.isRewinding = false;
                    }

                    //Debug.Log($"TweenPlayer => TweenSequence '{tweenSeq.gameObject.name}' Rewind() observable completed. runNum is {reservedRunNum}.");

                    ForceToRepaint();
                })
                .Select(_ => running)
            );
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TweenAnim"></typeparam>
    /// <typeparam name="TweenAnimInstance"></typeparam>
    /// <returns></returns>
    public Dictionary<TweenSequence, TweenSequenceRunning> dotweeenSeqRunnings = new Dictionary<TweenSequence, TweenSequenceRunning>();
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public void ForceToRepaint()
        {
#if UNITY_EDITOR
            var tweenSelector = GetComponent<TweenSelector>();

            if (tweenSelector != null)
                tweenSelector.ForceToRepaint();
#endif
        }

#if UNITY_EDITOR

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        Dictionary<int, List<string>> __runningHistory = new Dictionary<int, List<string>>();

        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public List<string> RunningHistory
        {
            get
            {
                var ret = new List<string>();

                foreach (var r in __runningHistory.Keys.OrderBy(k => k))
                    ret.AddRange(__runningHistory[r]);

                return ret;
            }
        }

#endif

    }

}