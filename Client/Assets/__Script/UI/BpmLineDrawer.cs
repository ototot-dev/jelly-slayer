using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{

    /// <summary>
    /// 
    /// </summary>
    public class BpmLineDrawer : Graphic
    {
        /// <summary>
        /// 
        /// </summary>
        public int segmentNum = 20;

        /// <summary>
        /// 
        /// </summary>
        public int SegmentPointNum => segmentNum + 1;

        /// <summary>
        /// 
        /// </summary>
        public float thickness = 1;

        /// <summary>
        /// 
        /// </summary>
        public float speed = 4;

        /// <summary>
        /// 
        /// </summary>
        public float displayDuration = 4;

        /// <summary>
        /// 
        /// </summary>
        public float SegmentDeltaTime => displayDuration / segmentNum;
        
        /// <summary>
        /// 
        /// </summary>
        public float bpm = 60f;

        protected override void Awake()
        {
            __vertexBuff = new List<UIVertex>(segmentNum * 4);
            __indexBuff = new List<int>(segmentNum * 6);
            __heightMap = new List<float>(SegmentPointNum);

            ResizeBuff();

            __rectTM = GetComponent<RectTransform>();
        }
        
        List<UIVertex> __vertexBuff;
        List<int> __indexBuff;
        List<float> __heightMap;
        RectTransform __rectTM;

        /// <summary>
        /// 
        /// </summary>
        void ResizeBuff()
        {
            __vertexBuff.Clear();
            __indexBuff.Clear();
            __heightMap.Clear();

            for (int i = 0; i < segmentNum; i++)
            {
                __vertexBuff.Add(new UIVertex());
                __vertexBuff.Add(new UIVertex());
                __vertexBuff.Add(new UIVertex());
                __vertexBuff.Add(new UIVertex());

                __indexBuff.Add(0);
                __indexBuff.Add(0);
                __indexBuff.Add(0);
                __indexBuff.Add(0);
                __indexBuff.Add(0);
                __indexBuff.Add(0);
            }

            for (int i = 0; i < SegmentPointNum; i++)
                __heightMap.Add(0);
        }

        public float pulseTimeStamp;

        // protected override void Start()
        // {
        //     Observable.EveryFixedUpdate().Subscribe(_ => 
        //     {
        //         if ((Time.realtimeSinceStartup - pulseTimeStamp) >= 60 / bpm)
        //             pulseTimeStamp = Time.realtimeSinceStartup;

        //         SetVerticesDirty();
        //     }).AddTo(this);
        // }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);
            
            if (__vertexBuff != null)
            {
                vh.Clear();

                if (segmentNum != __vertexBuff.Count / 4)
                    ResizeBuff();

                var timeStamp = Time.realtimeSinceStartup;

                for (int i = 0; i < segmentNum; i++)
                    UpdateSegment(i, timeStamp);

                vh.AddUIVertexStream(__vertexBuff, __indexBuff);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segmentIndex"></param>
        void UpdateSegment(int segmentIndex, float timeStamp)
        {
            UpdatePulseValue(segmentIndex, timeStamp);

            var rectScale = new Vector2(__rectTM.rect.width / segmentNum, __rectTM.rect.height * 0.5f);
            var start = new Vector3(segmentIndex * rectScale.x, __heightMap[segmentIndex]);
            var end = new Vector3((segmentIndex + 1) * rectScale.x, __heightMap[segmentIndex + 1]);
            var thicknessVec = Quaternion.Euler(0, 0, 90) * (end - start).normalized;

            var pointA = new UIVertex();
            var pointB = new UIVertex();
            var pointC = new UIVertex();
            var pointD = new UIVertex();

            pointA.position = start + thicknessVec * thickness;
            pointB.position = start - thicknessVec * thickness;
            pointC.position = end - thicknessVec * thickness;
            pointD.position = end + thicknessVec * thickness;
            
            pointA.color = pointB.color = pointC.color = pointD.color = color;

            __vertexBuff[segmentIndex * 4] = pointA;
            __vertexBuff[segmentIndex * 4 + 1] = pointB;
            __vertexBuff[segmentIndex * 4 + 2] = pointC;
            __vertexBuff[segmentIndex * 4 + 3] = pointD;
        
            __indexBuff[segmentIndex * 6] = segmentIndex * 4;
            __indexBuff[segmentIndex * 6 + 1] = segmentIndex * 4 + 1;
            __indexBuff[segmentIndex * 6 + 2] = segmentIndex * 4 + 2;
            __indexBuff[segmentIndex * 6 + 3] = segmentIndex * 4 + 2;
            __indexBuff[segmentIndex * 6 + 4] = segmentIndex * 4 + 3;
            __indexBuff[segmentIndex * 6 + 5] = segmentIndex * 4;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="segmentIndex"></param>
        /// <param name="timeStamp"></param>
        void UpdatePulseValue(int segmentIndex, float timeStamp)
        {            
            var rectScale = new Vector2(__rectTM.rect.width / segmentNum, __rectTM.rect.height * 0.5f);
            var timeA = timeStamp - SegmentDeltaTime * segmentIndex;
            var timeB = timeStamp - SegmentDeltaTime * (segmentIndex + 1);

            timeA *= speed;
            timeB *= speed;

            var pulse = 0f;
            
            if (timeStamp - pulseTimeStamp <= 0.4f)
                pulse =  1 - Mathf.Abs(timeStamp - (pulseTimeStamp + 0f)) / 0.4f;
            
            //* Rect 중심부 웨이트값 적용
            pulse *= Mathf.Max(1 / (segmentNum * 0.5f), 1 - Mathf.Abs(segmentNum * 0.4f - segmentIndex) / (segmentNum * 0.5f));

            pulse *= pulse * pulse;
            pulse *= 1.2f;

            if (segmentIndex == 0)
                __heightMap[segmentIndex] = Perlin.Noise(timeA) * rectScale.y * pulse;
                
            __heightMap[segmentIndex + 1] = Perlin.Noise(timeB) * rectScale.y * pulse;
        }
    }
}
