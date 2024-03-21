namespace Cat.Utilities
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Cat;

    public static class CoroutineHelper
    {
        private static readonly WaitForEndOfFrame nextUpdate = new();
        private static readonly WaitForFixedUpdate nextFixedUpdate = new();
        private static readonly Dictionary<float, WaitForSeconds> waits = new();
        private static readonly Dictionary<float, WaitForSecondsRealtime> realtimeWaits = new();

        public static MonoBehaviour Context => CatContent.Instance;
        
        public static WaitForEndOfFrame GetNextUpdate()
        {
            return nextUpdate;
        }

        public static WaitForFixedUpdate GetNextFixedUpdate()
        {
            return nextFixedUpdate;
        }

        public static WaitForSeconds GetWaitForSeconds(float seconds)
        {
            if (!waits.TryGetValue(seconds, out var wait))
            {
                wait = new WaitForSeconds(seconds);
                waits.Add(seconds, wait);
            }
            return wait;
        }

        public static WaitForSecondsRealtime GetWaitForRealtime(float seconds)
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
            if (context == null) context = Context;
            return context.StartCoroutine(NextUpdateCoroutine(time, action));
        }

        private static IEnumerator NextUpdateCoroutine(int time, System.Action action)
        {
            while (time != 0)
            {
                yield return nextUpdate;
                --time;
            }
            action?.Invoke();
        }

        public static Coroutine NextFixedUpdate(this MonoBehaviour context, System.Action action, int time = 1)
        {
            return NextFixedUpdate(action, time, context);
        }

        public static Coroutine NextFixedUpdate(System.Action action, int time = 1, MonoBehaviour context = null)
        {
            if (context == null) context = Context;
            return context.StartCoroutine(NextFixedUpdateCoroutine(time, action));
        }

        private static IEnumerator NextFixedUpdateCoroutine(int time, System.Action action)
        {
            while (time != 0)
            {
                yield return nextFixedUpdate;
                --time;
            }
            action?.Invoke();
        }

        public static Coroutine WaitForSeconds(this MonoBehaviour context, System.Action action, float time = 1f)
        {
            return WaitForSeconds(action, time, context);
        }

        public static Coroutine WaitForSeconds(System.Action action, float time = 1f, MonoBehaviour context = null)
        {
            if (context == null) context = Context;
            return context.StartCoroutine(WaitForSecondsCoroutine(time, action));
        }

        private static IEnumerator WaitForSecondsCoroutine(float time, System.Action action)
        {
            yield return GetWaitForSeconds(time);
            action?.Invoke();
        }

        public static Coroutine WaitForSecondsRealtime(this MonoBehaviour context, System.Action action, float time = 1f)
        {
            return WaitForSecondsRealtime(action, time, context);
        }

        public static Coroutine WaitForSecondsRealtime(System.Action action, float time = 1f, MonoBehaviour context = null)
        {

            if (context == null) context = Context;
            return context.StartCoroutine(WaitForSecondsRealtimeCoroutine(time, action));
        }

        private static IEnumerator WaitForSecondsRealtimeCoroutine(float time, System.Action action)
        {
            yield return GetWaitForRealtime(time);
            action?.Invoke();
        }

        public static Coroutine WaitUntil(this MonoBehaviour context, System.Func<bool> condition, System.Action action)
        {
            return WaitUntil(condition, action, context);
        }

        public static Coroutine WaitUntil(System.Func<bool> condition, System.Action action, MonoBehaviour context = null)
        {

            if (context == null) context = Context;
            return context.StartCoroutine(WaitUntilCoroutine(condition, action));
        }

        private static IEnumerator WaitUntilCoroutine(System.Func<bool> condition, System.Action action)
        {
            yield return new WaitUntil(condition);
            action?.Invoke();
        }
    }
}