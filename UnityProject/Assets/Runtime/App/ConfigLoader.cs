using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gloabal
{
    /// <summary>
    /// 多线程锁模式
    /// </summary>
	public sealed class ConfigLoader :IConfigLoader{
        private static volatile ConfigLoader instance;
        private static readonly Object syncRootObject = new Object ();
        public AssetConfigController AssetsConfig{get;private set;}
        public static ConfigLoader Instance {
            get {
                if (instance == null) {
                    lock (syncRootObject) {
                        if (instance == null) {
                            instance = new ConfigLoader ();
                        }
                    }
                }
                return instance;
            }
        }

		public void Load()
		{
			AssetsConfig = Resources.Load<AssetConfigController>(AssetConfigController.AssetConfigPath);
            ResourcesManager.Instance.UseSDAtlas(true);
            Debug.Log ("ConfigLoader init");
		}
        public void Unload(){
            Resources.UnloadAsset(AssetsConfig);
        }
	}
    /// <summary>
    /// 懒汉模式
    /// </summary>
    public sealed class ConfigLoader1:IConfigLoader {
        private static readonly Lazy<ConfigLoader1> _instance = new Lazy<ConfigLoader1> (() => new ConfigLoader1 ());

        public static ConfigLoader1 Instance { get { return _instance.Value; } }

        private ConfigLoader1 () {
            Debug.Log ("ConfigLoader1 private init");
        }

		public void Load()
		{
			Debug.Log ("ConfigLoader1 init");
		}
         public void Unload(){
            Resources.UnloadUnusedAssets();
        }
	}
    /// <summary>
    /// 饿汉模式
    /// </summary>
    public sealed class ConfigLoader2 :IConfigLoader{
        static ConfigLoader2 () {
            Debug.Log ("ConfigLoader2 static init");
         }

        private ConfigLoader2 () {
            Debug.Log ("ConfigLoader2 private init");
         }

        public static ConfigLoader2 Instance { get; } = new ConfigLoader2 ();
        public ConfigurationController GlobalConfig{get;private set;}
		public void Load()
		{
            GlobalConfig = Resources.Load<ConfigurationController>(ConfigurationController.ConfigurationControllerPath);
			Debug.Log ("ConfigLoader2 init");
		}
         public void Unload(){
            Resources.UnloadAsset(GlobalConfig);
        }
	}
}