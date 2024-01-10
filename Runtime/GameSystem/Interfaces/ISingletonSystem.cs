using UnityEngine;

namespace TUI
{
    public interface ISingletonSystem<Type> : IGameSystem<Type>
    {
        static Type Singleton { get; set; }
        static Type GetInstance()
        {
            if (Singleton != null) return Singleton;
            if (!typeof(Type).IsSubclassOf(typeof(Component)))
            {
                throw new System.Exception($"The System you are trying to access is not implemented as a Component or not initialized yet. You can try accessing it by ISingletonSystem<{typeof(Type).Name}>.Singleton");
            }
#pragma warning disable UNT0014 // Invalid type for call to GetComponent
            Singleton ??= new GameObject(typeof(Type).Name, typeof(Type)).GetComponent<Type>();
#pragma warning restore UNT0014 // Invalid type for call to GetComponent
            return Singleton;
        }
    }
}