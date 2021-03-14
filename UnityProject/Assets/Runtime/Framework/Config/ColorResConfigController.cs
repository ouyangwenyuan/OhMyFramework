using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BestHTTP;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace DragonU3DSDK.Asset
{
    [Serializable]
    public class ColorVersionFileItem : BaseVersionFileItem
    {
    }

    [Serializable]
    public class ColorFolderInfo : BaseFolderInfo
    {
    }

    [Serializable]
    public class ColorResFakeInfo : BaseResFakeInfo
    {
        public override string GetDownLoadUrl()
        {
            return string.Format("{0}/{1}", ColorResConfigController.AssetBundleDownloadPath, this.DownloadABPath.ToLower());
        }

        public override string GetDownLoadUrlForInitial()
        {
            string url = string.Empty;
#if UNITY_IOS
            url = ColorResConfigController.Instance.Res_Server_URL_Beta + "iphone";
#elif UNITY_ANDROID
            url = ColorResConfigController.Instance.Res_Server_URL_Beta + "android";
#else
			url = ColorResConfigController.Instance.Res_Server_URL_Beta + "android";
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

        public ColorResFakeInfo()
        {
            this.DependenciesBundleNames = new List<string>();
        }

        public ColorResFakeInfo(ColorFolderInfo baseInfo)
        {
            this.InInitialPacket = baseInfo.InInitialPacket;
            this.VersionCode = ColorResConfigController.Instance.ColorVersionCode;
            this.DownloadABPath = string.Format("{0}/{1}.ab", this.VersionCode.ToString(), baseInfo.Path);
            this.LocalABPath = string.Format("{0}.ab", baseInfo.Path);
            this.TargetPath = string.Format("{0}/{1}.ab", this.VersionCode.ToString(), baseInfo.Path);
            this.DependenciesBundleNames = new List<string>();
        }
    }

    public class ColorResConfigController : ScriptableObject
    {
        public static string AssetConfigPath = "Settings/ColorResConfigController";
        private static ColorResConfigController _instance = null;

        public static ColorResConfigController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<ColorResConfigController>(AssetConfigPath);
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

        public ColorResConfigController()
        {
            VersionFileName = "pathfile/pathfile_color.json";
            Res_Server_URL_Release = "https://res.dragonplus.com/ColorRes/";
            Res_Server_URL_Beta = "https://res.starcdn.cn/ColorRes/";
        }

        [Space(10)] [Header("服务器md5文件名")] public string VersionFileName;

        [Space(10)] [Header("线上资源服务器地址")] public string Res_Server_URL_Release;

        [Space(10)] [Header("测试资源服器地址")] public string Res_Server_URL_Beta;

        [Space(10)] [Header(" ------------------ ColorVersionCode ----------------")]
        public int ColorVersionCode = 1;

        [Space(10)] [Header("[填入相对Assets/Export的相对路径，大小写敏感，只支持文件夹]")] [Header(" ---------------------- Color设置，手动填写 -----------------------")]
        public ColorFolderInfo[] ColorResFolderInfos;

        public ColorFolderInfo[] ThumbnailResFolderInfos;

        [Space(10)] [Header(" ---------------------- 房间AB包version信息, 可自动生成 -----------------------")]
        public ColorResFakeInfo[] FakeVersionInfos;

        // ColorAssetBundle的下载路径
        public static string AssetBundleDownloadPath
        {
            get
            {
#if UNITY_IOS
                return ColorResConfigController.Instance.ResServerURL + "iphone";
#elif UNITY_ANDROID
                return ColorResConfigController.Instance.ResServerURL + "android";
#else
				return ColorResConfigController.Instance.ResServerURL + "android";
#endif
            }
        }

        public ColorResFakeInfo GetRoomResInfoByABPath(string abPath)
        {
            ColorResFakeInfo result = null;
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

        public void RefreshMd5(Dictionary<string, ColorVersionFileItem> items, bool isIos)
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


    public class ColorResConfigEditor
    {
#if UNITY_EDITOR
        [MenuItem("AssetBundle/ColorRes/生成ColorResConfig配置", false, 2)]
        static void GenerateColorABConfig()
        {
            List<ColorResFakeInfo> infos = new List<ColorResFakeInfo>();
            foreach (var baseInfo in ColorResConfigController.Instance.ColorResFolderInfos)
            {
                infos.Add(new ColorResFakeInfo(baseInfo));
            }

            foreach (var baseInfo in ColorResConfigController.Instance.ThumbnailResFolderInfos)
            {
                infos.Add(new ColorResFakeInfo(baseInfo));
            }

            ColorResConfigController.Instance.FakeVersionInfos = infos.ToArray();
            AssetDatabase.Refresh();
            DebugUtil.LogError("共生成" + infos.Count.ToString() + "条");
            LoadRemoteVersionFile();
        }

        [MenuItem("AssetBundle/ColorRes/Help/刷新ColorRes_Md5", false, 1)]
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
                fileUrl = string.Format("{0}/{1}", ColorResConfigController.Instance.ResServerURL + "iphone", ColorResConfigController.Instance.VersionFileName);
            }
            else
            {
                fileUrl = string.Format("{0}/{1}", ColorResConfigController.Instance.ResServerURL + "android", ColorResConfigController.Instance.VersionFileName);
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
                    Dictionary<string, ColorVersionFileItem> Items = new Dictionary<string, ColorVersionFileItem>();
                    JsonConvert.PopulateObject(rep.DataAsText, Items);
                    ColorResConfigController.Instance.RefreshMd5(Items, isIos);
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

        [MenuItem("AssetBundle/ColorRes/Help/生成AssetConfig_Scene配置", false, 2)]
        static void GenerateAssetConfigABConfig()
        {
            GenerateColorResAssetConfigABConfig();
            GenerateThumbnailResAssetConfigABConfig();
        }

        static void GenerateColorResAssetConfigABConfig()
        {
            BundleGroup bundleGroup = new BundleGroup();
            bundleGroup.GroupName = "ColorData";
            bundleGroup.Version = "1.0.0";
            bundleGroup.GroupIndex = 3;
            bundleGroup.Paths = new List<BundleState>();

            int sceneIndex = -1;
            for (int i = 0; i < AssetConfigController.Instance.Groups.Length; i++)
            {
                if (AssetConfigController.Instance.Groups[i].GroupName.Equals(bundleGroup.GroupName))
                {
                    sceneIndex = i;
                    break;
                }
            }

            if (sceneIndex == -1)
            {
                DebugUtil.LogError("AssetConfigController中没有的ColorData,请添加");
                return;
            }

            var bs = new BundleState();

            foreach (var baseInfo in ColorResConfigController.Instance.ColorResFolderInfos)
            {
                DebugUtil.LogError("开始构建" + baseInfo.Path);
                bs = new BundleState();
                bs.Path = baseInfo.Path;
                bs.InInitialPacket = baseInfo.InInitialPacket;
                bundleGroup.Paths.Add(bs);
            }

            AssetConfigController.Instance.Groups[sceneIndex] = bundleGroup;
            DebugUtil.LogError("AssetConfigController的ColorData打包配置已重新生成");
            AssetDatabase.Refresh();
        }

        static void GenerateThumbnailResAssetConfigABConfig()
        {
            // ThumbnailColorData
            BundleGroup bundleGroup = new BundleGroup();
            bundleGroup.GroupName = "ThumbnailColorData";
            bundleGroup.Version = "1.0.0";
            bundleGroup.GroupIndex = 4;
            bundleGroup.Paths = new List<BundleState>();

            int sceneIndex = -1;
            for (int i = 0; i < AssetConfigController.Instance.Groups.Length; i++)
            {
                if (AssetConfigController.Instance.Groups[i].GroupName.Equals(bundleGroup.GroupName))
                {
                    sceneIndex = i;
                    break;
                }
            }

            if (sceneIndex == -1)
            {
                DebugUtil.LogError("AssetConfigController中没有的ThumbnailColorData,请添加");
                return;
            }

            var bs = new BundleState();
            foreach (var baseInfo in ColorResConfigController.Instance.ThumbnailResFolderInfos)
            {
                DebugUtil.LogError("开始构建" + baseInfo.Path);
                bs = new BundleState();
                bs.Path = baseInfo.Path;
                bs.InInitialPacket = baseInfo.InInitialPacket;
                bundleGroup.Paths.Add(bs);
            }

            AssetConfigController.Instance.Groups[sceneIndex] = bundleGroup;

            DebugUtil.LogError("AssetConfigController的ThumbnailColorData打包配置已重新生成");
            AssetDatabase.Refresh();
        }

        [MenuItem("AssetBundle/ColorRes/Help/生成Scene公共库占位文件", false, 3)]
        static void InitFolder()
        {
            foreach (var baseInfo in ColorResConfigController.Instance.ColorResFolderInfos)
            {
                CommonResUtils.AddPlaceholderFile(Application.dataPath + "/Export/" + baseInfo.Path);
            }

            foreach (var baseInfo in ColorResConfigController.Instance.ThumbnailResFolderInfos)
            {
                CommonResUtils.AddPlaceholderFile(Application.dataPath + "/Export/" + baseInfo.Path);
            }
            AssetDatabase.Refresh();
        }
#endif
    }
}