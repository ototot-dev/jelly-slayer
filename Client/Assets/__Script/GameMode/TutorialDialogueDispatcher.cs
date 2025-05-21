using Finalfactory.Tagger;
using UnityEngine;

namespace Game
{
    public class TutorialDialogueDispatcher : DialogueDispatcher
    {
        protected override void AwakeInternal()
        {
            base.AwakeInternal();

            __runner.AddCommandHandler("spawnDrontBot", SpawnDroneBot);
            __runner.AddCommandHandler<float>("ringPhone", RingPhone);
        }

        public void SpawnDroneBot()
        {
            var spawnPosition = TaggerSystem.FindGameObjectWithTag("DroneBotSpawnPoint").transform.position;
            GameContext.Instance.playerCtrler.SpawnDroneBot(spawnPosition, Quaternion.identity);
        }

        public void RingPhone(float duration)
        {
            var found = TaggerSystem.FindGameObjectWithTag("Phone");
            __Logger.LogR1(gameObject, "RingPhone", "found", found);
        }
        
    }
}