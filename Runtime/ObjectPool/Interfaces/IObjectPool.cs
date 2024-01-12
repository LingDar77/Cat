namespace TUI.ObjectPool
{
    public interface IObjectPool<Type> where Type : IPooledObject<Type>, new()
    {
        int Count { get; }
        Type Get();
        void Return(Type obj);
    }
}