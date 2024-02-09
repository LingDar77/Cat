namespace Cat
{
    using UnityEngine;
    /// <summary>
    /// A base class for sperate singleton systems that can be accessed by:
    /// ISingletonSystem<ImplementType>.Singleton
    /// </summary>
    /// <typeparam name="ImplementType"></typeparam>
    public abstract class SingletonSystemBase<ImplementType> : MonoBehaviour, ISingletonSystem<ImplementType> where ImplementType : SingletonSystemBase<ImplementType>
    {
        protected virtual void OnEnable()
        {
            if (ISingletonSystem<ImplementType>.Singleton != null) return;
            ISingletonSystem<ImplementType>.Singleton = this as ImplementType;
            DontDestroyOnLoad(transform.root.gameObject);
        }

        protected virtual void OnDisable()
        {
            if (!ISingletonSystem<ImplementType>.Singleton.Equals(this)) return;
            ISingletonSystem<ImplementType>.Singleton = null;
        }

    }

}