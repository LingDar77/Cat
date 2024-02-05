namespace TUI.PoolingSystem
{
    public interface IPooledObject<PooledType> : System.IDisposable where PooledType : IPooledObject<PooledType>, new()
    {
        IPoolingSystem<PooledType> Pool { get; set; }
    }
}