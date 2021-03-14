/*-------------------------------------------------------------------------------------------
// Copyright (C) 2019 北京，天龙互娱
//
// 模块名：EditorResourcePaths
// 创建日期：2020-4-8
// 创建者：guomeng.lu
// 模块描述：管理编辑器用到的一些资源路径
//-------------------------------------------------------------------------------------------*/
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using DragonU3DSDK.Asset;

namespace DragonU3DSDK
{
    public class EditorResourcePaths
    {
        class PathPattern
        {
            public string dir;
            public string regex;
            public SearchOption searchOption;

            public PathPattern(string dir, string regex, SearchOption searchOption = SearchOption.TopDirectoryOnly)
            {
                this.dir = EditorCommonUtils.NormalizePath(dir);
                this.regex = regex;
                this.searchOption = searchOption;
            }
        }

        public const string ASSETS_ROOT = "Assets/";
        public const string CONFIG_NAME = "_resourcePathConfig.json";
        public const string CONFIG_PATH = ASSETS_ROOT + "Res/" + CONFIG_NAME;
        public const string DEFAULT_CONFIG_PATH = ASSETS_ROOT + "DragonSDK/DragonU3DSDK/Framework/Editor/" + CONFIG_NAME;
        public const string META_SUFFIX = ".meta";
        public const string ATLAS_SUFFIX = ".spriteatlas";
        public const string FILTER_META_REGEX = "^(?!.*\\.(meta)$)";
        public const string PNG_REGEX = ".*\\.((png)|(jpg))$";

        public static List<string> GetAllSelections(SelectionMode mode)
        {
            List<string> list = new List<string>();

            Object[] selections = Selection.GetFiltered(typeof(Object), mode);
            foreach (Object selection in selections)
            {
                list.Add(AssetDatabase.GetAssetPath(selection));
            }

            return list;
        }

        public static List<string> GetFileSelections()
        {
            List<string> list = GetAllSelections(SelectionMode.DeepAssets);
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (!File.Exists(list[i]))
                {
                    list.RemoveAt(i);
                }
            }
            return list;
        }

        public static HashSet<string> GetTextureFiles(List<string> files)
        {
            HashSet<string> list = new HashSet<string>();
            foreach (string file in files)
            {
                if (Regex.IsMatch(file, PNG_REGEX, RegexOptions.IgnoreCase))
                {
                    list.Add(file);
                }
                else if (file.EndsWith(ATLAS_SUFFIX))
                {
                    list.UnionWith(GetDependencies(new string[] { file }, PNG_REGEX));
                }
            }
            return list;
        }

        static HashSet<string> GetSelectionsBySamples(List<PathPattern> fileSamples, List<PathPattern> dirSamples)
        {
            List<PathPattern> samples = new List<PathPattern>();
            samples.AddRange(fileSamples);
            samples.AddRange(dirSamples);

            HashSet<string> hash = new HashSet<string>();

            for (int i = 0; i < samples.Count; i++)
            {
                PathPattern sample = samples[i];

                if (!Directory.Exists(sample.dir))
                {
                    DebugUtil.LogWarning("The directory isn't  exist : " + sample.dir);
                    continue;
                }

                string[] entries;
                if (i < fileSamples.Count)
                {
                    entries = FilePathTools.SelectEntries(FilePathTools.GetFiles(sample.dir, sample.regex, sample.searchOption), FILTER_META_REGEX);
                }
                else
                {
                    entries = FilePathTools.GetDirectories(sample.dir, sample.regex, sample.searchOption);
                }

                hash.UnionWith(entries);
                DebugUtil.Log("\"" + EditorCommonUtils.NormalizeDirectory(sample.dir) + sample.regex + "\" match " + (i < fileSamples.Count ? "files" : "directories") + " : " + entries.Length);
            }

            return hash;
        }

