using System;
using UnityEngine;
//using System.Net.NetworkInformation;
using System.Collections;
using System.Runtime.InteropServices;
// using DragonU3DSDK.Network.API.Protocol;
// using DragonU3DSDK.Network.API;
using System.Text.RegularExpressions;

namespace DragonU3DSDK
{

    public class Device : Manager<Device>
    {
        // android返回键功能
        Action BackButtonCallbacks { get; set; }
        public void AddBackButtonCallback(Action action)
        {
            BackButtonCallbacks += action;
        }
        public void RemoveBackButtonCallback(Action action)
        {
            BackButtonCallbacks -= action;
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

#if UNITY_ANDROID || UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Escape) && BackButtonCallbacks != null)
            {
                BackButtonCallbacks.Invoke();
            }
#endif
        }

        private void OnDisable()
        {

        }
    }


    public class DeviceHelper
    {
        static string s_OSName = SystemInfo.operatingSystem;
        static string s_Resolution = Screen.currentResolution.ToString();
        static string s_SystemLanguage = Application.systemLanguage.ToString();
        static string s_AppVersion = Application.version;
        static int s_SystemMemorySize = SystemInfo.systemMemorySize;
        static string s_DeviceModel = SystemInfo.deviceModel;
        static string s_ProcessorType = SystemInfo.processorType;
        public static int s_mainOSVersion { get; private set; }
        public static int s_secondaryOSVersion { get; private set; }

        static DeviceHelper()
        {
            string versionPattern;
            if (Application.platform == RuntimePlatform.Android)
            {
                versionPattern = "API-([\\d|\\.]*)";
            }
            else
            {
                versionPattern = "[^\\d]*([\\d|\\.]*)";
            }
            Match match = Regex.Match(s_OSName, versionPattern, RegexOptions.IgnoreCase);
            if (match.Success)
            {
                string osVersion = match.Groups[1].Value;
                string[] versionArray = osVersion.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                s_mainOSVersion = versionArray.Length > 0 ? int.Parse(versionArray[0]) : 0;
                s_secondaryOSVersion = versionArray.Length > 1 ? int.Parse(versionArray[1]) : 0;
            }
        }

        public static string GetDeviceId()
        {
            string deviceId ="";
//             var storageCommon = Storage.StorageManager.Instance.GetStorage<Storage.StorageCommon>();
//             if (storageCommon != null && !string.IsNullOrEmpty(storageCommon.DeviceId))
//             {
//                 deviceId = storageCommon.DeviceId;
//             }
//             else
//             {
// #if UNITY_IOS && !UNITY_EDITOR
//                 var deviceIdKey = Application.identifier + "_DeviceId";
//                 deviceId = DragonNativeBridge.GetDataFromKeyChain(deviceIdKey);
//                 if (string.IsNullOrEmpty(deviceId))
//                 {
//                     deviceId = SystemInfo.deviceUniqueIdentifier;
//                     DragonNativeBridge.SaveDataToKeyChain(deviceIdKey, deviceId);
//                 }
// #else
//                 deviceId = SystemInfo.deviceUniqueIdentifier;
// #endif
//                 if (storageCommon != null)
//                 {
//                     storageCommon.DeviceId = deviceId;
//                 }
//             }
            return deviceId;
        }
        public static string GetAppVersion()
        {
            return s_AppVersion;
        }

        public static long GetTotalMemory()
        {
            return s_SystemMemorySize;
        }

        public static string GetDeviceModel()
        {
            return s_DeviceModel;
        }

        // TODO
        public static string GetRegion()
        {
            return s_SystemLanguage;
        }

        // TODO
        public static string GetCountry()
        {
            return s_SystemLanguage;
        }

        public static string GetOSName()
        {
            return s_OSName;
        }

        public static string GetOSVersion()
        {
            return s_OSName;
        }

        public static string GetCPU()
        {
            return s_ProcessorType;
        }

        public static void CancelNotification(int id)
        {
#if UNITY_ANDROID
            CallActivityFunction("CancelNotification", id.ToString());
#endif
        }
        public static void ScheduleLocalNotification(int id, string title, string message, int seconds, bool sound)
        {
#if UNITY_ANDROID
            CallActivityFunction("ScheduleLocalNotification", id.ToString(), title, message, seconds.ToString());
#endif
        }

        // TODO
        public static string GetTimezone()
        {
            try
            {
                return TimeZoneInfo.Local.StandardName;
            }
            catch
            {
                return "UNKNOWN";
            }
        }

