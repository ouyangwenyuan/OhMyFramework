using UnityEngine;

namespace DragonU3DSDK.Asset
{
    public static partial class BuildAssetBundles
    {
        static void GenerateColorPlaceholderFile()
        {
            if (ColorResConfigController.Instance != null)
            {
                foreach (var baseInfo in ColorResConfigController.Instance.ColorResFolderInfos)
                {
                    CommonResUtils.AddPlaceholderFile(Application.dataPath + "/Export/" + baseInfo.Path);
                }
                foreach (var baseInfo in ColorResConfigController.Instance.ThumbnailResFolderInfos)
                {
                    CommonResUtils.AddPlaceholderFile(Application.dataPath + "/Export/" + baseInfo.Path);
                }
            }
        }
    }
}