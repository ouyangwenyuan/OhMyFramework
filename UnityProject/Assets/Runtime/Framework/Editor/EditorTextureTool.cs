/*-------------------------------------------------------------------------------------------
// Copyright (C) 2019 北京，天龙互娱
//
// 模块名：EditorTextureTool
// 创建日期：2020-4-9
// 创建者：guomeng.lu
// 模块描述：编辑器图片处理
//-------------------------------------------------------------------------------------------*/
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;
using DragonU3DSDK.Asset;

namespace DragonU3DSDK
{
    public class EditorTextureTool
    {
#if UNITY_2019_1_OR_NEWER
        const TextureImporterFormat IOS_NEW_RGBA = TextureImporterFormat.ASTC_4x4;
        const TextureImporterFormat IOS_NEW_RGB = TextureImporterFormat.ASTC_4x4;
#else
        const TextureImporterFormat IOS_NEW_RGBA = TextureImporterFormat.ASTC_RGBA_4x4;
        const TextureImporterFormat IOS_NEW_RGB = TextureImporterFormat.ASTC_RGB_4x4;
#endif

        const int MAX_TEXTURE_SIZE = 2048;
        const int COMPRESSION_QUALITY = 100;
        const int ATLAS_TYPE_NUMBER = 2;
        const string LOW_ATLAS_SUFFIX = ".SD.spriteatlas";
        static string[] PLATFORMS = { "Default", "Standalone", "Android", "iPhone" };

        [MenuItem("TextureTool/FindReferences (Select)")]
        static void FindReferencesBySelect()
        {
            var dependenciesDict = new Dictionary<string, HashSet<string>>();
            foreach (var prefab in EditorResourcePaths.GetAllPrefabFiles())
            {
                dependenciesDict[prefab] = new HashSet<string>(AssetDatabase.GetDependencies(prefab, false));
            }

            HashSet<string> fileSet = new HashSet<string>(EditorResourcePaths.GetFileSelections());
            foreach (string file in fileSet)
            {
                foreach (string prefab in dependenciesDict.Keys)
                {
                    if (!fileSet.Contains(prefab) && dependenciesDict[prefab].Contains(file))
                    {
                        DebugUtil.LogWarning(file + " References By " + prefab);
                    }
                }
            }
        }

        [MenuItem("TextureTool/FindExternalDependencies (Select)")]
        static void FindExternalDependenciesBySelect()
        {
            List<string> selections = EditorResourcePaths.GetAllSelections(SelectionMode.DeepAssets);
            Dictionary<string, List<string>> referenceDict = new Dictionary<string, List<string>>();
            EditorResourcePaths.GetExternalDependencies(selections, selections, null, referenceDict);
            foreach (string file in referenceDict.Keys)
            {
                Debug.LogWarning("Depend On " + file);
                foreach (string reference in referenceDict[file])
                {
                    Debug.Log(" In " + reference);
                }
            }
        }

        [MenuItem("TextureTool/CheckPrefab (Select)")]
        static void CheckPrefabBySelect()
        {
            var atlasSprites = EditorResourcePaths.GetAllSpriteFilesInAtlas();
            foreach (string file in EditorResourcePaths.GetFileSelections())
            {
                if (file.EndsWith(".prefab"))
                {
                    var sprites = EditorResourcePaths.GetDependencies(new string[] { file }, EditorResourcePaths.PNG_REGEX);
                    foreach (var sprite in sprites)
                    {
                        if (!atlasSprites.Contains(sprite))
                        {
                            DebugUtil.LogWarning(file + " 包含散图 " + sprite);
                        }
                    }
                }
            }
        }

        [MenuItem("TextureTool/CheckConflict (Select)")]
        static void CheckConflictBySelect()
        {
            foreach (string file in EditorResourcePaths.GetFileSelections())
            {
                string text = System.IO.File.ReadAllText(file);
                if (text.IndexOf(">>>>>>>") != -1)
                {
                    DebugUtil.LogWarning(file + " is Conflict");
                }
            }
        }

