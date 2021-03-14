/*-------------------------------------------------------------------------------------------
// Copyright (C) 2019 北京，天龙互娱
//
// 模块名：ResourcesManager
// 创建日期：2019-1-11
// 创建者：waicheng.wang
// 模块描述：资源加载管理器
//-------------------------------------------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using System.IO;
using Object = UnityEngine.Object;
using System;
using DragonU3DSDK.SDKEvents;

namespace DragonU3DSDK.Asset
{
    public partial class ResourcesManager : Manager<ResourcesManager>, IEventHandler<DownloadFileEvent>
    {
        // bundle包的缓存池
        public OnMemoryAssetBundleManager AssetBundleCache { private set; get; }

        // bundle包的缓存时长
        private int bundleCacheDuration = 20;

        // 从bundle包里load出来的asset的缓存时间
        private int objectCacheDuration = 120;

        // 总是缓存加载出来的物体，不清理
        private bool alwaysCache = false;

        private Dictionary<string, CacheObject> dicCacheObject = new Dictionary<string, CacheObject>();

        private const int MAX_DEPENDENCY_DEEP = 6;

        void Awake()
        {
            EventManager.Instance.Subscribe<DownloadFileEvent>(this);
            
            AssetBundleCache = new OnMemoryAssetBundleManager();
#if !UNITY_EDITOR
            AssetConfigController.Instance.UseAssetBundle = true;// 非编辑器模式下，永远使用ab包
#endif
            
#if !UNITY_STANDALONE            
            AssetBundle.SetAssetBundleDecryptKey(AssetConfigController.Instance.EnhancedEncryptionSecret);
#endif
        }

        // 设置bundle包的缓存时长
        public void SetBundleCacheDuration(int time)
        {
            bundleCacheDuration = time;
        }

        // 设置从ab包里load出来的object的缓存时长
        public void SetObjectCacheDuration(int time)
        {
            objectCacheDuration = time;
        }

        // 设置当ab是否一直存在，不被自动清理
        public void SetAlwaysCache(bool _alwaysCache)
        {
            alwaysCache = _alwaysCache;
        }

        public bool TryGetCoValue<T>(CacheObject cacheObject, out T outValue) where T : Object
        {
            if (cacheObject.obj == null)
            {
                outValue = null;
                return false;
            }

            if (typeof(T) == typeof(Sprite))
            {
                Sprite sprite = cacheObject.obj as Sprite;
                if (sprite == null || sprite.texture == null)
                {
                    outValue = null;
                    return false;
                }
            }
            else if (typeof(T) == typeof(TextAsset))
            {
                TextAsset ta = cacheObject.obj as TextAsset;
                if (ta == null || ta.text == null)
                {
                    outValue = null;
                    return false;
                }
            }

            outValue = cacheObject.obj as T;
            if (outValue != null)
            {
                return true;
            }

            return false;
        }

        #region AssetBundle包加载

        public T LoadResource<T>(string name, bool forceBundle = false, bool addToCache = true) where T : Object
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            stopWatch.Restart();
#endif
            try
            {
                List<string> path = new List<string>();
                if (AssetConfigController.Instance.UseAssetBundle || forceBundle)
                {
                    string normalizepath = FilePathTools.NormalizePath(name.ToLower());
                    CacheObject co;
                    T outValue;
                    if (dicCacheObject.TryGetValue(normalizepath, out co) && co != null && co.obj != null &&
                        TryGetCoValue<T>(co, out outValue) && outValue != null)
                    {
                        return outValue;
                    }
                    else
                    {
                        if (!alwaysCache)
                            TryClearCache();
                        object obj = SubLoadResource<T>(normalizepath, forceBundle, addToCache);
                        if (obj != null)
                            return obj as T;
                    }
                }
                else
                {
                    if (typeof(T) == typeof(Sprite))
                    {
                        path.Add(name + ".png");
                        path.Add(name + ".jpg");
                        path.Add(name + ".tga");
                    }
                    else if (typeof(T) == typeof(Texture2D))
                    {
                        path.Add(name + ".png");
                        path.Add(name + ".jpg");
                    }
                    else if (typeof(T) == typeof(GameObject))
                    {
                        path.Add(name + ".prefab");
                    }
                    else if (typeof(T) == typeof(AudioClip))
                    {
                        path.Add(name + ".mp3");
                        path.Add(name + ".wav");
                    }
                    else if (typeof(T) == typeof(Material))
                    {
                        path.Add(name + ".mat");
                    }
                    else if (typeof(T) == typeof(TextAsset))
                    {
                        path.Add(name + ".txt");
                        path.Add(name + ".json");
                        path.Add(name + ".xml");
                        path.Add(name + ".bytes");
                    }
                    else if (typeof(T) == typeof(SpriteAtlas))
                    {
                        path.Add(name + ".spriteatlas");
                    }
                    else if (typeof(T) == typeof(TMPro.TMP_FontAsset))
                    {
                        path.Add(name + ".asset");
                    }
                    else if (typeof(T) == typeof(Material))
                    {
                        path.Add(name + ".mat");
                    }
                    else if (typeof(T) == typeof(RuntimeAnimatorController))
                    {
                        path.Add(name + ".controller");
                    }
#if CK_CLIENT
                    else if (typeof(T) == typeof(Spine.Unity.SkeletonDataAsset))
                    {
                        path.Add(name + ".asset");
                    }
#endif

                    foreach (string itempath in path)
                    {
                        string normalizepath = FilePathTools.NormalizePath(itempath);
                        object obj = SubLoadResource<T>(normalizepath, false, addToCache);
                        if (obj != null)
                            return obj as T;
                    }
                }
            }
            catch (Exception e)
            {
                DebugUtil.LogError(e);
            }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            finally
            {
                stopWatch.Stop();
                long time_delta = stopWatch.ElapsedMilliseconds;
                loadTimes += time_delta;

                loadTimeMarks.Add(new LoadTimeMark(name, time_delta));
            }
#endif

            return null;
        }

        private T SubLoadResource<T>(string path, bool forceBundle = false, bool addToCache = true) where T : Object
        {
            if (!AssetConfigController.Instance.UseAssetBundle && !forceBundle) //使用编辑器里资源
            {
#if UNITY_EDITOR
                path = FilePathTools.GetAssetEditorPath(path);
                Object Obj = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(T));

                if (Obj == null)
                {
                    //DebugUtil.LogError("Asset not found at path:" + path);
                    return null;
                }

                return (T)Obj;
#endif
            }

            // 记录本次加载到内存的所有ab包
            List<string> loadedBundles = new List<string>();

            // 加载目标资源
            Object obj = null;
            string bundlePath = Path.GetDirectoryName(path) + ".ab";
            AssetBundle targetAB = LoadBundle(bundlePath, ref loadedBundles);
            if (targetAB == null)
            {
                DebugUtil.LogError("Asset not found at path:" + path);
                return null;
            }

            string tempFileName = Path.GetFileName(path);
            obj = targetAB.LoadAsset<T>(tempFileName);
            if (addToCache)
            {
                AddCache(path, obj);
            }

            // 释放加载到内存的ab
            if (!alwaysCache)
            {
                StartCoroutine(UnloadAssetbundle(loadedBundles));
            }

            return (T)obj;

        }

        //path 一定要是bundle的path
        private AssetBundle LoadBundle(string path, ref List<string> bundles, int stackSizeForCheck = 0)
        {
            if (stackSizeForCheck > MAX_DEPENDENCY_DEEP)
            {
                DebugUtil.LogError($"{GetType()}.LoadBundle, path = {path}, dependency stack reach max count({MAX_DEPENDENCY_DEEP}, check if its a bad bundle strategy or a dead code cycle!)");
                return null;
            }
            
            string abKey = path;

            AssetBundle targetAb;
            targetAb = AssetBundleCache.GetAssetBundle(abKey);

            AssetBundleInfo abInfo = VersionManager.Instance.GetAssetBundleInfo(abKey);
            string[] dependencies = abInfo.DependenciesBundleNames;

            // 加载自己
            if (targetAb == null)
            {
                string bundleFilePath = FilePathTools.GetBundleLoadPath(abKey);
                if (File.Exists(bundleFilePath))
                {
                    targetAb = AssetBundle.LoadFromFile(bundleFilePath);
                }

                if (targetAb != null)
                {
                    bundles.Add(abKey);

                    // if (HomeRoomConfigController.Instance != null)
                    // {
                    //     var info = HomeRoomConfigController.Instance.GetRoomResInfoByABPath(abKey);
                    //     if (info != null)
                    //     {
                    //         //homeroom 公共库的ab包名字是带versioncode的,需要去掉
                    //         targetAb.name = abKey;
                    //     }
                    // }
                    
                    // if (CookingGameConfigController.Instance != null)
                    // {
                    //     var info = CookingGameConfigController.Instance.GetRoomResInfoByABPath(abKey);
                    //     if (info != null)
                    //     {
                    //         //cookinggame 公共库的ab包名字是带versioncode的,需要去掉
                    //         targetAb.name = abKey;
                    //     }
                    // }

                    // if (ColorResConfigController.Instance != null)
                    // {
                    //     var info = ColorResConfigController.Instance.GetRoomResInfoByABPath(abKey);
                    //     if (info != null)
                    //     {
                    //         //colorRes公共库的ab包名字是带versioncode的,需要去掉
                    //         targetAb.name = abKey;
                    //     }
                    // }

                    AssetBundleCache.AddAssetBundle(targetAb, abInfo);
                }
                else
                {
                    return null;
                }
            }

            // 加载依赖包
            if (dependencies != null && dependencies.Length > 0)
            {
                ++stackSizeForCheck;
                foreach (string fileName in dependencies)
                {
                    if (!bundles.Contains(fileName))
                        LoadBundle(fileName, ref bundles, stackSizeForCheck);
                }
            }

            return targetAb;
        }

        private IEnumerator UnloadAssetbundle(List<string> list)
        {
            //1、在ios上同步加载后直接释放ab包，会造成加载出来的资源被回收
            //2、项目采用大assetbundle包，要考虑频繁加载的卡顿
            //这里把AB包缓存5秒，优化上面2个问题
            yield return new WaitForSeconds(bundleCacheDuration);
            for (int i = 0; i < list.Count; i++)
            {
                AssetBundleCache.Unload(list[i]);
            }
        }

        private void AddCache(string path, Object obj)
        {
            if (!dicCacheObject.ContainsKey(path))
            {
                dicCacheObject.Add(path, new CacheObject(obj, Time.time));
            }
            else
            {
                dicCacheObject[path] = new CacheObject(obj, Time.time);
            }
        }

        private void TryClearCache()
        {
            List<string> list = new List<string>(dicCacheObject.Keys);

            foreach (string key in list)
            {
                CacheObject co = dicCacheObject[key];
                if (Time.time - co.time > objectCacheDuration)
                {
                    dicCacheObject.Remove(key);
                }
            }
        }

        #endregion


        public void UnLoadAllCache()
        {
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        public void Clear()
        {
            dicCacheObject.Clear();
            AssetBundleCache.Clear();
        }

        public void ReleaseRes(string path, bool free = false)
        {
            var norm_path = FilePathTools.NormalizePath(path);
            if (dicCacheObject.TryGetValue(norm_path, out CacheObject oc))
            {                
                dicCacheObject.Remove(norm_path);
                if (free)
                {
                    if (oc.obj is Sprite)
                    {
                        var sp = oc.obj as Sprite;
                        Resources.UnloadAsset(sp.texture);
                    }
                    else
                        Resources.UnloadAsset(oc.obj);
                }
            }
            string bundlePath = Path.GetDirectoryName(path) + ".ab";
            AssetBundleCache.Unload(bundlePath);
        }
        
        public class CacheObject
        {
            public Object obj;
            public float time;

            public CacheObject(Object obj, float time)
            {
                this.obj = obj;
                this.time = time;
            }
        }
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// 异步加载
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// 同时最大的加载数
        private const int MAX_REQUEST = 50;

        /// 可再次申请的加载数
        private int _requestRemain = MAX_REQUEST;

        /// 当前申请要加载的队列
        private List<AssetBundleAsyncLoader> _requestQueue = new List<AssetBundleAsyncLoader>();

        /// 加载队列
        private List<AssetBundleAsyncLoader> _currentLoadQueue = new List<AssetBundleAsyncLoader>();

        /// 未完成的
        private HashSet<AssetBundleAsyncLoader> _nonCompleteLoaderSet = new HashSet<AssetBundleAsyncLoader>();

        /// 已创建的所有Loader列表(包括加载完成和未完成的)
        private Dictionary<string, AssetBundleAsyncLoader> _loaderCache = new Dictionary<string, AssetBundleAsyncLoader>();

        /// 当前是否还在异步加载，如果加载，则暂时不回收
        private bool _isCurrentLoading;

        private void Update()
        {
            if (_isCurrentLoading)
            {
                CheckNewLoaders();
                CheckQueue();
            }
        }

        public void LoadResourceAsync<T>(string name, Action<T> OnFinished = null) where T : Object
        {
            List<string> path = new List<string>();
            if (AssetConfigController.Instance.UseAssetBundle)
            {
                string normalizepath = FilePathTools.NormalizePath(name.ToLower());
                CacheObject co;
                if (dicCacheObject.TryGetValue(normalizepath, out co) && co.obj != null)
                {
                    OnFinished?.Invoke(co.obj as T);
                }
                else
                {
                    if (!alwaysCache)
                        TryClearCache();

                    string bundlePath = Path.GetDirectoryName(normalizepath) + ".ab";
                    string assetName = Path.GetFileName(normalizepath);

                    AssetBundle targetAb;
                    targetAb = AssetBundleCache.GetAssetBundle(bundlePath);
                    if (targetAb != null)
                    {
                        StartCoroutine(LoadFromAssetBundle<T>(targetAb, assetName, (obj) =>
                        {
                            AddCache(normalizepath, obj);
                            OnFinished?.Invoke(obj);
                        }));
                    }
                    else
                    {
                        LoadAssetBundleAsync(bundlePath, (loader) =>
                        {
                            if (loader.IsComplete)
                            {
                                StartCoroutine(LoadFromAssetBundle<T>(loader.AssetBundle, assetName, (obj) =>
                                {
                                    AddCache(normalizepath, obj);
                                    OnFinished?.Invoke(obj);
                                }));
                            }
                        });
                    }
                }
            }
            else
            {
                if (typeof(T) == typeof(Sprite))
                {
                    path.Add(name + ".png");
                    path.Add(name + ".jpg");
                    path.Add(name + ".tga");
                }
                else if (typeof(T) == typeof(Texture2D))
                {
                    path.Add(name + ".png");
                    path.Add(name + ".jpg");
                }
                else if (typeof(T) == typeof(GameObject))
                {
                    path.Add(name + ".prefab");
                }
                else if (typeof(T) == typeof(AudioClip))
                {
                    path.Add(name + ".mp3");
                    path.Add(name + ".wav");
                }
                else if (typeof(T) == typeof(Material))
                {
                    path.Add(name + ".mat");
                }
                else if (typeof(T) == typeof(TextAsset))
                {
                    path.Add(name + ".txt");
                    path.Add(name + ".json");
                    path.Add(name + ".xml");
                    path.Add(name + ".bytes");
                }
                else if (typeof(T) == typeof(SpriteAtlas))
                {
                    path.Add(name + ".spriteatlas");
                }
                else if (typeof(T) == typeof(TMPro.TMP_FontAsset))
                {
                    path.Add(name + ".asset");
                }
                else if (typeof(T) == typeof(Material))
                {
                    path.Add(name + ".mat");
                }
                else if (typeof(T) == typeof(RuntimeAnimatorController))
                {
                    path.Add(name + ".controller");
                }

                foreach (string itempath in path)
                {
                    string normalizepath = FilePathTools.NormalizePath(itempath);
                    object obj = SubLoadResource<T>(normalizepath);
                    if (obj != null)
                    {
                        OnFinished?.Invoke(obj as T);
                        break;
                    }
                }
            }
        }

        public AssetBundleAsyncLoader LoadAssetBundleAsync(string path, AssetBundleAsyncLoader.LoaderCompleteHandler handler = null)
        {
            string normalizepath = FilePathTools.NormalizePath(path.ToLower());
            AssetBundleAsyncLoader loader = this.CreateLoader(normalizepath);
            loader.onComplete += handler;
            if (!_isCurrentLoading)
            {
                _isCurrentLoading = true;
                //Application.backgroundLoadingPriority = ThreadPriority.High;
            }
            _nonCompleteLoaderSet.Add(loader);
            return loader;
        }

        internal AssetBundleAsyncLoader CreateLoader(string abFileName)
        {
            AssetBundleAsyncLoader loader = null;
            string bundleName = abFileName;
            if (_loaderCache.ContainsKey(bundleName))
            {
                loader = _loaderCache[bundleName];
            }
            else
            {
                AssetBundleInfo abInfo = VersionManager.Instance.GetAssetBundleInfo(bundleName);
                if (string.IsNullOrEmpty(abInfo.AssetBundleName))
                {
                    DebugUtil.LogError("### abInfo Error: ###" + abFileName);
                }

                loader = new AssetBundleAsyncLoader
                {
                    assetBundleInfo = abInfo
                };
                _loaderCache.Add(bundleName, loader);
            }

            return loader;
        }

        void CheckNewLoaders()
        {
            if (_nonCompleteLoaderSet.Count > 0)
            {
                foreach (var kv in _nonCompleteLoaderSet)
                {
                    Debug.LogError(kv.assetBundleInfo.AssetBundleName);
                }

                List<AssetBundleAsyncLoader> loaders = new List<AssetBundleAsyncLoader>();
                loaders.AddRange(_nonCompleteLoaderSet);
                _nonCompleteLoaderSet.Clear();

                var e = loaders.GetEnumerator();
                while (e.MoveNext())
                {
                    _currentLoadQueue.Add(e.Current);
                }

                e = loaders.GetEnumerator();
                while (e.MoveNext())
                {
                    e.Current.Start();
                }
                loaders.Clear();
            }
        }

        void CheckQueue()
        {
            while (_requestRemain > 0 && _requestQueue.Count > 0)
            {
                AssetBundleAsyncLoader loader = _requestQueue[0];
                _requestQueue.RemoveAt(0);
                LoadBundleAsync(loader);
            }
        }

        void LoadBundleAsync(AssetBundleAsyncLoader loader)
        {
            if (!loader.IsComplete)
            {
                loader.LoadBundle();
                _requestRemain--;
            }
        }

        internal void Enqueue(AssetBundleAsyncLoader loader)
        {
            if (_requestRemain < 0)
                _requestRemain = 0;
            _requestQueue.Add(loader);
        }

        internal void LoadError(AssetBundleAsyncLoader loader)
        {
            DebugUtil.LogError("### Cant load AB : " + loader.assetBundleInfo.AssetBundleName + " ###");
            LoadComplete(loader);
        }

        internal void LoadComplete(AssetBundleAsyncLoader loader)
        {
            _requestRemain++;
            AssetBundleCache.AddAssetBundle(loader.AssetBundle, loader.assetBundleInfo);
            _currentLoadQueue.Remove(loader);

            if (_currentLoadQueue.Count == 0 && _nonCompleteLoaderSet.Count == 0)
            {
                _isCurrentLoading = false;
                //Application.backgroundLoadingPriority = ThreadPriority.Low;
            }
        }

        IEnumerator LoadFromAssetBundle<T>(AssetBundle bundle, string fileName, Action<T> OnFinished = null) where T : Object
        {
            do
            {
                if (bundle == null)
                {
                    DebugUtil.LogError("LoadFromAssetBundle error: null, bundle = null");
                    break;
                }
                //float startTime = Time.time;
                var req = bundle.LoadAssetAsync<T>(fileName);
                yield return req;
                //Debug.LogError(fileName + " use time:" + (Time.time - startTime) + " From:" + startTime);
                OnFinished?.Invoke(req.asset as T);
            } while (false);
        }

        public void OnNotify(DownloadFileEvent downloadFileEvent)
        {
            switch (downloadFileEvent.stage)
            {
                case "finish":
                    AssetBundleCache.Unload(downloadFileEvent.name, true);
                    break;
            }
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// 统计时间
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
        public long loadTimes = 0;

        public struct LoadTimeMark
        {
            public string name;
            public long time;

            public LoadTimeMark(string _name, long _time)
            {
                name = _name;
                time = _time;
            }
        }
        public List<LoadTimeMark> loadTimeMarks = new List<LoadTimeMark>();
#endif
    }
}
