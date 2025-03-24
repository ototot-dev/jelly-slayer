using System.Collections;
using UnityEngine;
using Obi;
using System;

namespace Game
{
    public class RopeHookController : MonoBehaviour
    {
        [Header("Component")]
        public ObiSolver obiSolver;
        public ObiCollider obiCollider;
        public ObiRopeSection obiRopeSection;

        [Header("Parameter")]
        [Range(0, 1)]
        public float hookResolution = 0.5f;
        public float hookExtendRetractSpeed = 2f;
        public float hookLengthMultilier = 1f;
        public float hookShootSpeed = 30f;
        public int particlePoolSize = 100;

        [Header("Rendering")]
        public Material ropeMaterial;

        [Header("Hook")]
        public float hookingLength;
        public Vector3 hookingOffsetPoint;
        public Collider hookingCollider;
        public Action<Collider> onRopeHooked;
        public Action<Collider> onRopeReleased;
        public Collider SourceCollider => obiCollider.sourceCollider;

        public Vector3 GetFirstParticlePosition()
        {
            if ( __obiRope.elements == null || __obiRope.elements.Count <= 0 || __obiRope.solver.positions.count <= 0)
                return SourceCollider.transform.position;

            // first particle in the rope is the first particle of the first element:
            int index = __obiRope.elements[0].particle1;
            return obiSolver.transform.localToWorldMatrix.MultiplyPoint(__obiRope.solver.positions[index]);
        }

        public Vector3 GetLastParticlePosition()
        {
            if ( __obiRope.elements == null || __obiRope.elements.Count <= 0)
                return SourceCollider.transform.position;
            
            // last particle in the rope is the second particle of the last element:
            int index = __obiRope.elements[__obiRope.elements.Count - 1].particle2;
            if (index >= __obiRope.solver.positions.count)
                return SourceCollider.transform.position;
            
            return obiSolver.transform.localToWorldMatrix.MultiplyPoint(__obiRope.solver.positions[index]);
        }
        
        Vector3 __hitPoint;
        ObiRope __obiRope;
        ObiRopeCursor __obiRopeCursor;
        ObiRopeBlueprint __obiRopeBlueprint;
        ObiRopeExtrudedRenderer __obiRopeRenderer;

        void Awake()
        {
            // Create both the rope and the solver:
            __obiRope = gameObject.AddComponent<ObiRope>();
            __obiRopeRenderer = gameObject.AddComponent<ObiRopeExtrudedRenderer>();
            __obiRopeRenderer.section = obiRopeSection;
            __obiRopeRenderer.uvScale = new Vector2(1, 4);
            __obiRopeRenderer.normalizeV = false;
            __obiRopeRenderer.uvAnchor = 1;
            __obiRopeRenderer.thicknessScale = 0.4f;
            __obiRopeRenderer.material = ropeMaterial;

            // Setup a blueprint for the rope:
            __obiRopeBlueprint = ScriptableObject.CreateInstance<ObiRopeBlueprint>();
            __obiRopeBlueprint.resolution = 0.5f;
            __obiRopeBlueprint.pooledParticles = particlePoolSize;

            // Tweak rope parameters:
            __obiRope.maxBending = 0.02f;

            // Add a cursor to be able to change rope length:
            __obiRopeCursor = __obiRope.gameObject.AddComponent<ObiRopeCursor>();
            __obiRopeCursor.cursorMu = 0;
            __obiRopeCursor.direction = true;
        }

        void OnDestroy()
        {
            DestroyImmediate(__obiRopeBlueprint);
        }

        /**
         * Raycast against the scene to see if we can attach the hook to something.
         */
        public bool LaunchHook(Collider targetCollider = null)
        {
            //* __obiRope.isLoaded 값이 true면 이미 hook 상태임
            Debug.Assert(!__obiRope.isLoaded);

            if (targetCollider != null)
            {   
                __hitPoint = targetCollider.transform.position;
                hookingOffsetPoint = Vector3.zero;
                hookingCollider = targetCollider;
                StartCoroutine(AttachHook());

                return true;
            }
            else if (Physics.Raycast(new Ray(transform.position, transform.forward), out var hit))
            {
                __hitPoint = hit.point;
                hookingOffsetPoint = hit.collider.transform.InverseTransformPoint(__hitPoint);
                hookingCollider = hit.collider;
                StartCoroutine(AttachHook());

                return true;
            }
            else
            {
                return false;
            }
        }

        void LayParticlesInStraightLine(Vector3 origin, Vector3 direction)
        {
            // placing all particles in a straight line, respecting rope length
            float length = 0;
            for (int i = 0; i < __obiRope.elements.Count; ++i)
            {
                int p1 = __obiRope.elements[i].particle1;
                int p2 = __obiRope.elements[i].particle2;

                obiSolver.prevPositions[p1] = obiSolver.positions[p1] = origin + direction * length;
                length += __obiRope.elements[i].restLength;
                obiSolver.prevPositions[p2] = obiSolver.positions[p2] = origin + direction * length;
            }
        }

