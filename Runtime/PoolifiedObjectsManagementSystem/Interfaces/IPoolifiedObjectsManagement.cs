namespace TUI.PoolifiedObjects
{
    public interface IPoolifiedObjectsManagement : IGameSystem<IPoolifiedObjectsManagement>
    {
        IGameObjectGenerator Generator { get; }
        IPoolifiedObject Depool();
        void Enpool(IPoolifiedObject objectToEnpool);
    }
}