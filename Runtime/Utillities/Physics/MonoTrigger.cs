using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace SFC.Utillities
{

    public class MonoTrigger : MonoBehaviour
    {
        public UnityEvent<Collider> TriggerEntered;
        public UnityEvent<Collider> TriggerExited;
        public UnityEvent<Collider> TriggerStayed;

        private void OnTriggerEnter(Collider other)
        {
            TriggerEntered.Invoke(other);
        }
        private void OnTriggerExit(Collider other)
        {
            TriggerExited.Invoke(other);
        }
        private void OnTriggerStay(Collider other)
        {
            TriggerStayed.Invoke(other);
        }
    }
}
