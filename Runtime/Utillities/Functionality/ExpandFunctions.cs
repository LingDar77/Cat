using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TUI.Utillities
{
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
        public static Vector3 TransformVelocityTowards(this Vector2 input, Transform towards, Transform origin)
        {
            if (input == Vector2.zero)
                return Vector3.zero;

            var inputForwardInWorldSpace = towards.forward;
            if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(inputForwardInWorldSpace, origin.up)), 1f))
            {
                inputForwardInWorldSpace = -towards.up;
            }

            var inputForwardProjectedInWorldSpace = Vector3.ProjectOnPlane(inputForwardInWorldSpace, origin.up);
            var forwardRotation = Quaternion.FromToRotation(origin.forward, inputForwardProjectedInWorldSpace);

            var translationInRigSpace = forwardRotation * new Vector3(input.x, 0f, input.y);
            var translationInWorldSpace = origin.TransformDirection(translationInRigSpace);

            return translationInWorldSpace;
        }
        public static Vector3 TransformVelocityTowards(this Vector3 input, Transform towards, Transform origin)
        {
            if (input == Vector3.zero)
                return Vector3.zero;

            var inputForwardInWorldSpace = towards.forward;
            if (Mathf.Approximately(Mathf.Abs(Vector3.Dot(inputForwardInWorldSpace, origin.up)), 1f))
            {
                inputForwardInWorldSpace = -towards.up;
            }

            var inputForwardProjectedInWorldSpace = Vector3.ProjectOnPlane(inputForwardInWorldSpace, origin.up);
            var forwardRotation = Quaternion.FromToRotation(origin.forward, inputForwardProjectedInWorldSpace);

            var translationInRigSpace = forwardRotation * input;
            var translationInWorldSpace = origin.TransformDirection(translationInRigSpace);

            return translationInWorldSpace;
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

        public static Vector2 XZ(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }
        public static Vector2 XY(this Vector3 v)
        {
            return new Vector2(v.x, v.y);
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

        #region GameObject Expand
        public static void DestroyAllChildren(this GameObject content)
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
        public static void DestroyAllChildren(this GameObject content, System.Action<Object> method)
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
        #endregion
    }
}