using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GameContext : MonoSingleton<GameContext>
    {
        public string TerrainLayerName => "Terrain";
        public Camera MainCamera => mainCameraCtrler != null ? mainCameraCtrler.viewCamera : null;
        public HeroBrain HeroBrain => playerCtrler != null ? playerCtrler.possessedBrain : null;
        public DroneBotFormationController droneBotFormationCtrler;
        public PlayerController playerCtrler;
        public TargetingController playerTargetManager;
        public CameraController mainCameraCtrler;
        public TerrainManager terrainManager;
        public SlimeSpawnManager jellySpawnManager;
        public HeroSpawnManager heroSpawnManager;
        public CanvasController mainCanvasCtrler;
        
        void Awake()
        {
            // terrainManager = GameObject.FindWithTag("TerrainManager").GetComponent<TerrainManager>();

            // if (!GameContext.Instance.terrainManager.IsTerrainGenerated)
            //     GameContext.Instance.terrainManager.Generate();

            // playerCtrler = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
            // cameraCtrler = GameObject.FindWithTag("MainCamera").GetComponent<CameraController>();
            // mainCanvas = GameObject.FindWithTag("MainCanvas").GetComponent<Canvas>();

            // slimeSpawnManager = gameObject.Children().First(c => c.name == "Manager").GetComponent<SlimeSpawnManager>();
            // heroSpawnManager = gameObject.Children().First(c => c.name == "Manager").GetComponent<HeroSpawnManager>();
        }

    }

}
