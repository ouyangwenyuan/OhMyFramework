namespace OhMyFramework.Core
{
    public abstract class ASubModule : ISubModule
    {
        public IQFrameworkContainer FrameworkContainer;
        public void Init(IQFrameworkContainer mContainer)
        {
            FrameworkContainer = mContainer;
            mContainer.Inject(this);
        }

        public virtual void Destroy()
        {
           
        }
        public virtual void Awake(){}

        public virtual void Start(){}

        public virtual  void Update(){}
        
        public virtual  void LateUpdate(){}
        public virtual  void OnApplicationPause(bool isPause){}
    }
}