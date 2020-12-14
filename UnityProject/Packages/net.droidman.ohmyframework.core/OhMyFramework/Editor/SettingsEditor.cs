using UnityEngine;
using UnityEditor;
namespace OhMyFramework.Editor
{
    using Core;
    
    [CustomEditor(typeof(OmfSettings))]
    public class SettingsEditor : UnityEditor.Editor
    {
        private bool _override;
        public override void OnInspectorGUI()
        {
            
            GUILayout.Label(
                "Oh My framework Settings window.",
                EditorStyles.wordWrappedLabel);

            EditorGUILayout.Separator();

            if (GUILayout.Button("打开设置"))
            {
                OmfSettingsWindow.ShowWindow();
            }

            if (!_override)
            {
                if (GUILayout.Button("显示设置"))
                {
                    _override = true;
                }
            }
            else
            {
                GUILayout.Label(
                    "设置列表",
                    EditorStyles.wordWrappedLabel);
            }

            EditorGUILayout.Separator();

            if (_override)
            {
                base.OnInspectorGUI();
            }
        }
    }
  
//    [CustomEditor(typeof(OmfConfigs))]
//    public class ConfigsEditor : UnityEditor.Editor
//    {
//        private bool _override;
//        public override void OnInspectorGUI()
//        {
//            
//            GUILayout.Label(
//                "This asset contains the runtime settings used by OhMyFrameworkConfigs. It is recommended that this asset be edited only via the OhMyFrameworkConfigs Settings window.",
//                EditorStyles.wordWrappedLabel);
//
//            EditorGUILayout.Separator();
//
//            if (GUILayout.Button("打开配置"))
//            {
//                OmfSettingsWindow.ShowWindow();
//            }
//
//            if (!_override)
//            {
//                if (GUILayout.Button("显示配置"))
//                {
//                    _override = true;
//                }
//            }
//            else
//            {
//                GUILayout.Label(
//                    "配置列表",
//                    EditorStyles.wordWrappedLabel);
//            }
//
//            EditorGUILayout.Separator();
//
//            if (_override)
//            {
//                base.OnInspectorGUI();
//            }
//        }
//    }
}