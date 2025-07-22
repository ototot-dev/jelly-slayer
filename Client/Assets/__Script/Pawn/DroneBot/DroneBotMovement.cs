using ParadoxNotion.Animation;
using UniRx;
using UnityEngine;

namespace Game
{
    public class DroneBotMovement : PawnMovementEx
    {
        public bool CheckPrepareHangingDone() => Time.time - __catchStartTimeStamp > prepareHangingDuration;

        //* Hanging 상태에서 HostBrain의 위치값을 기준으로 자신의 현재 위치를 계산하기 위한 Offset 값
        //* (HostBrain.GetWorldPosition() + GetHangingPointOffsetVector() => DroneBot의 현재 위치)
        public Vector3 GetHangingPointOffsetVector() => __brain.Movement.capsule.transform.position - __brain.AnimCtrler.hangingAttachPoint.position;

        [Header("Parameter")]
        public float prepareHangingDuration;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<DroneBotBrain>();
        }

        DroneBotBrain __brain;
        float __moveAccelCached;
        float __moveBrakeCached;
        float __catchStartTimeStamp;
        Vector3 __catchingStartPosition;
        Quaternion __catchingStartRotation;

        protected override void StartInternal()
        {
            base.StartInternal();

            __moveAccelCached = moveAccel;
            __moveBrakeCached = moveBrake;

            //* 비행 모드 셋팅
            __ecmMovement.constrainToGround = false;

            //* Catch 시작 시간 기록
            __brain.BB.decision.currDecision.Subscribe(v =>
            {
                if (v == DroneBotBrain.Decisions.Catch)
                {
                    __catchStartTimeStamp = Time.time;
                    __catchingStartPosition = capsule.transform.position;
                    __catchingStartRotation = capsule.transform.rotation;
                    // __brain.AnimCtrler.tweenSelector.query.activeClasses.Clear();
                    // __brain.AnimCtrler.tweenSelector.query.Apply();
                }
                // else if (v == DroneBotBrain.Decisions.Hanging)
                // {
                //     __brain.AnimCtrler.tweenSelector.query.activeClasses.Add("shake");
                //     __brain.AnimCtrler.tweenSelector.query.Apply();
                // }
            }).AddTo(this);
        }

        protected override void OnFixedUpdateHandler()
        {
            //* Catch 상태에선 HostBrain의 이동을 따라가는 Attach 모드로 동작함
            if (__brain.BB.CurrDecision == DroneBotBrain.Decisions.Catch)
                return;

            base.OnFixedUpdateHandler();
        }

        public float testHeight = 2f;

        protected override void OnUpdateHandler()
        {
            if (__brain.BB.CurrDecision == DroneBotBrain.Decisions.Catch)
            {
                if (__brain.BB.IsHanging)
                {
                    var hostMovement = GameContext.Instance.playerCtrler.possessedBrain.Movement;

                    //* hangerMovement이 값을 주입해서 이동함
                    __ecmMovement.SimpleMove(hostMovement.moveSpeed * hostMovement.moveVec, hostMovement.moveSpeed, hostMovement.moveAccel, hostMovement.moveBrake, 1f, 1f, Vector3.zero, false, Time.deltaTime);
                    __ecmMovement.RotateTowards(hostMovement.faceVec, hostMovement.rotateSpeed);

                    //* 고도 유지
                    var newHeight = capsule.transform.position.y.LerpSpeed(TerrainManager.GetTerrainPoint(capsule.transform.position).y + __brain.BB.body.flyHeight, __brain.BB.body.flyHeightAdjustSpeed, Time.deltaTime);
                    __ecmMovement.SetPosition(capsule.transform.position.AdjustY(newHeight));
                }
                else
                {
                    var catchingElapsedTime = Time.time - __catchStartTimeStamp;
                    var alpha = Easing.Ease(EaseType.CubicInOut, 0f, 1f, catchingElapsedTime / prepareHangingDuration);
                    var lerpPosition = Vector3.Lerp(__catchingStartPosition, __brain.BB.HostBrain.GetWorldPosition() + GetHangingPointOffsetVector(), alpha);
                    var lerpRotation = Quaternion.Lerp(__catchingStartRotation, __brain.BB.HostBrain.GetWorldRotation(), alpha);

                    //* Catch 위치로 강제 이동함
                    __ecmMovement.SetPositionAndRotation(lerpPosition, lerpRotation);
                }
            }
            else
            {
                if (__brain.BB.CurrDecision == DroneBotBrain.Decisions.Spacing || __brain.BB.CurrDecision == DroneBotBrain.Decisions.Approach || __brain.BB.CurrDecision == DroneBotBrain.Decisions.Heal)
                {
                    if (__brain.BB.CurrDecision == DroneBotBrain.Decisions.Heal)
                    {
                        moveSpeed = __brain.BB.body.boostSpeed;
                    }
                    else
                    {
                        var speedAlpha = Mathf.Clamp01(((__brain.BB.HostBrain.GetWorldPosition() - capsule.transform.position).Vector2D().magnitude - __brain.BB.MinSpacingDistance) / __brain.BB.MaxSpacingDistance);
                        moveSpeed = Mathf.Lerp(__brain.BB.body.normalSpeed, __brain.BB.body.boostSpeed, speedAlpha);
                    }
                    
                    moveAccel = __moveAccelCached;
                    moveBrake = __moveBrakeCached;

                    //* 회전 방향 갱신
                    if (__brain.BB.HostBrain.BB.TargetBrain != null)
                        faceVec = (__brain.BB.HostBrain.BB.TargetBrain.GetWorldPosition() - __brain.GetWorldPosition()).Vector2D().normalized;
                    else if (__brain.BB.CurrDecision == DroneBotBrain.Decisions.Approach || __brain.BB.CurrDecision == DroneBotBrain.Decisions.Heal)
                        faceVec = (__brain.BB.HostBrain.GetWorldPosition() - __brain.GetWorldPosition()).Vector2D().normalized;
                    else
                        faceVec = __brain.BB.HostBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                }

                //* 고도 유지                
                var newPositionY = __brain.BB.CurrDecision == DroneBotBrain.Decisions.Heal ?
                    capsule.transform.position.y.LerpSpeed(TerrainManager.GetTerrainPoint(capsule.transform.position).y + __brain.BB.action.healFlyHeight, __brain.BB.action.healFlyHeightAdjustSpeed, Time.deltaTime) :
                    capsule.transform.position.y.LerpSpeed(TerrainManager.GetTerrainPoint(capsule.transform.position).y + __brain.BB.body.flyHeight, __brain.BB.body.flyHeightAdjustSpeed, Time.deltaTime);
                    
                __ecmMovement.SetPosition(capsule.transform.position.AdjustY(newPositionY));

                base.OnUpdateHandler();
            }
        }
    }
}