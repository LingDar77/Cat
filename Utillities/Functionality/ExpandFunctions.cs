namespace Cat.Utillities
{
    using System.Collections.Generic;
    using System.Linq;
    using Cat.Library;
    using Cat.ScreenLogManagementSystem;
    using UnityEngine;
    using UnityEngine.Audio;

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
        public static void DestroyAllChildren(this Component content)
        {
            for (var i = 0; i != content.transform.childCount; ++i)
            {
                GameObject.Destroy(content.transform.GetChild(i).gameObject);
            }
        }
        public static void DestroyAllChildren(this Transform content)
        {
            for (var i = 0; i != content.childCount; ++i)
            {
                GameObject.Destroy(content.GetChild(i).gameObject);
            }
        }
        public static void DestroyAllChildren(this Component content, System.Action<Object> method)
        {
            var children = new List<Transform>();
            for (var i = 0; i != content.transform.childCount; ++i)
            {
                children.Add(content.transform.GetChild(i));
            }
            foreach (var child in children)
            {
                method(child.gameObject);
            }
        }
        public static void DestroyAllChildren(this Transform content, System.Action<Object> method)
        {
            var children = new List<Transform>();
            for (var i = 0; i != content.childCount; ++i)
            {
                children.Add(content.GetChild(i));
            }
            foreach (var child in children)
            {
                method(child.gameObject);
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
        public static void Log(this Component context, string message, LogType type = LogType.Log)
        {
#if UNITY_EDITOR
            LogToConsole(context, message, type);
#else
            LogToScreen(context, message, type);
#endif
        }

        /// <summary>
        /// Log to unity console to gain more trace infomation.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="type"></param>
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

        /// <summary>
        /// Provide less trace infomation to reduce gc alloc at run time.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message"></param>
        /// <param name="type"></param> <summary>
        public static void LogToScreen(this Component context, string message, LogType type = LogType.Log)
        {
            if (IScreenLogManagement.Singleton == null) return;
            var trace = context.GetType().FullName;
            IScreenLogManagement.Singleton.LogToScreen(type, message, trace);
        }

        public static void LogFormat(this Component context, string format, LogType type = LogType.Log, params string[] args)
        {
            using (zstring.Block())
            {
                var zargs = new zstring[args.Length];
                for (int i = 0; i != args.Length; ++i)
                {
                    zargs[i] = args[i];
                }
                Log(context, zstring.Format(format, zargs), type);

            }
        }
        public static void LogFormatToScreen(this Component context, string format, LogType type = LogType.Log, params string[] args)
        {
            using (zstring.Block())
            {
                var zargs = new zstring[args.Length];
                for (int i = 0; i != args.Length; ++i)
                {
                    zargs[i] = args[i];
                }
                LogToScreen(context, zstring.Format(format, zargs), type);

            }
        }
        public static void LogFormatToConsole(this Component context, string format, LogType type = LogType.Log, params string[] args)
        {
            using (zstring.Block())
            {
                var zargs = new zstring[args.Length];
                for (int i = 0; i != args.Length; ++i)
                {
                    zargs[i] = args[i];
                }
                LogToConsole(context, zstring.Format(format, zargs), type);
            }
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
            return list[Random.Range(0, list.Count)];
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
}