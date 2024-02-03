namespace TUI.Utillities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public static class CoroutineHelper
    {
        public static readonly WaitForEndOfFrame nextUpdate = new();
        public static readonly WaitForFixedUpdate nextFixedUpdate = new();
        private static readonly Dictionary<float, WaitForSeconds> waits = new();
        private static readonly Dictionary<float, WaitForSecondsRealtime> realtimeWaits = new();

        public static MonoBehaviour Context => ISingletonSystem<SystemContent>.GetChecked();

        public static WaitForSeconds WaitFor(float seconds)
        {
            if (!waits.TryGetValue(seconds, out var wait))
            {
                wait = new WaitForSeconds(seconds);
                waits.Add(seconds, wait);
            }
            return wait;
        }

        public static WaitForSecondsRealtime WaitForRealtime(float seconds)
        {
            if (!realtimeWaits.TryGetValue(seconds, out var wait))
            {
                wait = new WaitForSecondsRealtime(seconds);
                realtimeWaits.Add(seconds, wait);
            }
            return wait;
        }

        public static Coroutine NextUpdate(this MonoBehaviour context, System.Action action, int time = 1)
        {
            return NextUpdate(action, time, context);
        }

        public static Coroutine NextUpdate(System.Action action, int time = 1, MonoBehaviour context = null)
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
            if (context == null) context = Context;
            return context.StartCoroutine(NextUpdateCoroutine());
        }

        public static Coroutine NextFixedUpdate(this MonoBehaviour context, System.Action action, int time = 1)
        {
            return NextFixedUpdate(action, time, context);
        }

        public static Coroutine NextFixedUpdate(System.Action action, int time = 1, MonoBehaviour context = null)
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
            if (context == null) context = Context;
            return context.StartCoroutine(NextFixedUpdateCoroutine());
        }

        public static Coroutine WaitForSeconds(this MonoBehaviour context, System.Action action, float time = 1f)
        {
            return WaitForSeconds(action, time, context);
        }

        public static Coroutine WaitForSeconds(System.Action action, float time = 1f, MonoBehaviour context = null)
        {
            var waitForSeconds = WaitFor(time);
            IEnumerator WaitForSecondsCoroutine()
            {
                yield return waitForSeconds;
                action?.Invoke();
            }
            if (context == null) context = Context;
            return context.StartCoroutine(WaitForSecondsCoroutine());
        }

        public static Coroutine WaitForSecondsRealtime(this MonoBehaviour context, System.Action action, float time = 1f)
        {
            return WaitForSecondsRealtime(action, time, context);
        }

        public static Coroutine WaitForSecondsRealtime(System.Action action, float time = 1f, MonoBehaviour context = null)
        {
            var waitForSecondsRealtime = WaitForRealtime(time);
            IEnumerator WaitForSecondsRealtimeCoroutine()
            {
                yield return waitForSecondsRealtime;
                action?.Invoke();
            }
            if (context == null) context = Context;
            return context.StartCoroutine(WaitForSecondsRealtimeCoroutine());
        }

        public static Coroutine WaitUntil(this MonoBehaviour context, System.Func<bool> condition, System.Action action)
        {
            return WaitUntil(condition, action, context);
        }

        public static Coroutine WaitUntil(System.Func<bool> condition, System.Action action, MonoBehaviour context = null)
        {
            var waitUntil = new WaitUntil(condition);
            IEnumerator WaitUntilCoroutine()
            {
                yield return waitUntil;
                action?.Invoke();
            }
            if (context == null) context = Context;
            return context.StartCoroutine(WaitUntilCoroutine());
        }


    }
}