using FIMSpace.BonesStimulation;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Game
{
    public class Etasphera42_AnimController : PawnAnimController
    {
        [Header("Component")]
        public Transform hookingPoint;
        public Transform pelves;
        public Transform turretBody;
        public Transform leftTurret;
        public Transform rightTurret;
        public Transform centerTurret;
        public Transform topTurret1;
        public Transform topTurret2;
        public BonesStimulator[] legBoneSimulators;

        [Header("Parameter")]
        public float rigBlendWeight = 1f;
        public float rigBlendSpeed = 1f;
        public float legAnimGlueBlendSpeed = 1f;
        public float ragDollAnimBlendSpeed = 1f;
        public float actionLayerBlendSpeed = 1f;
        Etasphera42_Brain __brain;
        Rig __rig;

        public enum TurretIndices : int
        {
            Base,
            Left,
            Right,
            Side,
            Center,
            Top,
            Max,
        }

        //* Animator 레이어 인덱스 값 
        enum LayerIndices : int
        {
            Base = 0,
            Action,
            Lower,
            Addictive,
            Max,
        }

        void Awake()
        {
            __brain = GetComponent<Etasphera42_Brain>();
            __turretBodyRotationCached = turretBody.transform.localRotation;
            __leftTurretRotationCached = leftTurret.transform.localRotation;
            __rightTurretRotationCached = rightTurret.transform.localRotation;
            __centerTurretRotationCached = centerTurret.transform.localRotation;
            __topTurret1_RotationCached = topTurret1.transform.localRotation;
            __topTurret2_RotationCached = topTurret2.transform.localRotation;

            __isDrivingParam = Animator.StringToHash("IsDriving");
            __onDriveTrigger = Animator.StringToHash("OnDrive");
        }

        int __isDrivingParam;
        int __onDriveTrigger;

        void Start()
        {
            // __brain.BB.action.isDriving.Subscribe(v =>
            // {
            //     if (v != mainAnimator.GetBool(__isDrivingParam))
            //     {
            //         mainAnimator.SetBool(__isDrivingParam, v);
            //         if (v) mainAnimator.SetTrigger(__onDriveTrigger);
            //     }
            // }).AddTo(this);

            __brain.onUpdate += () =>
            {
                if (mainAnimator.GetBool(__isDrivingParam))
                    pelves.transform.localRotation *= Quaternion.Euler(90f, 0f, 0f);

                // if (__brain.BB.IsDown)
                //     __brain.Movement.AddRootMotion(mainAnimator.deltaPosition, mainAnimator.deltaRotation);
                // else if (__brain.ActionCtrler.CheckActionRunning() && __brain.ActionCtrler.CanRootMotion(mainAnimator.deltaPosition))
                //     __brain.Movement.AddRootMotion(__brain.ActionCtrler.GetRootMotionMultiplier() * mainAnimator.deltaPosition, mainAnimator.deltaRotation);

                mainAnimator.transform.SetPositionAndRotation(__brain.coreColliderHelper.transform.position, __brain.coreColliderHelper.transform.rotation);
                // mainAnimator.SetLayerWeight((int)LayerIndices.Action, Mathf.Clamp01(mainAnimator.GetLayerWeight((int)LayerIndices.Action) + (__brain.ActionCtrler.CheckActionRunning() ? actionLayerBlendSpeed : -actionLayerBlendSpeed) * Time.deltaTime));
                // mainAnimator.SetLayerWeight((int)LayerIndices.Addictive, 1f);
                // mainAnimator.SetBool("IsMoving", __brain.Movement.CurrVelocity.sqrMagnitude > 0 && !__brain.ActionCtrler.CheckKnockBackRunning());
                // mainAnimator.SetBool("IsMovingStrafe", __brain.Movement.freezeRotation);
                // mainAnimator.SetFloat("MoveSpeed", __brain.Movement.CurrVelocity.magnitude);
                // mainAnimator.SetFloat("MoveAnimSpeed", 1f);

                var animMoveVec = __brain.coreColliderHelper.transform.InverseTransformDirection(__brain.Movement.CurrVelocity).Vector2D();
                // mainAnimator.SetFloat("MoveX", animMoveVec.x / __brain.Movement.moveSpeed);
                // mainAnimator.SetFloat("MoveY", animMoveVec.z / __brain.Movement.moveSpeed);

                // mainAnimator.SetLayerWeight((int)LayerIndices.Lower, 0f);

                if (__brain.BB.IsDead)
                {
                    legAnimator.LegsAnimatorBlend = Mathf.Clamp01(legAnimator.LegsAnimatorBlend - legAnimGlueBlendSpeed * Time.deltaTime);
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(false);
                }
                else if (__brain.BB.IsGroggy)
                {
                    legAnimator.LegsAnimatorBlend = 1f;
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(true);
                }
                else if (__brain.BB.IsDown || __brain.BB.IsJumping || __brain.BB.IsFalling)
                {
                    legAnimator.LegsAnimatorBlend = 0f;
                    legAnimator.User_SetIsMoving(false);
                    legAnimator.User_SetIsGrounded(false);
                }
                else
                {
                    legAnimator.LegsAnimatorBlend = 1f;
                    legAnimator.MainGlueBlend = 1f;

                    // legAnimator.User_SetIsMoving(__brain.Movement.CurrVelocity.sqrMagnitude > 0 && !__brain.ActionCtrler.CheckActionRunning() && !__brain.ActionCtrler.CheckKnockBackRunning());
                    // legAnimator.User_SetIsMoving(__brain.Movement.CurrVelocity.sqrMagnitude > 0 && !__brain.ActionCtrler.CheckActionRunning() && !__brain.ActionCtrler.CheckKnockBackRunning());
                    legAnimator.User_SetIsGrounded(__brain.Movement.IsOnGround);
                }

                ragdollAnimator.RagdollBlend = Mathf.Max(0f, ragdollAnimator.RagdollBlend - ragDollAnimBlendSpeed * Time.time);
            };

            __brain.onLateUpdate += () =>
            {
                __brain.BB.children.hookingPointColliderHelper.transform.position = hookingPoint.transform.position;

                // if (__brain.BB.IsDriving)
                //     pelves.transform.localRotation *= Quaternion.Euler(90f, 0f, 0f);

                if (__brain.BB.TargetColliderHelper != null)
                {
                    var targetPoint = __brain.BB.TargetColliderHelper.GetWorldCenter() + 0.2f * Vector3.up;
                    if (__brain.ActionCtrler.CheckActionRunning())
                    {
                        if (__brain.ActionCtrler.CurrActionName == "Bullet")
                            UpdateTurretRotation(targetPoint, __brain.BB.action.bulletTurretRotateSpeed, TurretIndices.Base, __brain.BB.action.bulletMaxShakeAngle * Mathf.Sin(Mathf.PI * 2f * (Time.time - __brain.ActionCtrler.currActionContext.startTimeStamp)));
                        else if (__brain.ActionCtrler.CurrActionName == "Torch")
                            UpdateTurretRotation(targetPoint, __brain.BB.action.torchTurretRotateSpeed, TurretIndices.Base);
                        else if (__brain.ActionCtrler.CurrActionName == "LaserA")
                            UpdateTurretRotation(__brain.BB.children.laserAimPoint.position, __brain.BB.action.laserA_turretRotateSpeed, TurretIndices.Base);
                        else 
                            UpdateTurretRotation(targetPoint, __brain.BB.body.turretRotateSpeed, TurretIndices.Base);
                        
                        UpdateTurretRotation(targetPoint, __brain.BB.body.turretRotateSpeed, TurretIndices.Left);
                        UpdateTurretRotation(targetPoint, __brain.BB.body.turretRotateSpeed, TurretIndices.Right);
                        UpdateTurretRotation(__brain.ActionCtrler.CheckAddictiveActionRunning("LaserB") ? __brain.BB.children.laserAimPoint.position : targetPoint, __brain.BB.action.laserB_turretRotateSpeed, TurretIndices.Top);

                    }
                    else
                    {
                        UpdateTurretRotation(targetPoint, __brain.BB.body.turretRotateSpeed, TurretIndices.Base);
                        UpdateTurretRotation(targetPoint, __brain.BB.body.turretRotateSpeed, TurretIndices.Left);
                        UpdateTurretRotation(targetPoint, __brain.BB.body.turretRotateSpeed, TurretIndices.Right);
                        UpdateTurretRotation(__brain.ActionCtrler.CheckAddictiveActionRunning("LaserB") ? __brain.BB.children.laserAimPoint.position : targetPoint, __brain.BB.action.laserB_turretRotateSpeed, TurretIndices.Top);
                    }
                }
            };

            __brain.PawnHP.onDead += (_) =>
            {
                mainAnimator.SetTrigger("OnDead");
            };
        }

        Quaternion __turretBodyRotationCached;
        Quaternion __leftTurretRotationCached;
        Quaternion __rightTurretRotationCached;
        Quaternion __centerTurretRotationCached;
        Quaternion __topTurret1_RotationCached;
        Quaternion __topTurret2_RotationCached;

        void UpdateTurretRotation(Vector3 targetPoint, float rotateSpeed, TurretIndices interestTurretIndex, float offsetAngle = 0f)
        {
            if (interestTurretIndex == TurretIndices.Base)
            {
                //* turretBody의 로컬 피봇으로 인해서 Right 벡터가 Up 벡터가 됨 (또한 90도 돌아가 있어서 이를 역보정해주기 위해 Vector3.down 축을 Forward 축으로 대입함) 
                var deltaAngle = Vector3.SignedAngle(Vector3.down, turretBody.transform.InverseTransformPoint(targetPoint).AdjustX(0f), Vector3.right);
                turretBody.transform.localRotation = Quaternion.Euler(offsetAngle, 0f, 0f) * __turretBodyRotationCached.LerpRefAngleSpeed(turretBody.transform.localRotation * Quaternion.Euler(deltaAngle, 0f, 0f), rotateSpeed, Time.deltaTime);
                // turretBody.transform.localRotation = __turretBodyRotationCached.LerpRefAngleSpeed(turretBody.transform.localRotation * Quaternion.Euler(deltaAngle, 0f, 0f), rotateSpeed, Time.deltaTime);

                //* centerTurret은 회전하지 않음
                centerTurret.transform.localRotation = __centerTurretRotationCached;
            }
            else if (interestTurretIndex == TurretIndices.Left)
            {
                //* leftTurret와 rightTurrent의 로컬 피봇도 Right 벡터가 Up 벡터가 됨
                var deltaAngle = Vector3.SignedAngle(Vector3.forward, leftTurret.transform.InverseTransformPoint(targetPoint).AdjustX(0f), Vector3.right);
                leftTurret.transform.localRotation = __leftTurretRotationCached.LerpRefAngleSpeed(leftTurret.transform.localRotation * Quaternion.Euler(deltaAngle, 0f, 0f), rotateSpeed, Time.deltaTime);
            }
            else if (interestTurretIndex == TurretIndices.Right)
            {
                //* leftTurret와 rightTurrent의 로컬 피봇도 Right 벡터가 Up 벡터가 됨
                var deltaAngle = Vector3.SignedAngle(Vector3.back, rightTurret.transform.InverseTransformPoint(targetPoint).AdjustX(0f), Vector3.left);
                rightTurret.transform.localRotation = __rightTurretRotationCached.LerpRefAngleSpeed(rightTurret.transform.localRotation * Quaternion.Euler(-deltaAngle, 0f, 0f), rotateSpeed, Time.deltaTime);
            }
            // else if (interestTurretIndex == TurretIndices.Center)
            // {
                   //* centerTurret은 피봇 정상
            //     var deltaAngle = Vector3.SignedAngle(Vector3.forward, centerTurret.transform.InverseTransformPoint(targetPoint).AdjustY(0f), Vector3.up);
            //     centerTurret.transform.localRotation = __centerTurretRotationCached.LerpRefAngleSpeed(centerTurret.transform.localRotation * Quaternion.Euler(0f, deltaAngle, 0f), rotateSpeed, Time.deltaTime);
            // }
            else if (interestTurretIndex == TurretIndices.Top)
            {
                //* topTurret1도 피봇 정상
                var deltaAngle1 = Vector3.SignedAngle(Vector3.forward, topTurret1.transform.InverseTransformPoint(targetPoint).AdjustY(0f), Vector3.up);
                topTurret1.transform.localRotation = __topTurret1_RotationCached.LerpRefAngleSpeed(topTurret1.transform.localRotation * Quaternion.Euler(0f, deltaAngle1, 0f), rotateSpeed, Time.deltaTime);

                //* topTurret2의 로컬 피봇은 Right 벡터가 Up 벡터가 됨
                var deltaAngle2 = Vector3.SignedAngle(Vector3.forward, topTurret2.transform.InverseTransformPoint(targetPoint).AdjustX(0f), Vector3.right);
                topTurret2.transform.localRotation = __topTurret2_RotationCached.LerpRefAngleSpeed(topTurret2.transform.localRotation * Quaternion.Euler(deltaAngle2, 0f, 0f), rotateSpeed, Time.deltaTime);
            }
        }
    }
}