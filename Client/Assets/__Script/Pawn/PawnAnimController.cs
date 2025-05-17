using System;
using System.Collections.Generic;
using System.Linq;
using FIMSpace.FProceduralAnimation;
using UniRx;
using UniRx.Triggers.Extension;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;

namespace Game
{
    public class PawnAnimController : MonoBehaviour
    {
        [Header("Component")]
        public Animator mainAnimator;
        public RigBuilder rigBuilder;
        public Rig rigSetup;
        public LegsAnimator legAnimator;
        public RagdollAnimator2 ragdollAnimator;
        public Action<AnimatorStateInfo, int> onAnimStateEnter;
        public Action<AnimatorStateInfo, int> onAnimStateExit;
        readonly Dictionary<string, ObservableStateMachineTriggerEx> __observableStateMachineTriggersCached = new();

        public ObservableStateMachineTriggerEx FindObservableStateMachineTriggerEx(string stateName)
        {
            if (__observableStateMachineTriggersCached.TryGetValue(stateName, out var ret))
                return ret;
            
            var found = mainAnimator.GetBehaviours<ObservableStateMachineTriggerEx>().First(s => s.stateName == stateName);
            __observableStateMachineTriggersCached.Add(stateName, found);

            return found;
        }

        void OnDestroy()
        {
            __playableGraph.Destroy();
        }

        protected PlayableGraph __playableGraph;
        protected AnimationPlayableOutput __playableOutput;
        protected AnimationMixerPlayable __playableMixer;
        protected AnimatorControllerPlayable __playableAnimator;
        protected readonly AnimationClipPlayable[] __playableClips = new AnimationClipPlayable[2];
        protected int __currPlayableClipIndex;
        protected IDisposable __blendSingleClipDisposable;
        public bool IsPlayableGraphRunning() => __playableGraph.IsValid() && __playableGraph.IsPlaying();

        protected virtual void CreatePlayableGraph()
        {
            __playableGraph = PlayableGraph.Create();
            __playableOutput = AnimationPlayableOutput.Create(__playableGraph, "Animation", mainAnimator);
            __playableMixer = AnimationMixerPlayable.Create(__playableGraph, 3);
            __playableClips[0] = AnimationClipPlayable.Create(__playableGraph, null);
            __playableClips[1] = AnimationClipPlayable.Create(__playableGraph, null);
            __playableAnimator = AnimatorControllerPlayable.Create(__playableGraph, mainAnimator.runtimeAnimatorController);

            __playableOutput.SetSourcePlayable(__playableMixer);
            __playableGraph.Connect(__playableClips[0], 0, __playableMixer, 0);
            __playableGraph.Connect(__playableClips[1], 0, __playableMixer, 1);
            __playableGraph.Connect(__playableAnimator, 0, __playableMixer, 2);

            __playableMixer.SetInputWeight(0, 0f);
            __playableMixer.SetInputWeight(1, 0f);
            __playableMixer.SetInputWeight(2, 1f);
        }
        public void PlaySingleClipLooping(AnimationClip clip, float blendInTime)
        {
            if (!__playableGraph.IsValid()) CreatePlayableGraph();

            __currPlayableClipIndex = __currPlayableClipIndex == 0 ? 1 : 0;
            __playableMixer.DisconnectInput(__currPlayableClipIndex);
            __playableClips[__currPlayableClipIndex].Destroy();
            __playableClips[__currPlayableClipIndex] = AnimationClipPlayable.Create(__playableGraph, clip);
            __playableGraph.Connect(__playableClips[__currPlayableClipIndex], 0, __playableMixer, __currPlayableClipIndex);
            
            if (!__playableGraph.IsPlaying())
                __playableGraph.Play();

            var blendSpeed = 1f / blendInTime;

            __blendSingleClipDisposable?.Dispose();
            __blendSingleClipDisposable = Observable.EveryUpdate().Subscribe(_ =>
            {
                __playableMixer.SetInputWeight(0, Mathf.Clamp01(__playableMixer.GetInputWeight(0) + (__currPlayableClipIndex == 0 ? blendSpeed : -blendSpeed)* Time.deltaTime));
                __playableMixer.SetInputWeight(1, Mathf.Clamp01(__playableMixer.GetInputWeight(1) + (__currPlayableClipIndex == 1 ? blendSpeed : -blendSpeed)* Time.deltaTime));
                __playableMixer.SetInputWeight(2, Mathf.Clamp01(__playableMixer.GetInputWeight(2) - blendSpeed * Time.deltaTime));
            }).AddTo(this);
        }
        
