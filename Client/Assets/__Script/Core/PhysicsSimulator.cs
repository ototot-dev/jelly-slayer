using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    public class PhysicsSimulator : MonoSingleton<GameContext>
    {
        public void Init()
        {
            Physics.autoSimulation = true;
        }
    }
}