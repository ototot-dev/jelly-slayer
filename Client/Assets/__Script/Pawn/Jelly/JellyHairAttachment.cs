using System;
using System.Collections.Generic;
using Obi;
using UnityEngine;

namespace Game
{
    public class JellyHairAttachment : MonoBehaviour
    {
        [Serializable]
        public struct Attachment
        {
            public ObiRope obiRope;
            public Transform attachPoint;
            public Vector3 attachOffset;
        }

        public List<Attachment> hairs;
    }
}