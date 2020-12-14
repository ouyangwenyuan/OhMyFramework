using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;

using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using OhMyFramework.Editor;

[OhMyFrameworkPanelGroup ("Group2")]
[OhMyFrameworkPanelTab ("Tool Editor", "BombMixIcon", 5)]
public class CommonModuleEditor : ModuleEditor<MonoBehaviour> {
    #region Icons
    public static Texture2D slotIcon;
    #endregion
    #region Colors
    Color missingSlotColor = new Color(1, 1, 1, .05f);
    public  static Color propertiesBackgroundColor = new Color(.8f, .8f, .8f, 1);
    #endregion
    // public LevelItem levelItem;
    // public LevelDesign design;
    
    private Vector2 levelListScrollPosition = new Vector2();
    private GUIHelper.LayoutSplitter splitterH; //分栏控件
    private StringBuilder resultSb = new StringBuilder();
    public override void OnGUI () {
        // 左右布局
        using (new GUIHelper.Horizontal (GUILayout.ExpandWidth (true), GUILayout.ExpandHeight (true))) {
            using (new GUIHelper.Vertical (Styles.berryArea, GUILayout.Width (200), GUILayout.ExpandHeight (true))) {
                if(GUILayout.Button(new GUIContent("Texture"), GUILayout.ExpandWidth(true))){
                    FindSameRes("Texture");
                }
                if(GUILayout.Button(new GUIContent("Material"), GUILayout.ExpandWidth(true))){
                    FindSameRes("Material");
                }
            }
            using (new GUIHelper.Vertical (Styles.berryArea, GUILayout.ExpandHeight (true))) {
                if(GUILayout.Button(new GUIContent("Clear"), GUILayout.ExpandWidth(true))){
                    resultSb.Clear();
                }
                // using(new GUIHelper.Scroll(Styles.area,GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true)).Start()){
                    levelListScrollPosition = EditorGUILayout.BeginScrollView(levelListScrollPosition, GUILayout.ExpandHeight(true));
                    if(resultSb != null){
                        GUILayout.TextArea(resultSb.ToString());
                    }
                    EditorGUILayout.EndScrollView();
                // }
            }
        }
        // 左右下布局
        // using (new GUIHelper.Horizontal (GUILayout.ExpandWidth (true), GUILayout.ExpandHeight (true))) {
        //     using (new GUIHelper.Vertical (Styles.berryArea, GUILayout.Width (200), GUILayout.ExpandHeight (true))) {
        //         if (GUILayout.Button ("button3")) {

        //         };
        //     }
        //     using (new GUIHelper.Vertical (Styles.area, GUILayout.ExpandHeight (true))) {
        //         if (GUILayout.Button ("button3")) {

        //         };     
        //     }
        // }
        // using (new GUIHelper.Horizontal (GUILayout.ExpandWidth (true), GUILayout.Height (50))) {
        //     if (GUILayout.Button ("button3")) {

        //     };
        // }
        //上左右布局
        // using (new GUIHelper.Horizontal (GUILayout.ExpandWidth (true), GUILayout.Height (50))) {
        //     if (GUILayout.Button ("button3")) {

        //     };
        // }
        // using (new GUIHelper.Horizontal (GUILayout.ExpandWidth (true), GUILayout.ExpandHeight (true))) {
        //     using (new GUIHelper.Vertical (Styles.berryArea, GUILayout.Width (200), GUILayout.ExpandHeight (true))) {
        //         if (GUILayout.Button ("button3")) {

        //         };
        //     }
        //     using (new GUIHelper.Vertical (Styles.area, GUILayout.ExpandHeight (true))) {
        //            if (GUILayout.Button ("button3")) {

        //         };     
        //     }
        // }

        // 分栏布局
        // using (new GUIHelper.Horizontal ()) {
        //     using (splitterH.Start (true, true, true)) {
        //         if (splitterH.Area ()) {
        //             //左布局
        //             using (new GUIHelper.Vertical (Styles.area, GUILayout.ExpandHeight (true))) {
        //             //     using (new GUIHelper.BackgroundColor (Color.Lerp (Color.white, Color.green, 0.6f))) {
        //             //         if (GUILayout.Button ("LoadAll")) {
        //             //             fileList.ReloadAllData ();
        //             //             AssetDatabase.Refresh ();
        //             //         }
        //             //          if (GUILayout.Button("SaveAll"))
        //             //         {
        //             //             fileList.SaveAllData();
        //             //             AssetDatabase.Refresh();
        //             //         }
        //             //     }
        //             //     levelListScrollPosition = EditorGUILayout.BeginScrollView (levelListScrollPosition, GUILayout.ExpandHeight (true));
        //             //     GUILayout.Label ("Levels List", Styles.centeredMiniLabel, GUILayout.ExpandWidth (true));
        //             //     Rect rect = GUILayoutUtility.GetRect (100, 100, GUILayout.MinHeight (fileList.totalHeight + 200), GUILayout.ExpandHeight (true));
        //             //     fileList.OnGUI (rect);
        //             //     EditorGUILayout.EndScrollView ();
        //             }
        //         }
        //         if (splitterH.Area ()) {
        //              //中布局
        //             using (new GUIHelper.Vertical(Styles.area, GUILayout.ExpandHeight(true)))
        //             {
        //                 // if (design != null)
        //                 // {
        //                 //     slots = design.slots.ToDictionary(x => x.position, x => x);
        //                 //     DrawLevelParameters();
        //                 // }
        //                 // else {
        //                 //     GUILayout.Box("please select level", EditorStyles.label, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
        //                 // }
        //             }
        //         }

        //         if (splitterH.Area ()) {
        //              //右布局
        //             using (new GUIHelper.Vertical (Styles.area, GUILayout.ExpandHeight (true))) {
        //                 // DrawFieldView(true, true, true);
        //             }
        //         }
        //     }
        // }
        
    }
    public override bool Initialize () {
        #region Levels list
        // List<TreeFolder> levelFolders = null;
        // FileList fileList;
        // TreeViewState levelTreeViewState = new TreeViewState();
        // List<LevelItem> levelItems = WorldLoader.getAllLevelFileInfo();
        // fileList = new FileList (levelItem, levelItems, levelFolders, levelTreeViewState);
        // fileList.onSelectedItemChanged += x => {
        //     // if (x.Count == 1 && x[0] != levelItem)
        //     //     SelectLevel (x[0], true);
        // };
       
         
//        splitterH = new GUIHelper.LayoutSplitter (OrientationLine.Horizontal, OrientationLine.Vertical, new float[2] { 200, 300 });
//        splitterH.drawCursor = x => GUI.Box (x, "", Styles.separator);
        #endregion

        #region Icons
        slotIcon = EditorIcons.GetIcon("SlotIcon");
        #endregion
        return true;
    }

