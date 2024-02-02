namespace TUI.Intergration.XRIT.LocomotionSystem.Actions
{
    using TUI.Attributes;
    using TUI.LocomotionSystem.Actions;
    using TUI.SDKManagementSystem;
    using TUI.SDKProvider;
    using TUI.Utillities;
    using UnityEngine;
    using UnityEngine.InputSystem;

    [DefaultExecutionOrder(1001)]
    public class BuiltinHeadVelocityProvider : ActionProviderBase
    {
        [SerializeField] private InputActionProperty HeadSpeedInput;
        [ReadOnlyInEditor]
        public Quaternion VelocityBias = Quaternion.identity;
        private Vector2 headVelocity;
        private Quaternion initial;
        private IXRSDKProvider sdk;

        private void Start()
        {
            initial = VelocityBias = transform.root.rotation;
            sdk = ISingletonSystem<BuiltinSDKManagement>.GetChecked().GetValidProvider<IXRSDKProvider>();
            if (sdk == null) return;
            sdk.OnRecenterSuccessed += OnRecenter;
        }
        private void OnDestroy()
        {
            if (sdk == null) return;
            sdk.OnRecenterSuccessed -= OnRecenter;
        }
        private void OnRecenter()
        {
            VelocityBias = initial;
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
