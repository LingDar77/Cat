namespace TUI.PoolingSystem.Test
{
    using TUI.Utillities;
    using UnityEngine;

    public class Test1 : IPooledObject<Test1>
    {
        public IPoolingSystem<Test1> Pool { get; set; }

        public void Dispose()
        {
           Pool.Enpool(this);
        }
    }
    public class TestObject : MonoBehaviour
    {
        private readonly BuiltinPoolingSystem<Test1> pool = new();
        private IPooledObject<Test1> obj;

        private System.Action DisposeObject;
        private void Start()
        {
            DisposeObject = () =>
            {
                obj?.Dispose();
            };
        }

        private void Update()
        {
            obj = pool.Depool();
            CoroutineHelper.NextUpdate(DisposeObject);
        }
    }
}