using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TUI.EventDispatchSystem
{
    public class BuiltinEventDispatcher : MonoBehaviour, IEventDispatchSystem<string>
    {
#if UNITY_EDITOR
        [SerializeField] float MaxDispatchTime = 1f;
#endif
        protected Queue<string> dispatchQueue = new();
        protected Queue<EventParam> dispatchParamQueue = new();

        protected bool isDispatching = false;
        protected readonly Dictionary<string, System.Action<EventParam>> events = new();

        protected virtual void OnEnable()
        {
            if (IEventDispatchSystem<string>.Singleton != null) return;

            IEventDispatchSystem<string>.Singleton = this;
            DontDestroyOnLoad(transform.root.gameObject);

        }

        protected virtual void OnDisable()
        {
            if (IEventDispatchSystem<string>.Singleton.transform != transform) return;
            IEventDispatchSystem<string>.Singleton = null;
        }

        public virtual void Dispatch(string type, EventParam data)
        {
            dispatchQueue.Enqueue(type);
            dispatchParamQueue.Enqueue(data);
            if (!isDispatching) StartCoroutine(DispatchAllEvents());
        }

        private IEnumerator DispatchAllEvents()
        {
            isDispatching = true;

#if UNITY_EDITOR
            var time = Time.realtimeSinceStartup;
#endif
            while (dispatchQueue.Count != 0)
            {
                var type = dispatchQueue.Dequeue();
                var data = dispatchParamQueue.Dequeue();

                if (events.TryGetValue(type, out var actions))
                {
                    foreach (var action in actions.GetInvocationList())
                    {
                        action.DynamicInvoke(data);
                        yield return CoroutineHelper.nextUpdate;
                    }
                }

#if UNITY_EDITOR
                var currentTime = Time.realtimeSinceStartup;
                if (currentTime - time > MaxDispatchTime)
                {
                    Debug.LogWarning($"One event dispatching process is taking too much time, may event loops are triggered. Last excuted event type: {type}. Forcing the current dispatching process to stop.");
                    dispatchParamQueue.Clear();
                    dispatchQueue.Clear();
                    break;
                }
#endif
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
            if (events.TryGetValue(type, out var list))
            {
                events[type] = list -= callback;
                return;
            }
        }
    }
}