using UnityEngine;

namespace DragonU3DSDK.Asset
{
    public static partial class BuildAssetBundles
    {
        static void GenerateCookingGamePlaceholderFile()
        {
            if (CookingGameConfigController.Instance != null)
            {
                foreach (var mapInfo in CookingGameConfigController.Instance.MapInfos)
                {
                    foreach (var path in CookingGameConfigController.Instance.FolderPaths)
                    {
                        var localPath = path.LocalPath.Replace("*", mapInfo.LocalId.ToString());
                        CommonResUtils.AddPlaceholderFile(Application.dataPath + "/Export/" + localPath);
                    }
                }
            }
        }
    }
}