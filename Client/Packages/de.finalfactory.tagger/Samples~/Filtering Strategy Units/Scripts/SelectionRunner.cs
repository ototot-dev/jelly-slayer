// // ***********************************************************************
// // Assembly         : Tagger.Plugins
// // Author           : HeiaSamahi
// // Created          : 13.05.2017 : 16:59
// //
// // Last Modified By : Heia Samahi
// // Last Modified On : 26.05.2017 : 10:40
// // ***********************************************************************
// // <copyright file="SelectionRunner.cs" company="Heikyu">
// //     Copyright (c) Heikyu. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

namespace FinalFactory.Tagger.Examples
{
    using UnityEngine;
    using UnityEngine.AI;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class SelectionRunner : MonoBehaviour
    {
        private NavMeshAgent _agent;

        private Vector3 _oldPos;

        public void Return()
        {
            _agent.destination = _oldPos;
        }

        public void SetTargetPos(Vector3 pos)
        {
            _agent.destination = pos;
        }

        public void Start()
        {
            _oldPos = transform.position;
            _agent = GetComponent<NavMeshAgent>();
        }
    }
}