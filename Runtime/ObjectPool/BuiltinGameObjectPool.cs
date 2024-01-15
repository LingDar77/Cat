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

            public GameObject Create(string reference, Vector3 position, Quaternion rotation)
            {
                var obj = new GameObject(reference);
                obj.transform.SetPositionAndRotation(position, rotation);
                return obj;
            }

            public GameObject Create(string reference)
            {
                return new GameObject(reference);
            }

            public GameObject Create(string reference, Transform transform)
            {
                var obj = new GameObject(reference);
                obj.transform.SetParent(transform, false);
                obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                return obj;
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
            if (!pool.ContainsKey(reference)) pool.Add(reference, new());
            if (pool[reference].Count >= count) return;
            
            factory ??= defaultFactory;
            processer ??= defaultProcesser;
            while (pool[reference].Count != count)
            {
                var obj = factory.Create(reference);
                Return(reference, obj, processer);
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

        public GameObject Get(string reference, Transform transform, IGameObjectFactory<string> factory = null, IGameObjectProcesser processer = null)
        {
            GameObject obj;
            factory ??= defaultFactory;
            processer ??= defaultProcesser;
            if (pool.ContainsKey(reference) && pool[reference].Count != 0)
            {
                obj = pool[reference].Dequeue();
                obj.transform.SetParent(transform);
                obj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                processer.Activate(obj);
                return obj;
            }

            obj = factory.Create(reference, transform);
            processer.Activate(obj);
            return obj;
        }

        public GameObject Get(string reference, Vector3 position, IGameObjectFactory<string> factory = null, IGameObjectProcesser processer = null)
        {
            return Get(reference, position, Quaternion.identity, factory, processer);
        }

        public GameObject Get(string reference, Vector3 position, Quaternion rotation, IGameObjectFactory<string> factory = null, IGameObjectProcesser processer = null)
        {
            GameObject obj;
            factory ??= defaultFactory;
            processer ??= defaultProcesser;
            if (pool.ContainsKey(reference) && pool[reference].Count != 0)
            {
                obj = pool[reference].Dequeue();
                obj.transform.SetPositionAndRotation(position, rotation);
                processer.Activate(obj);
                return obj;
            }

            obj = factory.Create(reference, position, rotation);
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