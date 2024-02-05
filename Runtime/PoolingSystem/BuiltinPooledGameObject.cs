namespace TUI.PoolingSystem
{
    using UnityEngine;

    public class BuiltinPooledGameObject : MonoBehaviour, IPooledObject<BuiltinPooledGameObject>
    {
        public IPoolingSystem<BuiltinPooledGameObject> Pool { get; set; }

        public void Dispose()
        {
            Pool?.Enpool(this);
        }
    }
}