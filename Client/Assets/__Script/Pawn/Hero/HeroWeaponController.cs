using System;
using System.Linq;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{

    /// <summary>
    /// 
    /// </summary>
    public class HeroWeaponController : MonoBehaviour
    {

        /// <summary>
        /// 
        /// </summary>
        public Transform leftHandAttachPoint;

        /// <summary>
        /// 
        /// </summary>
        public Transform rightHandAttachPoint;

        /// <summary>
        /// 
        /// </summary>
        public Material holoMeshMaterial;

        /// <summary>
        /// 
        /// </summary>
        public GameObject WeaponMesh { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public GameObject HoloMesh { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public GameObject SpikeMesh { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public GameObject GlitterFx { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public GameObject ShieldMesh { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public GameObject BurstPoint { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            __brain = GetComponent<HeroBrain>();
        }

        HeroBrain __brain;

        void Start()
        {
            __brain.onUpdate += () =>
            {
                //* guardRunning 값에 따른 ShieldMesh 외형 및 Anim 처리
                if (ShieldMesh != null)
                {
                    if (ShieldMesh.transform.localScale == Vector3.one)
                    {
                        if (__brain.BB.IsGuarding && !__brain.ActionCtrler.CheckActionRunning())
                        {
                            __brain.AnimCtrler.mainAnimator.SetBool("bGuarding", true);
                            __brain.AnimCtrler.mainAnimator.SetTrigger("toGuard");

                            ShieldMesh.transform.localScale = Vector3.one * 2.5f;
                        }
                    }
                    else
                    {
                        if (!__brain.BB.IsGuarding || __brain.ActionCtrler.CheckActionRunning())
                        {
                            __brain.AnimCtrler.mainAnimator.SetBool("bGuarding", false);
                            ShieldMesh.transform.localScale = Vector3.one;
                        }
                    }
                }
                
                BurstPoint.transform.rotation = Quaternion.identity;
            };

            RefreshWeaponMesh();
        }

        Vector3 __meshLocalScaleCached;
        EffectInstance __burstPendingFx;

        /// <summary>
        /// 
        /// </summary>
        void RefreshWeaponMesh()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        void OverrideMaterial(GameObject targetMesh)
        {
            foreach (var r in targetMesh.Descendants().Where(d => d.CompareTag("PawnWeaponMesh")).Select(d => d.GetComponent<Renderer>()).ToArray())
            {
                var textureCached = r.sharedMaterial.mainTexture;
                var colorCached = r.sharedMaterial.color;

                r.material = Resources.Load<Material>("Material/PawnParts");

                if (textureCached != null)
                {
                    r.material.SetTexture("_BodyTexture", textureCached);
                    r.material.SetColor("_BodyColor", Color.clear);
                }
                else
                {
                    r.material.SetColor("_BodyColor", colorCached);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executeBurst"></param>
        /// <returns></returns>
        public HeroSwordProjectile SpawnSwordProjectile(bool executeBurst = false)
        {
            var objRes = Resources.Load<GameObject>("Projectile/HeroSwordProjectile");
            var objProj = Instantiate(objRes, transform.position + transform.forward + Vector3.up, Quaternion.LookRotation(Vector3.down));
            var projectile = objProj.GetComponent<HeroSwordProjectile>();

            projectile.executeBurst = executeBurst;
            projectile.Go(__brain, 5, 1);

            return projectile;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="executeBurst"></param>
        /// <returns></returns>
        public HeroHammerProjectile SpawnHammerProjectile(bool executeBurst = false)
        {
            var objRes = Resources.Load<GameObject>("Projectile/HeroHammerProjectile");
            var objProj = Instantiate(objRes, transform.position + transform.forward + Vector3.up, Quaternion.LookRotation(Vector3.down));
            var projectile = objProj.GetComponent<HeroHammerProjectile>();

            projectile.executeBurst = executeBurst;
            projectile.Go(__brain, 5, 1);

            return projectile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executeBurst"></param>
        /// <returns></returns>
        public HeroSpikeProjectile SpawnSpikeProjectile(bool executeBurst = false)
        {
            var objRes = Resources.Load<GameObject>("Projectile/HeroSpikeProjectile");
            var objProj = Instantiate(objRes, transform.position, Quaternion.LookRotation(transform.forward));
            var projectile = objProj.GetComponent<HeroSpikeProjectile>();

            projectile.executeBurst = executeBurst;
            projectile.Go(__brain, 0, 0.6f);

            return projectile;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="position"></param>
        /// <param name="forward"></param>
        /// <param name="splitCount"></param>
        /// <param name="targetPoint"></param>
        /// <param name="executeBurst"></param>
        /// <returns></returns>
        public HeroWandProjectile SpawnWandProjectile(Vector3 position, Vector3 forward, int splitCount = 0, Transform targetPoint = null, bool executeBurst = false)
        {
            var objRes = Resources.Load<GameObject>("Projectile/HeroWandProjectile");
            var projectile = Instantiate(objRes, position, Quaternion.LookRotation(forward)).GetComponent<HeroWandProjectile>();

            projectile.targetPoint = targetPoint;
            projectile.splitCount = splitCount;
            projectile.executeBurst = executeBurst;
            projectile.Go(__brain, projectile.forwardSpeed, 0.5f);

            return projectile;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public HeroBombProjectile SpawnBombProjectile()
        {
            var objRes = Resources.Load<GameObject>("Projectile/HeroBombProjectile");
            var projectile = Instantiate(objRes, transform.position, Quaternion.LookRotation(transform.forward)).GetComponent<HeroBombProjectile>();

            projectile.transform.rotation *= Quaternion.Euler(projectile.impulsePitch, 0, 0);
            projectile.Pop(__brain, projectile.impulse, 1.6f);

            return projectile;
        }

        /// <summary>
        /// 
        /// </summary>
        public HeroTripWireProjectile SpawnTripWireProjectile()
        {
            // var projectile = Instantiate(Resources.Load<GameObject>("Projectile/HeroCaptureProjectile"), capturedBrain.transform.position, capturedBrain.transform.rotation).GetComponent<HeroCaptureProjectile>();

            // projectile.capturedBrain = capturedBrain;
            // projectile.Go(gameObject, 0, capturedBrain.BB.shared.curves.mainScaleCurve.Evaluate(capturedBrain.BB.ScaleFactor) * 1.2f);

            // return projectile;
            return null;
        }

    }

}