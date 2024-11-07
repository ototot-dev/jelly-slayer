#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace Assets.Rx
{
    /// <summary>
    /// AssetBundle build window.
    /// </summary>
    public class BundleBuildWindow : EditorWindow
    {

        static int DefaultWidth
        {
            get { return 400; }
        }

        static int DefaultHeight
        {
            get { return 200; }
        }

        static BuildTarget __targetPlatform;

        static string __outputPath;

        public static void Init(BuildTarget targetPlatform, string outputPath)
        {
            __targetPlatform = targetPlatform;
            __outputPath = outputPath;

            BundleBuilder.Init(__targetPlatform, __outputPath);

            var window = CreateInstance<BundleBuildWindow>();
            window.ShowUtility();

            window.position = new Rect(
                Screen.currentResolution.width / 2,
                Screen.currentResolution.height / 2,
                DefaultWidth,
                DefaultHeight
                );
        }

        void Update()
        {
            var ret = BundleBuilder.Process();

            if (ret == 0)
                return;

            var window = GetWindow(typeof(BundleBuildWindow));

            if (window != null)
                window.Close();

            if (ret == 1)
            {
                BuildPipeline.BuildAssetBundles(__outputPath, BuildAssetBundleOptions.None, __targetPlatform);
                Debug.LogFormat("Building AssetBundle for {0} completed.", __targetPlatform.ToString());
            }
            else
            {
                Debug.LogFormat("Building AssetBundle is canceled.");
            }
        }

        void OnGUI()
        {
            var progress = BundleBuilder.processedAssetNum / (float)BundleBuilder.totalAssetNum;

            EditorGUI.LabelField(new Rect(3, 5, position.width - 6, 20), "Assign Asset to AssetBundle");
            EditorGUI.ProgressBar(new Rect(3, 45, position.width - 6, 20), progress, string.Format("{0}/{1} - {2:f2}% processed~", BundleBuilder.processedAssetNum, BundleBuilder.totalAssetNum, progress * 100f));

            if (GUI.Button(new Rect(position.width - 56, 75, 50, 20), "Cancel"))
                BundleBuilder.isCancelPending = true;
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }

    }
}
#endif