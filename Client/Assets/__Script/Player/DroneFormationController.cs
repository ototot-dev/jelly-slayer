using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{ 
    public class DroneBotFormationController : MonoBehaviour
    {
        [Header("Component")]
        public Transform host;
        public Transform[] spots;
        Dictionary<int, DroneBotBrain> __assignedDroneBotBrains = new();

        public DroneBotBrain PickDroneBot()
        {
            return __assignedDroneBotBrains.Values.First();
        }

        public bool AssignDroneBot(DroneBotBrain droneBotBrain)
        {
            for (int i = 0; i < spots.Length; i++)
            {
                if (!__assignedDroneBotBrains.ContainsKey(i))
                {
                    __assignedDroneBotBrains.Add(i, droneBotBrain);
                    droneBotBrain.BB.body.formationSpot.Value = spots[i];
                    return true;
                }
            } 

            Debug.Assert(false);
            return false;
        }

        public void ReleaseDroneBot(DroneBotBrain droneBotBrain)
        {
            Debug.Assert(droneBotBrain.BB.FormationSpot != null);

            for (int i = 0; i < spots.Length; i++)
            {
                if (spots[i] == droneBotBrain.BB.FormationSpot)
                {
                    __assignedDroneBotBrains.Remove(i);
                    droneBotBrain.BB.body.formationSpot.Value = null;
                    return;
                }
            }

            Debug.Assert(false);
        }

        void LateUpdate()
        {
            transform.SetPositionAndRotation(host.transform.position, host.transform.rotation);
        }
    }
}