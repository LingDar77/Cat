namespace TUI.ObjectPool
{
    public interface IPooledObject<Type> where Type : IPooledObject<Type>, System.IDisposable, new()
    {
        IObjectPool<Type> Pool { get; set; }
    }
}