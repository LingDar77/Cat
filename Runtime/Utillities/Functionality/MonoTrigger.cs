namespace Cat.Utillities
{
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// MonoTrigger is used for the case when multiple triggers in a game object,
    /// and they need to work seperately.
    /// </summary>
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
