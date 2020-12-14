using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace OhMyFramework.Core
{
    
    public class ModuleManager
    {
        
        private static IQFrameworkContainer _container = null;

        public static IQFrameworkContainer Container
        {
            get
            {
                if (_container != null) return _container;
                _container = new QFrameworkContainer();
                InitializeContainer(_container);
                return _container;
            }
            set { _container = value; }
        }
        
        static List<ISubModule> _subModules = new List<ISubModule>();
        /// <summary>
        /// 获取SubModule
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetModule<T>() 
        {
            foreach (var item in _subModules)
            {
                if (item.GetType() == typeof(T))
                {
                    return (T) item;
                }
            }

            return default(T);
        }
        
        private static void InitializeContainer(IQFrameworkContainer container)
        {
            container.RegisterInstance(container);
            var viewTypes = FrameworkEngine.GetDerivedTypes<ISubModule>(false, false).ToArray();
            Debug.Log($"viewTypes = {viewTypes.Length} ");
            foreach (var view in viewTypes)
            {
                var viewInstance = Activator.CreateInstance(view) as ISubModule;
                if (viewInstance == null) continue;
                container.RegisterInstance(viewInstance, view.Name, false);
                container.RegisterInstance(viewInstance.GetType(), viewInstance);
            }
            container.InjectAll();
            
            _subModules = Container.ResolveAll<ISubModule>().ToList();
            Debug.Log($"_subModules = {_subModules.Count} ");

            foreach (var view in _subModules)
            {
                view.Init(Container);
            }

            foreach (var view in _subModules)
            {
                container.Inject(view);
            }
        }
        public void Awake()
        {
             _subModules.FindAll(x=> x is ASubModule).ForEach(x=>(x as ASubModule)?.Awake());
        }

        public void FixedUpdate()
        {
            
        }

        public void LateUpdate()
        {
            
        }

        public void OnApplicationPause(bool pauseStatus)
        {
            
        }

        public void OnApplicationQuit()
        {
            
        }

        public void OnDestroy()
        {
            _subModules.FindAll(x=> x is ASubModule).ForEach(x=>x.Destory());
            _container.Dispose();
        }

        public void Start()
        {
            _subModules.FindAll(x=> x is ASubModule).ForEach(x=>(x as ASubModule)?.Start());
        }

        public void Update()
        {
            _subModules.FindAll(x=> x is ASubModule).ForEach(x=>(x as ASubModule)?.Update());
        }
        
        
    }
}