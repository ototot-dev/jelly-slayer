using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Retween.Rx
{
    [RequireComponent(typeof(TweenPlayer))]
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

            if (query.enableInitTweens)
            {
                foreach (var c in query.initActiveClasses)
                    query.activeClasses.Add(c);
                foreach (var s in query.initActiveStates)
                    query.activeStates.Add(s);
            }
        }

        void Start()
        {
            if (query.activeClasses.Count > 0 || query.activeStates.Count > 0)
            {
                query.Apply();

                if (query.skipInitTweens)
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
        public Dictionary<TweenName, TweenAnimState> TweenAnimStates { get; private set; } = new Dictionary<TweenName, TweenAnimState>();
        public Dictionary<TweenName, TweenSequenceState> TweenSeqStates { get; private set; } = new Dictionary<TweenName, TweenSequenceState>();

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