using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Linq;
using System.Linq;

namespace Game
{
    public class JellySpringMassSystem : MonoBehaviour
    {
        [Header("Config")]
        public int gridNum = 4;
        public float k = 10;
        public float damping = 1f;
        public float dragging = 2f;
        public Vector3 gravity = new(0, -9.8f, 0);
        public AnimationCurve coreDraggingCurve;
        public AnimationCurve boundsRadiusCurve;
        public Transform coreAttachPoint;

        [Header("Network")]
        public NetPoint[] points;
        public NetConnection[] connections;

        [Header("Component")]
        public Transform body;
        public Transform root;
        public Transform bounds;
        public Transform boundsCenter;
        public BoxCollider boxBounds;
        public SphereCollider sphereBounds;
        public Transform core;
        public Rigidbody coreRigidBody;
        public BoxCollider coreBox;
        public SphereCollider coreSphere;
        public CapsuleCollider coreCapsule;

        public Vector3 BoundSize => boxBounds != null ? boxBounds.size : sphereBounds.radius * Vector3.one;
        public float CoreRadius => coreSphere != null ? coreSphere.radius : coreCapsule.radius;

        [Serializable]
        public class NetPoint
        {
            public int index;
            public float mass;
            public float boundsRadius;
            public Vector3 velocity;
            public Vector3 position;
            public Transform linkedBone;
            public Transform linkedGrid;

            public NetPoint(int index, float mass, Transform linkedBone, Transform linkedGrid)
            {
                this.index = index;
                this.mass = mass;
                position = linkedBone.position = linkedGrid.position;   //* 최초 위치는 linkedGrid 위치로 초기화함
                this.linkedBone = linkedBone;
                this.linkedGrid = linkedGrid;
            }

            public void OnFixedUpdate(JellySpringMassSystem system)
            {
                velocity += Time.fixedDeltaTime * system.gravity;
                position += Time.fixedDeltaTime * velocity;

                //* bounds 스케일은 변경될 수 있음으로 이를 보정해줘야함
                var scaledBoundsRadius = boundsRadius * Mathf.Min(system.bounds.transform.localScale.x, system.bounds.transform.localScale.y, system.bounds.transform.localScale.z);

                //* 메시 Deforming 결과가 너무 늘어나지 않도록, linkedGrid 위치에서 boundsRadius 안쪽 영역으로 position값을 수정함
                if (scaledBoundsRadius > 0 && (linkedGrid.position - position).sqrMagnitude > scaledBoundsRadius * scaledBoundsRadius)
                    position = linkedGrid.position + scaledBoundsRadius * (position - linkedGrid.position).normalized;

                //* boundsCenter 에서 position 까지 Raycast 검사를 해서 linkedBone 위치값을 수정함
                var rayVec = position - system.boundsCenter.position;
                if (Physics.Raycast(system.boundsCenter.position, rayVec.normalized, out var hit, rayVec.magnitude, LayerMask.GetMask("Terrain")))
                {
                    // __Logger.LogR(system.gameObject, "NetPoint collision detected", "hit.point", hit.point);
                    linkedBone.position = 0.5f * (position + hit.point);
                }
                else
                    linkedBone.position = position;
            }
        }

        [Serializable]
        public class NetConnection
        {
            public NetPoint pointA;
            public NetPoint pointB;
            public float currNetLen;
            public float restNetLen;

            public NetConnection(NetPoint pointA, NetPoint pointB)
            {
                this.pointA = pointA;
                this.pointB = pointB;
                currNetLen = restNetLen = (pointA.position - pointB.position).magnitude;
            }

            public void OnFixedUpdate(JellySpringMassSystem system)
            {
                var deltaVec = pointA.position - pointB.position;
                currNetLen = deltaVec.magnitude;

                var coreDraggingA = Mathf.Max(1f, system.coreDraggingCurve.Evaluate(Mathf.Clamp01((system.core.position - pointA.linkedGrid.position).magnitude / system.BoundSize.x * 2f)));
                var coreDraggingB = Mathf.Max(1f, system.coreDraggingCurve.Evaluate(Mathf.Clamp01((system.core.position - pointB.linkedGrid.position).magnitude / system.BoundSize.x * 2f)));
                var accelVecA = -system.k / coreDraggingA * (currNetLen - restNetLen) * deltaVec.normalized - system.damping * pointA.velocity + system.dragging * coreDraggingA * (pointA.linkedGrid.position - pointA.position);
                var accelVecB = system.k / coreDraggingB * (currNetLen - restNetLen) * deltaVec.normalized - system.damping * pointB.velocity + system.dragging * coreDraggingB * (pointB.linkedGrid.position - pointB.position);

                pointA.velocity += Time.fixedDeltaTime * accelVecA;
                pointB.velocity += Time.fixedDeltaTime * accelVecB;
            }
        }

        void Awake()
        {
            __jellyMeshBuilder = GetComponent<JellyMeshBuilder>();
        }

        JellyMeshBuilder __jellyMeshBuilder;

