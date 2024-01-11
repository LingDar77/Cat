using System;

namespace TUI.PoolifiedObjects
{
    public interface IPoolifiedObjectManagement<Type> where Type : IDisposable, new()
    {
        int Count { get; }
        Type Depool();
        void Enpool(Type obj);
    }
}