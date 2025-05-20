using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Retween.Rx
{
    public class TweenSelector : MonoBehaviour
    {
        public TweenPlayer Player { get; private set; }

        void Awake()
        {
            Player = GetComponent<TweenPlayer>();

            query.SetTargetSelector(this);
#if UNITY_EDITOR
            query.BuildSelectables();
#endif

            if (query.initialClasses.Count > 0 || query.initialStates.Count > 0)
            {
                foreach (var c in query.initialClasses)
                    query.activeClasses.Add(c);
                foreach (var s in query.initialStates)
                    query.activeStates.Add(s);
            }
        }

        void Start()
        {
            if (query.activeClasses.Count > 0 || query.activeStates.Count > 0)
            {
                query.Apply();

                if (query.forceCompleteTweenOnStart)
                {
                    foreach (var r in Player.tweenStates.Values)
                    {
                        Player.AdvanceElapsed(r, r.duration);
                        Player.UpdateAnimation(r);
                    }
                }
            }
        }

        public TweenSelectorQuery query = new();
        public HashSet<TweenName> matchingResults = new();
        public readonly Dictionary<TweenName, TweenAnimState> tweenAnimStates = new();
#if ENABLE_DOTWEEN_SEQUENCE
        public readonly Dictionary<TweenName, TweenSequenceState> tweenSeqStates = new();
#endif

#if UNITY_EDITOR
        void OnValidate()
        {
            OnValidateInternal();
        }

        public virtual void OnValidateInternal()
        {
            if (query.TargetSelector == null)
                query.SetTargetSelector(this);

            query.BuildSelectables();
        }

        public void ForceToRepaint()
        {
            if (query.TargetSelector == null)
                query.SetTargetSelector(this);

            query.forceToRepaintFlag = !query.forceToRepaintFlag;

            EditorUtility.SetDirty(this);
        }
#endif
    }
}