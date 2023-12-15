
using UnityEngine;

namespace SFC
{
    public interface ISingletonSystem<Type> : IGameSystem<Type>
    {
        static Type Singleton { get; set; }
        static Type GetSingletonChecked()
        {
            if (Singleton != null) return Singleton;
            if (!typeof(Type).IsSubclassOf(typeof(MonoBehaviour)))
            {
                throw new System.Exception("The Singleton System trying to access is not implemented as a MonoBehaviour.");
            }
#pragma warning disable UNT0014 // Invalid type for call to GetComponent
            Singleton ??= new GameObject(typeof(Type).Name, typeof(Type)).GetComponent<Type>();
#pragma warning restore UNT0014 // Invalid type for call to GetComponent
            return Singleton;
        }
    }
}