/*-------------------------------------------------------------------------------------------
// Copyright (C) 2019 北京，天龙互娱
//
// 模块名：AssetBundleInfo
// 创建日期：2019-1-10
// 创建者：waicheng.wang
// 模块描述：单个ab包的描述
//-------------------------------------------------------------------------------------------*/
namespace DragonU3DSDK.Asset
{
    [System.Serializable]
    public enum AssetState
    {
        NotExist,           //不存在
        ExistInDownLoad,    //在下载目录
    }

    [System.Serializable]
    public struct AssetBundleInfo
    {
        // ab包名
        public string AssetBundleName;

        // 依赖列表
        public string[] DependenciesBundleNames;

        // 作为增量id
        public string HashString;

        // 通过文件数据计算的MD5码
        // 作为下载校验码
        public string Md5;

        // 校验值
        public uint Crc;

        //资源状态
        public AssetState State;
    }
}
