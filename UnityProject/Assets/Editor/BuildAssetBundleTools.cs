using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DragonPlus.Assets;
using DragonU3DSDK.Asset;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace OyTools
{
    public class BuildAssetBundleTools
    {
        private static BuildAssetBundleOptions recommandBundleOptions =
            BuildAssetBundleOptions.ChunkBasedCompression |
            BuildAssetBundleOptions.DeterministicAssetBundle;
//            | BuildAssetBundleOptions.EnableProtection;

        private static string PathRoot = AssetBundlePathConst.AssetBundlePath;//"Assets/AssetBundle";
        [MenuItem("AssetsManager/资源打AB标签")]
        public static void SetSettings()
        {
            var Variant = "";
            if (Directory.Exists(PathRoot))
            {
                DirectoryInfo direction = new DirectoryInfo(PathRoot);
                FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);

                for (int i = 0; i < files.Length; i++)
                {
                    var file = files[i];
                    if (file.Name.EndsWith(".meta"))
                    {
                        continue;
                    }

                    if (file.Name.EndsWith(".DS_Store"))
                    {
                        continue;
                    }

                    var assetsRoot = AssetBundlePathConst.AssetsRoot;
                    var relativeName = file.FullName.Substring(file.FullName.IndexOf(assetsRoot));
                    var path = relativeName.Substring(0, relativeName.LastIndexOf('/'));
//                var path = relativeName.Substring(0,relativeName.Length - file.Name.Length -1);
                    //				var path = file.DirectoryName.Replace(Application.dataPath, "").Replace ("\\", "/").Substring(1);
                    //				ai.SetAssetBundleNameAndVariant (path.Substring(1), Variant);
                    Debug.Log($" name = {file.Name},\t path={path}");
                    AssetImporter ai = AssetImporter.GetAtPath(relativeName);
                    ai.SetAssetBundleNameAndVariant(path, Variant);
                }

                AssetDatabase.Refresh();
            }
        }
        [MenuItem("AssetsManager/生成常量类")]
        public static void WriteClass()
        {
            // "Assets/QFrameworkData".CreateDirIfNotExists();

//            var path = Path.GetFullPath(
//                Application.dataPath + Path.DirectorySeparatorChar + "AssetConst.cs");
            var path = Path.Combine(Application.dataPath, "_Script/App/AssetConst.cs");
            var writer = new StreamWriter(File.Open(path, FileMode.Create));
            WriteClass(writer, "DragonPlus.Assets");
            writer.Close();
            AssetDatabase.Refresh();
        }

        private static string Path2CamelName(string path)
        {
            var paths = path.Split('/');
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < paths.Length; i++)
            {
                sb.Append(paths[i].Substring(0, 1).ToUpper()).Append(paths[i].Substring(1));
            }

            return sb.ToString();
        }

        public static void WriteClass(TextWriter writer, string nameSpace)
        {
            var assetBundleInfos = BuildAssetBundleTools.FileInfo2AssetConst(); // AddABInfo2ResDatas();//new List<AssetBundleInfo>();
            var compileUnit = new CodeCompileUnit();
            var codeNamespace = new CodeNamespace(nameSpace);
            compileUnit.Namespaces.Add(codeNamespace);

            foreach (var assetBundleInfo in assetBundleInfos)
            {
                var className = assetBundleInfo.assetBundleName;
//                className = DesignerTools.SimplifyName(Path2CamelName(className));
                var codeType = new CodeTypeDeclaration(DesignerTools.SimplifyName(Path2CamelName(className)));
                codeNamespace.Types.Add(codeType);
                var assetsRoot = "Assets/";
                var bundleNameField = new CodeMemberField
                {
                    Attributes = MemberAttributes.Public | MemberAttributes.Const,
                    Name = "BundleName",
                    Type = new CodeTypeReference(typeof(System.String)),
                    InitExpression = new CodePrimitiveExpression(className)
                };
                codeType.Members.Add(bundleNameField);

                var prefix = "_";
                //TODO 不去重处理，理论上不应该有重复名字的资源在同一个ab包中
                //				var checkRepeatDict = new Dictionary<string, string>();
                foreach (var asset in assetBundleInfo.assetNames)
                {
                    var content = Path.GetFileNameWithoutExtension(asset);
                    var ext = Path.GetExtension(asset)?.Substring(1).ToUpper() + prefix;
                    var assetField = new CodeMemberField
                    {
                        Attributes = MemberAttributes.Public | MemberAttributes.Const,
                        Name = ext + DesignerTools.SimplifyName(content?.ToUpperInvariant()),
                        Type = new CodeTypeReference(typeof(System.String)),
                        InitExpression = new CodePrimitiveExpression(content)
                    };
                    //					if (!assetField.Name.StartsWith("[") && !assetField.Name.StartsWith(" [") &&
                    //					    !checkRepeatDict.ContainsKey(assetField.Name))
                    //					{
                    //						checkRepeatDict.Add(assetField.Name, asset);
                    codeType.Members.Add(assetField);
                    //					}
                }

                //				checkRepeatDict.Clear();
            }

            //TODO using Microsoft.CSharp;
            var provider = new Microsoft.CSharp.CSharpCodeProvider();
            var options = new CodeGeneratorOptions
            {
                BlankLinesBetweenMembers = false,
                BracingStyle = "C"
            };

            provider.GenerateCodeFromCompileUnit(compileUnit, writer, options);
        }
        [MenuItem ("AssetsManager/清除AB标签")]
        static void ClearAssetBundlesName()
        {
            int length = AssetDatabase.GetAllAssetBundleNames().Length;
            string[] oldAssetBundleNames = new string[length];
            for (int i = 0; i < length; i++)
            {
                oldAssetBundleNames[i] = AssetDatabase.GetAllAssetBundleNames()[i];
            }

            for (int j = 0; j < oldAssetBundleNames.Length; j++)
            {
                AssetDatabase.RemoveAssetBundleName(oldAssetBundleNames[j], true);
            }
        }
        [MenuItem ("AssetsManager/清除未使用的AB标签")]
        static void ClearUnuseABName()
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
        }
        
