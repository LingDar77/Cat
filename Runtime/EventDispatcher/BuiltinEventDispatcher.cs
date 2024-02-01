namespace TUI.EventDispatchSystem
{
    using System.Collections;
    using System.Collections.Generic;
    using TUI.Utillities;
    using UnityEngine;

    public class BuiltinEventDispatcher : MonoBehaviour, IEventDispatcher<string>
    {
        [Range(1, 16)]
        [SerializeField] private uint DispatchRate = 10;
        [Tooltip("Enable Immediately Dispatch will cause protentially infinite invocation loops and may  drop frame rate when dispatching.")]
        public bool DispatchImmediately = false;
        protected Queue<string> dispatchQueue = new();
        protected Queue<EventParam> dispatchParamQueue = new();

        protected bool isDispatching = false;
        protected readonly Dictionary<string, System.Action<EventParam>> events = new();

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

        public virtual void Dispatch(string type, EventParam data)
        {
            if (DispatchImmediately)
            {
                DispatchAllEventsImmediately(type, data);
            }
            else
            {
                DispatchAllEventsDelayed(type, data);
            }
        }

        private void DispatchAllEventsImmediately(string type, EventParam data)
        {
            if (!events.TryGetValue(type, out var actions) || actions == null) return;
            actions?.Invoke(data);
        }

        private void DispatchAllEventsDelayed(string type, EventParam data)
        {
            dispatchQueue.Enqueue(type);
            dispatchParamQueue.Enqueue(data);

            if (!isDispatching) StartCoroutine(DispatchAllEvents());
        }

        private IEnumerator DispatchAllEvents()
        {
            isDispatching = true;
            var count = DispatchRate;
            var hash = new HashSet<string>();
            while (dispatchQueue.Count != 0)
            {
                var type = dispatchQueue.Dequeue();
                var data = dispatchParamQueue.Dequeue();

                if (!events.TryGetValue(type, out var actions) || actions == null) continue;

                if (hash.Contains(type))
                {
                    this.LogFormat("Circular dependency detected when dispatching event: {0}, skipping it.", LogType.Warning, type);
                    continue;
                }

                hash.Add(type);
                foreach (var action in actions.GetInvocationList())
                {
                    (action as System.Action<EventParam>)?.Invoke(data);
                    if (--count != 0) continue;

                    count = DispatchRate;
                    yield return CoroutineHelper.nextUpdate;
                }
            }
            isDispatching = false;
        }

        public virtual void Subscribe(string type, System.Action<EventParam> callback)
        {
            if (events.TryGetValue(type, out var list))
            {
                events[type] = list += callback;
                return;
            }

            events.Add(type, callback);
        }

        public virtual void Unsubscribe(string type, System.Action<EventParam> callback)
        {
            if (!events.TryGetValue(type, out var list)) return;

            events[type] = list -= callback;
        }
    }
}