        [MenuItem("TextureTool/CheckAlpha (Select)")]
        static void CheckAlphaBySelect()
        {
            foreach (string file in EditorResourcePaths.GetTextureFiles(EditorResourcePaths.GetFileSelections()))
            {
                Texture2D texture = EditorCommonUtils.ReadTexture(file);
                if (ContainAlpha(texture))
                {
                    DebugUtil.LogWarning("该图片包含透明通道: " + file);
                }
            }
        }

        [MenuItem("TextureTool/FilterAlpha (Select)")]
        static void FilterAlphaBySelect()
        {
            foreach (string file in EditorResourcePaths.GetTextureFiles(EditorResourcePaths.GetFileSelections()))
            {
                FilterTextureAlpha(file);
            }
            AssetDatabase.Refresh();
        }

        [MenuItem("TextureTool/ToBytes (Select)")]
        static void ToBytesBySelect()
        {
            foreach (string file in EditorResourcePaths.GetFileSelections())
            {
                string newFile = file.Replace(EditorCommonUtils.GetExtension(file), ".bytes");
                EditorCommonUtils.MoveFile(file, newFile);
            }
        }

        [MenuItem("TextureTool/ShowAtlas (Select) &s")]
        static void ShowAtlasBySelect()
        {
            Object[] selections = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
            System.Array.Sort(selections, (a, b) => { return string.Compare(a.name, b.name); });
            foreach (Object selection in selections)
            {
                if (selection.GetType() == typeof(SpriteAtlas))
                {
                    Selection.activeObject = selection;
                    break;
                }
            }
        }

        [MenuItem("TextureTool/CheckETC2")]
        public static void CheckETC2()
        {
            foreach (string file in EditorResourcePaths.GetSpriteFiles())
            {
                Texture2D texture = EditorCommonUtils.ReadTexture(file);
                if (texture.width % 4 != 0 || texture.height % 4 != 0)
                {
                    DebugUtil.LogWarning("该图片不满足Android的压缩格式 : " + file);
                }
            }
            DebugUtil.Log("CheckETC2 Done!");
        }

        [MenuItem("TextureTool/FindRGBA")]
        public static void FindRGBA()
        {
            TextureImporterFormat[] formatConfig = new TextureImporterFormat[]
            {
                TextureImporterFormat.RGBA32,
                TextureImporterFormat.RGB24,
                TextureImporterFormat.ARGB32,
            };

            HashSet<TextureImporterFormat> filter = new HashSet<TextureImporterFormat>(formatConfig);
            foreach (string atlasFile in EditorResourcePaths.GetAllAtlasFiles())
            {
                if (atlasFile.EndsWith(LOW_ATLAS_SUFFIX))
                {
                    continue;
                }

                SpriteAtlas spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasFile);
                for (int i = 2; i < PLATFORMS.Length; i++)
                {
                    TextureImporterPlatformSettings patformSettings = SpriteAtlasExtensions.GetPlatformSettings(spriteAtlas, PLATFORMS[i]);
                    if (patformSettings.overridden && filter.Contains(patformSettings.format))
                    {
                        DebugUtil.LogWarning("高清图集 : " + atlasFile);
                        break;
                    }
                }
            }

            DebugUtil.Log("FindRGBA Done!");
        }

