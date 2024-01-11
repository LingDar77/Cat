namespace TUI.PoolifiedObjects
{
    public interface IPoolifiedGameObject : IEnabledSetable, ITransformGetable
    {
        IPoolifiedGameObjectManagement Pool { get; set; }

        void InitObject(IPoolifiedGameObjectManagement pool);
        void DisposeObject();
    }
}