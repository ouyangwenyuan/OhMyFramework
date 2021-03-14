using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using System;
using BestHTTP;
using System.Net;

namespace DragonU3DSDK.Asset
{
    public static partial class BuildAssetBundles
    {
        public static void CommonResLog(string format)
        {
            DebugUtil.Log(format);
            //Console.WriteLine(format);
        }

        public static void CommonResLog(string format, object arg1)
        {
            string str = string.Format(format, arg1);
            CommonResLog(str);
        }

        public static void CommonResLog(string format, object arg1, object arg2)
        {
            string str = string.Format(format, arg1, arg2);
            CommonResLog(str);
        }

        [MenuItem("AssetBundle/打包所有资源CompressMax")]
        public static void BuildAllAssetBundleWithCompressMax()
        {
            BuildAllAssetBundleWithOptions(BuildAssetBundleOptions.None | BuildAssetBundleOptions.DeterministicAssetBundle);
        }

        [MenuItem("AssetBundle/清除本地所有ab资源")]
        public static void ClearAB()
        {
#if UNITY_EDITOR
            string abOutPath = Application.dataPath + "/AssetBundleOut";
            if (Directory.Exists(abOutPath))
            {
                foreach (string dir in Directory.GetDirectories(abOutPath))
                {
                    Directory.Delete(dir, true);
                }

                foreach (string file in Directory.GetFiles(abOutPath))
                {
                    File.Delete(file);
                }
            }

            string saPath = Application.streamingAssetsPath;
            if (Directory.Exists(saPath))
            {
                foreach (string dir in Directory.GetDirectories(saPath))
                {
                    Directory.Delete(dir, true);
                }

                foreach (string file in Directory.GetFiles(saPath))
                {
                    File.Delete(file);
                }
            }

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Info", "成功清除AssetBundleOut与StreamingAssets，请重新打包", "ok");
#else
            EditorUtility.DisplayDialog("Info", "只支持editor", "ok");
#endif
        }

        [MenuItem("AssetBundle/清除本地download下资源")]
        public static void ClearDowload()
        {
#if UNITY_EDITOR
            string downloadPath = FilePathTools.downLoadPath;
            if (Directory.Exists(downloadPath))
            {
                foreach (string dir in Directory.GetDirectories(downloadPath))
                {
                    Directory.Delete(dir, true);
                }

                foreach (string file in Directory.GetFiles(downloadPath))
                {
                    File.Delete(file);
                }

                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("Info", "成功清除download下资源", "ok");
            }
            else
            {
                EditorUtility.DisplayDialog("Info", "文件夹不存在", "ok");
            }
#else
            EditorUtility.DisplayDialog("Info", "只支持editor", "ok");
#endif
        }

        [MenuItem("AssetBundle/打开本地download文件夹")]
        public static void OpenDowload()
        {
#if UNITY_EDITOR
            string downloadPath = FilePathTools.downLoadPath;
            if (Directory.Exists(downloadPath))
            {
                Process.Start(downloadPath);
            }
            else
            {
                EditorUtility.DisplayDialog("Info", "文件夹不存在", "ok");
            }
#else
            EditorUtility.DisplayDialog("Info", "只支持editor", "ok");
#endif
        }

        [MenuItem("AssetBundle/HomeRoom/Help/下载初始ab到StreamingAssets", false, 4)]
        public static void DownloadInitial()
        {
            foreach (RoomResFakeInfo info in HomeRoomConfigController.Instance.FakeVersionInfos)
            {
                if (info.InInitialPacket)
                {
                    //DownloadHomeInitialABWithBreakDownloader(info);
                    DownloadCommonInitialABWithHttpRequest(info);
                }
            }
        }

