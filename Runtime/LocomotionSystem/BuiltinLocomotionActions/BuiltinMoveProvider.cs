namespace Cat.LocomotionSystem.Actions
{
    using Cat.Utilities;
    using UnityEngine;
    using UnityEngine.InputSystem;

    [DefaultExecutionOrder(1000)]
    public class BuiltinMoveProvider : ActionProviderBase
    {
        [System.Serializable]
        public enum RotateMethod
        {
            KeepInputDirection,
            AlignToReference
        }

        [SerializeField] private InputActionProperty MoveControl;
        [SerializeField] private Transform ForwardReference;
        [SerializeField] private RotateMethod RotateType;
        [SerializeField] private float MaxMoveSpeed = 2f;
        [SerializeField] private float TurnSpeed = 8f;
        [SerializeField] private bool CanMoveInAir = true;
        private Vector2 moveInput;

        private void Start()
        {
            if (ForwardReference == null) ForwardReference = LocomotionSystem.transform;
        }
        protected void TryApplyGravity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (LocomotionSystem.IsStable()) return;
            currentVelocity.y -= 9.8f * deltaTime;
        }
        public override void BeforeProcess(float deltaTime)
        {
            base.BeforeProcess(deltaTime);
            moveInput = Vector2.zero;
            if (MoveControl != null)
            {
                moveInput = MoveControl.action.ReadValue<Vector2>();
            }
        }
        public override void ProcessVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            base.ProcessVelocity(ref currentVelocity, deltaTime);

            if (!CanMoveInAir && !LocomotionSystem.IsOnGround())
            {
                TryApplyGravity(ref currentVelocity, deltaTime);
                return;
            }

            var targetVelocity = ForwardReference.TransformDirection(new Vector3(moveInput.x, 0, moveInput.y));

            currentVelocity.x = targetVelocity.x * MaxMoveSpeed;
            currentVelocity.z = targetVelocity.z * MaxMoveSpeed;

            TryApplyGravity(ref currentVelocity, deltaTime);
        }

        public override void ProcessRotation(ref Quaternion currentRotation, float deltaTime)
        {
            if (RotateType == RotateMethod.AlignToReference)
            {
                var target = Quaternion.Euler(0, ForwardReference.eulerAngles.y, 0);
                currentRotation = Quaternion.Lerp(currentRotation, target, deltaTime * TurnSpeed / 4);
            }
            else
            {
                if (!MoveControl.action.IsPressed() || moveInput == Vector2.zero) return;
                var target = ForwardReference.rotation * Quaternion.LookRotation(new Vector3(moveInput.x, 0, moveInput.y));
                currentRotation = Quaternion.Euler(0, Quaternion.Slerp(currentRotation, target, deltaTime * TurnSpeed).eulerAngles.y, 0);
            }
        }

    }
}
