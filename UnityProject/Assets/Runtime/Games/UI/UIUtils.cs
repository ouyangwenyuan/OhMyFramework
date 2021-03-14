using DragonPlus.Assets;
using DragonU3DSDK;
using DragonU3DSDK.Asset;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI {
    public static class UIUtils {
        public static void SetImage(GameObject go, string name) {
            var img = go?.GetComponent<Image>();
            SetImage(img, name);
        }

        public static void SetImage(GameObject go, Sprite icon) {
            var img = go?.GetComponent<Image>();
            img.sprite = icon;
        }

        public static void SetImage(Image img, string name) {
            if (img != null) {
                if (string.IsNullOrEmpty(name)) {
                    img.sprite = null;
                } else {
                    var asset = ResourcesManager.Instance.LoadResource<Sprite>(name);
                    if (asset != null) {
                        SetImage(img, asset);
                    }
                }
            }
        }

        public static void SetAtlasImage(Image img, string atlasName, string name) {
            if (img != null) {
                if (string.IsNullOrEmpty(atlasName) || string.IsNullOrEmpty(name)) {
                    img.sprite = null;
                } else {
                    var asset = ResourcesManager.Instance.GetSpriteVariant(atlasName, name);
                    if (asset != null) {
                        SetImage(img, asset);
                    }
                }
            }
        }

        public static void SetImage(Image img, Sprite sprite) {
            if (img != null) {
                img.sprite = sprite;
            }
        }
    }
}