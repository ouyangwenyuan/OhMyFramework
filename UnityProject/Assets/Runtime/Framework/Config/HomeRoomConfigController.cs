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
    public class HomeRoomVersionFileItem : BaseVersionFileItem
    {
    }

    [Serializable]
    public class RoomFolderInfo : BaseFolderInfo
    {
    }

    [Serializable]
    public class RoomResFakeInfo : BaseResFakeInfo
    {
        public override string GetDownLoadUrl()
        {
            return string.Format("{0}/{1}", HomeRoomConfigController.AssetBundleDownloadPath, this.DownloadABPath.ToLower());
        }

        public override string GetDownLoadUrlForInitial()
        {
            string url = string.Empty;
#if UNITY_IOS
            url = HomeRoomConfigController.Instance.Res_Server_URL_Beta + "iphone";
#elif UNITY_ANDROID
            url = HomeRoomConfigController.Instance.Res_Server_URL_Beta + "android";
#else
			url = HomeRoomConfigController.Instance.Res_Server_URL_Beta + "android";
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

        public RoomResFakeInfo()
        {
            this.DependenciesBundleNames = new List<string>();
        }

        public RoomResFakeInfo(RoomFolderInfo baseInfo, string itemName)
        {
            this.InInitialPacket = baseInfo.InInitialPacket;
            this.VersionCode = HomeRoomConfigController.Instance.ResVersionCode;
            this.DownloadABPath = string.Format("{0}/{1}/{2}.ab", this.VersionCode.ToString(), baseInfo.Path, itemName);
            this.LocalABPath = string.Format("{0}/{1}.ab", baseInfo.Path, itemName);
            this.TargetPath = string.Format("{0}/{1}/{2}.ab", this.VersionCode.ToString(), baseInfo.Path, itemName);
            this.DependenciesBundleNames = new List<string>();

            //temp code
            if (itemName.ToLower().Equals("item"))
            {
                this.DependenciesBundleNames.Add("dummymaterial.ab");
            }
            else if (itemName.ToLower().Equals("room"))
            {
                this.DependenciesBundleNames.Add("dummymaterial.ab");
                this.DependenciesBundleNames.Add(string.Format("{0}/{1}.ab", baseInfo.Path, "item").ToLower());
            }
        }
    }

    public class HomeRoomConfigController : ScriptableObject
    {
        public static string AssetConfigPath = "Settings/HomeRoomConfigController";
        private static HomeRoomConfigController _instance = null;

        public static HomeRoomConfigController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<HomeRoomConfigController>(AssetConfigPath);
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

        public HomeRoomConfigController()
        {
            RoomChildFolders = new List<string>() {"2DRes", "Item", "Room"};
            VersionFileName = "pathfile/pathfile_scene.json";
            Res_Server_URL_Release = "https://res.dragonplus.com/HomeRoomRes/";
            Res_Server_URL_Beta = "https://res.starcdn.cn/HomeRoomRes/";
        }

        [Space(10)] [Header("服务器md5文件名")] public string VersionFileName;

        [Space(10)] [Header("线上资源服务器地址")] public string Res_Server_URL_Release;

        [Space(10)] [Header("测试资源服器地址")] public string Res_Server_URL_Beta;

        [Space(10)] [Header("[填入相对Assets/Export的相对路径，大小写敏感，只支持文件夹]")] [Header(" ---------------------- 房间设置，手动填写 -----------------------")]
        public RoomFolderInfo[] RoomFolderInfos;

        [Space(10)] [Header("房间子文件夹(固定Item,Room,2DRes)")]
        public List<string> RoomChildFolders;

        [Space(10)] [Header("是否使用通用动画文件")] public bool UseCommonAnimator;

        [Space(10)] [Header("公共库Version Code")]
        public int ResVersionCode;

        [Space(10)] [Header(" ---------------------- 房间AB包version信息, 可自动生成 -----------------------")]
        public RoomResFakeInfo[] FakeVersionInfos;

        // HomeRoomAssetBundle的下载路径
        public static string AssetBundleDownloadPath
        {
            get
            {
#if UNITY_IOS
                return HomeRoomConfigController.Instance.ResServerURL + "iphone";
#elif UNITY_ANDROID
                return HomeRoomConfigController.Instance.ResServerURL + "android";
#else
				return HomeRoomConfigController.Instance.ResServerURL + "android";
#endif
            }
        }

        public RoomResFakeInfo GetRoomResInfoByABPath(string abPath)
        {
            RoomResFakeInfo result = null;
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

        public void RefreshMd5(Dictionary<string, HomeRoomVersionFileItem> items, bool isIos)
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

    public class HomeRoomConfigEditor
    {
#if UNITY_EDITOR
        [MenuItem("AssetBundle/HomeRoom/生成HomeRoomConfig配置", false, 2)]
        static void GenerateHomeRoomABConfig()
        {
            List<RoomResFakeInfo> infos = new List<RoomResFakeInfo>();
            foreach (var baseInfo in HomeRoomConfigController.Instance.RoomFolderInfos)
            {
                foreach (string childFolder in HomeRoomConfigController.Instance.RoomChildFolders)
                {
                    infos.Add(new RoomResFakeInfo(baseInfo, childFolder));
                }
            }

            RoomResFakeInfo dmInfo = new RoomResFakeInfo();
            dmInfo.InInitialPacket = true;
            dmInfo.VersionCode = HomeRoomConfigController.Instance.ResVersionCode;
            dmInfo.DownloadABPath = string.Format("{0}/DummyMaterial/DummyMaterial.ab", HomeRoomConfigController.Instance.ResVersionCode);
            dmInfo.LocalABPath = "DummyMaterial.ab";
            dmInfo.TargetPath = string.Format("{0}/DummyMaterial/DummyMaterial.ab", HomeRoomConfigController.Instance.ResVersionCode);
            infos.Add(dmInfo);

            if (HomeRoomConfigController.Instance.UseCommonAnimator)
            {
                var animatorInfo = new RoomResFakeInfo();
                animatorInfo.InInitialPacket = true;
                animatorInfo.VersionCode = HomeRoomConfigController.Instance.ResVersionCode;
                animatorInfo.DownloadABPath = string.Format("{0}/Scene/Effect/Animator.ab", HomeRoomConfigController.Instance.ResVersionCode);
                animatorInfo.LocalABPath = $"Scene/Effect/Animator.ab";
                animatorInfo.TargetPath = string.Format("{0}/Scene/Effect/Animator.ab", HomeRoomConfigController.Instance.ResVersionCode);
                infos.Add(animatorInfo);
            }

            HomeRoomConfigController.Instance.FakeVersionInfos = infos.ToArray();
            AssetDatabase.Refresh();
            DebugUtil.LogError("共生成" + infos.Count.ToString() + "条");
            LoadRemoteVersionFile();
        }

        [MenuItem("AssetBundle/HomeRoom/Help/刷新HRConfig_Md5", false, 1)]
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
                fileUrl = string.Format("{0}/{1}", HomeRoomConfigController.Instance.ResServerURL + "iphone", HomeRoomConfigController.Instance.VersionFileName);
            }
            else
            {
                fileUrl = string.Format("{0}/{1}", HomeRoomConfigController.Instance.ResServerURL + "android", HomeRoomConfigController.Instance.VersionFileName);
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
                    Dictionary<string, HomeRoomVersionFileItem> Items = new Dictionary<string, HomeRoomVersionFileItem>();
                    JsonConvert.PopulateObject(rep.DataAsText, Items);
                    HomeRoomConfigController.Instance.RefreshMd5(Items, isIos);
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

        [MenuItem("AssetBundle/HomeRoom/Help/生成AssetConfig_Scene配置(ck5)", false, 2)]
        static void GenerateAssetConfigABConfig()
        {
            BundleGroup bundleGroup = new BundleGroup();
            bundleGroup.GroupName = "Scene";
            bundleGroup.Version = "1.0.0";
            bundleGroup.GroupIndex = 2;
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
                DebugUtil.LogError("AssetConfigController中没有的Scene,请添加");
                return;
            }

            var bs = new BundleState();
            bs.Path = "Scene/Effect/Animator";
            bs.InInitialPacket = true;
            bundleGroup.Paths.Add(bs);
            bs = new BundleState();
            bs.Path = "Scene/Effect/Gfx";
            bs.InInitialPacket = true;
            bundleGroup.Paths.Add(bs);
            bs = new BundleState();
            bs.Path = "DummyMaterial";
            bs.InInitialPacket = true;
            bundleGroup.Paths.Add(bs);

            foreach (var baseInfo in HomeRoomConfigController.Instance.RoomFolderInfos)
            {
                DebugUtil.LogError("开始构建" + baseInfo.Path);
                bs = new BundleState();
                bs.Path = baseInfo.Path + "/BGM";
                bs.InInitialPacket = baseInfo.InInitialPacket;
                bundleGroup.Paths.Add(bs);
                bs = new BundleState();
                bs.Path = baseInfo.Path + "/Animation";
                bs.InInitialPacket = baseInfo.InInitialPacket;
                bundleGroup.Paths.Add(bs);
                foreach (string childFolder in HomeRoomConfigController.Instance.RoomChildFolders)
                {
                    bs = new BundleState();
                    bs.Path = baseInfo.Path + "/" + childFolder;
                    bs.InInitialPacket = baseInfo.InInitialPacket;
                    bundleGroup.Paths.Add(bs);
                }
            }

            AssetConfigController.Instance.Groups[sceneIndex] = bundleGroup;
            DebugUtil.LogError("AssetConfigController的Scene打包配置已重新生成");
            AssetDatabase.Refresh();
        }

        [MenuItem("AssetBundle/HomeRoom/Help/生成Scene公共库占位文件", false, 3)]
        static void InitFolder()
        {
            CommonResUtils.AddPlaceholderFile(Application.dataPath + "/Export/DummyMaterial");
            foreach (var baseInfo in HomeRoomConfigController.Instance.RoomFolderInfos)
            {
                foreach (string childFolder in HomeRoomConfigController.Instance.RoomChildFolders)
                {
                    CommonResUtils.AddPlaceholderFile(Application.dataPath + "/Export/" + baseInfo.Path + "/" + childFolder);
                }
            }

            AssetDatabase.Refresh();
        }
        
        [MenuItem("AssetBundle/HomeRoom/Help/根据房间Id生成HomeIgnore文件(ck5)", false, 3)]
        static void AddIgnoreByMapId()
        {
            string filePath = Application.dataPath + "/A_HomeIgnore.txt";
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            StreamWriter sw;
            FileInfo t = new FileInfo(filePath);
            sw = t.CreateText();
            sw.WriteLine("#home room公共库需要忽略的文件, 请复制到.gitignore中");
            sw.WriteLine("Cooking5/Assets/Export/Scene/ResItem");
            sw.WriteLine("Cooking5/Assets/Export/Scene/ResItem.meta");
            sw.WriteLine("Cooking5/Assets/Export/Scene/ResRoom");
            sw.WriteLine("Cooking5/Assets/Export/Scene/ResRoom.meta");
            sw.WriteLine("Cooking5/Assets/Export/DummyMaterial");
            sw.WriteLine("Cooking5/Assets/Export/DummyMaterial.meta");

            foreach (var folderInfo in HomeRoomConfigController.Instance.RoomFolderInfos)
            {
                //以行的形式写入信息
                sw.WriteLine("Cooking5/Assets/Export/" + folderInfo.Path + "/Item");
                sw.WriteLine("Cooking5/Assets/Export/" + folderInfo.Path + "/Item.meta");
                sw.WriteLine("Cooking5/Assets/Export/" + folderInfo.Path + "/Room");
                sw.WriteLine("Cooking5/Assets/Export/" + folderInfo.Path + "/Room.meta");
                sw.WriteLine("Cooking5/Assets/Export/" + folderInfo.Path + "/2DRes");
                sw.WriteLine("Cooking5/Assets/Export/" + folderInfo.Path + "/2DRes.meta");
            }

            //关闭流
            sw.Close();
            //销毁流
            sw.Dispose();
            
            DebugUtil.LogError("Asset目录下已生成A_HomeIgnore.txt, 需要手动拷贝到.gitignore文件中");
            AssetDatabase.Refresh();
        }

#endif
    }
}