        static List<PathPattern> ReadConfig(string key)
        {
            if (!File.Exists(CONFIG_PATH))
            {
                File.Copy(DEFAULT_CONFIG_PATH, CONFIG_PATH);
                AssetDatabase.Refresh();
                throw new System.Exception(string.Format("当前没有配置文件，现已将默认配置文件拷贝到{0}，请根据具体项目修改配置后再进行操作!", CONFIG_PATH));
            }

            string text = File.ReadAllText(CONFIG_PATH);
            var data = JsonConvert.DeserializeObject<Dictionary<string, List<List<string>>>>(text)[key];

            List<PathPattern> list = new List<PathPattern>();
            foreach (var config in data)
            {
                list.Add(new PathPattern(ASSETS_ROOT + config[0], config[1], config[2].ToLower() == "true" ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
            }
            return list;
        }

        public static HashSet<string> GetAllAtlasFiles()
        {
            return GetSelectionsBySamples(ReadConfig("Atlas_White"), new List<PathPattern>());
        }

        public static HashSet<string> GetAtlasFiles()
        {
            HashSet<string> files = GetAllAtlasFiles();
            files.ExceptWith(GetSelectionsBySamples(ReadConfig("Atlas_Black"), new List<PathPattern>()));
            return files;
        }

        public static HashSet<string> GetAllRawTextureFiles()
        {
            return GetSelectionsBySamples(ReadConfig("RawTexture_White"), new List<PathPattern>());
        }

        public static HashSet<string> GetRawTextureFiles()
        {
            HashSet<string> files = GetAllRawTextureFiles();
            files.ExceptWith(GetSelectionsBySamples(ReadConfig("RawTexture_Black"), new List<PathPattern>()));
            return files;
        }

        public static HashSet<string> GetAllSpriteFiles()
        {
            HashSet<string> files = GetSelectionsBySamples(ReadConfig("Sprite_White"), new List<PathPattern>());
            files.ExceptWith(GetAllSpriteFilesInAtlas());
            return files;
        }

        public static HashSet<string> GetSpriteFiles()
        {
            HashSet<string> files = GetAllSpriteFiles();
            files.ExceptWith(GetSelectionsBySamples(ReadConfig("Sprite_Black"), new List<PathPattern>()));
            return files;
        }

        public static HashSet<string> GetAllPrefabFiles()
        {
            return GetSelectionsBySamples(ReadConfig("Prefab_White"), new List<PathPattern>());
        }

        public static HashSet<string> GetPrefabFiles()
        {
            HashSet<string> files = GetAllPrefabFiles();
            files.ExceptWith(GetSelectionsBySamples(ReadConfig("Prefab_Black"), new List<PathPattern>()));
            return files;
        }

        public static HashSet<string> GetDependencies(string[] pathNames, string regex)
        {
            string[] dependencies = AssetDatabase.GetDependencies(pathNames);
            dependencies = FilePathTools.SelectEntries(dependencies, regex);
            return new HashSet<string>(dependencies);
        }

        public static HashSet<string> GetAllSpriteFilesInAtlas()
        {
            return GetDependencies(new List<string>(GetAllAtlasFiles()).ToArray(), PNG_REGEX);
        }

        public static List<string> GetExternalDependencies(List<string> targets, List<string> filter = null, Dictionary<string, List<string>> dependencyDict = null, Dictionary<string, List<string>> referenceDict = null)
        {
            HashSet<string> targetSet = new HashSet<string>(targets);
            HashSet<string> filterSet = filter != null ? new HashSet<string>(filter) : new HashSet<string>();
            dependencyDict = dependencyDict != null ? dependencyDict : new Dictionary<string, List<string>>();
            referenceDict = referenceDict != null ? referenceDict : new Dictionary<string, List<string>>();

            HashSet<string> dependencySet = new HashSet<string>();

            foreach (string target in targetSet)
            {
                string[] dependencies = AssetDatabase.GetDependencies(new string[] { target });
                foreach (string dependency in dependencies)
                {
                    if (dependency == target || filterSet.Contains(dependency))
                    {
                        continue;
                    }

                    if (!dependencyDict.ContainsKey(target))
                    {
                        dependencyDict[target] = new List<string>();
                    }
                    dependencyDict[target].Add(dependency);

                    if (!referenceDict.ContainsKey(dependency))
                    {
                        referenceDict[dependency] = new List<string>();
                    }
                    referenceDict[dependency].Add(target);

                    dependencySet.Add(dependency);
                }
            }

            return new List<string>(dependencySet);
        }
    }
}