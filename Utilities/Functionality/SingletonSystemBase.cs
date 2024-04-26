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
        protected bool dontDestroyOnLoad = true;

        protected virtual void OnEnable()
        {
            if (Equals(ISingletonSystem<ImplementType>.Singleton)) return;
            ISingletonSystem<ImplementType>.Singleton = this as ImplementType;
            if (!dontDestroyOnLoad) return;
            DontDestroyOnLoad(transform.root.gameObject);
        }

        protected virtual void OnDisable()
        {
            if (!Equals(ISingletonSystem<ImplementType>.Singleton)) return;
            StopAllCoroutines();
            ISingletonSystem<ImplementType>.Singleton = null;
        }
    }

}