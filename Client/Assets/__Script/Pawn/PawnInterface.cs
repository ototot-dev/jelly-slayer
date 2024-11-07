using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public interface ISpawnable
    {
        Vector3 GetSpawnPosition();
        void OnStartSpawnHandler();
        void OnFinishSpawnHandler();
        void OnDespawnedHandler();
        void OnDeadHandler();
        void OnLifeTimeOutHandler();
    }

    public interface IMovable
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

    public interface IBuffContainer
    {
        Dictionary<BuffTypes, Tuple<float, float>> GetBuffTable();
        bool AddBuff(BuffTypes buff, float strength, float duration);
        void RemoveBuff(BuffTypes buff);
        bool CheckBuff(BuffTypes buff);
        float GetBuffStrength(BuffTypes buff);
        
#if UNITY_EDITOR
        Dictionary<BuffTypes, Tuple<float, float>>.Enumerator GetBuffEnumerator();
#endif
    }
}