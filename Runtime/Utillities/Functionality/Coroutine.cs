using System.Collections;
using UnityEngine;

namespace SFC
{
    public static class Coroutine
    {
        static WaitForEndOfFrame waitForEndOfFrame = new();
        static WaitForFixedUpdate waitForFixedUpdate = new();
        public static void NextUpdate(this MonoBehaviour monoBehaviour, System.Action action, int time = 1)
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
            monoBehaviour.StartCoroutine(NextUpdateCoroutine());
        }
        public static void NextFixedUpdate(this MonoBehaviour monoBehaviour, System.Action action, int time = 1)
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
            monoBehaviour.StartCoroutine(NextFixedUpdateCoroutine());
        }
        public static void WaitForSeconds(this MonoBehaviour monoBehaviour, System.Action action, float time = 1f)
        {
            var waitForSeconds = new WaitForSeconds(time);
            IEnumerator WaitForSecondsCoroutine()
            {
                yield return waitForSeconds;
                action?.Invoke();
            }
            monoBehaviour.StartCoroutine(WaitForSecondsCoroutine());
        }
        public static void WaitForSecondsRealtime(this MonoBehaviour monoBehaviour, System.Action action, float time = 1f)
        {
            var waitForSecondsRealtime = new WaitForSecondsRealtime(time);
            IEnumerator WaitForSecondsRealtimeCoroutine()
            {
                yield return waitForSecondsRealtime;
                action?.Invoke();
            }
            monoBehaviour.StartCoroutine(WaitForSecondsRealtimeCoroutine());
        }
        public static void WaitUntil(this MonoBehaviour monoBehaviour, System.Func<bool> condition, System.Action action, float time = 1f)
        {
            var waitUntil = new WaitUntil(condition);
            IEnumerator WaitUntilCoroutine()
            {
                yield return waitUntil;
                action?.Invoke();
            }
            monoBehaviour.StartCoroutine(WaitUntilCoroutine());
        }
    }
}