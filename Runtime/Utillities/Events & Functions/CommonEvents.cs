using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SFC
{
    public class CommonEvents : MonoBehaviour
    {
        public UnityEvent OnStart;
        public UnityEvent OnFirstFrame;
        private IEnumerator Start()
        {
            OnStart.Invoke();
            yield return new WaitForEndOfFrame();
            OnFirstFrame.Invoke();
        }
    }
}
