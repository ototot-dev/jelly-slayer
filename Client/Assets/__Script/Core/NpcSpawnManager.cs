using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class NpcSpawnManager : MonoSingleton<NpcSpawnManager>
    {
        public HashSet<PawnBrainController> spawnedBrains = new();

        public void Init()
        {
            return;
            
            foreach (var b in GameObject.FindGameObjectsWithTag("Jelly").Select(go => go.GetComponent<PawnBrainController>()))
            {
                spawnedBrains.Add(b);
                __Logger.LogR(gameObject, "spawnedBrains.Add()", "brain", b.name);
            }
        }
    }

}