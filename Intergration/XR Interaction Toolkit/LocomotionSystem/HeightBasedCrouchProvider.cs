namespace TUI.Intergration.XRIT.LocomotionSystem.Actions
{
    using System;
    using System.Collections;
    using TUI.LocomotionSystem.Actions;
    using TUI.SDKManagementSystem;
    using TUI.SDKProvider;
    using TUI.Utillities;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class HeightBasedCrouchProvider : ActionProviderBase
    {
        public float MinimumCrouchHeight = 1f;
        [SerializeField] private InputActionProperty HMDPosition;
        [SerializeField] private InputActionProperty IsTracked;
        [SerializeField] private InputActionProperty UserPresence;

        private CapsuleCollider capsule;
        private float initalCapsuleHeight;
        private float initialCapsuleOffset;
        private float initialHeight;
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
            initialHeight = HMDPosition.action.ReadValue<Vector3>().y;
        }
        public override void BeforeProcess(float deltaTime)
        {
            if (!IsTracked.action.IsPressed()) return;
            var bias = initialHeight - HMDPosition.action.ReadValue<Vector3>().y;
            SetCapsuleHeight(initalCapsuleHeight - bias);
        }

        protected void SetCapsuleHeight(float height)
        {
            var targetHeight = Mathf.Clamp(height, MinimumCrouchHeight, initalCapsuleHeight);
            this.LogFormat("Current Height: {0}", LogType.Log, targetHeight.ToString());
            capsule.height = targetHeight;
            capsule.center = new Vector3(capsule.center.x, initalCapsuleHeight - targetHeight / 2 - initialCapsuleOffset, capsule.center.z);
        }
    }
}