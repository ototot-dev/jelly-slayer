using UnityEngine;

namespace Game
{ 
    public class DroneFormationController : MonoBehaviour
    {
        [Header("Component")]
        public Transform host;
        public Transform[] spots;

        void LateUpdate()
        {
            transform.SetPositionAndRotation(host.transform.position, host.transform.rotation);
        }
    }
}