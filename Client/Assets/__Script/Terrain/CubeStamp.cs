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
    public class CubeStamp : MonoBehaviour
    {
        /// <summary>
        /// 
        /// </summary>
        public Vector2 size;

        /// <summary>
        /// 
        /// </summary>
        public int cubeSpawnCount = 1;

        /// <summary>
        /// 
        /// </summary>
        public GameObject[] cubePrefabs;

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
        public void Generate(TerrainManager terrainManager = null)
        {
            if (terrainManager == null)
                terrainManager = GameObject.FindWithTag("TerrainManager").GetComponent<TerrainManager>();

            var hit = TerrainManager.GetTerrainHitPoint(transform.position);

            if (hit.collider == null)
                return;

            transform.position = hit.point;

            var remainSpawnCount = cubeSpawnCount;
            var failureLoopCount = 0;

            while (remainSpawnCount > 0 && failureLoopCount < 10)
            {
                if (SpawnCube(terrainManager) != null)
                    remainSpawnCount--;
                else
                    failureLoopCount++;
            }
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

        GameObject SpawnCube(TerrainManager terrainManager)
        {
            var randX = UnityEngine.Random.Range(-size.x * 0.5f, size.x * 0.5f);
            var randZ = UnityEngine.Random.Range(-size.y * 0.5f, size.y * 0.5f);

            var spawnPoint = transform.position + new Vector3(randX, 0, randZ);
            var spawnPrefab = cubePrefabs[UnityEngine.Random.Range(0, cubePrefabs.Length)];
            var spawnExtents = Vector3.one * spawnPrefab.GetComponent<Collider>().bounds.extents.x;

            var hit = TerrainManager.GetTerrainHitPoint(spawnPoint);

            if (hit.collider == null || !ValidateSpawnPosition(spawnPoint, spawnExtents, "Obstacle"))
                return null;

            var forward = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0) * Vector3.forward;
            var rock = Instantiate<GameObject>(cubePrefabs[UnityEngine.Random.Range(0, cubePrefabs.Length)], hit.point + Vector3.down * 0.2f, Quaternion.LookRotation(forward, hit.normal));

            foreach (var c in rock.Children().Where(c => c.GetComponent<MeshRenderer>() != null))
                c.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            rock.transform.SetParent(transform, true);

            return rock;
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