using UnityEngine;
using UnityEngine.Events;
namespace TUI.EventDispatchSystem
{
    public class EventDispatcherProxy : MonoBehaviour, IEventDispatchSystem<string>
    {
        [System.Serializable]
        public class ReciveEventHandler
        {
            public string eventType;
            public UnityEvent handler;
        }
        [SerializeField] private ReciveEventHandler[] Handlers;

        private void Start()
        {
            foreach (var handler in Handlers)
            {
                Subscribe(handler.eventType, e => handler.handler.Invoke());
            }
        }
        public void Dispatch(string type)
        {
            IEventDispatchSystem<string>.GetChecked().Dispatch(type, null);
        }

        public void Dispatch(string type, EventParam data)
        {
            IEventDispatchSystem<string>.GetChecked().Dispatch(type, data);
        }

        public void Subscribe(string type, System.Action<EventParam> callback)
        {
            IEventDispatchSystem<string>.GetChecked().Subscribe(type, callback);
        }

        public void Unsubscribe(string type, System.Action<EventParam> callback)
        {
            IEventDispatchSystem<string>.GetChecked().Subscribe(type, callback);
        }
    }
}