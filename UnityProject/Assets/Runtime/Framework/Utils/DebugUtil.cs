using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System;
using System.Text;

namespace DragonU3DSDK
{
    public class DebugUtil
    {

        public static bool LogEnable = true;

        [Conditional("UNITY_EDITOR")]
        static public void Assert(bool test, string assertString)
        {
#if UNITY_EDITOR
            if (!test)
            {
                StackTrace trace = new StackTrace(true);
                StackFrame frame = trace.GetFrame(1);

                string assertInformation;
                assertInformation = "Filename: " + frame.GetFileName() + "\n";
                assertInformation += "Method: " + frame.GetMethod() + "\n";
                assertInformation += "Line: " + frame.GetFileLineNumber();

                UnityEngine.Debug.Break();

                string assertMessage = assertString + "\n\n" + assertInformation;
                if (UnityEditor.EditorUtility.DisplayDialog("Assert!", assertMessage, "OK"))
                {
                    UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(frame.GetFileName(), frame.GetFileLineNumber());
                    UnityEngine.Debug.Log(assertInformation);
                }
            }
#endif
        }

        [Conditional("UNITY_EDITOR")]
        static public void FixMaterialInEditor(GameObject go)
        {
#if UNITY_EDITOR
            var renders = go.GetComponentsInChildren<Renderer>();
            foreach (var render in renders)
            {
                render.material.shader = Shader.Find(render.material.shader.name);
            }
#endif
        }


        public static void Log(object obj, params object[] args)
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (LogEnable)
            {
                string msg = obj == null ? "NULL" : obj.ToString();

                Log(msg, args);
            }
#endif
        }
        
        public static void Log(string str, params object[] args)
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (LogEnable)
            {
                if (args != null && args.Length > 0)
                {
                    str = GetLogString(str, args);
                }
                UnityEngine.Debug.Log("[I]> " + str);
            }
#endif
        }

        public static void LogWarning(object obj, params object[] args)
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (LogEnable)
            {
                string msg = obj == null ? "NULL" : obj.ToString();

                LogWarning(msg, args);
            }
#endif
        }

        public static void LogWarning(string str, params object[] args)
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (LogEnable)
            {
                if (args != null && args.Length > 0)
                {
                    str = GetLogString(str, args);
                }
                UnityEngine.Debug.LogWarning("[W]> " + str);
            }
#endif
        }


        public static void LogError(object obj, params object[] args)
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (LogEnable)
            {
                string msg = obj == null ? "NULL" : obj.ToString();

                LogError(msg, args);
            }
#endif
        }

        public static void LogError(string str, params object[] args)
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (LogEnable)
            {
                if (args != null && args.Length > 0)
                {
                    str = GetLogString(str, args);
                }
                UnityEngine.Debug.LogError("[E]> " + str);
            }
#endif
        }

        private static string GetLogString(string str, params object[] args)
        {
            StringBuilder sb = new StringBuilder();

            DateTime now = DateTime.Now;

            sb.Append(now.ToString("HH:mm:ss.fff", System.Globalization.CultureInfo.CreateSpecificCulture("en-US")));
            sb.Append(" ");

            sb.AppendFormat(str, args);

            return sb.ToString();
        }
        
        //------Colorful Log-----
        public static void LogG(string str, params object[] args)
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (LogEnable)
            {
                if (args != null && args.Length > 0)
                {
                    str = GetLogString(str, args);
                }
                UnityEngine.Debug.Log($"<color=#00BB00>[I]> {str}</color>" );
            }
#endif
        }
        public static void LogP(string str, params object[] args)
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (LogEnable)
            {
                if (args != null && args.Length > 0)
                {
                    str = GetLogString(str, args);
                }
                UnityEngine.Debug.Log($"<color=#ff99ff>[I]> {str}</color>" );
            }
#endif
        }
        public static void LogY(string str, params object[] args)
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (LogEnable)
            {
                if (args != null && args.Length > 0)
                {
                    str = GetLogString(str, args);
                }
                UnityEngine.Debug.Log($"<color=#FFFF66>[I]> {str}</color>" );
            }
#endif
        }
        
        //--------Project Transfer Debug---------
        public static void LogPjTransTip(string msg,params object[] args)
        {
#if DEBUG || DEVELOPMENT_BUILD
            if (LogEnable)
            {
                if (args != null && args.Length > 0)
                {
                    msg = GetLogString(msg, args);
                }
                UnityEngine.Debug.Log("[project迁移] -->" + msg);
            }
#endif
        }
    }
}

