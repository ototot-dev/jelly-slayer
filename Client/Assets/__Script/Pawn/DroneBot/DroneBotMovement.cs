using System;
using DG.Tweening;
using ParadoxNotion.Animation;
using UniRx;
using UnityEngine;

namespace Game
{
    public class DroneBotMovement : PawnMovementEx
    {
        //* IsHanging 상태인 경우엔 HeroMovement 쪽에서 이동 제어를 하기 위해 '__ecmMovement'를 외부로 노출시킴
        public ECM2.CharacterMovement GetCharacterMovement() => __ecmMovement;
        public bool CheckCatchingDurationExpired() => Time.time - __catchingStartTimeStamp > __brain.BB.CatchingDuration;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<DroneBotBrain>();
        }

        DroneBotBrain __brain;
        float __moveAccelCached;
        float __moveBrakeCached;
        float __catchingStartTimeStamp;
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
                    __catchingStartTimeStamp = Time.time;
                    __catchingStartPosition = capsule.transform.position;
                    __catchingStartRotation = capsule.transform.rotation;
                    __brain.AnimCtrler.tweenSelector.query.activeClasses.Clear();
                    __brain.AnimCtrler.tweenSelector.query.Apply();
                }
                else if (v == DroneBotBrain.Decisions.Hanging)
                {
                    __brain.AnimCtrler.tweenSelector.query.activeClasses.Add("shake");
                    __brain.AnimCtrler.tweenSelector.query.Apply();
                }
            }).AddTo(this);
        }

        protected override void OnFixedUpdateHandler()
        {
            if (__brain.BB.CurrDecision != DroneBotBrain.Decisions.Catch && __brain.BB.CurrDecision != DroneBotBrain.Decisions.Hanging)
                base.OnFixedUpdateHandler();
        }

        public float testHeight = 2f;

        protected override void OnUpdateHandler()
        {
            if (__brain.BB.CurrDecision == DroneBotBrain.Decisions.Catch)
            {
                var catchingElapsedTime = Time.time - __catchingStartTimeStamp;
                var offsetVec = capsule.transform.position - __brain.AnimCtrler.hangingPoint.position;
                var alpha = Easing.Ease(EaseType.CubicOut, 0f, 1f, catchingElapsedTime / __brain.BB.CatchingDuration);
                var lerpPosition = Vector3.Lerp(__catchingStartPosition, GameContext.Instance.playerCtrler.heroBrain.GetWorldPosition() + offsetVec, alpha);
                var lerpRotation = Quaternion.Lerp(__catchingStartRotation, GameContext.Instance.playerCtrler.heroBrain.GetWorldRotation(), alpha);

                //* Catch 위치로 강제 이동함
                __ecmMovement.SetPositionAndRotation(lerpPosition, lerpRotation);
            }
            else if (__brain.BB.CurrDecision == DroneBotBrain.Decisions.Hanging)
            {
                var hangerMovement = GameContext.Instance.playerCtrler.heroBrain.Movement;

                //* hangerMovement이 값을 주입해서 이동함
                __ecmMovement.SimpleMove(hangerMovement.moveSpeed * hangerMovement.moveVec, hangerMovement.moveSpeed, hangerMovement.moveAccel, hangerMovement.moveBrake, 1f, 1f, Vector3.zero, false, Time.deltaTime);
                __ecmMovement.RotateTowards(hangerMovement.faceVec, hangerMovement.rotateSpeed);

                //* 고도 유지
                var newHeight = capsule.transform.position.y.LerpSpeed(TerrainManager.GetTerrainPoint(capsule.transform.position).y + __brain.BB.body.flyHeight, __brain.BB.body.flyHeightAdjustSpeed, Time.deltaTime);
                __ecmMovement.SetPosition(capsule.transform.position.AdjustY(newHeight));
            }
            else
            {
                if (__brain.BB.CurrDecision == DroneBotBrain.Decisions.Spacing)
                {
                    if (GameContext.Instance.playerCtrler.heroBrain != null)
                    {
                        //* 회전 방향 갱신
                        if (GameContext.Instance.playerCtrler.heroBrain.BB.TargetBrain != null)
                            faceVec = (GameContext.Instance.playerCtrler.heroBrain.BB.TargetBrain.GetWorldPosition() - GameContext.Instance.playerCtrler.heroBrain.GetWorldPosition()).Vector2D().normalized;
                        else
                            faceVec = GameContext.Instance.playerCtrler.heroBrain.coreColliderHelper.transform.forward.Vector2D().normalized;
                    }
                }
                else
                {
                    var speedAlpha = Mathf.Clamp01(((GameContext.Instance.playerCtrler.heroBrain.GetWorldPosition() - capsule.transform.position).Vector2D().magnitude - __brain.BB.action.minSpacingDistance) / __brain.BB.action.maxSpacingDistance);
                    moveSpeed = Mathf.Lerp(__brain.BB.body.normalSpeed, __brain.BB.body.boostSpeed, speedAlpha);
                    moveAccel = __moveAccelCached;
                    moveBrake = __moveBrakeCached;
                }

                //* 고도 유지
                var newHeight = capsule.transform.position.y.LerpSpeed(TerrainManager.GetTerrainPoint(capsule.transform.position).y + __brain.BB.body.flyHeight, __brain.BB.body.flyHeightAdjustSpeed, Time.deltaTime);
                __ecmMovement.SetPosition(capsule.transform.position.AdjustY(newHeight));

                base.OnUpdateHandler();
            }
        }
    }
}