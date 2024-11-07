using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Retween.Rx
{

    /// <summary>
    /// 
    /// </summary>
    [RequireComponent(typeof(TweenPlayer))]
    public class TweenSelector : MonoBehaviour
    {

        /// <summary>
        /// 
        /// </summary>
        public TweenPlayer Player { get; private set; }

        void Awake()
        {
            Player = GetComponent<TweenPlayer>();

            query.SetTarget(this);

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
                    foreach (var r in Player.animRunnings.Values)
                    {
                        Player.AdvanceElapsed(r, r.duration);
                        Player.UpdateAnimation(r);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public TweenSelectorQuery query = new TweenSelectorQuery();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TweenAnim"></typeparam>
        /// <returns></returns>
        public HashSet<TweenName> matchingResults = new HashSet<TweenName>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TweenName"></typeparam>
        /// <typeparam name="TweenAnimRunning"></typeparam>
        /// <returns></returns>
        public Dictionary<TweenName, TweenAnimRunning> TweenAnimRunnings { get; private set; } = new Dictionary<TweenName, TweenAnimRunning>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TweenName"></typeparam>
        /// <typeparam name="TweenSequenceRunning"></typeparam>
        /// <returns></returns>
        public Dictionary<TweenName, TweenSequenceRunning> TweenSeqRunnings { get; private set; } = new Dictionary<TweenName, TweenSequenceRunning>();


#if UNITY_EDITOR

        /// <summary>
        /// 
        /// </summary>
        void OnValidate()
        {
            OnValidateInternal();
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void OnValidateInternal()
        {
            if (query.Target == null)
                query.SetTarget(this);

            query.BuildSelectables();
        }

        /// <summary>
        ///     
        /// </summary>
        public void ForceToRepaint()
        {
            if (query.Target == null)
                query.SetTarget(this);

            query.forceToRepaintFlag = !query.forceToRepaintFlag;

            EditorUtility.SetDirty(this);
        }

#endif

    }

}