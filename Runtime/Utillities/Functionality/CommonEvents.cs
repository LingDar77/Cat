namespace Cat
{
    using System.Collections;
    using Cat.Utillities;
    using UnityEngine;

    public class CommonEvents : MonoBehaviour
    {
        public CatEvent OnAake;
        public CatEvent OnStart;
        public CatEvent OnFirstFrame;
        public CatEvent OnUpdate;
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
