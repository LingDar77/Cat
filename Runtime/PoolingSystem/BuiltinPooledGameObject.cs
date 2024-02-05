namespace TUI.PoolingSystem
{
    using TUI.Utillities;
    using UnityEngine;

    public class BuiltinPooledGameObject : MonoBehaviour, IPooledObject<BuiltinPooledGameObject>
    {
        public IPoolingSystem<BuiltinPooledGameObject> Pool { get; set; }

        public void Dispose()
        {
            Pool?.Enpool(this);
        }

        private void OnEnable()
        {
            CoroutineHelper.WaitForSeconds(() =>
            {
                Pool?.Enpool(this);
                gameObject.SetActive(false);
            }, 1f);
        }
    }
}