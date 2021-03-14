using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;
using BestHTTP;
using Newtonsoft.Json;

namespace DragonU3DSDK.Asset
{
    [Serializable]
    public class CookingGameVersionFileItem : BaseVersionFileItem
    {
    }

    [Serializable]
    public class CookingFolderInfo
    {
        public string LocalPath;
        public string RemotePath;
    }

    [Serializable]
    public class CookingMapInfo
    {
        public int LocalId;
        public int RemoteId;
        public bool InInitialPacket = false;
    }

    [Serializable]
    public class CookingGameFakeInfo : BaseResFakeInfo
    {
        public override string GetDownLoadUrl()
        {
            return string.Format("{0}/{1}", CookingGameConfigController.AssetBundleDownloadPath, this.DownloadABPath.ToLower());
        }

        public override string GetDownLoadUrlForInitial()
        {
            string url = string.Empty;
#if UNITY_IOS
            url = CookingGameConfigController.Instance.Res_Server_URL_Beta + "iphone";
#elif UNITY_ANDROID
            url = CookingGameConfigController.Instance.Res_Server_URL_Beta + "android";
#else
			url = CookingGameConfigController.Instance.Res_Server_URL_Beta + "android";
#endif
            return string.Format("{0}/{1}", url, this.DownloadABPath.ToLower());
        }

        public override string GetPlatformMd5()
        {
#if UNITY_IOS
            return this.Md5Ios;
#elif UNITY_ANDROID
            return this.Md5;
#else
			return this.Md5;
#endif
        }

        public CookingGameFakeInfo()
        {
            this.DependenciesBundleNames = new List<string>();
        }

        public CookingGameFakeInfo(bool inInitialPacket, string localPath, string remotePath)
        {
            this.InInitialPacket = inInitialPacket;
            var items = localPath.Split('/');
            string itemName = items[items.Length - 1];
            this.VersionCode = CookingGameConfigController.Instance.ResVersionCode;

            this.LocalABPath = string.Format("{0}.ab", localPath);
            this.DownloadABPath = string.Format("{0}/{1}.ab", this.VersionCode.ToString(), remotePath);
            this.TargetPath = string.Format("{0}/{1}.ab", this.VersionCode.ToString(), remotePath);
            this.DependenciesBundleNames = new List<string>();
        }
    }

    public class CookingGameConfigController : ScriptableObject
    {
        public static string AssetConfigPath = "Settings/CookingGameConfigController";
        private static CookingGameConfigController _instance = null;

        public static CookingGameConfigController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<CookingGameConfigController>(AssetConfigPath);
                }

