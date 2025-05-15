// // ***********************************************************************
// // Assembly         : Tagger.Plugins
// // Author           : HeiaSamahi
// // Created          : 21.05.2017 : 12:52
// //
// // Last Modified By : Heia Samahi
// // Last Modified On : 26.05.2017 : 10:40
// // ***********************************************************************
// // <copyright file="PerformanceExampleGUI.cs" company="Heikyu">
// //     Copyright (c) Heikyu. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using Finalfactory.Tagger;

namespace FinalFactory.Tagger.Examples.PerformanceTest
{
    using System.Collections.Generic;
    using System.Diagnostics;

    using UnityEngine;

    public class PerformanceExampleGUI : MonoBehaviour
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnassignedField.Global
        public PrefabSpawner PrefabSpawner;

        private TaggerAdvancedSearch _advancedSearch;

        private int _count = 1000;

        private float _elapsedTimeAddRandomTags = -1;

        private float _elapsedTimeCreateGameObject = -1;

        private float _elapsedTimeGetMatchedGameObjects = -1;

        private float _elapsedTimeResearch = -1;

        private HashSet<GameObject> _matchedGameObjects;

        public void OnGUI()
        {
            _matchedGameObjects ??= new HashSet<GameObject>();
            GUILayout.Label("This Performance example should better not run in the editor. The difference is sometimes 2-5x");
            GUILayout.Label("Preparing: Select the test batch size. (#1) Then create gameobjects. (#2) Now add random tags to the gameobjects.");
            GUILayout.Label(
                "Test #1: Execute the search. This will find the matching gameobjects. You can search as often you want. It may make things only faster due to JIT.");
            GUILayout.Label("Test #2: Get all matched gameobjects as HashSet collection");
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            if (GUILayout.Button("Test 100 Tagger GameObject"))
            {
                _count = 100;
                Reset();
            }

            if (GUILayout.Button("Test 1000 Tagger GameObject"))
            {
                _count = 1000;
                Reset();
            }

            if (GUILayout.Button("Test 10000 Tagger GameObject"))
            {
                _count = 10000;
                Reset();
            }

            if (GUILayout.Button("Test 100000 Tagger GameObject"))
            {
                _count = 100000;
                Reset();
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label("Tagger Performance Tests");
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical();
            GUILayout.Label("Test preparation");
            if (_elapsedTimeCreateGameObject < 0f)
            {
                if (GUILayout.Button("#1 Spawn " + _count + " Prefab with tagger scripts and register tagger GameObjects"))
                {
                    PrefabSpawner.Count = _count;
                    PrefabSpawner.Prepare();
                    _elapsedTimeCreateGameObject = PrefabSpawner.Spawn();
                }
            }
            else
            {
                GUILayout.Label(
                    _count + " Prefabs with Tagger Script created and registred in " + ToTime(_elapsedTimeCreateGameObject, _count));

                if (_elapsedTimeAddRandomTags < 0f)
                {
                    if (GUILayout.Button("#2 Add random tags to all GameObjects"))
                    {
                        _elapsedTimeAddRandomTags = PrefabSpawner.Colorize(true);
                    }
                }
                else
                {
                    GUILayout.Label(
                        _count + " - " + _count * 4 + " Tags added to GameObjects. " + ToTime(_elapsedTimeAddRandomTags, _count));
                }
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label("Tests");

            if (_elapsedTimeAddRandomTags > 0f)
            {
                if (GUILayout.Button("Test #1 Reserach"))
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    _advancedSearch.Research();
                    sw.Stop();
                    _elapsedTimeResearch = sw.ElapsedTicks / 1000f;
                }

                GUILayout.Label("Research in " + ToTime(_elapsedTimeResearch));
                if (GUILayout.Button("Test #2 Get Matched GameObjects"))
                {
                    Stopwatch sw = new Stopwatch();
                    sw.Start();
                    _advancedSearch.MatchedGameObjects(_matchedGameObjects);
                    sw.Stop();
                    _elapsedTimeGetMatchedGameObjects = sw.ElapsedTicks / 1000f;
                }

                GUILayout.Label("Matched GameObjects in " + ToTime(_elapsedTimeGetMatchedGameObjects));
                if (_matchedGameObjects != null)
                {
                    GUILayout.Label("Matched GameObjects: " + _matchedGameObjects.Count);
                }
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            GUILayout.Label("Test cleanup");

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void Reset()
        {
            _elapsedTimeCreateGameObject = -1f;
            _elapsedTimeAddRandomTags = -1f;
            _elapsedTimeGetMatchedGameObjects = -1f;
            _elapsedTimeResearch = -1f;
            _matchedGameObjects = null;
        }

        private static string ToTime(float elapsed, int count)
        {
            return ToTime(elapsed) + " ("+ (elapsed / count).ToString("#0.0###") + "ms per object)";
        }

        private static string ToTime(float elapsed)
        {
            return elapsed.ToString("F") + " ms";
        }

        public void Start()
        {
            _advancedSearch = new TaggerAdvancedSearch();
            _advancedSearch.SetTags("Green", "Red");
            TaggerAdvancedSearch taggerAdvancedSearch = new TaggerAdvancedSearch(true, "Red", "Blue");
            TaggerAdvancedSearch taggerAdvancedSearch3 = new TaggerAdvancedSearch(true, "Cube", "Green");
            taggerAdvancedSearch.Or(taggerAdvancedSearch3);
            _advancedSearch.Or(taggerAdvancedSearch);
        }
    }
}