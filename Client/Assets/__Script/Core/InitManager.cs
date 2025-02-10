using UnityEngine;

namespace Game
{
    public class InitManager
    {
        private static bool _isInit = false;

        public static void Initialize()
        {
            if (_isInit == true)
                return;

            DatasheetManager.Instance.Load();
            SoundManager.Instance.Init();
            NpcSpawnManager.Instance.Init();

            GameManager.Instance.CheckInstance();

            _isInit = true;
        }
    }
}