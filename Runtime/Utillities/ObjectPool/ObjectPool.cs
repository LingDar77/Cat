using System.Collections.Generic;
using UnityEngine;

namespace SFC.Utillities
{
    public sealed class ObjectPool : MonoBehaviour
    {
        private Dictionary<GameObject, Queue<GameObject>> gameObjects = new Dictionary<GameObject, Queue<GameObject>>();
        public GameObject GetObject(GameObject prototype)
        {
            GameObject result;
            if (!gameObjects.TryGetValue(prototype, out var queue) || queue.Count == 0)
            {
                result = Instantiate(prototype, transform);
                var comp = result.AddComponent<ObjectPoolChild>();
                comp.ResetInstance(prototype);
            }
            else
            {
                result = queue.Dequeue();
            }
            result.transform.SetParent(null);
            result.SetActive(true);
            return result;
        }

        public GameObject GetObject(GameObject prototype, Transform parent)
        {
            var result = GetObject(prototype);
            result.transform.SetParent(parent);
            result.transform.localScale = Vector3.one;
            result.transform.SetLocalPositionAndRotation(Vector3.zero, new Quaternion());
            return result;
        }

        public Transform GetObject(Transform prototype)
        {
            GameObject result;
            if (!gameObjects.TryGetValue(prototype.gameObject, out var queue) || queue.Count == 0)
            {
                result = Instantiate(prototype, transform).gameObject;
                var comp = result.AddComponent<ObjectPoolChild>();
                comp.ResetInstance(prototype.gameObject);


            }
            else
            {
                result = queue.Dequeue();
            }
            result.transform.SetParent(null);
            result.SetActive(true);
            return result.transform;
        }

        public Transform GetObject(Transform prototype, Transform parent)
        {
            var result = GetObject(prototype);
            result.SetParent(parent);
            result.localScale = Vector3.one;
            result.SetLocalPositionAndRotation(Vector3.zero, new Quaternion());
            return result;
        }

        public void CacheObject(GameObject obj)
        {
            if (obj.TryGetComponent<ObjectPoolChild>(out var comp))
            {
                if (!gameObjects.TryGetValue(comp.GetPrototype(), out var queue))
                {
                    queue = new Queue<GameObject>();
                    gameObjects.Add(comp.GetPrototype(), queue);
                }
                queue.Enqueue(obj);
                obj.transform.SetParent(transform);
                obj.SetActive(false);
            }
            else
            {
                Object.Destroy(obj);
            }
        }

        public void ClearPool()
        {
            foreach (var pair in gameObjects)
            {
                while (pair.Value.Count != 0)
                {
                    var obj = pair.Value.Dequeue();
                    Object.Destroy(obj);
                }
            }
            gameObjects.Clear();
        }

    }

    public sealed class ObjectPool<Type> where Type : IObjectPoolChild, new()
    {
        private Queue<Type> objects = new Queue<Type>();

        public Type GetObject()
        {
            Type result;
            if (objects.Count != 0)
            {
                result = objects.Dequeue();
                result.ResetInstance(default);
            }
            else
            {
                result = new Type();
            }
            return result;
        }
        public void CacheObject(Type obj)
        {
            objects.Enqueue(obj);
        }

        public void ClearPool()
        {
            objects.Clear();
        }
    }
}