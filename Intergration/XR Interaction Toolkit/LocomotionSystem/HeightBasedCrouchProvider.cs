namespace Cat.Intergration.XRIT.LocomotionSystem.Actions
{
    using Cat.LocomotionSystem.Actions;
    using Cat.SDKManagementSystem;
    using Cat.SDKProvider;
    using Cat.Utilities;
    using UnityEngine;
    using UnityEngine.InputSystem;

    [DefaultExecutionOrder(201)]
    public class HeightBasedCrouchProvider : ActionProviderBase
    {
        public float MinimumCrouchHeight = 1f;
        [SerializeField] private InputActionProperty HMDPosition;
        [SerializeField] private InputActionProperty IsTracked;
        [SerializeField] private InputActionProperty UserPresence;
        [SerializeField] private Transform SimulationRoot;
        private CapsuleCollider capsule;
        private float initalCapsuleHeight;
        private float initialCapsuleOffset;
        private float initialHeight;
        private float initialOffset;
        private float lastHeight;
        private IXRSDKProvider sdk;

        private void Start()
        {
            sdk = ISingletonSystem<BuiltinSDKManagement>.GetChecked().GetValidProvider<IXRSDKProvider>();
            if (sdk != null)
            {
                sdk.OnRecenterSuccessed += ResetInitialHeight;
            }
            CoroutineHelper.WaitUntil(() => IsTracked.action.IsPressed(), ResetInitialHeight);
        }

        private void OnDestroy()
        {
            if (sdk == null) return;
            sdk.OnRecenterSuccessed += ResetInitialHeight;

        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (LocomotionSystem == null) return;
            initialOffset = SimulationRoot.localPosition.y;
            capsule = LocomotionSystem.transform.GetComponent<CapsuleCollider>();
            initalCapsuleHeight = capsule.height;
            initialCapsuleOffset = initalCapsuleHeight / 2 - capsule.center.y;
            if (UserPresence == null) return;
            UserPresence.action.performed += ResetInitialHeight;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (UserPresence == null) return;
            UserPresence.action.performed -= ResetInitialHeight;
        }

        private void ResetInitialHeight()
        {
            ResetInitialHeight(default);
        }

        private void ResetInitialHeight(InputAction.CallbackContext context)
        {
            var height = HMDPosition.action.ReadValue<Vector3>().y;
            initialHeight = height;
        }

        public override void BeforeProcess(float deltaTime)
        {

            if (!UserPresence.action.IsPressed()) return;

            var bias = initialHeight - HMDPosition.action.ReadValue<Vector3>().y;
            SetCapsuleHeight(initalCapsuleHeight - bias);
        }

        protected void SetCapsuleHeight(float height)
        {
            if (lastHeight != 0 && Mathf.Abs(height - lastHeight) > Time.deltaTime * 4) return;

            var ajustOffset = 0f;
            if (height < MinimumCrouchHeight) ajustOffset = height - MinimumCrouchHeight;
            if (height > initalCapsuleHeight) ajustOffset = height - initalCapsuleHeight;

            var targetHeight = Mathf.Clamp(height, MinimumCrouchHeight, initalCapsuleHeight);
            capsule.height = targetHeight;
            capsule.center = new Vector3(capsule.center.x, initalCapsuleHeight - targetHeight / 2 - initialCapsuleOffset, capsule.center.z);

            SimulationRoot.transform.localPosition = new Vector3(
                SimulationRoot.transform.localPosition.x,
                initialOffset + ajustOffset,
                SimulationRoot.transform.localPosition.z);

            lastHeight = targetHeight;
        }
    }
}