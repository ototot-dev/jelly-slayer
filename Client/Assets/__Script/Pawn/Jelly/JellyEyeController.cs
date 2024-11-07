using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class JellyEyeController : MonoBehaviour
    {

        void Awake()
        {
            __springMassSystem = GetComponent<JellySpringMassSystem>();
        }

        JellySpringMassSystem __springMassSystem;

        void FixedUpdate()
        {
            var center = Vector3.zero;

            for (int i = 0; i < __springMassSystem.points.Length; i++)
                center += __springMassSystem.points[i].position;

            center /= __springMassSystem.points.Length;
        }
        
    }
}