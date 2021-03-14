using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonU3DSDK.Asset
{
    public enum LoadState
    {
        State_None = 0,
        State_Loading = 1,
        State_Error = 2,
        State_Complete = 3
    }

    public class AssetBundleAsyncLoader
    {
        public delegate void LoaderCompleteHandler(AssetBundleAsyncLoader loader);
        public LoaderCompleteHandler onComplete;

        public AssetBundleInfo assetBundleInfo;
        public LoadState state = LoadState.State_None;
        protected int _currentLoadingDepCount;
        protected AssetBundle _bundle;
        protected bool _hasError;
        protected List<AssetBundleAsyncLoader> depLoaders;

        public AssetBundle AssetBundle
        {
            get
            {
                return _bundle;
            }
        }

        public bool IsComplete
        {
            get
            {
                return state == LoadState.State_Error || state == LoadState.State_Complete;
            }
        }

        public void Start()
        {
            if (_hasError)
                state = LoadState.State_Error;

            if (state == LoadState.State_None)
            {
                state = LoadState.State_Loading;
                this.LoadDepends();
            }
            else if (state == LoadState.State_Error)
            {
                this.Error();
            }
            else if (state == LoadState.State_Complete)
            {
                this.Complete();
            }
        }

        // 加载所有依赖包
        void LoadDepends()
        {
            if (depLoaders == null && assetBundleInfo.DependenciesBundleNames != null && assetBundleInfo.DependenciesBundleNames.Length > 0)
            {
                depLoaders = new List<AssetBundleAsyncLoader>();
                for (int i = 0; i < assetBundleInfo.DependenciesBundleNames.Length; i++)
                {
                    if (ResourcesManager.Instance.AssetBundleCache.GetAssetBundle(assetBundleInfo.DependenciesBundleNames[i]) == null)
                    {
                        depLoaders.Add(ResourcesManager.Instance.CreateLoader(assetBundleInfo.DependenciesBundleNames[i]));
                    }
                }
            }

            _currentLoadingDepCount = 0;
            if (depLoaders != null)
            {
                for (int i = 0; i < depLoaders.Count; i++)
                {
                    AssetBundleAsyncLoader depLoader = depLoaders[i];
                    if (!depLoader.IsComplete)
                    {
                        _currentLoadingDepCount++;
                        depLoader.onComplete += OnDepComplete;
                        depLoader.Start();
                    }
                }
            }

            this.CheckDepComplete();
        }

        // 加载完了依赖包后，加载自己
        public void LoadBundle()
        {
            ResourcesManager.Instance.StartCoroutine(LoadFromCachedFile());
        }

        protected virtual IEnumerator LoadFromCachedFile()
        {
            if (state != LoadState.State_Error)
            {
                string bundleFilePath = FilePathTools.GetBundleLoadPath(assetBundleInfo.AssetBundleName);
                AssetBundleCreateRequest req = AssetBundle.LoadFromFileAsync(bundleFilePath);
                yield return req;
                _bundle = req.assetBundle;
                this.Complete();
            }
        }

        void OnDepComplete(AssetBundleAsyncLoader aba)
        {
            _currentLoadingDepCount--;
            this.CheckDepComplete();
        }

        void CheckDepComplete()
        {
            if (_currentLoadingDepCount == 0)
            {
                ResourcesManager.Instance.Enqueue(this);
            }
        }

        protected void Complete()
        {
            state = LoadState.State_Complete;
            FireEvent();
            ResourcesManager.Instance.LoadComplete(this);
        }

        protected void Error()
        {
            _hasError = true;
            this.state = LoadState.State_Error;
            FireEvent();
            ResourcesManager.Instance.LoadError(this);
        }

        public void FireEvent()
        {
            if (onComplete != null)
            {
                var handler = onComplete;
                onComplete = null;
                handler(this);
            }
        }

        private void OnBundleUnload(AssetBundleInfo abi)
        {
            this.state = LoadState.State_None;
        }
    }
}
