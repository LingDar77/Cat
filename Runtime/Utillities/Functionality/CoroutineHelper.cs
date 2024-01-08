using System.Collections;
using UnityEngine;

namespace TUI
{
    public static class CoroutineHelper
    {
        static readonly WaitForEndOfFrame waitForEndOfFrame = new();
        static readonly WaitForFixedUpdate waitForFixedUpdate = new();

        public static MonoBehaviour Context => ISingletonSystem<BaseGameSystem>.GetSingletonChecked();

        public static Coroutine NextUpdate(this MonoBehaviour monoBehaviour, System.Action action, int time = 1)
        {
            return NextUpdate(action, monoBehaviour, time);
        }

        public static Coroutine NextUpdate(System.Action action, MonoBehaviour monoBehaviour = null, int time = 1)
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
            if (monoBehaviour == null) monoBehaviour = Context;
            return monoBehaviour.StartCoroutine(NextUpdateCoroutine());
        }

        public static Coroutine NextFixedUpdate(this MonoBehaviour monoBehaviour, System.Action action, int time = 1)
        {
            return NextFixedUpdate(action, monoBehaviour, time);
        }

        public static Coroutine NextFixedUpdate(System.Action action, MonoBehaviour monoBehaviour = null, int time = 1)
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
            if (monoBehaviour == null) monoBehaviour = Context;
            return monoBehaviour.StartCoroutine(NextFixedUpdateCoroutine());
        }

        public static Coroutine WaitForSeconds(this MonoBehaviour monoBehaviour, System.Action action, float time = 1f)
        {
            return WaitForSeconds(action, monoBehaviour, time);
        }

        public static Coroutine WaitForSeconds(System.Action action, MonoBehaviour monoBehaviour = null, float time = 1f)
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
            return WaitForSecondsRealtime(action, monoBehaviour, time);
        }

        public static Coroutine WaitForSecondsRealtime(System.Action action, MonoBehaviour monoBehaviour = null, float time = 1f)
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