namespace Cat
{
    using Cat.Utilities;
    using UnityEngine;
    using UnityEngine.Events;

    [DefaultExecutionOrder(-1000)]
    public class SimpleEvents : MonoBehaviour
    {
        public UnityEvent OnEnabled;
        public UnityEvent OnStart;
        public UnityEvent OnFirstFrame;
        public UnityEvent OnUpdate;

        private void OnEnable()
        {
            OnEnabled.Invoke();
        }
        private void Start()
        {
            OnStart.Invoke();
            this.NextUpdate(() => OnFirstFrame.Invoke(), 2);
        }
        private void Update()
        {
            OnUpdate.Invoke();
        }
    }
}
