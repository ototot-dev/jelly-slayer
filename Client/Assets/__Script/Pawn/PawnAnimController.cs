using System;
using System.Collections.Generic;
using System.Linq;
using FIMSpace.FProceduralAnimation;
using UniRx;
using UniRx.Triggers.Extension;
using Unity.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations.Rigging;

namespace Game
{
    public class PawnAnimController : MonoBehaviour
    {
        [Header("Component")]
        public Animator mainAnimator;
        public LegsAnimator legAnimator;
        public RigBuilder rigBuilder;
        public  Dictionary<Rigidbody, Tuple<Vector3, Quaternion>> capturedPhysicsBodyTransforms = new();
        Dictionary<string, ObservableStateMachineTriggerEx> __observableStateMachineTriggersCached = new();
        
        public ObservableStateMachineTriggerEx FindObservableStateMachineTriggerEx(string stateName)
        {
            if (__observableStateMachineTriggersCached.TryGetValue(stateName, out var ret))
                return ret;
            
            var found = mainAnimator.GetBehaviours<ObservableStateMachineTriggerEx>().First(s => s.stateName == stateName);
            __observableStateMachineTriggersCached.Add(stateName, found);

            return found;
        }

        public void StartRagdoll(bool useGravity = true, bool refreshPhysicsBodyTransform = false)
        {
            if (refreshPhysicsBodyTransform)
            {   
                capturedPhysicsBodyTransforms.Clear();
                CaptureRagdollTransform();
            }
 
            foreach (var p in capturedPhysicsBodyTransforms)
            {
                p.Key.isKinematic = false;
                p.Key.useGravity = useGravity;
                if (p.Key.TryGetComponent<Collider>(out var collider))
                    collider.enabled = true;
            }

            mainAnimator.enabled = false;
            legAnimator.enabled = false;
        }

        void CaptureRagdollTransform()
        {
            foreach (var r in gameObject.Descendants().Where(d => d.layer == LayerMask.NameToLayer("PhysicsBody")).Select(d => d.GetComponent<Rigidbody>()))
            {
                if (capturedPhysicsBodyTransforms.ContainsKey(r))
                    capturedPhysicsBodyTransforms[r] = new(r.transform.position, r.transform.rotation);
                else
                    capturedPhysicsBodyTransforms.Add(r, new(r.transform.position, r.transform.rotation));
            }
        }

        public void FinishRagdoll(float blendDuration = -1f)
        {
            if (blendDuration > 0)
            {
                if (__ragdollDisposable != null)
                {
                    __ragdollDisposable.Dispose();
                    __ragdollDisposable = null;
                }
                
                CaptureRagdollTransform();
                GetComponent<PawnMovement>().capsule.transform.position = capturedPhysicsBodyTransforms.First().Key.transform.position.Vector2D();

                var alpha = 0f;
                var timeStamp = Time.time;

                //* Ragdoll에서 Animator로 블렌딩시킴
                __ragdollDisposable = Observable.EveryLateUpdate().TakeWhile(_ => alpha < 1f)
                    .DoOnCancel(() => FinishRagdollInternal())
                    .DoOnCompleted(() => FinishRagdollInternal())
                    .Subscribe(_ =>
                    {
                        alpha += Time.deltaTime / blendDuration;        
                        foreach(var p in capturedPhysicsBodyTransforms)
                            p.Key.transform.SetPositionAndRotation(Vector3.Slerp(p.Value.Item1, p.Key.transform.position, alpha), Quaternion.Slerp(p.Value.Item2, p.Key.transform.rotation, alpha));
                    }).AddTo(this);
            }
            else
            {
                FinishRagdollInternal();
            }
            
            if (mainAnimator != null)
                mainAnimator.enabled = true;
            if (legAnimator != null)
                legAnimator.enabled = true;
            if (rigBuilder != null)
                rigBuilder.enabled = true;
        }

        IDisposable __ragdollDisposable;

        void FinishRagdollInternal()
        {            
            foreach (var p in capturedPhysicsBodyTransforms)
            {
                p.Key.isKinematic = true;
                if (p.Key.TryGetComponent<Collider>(out var collider))
                    collider.enabled = false;
            }
        }
    }
}