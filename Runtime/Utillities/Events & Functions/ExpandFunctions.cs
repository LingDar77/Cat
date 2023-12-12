using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SFC.Utillities
{
    public static class ExpandFunctions
    {
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
        /// Get a random object in a list.
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static Type Random<Type>(this IList<Type> list)
        {
            return list[UnityEngine.Random.Range(0, list.Count)];
        }
        /// <summary>
        /// Note that a set has not ability to random access,
        /// this action will convert it to a array to do random access.
        /// </summary>
        /// <typeparam name="Type"></typeparam>
        /// <param name="set"></param>
        /// <returns></returns>
        public static Type Random<Type>(this ISet<Type> set)
        {
            var array = set.ToArray();
            return array.Random();
        }
        #endregion
    }
}