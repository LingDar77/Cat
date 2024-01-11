using UnityEngine;
namespace TUI.PoolifiedObjects
{
    public class BasicPoolifiedObject : MonoBehaviour, IPoolifiedGameObject
    {
        public IPoolifiedGameObjectManagement Pool { get; set; }

        public void DisposeObject()
        {
        }

        public void InitObject(IPoolifiedGameObjectManagement pool)
        {
            Pool = pool;
            transform.localPosition = Vector3.zero;
        }
    }
}