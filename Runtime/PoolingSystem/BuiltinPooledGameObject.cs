namespace Cat.PoolingSystem
{
    using UnityEngine;

    public class BuiltinPooledGameObject : MonoBehaviour, IMultiPooledObject<Transform, BuiltinPooledGameObject>
    {
        public IMultiPoolingSystem<Transform, BuiltinPooledGameObject> Pool { get; set; }
        public Transform Key { get; set; }

        public virtual void Dispose()
        {
            if (Pool == null) return;
            gameObject.SetActive(false);
            gameObject.transform.SetParent(Pool.transform);
            gameObject.transform.localPosition = Vector3.zero;
            Pool.Enpool(Key, this);
        }
    }
}