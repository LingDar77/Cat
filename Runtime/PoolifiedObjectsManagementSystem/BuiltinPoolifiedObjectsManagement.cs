using System.Collections.Generic;
using UnityEngine;
namespace SFC.PoolifiedObjects
{
    public class BuiltinPoolifiedObjectsManagement : MonoBehaviour, IPoolifiedObjectsManagement
    {
        [ImplementedInterface(typeof(IGameObjectGenerator))]
        public Object GameObjectGenerator;
        public IGameObjectGenerator Generator
        {
            get => GameObjectGenerator as IGameObjectGenerator;
            set => GameObjectGenerator = value as Object;
        }
        protected HashSet<IPoolifiedObject> poolifiedObjects = new();

        public virtual IPoolifiedObject Depool()
        {
            if (poolifiedObjects.Count == 0)
            {
                if (!Generator.GenerateObject().TryGetComponent<IPoolifiedObject>(out var poolifiedObject))
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
