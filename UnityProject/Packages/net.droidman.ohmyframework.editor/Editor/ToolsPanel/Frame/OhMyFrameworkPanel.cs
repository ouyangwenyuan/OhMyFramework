using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using OhMyFramework.Editor;

/// <summary>
/// 编辑器界面
/// </summary>
public class OhMyFrameworkPanel : EditorWindow
{
    public static OhMyFrameworkPanel instance = null;

    Texture BPLogo;
    static bool style_IsInitialized = false;
    static GUIStyle style_Exeption;
    static GUIStyle tabButtonStyle;
    private GUIHelper.LayoutSplitter splitterH; //分栏控件
    void InitializeStyles()
    {
        style_Exeption = new GUIStyle(GUI.skin.label);
        style_Exeption.normal.textColor = new Color(0.5f, 0, 0, 1);
        style_Exeption.alignment = TextAnchor.UpperLeft;
        style_Exeption.wordWrap = true;

        tabButtonStyle = new GUIStyle(EditorStyles.miniButton);
        tabButtonStyle.normal.textColor = Color.white;
        tabButtonStyle.active.textColor = new Color(1, .8f, .8f, 1);

        style_IsInitialized = true;
    }

    public OhMyFrameworkPanelTabAttribute editorAttribute = null;
    Color selectionColor;
    Color bgColor;

    [MenuItem("Window/OhMyFramework/OhMyFramework Panel &F")]
    public static OhMyFrameworkPanel CreateBerryPanel()
    {
        OhMyFrameworkPanel window;
        if (instance == null)
        {
            window = GetWindow<OhMyFrameworkPanel>();
            window.Show();
            window.Init();
        }
        else
        {
            window = instance;
            window.Show();
        }
        return window;
    }
    void OnEnable()
    {
        //在此函数内刷新panel
        Debug.Log(" Init editor style");
        Init();
    }
    void OnFocus()
    {
        if (current_editor != null)
            current_editor.OnFocus();
    }

    public static void RepaintAll()
    {
        if (instance)
            instance.Repaint();
    }

    void Init()
    {
        instance = this;

        // Styles.Initialize();

        titleContent.text = "OhMyFramework Panel";
        BPLogo = EditorIcons.GetIcon("BPLogo");

        LoadEditors();

        ShowFirstEditor();

        selectionColor = Color.Lerp(Color.red, Color.white, 0.7f);
        bgColor = Color.Lerp(GUI.backgroundColor, Color.black, 0.3f);
        //EditorCoroutine.start(DownloadHelpLibraryRoutine());
    }

