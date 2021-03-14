using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Text;
using DragonPlus.Assets;

namespace OyTools
{
    public class MyEditorTools
    {
        private const string PathRoot = "Assets/AssetBundle/";

        [MenuItem("Tools/UI/预制体中重复资源检查", priority = 203)]
        public static void FindSameTexture()
        {
            Dictionary<string, string> md5dic = new Dictionary<string, string>();
            string[] paths = AssetDatabase.FindAssets("t:prefab", new string[] {"Assets"});
            MD5 md5p = new MD5CryptoServiceProvider();
            foreach (var prefabguid in paths)
            {
                string prefabAssetPath = AssetDatabase.GUIDToAssetPath(prefabguid);
                string[] depends = AssetDatabase.GetDependencies(prefabAssetPath, true);
                for (int i = 0; i < depends.Length; i++)
                {
                    string assetPath = depends[i];
                    AssetImporter importer = AssetImporter.GetAtPath(assetPath);
                    if (importer is TextureImporter || importer is ModelImporter)
                    {
                        string md5 = getMd5Hash(Path.Combine(Directory.GetCurrentDirectory(), assetPath),md5p);
                        string path;
                        if (!md5dic.TryGetValue(md5, out path))
                        {
                            md5dic[md5] = assetPath;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(path) && path != assetPath)
                            {
                                Debug.Log($"<color=blue> {path}</color>" ,AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
							    Debug.Log($"<color=red> {assetPath}</color>" ,AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath));
                            }
                        }
                    }
                }
            }
        }

