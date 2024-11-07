using System;
using System.Linq;
using UnityEngine;

namespace Retween.Rx
{
    public class TweenName : MonoBehaviour
    {
        public string tagName;
        public string className;
        public string stateName;

#if UNITY_EDITOR
        public bool hasValidName = false;
#endif

        public void Parse()
        {
            var ret = ParseInternal(gameObject.name, ref tagName, ref className, ref stateName);

#if UNITY_EDITOR
            hasValidName = ret;
#endif
        }

        public static bool ParseInternal(string sourceName, ref string tagName, ref string className, ref string stateName)
        {
            var charArray = sourceName.ToCharArray();

            if (charArray.Count(c => c == '.') != 1)
            {
                Debug.LogWarning($"TweenName => {sourceName} is invalid name!!");
                tagName = className = stateName = string.Empty;
                return false;
            }

            if (charArray.Count(c => c == ':') > 1)
            {
                Debug.LogWarning($"TweenName => {sourceName} is invalid name!!");
                tagName = className = stateName = string.Empty;
                return false;
            }

            var entries = sourceName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (sourceName.StartsWith("."))
            {
                if (entries.Length != 1)
                {
                    Debug.LogWarning($"TweenName => {sourceName} is invalid name!!");
                    tagName = className = stateName = string.Empty;
                    return false;
                }

                entries = entries[0].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                if (entries.Length > 2)
                {
                    Debug.LogWarning($"TweenName => {sourceName} is invalid name!!");
                    tagName = className = stateName = string.Empty;
                    return false;
                }

                if (entries.Length == 1)
                {
                    className = entries[0];
                    stateName = string.Empty;
                }
                else
                {
                    className = entries[0];
                    stateName = entries[1];
                }

                tagName = string.Empty;
            }
            else
            {
                if (entries.Length != 2)
                {
                    Debug.LogWarning($"TweenName => {sourceName} is invalid name!!");
                    tagName = className = stateName = string.Empty;
                    return false;
                }

                var cacheTagName = entries[0];

                if (entries.Length == 2)
                {
                    entries = entries[1].Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

                    if (entries.Length > 2)
                    {
                        Debug.LogWarning($"TweenName => {sourceName} is invalid name!!");
                        tagName = className = stateName = string.Empty;
                        return false;
                    }

                    if (entries.Length == 1)
                    {
                        className = entries[0];
                        stateName = string.Empty;
                    }
                    else
                    {
                        className = entries[0];
                        stateName = entries[1];
                    }
                }

                tagName = cacheTagName;
            }

            return true;
        }

        public bool Match(TweenSelector tweenSelector)
        {
            if (!string.IsNullOrEmpty(tagName) && tagName != tweenSelector.query.tag)
                return false;

            if (!tweenSelector.query.activeClasses.Contains(className))
                return false;

            if (!string.IsNullOrEmpty(stateName) && !tweenSelector.query.activeStates.Contains(stateName))
                return false;

            return true;
        }
        
        void OnValidate()
        {
            Parse();
        }

    }

}