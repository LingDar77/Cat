namespace SFC.PoolifiedObjects
{
    public interface IPoolifiedObjectsManagement : IGameSystem<IPoolifiedObjectsManagement>
    {
        IPoolifiedObject Depool();
        void Enpool(IPoolifiedObject objectToEnpool);
    }
}