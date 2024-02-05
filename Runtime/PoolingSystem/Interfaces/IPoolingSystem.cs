namespace TUI.PoolingSystem
{
    public interface IPoolingSystem<PooledType> where PooledType : IPooledObject<PooledType>, new()
    {
        int Count { get; }
        IPoolMaintainer[] Maintainer { get; }
        void Enpool(PooledType obj);
        PooledType Depool();
    }

}