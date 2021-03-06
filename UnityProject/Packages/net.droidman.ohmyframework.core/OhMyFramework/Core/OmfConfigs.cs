using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;
#endif
namespace OhMyFramework.Core
{
    /// <summary>
    /// setting in code 
    /// </summary>
    [UnityEngine.CreateAssetMenu(fileName = "OmfConfigs", menuName = "OmfConfigs", order = 0)]
    public class OmfConfigs : UnityEngine.ScriptableObject
    {
        private const string ResourcesName = "OmfConfigs";
        private static OmfConfigs _instance;
        public static OmfConfigs Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GetOrCreateInstance();
                }
        
//                if (_instance._keyboardShortcuts != null && _instance._keyboardShortcuts.Length > 0)
//                {
//                    _instance.UpgradeKeyboardShortcuts();
//                }
        
                return _instance;
            }
        }
        
        #region Serialization

        [SerializeField] private bool _isEnabled = true;
        #endregion
        
        #region Settings

        public bool IsEnabled
        {
            get { return _isEnabled; }
#if UNITY_EDITOR
            set { _isEnabled = value; }
#endif
        }
        public string ResUrl;
        #endregion
        private static OmfConfigs GetOrCreateInstance()
        {
            var instance = Resources.Load<OmfConfigs>("Settings/" + ResourcesName);

            if (instance == null)
            {
                // Create instance
                instance = CreateInstance<OmfConfigs>();

#if UNITY_EDITOR

                // Get resources folder path
                var resourcesPath = Application.dataPath + "/Resources/Settings"; //GetResourcesPath();
//                Debug.Log("[SRDebugger] Creating settings asset at {0}/{1}".Fmt(resourcesPath, ResourcesName));
                var path = resourcesPath + "/" + ResourcesName + ".asset"; //绝对路径
                if (!File.Exists(path))
                {
                    // Create directory if it doesn't exist
                    Directory.CreateDirectory(resourcesPath);
                    // Save instance if in editor //assetdatbase 的路径以Assets开头的相对路径
                    AssetDatabase.CreateAsset(instance, "Assets/Resources/Settings/" + ResourcesName + ".asset");
                }
#endif
            }

            return instance;
        }
    }
}