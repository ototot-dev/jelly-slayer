using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Game
{
    public class SpiderBallProjectile : ProjectileMovement
    {
        [Header("Config")]
        public float maxRotationSpeed = 1;  //* 회전값을 제한함 (회전이 너무 빠르면 Mesh Deforming이 찢어지는 현상이 발생함)

        [Header("Component")]
        public JellySpringMassSystem springMassSystem;
        public JellyMeshBuilder meshBuilder;

        Quaternion __prevCoreRotation;

        protected override void OnFixedUpdateHandler()
        {
            var deltaAngle = Quaternion.Angle(__prevCoreRotation, springMassSystem.core.rotation);
            var deltaRotation = Quaternion.Slerp(Quaternion.identity, springMassSystem.core.rotation * Quaternion.Inverse(__prevCoreRotation), Mathf.Clamp01(maxRotationSpeed / deltaAngle));

            __prevCoreRotation = springMassSystem.core.rotation;
            springMassSystem.bounds.SetPositionAndRotation(springMassSystem.core.position, springMassSystem.bounds.rotation * deltaRotation);
        }

        protected override void StartInternal()
        {
            base.StartInternal();

            __prevCoreRotation = springMassSystem.core.rotation;
            Observable.Timer(TimeSpan.FromSeconds(0.2f)).Subscribe(_ => Pop(null, 10)).AddTo(this);
        }
    }
}