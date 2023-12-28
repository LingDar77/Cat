namespace TUI.PoolifiedObjects
{
    public interface IPoolifiedObject : IEnabledSetable, ITransformGetable
    {
        IPoolifiedObjectsManagement Pool { get; set; }

        void InitObject(IPoolifiedObjectsManagement pool);
        void DisposeObject();
    }
}