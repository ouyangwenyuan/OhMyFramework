using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace OhMyFramework.Core
{
    public enum GameSortingLayer {
        War_Terrain = -2,
        War_Shadow = -1,
        Default = 0,
        War_Unit = 1,
        Background,
        Background1,
        UI,
        UI1,
        Foreground,
        Foreground1,
        Guide,
        Guide1,
        Avatar,
        Avatar1,
        Effect,
        Effect1,
        Text,
        Debug
    }
    public enum CommonPath
    {
        Prefabs,
        FxPrefabs,
        Sounds,
        Materials,
        Textures,
        UiSprites,
        Animations,
        BuildTools,
        GenCode,
        Configs,
        Download,
        Shader
    }
    /// <summary>
    /// setting in unity editor 
    /// </summary>
    public class OmfSettings : ScriptableObject
    {
//        [Serializable]
//        public sealed class UserPathData
//        {
//            [SerializeField] public List<string> mOnToggles;
//            [SerializeField] public Dictionary<string, string> commonPaths; 
//            [SerializeField] public string dataPath;
//            [SerializeField] public string persistentDataPath;
//            [SerializeField] public string streamingAssetsPath;
//            [SerializeField] public string consoleLogPath;
//            [SerializeField] public string temporaryCachePath;
//            [SerializeField] public bool mUseDefault;
//        }
        private const string ResourcesName = "OmfSettings";
        private static OmfSettings _instance;
        public static OmfSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GetOrCreateInstance();
                }
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
        public readonly string CurrentRootPath = System.IO.Path.GetFullPath(".");
        public string GEN_CODE_PATH = "Runtime/Games";

        //自定义layer 名字
        public  string[] customLayers = { "War_Terrain", "War_Unit", "War_Obstacle", "Background", "Background1", "UI1", "UI2", "Foreground", "Foreground1", "Guide", "Guide1", "Avatar", "Avatar1", "Effect", "Effect1", "Text", "Debug", "Environments", "Buildings", "Enemies", "Teammates", "Characters", "Protagonists", "Players" };
        //自定义tag
        public  string[] customTags = {"Except","Export","Environments","Buildings","Enemies","Teammates","Cameras","Characters","Lights","Protagonists","Players", "UICamera", "War_Terrain", "War_Unit" };
     
        #endregion
        private static OmfSettings GetOrCreateInstance()
        {
            var instance = Resources.Load<OmfSettings>("Settings/" + ResourcesName);

            if (instance == null)
            {
                // Create instance
                instance = CreateInstance<OmfSettings>();
                instance.SetDefaultSettings();
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
        private void SetDefaultSettings()
        {
//            mUserPathData = new UserPathData()
//            {
//                mOnToggles = new List<string>(),
//                commonPaths = new Dictionary<string, string>(),
//
//                dataPath = Application.dataPath,
//                persistentDataPath = Application.persistentDataPath,
//                streamingAssetsPath = Application.streamingAssetsPath,
//                consoleLogPath = Application.consoleLogPath,
//                temporaryCachePath = Application.temporaryCachePath,
//                mUseDefault = true
//            };
//            mUserPathData.mOnToggles = Enum.GetNames(typeof(CommonPath)).ToList();
//            foreach (var t in mUserPathData.mOnToggles)
//            {
//                mUserPathData.commonPaths.Add(t,"Assets/"+t);
//            }
        }
        
    }
}