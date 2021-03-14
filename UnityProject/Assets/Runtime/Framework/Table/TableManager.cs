using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace DragonU3DSDK.Config
{
    public class TableManager : Manager<TableManager>
    {
        private string _tableLocation = "";
        private string _testConfigPath = "";

        public void InitLocation(string path)
        {
            _tableLocation = path;
            _testConfigPath = Application.persistentDataPath + "/TestConfig";
        }

        public static List<T> DeSerialize<T>(string json)
        {
            if (!json.StartsWith("{", System.StringComparison.Ordinal))
            {
                json = "{\"v\":" + json + "}";
            }

            WrapperList<T> list = JsonUtility.FromJson<WrapperList<T>>(json);
            return list.v;
        }

        bool CheckDebugConfigDirExists()
        {
            return Directory.Exists(_testConfigPath);
        }

        [System.Serializable]
        private class WrapperList<T>
        {
            public List<T> v = new List<T>();
        }

        public List<T> GetTable<T>() 
        {
            TextAsset json = GetTextAsset(typeof(T));
            if(json == null){
                //Fixed NUll reference, 如果没有找到本地的version文件，需要拷贝，return true ，否则会报错卡死进度条
                return new List<T>();
            }
            List<T> records = DeSerialize<T>(json?.text);

            return records;
        }


        TextAsset GetTextAsset(System.Type type)
        {
            string className = type.ToString().ToLower().Replace("table", "");
            if(Debug.isDebugBuild && CheckDebugConfigDirExists())
            {//测试修改table的功能
                TextAsset json = new TextAsset(File.ReadAllText(_testConfigPath + "/" + className + ".json"));
                return json;
            }
            else
            {//使用正常配置
                TextAsset json = Asset.ResourcesManager.Instance.LoadResource<TextAsset>(Path.Combine(_tableLocation,className));
                return json;
            }
        }

        public T GetTableByID<T>(int index) where T : TableBase
        {
            List<T> records = GetTable<T>();
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].GetID() == index)
                {
                    return records[i];
                }
            }

            //DebugUtil.LogError("Get Table Error! Type is {0}, index is {1}", typeof(T), index);
            return null;
        }
    }

}
