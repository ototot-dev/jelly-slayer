using UnityEngine;
using GoogleSheet.Core.Type;

namespace Game
{
    [UGS(typeof(MissionClearCondition))]
    public enum MissionClearCondition
    {
        NONE,
        KILLALL,
    }

    [UGS(typeof(MissionType))]
    public enum MissionType
    {
        NONE,
        TUTORIAL,
        BASECAMP,
        MISSION,
    }
}