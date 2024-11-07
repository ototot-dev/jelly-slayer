using System;
using System.Linq;
using UniRx;
using UnityEngine;
using Unity.Linq;

namespace Game
{

    /// <summary>
    /// 
    /// </summary>
    public class HeroTripWireProjectile : HeroProjectile
    {

        /// <summary>
        /// 
        /// </summary>
        public float forwardSpeed = 1;

        /// <summary>
        /// 
        /// </summary>
        public float forwardDecel = 1;

        /// <summary>
        /// 
        /// </summary>
        public bool IsWireConnected { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public void SetWireConnected()
        {
            IsWireConnected = true;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void StartInternal()
        {
            base.StartInternal();

            emitter.Where(v => v != null).Subscribe(v =>
            {
                emitterBrain = v.GetComponent<HeroBrain>();
            }).AddTo(this);

            InitWireFx();

            velocity = transform.forward.Vector2D() * forwardSpeed;

            __initPointCached = TerrainManager.GetTerrainPoint(transform.position) + Vector3.up * 0.5f;
        }

        Vector3 __initPointCached;

        /// <summary>
        /// 
        /// </summary>
        protected override void OnUpdateHandler()
        {
            base.OnUpdateHandler();

            velocity = velocity.Vector2D().normalized * Mathf.Max(0, velocity.Vector2D().magnitude - forwardDecel * Time.deltaTime);
        
            var startPoint = __initPointCached;
            var endPoint = TerrainManager.GetTerrainPoint(transform.position + velocity * Time.deltaTime) + Vector3.up * 0.5f;

            transform.position = endPoint;
            transform.rotation = Quaternion.LookRotation(startPoint - endPoint, Vector3.up);

            sensorCollider.transform.position = (startPoint + endPoint) * 0.5f;
            (sensorCollider as BoxCollider).size = new Vector3(1, 1, (startPoint - endPoint).Magnitude2D() * 5);
                
            UpdateWireFx(startPoint, endPoint);

        }

        #region WIRE-FX-SETTING

        [Header("WireFx")]
        public Transform startPoint;
        public Transform endPoint;
        public float maxLength;
        public float mainTextureLen = 1f;
        public float noiseTextureLen = 1f;
        public LineRenderer lineRenderer;

        /// <summary>
        /// 
        /// </summary>
        void InitWireFx()
        {
            lineRenderer.enabled = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        void UpdateWireFx(Vector3 startPoint, Vector3 endPoint)
        {
            var distanceVec = endPoint - startPoint;

            lineRenderer.SetPosition(0, endPoint);
            lineRenderer.SetPosition(1, startPoint);

            __mainTexutreTiling.x = mainTextureLen * distanceVec.magnitude;
            __noiseTextureTiling.x = noiseTextureLen * distanceVec.magnitude;

            lineRenderer.material.SetTextureScale("_MainTex", __mainTexutreTiling);
            lineRenderer.material.SetTextureScale("_Noise", __noiseTextureTiling);

            this.startPoint.position = startPoint;
            this.startPoint.rotation = Quaternion.LookRotation(distanceVec, Vector3.up);

            this.endPoint.position = transform.position;
            this.endPoint.rotation = transform.rotation;

            if (__particles == null)
            {
                __particles = lineRenderer.gameObject.DescendantsAndSelf().Select(d => d.GetComponent<ParticleSystem>()).Where(p => p != null).ToArray();

                foreach (var p in __particles)
                {
                    if (!p.isPlaying)
                        p.Play();
                }
            }
        }

        ParticleSystem[] __particles;
        Vector2 __mainTexutreTiling = new(1, 1);
        Vector2 __noiseTextureTiling = new(1, 1);

        /// <summary>
        /// 
        /// </summary>
        void CleanUpWireFx()
        {
            lineRenderer.enabled = false;

            if (__particles != null)
            {
                foreach (var p in __particles)
                {
                    if (p.isPlaying)
                        p.Stop();
                }
            }
        }

        #endregion

    }

}
