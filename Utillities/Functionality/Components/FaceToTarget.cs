namespace Cat.Utillities
{
    using UnityEngine;
    public class FaceToTarget : MonoBehaviour
    {
        [SerializeField] private Transform target;
        
        private void Start()
        {
            if (target == null) target = Camera.main.transform;
        }

        private void Update()
        {
            transform.LookAt(target);
        }
    }
}