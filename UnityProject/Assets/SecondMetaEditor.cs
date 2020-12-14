using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;
using OhMyFramework.Editor;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;


[OhMyFrameworkPanelGroup ("Group1")]
[OhMyFrameworkPanelTab ("Tool Editor1", "BombMixIcon2", 2)]
public class SecondModuleEditor : ModuleEditor<MonoBehaviour> {
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
        // using (new GUIHelper.Horizontal (GUILayout.ExpandWidth (true), GUILayout.ExpandHeight (true))) {
        //     using (new GUIHelper.Vertical (Styles.berryArea, GUILayout.Width (200), GUILayout.ExpandHeight (true))) {
        //         if(GUILayout.Button(new GUIContent("Texture"), GUILayout.ExpandWidth(true))){
        //             FindSameRes("Texture");
        //         }
        //         if(GUILayout.Button(new GUIContent("Material"), GUILayout.ExpandWidth(true))){
        //             FindSameRes("Material");
        //         }
        //     }
        //     using (new GUIHelper.Vertical (Styles.berryArea, GUILayout.ExpandHeight (true))) {
        //         if(GUILayout.Button(new GUIContent("Clear"), GUILayout.ExpandWidth(true))){
        //             resultSb.Clear();
        //         }
        //         // using(new GUIHelper.Scroll(Styles.area,GUILayout.ExpandWidth(true),GUILayout.ExpandHeight(true)).Start()){
        //             levelListScrollPosition = EditorGUILayout.BeginScrollView(levelListScrollPosition, GUILayout.ExpandHeight(true));
        //             if(resultSb != null){
        //                 GUILayout.TextArea(resultSb.ToString());
        //             }
        //             EditorGUILayout.EndScrollView();
        //         // }
        //     }
        // }
        // 左右下布局
        using (new GUIHelper.Horizontal (GUILayout.ExpandWidth (true), GUILayout.ExpandHeight (true))) {
            using (new GUIHelper.Vertical (Styles.berryArea, GUILayout.Width (200), GUILayout.ExpandHeight (true))) {
                if (GUILayout.Button ("SetEditLayerAndTagSettings"))
                {
//                    LayerAndTagEditTools.SetEditLayerAndTagSettings();
                }
            }
            using (new GUIHelper.Vertical (Styles.area, GUILayout.ExpandHeight (true))) {
                if (GUILayout.Button ("GenLayerAndTagCode"))
                {
//                    LayerAndTagEditTools.GenLayerAndTagCode();
                }  
            }
        }
        using (new GUIHelper.Horizontal (GUILayout.ExpandWidth (true), GUILayout.Height (50))) {
            if (GUILayout.Button ("button3")) {

            };
        }
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

}