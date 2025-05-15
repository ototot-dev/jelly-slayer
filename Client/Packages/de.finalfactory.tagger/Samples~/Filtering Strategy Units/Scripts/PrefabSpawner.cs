// // ***********************************************************************
// // Assembly         : Tagger.Plugins
// // Author           : HeiaSamahi
// // Created          : 13.05.2017 : 14:07
// //
// // Last Modified By : Heia Samahi
// // Last Modified On : 26.05.2017 : 10:40
// // ***********************************************************************
// // <copyright file="PrefabSpawner.cs" company="Heikyu">
// //     Copyright (c) Heikyu. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

namespace FinalFactory.Tagger.Examples
{
    using System.Collections.Generic;
    using System.Diagnostics;

    using UnityEngine;

    // ReSharper disable once ClassNeverInstantiated.Global
    public class PrefabSpawner : MonoBehaviour
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public bool AutoStart = true;

        public int Count = 100;

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once FieldCanBeMadeReadOnly.Global
        // ReSharper disable once CollectionNeverUpdated.Global
        public List<GameObject> Prefabs = new List<GameObject>();

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnassignedField.Global
        public Transform Target;

        private readonly Color[] _colors = { Color.red, Color.blue, Color.green };

        private readonly string[] _colorTags = { "Red", "Blue", "Green" };

        private readonly float[] _scale = { 0.5f, 0.8f, 1f };

        private readonly string[] _scaleTags = { "Small", "Normal", "Large" };

        private GameObject[] _gameObjects;

        private bool _randomPos;

        private Bounds _spawnBounds;

        public float Colorize(bool performanceTest = false)
        {
            Stopwatch sw = new Stopwatch();
            for (int i = 0; i < Count; i++)
            {
                GameObject gO = _gameObjects[i];
                //The Tagger resource file does not ship with the package
                //to prevent overwrite of your tags.
                //So the demo tags are lost on the demo prefabs.
                //That's the reason i need to set the prefab tags here.
                //Normally this can be done on the prefab it self.
                if (gO.name.StartsWith("Capsule"))
                {
                    gO.SetTag("Capsule");
                }
                else if (gO.name.StartsWith("Cube"))
                {
                    gO.SetTag("Cube");
                }
                else if (gO.name.StartsWith("Sphere"))
                {
                    gO.SetTag("Sphere");
                }
                else if (gO.name.StartsWith("Cylinder"))
                {
                    gO.SetTag("Cylinder");
                }
                
                int colorCount = Random.Range(1, 4);

                int index = Random.Range(0, _colors.Length);
                sw.Start();
                gO.AddTag(_colorTags[index]);
                sw.Stop();
                Color color = _colors[index];

                if (colorCount > 1)
                {
                    index = Random.Range(0, _colors.Length);
                    sw.Start();
                    gO.AddTag(_colorTags[index]);
                    sw.Stop();
                    Color color2 = _colors[index];

                    if (colorCount > 2)
                    {
                        index = Random.Range(0, _colors.Length);
                        sw.Start();
                        gO.AddTag(_colorTags[index]);
                        sw.Stop();

                        color = (_colors[index] + color2 + color) / 3f * 2f;
                    }
                    else
                    {
                        color = (color2 + color) / 2f * 1.5f;
                    }
                }

                if (!performanceTest)
                {
                    Renderer component = gO.GetComponent<Renderer>();
                    if (component != null) component.material.SetColor("_Color", color);
                }


                index = Random.Range(0, _scale.Length);
                sw.Start();
                gO.AddTag(_scaleTags[index]);
                sw.Stop();
                if (!performanceTest)
                {
                    float scale = _scale[index];
                    gO.transform.localScale = new Vector3(scale, scale, scale);
                }
            }

            return sw.ElapsedTicks / 10000f;
        }

        public void Prepare()
        {
            if (_gameObjects != null)
            {
                foreach (GameObject o in _gameObjects)
                {
                    if (o != null) Destroy(o);
                }
            }

            TaggerSystem.Data.AddTag("Red", "Color");
            TaggerSystem.Data.AddTag("Blue", "Color");
            TaggerSystem.Data.AddTag("Green", "Color");
            TaggerSystem.Data.AddTag("Small", "Size");
            TaggerSystem.Data.AddTag("Normal", "Size");
            TaggerSystem.Data.AddTag("Large", "Size");
            TaggerSystem.Data.AddTag("Capsule", "Shape");
            TaggerSystem.Data.AddTag("Cube", "Shape");
            TaggerSystem.Data.AddTag("Cylinder", "Shape");
            TaggerSystem.Data.AddTag("Sphere", "Shape");
            
            Renderer r = GetComponentInChildren<Renderer>();
            _randomPos = r;
            if (_randomPos)
            {
                _spawnBounds = r.bounds;
            }

            _gameObjects = new GameObject[Count];
        }

        public float Spawn()
        {
            Stopwatch sw = new Stopwatch();

            for (int i = 0; i < Count; i++)
            {
                Vector3 position = Vector3.zero;
                if (_randomPos)
                {
                    do
                    {
                        float x = Random.Range(_spawnBounds.min.x, _spawnBounds.max.x);
                        float z = Random.Range(_spawnBounds.min.z, _spawnBounds.max.z);

                        position = new Vector3(x, 1f, z);
                    }
                    while (Vector3.Distance(position, Target.position) < 5f);
                }

                int index = Random.Range(0, Prefabs.Count);
                GameObject prefab = Prefabs[index];
                sw.Start();
                _gameObjects[i] = Instantiate(prefab, position, Quaternion.identity, transform);
                sw.Stop();
            }

            return sw.ElapsedTicks / 10000f;
        }

        private void Start()
        {
            if (AutoStart)
            {
                Prepare();
                Spawn();
                Colorize();
            }
        }
    }
}