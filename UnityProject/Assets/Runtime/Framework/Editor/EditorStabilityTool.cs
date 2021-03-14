/*-------------------------------------------------------------------------------------------
// Copyright (C) 2020 成都，天龙互娱
//
// 模块名：EditorStabilityTool
// 创建日期：2020-9-10
// 创建者：qibo.li
// 模块描述：稳定性工具
//-------------------------------------------------------------------------------------------*/

using System.IO;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEditor;

namespace DragonU3DSDK
{
    public class EditorStabilityTool
    {
        [MenuItem("Assets/稳定性/导出额外的字体依赖")]
        static void DumpExtraFontDeps()
        {
            //预制件资源只能依赖英文字体，导出依赖了其它字体的预制件
            Dictionary<string, HashSet<string>> errors = new Dictionary<string, HashSet<string>>();
            Object cur = Selection.activeObject;
            string path = AssetDatabase.GetAssetPath(cur);
            if (!Directory.Exists(path)) DebugUtil.LogError("请选择文件夹！！！");
            string[] prefabs = AssetDatabase.FindAssets("t:Prefab", new[] { path });
            foreach (var p in prefabs)
            {
                string[] deps = AssetDatabase.GetDependencies(AssetDatabase.GUIDToAssetPath(p));
                foreach (var d in deps)
                {
                    Object obj = AssetDatabase.LoadMainAssetAtPath(d);
                    if (obj.GetType() == typeof(TMPro.TMP_FontAsset) && 
                        !obj.name.ToLower().Equals("LocaleFont_En SDF".ToLower()) && !obj.name.ToLower().Equals("LiberationSans SDF".ToLower()))//有其他跟友好的识别是否是英文？
                    {
                        HashSet<string> sets;
                        if (!errors.TryGetValue(d, out sets))
                        {
                            sets = new HashSet<string>();
                            errors.Add(d, sets);
                        }
                        sets.Add(p);
                    }
                }
            }

            if (errors.Count == 0)
            {
                DebugUtil.Log("空");
            }
            else
            {
                foreach (var p in errors)
                {
                    DebugUtil.LogWarning(string.Format("字体：{0}", p.Key));
                    foreach (var d in p.Value)
                    {
                        DebugUtil.Log(AssetDatabase.GUIDToAssetPath(d));
                    }
                }
            }
        }

        [MenuItem("Assets/稳定性/强制使用英文字体")]
        static void ForceUseEnFont()
        {
            Object cur = Selection.activeObject;
            string path = AssetDatabase.GetAssetPath(cur);
            if (!Directory.Exists(path)) DebugUtil.LogError("请选择文件夹！！！");
            string[] prefabs = AssetDatabase.FindAssets("t:Prefab", new[] { path });
            TMP_FontAsset font_en = null;
            foreach (var p in prefabs)
            {
                bool valid = true;
                string[] deps = AssetDatabase.GetDependencies(AssetDatabase.GUIDToAssetPath(p));
                foreach (var d in deps)
                {
                    Object obj = AssetDatabase.LoadMainAssetAtPath(d);
                    if (obj.GetType() == typeof(TMPro.TMP_FontAsset))
                    {
                        if (!obj.name.ToLower().Equals("LocaleFont_En SDF".ToLower()) && !obj.name.ToLower().Equals("LiberationSans SDF".ToLower()))
                        {
                            valid = false;

                            if (null != font_en)
                            {
                                break;
                            }
                        }
                        else if(null == font_en)
                        {
                            if (obj.name.ToLower().Equals("LocaleFont_En SDF".ToLower()))
                            {
                                font_en = obj as TMP_FontAsset;
                            }
                        }
                    }
                }

                if (!valid)
                {
                    GameObject gobj = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(p)) as GameObject;
                    TextMeshProUGUI[] comps = gobj.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var c in comps)
                    {
                        if (c.font != font_en)
                        {
                            c.font = font_en;
                        }
                    }

                    EditorUtility.SetDirty(gobj);
                }
            }
            AssetDatabase.SaveAssets();
        }
    }
}