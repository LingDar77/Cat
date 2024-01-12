namespace TUI.ObjectPool
{
    public interface IObjectPool<Type> where Type : class, new()
    {
        Type Get();
        void Return(Type obj);
    }
}