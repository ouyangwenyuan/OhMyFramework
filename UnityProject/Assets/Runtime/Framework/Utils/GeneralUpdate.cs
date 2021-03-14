using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace DragonU3DSDK
{
    public class GeneralUpdate : Manager<GeneralUpdate>
    {
        private List<Action> m_Updates = new List<Action>();

        public void Register(Action action)
        {
            if (action != null)
                m_Updates.Add(action);
        }
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            foreach (Action action in m_Updates)
            {
                action.Invoke();
            }
        }
    }

}