        [MenuItem("TextureTool/CheckMapDependencies")]
        static void CheckMapDependencies()
        {
            string mapRoot = "Assets/Export/UI/";
            foreach (string path in FilePathTools.GetDirectories(mapRoot, "Map"))
            {
                string name = path.Replace(mapRoot, "") + "/";
                string[] files = FilePathTools.GetFiles(path, ".*", System.IO.SearchOption.AllDirectories);
                Dictionary<string, List<string>> dependencyDict = new Dictionary<string, List<string>>();
                List<string> selections = new List<string>(files);
                EditorResourcePaths.GetExternalDependencies(selections, selections, dependencyDict);
                foreach (string selection in dependencyDict.Keys)
                {
                    dependencyDict[selection].ForEach((param) =>
                    {
                        if ((param.IndexOf("Res/Maps/") != -1 || param.IndexOf("Export/UI/Map") != -1) && param.IndexOf(name) == -1 && param.IndexOf("MapCom") == -1)
                        {
                            Debug.LogWarning(selection + "  Depend on : " + param);
                        }
                    });
                }
            }
        }

        [MenuItem("TextureTool/SetTextureFormat")]
        public static void SetTextureFormat()
        {
            HandleTextureFormat(EditorResourcePaths.GetSpriteFiles(), SetResourcesFormat);
            HandleTextureFormat(EditorResourcePaths.GetRawTextureFiles(), SetRawTextureFormat);
            HandleSpriteAtlasFormat(EditorResourcePaths.GetAtlasFiles());
            DebugUtil.Log("SetTextureFormat Done!");
        }

        [MenuItem("TextureTool/SetAtlasMaxSize")]
        public static void SetAtlasMaxSize()
        {
            MethodInfo getPreviewTexturesMI = typeof(SpriteAtlasExtensions).GetMethod("GetPreviewTextures", BindingFlags.Static | BindingFlags.NonPublic);
            foreach (string atlasFile in EditorResourcePaths.GetAllAtlasFiles())
            {
                if (atlasFile.EndsWith(LOW_ATLAS_SUFFIX))
                {
                    continue;
                }

                SpriteAtlas spriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasFile);
                List<Sprite> sprites = GetSpritesByAtlas(spriteAtlas);

                // 统计散图面积之和
                int total = 0;
                int minSize = 128;
                foreach (Sprite sprite in sprites)
                {
                    Rect rect = sprite.textureRect;
                    int width = Mathf.CeilToInt(rect.width);
                    int height = Mathf.CeilToInt(rect.height);
                    minSize = Mathf.Max(minSize, width, height);
                    total += width * height;
                }

                // 估算maxTextureSize值
                float guessArea = EditorCommonUtils.ToLargerPOT(total, 4);
                int guessSize = Mathf.RoundToInt(Mathf.Sqrt(guessArea));

                // 如果填充率太低，将估值尺寸减半
                if (total <= guessArea * 0.625f)
                {
                    guessSize /= 2;
                }

                // 用估值尺寸尝试打图集
                guessSize = Mathf.Max(guessSize, Mathf.RoundToInt(EditorCommonUtils.ToLargerPOT(minSize, 2)));
                foreach (string platform in PLATFORMS)
                {
                    TextureImporterPlatformSettings patformSettings = SpriteAtlasExtensions.GetPlatformSettings(spriteAtlas, platform);
                    if (patformSettings.maxTextureSize != guessSize)
                    {
                        patformSettings.maxTextureSize = guessSize;
                        SpriteAtlasExtensions.SetPlatformSettings(spriteAtlas, patformSettings);
                    }
                }
                SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { spriteAtlas }, EditorUserBuildSettings.activeBuildTarget);

                // 计算图集的实际面积
                Texture2D[] atlasTextures = (Texture2D[])getPreviewTexturesMI.Invoke(null, new object[] { spriteAtlas });
                int area = 0;
                for (int i = 0; i < atlasTextures.Length; i++)
                {
                    area += atlasTextures[i].width * atlasTextures[i].height;
                }

