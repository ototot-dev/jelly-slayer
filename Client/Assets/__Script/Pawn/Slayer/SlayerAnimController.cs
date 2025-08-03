using System;
using System.Linq;
using FIMSpace.BonesStimulation;
using MainTable;
using UniRx;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using ZLinq;

namespace Game
{
    public class SlayerAnimController : PawnAnimController
    {
        [Header("Component")]
        public OverrideTransform spineOverride;
        public MultiAimConstraint headMultiAim;
        public TwoBoneIKConstraint leftArmTwoBoneIK;
        public TwoBoneIKConstraint rightArmTwoBoneIK;
        public BonesStimulator leftLegBoneSimulator;
        public BonesStimulator rightLegBoneSimulator;
        public Transform weaponMeshSlot;
        public Transform HeadLookAt;
        public Transform hipBone;

        [Header("Parameter")]
        public float actionLayerBlendInSpeed = 1f;
        public float actionLayerBlendOutSpeed = 1f;
        public float legAnimatorBlendInSpeed = 1f;
        public float legAnimatorBlendOutSpeed = 1f;
        public float ragdollAnimatorBlendInSpeed = 1f;
        public float ragdollAnimatorBlendOutSpeed = 1f;
        public float guardParryRootMotionMultiplier = 1f;

        //* Animator 레이어 인덱스 값 
        enum LayerIndices : int
        {
            Base = 0,
            Action,
            LeftArm,
            RightArm,
            Addictive,
            Interaction,
            Max,
        }

        void Awake()
        {
            __brain = GetComponent<SlayerBrain>();
        }

        SlayerBrain __brain;
        ActionData __punchActionData;
        ActionData __punchParryActionData;
        ActionData __chainsawActionData;
        Vector3[] __8waysBlendTreePosXY;

        protected override float GetLegAnimatorBlendSpeed()
        {
            // if (ragdollAnimator.Handler.AnimatingMode == FIMSpace.FProceduralAnimation.RagdollHandler.EAnimatingMode.Falling)
            if (CheckAnimStateTriggered("Fall (Pose)"))
                return -legAnimatorBlendOutSpeed;
            else if (__brain.BB.IsRolling || __brain.BB.IsHanging)
                return -legAnimatorBlendOutSpeed;

            return legAnimatorBlendInSpeed;
        }

        protected override float GetRagdollAnimatorBlendSpeed()
        {
            if (__brain.BB.IsDown || __brain.BB.IsDead)
                return ragdollAnimatorBlendInSpeed;

            return -ragdollAnimatorBlendOutSpeed;
        }

        public override void OnAnimatorMoveHandler()
        {
            if (IsRootMotionForced())
            {
                __brain.Movement.AddRootMotion(GetForecedRootMotionMultiplier() * mainAnimator.deltaPosition, Quaternion.identity, Time.deltaTime);
            }
            else if (__brain.ActionCtrler.CheckActionRunning() && __brain.ActionCtrler.CanRootMotion(mainAnimator.deltaPosition))
            {
                //* 평면 방향 RootMotion에 대한 Constraints가 존재하면 값을 0으로 변경해준다.
                if (__brain.ActionCtrler.CheckRootMotionConstraints(RootMotionConstraints.FreezePositionX, RootMotionConstraints.FreezePositionZ))
                    __brain.Movement.AddRootMotion(__brain.ActionCtrler.GetRootMotionMultiplier() * mainAnimator.deltaPosition.AdjustXZ(0f, 0f), mainAnimator.deltaRotation, Time.deltaTime);
                else
                    __brain.Movement.AddRootMotion(__brain.ActionCtrler.GetRootMotionMultiplier() * mainAnimator.deltaPosition, mainAnimator.deltaRotation, Time.deltaTime);
            }
        }

