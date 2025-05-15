#if UNITY_LOCALIZATION

using System;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace FinalFactory.Localization
{
    public static class LocalizationTools
    {
        public static void LocalizationReady(Action onReady)
        {
            AsyncOperationHandle handle = LocalizationSettings.InitializationOperation;
            
            if (handle.IsDone)
            {
                onReady();
            }
            else
            {
                handle.Completed += OnReady;
            }

            return;

            void OnReady(AsyncOperationHandle obj)
            {
                handle.Completed -= OnReady;
                onReady();
            }
        }
    }
}

#endif