using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Linq;
using UniRx;
using UniRx.Triggers;

namespace Game
{
    public class TestBubble : MonoBehaviour
    {
        float __offset = 0f;
        float __frequency = 1f;
        float __amplitude = 0.2f;

        void Start()
        {
            var meshGenerator = GetComponent<TerrainMeshGenerator>();

            meshGenerator.Init();

            Observable.Interval(TimeSpan.FromSeconds(0.033f)).Subscribe(_ =>
            {
                __offset += 0.033f;
                // __frequency = Mathf.Cos(__offset * 0.3f) * 4f;
                // __amplitude = Mathf.Sin(__offset * 0.2f) * 2f;

                meshGenerator.UpdateMesh();
            }).AddTo(this);
        }
    }
}
