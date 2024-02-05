namespace TUI.EventDispatchSystem
{
    using TUI.PoolingSystem;
    public class EventParam : IPooledObject<EventParam>
    {
        public IPoolingSystem<EventParam> Pool { get; set; }

        public void Dispose()
        {
            Pool?.Enpool(this);
        }

    }
    public interface IEventDispatcher<EventType> : ISingletonSystem<IEventDispatcher<EventType>>
    {
        void Dispatch(EventType type, EventParam data);
        void Subscribe(EventType type, System.Action<EventParam> callback);
        void Unsubscribe(EventType type, System.Action<EventParam> callback);
    }
}