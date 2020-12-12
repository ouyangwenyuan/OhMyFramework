using System.ComponentModel;
using UnityEngine;

//namespace OhMyFramework.Tests
//{
    public partial class SROptions
    {
        private const string categoryUI = "界面";
        private const string categoryCurrency = "背包";
        private const string categoryAd = "广告";
        private const string categoryActivity = "活动";
        private const string categoryFunction = "功能测试";
        private const string categoryProperty = "开关、快捷设置";
        private const string categoryStorage = "存档";
        private const string categoryOther = "状态或其他";
        private const string categoryCrazyTruck = "周末狂欢";
        private const string categoryTest = "";

        private void HideDebugPanel()
        {
//            SRDebug.Instance.HideDebugPanel();
        }

        [Category(categoryUI)]
        public void TestLog()
        {
            Debug.Log("Hello, world!");
        }
    }
//}