using System.Collections.Generic;
using System.Linq;
using UGS;

namespace Game
{
    public class DatasheetManager : Singleton<DatasheetManager>
    {
        readonly Dictionary<PawnId, Dictionary<string, MainTable.ActionData>> __actionDataMap = new();
        public bool IsLoaded { get; private set; }

        public void Load()
        {
            if (IsLoaded)
                return;

            UnityGoogleSheet.LoadAllData();
            
            foreach (var d in MainTable.ActionData.ActionDataList)
            {
                if (!__actionDataMap.ContainsKey(d.pawnId))
                    __actionDataMap.Add(d.pawnId, new());
                if (!__actionDataMap[d.pawnId].TryAdd(d.actionName, d))
                    UnityEngine.Debug.LogWarning("err~~");
            }

            IsLoaded = true;
        }

        public MainTable.PlayerData GetPlayerData()
        {
            return MainTable.PlayerData.GetList().First();
        }

        public MainTable.ActionData GetActionData(PawnId pawnId, string actionName)
        {
            if (__actionDataMap.TryGetValue(pawnId, out var temp) && temp.TryGetValue(actionName, out var ret))
                return ret;
            else
                return null;
        }

        public MainTable.ActionData[] GetActionData(PawnId pawnId)
        {
            if (__actionDataMap.TryGetValue(pawnId, out var ret))
                return ret.Values.ToArray();
            else
                return null;
        }
    }
}