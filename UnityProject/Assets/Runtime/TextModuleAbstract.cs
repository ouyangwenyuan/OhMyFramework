using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OhMyFramework.Core;
public class TextModuleAbstract : ASubModule
{
    // Start is called before the first frame update
    public override void Start()
    {
        LogModule.Log("Start" + GetType().Name);
    }

    // Update is called once per frame
    public override void Update()
    {
//        LogModule.Log("Update" + GetType().Name);
    }

    public override void OnApplicationPause(bool isPause)
    {
        LogModule.Log($"OnApplicationPause{isPause}" );
    }
    public override void Destroy()
    {
        LogModule.Log("Destroy" ); 
    }
}
