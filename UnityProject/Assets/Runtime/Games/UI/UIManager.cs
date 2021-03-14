using System;
using System.Collections.Generic;
using System.Linq;
using DragonU3DSDK;
using UnityEngine;

public partial class UIManager : Manager<UIManager>
{
    public Dictionary<string, UIWindow> mMemoryWindows = new Dictionary<string, UIWindow>();
    public Stack<UIWindow> mStackWindows = new Stack<UIWindow>();

    private Dictionary<int, LinkedList<UIWindow>> _layerList = new Dictionary<int, LinkedList<UIWindow>>(16);

    public void Init()
    {
        _AllWindowMeta();
    }

    /// <summary>
    /// 打开一个UI
    /// UIManager.Instance.OpenHomeWindow("Common/GameUI")
    /// </summary>
    public UIWindow OpenHomeWindow(string path, params object[] objs)
    {
        string uipath = path;
        UIWindow window = null;
        //if (windowType != UIWindowType.Fixed && ConflictDialogManager.Instance.CheckConflict(uipath, mMemoryWindows))
        //{
        //    return window;
        //}
        WindowInfo wInfo;
        if (!_windowsInfo.ContainsKey(uipath))
        {
            //if (_windowsInfo.TryGetValue(uipath, out wInfo))
            {
                Debug.LogError($"Create ui fail: {uipath}, no windows info inited");
                return window;
            }
        }
        wInfo = _windowsInfo[uipath];
        var windowType = wInfo.windowType;
        var uiWindowLayer = wInfo.windowLayer;

        if (!mMemoryWindows.TryGetValue(uipath, out window))
        {
            window = UIRoot.Instance.CreateWindow(uipath, windowType);
            if (window != null)
            {
                mMemoryWindows[uipath] = window;
                window.WindowName = uipath;
                window.m_WindowType = windowType;
                window.uiWindowLayer = uiWindowLayer;
            }
            else
            {
                Debug.LogError("Create ui fail: " + uipath);
                return window;
            }
        }

        if (windowType != UIWindowType.Fixed && !mMemoryWindows.TryGetValue(uipath, out window))//不是常显UI
        {
            if (!mStackWindows.Contains(window))// 不在打开队列里
            {
                if (windowType == UIWindowType.Normal)//对于Normal类型的UI，需要在打开的时候，将前面打开的UI关闭掉
                {
                    if (mStackWindows.Count > 0)
                    {
                        UIWindow stackWindow = mStackWindows.Peek();
                        if (stackWindow != null)
                        {
                            //List<string> friends = ConflictDialogManager.Instance.GetFrinends(uipath);
                            //if (!friends.Contains(stackWindow.WindowName))//如果不在白名单里, 则关闭
                            {
                                stackWindow.CloseWindowWithinUIMgr(true);
                            }

                        }
                    }
                }
                mStackWindows.Push(window);//添加到打开队列
            }
            else
            {
                if (mStackWindows.Count > 0)
                {
                    try
                    {
                        while (!mStackWindows.Peek().Equals(window))
                        {
                            mStackWindows.Peek().CloseWindowWithinUIMgr(true);
                        }
                    }
                    catch (Exception e)
                    { 
                        Debug.LogError(e.Message);
                    }

                }
            }
        }
        else
        {
            //常显UI，单独处理
        }

        LayerListInsertWindow(window);
        window.OpenWindow(objs);
        return window;
    }

