using System;
using System.Linq;
using Retween.Rx;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    public class VegetationStamp : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public Vector2 size;

        /// <summary>
        /// 
        /// </summary>
        public float scale = 1;

        /// <summary>
        /// 
        /// </summary>
        public float density = 1;

        /// <summary>
        /// 
        /// </summary>
        public GameObject mainPrefab;

        /// <summary>
        /// 
        /// </summary>
        public float mainSpawnWeight = 1;

        /// <summary>
        /// 
        /// </summary>
        public GameObject extraPrefab;

        /// <summary>
        /// 
        /// </summary>
        public float extraSpawnWeight = 1;

        /// <summary>
        /// 
        /// </summary>
        public void Show()
        {
            foreach (var c in gameObject.Children())
            {
                if (c.GetComponent<TweenName>() == null)
                    c.SetActive(false);
            }

            var positions = new Vector3[] 
            {
                transform.GetChild(UnityEngine.Random.Range(0, transform.childCount)).transform.position,
                transform.GetChild(UnityEngine.Random.Range(0, transform.childCount)).transform.position,
                transform.GetChild(UnityEngine.Random.Range(0, transform.childCount)).transform.position,
                transform.GetChild(UnityEngine.Random.Range(0, transform.childCount)).transform.position,
                transform.GetChild(UnityEngine.Random.Range(0, transform.childCount)).transform.position,
                transform.GetChild(UnityEngine.Random.Range(0, transform.childCount)).transform.position,
                transform.GetChild(UnityEngine.Random.Range(0, transform.childCount)).transform.position,
                transform.GetChild(UnityEngine.Random.Range(0, transform.childCount)).transform.position
            };

            Observable.NextFrame().Subscribe(_ => 
            {   
                // EffectManager.Instance.Show("FX_ShardRock_Dust_01", positions[0], Quaternion.identity, Vector3.one, -1, 0.5f);
                // EffectManager.Instance.Show("FX_ShardRock_Dust_End_01", positions[1], Quaternion.identity, 2f * Vector3.one, -1, 0.9f);
                // EffectManager.Instance.Show("FX_ShardRock_Dust_01", positions[2], Quaternion.identity, Vector3.one, -1, 1.4f);
                // EffectManager.Instance.Show("FX_ShardRock_Dust_End_01", positions[3], Quaternion.identity, 2f * Vector3.one, -1, 1.9f);
                // EffectManager.Instance.Show("FX_ShardRock_Dust_01", positions[4], Quaternion.identity, Vector3.one, -1, 2.3f);
                // EffectManager.Instance.Show("FX_ShardRock_Dust_End_01", positions[5], Quaternion.identity, 2f * Vector3.one, -1, 2.7f);
                // EffectManager.Instance.Show("FX_ShardRock_Dust_01", positions[6], Quaternion.identity, Vector3.one, -1, 3.4f);
                // EffectManager.Instance.Show("FX_ShardRock_Dust_End_01", positions[7], Quaternion.identity, 2f * Vector3.one, -1, 3.9f);

                Observable.NextFrame().Subscribe(_ => {
                    GetComponent<TweenSelector>().query.Add(".spawn", true, true);

                    foreach (var c in gameObject.Children())
                    {
                        if (c.GetComponent<TweenName>() == null)
                            c.SetActive(true);
                    }
                }).AddTo(this);
            }).AddTo(this);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Generate(TerrainManager terrainManager = null)
        {
            var hit = TerrainManager.GetTerrainHitPoint(transform.position);

            if (hit.collider == null)
                return;

            transform.position = hit.point;

            var extents = mainPrefab.Children().Select(c => c.GetComponent<MeshRenderer>()).First().bounds.extents;

            var spawnNum = size.x * size.y / extents.x * extents.z * density;
            var spawnNumX = Mathf.Sqrt(spawnNum * size.x / size.y);
            var spawnNumZ = spawnNumX * size.y / size.x;

            var stepX = size.x / spawnNumX;
            var stepZ = size.x / spawnNumZ;

            for (float x = -size.x * 0.5f; x <= size.x * 0.5f;)
            {
                for (float z = -size.y * 0.5f; z <= size.y * 0.5f;)
                {
                    SpawnGrass(x, z, terrainManager);
                    z += stepZ;
                }

                x += stepX;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="terrainManager"></param>
        /// <returns></returns>
        GameObject SpawnGrass(float x, float z, TerrainManager terrainManager)
        {
            var spawnPoint = transform.position + Quaternion.Euler(0, 45, 0) * (Vector3.right * x + Vector3.forward * z);

            var hit = TerrainManager.GetTerrainHitPoint(spawnPoint);

            if (hit.collider == null)
                return null;

            var prefab = (int)(mainSpawnWeight / (mainSpawnWeight + extraSpawnWeight) * 100) > UnityEngine.Random.Range(0, 100) ? mainPrefab : extraPrefab;
            var forward = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0) * Vector3.forward;
            var grass = Instantiate<GameObject>(prefab, hit.point, Quaternion.LookRotation(forward, hit.normal));

            foreach (var c in grass.Children())
            {
                c.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                c.transform.localScale = Vector3.one * scale;
            }

            grass.transform.SetParent(transform, true);

            return grass;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            foreach (var c in gameObject.Children().ToArray())
            {
                if (c.GetComponent<TweenName>() != null)
                    continue;

                DestroyImmediate(c);
            }
        }
    }
}