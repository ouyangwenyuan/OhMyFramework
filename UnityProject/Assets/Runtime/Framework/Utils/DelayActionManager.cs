using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DragonU3DSDK
{

    public class DelayActionManager
    {
        private static DelayActionManager instance = null;
        private static readonly object syslock = new object();


        public static DelayActionManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syslock)
                    {
                        if (instance == null)
                        {
                            instance = new DelayActionManager();
                        }
                    }
                }
                return instance;
            }
        }

        private ConcurrentDictionary<string, long> m_MainThreadTimeRecord = null;
        private ConcurrentDictionary<string, Action> m_MainThreadActionRecord = null;

        private static float CD = 0.1f;

        private float current_time = 0;

        //private SortedList<long, Action> m_MainThreadActionRecord = null;

        private DelayActionManager()
        {
            m_MainThreadTimeRecord = new ConcurrentDictionary<string, long>();
            m_MainThreadActionRecord = new ConcurrentDictionary<string, Action>();
            //m_MainThreadActionRecord = new SortedList<long, Action>();

            TimerManager.Instance.AddDelegate(Update);
        }

        void Update(float delta)
        {
            if (current_time < CD)
            {
                current_time += delta;
                return;
            }

            List<string> removedKeys = new List<string>();
            foreach (string key in m_MainThreadTimeRecord.Keys)
            {
                DebugUtil.Log("DelayActionManager: Key {0}, CurrentTime {1} , ExeTime {2}", key, Utils.GetTimeStamp(), m_MainThreadTimeRecord[key]);
                if (Utils.GetTimeStamp() < m_MainThreadTimeRecord[key])
                {
                    DebugUtil.Log("DelayActionManager: not meet time, skip");
                    continue;
                }

                DebugUtil.Log("DelayActionManager: Task will execute: key {0}", key);

                Action action = null;
                if (m_MainThreadActionRecord.TryGetValue(key, out action))
                {
                    DebugUtil.Log("DelayActionManager: Task executing: key {0}", key);
                    action?.Invoke();
                }
                removedKeys.Add(key);
            }

            foreach (string key in removedKeys)
            {
                long time;
                Action a;
                m_MainThreadTimeRecord.TryRemove(key, out time);
                m_MainThreadActionRecord.TryRemove(key, out a);
            }
            current_time = 0;
        }


        /// <summary>
        /// 防抖，但在主线程完成逻辑，避免部分action逻辑无法在非主线程调用而报错
        /// </summary>
        /// <param name="key">任务key,不可为空</param>
        /// <param name="timeMs">延迟timesMs后执行。 在此期间如果再次调用，则重新计时</param>
        /// <param name="action"></param>
        public void DebounceInMainThread(string key, int timeMs, Action action)
        {
            if (action == null || string.IsNullOrEmpty(key))
            {
                return;
            }

            if (timeMs <= 0)
            {
                DebugUtil.Log("DebounceInMainThread：Task scheduled: timeMs < 0, execute immediately.  timeMs = {0}", m_MainThreadTimeRecord[key]);
                action();
                return;
            }

            m_MainThreadTimeRecord[key] = Utils.GetTimeStamp() + timeMs;
            m_MainThreadActionRecord[key] = action;

            DebugUtil.Log("DebounceInMainThread：Task scheduled: it will execute at {0}", m_MainThreadTimeRecord[key]);
        }
    }
}
