/*-------------------------------------------------------------------------------------------
// Copyright (C) 2019 北京，天龙互娱
//
// 模块名：OnMemoryAssetBundleManager
// 创建日期：2019-1-10
// 创建者：waicheng.wang
// 模块描述：AssetBundle包的缓存池
//-------------------------------------------------------------------------------------------*/
using System.Collections.Generic;
using UnityEngine;

namespace DragonU3DSDK.Asset
{
    public class OnMemoryAssetBundleManager
    {
        private readonly Dictionary<string, OnMemoryAssetBundle> onMemoryAssetbundleDictionary = new Dictionary<string, OnMemoryAssetBundle>();
        public void AddAssetBundle(AssetBundle assetBundle, AssetBundleInfo abInfo)
        {
            if (assetBundle != null)
            {
                var onMemoryAssetBundle = new OnMemoryAssetBundle(assetBundle);
                if (string.IsNullOrEmpty(abInfo.AssetBundleName))
                {
                    string newName = assetBundle.name.Replace('_', '/');
                    this.onMemoryAssetbundleDictionary.Add(newName, onMemoryAssetBundle);
                }
                else
                {
                    //this.onMemoryAssetbundleDictionary.Add(assetBundle.name, onMemoryAssetBundle);
                    this.onMemoryAssetbundleDictionary.Add(abInfo.AssetBundleName, onMemoryAssetBundle);
                }
            }
        }

        public AssetBundle GetAssetBundle(string assetBundleName)
        {
#if UNITY_STANDALONE_WIN
            assetBundleName = assetBundleName.Replace('\\', '/');
#endif
            OnMemoryAssetBundle onMemoryAssetBundle;
            if (!this.onMemoryAssetbundleDictionary.TryGetValue(assetBundleName, out onMemoryAssetBundle))
            {
                return null;
            }

            onMemoryAssetBundle.AddRef();
            return onMemoryAssetBundle.AssetBundle;
        }

        public bool CheckAssetBundleExists(string assetBundleName)
        {
#if UNITY_STANDALONE_WIN
            assetBundleName = assetBundleName.Replace('\\', '/');
#endif
            return this.onMemoryAssetbundleDictionary.ContainsKey(assetBundleName);
        }

        public string[] GetNoOnMemoryNames(params string[] assetBundleNames)
        {
            var noMemoryNames = new List<string>();
            foreach (var assetBundleName in assetBundleNames)
            {
                if (!this.CheckAssetBundleExists(assetBundleName))
                {
                    noMemoryNames.Add(assetBundleName);
                }
            }

            return noMemoryNames.ToArray();
        }

        public void Unload(string assetBundleName, bool force = false)
        {
#if UNITY_STANDALONE_WIN
            assetBundleName = assetBundleName.Replace('\\', '/');
#endif
            OnMemoryAssetBundle onMemoryAssetBundle;
            if (!this.onMemoryAssetbundleDictionary.TryGetValue(assetBundleName, out onMemoryAssetBundle))
            {
                return;
            }

            onMemoryAssetBundle.Unload();

            if (onMemoryAssetBundle.RefCount <= 0 || force)
            {
                onMemoryAssetBundle.AssetBundle.Unload(false);
                this.onMemoryAssetbundleDictionary.Remove(assetBundleName);
            }
        }
        public void Clear()
        {
            foreach (var item in this.onMemoryAssetbundleDictionary.Values)
            {
                if (item != null && item.AssetBundle != null)
                {
                    item.AssetBundle.Unload(false);
                }
            }
            this.onMemoryAssetbundleDictionary.Clear();
        }
    }
}
