using System;
using System.Linq;
using OhMyFramework.Core;
using UnityEditor;
using UnityEngine;

namespace OhMyFramework.Editor
{
    enum Tabs
    {
        General,
        UserInterface,
        AssetBundle,
        Server,
        Storage,
        Config,
        Etc
    }
    public class OmfSettingsWindow : EditorWindow
    {
        private Tabs _selectedTab;
        private bool _showBugReportSignupForm;
        private string[] _tabs = Enum.GetNames(typeof (Tabs)).ToArray();
        [MenuItem(MenuPathConst.OhMyFrameworkConfigs)]
        public static void ShowWindow()
        {
//            var window = GetWindow<OhMyFrameworkConfigsWindow>();
//            window.titleContent = new GUIContent("OhMyFrameworkConfigs");
//            window.Show();
            GetWindowWithRect<OmfSettingsWindow>(new Rect(0, 0, 520, 520), true, "OhMyFramework Configs", true);
        }

        private void OnGUI()
        {
            // title or logo
            EditorGUILayout.BeginVertical();
            GUILayout.Label("OhMyFramework");
            EditorGUILayout.EndVertical();

            // Draw tab buttons
            var rect = EditorGUILayout.BeginVertical(GUI.skin.box);

            --rect.width;
            var height = 18;

            EditorGUI.BeginChangeCheck();
//            EditorGUI.BeginDisabledGroup(!_enableTabChange);
            for (var i = 0; i < _tabs.Length; ++i)
            {
                var xStart = Mathf.RoundToInt(i*rect.width/_tabs.Length);
                var xEnd = Mathf.RoundToInt((i + 1)*rect.width/_tabs.Length);

                var pos = new Rect(rect.x + xStart, rect.y, xEnd - xStart, height);

                if (GUI.Toggle(pos, (int) _selectedTab == i, new GUIContent(_tabs[i]), EditorStyles.toolbarButton))
                {
                    _selectedTab = (Tabs) i;
                }
            }

            GUILayoutUtility.GetRect(10f, height);
            
            // Draw selected tab
            DrawTabGeneral();
            switch (_selectedTab)
            {
                case Tabs.General:
//                    DrawTabGeneral();
                    break;

                case Tabs.UserInterface:
//                    DrawTabLayout();
                    break;

                case Tabs.AssetBundle:
//                    DrawTabBugReporter();
                    break;

                case Tabs.Storage:
//                    DrawTabShortcuts();
                    break;

                case Tabs.Server:
//                    DrawTabAdvanced();
                    break; 
                case Tabs.Config:
//                    DrawTabShortcuts();
                    break;

                case Tabs.Etc:
//                    DrawTabAdvanced();
                    break;
            }

            EditorGUILayout.EndVertical();
            
            //foot 版权信息
            EditorGUILayout.BeginVertical();
            GUILayout.Label("copyright@OhMyFramework");
            EditorGUILayout.EndVertical();
            

            if (GUI.changed)
            {
                EditorUtility.SetDirty(OmfSettings.Instance);
            }
        }

        private void DrawTabGeneral()
        {
            OmfSettings.Instance.IsEnabled = true;
            GUILayout.Label("General");
            // Expand content area to fit all available space
            GUILayout.FlexibleSpace();
        }
    }
}