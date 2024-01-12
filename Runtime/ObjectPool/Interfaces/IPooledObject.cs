namespace TUI.ObjectPool
{
    public interface IPooledObject<Type> : System.IDisposable where Type : IPooledObject<Type>, new()
    {
        IObjectPool<Type> Pool { get; set; }
    }
}