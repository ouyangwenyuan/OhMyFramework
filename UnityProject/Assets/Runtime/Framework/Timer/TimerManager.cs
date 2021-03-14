using UnityEngine;
using System;
using System.Collections.Generic;

namespace DragonU3DSDK
{
    public class TimerManager : Manager<TimerManager>
    {
        List<Action<float>> timerDelegates = new List<Action<float>>();
        List<Action<float>> updateList = new List<Action<float>>();

        public void AddDelegate(Action<float> action)
        {
            if (action != null)
            {
                timerDelegates.Add(action);
            }
        }

        void Update()
        {
            if (timerDelegates.Count > 0)
            {
                var deltaTime = Time.deltaTime;
                updateList.Clear();
                for (int i = 0; i < timerDelegates.Count; i++)
                {
                    updateList.Add(timerDelegates[i]);
                }

                for (int i = 0; i < updateList.Count; i++)
                {
                    updateList[i]?.Invoke(deltaTime);
                }
            }
        }
    }
}
