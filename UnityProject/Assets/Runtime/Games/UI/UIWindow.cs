using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

using DragonPlus;
using DragonU3DSDK;

public abstract class UIWindow : MonoBehaviour
{
    public Action UIcloseAction;
    private string mWindowName;

    // 是否播放默认的打开对话框的声音
    public bool isPlayDefaultOpenAudio = true;
    // 是否播放默认的关闭对话框的声音
    public bool isPlayDefaultCloseAudio = true;

    public string WindowName
    {
        set { mWindowName = value; }
        get { return mWindowName; }
    }

    [HideInInspector]
    public UIWindowType m_WindowType;
    //[HideInInspector]
    public UIWindowLayer uiWindowLayer;

    [HideInInspector]
    public bool mIsOpen = false;

    void Awake()
    {
        //todo : it is only a i18n check, remove it..
//        Text[] labels = this.transform.GetComponentsInChildren<Text>(true);
//        foreach (Text label in labels)
//        {
//            //Debug.Log("find label : /"+label.text+"/");
//            if (LocalizationManager.Instance.TryGetLocalizedString(label.text.Trim(), out var result))
//            {
//                label.text = result;
//            }
////            else
////            {
////                if (Regex.IsMatch(label.text, "[a-zA-Z]+"))
////                {
////                    DebugUtil.LogError($"[{name}:{label.name}] cannot get i18n, text:[{label.text}] " +
////                                       $"if it is a local text, use locale text pro instead and add i18n");
////                }
////            }
//            
//            //label.text = DragonPlus.LocalizationManager.Instance.GetLocalizedString(label.text.Trim());
//        }

        PrivateAwake();
    }

    void Start()
    {
    }

    public void OpenWindow(params object[] objs)
    {
        ShowUI();
        OnOpenWindow(objs);
        OpenWindowAudio();
    }

    protected virtual void OpenWindowAudio()
    {
        if (isPlayDefaultOpenAudio)
        {
//            DragonPlus.AudioManager.Instance.PlaySound(SfxNameConst.panel_in);
        }
    }

    void CloseWindowAudio()
    {
        if (isPlayDefaultCloseAudio && mIsOpen)
        {
//            DragonPlus.AudioManager.Instance.PlaySound(SfxNameConst.panel_out);
        }
    }

    public void CloseWindow(bool destroy = false)
    {
        OnCloseWindow(destroy);
        CloseWindowAudio();
        //Global.hideZhezhao();

        if (destroy)
        {
            DestroyUI();
        }
        else
        {
            HideUI();
        }
    }
    public virtual bool OnBack()
    {
//        DragonPlus.AudioManager.Instance.PlaySound(SfxNameConst.button_s);
        return UIManager.Instance.CloseWindow(mWindowName, true);
    }

    public void CloseWindowWithinUIMgr(bool destroy = false)
    {
        UIManager.Instance.CloseWindow(mWindowName, destroy);
    }

    public void ReloadWindow()
    {
        ShowUI();
        OnReloadWindow();
    }

    private void ShowUI()
    {
        mIsOpen = true;
        if (gameObject == null)
        {
            Debug.Log("UI已被销毁:" + WindowName);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    private void HideUI()
    {
        mIsOpen = false;
        gameObject.SetActive(false);
    }

    private void DestroyUI()
    {
        mIsOpen = false;
        Destroy(gameObject);
    }

    #region 子类继承重写
    /// <summary>
    /// 打开界面时调用
    /// </summary>
    /// <param name="objs"></param>
    protected virtual void OnOpenWindow(params object[] objs) { }

    /// <summary>
    /// 关闭界面时调用
    /// </summary>
    protected virtual void OnCloseWindow(bool destroy = false) { }

    /// <summary>
    /// 重新加载界面时调用
    /// </summary>
    protected virtual void OnReloadWindow() { }
    /// <summary>
    /// 私有Awake方法，会在基类Awake执行后调用
    /// </summary>
    public abstract void PrivateAwake();

    public GameObject BindEvent(string target, GameObject par = null, Action<GameObject> action = null, bool playAudio = true)
    {
        if (par == null)
        {
            par = this.gameObject;
        }

        GameObject obj = par?.transform.Find(target)?.gameObject;
        if (obj != null)
        {
            Button button = obj.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener
                    (
                    delegate ()
                    {
                        if (playAudio)
                        {
//                            AudioManager.Instance.PlaySound(SfxNameConst.button_s);
                        }
                        action?.Invoke(obj);
                    }
                    );
            }
        }
        else
        {
            DragonU3DSDK.DebugUtil.LogError("未找到　" + gameObject.name + "/" + target);
        }

        return obj;
    }

    public GameObject FindObj(string path, GameObject par = null)
    {
        if (par == null)
        {
            par = this.gameObject;
        }
        GameObject obj = par.transform.Find(path)?.gameObject;
        return obj;
    }
    #endregion
}
