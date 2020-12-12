namespace OhMyFramework.Core
{
    interface ISubModule 
    {
        // 游戏启动时
        void Init(IQFrameworkContainer mContainer);


        // 游戏终止
        void Destory();
    }
    
    
    interface IInitable
    {
        // 游戏启动时
        void Init();
        // 游戏终止
        void Release();
    }

    interface IStart
    {
        // 第一帧之前
        void Start();
    }
    
    interface IUpdatable
    {
        // 每帧
        void Update(float deltaTime);
    }

    interface IOnApplicationPause
    {
        void OnApplicationPause(bool pauseStatus);
    }


    interface IGlobal
    {
        void ToGlobal();
    }
    
}