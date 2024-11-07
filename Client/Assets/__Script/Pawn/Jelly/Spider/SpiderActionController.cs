using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using DG.Tweening;

namespace Game
{
    public class SpiderActionController : PawnActionController
    {
        public Transform emitPoint;

        protected override void AwakeInternal()
        {
            base.AwakeInternal();
            __brain = GetComponent<SpiderBrain>();
        }

        SpiderBrain __brain;
        IDisposable __actionDisposable;

        void Start()
        {
            onActionStart += (actionContext, damageContext) =>
            {
                if (__actionDisposable != null)
                {
                    __actionDisposable.Dispose();
                    __actionDisposable = null;
                }

                if (actionContext.actionName == "Emit")
                {
                    ExecuteEmitAction();
                }
                else if (actionContext.actionName == "!Damaged")
                {
                    // if (__brain.BB.HeartPoint >= __brain.BB.shared.ranged.minHeartPointToEmitPassive)
                    //     __brain.BB.emit.remainDamageCountToEmitPassiveProjectile--;

                    // __actionDisposable = Observable.Timer(TimeSpan.FromSeconds(0.5f))
                    //     .Subscribe(_ =>
                    //     {
                    //         FinishAction();

                    //         if (__brain.BB.emit.remainDamageCountToEmitPassiveProjectile <= 0)
                    //         {
                    //             selectedPassiveProjectile = __brain.BB.emit.passiveProjectile;

                    //             __brain.BB.emit.remainDamageCountToEmitPassiveProjectile = UnityEngine.Random.Range(__brain.BB.shared.ranged.damageCountToEmitPassive.min, __brain.BB.shared.ranged.damageCountToEmitPassive.max + 1);
                    //             __Logger.Log(gameObject, nameof(__brain.BB.emit.remainDamageCountToEmitPassiveProjectile), __brain.BB.emit.remainDamageCountToEmitPassiveProjectile);
                    //         }
                    //     }).AddTo(this);
                }
            };

            onActionCanceled += (actionName, rewindSpeed) =>
            {
                if (__actionDisposable != null)
                {
                    __actionDisposable.Dispose();
                    __actionDisposable = null;
                }
            };
        }

        /// <summary>
        /// 
        /// </summary>
        public void ExecuteEmitAction()
        {
            // __jellyBallPrefab = Resources.Load<GameObject>("Projectile/JellyBall").GetComponent<JellyBallController>();

            // var timeStamp = Time.time;

            // __actionDisposable = Observable.Timer(TimeSpan.FromSeconds(1))
            //     .ContinueWith(Observable.FromCoroutine(EmitActiveInternal_Coroutine))
            //     .ContinueWith(Observable.Timer(TimeSpan.FromSeconds(1)))
            //     .Do(_ =>
            //     {
            //         selectedActiveProjectie = JellySlimeBlackboard.ActiveProjectiles.None;
            //         FinishAction();
            //     })
            //     .Subscribe().AddTo(this);

            // __Logger.VerboseR(gameObject, nameof(ExecuteEmitAction), nameof(selectedActiveProjectie), selectedActiveProjectie);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetPoint"></param>
        /// <returns></returns>
        public float CalcEmitAnglePitchOffset(Vector3 targetPoint)
        {
            const float __MIN_DISTANCE_TO_TARGET = 2f;

            var deltaVec = targetPoint - transform.position;

            //* 거리가 너무 가까우면 최소 거리로 보정함
            if (deltaVec.magnitude < __MIN_DISTANCE_TO_TARGET)
            {
                deltaVec = TerrainManager.GetTerrainPoint(transform.position + deltaVec.Vector2D().normalized * __MIN_DISTANCE_TO_TARGET) - transform.position;
                __emitPitchOffsetCached = -Mathf.Rad2Deg * Mathf.Atan2(deltaVec.y, deltaVec.Magnitude2D());

                return __emitPitchOffsetCached;
            }

            __emitPitchOffsetCached = Mathf.Rad2Deg * Mathf.Atan2(deltaVec.y, deltaVec.Magnitude2D());

            var lerpPoint0 = Vector3.Lerp(transform.position, targetPoint, 0.2f);
            var lerpPoint1 = Vector3.Lerp(transform.position, targetPoint, 0.4f);
            var lerpPoint2 = Vector3.Lerp(transform.position, targetPoint, 0.6f);
            var lerpPoint3 = Vector3.Lerp(transform.position, targetPoint, 0.8f);

            var terrainPoint0 = TerrainManager.GetTerrainPoint(lerpPoint0);
            var terrainPoint1 = TerrainManager.GetTerrainPoint(lerpPoint1);
            var terrainPoint2 = TerrainManager.GetTerrainPoint(lerpPoint2);
            var terrainPoint3 = TerrainManager.GetTerrainPoint(lerpPoint3);

            if (terrainPoint0.y > lerpPoint0.y)
            {
                deltaVec = terrainPoint0 - transform.position;
                __emitPitchOffsetCached = MathF.Max(__emitPitchOffsetCached, Mathf.Rad2Deg * Mathf.Atan2(deltaVec.y, deltaVec.Magnitude2D()));
            }
            if (terrainPoint1.y > lerpPoint1.y)
            {
                deltaVec = terrainPoint1 - transform.position;
                __emitPitchOffsetCached = MathF.Max(__emitPitchOffsetCached, Mathf.Rad2Deg * Mathf.Atan2(deltaVec.y, deltaVec.Magnitude2D()));
            }
            if (terrainPoint2.y > lerpPoint2.y)
            {
                deltaVec = terrainPoint2 - transform.position;
                __emitPitchOffsetCached = MathF.Max(__emitPitchOffsetCached, Mathf.Rad2Deg * Mathf.Atan2(deltaVec.y, deltaVec.Magnitude2D()));
            }
            if (terrainPoint3.y > lerpPoint3.y)
            {
                deltaVec = terrainPoint3 - transform.position;
                __emitPitchOffsetCached = MathF.Max(__emitPitchOffsetCached, Mathf.Rad2Deg * Mathf.Atan2(deltaVec.y, deltaVec.Magnitude2D()));
            }

            __emitPitchOffsetCached *= -1;

            return __emitPitchOffsetCached;
        }

        float __emitPitchOffsetCached;
        public float emitActiveCoolTime;
        public float EmitActiveTimeStamp { get; private set; }

        IEnumerator EmitActiveInternal_Coroutine()
        {
            EmitActiveTimeStamp = Time.time;
            // Instantiate(__jellyBallPrefab, emitPoint.position, Quaternion.LookRotation(emitPoint.forward)).GetComponent<JellyBallController>().Pop(gameObject, 10);
            yield return new WaitForSeconds(0.05f);
        }
    }
}