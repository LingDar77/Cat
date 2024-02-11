namespace Cat.PoolingSystem
{
    public interface IPoolingSystem<PooledType> where PooledType : IPooledObject<PooledType>, new()
    {
        int Count { get; }
        void Enpool(PooledType obj);
        PooledType Depool();
    }
    public interface IMultiPoolingSystem<KeyType, PooledType> : ICatSystem<IMultiPoolingSystem<KeyType, PooledType>> where PooledType : IMultiPooledObject<KeyType, PooledType>, new()
    {
        int Count(KeyType key);
        void Enpool(KeyType key, PooledType obj);
        PooledType Depool(KeyType key);
    }

}