using UnityEngine;
using System.IO;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using UnityEditor;
using System.Text;
using UnityEditor.Build.Reporting;
using DragonU3DSDK.Asset;
using Unity;

namespace DragonU3DSDK
{
    public class Automation : Editor
    {
        public static readonly string commandProtocol = @"Assets/../../ExcelTools/build_protocol.sh";
        public static readonly string commandStorage = @"Assets/../../ExcelTools/build_storage.sh";
        public static readonly string commandConfigs = @"Assets/../../ExcelTools/gbuild.sh";

        [MenuItem("Automation/BuildProtocol", false, 1)]
        public static void BuildProtocol()
        {
            ShellExecutor.ExecuteShell(commandProtocol, "");
            AssetDatabase.Refresh();
        }

        [MenuItem("Automation/ClearAllLocalData", false, 2)]
        public static void ClearAllLocalData()
        {
            PlayerPrefs.DeleteAll();
        }

        [MenuItem("Automation/BuildStorage", false, 3)]
        public static void BuildStorage()
        {
            ShellExecutor.ExecuteShell(commandStorage, "");
            AssetDatabase.Refresh();
        }

        [MenuItem("Automation/BuildConfigs", false, 4)]
        public static void BuildConfigs()
        {
            ShellExecutor.ExecuteShell(commandConfigs, "");
            AssetDatabase.Refresh();
        }

        [MenuItem("Automation/DeleteAllEmptyDirectories", false, 5)]
        static void FindAndRemove()
        {
            var root = Application.dataPath;
            string[] dirs = Directory.GetDirectories(root, "*", SearchOption.AllDirectories);
            List<DirectoryInfo> emptyDirs = new List<DirectoryInfo>();
            foreach (var dir in dirs)
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                if (IsDirectoryEmpty(di))
                    emptyDirs.Add(di);
            }
            foreach (var emptyDir in emptyDirs)
            {
                if (Directory.Exists(emptyDir.FullName))
                {
                    Directory.Delete(emptyDir.FullName, true);
                    DebugUtil.Log("Recursively delete folder: " + emptyDir.FullName);
                }
            }
            AssetDatabase.Refresh();
        }

        static bool HasNoFile(DirectoryInfo dir)
        {
            bool noFile = true;
            foreach (var file in dir.GetFiles())
            {
                if (file.Name == ".DS_Store")
                    continue;

                if (file.Name.EndsWith(".meta") && Directory.Exists(
                        Path.Combine(dir.FullName, file.Name.Substring(0, file.Name.IndexOf(".meta")))))
                    continue;

                noFile = false;
                break;
            }
            return noFile;
        }

        static bool IsDirectoryEmpty(DirectoryInfo dir)
        {
            if (HasNoFile(dir))
            {
                var subDirs = dir.GetDirectories();
                bool allEmpty = true;
                foreach (var subDir in subDirs)
                {
                    if (!IsDirectoryEmpty(subDir))
                    {
                        allEmpty = false;
                        break;
                    }
                }
                return allEmpty;
            }
            return false;
        }

        [MenuItem("Automation/Tools/ExportScriptableObj/Configuration", false, 2)]
        public static void CreateConfigrationAsset()
        {
            CreateAsset<ConfigurationController>(ConfigurationController.ConfigurationControllerPath);
        }

        // [MenuItem("Automation/Tools/ExportScriptableObj/DebugConfig", false, 3)]
        // public static void CreateServerConfigAsset()
        // {
        //     CreateAsset<DebugConfigController>(DebugConfigController.DebugConfigControllerPath);
        // }
        [MenuItem("Automation/Tools/ExportScriptableObj/AssetConfig", false, 4)]
        private static void CreateController()
        {
            CreateAsset<AssetConfigController>(AssetConfigController.AssetConfigPath);
        }
        [MenuItem("AssetBundle/HomeRoom/创建HomeRoomConfig", false, 1)]
        private static void CreateHomeRoomController()
        {
            CreateAsset<HomeRoomConfigController>(HomeRoomConfigController.AssetConfigPath);
        }

        [MenuItem("AssetBundle/ColorRes/创建ColorResConfig", false, 1)]
        private static void CreateColorResController()
        {
            CreateAsset<ColorResConfigController>(ColorResConfigController.AssetConfigPath);
        }

        [MenuItem("AssetBundle/CookingGame/创建CookingGameConfig", false, 1)]
        private static void CreateCookingGameController()
        {
            CreateAsset<CookingGameConfigController>(CookingGameConfigController.AssetConfigPath);
        }

        [MenuItem("Automation/Tools/Build/iOSBuild", false, 4)]
        public static void iOSBuild()
        {
            Build(BuildTarget.iOS);
        }

        [MenuItem("Automation/Tools/Build/AndroidBuild", false, 5)]
        public static void AndroidBuild()
        {
            Build(BuildTarget.Android);
        }

