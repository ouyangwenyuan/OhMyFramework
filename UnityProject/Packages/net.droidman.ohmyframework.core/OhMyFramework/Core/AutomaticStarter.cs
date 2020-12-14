using UnityEngine;

namespace OhMyFramework.Core
{
    public static class AutomaticStarter
    {
        /// <summary>
        /// Initialize the console service before the scene has loaded to catch more of the initialization log.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void OnLoadBeforeScene()
        {
            //检查设置是否开启
            if (!OmfSettings.Instance.IsEnabled)
            {
                OmfSettings.Instance.IsEnabled = true;
            }
        }

        /// <summary>
        /// Initialize SRDebugger after the scene has loaded.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void OnLoad()
        {
            if (OmfSettings.Instance.IsEnabled)
            {
                FrameworkEngine.Init();
            }
        }
    }
}