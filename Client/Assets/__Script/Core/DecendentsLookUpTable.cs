using System;
using System.Collections.Generic;
using Retween.Rx;
using UnityEngine;
using UniRx;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    public class DecendentsLookUpTable : MonoBehaviour
    {
        /// <summary>
        /// FSM 노출용
        /// </summary>
        public DecendentsLookUpTable Self => this;

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="GameObject"></typeparam>
        /// <returns></returns>
        public List<GameObject> decendents = new List<GameObject>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GameObject Get(string name)
        {
            GameObject ret;

            __lookUpTable.TryGetValue(name, out ret);

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        public void RefreshLookUpTable()
        {
            __lookUpTable.Clear();

            foreach (var d in decendents)
                __lookUpTable.Add(d.name, d);
        }

        Dictionary<string, GameObject> __lookUpTable = new Dictionary<string, GameObject>();

        void Start()
        {
            RefreshLookUpTable();
        }
    }

}