using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DesignerTools : MonoBehaviour {
    const string PrefabPath = "Prefabs/UI/Cooking/Home";
    const string scriptBaseRoot = "Assets/CookingSubmodule/Scripts/CodeGen/UI";
    const string scriptControllerRoot = "Assets/CookingSubmodule/Scripts/World/NewMap";
    const string controllerType = "Controller";
    const string prefabType = ".prefab";
    const string scriptType = ".cs";
    const string controller_temp_str = @"
using UnityEngine;
public partial class #Name#Controller {
}
";
    const string view_temp_str = @"
using UnityEngine;
using UnityEngine.UI;
public partial class #Name#Controller : MonoBehaviour {
#Properties#
    public void Awake(){
#GetProperties#
#AddPropertiesListerner#
        Initialize ();
    }
}
/*
using UnityEngine;
using DragonU3DSDK.Asset;
/// <summary>
/// #FULL_PATH#
/// </summary>
public partial class #Name#Controller{
    public static readonly string AssetPath = #AssetPath4Mono#;
    public static #Name#Controller Create(Transform parent){
        var go = ResourcesManager.Instance.LoadResource<GameObject>(AssetPath);
        return Instantiate(go, parent, false).AddComponent<#Name#Controller>();
    }
    protected void Initialize(){
        //TODO
    }
    protected void OnDestroy(){
#RemovePropertiesListerner#
    }
#ClickListernerOverride#
}
*/
";
    const string window_temp_str = @"
using UnityEngine;
using UnityEngine.UI;
public partial class #Name#Controller : UIWindow {
    public System.Action CloseAction;
#Properties#
    public override void PrivateAwake(){
#GetProperties#
#AddPropertiesListerner#
        Initialize ();
    }

    public void CloseButtonClick(){
        CommonUtils.TweenClose (transform, () => {
            CloseWindowWithinUIMgr (true);
            CloseAction?.Invoke();
        });
    }



    protected override void OnCloseWindow (bool destroy = false) {
        if (destroy) {
    #RemovePropertiesListerner#
        } else {
            OnHide ();
        }
        base.OnCloseWindow(destroy);
    }
}
/*
using UnityEngine;
/// <summary>
/// #FULL_PATH#
/// </summary>
public partial class #Name#Controller {
    public static readonly string AssetPath = #AssetPath4UIWindow#;
    public static void Open(params object[] objs)
    {
        UIManager.Instance.OpenHomeWindow(AssetPath, objs);
        // UIManager.Instance.OpenCookingWindow(AssetPath, UIWindowLayer.Tips, UIWindowType.PopupTip, objs);
    }
    public static #Name#Controller Open(System.Action CloseAction= null,params object[] objs)
    {
        #Name#Controller window = UIManager.Instance.OpenHomeWindow(AssetPath, objs) as #Name#Controller;
        // var window = UIManager.Instance.OpenCookingWindow<#Name#Controller>(AssetPath, UIWindowLayer.Tips, UIWindowType.PopupTip, objs);
        window.CloseAction = CloseAction;
        return window;
    }
    protected void Initialize(){
        //TODO
    }
    protected override void OnOpenWindow (params object[] objs){
        CommonUtils.TweenOpen(transform);
    }
    protected void OnHide (){

    }
    private void OnDestroy(){

    }
#ClickListernerOverride#
}
*/
";
    private static readonly Dictionary<string, string> SimplifyNameDictionary = new Dictionary<string, string> { { "LocalizeTextMeshProUGUI", "TextMesh" },
        { " ", "_" },
        { "`", "_" },
        { "~", "_" },
        { "!", "_" },
        { "@", "_" },
        { "#", "_" },
        { "$", "_" },
        { "%", "_" },
        { "^", "_" },
        { "&", "_" },
        { "*", "_" },
        { "(", "_" },
        { ")", "_" },
        { "-", "_" },
        { "+", "_" },
        { "=", "_" },
        { "{", "_" },
        { "}", "_" },
        { "[", "_" },
        { "]", "_" },
        { "|", "_" },
        { "\\", "_" },
        { ":", "_" },
        { ";", "_" },
        { "\"", "_" },
        { "'", "_" },
        { "?", "_" },
        { "/", "_" },
        { "<", "_" },
        { ">", "_" },
        { ",", "_" },
        { ".", "_" },
        { "\n", "_" },
    };

    /// <summary>
    /// order is important
    /// </summary>
    private static readonly Dictionary<string, string> SimplifyClassNameDictionary = new Dictionary<string, string> { { "UnityEngine.UI.", "" },
        { "UnityEngine.", "" },
        // { "DragonPlus.", "" },
    };

    public static string SimplifyName (string name) {
        var newName = SimplifyNameDictionary.Aggregate (name, (current, n) => current.Replace (n.Key, n.Value));
        newName = newName.Substring(0, 1).ToUpper() + newName.Substring(1);
        return newName;
    }

    private static string SimplifyClassName (string name) {
        return SimplifyClassNameDictionary.Aggregate (name, (current, n) => current.Replace (n.Key, n.Value));
    }
    // const string templatePath = "Assets/Match3Submodule/Scripts/Editor/Templ/UIBase.cs.txt";
    static StringBuilder propertiesStr = new StringBuilder ();
    static StringBuilder getPropertiesStr = new StringBuilder ();
    static StringBuilder addListenerStr = new StringBuilder ();