        void UpdateCombatMode()
        {
            mainAnimator.SetLayerWeight((int)LayerIndices.Interaction, 0f);

            legAnimator.LegsAnimatorBlend = Mathf.Clamp01(legAnimator.LegsAnimatorBlend + GetLegAnimatorBlendSpeed() * Time.deltaTime);

            if (legAnimator.enabled && legAnimator.LegsAnimatorBlend <= 0f)
                legAnimator.enabled = false;
            else if (!legAnimator.enabled && legAnimator.LegsAnimatorBlend > 0f)
                legAnimator.enabled = true;

            ragdollAnimator.RagdollBlend = Mathf.Clamp(ragdollAnimator.RagdollBlend + GetRagdollAnimatorBlendSpeed() * Time.deltaTime, 0.01f, 1f);

            //* Down, Dead 상태에선 Animation 처리를 모두 끈다.
            if (__brain.BB.IsDown || __brain.BB.IsDead)
            {
                rigSetup.weight = 0f;
                spineOverride.weight = 0f;
                headMultiAim.weight = 0f;
                leftArmTwoBoneIK.weight = rightArmTwoBoneIK.weight = 0f;
                leftLegBoneSimulator.StimulatorAmount = rightLegBoneSimulator.StimulatorAmount = 0f;

                mainAnimator.SetLayerWeight((int)LayerIndices.LeftArm, 0f);
                mainAnimator.SetLayerWeight((int)LayerIndices.RightArm, 0f);
                mainAnimator.SetLayerWeight((int)LayerIndices.Action, __brain.ActionCtrler.GetAdvancedActionLayerWeight(mainAnimator.GetLayerWeight((int)LayerIndices.Action), actionLayerBlendInSpeed, actionLayerBlendOutSpeed, Time.deltaTime));

                mainAnimator.SetBool("IsGuarding", false);
                mainAnimator.SetBool("IsMoving", false);
            }
            else if (__brain.BB.IsRolling)
            {
                rigSetup.weight = 0f;
                spineOverride.weight = 0f;
                headMultiAim.weight = 0f;
                leftArmTwoBoneIK.weight = rightArmTwoBoneIK.weight = 0f;
                leftLegBoneSimulator.StimulatorAmount = rightLegBoneSimulator.StimulatorAmount = 0f;

                mainAnimator.SetLayerWeight((int)LayerIndices.LeftArm, 0f);
                mainAnimator.SetLayerWeight((int)LayerIndices.RightArm, 0f);
                mainAnimator.SetLayerWeight((int)LayerIndices.Action, 1f);

                mainAnimator.SetBool("IsMoving", false);
                mainAnimator.SetFloat("MoveSpeed", 0f);
                mainAnimator.SetFloat("MoveAnimSpeed", 1f);
            }
            else if (__brain.BB.IsHanging)
            {
                rigSetup.weight = 1f;
                spineOverride.weight = 0f;
                headMultiAim.weight = 1f;
                leftArmTwoBoneIK.weight = leftArmTwoBoneIK.weight.LerpSpeed(1f, 4f, Time.deltaTime);
                rightArmTwoBoneIK.weight = leftArmTwoBoneIK.weight.LerpSpeed(1f, 4f, Time.deltaTime);
                leftLegBoneSimulator.StimulatorAmount = leftLegBoneSimulator.StimulatorAmount.LerpSpeed(0.4f, 1f, Time.deltaTime);
                rightLegBoneSimulator.StimulatorAmount = rightLegBoneSimulator.StimulatorAmount.LerpSpeed(0.5f, 1f, Time.deltaTime);

                mainAnimator.SetLayerWeight((int)LayerIndices.LeftArm, 0f);
                mainAnimator.SetLayerWeight((int)LayerIndices.RightArm, 0f);
                mainAnimator.SetLayerWeight((int)LayerIndices.Action, __brain.ActionCtrler.GetAdvancedActionLayerWeight(mainAnimator.GetLayerWeight((int)LayerIndices.Action), actionLayerBlendInSpeed, actionLayerBlendOutSpeed, Time.deltaTime));

                mainAnimator.SetBool("IsMoving", false);
                mainAnimator.SetFloat("MoveSpeed", 0f);
                mainAnimator.SetFloat("MoveAnimSpeed", 1f);
                mainAnimator.SetFloat("HangingBlendWeight", 1f);
            }
            else
            {
                rigSetup.weight = 1f;

                if (__brain.ActionCtrler.CheckActionRunning())
                {
                    headMultiAim.weight = 0f;
                    spineOverride.weight = 0f;
                    leftArmTwoBoneIK.weight = rightArmTwoBoneIK.weight = 0f;
                    leftLegBoneSimulator.StimulatorAmount = rightLegBoneSimulator.StimulatorAmount = 0f;

                    __punchActionData ??= DatasheetManager.Instance.GetActionData(PawnId.Hero, "Punch");
                    __punchParryActionData ??= DatasheetManager.Instance.GetActionData(PawnId.Hero, "PunchParry");
                    __chainsawActionData ??= DatasheetManager.Instance.GetActionData(PawnId.Hero, "Chainsaw");

                    if (__brain.ActionCtrler.currActionContext.actionData == __punchActionData || __brain.ActionCtrler.currActionContext.actionData == __punchParryActionData || __brain.ActionCtrler.currActionContext.actionData == __chainsawActionData)
                    {
                        mainAnimator.SetLayerWeight((int)LayerIndices.LeftArm, 0f);
                        mainAnimator.SetLayerWeight((int)LayerIndices.RightArm, 0f);
                    }
                    else if (__brain.ActionCtrler.CurrActionName.StartsWith("Punch"))
                    {
                        mainAnimator.SetLayerWeight((int)LayerIndices.LeftArm, 0f);
                        mainAnimator.SetLayerWeight((int)LayerIndices.RightArm, 0f);
                    }
                    else
                    {
                        mainAnimator.SetLayerWeight((int)LayerIndices.LeftArm, __brain.ActionCtrler.CurrActionName == "Slash#1" || __brain.ActionCtrler.CurrActionName == "Slash#2" ? 0f : 1f);
                        mainAnimator.SetLayerWeight((int)LayerIndices.RightArm, 0f);
                    }
                }
                else
                {
                    headMultiAim.weight = 1f;
                    spineOverride.weight = (mainAnimator.GetBool("IsGuarding") || CheckAnimStateTriggered("GuardParry")) ? 1f : 0f;
                    leftArmTwoBoneIK.weight = Mathf.Clamp01(leftArmTwoBoneIK.weight.LerpSpeed(0f, 4f, Time.deltaTime));
                    rightArmTwoBoneIK.weight = Mathf.Clamp01(leftArmTwoBoneIK.weight.LerpSpeed(0f, 4f, Time.deltaTime));
                    leftLegBoneSimulator.StimulatorAmount = Mathf.Clamp01(leftLegBoneSimulator.StimulatorAmount.LerpSpeed(0f, 1f, Time.deltaTime));
                    rightLegBoneSimulator.StimulatorAmount = Mathf.Clamp01(rightLegBoneSimulator.StimulatorAmount.LerpSpeed(0f, 1f, Time.deltaTime));

                    mainAnimator.SetLayerWeight((int)LayerIndices.LeftArm, __brain.BB.IsGuarding || CheckAnimStateTriggered("DrinkPotion") ? 1f : 0.5f);
                    mainAnimator.SetLayerWeight((int)LayerIndices.RightArm, __brain.BB.IsGuarding || CheckAnimStateTriggered("DrinkPotion") ? 1f : 0f);
                }

                legAnimator.MainGlueBlend = 1f - Mathf.Clamp01(__brain.Movement.CurrVelocity.magnitude / __brain.Movement.moveSpeed);
                legAnimator.User_SetIsMoving(__brain.Movement.CurrVelocity.sqrMagnitude > 0f);
                legAnimator.User_SetIsGrounded(__brain.Movement.IsOnGround && !__brain.BB.IsJumping && (!__brain.ActionCtrler.CheckActionRunning() || __brain.ActionCtrler.currActionContext.legAnimGlueEnabled));

                if (__brain.StatusCtrler.CheckStatus(PawnStatus.Staggered) || __brain.StatusCtrler.CheckStatus(PawnStatus.CanNotGuard))
                    mainAnimator.SetBool("IsGuarding", false);
                else if (__brain.BB.IsGuarding && __brain.BB.action.punchChargingLevel.Value < 0)
                    mainAnimator.SetBool("IsGuarding", true);

                if (CheckAnimStateTriggered("GuardParry"))
                    mainAnimator.SetLayerWeight((int)LayerIndices.Action, 1f);
                else
                    mainAnimator.SetLayerWeight((int)LayerIndices.Action, __brain.ActionCtrler.GetAdvancedActionLayerWeight(mainAnimator.GetLayerWeight((int)LayerIndices.Action), actionLayerBlendInSpeed, actionLayerBlendOutSpeed, Time.deltaTime));

                mainAnimator.SetBool("IsMoving", __brain.Movement.CurrVelocity.sqrMagnitude > 0f);
                mainAnimator.SetFloat("MoveForward", __brain.Movement.freezeRotation ? -1 : __brain.Movement.CurrVelocity.Vector2D().magnitude / __brain.BB.body.walkSpeed);
                mainAnimator.SetFloat("MoveAnimSpeed", __brain.Movement.freezeRotation ? (__brain.BB.IsGuarding ? 1f : 1.5f) : 1.2f);

                __8waysBlendTreePosXY ??= new Vector3[]
                {
                    new(0f, 0f, 1f),
                    new(0f, 0f, -1f),
                    new(-1f, 0f, 1f),
                    new(-1f, 0f, 0f),
                    new(-1f, 0f, -1f),
                    new(1f, 0f, 1f),
                    new(1f, 0f, 0f),
                    new(1f, 0f, -1f),
                };

                var animMoveVec = __brain.coreColliderHelper.transform.InverseTransformDirection(__brain.Movement.CurrVelocity).Vector2D();
                var animMoveVecClamped = __8waysBlendTreePosXY.AsValueEnumerable().OrderBy(v => Vector3.Angle(v, animMoveVec)).First();

                animMoveVec = animMoveVec.magnitude * Vector3.Lerp(animMoveVec.normalized, animMoveVecClamped.normalized, 0.5f);
                mainAnimator.SetFloat("MoveX", animMoveVec.x / __brain.Movement.moveSpeed);
                mainAnimator.SetFloat("MoveY", animMoveVec.z / __brain.Movement.moveSpeed);
                mainAnimator.SetFloat("HangingBlendWeight", 0f);
            }
        }

