
using UnityEngine;
using System;

namespace DragonU3DSDK
{
    public class UnitySubject : MonoBehaviour
    {
        private SubjectAggregation container;

        void Awake()
        {
            container = new SubjectAggregation();
        }

        public IEventHandler<Type> Subscribe<Type>(Action<Type> func)
            where Type : Subject<Type>, new()
        {
            return container.Subscribe<Type>(func);
        }

        public bool Subscribe<Type>(IEventHandler<Type> handler)
            where Type : Subject<Type>, new()
        {
            return container.Subscribe<Type>(handler);
        }

        public bool Unsubscribe<Type>(IEventHandler<Type> handler)
            where Type : Subject<Type>, new()
        {
            return container.Unsubscribe<Type>(handler);
        }

        public Type Notification<Type>()
            where Type : Subject<Type>, new()
        {
            var notification = container.Trigger<Type>();
            if (notification != null)
            {
                return notification;
            }
            else
            {
                // Dummy notification
                DebugUtil.LogWarning("No one subscribes to the notification: " + typeof(Type).Name);
                return new Type();
            }
        }
    }

    public static class GameObject_UnitySubject
    {
        public static Type Notification<Type>(this GameObject _this)
            where Type : Subject<Type>, new()
        {
            if (_this != null)
            {
                UnitySubject nc = _this.GetComponent<UnitySubject>();
                if (nc)
                {
                    Type ret = nc.Notification<Type>();
                    return ret;
                }
                else
                {
                    DebugUtil.LogWarning("[GameObject_UnitySubject] This game object does not have UnitySubject component. " + _this);
                }
            }
            else
            {
                DebugUtil.LogWarning("[GameObject_UnitySubject] The game object is null.");
            }

            // return a dummy instance as fallback, which should not happened under any circumstances, theoretically
            return new Type();
        }

        public static bool Subscribe<Type>(this GameObject _this, IEventHandler<Type> handler)
            where Type : Subject<Type>, new()
        {
            if (_this != null)
            {
                UnitySubject nc = _this.GetComponent<UnitySubject>();
                if (nc)
                {
                    return nc.Subscribe<Type>(handler);
                }
                else
                {
                    DebugUtil.LogWarning("[GameObject_UnitySubject] This game object does not have UnitySubject component. " + _this);
                    return false;
                }
            }
            else
            {
                DebugUtil.LogWarning("[GameObject_UnitySubject] The game object is null.");
                return false;
            }
        }

        public static bool Unsubscribe<Type>(this GameObject _this, IEventHandler<Type> handler)
            where Type : Subject<Type>, new()
        {
            if (_this != null)
            {
                UnitySubject nc = _this.GetComponent<UnitySubject>();
                if (nc)
                {
                    return nc.Unsubscribe<Type>(handler);
                }
                else
                {
                    DebugUtil.LogWarning("[GameObject_UnitySubject] This game object does not have UnitySubject component. " + _this);
                    return false;
                }
            }
            else
            {
                DebugUtil.LogWarning("[GameObject_UnitySubject] The game object is null.");
                return false;
            }
        }
    }
}
