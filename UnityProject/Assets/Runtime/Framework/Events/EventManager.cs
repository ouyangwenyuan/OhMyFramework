using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DragonU3DSDK
{
    public class EventManager : SubjectAggregation
    {
        private static EventManager instance;
        static void createInstance()
        {
            instance = new EventManager();
        }
        public static EventManager Instance
        {
            get
            {
                if (instance == null)
                {
                    createInstance();
                }
                return instance;
            }
        }
    }

    public class EventSpawner : Manager<EventSpawner>
    {
        private Queue<ISubject> eventQueue = new Queue<ISubject>();
        private readonly object eventLock = new object();

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            //ISubject message handle
            if (eventQueue.Count > 0)
            {
                lock (eventLock)
                {
                    while (eventQueue.Count > 0)
                    {
                        ISubject subject = eventQueue.Dequeue();
                        if (subject != null)
                        {
                            subject.Trigger();
                        }
                    }
                }
            }
        }

        public void TriggerInMainThread(ISubject subject)
        {
            lock (eventLock)
            {
                eventQueue.Enqueue(subject);
            }
        }

        public void Trigger(ISubject subject)
        {
            if (subject != null)
            {
                subject.Trigger();
            }
        }
    }
}
