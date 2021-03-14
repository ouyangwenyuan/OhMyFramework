using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class AssetBundleNode
{
    public string AssetBundleName;
    public string AssetBundlePath;
    public bool InInitialPacket; //是否为本地zi y
}

public class AsetBundleConfigController : ScriptableObject
{
    public static string AtlasConfigPath = "Settings/AsetBundleConfigController";
    private static AsetBundleConfigController _instance = null;

    public static AsetBundleConfigController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<AsetBundleConfigController>(AtlasConfigPath);
            }
            return _instance;
        }
    }

    [Space(10)]
    [Header("[相对Export的路径，使用菜单'AssetBundle/SpriteAtlas/生成AtlasConfig'自动生成]")]
    [Header(" ---------------------- 图集路径 -----------------------")]
    public List<AssetBundleNode> AtlasPathNodeList = new List<AssetBundleNode>();

    public void ParseAtlasPath()
    {
        string spriteAtlasRootPath = Application.dataPath + "/Export";
        AtlasPathNodeList.Clear();
        SearchDirectory(spriteAtlasRootPath);
    }

//    public AtlasPathNode GetAtlasPath(string atlasName)
//    {
//        if (AtlasPathNodeList == null) return null;
//        if (atlasName.Contains("/"))                //atlasName中不允许出现路径
//        {
//            var nameArray = atlasName.Split('/');
//            atlasName = nameArray[nameArray.Length - 1];
//        }
//        return AtlasPathNodeList.Find((a) => { return a.AtlasName == atlasName; });
//    }

    private void GetFileName(string path)
    {
        DirectoryInfo root = new DirectoryInfo(path);
        foreach (FileInfo f in root.GetFiles("*.spriteatlas"))
        {
            Debug.Log(f.FullName);
            AddAtlasPathNode(f.Name, f.FullName);
        }
    }

    private void SearchDirectory(string path)
    {
//        GetFileName(path);
        DirectoryInfo root = new DirectoryInfo(path);
        foreach (DirectoryInfo d in root.GetDirectories())
        {
            string _atlasRelativePath = d.FullName.Split(new string[] { "Export/" }, StringSplitOptions.RemoveEmptyEntries)[1];
            if (d.GetDirectories().Length == 0)
            {
                AssetBundleNode node = new AssetBundleNode
                {
                    AssetBundleName = d.Name,
                    AssetBundlePath = _atlasRelativePath
                };
                AtlasPathNodeList.Add(node);
            }
            else
            {
                SearchDirectory(d.FullName);
            }
        }
    }

    private void AddAtlasPathNode(string atlasName, string fullPath)
    {
//        if (AtlasPathNodeList == null) AtlasPathNodeList = new List<AtlasPathNode>();
//
//        string _atlasName = atlasName.Substring(0, atlasName.Length - 12);// 删除".spriteatlas";
//        string _atlasRelativePath = fullPath.Split(new string[] { "Export/" }, StringSplitOptions.RemoveEmptyEntries)[1];
//
//        AtlasPathNode _AtlasPathNode = GetAtlasPath(_atlasName);
//        if (_AtlasPathNode == null)
//        {
//            _AtlasPathNode = new AtlasPathNode { AtlasName = _atlasName };
//            RefreshAtlasPath(_AtlasPathNode, _atlasRelativePath);
//            AtlasPathNodeList.Add(_AtlasPathNode);
//        }
//        else
//        {
//            RefreshAtlasPath(_AtlasPathNode, _atlasRelativePath);
//        }
    }

    private void RefreshAtlasPath(AtlasPathNode atlasPathNode, string relativePath)
    {
        string relativePathWithoutExt = relativePath.Substring(0, relativePath.Length - 12);// 删除".spriteatlas";

        if (relativePathWithoutExt.Contains("/Sd/"))
            atlasPathNode.SdPath = relativePathWithoutExt;
        else if (relativePathWithoutExt.Contains("/Hd/"))
            atlasPathNode.HdPath = relativePathWithoutExt;
        else{
            atlasPathNode.HdPath = relativePathWithoutExt;
            atlasPathNode.SdPath = relativePathWithoutExt;
        }
    }
}
