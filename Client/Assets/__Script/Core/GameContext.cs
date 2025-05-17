using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

namespace Game
{
    public class GameContext : MonoSingleton<GameContext>
    {
        public PlayerController playerCtrler;
        public CameraController cameraCtrler;
        public DialogueRunnerDispatcher dialogueRunnerDispatcher;
        public TerrainManager terrainManager;
        public SlimeSpawnManager jellySpawnManager;
        public HeroSpawnManager heroSpawnManager;
        public DamageTextManager damageTextManager;
        public HPBarManager hpBarManager;
        public PawnBrainController _mainTarget = null; // Boss, Target

        public CanvasManager CanvasManager
        {
            get
            {
                __canvasManager ??= FindFirstObjectByType<CanvasManager>();
                return __canvasManager;
            }
        }

        CanvasManager __canvasManager;

        public DialogueRunner dialogueRunner;

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
        public void SetMainTarget(PawnBrainController brain) 
        { 
            _mainTarget = brain;
        }
    }
}
