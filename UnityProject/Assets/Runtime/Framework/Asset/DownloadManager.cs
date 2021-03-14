/*-------------------------------------------------------------------------------------------
// Copyright (C) 2019 北京，天龙互娱
//
// 模块名：DownloadManager
// 创建日期：2019-1-11
// 创建者：waicheng.wang
// 模块描述：下载管理器
//-------------------------------------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using BestHTTP;
using UnityEngine.Networking;

namespace DragonU3DSDK.Asset
{
    public class  DownloadManager : Manager<DownloadManager>
    {
        // 最大并行下载个数
        private int maxDownloads = 4;

        // 等待下载的任务队列
        protected Queue<DownloadInfo> pendingDownloads = new Queue<DownloadInfo>();

        // 下载任务列表
        public Dictionary<string, DownloadingTask> m_DownloadingTasks = new Dictionary<string, DownloadingTask>();

        // 当前同时进行的加载数量
        protected int downloadThreads = 0;

        // 是否在进行中止任务
        private bool isAborting = false;

        public void SetMaxDownloadsCount(int count = 4)
        {
            this.maxDownloads = count;
            DebugUtil.Log("Set Max Downloads Count:{0}", count.ToString());
        }

        // 添加一个下载任务
        public DownloadInfo DownloadInSeconds(string filename, string md5, Action<DownloadInfo> onComplete)
        {
            filename = FilePathTools.NormalizePath(filename);
            DownloadInfo info = new DownloadInfo(filename, md5, onComplete);

            pendingDownloads.Enqueue(info);
            return info;
        }

        // 下载运营活动/礼包的资源
        public DownloadInfo DownloadInSeconds(string directoryName, string fileNameWithMd5, ActivityResType resType, Action<DownloadInfo> onComplete)
        {
            fileNameWithMd5 = FilePathTools.NormalizePath(fileNameWithMd5);
            DownloadInfo info = new DownloadInfo(directoryName, fileNameWithMd5, resType, onComplete);

            pendingDownloads.Enqueue(info);
            return info;
        }

        public DownloadInfo DownloadInSeconds(string directoryName, string filename, string md5, Action<DownloadInfo> onComplete , Action<int,int> onProgressChange = null)
        {
            filename = FilePathTools.NormalizePath(filename);
            DownloadInfo info = new DownloadInfo(directoryName, filename, md5, onComplete,onProgressChange);

            pendingDownloads.Enqueue(info);
            return info;
        }

        // 重试下载任务 Warning:只有下载器需要调用这个借口，业务层不要调用
        public void RetryDownload(DownloadInfo info)
        {
            if (info.retry > 0)
            {
                info.retry -= 1;
                pendingDownloads.Enqueue(info);
                m_DownloadingTasks.Remove(info.fileName);
                --downloadThreads;
            }
            else
            {
                FinishDownloadTask(info);
            }
        }


        private IEnumerator StopDownLoadTask()
        {
            AbortAllDownloadTask();
            yield return null;
        }

        public void StopAllDownloadTask()
        {
            StartCoroutine(StopDownLoadTask());
        }

        public void AbortDownloadTask (string fileName)
        {
            DownloadingTask task;
            if (this.m_DownloadingTasks.TryGetValue(fileName, out task))
            {
                AbortDownloadTask(task);
            }
        }

        public void AbortDownloadTask (DownloadingTask task)
        {
            if (task != null)
            {
                HTTPRequest headRequest = task.HeadRequest;
                if (headRequest != null)
                {
                    long headstarttime = DragonU3DSDK.Utils.TotalMilliseconds();
                    EventManager.Instance.Trigger<SDKEvents.DownloadFileEvent>().Data(task.DownloadInfo.fileName, "head_abort", 0, task.DownloadInfo.retry.ToString()).Trigger();
                    long headbistarttime = DragonU3DSDK.Utils.TotalMilliseconds();
                    headRequest.Abort();
                    long headaborttime = DragonU3DSDK.Utils.TotalMilliseconds();

                    //DebugUtil.LogError("Download Downloading loop {0} headBI is {1} headAbort is {2}", i, (headbistarttime - headstarttime), (headaborttime - headbistarttime));
                }
                if (task.Downloader != null)
                {
                    long taskstarttime = DragonU3DSDK.Utils.TotalMilliseconds();
                    EventManager.Instance.Trigger<SDKEvents.DownloadFileEvent>().Data(task.DownloadInfo.fileName, "abort", task.DownloadInfo.downloadedSize, task.DownloadInfo.retry.ToString()).Trigger();
                    long taskbistarttime = DragonU3DSDK.Utils.TotalMilliseconds();
                    task.Downloader.CancelDownload(false);
                    long tastaborttime = DragonU3DSDK.Utils.TotalMilliseconds();

                    //DebugUtil.LogError("Download Downloading loop {0} taskBI is {1} headAbort is {2}", i, (taskbistarttime - taskstarttime), (tastaborttime - taskbistarttime));
                }
                task.DownloadInfo.isFinished = true;
                task.DownloadInfo.retry = 0;//保证不会再被添加到下载队列里了
            }
        }

        // 中止所有下载任务
        public void AbortAllDownloadTask()
        {
            if (isAborting)
            {
                return;
            }

            isAborting = true;

            long totalstarttime = DragonU3DSDK.Utils.TotalMilliseconds();
            pendingDownloads.Clear();

            List<DownloadingTask> list = new List<DownloadingTask>(this.m_DownloadingTasks.Values);
            //DebugUtil.LogError("Download Downloading task num is " + list.Count);


            long starttime = DragonU3DSDK.Utils.TotalMilliseconds();
            for (int i = 0; i < list.Count; i++)
            {
                long loopstarttime = DragonU3DSDK.Utils.TotalMilliseconds();
                AbortDownloadTask(list[i]);
                long loopendtime = DragonU3DSDK.Utils.TotalMilliseconds();

                //DebugUtil.LogError("Download Downloading loop {0} time is {1}", i, (loopendtime - loopstarttime));
            }
            long endtime = DragonU3DSDK.Utils.TotalMilliseconds();

            //DebugUtil.LogError("Download Downloading loop time is " + (endtime - starttime));

            this.m_DownloadingTasks.Clear();
            pendingDownloads.Clear();
            downloadThreads = 0;
            long totalendtime = DragonU3DSDK.Utils.TotalMilliseconds();

            //DebugUtil.LogError("Download Downloading total time is " + (totalendtime - totalstarttime));
            isAborting = false;
        }

        void Update()
        {
            if (pendingDownloads.Count > 0)
            {
                int freeThreads = maxDownloads - downloadThreads;
                for (int i = 0; i < freeThreads; ++i)
                {
                    if (pendingDownloads.Count > 0)
                    {
                        DownloadInfo info = pendingDownloads.Dequeue();
                        StartTask(info);
                    }
                }
            }
        }

        // 开始一个下载任务
        private void StartTask(DownloadInfo taskinfo)
        {
            if (this.m_DownloadingTasks.ContainsKey(taskinfo.fileName))
            {
                DebugUtil.Log("task already in progress :{0}", taskinfo.fileName);
            }
            else
            {
                DownloadingTask value = new DownloadingTask(taskinfo);
                this.m_DownloadingTasks[taskinfo.fileName] = value;

                if (!File.Exists(taskinfo.tempPath))
                {
                    FilePathTools.CreateFolderByFilePath(taskinfo.tempPath);
                    File.Create(taskinfo.tempPath).Dispose();
                }

                using (var sw = new FileStream(taskinfo.tempPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    taskinfo.downloadedSize = (int)sw.Length;
                }

                HttpDownload(value);
            }
        }

        // 新建下载器开始下载
        private void HttpDownload(DownloadingTask task)
        {
            DebugUtil.Log("Start Download:{0}", task.DownloadInfo.fileName);

            DownloadingTask tmpTask = task;
            HTTPRequest httprequest = new HTTPRequest(new Uri(tmpTask.DownloadInfo.url), HTTPMethods.Head, (req, rep) =>
            {
                switch (req.State)
                {
                    case HTTPRequestStates.ConnectionTimedOut:
                    case HTTPRequestStates.TimedOut:
                        EventManager.Instance.Trigger<SDKEvents.DownloadFileEvent>().Data(tmpTask.DownloadInfo.fileName, "head_timeout", 0, tmpTask.DownloadInfo.retry.ToString()).Trigger();
                        break;
                    case HTTPRequestStates.Aborted:
                        EventManager.Instance.Trigger<SDKEvents.DownloadFileEvent>().Data(tmpTask.DownloadInfo.fileName, "head_abort", 0, tmpTask.DownloadInfo.retry.ToString()).Trigger();
                        break;
                    case HTTPRequestStates.Error:
                        EventManager.Instance.Trigger<SDKEvents.DownloadFileEvent>().Data(tmpTask.DownloadInfo.fileName, "head_failure", 0, tmpTask.DownloadInfo.retry.ToString()).Trigger();
                        break;
                    case HTTPRequestStates.Finished:
                        if (rep != null && rep.StatusCode >= 200 && rep.StatusCode < 400)
                        {
                            EventManager.Instance.Trigger<SDKEvents.DownloadFileEvent>().Data(tmpTask.DownloadInfo.fileName, "head_finish", 0, tmpTask.DownloadInfo.retry.ToString()).Trigger();
                        }
                        else
                        {
                            EventManager.Instance.Trigger<SDKEvents.DownloadFileEvent>().Data(tmpTask.DownloadInfo.fileName, "head_failure", 0, tmpTask.DownloadInfo.retry.ToString()).Trigger();
                        }
                        break;
                }

                if (!this.m_DownloadingTasks.ContainsKey(tmpTask.DownloadInfo.fileName))
                {
                    DebugUtil.Log("Cancelled Task :{0}", tmpTask.DownloadInfo.fileName);
                    return;
                }

                tmpTask.HeadRequest = null;
                if (rep == null)
                {
                    DebugUtil.LogError("Download failed due to network for :{0}", tmpTask.DownloadInfo.fileName);
                    tmpTask.DownloadInfo.result = DownloadResult.ServerUnreachable;
                    RetryDownload(tmpTask.DownloadInfo);
                }
                else if (rep.StatusCode == 200 || rep.StatusCode == 206)
                {
                    try
                    {
                        string firstHeaderValue = rep.GetFirstHeaderValue("Content-Length");
                        tmpTask.DownloadInfo.downloadSize = int.Parse(firstHeaderValue);
                        DebugUtil.Log("Will download {0} bytes for  '{1}'", tmpTask.DownloadInfo.downloadSize, tmpTask.DownloadInfo.fileName);
                        BreakPointDownloader downloader = new BreakPointDownloader(tmpTask.DownloadInfo, this.m_DownloadingTasks);
                        tmpTask.Downloader = downloader;
                        downloader.StartDownload();
                    }
                    catch (Exception ex)
                    {
                        DebugUtil.LogError("An error occured during download '{0}' due to {1}", tmpTask.DownloadInfo.fileName, ex);
                        tmpTask.DownloadInfo.result = DownloadResult.Failed;
                        RetryDownload(tmpTask.DownloadInfo);
                    }
                }
                else
                {
                    DebugUtil.Log("statecode = {0}.Response is not ok! for: {1}", rep.StatusCode, tmpTask.DownloadInfo.url);
                    tmpTask.DownloadInfo.result = DownloadResult.Failed;
                    RetryDownload(tmpTask.DownloadInfo);
                }
            })
            {
                DisableCache = true
            };
            this.m_DownloadingTasks[tmpTask.DownloadInfo.fileName].HeadRequest = httprequest;
            EventManager.Instance.Trigger<SDKEvents.DownloadFileEvent>().Data(tmpTask.DownloadInfo.fileName, "head_start", 0, tmpTask.DownloadInfo.retry.ToString()).Trigger();
            httprequest.Send();
        }

        // 结束一个下载任务，归还并发数
        public void FinishDownloadTask(DownloadInfo info)
        {
            info.isFinished = true;

            if (info.onComplete != null)
                info.onComplete(info);

            m_DownloadingTasks.Remove(info.fileName);
            --downloadThreads;
        }

        #region 拷贝AB包

        public void CopyBundleToDownloadFolder(string sourceFolderName, string desFolderName, List<string> fileNames, Action onComplete, Action<Exception> onError = null, bool clearDir = true)
        {
            int totalCount = fileNames.Count;
            int hasCopyCount = 0;

            DebugUtil.Log("desFolderName--------->" + desFolderName);
            if (clearDir && Directory.Exists(desFolderName))
            {
                DebugUtil.Log("desFolderName--------存在->" + desFolderName);
                DirectoryInfo dir = new DirectoryInfo(desFolderName);
                dir.Empty();
            }

            for (int k = 0; k < totalCount; k++)
            {
                string fileName = fileNames[k];
                string filePath = string.Format("{0}/{1}.ab", sourceFolderName, fileName);
                DebugUtil.Log("开始拷贝：" + filePath);

                StartCoroutine(SubCopy(fileName, filePath, desFolderName, () =>
                {
                    hasCopyCount += 1;
                    if (hasCopyCount >= totalCount)
                    {
                        if (onComplete != null)
                            onComplete();
                    }
                }, onError));
            }
        }


        private IEnumerator SubCopy(string fileName, string filePath, string desFolderName, Action onComlete, Action<Exception> onError = null)
        {
            using (UnityWebRequest www = UnityWebRequest.Get(filePath))
            {

                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    DebugUtil.Log(www.url + " copy error:" + www.error);
                    onError?.Invoke(new Exception(www.error));
                    yield break;
                }


                byte[] b = www.downloadHandler.data;
                string destFile = string.Format("{0}/{1}.ab", desFolderName, fileName);
                string destDir = Path.GetDirectoryName(destFile);
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                if (File.Exists(destFile))
                {
                    File.Delete(destFile);
                }

                try
                {
                    using (FileStream fs = new FileStream(destFile, FileMode.Create))
                    {
                        fs.Seek(0, SeekOrigin.Begin);
                        fs.Write(b, 0, b.Length);
                        fs.Close();
                    }
                }
                catch (Exception e)
                {// "Disk Full" 错误处理
                    onError?.Invoke(e);
                    yield break;
                }

                onComlete?.Invoke();
            }
        }

        #endregion
        
        #region 资源下载优化

        public Dictionary<string, string> FiterUpdateFiles(Dictionary<string, string> srcDict)
        {
            Dictionary<string, string> destDict = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> kv in srcDict)
            {
                string localFilePath = string.Format("{0}/{1}", FilePathTools.persistentDataPath_Platform, kv.Key);
                string localFileMd5 = AssetUtils.BuildFileMd5(localFilePath);
                if (localFileMd5 == null || (localFileMd5.Trim() != kv.Value.Trim()))
                {
                    destDict.Add(kv.Key, kv.Value);
                }
            }
            return destDict;
        }

        #endregion
    }
}