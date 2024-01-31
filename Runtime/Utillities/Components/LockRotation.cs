namespace TUI.Utillities
{
    using UnityEngine;

    public class LockRotation : MonoBehaviour
    {
        [SerializeField] private bool UseInitialRotation = true;
        [SerializeField] private Quaternion TargetRotation = Quaternion.identity;
        private Quaternion initialRotation;

        public void SetTargetRotation(Quaternion target)
        {
            TargetRotation = target;
        }
        private void OnEnable()
        {
            initialRotation = transform.rotation;
        }
        private void Update()
        {
            if (UseInitialRotation)
                transform.rotation = initialRotation;
            else
                transform.rotation = TargetRotation;
        }
    }
}