namespace Cat.Utillities
{
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    public class ClearEmptyObjects
    {
        [MenuItem("Window/Cat/Scene/Clear Empty Objects")]
        static void ClearAllEmptyObjects()
        {
            foreach (var obj in Object.FindObjectsOfType<GameObject>())
            {
                var components = obj.GetComponentsInChildren<Component>();
                if(components.All(e=>e is Transform))
                {
                    Object.DestroyImmediate(obj);
                }
            }
        }
    }
}