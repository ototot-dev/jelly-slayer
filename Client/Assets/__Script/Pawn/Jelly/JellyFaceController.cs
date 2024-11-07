using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class JellyFaceController : MonoBehaviour
    {
        public Transform face;
        public PawnBrainController brain;
        public JellySpringMassSystem springMassSystem;

        void Start()
        {
            brain.onFixedUpdate += OnFixedUpdateHandler;
        }

        void OnFixedUpdateHandler()
        {
            face.SetPositionAndRotation(springMassSystem.bounds.position, springMassSystem.core.rotation);
        }
    }
}
