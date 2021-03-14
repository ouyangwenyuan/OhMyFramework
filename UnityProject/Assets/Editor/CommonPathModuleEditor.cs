using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OhMyFramework.Core;
using OhMyFramework.Editor;
using UnityEditor;
using UnityEngine;

[OhMyFrameworkPanelTab("常用路径打开", "BombMixIcon1", 0)]
public class CommonPathModuleEditor : ModuleEditor<MonoBehaviour>
{
    #region Icons

    public static Texture2D slotIcon;

    #endregion

    #region Colors

    Color missingSlotColor = new Color(1, 1, 1, .05f);
    public static Color propertiesBackgroundColor = new Color(.8f, .8f, .8f, 1);

    #endregion

    private Vector2 levelListScrollPosition = new Vector2();

//    private GUIHelper.LayoutSplitter splitterH; //分栏控件
    private StringBuilder resultSb = new StringBuilder();
//    private OmfSettings.UserPathData m_UserData;
    private string gamePath;
    private readonly string projectPathLabel = "工程路径:";
    public override bool Initialize()
    {
//        m_UserData = OmfSettings.Instance.mUserPathData;
//        splitterH = new GUIHelper.LayoutSplitter (OrientationLine.Horizontal, OrientationLine.Vertical, new float[1] {  300 });
//        splitterH.drawCursor = x => GUI.Box (x, "", Styles.separator);
        gamePath = System.IO.Path.GetFullPath(".");
        gamePath = gamePath.Replace("\\", "/");
        return true;
    }

    public override void OnGUI()
    {

        using (new GUIHelper.Vertical(Styles.area, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
        {
            using (new GUIHelper.BackgroundColor(Color.Lerp(Color.white, Color.green, 0.6f)))
            {

                using (new GUIHelper.Horizontal())
                {
                    EditorGUILayout.TextField(projectPathLabel, gamePath,GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("Open",GUILayout.MaxWidth(100)))
                    {
                        Process.Start (gamePath);
                    }
                }

                using (new GUIHelper.Horizontal())
                {
                    EditorGUILayout.TextField("DataPath", Application.dataPath,GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("Open",GUILayout.MaxWidth(100)))
                    {
                        Process.Start (Application.dataPath);
                    }
                }

                using (new GUIHelper.Horizontal())
                {
                    EditorGUILayout.TextField("PersistentDataPath", Application.persistentDataPath,GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("Open",GUILayout.MaxWidth(100)))
                    {
                        Process.Start (Application.persistentDataPath);
                    }
                }

                using (new GUIHelper.Horizontal())
                {
                    EditorGUILayout.TextField("StreamingAssetsPath", Application.streamingAssetsPath,GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("Open",GUILayout.MaxWidth(100)))
                    {
                        Process.Start (Application.streamingAssetsPath);
                    }
                }
                using (new GUIHelper.Horizontal())
                {
                    EditorGUILayout.TextField("ConsoleLogPath", Application.consoleLogPath,GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("Open",GUILayout.MaxWidth(100)))
                    {
                        Process.Start (Application.consoleLogPath);
                    }
                }
                using (new GUIHelper.Horizontal())
                {
                    EditorGUILayout.TextField("TemporaryCachePath", Application.temporaryCachePath,GUILayout.ExpandWidth(true));
                    if (GUILayout.Button("Open",GUILayout.MaxWidth(100)))
                    {
                        Process.Start (Application.temporaryCachePath);
                    }
                }

//                foreach (var item in m_UserData.commonPaths)
//                {
//                    using (new GUIHelper.Horizontal())
//                    {
//                        EditorGUILayout.TextField(item.Key,item.Value,GUILayout.ExpandWidth(true));
//                        if (GUILayout.Button("select folder",GUILayout.MaxWidth(100)))
//                        {
//                            m_UserData.commonPaths[item.Key] = BrowseForFolder();
//                        }
//                        if (GUILayout.Button("Open",GUILayout.MaxWidth(100)))
//                        {
//                            Process.Start (m_UserData.commonPaths[item.Key]);
//                        }
//                    } 
//                }
            }
        }
        
    }

    public override void OnLostFocus()
    {

    }

    public override void OnFocus()
    {

    }

    

    private string BrowseForFolder()
    {
        return EditorUtility.OpenFolderPanel("Select Folder", gamePath, "Assets");
    }

    public static string transformAssetFolder(string fullpath)
    {
        string newPath = fullpath;
        var gamePath = System.IO.Path.GetFullPath(".");
        if (!string.IsNullOrEmpty(fullpath))
        {
            if (fullpath.StartsWith(gamePath) && fullpath.Length > gamePath.Length)
                newPath = fullpath.Remove(0, gamePath.Length + 1);
//            m_UserData.m_OutputPath = newPath;
//            EditorUserBuildSettings.SetPlatformSettings(EditorUserBuildSettings.activeBuildTarget.ToString(), "AssetBundleOutputPath", newPath);
        }
        return newPath;
    }
}