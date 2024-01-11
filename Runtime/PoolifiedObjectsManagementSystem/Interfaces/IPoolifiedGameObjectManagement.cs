namespace TUI.PoolifiedObjects
{
    public interface IPoolifiedGameObjectManagement : IGameSystem<IPoolifiedGameObjectManagement>
    {
        IGameObjectGenerator Generator { get; }
        IPoolifiedGameObject Depool();
        void Enpool(IPoolifiedGameObject objectToEnpool);
    }
}