namespace TUI.LocomotionSystem.Actions
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// A basic implementation of a jump action
    /// </summary>
    public class BuiltinJumpProvider : ActionProviderBase
    {
        [Tooltip("The Key to Perform Jump.")]
        [SerializeField] private InputActionProperty JumpControl;

        [Tooltip("The Velocity to Gain.")]
        [SerializeField] private float JumpVelocity = 5f;

        private bool shouldJump;

        public override void BeforeProcess(float deltaTime)
        {
            base.BeforeProcess(deltaTime);
            shouldJump = JumpControl.reference != null && JumpControl.action.IsPressed();
        }
        public override void ProcessVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            base.ProcessVelocity(ref currentVelocity, deltaTime);
            if (!LocomotionSystem.IsOnGround() || !shouldJump) return;

            LocomotionSystem.MarkUngrounded();
            currentVelocity.y = JumpVelocity;
        }
    }
}
