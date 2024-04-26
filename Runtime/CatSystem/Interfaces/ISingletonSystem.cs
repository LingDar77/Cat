namespace Cat
{
    using UnityEngine;
    public interface ISingletonSystem<Type> : ICatSystem<Type>
    {
        static Type Singleton;
        static Type GetChecked()
        {
            var instance = Singleton as Component;
            if (instance != null) return Singleton;
            if (!typeof(Type).IsSubclassOf(typeof(Component)))
            {
                throw new System.Exception($"The System you are trying to access is not implemented as a Component or not initialized yet. You can try accessing it by ISingletonSystem<{typeof(Type).Name}>.Singleton");
            }
#pragma warning disable UNT0014 // Invalid type for call to GetComponent
            Singleton = new GameObject(typeof(Type).Name, typeof(Type)).GetComponent<Type>();
#pragma warning restore UNT0014 // Invalid type for call to GetComponent
            return Singleton;
        }
    }
}