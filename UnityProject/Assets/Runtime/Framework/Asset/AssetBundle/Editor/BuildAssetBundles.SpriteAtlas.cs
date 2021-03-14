using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace DragonU3DSDK.Asset
{
    public static partial class BuildAssetBundles
    {
        [MenuItem("AssetBundle/SpriteAtlas/生成AtlasConfig")]
        public static void CreateAtlasConfig()
        {

            AtlasConfigController asset = ScriptableObject.CreateInstance<AtlasConfigController>();
            asset.ParseAtlasPath();

            AssetDatabase.CreateAsset(asset, "Assets/Resources/Settings/AtlasConfigController.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        } 
        
        [MenuItem("AssetBundle/SpriteAtlas/生成AssetBundlConfig")]
        public static void CreateAssetBundleConfig()
        {

            AsetBundleConfigController asset = ScriptableObject.CreateInstance<AsetBundleConfigController>();
            asset.ParseAtlasPath();

            AssetDatabase.CreateAsset(asset, "Assets/Resources/Settings/AsetBundleConfigController.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = asset;
        }

    }
}
