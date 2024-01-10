using System.Collections;
using UnityEngine;

namespace TUI
{
    public static class CoroutineHelper
    {
        public static readonly WaitForEndOfFrame nextUpdate = new();
        public static readonly WaitForFixedUpdate nextFixedUpdate = new();

        public static MonoBehaviour Context => ISingletonSystem<BaseGameSystem>.GetInstance();

        public static Coroutine NextUpdate(this MonoBehaviour monoBehaviour, System.Action action, int time = 1)
        {
            return NextUpdate(action, time, monoBehaviour);
        }

        public static Coroutine NextUpdate(System.Action action, int time = 1, MonoBehaviour monoBehaviour = null)
        {
            IEnumerator NextUpdateCoroutine()
            {
                while (time != 0)
                {
                    yield return nextUpdate;
                    --time;
                }
                action?.Invoke();
            }
            if (monoBehaviour == null) monoBehaviour = Context;
            return monoBehaviour.StartCoroutine(NextUpdateCoroutine());
        }

        public static Coroutine NextFixedUpdate(this MonoBehaviour monoBehaviour, System.Action action, int time = 1)
        {
            return NextFixedUpdate(action, time, monoBehaviour);
        }

        public static Coroutine NextFixedUpdate(System.Action action, int time = 1, MonoBehaviour monoBehaviour = null)
        {
            IEnumerator NextFixedUpdateCoroutine()
            {
                while (time != 0)
                {
                    yield return nextFixedUpdate;
                    --time;
                }
                action?.Invoke();
            }
            if (monoBehaviour == null) monoBehaviour = Context;
            return monoBehaviour.StartCoroutine(NextFixedUpdateCoroutine());
        }

        public static Coroutine WaitForSeconds(this MonoBehaviour monoBehaviour, System.Action action, float time = 1f)
        {
            return WaitForSeconds(action, time, monoBehaviour);
        }

        public static Coroutine WaitForSeconds(System.Action action, float time = 1f, MonoBehaviour monoBehaviour = null)
        {
            var waitForSeconds = new WaitForSeconds(time);
            IEnumerator WaitForSecondsCoroutine()
            {
                yield return waitForSeconds;
                action?.Invoke();
            }
            if (monoBehaviour == null) monoBehaviour = Context;
            return monoBehaviour.StartCoroutine(WaitForSecondsCoroutine());
        }

        public static Coroutine WaitForSecondsRealtime(this MonoBehaviour monoBehaviour, System.Action action, float time = 1f)
        {
            return WaitForSecondsRealtime(action, time, monoBehaviour);
        }

        public static Coroutine WaitForSecondsRealtime(System.Action action, float time = 1f, MonoBehaviour monoBehaviour = null)
        {
            var waitForSecondsRealtime = new WaitForSecondsRealtime(time);
            IEnumerator WaitForSecondsRealtimeCoroutine()
            {
                yield return waitForSecondsRealtime;
                action?.Invoke();
            }
            if (monoBehaviour == null) monoBehaviour = Context;
            return monoBehaviour.StartCoroutine(WaitForSecondsRealtimeCoroutine());
        }

        public static Coroutine WaitUntil(this MonoBehaviour monoBehaviour, System.Func<bool> condition, System.Action action)
        {
            return WaitUntil(condition, action, monoBehaviour);
        }

        public static Coroutine WaitUntil(System.Func<bool> condition, System.Action action, MonoBehaviour monoBehaviour = null)
        {
            var waitUntil = new WaitUntil(condition);
            IEnumerator WaitUntilCoroutine()
            {
                yield return waitUntil;
                action?.Invoke();
            }
            if (monoBehaviour == null) monoBehaviour = Context;
            return monoBehaviour.StartCoroutine(WaitUntilCoroutine());
        }


    }
}