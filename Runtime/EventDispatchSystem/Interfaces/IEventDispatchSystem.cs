
namespace TUI.EventDispatchSystem
{
    public class EventParam { }
    public interface IEventDispatchSystem<EventType> : ISingletonSystem<IEventDispatchSystem<EventType>>
    {
        void Dispatch(EventType type, EventParam data);
        void Subscribe(EventType type, System.Action<EventParam> callback);
        void Unsubscribe(EventType type, System.Action<EventParam> callback);
    }
}