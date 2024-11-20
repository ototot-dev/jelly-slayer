using System.Collections.Generic;

namespace Game
{
    public class NpcSpawnManager : Singleton<NpcSpawnManager>
    {
        public HashSet<PawnBrainController> spawnedBrains = new();
        public void Init() {}
    }
}