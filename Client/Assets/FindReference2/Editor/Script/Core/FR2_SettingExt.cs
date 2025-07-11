using System;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace vietlabs.fr2
{
    [Serializable] internal class FR2_SettingExt
    {
        
        public static FR2_AutoRefreshMode autoRefreshMode
        {
            get => inst._autoRefresh;
            set
            {
                if (inst._autoRefresh == value) return;
                inst._autoRefresh = value;
                
                EditorUtility.SetDirty(FR2_Cache.Api);
                EditorApplication.update -= DelaySave;
                EditorApplication.update += DelaySave;
            }
        }

        public static bool isAutoRefreshEnabled
        {
            get
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode) return false;
                return autoRefreshMode == FR2_AutoRefreshMode.On;
            }
        }
        
        public static bool disable
        {
            get => inst.internalDisabled;
            set => inst.internalDisabled = value;
        }
        
        private const string path = "Library/FR2/fr2.cfg";
        private static FR2_SettingExt inst;
        
        static FR2_SettingExt()
        {
            
            inst = new FR2_SettingExt();
            if (!File.Exists(path)) return;

            try
            {
                string content = File.ReadAllText(path);
                JsonUtility.FromJsonOverwrite(content, inst);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                //nothing
            }
        }

        static void DelaySave()
        {
            EditorApplication.update -= DelaySave;
            
            try
            {
                Directory.CreateDirectory("Library/FR2/");
                File.WriteAllText(path, JsonUtility.ToJson(inst));
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                //nothing
            }
        }
        
        [SerializeField] private bool _disableInPlayMode = true;
        [SerializeField] private bool _disabled;
        [SerializeField] private FR2_AutoRefreshMode _autoRefresh;
        
        private bool internalDisabled
        {
            get => _disabled || (_disableInPlayMode && EditorApplication.isPlayingOrWillChangePlaymode);
            set
            {
                ref bool disableRef = ref _disabled;
                if (EditorApplication.isPlayingOrWillChangePlaymode) disableRef = ref _disableInPlayMode;
                
                if (disableRef == value) return;
                disableRef = value;
                
                // disable at runtime: only disable `disableInPlayMode`
                // enable at runtime: enable all
                if (!value) _disabled = false;
                EditorUtility.SetDirty(FR2_Cache.Api);
                EditorApplication.update -= DelaySave;
                EditorApplication.update += DelaySave;
            }
        }
    }
}
