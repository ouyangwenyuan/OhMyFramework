/*-------------------------------------------------------------------------------------------
// Copyright (C) 2019 北京，天龙互娱
//
// 模块名：DownloadInfo
// 创建日期：2019-1-11
// 创建者：waicheng.wang
// 模块描述：单个下载任务的描述
//-------------------------------------------------------------------------------------------*/
using System;
using System.IO;

namespace DragonU3DSDK.Asset
{
    public enum ActivityResType
    {
        Non = -1,
        Activity,//运营活动
        Bundle//打折礼包
    }

    public enum DownloadResult
    {
        Unknown = -1,
        Success,
        Failed,
        TimeOut,
        ServerUnreachable,
        Md5Error,
        ForceAbort
    }

    public class DownloadInfo
    {
        // 文件名
        public string fileName = "";

        // 文件md5
        public string fileMd5 = "";

        // 下载地址
        public string url = "";

        // 下载成功后的保存路径
        public string savePath = "";

        // 断点文件的保存路径
        public string tempPath = "";

        // 超时时间
        public float timeout = 20.0f;

        // 需下载大小(字节数)
        public int downloadSize;

        // 已下载大小(字节数)
        public int downloadedSize;

        // 处理下载结果的回调
        public Action<DownloadInfo> onComplete = null;

        // 处理下载进度回调
        public Action<int, int> onProgressChange = null;

        // 重试次数
        public int retry = 3;

        // 当前下载的进度
        public float currProgress = 0f;

        // 是否下载结束
        public bool isFinished = false;

        // 下载结果
        public DownloadResult result = DownloadResult.Unknown;

        public DownloadInfo(string _filename, string _md5, Action<DownloadInfo> _callback)
        {
            fileName = _filename;
            fileMd5 = _md5;
            onComplete = _callback;
            //------- home包的url换成公共库的 ---------//
            // if (HomeRoomConfigController.Instance != null)
            // {
            //     var roomResInfo = HomeRoomConfigController.Instance.GetRoomResInfoByABPath(fileName);
            //     if (roomResInfo != null)
            //     {
            //         url = roomResInfo.GetDownLoadUrl();
            //     }
            // }
            
            // //------- cooking包的url换成公共库的 ---------//
            // if (CookingGameConfigController.Instance != null)
            // {
            //     var roomResInfo = CookingGameConfigController.Instance.GetRoomResInfoByABPath(fileName);
            //     if (roomResInfo != null)
            //     {
            //         url = roomResInfo.GetDownLoadUrl();
            //     }
            // }

            // //------- color包的url换成公共库的 ---------//
            // if (ColorResConfigController.Instance != null)
            // {
            //     var colorResInfo = ColorResConfigController.Instance.GetRoomResInfoByABPath(fileName);
            //     if (colorResInfo != null)
            //     {
            //         url = colorResInfo.GetDownLoadUrl();
            //     }
            // }
            
            if (string.IsNullOrEmpty(url))
            {
                url = string.Format("{0}/{1}/{2}", FilePathTools.AssetBundleDownloadPath, VersionManager.Instance.GetResRootVersion(), fileName);
            }
            savePath = string.Format("{0}/{1}", FilePathTools.persistentDataPath_Platform, fileName);
            tempPath = string.Format("{0}/{1}.temp", FilePathTools.persistentDataPath_Platform, fileName);
        }

        public DownloadInfo(string directoryName, string _filename, string _md5, Action<DownloadInfo> _callback, Action<int, int> _onProgressChange = null)
        {
            fileName = _filename;
            fileMd5 = _md5;
            onComplete = _callback;
            onProgressChange = _onProgressChange;
            url = Path.Combine(FilePathTools.AssetBundleDownloadPath, directoryName, fileName);
            savePath = string.Format("{0}/{1}", FilePathTools.persistentDataPath_Platform, fileName);
            tempPath = string.Format("{0}/{1}.temp", FilePathTools.persistentDataPath_Platform, fileName);
        }

        public void OnProgressChange()
        {
            onProgressChange?.Invoke(downloadedSize, downloadSize);
        }

        // 运营活动资源
        public DownloadInfo(string directoryName, string fileNameWithMd5, ActivityResType resType, Action<DownloadInfo> _callback)
        {
            string fileNameWithNoSuffix = fileNameWithMd5.Substring(0, fileNameWithMd5.Length - 3);//移除".ab"
            string[] subPaths = fileNameWithNoSuffix.Split('_');

            //------------------ 拼接fileName
            for (int i = 0; i < subPaths.Length - 2; i++)
            {
                if (i > 0) fileName += "/";
                fileName += subPaths[i];
            }
            fileName += ".ab";

            //------------------ 提取md5
            fileMd5 = subPaths[subPaths.Length - 1];

            onComplete = _callback;
            if (resType == ActivityResType.Activity)
                url = string.Format("{0}/activities/{1}/{2}", FilePathTools.AssetBundleDownloadPath, directoryName, fileNameWithMd5);
            else if (resType == ActivityResType.Bundle)
                url = string.Format("{0}/bundles/{1}/{2}", FilePathTools.AssetBundleDownloadPath, directoryName, fileNameWithMd5);
            else
                DebugUtil.LogError("DownloadInfo Error:" + resType);
            savePath = string.Format("{0}/{1}", FilePathTools.persistentDataPath_Platform, fileName);
            tempPath = string.Format("{0}/{1}.temp", FilePathTools.persistentDataPath_Platform, fileName);
        }
    }
}
