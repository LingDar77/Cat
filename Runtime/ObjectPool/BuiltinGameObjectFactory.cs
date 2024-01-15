namespace TUI.ObjectPool
{
    using System.Collections.Generic;
    using UnityEngine;
    public class BuiltinGameObjectFactory : MonoBehaviour, IGameObjectFactory<string>
    {
        [SerializeField] private GameObject[] prefabs;
        private Dictionary<string, GameObject> pool = new();
        private void OnEnable()
        {
            pool.Clear();
            foreach (var item in prefabs)
            {
                pool.Add(item.name, item);
            }
        }
        public GameObject Create(string reference)
        {
            return Instantiate(pool[reference]);
        }
    }
}