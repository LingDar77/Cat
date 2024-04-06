namespace Cat.Utilities
{
    using UnityEngine;
    public class FaceToTarget : MonoBehaviour
    {
        public enum FaceMode
        {
            OnlyY,
            OnlyYInversed,
            Full,
            FullInversed
        }
        [SerializeField] private Transform target;
        [SerializeField] private FaceMode mode;

        private void OnEnable()
        {
            if (target == null) target = Camera.main.transform;
        }
        private void Update()
        {
            switch (mode)
            {
                case FaceMode.OnlyY:
                    transform.rotation = Quaternion.Euler(0, target.rotation.eulerAngles.y, 0);
                    break;
                case FaceMode.OnlyYInversed:
                    transform.rotation = Quaternion.Inverse(Quaternion.Euler(0, target.rotation.eulerAngles.y, 0));
                    break;
                case FaceMode.Full:
                    transform.rotation = target.rotation;
                    break;
                case FaceMode.FullInversed:
                    transform.rotation = Quaternion.Inverse(target.rotation);
                    break;
            }
        }
    }
}