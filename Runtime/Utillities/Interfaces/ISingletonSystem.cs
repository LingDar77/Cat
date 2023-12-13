
namespace SFC
{
    public interface ISingletonSystem<Type> : IGameSystem<Type>
    {
        static Type Singleton { get; set; }
    }
}