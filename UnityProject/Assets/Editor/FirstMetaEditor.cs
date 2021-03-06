﻿using UnityEditor.IMGUI.Controls;
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


[OhMyFrameworkPanelGroup ("Tags&Layers")]
[OhMyFrameworkPanelTab ("Tags", "BombMixIcon1")]
public class FirstModuleEditor : ModuleEditor<MonoBehaviour> {
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
            var tags = OmfSettings.Instance.customTags;
            for (int i = 0; i < tags.Length; i++)
            {
                EditorGUILayout.TextField($"Tag{7+i}",tags[i]);
            }

            using (new GUIHelper.Horizontal())
            {
                if (GUILayout.Button("Set Tags"))
                {
                    GameTag.SetEditorTag();
                }

                if (GUILayout.Button("GenerateConstTags"))
                {
                    GameTag.GenerateGameTag();
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