namespace Cat.PoolingSystem
{
    public interface IPoolingSystem<PooledType> where PooledType : IPooledObject<PooledType>, new()
    {
        int Count { get; }
        void Enpool(PooledType obj);
        PooledType Depool();
    }

}