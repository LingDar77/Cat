namespace Cat.Utilities
{
    using System.Buffers;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Cat.PoolingSystem;
    using Cat.ScreenLogManagementSystem;
    using UnityEngine;
    using UnityEngine.Audio;
    using Debug = UnityEngine.Debug;

    public static class ExpandFunctions
    {
        #region Math Expand
        public static float MaxWithThreshold(this float lhs, float rhs = 0f, float threshold = .01f)
        {
            return Mathf.Abs(lhs) > rhs + threshold ? lhs : rhs;
        }
        public static bool NearlyEqualsTo(this float lhs, float rhs, float threshold = .01f)
        {
            return Mathf.Abs(lhs - rhs) <= threshold;
        }
        public static bool NearlyEqualsTo(this Vector3 lhs, Vector3 rhs, float threshold = .01f)
        {
            return lhs.x.NearlyEqualsTo(rhs.x, threshold) && lhs.y.NearlyEqualsTo(rhs.y, threshold) && lhs.z.NearlyEqualsTo(rhs.z, threshold);
        }
        public static bool NearlyEqualsTo(this Vector2 lhs, Vector2 rhs, float threshold = .01f)
        {
            return lhs.x.NearlyEqualsTo(rhs.x, threshold) && lhs.y.NearlyEqualsTo(rhs.y, threshold);
        }

        public static string LimitDecimal(this float number, int totalDigits, bool fixedLength = true)
        {
            var n = number.ToString();
            if (n.Length < totalDigits)
            {
                if (fixedLength)
                {
                    while (n.Length != totalDigits)
                    {
                        n += "0";
                    }
                }
                return n.ToString();
            }
            return number.ToString()[..totalDigits];
        }

        public static float NormalizeAngle(this float angle, float max = 180f)
        {
            angle = Mathf.Repeat(angle + max, 360f) + max - 360f;
            return angle;
        }

        public static Vector3 GetDirectionTangentToSurface(this Vector3 direction, Vector3 surfaceNormal, Vector3 up)
        {
            Vector3 directionRight = Vector3.Cross(direction, up);
            return Vector3.Cross(surfaceNormal, directionRight).normalized;
        }

        public static Vector2 XZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }
        public static Vector2 XY(this Vector3 v)
        {
            return (Vector2)v;
        }
        public static Vector2 YZ(this Vector3 v)
        {
            return new Vector2(v.y, v.z);
        }
        public static Vector2 YX(this Vector3 v)
        {
            return new Vector2(v.y, v.x);
        }
        public static Vector2 ZX(this Vector3 v)
        {
            return new Vector2(v.z, v.x);
        }
        public static Vector2 ZY(this Vector3 v)
        {
            return new Vector2(v.z, v.y);
        }
        #endregion

        #region Compoent Expand
        public static bool TryGetComponentInParent<Type>(this Component context, out Type component, bool includeInactive = false)
        {
            component = (Type)(object)context.GetComponentInParent(typeof(Type), includeInactive);
            return component != null;
        }
        public static bool TryGetComponentInChildren<Type>(this Component context, out Type component, bool includeInactive = false)
        {
            component = (Type)(object)context.GetComponentInChildren(typeof(Type), includeInactive);
            return component != null;
        }

        public static void DestroyOrDispose(this Transform content)
        {
            if (content.TryGetComponent<BuiltinPooledGameObject>(out var obj))
            {
                obj.Dispose();
                return;
            }
            GameObject.DestroyImmediate(content.gameObject);
        }

        public static void DestroyAllChildren(this Transform content)
        {
            while (content.childCount != 0)
            {
                var child = content.GetChild(0);
                child.DestroyOrDispose();
            }
        }

        public static void EnsureComponent<ComponentType>(this Component content, ref ComponentType component)
        {
#if UNITY_EDITOR
#pragma warning disable UNT0014 // Invalid type for call to GetComponent
#pragma warning disable IDE0074 // 使用复合分配
            if (component == null) component = content.GetComponent<ComponentType>();
#pragma warning restore IDE0074 // 使用复合分配
#pragma warning restore UNT0014 // Invalid type for call to GetComponent
#endif
        }
        public static void EnsureComponentInChildren<ComponentType>(this Component content, ref ComponentType component)
        {
#if UNITY_EDITOR
#pragma warning disable UNT0014 // Invalid type for call to GetComponent
#pragma warning disable IDE0074 // 使用复合分配
            if (component == null) component = content.GetComponentInChildren<ComponentType>(true);
#pragma warning restore IDE0074 // 使用复合分配
#pragma warning restore UNT0014 // Invalid type for call to GetComponent
#endif
        }
        public static void EnsureComponentInParent<ComponentType>(this Component content, ref ComponentType component)
        {
#if UNITY_EDITOR
#pragma warning disable UNT0014 // Invalid type for call to GetComponent
#pragma warning disable IDE0074 // 使用复合分配
            if (component == null) component = content.GetComponentInParent<ComponentType>(true);
#pragma warning restore IDE0074 // 使用复合分配
#pragma warning restore UNT0014 // Invalid type for call to GetComponent
#endif
        }

        #endregion

        #region AudioSource Expand
        public static AudioSource Clip(this AudioSource source, AudioClip clip)
        {
            source.clip = clip;
            return source;
        }
        public static AudioSource SpatialBlend(this AudioSource source, float spatialBlend)
        {
            source.spatialBlend = spatialBlend;
            return source;
        }
        public static AudioSource Loop(this AudioSource source, bool loop)
        {
            source.loop = loop;
            return source;
        }
        public static AudioSource Volume(this AudioSource source, float volume)
        {
            source.volume = volume;
            return source;
        }
        public static AudioSource MixerGroup(this AudioSource source, AudioMixerGroup mixerGroup)
        {
            source.outputAudioMixerGroup = mixerGroup;
            return source;
        }
        public static void PlayAWhile(this AudioSource source, float time)
        {
            source.Play();
            CoroutineHelper.WaitForSeconds(() => { source.Stop(); }, time);
        }
        #endregion

        #region Collection Expand
        /// <summary>
        /// Get a random element in a list.
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static Type RandomElement<Type>(this IList<Type> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
        /// <summary>
        /// Note that a set has not ability to random access,
        /// this action will convert it to a array to do random access.
        /// It's better to cache the array and use list version for better performence.
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="set"></param>
        /// <returns></returns>
        public static Type RandomElement<Type>(this ISet<Type> set)
        {
            var array = set.ToArray();
            return array.RandomElement();
        }

        public static int IndexOf<Type>(this Type[] collection, Type value) where Type : class
        {
            for (int i = 0; i != collection.Length; ++i)
            {
                if (value == collection[i]) return i;
            }
            return -1;
        }

        public static int IndexOf<Type>(this IEnumerable<Type> collection, Type value) where Type : class
        {
            var i = 0;
            foreach (var item in collection)
            {
                if (value == item) return i;
                ++i;
            }
            return -1;
        }

        #endregion

    }

    public static class LogExpandFunctions
    {
        [Conditional("INCLUDE_LOG")]
        public static void Log(this Component context, string message, LogType type = LogType.Log)
        {
#if UNITY_EDITOR
            LogToConsole(context, message, type);
#else
            LogToScreen(context, message, type);
#endif
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogToConsole(Component context, string message, LogType type)
        {
            switch (type)
            {
                case LogType.Log:
                    Debug.Log(message, context);
                    break;
                case LogType.Warning:
                    Debug.LogWarning(message, context);
                    break;
                default:
                    Debug.LogError(message, context);
                    break;
            }
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogToScreen(this Component context, string message, LogType type = LogType.Log)
        {
            if (IScreenLogManagement.Singleton == null) return;
            var trace = context.GetType().FullName;
            IScreenLogManagement.Singleton.LogToScreen(type, message, trace);
        }



        [Conditional("INCLUDE_LOG")]
        public static void LogFormat(this Component context, string format, LogType type = LogType.Log, params object[] args)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(args.Length);
            for (int i = 0; i != args.Length; ++i)
            {
                zargs[i] = args[i].ToString();
            }
            Log(context, zstring.Format(format, zargs, args.Length), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToScreen(this Component context, string format, LogType type = LogType.Log, params object[] args)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(args.Length);
            for (int i = 0; i != args.Length; ++i)
            {
                zargs[i] = args[i].ToString();
            }
            LogToScreen(context, zstring.Format(format, zargs, args.Length), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToConsole(this Component context, string format, LogType type = LogType.Log, params object[] args)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(args.Length);
            for (int i = 0; i != args.Length; ++i)
            {
                zargs[i] = args[i].ToString();
            }
            LogToConsole(context, zstring.Format(format, zargs, args.Length), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }



        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToScreen<T0>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(1);
            zargs[0] = arg0.ToString();
            LogToScreen(context, zstring.Format(format, zargs, 1), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToScreen<T0, T1>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(2);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            LogToScreen(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToScreen<T0, T1, T2>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(3);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            LogToScreen(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToScreen<T0, T1, T2, T3>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(4);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            LogToScreen(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToScreen<T0, T1, T2, T3, T4>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(5);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            LogToScreen(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToScreen<T0, T1, T2, T3, T4, T5>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(6);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            zargs[cnt++] = arg5.ToString();
            LogToScreen(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToScreen<T0, T1, T2, T3, T4, T5, T6>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(7);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            zargs[cnt++] = arg5.ToString();
            zargs[cnt++] = arg6.ToString();
            LogToScreen(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToScreen<T0, T1, T2, T3, T4, T5, T6, T7>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(8);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            zargs[cnt++] = arg5.ToString();
            zargs[cnt++] = arg6.ToString();
            zargs[cnt++] = arg7.ToString();
            LogToScreen(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToScreen<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default, T8 arg8 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(9);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            zargs[cnt++] = arg5.ToString();
            zargs[cnt++] = arg6.ToString();
            zargs[cnt++] = arg7.ToString();
            zargs[cnt++] = arg8.ToString();
            LogToScreen(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToScreen<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default, T8 arg8 = default, T9 arg9 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(10);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            zargs[cnt++] = arg5.ToString();
            zargs[cnt++] = arg6.ToString();
            zargs[cnt++] = arg7.ToString();
            zargs[cnt++] = arg8.ToString();
            zargs[cnt++] = arg9.ToString();
            LogToScreen(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }



        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToConsole<T0>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(1);
            zargs[0] = arg0.ToString();
            LogToConsole(context, zstring.Format(format, zargs, 1), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToConsole<T0, T1>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(2);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            LogToConsole(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToConsole<T0, T1, T2>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(3);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            LogToConsole(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToConsole<T0, T1, T2, T3>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(4);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            LogToConsole(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToConsole<T0, T1, T2, T3, T4>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(5);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            LogToConsole(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToConsole<T0, T1, T2, T3, T4, T5>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(6);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            zargs[cnt++] = arg5.ToString();
            LogToConsole(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToConsole<T0, T1, T2, T3, T4, T5, T6>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(7);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            zargs[cnt++] = arg5.ToString();
            zargs[cnt++] = arg6.ToString();
            LogToConsole(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToConsole<T0, T1, T2, T3, T4, T5, T6, T7>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(8);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            zargs[cnt++] = arg5.ToString();
            zargs[cnt++] = arg6.ToString();
            zargs[cnt++] = arg7.ToString();
            LogToConsole(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToConsole<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default, T8 arg8 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(9);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            zargs[cnt++] = arg5.ToString();
            zargs[cnt++] = arg6.ToString();
            zargs[cnt++] = arg7.ToString();
            zargs[cnt++] = arg8.ToString();
            LogToConsole(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormatToConsole<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default, T8 arg8 = default, T9 arg9 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(10);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            zargs[cnt++] = arg5.ToString();
            zargs[cnt++] = arg6.ToString();
            zargs[cnt++] = arg7.ToString();
            zargs[cnt++] = arg8.ToString();
            zargs[cnt++] = arg9.ToString();
            LogToConsole(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }



        [Conditional("INCLUDE_LOG")]
        public static void LogFormat<T0>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(1);
            zargs[0] = arg0.ToString();
            Log(context, zstring.Format(format, zargs, 1), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormat<T0, T1>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(2);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            Log(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormat<T0, T1, T2>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(3);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            Log(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormat<T0, T1, T2, T3>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(4);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            Log(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormat<T0, T1, T2, T3, T4>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(5);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            Log(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormat<T0, T1, T2, T3, T4, T5>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(6);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            zargs[cnt++] = arg5.ToString();
            Log(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormat<T0, T1, T2, T3, T4, T5, T6>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(7);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            zargs[cnt++] = arg5.ToString();
            zargs[cnt++] = arg6.ToString();
            Log(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormat<T0, T1, T2, T3, T4, T5, T6, T7>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(8);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            zargs[cnt++] = arg5.ToString();
            zargs[cnt++] = arg6.ToString();
            zargs[cnt++] = arg7.ToString();
            Log(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default, T8 arg8 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(9);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            zargs[cnt++] = arg5.ToString();
            zargs[cnt++] = arg6.ToString();
            zargs[cnt++] = arg7.ToString();
            zargs[cnt++] = arg8.ToString();
            Log(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

        [Conditional("INCLUDE_LOG")]
        public static void LogFormat<T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(this Component context, string format, LogType type = LogType.Log, T0 arg0 = default, T1 arg1 = default, T2 arg2 = default, T3 arg3 = default, T4 arg4 = default, T5 arg5 = default, T6 arg6 = default, T7 arg7 = default, T8 arg8 = default, T9 arg9 = default)
        {
            using var block = zstring.Block();
            var zargs = ArrayPool<zstring>.Shared.Rent(10);
            var cnt = 0;
            zargs[cnt++] = arg0.ToString();
            zargs[cnt++] = arg1.ToString();
            zargs[cnt++] = arg2.ToString();
            zargs[cnt++] = arg3.ToString();
            zargs[cnt++] = arg4.ToString();
            zargs[cnt++] = arg5.ToString();
            zargs[cnt++] = arg6.ToString();
            zargs[cnt++] = arg7.ToString();
            zargs[cnt++] = arg8.ToString();
            zargs[cnt++] = arg9.ToString();
            Log(context, zstring.Format(format, zargs, cnt), type);
            ArrayPool<zstring>.Shared.Return(zargs);
        }

    }
}