    public UIWindow OpenCookingWindow(string uipath, UIWindowType windowType = UIWindowType.PopupTip, params object[] objs){
        UIWindowLayer uiWindowLayer = UIWindowLayer.Tips;
        return OpenCookingWindow(uipath, uiWindowLayer, windowType, objs);
    }
    public UIWindow OpenCookingWindow(string uipath, UIWindowLayer uiWindowLayer,  UIWindowType windowType = UIWindowType.PopupTip,params object[] objs)
    {
        UIWindow window = null;
        //if (windowType != UIWindowType.Fixed && ConflictDialogManager.Instance.CheckConflict(uipath, mMemoryWindows))
        //{
        //    return window;
        //}
        if (!mMemoryWindows.TryGetValue(uipath, out window))
        {
            window = UIRoot.Instance.CreateWindow(uipath, windowType);
            if (window != null)
            {
                mMemoryWindows[uipath] = window;
                window.WindowName = uipath;
                window.m_WindowType = windowType;
                window.uiWindowLayer = uiWindowLayer;
            }
            else
            {
                Debug.LogError("Create ui fail: " + uipath);
                return window;
            }
        }

        if (windowType != UIWindowType.Fixed)//不是常显UI
        {
            if (!mStackWindows.Contains(window))// 不在打开队列里
            {
                if (windowType == UIWindowType.Normal)//对于Normal类型的UI，需要在打开的时候，将前面打开的UI关闭掉
                {
                    if (mStackWindows.Count > 0)
                    {
                        UIWindow stackWindow = null;
                        try
                        {
                            stackWindow = mStackWindows.Peek();
                        }
                        catch (System.Exception e)
                        {
                            Debug.Log(e.Message);
                        }

                        if (stackWindow != null)
                        {
//                            List<string> friends = DragonPlus.ConflictDialogManager.Instance.GetFrinends(uipath);
//                            if (!friends.Contains(stackWindow.WindowName))//如果不在白名单里, 则关闭
//                            {
//                                stackWindow.CloseWindowWithinUIMgr(true);
//                            }
                        }
                    }
                }
                mStackWindows.Push(window);//添加到打开队列
            }
            else
            {
                //在打开队列里，从队列里移除所有挡住该UI的其他UI
                if (mStackWindows.Count > 0)
                {
                    while (!mStackWindows.Peek().Equals(window))
                    {
                        mStackWindows.Peek().CloseWindowWithinUIMgr(true);
                        mStackWindows.Pop();
                    }
                }
            }
        }
        else
        {
            //常显UI，单独处理
        }
        if(uiWindowLayer != UIWindowLayer.None){
            LayerListInsertWindow(window);
        }
        window.OpenWindow(objs);
        return window;
    }

    public T OpenCookingWindow<T>(string uipath, UIWindowType windowType = UIWindowType.PopupTip, params object[] objs) where T : UIWindow{
        UIWindowLayer uiWindowLayer =  UIWindowLayer.Tips;
        return OpenCookingWindow<T>(uipath, uiWindowLayer, windowType, objs);
    }

    public T OpenCookingWindow<T>(string uipath, UIWindowLayer uiWindowLayer,
        UIWindowType windowType = UIWindowType.PopupTip, params object[] objs) where T : UIWindow
    {

        UIWindow window = null;
        window = OpenCookingWindow(uipath, uiWindowLayer, windowType, objs);
        if (window == null)
        {
            return default(T);
        }

        return (T) window;
    }

    public bool Back()
    {
        if (mMemoryWindows == null || mMemoryWindows.Count <= 0)
        {
            return false;
        }
        foreach (var key in mMemoryWindows.Keys)
        {
            if (mMemoryWindows[key] != null && mMemoryWindows[key].m_WindowType != UIWindowType.Fixed)
            {
                return mMemoryWindows[key].OnBack();
            }

        }

        return false;
    }

    public bool CloseWindow(string wName, bool destroy = false)
    {
        string windowName = wName;
        bool result = false;
        UIWindow window;
        if (mMemoryWindows.TryGetValue(windowName, out window))
        {
            window.CloseWindow(destroy);
            if (destroy)
            {
                LayerListRemoveWindow(window);
                mMemoryWindows.Remove(windowName);
            }
            if (mStackWindows.Count > 0)
            {
                if (mStackWindows.Peek().Equals(window))
                {
                    mStackWindows.Pop();
                    //只有是普通界面 才刷新 2018.12.20 wwc 注释 原因:购买道具，弹出商店界面，需要关闭后刷新后面的UI，更新道具数量
                    //if (window.m_WindowType == UIWindowType.Normal)
                    //{
                    //2018.12.20.17:25 qu 移除栈顶的已经destroy掉的, 因为前面的窗口有可能destroy了 (窗口关闭有延时, 导致关闭时不一定被pop出去)
                    while (mStackWindows.Count > 0 && !mMemoryWindows.ContainsKey(mStackWindows.Peek().WindowName))
                    {
                        mStackWindows.Pop();
                    }
                    if (mStackWindows.Count > 0)
                    {
                        mStackWindows.Peek().ReloadWindow();
                    }
                    //}
                }
            }
            result = true;
        }
        //var fsName = FSModel.Instance.GetFSWindowName(windowName);
        //if (!fsName.Equals(windowName))
        //{
        //    CloseWindow(fsName, destroy);
        //}
        return result;
    }

