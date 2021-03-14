/*-------------------------------------------------------------------------------------------
// Copyright (C) 2019 北京，天龙互娱
//
// 模块名：OnMemoryAssetBundle
// 创建日期：2019-1-10
// 创建者：waicheng.wang
// 模块描述：AssetBundle包在内存里的缓存情况
//-------------------------------------------------------------------------------------------*/
using UnityEngine;

namespace DragonU3DSDK.Asset
{
    public class OnMemoryAssetBundle
    {
        public string AssetBundleName { get; private set; }

        public AssetBundle AssetBundle { get; private set; }

        public int RefCount { get; private set; }

        public OnMemoryAssetBundle(AssetBundle assetBundle)
        {
            this.AssetBundleName = assetBundle.name;
            this.AssetBundle = assetBundle;
            this.RefCount = 0;
        }

        public void AddRef()
        {
            this.RefCount++;
        }

        public void Unload()
        {
            this.RefCount--;
        }
    }
}