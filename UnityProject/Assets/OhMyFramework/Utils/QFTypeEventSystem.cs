/****************************************************************************
 * Copyright (c) 2017 ~ 2020 liangxie
 * 
 * http://qframework.io
 * https://github.com/liangxiegame/QFramework
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 ****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;

namespace OhMyFramework.Utils
{
    public interface ITypeEventSystem : IDisposable
    {
        IDisposable RegisterEvent<T>(Action<T> onReceive);
        void UnRegisterEvent<T>(Action<T> onReceive);

        void SendEvent<T>() where T : new();

        void SendEvent<T>(T e);
        void Clear();
    }

    
    public class TypeEventUnregister<T> : IDisposable
    {
        public Action<T> OnReceive; 
            
        public void Dispose()
        {
            TypeEventSystem.UnRegister(OnReceive);
        }
    }

    public class TypeEventSystem : ITypeEventSystem
    {
        /// <summary>
        /// 接口 只负责存储在字典中
        /// </summary>
        interface IRegisterations : IDisposable
        {

        }


        /// <summary>
        /// 多个注册
        /// </summary>
        class Registerations<T> : IRegisterations
        {
            /// <summary>
            /// 因为委托本身就可以一对多注册
            /// </summary>
            public Action<T> OnReceives = obj => { };

            public void Dispose()
            {
                OnReceives = null;
            }
        }

        /// <summary>
        /// 全局注册事件
        /// </summary>
        private static readonly ITypeEventSystem mGlobalEventSystem = new TypeEventSystem();

        /// <summary>
        /// 字典池
        /// </summary>
        private Dictionary<Type, IRegisterations> mTypeEventDict = new Dictionary<Type, IRegisterations>(8);//DictionaryPool<Type, IRegisterations>.Get();

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="onReceive"></param>
        /// <typeparam name="T"></typeparam>
        public static IDisposable Register<T>(System.Action<T> onReceive)
        {
            return mGlobalEventSystem.RegisterEvent<T>(onReceive);
        }

        /// <summary>
        /// 注销事件
        /// </summary>
        /// <param name="onReceive"></param>
        /// <typeparam name="T"></typeparam>
        public static void UnRegister<T>(System.Action<T> onReceive)
        {
            mGlobalEventSystem.UnRegisterEvent<T>(onReceive);
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="t"></param>
        /// <typeparam name="T"></typeparam>
        public static void Send<T>(T t)
        {
            mGlobalEventSystem.SendEvent<T>(t);
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void Send<T>() where T : new()
        {
            mGlobalEventSystem.SendEvent<T>();
        }


        public IDisposable RegisterEvent<T>(Action<T> onReceive)
        {
            var type = typeof(T);

            IRegisterations registerations = null;

            if (mTypeEventDict.TryGetValue(type, out registerations))
            {
                var reg = registerations as Registerations<T>;
                reg.OnReceives += onReceive;
            }
            else
            {
                var reg = new Registerations<T>();
                reg.OnReceives += onReceive;
                mTypeEventDict.Add(type, reg);
            }

            return new TypeEventUnregister<T> {OnReceive = onReceive};
        }

        public void UnRegisterEvent<T>(Action<T> onReceive)
        {
            var type = typeof(T);

            IRegisterations registerations = null;

            if (mTypeEventDict.TryGetValue(type, out registerations))
            {
                var reg = registerations as Registerations<T>;
                reg.OnReceives -= onReceive;
            }
        }

        public void SendEvent<T>() where T : new()
        {
            var type = typeof(T);

            IRegisterations registerations = null;

            if (mTypeEventDict.TryGetValue(type, out registerations))
            {
                var reg = registerations as Registerations<T>;
                reg.OnReceives(new T());
            }
        }

        public void SendEvent<T>(T e)
        {
            var type = typeof(T);

            IRegisterations registerations = null;

            if (mTypeEventDict.TryGetValue(type, out registerations))
            {
                var reg = registerations as Registerations<T>;
                reg.OnReceives(e);
            }
        }

        public void Clear()
        {
            foreach (var keyValuePair in mTypeEventDict)
            {
                keyValuePair.Value.Dispose();
            }
            
            mTypeEventDict.Clear();
        }

        public void Dispose()
        {

        }
    }

    public interface IDisposableList : IDisposable
    {
        void Add(IDisposable disposable);
    }

    public class DisposableList : IDisposableList
    {
        List<IDisposable> mDisposableList = new List<IDisposable>(); //ListPool<IDisposable>.Get();

        public void Add(IDisposable disposable)
        {
            mDisposableList.Add(disposable);
        }

        public void Dispose()
        {
            foreach (var disposable in mDisposableList)
            {
                disposable.Dispose();
            }

            mDisposableList.Clear();//.Release2Pool();
            mDisposableList = null;
        }
    }

    public static class DisposableExtensions
    {
        public static void AddTo(this IDisposable self, IDisposableList component)
        {
            component.Add(self);
        }
    }
}