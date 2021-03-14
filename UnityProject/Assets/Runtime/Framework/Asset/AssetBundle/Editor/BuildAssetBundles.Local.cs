/*-------------------------------------------------------------------------------------------
// Copyright (C) 2019 北京，天龙互娱
//
// 模块名：BuildAssetBundles.Local
// 创建日期：2020-7-24
// 创建者：waicheng.wang
// 模块描述：AssetBundle打包 用于本地，非正式出包流程
//-------------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;
using Dlugin;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using UObject = UnityEngine.Object;

namespace DragonU3DSDK.Asset
{
    public static partial class BuildAssetBundles
    {
        #region  活动资源打包
        [MenuItem("Assets/运营活动/打包整个文件夹")]
        static void BuildActivityAssetBundle()
        {
            AssetDatabase.Refresh();
            List<AssetBundleBuild> buildMap = new List<AssetBundleBuild>();
            string topPath;
            string abFileNameNoSuffix;
            string abFileName;
            string outPath = FilePathTools.assetBundleOutPath + "/Activity" + DateTime.Now.ToString("yyyy-MM-dd");

            UnityEngine.Object[] selects = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
            int length = selects.Length;
            if (length > 0)
            {
                topPath = GetSelectedPathOrFallback(selects);
                //DebugUtil.Log("F: " + topPath);
                topPath = topPath.Substring(14);//删除"Asset/Export/"
                //DebugUtil.Log("S: " + topPath);
                abFileNameNoSuffix = topPath.Replace('/', '_').ToLower();
                abFileName = string.Format("{0}.ab", abFileNameNoSuffix);
                //DebugUtil.Log(abFileNameNoSuffix);
            }
            else
            {
                DebugUtil.LogError("不要对空文件夹进行打包!!");
                return;
            }

            List<string> assetnames = new List<string>();
            AssetBundleBuild build = new AssetBundleBuild
            {
                assetBundleName = abFileName
            };

            for (int i = 0; i < length; i++)
            {
                string objPath = AssetDatabase.GetAssetPath(selects[i]);
                //DebugUtil.Log(objPath);
                assetnames.Add(objPath);
            }
            build.assetNames = assetnames.ToArray();
            buildMap.Add(build);


            //if (Directory.Exists(TargetPath))
            //Directory.Delete(TargetPath, true);

            if (!Directory.Exists(outPath))
                Directory.CreateDirectory(outPath);

            BuildPipeline.SetAssetBundleEncryptKey(AssetConfigController.Instance.EnhancedEncryptionSecret);
            AssetBundleManifest mainfest = BuildPipeline.BuildAssetBundles(outPath, buildMap.ToArray(), recommandBundleOptions, EditorUserBuildSettings.activeBuildTarget);

            //------------------------------------ 压缩，记录md5 ---------------------------------//

            string assetBundlePath = string.Format("{0}/{1}", outPath, abFileName);
            //string tempPath = assetBundlePath + ".temp";
            //Zip.Tool.CompressFileLZMA(assetBundlePath, tempPath);
            //File.Delete(assetBundlePath);
            string hash = mainfest.GetAssetBundleHash(abFileName).ToString();
            string md5 = AssetUtils.BuildFileMd5(assetBundlePath);
            string abFileNameWithMd5 = string.Format("{0}/{1}_{2}_{3}.ab", outPath, abFileNameNoSuffix, hash, md5);
            File.Move(assetBundlePath, abFileNameWithMd5);

            //----------------------------------- End 压缩，记录md5 ------------------------------//

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            DebugUtil.Log("BuildActivityAssetBundle finish");
        }

        public static string GetSelectedPathOrFallback(UnityEngine.Object[] objs)
        {
            string path = "Assets";

            foreach (UnityEngine.Object obj in objs)
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }

        #endregion
        
        [MenuItem("AssetBundle/Patch/打包AssetCheck场景")]
        public static void BuildAssetCheckScene()
        {
            string target = "";
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                target = "Android";
            }
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                target = "iOS";
            }
            else
            {
                DebugUtil.LogError(string.Format("{0}当前平台不支持", EditorUserBuildSettings.activeBuildTarget));
                return;
            }
            
            BuildPipeline.BuildPlayer(new string[] {"Assets/DragonSDK/DragonU3DSDK/Framework/Asset/AssetCheck/AssetCheck.unity"}, 
                string.Format("{0}/DragonSDK/DragonU3DSDK/Framework/Asset/AssetCheck/Resources/{1}/AssetCheck.bytes", Application.dataPath, target), 
                EditorUserBuildSettings.activeBuildTarget, BuildOptions.BuildAdditionalStreamedScenes);  
            AssetDatabase.Refresh();
        }
        
        public static void CheckPatchDiff()
        {
            string path1 = "";
            string path2 = "";
            string[] allPatch1 = FilePathTools.GetFiles(path1, @"(.*)(\.patch)$", SearchOption.AllDirectories);
            string[] allPatch2 = FilePathTools.GetFiles(path2, @"(.*)(\.patch)$", SearchOption.AllDirectories);
            foreach (var p1 in allPatch1)
            {
                JObject jobj1 = JObject.Parse(File.ReadAllText(p1));
                string hash1 = jobj1["hash"].ToString();
                int index = allPatch2.IndexOfEx((x) => x.Substring(path2.Length).Equals(p1.Substring(path1.Length)));
                if (index >= 0)
                {
                    JObject jobj2 = JObject.Parse(File.ReadAllText(allPatch2[index]));
                    string hash2 = jobj2["hash"].ToString();
                    if (!hash1.Equals(hash2))
                    {
                        Debug.Log(string.Format("dif : {0}", p1));
                    }
                }
                else
                {
                    Debug.Log(string.Format("only 1 : {0}", p1));
                }
            }
            
            foreach (var p1 in allPatch2)
            {
                JObject jobj1 = JObject.Parse(File.ReadAllText(p1));
                string hash1 = jobj1["hash"].ToString();
                int index = allPatch1.IndexOfEx((x) => x.Substring(path1.Length).Equals(p1.Substring(path2.Length)));
                if (index >= 0)
                {
                    JObject jobj2 = JObject.Parse(File.ReadAllText(allPatch1[index]));
                    string hash2 = jobj2["hash"].ToString();
                    if (!hash1.Equals(hash2))
                    {
                        Debug.Log(string.Format("dif : {0}", p1));
                    }
                }
                else
                {
                    Debug.Log(string.Format("only 2 : {0}", p1));
                }
            }
            
            Debug.Log("finish");
        }
        
        [MenuItem("AssetBundle/Patch/打包所有资源[Jenkins风格]")]
        public static void BuildAllAssetBundleWithJenkinsStyle()
        {
            BuildPre();
            
            if (AssetConfigController.Instance.ProjectPretreatmentDelegate)
                AssetConfigController.Instance.ProjectPretreatmentDelegate.ExecutePretreatment();
#if !UNITY_STANDALONE             
            BuildPipeline.SetAssetBundleEncryptKey(AssetConfigController.Instance.EnhancedEncryptionSecret);
#endif
            BuildAllAssetBundleWithOptions(recommandBundleOptions);
            
            BuildPost();
            
            DebugUtil.Log("BuildAllAssetBundle finish");
        }
        
        [MenuItem("AssetBundle/Patch/执行工程委托")]
        public static void OperProjectPretreatmentDelegate()
        {
            if (AssetConfigController.Instance.ProjectPretreatmentDelegate)
                AssetConfigController.Instance.ProjectPretreatmentDelegate.ExecutePretreatment();
            
            DebugUtil.Log("OperProjectPretreatmentDelegate finish");
        }
    }
}