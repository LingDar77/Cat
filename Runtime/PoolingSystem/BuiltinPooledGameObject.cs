namespace Cat.PoolingSystem
{
    using UnityEngine;

    public class BuiltinPooledGameObject : MonoBehaviour, IMultiPooledObject<Transform, BuiltinPooledGameObject>
    {
        public IMultiPoolingSystem<Transform, BuiltinPooledGameObject> Pool { get; set; }
        public Transform Key { get; set; }

        public void SetParent(Transform target)
        {
            transform.SetParent(target);
            transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            transform.localScale = Vector3.one;
        }

        public void SetLocalPositionAndRotation(Vector3 posistion, Quaternion quaternion)
        {
            transform.SetLocalPositionAndRotation(posistion, quaternion);
            transform.localScale = Vector3.one;
        }

        public void SetPositionAndRotation(Vector3 posistion, Quaternion quaternion)
        {
            transform.SetPositionAndRotation(posistion, quaternion);
            transform.localScale = Vector3.one;
        }
        
        public virtual void Dispose()
        {
            if (Pool == null)
            {
                DestroyImmediate(gameObject);
                return;
            }
            gameObject.transform.SetParent(Pool.transform);
            gameObject.SetActive(false);
            gameObject.transform.localPosition = Vector3.zero;
            Pool.Enpool(Key, this);
        }

    }
}