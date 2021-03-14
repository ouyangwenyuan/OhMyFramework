using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OhMyFramework.Core;
public class TestModuleInterface : ISubModule
{

    public void Init(IQFrameworkContainer mContainer)
    {
        LogModule.Log(GetType().Name + "mContainer: " + mContainer);
    }
    
    public void Destroy()
    {
        LogModule.Log("Destory :" + GetType().Name);
    }
    
}
