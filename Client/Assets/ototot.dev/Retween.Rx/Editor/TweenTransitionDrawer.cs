#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Retween.Rx
{
    /// <summary>
    /// 
    /// </summary>
    [CustomPropertyDrawer(typeof(TweenAnim.Transition))]
    public class TweenAnimTransitionDrawer : PropertyDrawer
    {

        /// <summary>
        /// 
        /// </summary>
        readonly float __lineHeight = 18;

        /// <summary>
        /// 
        /// </summary>
        readonly float __animCurveHeight = 50;

        /// <summary>
        /// 
        /// </summary>    
        readonly int __easingCurveSampleNum = 36;

        /// <summary>
        /// 
        /// </summary>
        float __propertyHeight = 0;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var transition = fieldInfo.GetValue(property.serializedObject.targetObject) as TweenAnim.Transition;

            position.height = __lineHeight;
            __propertyHeight = 0f;

            if (!transition.isRollback)
            {
                var loopProperty = property.FindPropertyRelative("loop");

                EditorGUI.PropertyField(position, loopProperty, new GUIContent("Loop"));

                position.y += __lineHeight;
                __propertyHeight += __lineHeight;
            }

            var easingProperty = property.FindPropertyRelative("easing");

            EditorGUI.PropertyField(position, easingProperty, new GUIContent("Easing"));

            position.y += __lineHeight;
            __propertyHeight += __lineHeight;

            if (transition.easing != TweenFunc.Easings.Linear)
            {
                var easingCurveProperty = property.FindPropertyRelative("easingCurve");
                var animCurve = new AnimationCurve();

                for (int i = 0; i <= __easingCurveSampleNum; ++i)
                {
                    var t = (float)i / __easingCurveSampleNum;

                    if (!transition.isRollback)
                        animCurve.AddKey(t, TweenFunc.GetValue(transition.easing, 0f, 1f, t));
                    else
                        animCurve.AddKey(t, TweenFunc.GetValue(transition.easing, 1f, 0f, t));
                }

                easingCurveProperty.animationCurveValue = animCurve;

                position.height = __animCurveHeight - 3;

                EditorGUI.PropertyField(position, easingCurveProperty, new GUIContent(" "));

                position.y += __animCurveHeight;
                __propertyHeight += __animCurveHeight;

                position.height = __lineHeight;
            }

            var durationProperty = property.FindPropertyRelative("duration");

            EditorGUI.PropertyField(position, durationProperty, new GUIContent("Duration"));

            position.y += __lineHeight;
            __propertyHeight += __lineHeight;

            EditorGUI.EndProperty();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="label"></param>
        /// <returns></returns>dlf
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (__propertyHeight <= 0)
                return base.GetPropertyHeight(property, label);
            else
                return __propertyHeight;
        }

    }
}
#endif