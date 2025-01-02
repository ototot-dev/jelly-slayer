using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GameContext : MonoSingleton<GameContext>
    {
        public string TerrainLayerName => "Terrain";
        public Camera MainCamera => cameraCtrler != null ? cameraCtrler.viewCamera : null;
        public CursorController CursorCtrler => playerCtrler != null ? playerCtrler.cursorCtrler : null;
        public HeroBrain HeroBrain => playerCtrler != null ? playerCtrler.heroBrain : null;
        public DroneBotFormationController droneBotFormationCtrler;
        public PlayerController playerCtrler;
        public PlayerTargetManager playerTargetManager;
        public CameraController cameraCtrler;
        public TerrainManager terrainManager;
        public SlimeSpawnManager jellySpawnManager;
        public HeroSpawnManager heroSpawnManager;
        public MainCanvasController mainCanvasCtrler;
        
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
