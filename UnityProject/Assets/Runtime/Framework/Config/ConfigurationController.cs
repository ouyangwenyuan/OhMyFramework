using UnityEngine;

public enum VersionStatus
{
    RELEASE = 0,
    DEBUG = 1,
    LOCAL = 2
}

public class ConfigurationController : ScriptableObject
{
    public static string ConfigurationControllerPath = "Settings/ConfigurationController";

    private static ConfigurationController _instance = null;

    public static ConfigurationController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ConfigurationController>(ConfigurationControllerPath);
                // 注意：因为该副本是存储关键参数的，所以不要自动生成副本并在脚本中指定参数，否则如被反编译将泄漏关键参数
            }
            return _instance;
        }
    }

    public  VersionStatus version = VersionStatus.DEBUG;

    public string APIServerSecret
    {
        get
        {
            if (version == VersionStatus.RELEASE)
            {
                return API_Server_Secret_Release;
            }
            else
            {
                return API_Server_Secret_Beta;
            }
        }
    }

    public string APIServerURL
    {
        get
        {
            if (version == VersionStatus.RELEASE)
            {
                return API_Server_URL_Release;
            }
            else
            {
                return API_Server_URL_Beta;

            }
        }
    }
    public int APIServerTimeout
    {
        get
        {
            if (version == VersionStatus.RELEASE)
            {
                return API_Server_Timeout_Release;
            }
            else
            {
                return API_Server_Timeout_Beta;

            }
        }
    }
    public string APIConfigClassName
    {
        get
        {
            return API_Config_Class_Name;
        }
    }

    public bool SocketIOEnabled
    {
        get {
            return Socket_IO_Enabled;
        }
    }

    public string ResServerURL
    {
        get
        {
            if (version == VersionStatus.RELEASE)
            {
                return Res_Server_URL_Release;
            }
            else
            {
                return Res_Server_URL_Beta;

            }
        }
    }

    public bool BuildAppBundle
    {
        get
        {
            if (version == VersionStatus.RELEASE)
            {
                return ReleaseBuildAppBundle;
            }
            else
            {
                return DebugBuildAppBundle;

            }
        }
    }

    [Space(10)]
    [Header("API Config Class Name")]
    public string API_Config_Class_Name;

    [Space(10)]
    [Header("API Server Prod")]
    public string API_Server_URL_Release;
    public string API_Server_Secret_Release;
    public int API_Server_Timeout_Release = 15;

    [Space(10)]
    [Header("API Server Beta")]
    public string API_Server_URL_Beta;
    public string API_Server_Secret_Beta = "foobar";
    public int API_Server_Timeout_Beta = 15;

    [Space(10)]
    [Header("启动长连接")]
    public bool Socket_IO_Enabled;


    [Space(10)]
    [Header("线上资源服务器地址")]
    public string Res_Server_URL_Release;

    [Space(10)]
    [Header("测试资源服器地址")]
    public string Res_Server_URL_Beta;


    //[Space(10)]
    //[Header("默认语言")]
    //public Language _DefaultLanguage = Language.Chinese;
    [Space(10)]
    [Header("Android Key Store ")]
    public bool AndroidKeyStoreUseConfiguration = false;
    public string AndroidKeyStorePath = "";
    public string AndroidKeyStorePass = "";
    public string AndroidKeyStoreAlias = "";
    public string AndroidKeyStoreAliasPass = "";

    [Space(10)]
    [Header("AES秘钥")]
    public string _AesKey;

    [Space(10)]
    [Header("RC4 Key")]
    public string _RC4Key;

    [Space(10)]
    [Header("Facebook Permissions")]
    public bool kUserPermissionLoginToken = true;
    public bool kUserPermissionBasicInfo = true;
    public bool kUserPermissionPublish = false;
    public bool kUserPermissionFriendList = true;

    [Space(10)]
    [Header("iOS Project Capabilities")]
    public bool iOSAddPushNotification = false;

    [Space(10)]
    [Header("iOS AppleSignIn Capabilities")]
    public bool iOSAddSignInWithApple = false;

    [Space(10)]
    [Header("AES Tool Path")]
    public string AESToolPath = "";

    [Space(10)]
    [Header("更新安装包的地址")]
    public string UPDATE_URL = "";

    [Space(10)]
    [Header("隐私协议地址")]
    public string PrivacyPolicyURL = "";

    [Space(10)]
    [Header("Android Debug是否打AppBundle")]
    public bool DebugBuildAppBundle = false;

    [Space(10)]
    [Header("Android Release是否打AppBundle")]
    public bool ReleaseBuildAppBundle = false;
}