        [MenuItem("Tools/UI/重复Texture检查", priority = 202)]
        public static void FindSameRes()
        {
            Dictionary<string, string> md5dic = new Dictionary<string, string>();
            string[] paths = AssetDatabase.FindAssets("t:Texture", new string[] {"Assets"});
            string currentpath = Directory.GetCurrentDirectory();
            Debug.LogFormat("currentDirectory:{0}", currentpath);
            MD5 md5p = new MD5CryptoServiceProvider();
            foreach (var prefabguid in paths)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(prefabguid);
                // AssetImporter importer = AssetImporter.GetAtPath (assetPath);
                // if (importer is TextureImporter || importer is ModelImporter) {
                string md5 = getMd5Hash(Path.Combine(currentpath, assetPath),md5p);
                string path;
                if (!md5dic.TryGetValue(md5, out path))
                {
                    md5dic[md5] = assetPath;
                }
                else
                {
                    if (!string.IsNullOrEmpty(path) && path != assetPath)
                    {
                        Debug.Log($"<color=green>{path}</color>" ,AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
						Debug.Log($"<color=red> {assetPath}</color>" ,AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath));
                    }
                }

                // }
            }
        }

        public static string getMd5Hash(string path,MD5 md5 )
        {
            string rst = BitConverter.ToString(md5.ComputeHash(File.ReadAllBytes(path)));
            rst = rst.Replace("-", "").ToLower();
            return rst;
        }

        [MenuItem("Assets/FindwhoUsedMe", false, 10)]
        public static void findRef()
        {
            Dictionary<string, string> guidDics = new Dictionary<string, string> ();
            foreach (UnityEngine.Object o in Selection.objects) {
                string path = AssetDatabase.GetAssetPath (o);
                if (!string.IsNullOrEmpty (path)) {
    //				string guid = AssetDatabase.AssetPathToGUID (path);
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(o, out var guid, out long localId);
                    if (!guidDics.ContainsKey (guid)) {
                        guidDics[guid] = path;
                    }
                    Debug.Log($"{o.name }'s localid ={localId}");
                }
            }
            if (guidDics.Count > 0) {
                List<string> extensions = new List<string> () { ".prefab", ".unity", ".mat", ".asset" };
                string[] files = Directory.GetFiles (Application.dataPath, "*.*", SearchOption.AllDirectories).Where (
                    s => extensions.Contains (Path.GetExtension (s).ToLower ())).ToArray ();

                for (int i = 0; i < files.Length; i++) {
                    string file = files[i];
                    if(i %20 == 0){
                        bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中",file,(float)i/(float)files.Length);
                        if(isCancel){
                            break;
                        }
                    }
                    foreach(KeyValuePair<string,string> guidItem in guidDics){
                        if(Regex.IsMatch(File.ReadAllText(file),guidItem.Key)){
                            Debug.Log($"<color=green>{guidItem.Value}</color>" ,AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(guidItem.Value));
                            Debug.Log($"<color=red> {file}</color>" ,AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(getRelativeAssetPath(file)));
                        }
                    }
                }
                EditorUtility.ClearProgressBar();
            }
        }

        [MenuItem("Assets/FindwhoUsedMe", true)]
        public static bool vfindRef()
        {
            if (!Selection.activeObject)
            {
                return false;
            }

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            return (!string.IsNullOrEmpty(path));
        }

        /// <summary>
        /// 相对Assets 相对目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static string getRelativeAssetPath(string path)
        {
            return path.Replace(Application.dataPath, "Assets").Replace("\\", "/");
        }

        
        
        private static List<AssetBundleBuildInfo> FileInfo2AssetConst()
        {
            var PathRoot = "Assets/AssetBundle";
//        var Variant = "";
            Dictionary<string, AssetBundleBuildInfo> abnames = new Dictionary<string, AssetBundleBuildInfo>();
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

                    var relativeName =
                        file.FullName.Substring(file.FullName.IndexOf("Assets/", StringComparison.Ordinal));
                    AssetImporter ai = AssetImporter.GetAtPath(relativeName);
                    var path = relativeName.Substring(0, relativeName.Length - file.Name.Length - 1);
                    Debug.Log($" name = {file.Name},\t path={path}");
                    if (!abnames.ContainsKey(path))
                    {
                        abnames.Add(path,
                            new AssetBundleBuildInfo {Name = path, AssetNames = new List<string>() {file.Name}});
                    }
                    else
                    {
                        abnames[path].AssetNames.Add(file.Name);
                    }
                }

                return abnames.Values.ToList();
            }

            return null;
        }
    }


    /// <summary>
    /// AlterAssetBundle类为修改批量修改AssetBundle的Name与Variant的编辑器窗口
    /// </summary>
    public class AlterAssetBundle : EditorWindow
    {

        [MenuItem("AssetsManager/TagAssetBundle")]
        static void AddWindow()
        {
            //创建窗口
            AlterAssetBundle window =
                (AlterAssetBundle) EditorWindow.GetWindow(typeof(AlterAssetBundle), false, "批量修改AssetBundle");
            window.Show();

        }


        //输入文字的内容
        private const string AssetBundleRoot = "Assets/RefRes"; // "Assets/assetbundle";
        private string PathRoot = "Assets/AssetBundle/", AssetBundleName = "bundlename", Variant = "";
        private bool IsThisName = true;

        void OnGUI()
        {
            GUIStyle text_style = new GUIStyle();
            text_style.fontSize = 15;
            text_style.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("默认使用源文件名", GUILayout.MinWidth(0));
            IsThisName = EditorGUILayout.Toggle(IsThisName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("AssetBundleName:", GUILayout.MinWidth(0));
            if (IsThisName)
                GUILayout.Label("文件相对Assets/Export的路径", GUILayout.MinWidth(0));
            else
                AssetBundleName = EditorGUILayout.TextField(AssetBundleName.ToLower());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Variant:", GUILayout.MinWidth(0));
            Variant = EditorGUILayout.TextField(Variant.ToLower());
            EditorGUILayout.EndHorizontal();

            GUILayout.Label("\n");

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("文件夹路径", GUILayout.MinWidth(60));
            if (GUILayout.Button("浏览", GUILayout.MinWidth(60)))
            {
                OpenFolder();
            }

            PathRoot = EditorGUILayout.TextField(PathRoot);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("修改该文件夹下的AssetName及Variant"))
            {
                BuildAssetBundleTools.SetSettings();
            }

            if (GUILayout.Button("清除所有未被引用的AssetName及Variant"))
            {
                AssetDatabase.RemoveUnusedAssetBundleNames();
            }

            if (GUILayout.Button("清空所有AssetName及Variant"))
            {
                ClearAssetBundlesName();
            }
        }

        /// <summary>
        /// 此函数用来打开文件夹修改路径
        /// </summary>
        void OpenFolder()
        {
            string m_path = EditorUtility.OpenFolderPanel("选择文件夹", "", "");
            if (!m_path.Contains(Application.dataPath))
            {
                Debug.LogError("路径应在当前工程目录下");
                return;
            }

            if (m_path.Length != 0)
            {
                int firstindex = m_path.IndexOf("Assets");
                PathRoot = m_path.Substring(firstindex) + "/";
                EditorUtility.FocusProjectWindow();
            }
        }


        /// <summary>
        /// 清除之前设置过的AssetBundleName，避免产生不必要的资源也打包
        /// 工程中只要设置了AssetBundleName的，都会进行打包
        /// </summary>
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

        

        /// <summary>
        /// 此函数用来修改AssetBundleName与Variant
        /// </summary>



//	 public static List<AssetBundleBuildInfo> AddABInfo2ResDatas()
//        {
//            AssetDatabase.RemoveUnusedAssetBundleNames();
//
//            var abNames = AssetDatabase.GetAllAssetBundleNames();
//			List<AssetBundleBuildInfo> assetBundleInfos = new List<AssetBundleBuildInfo>();
//            if (abNames != null && abNames.Length > 0)
//            {
//                foreach (var abName in abNames)
//                {
//                    // var depends = AssetDatabase.GetAssetBundleDependencies(abName, false);
//                    // AssetDataGroup group;
//                    // var abIndex = assetBundleConfigFile.AddAssetBundleName(abName, depends, out @group);
//                    // if (abIndex < 0)
//                    // {
//                    //     continue;
//                    // }
//
//                    var assets1 = AssetDatabase.GetAssetPathsFromAssetBundle(abName);
//					var assetBundleInfo = new AssetBundleBuildInfo{
//						Name = abName,
//						assets = assets1.ToList()
//					};
//					assetBundleInfos.Add(assetBundleInfo);
//                    // foreach (var cell in assets)
//                    // {
//						
//                    //     var type = AssetDatabase.GetMainAssetTypeAtPath(cell);
//
//                    //     var code = type.ToCode();
//
//                    //     // Debug.Log(cell + ":" + code + ":" + type);
//
//                    //     @group.AddAssetData(cell.EndsWith(".unity")
//                    //         ? new AssetData(AssetPath2Name(cell), ResLoadType.ABScene, abIndex, abName, code)
//                    //         : new AssetData(AssetPath2Name(cell), ResLoadType.ABAsset, abIndex, abName, code));
//                    // }
//                }
//            }
//
//			return assetBundleInfos ;
//        }

        // [MenuItem ("AssetsManager/重命名小写")]
        // public static void ChangeAbFileNameToLower(){
        // 	if (Directory.Exists (AssetBundleRoot)) {
        // 		DirectoryInfo direction = new DirectoryInfo (AssetBundleRoot);
        // 		// DirectoryInfo[] dirs= direction.GetDirectories("*", SearchOption.AllDirectories);
        // 		// for (var j = 0; j < dirs.Length; j++)
        // 		// {
        // 		// 	dirs[j].MoveTo(dirs[j].FullName.ToLower());
        // 		// }

        // 		FileInfo[] files = direction.GetFiles ("*", SearchOption.AllDirectories);
        // 		for (int i = 0; i < files.Length; i++) {
        // 			var file = files[i];
        // 			if (file.Name.EndsWith (".meta")) {
        // 				continue;
        // 			}
        // 			if (file.Name.EndsWith (".DS_Store")) {
        // 				continue;
        // 			}
        // 			file.MoveTo(file.FullName.ToLower());

        // 		// 	// AssetImporter ai = AssetImporter.GetAtPath (file.FullName.Substring (file.FullName.IndexOf ("Assets")));
        // 		// 	// var path = file.DirectoryName.Replace(Application.dataPath, "").Replace ("\\", "/");
        // 		// 	// Debug.Log($"file dir={file.DirectoryName} fullname={file.FullName},name = {file.Name},path={path}");
        // 		}
        // 		AssetDatabase.Refresh ();
        // 	}
        // }
        void OnInspectorUpdate()
        {
            this.Repaint(); //窗口的重绘
        }
    }
}