/*-------------------------------------------------------------------------------------------
// Copyright (C) 2019 北京，天龙互娱
//
// 模块名：DownloadingTask
// 创建日期：2019-2-13
// 创建者：waicheng.wang
// 模块描述：正在下载中的任务
//-------------------------------------------------------------------------------------------*/
using BestHTTP;

namespace DragonU3DSDK.Asset
{
    public class DownloadingTask
    {
        public DownloadingTask(DownloadInfo info)
        {
            this.DownloadInfo = info;
        }

        // 任务数据
        public DownloadInfo DownloadInfo { private set; get; }

        // 下载器
        public BreakPointDownloader Downloader;

        // http head
        public HTTPRequest HeadRequest;
    }
}
