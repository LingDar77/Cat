namespace Cat
{
    using Cat.Utillities;
    using UnityEngine;
    using UnityEngine.Events;

    public class CommonEvents : MonoBehaviour
    {
        public UnityEvent OnAake;
        public UnityEvent OnStart;
        public UnityEvent OnFirstFrame;
        public UnityEvent OnUpdate;
        private void Awake()
        {
            OnAake.Invoke();
        }
        private void Start()
        {
            OnStart.Invoke();
            this.NextUpdate(() => OnFirstFrame.Invoke());
        }
        private void Update()
        {
            OnUpdate.Invoke();
        }
    }
}
