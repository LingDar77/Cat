using System.Collections.Generic;

namespace TUI.ObjectPool
{
    public class BuiltinObjectPool<Type> : IObjectPool<Type> where Type : class, new()
    {
        public System.Action<Type> CreateProcesser;
        public System.Action<Type> ReturnProcesser;


        protected Queue<Type> pool = new();

        protected Type CreateNew()
        {
            var obj = new Type();
            CreateProcesser?.Invoke(obj);
            return obj;
        }

        public Type Get()
        {
            if (pool.Count == 0)
            {
                return CreateNew();
            }
            return pool.Dequeue();
        }

        public void Return(Type obj)
        {
            ReturnProcesser?.Invoke(obj);
            pool.Enqueue(obj);
        }
    }
}