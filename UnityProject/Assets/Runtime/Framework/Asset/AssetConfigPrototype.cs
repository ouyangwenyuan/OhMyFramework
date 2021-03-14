using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

namespace DragonU3DSDK.Asset
{
    public class AssetConfigPrototype : ScriptableObject
    {
        public static string AssetConfigPath = "Settings/AssetConfigPrototype";
        private static AssetConfigPrototype _instance = null;

        public static AssetConfigPrototype Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<AssetConfigPrototype>(AssetConfigPath);
                }
                return _instance;
            }
        }

        [Space(10)]
        [Header("[填入相对Assets/Export的相对路径，大小写敏感，只支持文件夹]")]
        [Header(" ---------------------- AB包分组 -----------------------")]
        public BundleGroup[] Groups;

        public void Modify(AssetConfigController assetConfigController)
        {
            HashSet<string> activityResSet = new HashSet<string>(assetConfigController.ActivityResPaths);

            string root = "Assets/Export/";
            for (int i = 0; i < Groups.Length; i++)
            {
                BundleGroup group = Groups[i];
                var newGroup = new BundleGroup();
                newGroup.GroupName = group.GroupName;
                newGroup.Version = group.Version;
                newGroup.GroupIndex = group.GroupIndex;
                newGroup.Paths = new List<BundleState>();
                newGroup.UpdateWholeAB = group.UpdateWholeAB;

                var dict = new Dictionary<string, BundleState>();
                var datas = new List<BundleState>(group.Paths);
                datas.Sort((a, b) => Convert.ToInt32(a.InInitialPacket) - Convert.ToInt32(b.InInitialPacket));

                foreach (var data in datas)
                {
                    string dir = Path.GetDirectoryName(data.Path);
                    string name = Path.GetFileName(data.Path);
                    string[] subDirs = FilePathTools.GetDirectories(root + dir, "^" + name + "$");
                    foreach (string subDir in subDirs)
                    {
                        string path = subDir.Replace(root, "");
                        if (activityResSet.Contains(path) || FilePathTools.GetFiles(subDir, "^(?!\\.)").Length == 0)
                        {
                            continue;
                        }

                        BundleState bundleState = new BundleState
                        {
                            Path = path,
                            InInitialPacket = data.InInitialPacket
                        };
                        dict[bundleState.Path] = bundleState;
                    }
                }
                newGroup.Paths.AddRange(dict.Values);
                newGroup.Paths.Sort((a, b) => string.Compare(a.Path, b.Path));

                if (i < assetConfigController.Groups.Length)
                {
                    assetConfigController.Groups[i] = newGroup;
                }
            }
        }
    }
}