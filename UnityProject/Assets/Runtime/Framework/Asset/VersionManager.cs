/*-------------------------------------------------------------------------------------------
// Copyright (C) 2019 北京，天龙互娱
//
// 模块名：VersionManager
// 创建日期：2019-1-11
// 创建者：waicheng.wang
// 模块描述：版本管理
//-------------------------------------------------------------------------------------------*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;
// using DragonU3DSDK.Network.API;
// using DragonU3DSDK.Network.API.Protocol;
using Newtonsoft.Json.Linq;
using BestHTTP;
using UnityEngine.Networking;
using File = System.IO.File;

namespace DragonU3DSDK.Asset
{
    public enum AppUpdateResult
    {
        Newest = 0,//最新版
        Suggested = 1,//建议更新
        Force = 2,//强制更新
    }

    public class VersionManager : Manager<VersionManager>
    {
        private VersionInfo localVersionFile;
        private VersionInfo remoteVersionFile;
        private readonly string versionFileName = "Version.txt";

        // 强更检查CD:24小时
        private const float checkAppVersionCD = 24f * 60f * 60f;
        private float lastCheckAppVersionTime = -1f;

        // 远端version文件拉取CD:24小时
        private const float downloadRemoteVersionCD = 24f * 60f * 60f;
        private float lastDownloadRemoteVersionTime = -1f;

        // 线上iOS version拉取CD:24小时
        private const float onlineIOSVersionCD = 24f * 60f * 60f;
        private float lastOnlineIOSVersionTime = -1f;

        // cache强更状态
        private AppUpdateResult versionStatus = AppUpdateResult.Newest;

        // 线上版本号
        //private int versionCodeOnline = DragonNativeBridge.GetVersionCode();
        private int versionCodeOnline = -1; //qushuang: temp test

        // 当前版本对应的远端version文件名
        public string RemoteVersionFileName
        {
            get
            {
#if UNITY_ANDROID
                return string.Format("Version.{0}.txt", AssetConfigController.Instance.VersionCode);
#elif UNITY_IOS
                return string.Format("Version.{0}.txt", AssetConfigController.Instance.IOSVersionCode);
#else
                return string.Format("Version.{0}.txt", AssetConfigController.Instance.VersionCode);
#endif
            }
        }

        public int GetVersionCode()
        {
            string versionStr = "";
#if UNITY_ANDROID
            versionStr = AssetConfigController.Instance.VersionCode;
#elif UNITY_IOS
            versionStr = AssetConfigController.Instance.IOSVersionCode;
#else
            versionStr = AssetConfigController.Instance.VersionCode;
#endif

            int version = 0;

            try
            {
                version = int.Parse(versionStr);
            }
            catch (Exception e)
            {
                DebugUtil.LogWarning("version is not in proper format. err = {0}", e.ToString());
            }

            return version;

        }

        // 获取到远端version文件内容，需要判空
        public VersionInfo GetRemoteVersion()
        {
            return remoteVersionFile;
        }
        
        public VersionInfo GetLocalVersion()
        {
            return localVersionFile;
        }

        // 是否处在离线模式下
#if !UNITY_STANDALONE
        public bool OfflineMode { private set; get; }
#else
        public bool OfflineMode
        {
            get
            {
                return true;
            }
            set
            {

            }
        }
#endif

        // 是否在新手保护期
        public bool ProtectMode = false;

        protected override void InitImmediately()
        {
            TextAsset ta = Resources.Load<TextAsset>("versionfile/Version");
            this.localVersionFile = JsonConvert.DeserializeObject<VersionInfo>(ta.text);
        }

        // 获得资源版本号
        public string GetResRootVersion()
        {
            return this.remoteVersionFile.Version;
        }

        // 获得资源版本号(用于界面显示)
        public string GetResDesplayVersion()
        {
            if (this.remoteVersionFile == null)
                return "v0.0.0";

            return this.remoteVersionFile.Version.Replace('_', '.');
        }

        // 进入离线模式
        public void StartOfflineMode()
        {
            OfflineMode = true;
            StartCoroutine(ShouldQuiteOffline());
        }

        // 检查是否需要退出离线模式
        private IEnumerator ShouldQuiteOffline()
        {
            yield return new WaitForSeconds(10.0f);
            while(Application.internetReachability == NetworkReachability.NotReachable)
            {
                yield return new WaitForSeconds(1.0f);
            }
            OfflineMode = false;
            yield break;
        }

        // 开启保护新手：强制“版本更新检查”和“下载远端version文件”进入CD。目前暂定保护15分钟。
        public void StartProtect()
        {
            lastCheckAppVersionTime = 15 * 60f;
            lastDownloadRemoteVersionTime = 15 * 60f;

            TextAsset ta = Resources.Load<TextAsset>("versionfile/Version");
            this.localVersionFile = JsonConvert.DeserializeObject<VersionInfo>(ta.text);
            this.remoteVersionFile = JsonConvert.DeserializeObject<VersionInfo>(ta.text);

            this.SaveVersionFile();

            ProtectMode = true;
        }

        [Obsolete("StartProtectAndMergeDownloadedVersions is Obsolete,and will be removed")]
        public void StartProtectAndMergeDownloadedVersions(VersionInfo downloadedVersionFile)
        {
            lastCheckAppVersionTime = 15 * 60f;
            lastDownloadRemoteVersionTime = 15 * 60f;

            var ta = Resources.Load<TextAsset>("versionfile/Version");
            this.localVersionFile = JsonConvert.DeserializeObject<VersionInfo>(ta.text);
            this.remoteVersionFile = JsonConvert.DeserializeObject<VersionInfo>(ta.text);

            if (downloadedVersionFile != null)
            {
                foreach (var resGroups in downloadedVersionFile.ResGroups)
                {
                    if (!localVersionFile.ResGroups.ContainsKey(resGroups.Key))
                    {
                        // 下载过的资源组
                        localVersionFile.ResGroups.Add(resGroups.Key, resGroups.Value);
                        remoteVersionFile.ResGroups.Add(resGroups.Key, resGroups.Value);
                        DebugUtil.Log($"StartProtectAndMergeDownloadedVersions ResGroups {resGroups.Key}");
                    }
                    else
                    {
                        var labs = localVersionFile.ResGroups[resGroups.Key];
                        var rabs = remoteVersionFile.ResGroups[resGroups.Key];
                        foreach (var assetBundles in resGroups.Value.AssetBundles)
                        {
                            if (!labs.AssetBundles.ContainsKey(assetBundles.Key))
                            {
                                // 下载过的资源
                                labs.AssetBundles.Add(assetBundles.Key, assetBundles.Value);
                                rabs.AssetBundles.Add(assetBundles.Key, assetBundles.Value);
                                DebugUtil.Log($"StartProtectAndMergeDownloadedVersions AssetBundles {assetBundles.Key}");
                            }
                        }
                    }
                }
            }

            this.SaveVersionFile();

            ProtectMode = true;
        }

        // 结束新手保护
        public void EndProtect()
        {
            lastCheckAppVersionTime = -checkAppVersionCD;
            lastDownloadRemoteVersionTime = -downloadRemoteVersionCD;
            OfflineMode = false;
            ProtectMode = false;
        }

        // 拷贝完所有ab包后，把version文件也写入到下载目录
        [Obsolete("SaveInitialVersionFileToDownloadPath is Obsolete,and will be removed")]
        public void SaveInitialVersionFileToDownloadPath()
        {
            TextAsset ta = Resources.Load<TextAsset>("versionfile/Version");
            this.CreateFile(FilePathTools.persistentDataPath_Platform, this.versionFileName, ta.text);
        }

        [Obsolete("LoadVersionFileInDownloadPath is Obsolete,and will be removed")]
        public VersionInfo LoadVersionFileInDownloadPath()
        {
            var versionFileInDownloadPath = $"{FilePathTools.persistentDataPath_Platform}/{versionFileName}";
            DebugUtil.Log($"LoadVersionFileInDownloadPath: {versionFileInDownloadPath}");
            if (!System.IO.File.Exists(versionFileInDownloadPath)) return null;
            var versionStr = System.IO.File.ReadAllText(versionFileInDownloadPath);
            return JsonConvert.DeserializeObject<VersionInfo>(versionStr);
        }

        // 是否需要拷贝初始包里的bundle包到下载目录
        [Obsolete("IsNeedCopyFilesToDownloadPath is Obsolete,and will be removed")]
        public bool IsNeedCopyFilesToDownloadPath()
        {
            DebugUtil.Log("Start IsNeedCopyFilesToDownloadPath");
            bool needCopy = false;
            string versionFileInDownloadPath = string.Format("{0}/{1}", FilePathTools.persistentDataPath_Platform, versionFileName);
            if (System.IO.File.Exists(versionFileInDownloadPath))
            {
                string versionStr = System.IO.File.ReadAllText(versionFileInDownloadPath);
                VersionInfo downloadVersion = JsonConvert.DeserializeObject<VersionInfo>(versionStr);

                TextAsset ta = Resources.Load<TextAsset>("versionfile/Version");
                if(ta == null){return true;}//Fixed NUll reference, 如果没有找到本地的version文件，需要拷贝，return true ，否则会报错卡死进度条
                VersionInfo localVersion = JsonConvert.DeserializeObject<VersionInfo>(ta.text);
                
                if (downloadVersion?.Version == localVersion?.Version)//下载目录里的version版本和初始包里的version版本一致，不copy
                    needCopy = false;
                else
                    needCopy = true;//两个版本不一致，要copy
            }
            else
            {//下载目录里没有version文件，要拷贝
                needCopy = true;
            }
            return needCopy;
        }

        /// <summary>
        ///  是否是第一次安装
        /// </summary>
        /// <returns>true:是</returns>
        public bool IsFirstInstall()
        {
            string versionFileInDownloadPath = string.Format("{0}/{1}", FilePathTools.persistentDataPath_Platform, versionFileName);
            if (System.IO.File.Exists(versionFileInDownloadPath))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 判断是否需要：覆盖安装后，用包里的Version迭代下载目录的Version
        /// </summary>
        public bool IsNeedPatchVersionAfterOverLayInstall()
        {
            bool state = false;
            string versionFileInDownloadPath = string.Format("{0}/{1}", FilePathTools.persistentDataPath_Platform, versionFileName);
            if (System.IO.File.Exists(versionFileInDownloadPath))
            {
                string versionStr = System.IO.File.ReadAllText(versionFileInDownloadPath);
                VersionInfo downloadVersion = JsonConvert.DeserializeObject<VersionInfo>(versionStr);

                TextAsset ta = Resources.Load<TextAsset>("versionfile/Version");
                VersionInfo localVersion = JsonConvert.DeserializeObject<VersionInfo>(ta.text);

                //下载目录里的UniqueID和初始包里的UniqueID不一致,需要迭代
                if (downloadVersion?.UniqueID != localVersion?.UniqueID)
                {
                    state = true;
                }
            }
            else
            {
                state = true;
            }
            return state;
        }

        /// <summary>
        /// 覆盖安装后，用包里的Version迭代下载目录的Version
        /// </summary>
        /// <returns></returns>
        public IEnumerator PatchVersionAfterOverLayInstall()
        {
            DebugUtil.Log("PatchVersionAfterOverLayInstall begin");

            string versionFileInDownloadPath = string.Format("{0}/{1}", FilePathTools.persistentDataPath_Platform, versionFileName);
            if (System.IO.File.Exists(versionFileInDownloadPath))
            {
                string versionStr = System.IO.File.ReadAllText(versionFileInDownloadPath);
                VersionInfo downloadVersion = JsonConvert.DeserializeObject<VersionInfo>(versionStr);

                TextAsset ta = Resources.Load<TextAsset>("versionfile/Version");
                VersionInfo localVersion = JsonConvert.DeserializeObject<VersionInfo>(ta.text);

                //老版本，则清空下载目录
                if(string.IsNullOrEmpty(downloadVersion.UniqueID))
                {
                    if (Directory.Exists(FilePathTools.persistentDataPath_Platform))
                    {
                        DirectoryInfo dir = new DirectoryInfo(FilePathTools.persistentDataPath_Platform);
                        dir.Empty();

                        DebugUtil.Log("clear persistentDataPath_Platform because overlayinstall in old version");
                    }
                }
                //UniqueID不一样，需要迭代下载目录下的资源
                else if(downloadVersion?.UniqueID != localVersion?.UniqueID)
                {
                    //patch原则：以localversion为主，将已经下载的ab资源在localversion中标示[注意只能修改State值]，然后将localversion保存到下载目录。
                    foreach (var g in downloadVersion.ResGroups)
                    {
                        VersionItemInfo localGroup;
                        if (localVersion.ResGroups.TryGetValue(g.Key, out localGroup))
                        {
                            foreach (var ab in g.Value.AssetBundles)
                            {
                                AssetBundleInfo localAB;
                                if (localGroup.AssetBundles.TryGetValue(ab.Key, out localAB))
                                {
                                    //if (ab.Value.State == AssetState.ExistInDownLoad)
                                    {
                                        string path = string.Format("{0}/{1}", FilePathTools.persistentDataPath_Platform, ab.Key);
                                        if (System.IO.File.Exists(path))
                                        {
                                            //hash不一样，则删除下载目录下的资源
                                            if (localAB.HashString != ab.Value.HashString)
                                            {
                                                System.IO.File.Delete(path);
                                                DebugUtil.Log(string.Format("PatchVersionAfterOverLayInstall delete : {0} group : {1} ,because ExistInDownLoad with different HashString", ab.Key, g.Key));
                                            }
                                            else
                                            {
                                                localAB.State = AssetState.ExistInDownLoad;
                                                localGroup.Refresh(ab.Key, localAB);
                                                DebugUtil.Log(string.Format("PatchVersionAfterOverLayInstall sync : {0} group : {1}", ab.Key, g.Key));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                
                //更新下载目录下的version
                string json = JsonConvert.SerializeObject(localVersion);
                CreateFile(FilePathTools.persistentDataPath_Platform, this.versionFileName, json);
            }
            //说明是第一次安装，直接拷贝Version即可
            else
            {
                DebugUtil.Log(string.Format("PatchVersionAfterOverLayInstall SaveInitialVersionFileToDownloadPath"));

                TextAsset ta = Resources.Load<TextAsset>("versionfile/Version");
                this.CreateFile(FilePathTools.persistentDataPath_Platform, this.versionFileName, ta.text);
            }

            yield return StartCoroutine(CheckInInitialPacketExport(true));
            
            DebugUtil.Log("PatchVersionAfterOverLayInstall end");
        }

        public IEnumerator CheckInInitialPacketExport(bool forceExport)
        {
            DebugUtil.Log("CheckInInitialPacketExport enter");
            
            TextAsset ta = Resources.Load<TextAsset>("versionfile/Version");
            VersionInfo localVersion = JsonConvert.DeserializeObject<VersionInfo>(ta.text);

            if (!forceExport && PlayerPrefs.GetString("InInitialPacketExported").Equals(localVersion.UniqueID))
            {
                yield break;
            }
            
            DebugUtil.Log("CheckInInitialPacketExport start");
            
            //拷贝初包资源到下载目录
            bool complete = false;
            bool diskFullError = false;
            List<string> allABFiles = new List<string>();
            foreach (BundleGroup group in AssetConfigController.Instance.Groups)
            {
                allABFiles.AddRange(group.GetInInitialPacketPaths());
            }

            DownloadManager.Instance.CopyBundleToDownloadFolder(FilePathTools.streamingAssetsPath_Platform_ForWWWLoad, FilePathTools.persistentDataPath_Platform, allABFiles,
                () => { complete = true; },
                (ex) =>
                {
                    if (ex.GetType() == typeof(System.IO.IOException))
                    {
                        if (Utils.IsDiskFull(ex) && !diskFullError)
                        {
                            diskFullError = true;
                            // CommonUtils.OpenCommonConfirmWindow(new NoticeUIData
                            // {
                            //     DescString = "Oops, your phone storage is full, please clean up and try again."//这里多语言不一定能保证可以使用，所以直接用文本
                            // });
                        }
                    }
                }, 
                false);

            while (!complete)
            {
                yield return null;
                
                DebugUtil.Log("Copy InitialPacket to download end");
            }

            //标记初包资源已导出完成
            if(!diskFullError)
            {
                PlayerPrefs.SetString("InInitialPacketExported", localVersion.UniqueID);
                
                DebugUtil.Log("CheckInInitialPacketExport finish");
            }
        }
        
        /// <summary>
        /// 判断iOS版本是否在审核中
        /// </summary>
        /// <returns></returns>
        public bool IsAppInReview()
        {
#if UNITY_IOS
            if (OfflineMode)
            {
                return true;
            }

            // if(DragonNativeBridge.GetVersionCode() > versionCodeOnline)
            // {
            //     return true;
            // }
#endif
            return false;
        }


        /// <summary>
        /// 获取线上iOS面向玩家的最大版本号
        /// </summary>
        /// <returns></returns>
        public int GetIOSVersionCodeOnline()
        {
#if UNITY_IOS
            // 离线模式 或 检查CD 或 使用本地bundle包
            // if (OfflineMode || (lastOnlineIOSVersionTime > 0f && (Time.time - lastOnlineIOSVersionTime < onlineIOSVersionCD)))
            // {
            //     return versionCodeOnline;
            // }

            // lastOnlineIOSVersionTime = Time.time;

            // DebugUtil.Log("Start GetIOSVersionCodeOnline");
            // var cGetConfig = new CGetConfig
            // {
            //     Route = "version/ios",
            // };

            // APIManager.Instance.Send(cGetConfig, (SGetConfig sGetConfig) =>
            // {

            //     DebugUtil.LogWarning("MaxVersionCode json raw data is {0}", sGetConfig.Config.Json);

            //     var obj = JObject.Parse(sGetConfig.Config.Json);

            //     var remoteVersionCode = versionCodeOnline;
            //     try
            //     {
            //         remoteVersionCode = int.Parse(obj["MaxVersionCode"].ToString());
            //         DebugUtil.LogWarning("MaxVersionCode  = {0}", remoteVersionCode);
            //     }
            //     catch (Exception e)
            //     {
            //         DebugUtil.LogWarning("MaxVersionCode format error. err = {0}", e.ToString());
            //     }

            //     versionCodeOnline = remoteVersionCode;

            // }, (errno, errmsg, resp) =>
            // {
            //     DebugUtil.LogWarning("MaxVersionCode fetch error. err = {0}", errmsg);
            // });
#endif
            return versionCodeOnline;
        }




        // 游戏强制更新检查 true:强制更新  false:不强制更新
        public void CheckAppVersion(Action<AppUpdateResult> callback)
        {
            // 离线模式 或 检查CD 或 使用本地bundle包
//             if (OfflineMode || (lastCheckAppVersionTime > 0f && (Time.time - lastCheckAppVersionTime < checkAppVersionCD)))
//             {
//                 callback?.Invoke(versionStatus);
//                 return;
//             }
//             DebugUtil.Log("Start CheckAppVersion");
//             lastCheckAppVersionTime = Time.time;
//             EventManager.Instance.Trigger<SDKEvents.CheckUpdateEvent>().Data(SDKEvents.CheckUpdateEvent.CheckUpdateEventType.CHECK_UPDATE_EVENT_TYPE_START, AppUpdateResult.Newest).Trigger();
//             var localVersionCode = DragonNativeBridge.GetVersionCode();
//             var cGetConfig = new CGetConfig
//             {
//                 Route = "minSupportAppVer",
//             };

//             APIManager.Instance.Send(cGetConfig, (SGetConfig sGetConfig) =>
//             {
//                 var obj = JObject.Parse(sGetConfig.Config.Json);
//                 bool isForceUpdate = false;
//                 string key;
// #if UNITY_ANDROID
//                 key = "google";
// #elif UNITY_IOS
//                 key = "ios";
// #else
//                 key = "google";
// #endif
//                 int.TryParse(obj[key]?.ToString(), out var remoteVersionCode);
//                 if (obj["force"] != null)
//                 {
//                     var forceContent = JObject.Parse(obj["force"].ToString());
//                     bool.TryParse(forceContent[key]?.ToString(), out isForceUpdate);
//                 }

//                 DebugUtil.Log("localVersionCode : " + localVersionCode + "   remoteVersionCode : " + remoteVersionCode);
//                 if (localVersionCode < remoteVersionCode)
//                 {
//                     if (isForceUpdate)
//                     {
//                         //callback(AppUpdateResult.Force);
//                         versionStatus = AppUpdateResult.Force;
//                     }
//                     else
//                     {
//                         //callback(AppUpdateResult.Suggested);
//                         versionStatus = AppUpdateResult.Suggested;
//                     }
//                     EventManager.Instance.Trigger<SDKEvents.CheckUpdateEvent>().Data(SDKEvents.CheckUpdateEvent.CheckUpdateEventType.CHECK_UPDATE_EVENT_TYPE_FINISH, versionStatus).Trigger();
//                 }
//                 else
//                 {
//                     //callback(AppUpdateResult.Newest);
//                     versionStatus = AppUpdateResult.Newest;
//                     EventManager.Instance.Trigger<SDKEvents.CheckUpdateEvent>().Data(SDKEvents.CheckUpdateEvent.CheckUpdateEventType.CHECK_UPDATE_EVENT_TYPE_FINISH, versionStatus).Trigger();
//                 }

//             }, (errno, errmsg, resp) =>
//             {
//                 DebugUtil.Log("check version get config failed with errno = {0} errmsg = {1}", errno.ToString(), errmsg);
//                 versionStatus = AppUpdateResult.Newest;
//                 EventManager.Instance.Trigger<SDKEvents.CheckUpdateEvent>().Data(SDKEvents.CheckUpdateEvent.CheckUpdateEventType.CHECK_UPDATE_EVENT_TYPE_FAILURE, versionStatus, errno.ToString()).Trigger();
//                 //callback(AppUpdateResult.Newest);

//             });
//             callback?.Invoke(versionStatus);
        }

        // 分别加载本地和远端的version文件
        public void LoadVersionFile(Action<bool> onComplete)
        {
            // 本地和远端version文件都加载过了，且在CD时间内，不刷新version
            if (localVersionFile != null && remoteVersionFile != null && lastDownloadRemoteVersionTime > 0f && (Time.time - lastDownloadRemoteVersionTime < downloadRemoteVersionCD))
            {
                if (onComplete != null)
                    onComplete(true);
                return;
            }

            StartCoroutine(LoadLocalVersionFile(() =>
            {
                StartCoroutine(LoadRemoteVersionFile(onComplete));
            }));
        }

        // 加载本地version文件
        // 优先去下载目录加载，如果不存在，就去Resources目录下加载
        public IEnumerator LoadLocalVersionFile(Action onComplete)
        {
            string url = string.Empty;
            string localVersionPath = string.Format("{0}/{1}", FilePathTools.persistentDataPath_Platform, versionFileName);
            if (System.IO.File.Exists(localVersionPath))
            {
                url = string.Format("{0}/{1}", FilePathTools.persistentDataPath_Platform_ForWWWLoad, versionFileName);
                //WWW www = new WWW(url);

                UnityWebRequest www = UnityWebRequest.Get(url);
                yield return www.SendWebRequest();
                if (!www.isHttpError && !www.isNetworkError)
                {
                    this.localVersionFile = JsonConvert.DeserializeObject<VersionInfo>(www.downloadHandler.text);
                    www.Dispose();
                    if (onComplete != null)
                        onComplete();
                    yield break;
                }
                else
                {
                    TextAsset ta = Resources.Load<TextAsset>("versionfile/Version");
                    this.localVersionFile = JsonConvert.DeserializeObject<VersionInfo>(ta.text);
                    if (onComplete != null)
                        onComplete();
                    www.Dispose();
                    yield break;
                }
            }
            else
            {
                TextAsset ta = Resources.Load<TextAsset>("versionfile/Version");
                this.localVersionFile = JsonConvert.DeserializeObject<VersionInfo>(ta.text);
                if (onComplete != null)
                    onComplete();
                yield break;
            }
        }

        // 加载远端version文件
        // 优先去服务器下载，如果下载不成功，
        public IEnumerator LoadRemoteVersionFile(Action<bool> onComplete)
        {
            bool hasResult = false;

            if (OfflineMode)//离线模式，不去服务器下version文件
            {
                TextAsset ta = Resources.Load<TextAsset>("versionfile/Version");
                this.remoteVersionFile = JsonConvert.DeserializeObject<VersionInfo>(ta.text);
                if (onComplete != null)
                    onComplete(true);

                hasResult = true;
            }
            else
            {
                string fileUrl = string.Format("{0}/{1}", FilePathTools.AssetBundleDownloadPath, RemoteVersionFileName);
                new HTTPRequest(new Uri(fileUrl), (req, rep) =>
                {
                    if (rep == null)
                    {
                        DebugUtil.LogError("Response null. Server unreachable? Try again later. {0}", fileUrl);

                        TextAsset ta = Resources.Load<TextAsset>("versionfile/Version");
                        this.remoteVersionFile = JsonConvert.DeserializeObject<VersionInfo>(ta.text);
                        if (onComplete != null)
                            onComplete(false);

                        hasResult = true;
                    }
                    else if (rep.StatusCode >= 200 && rep.StatusCode < 300)
                    {
                        this.remoteVersionFile = JsonConvert.DeserializeObject<VersionInfo>(rep.DataAsText);
                        // Storage.StorageManager.Instance.GetStorage<Storage.StorageCommon>().ResVersion = GetResRootVersion();
                        if (onComplete != null)
                            onComplete(true);

                        lastDownloadRemoteVersionTime = Time.time;
                        hasResult = true;
                    }
                    else
                    {
                        DebugUtil.LogError("Unexpected response from server: {0}, StatusCode = {1}", fileUrl, rep.StatusCode);

                        TextAsset ta = Resources.Load<TextAsset>("versionfile/Version");
                        this.remoteVersionFile = JsonConvert.DeserializeObject<VersionInfo>(ta.text);
                        if (onComplete != null)
                            onComplete(false);

                        hasResult = true;
                    }

                })
                {
                    DisableCache = true,
                    IsCookiesEnabled = false,
                    ConnectTimeout = TimeSpan.FromSeconds(5),
                    Timeout = TimeSpan.FromSeconds(10)
                }.Send();
            }

            while (!hasResult)
            {
                yield return new WaitForEndOfFrame();
            }

            yield break;
        }

        // 通过组名，ab包名，获得需要下载的ab包
        public List<string> GetUpdateFiles(string groupName, string assetbundleName = null)
        {
            List<string> updateFileNames = new List<string>();

            if (localVersionFile == null || remoteVersionFile == null)
            {
                DebugUtil.LogError("remoteVersion文件出错");
                return updateFileNames;
            }

            VersionItemInfo item = null;
            if (localVersionFile.ResGroups.ContainsKey(groupName))
            {
                item = localVersionFile.ResGroups[groupName];
                if (item.UpdateWholeGroup)
                {
                    List<string> remoteAB = remoteVersionFile.GetAssetBundlesByKey(groupName);
                    List<string> localAB = localVersionFile.GetAssetBundlesByKey(groupName);

                    Dictionary<string, string> localFilesHash = new Dictionary<string, string>();
                    foreach (string assetname in localAB)
                    {
                        localFilesHash.Add(assetname, localVersionFile.GetAssetBundleHash(groupName, assetname));
                    }
                    foreach (string assetname in remoteAB)
                    {
                        if (localFilesHash.ContainsKey(assetname))
                        {
                            string remoteHash = remoteVersionFile.GetAssetBundleHash(groupName, assetname);
                            string localPath = FilePathTools.GetBundleLoadPath(assetname);
                            if (!File.Exists(localPath) || remoteHash != localFilesHash[assetname])
                            {
                                updateFileNames.Add(assetname);
                            }
                        }
                        else
                        {
                            updateFileNames.Add(assetname);
                        }
                    }
                }
                else
                {
                    string remoteName = remoteVersionFile.GetAssetBundleByKeyAndName(groupName, assetbundleName);
                    string localName = localVersionFile.GetAssetBundleByKeyAndName(groupName, assetbundleName);
                    string remoateHash = remoteVersionFile.GetAssetBundleHash(groupName, remoteName);

                    if (string.IsNullOrEmpty(localName))//本地没有
                    {
                        if (!string.IsNullOrEmpty(remoteName))//远端有
                        {
                            updateFileNames.Add(remoteName);//远端有，本地没有
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(remoteName))//本地有，远端有
                        {
                            // 比较两者的md5
                            string localHash = localVersionFile.GetAssetBundleHash(groupName, localName);
                            string localPath = FilePathTools.GetBundleLoadPath(localName);
                            if (!File.Exists(localPath) || localHash.Trim() != remoateHash.Trim())//md5不一样，要更新
                            {
                                updateFileNames.Add(remoteName);
                            }
                        }
                    }
                }
            }
            else
            {// 本地version文件不包含这个组，目前只发生在editor模式下，修复下。将来不一定。

                if (remoteVersionFile.ResGroups.ContainsKey(groupName))
                {
                    item = remoteVersionFile.ResGroups[groupName];

                    if (item.UpdateWholeGroup)
                    {
                        List<string> remoteAB = remoteVersionFile.GetAssetBundlesByKey(groupName);
                        Dictionary<string, string> localFilesMd5 = new Dictionary<string, string>();
                        foreach (string assetname in remoteAB)
                        {
                            updateFileNames.Add(assetname);// 组内文件全更
                        }
                    }
                    else
                    {
                        string remoteName = remoteVersionFile.GetAssetBundleByKeyAndName(groupName, assetbundleName);
                        if (!string.IsNullOrEmpty(remoteName))//远端有
                        {
                            updateFileNames.Add(remoteName);//远端有，本地没有
                        }
                    }
                }
                else
                {
                    DebugUtil.LogError("远端和本地的version文件都不包含:" + groupName);
                }
            }


            return updateFileNames;
        }


        /// <summary>
        /// 通过组名，ab包名，获得需要下载的ab包
        /// </summary>
        /// <returns>The update files dict.[name,md5]</returns>
        /// <param name="groupName">Group name.</param>
        /// <param name="assetbundleName">Assetbundle name.</param>
        public Dictionary<string, string> GetUpdateFilesDict(string groupName, string assetbundleName = null)
        {
            Dictionary<string, string> updateFileNames = new Dictionary<string, string>();

            if (localVersionFile == null || remoteVersionFile == null)
            {
                DebugUtil.LogError("Version文件出错");
                return updateFileNames;
            }

            VersionItemInfo item = null;
            if (localVersionFile.ResGroups.ContainsKey(groupName))
            {
                item = localVersionFile.ResGroups[groupName];
                if (item.UpdateWholeGroup)//强制更新整个组里的ab包
                {
                    List<string> remoteAB = remoteVersionFile.GetAssetBundlesByKey(groupName);
                    List<string> localAB = localVersionFile.GetAssetBundlesByKey(groupName);

                    Dictionary<string, string> localFilesHash = new Dictionary<string, string>();
                    foreach (string assetname in localAB)
                    {
                        localFilesHash.Add(assetname, localVersionFile.GetAssetBundleHash(groupName, assetname));
                    }
                    foreach (string assetname in remoteAB)
                    {
                        string remoteHash = remoteVersionFile.GetAssetBundleHash(groupName, assetname);
                        string remoteMd5 = remoteVersionFile.GetAssetBundleMd5(groupName, assetname);
                        string localPath = FilePathTools.GetBundleLoadPath(assetname);
                        if (localFilesHash.ContainsKey(assetname))
                        {
                            //添加“remoteState == AssetState.ExistInDownLoad”在于兼容ProtectMode
                            if (!File.Exists(localPath) || remoteHash.Trim() != localFilesHash[assetname].Trim())
                            {
                                updateFileNames.Add(assetname, remoteMd5);
                            }
                        }
                        else
                        {
                            updateFileNames.Add(assetname, remoteMd5);
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(assetbundleName))
                    {
                        string remoteName = remoteVersionFile.GetAssetBundleByKeyAndName(groupName, assetbundleName);
                        string localName = localVersionFile.GetAssetBundleByKeyAndName(groupName, assetbundleName);
                        string remoateHash = remoteVersionFile.GetAssetBundleHash(groupName, remoteName);
                        string remoateMd5 = remoteVersionFile.GetAssetBundleMd5(groupName, remoteName);
                        if (string.IsNullOrEmpty(localName))//本地没有
                        {
                            if (!string.IsNullOrEmpty(remoteName))//远端有
                            {
                                updateFileNames.Add(remoteName, remoateMd5);//远端有，本地没有
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(remoteName))//本地有，远端有
                            {
                                // 比较两者的hash
                                string localHash = localVersionFile.GetAssetBundleHash(groupName, localName);
                                string localPath = FilePathTools.GetBundleLoadPath(localName);
                                if (!File.Exists(localPath) || localHash.Trim() != remoateHash.Trim())//hash不一样，要更新
                                {
                                    updateFileNames.Add(remoteName, remoateMd5);
                                }
                            }
                        }
                    }
                }
            }
            else
            {// 本地version文件不包含这个组，目前只发生在editor模式下，修复下。将来不一定。
                if (remoteVersionFile.ResGroups.ContainsKey(groupName))
                {
                    item = remoteVersionFile.ResGroups[groupName];
                    if (item.UpdateWholeGroup)
                    {
                        List<string> remoteAB = remoteVersionFile.GetAssetBundlesByKey(groupName);
                        foreach (string assetname in remoteAB)
                        {
                            string remoteMd5 = remoteVersionFile.GetAssetBundleMd5(groupName, assetname);
                            updateFileNames.Add(assetname, remoteMd5);
                        }

                    }
                    else
                    {
                        string remoteName = remoteVersionFile.GetAssetBundleByKeyAndName(groupName, assetbundleName);
                        string remoteMd5 = remoteVersionFile.GetAssetBundleMd5(groupName, remoteName);
                        if (!string.IsNullOrEmpty(remoteName))//远端有
                        {
                            updateFileNames.Add(remoteName, remoteMd5);
                        }
                    }
                }
                else
                {
                    DebugUtil.LogError("远端和本地的version文件都不包含:" + groupName);
                }
            }
            return updateFileNames;
        }

        // 获得某组下的所有资源路径。慎用！因为没有跟远端version文件比对。
        public List<string> GetFilesInGroup(string groupName)
        {
            List<string> allFiles = new List<string>();

            if (localVersionFile == null)
            {
                TextAsset ta = Resources.Load<TextAsset>("versionfile/Version");//todo:有隐患,应该拿下载目录里的version文件
                this.localVersionFile = JsonConvert.DeserializeObject<VersionInfo>(ta.text);
            }

            VersionItemInfo item = null;
            if (localVersionFile.ResGroups.ContainsKey(groupName))
            {
                item = localVersionFile.ResGroups[groupName];
            }
            else
            {
                DebugUtil.LogError("grounName " + groupName + ", Not Exsits!");
                return allFiles;
            }

            allFiles = localVersionFile.GetAssetBundlesByKey(groupName);
            return allFiles;
        }

        public void RefreshRemoteToLocal(List<string> files)
        {
            if (localVersionFile == null || remoteVersionFile == null)
            {
                DebugUtil.LogError("Version文件出错");
                return;
            }

            foreach (KeyValuePair<string, VersionItemInfo> kv in remoteVersionFile.ResGroups)
            {
                foreach (KeyValuePair<string, AssetBundleInfo> kv2 in kv.Value.AssetBundles)
                {
                    if (files.Contains(kv2.Key))
                    {
                        if (localVersionFile.ResGroups.ContainsKey(kv.Key))
                        {
                            AssetBundleInfo newABI = remoteVersionFile.ResGroups[kv.Key].AssetBundles[kv2.Key];
                            localVersionFile.ResGroups[kv.Key].Refresh(kv2.Key, newABI);
                        }
                        else
                        {
                            VersionItemInfo newVii = new VersionItemInfo
                            {
                                Version = kv.Value.Version,
                                UpdateWholeGroup = kv.Value.UpdateWholeGroup
                            };
                            newVii.Refresh(kv2.Key, kv2.Value);
                            localVersionFile.Add(kv.Key, newVii);
                        }
                    }
                }
            }

            SaveVersionFile();
        }

        public void RefreshRemoteToLocal(string groupName, string assetbundleName = null)
        {
            if (localVersionFile == null || remoteVersionFile == null)
            {
                DebugUtil.LogError("Version文件出错");
                return;
            }

            VersionItemInfo itemInfo = GetGroupVersion(groupName);
            if (itemInfo != null)
            {
                if (itemInfo.UpdateWholeGroup)
                {
                    localVersionFile.ResGroups[groupName].Refresh(remoteVersionFile.ResGroups[groupName].AssetBundles);
                    SaveVersionFile();
                }
                else
                {
                    if (assetbundleName == null || !remoteVersionFile.ResGroups[groupName].AssetBundles.ContainsKey(assetbundleName))
                    {
                        DebugUtil.LogError("远端没有资源:" + assetbundleName);
                        return;
                    }
                    AssetBundleInfo newABI = remoteVersionFile.ResGroups[groupName].AssetBundles[assetbundleName];
                    localVersionFile.ResGroups[groupName].Refresh(assetbundleName, newABI);
                    SaveVersionFile();
                }
            }
        }

        private VersionItemInfo GetGroupVersion(string groupName)
        {
            if (string.IsNullOrEmpty(groupName))
                return null;

            if (localVersionFile.ResGroups.ContainsKey(groupName))
            {
                return localVersionFile.ResGroups[groupName];
            }

            return null;
        }

        public AssetBundleInfo GetAssetBundleInfo(string assetBundleName)
        {
            var result = new AssetBundleInfo();
            foreach (var kv in localVersionFile.ResGroups)
            {
                if (kv.Value.AssetBundles.ContainsKey(assetBundleName))
                {
                    result = kv.Value.AssetBundles[assetBundleName];
                    return result;
                }
            }
            return result;
        }

        public void SaveVersionFile()
        {
            string json = JsonConvert.SerializeObject(localVersionFile);
            CreateFile(FilePathTools.persistentDataPath_Platform, this.versionFileName, json);
        }

        private void CreateFile(string path, string filename, string info)
        {
            StreamWriter sw;
            FileInfo t = new FileInfo(path + "/" + filename);
            DirectoryInfo dir = t.Directory;
            if (!dir.Exists)
            {
                dir.Create();
            }

            sw = t.CreateText();
            DebugUtil.Log("write version:" + t);
            //以行的形式写入信息
            sw.WriteLine(info);
            //关闭流
            sw.Close();
            //销毁流
            sw.Dispose();
        }
    }
}


