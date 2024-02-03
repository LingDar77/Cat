using TUI.ObjectPool;

namespace TUI.EventDispatchSystem
{
    public class EventParam : IPooledObject<EventParam>
    {
        public IObjectPool<EventParam> Pool { get; set; }

        public void Dispose()
        {
            Pool?.Return(this);
        }
    }
    public interface IEventDispatcher<EventType> : ISingletonSystem<IEventDispatcher<EventType>>
    {
        void Dispatch(EventType type, EventParam data);
        void Subscribe(EventType type, System.Action<EventParam> callback);
        void Unsubscribe(EventType type, System.Action<EventParam> callback);
    }
}