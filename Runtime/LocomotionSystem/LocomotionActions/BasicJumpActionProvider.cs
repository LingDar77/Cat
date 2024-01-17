using TUI.LocomotioinSystem.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TUI.KinematicLocomotionSystem.Actions
{
    /// <summary>
    /// A basic implementation of a jump action
    /// </summary>
    public class BasicJumpActionProvider : ActionProviderBase
    {
        [Tooltip("The Key to Perform Jump.")]
        [SerializeField] private InputActionProperty JumpInput;
        [Tooltip("The Velocity to Gain.")]
        [SerializeField] private float JumpVelocity = 5f;

        private bool shouldJump;
        protected override void OnEnable()
        {
            base.OnEnable();
        }
        public override void BeforeProcess(float deltaTime)
        {
            base.BeforeProcess(deltaTime);
            shouldJump = JumpInput.reference != null && JumpInput.action.IsPressed();
        }
        public override void ProcessVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            base.ProcessVelocity(ref currentVelocity, deltaTime);
            if (!LocomotionSystem.IsStableOnGround() || !shouldJump) return;

            LocomotionSystem.MarkUngrounded();
            currentVelocity.y = JumpVelocity;
        }
    }
}
