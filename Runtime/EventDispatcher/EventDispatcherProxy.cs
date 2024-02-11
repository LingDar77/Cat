namespace Cat.EventDispatchSystem
{
    using UnityEngine;
    using UnityEngine.Events;
    using IDisposable = System.IDisposable;

    public class EventDispatcherProxy : MonoBehaviour, ISingletonSystem<BuiltinEventDispatcher>
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
            ISingletonSystem<BuiltinEventDispatcher>.GetChecked().Dispatch(type, null);
        }

        public void Dispatch(string type, IDisposable data)
        {
            ISingletonSystem<BuiltinEventDispatcher>.GetChecked().Dispatch(type, data);
        }

        public void Subscribe(string type, System.Action<IDisposable> callback)
        {
            ISingletonSystem<BuiltinEventDispatcher>.GetChecked().Subscribe(type, callback);
        }

        public void Unsubscribe(string type, System.Action<IDisposable> callback)
        {
            ISingletonSystem<BuiltinEventDispatcher>.GetChecked().Subscribe(type, callback);
        }
    }
}