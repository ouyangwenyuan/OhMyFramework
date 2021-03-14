/*-------------------------------------------------------------------------------------------
// Copyright (C) 2019 北京，天龙互娱
//
// 模块名：VersionInfo
// 创建日期：2019-1-9
// 创建者：waicheng.wang
// 模块描述：version文件的数据结构，用来描述所有assetbundle包
//-------------------------------------------------------------------------------------------*/
using System.Collections.Generic;

namespace DragonU3DSDK.Asset
{
    [System.Serializable]
    public class VersionInfo
    {
        public string Version;

        public Dictionary<string, VersionItemInfo> ResGroups;

        public string UniqueID;

        public VersionInfo()
        {
            ResGroups = new Dictionary<string, VersionItemInfo>();
        }

        public void Add(string key, VersionItemInfo value)
        {
            if (this.ResGroups.ContainsKey(key))
            {
                DebugUtil.Log("资源组重名:" + key);
            }
            else
            {
                this.ResGroups.Add(key, value);
            }
        }

        public List<string> GetAssetBundlesByKey(string key)
        {
            List<string> names = new List<string>();
            if (ResGroups.ContainsKey(key))
            {
                int count = ResGroups[key].AssetBundles.Count;
                foreach (string name in ResGroups[key].AssetBundles.Keys)
                {
                    names.Add(name);
                }
            }
            return names;
        }

        public string GetAssetBundleByKeyAndName(string key, string name)
        {
            string abname = string.Empty;


            if (ResGroups.ContainsKey(key) && !string.IsNullOrEmpty(name))
            {
                if (ResGroups[key].AssetBundles.ContainsKey(name))
                {
                    abname = ResGroups[key].AssetBundles[name].AssetBundleName;
                }
            }
            return abname;
        }

        public string GetAssetBundleHash(string key, string name)
        {
            string hash = "";
            if (ResGroups.ContainsKey(key) && !string.IsNullOrEmpty(name))
            {
                if (ResGroups[key].AssetBundles.ContainsKey(name))
                {
                    hash = ResGroups[key].AssetBundles[name].HashString;
                }
                else
                {
                    DebugUtil.LogError("GetAssetBundleHash error2:" + key + "->" + name);
                }
            }
            else
            {
                DebugUtil.LogError("GetAssetBundleHash error:" + key + "->" + name);
            }
            return hash;
        }

        public string GetAssetBundleMd5(string key, string name)
        {
            string md5 = "";
            if (ResGroups.ContainsKey(key) && !string.IsNullOrEmpty(name))
            {
                if (ResGroups[key].AssetBundles.ContainsKey(name))
                {
                    md5 = ResGroups[key].AssetBundles[name].Md5;
                }
                else
                {
                    DebugUtil.LogError("GetAssetBundleMd5 error2:" + key + "->" + name);
                }
            }
            else
            {
                DebugUtil.LogError("GetAssetBundleMd5 error:" + key + "->" + name);
            }
            return md5;
        }

        /*
        public AssetState GetAssetBundleState(string key, string name)
        {
            if (ResGroups.ContainsKey(key) && !string.IsNullOrEmpty(name))
            {
                if (ResGroups[key].AssetBundles.ContainsKey(name))
                {
                    return ResGroups[key].AssetBundles[name].State;
                }
                else
                {
                    DebugUtil.LogError("GetAssetBundleState error2:" + key + "->" + name);
                    return AssetState.NotExist;
                }
            }
            else
            {
                DebugUtil.LogError("GetAssetBundleState error:" + key + "->" + name);
                return AssetState.NotExist;
            }
        }
        */
    }
}
