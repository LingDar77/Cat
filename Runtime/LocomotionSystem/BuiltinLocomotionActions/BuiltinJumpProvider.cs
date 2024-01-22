using TUI.LocomotionSystem.Actions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TUI.KinematicLocomotionSystem.Actions
{
    /// <summary>
    /// A basic implementation of a jump action
    /// </summary>
    public class BuiltinJumpProvider : ActionProviderBase
    {
        [Tooltip("The Key to Perform Jump.")]
        [SerializeField] private InputActionProperty jumpContrl;

        [Tooltip("The Velocity to Gain.")]
        [SerializeField] private float JumpVelocity = 5f;

        private bool shouldJump;
        
        public override void BeforeProcess(float deltaTime)
        {
            base.BeforeProcess(deltaTime);
            shouldJump = jumpContrl.reference != null && jumpContrl.action.IsPressed();
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
