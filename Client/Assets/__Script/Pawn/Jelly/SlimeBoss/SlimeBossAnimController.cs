using System;
using System.Collections.Generic;
using FIMSpace.FEyes;
using FIMSpace.FProceduralAnimation;
using Retween.Rx;
using UniRx;
using UnityEngine;

namespace Game
{
    public class SlimeBossAnimController : PawnAnimController
    {
        [Header("Component")]
        public Transform jellySocket;
        public Transform eyeSocket;
        public Transform lookAt;
        public FEyesAnimator eyeAnimator;
        public JellySpringMassSystem springMass;
        public SphereCollider springMassCore; 
        public TweenSelector jellyTweenSelector;

        void Awake()
        {
            __brain = GetComponent<SlimeBossBrain>();

            if (springMass != null)
                springMass.coreAttachPoint = jellySocket;
        }

        SlimeBossBrain __brain;

        void Start()
        {
            __brain.BB.isJumping.Skip(1).Subscribe(v =>
            {
                if (v)
                {
                    springMass.AddImpulse(10f * Vector3.down);
                    jellyTweenSelector.query.activeClasses.Clear();
                    jellyTweenSelector.query.activeStates.Clear();
                    jellyTweenSelector.query.activeClasses.Add("prejump");
                    jellyTweenSelector.query.Execute();
                }
                else
                {
                    //* 착지 시 충격으로 인한 흔들림 표현함
                    springMass.AddImpulse(10f * Vector3.down);
                    jellyTweenSelector.query.activeClasses.Clear();
                    jellyTweenSelector.query.activeStates.Clear();
                    jellyTweenSelector.query.activeClasses.Add("landing");
                    jellyTweenSelector.query.activeStates.Add("small");
                    jellyTweenSelector.query.Execute();
                }
            }).AddTo(this);

            __brain.BB.isBumping.Skip(1).Subscribe(v =>
            {
                if (v)
                {
                    springMass.AddImpulse(10f * Vector3.down);
                }
                else
                {
                    //* 착지 시 충격으로 인한 흔들림 표현함
                    springMass.AddImpulse(10f * Vector3.down);
                    jellyTweenSelector.query.activeClasses.Clear();
                    jellyTweenSelector.query.activeStates.Clear();
                    jellyTweenSelector.query.activeClasses.Add("landing");
                    jellyTweenSelector.query.activeStates.Add("small");
                    jellyTweenSelector.query.Execute();
                }
            }).AddTo(this);

            __brain.BB.isSmashing.Skip(1).Subscribe(v =>
            {
                if (v)
                {
                    springMass.AddImpulse(10f * Vector3.down);
                    jellyTweenSelector.query.activeClasses.Clear();
                    jellyTweenSelector.query.activeStates.Clear();
                    jellyTweenSelector.query.activeClasses.Add("prejump");
                    jellyTweenSelector.query.Execute();
                }
                else
                {
                    //* 착지 시 충격으로 인한 흔들림 표현함
                    springMass.AddImpulse(10f * Vector3.down);
                    jellyTweenSelector.query.activeClasses.Clear();
                    jellyTweenSelector.query.activeStates.Clear();
                    jellyTweenSelector.query.activeClasses.Add("landing");
                    jellyTweenSelector.query.activeStates.Add("big");
                    jellyTweenSelector.query.Execute();

                    //* 카메라 쉐이킹
                    // GameContext.Instance.cameraCtrler.Shake(0.2f, 0.4f);
                }
            }).AddTo(this);

            __brain.BB.swellingLevel.Subscribe(v =>
            {
                if (v > 0)
                {
                    if (v == 1)
                    {
                        jellyTweenSelector.query.activeClasses.Clear();
                        jellyTweenSelector.query.activeStates.Clear();
                        jellyTweenSelector.query.activeClasses.Add("swell");
                    }

                    jellyTweenSelector.query.activeStates.Add($"{v}");
                    jellyTweenSelector.query.Execute();
                }
                else
                {
                    jellyTweenSelector.query.activeClasses.Clear();
                    jellyTweenSelector.query.activeClasses.Add("pop");
                    jellyTweenSelector.query.Execute();
                }
            }).AddTo(this);

            __brain.onUpdate += () =>
            {
                //* Core가 땅 밑으로 들어가지 안도록 최소 높이값을 조정함
                if (jellySocket.transform.localPosition.y != springMass.CoreRadius)
                    jellySocket.transform.localPosition = springMass.CoreRadius * Vector3.up;

                if (__brain.ActionCtrler.CheckActionRunning())
                {
                    // if (__brain.ActionCtrler.currActionContext.rootMotionCurve != null)
                    // {
                    //     var rootMotionVec = Mathf.Max(0f, __brain.ActionCtrler.EvaluateRootMotion(Time.deltaTime)) * __brain.coreColliderHelper.transform.forward.Vector2D().normalized;
                    //     if (__brain.ActionCtrler.CanRootMotion(rootMotionVec))
                    //         __brain.Movement.AddRootMotion(Mathf.Max(0f, __brain.ActionCtrler.EvaluateRootMotion(Time.deltaTime)) * __brain.coreColliderHelper.transform.forward.Vector2D().normalized, Quaternion.identity);
                    // }
                }

                if (__brain.BB.IsDead)
                    eyeAnimator.MinOpenValue = Mathf.Clamp01(eyeAnimator.MinOpenValue - Time.deltaTime);
            };

            __brain.onLateUpdate += () =>
            {
                if (eyeSocket != null)
                    eyeAnimator.transform.position = eyeSocket.position;

                if (!__brain.BB.IsDead && __brain.BB.TargetBrain != null)
                    lookAt.position = __brain.BB.TargetBrain.coreColliderHelper.GetWorldCenter();
                else
                    lookAt.position = __brain.Movement.IsMovingToDestination ? __brain.Movement.destination : __brain.coreColliderHelper.transform.position + __brain.SensorCtrler.visionLen * __brain.coreColliderHelper.transform.forward;
            };
        }
    }
}