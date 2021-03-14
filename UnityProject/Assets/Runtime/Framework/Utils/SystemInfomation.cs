using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// using com.adjust.sdk;

namespace DragonU3DSDK
{
    public class SystemInfomation : Manager<SystemInfomation>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnRuntimeLoad()
        {
            DebugUtil.Log("Start Runtimeload SystemInfomation");
            Application.RequestAdvertisingIdentifierAsync(RequestADIDCallback);
        }

        static void RequestADIDCallback(string adid, bool success, string error)
        {
            if (success)
            {
                DebugUtil.Log(" SystemInfomation completed , and trigger the event");
                ADID = adid;
#if DEBUG
                if (!string.IsNullOrEmpty(ADID))
                {
                    //AdSettings.AddTestDevice(ADID);
                }   
#endif
            }
        }


        public static string ADID;
        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public static string GetADID()
        {
            return ADID;
        }
    }

}
