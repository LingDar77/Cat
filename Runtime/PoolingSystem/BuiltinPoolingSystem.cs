namespace TUI.PoolingSystem
{
    using System.Collections.Generic;
    public class BuiltinPoolingSystem<Type> : IPoolingSystem<Type> where Type : IPooledObject<Type>, new()
    {
        public int Count { get; }
        protected Queue<Type> pool = new();
        public IPoolMaintainer[] Maintainer { get; }
        public System.Action<Type> OnEnpool;

        public void Enpool(Type obj)
        {
            pool.Enqueue(obj);
            OnEnpool?.Invoke(obj);
        }

        public Type Depool()
        {
            if (pool.Count == 0)
            {
                return CreateNew();
            }
            return pool.Dequeue();
        }

        protected virtual Type CreateNew()
        {
            var obj = new Type
            {
                Pool = this
            };
            return obj;
        }
    }
}