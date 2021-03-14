/*-------------------------------------------------------------------------------------------
// Copyright (C) 2019 北京，天龙互娱
//
// 模块名：VersionItemInfo
// 创建日期：2019-1-9
// 创建者：waicheng.wang
// 模块描述：一组assetbundle包的描述结构
//-------------------------------------------------------------------------------------------*/
using System.Collections.Generic;

namespace DragonU3DSDK.Asset
{
    [System.Serializable]
    public class VersionItemInfo
    {
        public Dictionary<string, AssetBundleInfo> AssetBundles;

        public string Version;

        public bool UpdateWholeGroup;

        public VersionItemInfo()
        {
            AssetBundles = new Dictionary<string, AssetBundleInfo>();
        }

        public void Add(string key, AssetBundleInfo value)
        {
            if (this.AssetBundles.ContainsKey(key))
            {
                DebugUtil.Log("AssetBundle重名:" + key);
            }
            else
            {
                this.AssetBundles.Add(key, value);
            }
        }

        public void Refresh(Dictionary<string, AssetBundleInfo> remoteDict)
        {
            AssetBundles.Clear();
            foreach (KeyValuePair<string, AssetBundleInfo> kv in remoteDict)
            {
                AssetBundles.Add(kv.Key, kv.Value);
            }
        }

        public void Refresh(string key, AssetBundleInfo value)
        {
            AssetBundleInfo newABI = new AssetBundleInfo();
            newABI.AssetBundleName = value.AssetBundleName;
            newABI.DependenciesBundleNames = value.DependenciesBundleNames;
            newABI.HashString = value.HashString;
            newABI.Md5 = value.Md5;
            newABI.Crc = value.Crc;
            newABI.State = value.State;

            if (AssetBundles.ContainsKey(key))
            {
                AssetBundles[key] = newABI;
            }
            else
            {
                AssetBundles.Add(key, newABI);
            }
        }
    }
}
