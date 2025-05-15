using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{
    public class GlobalSpawnManager : MonoSingleton<GlobalSpawnManager>
    {   
        public HashSet<PawnBrainController> spawnedBrains = new();

        public SlayerBrain SpawnSlayerPawn(GameObject sourcePrefab, Vector3 spawnPosition, Quaternion spawnRotation, bool possessImmediately = false)
        {
            var slayerBrain = Instantiate(sourcePrefab, spawnPosition, spawnRotation).GetComponent<SlayerBrain>();
            spawnedBrains.Add(slayerBrain);

            if (possessImmediately)
                GameContext.Instance.playerCtrler.Possess(slayerBrain);

            return slayerBrain;
        }

        public DroneBotBrain SpawnDroneBot(GameObject sourcePrefab, Vector3 spawnPosition, Quaternion spawnRotation)
        {
            var droneBotBrain = Instantiate(sourcePrefab, spawnPosition, spawnRotation).GetComponent<DroneBotBrain>();
            spawnedBrains.Add(droneBotBrain);

            return droneBotBrain;
        }
    }
}