namespace Cat.EventDispatchSystem
{
    using System.Collections;
    using System.Collections.Generic;
    using Cat.PoolingSystem;
    using Cat.Utillities;
    using UnityEngine;
    using SerializableAttribute = System.SerializableAttribute;

    public class BuiltinEventDispatcher : MonoBehaviour, IEventDispatcher<string>
    {
        [Serializable]
        public enum DispatchingMode
        {
            Synchronous,
            Asynchronous,
            PerInvocation
        }

        public class Subscriptioin : IPooledObject<Subscriptioin>
        {
            public string Type;
            public System.Action<EventParam> Callback;
            public bool Subscribed;
            public IPoolingSystem<Subscriptioin> Pool { get; set; }

            public Subscriptioin Init(string type, System.Action<EventParam> callback, bool subscribed)
            {
                Type = type;
                Callback = callback;
                Subscribed = subscribed;
                return this;
            }


            public void Dispose()
            {
                Pool?.Enpool(this);
            }

        }
        [SerializeField] private uint DispatchRate = 10;
        public DispatchingMode Mode = DispatchingMode.Asynchronous;
        public bool CheckCircularDependency = false;

        protected Queue<string> dispatchQueue = new();
        protected Queue<EventParam> dispatchParamQueue = new();
        protected HashSet<string> dispatched = new();
        protected bool isDispatching = false;
        protected readonly Dictionary<string, HashSet<System.Action<EventParam>>> events = new();
        protected Queue<Subscriptioin> subscriptioins = new();
        protected readonly BuiltinPoolingSystem<Subscriptioin> pool = new();

        protected virtual void OnEnable()
        {
            if (IEventDispatcher<string>.Singleton != null) return;

            IEventDispatcher<string>.Singleton = this;
            DontDestroyOnLoad(transform.root.gameObject);

        }

        protected virtual void OnDisable()
        {
            if (IEventDispatcher<string>.Singleton.transform != transform) return;
            IEventDispatcher<string>.Singleton = null;
        }

        public void Dispatch(string type, EventParam data)
        {
            if (Mode == DispatchingMode.Synchronous)
            {
                DispatchSynchronously(type, data);
            }
            else
            {
                DispatchAsynchronously(type, data);
            }
        }

        public virtual void Subscribe(string type, System.Action<EventParam> callback)
        {
            if (isDispatching)
            {
                subscriptioins.Enqueue(pool.Depool().Init(type, callback, true));
                return;
            }
            if (events.TryGetValue(type, out var list))
            {
                events[type].Add(callback);
                return;
            }

            events.Add(type, new() { callback });
        }

        public virtual void Unsubscribe(string type, System.Action<EventParam> callback)
        {
            if (isDispatching)
            {
                subscriptioins.Enqueue(pool.Depool().Init(type, callback, false));
                return;
            }
            if (!events.TryGetValue(type, out var list)) return;

            events[type].Remove(callback);
        }

        protected virtual void DispatchSynchronously(string type, EventParam data)
        {
            if (!events.ContainsKey(type)) return;
            var callbacks = events[type];
            foreach (var callback in callbacks)
            {
                callback.Invoke(data);
            }
            data?.Dispose();
        }

        protected virtual void DispatchAsynchronously(string type, EventParam data)
        {
            dispatchQueue.Enqueue(type);
            dispatchParamQueue.Enqueue(data);
            if (isDispatching) return;

            StartCoroutine(DoDispatchAsynchronously());
        }

        private IEnumerator DoDispatchAsynchronously()
        {
            isDispatching = true;
            yield return CoroutineHelper.nextUpdate;
            var count = DispatchRate;
            if (CheckCircularDependency) dispatched.Clear();

            while (dispatchQueue.Count != 0)
            {
                var type = dispatchQueue.Dequeue();
                using var data = dispatchParamQueue.Dequeue();

                if (!events.TryGetValue(type, out var actions) || actions == null)
                {
                    continue;
                }


                if (CheckCircularDependency)
                {
                    if (dispatched.Contains(type))
                    {
                        this.LogFormat("Circular dependency detected when dispatching event: {0}, skipping it.", LogType.Warning, type);
                        continue;
                    }
                    dispatched.Add(type);
                }

                if (Mode == DispatchingMode.Asynchronous)
                {
                    foreach (var action in actions)
                    {
                        action.Invoke(data);
                        if (--count != 0) continue;
                        count = DispatchRate;
                        yield return CoroutineHelper.nextUpdate;
                    }
                }
                else
                {
                    foreach (var action in actions)
                    {
                        foreach (var invocation in action.GetInvocationList())
                        {
                            (invocation as System.Action<EventParam>)?.Invoke(data);
                            if (--count != 0) continue;
                            count = DispatchRate;
                            yield return CoroutineHelper.nextUpdate;
                        }
                    }
                }
            }

            isDispatching = false;

            if (subscriptioins.Count != 0)
            {
                while (subscriptioins.Count != 0)
                {
                    using var sub = subscriptioins.Dequeue();
                    if (sub.Subscribed)
                    {
                        Subscribe(sub.Type, sub.Callback);
                    }
                    else
                    {
                        Unsubscribe(sub.Type, sub.Callback);
                    }
                }
            }

        }
    }
}
