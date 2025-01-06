using UnityEngine;

namespace Game
{
    // ScriptableObject를 상속받는 클래스 정의
    [CreateAssetMenu(fileName = "NewPawnRootMotionCurveData", menuName = "Game/ScriptableObjects/CurvPawnRootMotionCurveData", order = 1)]
    public class PawnRootMotionCurveData : ScriptableObject
    {
        public AnimationCurve curvePositionX;
        public AnimationCurve curvePositionY;
        public AnimationCurve curvePositionZ;

        public Vector3 EvaluatePosition(float time)
        {
            var x = curvePositionX?.Evaluate(time) ?? 0;
            var y = curvePositionY?.Evaluate(time) ?? 0;
            var z = curvePositionZ?.Evaluate(time) ?? 0;
            return new Vector3(x, y, z);
        }
    }
}