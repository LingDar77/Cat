namespace TUI.PoolingSystem
{
    public interface IPoolMaintainer
    {
        void Init<PooledType>(IPoolingSystem<PooledType> pool) where PooledType : IPooledObject<PooledType>, new();
        void OnEnpool<PooledType>(IPoolingSystem<PooledType> pool) where PooledType : IPooledObject<PooledType>, new();
        void OnDepool<PooledType>(IPoolingSystem<PooledType> pool) where PooledType : IPooledObject<PooledType>, new();
    }
}