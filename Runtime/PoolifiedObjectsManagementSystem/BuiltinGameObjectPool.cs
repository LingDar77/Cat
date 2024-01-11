using System.Collections.Generic;
using UnityEngine;
namespace TUI.PoolifiedObjects
{
    public class BuiltinGameObjectPool : MonoBehaviour, IPoolifiedGameObjectManagement
    {
        [ImplementedInterface(typeof(IGameObjectGenerator))]
        public Object GameObjectGenerator;
        public IGameObjectGenerator Generator
        {
            get => GameObjectGenerator as IGameObjectGenerator;
            set => GameObjectGenerator = value as Object;
        }
        protected HashSet<IPoolifiedGameObject> poolifiedObjects = new();

        public virtual IPoolifiedGameObject Depool()
        {
            if (poolifiedObjects.Count == 0)
            {
                if (!Generator.GenerateObject().TryGetComponent<IPoolifiedGameObject>(out var poolifiedObject))
                {
                    throw new System.Exception("The prefab tring to instantiate is not a poolified object!");
                }
                poolifiedObject.InitObject(this);
                return poolifiedObject;
            }
            IPoolifiedGameObject obj = null;
            poolifiedObjects.RemoveWhere(item => { obj = item; return true; });
            obj.InitObject(this);
            return obj;
        }
        public virtual void Enpool(IPoolifiedGameObject objectToEnpool)
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
