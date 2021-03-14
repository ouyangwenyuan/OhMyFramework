
using UnityEngine;
using System;
using System.IO;
using DragonU3DSDK;
using UnityEngine.U2D;
using UnityEngine.UI;
using DragonU3DSDK.Asset;


public partial class UIRoot : Manager<UIRoot>
{
    public Canvas mRootCanvas;
    // 所有UI的根节点
    public GameObject mRoot;
    public Camera mUICamera;
    public GameObject worldRoot;
    public GameObject guideRoot;
    public GameObject cookRoot;
    
    public enum ECanvasScaler
    { 
        S1365_768,
        S1334_750,
    }

    // 缓存UI摄像机的6个裁剪面
    public Plane[] CameraPlanes { private set; get; }

    // Loading节点
//    public GameObject mUILoading;

    // 尝试修复花屏的问题
    bool isDirty = false;
    Font dirtyFont = null;

    private Canvas _canvas;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (mUICamera != null)
        {
            CameraPlanes = GeometryUtility.CalculateFrustumPlanes(mUICamera);
        }

        Font.textureRebuilt += delegate (Font font1)
        {
            isDirty = true;
            dirtyFont = font1;
        };

        _canvas = mRoot.GetComponent<Canvas>();
//        mUILoading.GetOrCreateComponent<SliderZero>();
    }
    

    private void LateUpdate()
    {
        if (isDirty)
        {
            isDirty = false;
            foreach (Text text in FindObjectsOfType<Text>())
            {
                if (text.font == dirtyFont)
                {
                    text.FontTextureChanged();
                }
            }
            dirtyFont = null;
        }
    }

    /// <summary>
    /// Creates the window.
    /// </summary>
    /// <returns>The window.</returns>
    /// <param name="windowName">UI预设名</param>
    /// <param name="type">UI类型</param>
    /// <param name="layer">UI层级</param>
    public UIWindow CreateWindow(string windowName, UIWindowType type, UIWindowLayer layer = UIWindowLayer.Normal)
    {
        GameObject uiPrefab = LoadPrefab(windowName);
        if (uiPrefab == null)
        {
            Debug.LogError($"{GetType()}.CreateWindow, cannot find window resource : {windowName}");
            return null;
        }

        UIWindow window = null;
        GameObject obj = Instantiate(uiPrefab, mRoot.transform, false) as GameObject;
        string[] dirs = windowName.Split('/');
        string className = dirs[dirs.Length - 1] + "Controller";
        window = obj.AddComponent(Type.GetType(className)) as UIWindow;
        if (window == null)
        {
            Debug.LogError($"Cant find UIWindow: {className}, check the name or remove any outter namespace.");
            return null;
        }
        
        return window;
    }

    GameObject LoadObj(string windowName,string path = "")
    {
        GameObject uiPrefab = null;
        uiPrefab = ResourcesManager.Instance.LoadResource<GameObject>(Path.Combine(path, windowName));
        return uiPrefab;
    }

    private GameObject LoadPrefab(string wName)
    {
        string windowName = wName;
        GameObject uiPrefab = null;
//        if (CommonUtils.IsLE_16_10()) //TODO pad size
//        {
//            uiPrefab = LoadObj(windowName + "_Pad");
//            if (uiPrefab == null)// 增加一层兼容。如果ab包里加载不到，就去Resources目录里加载
//            {
//                uiPrefab = Resources.Load<GameObject>(Path.Combine("Loading",windowName + "_Pad"));
//            }
//        }

        if (uiPrefab == null)
        {
            uiPrefab = LoadObj(windowName);
            if (uiPrefab == null) // 增加一层兼容。如果ab包里加载不到，就去Resources目录里加载
            {
                uiPrefab = Resources.Load<GameObject>(Path.Combine("Loading",windowName));
            }
        }

        return uiPrefab;
    }
    

    public Vector2 GetScreenCanvasScale()
    {
        var rectTransform = mRoot.GetComponent<RectTransform>();
        var screenSize = new Vector2(Screen.width, Screen.height);
        return new Vector2(screenSize.x / rectTransform.rect.width, screenSize.y / rectTransform.rect.height);
    }

    public void EnableTouch(bool b)
    {
        var cg = mRootCanvas.GetComponent<CanvasGroup>();
        cg.interactable = b;
        cg.blocksRaycasts = b;
    }

    public void SwitchCanvasScaler(ECanvasScaler scaler)
    {
        CanvasScaler cs = mRootCanvas.transform.GetComponent<CanvasScaler>();
        if (cs != null)
        {
            switch (scaler)
            {
                case ECanvasScaler.S1365_768:
                    cs.referenceResolution = new Vector2(1365, 768);
                    break;
                case ECanvasScaler.S1334_750:
                default:
                    cs.referenceResolution = new Vector2(1334, 750);
                    break;
            }
        }
    }
}