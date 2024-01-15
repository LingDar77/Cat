namespace TUI.ObjectPool
{
    using System.Collections.Generic;
    using UnityEngine;
    public class BuiltinGameObjectPool : SingletonSystemBase<BuiltinGameObjectPool>, IGameObjectPool<string>
    {
        public class DefaultGameObjectProcesser : IGameObjectProcesser
        {
            public void Activate(GameObject gameObject)
            {
                gameObject.SetActive(true);
            }

            public void Deactivate(GameObject gameObject)
            {
                gameObject.SetActive(false);
            }
        }
        public class DefaultGameObjectFactory : IGameObjectFactory<string>
        {
            public GameObject Create(string reference)
            {
                return new GameObject(reference);
            }
        }

      
        [ImplementedInterface(typeof(IGameObjectFactory<string>))]
        [SerializeField] protected MonoBehaviour FactoryOverrride;
        [ImplementedInterface(typeof(IGameObjectProcesser))]
        [SerializeField] protected MonoBehaviour ProcesserOverride;


        protected Dictionary<string, Queue<GameObject>> pool = new();
        protected IGameObjectFactory<string> defaultFactory = new DefaultGameObjectFactory();
        protected IGameObjectProcesser defaultProcesser = new DefaultGameObjectProcesser();

        protected override void OnEnable()
        {
            base.OnEnable();
            if (FactoryOverrride != null)
            {
                defaultFactory = (IGameObjectFactory<string>)FactoryOverrride;
            }
            if (ProcesserOverride != null)
            {
                defaultProcesser = (IGameObjectProcesser)ProcesserOverride;
            }
        }
        public int Count(string reference)
        {
            if (!pool.ContainsKey(reference)) return 0;
            return pool[reference].Count;
        }

        public void Preserve(string reference, int count, IGameObjectFactory<string> factory = null, IGameObjectProcesser processer = null)
        {
            factory ??= defaultFactory;
            processer ??= defaultProcesser;
            while (count-- != 0)
            {
                Return(reference, factory.Create(reference), processer);
            }
        }

        public GameObject Get(string reference, IGameObjectFactory<string> factory = null, IGameObjectProcesser processer = null)
        {
            GameObject obj;
            factory ??= defaultFactory;
            processer ??= defaultProcesser;
            if (pool.ContainsKey(reference) && pool[reference].Count != 0)
            {
                obj = pool[reference].Dequeue();
                processer.Activate(obj);
                return obj;
            }

            obj = factory.Create(reference);
            processer.Activate(obj);
            return obj;
        }

        public void Return(string reference, GameObject gameObject, IGameObjectProcesser processer = null)
        {
            processer ??= defaultProcesser;
            processer.Deactivate(gameObject);
            if (!pool.ContainsKey(reference))
            {
                pool.Add(reference, new Queue<GameObject>());
            }
            pool[reference].Enqueue(gameObject);
        }
    }
}