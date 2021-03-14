/*-------------------------------------------------------------------------------------------
// Copyright (C) 2019 北京，天龙互娱
//
// 模块名：EditorCommonUtils
// 创建日期：2020-4-9
// 创建者：guomeng.lu
// 模块描述：编辑器相关通用方法
//-------------------------------------------------------------------------------------------*/
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System;

namespace DragonU3DSDK
{
    public class EditorCommonUtils
    {
        public static bool IsPOT(float f, float p)
        {
            float l = Mathf.Log(f, p);
            return l.Equals(Mathf.RoundToInt(l));
        }

        public static float ToLargerPOT(float f, float p)
        {
            int l = Mathf.CeilToInt(Mathf.Log(f, p));
            return Mathf.Pow(p, l);
        }

        public static byte[] ReadBytes(string name)
        {
            try
            {
                return File.ReadAllBytes(name);
            }
            catch (Exception e)
            {
                DebugUtil.LogWarning("Read bytes failed : " + name + "\n" + e);
                return null;
            }
        }

        public static Texture2D ReadTexture(string name, TextureFormat format = TextureFormat.ARGB32)
        {
            Texture2D tex = null;
            byte[] bytes = ReadBytes(name);
            if (bytes != null)
            {
                tex = new Texture2D(0, 0, format, false);
                tex.LoadImage(bytes);
            }
            return tex;
        }

        public static bool WriteBytes(string name, byte[] bytes)
        {
            try
            {
                CreateDirectory(Path.GetDirectoryName(name));
                File.WriteAllBytes(name, bytes);
                return true;
            }
            catch (Exception e)
            {
                DebugUtil.LogWarning("Write bytes failed : " + name + "\n" + e);
                return false;
            }
        }

        public static bool CreateDirectory(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return true;
            }

            try
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return true;
            }
            catch (Exception e)
            {
                DebugUtil.LogWarning("Create directory failed : " + path + "\n" + e);
                return false;
            }
        }

        public static bool MoveFile(string srcPath, string destPath)
        {
            if (srcPath == destPath)
            {
                return true;
            }

            try
            {
                CreateDirectory(Path.GetDirectoryName(destPath));
                if (File.Exists(srcPath))
                {
                    File.Delete(destPath);
                    File.Move(srcPath, destPath);
                }
                return true;
            }
            catch (Exception e)
            {
                DebugUtil.LogWarning("Move file failed : " + srcPath + " --> " + destPath + "\n" + e);
                return false;
            }
        }

        public static string GetDirectoryName(string path)
        {
            return NormalizePath(Path.GetDirectoryName(path));
        }

        public static string GetFileName(string path)
        {
            return Path.GetFileName(path);
        }

        public static string NormalizePath(string path)
        {
            return path.Replace('\\', '/');
        }

        public static string NormalizeDirectory(string path)
        {
            path = NormalizePath(path);
            if (path != "" && !path.EndsWith("/"))
            {
                path += "/";
            }
            return path;
        }

        public static string GetExtension(string name)
        {
            string suffix = "";

            int index = name.LastIndexOf(".");
            if (index != -1)
            {
                suffix = name.Substring(index);
            }

            return suffix;
        }

        // 获取对象的属性
        public static Dictionary<string, object> GetValues(object o, Dictionary<string, object> dict, string prefix = "")
        {
            Type type = o.GetType();
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

            FieldInfo[] fieldInfos = type.GetFields(bindingFlags);
            for (int i = 0; i < fieldInfos.Length; i++)
            {
                FieldInfo fieldInfo = fieldInfos[i];
                string key = !string.IsNullOrEmpty(prefix) ? prefix + fieldInfo.Name : fieldInfo.Name;
                dict.Add(key, fieldInfo.GetValue(o));
            }

            PropertyInfo[] propertyInfos = type.GetProperties(bindingFlags);
            for (int i = 0; i < propertyInfos.Length; i++)
            {
                PropertyInfo propertyInfo = propertyInfos[i];
                string key = !string.IsNullOrEmpty(prefix) ? prefix + propertyInfo.Name : propertyInfo.Name;
                dict.Add(key, propertyInfo.GetValue(o, null));
            }

            return dict;
        }

        // 获取对象的属性
        public static Dictionary<string, object> GetValues(object o)
        {
            return GetValues(o, new Dictionary<string, object>());
        }
    }
}