namespace Cat.LocomotionSystem.Actions
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class BuiltinCrouchProvider : ActionProviderBase
    {
        public float MinimumCrouchHeight = 1f;
        public float CrouchSpeed = 5;
        [Tooltip("The Key to Perform Crouch.")]
        [SerializeField] private InputActionProperty CrouchControl;
        [SerializeField] private Transform offset;
        private CapsuleCollider capsule;
        private float initalCapsuleHeight;
        private float initialCapsuleOffset;
        private float initialOffset;

        protected override void OnEnable()
        {
            base.OnEnable();
            capsule = LocomotionSystem.transform.GetComponent<CapsuleCollider>();
            initalCapsuleHeight = capsule.height;
            initialCapsuleOffset = initalCapsuleHeight / 2 - capsule.center.y;
            initialOffset = offset.localPosition.y;
        }

        public override void BeforeProcess(float deltaTime)
        {
            if (CrouchControl.action.IsPressed())
            {
                var targetHeight = Mathf.Lerp(capsule.height, MinimumCrouchHeight, deltaTime * CrouchSpeed);
                capsule.height = targetHeight;
                capsule.center = new Vector3(capsule.center.x, initalCapsuleHeight - targetHeight / 2 - initialCapsuleOffset, capsule.center.z);
            }
            else
            {
                var targetHeight = Mathf.Lerp(capsule.height, initalCapsuleHeight, deltaTime * CrouchSpeed);
                capsule.height = targetHeight + offset.localPosition.y;
                capsule.center = new Vector3(capsule.center.x, initalCapsuleHeight - targetHeight / 2 - initialCapsuleOffset, capsule.center.z);
            }
        }
    }
}
