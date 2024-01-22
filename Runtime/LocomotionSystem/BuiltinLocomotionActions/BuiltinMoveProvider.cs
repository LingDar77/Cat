using TUI.LocomotionSystem.Actions;
using TUI.Utillities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace TUI
{
    public class BuiltinMoveProvider : ActionProviderBase
    {
        [SerializeField] private InputActionProperty primaryControl;
        [SerializeField] private InputActionProperty secondaryControl;
        [SerializeField] private Transform forwardReference;
        [SerializeField] private float MaxMoveSpeed = 2f;
        [SerializeField] private bool CanMoveInAir = true;
        public UnityEvent<float> OnVelocityUpdated;
        private Vector2 moveInput;

        protected void TryApplyGravity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (LocomotionSystem.IsOnGround() && LocomotionSystem.IsStable()) return;
            currentVelocity.y -= 9.8f * deltaTime;
        }
        public override void BeforeProcess(float deltaTime)
        {
            base.BeforeProcess(deltaTime);
            moveInput = Vector2.zero;
            if (primaryControl != null)
            {
                moveInput = primaryControl.action.ReadValue<Vector2>();
            }
            if (secondaryControl != null)
            {
                moveInput += secondaryControl.action.ReadValue<Vector2>();
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

            var referencedMoveInputValue = moveInput.TransformVelocityTowards(forwardReference, transform).normalized;
            currentVelocity.x = referencedMoveInputValue.x;
            currentVelocity.z = referencedMoveInputValue.z;

            OnVelocityUpdated.Invoke(new Vector2(currentVelocity.x, currentVelocity.z).magnitude);

            TryApplyGravity(ref currentVelocity, deltaTime);
        }

        public override void ProcessRotation(ref Quaternion currentRotation, float deltaTime)
        {
            currentRotation = Quaternion.Euler(0, forwardReference.eulerAngles.y, 0);
        }

    }
}
