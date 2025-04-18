﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class GameContext : MonoSingleton<GameContext>
    {
        public PlayerController playerCtrler;
        public CameraController cameraCtrler;
        public TerrainManager terrainManager;
        public SlimeSpawnManager jellySpawnManager;
        public HeroSpawnManager heroSpawnManager;
        public DamageTextManager damageTextManager;

        public RectTransform MainCanvas
        {
            get
            {
                __mainCanvas ??= GameObject.FindWithTag("MainCanvas").GetComponent<RectTransform>();
                return __mainCanvas;
            }
        }

        public CanvasManager canvasManager
        {
            get
            {
                __mainCanvasCtrler ??= MainCanvas.GetComponent<CanvasManager>();
                return __mainCanvasCtrler;
            }
        }

        RectTransform __mainCanvas;
        CanvasManager __mainCanvasCtrler;
        
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
