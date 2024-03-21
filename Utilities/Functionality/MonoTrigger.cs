namespace Cat.Utilities
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

        public CatDriver<bool, Collider> driver;

        private void OnTriggerEnter(Collider other)
        {
            TriggerEntered.Invoke(other);
            if (driver == null) return;
            driver.Drive(true, other);
        }
        private void OnTriggerExit(Collider other)
        {
            TriggerExited.Invoke(other);
            if (driver == null) return;
            driver.Drive(false, other);
        }

    }
}
