using System;
using UniRx;
using UnityEngine;

namespace Game
{
    public class DroneBotMovement : PawnMovementEx
    {
        [Header("Movement (DroneBot)")]
        public float flyHeight = 1f;

        protected override void StartInternal()
        {
            base.StartInternal();

            //* 비행 모드 셋팅
            __ecmMovement.constrainToGround = false;

            //* 높이 유지
            __pawnBrain.onLateUpdate += () =>
            {
                __ecmMovement.SetPosition(__ecmMovement.GetPosition().AdjustY(TerrainManager.GetTerrainPoint(__ecmMovement.position).y + flyHeight));
            };
        }


    }
}