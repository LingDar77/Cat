namespace Cat.EventDispatchSystem
{
    using IDisposable = System.IDisposable;

    public interface IEventDispatcher<EventType> : ISingletonSystem<IEventDispatcher<EventType>>
    {
        void Dispatch(EventType type, IDisposable data);
        void Subscribe(EventType type, System.Action<IDisposable> callback);
        void Unsubscribe(EventType type, System.Action<IDisposable> callback);
    }
}