    private void ShowFirstEditor()
    {
        if (!save_CurrentEditor.IsEmpty())
        {
            Type _interface = typeof(IModuleEditor);
            Type _base_type = typeof(ModuleEditor<>);
            string editorName = save_CurrentEditor.String;
            Type savedEditor = editors.Values.SelectMany(x => x).FirstOrDefault(x => x.FullName == editorName);
            if (_interface.IsAssignableFrom(savedEditor) && savedEditor != _interface && savedEditor != _base_type)
            {
                Show((IModuleEditor)Activator.CreateInstance(savedEditor));
                return;
            }
        }

        Type defaultEditor = editors.Values.SelectMany(x => x).FirstOrDefault(x => x.GetCustomAttributes(true).FirstOrDefault(y => y is OhMyFrameworkPanelTabAttribute) != null);
        if (defaultEditor != null)
            Show((IModuleEditor)Activator.CreateInstance(defaultEditor));
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

    Color defalutColor;
    public Vector2 editorScroll, tabsScroll = new Vector2();
    IModuleEditor current_editor = null;
    public IModuleEditor CurrentEditor
    {
        get
        {
            return current_editor;
        }
    }

    Action editorRender;
    void OnGUI()
    {
        Styles.Update();

        if (!style_IsInitialized)
            InitializeStyles();

        if (BPLogo == null)
            BPLogo = EditorIcons.GetIcon("BPLogo");
//        GUI.DrawTexture(EditorGUILayout.GetControlRect(GUILayout.Width(BPLogo.width), GUILayout.Height(BPLogo.height)), BPLogo);
        EditorGUILayout.Space();

        if (editorRender == null || current_editor == null)
        {
            editorRender = null;
            current_editor = null;
        }

        defalutColor = GUI.backgroundColor;
        using (new GUIHelper.Horizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
        {
            using (new GUIHelper.Vertical(Styles.berryArea, GUILayout.Width(256), GUILayout.ExpandHeight(true)))
            {
                tabsScroll = EditorGUILayout.BeginScrollView(tabsScroll);
                DrawTabs();
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();

                Rect editorRect = EditorGUILayout.BeginVertical(Styles.berryArea, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                editorScroll = EditorGUILayout.BeginScrollView(editorScroll);
                if (current_editor != null && editorRender != null)
                {
                    if (editorAttribute != null)
                        DrawTitle(editorAttribute.Title);
                        if (EditorApplication.isCompiling)
                        {
                            GUILayout.Label("Compiling...", Styles.centeredMiniLabel, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                        }
                        else
                        {
                            if (Event.current.type == EventType.Repaint)
                                currectEditorRect = editorRect;
                            editorRender.Invoke();
                        }
                } else{
                    GUILayout.Label("Nothing selected", Styles.centeredMiniLabel, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                }  
                
                EditorGUILayout.EndScrollView();
            }
        }

        GUILayout.Label(string.Format("Simple Rules Tools Panel\n@Copyright 2015 - {0} by Droidman",
            DateTime.Now.Year), Styles.centeredMiniLabel, GUILayout.ExpandWidth(true));
    }

    void DrawTabs()
    {
        //没有分组的类型先绘制
        DrawTabs("");

        foreach (var group in editors)
            if (!string.IsNullOrEmpty(group.Key))
                DrawTabs(group.Key);
    }

    void DrawTabs(string group)
    {
        if (editors.ContainsKey(group))
        {
            if (!string.IsNullOrEmpty(group))
                DrawTabTitle(group);

            foreach (Type editor in editors[group])
            {
                OhMyFrameworkPanelTabAttribute attr = (OhMyFrameworkPanelTabAttribute)editor.GetCustomAttributes(true).FirstOrDefault(x => x is OhMyFrameworkPanelTabAttribute);
                if (attr != null && DrawTabButton(attr))
                    Show((IModuleEditor)Activator.CreateInstance(editor));
            }
        }
    }

    bool DrawTabButton(OhMyFrameworkPanelTabAttribute tabAttribute)
    {
        bool result = false;
        if (tabAttribute != null)
        {
            using (new GUIHelper.BackgroundColor(editorAttribute != null && editorAttribute.Match(tabAttribute) ? selectionColor : Color.white))
            using (new GUIHelper.ContentColor(Styles.centeredMiniLabel.normal.textColor))
                result = GUILayout.Button(new GUIContent(tabAttribute.Title, tabAttribute.Icon), tabButtonStyle, GUILayout.ExpandWidth(true));

            if (editorAttribute != null && editorAttribute.Match(tabAttribute) && editorRender == null)
                result = true;
        }

        return result;
    }

    void DrawTabTitle(string text)
    {
        GUILayout.Label(text, Styles.centeredMiniLabel, GUILayout.ExpandWidth(true));
    }

    void DrawTitle(string text)
    {
        GUILayout.Label(text, Styles.largeTitle, GUILayout.ExpandWidth(true));
        GUILayout.Space(10);
    }

    public static void Scroll(float position)
    {
        if (instance != null)
            instance.editorScroll = new Vector2(0, position);
    }

    PrefVariable save_CurrentEditor = new PrefVariable("BerryPanel_CurrentEditor");
    public static Rect currectEditorRect = new Rect();

    public void Show(IModuleEditor editor)
    {
        EditorGUI.FocusTextInControl("");
        if (editor.Initialize())
        {
            current_editor = editor;
            save_CurrentEditor.String = editor.GetType().FullName;

            OhMyFrameworkPanelTabAttribute attribute = (OhMyFrameworkPanelTabAttribute)editor.GetType().GetCustomAttributes(true).FirstOrDefault(x => x is OhMyFrameworkPanelTabAttribute);
            editorAttribute = attribute;

            editorRender = editor.OnGUI;
        }
    }



    public static IModuleEditor GetCurrentEditor()
    {
        if (instance == null)
            return null;
        return instance.current_editor;
    }

    public void Show(string editorName)
    {
        Type editor = editors.SelectMany(x => x.Value).FirstOrDefault(x => x.Name == editorName);
        if (editor != null)
            Show((IModuleEditor)Activator.CreateInstance(editor));
    }



    private void OnDestroy()
    {
        //DifficultValue.Clear();
    }
    private void OnLostFocus()
    {
        if (current_editor != null)
        {
            current_editor.OnLostFocus();
        }
    }
}