    public override void OnLostFoce () {

    }

    public override void OnFocus () {

    }
    Dictionary<string, List<Type>> editors = new Dictionary<string, List<Type>>();
    /// <summary>
    ///  Editor 框架 的设计，1、定义接口，定义基类 2、通过给实现类添加注解（属性标签）找到所有实现类，3、通过反射拿到实现类的Type 4、实例化实现类 ：Activator.CreateInstance(type)。5、操作接口和基类来实现逻辑
    /// </summary>
    void LoadEditors()
    {
        Type _interface = typeof(IModuleEditor); //接口
        Type _base_type = typeof(ModuleEditor<>); //基类（可范型）

        List<Type> types = new List<Type>();
        //获取当前domain的所有Assembly
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] assemblyTypes;
            try
            {
                //获取所有Assembly的所有Type 
                assemblyTypes = assembly.GetTypes();
                foreach (Type type in assemblyTypes)
                    if (_interface.IsAssignableFrom(type) && type != _interface && type != _base_type)
                        types.Add(type);
            }
            catch (ReflectionTypeLoadException e) { 
                Debug.LogError("反射异常 + e" + e.Message);
            }
        }

        //移除没有加属性注解的 MetaEditor 子类
        types.RemoveAll(x => x.GetCustomAttributes(true).FirstOrDefault(y => y is OhMyFrameworkPanelTabAttribute) == null);

        // 按优先级排序
        types.Sort((Type a, Type b) =>
        {
            OhMyFrameworkPanelTabAttribute _a = (OhMyFrameworkPanelTabAttribute)a.GetCustomAttributes(true).FirstOrDefault(x => x is OhMyFrameworkPanelTabAttribute);
            OhMyFrameworkPanelTabAttribute _b = (OhMyFrameworkPanelTabAttribute)b.GetCustomAttributes(true).FirstOrDefault(x => x is OhMyFrameworkPanelTabAttribute);
            return _a.Priority.CompareTo(_b.Priority);
        });

        editors.Clear();
        foreach (Type editor in types)
        {
            //分组，没有加BerryPanelGroupAttribute的放在“” 中
            OhMyFrameworkPanelGroupAttribute attr = (OhMyFrameworkPanelGroupAttribute)editor.GetCustomAttributes(true).FirstOrDefault(x => x is OhMyFrameworkPanelGroupAttribute);
            string group = attr != null ? attr.Group : "";
            if (!editors.ContainsKey(group))
                editors.Add(group, new List<Type>());
            editors[group].Add(editor);
        }
    }

    public  void FindSameRes(string searchType){
		// Dictionary<string, string> md5dic = new Dictionary<string, string> ();
		// string[] paths = AssetDatabase.FindAssets ("t:" + searchType, new string[] { "Assets" });
		// string currentpath = Directory.GetCurrentDirectory ();
		// Debug.LogFormat ("currentDirectory:{0}",currentpath);
		// foreach (var prefabguid in paths) {
		// 	string assetPath = AssetDatabase.GUIDToAssetPath (prefabguid);
		// 	// AssetImporter importer = AssetImporter.GetAtPath (assetPath);
		// 	// if (importer is TextureImporter || importer is ModelImporter) {
		// 		string md5 = getMd5Hash (Path.Combine (currentpath, assetPath));
		// 		string path;
		// 		if (!md5dic.TryGetValue (md5, out path)) {
		// 			md5dic[md5] = assetPath;
		// 		} else {
		// 			if (!string.IsNullOrEmpty (path) && path != assetPath) {
        //                 string result = string.Format("{0},{1}，资源有重复", path, assetPath);
		// 				// Debug.LogFormat (result);
        //                 resultSb.Append(result);
		// 			}
		// 		}
		// 	// }
		// }
	}
    string getMd5Hash (string path) {
		MD5 md5 = new MD5CryptoServiceProvider ();
		string rst = BitConverter.ToString (md5.ComputeHash (File.ReadAllBytes (path)));
		rst = rst.Replace ("-", "").ToLower ();
		return rst;
	}

}