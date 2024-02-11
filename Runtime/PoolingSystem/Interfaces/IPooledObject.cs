namespace Cat.PoolingSystem
{
    public interface IPooledObject<PooledType> : System.IDisposable
    where PooledType : IPooledObject<PooledType>, new()
    {
        IPoolingSystem<PooledType> Pool { get; set; }
    }

    public interface IMultiPooledObject<KeyType, PooledType> : System.IDisposable
    where PooledType : IMultiPooledObject<KeyType, PooledType>, new()
    {
        IMultiPoolingSystem<KeyType, PooledType> Pool { get; set; }
        KeyType Key { get; set; }
    }
}