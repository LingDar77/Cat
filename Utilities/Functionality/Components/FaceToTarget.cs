namespace Cat.Utilities
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class FaceToTarget : MonoBehaviour
    {
        public enum FaceMode
        {
            OnlyY,
            OnlyYInversed,
            Full,
            FullInversed
        }

        public enum AlignMode
        {
            None,
            AlignY,
            AlignAll
        }
        [SerializeField] private Transform target;
        [SerializeField] private FaceMode mode;
        [SerializeField] private AlignMode align;

        private void OnEnable()
        {
            if (target == null) target = Camera.main.transform;

            InputSystem.onAfterUpdate += OnUpdate;
            OnUpdate();
        }

        private void OnDisable()
        {
            InputSystem.onAfterUpdate -= OnUpdate;
        }

        private void OnUpdate()
        {
            switch (align)
            {
                case AlignMode.AlignY:
                    transform.position = new Vector3(transform.position.x, target.position.y, transform.position.z);
                    break;
                case AlignMode.AlignAll:
                    transform.position = target.position;
                    break;
            }
            switch (mode)
            {
                case FaceMode.OnlyY:
                    transform.rotation = Quaternion.Euler(0, target.rotation.eulerAngles.y, 0);
                    break;
                case FaceMode.OnlyYInversed:
                    transform.rotation = Quaternion.Euler(0, target.rotation.eulerAngles.y + 180, 0);
                    break;
                case FaceMode.Full:
                    transform.rotation = target.rotation;
                    break;
                case FaceMode.FullInversed:
                    transform.rotation = target.rotation * Quaternion.Euler(0, 180, 0);
                    break;
            }
        }
    }
}