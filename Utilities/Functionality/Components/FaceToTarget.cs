namespace Cat.Utilities
{
    using UnityEngine;
    public class FaceToTarget : MonoBehaviour
    {
        [SerializeField] private Transform target;
        private void OnEnable()
        {
            if (target == null) target = Camera.main.transform;
            transform.rotation = target.rotation;
        }
        private void Update()
        {
            transform.rotation = target.rotation;
        }
    }
}