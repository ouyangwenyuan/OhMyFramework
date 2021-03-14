/*-------------------------------------------------------------------------------------------
// Copyright (C) 2019 北京，天龙互娱
//
// 模块名：BuildAssetBundles
// 创建日期：2019-1-11
// 创建者：waicheng.wang
// 模块描述：AssetBundle打包，加密
//-------------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using System;
using System.Text;
using Dlugin;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using UnityEditor.U2D;
using UObject = UnityEngine.Object;

namespace DragonU3DSDK.Asset
{
    public static partial class BuildAssetBundles
    {
        // 版本文件
        private static VersionInfo RemoteVersion;
        private static VersionInfo LocalVersion;

        // assetbundle输出路径
#if UNITY_IOS
        static string exportPath { get { return FilePathTools.assetBundleOutPath + "/" + AssetConfigController.Instance.IOSRootVersion; } }
#else
        static string exportPath { get { return FilePathTools.assetBundleOutPath + "/" + AssetConfigController.Instance.RootVersion; } }
#endif

        private static BuildAssetBundleOptions recommandBundleOptions =
            BuildAssetBundleOptions.ChunkBasedCompression |
            BuildAssetBundleOptions.DeterministicAssetBundle 
#if !UNITY_STANDALONE            
            | BuildAssetBundleOptions.EnableProtection
#endif
                ;

        [MenuItem("AssetBundle/打包所有资源")]
        public static void BuildAllAssetBundle()
        {
            if (AssetConfigController.Instance.ProjectPretreatmentDelegate)
                AssetConfigController.Instance.ProjectPretreatmentDelegate.ExecutePretreatment();
#if !UNITY_STANDALONE             
            BuildPipeline.SetAssetBundleEncryptKey(AssetConfigController.Instance.EnhancedEncryptionSecret);
#endif
            BuildAllAssetBundleWithOptions(recommandBundleOptions);
            
            DebugUtil.Log("BuildAllAssetBundle finish");
        }

        static void BuildAllAssetBundleWithOptions(BuildAssetBundleOptions bundleOptions)
        {
            ReadAllFileMD5File();
            ReadAllSpriteAtlasNameFile();
            
            Caching.ClearCache();

            AssetDatabase.RemoveUnusedAssetBundleNames();

            // GenerateHomePlaceholderFile();
            // GenerateCookingGamePlaceholderFile();
            // GenerateColorPlaceholderFile();

            EditorUtility.DisplayCancelableProgressBar("打包中...", "0%", 0.01f);

            string uniqueID = System.Guid.NewGuid().ToString().Replace("-", "");
            RemoteVersion = new VersionInfo
            {
#if UNITY_IOS
                Version = AssetConfigController.Instance.IOSRootVersion,
#else
                Version = AssetConfigController.Instance.RootVersion,
#endif
                UniqueID = uniqueID
            };

            EditorUtility.DisplayCancelableProgressBar("打包中...", "5%", 0.05f);
            LocalVersion = new VersionInfo
            {
#if UNITY_IOS
                Version = AssetConfigController.Instance.IOSRootVersion,
#else
                Version = AssetConfigController.Instance.RootVersion,
#endif
                UniqueID = uniqueID
            };

            EditorUtility.DisplayCancelableProgressBar("打包中...", "10%", 0.1f);
            
            if (!Directory.Exists(FilePathTools.assetbundlePatchPath))
                Directory.CreateDirectory(FilePathTools.assetbundlePatchPath);
            
            if(Directory.Exists(exportPath))
                Directory.Delete(exportPath, true);
            if (!Directory.Exists(exportPath))
                Directory.CreateDirectory(exportPath);

            if (Directory.Exists(FilePathTools.streamingAssetsPath_Platform))
                Directory.Delete(FilePathTools.streamingAssetsPath_Platform, true);
            
            float deltaProgress = 0.8f / AssetConfigController.Instance.Groups.Length;
            for (int i = 0; i < AssetConfigController.Instance.Groups.Length; i++)
            {
                float currProgress = 0.1f + deltaProgress * i;
                EditorUtility.DisplayCancelableProgressBar("打包中...", currProgress * 100 + "%", currProgress);
                BuildGroup(AssetConfigController.Instance.Groups[i], bundleOptions);
            }

            //---------------------- 保存远端version.txt ----------------------------//
            string remoteVersionJsonStr = JsonConvert.SerializeObject(RemoteVersion);
            // 把version存到exportPath的上一级目录
            DirectoryInfo parent = Directory.GetParent(exportPath);
            string parentPath = parent.ToString();
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                AssetConfigController.Instance.VersionCode = PlayerSettings.Android.bundleVersionCode.ToString();
                CreateFile(parentPath, string.Format("Version.{0}.txt", AssetConfigController.Instance.VersionCode), remoteVersionJsonStr);
            }
            else
            {
                AssetConfigController.Instance.IOSVersionCode = PlayerSettings.iOS.buildNumber;
                CreateFile(parentPath, string.Format("Version.{0}.txt", AssetConfigController.Instance.IOSVersionCode), remoteVersionJsonStr);
            }

            EditorUtility.DisplayCancelableProgressBar("打包中...", "85%", 0.85f);

            //---------------------- 保存本地version.txt ----------------------------//
            string localPath = Application.dataPath + "/Resources/versionfile";
            string localVersionJsonStr = JsonConvert.SerializeObject(LocalVersion);
            CreateFile(localPath, "Version.txt", localVersionJsonStr);
            EditorUtility.DisplayCancelableProgressBar("打包中...", "90%", 0.90f);

            //------- 拷贝 InInitialPacket==true 的包到streamingasset文件夹下 ---------//
            foreach (BundleGroup group in AssetConfigController.Instance.Groups)
            {
                List<string> paths = group.GetInInitialPacketPaths();
                foreach (string item in paths)
                {
                    string srcFile = FilePathTools.assetbundlePatchPath + "/" + item.ToLower() + ".ab";
                    string destFile = FilePathTools.streamingAssetsPath_Platform + "/" + item.ToLower() + ".ab";
                    FilePathTools.CreateDirectory(destFile);
                    //Zip.Tool.DecompressFileLZMA(srcFile, destFile);
                    File.Copy(srcFile, destFile);
                }
            }

            Action endAction = () =>
            {
                EditorUtility.DisplayCancelableProgressBar("打包中...", "100%", 1f);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorUtility.ClearProgressBar();
            };

            //------- 下载 InitialPacket==true 的home包到streamingasset文件夹下 ---------//
            var hasHomeRoomConfig = HomeRoomConfigController.Instance != null;
            var hasCookingGameConfig = CookingGameConfigController.Instance != null;
            var hasColorResConfig = ColorResConfigController.Instance != null;

            if (hasHomeRoomConfig)
            {
                CommonResLog("qushuang -----> begin download homeroom initial ab");
                List<RoomResFakeInfo> initialInfos = new List<RoomResFakeInfo>();
                foreach (RoomResFakeInfo info in HomeRoomConfigController.Instance.FakeVersionInfos)
                {
                    if (info.InInitialPacket)
                    {
                        initialInfos.Add(info);
                    }
                }
                CommonResLog("qushuang -----> find {0} homeroom initial abs need to download", initialInfos.Count);
                if (initialInfos.Count > 0)
                {
                    EditorUtility.DisplayCancelableProgressBar("从home公共库同步初始资源中...", "92%", 0.92f);
                    foreach (RoomResFakeInfo info in initialInfos)
                    {
                        DownloadCommonInitialAB(info);
                    }
                }
            }
            if (hasCookingGameConfig)
            {
                CommonResLog("qushuang -----> begin download cooking initial ab");
                List<CookingGameFakeInfo> initialInfos = new List<CookingGameFakeInfo>();
                foreach (CookingGameFakeInfo info in CookingGameConfigController.Instance.FakeVersionInfos)
                {
                    if (info.InInitialPacket)
                    {
                        initialInfos.Add(info);
                    }
                }
                CommonResLog("qushuang -----> find {0} cooking initial abs need to download", initialInfos.Count);
                if (initialInfos.Count > 0)
                {
                    EditorUtility.DisplayCancelableProgressBar("从cooking公共库同步初始资源中...", "94%", 0.94f);
                    foreach (CookingGameFakeInfo info in initialInfos)
                    {
                        DownloadCommonInitialAB(info);
                    }
                }
            }
            if (hasColorResConfig)
            {
                CommonResLog("qushuang -----> begin download color initial ab");
                List<ColorResFakeInfo> initialInfos = new List<ColorResFakeInfo>();
                foreach (ColorResFakeInfo info in ColorResConfigController.Instance.FakeVersionInfos)
                {
                    if (info.InInitialPacket)
                    {
                        initialInfos.Add(info);
                    }
                }
                CommonResLog("qushuang -----> find {0} color initial abs need to download", initialInfos.Count);
                if (initialInfos.Count > 0)
                {
                    EditorUtility.DisplayCancelableProgressBar("color公共库同步初始资源中...", "96%", 0.96f);
                    foreach (ColorResFakeInfo info in initialInfos)
                    {
                        DownloadCommonInitialAB(info);
                    }
                }
            }

            endAction(); 
            AfterBuildAllAssetBundleCheck();

        }

        static void BuildGroup(BundleGroup singleGroup, BuildAssetBundleOptions bundleOptions)
        {
            List<string> paths = singleGroup.GetBundlePaths();
            if (paths == null || paths.Count <= 0) return;

            string groupName = singleGroup.GroupName;
            string version = singleGroup.Version;
            bool updateWholeGroup = singleGroup.UpdateWholeAB;

            Dictionary<string, string> bundleHash = new Dictionary<string, string>();
            Dictionary<string, JObject> needPatchFileInfo = new Dictionary<string, JObject>();
            
            List<AssetBundleBuild> buildMap = new List<AssetBundleBuild>();

            //---------------------- 打成AssetBundle包 ----------------------------//
            for (int i = 0; i < paths.Count; i++)
            {
                List<string> assetnames = new List<string>();
                AssetBundleBuild build = new AssetBundleBuild
                {
                    assetBundleName = paths[i],
                    assetBundleVariant = "ab",
                };

                FileInfo[] files = FilePathTools.GetFiles(string.Format("{0}/{1}/{2}", Application.dataPath, "PackRes", paths[i]));
                for (int j = 0; j < files.Length; j++)
                {
                    //DebugUtil.Log((files[j]);
                    string path = FilePathTools.GetRelativePath(files[j].FullName);
                    if(path.Contains(".meta")){

                    }
                    if (path.Contains(".DS_Store") || path.Contains(".gitkeep"))
                    {
                        continue;
                    }
                    assetnames.Add(path);
                }

                build.assetNames = assetnames.ToArray();
                buildMap.Add(build);
                
                PatchBundleHash(paths, paths[i], paths[i].ToLower() + ".ab", build.assetNames, FilePathTools.assetbundlePatchPath, bundleHash, needPatchFileInfo, EditorUserBuildSettings.activeBuildTarget);
            }

            AssetBundleManifest assetBundleManifest = BuildPipeline.BuildAssetBundles(FilePathTools.assetbundlePatchPath, buildMap.ToArray(), bundleOptions, EditorUserBuildSettings.activeBuildTarget);
            //------------------------------- end ---------------------------------//


            //-------------------- 更新远端和本地的version.txt -----------------------//
            VersionItemInfo remoteVii = new VersionItemInfo
            {
                Version = version,
                UpdateWholeGroup = updateWholeGroup
            };

            VersionItemInfo localVii = new VersionItemInfo
            {
                Version = version,
                UpdateWholeGroup = updateWholeGroup
            };

            string[] builtAssetBundleNames = assetBundleManifest.GetAllAssetBundles();
            for (var i = 0; i < builtAssetBundleNames.Length; ++i)
            {
                string assetBundlePath = string.Format("{0}/{1}", FilePathTools.assetbundlePatchPath, builtAssetBundleNames[i]);
                
                if (needPatchFileInfo.ContainsKey(builtAssetBundleNames[i]))
                {
                    //压缩 ab
                    //string tempPath = assetBundlePath + ".temp";
                    //File.Move(assetBundlePath, tempPath);
                    //Zip.Tool.CompressFileLZMA(tempPath, assetBundlePath);
                    //File.Delete(tempPath);
                    
                    //备份 ab
                    string back_ab_path = assetBundlePath + ".back";
                    if (File.Exists(back_ab_path))
                    {
                        File.Delete(back_ab_path);
                    }
                    File.Copy(assetBundlePath, back_ab_path);
                    
                    //生成ab的 patch文件
                    needPatchFileInfo[builtAssetBundleNames[i]]["md5"] = AssetUtils.BuildFileMd5(assetBundlePath);
                    StreamWriter writer = File.CreateText(assetBundlePath + ".patch");
                    writer.Write(needPatchFileInfo[builtAssetBundleNames[i]].ToString());
                    writer.Close();
                    writer.Dispose();
                }
                
                //必须保证hash和ab的md5是一一对应的
                //自己计算的hash没变，但是unity重新生成了ab，那么需要将back ab覆盖生成的ab，但是保留新的manifest
                //以上这么做的原因是：为了让ab兼容不同版本的unity，每次发版时都要检查本次打包资源是否有效，那么就需要做到ab资源版本只有唯一一份
                {
                    string cur_ab_file_md5 = AssetUtils.BuildFileMd5(assetBundlePath);
                    string back_ab_path = assetBundlePath + ".back";
                    string persisten_ab_file_md5 = AssetUtils.BuildFileMd5(back_ab_path);
                    if (cur_ab_file_md5 != persisten_ab_file_md5)
                    {
                        File.Delete(assetBundlePath);
                        File.Copy(back_ab_path, assetBundlePath);
                    
                        DebugUtil.Log(string.Format("force keep old ab : {0}", assetBundlePath));
                    }
                }

                string destPath = string.Format("{0}/{1}", exportPath, builtAssetBundleNames[i]);
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                File.Copy(assetBundlePath, destPath);

                string hash = string.Empty;
                string md5 = string.Empty;
                string[] dpBundleNames = { };
                //------- home包的md5和DependenciesBundleNames换成公共库的 ---------//
                if (HomeRoomConfigController.Instance != null)
                {
                    var homeRoomStateInfo = HomeRoomConfigController.Instance.GetRoomResInfoByABPath(builtAssetBundleNames[i]);
                    if (homeRoomStateInfo != null)
                    {
                        hash = homeRoomStateInfo.GetPlatformMd5();
                        md5 = AssetUtils.BuildFileMd5(assetBundlePath);
                        CommonResLog("qushuang -----> change to home res md5 {0}", homeRoomStateInfo.LocalABPath);
                        dpBundleNames = homeRoomStateInfo.DependenciesBundleNames.ToArray();
                        CommonResLog("qushuang -----> change to home res dpBundleNames {0}", homeRoomStateInfo.LocalABPath);
                    }
                }
                //------- cooking包的md5和DependenciesBundleNames换成公共库的 ---------//
                if (CookingGameConfigController.Instance != null)
                {
                    var cgStateInfo = CookingGameConfigController.Instance.GetRoomResInfoByABPath(builtAssetBundleNames[i]);
                    if (cgStateInfo != null)
                    {
                        md5 = cgStateInfo.GetPlatformMd5();
                        CommonResLog("qushuang -----> change to cooking res md5 {0}", cgStateInfo.LocalABPath);
                        dpBundleNames = cgStateInfo.DependenciesBundleNames.ToArray();
                        CommonResLog("qushuang -----> change to cooking res dpBundleNames {0}", cgStateInfo.LocalABPath);
                    }
                }
                //------- color包的md5和DependenciesBundleNames换成公共库的 ---------//
                if (ColorResConfigController.Instance != null)
                {
                    var colorResStateInfo = ColorResConfigController.Instance.GetRoomResInfoByABPath(builtAssetBundleNames[i]);
                    if (colorResStateInfo != null)
                    {
                        hash = colorResStateInfo.GetPlatformMd5();
                        md5 = AssetUtils.BuildFileMd5(assetBundlePath);
                        CommonResLog("qushuang -----> change to color res md5 {0}", colorResStateInfo.LocalABPath);
                        dpBundleNames = colorResStateInfo.DependenciesBundleNames.ToArray();
                        CommonResLog("qushuang -----> change to color res dpBundleNames {0}", colorResStateInfo.LocalABPath);
                    }
                }

                if (string.IsNullOrEmpty(hash))
                {
                    hash = bundleHash[builtAssetBundleNames[i]];
                    md5 = AssetUtils.BuildFileMd5(assetBundlePath);
                }
                if (dpBundleNames.Length == 0)
                {
                    dpBundleNames = assetBundleManifest.GetAllDependencies(builtAssetBundleNames[i]);
                }
                //DebugUtil.Log("assetBundlePath : " + assetBundlePath + ", md5 : " + md5);

                AssetBundleInfo remoteAssetBundleInfo = new AssetBundleInfo
                {
                    AssetBundleName = builtAssetBundleNames[i],
                    DependenciesBundleNames = dpBundleNames,
                    HashString = hash,
                    Md5 = md5,
                    State = AssetState.ExistInDownLoad
                };
                remoteVii.Add(remoteAssetBundleInfo.AssetBundleName, remoteAssetBundleInfo);

                /*
                 * author：qibo.li
                 * date：2020-6-23
                 * des：
                 * 1.因为需要优化覆盖安装后，继续使用下载目录的ab资源，所以需要标记资源的多种状态
                 * 2.废弃以前用HashString为""与否，来判断资源是否已下载。
                 * 3.采用AssetState来标记
                 */
                AssetBundleInfo localAssetBundleInfo = new AssetBundleInfo
                {
                    AssetBundleName = builtAssetBundleNames[i],
                    DependenciesBundleNames = dpBundleNames,
                    HashString = hash,
                    Md5 = md5
                };
                if (singleGroup.IsInInitialPacket(builtAssetBundleNames[i]))
                {
                    localAssetBundleInfo.State = AssetState.ExistInDownLoad;
                }
                else
                {
                    localAssetBundleInfo.State = AssetState.NotExist;
                }
                localVii.Add(localAssetBundleInfo.AssetBundleName, localAssetBundleInfo);
            }
            RemoteVersion.Add(groupName, remoteVii);
            LocalVersion.Add(groupName, localVii);
            //------------------------------- end ---------------------------------//
        }

        static void CreateFile(string path, string filename, string info)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            //文件流信息
            StreamWriter sw;
            FileInfo t = new FileInfo(path + "/" + filename);
            sw = t.CreateText();

            //DebugUtil.Log("Version path=" + t);

            //以行的形式写入信息
            sw.WriteLine(info);
            //关闭流
            sw.Close();
            //销毁流
            sw.Dispose();
        }

        /// <summary>
        /// 记录所有用于打包的文件的md5值，这个要在BuildPipeline.BuildAssetBundles之前调用
        /// 保证计算hash的数据为git上的数据
        /// </summary>
        static Dictionary<string, string> allFilesMD5 = new Dictionary<string, string>();
        public static void MarkFilesMD5ForBuildAssetbundle()
        {
            allFilesMD5.Clear();
            
            List<string> allABPaths = new List<string>();
            foreach (var g in AssetConfigController.Instance.Groups)
            {
                allABPaths.AddRange(g.GetBundlePaths());
            }
            if (AssetConfigController.Instance.ActivityResPaths != null)
            {
                foreach (var p in AssetConfigController.Instance.ActivityResPaths)
                {
                    allABPaths.Add(p);
                }
            }
            
            foreach (var abPath in allABPaths)
            {
                List<string> assetnames = new List<string>();

                FileInfo[] files = FilePathTools.GetFiles(string.Format("{0}/{1}/{2}", Application.dataPath, "PackRes", abPath));
                for (int j = 0; j < files.Length; j++)
                {
                    string path = FilePathTools.GetRelativePath(files[j].FullName);
                    if (path.Contains(".DS_Store") || path.Contains(".gitkeep"))
                    {
                        continue;
                    }

                    assetnames.Add(path);
                }

                string[] allDeps = GetDependencies(assetnames.ToArray(), true);
                foreach (var p in allDeps)
                {
                    if (!File.Exists(p)) continue;

                    if (p.Substring(p.LastIndexOf(".")) == ".cs") continue;
                    
                    {
                        string path = p;
                        if (!allFilesMD5.ContainsKey(path))
                        {
                            string md5 = AssetUtils.BuildFileMd5(path);
                            allFilesMD5.Add(path, md5);
                        }
                    }

                    {
                        string path = p + ".meta";
                        if (!allFilesMD5.ContainsKey(path))
                        {
                            string md5 = AssetUtils.BuildFileMd5(path);
                            allFilesMD5.Add(path, md5);
                        }
                    }
                }
            }
            
            string info = JsonConvert.SerializeObject(allFilesMD5);
            CreateFile(FilePathTools.assetbundlePatchPath, "AllFilesMD5.txt", info);
        }

        public static void ReadAllFileMD5File()
        {
            string path = FilePathTools.assetbundlePatchPath + "/AllFilesMD5.txt";
            if (File.Exists(path))
            {
                allFilesMD5 = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
                
                DebugUtil.Log(string.Format("ReadAllFileMD5File size : {0}", allFilesMD5.Count));
            }
        }

        /// <summary>
        /// 记录所有Sprite的atlas名字
        /// 用于sprite查询归属与哪个图集
        /// </summary>
        static Dictionary<string, HashSet<string>> allSpriteAtlasName = new Dictionary<string, HashSet<string>>();
        public static void MarkSpriteAtlasName()
        {
            allSpriteAtlasName.Clear();
            
            List<string> allABPaths = new List<string>();
            foreach (var g in AssetConfigController.Instance.Groups)
            {
                allABPaths.AddRange(g.GetBundlePaths());
            }
            if (AssetConfigController.Instance.ActivityResPaths != null)
            {
                foreach (var p in AssetConfigController.Instance.ActivityResPaths)
                {
                    allABPaths.Add(p);
                }
            }

            foreach (var abPath in allABPaths)
            {
                FileInfo[] files = FilePathTools.GetFiles(string.Format("{0}/{1}/{2}", Application.dataPath, "PackRes", abPath));
                for (int j = 0; j < files.Length; j++)
                {
                    if (files[j].Extension.Equals(".spriteatlas"))
                    {
                        string path = FilePathTools.GetRelativePath(files[j].FullName);
                        string[] deps = GetDependencies(new []{ path }, true);
                        foreach (var p in deps)
                        {
                            if (!Path.GetExtension(p).Equals(".spriteatlas") && File.Exists(p))
                            {
                                HashSet<string> tempSet;
                                if (!allSpriteAtlasName.TryGetValue(p, out tempSet))
                                {
                                    tempSet = new HashSet<string>();
                                    allSpriteAtlasName.Add(p, tempSet);
                                }
                                tempSet.Add(path);
                            }
                        }
                    }
                }
            }

            string info = JsonConvert.SerializeObject(allSpriteAtlasName);
            CreateFile(FilePathTools.assetbundlePatchPath, "AllSpriteAtlasName.txt", info);
        }
        
        public static void ReadAllSpriteAtlasNameFile()
        {
            string path = FilePathTools.assetbundlePatchPath + "/AllSpriteAtlasName.txt";
            if (File.Exists(path))
            {
                allSpriteAtlasName = JsonConvert.DeserializeObject<Dictionary<string, HashSet<string>>>(File.ReadAllText(path));
                
                DebugUtil.Log(string.Format("ReadAllSpriteAtlasNameFile size : {0}", allSpriteAtlasName.Count));
            }
        }

        static string[] GetDependencies(string[] pathNames, bool recursive)
        {
            List<string> allDeps = new List<string>();
            allDeps.AddRange(AssetDatabase.GetDependencies(pathNames, recursive));
            //补充处理
            foreach (var asset in pathNames)
            {
                string strExt = Path.GetExtension(asset);
                switch (strExt)
                {
                    //图集依赖文件夹，GetDependencies不一定能获取到真实依赖，但文件夹一定能获取到
                    case ".spriteatlas":
                    {
                        string[] deps = AssetDatabase.GetDependencies(asset, false);
                        foreach (var dep in deps)
                        {
                            if (Directory.Exists(dep))
                            {
                                string[] depSprites = AssetDatabase.FindAssets("t:Sprite", new[] { dep });
                                foreach (var tex in depSprites)
                                {
                                    string path = AssetDatabase.GUIDToAssetPath(tex);
                                    if (!allDeps.Exists(x => { return x.Equals(path); }))
                                    {
                                        allDeps.Add(path);
                                    }
                                }
                            }
                        }
                    } break;
                }
            }

            // if (AssetBundleSet.Instance.SortDeps)
            // {
            //     allDeps.Sort();
            // }
            return allDeps.ToArray();
        }
        
        public static void BuildMultipleActivityAssetBundleWithCommandLine()
        {
            string targetPlatform = string.Empty;
            string[] cmdArguments = Environment.GetCommandLineArgs();
            for (int count = 0; count < cmdArguments.Length; count++)
            {
                if (cmdArguments[count] == "-targetPlatform")
                {
                    targetPlatform = cmdArguments[count + 1];
                }
            }

            if (targetPlatform != "android" && targetPlatform != "iphone")
            {
                DragonU3DSDK.DebugUtil.LogError("BuildMultipleActivityAssetBundleWithCommandLine 目标平台错误 : " + targetPlatform);
                return;
            }
            
            if (AssetConfigController.Instance.ActivityResPaths != null &&
                AssetConfigController.Instance.ActivityResPaths.Length > 0)
            {
                ReadAllFileMD5File();
                ReadAllSpriteAtlasNameFile();
                
#if !UNITY_STANDALONE                
                BuildPipeline.SetAssetBundleEncryptKey(AssetConfigController.Instance.EnhancedEncryptionSecret);
#endif
                for (int i = 0; i < AssetConfigController.Instance.ActivityResPaths.Length; i++)
                {
                    BuildActivityAssetBundleWithCommandLine(AssetConfigController.Instance.ActivityResPaths[i], targetPlatform);
                }
            }
            
            AfterBuildActivityAssetBundleCheck();
            
            DebugUtil.Log("BuildMultipleActivityAssetBundleWithCommandLine finish");
        }

        static void BuildActivityAssetBundleWithCommandLine(string path = "", string targetPlatform = "")
        {
            List<AssetBundleBuild> buildMap = new List<AssetBundleBuild>();
            string topPath = Application.dataPath + "/PackRes/" + path;
            DirectoryInfo dir = new DirectoryInfo(topPath);
            if (!dir.Exists)
            {
                DebugUtil.Log("该文件夹不存在 : " + topPath);
                return;
            }

            FileInfo[] filesInfo = dir.GetFiles("*.*", SearchOption.AllDirectories);
            if (filesInfo.Length <= 0)
            {
                DebugUtil.LogError("不要对空文件夹进行打包 : " + topPath);
                return;
            }

            string outPath = FilePathTools.assetbundlePatchPath + "/Activity";
            topPath = topPath.Substring((Application.dataPath + "/PackRes/").Length);

            string abFileNameNoSuffix = topPath.Replace('/', '_').ToLower();
            string abFileName = string.Format("{0}.ab", abFileNameNoSuffix);

            List<string> assetnames = new List<string>();
            AssetBundleBuild build = new AssetBundleBuild
            {
                assetBundleName = abFileName
            };

            for (int i = 0; i < filesInfo.Length; i++)
            {
                string filePath = filesInfo[i].ToString();
                string objPath = "Assets/" + filePath.Substring(Application.dataPath.Length + 1);
                if (!objPath.EndsWith(".meta", StringComparison.Ordinal))
                {
                    assetnames.Add(objPath);
                }
            }
            build.assetNames = assetnames.ToArray();
            buildMap.Add(build);

            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);
            
            Dictionary<string, string> bundleHash = new Dictionary<string, string>();
            Dictionary<string, JObject> needPatchFileInfo = new Dictionary<string, JObject>();
            
            PatchBundleHash(new List<string>(){ path }, path, abFileName, build.assetNames, outPath, bundleHash, needPatchFileInfo,targetPlatform == "android" ? BuildTarget.Android : BuildTarget.iOS);
            
            AssetBundleManifest mainfest = null;
            if (targetPlatform == "android")
                mainfest = BuildPipeline.BuildAssetBundles(outPath, buildMap.ToArray(), recommandBundleOptions, BuildTarget.Android);
            else if (targetPlatform == "iphone")
                mainfest = BuildPipeline.BuildAssetBundles(outPath, buildMap.ToArray(), recommandBundleOptions, BuildTarget.iOS);
            else
            {
                DebugUtil.LogWarning("目标平台错误 : " + targetPlatform);
                return;
            }

            string assetBundlePath = string.Format("{0}/{1}", outPath, abFileName);
            if (needPatchFileInfo.ContainsKey(abFileName))
            {
                //压缩 ab
                //string tempPath = assetBundlePath + ".temp";
                //File.Move(assetBundlePath, tempPath);
                //Zip.Tool.CompressFileLZMA(tempPath, assetBundlePath);
                //File.Delete(tempPath);
                
                //备份 ab
                string back_ab_path = assetBundlePath + ".back";
                if (File.Exists(back_ab_path))
                {
                    File.Delete(back_ab_path);
                }
                File.Copy(assetBundlePath, back_ab_path);
                
                //生成ab的 patch文件
                needPatchFileInfo[abFileName]["md5"] = AssetUtils.BuildFileMd5(assetBundlePath);
                StreamWriter writer = File.CreateText(assetBundlePath + ".patch");
                writer.Write(needPatchFileInfo[abFileName].ToString());
                writer.Close();
                writer.Dispose();
            }

            //必须保证hash和ab的md5是一一对应的
            //自己计算的hash没变，但是unity重新生成了ab，那么需要将back ab覆盖生成的ab，但是保留新的manifest
            {
                string cur_ab_file_md5 = AssetUtils.BuildFileMd5(assetBundlePath);
                string back_ab_path = assetBundlePath + ".back";
                string persisten_ab_file_md5 = AssetUtils.BuildFileMd5(back_ab_path);
                if (cur_ab_file_md5 != persisten_ab_file_md5)
                {
                    File.Delete(assetBundlePath);
                    File.Copy(back_ab_path, assetBundlePath);
                    
                    DebugUtil.Log(string.Format("force keep old ab : {0}", assetBundlePath));
                }
            }
            
            //拷贝ab到输出目录
            {
                string md5 = AssetUtils.BuildFileMd5(assetBundlePath);
                string hash = bundleHash[abFileName];
                string destPath = string.Format("{0}/{1}_{2}_{3}.ab", FilePathTools.assetBundleOutPath + "/Activity", abFileNameNoSuffix, hash, md5);
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                if (File.Exists(destPath))
                {
                    File.Delete(destPath);
                }
                File.Copy(assetBundlePath, destPath);
            }
        }
        
        /// <summary>
        /// 迭代bundle的hash值
        /// </summary>
        /// <param name="buildPathsList">一次BuildPipeline.BuildAssetBundles所有打包路径</param>
        /// <param name="curBuildPath">本次打包的路径</param>
        /// <param name="assets">本次打包包含的所有assets</param>
        /// <param name="bundleHash">一次BuildPipeline.BuildAssetBundles所有bundle的hash值</param>
        /// <param name="needPatchFileInfo">一次BuildPipeline.BuildAssetBundles所有需要迭代的patch文件信息</param>
        static void PatchBundleHash(List<string> buildPathsList, string curBuildPath, string bundleName, string[] assets, string absoluteRootPath, 
            Dictionary<string, string> bundleHash, Dictionary<string, JObject> needPatchFileInfo, BuildTarget buildTarget)
        {
            string patchFile = string.Format("{0}/{1}.patch", absoluteRootPath, bundleName);
            string hash_persistent = "";
            //string mad_persistent = "";
            string hash_cur = "";
            if (File.Exists(patchFile))
            {
                JObject jobj = JObject.Parse(File.ReadAllText(patchFile));
                hash_persistent = jobj["hash"].ToString();
                //mad_persistent = jobj["md5"].ToString();
            }

            string[] allDeps = GetDependencies(assets, true);
            List<string> validDeps = new List<string>();
            foreach (var p in allDeps)
            {
                if (!File.Exists(p))
                {
                    continue;
                }
                
                //依赖的资源如果在本次的其它ab中，则不能算作计算hash的依赖项
                if (null == buildPathsList.Find(x =>
                    x != curBuildPath && 0 == p.IndexOf(string.Format("Assets/PackRes/{0}/", x))))
                {
                    validDeps.Add(p);
                }
            }
            
            StringBuilder markData = new StringBuilder();
            Dictionary<string, string> validDepsMD5 = new Dictionary<string, string>();
            foreach (var p in validDeps)
            {
                string depExt = Path.GetExtension(p);
                if (depExt.Equals(".cs"))
                {
                    markData.Append(p);
                }
                else
                {
                    {
                        string path = p;

                        if (allFilesMD5.Count > 0 && !allFilesMD5.ContainsKey(path))
                        {
                            DebugUtil.Log("{0} not in allFilesMD5 with PatchBundleHash oper!!! - param : {1}", path, bundleName);
                            DebugUtil.Log("AssetConfigController.asset在某些项目中是动态配置，这个有可能影响打包，试试将最终配置上传git，可以解决这个问题");
                        }
                        
                        string md5 = allFilesMD5.Count > 0 ? allFilesMD5[path] : AssetUtils.BuildFileMd5(path);//兼容本地打包和打包机上打包【本地打包：即在自己机器上打包】
                        validDepsMD5.Add(path, md5);
                        markData.Append(path + md5);
                    }

                    {
                        string path = p + ".meta";
                        
                        if (allFilesMD5.Count > 0 && !allFilesMD5.ContainsKey(path))
                        {
                            DebugUtil.Log("{0} not in allFilesMD5 with PatchBundleHash oper!!! - param : {1}", path, bundleName);
                            DebugUtil.Log("AssetConfigController.asset在某些项目中是动态配置，这个有可能影响打包，试试将最终配置上传git，可以解决这个问题");
                        }
                        
                        string md5 = allFilesMD5.Count > 0 ? allFilesMD5[path] : AssetUtils.BuildFileMd5(path);
                        validDepsMD5.Add(path, md5);
                        markData.Append(path + md5);
                    }
                }
            }
            
            //预制件依赖的图集
            //这里只需要关心依赖了哪些图集，不需要关心图集的修改
            List<string> atlasDeps = new List<string>();
            foreach (var p in validDeps)
            {
                string strExt = Path.GetExtension(p);
                if (strExt.Equals(".prefab"))
                {
                    string[] deps = AssetDatabase.GetDependencies(p, true);
                    foreach (var dep in deps)
                    {
                        if (File.Exists(dep))
                        {
                            HashSet<string> tempSet;
                            if (allSpriteAtlasName.TryGetValue(dep, out tempSet))
                            {
                                foreach (var atlasPath in tempSet)
                                {
                                    if (!validDeps.Exists(x => { return x.Equals(atlasPath); }) && !atlasDeps.Exists(x => { return x.Equals(atlasPath); }))
                                    {
                                        atlasDeps.Add(atlasPath);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            atlasDeps.Sort();
            foreach (var p in atlasDeps)
            {
                markData.Append(p);
            }
            
            //asset patch version config
            List<AssetPatchVersion> patchVersion = buildTarget == BuildTarget.Android
                ? AssetPatchVersionConfig.Instance?.AndroidAssetsPatchVersion
                : AssetPatchVersionConfig.Instance?.IOSAssetsPatchVersion;
            int version = 1;
            if (null != patchVersion)
            {
                AssetPatchVersion data =patchVersion.Find(x => x.name == curBuildPath);
                if (null != data)
                {
                    version = data.version;
                }
            }
            markData.Append(version.ToString());

            hash_cur = AssetUtils.FormatMD5(AssetUtils.CreateMD5(Encoding.ASCII.GetBytes(markData.ToString())));

            if (hash_persistent != hash_cur)
            {
                string path_old = string.Format("{0}/{1}", absoluteRootPath, bundleName);
                if(File.Exists(path_old))
                    File.Delete(path_old);
                path_old = path_old + ".back";
                if(File.Exists(path_old))
                    File.Delete(path_old);
                path_old = string.Format("{0}/{1}.manifest", absoluteRootPath,bundleName);
                if(File.Exists(path_old))
                    File.Delete(path_old);
                path_old = string.Format("{0}/{1}.patch", absoluteRootPath, bundleName);
                if(File.Exists(path_old))
                    File.Delete(path_old);
                
                JObject jobRoot = new JObject();
                JObject jobjDeps = new JObject();
                foreach (var p in validDeps)
                {
                    JObject jobItem = new JObject();
                    string strExt = Path.GetExtension(p);
                    if (strExt.Equals(".cs"))
                    {
                            
                    }
                    else
                    {
                        jobItem["md5_self"] = validDepsMD5[p];
                        jobItem["md5_meta"] = validDepsMD5[p + ".meta"];
                    }
                    jobjDeps[p] = jobItem;
                }
                
                foreach (var p in atlasDeps)
                {
                    JObject jobItem = new JObject();
                    jobjDeps[p] = jobItem;
                }

                jobRoot["assets"] = jobjDeps;
                jobRoot["version"] = version;
                jobRoot["hash"] = hash_cur;

                needPatchFileInfo.Add(bundleName, jobRoot);

                DebugUtil.Log(string.Format("asset : {0} need patch", bundleName));
            }

            bundleHash.Add(bundleName, hash_cur);
        }

        /// <summary>
        /// 检查Patch资源
        /// </summary>
        /// <param name="path"></param>
        /// <exception cref="Exception"></exception>
        static void CheckPatch(string path)
        {
            string[] allAB = FilePathTools.GetFiles(path, @"(.*)(\.ab)$", SearchOption.AllDirectories);
            foreach (var p in allAB)
            {
                string manifest = p + ".manifest";
                string back = p + ".back";
                string patch = p + ".patch";
                if (!File.Exists(manifest)) throw new Exception(string.Format("{0} not exist!!!", manifest));
                if (!File.Exists(back)) throw new Exception(string.Format("{0} not exist!!!", back));
                if (!File.Exists(patch)) throw new Exception(string.Format("{0} not exist!!!", patch));
                string md5 = AssetUtils.BuildFileMd5(p);
                string md5_back = AssetUtils.BuildFileMd5(back);
                if(!md5.Equals(md5_back)) throw new Exception(string.Format("{0} not euqal back!!!", p));
                JObject jobj = JObject.Parse(File.ReadAllText(patch));
                string md5_patch = jobj["md5"].ToString();
                if(!md5.Equals(md5_patch)) throw new Exception(string.Format("{0} md5 not equal persisten value!!!", p));
            }
            
            DebugUtil.Log(string.Format("Patch {0} check success!!!", path));
        }

        /// <summary>
        /// 打包之前执行
        /// </summary>
        public static void BuildPre()
        {
            if (!AssetConfigController.Instance.IsPathsNoRepetition())
            {
                throw new Exception("Duplicate path in configuration");
            }

            MarkFilesMD5ForBuildAssetbundle();

            MarkSpriteAtlasName();
            
            SpriteAtlasUtility.PackAllAtlases(EditorUserBuildSettings.activeBuildTarget, false);

            CheckPatch(FilePathTools.assetbundlePatchPath);
            
            string backPath = FilePathTools.assetbundlePatchPath + "_Back";
            if (Directory.Exists(backPath))
            {
                DirectoryInfo dir = new DirectoryInfo(backPath);
                dir.Empty();
            }

            //back patch
            if (Directory.Exists(FilePathTools.assetbundlePatchPath))
            {
                if (!Directory.Exists(backPath))
                {
                    Directory.CreateDirectory(backPath);
                }
                FilePathTools.CopyDirectory(FilePathTools.assetbundlePatchPath, backPath);
            }
            
            DebugUtil.Log("BuildPre Finish!!!");
        }

        /// <summary>
        /// 打包之后执行
        /// </summary>
        public static void BuildPost()
        {
            CheckPatch(FilePathTools.assetbundlePatchPath);
            
            string backPath = FilePathTools.assetbundlePatchPath + "_Back";
            
            //核实hash和md5是一一对应的
            if (Directory.Exists(backPath))
            {
                string[] allAB_Patch = FilePathTools.GetFiles(FilePathTools.assetbundlePatchPath, @"(.*)(\.ab)$", SearchOption.AllDirectories);
                string[] allAB_Back = FilePathTools.GetFiles(backPath, @"(.*)(\.ab)$", SearchOption.AllDirectories);
                foreach (var p in allAB_Patch)
                {
                    int index = allAB_Back.IndexOfEx(x => x.Replace(backPath, "").Equals(p.Replace(FilePathTools.assetbundlePatchPath, "")));
                    if (index >= 0)
                    {
                        JObject jobj_patch = JObject.Parse(File.ReadAllText(p + ".patch"));
                        string hash_patch = jobj_patch["hash"].ToString();
                        string md5_patch = jobj_patch["md5"].ToString();

                        JObject jobj_back = JObject.Parse(File.ReadAllText(allAB_Back[index] + ".patch"));
                        string hash_back = jobj_back["hash"].ToString();
                        string md5_back = jobj_back["md5"].ToString();
                        
                        if(hash_patch.Equals(hash_back) && !md5_patch.Equals(md5_back)) throw new Exception(string.Format("{0} hash with md5 not match!!!", p));
                    }
                }
            }
            
            //打印patch差异
            {
                StringBuilder dump = new StringBuilder();
                
                dump.AppendLine("----------打印本次更新的ab文件 开始----------");
                
                string versionPath = Application.dataPath + "/Resources/versionfile/Version.txt";
                if (!File.Exists(versionPath)) throw new Exception(string.Format("{0} not exist!!!", versionPath));
                VersionInfo version = JsonConvert.DeserializeObject<VersionInfo>(File.ReadAllText(versionPath));
                foreach (var group in version.ResGroups)
                {
                    foreach (var ab in group.Value.AssetBundles)
                    {
                        string ab_patch = string.Format("{0}/{1}.patch", FilePathTools.assetbundlePatchPath, ab.Key);
                        string ab_back = string.Format("{0}/{1}.patch", backPath, ab.Key);
                        if (!File.Exists(ab_patch)) throw new Exception(string.Format("{0} not exist!!!", ab_patch));
                        if (File.Exists(ab_back))
                        {
                            JObject jobj_patch = JObject.Parse(File.ReadAllText(ab_patch));
                            string hash_patch = jobj_patch["hash"].ToString();

                            JObject jobj_back = JObject.Parse(File.ReadAllText(ab_back));
                            string hash_back = jobj_back["hash"].ToString();

                            if (!hash_patch.Equals(hash_back)) dump.AppendLine(ab.Key);
                        }
                        else
                        {
                            dump.AppendLine(ab.Key);
                        }
                    }
                }

                dump.AppendLine("----------打印本次更新的ab文件 结束----------");
                
                DebugUtil.Log(dump.ToString());
            }
            
            //delete patch back
            if (Directory.Exists(backPath))
            {
                DirectoryInfo dir = new DirectoryInfo(backPath);
                dir.Empty();
            }
            
            DebugUtil.Log("BuildPost Finish!!!");
        }

        /// <summary>
        /// 通用资源打包后检查
        /// </summary>
        static void AfterBuildAllAssetBundleCheck()
        {
            CheckPatch(FilePathTools.assetbundlePatchPath);

            //配置和out一一对应，并且都在patch里能找到
            string[] allOutAB = FilePathTools.GetFiles(exportPath, @"(.*)(\.ab)$", SearchOption.AllDirectories);
            int cnt = 0;
            foreach (BundleGroup group in AssetConfigController.Instance.Groups)
            {
                foreach (var p in group.Paths)
                {
                    string path_patch = FilePathTools.assetbundlePatchPath + "/" + p.Path.ToLower() + ".ab";
                    string path_out = exportPath + "/" + p.Path.ToLower() + ".ab";
                    if (!File.Exists(path_patch)) throw new Exception(string.Format("{0} not exist!!!", path_patch));
                    if (!File.Exists(path_out)) throw new Exception(string.Format("{0} not exist!!!", path_out));
                    string md5_patch = AssetUtils.BuildFileMd5(path_patch);
                    string md5_out = AssetUtils.BuildFileMd5(path_out);
                    if (!md5_patch.Equals(md5_out)) throw new Exception(string.Format("{0} not equal src patch!!!", path_out));
                }

                cnt += group.Paths.Count;
            }

            if (allOutAB.Length != cnt) throw new Exception("size not equal!!!");

            //检查初包一一对应
            string[] allInitAB = FilePathTools.GetFiles(FilePathTools.streamingAssetsPath_Platform, @"(.*)(\.ab)$", SearchOption.AllDirectories);
            cnt = 0;
            foreach (BundleGroup group in AssetConfigController.Instance.Groups)
            {
                List<string> paths = group.GetInInitialPacketPaths();
                foreach (string item in paths)
                {
                    string srcFile = FilePathTools.assetbundlePatchPath + "/" + item.ToLower() + ".ab";
                    string destFile = FilePathTools.streamingAssetsPath_Platform + "/" + item.ToLower() + ".ab";
                    if (!File.Exists(srcFile)) throw new Exception(string.Format("{0} not exist!!!", srcFile));
                    if (!File.Exists(destFile)) throw new Exception(string.Format("{0} not exist!!!", destFile));
                    string md5_src = AssetUtils.BuildFileMd5(srcFile);
                    string md5_dest = AssetUtils.BuildFileMd5(destFile);
                    if (!md5_src.Equals(md5_dest)) throw new Exception(string.Format("{0} not equal src patch!!!", destFile));
                }

                cnt += paths.Count;
            }

            if (allInitAB.Length != cnt) throw new Exception("size not equal!!!");

            //检查2个Version
            string localPath = Application.dataPath + "/Resources/versionfile/Version.txt";
#if UNITY_IOS
            string remotePath = FilePathTools.assetBundleOutPath + "/" + string.Format("Version.{0}.txt", AssetConfigController.Instance.IOSVersionCode);
#else
            string remotePath = FilePathTools.assetBundleOutPath + "/" + string.Format("Version.{0}.txt", AssetConfigController.Instance.VersionCode);
#endif
            if (!File.Exists(localPath)) throw new Exception(string.Format("{0} not exist!!!", localPath));
            if (!File.Exists(remotePath)) throw new Exception(string.Format("{0} not exist!!!", remotePath));
            VersionInfo localVersion = JsonConvert.DeserializeObject<VersionInfo>(File.ReadAllText(localPath));
            VersionInfo remoteVersion = JsonConvert.DeserializeObject<VersionInfo>(File.ReadAllText(remotePath));
            foreach (var p in localVersion.ResGroups)
            {
                List<AssetBundleInfo> tempList = new List<AssetBundleInfo>();
                foreach (var a in p.Value.AssetBundles)
                {
                    AssetBundleInfo temp = a.Value;
                    temp.State = AssetState.ExistInDownLoad;
                    tempList.Add(temp);
                }

                foreach (var t in tempList)
                {
                    p.Value.AssetBundles[t.AssetBundleName] = t;
                }
            }

            string localJson = JsonConvert.SerializeObject(localVersion);
            string remoteJson = JsonConvert.SerializeObject(remoteVersion);
            if (!localJson.Equals(remoteJson)) throw new Exception("two version not equal!!!");

            VersionInfo checkVersionLocal = JsonConvert.DeserializeObject<VersionInfo>(File.ReadAllText(localPath));
            foreach (var p in checkVersionLocal.ResGroups)
            {
                List<AssetBundleInfo> tempList = new List<AssetBundleInfo>();
                foreach (var a in p.Value.AssetBundles)
                {
                    AssetBundleInfo temp = a.Value;
                    temp.DependenciesBundleNames = null;
                    temp.HashString = "";
                    tempList.Add(temp);
                }

                tempList.Sort((AssetBundleInfo a, AssetBundleInfo b) =>
                {
                    return a.AssetBundleName.CompareTo(b.AssetBundleName);
                });
                p.Value.AssetBundles.Clear();
                foreach (var t in tempList)
                {
                    p.Value.AssetBundles.Add(t.AssetBundleName, t);
                }
            }

            VersionInfo checkVersionCalculate = new VersionInfo
            {
#if UNITY_IOS
                Version = AssetConfigController.Instance.IOSRootVersion,
#else
                Version = AssetConfigController.Instance.RootVersion,
#endif
                UniqueID = checkVersionLocal.UniqueID
            };
            foreach (BundleGroup group in AssetConfigController.Instance.Groups)
            {
                VersionItemInfo item = new VersionItemInfo
                {
                    Version = group.Version,
                    UpdateWholeGroup = group.UpdateWholeAB
                };
                List<AssetBundleInfo> list = new List<AssetBundleInfo>();
                foreach (BundleState p in group.Paths)
                {
                    string bundleName = p.Path.ToLower() + ".ab";
                    AssetBundleInfo bundleInfo = new AssetBundleInfo
                    {
                        AssetBundleName = bundleName,
                        DependenciesBundleNames = null,
                        HashString = "",
                        Md5 = AssetUtils.BuildFileMd5(string.Format("{0}/{1}", FilePathTools.assetbundlePatchPath, bundleName))
                    };
                    if (group.IsInInitialPacket(bundleName))
                    {
                        bundleInfo.State = AssetState.ExistInDownLoad;
                    }
                    else
                    {
                        bundleInfo.State = AssetState.NotExist;
                    }

                    list.Add(bundleInfo);
                }

                list.Sort((AssetBundleInfo a, AssetBundleInfo b) =>
                {
                    return a.AssetBundleName.CompareTo(b.AssetBundleName);
                });
                foreach (var i in list)
                {
                    item.Add(i.AssetBundleName, i);
                }

                checkVersionCalculate.Add(group.GroupName, item);
            }

            string checkLocalJson = JsonConvert.SerializeObject(checkVersionLocal);
            string checkCalculateJson = JsonConvert.SerializeObject(checkVersionCalculate);
            if (!checkLocalJson.Equals(checkCalculateJson)) throw new Exception("two version not equal!!!");
            
            DebugUtil.Log("AfterBuildAllAssetBundleCheck Finish!!!");
        }

        /// <summary>
        /// 活动打包后检查
        /// </summary>
        static void AfterBuildActivityAssetBundleCheck()
        {
            CheckPatch(FilePathTools.assetbundlePatchPath);
            
            string patchPath = FilePathTools.assetbundlePatchPath + "/Activity/";
            string outPath = FilePathTools.assetBundleOutPath + "/Activity";
            
            //配置和out一一对应，并且都在patch里能找到
            string[] allOutAB = FilePathTools.GetFiles(outPath, @"(.*)(\.ab)$", SearchOption.AllDirectories);
            int cnt = 0;
            if (AssetConfigController.Instance.ActivityResPaths != null && AssetConfigController.Instance.ActivityResPaths.Length > 0)
            {
                for (int i = 0; i < AssetConfigController.Instance.ActivityResPaths.Length; i++)
                {
                    string abFileNameNoSuffix = AssetConfigController.Instance.ActivityResPaths[i].Replace('/', '_').ToLower();
                    string path = patchPath + abFileNameNoSuffix + ".ab";
                    if(!File.Exists(path)) throw new Exception(string.Format("{0} not exist!!!", path));
                    string patch = path + ".patch";
                    if (!File.Exists(patch)) throw new Exception(string.Format("{0} not exist!!!", patch));
                    JObject jobj = JObject.Parse(File.ReadAllText(patch));
                    string hash = jobj["hash"].ToString();
                    string md5_patch = AssetUtils.BuildFileMd5(path);
                    string ab_out = outPath + "/" + abFileNameNoSuffix + string.Format("_{0}_{1}.ab", hash, md5_patch);
                    if(!File.Exists(ab_out)) throw new Exception(string.Format("{0} not exist!!!", ab_out));
                    string md5_out = AssetUtils.BuildFileMd5(ab_out);
                    if(!md5_out.Equals(md5_patch)) throw new Exception(string.Format("{0} md5 not equal persisten value!!!", abFileNameNoSuffix));
                }
                cnt += AssetConfigController.Instance.ActivityResPaths.Length;
            }
            if(allOutAB.Length != cnt) throw new Exception("size not equal!!!");
            
            DebugUtil.Log("AfterBuildActivityAssetBundleCheck Finish!!!");
        }
    }
}