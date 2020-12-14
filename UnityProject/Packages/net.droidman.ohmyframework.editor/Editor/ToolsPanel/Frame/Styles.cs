using System;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 编辑器窗口样式
/// </summary>
namespace OhMyFramework.Editor
{
    public static class Styles
    {

        static bool initialized = false;

        [InitializeOnLoadMethod]
        internal static void OnLoad()
        {
            EditorApplication.update += Update;
        }

        public static bool IsInitialized()
        {
            return initialized;
        }

        internal static void Initialize()
        {
            try
            {
                richLabel = new GUIStyle(EditorStyles.label);
                richLabel.richText = true;


                whiteLabel = new GUIStyle(EditorStyles.whiteLabel);
                whiteLabel.normal.textColor = Color.white;

                whiteBoldLabel = new GUIStyle(EditorStyles.whiteBoldLabel);
                whiteBoldLabel.normal.textColor = Color.white;

                multilineLabel = new GUIStyle(EditorStyles.label);
                multilineLabel.clipping = TextClipping.Clip;

                centeredLabel = new GUIStyle(EditorStyles.label);
                centeredLabel.alignment = TextAnchor.MiddleCenter;

                centeredMiniLabel = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
                centeredMiniLabel.normal.textColor = EditorStyles.label.normal.textColor;

                miniLabel = new GUIStyle(centeredMiniLabel);
                miniLabel.alignment = richLabel.alignment;

                centeredMiniLabelWhite = new GUIStyle(centeredMiniLabel);
                centeredMiniLabelWhite.normal.textColor = Color.white;

                centeredMiniLabelBlack = new GUIStyle(centeredMiniLabel);
                centeredMiniLabelBlack.normal.textColor = Color.black;

                textAreaLineBreaked = new GUIStyle(EditorStyles.textArea);
                textAreaLineBreaked.clipping = TextClipping.Clip;

                monospaceLabel = new GUIStyle(textAreaLineBreaked);
                monospaceLabel.font = Resources.Load<Font>("Fonts/CourierNew");
                //monospaceLabel.wordWrap = true;
                monospaceLabel.fontSize = 11;

                largeTitle = new GUIStyle(EditorStyles.label);
                largeTitle.normal.textColor = EditorStyles.label.normal.textColor;
                largeTitle.fontStyle = FontStyle.Bold;
                largeTitle.fontSize = 21;

                title = new GUIStyle(EditorStyles.label);
                title.normal.textColor = EditorStyles.label.normal.textColor;
                title.fontStyle = FontStyle.Bold;
                title.fontSize = 18;

                berryArea = new GUIStyle(EditorStyles.textArea);
                berryArea.normal.background = EditorIcons.GetIcon("BerryArea");
                berryArea.border = new RectOffset(4, 4, 5, 3);
                berryArea.margin = new RectOffset(2, 2, 2, 2);
                berryArea.padding = new RectOffset(2, 2, 2, 2);

                area = new GUIStyle(EditorStyles.textArea);
                area.normal.background = EditorIcons.GetIcon("Area");
                area.border = new RectOffset(4, 4, 5, 3);
                area.margin = new RectOffset(2, 2, 2, 2);
                area.padding = new RectOffset(4, 4, 4, 4);

                levelArea = new GUIStyle(area);
                levelArea.normal.background = EditorIcons.GetIcon("LevelArea");
                levelArea.border = new RectOffset(4, 4, 5, 3);
                levelArea.margin = new RectOffset(2, 2, 2, 2);
                levelArea.padding = new RectOffset(4, 4, 4, 4);

                separator = new GUIStyle(area);
                separator.normal.background = EditorIcons.GetIcon("Separator");
                separator.border = new RectOffset(1, 1, 1, 1);
                separator.margin = new RectOffset(0, 0, 0, 0);
                separator.padding = new RectOffset(0, 0, 0, 0);

                highlightStrongBlue = "<color=#" + (EditorGUIUtility.isProSkin ? "8888ff" : "4444ff") + "ff>{0}</color>";
                highlightBlue = "<color=#" + (EditorGUIUtility.isProSkin ? "5555bb" : "222266") + "ff>{0}</color>";

                highlightStrongRed = "<color=#" + (EditorGUIUtility.isProSkin ? "ff8888" : "ff4444") + "ff>{0}</color>";
                highlightRed = "<color=#" + (EditorGUIUtility.isProSkin ? "bb5555" : "662222") + "ff>{0}</color>";

                highlightStrongGreen = "<color=#" + (EditorGUIUtility.isProSkin ? "88ff88" : "44ff44") + "ff>{0}</color>";
                highlightGreen = "<color=#" + (EditorGUIUtility.isProSkin ? "55bb55" : "226622") + "ff>{0}</color>";
            }
            catch (Exception)
            {
                return;
            }

            initialized = true;
        }

        internal static void Update()
        {
            if (!initialized || area.normal.background == null)
                Initialize();
        }

        public static GUIStyle richLabel;
        public static GUIStyle whiteLabel;
        public static GUIStyle whiteBoldLabel;
        public static GUIStyle multilineLabel;
        public static GUIStyle centeredLabel;
        public static GUIStyle miniLabel;
        public static GUIStyle centeredMiniLabel;
        public static GUIStyle centeredMiniLabelWhite;
        public static GUIStyle centeredMiniLabelBlack;

        public static GUIStyle largeTitle;
        public static GUIStyle title;
        public static GUIStyle berryArea;
        public static GUIStyle area;
        public static GUIStyle levelArea;
        public static GUIStyle textAreaLineBreaked;
        public static GUIStyle separator;
        public static GUIStyle monospaceLabel;

        public static string highlightStrongBlue;
        public static string highlightBlue;

        public static string highlightStrongRed;
        public static string highlightRed;

        public static string highlightStrongGreen;
        public static string highlightGreen;
    }
}