using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{

    /// <summary>
    /// 
    /// </summary>
    public class HeroSpawnManager : MonoBehaviour
    {   
        /// <summary>
        ///
        /// </summary>
        public AnimationCurve mainScaleCurve;
        
        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public AnimationCurve directScaleCurve;

        /// <summary>
        /// 
        /// </summary>
        [HideInInspector]
        public AnimationCurve inverseScaleCurve;

        void Awake()
        {
            //* mainScaleCurve을 기반으로 한 정방향 (점점 커지는 방향) 커브
            directScaleCurve = new AnimationCurve(
                new Keyframe(0, 1),
                new Keyframe(1,  mainScaleCurve.Evaluate(1) / mainScaleCurve.Evaluate(0)),
                new Keyframe(2, mainScaleCurve.Evaluate(2) / mainScaleCurve.Evaluate(0))
            );

            //* mainScaleCurve을 기반으로 한 역방향 (점점 작아지는 방향) 커브
            inverseScaleCurve = new AnimationCurve(
                new Keyframe(0, 1),
                new Keyframe(1,  mainScaleCurve.Evaluate(0) / mainScaleCurve.Evaluate(1)),
                new Keyframe(2, mainScaleCurve.Evaluate(0) / mainScaleCurve.Evaluate(2))
            );
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="weight"></param>
        public void Spawn(int weight)
        {
            var spawnBB = Instantiate(Resources.Load<GameObject>("Pawn/Hero"), GameContext.Instance.playerCtrler.transform.position, Quaternion.identity).GetComponent<HeroBlackboard>();
        }
    
        /// <summary>
        /// 
        /// </summary>
        public int __editorSpawnWeight = 10;

    }

}