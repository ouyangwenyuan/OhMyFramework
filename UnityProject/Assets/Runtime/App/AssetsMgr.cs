using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using QFramework;
using UnityEngine;
using UnityEngine.U2D;
using Object = UnityEngine.Object;

namespace DragonPlus.Assets
{
     public class AssetBundlePathTools{
        // 创建目录
        public static void CreateDirectoryByFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                string dirName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dirName))
                {
                    Directory.CreateDirectory(dirName);
                }
            }
        }
        public static void CreateFolder(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            if (!dir.Exists)
            {
                dir.Create();
            }
        }

        // 文件是否存在
        public static bool IsFileExists(string path)
        {
            string localVersionPath = string.Format("{0}/{1}", AssetBundlePathConst.persistentDataPath_Platform, path);
            if (File.Exists(localVersionPath))
            {
                return true;
            }
            return false;
        }

        // 拷贝文件
        public static void CopyFile(string fromFilePath, string toFilePath)
        {
            CreateDirectoryByFile(toFilePath);
            File.Copy(fromFilePath, toFilePath, true);
        }
        
        //拷贝文件夹
        public static void CopyDirectory(string sourceDirectory, string targetDirectory)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirectory);
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
            foreach (FileSystemInfo i in fileinfo)
            {
                if (i is DirectoryInfo)
                {
                    if (!Directory.Exists(Path.Combine(targetDirectory, i.Name)))
                    {
                        Directory.CreateDirectory(Path.Combine(targetDirectory, i.Name));
                    }
                    CopyDirectory(i.FullName, Path.Combine(targetDirectory, i.Name));
                }
                else
                {
                    if (i.Name.EndsWith (".meta")) {
                        continue;
                    }
                    if (i.Name.EndsWith (".DS_Store")) {
                        continue;
                    }
                    File.Copy(i.FullName, Path.Combine(targetDirectory, i.Name), true);
                }
            }
        }
     }
    public class AssetBundlePathConst
    {
        public static bool EditorUseAssetBundle = true;
        public static readonly string ResServerURL = "http://localhost:8000";
        public static readonly string fileSchme = "file:///";
        public static readonly string AssetBundleOut = "AssetBundleOut";
        public static readonly string RuntimeAssetsRoot = "DownLoad";
        public static readonly string AssetsRoot = "Assets";
        public static readonly string AssetBundleRoot = "AssetBundle";
        public static string AssetBundlePath = Path.Combine(AssetsRoot,AssetBundleRoot);
#if UNITY_ANDROID
        public static readonly string targetName = "android";
#elif UNITY_IPHONE
        public static readonly string targetName = "iOS";
#else
        public static readonly string targetName = "window";
#endif

        /*==================== persistent目录缓存,避免多次字符串拼接 ========================*/
#if UNITY_EDITOR || !UNITY_STANDALONE
        public static readonly string downLoadPath = Path.Combine(Application.persistentDataPath , RuntimeAssetsRoot);
#else
        public static readonly string downLoadPath = Directory.GetCurrentDirectory() + "/DownLoad/";
#endif
        public static readonly string persistentDataPath_Platform = Path.Combine(downLoadPath , targetName);
        public static readonly string persistentDataPath_Platform_ForWWWLoad = fileSchme + persistentDataPath_Platform;

        /*========================== streamingAssets目录缓存 =============================*/
        public static readonly string streamingAssetsPath_Platform = Path.Combine(Application.streamingAssetsPath , targetName);
#if !UNITY_EDITOR && UNITY_ANDROID
        public static readonly string streamingAssetsPath_Platform_ForWWWLoad = Path.Combine(streamingAssetsPath_Platform);
#else
        public static readonly string streamingAssetsPath_Platform_ForWWWLoad = fileSchme + streamingAssetsPath_Platform;
#endif

        /*======================== assetbundle打完包后的存放路径 ===========================*/
#if UNITY_EDITOR
        public static readonly string assetbundlePatchPath = Path.Combine(Application.dataPath.Substring(0,Application.dataPath.LastIndexOf("/Assets", StringComparison.Ordinal)) , AssetBundleOut , targetName);
        public static readonly string assetBundleOutPath = Path.Combine(Application.dataPath.Substring(0,Application.dataPath.LastIndexOf("/Assets", StringComparison.Ordinal)) , AssetBundleOut , targetName);
        public static readonly string assetBundleOutPath_ForWWWLoad = fileSchme + Application.dataPath + AssetBundleOut + targetName;
#endif


         
        // AssetBundle的加载路径
//        public static string AssetBundleLoadPath
//        {
//            get
//            {
//                if (UseAssetBundle)
//                {
//                    // 不使用bundle包，从编辑器的asset目录下加载
//                    return Application.dataPath;
//                }
//                else
//                {
//                    // 使用bundle包，从SD卡路径下加载
//                    return persistentDataPath_Platform_ForWWWLoad;
//                }
//            }
//        }

        // AssetBundle的下载路径
        public static string AssetBundleDownloadPath
        {
            get
            {
                return ResServerURL  + Path.PathSeparator + targetName;

            }
        }
    }
    public class AssetBundleBuildInfo{
        public string Name;
        public List<string> AssetNames = new List<string>();
    }
    public class AssetBundleLoadInfo{
        public string Name;
        public string Hash;
        public string Md5Str;
        public List<string> Dependancies = new List<string>();
    }

    public class AssetBundleVersionWraper
    {
        public string Semver;
        public string VersionCode;
        public string VersionBuild;
        public Dictionary<string,AssetBundleLoadInfo> LoadInfos = new Dictionary<string, AssetBundleLoadInfo>();
    }

    public class AssetsMgr
    {
        Dictionary<string,AssetBundle> _assetBundlesCache = new Dictionary<string, AssetBundle>();
        /// <summary>
        /// 加载bundle中的资源
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="resName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public  T LoadAsset<T>(string bundleName, string resName) where T : Object
        {
            Object obj = LoadAssetFromAb<T>(bundleName, resName);
            if (obj == null)
            {
                obj = LoadAssetFromResources<T>(bundleName, resName);
            }

            if (obj != null) return (T) obj;
            Debug.Log($"{bundleName}/{resName} is not exist ");
            return default;
        }
        /// <summary>
        /// 加载bundle资源，在编辑器模式使用assets资源，其他情况使用bundle
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="resName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public  T LoadAssetFromAb<T>(string bundleName, string resName) where T : Object
        {
#if UNITY_EDITOR
            if (!AssetBundlePathConst.EditorUseAssetBundle)
            {
                return LoadAssetFromEditor<T>(bundleName, resName);
            }
#endif
            return LoadAssetFromBundle<T>(bundleName, resName);
        }

        private static T LoadAssetFromEditor<T>(string bundleName, string resName) where T : Object
        {
            var perhapsNames = new List<string>();
            if (typeof(T) == typeof(GameObject))
            {
                perhapsNames.Add(resName + ".prefab");
            }
            else if (typeof(T) == typeof(SpriteAtlas))
            {
                perhapsNames.Add(resName + ".spriteatlas");
            }
            else if (typeof(T) == typeof(AudioClip))
            {
                perhapsNames.Add(resName + ".mp3");
                perhapsNames.Add(resName + ".wav");
            }
            else if (typeof(T) == typeof(Sprite) || typeof(T) == typeof(Texture2D))
            {
                perhapsNames.Add(resName + ".png");
                perhapsNames.Add(resName + ".jpg");
                perhapsNames.Add(resName + ".tga");
            }
            else if (typeof(T) == typeof(TextAsset))
            {
                perhapsNames.Add(resName + ".txt");
                perhapsNames.Add(resName + ".json");
                perhapsNames.Add(resName + ".xml");
                perhapsNames.Add(resName + ".bytes");
            }
            else if (typeof(T) == typeof(Material))
            {
                perhapsNames.Add(resName + ".mat");
            }
            else if (typeof(T) == typeof(Material))
            {
                perhapsNames.Add(resName + ".mat");
            }
            else if (typeof(T) == typeof(RuntimeAnimatorController))
            {
                perhapsNames.Add(resName + ".controller");
            }
            else
            {
                perhapsNames.Add(resName + ".asset");
            }
#if UNITY_EDITOR
            foreach (var obj in perhapsNames.Select(itemName => Path.Combine(bundleName, itemName))
                .Select(path => UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(T))).Where(obj => obj != null))
            {
                return (T) obj;
            }
#endif
            Debug.Log($"{bundleName} is not exist in Assets/AssetBundle");
            return default;
        }

        private T LoadAssetFromBundle<T>(string bundleName, string resName) where T : Object
        {
            var bundleLowerName = bundleName.ToLower();
            AssetBundle ab = null;
//            try
//            {
//                loadBundleDepends(bundleLowerName,true);
//                    ab = AssetBundle.LoadFromFile(Path.Combine(Application.persistentDataPath, AssetBundlePathConst.RuntimeAssetsRoot, bundleLowerName));
//                if (!_assetBundlesCache.ContainsKey(bundleLowerName))
//                {
//                    _assetBundlesCache.Add(bundleLowerName,ab);
//                }
//            }
//            catch (Exception e)
//            {
//                Debug.Log($"{bundleLowerName} is not exist in persistentDataPath,{e}");
//            }

            if (ab == null)
            {
                try
                {
                    loadBundleDepends(bundleLowerName, false);
                    ab = AssetBundle.LoadFromFile(Path.Combine(Application.streamingAssetsPath,
                        AssetBundlePathConst.RuntimeAssetsRoot, bundleLowerName));
                    if (!_assetBundlesCache.ContainsKey(bundleLowerName))
                    {
                        _assetBundlesCache.Add(bundleLowerName, ab);
                    }
                }
                catch (Exception exception)
                {
                    Debug.Log($"{bundleLowerName} is not exist in streamingAssetsPath,{exception}");
                }
            }

//            Path.GetFileNameWithoutExtension()
            if (ab != null) return ab.LoadAsset<T>(resName.ToLower());
            Debug.Log($"{bundleLowerName} is not exist in streamingAssetsPath");
            return default;
        }

        /// <summary>
        /// 加载bundle依赖
        /// </summary>
        /// <param name="bundleLowerName"></param>
        private void loadBundleDepends(string bundleLowerName,bool download)
        {
            var abRootPath = Path.Combine(download?Application.persistentDataPath:Application.streamingAssetsPath, AssetBundlePathConst.RuntimeAssetsRoot);
            string json = File.ReadAllText(Path.Combine(abRootPath, "Version.txt"));
            JsonSerializerSettings setting = new JsonSerializerSettings();
            Debug.Log($"json={json}");
            setting.NullValueHandling = NullValueHandling.Ignore;
//            Dictionary<string,AssetBundleLoadInfo> loadInfos  = JsonConvert.DeserializeObject<Dictionary<string,AssetBundleLoadInfo>>(json, setting);
                AssetBundleVersionWraper versionWraper = JsonConvert.DeserializeObject<AssetBundleVersionWraper>(json, setting);
//                Debug.Log($"versionWraper={versionWraper}");
                Dictionary<string,AssetBundleLoadInfo> loadInfos = versionWraper.LoadInfos;
            Debug.Log($"loadInfos={loadInfos.Count},abname={bundleLowerName}");
//                foreach (var info in loadInfos)
//                {
//                    Debug.Log($"loadInfo={info.Key}");
//                }
            AssetBundleLoadInfo loadInfo = null;
            loadInfos.TryGetValue(bundleLowerName, out loadInfo);
//                Debug.Log($"loadInfo={loadInfo}");
            if (loadInfo != null)
            {
                foreach (var dependency in loadInfo.Dependancies)
                {
                    Debug.Log($"dependency={dependency}");
                    if (!_assetBundlesCache.ContainsKey(dependency))
                    {
                        var dependAb = AssetBundle.LoadFromFile(Path.Combine(download?Application.persistentDataPath:Application.streamingAssetsPath, AssetBundlePathConst.RuntimeAssetsRoot, dependency));
                        _assetBundlesCache.Add(dependency,dependAb);
                    }

                }
            }
        }

        /// <summary>
        /// 从Resources中取资源
        /// </summary>
        /// <param name="bundleName"></param>
        /// <param name="resName"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public  T LoadAssetFromResources<T>(string bundleName, string resName) where T : Object
        {
            Object obj = null;
            try
            {
                var path = Path.Combine(bundleName, resName);
                obj = Resources.Load<T>(path);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }

            if (obj != null) return (T) obj;
            Debug.Log($"{resName} is not exist in Resources");
            return default;
        }
        /// <summary>
        /// 预制体创建游戏对象
        /// </summary>
        /// <param name="itemPrefab"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static GameObject CreateGameObject (GameObject itemPrefab, Transform parent) {
            GameObject item = Object.Instantiate (itemPrefab, parent, true);
//            item.transform.Reset ();
            return item;
        }
    }
}