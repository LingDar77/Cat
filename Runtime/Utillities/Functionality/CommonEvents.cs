namespace TUI
{
    using System.Collections;
    using TUI.Utillities;
    using UnityEngine;

    public class CommonEvents : MonoBehaviour
    {
        public TUIEvent OnAake;
        public TUIEvent OnStart;
        public TUIEvent OnFirstFrame;
        public TUIEvent OnUpdate;
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
