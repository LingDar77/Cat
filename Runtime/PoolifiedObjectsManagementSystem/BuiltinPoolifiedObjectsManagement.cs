using System.Collections.Generic;
using UnityEngine;
namespace SFC.PoolifiedObjects
{
    public class BuiltinPoolifiedObjectsManagement : MonoBehaviour, IPoolifiedObjectsManagement
    {
        public IGameObjectGenerator generator;
        protected HashSet<IPoolifiedObject> poolifiedObjects = new();
        protected virtual void OnEnable()
        {
            generator = GetComponent<IGameObjectGenerator>();
        }

        public virtual IPoolifiedObject Depool()
        {
            if (poolifiedObjects.Count == 0)
            {
                if (!generator.GenerateObject().TryGetComponent<IPoolifiedObject>(out var poolifiedObject))
                {
                    throw new System.Exception("The prefab tring to instantiate is not a poolified object!");
                }
                poolifiedObject.InitObject(this);
                return poolifiedObject;
            }
            IPoolifiedObject obj = null;
            poolifiedObjects.RemoveWhere(item => { obj = item; return true; });
            obj.InitObject(this);
            return obj;
        }
        public virtual void Enpool(IPoolifiedObject objectToEnpool)
        {
            if (objectToEnpool.Pool.transform != transform)
            {
                throw new System.Exception("The object to enpool is not from this pool!");
            }
            poolifiedObjects.Add(objectToEnpool);
            objectToEnpool.DisposeObject();
            objectToEnpool.transform.SetParent(transform, false);
        }
    }
}
