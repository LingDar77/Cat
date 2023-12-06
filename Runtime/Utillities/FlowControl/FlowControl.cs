using System.Collections;
using UnityEngine;

namespace SFC
{
    public static class FlowControlMonoBehaviour
    {
        public static void NextUpdate(this MonoBehaviour monoBehaviour, System.Action action, int time = 1)
        {
            IEnumerator NextUpdateCoroutine()
            {
                var wait = new WaitForEndOfFrame();
                while (time != 0)
                {
                    yield return wait;
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
                var wait = new WaitForFixedUpdate();
                while (time != 0)
                {
                    yield return wait;
                    --time;
                }
                action?.Invoke();
            }
            monoBehaviour.StartCoroutine(NextFixedUpdateCoroutine());
        }
        public static void WaitForSeconds(this MonoBehaviour monoBehaviour, System.Action action, float time = 1f)
        {
            IEnumerator WaitForSecondsCoroutine()
            {
                yield return new WaitForSeconds(time);
                action?.Invoke();
            }
            monoBehaviour.StartCoroutine(WaitForSecondsCoroutine());
        }
        public static void WaitForSecondsRealtime(this MonoBehaviour monoBehaviour, System.Action action, float time = 1f)
        {
            IEnumerator WaitForSecondsRealtimeCoroutine()
            {
                yield return new WaitForSecondsRealtime(time);
                action?.Invoke();
            }
            monoBehaviour.StartCoroutine(WaitForSecondsRealtimeCoroutine());
        }
        public static void WaitUntil(this MonoBehaviour monoBehaviour, System.Func<bool> condition, System.Action action, float time = 1f)
        {
            IEnumerator WaitUntilCoroutine()
            {
                yield return new WaitUntil(condition);
                action?.Invoke();
            }
            monoBehaviour.StartCoroutine(WaitUntilCoroutine());
        }
    }
}