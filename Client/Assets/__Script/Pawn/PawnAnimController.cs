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
using ZLinq;

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

        readonly Dictionary<string, PawnAnimStateMachineTrigger> __stateMachineTriggerObservables = new();
        readonly Dictionary<int, PawnAnimStateMachineTrigger> __runningStateMachineTriggers = new();
        public bool CheckAnimStateTriggered(string stateName) => __runningStateMachineTriggers.ContainsKey(Animator.StringToHash(stateName));
        public bool CheckAnimStateTriggered(int stateNameHash) => __runningStateMachineTriggers.ContainsKey(stateNameHash);
        protected virtual float GetLegAnimatorBlendSpeed() => 0f;
        protected virtual float GetRagdollAnimatorBlendSpeed() => 0f;

        public PawnAnimStateMachineTrigger FindStateMachineTriggerObservable(string stateName)
        {
            if (__stateMachineTriggerObservables.TryGetValue(stateName, out var found))
                return found;

            found = mainAnimator.GetBehaviours<PawnAnimStateMachineTrigger>().FirstOrDefault(t => t.stateName == stateName);

            if (found == null)
            {
                __Logger.WarningR2(gameObject, "FindObservableStateMachineTrigger()", "Found failed", "stateName", stateName);
                return null;
            }

            __stateMachineTriggerObservables.Add(stateName, found);
            return found;
        }

        void OnDestroy()
        {
            if (__playableGraph.IsValid())
                __playableGraph.Destroy();
        }

        protected PlayableGraph __playableGraph;
        protected AnimationPlayableOutput __playableOutput;
        protected AnimationMixerPlayable __playableMixer;
        protected AnimatorControllerPlayable __playableAnimator;
        protected readonly AnimationClipPlayable[] __playableClips = new AnimationClipPlayable[2];
        protected int __currPlayableClipIndex = -1;
        protected IDisposable __blendSingleClipDisposable;
        // public bool IsPlayableGraphRunning() => __playableGraph.IsValid() && (__playableGraph.IsPlaying() || __blendSingleClipDisposable != null);
        public bool IsPlayableGraphRunning() => __playableGraph.IsValid() && __playableGraph.IsPlaying();
        public virtual bool IsRootMotionForced() => __runningStateMachineTriggers.AsValueEnumerable().Any(t => t.Value.isRootMotionForced);
        public virtual float GetForecedRootMotionMultiplier() => __runningStateMachineTriggers.AsValueEnumerable().FirstOrDefault(t => t.Value.isRootMotionForced).Value.rootMotionMultiplier;

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
            __playableMixer.SetInputWeight(__currPlayableClipIndex, 0f);

            if (!__playableGraph.IsPlaying())
                __playableGraph.Play();

            var blendSpeed = 1f / blendInTime;

            __blendSingleClipDisposable?.Dispose();
            __blendSingleClipDisposable = Observable.EveryUpdate().Subscribe(_ =>
            {
                if (__currPlayableClipIndex == 0)
                {
                    var newWeight = Mathf.Clamp01(__playableMixer.GetInputWeight(0) + blendSpeed * Time.deltaTime);
                    __playableMixer.SetInputWeight(0, newWeight);
                    __playableMixer.SetInputWeight(1, 1f - newWeight);
                }
                else
                {
                    var newWeight = Mathf.Clamp01(__playableMixer.GetInputWeight(1) + blendSpeed * Time.deltaTime);
                    __playableMixer.SetInputWeight(0, 1f - newWeight);
                    __playableMixer.SetInputWeight(1, newWeight);
                }

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
            __playableMixer.SetInputWeight(__currPlayableClipIndex, 0f);

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

                if (__currPlayableClipIndex == 0)
                {
                    var newWeight = Mathf.Clamp01(__playableMixer.GetInputWeight(0) + blendSpeed * Time.deltaTime);
                    __playableMixer.SetInputWeight(0, newWeight);
                    __playableMixer.SetInputWeight(1, 1f - newWeight);
                }
                else
                {
                    var newWeight = Mathf.Clamp01(__playableMixer.GetInputWeight(1) + blendSpeed * Time.deltaTime);
                    __playableMixer.SetInputWeight(0, 1f - newWeight);
                    __playableMixer.SetInputWeight(1, newWeight);
                }

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

        public virtual void OnAnimatorMoveHandler() { }
        public virtual void OnAnimatorStateEnterHandler(AnimatorStateInfo stateInfo, int layerIndex, PawnAnimStateMachineTrigger trigger)
        {
            __Logger.LogR1(gameObject, "OnAnimatorStateEnterHandler()", "stateName", trigger.stateName);
            __runningStateMachineTriggers.Add(stateInfo.shortNameHash, trigger);
        }
        public virtual void OnAniamtorStateExitHandler(AnimatorStateInfo stateInfo, int layerIndex, PawnAnimStateMachineTrigger trigger)
        {
            __Logger.LogR1(gameObject, "OnAniamtorStateExitHandler()", "stateName", trigger.stateName);
            __runningStateMachineTriggers.Remove(stateInfo.shortNameHash);
        }
        public virtual void OnAnimatorFootHandler(bool isRight) { }
        public virtual void OnRagdollAnimatorFalling() { }
        public virtual void OnRagdollAnimatorGetUp() { }
    }
}