using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{ 
    public class RoboDogFormationController : MonoBehaviour
    {
        [Header("Component")]
        public Transform host;
        public Transform[] spots;
        Dictionary<int, RoboDogBrain> __assignedBrains = new();

        public RoboDogBrain PickRoboDog()
        {
            return __assignedBrains.Values.First();
        }

        public bool AssignRoboDog(RoboDogBrain roboDogBrain)
        {
            for (int i = 0; i < spots.Length; i++)
            {
                if (!__assignedBrains.ContainsKey(i))
                {
                    __assignedBrains.Add(i, roboDogBrain);
                    roboDogBrain.BB.body.formationSpot.Value = spots[i];
                    return true;
                }
            } 

            Debug.Assert(false);
            return false;
        }

        public void ReleaseRoboDog(RoboDogBrain roboDogBrain)
        {
            Debug.Assert(roboDogBrain.BB.FormationSpot != null);

            for (int i = 0; i < spots.Length; i++)
            {
                if (spots[i] == roboDogBrain.BB.FormationSpot)
                {
                    __assignedBrains.Remove(i);
                    roboDogBrain.BB.body.formationSpot.Value = null;
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