namespace Cat.Intergration.XRIT.LocomotionSystem.Actions
{
    using Cat.LocomotionSystem.Actions;
    using Cat.SDKManagementSystem;
    using Cat.SDKProvider;
    using Cat.Utillities;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class HeightBasedCrouchProvider : ActionProviderBase
    {
        public float MinimumCrouchHeight = 1f;
        [SerializeField] private InputActionProperty HMDPosition;
        [SerializeField] private InputActionProperty IsTracked;
        [SerializeField] private InputActionProperty UserPresence;
        [SerializeField] private Transform Offset;
        private CapsuleCollider capsule;
        private float initalCapsuleHeight;
        private float initialCapsuleOffset;
        private float initialHeight;
        private float initialOffset;
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
            initialOffset = Offset.localPosition.y;
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
            if (!IsTracked.action.IsPressed()) return;
            var bias = initialHeight - HMDPosition.action.ReadValue<Vector3>().y;
            SetCapsuleHeight(initalCapsuleHeight - bias);
        }

        protected void SetCapsuleHeight(float height)
        {
            var ajustOffset = 0f;
            if (height < MinimumCrouchHeight) ajustOffset = MinimumCrouchHeight - height;
            if (height > initalCapsuleHeight) ajustOffset = height - initalCapsuleHeight;
            var targetHeight = Mathf.Clamp(height, MinimumCrouchHeight, initalCapsuleHeight);
            capsule.height = targetHeight;
            capsule.center = new Vector3(capsule.center.x, initalCapsuleHeight - targetHeight / 2 - initialCapsuleOffset, capsule.center.z);
            Offset.transform.localPosition = new Vector3(Offset.transform.localPosition.x, initialOffset + ajustOffset, Offset.transform.localPosition.z);
        }
    }
}