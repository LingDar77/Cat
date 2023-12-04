using SFC.KinematicLocomotionSystem;
using SFC.KinematicLocomotionSystem.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SFC.Intergration.XRIT.KinematicLocomotionSystem.Actions
{
    /// <summary>
    /// A basic crouch action implementation for vr charactor.
    /// </summary>
    public class BasicCrouchActionProvider : ActionProviderBase
    {

        [Header("Inputs")]
        [Tooltip("The Inputed HMD Velocity.")]
        [SerializeField] private InputActionProperty HeadVelocityInput;
        [Tooltip("The Inputed HMD User Presence.")]
        [SerializeField] private InputActionProperty UserPresenceInput;
        [Tooltip("The Inputed HMD Is Tracked Action.")]
        [SerializeField] private InputActionProperty IsTrackedInput;
        [Tooltip("The Offset Transform to Control the Capsule Height and Bias.")]
        [SerializeField] private Transform Offset;
        [Tooltip("The Minimal Crouch Height.")]
        [SerializeField] private float MinCrouchHeight = 1.3f;

        [Header("Simulation Input")]
        [Tooltip("The Inputed Crouch Action.")]
        [SerializeField] private InputActionProperty CrouchInput;

        private KinematicCharacterMotor motor;
        private float normalHeight;
        private float currentCrouchHeight;

        /// <summary>
        /// Reset the current height to its original height.
        /// Commonly used to ajust charactor height when the player is switched another pose.
        /// </summary>
        [ContextMenu("Reset Head Height")]
        public void ResetHeadHeight()
        {
            currentCrouchHeight = normalHeight;
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            UserPresenceInput.action.performed += OnUserPresenced;
            if (LocomotionSystem is KinematicLocomotionController controller)
            {
                motor = controller.Motor;
                normalHeight = controller.Motor.Capsule.height;
            }
            ResetHeadHeight();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            UserPresenceInput.action.performed -= OnUserPresenced;
        }

        private void OnUserPresenced(InputAction.CallbackContext context)
        {
            ResetHeadHeight();
        }

        public override void BeforeProcess(float deltaTime)
        {
            base.BeforeProcess(deltaTime);
            if (!LocomotionSystem.IsStableOnGround()) return;

            var bias = HeadVelocityInput.action.ReadValue<Vector3>().y * deltaTime;

            if (!UserPresenceInput.action.IsPressed() && !IsTrackedInput.action.IsPressed())
            {
                bias = CrouchInput.action.IsPressed() ? -normalHeight : normalHeight;
                bias *= deltaTime * 2;
            }

            currentCrouchHeight = Mathf.Clamp(currentCrouchHeight + bias, MinCrouchHeight, normalHeight);
            var offsetHeight = currentCrouchHeight + Offset.localPosition.y;
            motor.SetCapsuleDimensions(motor.Capsule.radius, offsetHeight, normalHeight - offsetHeight / 2);
        }
    }
}