        [MenuItem("Automation/Tools/Build/iOSBuild_Debug", false, 6)]
        public static void iOSBuild_Debug()
        {
            Build(BuildTarget.iOS, true);
        }

        [MenuItem("Automation/Tools/Build/AndroidBuild_Debug", false, 7)]
        public static void AndroidBuild_Debug()
        {
            Build(BuildTarget.Android, true);
        }

        [MenuItem("Automation/Tools/Build/MacBuild_Debug", false, 8)]
        public static void MacBuild_Debug()
        {
            Build(BuildTarget.StandaloneOSX, true);
        }

        // [MenuItem("Automation/Tools/Build/ForceResolve", false, 9)]
        // public static void ForceResovle()
        // {
        //     //Build前Resolve 所有依赖
        //     GooglePlayServices.PlayServicesResolver.ResolveSync(true);

        // }

        class BuildArgs
        {
            public BuildTarget target;
            public bool isDebug;
        }
        [UnityEditor.Callbacks.DidReloadScripts]
        static void CheckBuild()
        {
            var desc = EditorPrefs.GetString("BuildParams", "");
            if (!string.IsNullOrEmpty(desc))
            {
                EditorPrefs.DeleteKey("BuildParams");
                BuildArgs args = JsonUtility.FromJson<BuildArgs>(desc);
                AfterBuild(args.target, args.isDebug);
            }
        }

        static void Build(BuildTarget target, bool isDebug = false)
        {
            BuildArgs args = new BuildArgs();
            args.target = target;
            args.isDebug = isDebug;

            bool wait = false;
            var type = System.Reflection.Assembly.GetExecutingAssembly().GetType("CKEditor." + "ClientMacro");
            if (type != null)
            {
                type.InvokeMember("SetPlatform", System.Reflection.BindingFlags.InvokeMethod |
                 System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, null, null);
            }

            if (EditorApplication.isCompiling)
            {
                var str = JsonUtility.ToJson(args);
                EditorPrefs.SetString("BuildParams", str);
                wait = true;
            }

            if (!wait)
                AfterBuild(target, isDebug);
        }

