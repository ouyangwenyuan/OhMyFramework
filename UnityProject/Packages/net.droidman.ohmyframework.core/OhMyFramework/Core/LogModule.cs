using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OhMyFramework.Core
{
    public class LogModule : ASubModule 
    {
        private const string logDivide = "=>";
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
            
            AppDomain.CurrentDomain.UnhandledException += OnUnresolvedExceptionHandler;
            Application.logMessageReceived += LogCallback;
//            originalLogHandler = Debug.unityLogger.logHandler;
//            Debug.unityLogger.logHandler = this;
        }

        public override void Start()
        {
            base.Start();
            Debug.Log($"Start {GetType().Name}");
        }

        public override void Destroy()
        {
            base.Destroy();
            Application.logMessageReceived -= LogCallback;
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
        
//        public static void Log(string log,  LogType type = LogType.Log)
//        {
//            if (DebugMode)
//            {
//                string message = $"omf{logDivide}{log}";
//                switch (type)
//                {
//                    case LogType.Warning:
//                        Debug.LogWarning(message);
//                        break;
//                    case LogType.Error:
//                    case LogType.Exception:
//                        Debug.LogError(message);
//                        break;
//                    default:
//                        Debug.Log(message);
//                        break;
//                }
//            }
//        }
        public static void Log(string msg,LogType logLevel = LogType.Log, bool releaseLog = false,[CallerFilePath]string file = null,[CallerMemberName]string name = null,[CallerLineNumber]int line = 0) {
            if(Debug.isDebugBuild){
                file = file?.Substring(file.LastIndexOf('/'));
                string message = $"oywy{file}({line}).{name}::{msg}";
                if (logLevel == LogType.Error)
                    Debug.LogError ($"<color=#ff99ff>{message}</color>");
                else if (logLevel == LogType.Warning)
                    Debug.LogWarning ($"<color=#FFFF66>{message}</color>");
                else
                    Debug.Log ($"<color=#00BB00>{message}</color>");
            }
            if(releaseLog){ //打 release 仍可见的log，以LogError输出
                Debug.LogError($"oywy{file}({line}).{name}::{msg}");
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
            {
                //Debug not output log
                return;
            }
            if (type == LogType.Error || type == LogType.Assert || type == LogType.Exception)
            {
                LogStr = GetCurrTime() + logDivide + condition + logDivide + stackTrace;
                WriteLogTxt(LogStr);
            }
        }
        
        private static void OnUnresolvedExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Log("Catch UnhandledExceptionEventArgs" + e);
            WriteLogTxt(e.ToString());
//            throw new NotImplementedException();
        }

//        public void LogFormat(LogType type, Object context, string format, params object[] args)
//        {
//            if (DebugMode)
//            {
//                originalLogHandler.LogFormat(type, context, format, args);
//                return;
//            }
//
//            if (type == LogType.Error)
//            {
//                LogStr = GetCurrTime() + logDivide + string.Format(format,args);
//                WriteLogTxt(LogStr);
//            }
//        }

//        public void LogException(Exception exception, Object context)
//        {
//            if (DebugMode)
//            {
//                originalLogHandler.LogException(exception, context);
//                return;
//            }
//            LogStr =  GetCurrTime() + logDivide + exception.Message;
//            WriteLogTxt(LogStr);
//        }
    }
}