using System.Collections;
using UnityEngine;

namespace SFC
{
    public static class CoroutineHelper
    {
        static WaitForEndOfFrame waitForEndOfFrame = new();
        static WaitForFixedUpdate waitForFixedUpdate = new();
        public static Coroutine NextUpdate(this MonoBehaviour monoBehaviour, System.Action action, int time = 1)
        {
            IEnumerator NextUpdateCoroutine()
            {
                while (time != 0)
                {
                    yield return waitForEndOfFrame;
                    --time;
                }
                action?.Invoke();
            }
            return monoBehaviour.StartCoroutine(NextUpdateCoroutine());
        }
        public static Coroutine NextFixedUpdate(this MonoBehaviour monoBehaviour, System.Action action, int time = 1)
        {
            IEnumerator NextFixedUpdateCoroutine()
            {
                while (time != 0)
                {
                    yield return waitForFixedUpdate;
                    --time;
                }
                action?.Invoke();
            }
            return monoBehaviour.StartCoroutine(NextFixedUpdateCoroutine());
        }
        public static Coroutine WaitForSeconds(this MonoBehaviour monoBehaviour, System.Action action, float time = 1f)
        {
            var waitForSeconds = new WaitForSeconds(time);
            IEnumerator WaitForSecondsCoroutine()
            {
                yield return waitForSeconds;
                action?.Invoke();
            }
            return monoBehaviour.StartCoroutine(WaitForSecondsCoroutine());
        }
        public static Coroutine WaitForSecondsRealtime(this MonoBehaviour monoBehaviour, System.Action action, float time = 1f)
        {
            var waitForSecondsRealtime = new WaitForSecondsRealtime(time);
            IEnumerator WaitForSecondsRealtimeCoroutine()
            {
                yield return waitForSecondsRealtime;
                action?.Invoke();
            }
            return monoBehaviour.StartCoroutine(WaitForSecondsRealtimeCoroutine());
        }
        public static Coroutine WaitUntil(this MonoBehaviour monoBehaviour, System.Func<bool> condition, System.Action action, float time = 1f)
        {
            var waitUntil = new WaitUntil(condition);
            IEnumerator WaitUntilCoroutine()
            {
                yield return waitUntil;
                action?.Invoke();
            }
            return monoBehaviour.StartCoroutine(WaitUntilCoroutine());
        }
    }
}