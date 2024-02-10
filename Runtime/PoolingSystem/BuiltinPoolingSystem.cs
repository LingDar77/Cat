namespace Cat.PoolingSystem
{
    using System.Collections.Generic;
    using Cat.Utillities;
    using UnityEngine;

    public class BuiltinPoolingSystem<Type> : IPoolingSystem<Type> where Type : IPooledObject<Type>, new()
    {
        public int Count { get; }
        public System.Action<Type> OnEnpool;
        public System.Action<Type> OnDepool;
        public System.Func<Type> CreateInstance;
        protected Queue<Type> pool = new();

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

    public class BuiltinPoolingSystem : MonoBehaviour, ICatSystem<BuiltinPoolingSystem>, IPoolingSystem<BuiltinPooledGameObject>
    {
        public int Count { get; }

        public Transform Prefab;
        public CatDriver<BuiltinPooledGameObject> OnEnpool;
        public CatDriver<BuiltinPooledGameObject> OnDepool;
        public System.Func<BuiltinPooledGameObject> CreateInstance => CreateNew;
        protected Queue<BuiltinPooledGameObject> pool = new();

        protected BuiltinPoolingSystem() { }

        public void Enpool(BuiltinPooledGameObject obj)
        {
            pool.Enqueue(obj);
            if (OnEnpool == null) return;
            OnEnpool.Drive(obj);
        }

        public BuiltinPooledGameObject Depool()
        {
            var instance = pool.Count == 0 ? CreateInstance() : pool.Dequeue();
            if (OnDepool != null)
                OnDepool.Drive(instance);
            instance.gameObject.SetActive(true);
            return instance;
        }
        private BuiltinPooledGameObject CreateNew()
        {
            BuiltinPooledGameObject retsult;
            if (Prefab == null)
            {
                retsult = new GameObject("GameObject", typeof(BuiltinPooledGameObject)).GetComponent<BuiltinPooledGameObject>();
            }
            else
            {
                retsult = Instantiate(Prefab).GetComponent<BuiltinPooledGameObject>();
            }
            retsult.Pool = this;
            return retsult;
        }
    }
}