        static void AfterBuild(BuildTarget target, bool isDebug = false)
        {
            // SDKEditor.SetFirebase();
            // PluginConfigInfo info = PluginsInfoManager.LoadPluginConfig();

            // if (info != null && info.m_Map.ContainsKey(Constants.FaceBook))
            // {
            //     FacebookConfigInfo fbInfo = info.m_Map[Constants.FaceBook] as FacebookConfigInfo;
            //     SDKEditor.SetFacebook(fbInfo.AppID);
            // }

#if !UNITY_2019_1_OR_NEWER
            //所有项目已经弃用 2018.4以下，2019.2以下版本。NDK统一为r16b,以下代码已无用。
            //if (!EditorPrefs.HasKey("AndroidNdkRoot") && string.IsNullOrEmpty(EditorPrefs.GetString("AndroidNdkRoot")))
            //{
            //    EditorPrefs.SetString("AndroidNdkRoot", "/Users/dragonplus/Downloads/android-ndk-r13b");
            //}

            //if (!EditorPrefs.HasKey("JdkPath") || string.IsNullOrEmpty(EditorPrefs.GetString("JdkPath")))
            //{
            //    EditorPrefs.SetString("JdkPath", "/Library/Java/JavaVirtualMachines/jdk1.8.0_181.jdk/Contents/Home/");
            //}

            //if (!EditorPrefs.HasKey("AndroidSdkRoot") || string.IsNullOrEmpty(EditorPrefs.GetString("AndroidSdkRoot")))
            //{
            //    EditorPrefs.SetString("AndroidSdkRoot", "/Users/dragonplus/Library/Android/sdk");
            //}

#endif
            // DebugUtil.Log("Android NDK Path is now {0}", EditorPrefs.GetString("AndroidNdkRoot"));

#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER

            //强制设置NDK版本
            //Unity修改本 key 为 AndroidNdkRootR16b，详见：https://forum.unity.com/threads/android-ndk-path-editorprefs-key-changed.639103/
            if (!EditorPrefs.HasKey("AndroidNdkRoot") || string.IsNullOrEmpty(EditorPrefs.GetString("AndroidNdkRoot")))
                EditorPrefs.SetString("AndroidNdkRoot", "/Users/dragonplus/Downloads/android-ndk-r16b");
            if (!EditorPrefs.HasKey("AndroidNdkRootR16b") || string.IsNullOrEmpty(EditorPrefs.GetString("AndroidNdkRootR16b")))
                EditorPrefs.SetString("AndroidNdkRootR16b", "/Users/dragonplus/Downloads/android-ndk-r16b");
#endif
            
#if UNITY_2019_3_OR_NEWER
            if (!EditorPrefs.HasKey("AndroidNdkRootR19") || string.IsNullOrEmpty(EditorPrefs.GetString("AndroidNdkRootR19")))
                EditorPrefs.SetString("AndroidNdkRootR19", "/Users/dragonplus/Downloads/android-ndk-r19");
#endif
            

            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = GetScenes();

            PlayerSettings.SplashScreen.showUnityLogo = false;

            string platform = "";
            string platformFolder = "";
            if (target == BuildTarget.Android)
            {
                platform = "Android";

                EditorUserBuildSettings.androidCreateSymbolsZip = false;
                if (!isDebug)
                {
                    EditorUserBuildSettings.androidCreateSymbolsZip = true;

                    //PlayerSettings.Android.keystoreName = "/Users/dragonplus/flydragon.keystore";
                    //PlayerSettings.Android.keystorePass = "FlyDragon123";
                    //PlayerSettings.Android.keyaliasName = "flydragon.keystore";
                    //PlayerSettings.Android.keyaliasPass = "FlyDragon123";

#if UNITY_2019_1_OR_NEWER

                    PlayerSettings.Android.useCustomKeystore = true;
#endif

                    if (ConfigurationController.Instance.AndroidKeyStoreUseConfiguration)
                    {
                        PlayerSettings.Android.keystoreName = ConfigurationController.Instance.AndroidKeyStorePath;
                        PlayerSettings.Android.keystorePass = ConfigurationController.Instance.AndroidKeyStorePass;
                        PlayerSettings.Android.keyaliasName = ConfigurationController.Instance.AndroidKeyStoreAlias;
                        PlayerSettings.Android.keyaliasPass = ConfigurationController.Instance.AndroidKeyStoreAliasPass;
                    }
                    else
                    {
#if !UNITY_2019_1_OR_NEWER
                        PlayerSettings.Android.keystoreName = "/Users/dragonplus/smartfunapp.keystore";
                        PlayerSettings.Android.keystorePass = "SmartFun123";
                        PlayerSettings.Android.keyaliasName = "SmartFun.keystore";
                        PlayerSettings.Android.keyaliasPass = "SmartFun123";
#endif
                    }
                }

#if UNITY_2019_1_OR_NEWER
                DebugUtil.Log("useCustomKeystore is " + PlayerSettings.Android.useCustomKeystore);
#endif
                DebugUtil.Log("PlayerSettings.Android.keystoreName is " + PlayerSettings.Android.keystoreName);
                DebugUtil.Log("PlayerSettings.Android.keystorePass is " + PlayerSettings.Android.keystorePass);
                DebugUtil.Log("PlayerSettings.Android.keyaliasName is " + PlayerSettings.Android.keyaliasName);
                DebugUtil.Log("PlayerSettings.Android.keyaliasPass is " + PlayerSettings.Android.keyaliasPass);

#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                if (ConfigurationController.Instance.BuildAppBundle)
                    EditorUserBuildSettings.buildAppBundle = true;
                else
                    EditorUserBuildSettings.buildAppBundle = false;

#endif
            }
            else if (target == BuildTarget.iOS)
            {
                platform = "iOS";
                PlayerSettings.iOS.appleEnableAutomaticSigning = true;
            }
            platformFolder = Path.GetFullPath(Application.dataPath + "/../" + platform + "/build/");
            if (!Directory.Exists(platformFolder))
                Directory.CreateDirectory(platformFolder);

            if (target == BuildTarget.Android)
            {
#if UNITY_2018_4_OR_NEWER || UNITY_2019_1_OR_NEWER
                if (ConfigurationController.Instance.BuildAppBundle)
                    platformFolder = platformFolder + PlayerSettings.productName + ".aab";
                else
                    platformFolder = platformFolder + PlayerSettings.productName + ".apk";
#else
                platformFolder = platformFolder + PlayerSettings.productName + ".apk";
#endif
            }

            if (target == BuildTarget.StandaloneOSX)
            {
                platformFolder = platformFolder + PlayerSettings.productName + ".app";
            }

            buildPlayerOptions.locationPathName = platformFolder;


            buildPlayerOptions.target = target;
            if (isDebug)
            {
                buildPlayerOptions.options |= BuildOptions.Development;
            }
            //else
            //{
            //    buildPlayerOptions.options = BuildOptions.None;
            //}

            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            BuildSummary summary = report.summary;
            DebugUtil.Log(summary.result.ToString());
        }

        public static void CreateAsset<T>(string path) where T : ScriptableObject
        {
            var folder = Path.GetDirectoryName(Application.dataPath + "/Resources/" + path);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            T ac = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(ac, "Assets/Resources/" + path + ".asset");
        }

        static string[] GetScenes()
        {
            List<string> scenes = new List<string>();
            foreach (var scene in EditorBuildSettings.scenes)
            {
                scenes.Add(scene.path);
            }

            return scenes.ToArray();
        }
    }
}
