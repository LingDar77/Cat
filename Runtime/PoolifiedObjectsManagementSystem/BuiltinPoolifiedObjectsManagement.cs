using System.Collections.Generic;
using UnityEngine;
namespace SFC.PoolifiedObjects
{
    public class BuiltinPoolifiedObjectsManagement : MonoBehaviour, IPoolifiedObjectsManagement
    {
        [InterfaceRequired(typeof(IGameObjectGenerator))]
        public Object GameObjectGenerator;
        protected IGameObjectGenerator generator;
        protected HashSet<IPoolifiedObject> poolifiedObjects = new();
        [ContextMenu("Test Depool")]
        private void TestDepool()
        {
            var obj = Depool();
            obj.transform.SetParent(null);
            obj.transform.position = new Vector3(Random.Range(0, 10f), Random.Range(0, 10f), Random.Range(0, 10f));
            this.WaitForSeconds(() =>
            {
                Enpool(obj);
            }, 5);
        }
        protected virtual void OnEnable()
        {
            generator = GameObjectGenerator as IGameObjectGenerator;
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
