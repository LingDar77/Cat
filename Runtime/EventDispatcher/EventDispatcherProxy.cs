using UnityEngine;
using UnityEngine.Events;
namespace TUI.EventDispatchSystem
{
    public class EventDispatcherProxy : MonoBehaviour, IEventDispatcher<string>
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
            IEventDispatcher<string>.GetChecked().Dispatch(type, null);
        }

        public void Dispatch(string type, EventParam data)
        {
            IEventDispatcher<string>.GetChecked().Dispatch(type, data);
        }

        public void Subscribe(string type, System.Action<EventParam> callback)
        {
            IEventDispatcher<string>.GetChecked().Subscribe(type, callback);
        }

        public void Unsubscribe(string type, System.Action<EventParam> callback)
        {
            IEventDispatcher<string>.GetChecked().Subscribe(type, callback);
        }
    }
}