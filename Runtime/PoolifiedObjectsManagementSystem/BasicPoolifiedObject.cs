using UnityEngine;
namespace SFC.PoolifiedObjects
{
    public class BasicPoolifiedObject : MonoBehaviour, IPoolifiedObject
    {
        public IPoolifiedObjectsManagement Pool { get; set; }

        public void DisposeObject()
        {
        }

        public void InitObject(IPoolifiedObjectsManagement pool)
        {
            Pool = pool;
            transform.localPosition = Vector3.zero;
        }
    }
}