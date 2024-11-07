using System;
using System.Collections.Generic;
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
    public class RockStamp : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public Vector2 size;

        /// <summary>
        /// 
        /// </summary>
        public int rockSpawnCount = 1;

        /// <summary>
        /// 
        /// </summary>
        public GameObject[] rockPrefabs;

        /// <summary>
        /// 
        /// </summary>
        public int shrubSpawnCount = 1;

        /// <summary>
        /// 
        /// </summary>
        public GameObject shrubPrefab;

        /// <summary>
        /// 
        /// </summary>
        public float flowerDensity = 1;

        /// <summary>
        /// 
        /// </summary>
        public float flowerSuffleStrength;

        /// <summary>
        /// 
        /// </summary>
        public GameObject[] flowerPrefabs;

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
                EffectManager.Instance.Show("FX_ShardRock_Dust_01", positions[0], Quaternion.identity, Vector3.one, -1, 0.5f);
                EffectManager.Instance.Show("FX_ShardRock_Dust_End_01", positions[1], Quaternion.identity, 2f * Vector3.one, -1, 0.9f);
                EffectManager.Instance.Show("FX_ShardRock_Dust_01", positions[2], Quaternion.identity, Vector3.one, -1, 1.4f);
                EffectManager.Instance.Show("FX_ShardRock_Dust_End_01", positions[3], Quaternion.identity, 2f * Vector3.one, -1, 1.9f);
                EffectManager.Instance.Show("FX_ShardRock_Dust_01", positions[4], Quaternion.identity, Vector3.one, -1, 2.3f);
                EffectManager.Instance.Show("FX_ShardRock_Dust_End_01", positions[5], Quaternion.identity, 2f * Vector3.one, -1, 2.7f);
                EffectManager.Instance.Show("FX_ShardRock_Dust_01", positions[6], Quaternion.identity, Vector3.one, -1, 3.4f);
                EffectManager.Instance.Show("FX_ShardRock_Dust_End_01", positions[7], Quaternion.identity, 2f * Vector3.one, -1, 3.9f);

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
        public void Generate()
        {
            var hit = TerrainManager.GetTerrainHitPoint(transform.position);

            if (hit.collider == null)
                return;

            transform.position = hit.point;

            var remainSpawnCount = rockSpawnCount;
            var failureLoopCount = 0;

            while (remainSpawnCount > 0 && failureLoopCount < 10)
            {
                if (SpawnRock() != null)
                    remainSpawnCount--;
                else
                    failureLoopCount++;
            }

            remainSpawnCount = shrubSpawnCount;
            failureLoopCount = 0;

            var spawnedShrubs = new List<GameObject>();

            while (remainSpawnCount > 0 && failureLoopCount < 10)
            {
                var shrub = SpawnShrub();

                if (shrub != null)
                {
                    spawnedShrubs.Add(shrub);
                    remainSpawnCount--;
                }
                else
                {
                    failureLoopCount++;
                }
            }

            // Spawn 위치 Validate 용으로 추가한 BoxCollider는 삭제함
            foreach (var s in spawnedShrubs)
                DestroyImmediate(s.GetComponent<BoxCollider>());

            SpawnFlower();
        }

        /// <summary>
        /// 스폰 위치에 이미 스폰된 물체가 있는지 체크 (위치가 비어있으면 true 값 리턴)
        /// </summary>
        /// <param name="position"></param>
        /// <param name="extents"></param>
        /// <param name="layerNames"></param>
        /// <returns></returns>
        bool ValidateSpawnPosition(Vector3 position, Vector3 extents, params string[] layerNames)
        {
            RaycastHit hit = new RaycastHit();

            if (Physics.SphereCast(position + Vector3.up * 99, Mathf.Max(extents.x, extents.z), Vector3.down, out hit, 199, LayerMask.GetMask(layerNames)))
            {
                Debug.Log($"2?? Physics.SphereCast() => hit.collider.gameObject: {hit.collider.gameObject.name}");
                return false;
            }
            else
            {
                return true;
            }
        }

        GameObject SpawnRock()
        {
            var randX = UnityEngine.Random.Range(-size.x * 0.5f, size.x * 0.5f);
            var randZ = UnityEngine.Random.Range(-size.y * 0.5f, size.y * 0.5f);

            var spawnPoint = transform.position + new Vector3(randX, 0, randZ);
            var spawnPrefab = rockPrefabs[UnityEngine.Random.Range(0, rockPrefabs.Length)];
            // var spawnExtents = Vector3.one * spawnPrefab.GetComponent<SphereCollider>().radius;
            var spawnExtents = Vector3.one * spawnPrefab.GetComponent<Collider>().bounds.extents.x;

            var hit = TerrainManager.GetTerrainHitPoint(spawnPoint);

            if (hit.collider == null || !ValidateSpawnPosition(spawnPoint, spawnExtents, "Obstacle"))
                return null;

            var forward = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0) * Vector3.forward;
            var rock = Instantiate<GameObject>(rockPrefabs[UnityEngine.Random.Range(0, rockPrefabs.Length)], hit.point + Vector3.down * 0.2f, Quaternion.LookRotation(forward, hit.normal));

            foreach (var c in rock.Children().Where(c => c.GetComponent<MeshRenderer>() != null))
                c.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            rock.transform.SetParent(transform, true);

            return rock;
        }

        GameObject SpawnShrub()
        {
            var randX = UnityEngine.Random.Range(-size.x * 0.5f, size.x * 0.5f);
            var randZ = UnityEngine.Random.Range(-size.y * 0.5f, size.y * 0.5f);

            var spawnPoint = transform.position + new Vector3(randX, 0, randZ);
            var spawnExtents = shrubPrefab.Children().Select(c => c.GetComponent<MeshRenderer>()).First().bounds.extents;

            var hit = TerrainManager.GetTerrainHitPoint(spawnPoint);

            if (hit.collider == null || !ValidateSpawnPosition(spawnPoint, spawnExtents, "Default", "Obstacle"))
                return null;
                
            var forward = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0) * Vector3.forward;
            var shrub = Instantiate<GameObject>(shrubPrefab, hit.point, Quaternion.LookRotation(forward, hit.normal));

            foreach (var c in shrub.Children().Where(c => c.GetComponent<MeshRenderer>() != null))
                c.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            shrub.transform.SetParent(transform, true);
            shrub.AddComponent<BoxCollider>().size = spawnExtents * 2f;

            return shrub;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="terrainManager"></param>
        void SpawnFlower()
        {
            var extents = flowerPrefabs[0].GetComponent<MeshRenderer>().bounds.extents;

            var spawnNum = size.x * size.y / extents.x * extents.z * flowerDensity;
            var spawnNumX = Mathf.Sqrt(spawnNum * size.x / size.y);
            var spawnNumZ = spawnNumX * size.y / size.x;

            var stepX = size.x / spawnNumX;
            var stepZ = size.x / spawnNumZ;

            for (float x = -size.x * 0.5f; x <= size.x * 0.5f;)
            {
                for (float z = -size.y * 0.5f; z <= size.y * 0.5f;)
                {
                    var randX = UnityEngine.Random.Range(-stepX * flowerSuffleStrength, stepX * flowerSuffleStrength);
                    var randZ = UnityEngine.Random.Range(-stepZ * flowerSuffleStrength, stepZ * flowerSuffleStrength);
                    var spawnPoint = transform.position + Quaternion.Euler(0, 45, 0) * (Vector3.right * (x + randX) + Vector3.forward * (z + randZ));

                    var hit = TerrainManager.GetTerrainHitPoint(spawnPoint);

                    if (hit.collider != null)
                    {
                        var prefab = flowerPrefabs[UnityEngine.Random.Range(0, flowerPrefabs.Length)];
                        var forward = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0) * Vector3.forward;
                        var flower = Instantiate<GameObject>(prefab, hit.point + Vector3.up * 0.2f, Quaternion.LookRotation(forward, hit.normal));

                        flower.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                        flower.transform.SetParent(transform, true);
                    }

                    z += stepZ;
                }

                x += stepX;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Reset()
        {
            foreach (var c in gameObject.Children().ToArray())
            {
                if (c.GetComponent<TweenName>() == null)
                    DestroyImmediate(c);
            }
        }
    }
}