                return _instance;
            }
        }

        public string ResServerURL
        {
            get
            {
                if (ConfigurationController.Instance.version == VersionStatus.RELEASE)
                {
                    return Res_Server_URL_Release;
                }
                else
                {
                    return Res_Server_URL_Beta;
                }
            }
        }

        public CookingGameConfigController()
        {
            VersionFileName = "pathfile/pathfile_cooking.json";
            Res_Server_URL_Release = "https://res.dragonplus.com/CommonCooking/";
            Res_Server_URL_Beta = "https://res.starcdn.cn/CommonCooking/";
        }

        [Space(10)] [Header("[服务器md5文件名]")] public string VersionFileName;

        [Space(10)] [Header("[线上资源服务器地址]")] public string Res_Server_URL_Release;

        [Space(10)] [Header("[测试资源服器地址]")] public string Res_Server_URL_Beta;

        [Space(10)] [Header("[填入相对Assets/Export的相对路径，大小写敏感，只支持文件夹, *表示数字]")]
        public CookingFolderInfo[] FolderPaths;

        [Space(10)] [Header("[Map Id 映射]")] public CookingMapInfo[] MapInfos;

        [Space(10)] [Header("[公共库Version Code]")]
        public int ResVersionCode;

        [Space(10)] [Header(" ---------------------- AB包version信息, 可自动生成 -----------------------")]
        public CookingGameFakeInfo[] FakeVersionInfos;

        // HomeRoomAssetBundle的下载路径
        public static string AssetBundleDownloadPath
        {
            get
            {
#if UNITY_IOS
                return CookingGameConfigController.Instance.ResServerURL + "iphone";
#elif UNITY_ANDROID
                return CookingGameConfigController.Instance.ResServerURL + "android";
#else
				return CookingGameConfigController.Instance.ResServerURL + "android";
#endif
            }
        }

        public CookingGameFakeInfo GetRoomResInfoByABPath(string abPath)
        {
            CookingGameFakeInfo result = null;
            if (this.FakeVersionInfos != null)
            {
                for (int i = 0; i < FakeVersionInfos.Length; i++)
                {
                    if (FakeVersionInfos[i].LocalABPath.ToLower().Equals(abPath.ToLower()))
                    {
                        result = FakeVersionInfos[i];
                        break;
                    }
                }
            }

            return result;
        }

        public void RefreshMd5(Dictionary<string, CookingGameVersionFileItem> items, bool isIos)
        {
            foreach (var p in this.FakeVersionInfos)
            {
                foreach (var item in items)
                {
                    if (item.Value.versionCode == p.VersionCode
                        && item.Value.targetPath.ToLower().Equals(p.TargetPath.ToLower()))
                    {
                        if (isIos)
                        {
                            p.Md5Ios = item.Value.md5;
                        }
                        else
                        {
                            p.Md5 = item.Value.md5;
                        }

                        break;
                    }
                }
            }
        }
    }

    public class CookingGameConfigEditor
    {
#if UNITY_EDITOR
        [MenuItem("AssetBundle/CookingGame/生成CookingGameConfig配置", false, 2)]
        static void GenerateCookingGameABConfig()
        {
            List<CookingGameFakeInfo> infos = new List<CookingGameFakeInfo>();
            foreach (var mapInfo in CookingGameConfigController.Instance.MapInfos)
            {
                foreach (var path in CookingGameConfigController.Instance.FolderPaths)
                {
                    var localPath = path.LocalPath.Replace("*", mapInfo.LocalId.ToString());
                    var remotePath = path.RemotePath.Replace("*", mapInfo.RemoteId.ToString());
                    infos.Add(new CookingGameFakeInfo(mapInfo.InInitialPacket, localPath, remotePath));
                }
            }

            CookingGameConfigController.Instance.FakeVersionInfos = infos.ToArray();
            AssetDatabase.Refresh();
            DebugUtil.LogError("共生成" + infos.Count.ToString() + "条");
            LoadRemoteVersionFile();
        }

        [MenuItem("AssetBundle/CookingGame/Help/刷新CGConfig_Md5", false, 1)]
        static void LoadRemoteVersionFile()
        {
            LoadRemoteVersionFile(false);
            LoadRemoteVersionFile(true);
        }

        static void LoadRemoteVersionFile(bool isIos)
        {
            string fileUrl = string.Empty;
            if (isIos)
            {
                fileUrl = string.Format("{0}/{1}", CookingGameConfigController.Instance.ResServerURL + "iphone", CookingGameConfigController.Instance.VersionFileName);
            }
            else
            {
                fileUrl = string.Format("{0}/{1}", CookingGameConfigController.Instance.ResServerURL + "android", CookingGameConfigController.Instance.VersionFileName);
            }

            new HTTPRequest(new Uri(fileUrl), (req, rep) =>
            {
                if (rep == null)
                {
                    DebugUtil.LogError("Response null. Server unreachable? Try again later. {0}", fileUrl);
                }
                else if (rep.StatusCode >= 200 && rep.StatusCode < 300)
                {
                    DebugUtil.LogError("Response Successfully! {0}", fileUrl);
                    DebugUtil.LogError("Content is {0}", rep.DataAsText);
                    Dictionary<string, CookingGameVersionFileItem> Items = new Dictionary<string, CookingGameVersionFileItem>();
                    JsonConvert.PopulateObject(rep.DataAsText, Items);
                    CookingGameConfigController.Instance.RefreshMd5(Items, isIos);
                    AssetDatabase.Refresh();
                }
                else
                {
                    DebugUtil.LogError("Unexpected response from server: {0}, StatusCode = {1}", fileUrl, rep.StatusCode);
                }
            })
            {
                DisableCache = true,
                IsCookiesEnabled = false,
                ConnectTimeout = TimeSpan.FromSeconds(5),
                Timeout = TimeSpan.FromSeconds(10)
            }.Send();
        }

        [MenuItem("AssetBundle/CookingGame/Help/生成AssetConfig_MapRes配置(ck5)", false, 2)]
        static void GenerateAssetConfigABConfig()
        {
            BundleGroup bundleGroup = new BundleGroup();
            bundleGroup.GroupName = "MapRes";
            bundleGroup.Version = "1.0.0";
            bundleGroup.Paths = new List<BundleState>();

            int groupIndex = -1;
            for (int i = 0; i < AssetConfigController.Instance.Groups.Length; i++)
            {
                if (AssetConfigController.Instance.Groups[i].GroupName.Equals(bundleGroup.GroupName))
                {
                    groupIndex = i;
                    break;
                }
            }

            if (groupIndex == -1)
            {
                DebugUtil.LogError("AssetConfigController中没有的MapRes,请添加");
                return;
            }

            bundleGroup.GroupIndex = groupIndex;
            var bs = new BundleState();
            foreach (var path in CookingGameConfigController.Instance.FolderPaths)
            {
                foreach (var mapInfo in CookingGameConfigController.Instance.MapInfos)
                {
                    var localPath = path.LocalPath.Replace("*", mapInfo.LocalId.ToString());
                    bs = new BundleState();
                    bs.Path = localPath;
                    bs.InInitialPacket = mapInfo.InInitialPacket;
                    bundleGroup.Paths.Add(bs);
                }
            }

            AssetConfigController.Instance.Groups[groupIndex] = bundleGroup;
            DebugUtil.LogError("AssetConfigController的MapRes打包配置已重新生成");
            AssetDatabase.Refresh();
        }

        [MenuItem("AssetBundle/CookingGame/Help/生成CookingGame公共库占位文件", false, 3)]
        static void InitFolder()
        {
            foreach (var mapInfo in CookingGameConfigController.Instance.MapInfos)
            {
                foreach (var path in CookingGameConfigController.Instance.FolderPaths)
                {
                    var localPath = path.LocalPath.Replace("*", mapInfo.LocalId.ToString());
                    CommonResUtils.AddPlaceholderFile(Application.dataPath + "/Export/" + localPath);
                }
            }

            AssetDatabase.Refresh();
        }

        [MenuItem("AssetBundle/CookingGame/Help/根据mapId生成CKIgnore文件(ck5)", false, 3)]
        static void AddIgnoreByMapId()
        {
            string filePath = Application.dataPath + "/A_CKIgnore.txt";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            StreamWriter sw;
            FileInfo t = new FileInfo(filePath);
            sw = t.CreateText();
            sw.WriteLine("#common cooking公共库需要忽略的文件, 请复制到.gitignore中");
            foreach (var mapInfo in CookingGameConfigController.Instance.MapInfos)
            {
                //以行的形式写入信息
                sw.WriteLine("Cooking5/Assets/Export/Audios/BGM/bgm_map" + mapInfo.RemoteId);
                sw.WriteLine("Cooking5/Assets/Export/Audios/BGM/bgm_map" + mapInfo.RemoteId + ".meta");
                sw.WriteLine("Cooking5/Assets/Export/SpriteAtlas/Cooking/Map" + mapInfo.RemoteId + "Atlas");
                sw.WriteLine("Cooking5/Assets/Export/SpriteAtlas/Cooking/Map" + mapInfo.RemoteId + "Atlas.meta");
                sw.WriteLine("Cooking5/Assets/Export/SpriteAtlas/Cooking/Map" + mapInfo.RemoteId + "GameAtlas");
                sw.WriteLine("Cooking5/Assets/Export/SpriteAtlas/Cooking/Map" + mapInfo.RemoteId + "GameAtlas.meta");
                sw.WriteLine("Cooking5/Assets/Export/UI/Cooking/Map" + mapInfo.RemoteId);
                sw.WriteLine("Cooking5/Assets/Export/UI/Cooking/Map" + mapInfo.RemoteId + ".meta");
                sw.WriteLine("Cooking5/Assets/Res/Cooking/Maps/Map" + mapInfo.RemoteId);
                sw.WriteLine("Cooking5/Assets/Res/Cooking/Maps/Map" + mapInfo.RemoteId + ".meta");
            }

            //关闭流
            sw.Close();
            //销毁流
            sw.Dispose();
            DebugUtil.LogError("Asset目录下已生成A_CKIgnore.txt, 需要手动拷贝到.gitignore文件中");
            AssetDatabase.Refresh();
        }
#endif
    }
}