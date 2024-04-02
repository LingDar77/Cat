using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Cat
{
    public class AttachToObject : MonoBehaviour
    {
        [SerializeField] private Transform target;
        private void OnEnable()
        {
            transform.SetParent(target);
        }
    }
}