        static void DownloadCommonInitialABWithBreakDownloader(BaseResFakeInfo rrInfo)
        {
            DownloadInfo taskinfo = new DownloadInfo(rrInfo.LocalABPath, rrInfo.GetPlatformMd5(), null);
            taskinfo.savePath = FilePathTools.streamingAssetsPath_Platform + "/" + rrInfo.LocalABPath.ToLower();
            if (File.Exists(taskinfo.savePath))
            {
                File.Delete(taskinfo.savePath);
            }

            FilePathTools.CreateFolderByFilePath(taskinfo.savePath);

            if (!File.Exists(taskinfo.tempPath))
            {
                FilePathTools.CreateFolderByFilePath(taskinfo.tempPath);
                File.Create(taskinfo.tempPath).Dispose();
            }

            using (var sw = new FileStream(taskinfo.tempPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                taskinfo.downloadedSize = (int) sw.Length;
            }

            DownloadingTask tmpTask = new DownloadingTask(taskinfo);
            HTTPRequest httprequest = new HTTPRequest(new Uri(tmpTask.DownloadInfo.url), HTTPMethods.Head, (req, rep) =>
            {
                tmpTask.HeadRequest = null;
                if (rep == null)
                {
                    CommonResLog("Download failed due to network for :{0}", tmpTask.DownloadInfo.fileName);
                    tmpTask.DownloadInfo.result = DownloadResult.ServerUnreachable;
                    DownloadCommonInitialABWithBreakDownloader(rrInfo);
                }
                else if (rep.StatusCode == 200)
                {
                    try
                    {
                        string firstHeaderValue = rep.GetFirstHeaderValue("Content-Length");
                        tmpTask.DownloadInfo.downloadSize = int.Parse(firstHeaderValue);
                        CommonResLog("Will download {0} bytes for  '{1}'", tmpTask.DownloadInfo.downloadSize, tmpTask.DownloadInfo.fileName);
                        BreakPointDownloader downloader = new BreakPointDownloader(tmpTask.DownloadInfo, new Dictionary<string, DownloadingTask>());
                        tmpTask.Downloader = downloader;
                        downloader.StartDownload();
                    }
                    catch (Exception ex)
                    {
                        CommonResLog("An error occured during download '{0}' due to {1}", tmpTask.DownloadInfo.fileName, ex);
                        tmpTask.DownloadInfo.result = DownloadResult.Failed;
                        DownloadCommonInitialABWithBreakDownloader(rrInfo);
                    }
                }
                else
                {
                    CommonResLog("Response is not ok! for: {0}", tmpTask.DownloadInfo.url);
                    tmpTask.DownloadInfo.result = DownloadResult.Failed;
                    DownloadCommonInitialABWithBreakDownloader(rrInfo);
                }
            })
            {
                DisableCache = true
            };
            httprequest.Send();
        }

        static void DownloadCommonInitialABWithHttpRequest(BaseResFakeInfo rrInfo)
        {
            string fileUrl = rrInfo.GetDownLoadUrl();
            string savePath = FilePathTools.streamingAssetsPath_Platform + "/" + rrInfo.LocalABPath.ToLower();
            CommonResLog("qushuang -----> Start DownloadHomeInitialAB {0}", fileUrl);
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            FilePathTools.CreateFolderByFilePath(savePath);

            new HTTPRequest(new Uri(fileUrl), HTTPMethods.Get, (req, rep) =>
            {
                if (rep == null)
                {
                    CommonResLog("qushuang -----> Response null. try again! {0}", fileUrl);
                    DownloadCommonInitialABWithHttpRequest(rrInfo);
                }
                else if (rep.StatusCode >= 200 && rep.StatusCode < 300)
                {
                    string localfile = savePath;
                    try
                    {
                        List<byte[]> streamedFragments = rep.GetStreamedFragments();
                        if (streamedFragments != null)
                        {
                            int num = 0;
                            using (FileStream fileStream = new FileStream(localfile, FileMode.Append))
                            {
                                foreach (byte[] array in streamedFragments)
                                {
                                    num += array.Length;
                                    fileStream.Write(array, 0, array.Length);
                                }
                            }

                            CommonResLog("qushuang -----> Download successfully! {0}, total size is {1}", fileUrl, num);
                        }
                        else
                        {
                            CommonResLog("qushuang -----> No streamedFragments! try again! {0}", localfile);
                            DownloadCommonInitialABWithHttpRequest(rrInfo);
                        }
                    }
                    catch (Exception ex)
                    {
                        CommonResLog("qushuang -----> An error occured while downloading {0} due to {1}. try again!", localfile, ex);
                        DownloadCommonInitialABWithHttpRequest(rrInfo);
                        return;
                    }
                }
                else
                {
                    CommonResLog("qushuang -----> Unexpected response from server: {0}, StatusCode = {1}, try again!", fileUrl, rep.StatusCode);
                    DownloadCommonInitialABWithHttpRequest(rrInfo);
                }
            })
            {
                DisableCache = true,
                IsCookiesEnabled = false,
                UseStreaming = true,
                StreamFragmentSize = 1 * 1024 * 1024 * 100, // 1k 1Mb 100Mb
                ConnectTimeout = TimeSpan.FromSeconds(5),
                Timeout = TimeSpan.FromSeconds(10)
            }.Send();
        }

        static void DownloadCommonInitialAB(BaseResFakeInfo rrInfo)
        {
            string fileUrl = rrInfo.GetDownLoadUrlForInitial();
            string savePath = FilePathTools.streamingAssetsPath_Platform + "/" + rrInfo.LocalABPath.ToLower();
            CommonResLog("qushuang -----> Start DownloadHomeInitialAB {0}", fileUrl);
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
            }

            FilePathTools.CreateFolderByFilePath(savePath);
            try
            {
                Stream outStream;
                HttpWebRequest request = (HttpWebRequest) WebRequest.Create(fileUrl);
                WebResponse response = request.GetResponse();
                Stream inStream = response.GetResponseStream(); //获取http
                byte[] b = new byte[1024 * 1024]; //每一次获取的长度
                FileInfo fi = new FileInfo(savePath);
                outStream = fi.Create(); //创建文件
                int num = 0;
                int readCount = inStream.Read(b, 0, b.Length); //读流
                num += readCount;
                while (readCount > 0)
                {
                    outStream.Write(b, 0, readCount); //写流
                    readCount = inStream.Read(b, 0, b.Length); //再读流
                    num += readCount;
                }

                outStream.Close();
                inStream.Close();
                response.Close();
                CommonResLog("qushuang -----> Download successfully! {0}, total size is {1}", fileUrl, num);
            }
            catch (Exception ex)
            {
                CommonResLog("qushuang -----> An error occured while downloading {0} due to {1}. try again!", fileUrl, ex);
                DownloadCommonInitialAB(rrInfo);
                return;
            }
        }
    }
}