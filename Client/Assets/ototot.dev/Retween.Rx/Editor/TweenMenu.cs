#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Retween.Rx
{
    public class TweenMenu : MonoBehaviour
    {
        [MenuItem("GameObject/Retween.Rx/TweenAnim", false, 10)]
        static void CreateTweenAnim(MenuCommand menuCommand)
        {
            var tweenAnim = new GameObject(".~:-").AddComponent<TweenAnim>();

            tweenAnim.gameObject.AddComponent<TweenName>();

            var parentObj = menuCommand.context as GameObject;

            if (parentObj != null && parentObj.GetComponent<Animation>() != null)
            {
                foreach (AnimationState s in parentObj.GetComponent<Animation>())
                    tweenAnim.animClips.Add(s.clip);
            }

            GameObjectUtility.SetParentAndAlign(tweenAnim.gameObject, parentObj);
            Undo.RegisterCreatedObjectUndo(tweenAnim.gameObject, "Create " + tweenAnim.gameObject.name);

            Selection.activeObject = tweenAnim.gameObject;

            tweenAnim.gameObject.SetActive(false);
        }

    }
}
#endif