        void UpdateDialogueMode()
        {
            mainAnimator.SetLayerWeight((int)LayerIndices.Base, 0f);
            mainAnimator.SetLayerWeight((int)LayerIndices.LeftArm, 0f);
            mainAnimator.SetLayerWeight((int)LayerIndices.RightArm, 0f);
            mainAnimator.SetLayerWeight((int)LayerIndices.Addictive, 0f);
            mainAnimator.SetLayerWeight((int)LayerIndices.Interaction, 1f);

            //* __playableGraph가 실행 중에는 Animation PostProcess는 모두 꺼줌
            if (IsPlayableGraphRunning())
            {
                rigSetup.weight = 0f;
                spineOverride.weight = 0f;
                leftArmTwoBoneIK.weight = rightArmTwoBoneIK.weight = 0f;
                leftLegBoneSimulator.StimulatorAmount = rightLegBoneSimulator.StimulatorAmount = 0f;

                if (legAnimator.enabled) legAnimator.enabled = false;

                legAnimator.User_SetIsMoving(false);
                legAnimator.User_SetIsGrounded(false);
                legAnimator.MainGlueBlend = 0f;
                legAnimator.LegsAnimatorBlend = 0f;
                ragdollAnimator.RagdollBlend = 0.01f;
            }
            else if (GameContext.Instance.launcher.currGameMode.CanPlayerConsumeInput())
            {
                if (!legAnimator.enabled) legAnimator.enabled = true;

                legAnimator.LegsAnimatorBlend = 1f;
                legAnimator.MainGlueBlend = 1f - Mathf.Clamp01(__brain.Movement.CurrVelocity.magnitude / __brain.Movement.moveSpeed);
                legAnimator.User_SetIsMoving(__brain.Movement.CurrVelocity.sqrMagnitude > 0f);
                legAnimator.User_SetIsGrounded(__brain.Movement.IsOnGround && !__brain.BB.IsJumping && (!__brain.ActionCtrler.CheckActionRunning() || __brain.ActionCtrler.currActionContext.legAnimGlueEnabled));

                mainAnimator.SetBool("IsMoving", __brain.Movement.CurrVelocity.sqrMagnitude > 0f);
                mainAnimator.SetFloat("MoveForward", __brain.Movement.freezeRotation ? -1 : __brain.Movement.CurrVelocity.Vector2D().magnitude / __brain.BB.body.walkSpeed);
                mainAnimator.SetFloat("MoveAnimSpeed", __brain.Movement.freezeRotation ? (__brain.BB.IsGuarding ? 1f : 1.5f) : 1.2f);

                var animMoveVec = __brain.coreColliderHelper.transform.InverseTransformDirection(__brain.Movement.CurrVelocity).Vector2D();
                mainAnimator.SetFloat("MoveX", animMoveVec.x / __brain.Movement.moveSpeed);
                mainAnimator.SetFloat("MoveY", animMoveVec.z / __brain.Movement.moveSpeed);
            }
        }