        public static string GetPlatformString()
        {
#if UNITY_IOS
            if (SystemInfo.deviceModel.Contains("iPad"))
            {
                return "Ipad";
            }
            else
            {
                return "Iphone";
            }
#elif UNITY_ANDROID
            return "Google";
#elif UNITY_FACEBOOK
        return "GameRoom";
#elif UNITY_WEBGL
        return "Facebook";
#else
        return "unity_editor";
#endif

        }

        private static void CallActivityFunction(string methodName, params object[] args)
        {
#if UNITY_ANDROID

            if (Application.platform != RuntimePlatform.Android)
            {
                return;
            }

            try
            {

                AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
                jo.Call(methodName, args);
                //jo.Call("runOnUiThread", new AndroidJavaRunnable(() => { jo.Call(methodName, args); }));

            }
            catch (System.Exception ex)
            {
                DebugUtil.LogWarning(ex.Message);
            }
#endif
        }

        public enum NetworkStatus
        {
            Unknown = 0,
            NoNetwork = 1,
            Cellular = 2,
            Wifi = 3
        };

        // public static NetworkStatus GetNetworkStatus()
        // {
        //     switch (APIManager.Instance.GetNetworkStatus)
        //     {
        //         case NetworkReachability.ReachableViaCarrierDataNetwork:
        //             return NetworkStatus.Cellular;
        //         case NetworkReachability.ReachableViaLocalAreaNetwork:
        //             return NetworkStatus.Wifi;
        //         case NetworkReachability.NotReachable:
        //             return NetworkStatus.NoNetwork;
        //         default:
        //             return NetworkStatus.Unknown;
        //     }
        // }

        // public static string GetDeviceType()
        // {
        //     return DragonNativeBridge.getDeviceType();
        // }

        public static bool IsNative()
        {
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID || UNITY_FACEBOOK
            return true;
#else
        return false;
#endif
        }

        public static bool IsIOSOrAndroid()
        {
#if UNITY_EDITOR || UNITY_IOS || UNITY_ANDROID
            return true;
#else
        return false;
#endif
        }

        public static string GetSchemeParams()
        {
#if UNITY_IOS || UNITY_ANDROID
            //if(Dlugin.SDK.GetInstance().osService != null)
            //{
            //    return Dlugin.SDK.GetInstance().osService.GetUnprocessedURLRequest();
            //}
#endif
            return "";
        }

//         public static DragonU3DSDK.Network.API.Protocol.Platform GetPlatform()
//         {
// #if UNITY_EDITOR
//             return Platform.UnityEditor;
// #elif UNITY_IOS
//             return Platform.Ios;
// #elif UNITY_ANDROID
//             return Platform.Google;
// #elif UNITY_FACEBOOK
//             return Platform.Facebook;
// #elif UNITY_WEBGL
//             return Platform.Facebook;
// #else
//             return Platform.Wp8;
// #endif
//         }

        public static ulong CurrentTimeMillis()
        {
            System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            return (ulong)(System.DateTime.UtcNow - epochStart).TotalMilliseconds;
        }

        public static string GetResolution()
        {
            return s_Resolution;
        }

        public static string GetLanguage()
        {
            return s_SystemLanguage;
        }

        public static string GetLocalIp()
        {
            return "0.0.0.0";
        }

        public static bool IsFullScreenIOS()
        {
#if UNITY_IOS && !UNITY_EDITOR
        var generation = UnityEngine.iOS.Device.generation;
        if (generation == UnityEngine.iOS.DeviceGeneration.iPhoneX ||
            generation == UnityEngine.iOS.DeviceGeneration.iPhoneXR ||
            generation == UnityEngine.iOS.DeviceGeneration.iPhoneXS ||
            generation == UnityEngine.iOS.DeviceGeneration.iPhoneXSMax ||
            generation == UnityEngine.iOS.DeviceGeneration.iPhone11 ||
            generation == UnityEngine.iOS.DeviceGeneration.iPhone11Pro ||
            generation == UnityEngine.iOS.DeviceGeneration.iPhone11ProMax ||
            generation == UnityEngine.iOS.DeviceGeneration.iPhoneUnknown)
        {
            return true;
        }
#endif
            return false;
        }

        // 判断宽屏设备
        public static bool IsWideScreenDevice()
        {
            return ((float)Screen.width / Screen.height <= 1.5f);
        }

        /// <summary>
        /// 获取屏幕宽高比
        /// </summary>
        /// <returns></returns>
        public static float GetScreenWHRate()
        {
            return (float)Screen.width / Screen.height;
        }
    }
}


