using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game
{
    public class PawnSoundSourceGenerator : MonoBehaviour
    {   
        public struct SoundSource
        {
            public Collider collider;
            public Vector3 position;
            public float timeStamp;
            public float intensity;
            public float duration;
            public string desc;

            public SoundSource(Collider collider, Vector3 position, float timeStamp, float intensity, float duration, string desc)
            {
                this.collider = collider;
                this.position = position;
                this.timeStamp = timeStamp;
                this.intensity = intensity;
                this.duration = duration;
                this.desc = desc;
            }
        }

        readonly List<SoundSource> __soundSources  = new();

        /// <summary>
        /// 가장 최근 SoundSource가 발생한 시간 기록
        /// </summary>
        public float LastGenerateTimeStamp { get; private set; }

        public void GenerateSoundSource(Collider collider, float intensity, float duration, string desc = "")
        {
            LastGenerateTimeStamp = Time.time;

            //* duration이 경과된 사운드 삭제
            __soundSources.RemoveAll(e => (LastGenerateTimeStamp - e.timeStamp) > e.duration);
            __soundSources.Add(new SoundSource(collider, transform.position, LastGenerateTimeStamp, intensity, duration, desc));
        }

        public bool QuerySoundSource(float compareIntensity, float compareTimeStamp, out SoundSource result)
        {
            //* duration이 경과된 사운드 삭제
            __soundSources.RemoveAll(e => (Time.time - e.timeStamp) > e.duration);
            
            result = __soundSources.FirstOrDefault(s => s.intensity >= compareIntensity && (s.timeStamp + s.duration) > compareTimeStamp);
            return result.timeStamp > 0;
        }

        public SoundSource[] QuerySoundSource(float compareIntensity, float compareTimeStamp)
        {
            //* duration이 경과된 사운드 삭제
            __soundSources.RemoveAll(e => (Time.time - e.timeStamp) > e.duration);
            return __soundSources.Where(s => s.intensity >= compareIntensity && s.timeStamp > compareTimeStamp).ToArray();
        }
    }
}