/*-------------------------------------------------------------------------------------------
// Copyright (C) 2020 成都，天龙互娱
//
// 模块名：AsyncTask
// 创建日期：2020-7-1
// 创建者：qibo.li
// 模块描述：统一异步处理模块
//-------------------------------------------------------------------------------------------*/
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace DragonU3DSDK.AsyncTask
{
	/*
	public class AsyncTaskManager : MonoBehaviour
	{
		private static AsyncTaskManager m_Instance;
		public static AsyncTaskManager Instance
		{
			get
			{
				if (m_Instance == null)
				{
					m_Instance = (AsyncTaskManager)FindObjectOfType(typeof(AsyncTaskManager));

					if (m_Instance == null)
					{
						var singletonObject = new GameObject();
						m_Instance = singletonObject.AddComponent<AsyncTaskManager>();
						singletonObject.name = " AsyncTaskManager(Singleton)";
						singletonObject.hideFlags = HideFlags.HideInHierarchy;
						DontDestroyOnLoad(singletonObject);
					}
				}
				return m_Instance;
			}
		}

		private System.Object _lock = new System.Object();
		private Queue<Task> tasks = new Queue<Task>();
		private Queue<Task> finishs = new Queue<Task>();

		private float idleTime = .0f;

		private bool terminate = true;
		private Thread thread = null;
		private Stopwatch stopWatch = new Stopwatch();

		private void StartThread()
		{
			terminate = false;
			thread = new Thread(() =>
			{
				while (!terminate)
				{
					stopWatch.Restart();

					Task task = null;
					lock (_lock)
					{
						if (tasks.Count > 0)
						{
							task = tasks.Peek();
						}
					}
					if(null != task)
                    {
                        try
                        {
							task.Execute();
						}
						catch(System.Exception e)
                        {
							task.exception = e;
						}
					}
					lock (_lock)
					{
						if (null != task)
						{
							tasks.Dequeue();
							finishs.Enqueue(task);
						}
					}

					int time = (int)(16 - stopWatch.ElapsedMilliseconds);
					if (time <= 0)
						time = 1;
					else if (time >= 16)
						time = 16;
					Thread.Sleep(time);
				}
			});
			thread.Start();
			thread.Priority = System.Threading.ThreadPriority.AboveNormal;

			DebugUtil.Log("StartThread");
		}

		private void StopThread()
		{
			terminate = true;
			thread.Abort();
			thread = null;

			DebugUtil.Log("StopThread");
		}

		private void Update()
		{
			//stop
			if (!terminate)
			{
				if (tasks.Count == 0 && finishs.Count == 0)
				{
					idleTime += Time.deltaTime;
				}
				else
				{
					idleTime = .0f;
				}

				if (idleTime >= 300.0f)
				{
					StopThread();
				}
			}

			//finish
			Task fnish = null;
			lock (_lock)
			{
				if (finishs.Count > 0)
				{
					fnish = finishs.Dequeue();
				}
			}
			if(null != fnish)
            {
	            try
	            {
		            fnish.OnFinish();
	            }
	            catch(System.Exception e)
	            {
		            DebugUtil.LogError(e.ToString());
	            }
			}
		}

		public void AddTask(Task item)
		{
			lock (_lock)
			{
				tasks.Enqueue(item);
			}
			if (terminate)
			{
				StartThread();
			}
		}
	}
	*/
}


