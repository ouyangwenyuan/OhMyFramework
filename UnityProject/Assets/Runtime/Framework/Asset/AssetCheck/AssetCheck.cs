/*-------------------------------------------------------------------------------------------
// Copyright (C) 2020 成都，天龙互娱
//
// 模块名：AssetCheck
// 创建日期：2020-7-22
// 创建者：qibo.li
// 模块描述：资源检测模块
//-------------------------------------------------------------------------------------------*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BestHTTP;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// 普通资源：
/// 1.全部下载；2.全部加载
/// 活动资源：
/// 活动资源一定会走QA流程，所有不需要这里检测
/// </summary>
namespace DragonU3DSDK.Asset
{
    public class AssetCheck : MonoBehaviour
    {
        public ScrollRect scrollControl;
        public Text text;

        private StringBuilder logValue = new StringBuilder();
        private int lastIndex = 0;
        private int lastLength = 0;
        private bool lastRefreshEndLine = false;

        private VersionInfo localVersion;
        private VersionInfo remoteVersion;
        
        private bool fail = false;
        
        void Start()
        {
            StartCoroutine(Check());
        }
        
        void Log(string value, bool refreshEndLine)
        {
            value += "\n";
            if (refreshEndLine && lastRefreshEndLine)
            {
                if (lastIndex >= 0 && lastLength > 0)
                {
                    logValue.Remove(lastIndex, lastLength);
                }
            }
            lastIndex = logValue.Length;
            lastLength = value.Length;
            lastRefreshEndLine = refreshEndLine;
            logValue.Append(value);
            
            text.text = logValue.ToString();

            StartCoroutine(Bottom());
        }

        IEnumerator Bottom()
        {
            yield return new WaitForEndOfFrame();
            scrollControl.verticalNormalizedPosition = 0f;
        }
        
        void Fail(string value)
        {
            Log(string.Format("<color=#880000FF>{0}</color>", value), false);
            fail = true;
        }

        void Warning(string value)
        {
            Log(string.Format("<color=#CCCC00FF>{0}</color>", value), false);
        }

        void Result()
        {
            if (!fail) { Log("<color=#00FF00FF>success!!!</color>", false); }
            else { Log("<color=#880000FF>fail!!!</color>", false); }
        }
        
        IEnumerator Check()
        {
            yield return StartCoroutine(InitVersion());

            yield return StartCoroutine(CheckInitPackage());
            
            yield return StartCoroutine(DownLoadAllAsset());

            yield return StartCoroutine(CheckAllAsset());
            
            Result();
        }

        IEnumerator InitVersion()
        {
            TextAsset ta = Resources.Load<TextAsset>("versionfile/Version");
            localVersion = JsonConvert.DeserializeObject<VersionInfo>(ta.text);

            bool downFinish = false;

            string fileUrl = string.Format("{0}/{1}", FilePathTools.AssetBundleDownloadPath, VersionManager.Instance.RemoteVersionFileName);
            new HTTPRequest(new Uri(fileUrl), (req, rep) =>
            {
                if (rep == null)
                {
                    Fail(string.Format("Response null. Server unreachable? Try again later. {0}", fileUrl));
                }
                else if (rep.StatusCode >= 200 && rep.StatusCode < 300)
                {
                    remoteVersion = JsonConvert.DeserializeObject<VersionInfo>(rep.DataAsText);
                    downFinish = true;
                }
                else
                {
                    Fail(string.Format("Unexpected response from server: {0}, StatusCode = {1}", fileUrl, rep.StatusCode));
                }
            })
            {
                DisableCache = true,
                IsCookiesEnabled = false,
                ConnectTimeout = TimeSpan.FromSeconds(5),
                Timeout = TimeSpan.FromSeconds(10)
            }.Send();

            while (!downFinish)
            {
                yield return null;
            }

            Log("init version done", false);
        }

        IEnumerator CheckInitPackage()
        {
            if (Directory.Exists(FilePathTools.persistentDataPath_Platform))
            {
                DirectoryInfo dir = new DirectoryInfo(FilePathTools.persistentDataPath_Platform);
                dir.Empty();
            }
            
            Log("clean initpackage done", false);
            
            List<string> bundles = new List<string>();
            List<string> md5List = new List<string>();
            foreach (BundleGroup group in AssetConfigController.Instance.Groups)
            {
                List<string> temp = group.GetInInitialPacketPaths();
                foreach (var p in temp)
                {
                    bundles.Add(p + ".ab");
                    md5List.Add(localVersion.GetAssetBundleMd5(group.GroupName, p.ToLower() + ".ab"));
                }
            }

            Log(string.Format("copy progress：{00:F1}%", 0.0f), false);

            int cnt = 0;
            foreach (var p in bundles)
            {
                string path = string.Format("{0}/{1}", FilePathTools.streamingAssetsPath_Platform_ForWWWLoad, p);
                
                using (UnityWebRequest www = UnityWebRequest.Get(path))
                {
                    yield return www.SendWebRequest();

                    if (www.isNetworkError || www.isHttpError)
                    {
                        Fail(string.Format("can not copy file : {0}", path));
                    }
                    else
                    {
                        try
                        {
                            byte[] b = www.downloadHandler.data;
                            string destFile = string.Format("{0}/{1}", FilePathTools.persistentDataPath_Platform, p);
                            string destDir = Path.GetDirectoryName(destFile);
                            if (!Directory.Exists(destDir))
                            {
                                Directory.CreateDirectory(destDir);
                            }

                            if (File.Exists(destFile))
                            {
                                File.Delete(destFile);
                            }
                        
                            using (FileStream fs = new FileStream(destFile, FileMode.Create))
                            {
                                fs.Seek(0, SeekOrigin.Begin);
                                fs.Write(b, 0, b.Length);
                                fs.Close();
                            }

                            string md5 = AssetUtils.BuildFileMd5(destFile);
                            string md5_persistent = md5List[cnt];

                            if (!md5.Equals(md5_persistent))
                            {
                                Fail(string.Format("{0} md5 not equal version record", path));
                            }
                            
                            Log(string.Format("copy progress：{00:F1}%", (++cnt / (bundles.Count * 1.0f)) * 100.0f), true);
                        }
                        catch (Exception e)
                        {
                            Fail(e.ToString());
                        }
                    }
                }

                yield return null;
            }
            
            Log(string.Format("copy progress：{00:F1}%", 100.0f), true);
            
            Log("copy initpackage done", false);

            yield return StartCoroutine(CheckAsset(bundles));
            
            Log("check initpackage done", false);
            
            yield return null;
        }

        IEnumerator CheckAsset(List<string> bundleNames)
        {
            Log(string.Format("load bundle progress：{00:F1}%", 0.0f) ,false);
            
            List<AssetBundle> allBundles = new List<AssetBundle>();
            for (int m = 0; m < bundleNames.Count; ++m)
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(FilePathTools.GetBundleLoadPath(bundleNames[m]));
                if (null == bundle)
                {
                    Fail(string.Format("can not load bundle file : {0}", bundleNames[m]));
                }

                allBundles.Add(bundle);

                Log(string.Format("load bundle progress：{00:F1}%", (m + 1) / (bundleNames.Count * 1.0f) * 100.0f), true);

                yield return null;
            }

            Log(string.Format("load bundle progress：{00:F1}%", 100.0f), true);

            Log(string.Format("Object progress：{00:F1}% ", 0.0f) ,false);
            
            for (int m = 0; m < allBundles.Count; ++m)
            {
                UnityEngine.Object[] objs = allBundles[m].LoadAllAssets();
                foreach (var obj in objs)
                {
                    if (obj == null)
                    {
                        Fail(string.Format("null in bundle load : {0}", bundleNames[m]));
                    }

                    if (obj.GetType() == typeof(GameObject))
                    {
                        GameObject prefab = obj as GameObject;
                        if (null == prefab)
                        {
                            Fail(string.Format("{0} convert Prefab fail in bundle ：{1}", obj.name, bundleNames[m]));
                        }

                        Component[] comps = prefab.GetComponentsInChildren<Component>(true);
                        foreach (var com in comps)
                        {
                            if (null == com)
                            {
                                Fail(string.Format("{0} prefab exist null component in bundle ：{1}", obj.name, bundleNames[m]));
                            }
                        }

                        GameObject instance = Instantiate(prefab);
                        if (null == instance)
                        {
                            Fail(string.Format("{0} instance is null inbundle ：{1}", obj.name, bundleNames[m]));
                        }
                        
                        comps = instance.GetComponentsInChildren<Component>(true);
                        foreach (var com in comps)
                        {
                            if (null == com)
                            {
                                Fail(string.Format("{0} instance exist null component in bundle ：{1}", obj.name, bundleNames[m]));
                            }
                        }
                        
                        Destroy(instance);
                    }
                }

                Log(string.Format("Object progress：{00:F1}% ", (m + 1) / (allBundles.Count * 1.0f) * 100.0f), true);

                yield return null;
            }

            Log(string.Format("Object progress：{00:F1}% ", 100.0f), true);

            Log(string.Format("unload bundle progress：{00:F1}%", 0.0f) ,false);
            
            for (int m = 0; m < allBundles.Count; ++m)
            {
                allBundles[m].Unload(false);
                
                Log(string.Format("unload bundle progress：{00:F1}%", (m + 1) / (allBundles.Count * 1.0f) * 100.0f), true);

                yield return null;
            }
            
            Log(string.Format("unload bundle progress：{00:F1}%", 100.0f), true);
            
            yield return null;
        }

        IEnumerator DownLoadAllAsset()
        {
            Dictionary<string, string> updateFileNames = new Dictionary<string, string>();
            foreach (var g in remoteVersion.ResGroups)
            {
                foreach (var p in g.Value.AssetBundles)
                {
                    updateFileNames[p.Key] = p.Value.Md5;
                }
            }

            bool downloadFinish = false;
            int resCount = updateFileNames.Count;
            foreach (var kv in updateFileNames)
            {
                DownloadManager.Instance.DownloadInSeconds(kv.Key, kv.Value, (downloadinfo) =>
                {
                    if (downloadinfo.result == DownloadResult.Success)
                    {
                        resCount--;
                        if (resCount <= 0) 
                        {
                            downloadFinish = true;
                        }
                    }
                    else
                    {
                        Fail(string.Format("can not download : {0}", downloadinfo.url));
                    }
                });
            }

            Log(string.Format("download progress：{0:F1}%", 0.0f), false);
                
            while (!downloadFinish)
            {
                Log(string.Format("download progress：{0:F1}%", (updateFileNames.Count - resCount) / (updateFileNames.Count * 1.0f) * 100.0f), true);

                yield return null;
            }
                
            Log(string.Format("download progress：{0:F1}%", 100.0f), true);
                
            Log("download all asset done", false);
        }

        IEnumerator CheckAllAsset()
        {
            List<string> bundleNames = new List<string>();
            foreach (var g in remoteVersion.ResGroups)
            {
                foreach (var p in g.Value.AssetBundles)
                {
                    bundleNames.Add(p.Key);
                }
            }

            yield return StartCoroutine(CheckAsset(bundleNames));
            
            Log("check all asset done", false);
            
            yield return null;
        }
        
        public static void CheckStart()
        {
            string target = 
#if UNITY_ANDROID
                "Android/AssetCheck";
#elif UNITY_IOS
                "iOS/AssetCheck";
#else
                "";
#endif
            if (!string.IsNullOrEmpty(target))
            {
                //clear
                {
                    EventManager.Instance.Trigger<SDKEvents.AssetCheckClearEvent>().Trigger();
                    
                    // Destroy(UIRoot.Instance.gameObject);
                    ResourcesManager.Instance.Clear();
                }
                
                TextAsset level = Resources.Load<TextAsset>(target);
                AssetBundle.LoadFromMemory(level.bytes);
                SceneManager.LoadScene("AssetCheck");
            }
            else
            {
                // CommonUtils.OpenCommonConfirmWindow(new NoticeUIData
                // {
                //     DescString = "Not supported on the current platform"
                // });
            }
        }
    }
}
