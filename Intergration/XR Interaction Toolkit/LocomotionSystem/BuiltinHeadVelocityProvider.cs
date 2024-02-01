namespace TUI.Intergration.XRIT
{
    using TUI.Attributes;
    using TUI.LocomotionSystem.Actions;
    using TUI.Utillities;
    using UnityEngine;
    using UnityEngine.InputSystem;

    [DefaultExecutionOrder(1001)]
    public class BuiltinHeadVelocityProvider : ActionProviderBase
    {
        [SerializeField] private InputActionProperty HeadSpeedInput;
        [ReadOnlyInEditor]
        public Quaternion VelocityBias;
        private Vector2 headVelocity;

        private void Start()
        {
            VelocityBias = transform.root.rotation;
        }

        public override void BeforeProcess(float deltaTime)
        {
            base.BeforeProcess(deltaTime);

            if (HeadSpeedInput != null)
            {
                headVelocity = (VelocityBias * HeadSpeedInput.action.ReadValue<Vector3>()).XZ();
            }
        }
        public override void ProcessVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            currentVelocity.x += headVelocity.x;
            currentVelocity.z += headVelocity.y;
        }

    }
}
