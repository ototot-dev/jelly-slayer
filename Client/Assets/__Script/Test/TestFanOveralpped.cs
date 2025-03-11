using UnityEngine;

namespace Game
{    
    [ExecuteInEditMode]
    public class TestFanOverlapped : MonoBehaviour
    {
        public float fanRadius = 1f;
        public float fanAngle = 90f;
        public float fanHeight = 1f;

        public Material hitMaterial;
        public Material noHitMaterial;

        public BoxCollider testBox;
        public SphereCollider testSphere;
        public CapsuleCollider testCapsule;

        // Update is called once per frame
        void Update()
        {
            if (testBox != null)
            {
                // if (testBox.CheckOverlappedWithFan(fanAngle, fanRadius, fanHeight, transform.worldToLocalMatrix))
                if (testBox.CheckOverlappedWithBox(Vector3.one, transform.localToWorldMatrix))
                    testBox.GetComponent<MeshRenderer>().sharedMaterial = hitMaterial;
                else
                    testBox.GetComponent<MeshRenderer>().sharedMaterial = noHitMaterial;
            }

            if (testSphere != null)
            {
                if (testSphere.CheckOverlappedWithFan(fanAngle, fanRadius, fanHeight, transform.worldToLocalMatrix))
                    testSphere.GetComponent<MeshRenderer>().sharedMaterial = hitMaterial;
                else
                    testSphere.GetComponent<MeshRenderer>().sharedMaterial = noHitMaterial;
            }

            if (testCapsule != null)
            {
                if (testCapsule.CheckOverlappedWithFan(fanAngle, fanRadius, fanHeight, transform.worldToLocalMatrix))
                    testCapsule.GetComponent<MeshRenderer>().sharedMaterial = hitMaterial;
                else
                    testCapsule.GetComponent<MeshRenderer>().sharedMaterial = noHitMaterial;
            }
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            GizmosDrawExtension.DrawFanCylinder(transform.localToWorldMatrix, fanRadius, fanAngle, fanHeight, 12);
            GizmosDrawExtension.DrawBox(transform.position, transform.rotation, 0.5f * Vector3.one);
        }
    }
}