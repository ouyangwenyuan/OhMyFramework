using System.Collections.Generic;
using System;
using System.IO;

namespace DragonU3DSDK.Asset
{
    [Serializable]
    public class BaseVersionFileItem
    {
        public string batchName;
        public string sourcePath;
        public string targetPath;
        public string md5;
        public int versionCode;
    }

    [Serializable]
    public class BaseFolderInfo
    {
        // 本地文件夹路径
        public string Path;
        public bool InInitialPacket = false;
    }

    [Serializable]
    public abstract class BaseResFakeInfo
    {
        public string DownloadABPath;
        public string LocalABPath;
        public bool InInitialPacket = false;
        public int VersionCode;
        public string Md5;
        public string Md5Ios;
        public string TargetPath;
        public List<string> DependenciesBundleNames;

        /// <summary>
        /// 资源下载地址,区分debug/release
        /// </summary>
        /// <returns></returns>
        public abstract string GetDownLoadUrl();

        /// <summary>
        /// 打包时,初始资源都从测试服拉取
        /// </summary>
        /// <returns></returns>
        public abstract string GetDownLoadUrlForInitial();

        public abstract string GetPlatformMd5();
    }

    public static class CommonResUtils
    {
        public static void AddPlaceholderFile(string path)
        {
            if (Directory.Exists(path))
            {
                //foreach (string dir in Directory.GetDirectories(path))
                //{
                //    Directory.Delete(dir, true);
                //}
                //foreach (string file in Directory.GetFiles(path))
                //{
                //    File.Delete(file);
                //}
                //DebugUtil.LogError("成功清除" + path);
            }
            else
            {
                Directory.CreateDirectory(path);
                DebugUtil.LogError("成功创建" + path);
            }

            string filePath = path + "/placeholder.txt";
            if (!File.Exists(filePath))
            {
                StreamWriter sw;
                FileInfo t = new FileInfo(filePath);
                sw = t.CreateText();
                //以行的形式写入信息
                sw.WriteLine("this is a placeholder_file for buildAssetBundles");
                //关闭流
                sw.Close();
                //销毁流
                sw.Dispose();
            }
        }

        // public static int GetRemoteResMapId(int localId)
        // {
        //     if (CookingGameConfigController.Instance != null)
        //     {
        //         foreach (var mapInfo in CookingGameConfigController.Instance.MapInfos)
        //         {
        //             if (mapInfo.LocalId == localId)
        //             {
        //                 return mapInfo.RemoteId;
        //             }
        //         }
        //     }

        //     return localId;
        // }
        
        // public static int GetLocalResMapId(int remoteId)
        // {
        //     if (CookingGameConfigController.Instance != null)
        //     {
        //         foreach (var mapInfo in CookingGameConfigController.Instance.MapInfos)
        //         {
        //             if (mapInfo.RemoteId == remoteId)
        //             {
        //                 return mapInfo.LocalId;
        //             }
        //         }
        //     }

        //     return remoteId;
        // }
    }
}