namespace Cat.PoolingSystem
{
    using System.Collections.Generic;
    using Cat.Utilities;
    using UnityEngine;

    public class BuiltinPoolingSystem<Type> : IPoolingSystem<Type> where Type : IPooledObject<Type>, new()
    {
        public int Count { get; }
        public System.Action<Type> OnEnpool;
        public System.Action<Type> OnDepool;
        public System.Func<Type> CreateInstance;
        protected Queue<Type> pool = new();

        private static BuiltinPoolingSystem<Type> shared;

        public static BuiltinPoolingSystem<Type> Shared => shared ??= new();

        public BuiltinPoolingSystem()
        {
            CreateInstance = CreateNew;
        }

        public void Enpool(Type obj)
        {
            pool.Enqueue(obj);
            OnEnpool?.Invoke(obj);
        }

        public Type Depool()
        {
            var instance = pool.Count == 0 ? CreateInstance() : pool.Dequeue();
            OnDepool?.Invoke(instance);
            return instance;
        }

        private Type CreateNew()
        {
            var obj = new Type
            {
                Pool = this
            };
            return obj;
        }
    }



    public class BuiltinPoolingSystem : SingletonSystemBase<BuiltinPoolingSystem>, IMultiPoolingSystem<Transform, BuiltinPooledGameObject>
    {
        protected Dictionary<Transform, Queue<BuiltinPooledGameObject>> pools = new();
        public List<Transform> prefabs;

        public void Enpool(Transform key, BuiltinPooledGameObject obj)
        {
            if (obj == null) return;
            var pool = GetPool(key);
            pool.Enqueue(obj);
        }

        public BuiltinPooledGameObject Depool(Transform key)
        {
            var pool = GetPool(key);
            if (pool.Count == 0)
            {
                return CreateNew(key);
            }
            return pool.Dequeue();
        }
        public BuiltinPooledGameObject Depool(string name)
        {
            var prefab = prefabs.Find(item => item.name == name);
            if (prefab == null) return null;
            var pool = GetPool(prefab);
            if (pool.Count == 0)
            {
                return CreateNew(prefab);
            }
            return pool.Dequeue();
        }

        public int Count(Transform key)
        {
            return GetPool(key).Count;
        }

        protected Queue<BuiltinPooledGameObject> GetPool(Transform key)
        {
            if (!pools.TryGetValue(key, out var pool))
            {
                pool = new();
                pools.Add(key, pool);
            }
            return pool;
        }

        protected virtual BuiltinPooledGameObject CreateNew(Transform key)
        {
            var instance = Instantiate(key).GetComponent<BuiltinPooledGameObject>();
            instance.transform.SetParent(transform);
            if (!instance.gameObject.activeSelf) instance.gameObject.SetActive(true);
            instance.Pool = this;
            instance.Key = key;
            instance.gameObject.SetActive(false);
            return instance;
        }
    }
}