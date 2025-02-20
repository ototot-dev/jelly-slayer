using System;
using System.Collections.Generic;
using FIMSpace.BonesStimulation;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Game
{
    public class HeroAnimController : PawnAnimController
    {
        [Header("Component")]
        public OverrideTransform spineOverrideTransform;
        public TwoBoneIKConstraint leftArmTwoBoneIK;
        public TwoBoneIKConstraint rightArmTwoBoneIK;
        public BonesStimulator leftLegBoneSimulator;
        public BonesStimulator rightLegBoneSimulator;
        public Transform weaponMeshSlot;
        public Transform shieldMeshSlot;
        public Transform HeadLookAt;
        public Transform hipBone;

        [Header("Parameter")]
        public float guardParryRootMotionMultiplier = 1f;
        public float animLayerBlendSpeed = 1f;
        public float legAnimGlueBlendSpeed = 1f;
        public AnimationClip[] blockAdditiveAnimClips;

        //* Animator 레이어 인덱스 값 
        enum LayerIndices : int
        {
            Base = 0,
            Upper,
            Arms,
            Action,
            Addictive,
            Max,
        }

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
        public bool CheckWatchingState(string stateName) => __watchingStateNames.Contains(stateName);

        void Start()
        {
            __brain.BB.body.isGuarding.CombineLatest(__brain.BB.body.isCharging, (a, b) => new Tuple<bool, bool>(a, b)).Subscribe(v =>
            {   
                if (!v.Item1 && !v.Item2)
                {
                    mainAnimator.SetBool("IsGuarding", false);
                    __brain.BB.graphics.forceShieldRenderer.transform.localScale = Vector3.one;
                }
            }).AddTo(this);

            __brain.BB.body.isRolling.Subscribe(v =>
            {
                if (v)
                    legAnimator.User_FadeEnabled(0f);
                else
                    legAnimator.User_FadeToDisabled(0f);

            }).AddTo(this);

            FindObservableStateMachineTriggerEx("Empty (UpperLayer)").OnStateEnterAsObservable().Subscribe(_ => mainAnimator.SetLayerWeight(1, 0f)).AddTo(this);
            FindObservableStateMachineTriggerEx("Empty (UpperLayer)").OnStateExitAsObservable().Subscribe(_ => mainAnimator.SetLayerWeight(1, 1f)).AddTo(this);
            FindObservableStateMachineTriggerEx("DrinkPotion").OnStateEnterAsObservable().Subscribe(s => __watchingStateNames.Add("DrinkPotion")).AddTo(this);
            FindObservableStateMachineTriggerEx("DrinkPotion").OnStateExitAsObservable().Subscribe(s => __watchingStateNames.Remove("DrinkPotion")).AddTo(this);
            FindObservableStateMachineTriggerEx("GuardParry").OnStateEnterAsObservable().Subscribe(s => __watchingStateNames.Add("GuardParry")).AddTo(this);
            FindObservableStateMachineTriggerEx("GuardParry").OnStateExitAsObservable().Subscribe(s => __watchingStateNames.Remove("GuardParry")).AddTo(this);
            FindObservableStateMachineTriggerEx("OnDown (Start)").OnStateEnterAsObservable().Subscribe(s => 
            {
                __watchingStateNames.Add("OnDown");
                legAnimator.User_FadeToDisabled(0.1f);
                ragdollAnimator.Handler.AnimatingMode = FIMSpace.FProceduralAnimation.RagdollHandler.EAnimatingMode.Standing;
                Observable.Timer(TimeSpan.FromSeconds(0.1f)).Subscribe(_ => ragdollAnimator.Handler.AnimatingMode = FIMSpace.FProceduralAnimation.RagdollHandler.EAnimatingMode.Falling).AddTo(this);
            }).AddTo(this);
            FindObservableStateMachineTriggerEx("OnDown (End)").OnStateEnterAsObservable().Subscribe(s => 
            {
                __brain.Movement.GetCharacterMovement().SetPosition(__brain.AnimCtrler.ragdollAnimator.Handler.DummyReference.transform.GetChild(0).position);
                ragdollAnimator.Handler.AnimatingMode = FIMSpace.FProceduralAnimation.RagdollHandler.EAnimatingMode.Standing;
            }).AddTo(this);
            FindObservableStateMachineTriggerEx("OnDown (End)").OnStateExitAsObservable().Subscribe(s => 
            {
                __watchingStateNames.Remove("OnDown");
                legAnimator.User_FadeEnabled(0.1f);
                // __brain.Movement.GetCharacterMovement().SetPosition(__brain.AnimCtrler.ragdollAnimator.Handler.DummyReference.transform.GetChild(0).position);
                ragdollAnimator.Handler.AnimatingMode = FIMSpace.FProceduralAnimation.RagdollHandler.EAnimatingMode.Off;
            }).AddTo(this);

            spineOverrideTransform.weight = 1f;

            __brain.onUpdate += () =>
            {
                //* Down, Dead 상태에선 별도의 Animation 처리를 모두 끈다.
                if (__watchingStateNames.Contains("OnDown") || __watchingStateNames.Contains("OnDead"))
                {
                    rigSetup.weight = 0f;
                    spineOverrideTransform.weight = 0f;
                    leftArmTwoBoneIK.weight = rightArmTwoBoneIK.weight = 0f;
                    leftLegBoneSimulator.StimulatorAmount = rightLegBoneSimulator.StimulatorAmount = 0f;
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(false);
                    legAnimator.MainGlueBlend = 0f;

                    mainAnimator.transform.SetPositionAndRotation(__brain.coreColliderHelper.transform.position, __brain.coreColliderHelper.transform.rotation);
                    mainAnimator.SetLayerWeight((int)LayerIndices.Arms, 0f);
                    mainAnimator.SetLayerWeight((int)LayerIndices.Upper, 0f);
                    mainAnimator.SetLayerWeight((int)LayerIndices.Action, 1f);
                    mainAnimator.SetBool("IsGuarding", false);
                    mainAnimator.SetBool("IsMoving", false);
                     
                    return;
                }

                if ((__brain.ActionCtrler.CheckActionRunning() || __watchingStateNames.Contains("OnDown")) && __brain.ActionCtrler.CanRootMotion(mainAnimator.deltaPosition))
                {
                    //* 평면 방향 RootMotion에 대한 Constraints가 존재하면 값을 0으로 변경해준다.
                    if (__brain.ActionCtrler.CheckRootMotionConstraint(RootMotionConstraints.FreezePositionX, RootMotionConstraints.FreezePositionZ))
                        __brain.Movement.AddRootMotion(__brain.ActionCtrler.GetRootMotionMultiplier() * mainAnimator.deltaPosition.AdjustXZ(0f, 0f), mainAnimator.deltaRotation);
                    else
                        __brain.Movement.AddRootMotion(__brain.ActionCtrler.GetRootMotionMultiplier() * mainAnimator.deltaPosition, mainAnimator.deltaRotation);
                }
                else if (__watchingStateNames.Contains("GuardParry"))
                {
                    __brain.Movement.AddRootMotion(guardParryRootMotionMultiplier * mainAnimator.deltaPosition, Quaternion.identity);
                }

                mainAnimator.transform.SetPositionAndRotation(__brain.coreColliderHelper.transform.position, __brain.coreColliderHelper.transform.rotation);

                if (__brain.BB.IsRolling)
                {
                    rigSetup.weight = 0f;
                    spineOverrideTransform.weight = 0f;
                    leftArmTwoBoneIK.weight = rightArmTwoBoneIK.weight = 0f;
                    leftLegBoneSimulator.StimulatorAmount = rightLegBoneSimulator.StimulatorAmount = 0f;

                    mainAnimator.SetLayerWeight((int)LayerIndices.Arms, 0f);
                    mainAnimator.SetLayerWeight((int)LayerIndices.Upper, 0f);
                    mainAnimator.SetLayerWeight((int)LayerIndices.Action, 1f);
                    mainAnimator.SetFloat("MoveSpeed", 0f);
                    mainAnimator.SetFloat("MoveAnimSpeed", 1f);
                    mainAnimator.SetBool("IsMoving", false);

                    if (!legAnimator.enabled)
                        legAnimator.User_FadeToDisabled(0);
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(false);
                    legAnimator.MainGlueBlend = 0f;
                }
                else if (__brain.BB.IsHanging)
                {
                    rigSetup.weight = 1f;
                    spineOverrideTransform.weight = 0f;
                    leftArmTwoBoneIK.weight = leftArmTwoBoneIK.weight.LerpSpeed(1f, 4f, Time.deltaTime);
                    rightArmTwoBoneIK.weight = leftArmTwoBoneIK.weight.LerpSpeed(1f, 4f, Time.deltaTime);
                    leftLegBoneSimulator.StimulatorAmount = leftLegBoneSimulator.StimulatorAmount.LerpSpeed(0.4f, 1f, Time.deltaTime);
                    rightLegBoneSimulator.StimulatorAmount = rightLegBoneSimulator.StimulatorAmount.LerpSpeed(0.5f, 1f, Time.deltaTime);

                    mainAnimator.SetLayerWeight((int)LayerIndices.Arms, 0f);
                    mainAnimator.SetLayerWeight((int)LayerIndices.Upper, 0f);
                    mainAnimator.SetLayerWeight((int)LayerIndices.Action, Mathf.Clamp01(mainAnimator.GetLayerWeight((int)LayerIndices.Action) + (__brain.ActionCtrler.CheckActionRunning() ? animLayerBlendSpeed : -animLayerBlendSpeed) * Time.deltaTime));
                    mainAnimator.SetFloat("MoveSpeed", 0f);
                    mainAnimator.SetFloat("MoveAnimSpeed", 1f);
                    mainAnimator.SetBool("IsMoving", false);

                    if (!legAnimator.enabled)
                        legAnimator.User_FadeToDisabled(0);
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(false);
                    legAnimator.MainGlueBlend = 0f;
                }
                else
                {
                    rigSetup.weight = 1f;

                    if (__brain.ActionCtrler.CheckActionRunning() || mainAnimator.GetBool("IsGuarding") || __watchingStateNames.Contains("GuardParry") || __watchingStateNames.Contains("DrinkPotion"))
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
                        leftArmTwoBoneIK.weight = leftArmTwoBoneIK.weight.LerpSpeed(0f, 4f, Time.deltaTime);
                        rightArmTwoBoneIK.weight = leftArmTwoBoneIK.weight.LerpSpeed(0f, 4f, Time.deltaTime);
                        leftLegBoneSimulator.StimulatorAmount = leftLegBoneSimulator.StimulatorAmount.LerpSpeed(0f, 1f, Time.deltaTime);
                        rightLegBoneSimulator.StimulatorAmount = rightLegBoneSimulator.StimulatorAmount.LerpSpeed(0f, 1f, Time.deltaTime);
                    }

                    if (__brain.StatusCtrler.CheckStatus(PawnStatus.Staggered) || __brain.StatusCtrler.CheckStatus(PawnStatus.CanNotGuard))
                        mainAnimator.SetBool("IsGuarding", false);
                    else if (__brain.BB.IsGuarding)
                        mainAnimator.SetBool("IsGuarding", true);

                    if (mainAnimator.GetBool("IsGuarding"))
                    {
                        shieldMeshSlot.transform.localEulerAngles = new Vector3(0f, -45f, -90f);
                        __brain.BB.graphics.forceShieldRenderer.transform.localScale = 2f * Vector3.one;
                    }
                    else
                    {
                        shieldMeshSlot.transform.localEulerAngles = new Vector3(0f, -90f, -90f);
                        __brain.BB.graphics.forceShieldRenderer.transform.localScale = Vector3.one;
                    }

                    // TODO: healingPotion Show/Hide 임시 코드
                    if (__watchingStateNames.Contains("DrinkPotion"))
                    {
                        if (!__brain.BB.attachment.healingPotion.activeSelf) 
                            __brain.BB.attachment.healingPotion.SetActive(true);
                    }
                    else
                    {
                        if (__brain.BB.attachment.healingPotion.activeSelf) 
                            __brain.BB.attachment.healingPotion.SetActive(false);
                    }

                    mainAnimator.SetLayerWeight((int)LayerIndices.Arms, __brain.BB.IsJumping || __watchingStateNames.Contains("DrinkPotion") ? 0f : 1f);
                    mainAnimator.SetLayerWeight((int)LayerIndices.Upper, __watchingStateNames.Contains("DrinkPotion") ? 1f : 0f);

                    if (__watchingStateNames.Contains("DrinkPotion"))
                        mainAnimator.SetLayerWeight((int)LayerIndices.Action, 0f);
                    else if (__brain.BB.IsRolling || __watchingStateNames.Contains("GuardParry"))
                        mainAnimator.SetLayerWeight((int)LayerIndices.Action, 1f);
                    else
                        mainAnimator.SetLayerWeight((int)LayerIndices.Action, Mathf.Clamp01(mainAnimator.GetLayerWeight((int)LayerIndices.Action) + (__brain.ActionCtrler.CheckActionRunning() ? animLayerBlendSpeed : -animLayerBlendSpeed) * Time.deltaTime));

                    mainAnimator.SetFloat("MoveSpeed", __brain.Movement.freezeRotation ? -1 : __brain.Movement.CurrVelocity.Vector2D().magnitude / __brain.BB.body.walkSpeed);
                    mainAnimator.SetFloat("MoveAnimSpeed", __brain.Movement.freezeRotation ? (__brain.BB.IsGuarding ? 0.8f : 1.2f) : 1f);
                    mainAnimator.SetBool("IsMoving", __brain.Movement.CurrVelocity.sqrMagnitude > 0f);

                    var animMoveVec = __brain.coreColliderHelper.transform.InverseTransformDirection(__brain.Movement.CurrVelocity).Vector2D();
                    mainAnimator.SetFloat("MoveX", animMoveVec.x / __brain.Movement.moveSpeed);
                    mainAnimator.SetFloat("MoveY", animMoveVec.z / __brain.Movement.moveSpeed);

                    legAnimator.User_SetIsMoving(__brain.Movement.CurrVelocity.sqrMagnitude > 0f);
                    legAnimator.User_SetIsGrounded(__brain.Movement.IsOnGround && !__brain.BB.IsJumping && (!__brain.ActionCtrler.CheckActionRunning() || __brain.ActionCtrler.currActionContext.legAnimGlueEnabled));
                    legAnimator.MainGlueBlend = Mathf.Clamp(legAnimator.MainGlueBlend + (__brain.Movement.CurrVelocity.sqrMagnitude > 0f ? -1f : 1f) * legAnimGlueBlendSpeed * Time.deltaTime, __brain.Movement.freezeRotation ? 0.6f : 0.5f, 1f);
                }
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