        void Start()
        {
            __brain.BB.body.isGuarding.CombineLatest(__brain.BB.action.punchChargingLevel, (a, b) => new Tuple<bool, int>(a, b)).Subscribe(v =>
            {
                if (v.Item2 >= 0)
                {
                    mainAnimator.SetBool("IsPunchCharging", true);
                    mainAnimator.SetBool("IsGuarding", false);
                }
                else if (v.Item1)
                {
                    mainAnimator.SetBool("IsPunchCharging", false);
                    mainAnimator.SetBool("IsGuarding", true);
                }
                else
                {
                    mainAnimator.SetBool("IsPunchCharging", false);
                    mainAnimator.SetBool("IsGuarding", false);
                }
            }).AddTo(this);

            __brain.BB.body.isRolling.Skip(1).Subscribe(v =>
            {
                if (v)
                    legAnimator.User_FadeToDisabled(0f);
                else
                    legAnimator.User_FadeEnabled(0f);

            }).AddTo(this);
            
            FindStateMachineTriggerObservable("Idle (LArm)").OnStateEnterAsObservable().Subscribe(_ => mainAnimator.SetLayerWeight(1, 0f)).AddTo(this);
            FindStateMachineTriggerObservable("Idle (LArm)").OnStateExitAsObservable().Subscribe(_ => mainAnimator.SetLayerWeight(1, 1f)).AddTo(this);

            //* PunchParry 차징 중에 애님이 특정 구간을 루핑하도록 강제로 animAdvance를 제어함
            FindStateMachineTriggerObservable("PunchParry (Charge)").OnStateEnterAsObservable().Subscribe(_ =>
            {
                var animAdvance = 0f;
                var animAdvanceOffset = 0f;
                mainAnimator.SetFloat("AnimAdvance", animAdvance);

                Observable.EveryUpdate().TakeWhile(_ => CheckAnimStateTriggered("PunchParry (Charge)"))
                    .Subscribe(_ =>
                    {
                        if (__brain.BB.action.punchChargingLevel.Value >= 0)
                        {
                            animAdvance += __brain.BB.action.punchChargingAnimAdvanceSpeed * Time.deltaTime;

                            if (animAdvance > __brain.BB.action.punchChargingAnimAdvanceEnd)
                            {
                                animAdvance = __brain.BB.action.punchChargingAnimAdvanceEnd;
                                animAdvanceOffset += __brain.BB.action.punchChargingAnimAdvanceOffsetSinFrequency * 2f * Mathf.PI * Time.deltaTime;
                                mainAnimator.SetFloat("AnimAdvance", animAdvance + __brain.BB.action.punchChargingAnimAdvanceOffsetAmplitude * Mathf.Sin(animAdvanceOffset));
                            }
                            else
                            {
                                mainAnimator.SetFloat("AnimAdvance", animAdvance);
                            }
                        }
                        else
                        {
                            animAdvance -= Time.deltaTime;
                            mainAnimator.SetFloat("AnimAdvance", animAdvance);
                        }
                    }).AddTo(this);
            }).AddTo(this);

            spineOverride.weight = 0f;

            __brain.onUpdate += () =>
            {
                if (GameContext.Instance.launcher.currGameMode.IsInCombat())
                    UpdateCombatMode();
                else
                    UpdateDialogueMode();
            };

            __brain.onLateUpdate += () =>
            {
                mainAnimator.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            };
        }
    }
}