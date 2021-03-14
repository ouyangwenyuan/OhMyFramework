public enum UIWindowType
{
    Normal, //普通的(打开的时候，关闭其他UI)
    PopupTip,  //弹窗的(打开的时候，不关其他UI)
    Fixed,  //常显UI
}

public enum UIWindowLayer
{
    // 必须连续，数值越大，层级越高, todo 精简层级
    None = 0,
    BackGround, //
    Normal,    //普通层级
    ForeGround, // 
    Tips,    //提示层级
    Notice,  //公告层级
    Currency,   //顶级货币
    Guide,//新手引导层
    Effect,  //特效层级
    Waiting, //菊花层级
    Max    //最高层级


    // Normal = 10,    //普通层级
    // Tips = 1500,    //提示层级
    // Notice = 3000,  //公告层级
    // Effect = 8000,  //特效层级
    // Waiting = 9000, //菊花层级
    // Max = 10000,    //最高层级
}

