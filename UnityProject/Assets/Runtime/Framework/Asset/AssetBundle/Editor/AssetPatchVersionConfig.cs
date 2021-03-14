/*-------------------------------------------------------------------------------------------
// Copyright (C) 2020 成都，天龙互娱
//
// 模块名：BuildAssetBundles
// 创建日期：2020-7-21
// 创建者：qibo.li
// 模块描述：AssetBundle打包迭代bundle的hash值
//-------------------------------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace DragonU3DSDK.Asset
{
    [Serializable]
    public class AssetPatchVersion
    {
        public string name;
        public int version;
    }
    
    public class AssetPatchVersionConfig : ScriptableObject
    {
        private static AssetPatchVersionConfig _instance = null;
        public static AssetPatchVersionConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = AssetDatabase.LoadAssetAtPath<AssetPatchVersionConfig>("Assets/Editor/AssetPatchVersionConfig.asset");
                }
                return _instance;
            }
        }

        #region data

        [Space(10)]
        [Header(" --------------------- Android 资源迭代版本 ----------------------")]
        public List<AssetPatchVersion> AndroidAssetsPatchVersion = new List<AssetPatchVersion>();
        
        [Space(10)]
        [Header(" --------------------- IOS 资源迭代版本 ----------------------")]
        public List<AssetPatchVersion> IOSAssetsPatchVersion = new List<AssetPatchVersion>();
        #endregion

        #region tool

        [MenuItem("AssetBundle/Patch/创建资源迭代版本配置")]
        public static void CreateAssetPatchVersionConfig()
        {
            AssetPatchVersionConfig asset = CreateInstance<AssetPatchVersionConfig>();

            if (!Directory.Exists("Assets/Editor"))
                Directory.CreateDirectory("Assets/Editor");
            
            AssetDatabase.CreateAsset(asset, "Assets/Editor/AssetPatchVersionConfig.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }
        
        [MenuItem("AssetBundle/Patch/Android/迭代所有的普通资源")]
        public static void PatchAllAndroidNormalAssets() { PatchAllNormalAssets(0); }
        [MenuItem("AssetBundle/Patch/IOS/迭代所有的普通资源")]
        public static void PatchAllIOSNormalAssets() { PatchAllNormalAssets(1); }
        static void PatchAllNormalAssets(int type)
        {
            for (int i = 0; i < AssetConfigController.Instance.Groups.Length; i++)
            {
                List<string> patchs = AssetConfigController.Instance.Groups[i].GetBundlePaths();
                foreach (var p in patchs)
                {
                    PatchAsset(type, p);
                }
            }
            
            EditorUtility.SetDirty(Instance);
            AssetDatabase.SaveAssets();
        }

        [MenuItem("AssetBundle/Patch/Android/迭代所有的活动资源")]
        public static void PatchAllAndroidActivityAssets() { PatchAllActivityAssets(0); }
        [MenuItem("AssetBundle/Patch/IOS/迭代所有的活动资源")]
        public static void PatchAllIOSActivityAssets() { PatchAllActivityAssets(1); }
        static void PatchAllActivityAssets(int type)
        {
            for (int i = 0; i < AssetConfigController.Instance.ActivityResPaths.Length; i++)
            {
                PatchAsset(type ,AssetConfigController.Instance.ActivityResPaths[i]);
            }
            
            EditorUtility.SetDirty(Instance);
            AssetDatabase.SaveAssets();
        }
        
        [MenuItem("Assets/Patch/Android/迭代选中资源")]
        public static void PatchSelectAssets() { _PatchSelectAssets(0); }
        [MenuItem("Assets/Patch/IOS/迭代选中资源")]
        public static void PatchIOSSelectAssets() { _PatchSelectAssets(1); }
        static void _PatchSelectAssets(int type)
        {
            if (null == Selection.activeObject)
            {
                DebugUtil.LogError("没有选中任何资源");
            }
            else
            {
                string select_path = AssetDatabase.GetAssetPath(Selection.activeObject);
                if (select_path.IndexOf("Assets/Export") != 0)
                {
                    DebugUtil.LogError("请选择Assets/Export目录下的资源");
                }
                else if (!Directory.Exists(select_path))
                {
                    DebugUtil.LogError("请选择有效的文件夹目录");
                }
                else
                {
                    string ab_patch = select_path.Substring("Assets/Export".Length + 1);
                    PatchAsset(type, ab_patch);
                    
                    EditorUtility.SetDirty(Instance);
                    AssetDatabase.SaveAssets();
                }
            }
        }
        
        static void PatchAsset(int type, string name)
        {
            List<AssetPatchVersion> list = type == 0 ? Instance.AndroidAssetsPatchVersion : Instance.IOSAssetsPatchVersion;
            
            AssetPatchVersion patchVersion = list.Find(x => x.name.Equals(name));
            if (null == patchVersion)
            {
                patchVersion = new AssetPatchVersion();
                patchVersion.name = name;
                patchVersion.version = 1;
                list.Add(patchVersion);
            }

            patchVersion.version++;
            
            DebugUtil.Log(string.Format("asset : {0}, patch version : {1}, target : {2}", name, patchVersion.version, type == 0 ? "android" : "ios"));
        }

        #endregion
    }
}
