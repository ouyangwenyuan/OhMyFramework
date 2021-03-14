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
using OhMyFramework.Core;
using OhMyFramework.Utils;


[OhMyFrameworkPanelGroup ("Tags&Layers")]
[OhMyFrameworkPanelTab ("SortLayers", "BombMixIcon2", 2)]
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
        using (new GUIHelper.Vertical())
        {
            int[] ids;
            string[] names;
            CommonUtils.GetValuesAndFieldNames<GameSortingLayer>(out ids, out names);
            
            for (int i = 0; i < names.Length; i++)
            {
                EditorGUILayout.TextField($"SortLayer{ids[i]}",names[i]);
            }

            using (new GUIHelper.Horizontal())
            {
                if (GUILayout.Button("Set SortLayer"))
                {
                    GameSortingLayerEditor.SetEditorTag();
                }

                if (GUILayout.Button("GenerateConstSortLayer"))
                {
                    GameSortingLayerEditor.GameSortingLayer();
                }
            }

        }
    }
    public override bool Initialize () {

        return true;
    }

    public override void OnLostFocus () {

    }

    public override void OnFocus () {

    }

}