    [Obsolete("逻辑中不要使用全关界面，会卡，有些常驻界面没必要销毁，这个函数之后会删除")]
    public void CloseAllWindows(bool destroy = false)
    {
        try
        {
            while (mStackWindows.Count > 0)
            {
                var window = mStackWindows.Peek();
                if (window.m_WindowType == UIWindowType.Fixed) break;
                var windowName = window.WindowName;

                if (mMemoryWindows.TryGetValue(windowName, out window))
                {
                    try
                    {
                        window.CloseWindow(destroy);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                    if (destroy) mMemoryWindows.Remove(windowName);
                }

                mStackWindows.Pop();
            }

            ClearAllWindows();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        
    }

    [Obsolete("逻辑中不要使用全关界面，会卡，有些常驻界面没必要销毁，这个函数之后会删除")]
    public void ClearAllWindows()
    {
        try
        {
            List<string> keys = new List<string>(mMemoryWindows.Keys);
            for (int i = 0, count = keys.Count; i < count; i++)
            {
                if (mMemoryWindows.TryGetValue(keys[i], out var window))
                    window?.CloseWindowWithinUIMgr(true);
            }
            mMemoryWindows.Clear();
            _layerList.Clear();
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        
    }

    public T GetOpenedWindowByPath<T>(string uipath) where T : UIWindow
    {
        if (mMemoryWindows.ContainsKey(uipath))
        {
            var window = (T)(mMemoryWindows[uipath]);
            return window.gameObject.activeSelf ? window : null;
        }
        return default(T);
    }
    
    public UIWindow GetOpenedWindowByPath(string uipath)
    {
        if (mMemoryWindows.ContainsKey(uipath))
        {
            var window = mMemoryWindows[uipath];
            return window.gameObject.activeSelf ? window : null;
        }
        return null;
    }

    void LayerListInsertWindow(UIWindow window)
    {
        try
        {
            if (window != null)
            {
                LinkedList<UIWindow> currentLayerWindows;
                if (_layerList.TryGetValue((int) window.uiWindowLayer, out currentLayerWindows)
                    && currentLayerWindows != null
                    && currentLayerWindows.Count > 0)
                {
                    if (currentLayerWindows.Remove(window))
                    {

                    }

                    currentLayerWindows.AddLast(window);
                    Debug.Log($"{window.WindowName} sibling index is {window.transform.GetSiblingIndex()}");
                    //return;
                }
                else
                {
                    if (currentLayerWindows == null)
                    {
                        currentLayerWindows = new LinkedList<UIWindow>();
                        _layerList.Add((int) window.uiWindowLayer, currentLayerWindows);
                    }

                    currentLayerWindows.AddLast(window);
                }

                int siblingIndex = 5; // 让出loading其他想挂在canvas下的奇怪东西
                LinkedList<UIWindow> layerWindows;
                for (int i = 0; i <= (int) UIWindowLayer.Max; i++)
                {
                    if (_layerList.TryGetValue(i, out layerWindows) && layerWindows != null)
                    {
                        if (layerWindows.Count > 0)
                        {
                            var e = layerWindows.GetEnumerator();
                            while (e.MoveNext())
                            {
                                e.Current?.transform?.SetSiblingIndex(siblingIndex++);
                            }
                        }
                    }
                }

                var index = window.transform.GetSiblingIndex();
                Debug.Log($"{window.WindowName} sibling index is {index}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            Debug.LogError($"{GetType()}.LayerListInsertWindow error, window name = {window?.name}");
        }

    }

    void LayerListRemoveWindow(UIWindow window)
    {
        try
        {
            if (window != null)
            {
                LinkedList<UIWindow> currentLayerWindows;
                if (_layerList.TryGetValue((int)window.uiWindowLayer, out currentLayerWindows))
                {
                    currentLayerWindows?.Remove(window);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        
    }

    public bool GetTopWindow(out UIWindow window, out UIWindowLayer layer, int banLayerMask)
    {
        for (int i = (int)UIWindowLayer.Max; i >= 0; i--)
        {
            if (((1 << i) & banLayerMask) == 0)
            {
                LinkedList<UIWindow> highLayerWindows;
                if (_layerList.TryGetValue(i, out highLayerWindows))
                {
                    if (highLayerWindows.Count > 0)
                    {
                        window = highLayerWindows.Last.Value;
                        if (window != null)
                        {
                            layer = (UIWindowLayer)i;
                            return true;
                        }
                    }
                }
            }
        }

        window = null;
        layer = UIWindowLayer.Max;
        return false;
    }

    public int LayerMask(UIWindowLayer layer)
    {
        return 1 << ((int)layer);
    }

    public void ShowLoading()
    {
//        UIRoot.Instance.mUILoading.SetActive(true);
    }

    public void HideLoading()
    {
//        UIRoot.Instance.mUILoading.SetActive(false);
    }
}
