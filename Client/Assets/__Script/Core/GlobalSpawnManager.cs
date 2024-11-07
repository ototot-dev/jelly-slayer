using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    public class GlobalSpawnManager : MonoSingleton<GlobalSpawnManager>
    {   
        /// <summary>
        /// 
        /// </summary>
        public int maxGrassNumPerZone = 8;

        /// <summary>
        /// 
        /// </summary>
        public int maxRockNumPerZone = 4;

        /// <summary>
        /// 
        /// </summary>
        public int maxNpcNumPerZone = 16;

        /// <summary>
        /// 
        /// </summary>
        public int maxSlimeNumPerZone = 4;

        /// <summary>
        /// 
        /// </summary>
        public int maxSlimeJuniorNumPerZone = 16;

        /// <summary>
        /// 
        /// </summary>
        public int maxSlimeJuniorFollowerNum = 4;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="givenHeartPoint"></param>
        /// <returns></returns>
        public GameObject[] SelectSpawningPawn(int givenHeartPoint)
        {
            // if (givenHeartPoint <= 10)
            //     return Resources.Load<GameObject>("Pawn/Slime");
            // else
                // return Resources.Load<GameObject>("Pawn/BoximonFiery");π
                
            return new GameObject[] {
                Resources.Load<GameObject>("Pawn/MinerMale"),
                Resources.Load<GameObject>("Pawn/MinerFemale"),
            };

            // return new GameObject[] { 
            //     Resources.Load<GameObject>("Pawn/Footman"),
            //     Resources.Load<GameObject>("Pawn/Footman"),
            //     Resources.Load<GameObject>("Pawn/Footman"),
            //     Resources.Load<GameObject>("Pawn/Footman"),
            //     Resources.Load<GameObject>("Pawn/Footman"),
            // };
        }

        /// <summary>
        /// 
        /// </summary>
        public void Init()
        {   
            // Observable.Timer(TimeSpan.FromSeconds(1.1f)).Subscribe(_ => StartCoroutine(StartSpawnSlime())).AddTo(this);
            // Observable.Timer(TimeSpan.FromSeconds(2.2f)).Subscribe(_ => StartCoroutine(StartSpawnTerrainStamp())).AddTo(this);
            // Observable.Timer(TimeSpan.FromSeconds(3.3f)).Subscribe(_ => StartCoroutine(StartSpawnHollow())).AddTo(this);

            // Observable
        }

        /// <summary>
        /// 
        /// </summary>
        IEnumerator StartSpawnTerrainStamp()
        {
            for (int i = 0; i < maxGrassNumPerZone / 2; i++)
            {
                foreach (var c in GameContext.Instance.terrainManager.gameObject.Children())
                    SpawnGrassStamp(c.GetComponent<TerrainMeshGenerator>().GetRandomPoint(0.9f, 0.9f), true);

                yield return new WaitForSeconds(1);
            }

            for (int i = 0; i < maxRockNumPerZone / 2; i++)
            {
                foreach (var c in GameContext.Instance.terrainManager.gameObject.Children())
                {
                    var spawnPosition = c.GetComponent<TerrainMeshGenerator>().GetRandomPoint(0.9f, 0.9f);
                    var rockSpawnNum = UnityEngine.Random.Range(1, 4);

                    SpawnRockStamp(spawnPosition, rockSpawnNum, true);
                }

                yield return new WaitForSeconds(1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void SpawnGrassStamp(Vector3 spawnPosition, bool showStamp = true)
        {
            var stamp = Instantiate(Resources.Load<GameObject>("Terrain/GrassStamp"), spawnPosition, Quaternion.identity).GetComponent<VegetationStamp>();

            Observable.NextFrame().Subscribe(_ =>
            {
                stamp.Generate(GameContext.Instance.terrainManager);

                if (showStamp)
                    stamp.Show();
            }).AddTo(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spawnPosition"></param>
        /// <param name="rockSpawnNum"></param>
        /// <param name="showStamp"></param>
        public void SpawnRockStamp(Vector3 spawnPosition, int rockSpawnNum, bool showStamp = true)
        {
            var stamp = Instantiate(Resources.Load<GameObject>("Terrain/RockStamp"), spawnPosition, Quaternion.identity).GetComponent<RockStamp>();

            stamp.size = Vector2.one * Mathf.Min(5, rockSpawnNum + 2);
            stamp.rockSpawnCount = rockSpawnNum;

            Observable.NextFrame().Subscribe(_ =>
            {
                stamp.Generate();

                if (showStamp)
                    stamp.Show();
            }).AddTo(this);
        }

    }
}