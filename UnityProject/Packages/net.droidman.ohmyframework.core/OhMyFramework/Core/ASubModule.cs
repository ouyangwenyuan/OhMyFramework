namespace OhMyFramework.Core
{
    public abstract class ASubModule : ISubModule
    {
        public IQFrameworkContainer FrameworkContainer;
        public void Init(IQFrameworkContainer mContainer)
        {
            FrameworkContainer = mContainer;
        }

        public void Destory()
        {
           
        }
        public virtual void Awake(){}

        public virtual void Start(){}

        public virtual  void Update(){}
    }
}