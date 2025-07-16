using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

namespace Game
{
    public class GameContext : MonoSingleton<GameContext>
    {
        public Launcher launcher;
        public PlayerController playerCtrler;
        public CameraController cameraCtrler;
        public DialogueRunner dialogueRunner;
        public CanvasManager canvasManager;
        public TerrainManager terrainManager;
        public SlimeSpawnManager jellySpawnManager;
        public HeroSpawnManager heroSpawnManager;
        public DamageTextManager damageTextManager;
        public HPBarManager hpBarManager;
        public HashSet<InteractionKeyController> interactionKeyCtrlers = new();
    }
}