//    static StringBuilder clickListenStr = new StringBuilder ();
    static StringBuilder clickListenOverrideStr = new StringBuilder ();
    static StringBuilder removeListenerstr = new StringBuilder ();
    //  确保变量唯一
    static Dictionary<string, int> uniqueNames = new Dictionary<string, int> ();
    //确保路径唯一
    static HashSet<string> uniquePaths = new HashSet<string> ();

    // [MenuItem ("Tools/UI/Refresh Git", priority = 20)]
    // public static void RefreshGit () {
    //     string scriptName = Application.dataPath + "/../../do_git";
    //     EditorUtils.ProcessCommand (scriptName, new string[] { });
    // }

    [MenuItem ("Tools/UI/导出模型预制体", priority = 201)]
    [MenuItem ("GameObject/导出模型预制体", priority = 1)]
    public static void ExportModels () {
        Transform[] trans = Selection.GetTransforms (SelectionMode.TopLevel);
        foreach (Transform tran in trans) {
            GameObject selectObj = tran.gameObject;
            string prefabPath = string.Format ("Assets/DecoModelPrefab/{0}{1}", tran.name, prefabType);
            bool success = false;
            try {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject> (prefabPath);
                if (prefab) {
                    // if(EditorUtility.DisplayDialog("警告","已存在预制体，可能存在同名，是否需要覆盖？","yes","cancel")){
                    PrefabUtility.SaveAsPrefabAsset (selectObj, prefabPath, out success);
                    // }
                    success = true;
                } else {
                    PrefabUtility.SaveAsPrefabAsset (selectObj, prefabPath, out success);
                }
            } catch (System.Exception e) {
                UnityEngine.Debug.LogError ("生成预制体失败,请检查预制体的路径或者检查Resource/Settinggs/文件下是否有MatchPropertyConfig文件，没有请先生成" + e.Message);
                // if (!success) {
                //     EditorUtility.DisplayDialog("警告","生成预制体失败,请检查预制体的路径或者检查Resource/Settinggs/文件下是否有MatchPropertyConfig文件，没有请先生成","ok");
                //     return;
                // }
            }
        }
    }

    public static void ExportScripts (bool isWindow) {
        GameObject[] objs = Selection.gameObjects;
        foreach (var obj in objs) {
            ExportScript (isWindow, obj);
        }
    }

    [MenuItem ("Tools/UI/UIWindow模版 &M", priority = 1)] // & = alt + shift ; % = cmd + shift; #/^ = shift;
    public static void CreateUIPanelBase () {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject> ("Assets/Plugins/CommonTools/Editor/OyTools/Panel"+prefabType);
        if(prefab){
             GameObject canvas = GameObject.Find ("Canvas");
            if (canvas == null) {
                EditorUtility.DisplayDialog ("警告", "没有Canvas 节点", "ok");
                return;
            }
            var obj = GameObject.Instantiate(prefab);
            obj.name = "UIPanel";
            obj.transform.SetParent (canvas.transform);
            (obj.transform as RectTransform).sizeDelta = Vector2.zero;
            (obj.transform as RectTransform).anchoredPosition = Vector2.zero;
        }else{
            CreateUIPanelBase1();
        }
    }
   static void SetFullRect( RectTransform t) {
        t.anchorMin = Vector2.zero;
        t.anchorMax = Vector2.one;
        t.anchoredPosition = Vector2.zero;
        t.sizeDelta = Vector2.zero;
        t.pivot = Vector2.one * 0.5f;
        t.localScale = Vector3.one;
    }
    public static void CreateUIPanelBase1 () {
        GameObject canvas = GameObject.Find ("Canvas");
        if (canvas == null) {
            EditorUtility.DisplayDialog ("警告", "没有Canvas 节点", "ok");
            return;
        }

        GameObject obj = new GameObject ("BasePanel");
        obj.layer = LayerMask.NameToLayer ("UI");
        // obj.AddComponent<Canvas> ();
        // obj.AddComponent<CanvasGroup> ();
        obj.AddComponent<Image> ().color = Color.black;
        // obj.AddComponent<GraphicRaycaster> ();
        Animator animator = obj.AddComponent<Animator> ();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath ("Assets/Export/UI/Cooking/Common/CommonPanelController.controller", typeof (RuntimeAnimatorController)) as RuntimeAnimatorController;
        RectTransform rootTrans = obj.GetComponent<RectTransform> ();
        // rootTrans.anchorMin = Vector2.zero;
        // rootTrans.anchorMax = Vector2.one;
        rootTrans.pivot = Vector2.one * 0.5f;
        CanvasScaler cs = canvas.GetComponent<CanvasScaler> ();
        rootTrans.sizeDelta = cs.referenceResolution;
        obj.transform.SetParent (canvas.transform);
        obj.transform.localScale = Vector3.one;
        // rootTrans.Reset();

        GameObject animLayer = new GameObject ("Root");
        animLayer.AddComponent<CanvasGroup> ();
        animLayer.AddComponent<RectTransform> ();
        animLayer.transform.SetParent (obj.transform);
        animLayer.transform.localScale = Vector3.one;

        // GameObject bgObj = new GameObject ("BlackBg");
        // RectTransform rectt = bgObj.AddComponent<RectTransform> ();
        // rectt.sizeDelta = new Vector2 (3000, 3000);
        // Image bg = bgObj.AddComponent<Image> ();
        // bg.color = new Color32 (0, 0, 0, 128);
        // bgObj.transform.SetParent (animLayer.transform);

        GameObject BaseBg = new GameObject ("BaseBoard");
        RectTransform rectbase = BaseBg.AddComponent<RectTransform> ();
        rectbase.sizeDelta = 500 * Vector2.one;
        BaseBg.AddComponent<Image> ().sprite = AssetDatabase.LoadAssetAtPath ("Assets/Res/Cooking/UI/Cooking/Common/background_14.png", typeof (Sprite)) as Sprite;
        BaseBg.transform.SetParent (animLayer.transform);
        BaseBg.transform.localScale = Vector3.one;

        GameObject CloseBtn = new GameObject ("CloseBtn");
        RectTransform CloseBtnRect = CloseBtn.AddComponent<RectTransform> ();
        CloseBtnRect.anchorMin = Vector2.one;
        CloseBtnRect.anchorMax = Vector2.one;
        // CloseBtnRect.pivot = Vector2.one * 0.5f;
        CloseBtnRect.sizeDelta = 50 * Vector2.one;
        CloseBtnRect.localPosition = cs.referenceResolution / 200;
        UnityEngine.Debug.Log ("localpos =" + CloseBtnRect.anchoredPosition);

        Button btn = CloseBtn.AddComponent<Button> ();
        CloseBtn.AddComponent<Image> ().sprite = AssetDatabase.LoadAssetAtPath ("Assets/Res/Cooking/UI/Cooking/Common/button_close.png", typeof (Sprite)) as Sprite;
        CloseBtn.transform.SetParent (obj.transform);
        CloseBtn.transform.localScale = Vector3.one;

        // obj.transform.Reset ();
    }
    // [MenuItem ("Tools/UI/创建预制体路径配置文件", priority = 11)]
    // public static void CreateConfigAsset () {
    //     CreateAsset<MatchPropertyConfig> (MatchPropertyConfig.PropertyConfigPath);
    //     MatchPropertyConfig config =   MatchPropertyConfig.Instance;
    //     config.scriptBaseRoot = "Assets/Match3Submodule/Scripts/UI/UIBase";
    // }
    /// <summary>
    ///  file directory 操作的是绝对路径， assetdatabase 操作的是工程目录，assets的父目录
    /// </summary>
    /// <param name="path"></param>
    /// <typeparam name="T"></typeparam>
    public static void CreateAsset<T> (string path) where T : ScriptableObject {
        string filepath = Path.Combine (Application.dataPath, "Resources", path + ".asset");
        // var folder = Path.GetDirectoryName (filepath);
        // UnityEngine.Debug.Log( "filepath " + filepath + ",folder=" + folder);
        // if (!Directory.Exists (folder)){
        //     Directory.CreateDirectory (folder);
        // }
        FileInfo f = new FileInfo (filepath);
        DirectoryInfo d = f.Directory;
        if (!d.Exists) {
            d.Create ();
        }
        if (File.Exists (filepath)) {
            return;
        }
        T ac = ScriptableObject.CreateInstance<T> ();
        AssetDatabase.CreateAsset (ac, "Assets/Resources/" + path + ".asset");
    }

    [MenuItem ("Tools/UI/导出预制体", priority = 1)]
    [MenuItem ("GameObject/导出预制体", priority = 1)]
    public static void ExportUI () {
        // List<string> prefabPathRoot = new List<string> ();
        // prefabPathRoot.AddRange (MatchPropertyConfig.Instance.match_Game_Effect_Paths);

        GameObject selectObj = Selection.activeGameObject;
        exportPrefab (selectObj);
        // if (!selectObj.name.StartsWith("UIWindow")) {
        //     UnityEngine.Debug.LogError ("导出的对象不是弹窗!不需要生成window脚本 ");
        //     return;
        // }
        // GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        // if(!prefab){
        //     prefab = PrefabUtility.CreateEmptyPrefab(prefabPath) as GameObject;
        //     PrefabUtility.CreatePrefab(prefabPath,selectObj,ReplacePrefabOptions.ConnectToPrefab);
        // } else {
        //     PrefabUtility.ReplacePrefab(selectObj,prefab,ReplacePrefabOptions.ConnectToPrefab);
        // }
        // ExportScript();
    }

    [MenuItem ("Tools/UI/导出UIWindow脚本 &P", priority = 10)]
    [MenuItem ("GameObject/导出UIWindow脚本", priority = 1)]
    public static void ExportUIwindowScript () {
        ExportScripts (true);
    }

    [MenuItem ("Tools/UI/导出预制体脚本 &D", priority = 13)]
    [MenuItem ("GameObject/导出预制体脚本", priority = 1)]
    public static void ExportMonoScript () {
        ExportScripts (false);
    }

    /// <summary>
    /// 默认全部对象导出，标记为except tag 的对象不导出，如果根目录tag 为Export，只有标记为Export 的子对象才会导出。
    /// </summary>
    /// <param name="isWindow"></param>
    /// <param name="selectObj"></param>
    private static void ExportScript (bool isWindow, GameObject selectObj) {
        // 生成UIbase
        // GameObject selectObj = Selection.activeGameObject;
        // string scriptBaseRoot = scriptBaseRoot;
        string name = selectObj.name;
        // if (name.Contains ("_")) { //路径
        //     name = name.Split ('_') [1];
        // }
        string scriptPath = string.Format ("{0}/{1}{2}", scriptBaseRoot, name, scriptType);
        string scriptControllerPath = string.Format ("{0}/{1}{2}", scriptControllerRoot, name + controllerType, scriptType);

        Transform selectTrans = selectObj.transform; // Selection.activeTransform;
        // foreach(Transform trans in selectObj.GetComponentInChildren<Transform>(true)){
        //     findChild(trans);
        // }
//        string exportType = selectTrans.tag;
        // UnityEngine.Debug.LogError ("tag=" + exportType);
        if (selectTrans.gameObject.CompareTag("Export")) { //按最少对象导出，只导出标记需要（Export）导出的，减少生成太多无用的对象。
            for (int i = 0; i < selectTrans.childCount; i++) {
                Transform trans = selectTrans.GetChild (i);
                // UnityEngine.Debug.LogError ("e trans.tag=" + trans.tag);
                findChild (trans, selectTrans, true);
            }
        } else { //按最多对象导出，不需要导出的标记（Except），用于不确定哪些对象是否需要导出。
            for (int i = 0; i < selectTrans.childCount; i++) {
                Transform trans = selectTrans.GetChild (i);
                // UnityEngine.Debug.LogError ("trans.tag=" + trans.tag);
                findChild (trans, selectTrans, false);
            }
        }
        string basecontent = null;
        if (isWindow) {
            // basecontent = basecontent.Replace ("MonoBehaviour", "UIWindow");
            basecontent = window_temp_str; // AssetDatabase.LoadAssetAtPath<TextAsset> (MatchPropertyConfig.Instance.UIBaseTemplatePath).text;
        } else {
            basecontent = view_temp_str; //AssetDatabase.LoadAssetAtPath<TextAsset> ("Assets/Match3Submodule/Scripts/Editor/Templ/UIBase.cs1.txt").text;
        }
        var assetPath = AssetDatabase.GetAssetPath (selectObj.GetInstanceID ());
        string assetPath4UIWindow = null;
        if (assetPath.Contains ("Assets/Export/Prefabs/UI/Cooking")) {
            assetPath4UIWindow = assetPath.Replace ("Assets/Export/Prefabs/UI/Cooking/", "").Replace (".prefab", "");
        } else {
            assetPath4UIWindow = assetPath.Replace ("Assets/Export/Prefabs/UI/Cooking/Home/", "").Replace (".prefab", "");
        }
        var assetPath4MonoBehaviour = assetPath.Replace ("Assets/Export/", "").Replace (".prefab", "");

        basecontent = basecontent.Replace("#FULL_PATH#", assetPath)
            .Replace("#AssetPath4UIWindow#", $"\"{assetPath4UIWindow}\"")
            .Replace("#AssetPath4Mono#", $"\"{assetPath4MonoBehaviour}\"")
            .Replace("#Name#", name) //Path.GetFileNameWithoutExtension (scriptPath));
            .Replace("#Properties#", propertiesStr.ToString())
            .Replace("#GetProperties#", getPropertiesStr.ToString())
            .Replace("#AddPropertiesListerner#", addListenerStr.ToString())
//            .Replace("#ClickListernerImpl#", clickListenStr.ToString())
            .Replace("#ClickListernerOverride#", clickListenOverrideStr.ToString())
            .Replace("#RemovePropertiesListerner#", removeListenerstr.ToString());
        if (File.Exists (scriptPath.Replace ("Assets", Application.dataPath))) {
            // if (EditorUtility.DisplayDialog ("警告", "已存在脚本，是否需要覆盖", "yes", "cancel")) {
                saveScript (scriptPath, basecontent);
            // }
        } else {
            saveScript (scriptPath, basecontent);
            if (!File.Exists (scriptControllerPath.Replace ("Assets", Application.dataPath))) {
                ExportControllerScript (scriptControllerPath, name);
            }
        }
    }

    public static void ExportControllerScript (string scriptPath, string name) {
        string basecontent = controller_temp_str; // AssetDatabase.LoadAssetAtPath<TextAsset> (MatchPropertyConfig.Instance.UIControllerTemplatePath).text;
        basecontent = basecontent.Replace ("#Name#", name); // Path.GetFileNameWithoutExtension (scriptPath));
        File.WriteAllText (scriptPath, basecontent);
    }
    private static void saveScript (string scriptPath, string basecontent) {
        File.WriteAllText (scriptPath, basecontent);
        propertiesStr.Clear ();
        getPropertiesStr.Clear ();
        uniqueNames.Clear ();
        uniquePaths.Clear ();
        addListenerStr.Clear ();
        removeListenerstr.Clear ();
//        clickListenStr.Clear ();
        clickListenOverrideStr.Clear ();
        uniqueNames.Clear ();
        AssetDatabase.Refresh ();
    }

    private static void exportPrefab (GameObject selectObj) {
        if (!Selection.activeTransform) {
            UnityEngine.Debug.LogError ("请选择需要导出的对象");
            EditorUtility.DisplayDialog ("警告", "请选择需要导出的对象", "ok");
            return;
        }
        // GameObject selectObj = Selection.activeGameObject;
        string filename = selectObj.name;
        // if (!filename.Contains ("_")) {
        //     UnityEngine.Debug.LogError ("导出的对象命名不规范");
        //     EditorUtility.DisplayDialog ("警告", "导出的对象命名不规范", "ok");
        //     return;
        // }
        // string[] filesplit = filename.Split ('_');
        string path = PrefabPath; // rootPath[0];
        // for (int i = 0; i < rootPath.Count; i++) {
        //     string prefabName = rootPath[i];
        //     string name = prefabName.Substring (prefabName.LastIndexOf ('/') + 1);
        //     if (filesplit[0].StartsWith (name)) {
        //         path = prefabName;
        //         break;
        //     }
        // }
        if (string.IsNullOrEmpty (path)) {
            UnityEngine.Debug.LogError ("导出的对象命名不规范");
            EditorUtility.DisplayDialog ("警告", "导出的对象命名不规范", "ok");
            return;
        }
        // 生成prefab
        string prefabPath = string.Format ("Assets/Export/{0}/{1}{2}", path, filename, prefabType);
        bool success = false;
        try {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject> (prefabPath);
            if (prefab) {
                if (EditorUtility.DisplayDialog ("警告", "已存在预制体，可能存在同名，是否需要覆盖？", "yes", "cancel")) {
                    PrefabUtility.SaveAsPrefabAsset (selectObj, prefabPath, out success);
                    ExportScript (true, selectObj);
                }
                success = true;
            } else {
                PrefabUtility.SaveAsPrefabAsset (selectObj, prefabPath, out success);
                ExportScript (true, selectObj);
            }
        } catch (System.Exception e) {
            UnityEngine.Debug.LogError ("生成预制体失败,请检查预制体的路径或者检查Resource/Settinggs/文件下是否有MatchPropertyConfig文件，没有请先生成" + e.Message);
            if (!success) {
                EditorUtility.DisplayDialog ("警告", "生成预制体失败,请检查预制体的路径或者检查Resource/Settinggs/文件下是否有MatchPropertyConfig文件，没有请先生成", "ok");
                return;
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="selectTrans"></param>
    /// <param name="export">导出对象的根节点是否为export类型</param>
    private static void findChild (Transform trans, Transform selectTrans, bool export) {
        if (!export && trans.CompareTag("Except")) {
            return;
        }
        string propName = SimplifyName (trans.name);
        if (trans.CompareTag("Prefabs")) //嵌套的预制体
        {
            propertiesStr.Append ($"   protected {propName}Controller _{propName}Ctrl;\n");
            getPropertiesStr.Append ($"        _{propName}Ctrl = new {propName}Controller();\n        _{propName}Ctrl.Init({selectTrans.name}.transform);\n");
            return;
        }
        if (trans.childCount > 0) {
            for (int j = 0; j < trans.childCount; j++) {
                // UnityEngine.Debug.LogError ("c trans.tag=" + trans.tag);
                findChild (trans.GetChild (j), selectTrans, export);
            }
        }
        if (export && !trans.CompareTag("Export")) {
            return;
        }
        /*
        "Slider", "Button", "Toggle", "ScrollRect", 
        "Image", "Animator", "Canvas", 
        "LocalizeTextMeshProUGUI", "Text",
        */
        
        int uniqueValue;
        if (uniqueNames.TryGetValue (propName, out uniqueValue)) {
            uniqueValue = uniqueNames[propName] += 1;
        } else {
            uniqueValue = uniqueNames[propName] = 0;
        }
        if (uniqueValue > 0) {
            propName = $"{propName}_{uniqueValue}";
        }
        string menType = "btn";
        Component comp = trans.GetComponent<Button> ();
        if (comp == null) {
            comp = trans.GetComponent<Image> ();
            menType = "img";
        }
        if (comp == null) {
            comp = trans.GetComponent<TMPro.TextMeshProUGUI> ();
            menType = "tmp";
        }
        if (comp == null) {
            comp = trans.GetComponent<Text> ();
            menType = "txt";
        }
        if (comp == null) {
            comp = trans.GetComponent<Animator> ();
            menType = "anim";
        }
        if (comp == null) {
            comp = trans.GetComponent<Slider> ();
            menType = "sld";
        }
        if (comp == null) {
            comp = trans.GetComponent<Toggle> ();
            menType = "tog";
        }
        if (comp == null) {
            comp = trans.GetComponent<InputField> ();
            menType = "inf";
        }
        if (comp == null) {
            comp = trans.GetComponent<ScrollRect> ();
            menType = "scr";
        }
        if (comp == null) {
            comp = trans.GetComponent<Dropdown> ();
            menType = "dpd";
        }
        if (comp == null) {
            comp = trans.GetComponent<Scrollbar> ();
            menType = "scb";
        }
        if (comp == null) {
            comp = trans.GetComponent<Canvas> ();
            menType = "cvs";
        }
        if (comp == null) {
            comp = trans.GetComponent<CanvasGroup> ();
            menType = "cgp";
        }
        if (comp == null) {
            comp = trans.GetComponent<RawImage> ();
            menType = "imgr";
        }
        if (comp == null) {
            comp = trans.GetComponent<GridLayoutGroup> ();
            menType = "grid";
        }
        if (comp == null) {
            comp = trans.GetComponent<VerticalLayoutGroup> ();
            menType = "vlg";
        }
        if (comp == null) {
            comp = trans.GetComponent<HorizontalLayoutGroup> ();
            menType = "hlg";
        }
        if (comp == null) {
            comp = trans.GetComponent<HorizontalOrVerticalLayoutGroup> ();
            menType = "hvlg";
        }

        var fieldName = $"_{menType}{propName}";
        if (comp != null) {
            string className = SimplifyClassName (comp.GetType ()!= typeof(TMPro.TextMeshProUGUI) ? comp.GetType ().ToString (): "DragonPlus.LocalizeTextMeshProUGUI");

            propertiesStr.Append ($"   protected {className} {fieldName};\n");
            //获取组件在预制体中的路径
            string compPath = AnimationUtility.CalculateTransformPath (comp.transform, selectTrans);
            if (!uniquePaths.Contains (compPath)) {
                uniquePaths.Add (compPath);
            } else {
                UnityEngine.Debug.LogError (compPath + "路径重复");
            }
            getPropertiesStr.Append ($"        {fieldName} = transform.Find(\"{compPath}\").GetComponent<{className}>();\n");
            if (comp.GetType () == typeof (Button)) {
                addListenerStr.Append ($"        {fieldName}.onClick.AddListener({propName}Clicked);\n");
//                clickListenStr.Append ($"    public abstract void {propName}Clicked();\n");
                clickListenOverrideStr.Append($"    public void {propName}Clicked()").Append("{}\n");
                removeListenerstr.Append ($"        {fieldName}.onClick.RemoveAllListeners();\n");
            }
        } else {
            if (!export && trans.tag != "Export") {
                UnityEngine.Debug.LogError (comp + "减少不必要的导出");
                return;
            }
            comp = trans.GetComponent<RectTransform> ();
            if (!comp) {
                UnityEngine.Debug.LogError (comp + "不是UI组件");
                return;
            }
            menType = "rtf";
            string className = SimplifyClassName (comp?comp.GetType ().ToString () : trans.GetType ().ToString ());
            propertiesStr.Append ($"   protected {className} {fieldName};\n");
            string compPath = AnimationUtility.CalculateTransformPath (trans, selectTrans);
            if (!uniquePaths.Contains (compPath)) {
                uniquePaths.Add (compPath);
            } else {
                UnityEngine.Debug.LogError (compPath + "路径重复");
            }
            getPropertiesStr.Append ($"        {fieldName} = transform.Find(\"{compPath}\") as {className};\n");
        }

    }
}

public class EditorUtils1 {
    public static void ProcessCommand (string scriptName, string[] parameters) {
        StringBuilder sb = new StringBuilder ();
        foreach (var param in parameters) {
            if (sb.Length > 0)
                sb.Append (" ");
            sb.Append (param);
        }

        var command = scriptName + ".sh " + sb.ToString ();
        UnityEngine.Debug.Log ("Execute command: " + command);
        ProcessCommand ("/bin/sh", command);
        //ProcessScripts(command);
    }

    public static void ProcessScripts (string scriptPath) {
        Process process = new Process ();
        process.EnableRaisingEvents = false;
        process.StartInfo.FileName = "/bin/sh";
        process.StartInfo.Arguments = scriptPath;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardError = true;
        process.OutputDataReceived += new DataReceivedEventHandler (P_OutputDataReceived);
        process.ErrorDataReceived += new DataReceivedEventHandler (P_OutputDataReceived);
        process.Start ();
        process.BeginOutputReadLine ();
        StreamWriter messageStream = process.StandardInput;
    }

    public static void ProcessCommand (string command, string argument) {
        ProcessStartInfo start = new ProcessStartInfo (command);
        start.Arguments = argument;
        start.CreateNoWindow = true;
        start.ErrorDialog = true;
        start.UseShellExecute = false;
        if (start.UseShellExecute) {
            start.RedirectStandardOutput = false;
            start.RedirectStandardError = false;
            start.RedirectStandardInput = false;
        } else {
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;
            start.RedirectStandardInput = true;
            start.StandardOutputEncoding = UTF8Encoding.UTF8;
            start.StandardErrorEncoding = UTF8Encoding.UTF8;

        }
        Process p = Process.Start (start);
        //p.OutputDataReceived += P_OutputDataReceived;
        //p.BeginOutputReadLine();
        //p.BeginErrorReadLine();
        if (!start.UseShellExecute) {
            UnityEngine.Debug.Log (p.StandardOutput.ReadToEnd ());
            UnityEngine.Debug.Log (p.StandardError.ReadToEnd ());
        }
        p.WaitForExit ();
        p.Close ();
    }

    static void P_OutputDataReceived (object sender, DataReceivedEventArgs e) {
        UnityEngine.Debug.Log ("Received?");
        UnityEngine.Debug.Log (sender.GetType ());
        UnityEngine.Debug.Log (e.Data);
    }

}