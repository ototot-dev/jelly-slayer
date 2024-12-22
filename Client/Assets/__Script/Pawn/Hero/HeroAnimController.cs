using System;
using System.Collections.Generic;
using FIMSpace.BonesStimulation;
using FIMSpace.FProceduralAnimation;
using NodeCanvas.Framework.Internal;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Game
{
    public class HeroAnimController : PawnAnimController
    {
        [Header("Component")]
        public RagdollAnimator2 ragdollAnimator;
        public OverrideTransform spineOverrideTransform;
        public TwoBoneIKConstraint leftArmTwoBoneIK;
        public TwoBoneIKConstraint rightArmTwoBoneIK;
        public BonesStimulator leftLegBoneSimulator;
        public BonesStimulator rightLegBoneSimulator;
        public Transform shieldMeshSlot;
        public Transform weaponMeshSlot;
        public Transform HeadLookAt;
        public Transform hipBone;

        [Header("Parameter")]
        public float animLayerBlendSpeed = 1;
        public float legAnimGlueBlendSpeed = 1;
        public AnimationClip[] blockAdditiveAnimClips;

        void Awake()
        {
            __brain = GetComponent<HeroBrain>();
#if UNITY_EDITOR
            //* Block 애님의 Additive Ref-Pose를 셋팅
            foreach (var c in blockAdditiveAnimClips)
                AnimationUtility.SetAdditiveReferencePose(c, blockAdditiveAnimClips[0], 0);
#endif
        }

        HeroBrain __brain;
        HashSet<string> __watchingStateNames = new();

        void Start()
        {
            __brain.BB.action.isGuarding.CombineLatest(__brain.BB.action.isCharging, (a, b) => new Tuple<bool, bool>(a, b)).Subscribe(v =>
            {   
                if (!v.Item1 && !v.Item2)
                {
                    mainAnimator.SetBool("IsGuarding", false);
                    shieldMeshSlot.localEulerAngles = Vector3.zero;
                }
            }).AddTo(this);

            FindObservableStateMachineTriggerEx("Empty (UpperLayer)").OnStateEnterAsObservable().Subscribe(_ => mainAnimator.SetLayerWeight(1, 0f)).AddTo(this);
            FindObservableStateMachineTriggerEx("Empty (UpperLayer)").OnStateExitAsObservable().Subscribe(_ => mainAnimator.SetLayerWeight(1, 1f)).AddTo(this);
            FindObservableStateMachineTriggerEx("DrinkPotion").OnStateEnterAsObservable().Subscribe(s => __watchingStateNames.Add("DrinkPotion")).AddTo(this);
            FindObservableStateMachineTriggerEx("DrinkPotion").OnStateExitAsObservable().Subscribe(s => __watchingStateNames.Remove("DrinkPotion")).AddTo(this);
            FindObservableStateMachineTriggerEx("GuardParry").OnStateEnterAsObservable().Subscribe(s => __watchingStateNames.Add("GuardParry")).AddTo(this);
            FindObservableStateMachineTriggerEx("GuardParry").OnStateExitAsObservable().Subscribe(s => __watchingStateNames.Remove("GuardParry")).AddTo(this);

            mainAnimator.SetLayerWeight(1, 0f);
            spineOverrideTransform.weight = 1f;

            __brain.onUpdate += () =>
            {
                if (__brain.ActionCtrler.CheckActionRunning())
                {
                    if (__brain.ActionCtrler.CanRootMotion(mainAnimator.deltaPosition))
                        __brain.Movement.AddRootMotion(__brain.ActionCtrler.currActionContext.rootMotionMultiplier * mainAnimator.deltaPosition, mainAnimator.deltaRotation);

                    if (__brain.ActionCtrler.currActionContext.rootMotionCurve != null)
                    {
                        var rootMotionVec = __brain.ActionCtrler.EvaluateRootMotion(Time.deltaTime) * __brain.coreColliderHelper.transform.forward.Vector2D().normalized;
                        if (__brain.ActionCtrler.CanRootMotion(rootMotionVec))
                            __brain.Movement.AddRootMotion(__brain.ActionCtrler.EvaluateRootMotion(Time.deltaTime) * __brain.coreColliderHelper.transform.forward.Vector2D().normalized, Quaternion.identity);
                    }
                }
                else if (__watchingStateNames.Contains("GuardParry"))
                {
                    __brain.Movement.AddRootMotion(2f * mainAnimator.deltaPosition, Quaternion.identity);
                }

                if (__brain.BB.IsHanging || __brain.ActionCtrler.CheckActionRunning() || mainAnimator.GetBool("IsGuarding") || __watchingStateNames.Contains("DrinkPotion") || __watchingStateNames.Contains("GuardParry"))
                    spineOverrideTransform.weight = 0f;
                else
                    spineOverrideTransform.weight = 1f;

                if (__brain.ActionCtrler.CheckActionRunning())
                {
                    leftArmTwoBoneIK.weight = rightArmTwoBoneIK.weight = 0f;
                    leftLegBoneSimulator.StimulatorAmount = rightLegBoneSimulator.StimulatorAmount = 0f;
                }
                else
                {
                    leftArmTwoBoneIK.weight = leftArmTwoBoneIK.weight.LerpSpeed(__brain.BB.IsHanging ? 1f : 0f, 4f, Time.deltaTime);
                    rightArmTwoBoneIK.weight = leftArmTwoBoneIK.weight.LerpSpeed(__brain.BB.IsHanging ? 1f : 0f, 4f, Time.deltaTime);
                    leftLegBoneSimulator.StimulatorAmount = leftLegBoneSimulator.StimulatorAmount.LerpSpeed(__brain.BB.IsHanging ? 0.4f : 0f, 1f, Time.deltaTime);
                    rightLegBoneSimulator.StimulatorAmount = rightLegBoneSimulator.StimulatorAmount.LerpSpeed(__brain.BB.IsHanging ? 0.5f : 0f, 1f, Time.deltaTime);
                }

                if (__brain.StatusCtrler.CheckStatus(PawnStatus.Staggered) || __brain.StatusCtrler.CheckStatus(PawnStatus.CanNotGuard))
                {
                    mainAnimator.SetBool("IsGuarding", false);
                    shieldMeshSlot.localEulerAngles = Vector3.zero;
                }
                else if (__brain.BB.IsGuarding || __brain.BB.IsCharging)
                {
                    mainAnimator.SetBool("IsGuarding", true);
                    shieldMeshSlot.localEulerAngles = new Vector3(0f, 0f, -60f);
                }

                mainAnimator.transform.SetPositionAndRotation(__brain.coreColliderHelper.transform.position, __brain.coreColliderHelper.transform.rotation);
                mainAnimator.SetLayerWeight(2, __brain.BB.IsJumping || __brain.BB.IsHanging ? 0f : 1f);

                if (__watchingStateNames.Contains("GuardParry"))
                    mainAnimator.SetLayerWeight(3, 1f);
                else if (__watchingStateNames.Contains("DrinkPotion"))
                    mainAnimator.SetLayerWeight(3, 0f);
                else
                    mainAnimator.SetLayerWeight(3, Mathf.Clamp01(mainAnimator.GetLayerWeight(3) + ((__brain.BB.IsRolling || __brain.ActionCtrler.CheckActionRunning()) ? animLayerBlendSpeed : -animLayerBlendSpeed) * Time.deltaTime));

                mainAnimator.SetFloat("MoveSpeed", __brain.Movement.freezeRotation ? -1 : __brain.Movement.CurrVelocity.Vector2D().magnitude / __brain.BB.body.walkSpeed);
                mainAnimator.SetFloat("MoveAnimSpeed", __brain.Movement.freezeRotation ? (__brain.BB.IsGuarding ? 0.8f : 1.2f) : 1);
                mainAnimator.SetBool("IsMoving", __brain.Movement.CurrVelocity.sqrMagnitude > 0);

                var animMoveVec = __brain.coreColliderHelper.transform.InverseTransformDirection(__brain.Movement.CurrVelocity).Vector2D();
                mainAnimator.SetFloat("MoveX", animMoveVec.x / __brain.Movement.moveSpeed);
                mainAnimator.SetFloat("MoveY", animMoveVec.z / __brain.Movement.moveSpeed);

                legAnimator.User_SetIsMoving(__brain.Movement.CurrVelocity.sqrMagnitude > 0);
                legAnimator.User_SetIsGrounded(__brain.Movement.IsOnGround && !__brain.BB.IsRolling && !__brain.BB.IsJumping && !__brain.BB.IsHanging && !__brain.BB.IsDown && !__brain.BB.IsDead && (!__brain.ActionCtrler.CheckActionRunning() || __brain.ActionCtrler.currActionContext.legAnimGlueEnabled));
                legAnimator.MainGlueBlend = Mathf.Clamp(legAnimator.MainGlueBlend + (__brain.Movement.CurrVelocity.sqrMagnitude > 0 ? -1 : 1) * legAnimGlueBlendSpeed * Time.deltaTime, __brain.Movement.freezeRotation ? 0.5f : 0.4f, 1);
            };
        }

        public bool Jump() 
        {
            mainAnimator.SetTrigger("Jump");
            mainAnimator.SetBool("IsJumping", true);

            return true;
        }

        public void Dash()
        {
            mainAnimator.SetTrigger("Rolling");
        }

        public void OnEventLand() 
        {
            mainAnimator.SetBool("IsJumping", false);
        }
        /*
        public void ChangeWeapon(WEAPONSLOT weaponSlot)
        {
            switch (weaponSlot) {
                case WEAPONSLOT.MAINSLOT:
                    mainAnimator.runtimeAnimatorController = _animControllers[0];
                    break;
                case WEAPONSLOT.SUBSLOT:
                    mainAnimator.runtimeAnimatorController = _animControllers[1];
                    break;
            }
        }
        */
    }

}