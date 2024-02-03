namespace TUI.ObjectPool
{
    using UnityEngine;
    public interface IGameObjectFactory<RefType>
    {
        GameObject Create(RefType reference, Vector3 position, Quaternion rotation);
        GameObject Create(RefType reference);
        GameObject Create(RefType reference, Transform transform);

    }
    public interface IGameObjectProcesser
    {
        void Activate(GameObject gameObject);
        void Deactivate(GameObject gameObject);
    }
    public interface IGameObjectPool<RefType> : IGameSystem<IGameObjectPool<RefType>>
    {
        int Count(RefType reference);
        void Preserve(RefType reference, int count, IGameObjectFactory<string> factory = null, IGameObjectProcesser processer = null);
        GameObject Get(RefType reference, IGameObjectFactory<RefType> factory = null, IGameObjectProcesser processer = null);
        GameObject Get(RefType reference, Transform transform, IGameObjectFactory<RefType> factory = null, IGameObjectProcesser processer = null);
        GameObject Get(RefType reference, Vector3 position, IGameObjectFactory<RefType> factory = null, IGameObjectProcesser processer = null);
        GameObject Get(RefType reference, Vector3 position, Quaternion rotation, IGameObjectFactory<RefType> factory = null, IGameObjectProcesser processer = null);
        void Return(RefType reference, GameObject gameObject, IGameObjectProcesser processer = null);
    }

}