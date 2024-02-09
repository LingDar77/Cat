namespace Cat
{
    using System.Collections;
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
        private IEnumerator Start()
        {
            OnStart.Invoke();
            yield return new WaitForEndOfFrame();
            OnFirstFrame.Invoke();
        }
        private void Update()
        {
            OnUpdate.Invoke();
        }
    }
}
