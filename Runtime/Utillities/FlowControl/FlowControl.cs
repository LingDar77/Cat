using System.Collections;
using UnityEngine;

namespace SFC
{
    public static class FlowControlMonoBehaviour
    {
        public static void EndOfThisFrame(this MonoBehaviour monoBehaviour, System.Action action)
        {
            IEnumerator EndOfThisFrameCoroutine()
            {
                yield return new WaitForEndOfFrame();
                action?.Invoke();
            }
            monoBehaviour.StartCoroutine(EndOfThisFrameCoroutine());
        }
        public static void NextFixedUpdate(this MonoBehaviour monoBehaviour, System.Action action)
        {
            IEnumerator NextFixedUpdateCoroutine()
            {
                yield return new WaitForFixedUpdate();
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