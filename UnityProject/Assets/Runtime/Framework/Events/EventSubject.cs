
using System;
using System.Collections.Generic;

namespace DragonU3DSDK
{
    public interface ISubject
    {
        void Trigger();
    }

    public interface IEventHandler<Type>
        where Type : class, ISubject
    {
        void OnNotify(Type message);
    }

    class EventFunctor<Type> : IEventHandler<Type>
        where Type : class, ISubject
    {
        Action<Type> functor;
        public EventFunctor(Action<Type> func)
        {
            functor = func;
        }

        public void OnNotify(Type message)
        {
            if (functor != null)
            {
                functor(message);
            }
        }
    }

    public class Subject<Type> : ISubject
        where Type : Subject<Type>, new()
    {
        Type self;
        List<IEventHandler<Type>> handlers;

        public Type Clone()
        {
            Type ret = new Type();
            ret.handlers = new List<IEventHandler<Type>>(this.handlers);
            return ret;
        }

        protected Subject()
        {
            self = (Type)this;
            handlers = new List<IEventHandler<Type>>();
        }

        public bool Subscribe(IEventHandler<Type> handler)
        {
            if (handlers.Contains(handler))
            {
                return true;
            }
            handlers.Add(handler);
            return true;
        }

        public bool Unsubscribe(IEventHandler<Type> handler)
        {
            if (!handlers.Contains(handler))
            {
                return true;
            }
            return handlers.Remove(handler);
        }

        public virtual void Trigger()
        {
            foreach (var handler in handlers)
            {
                handler.OnNotify(self);
            }
        }

        public void Notify(IEventHandler<Type> handler)
        {
            if (handler != null)
            {
                handler.OnNotify(self);
            }
        }

        public virtual void TriggerInMainThread()
        {
            EventSpawner.Instance.TriggerInMainThread(this);
        }
    }

    public class SubjectAggregation
    {
        Dictionary<Type, ISubject> msgTemplates;

        public SubjectAggregation()
        {
            msgTemplates = new Dictionary<Type, ISubject>();
        }

        public IEventHandler<Type> Subscribe<Type>(Action<Type> func)
            where Type : Subject<Type>, new()
        {
            var handler = new EventFunctor<Type>(func);
            Subscribe<Type>(handler);
            return handler;
        }

        public bool Subscribe<Type>(IEventHandler<Type> handler)
            where Type : Subject<Type>, new()
        {
            Type msg = null;
            var key = typeof(Type);
            if (msgTemplates.ContainsKey(key))
            {
                msg = (Type)msgTemplates[key];
            }
            else
            {
                msg = new Type();
                msgTemplates[key] = msg;
            }

            return (msg == null) ? false : ((Type)msg).Subscribe(handler);
        }

        public bool Unsubscribe<Type>(IEventHandler<Type> handler)
            where Type : Subject<Type>, new()
        {
            var key = typeof(Type);
            if (!msgTemplates.ContainsKey(key))
            {
                return true;
            }

            Type msg = (Type)msgTemplates[key];
            return msg.Unsubscribe(handler);
        }

        public Type Trigger<Type>()
            where Type : Subject<Type>, new()
        {
            var key = typeof(Type);
            if (!msgTemplates.ContainsKey(key))
            {
                // return dummy notification
                return new Type();
            }

            var msg = (Type)msgTemplates[key];
            var ret = msg.Clone();
            return ret;
        }
    }

}
