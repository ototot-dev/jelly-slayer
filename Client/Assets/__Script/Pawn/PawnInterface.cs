using System;
using System.Collections.Generic;
using UnityEngine;

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
        void AddRootMotion(Vector3 deltaPosition, Quaternion deltaRotation);
        void Teleport(Vector3 destination);
        void MoveTo(Vector3 destination);
        void FaceTo(Vector3 direction);
        void Stop();
    }

    public interface IPawnDecision
    {
        void OnDecisionFinishedHandler() {}
        void InvalidateDecision(float decisionCoolTime = 0) {}
    }

    public interface IStatusContainer
    {
        Dictionary<PawnStatus, Tuple<float, float>> GetStatusTable();
        bool AddBuff(PawnStatus status, float strength, float duration);
        void RemoveBuff(PawnStatus status);
        bool CheckBuff(PawnStatus status);
        float GetBuffStrength(PawnStatus status);
        
#if UNITY_EDITOR
        Dictionary<PawnStatus, Tuple<float, float>>.Enumerator GetStatusEnumerator();
#endif
    }
}