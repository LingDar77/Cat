using System;
using System.Collections.Generic;
using UnityEngine;
namespace TUI.PoolifiedObjects
{
    public class BuiltinObjectPool<Type> : IPoolifiedObjectManagement<Type> where Type : IDisposable, new()
    {
        private static BuiltinObjectPool<Type> instance;

        public static BuiltinObjectPool<Type> Instance
        {
            get
            {
                instance ??= new BuiltinObjectPool<Type>();
                return instance;
            }
        }

        private BuiltinObjectPool() { }

        public int Count { get => Pool.Count; }

        public Queue<Type> Pool = new();


        public Type Depool()
        {
            if (Pool.Count != 0)
                return Pool.Dequeue();
            return new Type();
        }

        public void Enpool(Type obj)
        {
            Debug.Log(Pool.Count);
            Pool.Enqueue(obj);
        }
    }
}