using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    public class CubeController : MonoBehaviour
    {

        /// <summary>
        /// 
        /// </summary>
        public enum CubeTypes
        {
            None = -1,
            Sand,
            Soil,
            Rock,
            Steel,
            Max,
        }

        /// <summary>
        /// 
        /// </summary>
        public CubeTypes type;

        /// <summary>
        /// 
        /// </summary>
        public IntReactiveProperty overlappedCount = new();

        /// <summary>
        /// 
        /// </summary>
        public float jellyTransmittance;

        /// <summary>
        /// 
        /// </summary>
        public float projectileTransmittance;

        /// <summary>
        /// 
        /// </summary>
        public float spawnWaitingTime;

        /// <summary>
        /// 
        /// </summary>
        public float despawnWaitingTime;

        /// <summary>
        /// 
        /// </summary>
        public BoxCollider sensorCollier;

        /// <summary>
        /// 
        /// </summary>
        public BoxCollider blockCollider;

        /// <summary>
        /// 
        /// </summary>
        public MeshRenderer meshRenderer;

        /// <summary>
        /// 
        /// </summary>
        public float ScaledSize => transform.localScale.x * blockCollider.size.x;

        /// <summary>
        /// 
        /// </summary>
        public float ScaleHalfSize => transform.localScale.x * blockCollider.size.x * 0.5f;

        void Start()
        {
            transform.position += Vector3.up * ScaleHalfSize * 0.8f;

            // Observable.Timer(TimeSpan.FromSeconds(spawnWaitingTime)).Subscribe(_ =>
            // {
            //     sensorCollier.OnTriggerEnterAsObservable().Subscribe(v =>
            //     {
            //         if (v.TryGetComponent<JellySlimeMovementController>(out var moveCtrler))
            //         {
            //             moveCtrler.StartCubeOverlapped(this);
            //             overlappedCount.Value++;
            //         }
            //         else if (v.TryGetComponent<JellyProjectile>(out var projectile))
            //         {
            //             projectile.StartCubeOverlapped(this);
            //             overlappedCount.Value++;
            //         }
            //     }).AddTo(this);

            //     sensorCollier.OnTriggerExitAsObservable().Subscribe(v =>
            //     {
            //         if (v.TryGetComponent<JellySlimeMovementController>(out var moveCtrler))
            //         {
            //             moveCtrler.StopCubeOverlapped(this);
            //             overlappedCount.Value--;
            //         }
            //         else if (v.TryGetComponent<JellyProjectile>(out var projectile))
            //         {
            //             projectile.StopCubeOverlapped(this);
            //             overlappedCount.Value--;
            //         }
            //     }).AddTo(this);

            //     overlappedCount.Subscribe(v =>
            //     {
            //         meshRenderer.material.SetFloat("_DitherAlpha", v > 0 ? 0.5f : 1);
            //     }).AddTo(this);
            // }).AddTo(this);
        }

    }

}