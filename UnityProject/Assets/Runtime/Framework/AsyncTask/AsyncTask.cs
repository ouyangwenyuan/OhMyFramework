/*-------------------------------------------------------------------------------------------
// Copyright (C) 2020 成都，天龙互娱
//
// 模块名：AsyncTask
// 创建日期：2020-7-1
// 创建者：qibo.li
// 模块描述：统一异步处理模块
//-------------------------------------------------------------------------------------------*/

namespace DragonU3DSDK.AsyncTask
{
    public enum Priority
    {
        Low,
        Normal,
        High
    }

    /// <summary>
    /// 任务
    /// </summary>
    public abstract class Task
    {
        /// <summary>
        /// 执行优先级
        /// </summary>
        public abstract Priority Priority { get; }

        /// <summary>
        /// 是否线程安全
        /// </summary>
        public abstract bool ThreadSafe { get; }

        /// <summary>
        /// 捕获的异常，用于OnFinish判断任务是否有异常
        /// </summary>
        public System.Exception exception = null;

        /// <summary>
        /// 异步执行逻辑，不能访问unity
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// 异步执行完后，主线程调用
        /// </summary>
        public abstract void OnFinish();
    }
}


