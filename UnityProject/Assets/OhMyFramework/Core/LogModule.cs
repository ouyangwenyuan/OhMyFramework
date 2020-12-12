using System;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OhMyFramework.Core
{
    public enum ELogType
    {
        NORMAL,
        WARRNING,
        ERROR
    }

    public class LogModule : ASubModule, ILogHandler
    {
        public override void Awake()
        {
            base.Awake();
            Debug.Log($"Awake {GetType().Name}");
            
            logTxtPath = Path.Combine(Application.persistentDataPath , "Log");
            if (!Directory.Exists(logTxtPath))
            {
                Directory.CreateDirectory(logTxtPath);
            }
            InitLogTxt();
            
            Application.logMessageReceived += LogCallback;
            AppDomain.CurrentDomain.UnhandledException += OnUnresolvedExceptionHandler;
            originalLogHandler = Debug.unityLogger.logHandler;
            Debug.unityLogger.logHandler = this;
        }

        public override void Start()
        {
            base.Start();
            Debug.Log($"Start {GetType().Name}");
        }
        
        /// <summary>
        /// Log输出开关
        /// </summary>
        public static bool DebugMode = true;

        private static StreamWriter writer;
        private static string logTxtPath ;
        private static string LogStr;
        private ILogHandler originalLogHandler;
        public static string GetCurrTime()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
        }
 
        public static void Log(object str)
        {
            if (DebugMode)
            {
                Debug.Log(GetCurrTime() + "==>" + str);
            }
        }
 
        public static void LogWarning(object str)
        {
            if (DebugMode)
            {
                Debug.LogWarning(GetCurrTime() + "==>" + str);
            }
        }

        public static void LogError(object str)
        {
            if (DebugMode)
            {
                Debug.LogError(GetCurrTime() + "==>" + str);
            }
        }
        public static void Log(string log, string tag = null, ELogType type = ELogType.NORMAL)
        {
            string message = $"{GetCurrTime()}==>oywy/{tag}/{log}";
            switch (type)
            {
                case ELogType.WARRNING:
                    Debug.LogWarning(message); break;
                case ELogType.ERROR:
                    Debug.LogError(message); break;
                default:
                    Debug.Log(message); break;
            }
        }

        /// <summary>
        /// 初始化日志
        /// </summary>
        public static void InitLogTxt()
        {
            FileInfo file = new FileInfo(logTxtPath + "/logTxt.txt");
            if (file.Exists)
            {
                file.Delete();
                file.Refresh();
            }
        }
        
        /// <summary>
        /// 把日志写入文本并保存
        /// </summary>
        /// <param name="str">日志信息</param>
        private static void WriteLogTxt(string str)
        {
            FileInfo file = new FileInfo(logTxtPath + "/logTxt.txt");
            if (!file.Exists)
            {
                writer = file.CreateText();
            }
            else
            {
                writer = file.AppendText();
            }
            writer.WriteLine(str);
            writer.Flush();
            writer.Dispose();
            writer.Close();
        }

        /// <summary>
        /// 捕捉日志
        /// </summary>
        private static void LogCallback(string condition, string stackTrace, LogType type)
        {
            if (DebugMode)
            {//Debug not output log
                return;;
            }
            if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception)
            {
                LogStr = "[" + type + "]" + GetCurrTime() + "==>" + condition + " : " + stackTrace;
                WriteLogTxt(LogStr);
            }
            else
            {
                LogStr = "[" + type + "]" + GetCurrTime() + "==>" + condition;
                WriteLogTxt(LogStr);
            }
        }
        
        private static void OnUnresolvedExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.Log("UnhandledExceptionEventArgs" + e);
            throw new NotImplementedException();
        }

        public void LogFormat(LogType type, Object context, string format, params object[] args)
        {
            if (DebugMode)
            {//Debug not output log
                return;;
            }
            LogStr = "[" + type + "]" + GetCurrTime() + "==>" + string.Format(format,args);
            WriteLogTxt(LogStr);
            originalLogHandler.LogFormat(type, context, format, args);
        }

        public void LogException(Exception exception, Object context)
        {
            if (DebugMode)
            {//Debug not output log
                return;;
            }
            LogStr =  GetCurrTime() + "==>" + exception;
            WriteLogTxt(LogStr);
            
            originalLogHandler.LogException(exception, context);
        }
    }
}