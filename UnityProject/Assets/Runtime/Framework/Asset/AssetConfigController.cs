using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace DragonU3DSDK.Asset
{
    [Serializable]
    public class BundleState
    {
        // ab包路径
        public string Path;
        // 是否在初始包里
        public bool InInitialPacket = false;
    }

    [Serializable]
    public class BundleGroup
    {
        public string GroupName;
        public string Version;
        public int GroupIndex;
        public List<BundleState> Paths;
        public bool UpdateWholeAB;

        public List<string> GetBundlePaths()
        {
            List<string> _paths = new List<string>();
            foreach (BundleState item in Paths)
            {
                _paths.Add(item.Path);
            }
            return _paths;
        }

        public bool IsInInitialPacket(string path)
        {
            string partPath = path.Substring(0, path.Length - 3);
            foreach (BundleState item in Paths)
            {
                if (item.Path.ToLower() == partPath)
                    return item.InInitialPacket;
            }
            return false;
        }

        public List<string> GetInInitialPacketPaths()
        {
            List<string> _paths = new List<string>();
            foreach (BundleState item in Paths)
            {
                if (item.InInitialPacket)
                    _paths.Add(item.Path.ToLower());
            }
            return _paths;
        }
    }

    [Serializable]
    public class PretreatmentStreamDelegate : ScriptableObject
    {
        public virtual void ExecutePretreatment () 
        {
            Debug.LogError("PretreatmentStreamDelegate.ExecutePretreatment");
        }
    }

    public class AssetConfigController : ScriptableObject
    {
        public static string AssetConfigPath = "Settings/AssetConfigController";
        private static AssetConfigController _instance = null;

        public static AssetConfigController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<AssetConfigController>(AssetConfigPath);
                }
#if UNITY_EDITOR
                // 如果存在AssetConfigPrototype(可配置正则表达式)，将用AssetConfigPrototype里的数据修改AssetConfigController
                if (AssetConfigPrototype.Instance && !Application.isPlaying)
                {
                    AssetConfigPrototype.Instance.Modify(_instance);
                    UnityEditor.EditorUtility.SetDirty(_instance);
                }
#endif
                return _instance;
            }
        }

        [Space(10)]
        [Header(" --------------------- 资源版本号 ----------------------")]
        public string RootVersion = "v1_0_0";
        public string IOSRootVersion = "v1_0_0";

        [Space(10)]
        [Header(" ------------------ bundleVersionCode ----------------")]
        public string VersionCode = "1";
        public string IOSVersionCode = "1";

        [Space(10)]
        [Header("[true:使用AB包  false:使用编辑器里资源   真机下应为true   ]")]
        [Header("------------------ UseAssetBundle--------------------")]
        public bool UseAssetBundle = false;

        [Space(10)]
        [Header("[true:拷贝AB包到下载目录 false:从服务器下载  真机下应为false]")]
        [Header(" -------------- CopyBundleToDownloadPath--------------")]
        public bool CopyBundleToDownloadPath = false;

        [Space(10)]
        [Header("[填入相对Assets/Export的相对路径，大小写敏感，只支持文件夹]")]
        [Header(" ---------------------- AB包分组 -----------------------")]
        public BundleGroup[] Groups;

        
        [Space(5)]
        [Header("----------------------活动资源路径-----------------------")]
        public string[] ActivityResPaths;

        [Space(5)]
        [Header("-------------------项目打包预处理的代理-------------------")]
        public PretreatmentStreamDelegate ProjectPretreatmentDelegate = null;

        [Space(5)]
        [Header("-------------------增强版加密-------------------")]
        public string EnhancedEncryptionSecret = "";

        public bool LocalMode()
        {
            return CopyBundleToDownloadPath;
        }

        public bool BundleMode()
        {
            if (!CopyBundleToDownloadPath && !UseAssetBundle)
            {
                return true;
            }
            else
            {
                return UseAssetBundle;
            }
        }
        
        public bool IsPathsNoRepetition()
        {
            HashSet<string> temp = new HashSet<string>();
            int cnt = 0;
            foreach (var g in Groups)
            {
                cnt += g.Paths.Count;
                foreach (var p in g.Paths)
                {
                    temp.Add(p.Path);
                }
            }
            return temp.Count == cnt;
        }
    }
}