        public void PlaySingleClip(AnimationClip clip, float blendInTime, float blendOutTime)
        {
            if (!__playableGraph.IsValid()) CreatePlayableGraph();

            __currPlayableClipIndex = __currPlayableClipIndex == 0 ? 1 : 0;
            __playableMixer.DisconnectInput(__currPlayableClipIndex);
            __playableClips[__currPlayableClipIndex].Destroy();
            __playableClips[__currPlayableClipIndex] = AnimationClipPlayable.Create(__playableGraph, clip);
            __playableGraph.Connect(__playableClips[__currPlayableClipIndex], 0, __playableMixer, __currPlayableClipIndex);

            if (!__playableGraph.IsPlaying())
                __playableGraph.Play();

            var blendStartTimeStamp = Time.time;
            var blendSpeed = 1f / blendInTime;
            var clipDuration = clip.length;

            __blendSingleClipDisposable?.Dispose();
            __blendSingleClipDisposable = Observable.EveryUpdate().Subscribe(_ =>
            {
                if ((Time.time - blendStartTimeStamp) > (clipDuration - blendOutTime))
                    StopSingleClip(blendOutTime);

                __playableMixer.SetInputWeight(0, Mathf.Clamp01(__playableMixer.GetInputWeight(0) + (__currPlayableClipIndex == 0 ? blendSpeed : -blendSpeed) * Time.deltaTime));
                __playableMixer.SetInputWeight(1, Mathf.Clamp01(__playableMixer.GetInputWeight(1) + (__currPlayableClipIndex == 1 ? blendSpeed : -blendSpeed) * Time.deltaTime));
                __playableMixer.SetInputWeight(2, Mathf.Clamp01(__playableMixer.GetInputWeight(2) - blendSpeed * Time.deltaTime));
            }).AddTo(this);
        }

        public void StopSingleClip(float blendOutTime)
        {
            var blendSpeed = 1f / blendOutTime;

            __blendSingleClipDisposable?.Dispose();
            __blendSingleClipDisposable = Observable.EveryUpdate().TakeUntil(Observable.Timer(TimeSpan.FromSeconds(blendOutTime)))
                .DoOnCompleted(() => 
                {
                    __playableMixer.SetInputWeight(0, 0f);
                    __playableMixer.SetInputWeight(1, 0f);
                    __playableMixer.SetInputWeight(2, 1f);
                    __playableGraph.Stop();
                })
                .Subscribe(_ =>
                {
                    __playableMixer.SetInputWeight(0, Mathf.Clamp01(__playableMixer.GetInputWeight(0) - blendSpeed * Time.deltaTime));
                    __playableMixer.SetInputWeight(1, Mathf.Clamp01(__playableMixer.GetInputWeight(1) - blendSpeed * Time.deltaTime));
                    __playableMixer.SetInputWeight(2, Mathf.Clamp01(__playableMixer.GetInputWeight(2) + blendSpeed * Time.deltaTime));
                }).AddTo(this);
        }
    
        public virtual void OnAnimatorMoveHandler() {}
        public virtual void OnAnimatorStateEnterHandler(AnimatorStateInfo stateInfo, int layerIndex) { onAnimStateEnter?.Invoke(stateInfo, layerIndex); }
        public virtual void OnAniamtorStateExitHandler(AnimatorStateInfo stateInfo, int layerIndex) { onAnimStateExit?.Invoke(stateInfo, layerIndex); }
        public virtual void OnAnimatorFootHandler(bool isRight) {}
        public virtual void OnRagdollAnimatorFalling() {}
        public virtual void OnRagdollAnimatorGetUp() {}
    }
}