using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game
{
    public interface IPawnSpawnable
    {
        Vector3 GetSpawnPosition();
        void OnStartSpawnHandler();
        void OnFinishSpawnHandler();
        void OnDespawnedHandler();
        void OnDeadHandler();
        void OnLifeTimeOutHandler();
    }

    public interface IPawnMovable
    {
        bool IsJumping();
        bool IsRolling();
        bool IsOnGround();
        Vector3 GetDestination();
        bool CheckReachToDestination();
        float GetEstimateTimeToDestination();
        float GetDefaultMinApproachDistance();
        bool GetFreezeMovement();
        bool GetFreezeRotation();
        void ReserveDestination(Vector3 destination);
        float SetDestination(Vector3 destination);
        void SetMinApproachDistance(float distance);
        void SetFaceVector(Vector3 faceVec);
        void FreezeMovement(bool newValue);
        void FreezeRotation(bool newValue);
        void AddRootMotion(Vector3 deltaPosition, Quaternion deltaRotation, float deltaTime);
        void StartJump(float jumpHeight);
        void FinishJump();
        void Teleport(Vector3 destination);
        void MoveTo(Vector3 destination);
        void FaceTo(Vector3 direction);
        void Stop();
    }

    public interface IPawnTargetable
    {
        PawnColliderHelper StartTargeting();
        PawnColliderHelper NextTarget();
        PawnColliderHelper CurrTarget();
        void StopTargeting();
    }

    public interface IStatusContainer
    {
        Dictionary<PawnStatus, Tuple<float, float>> GetStatusTable();
        bool AddStatus(PawnStatus status, float strength, float duration);
        void RemoveStatus(PawnStatus status);
        bool CheckStatus(PawnStatus status);
        float GetStatusStrength(PawnStatus status);
        
#if UNITY_EDITOR
        Dictionary<PawnStatus, Tuple<float, float>>.Enumerator GetStatusEnumerator();
#endif
    }

    public interface IPlayerActionListener
    {
        void OnPlayerActionEvent(string eventName);
        void OnPlayerActionStatus(PawnStatus status, float strength, float duration);
        void OnPlayerActionDamage(PawnHeartPointDispatcher.DamageContext damageContext);
    }
}