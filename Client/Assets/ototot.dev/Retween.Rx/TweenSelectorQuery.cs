using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx;

namespace Retween.Rx
{

    /// <summary>
    /// 
    /// </summary>
    [Serializable]
    public class TweenSelectorQuery
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        public void SetTarget(TweenSelector target)
        {
            Target = target;
        }

        /// <summary>
        /// 
        /// </summary>
        public TweenSelector Target { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public bool usingTag;

        /// <summary>
        /// 
        /// </summary>
        public string tag;

        /// <summary>
        /// 
        /// </summary>
        public HashSet<string> activeClasses = new HashSet<string>();

        /// <summary>
        /// 
        /// </summary>
        public HashSet<string> activeStates = new HashSet<string>();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<string> GetActiveNames()
        {
            List<string> ret = new List<string>();

            var hasTag = usingTag && !string.IsNullOrEmpty(tag);

            foreach (var c in activeClasses)
            {
                ret.Add(hasTag ? $"{tag}.{c}" : $".{c}");

                foreach (var s in activeStates)
                    ret.Add(hasTag ? $"{tag}.{c}:{s}" : $".{c}:{s}");
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool enableInitTweens;

        /// <summary>
        /// 
        /// </summary>
        public bool skipInitTweens;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="string"></typeparam>
        public List<string> initActiveClasses = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="string"></typeparam>
        public List<string> initActiveStates = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TweenName"></typeparam>
        public List<TweenName> sources = new List<TweenName>();

        /// <summary>
        /// 
        /// </summary>
        public List<TweenAnim> TweenAnims => sources.Select(t => t?.GetComponent<TweenAnim>()).Where(a => a != null).ToList();

        /// <summary>
        /// 
        /// </summary>
        public List<TweenSequence> TweenSeqs => sources.Select(t => t?.GetComponent<TweenSequence>()).Where(s => s != null).ToList();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public void Add(string name, bool clearBeforeAdd = false, bool applyAfterAdd = false)
        {
            if (name.StartsWith("."))
            {
                if (clearBeforeAdd)
                    activeClasses.Clear();
                if (activeClasses.Add(name.Substring(1)) && applyAfterAdd)
                    Apply();
            }
            else if (name.StartsWith(":"))
            {
                if (clearBeforeAdd)
                    activeStates.Clear();
                if (activeStates.Add(name.Substring(1)) && applyAfterAdd)
                    Apply();
            }
            else
            {
                Debug.LogWarning($"TweenSelectorQuery => {name} is invalid!!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="applyAfterRemove"></param>
        public void Remove(string name, bool applyAfterRemove = false)
        {
            if (name.StartsWith("."))
            {
                if (activeClasses.Remove(name.Substring(1)) && applyAfterRemove)
                    Apply();
            }
            else if (name.StartsWith(":"))
            {
                if (activeStates.Remove(name.Substring(1)) && applyAfterRemove)
                    Apply();
            }
            else
            {
                Debug.LogWarning($"TweenSelectorQuery => {name} is invalid!!");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetName"></param>
        /// <param name="newName"></param>
        /// <param name="applyAfterReplace"></param>
        public void Replace(string targetName, string newName, bool applyAfterRemove = false, bool applyAfterAdd = false)
        {
            if (targetName.StartsWith("."))
            {
                if (activeClasses.Remove(targetName.Substring(1)) && applyAfterRemove)
                    Apply();
            }
            else if (targetName.StartsWith(":"))
            {
                if (activeStates.Remove(targetName.Substring(1)) && applyAfterRemove)
                    Apply();
            }
            else
            {
                Debug.LogWarning($"TweenSelectorQuery => {targetName} is invalid!!");
            }

            if (newName.StartsWith("."))
            {
                activeClasses.Add(targetName.Substring(1));

                if (applyAfterAdd)
                    Apply();
            }
            else if (targetName.StartsWith(":"))
            {
                activeStates.Remove(targetName.Substring(1));

                if (applyAfterAdd)
                    Apply();
            }
            else
            {
                Debug.LogWarning($"TweenSelectorQuery => {targetName} is invalid!!");
            }
        }
    
        public void Apply(bool rollbackOnly = false)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            if (Target == null)
                return;

            foreach (var t in TweenAnims)
            {
                if (t.TweenName.Match(Target))
                {
                    if (!rollbackOnly && !Target.matchingResults.Contains(t.TweenName))
                    {
                        Target.matchingResults.Add(t.TweenName);
                        Target.Player.Run(t, !t.rewindOnCancelled && !t.runRollback)?.Subscribe();

                        // Debug.Log($"TweenSelector => {t.gameObject.name} in TweenAnim is selected.");
                    }
                }
                else
                {
                    if (Target.matchingResults.Contains(t.TweenName))
                    {
                        Target.matchingResults.Remove(t.TweenName);
                        Target.Player.Rollback(t).Subscribe();

                        // Debug.Log($"TweenSelector => {t.gameObject.name} in TweenAnim is unselected.");
                    }
                }
            }

#if ENABLE_DOTWEEN_SEQUENCE
        foreach (var t in TweenSeqs) {
            if (t.TweenName.Match(Target)) {
                if (!rollbackOnly && !Target.matchingResults.Contains(t.TweenName)) {
                    Target.matchingResults.Add(t.TweenName);
                    Target.Player.Run(t, !t.rewindOnCancelled)?.Subscribe();

                    // Debug.Log($"TweenSelector => {t.gameObject.name} at TweenSequence is selected.");
                }
            }
            else {
                if (Target.matchingResults.Contains(t.TweenName)) {
                    Target.matchingResults.Remove(t.TweenName);
                    Target.Player.Rewind(t)?.Subscribe();

                    // Debug.Log($"TweenSelector => {t.gameObject.name} at TweenSequence is unselected.");
                }
            }
        }
#endif

#if UNITY_EDITOR
            BuildSelectables();
            Target.ForceToRepaint();
#endif
        }

#if UNITY_EDITOR

        /// <summary>
        /// 
        /// </summary>
        public List<string> selectableNames = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        public List<string> selectableClasses = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        public List<string> selectableStates = new List<string>();

        /// <summary>
        /// 
        /// </summary>
        public void BuildSelectables()
        {
            selectableNames.Clear();
            selectableClasses.Clear();
            selectableStates.Clear();

            foreach (var a in TweenAnims)
            {
                if (a == null || a.TweenName == null)
                    continue;

                selectableNames.Add(a.gameObject.name);

                a.TweenName.Parse();

                if (!string.IsNullOrEmpty(a.TweenName.className))
                    selectableClasses.Add(a.TweenName.className);

                if (!string.IsNullOrEmpty(a.TweenName.stateName))
                    selectableStates.Add(a.TweenName.stateName);
            }

            foreach (var s in TweenSeqs)
            {
                if (s == null || s.TweenName == null)
                    continue;

                selectableNames.Add(s.gameObject.name);

                s.TweenName.Parse();

                if (!string.IsNullOrEmpty(s.TweenName.className))
                    selectableClasses.Add(s.TweenName.className);

                if (!string.IsNullOrEmpty(s.TweenName.stateName))
                    selectableStates.Add(s.TweenName.stateName);
            }

            selectableClasses = selectableClasses.Distinct().ToList();
            selectableStates = selectableStates.Distinct().ToList();
        }

        public bool forceToRepaintFlag;

#endif

    }

}