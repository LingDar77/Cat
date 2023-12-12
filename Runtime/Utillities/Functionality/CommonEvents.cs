using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace SFC
{
    public class CommonEvents : MonoBehaviour
    {
        public UnityEvent OnAake;
        public UnityEvent OnStart;
        public UnityEvent OnFirstFrame;
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
    }
}
