using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace Game
{
    public class JellyCrowdController : MonoBehaviour
    {    
        [Header("Config")]
        public float spacingDistance = 1f;
        public AnimationCurve spacingPushingCurve;
        public AnimationCurve condenseDraggingCurve;
        public AnimationCurve connectionDraggingCurve;

        [Header("Component")]
        public CapsuleCollider capsuleCollider;

        [Serializable]
        public class CrowdPoint
        {
            public int index = -1;
            public Vector3 spacingForce;
            public Vector3 condenseForce;
            public Vector3 connectionForce;
            public JellySpringMassSystem linkedSpringMass;
        }

        [Serializable]
        public class CrowdConnection
        {
            public int indexA = -1;
            public int indexB = -1;
            public CrowdPoint pointA;
            public CrowdPoint pointB;

            public void OnFixedUpdate(JellyCrowdController crowdCtrler)
            {
                if (indexA > indexB)
                {
                    var distanceVec = pointA.linkedSpringMass.core.position - pointB.linkedSpringMass.core.position;
                    pointA.connectionForce = crowdCtrler.connectionDraggingCurve.Evaluate(distanceVec.magnitude) * distanceVec.normalized;
                }
                else
                {
                    var distanceVec = pointB.linkedSpringMass.core.position - pointA.linkedSpringMass.core.position;
                    pointB.connectionForce = crowdCtrler.connectionDraggingCurve.Evaluate(distanceVec.magnitude) * distanceVec.normalized;
                }
            }
        }

        public List<CrowdPoint> points = new();
        public List<CrowdConnection> connections = new();

        void Start()
        {
            // foreach (var p in points)
            // {
            //     Debug.Assert(p.index >= 0);
            //     p.linkedSpringMass.coreRigidBody.position = p.draggingPoint.position;
            // }

            foreach (var c in connections)
            {
                Debug.Assert(c.indexA >= 0);
                Debug.Assert(c.indexB >= 0);
                Debug.Assert(c.indexA != c.indexB);

                c.pointA = points.ElementAt(c.indexA);
                c.pointB = points.ElementAt(c.indexB);
                // c.restDistance = (c.pointA.linkedSpringMass.core.position - c.pointB.linkedSpringMass.core.position).magnitude;
                // c.restDistance = 1f;
            }
        }

        void FixedUpdate()
        {   
            foreach (var p in points)
                p.spacingForce = p.condenseForce = p.condenseForce = Vector3.zero;
        }
    }
}