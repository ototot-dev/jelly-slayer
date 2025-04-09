using UnityEngine;

namespace ototot.dev
{
    /// <summary>
    /// return a none Monobehaviour type singleton object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Singleton<T> where T : class, new()
    {

        public static T Instance
        {
            get
            {
                if (__instance == null)
                    __instance = new T();

                return __instance;
            }
        }

        static T __instance;
    }

    /// <summary>
    /// Returns a singleton object which is a child of MonoBehaviour class. (this pattern is based on 'http://wiki.unity3d.com/index.php/Singleton')
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {

        public static T Instance
        {
            get
            {
                if (__isAppQuitting)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                        "' already destroyed on application quit." +
                        " Won't create again - returning null.");

                    return null;
                }

                if (__instance == null)
                {
                    __instance = FindObjectOfType(typeof(T)) as T;

                    if (FindObjectsOfType(typeof(T)).Length > 1)
                    {
                        Debug.LogError("[Singleton] Something went really wrong " +
                            " - there should never be more than 1 singleton!" +
                            " Reopening the scene might fix it.");

                        return __instance;
                    }

                    if (__instance == null)
                    {
                        __instance = new GameObject().AddComponent<T>();
                        __instance.gameObject.name = "(singleton) " + typeof(T).ToString();

                        DontDestroyOnLoad(__instance.gameObject);

                        Debug.Log("[Singleton] An instance of " + typeof(T) +
                            " is needed in the scene, so '" + __instance.gameObject + "' was created with DontDestroyOnLoad.");
                    }
                    else
                    {
                        Debug.Log("[Singleton] Using instance already created: " + __instance.gameObject.name);
                    }
                }

                return __instance;
            }
        }

        static T __instance;

        void OnDestroy()
        {
            __isAppQuitting = true;
        }

        static bool __isAppQuitting = false;
    }
}