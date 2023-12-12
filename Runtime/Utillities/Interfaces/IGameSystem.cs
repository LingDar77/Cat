
namespace SFC
{
    public interface IGameSystem<Type> : ITransformGetable, IEnabledSetable
    {
        void OnEnable();
        void OnDisable();
    }
}