        IEnumerator AttachHook()
        {
            yield return null;

            // Clear pin constraints:
            var pinConstraints = __obiRope.GetConstraintsByType(Oni.ConstraintType.Pin) as ObiConstraints<ObiPinConstraintsBatch>;
            pinConstraints.Clear();

            Vector3 localHit = __obiRope.transform.InverseTransformPoint(__hitPoint);

            // Procedurally generate the rope path (just a short segment, as we will extend it over time):
            int filter = ObiUtils.MakeFilter(ObiUtils.CollideWithEverything, 0);
            __obiRopeBlueprint.path.Clear();
            __obiRopeBlueprint.path.AddControlPoint(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "Hook start");
            __obiRopeBlueprint.path.AddControlPoint(localHit.normalized * 0.5f, Vector3.zero, Vector3.zero, Vector3.up, 0.1f, 0.1f, 1, filter, Color.white, "Hook end");
            __obiRopeBlueprint.path.FlushEvents();

            // Generate the particle representation of the rope (wait until it has finished):
            yield return __obiRopeBlueprint.Generate();

            // Set the blueprint (this adds particles/constraints to the solver and starts simulating them).
            __obiRope.ropeBlueprint = __obiRopeBlueprint;
            __obiRope.GetComponent<ObiRopeExtrudedRenderer>().enabled = true;

            // wait for the solver to load the rope, after the next physics step:
            yield return new WaitForFixedUpdate();
            yield return null;

            // set masses to zero, as we're going to override positions while we extend the rope:
            for (int i = 0; i < __obiRope.activeParticleCount; ++i)
                obiSolver.invMasses[__obiRope.solverIndices[i]] = 0;

            // while the last particle hasn't reached the hit, extend the rope:
            Vector3 origin;
            Vector3 destination;
            Vector3 direction;
            float distanceLeft = 0f;

            while (true)
            {
                // calculate rope origin in solver space:
                origin = obiSolver.transform.InverseTransformPoint(__obiRope.transform.position);
                destination = obiSolver.transform.InverseTransformPoint(__hitPoint);

                // update direction and distance to hook point:
                direction = destination - origin;
                float distance = direction.magnitude;
                direction.Normalize();

                LayParticlesInStraightLine(origin, direction);
                
                // increase length:
                distanceLeft = distance - __obiRopeCursor.ChangeLength(hookShootSpeed * Time.deltaTime);

                // if we have exceeded the desired length, correct it and break the loop:
                if (distanceLeft < 0)
                {
                    __obiRopeCursor.ChangeLength(distanceLeft);
                    break;
                }

                // wait for next frame:
                yield return null;
            }

            // wait for the last length change to take effect, and ensure the rope is straight:
            yield return new WaitForFixedUpdate();
            yield return null;
            LayParticlesInStraightLine(origin, direction);

            // restore masses so that the simulation takes over now that the rope is in place:
            for (int i = 0; i < __obiRope.activeParticleCount; ++i)
                obiSolver.invMasses[__obiRope.solverIndices[i]] = 10; // 1/0.1 = 10

            // Pin both ends of the rope (this enables two-way interaction between character and rope):
            var batch = new ObiPinConstraintsBatch();
            batch.AddConstraint(__obiRope.elements[0].particle1, obiCollider, transform.localPosition, Quaternion.identity, 0, 0, float.PositiveInfinity);
            batch.AddConstraint(__obiRope.elements[__obiRope.elements.Count - 1].particle2, hookingCollider.GetComponent<ObiColliderBase>(),  hookingOffsetPoint, Quaternion.identity, 0, 0, float.PositiveInfinity);
            batch.activeConstraintCount = 2;
            pinConstraints.AddBatch(batch);

            __obiRope.SetConstraintsDirty(Oni.ConstraintType.Pin);

            //* 기본 길이값 셋팅
            hookingLength = (hookingCollider.transform.position - transform.position).magnitude * hookLengthMultilier;
            onRopeHooked?.Invoke(hookingCollider);
        }

        public void DetachHook()
        {
            var hookColliderCached = hookingCollider;

            hookingLength = -1f;
            hookingCollider = null;

            // Set the rope blueprint to null (automatically removes the previous blueprint from the solver, if any).
            __obiRope.ropeBlueprint = null;
            __obiRope.GetComponent<ObiRopeExtrudedRenderer>().enabled = false;

            onRopeReleased?.Invoke(hookColliderCached);
        }

        float __needLength;

        void FixedUpdate()
        {
            if (__obiRope.isLoaded && hookingLength > 0f)
            {
                var currLength = __obiRope.CalculateLength();
                var restLength = __obiRope.restLength;

                hookingLength = (hookingCollider.transform.position - transform.position).magnitude * hookLengthMultilier;
                // __Logger.LogR(gameObject, nameof(FixedUpdate), "hookLength", hookLength, "currLength", currLength, "restLength", restLength);

                // restLength - Mathf.Floor(distance);
                if (Mathf.Abs(hookingLength - __obiRope.restLength) > 0.1f)
                {
                    if (restLength < hookingLength)
                        __obiRopeCursor.ChangeLength(hookExtendRetractSpeed * Time.deltaTime);
                    else
                        __obiRopeCursor.ChangeLength(-hookExtendRetractSpeed * Time.deltaTime);
                }


                // if (Input.GetKey(KeyCode.Z))
                //     __obiRopeCursor.ChangeLength(-hookExtendRetractSpeed * Time.deltaTime);
                // else if (Input.GetKey(KeyCode.X))
                //     __obiRopeCursor.ChangeLength(hookExtendRetractSpeed * Time.deltaTime);
            }
        }
    }
}