//        [MenuItem ("AssetsManager/生成ABiOS")]
        static void BuildIOSAB()
        {
            BuildAssetBundle("iOS");
        } 
//        [MenuItem ("AssetsManager/生成ABAndroid")]
        static void BuildAndroidAB()
        {
            BuildAssetBundle("android");
        } 
//        [MenuItem ("AssetsManager/生成ABPC")]
        static void BuildPCAB()
        {
            BuildAssetBundle("window");
        } 
        [MenuItem ("AssetsManager/生成AB")]
        static void BuildCurrentPlatf()
        {
            BuildAssetBundle(AssetBundlePathConst.targetName);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns>AssetBundle 资源列表</returns>
        public static List<AssetBundleBuild> FileInfo2AssetConst ()
        {
//            var PathRoot = "Assets/AssetBundle";
            // var variant = "";
            Dictionary<string,AssetBundleBuildInfo> abnames = new Dictionary<string,AssetBundleBuildInfo>();
            if (Directory.Exists (PathRoot)) {
                DirectoryInfo direction = new DirectoryInfo (PathRoot);
                FileInfo[] files = direction.GetFiles ("*", SearchOption.AllDirectories);
    
                for (int i = 0; i < files.Length; i++) {
                    var file = files[i];
                    if (file.Name.EndsWith (".meta")) {
                        continue;
                    }
                    if (file.Name.EndsWith (".DS_Store")) {
                        continue;
                    }
    
                    var relativeName = file.FullName.Substring(file.FullName.IndexOf("Assets/", StringComparison.Ordinal));
                    AssetImporter ai = AssetImporter.GetAtPath (relativeName);
                    var path = relativeName.Substring(0,relativeName.Length - file.Name.Length -1);
                    Debug.Log($"name = {file.Name},\n path={path}");
                    if (!abnames.ContainsKey(path))
                    {
                        abnames.Add(path,new AssetBundleBuildInfo{Name=path,AssetNames = new List<string>(){relativeName}});
                    }
                    else
                    {
                        abnames[path].AssetNames.Add(relativeName);
                    }
                }
                var results = from info in abnames
                    select new AssetBundleBuild
                    {
                        assetBundleName = info.Key,
                        assetNames = info.Value.AssetNames.ToArray(),
                    };
                return results.ToList();
            }
    
            return null;
        }
        static void BuildAssetBundle(string platform)
        {
            AssetBundlePathTools.CreateFolder(AssetBundlePathConst.assetBundleOutPath);
            if(!Directory.Exists(AssetBundlePathConst.assetBundleOutPath)){
                Debug.Log($" path={AssetBundlePathConst.assetBundleOutPath} is not exist");
                return;
            }

            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            
            AssetBundleManifest assetBundleManifest = BuildPipeline.BuildAssetBundles(AssetBundlePathConst.assetBundleOutPath, recommandBundleOptions, buildTarget);
            if(assetBundleManifest != null){
                CopyToStreamingAsset(assetBundleManifest);
            }
        }
        static void BuildAssetBundle2(string platform)
        {
            AssetBundlePathTools.CreateFolder(AssetBundlePathConst.assetBundleOutPath);
            if(!Directory.Exists(AssetBundlePathConst.assetBundleOutPath)){
                Debug.Log($" path={AssetBundlePathConst.assetBundleOutPath} is not exist");
                return;
            }
            List<AssetBundleBuild> buildMap = FileInfo2AssetConst();//new List<AssetBundleBuild>();
            if(buildMap == null){
                Debug.Log($"not config assetbundle");
                return;
            }

            Debug.Log($"{buildMap.Count} + fisrt{ buildMap[0].assetBundleName},path={buildMap[0].assetNames.Length}");
            AssetBundleManifest assetBundleManifest = BuildPipeline.BuildAssetBundles(AssetBundlePathConst.assetBundleOutPath, buildMap.ToArray(), recommandBundleOptions, EditorUserBuildSettings.activeBuildTarget);
//            List<AssetBundleLoadInfo> assetBundleDependInfos = new List<AssetBundleLoadInfo>();
            
            if(assetBundleManifest != null){
                CopyToStreamingAsset(assetBundleManifest);
            }
        }

        static void CopyToStreamingAsset(AssetBundleManifest assetBundleManifest)
        {
            AssetBundleVersionWraper versionWraper = new AssetBundleVersionWraper();
            Dictionary<string,AssetBundleLoadInfo> loadInfos = new Dictionary<string, AssetBundleLoadInfo>();
            var buildMap = assetBundleManifest.GetAllAssetBundles();
            foreach (var bundleBuild in buildMap)
            {
                var bundleName = bundleBuild;
                loadInfos.Add(bundleName,new AssetBundleLoadInfo
                {
                    Name = bundleName,
                    Hash = assetBundleManifest.GetAssetBundleHash(bundleName).ToString(),
                    Dependancies = assetBundleManifest.GetAllDependencies(bundleName).ToList(),
                    Md5Str = AssetUtils.BuildFileMd5(Path.Combine(AssetBundlePathConst.assetBundleOutPath,bundleName)),
                });
            }

            foreach (var loadInfo in loadInfos)
            {
                Debug.Log($"json={loadInfo.Value.Dependancies.Count},key={loadInfo.Value.Hash}");
            }
            versionWraper.LoadInfos = loadInfos;
            var sourcesPath = AssetBundlePathConst.assetBundleOutPath;
            var targetPath =Path.Combine(Application.streamingAssetsPath, AssetBundlePathConst.RuntimeAssetsRoot);
            AssetBundlePathTools.CreateFolder(targetPath);
            AssetBundlePathTools.CopyDirectory(sourcesPath,targetPath);
            
            JsonSerializerSettings setting = new JsonSerializerSettings();
            setting.NullValueHandling = NullValueHandling.Ignore;
            string json = JsonConvert.SerializeObject(versionWraper,setting);
            Debug.Log($"json={json}");
            string filePath = Path.Combine(targetPath, "Version.txt");
            File.WriteAllText(filePath,json,Encoding.UTF8);
        }
    }
}