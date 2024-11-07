using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{

    /// <summary>
    /// 
    /// </summary>
    public class PawnSoundSource : MonoBehaviour
    {      

        /// <summary>
        /// 
        /// </summary>
        public PawnBrainController PawnBrain { get; private set; }

        void Awake()
        {
            PawnBrain  = GetComponent<PawnBrainController>();
        }

        /// <summary>
        /// 
        /// </summary>
        public struct SoundEvent
        {
            /// <summary>
            /// 발생 위치
            /// </summary>
            public Vector3 position;

            /// <summary>
            /// 발생 시간
            /// </summary>
            public float timeStamp;

            /// <summary>
            /// 사운드 강도
            /// </summary>
            public float intensity;

            /// <summary>
            /// 사운드 지속 시간
            /// </summary>
            public float duration;

            /// <summary>
            /// 추가 정보
            /// </summary>
            public string desc;

            /// <summary>
            /// Ctor.
            /// </summary>
            public SoundEvent(Vector3 position, float timeStamp, float intensity, float duration, string desc)
            {
                this.position = position;
                this.timeStamp = timeStamp;
                this.intensity = intensity;
                this.duration = duration;
                this.desc = desc;
            }
        }
        
        /// <summary>
        /// Item1 => 사운드 발생 위치, Item2 => 사운드 강도
        /// Item3 => 사운드 지속 시간, Item4 => 사운드 발생 시간 (Time.time)
        /// </summary>
        /// <returns></returns>
        List<SoundEvent> __soundEvents  = new List<SoundEvent>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="intensity"></param>
        /// <param name="duration"></param>
        /// <param name="desc"></param>
        public void GenerateSoundEvent(float intensity, float duration, string desc = "")
        {
            var timeStamp = Time.time;

            // duration이 경과된 사운드 삭제
            __soundEvents.RemoveAll(e => (timeStamp - e.timeStamp) > e.duration);
            __soundEvents.Add(new SoundEvent(transform.position, timeStamp, intensity, duration, desc));
        }

        /// <summary>
        /// 
        /// </summary>
        public bool QuerySoundEvent(float compareIntensity, float compareTimeStamp, out SoundEvent result)
        {
            // duration이 경과된 사운드 삭제
            __soundEvents.RemoveAll(e => (Time.time - e.timeStamp) > e.duration);

            result = __soundEvents.FirstOrDefault(s => s.intensity >= compareIntensity && (s.timeStamp + s.duration) > compareTimeStamp);

            return result.timeStamp > 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="compareIntensity"></param>
        /// <param name="compareTimeStamp"></param>
        /// <returns></returns>
        public SoundEvent[] QuerySoundEvent(float compareIntensity, float compareTimeStamp)
        {
            // duration이 경과된 사운드 삭제
            __soundEvents.RemoveAll(e => (Time.time - e.timeStamp) > e.duration);

            return __soundEvents.Where(s => s.intensity >= compareIntensity && s.timeStamp > compareTimeStamp).ToArray();
        }
    }
}