        void Start()
        {
            var tempPoints = new List<NetPoint>();
            for (int i = 0; i < root.childCount; i++)
            {
                var point = new NetPoint(i, 1, root.GetChild(i), boundsCenter.GetChild(i));
                if (boundsRadiusCurve != null)
                    point.boundsRadius = boundsRadiusCurve.Evaluate((boundsCenter.position - point.linkedGrid.position).magnitude);

                tempPoints.Add(point);
            }

            points = tempPoints.ToArray();

            var tempConns = new List<NetConnection>();
            for (int z = 0; z < gridNum; z++)
            {
                for (int y = 0; y < gridNum; y++)
                {
                    for (int x = 0; x < gridNum; x++)
                    {
                        var connA = MakeConnection(ToIndex(x, y, z), ToIndex(x + 1, y, z));
                        var connB = MakeConnection(ToIndex(x, y, z), ToIndex(x, y + 1, z));
                        var connC = MakeConnection(ToIndex(x, y, z), ToIndex(x, y, z + 1));
                        var connX = MakeConnection(ToIndex(x, y, z), ToIndex(x + 1, y + 1, z + 1));
                        var connY = MakeConnection(ToIndex(x, y, z), ToIndex(x + 1, y - 1, z + 1));
                        var connW = MakeConnection(ToIndex(x, y, z), ToIndex(x - 1, y + 1, z + 1));
                        var connZ = MakeConnection(ToIndex(x, y, z), ToIndex(x - 1, y - 1, z + 1));

                        if (connA != null) tempConns.Add(connA);
                        if (connB != null) tempConns.Add(connB);
                        if (connC != null) tempConns.Add(connC);
                        if (connX != null) tempConns.Add(connX);
                        if (connY != null) tempConns.Add(connY);
                        if (connZ != null) tempConns.Add(connZ);
                        if (connW != null) tempConns.Add(connW);
                    }
                }
            }

            connections = tempConns.ToArray();
        }

        public void Build()
        {
            //* 기존 NetPoint 제거함
            foreach (var c in root.gameObject.Children().Skip(1).ToArray())
                DestroyImmediate(c);
            foreach (var c in boundsCenter.gameObject.Children().Skip(1).ToArray())
                DestroyImmediate(c);

            var stepSize = BoundSize / gridNum;
            for (int index = 0, z = 0; z < gridNum; z++)
            {
                for (int y = 0; y < gridNum; y++)
                {
                    for (int x = 0; x < gridNum; x++)
                    {
                        var position = -0.5f * BoundSize + 0.5f * stepSize + new Vector3(stepSize.x * x, stepSize.y * y, stepSize.x * z) + transform.position;
                        if (index == 0)
                        {
                            boundsCenter.GetChild(0).position = root.GetChild(0).position = position;
                            boundsCenter.GetChild(0).name = $"Grid(0,0,0)-0";
                            root.GetChild(0).name = $"bPoint(0,0,0)-0";
                        }
                        else
                        {
                            var grid = Instantiate(boundsCenter.GetChild(0).gameObject, boundsCenter).transform;
                            var point = Instantiate(root.GetChild(0).gameObject, root).transform;
                            grid.position = point.position = position;
                            grid.name = $"Grid({x},{y},{z})-{index}";
                            point.name = $"bPoint({x},{y},{z})-{index}";
                        }

                        index++;
                    }
                }
            }
        }
        
        public NetConnection MakeConnection(int indexA, int indexB)
        {
            if (indexA >= 0 && indexB >= 0)
                return new NetConnection(points[indexA], points[indexB]);
            else
                return null;
        }
            
        public int ToIndex(int x, int y, int z)
        { 
            if (x < 0 || x >= gridNum) return -1;
            if (y < 0 || y >= gridNum) return -1;
            if (z < 0 || z >= gridNum) return -1;
            return gridNum * gridNum * z + gridNum * y + x;
        }

        public ValueTuple<int, int, int> FromIndex(int index)
        {
            var x = index % gridNum;
            var z = index / (gridNum * gridNum);
            var y = (index - gridNum * gridNum * z - x) / gridNum;
            return (x, y, z);
        }

        void FixedUpdate()
        {
            if ((coreRigidBody == null || coreRigidBody.isKinematic) && coreAttachPoint != null)
                core.SetPositionAndRotation(coreAttachPoint.position, coreAttachPoint.rotation);
                
            bounds.SetPositionAndRotation(core.position, core.rotation);
            body.SetPositionAndRotation(bounds.position, bounds.rotation);

            foreach (var c in connections)
                c.OnFixedUpdate(this);
            foreach (var p in points)
                p.OnFixedUpdate(this);
        }

        public void SetScale(float x, float y, float z)
        {
            bounds.localScale = new Vector3(x, y, z);
        }

        public void AddImpulse(Vector3 impluse)
        {
            foreach (var p in points)
                p.velocity += impluse;
        }

        public void AddImpulseRandom(float impluse)
        {
            foreach (var p in points)
                p.velocity += impluse * Vector3.one.Random();
        }

        [Header("Gizmos")]
        public bool drawGizmosEnabled = false;

        void OnDrawGizmos()
        {
            if (drawGizmosEnabled)
            {
                Gizmos.color = Color.magenta; foreach (var c in connections) Gizmos.DrawLine(c.pointA.position, c.pointB.position);
                Gizmos.color = Color.yellow; foreach (var p in points) Gizmos.DrawCube(p.position, Vector3.one * 0.1f);
            }
        }
    }
}