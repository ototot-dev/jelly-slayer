using System.Linq;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{
    public class FormationController : MonoBehaviour
    {
        void Awake()
        {
            __formationObj = gameObject.Children().First(c => c.name == "Formation");
        }

        GameObject __formationObj;

        void Start()
        {
            Rebuild();
        }
    
        GameObject[] __formationSpots;

        public Vector3 GetFormationPosition(int index)
        {
            Debug.Assert(index >= 0 && index < __formationSpots.Length);

            return TerrainManager.GetTerrainPoint(__formationSpots[index].transform.position);
        }

        public float radius = 0.5f;
        public float padding = 0.1f;
        public float Spacing => radius * 2 + padding;
        
        public void Rebuild(float radius = 0.5f, float padding = 0.1f)
        {
            // this.radius = radius;
            // this.padding = padding;

            RebuildInternal();

            __formationSpots = __formationObj.Children().ToArray();
        }

        public void RebuildInternal()
        {
            if (__formationObj == null)
                __formationObj = gameObject.Children().First(c => c.name == "Formation");

            Debug.Assert(__formationObj != null);

            foreach (var c in __formationObj.Children().ToArray())
                DestroyImmediate(c);

            var spotObj = new GameObject($"Spot-0");

            spotObj.transform.SetParent(__formationObj.transform);
            spotObj.transform.localPosition = Vector3.zero;
            spotObj.transform.localRotation = Quaternion.identity;

            spotObj = new GameObject($"Spot-1");

            spotObj.transform.SetParent(__formationObj.transform);
            spotObj.transform.localPosition = Vector3.left * Spacing;
            spotObj.transform.localRotation = Quaternion.identity;

            spotObj = new GameObject($"Spot-2");

            spotObj.transform.SetParent(__formationObj.transform);
            spotObj.transform.localPosition = Vector3.right * Spacing;
            spotObj.transform.localRotation = Quaternion.identity;

            spotObj = new GameObject($"Spot-3");

            spotObj.transform.SetParent(__formationObj.transform);
            spotObj.transform.localPosition = Vector3.left * Spacing * 2;
            spotObj.transform.localRotation = Quaternion.identity;

            spotObj = new GameObject($"Spot-4");

            spotObj.transform.SetParent(__formationObj.transform);
            spotObj.transform.localPosition = Vector3.right * Spacing * 2;
            spotObj.transform.localRotation = Quaternion.identity;

            spotObj = new GameObject($"Spot-5");

            spotObj.transform.SetParent(__formationObj.transform);
            spotObj.transform.localPosition = Vector3.back * Spacing;
            spotObj.transform.localRotation = Quaternion.identity;

            spotObj = new GameObject($"Spot-6");

            spotObj.transform.SetParent(__formationObj.transform);
            spotObj.transform.localPosition = Vector3.back * Spacing + Vector3.left * Spacing;
            spotObj.transform.localRotation = Quaternion.identity;

            spotObj = new GameObject($"Spot-7");

            spotObj.transform.SetParent(__formationObj.transform);
            spotObj.transform.localPosition = Vector3.back * Spacing + Vector3.right * Spacing;
            spotObj.transform.localRotation = Quaternion.identity;

            spotObj = new GameObject($"Spot-8");

            spotObj.transform.SetParent(__formationObj.transform);
            spotObj.transform.localPosition = Vector3.back * Spacing + Vector3.left * Spacing * 2;
            spotObj.transform.localRotation = Quaternion.identity;

            spotObj = new GameObject($"Spot-9");

            spotObj.transform.SetParent(__formationObj.transform);
            spotObj.transform.localPosition = Vector3.back * Spacing + Vector3.right * Spacing * 2;
            spotObj.transform.localRotation = Quaternion.identity;
        }

        void OnDrawGizmosSelected()
        {
            if (__formationObj == null)
                return;

            Gizmos.color = Color.green;

            //for (int i = 0; i < __formationObj.transform.childCount; i++)
                //GizmosDrawExtension.DrawCylinder(__formationObj.transform.GetChild(i).position, radius, 1, 12);
        }
    }

}