                // 如果估值尺寸效果一般，将其翻倍
                if (area > guessSize * guessSize * 2.5f)
                {
                    foreach (string platform in PLATFORMS)
                    {
                        TextureImporterPlatformSettings patformSettings = SpriteAtlasExtensions.GetPlatformSettings(spriteAtlas, platform);
                        patformSettings.maxTextureSize = Mathf.Min(guessSize * 2, MAX_TEXTURE_SIZE);
                        SpriteAtlasExtensions.SetPlatformSettings(spriteAtlas, patformSettings);
                    }
                    SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { spriteAtlas }, EditorUserBuildSettings.activeBuildTarget);
                }
                else if (area > guessSize * guessSize * 1.5f && area <= guessSize * guessSize * 2)
                {
                    TextureImporterPlatformSettings patformSettings = SpriteAtlasExtensions.GetPlatformSettings(spriteAtlas, "Android");
                    patformSettings.maxTextureSize = Mathf.Min(guessSize * 2, MAX_TEXTURE_SIZE);
                    SpriteAtlasExtensions.SetPlatformSettings(spriteAtlas, patformSettings);
                    SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { spriteAtlas }, EditorUserBuildSettings.activeBuildTarget);
                }

                SpriteAtlas lowSpriteAtlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasFile.Replace(EditorResourcePaths.ATLAS_SUFFIX, LOW_ATLAS_SUFFIX));
                foreach (string platform in PLATFORMS)
                {
                    TextureImporterPlatformSettings patformSettings = SpriteAtlasExtensions.GetPlatformSettings(spriteAtlas, platform);
                    TextureImporterPlatformSettings lowPatformSettings = SpriteAtlasExtensions.GetPlatformSettings(lowSpriteAtlas, platform);
                    lowPatformSettings.maxTextureSize = patformSettings.maxTextureSize;
                    SpriteAtlasExtensions.SetPlatformSettings(lowSpriteAtlas, lowPatformSettings);
                }
            }
            DebugUtil.Log("SetAtlasMaxSize Done!");
        }

        static Dictionary<string, object> GetTextureImporterValues(TextureImporter importer)
        {
            var dict = EditorCommonUtils.GetValues(importer);
            EditorCommonUtils.GetValues(importer.GetDefaultPlatformTextureSettings(), dict, "Default");
            EditorCommonUtils.GetValues(importer.GetPlatformTextureSettings("Standalone"), dict, "Standalone");
            EditorCommonUtils.GetValues(importer.GetPlatformTextureSettings("Android"), dict, "Android");
            EditorCommonUtils.GetValues(importer.GetPlatformTextureSettings("iPhone"), dict, "iPhone");
            return dict;
        }

        static void HandleTextureFormat(HashSet<string> files, System.Action<TextureImporter> setDelegate)
        {
            foreach (string file in files)
            {
                TextureImporter importer = AssetImporter.GetAtPath(file) as TextureImporter;
                var originalDict = GetTextureImporterValues(importer);
                setDelegate(importer);
                var newDict = GetTextureImporterValues(importer);

                foreach (string key in originalDict.Keys)
                {
                    if (originalDict[key].ToString() != newDict[key].ToString())
                    {
                        importer.SaveAndReimport();
                        break;
                    }
                }
            }
            AssetDatabase.Refresh();
        }

        static Dictionary<string, object> GetSpriteAtlasSettingsValues(SpriteAtlasTextureSettings[] textureSettingsArray, Dictionary<string, TextureImporterPlatformSettings>[] platformSettingsDictArray)
        {
            var dict = new Dictionary<string, object>();
            for (int i = 0; i < ATLAS_TYPE_NUMBER; i++)
            {
                EditorCommonUtils.GetValues(textureSettingsArray[i], dict, "textureSettings" + i);
                foreach (string platform in PLATFORMS)
                {
                    EditorCommonUtils.GetValues(platformSettingsDictArray[i][platform], dict, platform + i);
                }
            }
            return dict;
        }

        static void HandleSpriteAtlasFormat(HashSet<string> atlasFiles)
        {
            string[] atlasFileArray = new string[ATLAS_TYPE_NUMBER];
            SpriteAtlas[] spriteAtlasArray = new SpriteAtlas[ATLAS_TYPE_NUMBER];
            SpriteAtlasPackingSettings[] packingSettingsArray = new SpriteAtlasPackingSettings[ATLAS_TYPE_NUMBER];
            SpriteAtlasTextureSettings[] textureSettingsArray = new SpriteAtlasTextureSettings[ATLAS_TYPE_NUMBER];
            Dictionary<string, TextureImporterPlatformSettings>[] platformSettingsDictArray = new Dictionary<string, TextureImporterPlatformSettings>[ATLAS_TYPE_NUMBER];
            platformSettingsDictArray[0] = new Dictionary<string, TextureImporterPlatformSettings>();
            platformSettingsDictArray[1] = new Dictionary<string, TextureImporterPlatformSettings>();

            foreach (string file in atlasFiles)
            {
                if (file.EndsWith(LOW_ATLAS_SUFFIX))
                {
                    continue;
                }

                atlasFileArray[0] = file;
                atlasFileArray[1] = file.Replace(EditorResourcePaths.ATLAS_SUFFIX, LOW_ATLAS_SUFFIX);

                for (int i = 0; i < ATLAS_TYPE_NUMBER; i++)
                {
                    spriteAtlasArray[i] = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasFileArray[i]);
                    packingSettingsArray[i] = SpriteAtlasExtensions.GetPackingSettings(spriteAtlasArray[i]);
                    textureSettingsArray[i] = SpriteAtlasExtensions.GetTextureSettings(spriteAtlasArray[i]);
                    foreach (string platform in PLATFORMS)
                    {
                        platformSettingsDictArray[i][platform] = SpriteAtlasExtensions.GetPlatformSettings(spriteAtlasArray[i], platform);
                    }
                }

                HashSet<string> spriteSet = EditorResourcePaths.GetDependencies(atlasFileArray, EditorResourcePaths.PNG_REGEX);
                bool containAlpha = false;
                foreach (string spriteFile in spriteSet)
                {
                    Texture2D texture = EditorCommonUtils.ReadTexture(spriteFile);
                    if (ContainAlpha(texture))
                    {
                        containAlpha = true;
                        break;
                    }
                }

                for (int i = 0; i < ATLAS_TYPE_NUMBER; i++)
                {
                    packingSettingsArray[i].enableRotation = false;
                    packingSettingsArray[i].enableTightPacking = false;
                    SetDefaultTextureSettings(ref textureSettingsArray[i]);
                }

                if (containAlpha)
                {
                    SetPlatformTextureSettings(platformSettingsDictArray[0]["Default"], "Default", TextureImporterFormat.RGBA32);
                    SetPlatformTextureSettings(platformSettingsDictArray[0]["Standalone"], "Standalone", TextureImporterFormat.DXT5);
                    SetPlatformTextureSettings(platformSettingsDictArray[0]["Android"], "Android", TextureImporterFormat.ETC2_RGBA8);
                    SetPlatformTextureSettings(platformSettingsDictArray[0]["iPhone"], "iPhone", IOS_NEW_RGBA);

                    SetPlatformTextureSettings(platformSettingsDictArray[1]["Default"], "Default", TextureImporterFormat.RGBA32);
                    SetPlatformTextureSettings(platformSettingsDictArray[1]["Standalone"], "Standalone", TextureImporterFormat.DXT5Crunched);
                    SetPlatformTextureSettings(platformSettingsDictArray[1]["Android"], "Android", TextureImporterFormat.ETC2_RGBA8Crunched);
                    SetPlatformTextureSettings(platformSettingsDictArray[1]["iPhone"], "iPhone", TextureImporterFormat.PVRTC_RGBA4);
                }
                else
                {
                    SetPlatformTextureSettings(platformSettingsDictArray[0]["Default"], "Default", TextureImporterFormat.RGB24);
                    SetPlatformTextureSettings(platformSettingsDictArray[0]["Standalone"], "Standalone", TextureImporterFormat.DXT1);
                    SetPlatformTextureSettings(platformSettingsDictArray[0]["Android"], "Android", TextureImporterFormat.ETC2_RGB4);
                    SetPlatformTextureSettings(platformSettingsDictArray[0]["iPhone"], "iPhone", TextureImporterFormat.PVRTC_RGB4);

                    SetPlatformTextureSettings(platformSettingsDictArray[1]["Default"], "Default", TextureImporterFormat.RGB24);
                    SetPlatformTextureSettings(platformSettingsDictArray[1]["Standalone"], "Standalone", TextureImporterFormat.DXT1Crunched);
                    SetPlatformTextureSettings(platformSettingsDictArray[1]["Android"], "Android", TextureImporterFormat.ETC_RGB4Crunched);
                    SetPlatformTextureSettings(platformSettingsDictArray[1]["iPhone"], "iPhone", TextureImporterFormat.PVRTC_RGB4);
                }

                for (int i = 0; i < ATLAS_TYPE_NUMBER; i++)
                {
                    SpriteAtlasExtensions.SetIncludeInBuild(spriteAtlasArray[i], false);
                    SpriteAtlasExtensions.SetPackingSettings(spriteAtlasArray[i], packingSettingsArray[i]);
                    SpriteAtlasExtensions.SetTextureSettings(spriteAtlasArray[i], textureSettingsArray[i]);
                    foreach (string platform in PLATFORMS)
                    {
                        SpriteAtlasExtensions.SetPlatformSettings(spriteAtlasArray[i], platformSettingsDictArray[i][platform]);
                    }
                }
                SpriteAtlasExtensions.SetIsVariant(spriteAtlasArray[1], false);
                SpriteAtlasExtensions.Remove(spriteAtlasArray[1], SpriteAtlasExtensions.GetPackables(spriteAtlasArray[1]));
                SpriteAtlasExtensions.Add(spriteAtlasArray[1], SpriteAtlasExtensions.GetPackables(spriteAtlasArray[0]));
            }
            AssetDatabase.Refresh();
        }

        static void SetDefaultFormat(TextureImporter importer)
        {
            importer.textureType = TextureImporterType.Default;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.sRGBTexture = true;
            importer.isReadable = false;
            importer.mipmapEnabled = false;
            importer.streamingMipmaps = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = MAX_TEXTURE_SIZE;
            importer.anisoLevel = 0;
            importer.spritePackingTag = "";
            importer.allowAlphaSplitting = false;
            importer.compressionQuality = COMPRESSION_QUALITY;
        }

        static void SetSpriteFormat(TextureImporter importer)
        {
            SetDefaultFormat(importer);
            importer.textureType = TextureImporterType.Sprite;
        }

        static void SetPlatformTextureSettings(TextureImporterPlatformSettings patformSettings, string platform, TextureImporterFormat textureFormat, bool allowsAlphaSplit = false)
        {
            patformSettings.overridden = true;
            patformSettings.name = platform;
            patformSettings.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
            patformSettings.format = textureFormat;
            patformSettings.compressionQuality = COMPRESSION_QUALITY;
            patformSettings.allowsAlphaSplitting = allowsAlphaSplit;
            patformSettings.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;
        }

        static void SetPlatformTextureImporterSettings(TextureImporter importer, string platform, TextureImporterFormat textureFormat, bool allowsAlphaSplit = false)
        {
            TextureImporterPlatformSettings settings = new TextureImporterPlatformSettings();
            SetPlatformTextureSettings(settings, platform, textureFormat, allowsAlphaSplit);
            settings.maxTextureSize = MAX_TEXTURE_SIZE;
            importer.SetPlatformTextureSettings(settings);
        }

        static void SetTextureImporterFormat(TextureImporter importer)
        {
            Texture2D texture = EditorCommonUtils.ReadTexture(importer.assetPath);

            if (ContainAlpha(texture))
            {
                SetPlatformTextureImporterSettings(importer, "Default", TextureImporterFormat.RGBA32);
                SetPlatformTextureImporterSettings(importer, "Standalone", TextureImporterFormat.DXT5);
                SetPlatformTextureImporterSettings(importer, "Android", TextureImporterFormat.ETC2_RGBA8);
                SetPlatformTextureImporterSettings(importer, "iPhone", IOS_NEW_RGBA);
            }
            else
            {
                SetPlatformTextureImporterSettings(importer, "Default", TextureImporterFormat.RGB24);
                SetPlatformTextureImporterSettings(importer, "Standalone", TextureImporterFormat.DXT1);
                SetPlatformTextureImporterSettings(importer, "Android", TextureImporterFormat.ETC2_RGB4);

                bool pvrtc;
                if (importer.npotScale == TextureImporterNPOTScale.None)
                {
                    pvrtc = texture.width == texture.height && EditorCommonUtils.IsPOT(texture.width, 2) && EditorCommonUtils.IsPOT(texture.height, 2);
                }
                else
                {
                    Texture2D temp = AssetDatabase.LoadAssetAtPath<Texture2D>(importer.assetPath);
                    pvrtc = temp.width == temp.height;
                }

                if (pvrtc)
                {
                    SetPlatformTextureImporterSettings(importer, "iPhone", TextureImporterFormat.PVRTC_RGB4);
                }
                else
                {
                    SetPlatformTextureImporterSettings(importer, "iPhone", IOS_NEW_RGB);
                }
            }
        }

        static void SetResourcesFormat(TextureImporter importer)
        {
            SetSpriteFormat(importer);
            SetTextureImporterFormat(importer);
        }

        static void SetRawTextureFormat(TextureImporter importer)
        {
            SetDefaultFormat(importer);
            SetTextureImporterFormat(importer);
        }

        static void SetDefaultTextureSettings(ref SpriteAtlasTextureSettings textureSettings)
        {
            textureSettings.anisoLevel = 0;
            textureSettings.filterMode = FilterMode.Bilinear;
            textureSettings.generateMipMaps = false;
            textureSettings.readable = false;
            textureSettings.sRGB = true;
        }

        static bool ContainAlpha(Texture2D readableTexture)
        {
            for (int i = 0; i < readableTexture.width; i++)
            {
                for (int j = 0; j < readableTexture.height; j++)
                {
                    Color color = readableTexture.GetPixel(i, j);
                    if (!color.a.Equals(1))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static void FilterTextureAlpha(string path)
        {
            Texture2D texture = EditorCommonUtils.ReadTexture(path);
            for (int i = 0; i < texture.width; i++)
            {
                for (int j = 0; j < texture.height; j++)
                {
                    Color color = texture.GetPixel(i, j);
                    color.a = 1;
                    texture.SetPixel(i, j, color);
                }
            }
            EditorCommonUtils.WriteBytes(path, texture.EncodeToPNG());
        }

        public static List<Sprite> GetSpritesByFile(string path)
        {
            List<Sprite> sprites = new List<Sprite>();
            foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                Sprite sprite = o as Sprite;
                if (sprite)
                {
                    sprites.Add(sprite);
                }
            }
            return sprites;
        }

        public static List<Sprite> GetSpritesByAtlas(SpriteAtlas spriteAtlas)
        {
            List<Sprite> sprites = new List<Sprite>();
            foreach (Object asset in SpriteAtlasExtensions.GetPackables(spriteAtlas))
            {
                string path = AssetDatabase.GetAssetOrScenePath(asset);
                if (System.IO.Directory.Exists(path))
                {
                    foreach (string filePath in System.IO.Directory.GetFiles(path, "*", System.IO.SearchOption.AllDirectories))
                    {
                        sprites.AddRange(GetSpritesByFile(filePath));
                    }
                }
                else
                {
                    sprites.AddRange(GetSpritesByFile(path));
                }
            }
            return sprites;
        }
    }
}