using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OhMyFramework.Core
{
    public class FrameworkEngine
    {
        private static bool isInit;
        
        private static EngineMono _engineMono;
        private static EngineMono EngineMono
        {
            get
            {
                if (_engineMono == null)
                {
                    _engineMono = new GameObject("OhMyFrameworkEngineMono").AddComponent<EngineMono>();
                    Object.DontDestroyOnLoad(_engineMono);
                }
                return _engineMono;
            }
        }
        /// <summary>
        ///  framework 初始化，可以多次调用
        /// </summary>
        public static void Init()
        {
            if (isInit)
            {
                return;
            }

            OmfConfigs.Instance.ResUrl = OmfSettings.Instance.ResUrl;
            OmfConfigs.Instance.IsEnabled = OmfSettings.Instance.IsEnabled;
            LogModule.Log("ResUrl = " + OmfConfigs.Instance.ResUrl +",isenable=" + OmfConfigs.Instance.IsEnabled);
//            var engineMono = new GameObject("OhMyFramework_Engine_DONOTDESTORY").AddComponent<EngineMono>();
//            engineMono.tag = "EngineGameObject";
//            _engineMono = EngineMono;
            
            LogModule.Log("ModuleManager.Container = " + ModuleManager.Container.ToString());
            LogModule.Log("ModuleManager = " + EngineMono.ModuleManager.ToString());
            isInit = true;
        }

        public void UpdateConfigs()
        {
            if (!isInit)
            {
                return;
            }
            LogModule.Log("Update configs");
        }
        
        public static void Stop()
        {
            isInit = false;
            Object.Destroy(_engineMono.gameObject,0.5f);
        }
        
        static List<Assembly> CachedAssemblies { get; set; }
        static FrameworkEngine()
        {
            LogModule.Log("FrameworkEngine constructor");
            CachedAssemblies = new List<Assembly>
            {
                typeof(int).Assembly, typeof(List<>).Assembly
            };

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
//                if (assembly.FullName.StartsWith("Assembly-CSharp") || assembly.FullName.StartsWith("Assembly-CSharp-Editor") || assembly.FullName.Contains("OhMyFramework"))
//                {
                    CachedAssembly(assembly);
//                }
            }
        }

        static void CachedAssembly(Assembly assembly)
        {
            if (CachedAssemblies.Contains(assembly)) return;
            CachedAssemblies.Add(assembly);
        }
        public static IEnumerable<Type> GetDerivedTypes<T>(bool includeAbstract = false, bool includeBase = true)
        {
            LogModule.Log($"GetDerivedTypes {typeof(T)}");
            var type = typeof(T);
            if (includeBase)
                yield return type;
            if (includeAbstract)
            {
                foreach (var t in CachedAssemblies.SelectMany(assembly => assembly
                    .GetTypes()
                    .Where(x => type.IsAssignableFrom(x))))
                {
                    yield return t;
                }
            }
            else
            {
                var items = new List<Type>();
                foreach (var assembly in CachedAssemblies)
                {
                    try
                    {
                        items.AddRange(assembly.GetTypes()
                            .Where(x => type.IsAssignableFrom(x) && !x.IsAbstract));
                    }
                    catch (Exception ex)
                    {
                        LogModule.Log(ex.Message);
                    }
                }

                foreach (var item in items)
                    yield return item;
            }
        }
    }
}