using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TUI.EventDispatchSystem
{
    public enum GameEvent
    {
        Event1,
        Event2
    }
    public class BuiltinEventDispatcher : MonoBehaviour, IEventDispatchSystem<GameEvent>
    {
        protected Queue<GameEvent> dispatchQueue = new();
        protected Queue<EventParam> dispatchParamQueue = new();

        protected bool isDispatching = false;
        protected readonly Dictionary<GameEvent, HashSet<System.Action<EventParam>>> events = new();

        protected virtual void OnEnable()
        {
            if (IEventDispatchSystem<GameEvent>.Singleton != null) return;

            IEventDispatchSystem<GameEvent>.Singleton = this;
            DontDestroyOnLoad(transform.root.gameObject);
        }

        protected virtual void OnDisable()
        {
            if (IEventDispatchSystem<GameEvent>.Singleton.transform != transform) return;
            IEventDispatchSystem<GameEvent>.Singleton = null;
        }

        public virtual void Dispatch(GameEvent type, EventParam data)
        {
            dispatchQueue.Enqueue(type);
            dispatchParamQueue.Enqueue(data);
            if (!isDispatching) StartCoroutine(DispatchAllEvents());
        }

        private IEnumerator DispatchAllEvents()
        {
            isDispatching = true;
            while (dispatchQueue.Count != 0)
            {
                var type = dispatchQueue.Dequeue();
                var data = dispatchParamQueue.Dequeue();
                
                if (events.TryGetValue(type, out var actions))
                {
                    foreach (var action in actions)
                    {
                        action?.Invoke(data);
                        yield return CoroutineHelper.nextUpdate;
                    }
                }
            }
            isDispatching = false;
        }

        public virtual void Subscribe(GameEvent type, System.Action<EventParam> callback)
        {
            if (events.TryGetValue(type, out var list))
            {
                list.Add(callback);
                return;
            }

            events.Add(type, new() { callback });
        }

        public virtual void Unsubscribe(GameEvent type, System.Action<EventParam> callback)
        {
            if (events.TryGetValue(type, out var list))
            {
                list.Remove(callback);
                return;
            }
        }
    }
}