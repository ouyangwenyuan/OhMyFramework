using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using Newtonsoft.Json;
using System.Diagnostics;
using System;
using UnityEngine.U2D;
using UnityEditor.U2D;
using BestHTTP;
using System.Net;

namespace DragonU3DSDK.Asset
{
    public static partial class BuildAssetBundles
    {
        static void GenerateHomePlaceholderFile()
        {
            if (HomeRoomConfigController.Instance != null)
            {
                CommonResUtils.AddPlaceholderFile(Application.dataPath + "/Export/DummyMaterial");
                foreach (var baseInfo in HomeRoomConfigController.Instance.RoomFolderInfos)
                {
                    foreach (string childFolder in HomeRoomConfigController.Instance.RoomChildFolders)
                    {
                        CommonResUtils.AddPlaceholderFile(Application.dataPath + "/Export/" + baseInfo.Path + "/" + childFolder);
                    }
                }
                AssetDatabase.Refresh();
            }
        }
    }
}