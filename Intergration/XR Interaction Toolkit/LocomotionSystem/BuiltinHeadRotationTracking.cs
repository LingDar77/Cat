namespace Cat.Intergration.XRIT.LocomotionSystem
{
    using System;
    using System.Collections.Generic;
    using Cat.SDKManagementSystem;
    using Cat.SDKProvider;
    using Cat.Utillities;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class BuiltinHeadRotationTracking : MonoBehaviour
    {
        [SerializeField] private Quaternion bias = Quaternion.identity;
        [SerializeField] private Transform[] SyncTransforms;
        [SerializeField] private InputActionProperty HeadRotationInput;
        [SerializeField] private InputActionProperty UserPresenceInput;
        [Header("Simulation Input")]
        [SerializeField] private InputActionProperty RotateViewInput;
        private bool initialized = false;
        public Quaternion RotationBias { get => bias; set => bias = value; }

        private Quaternion initial;
        private IXRSDKProvider sdk;
        private Quaternion targetRotation;
        private Vector2 simulationInput;

        private void OnEnable()
        {
            RotationBias = initial = transform.root.rotation; HeadRotationInput.action.performed += HeadRotationPerformed;
            UserPresenceInput.action.performed += OnUserPresence;
            InputSystem.onAfterUpdate += OnUpdate;
        }

        private void OnDisable()
        {
            HeadRotationInput.action.performed -= HeadRotationPerformed;
            UserPresenceInput.action.performed -= OnUserPresence;
            InputSystem.onAfterUpdate -= OnUpdate;

            if (sdk != null)
                sdk.OnRecenterSuccessed += OnRecenter;
        }

        private void OnUpdate()
        {
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX
            if (Mouse.current.rightButton.isPressed)
            {
                var input = Vector2.zero;
                input = RotateViewInput.action.ReadValue<Vector2>();

                simulationInput += input;
                simulationInput.y = Mathf.Clamp(simulationInput.y, -90, 90);
                targetRotation = Quaternion.Euler(-simulationInput.y, simulationInput.x, 0);
            }
#endif
            transform.rotation = RotationBias * targetRotation;
            foreach (var trans in SyncTransforms)
            {
                trans.rotation = RotationBias;
            }
        }

        private void HeadRotationPerformed(InputAction.CallbackContext context)
        {
            var value = context.ReadValue<Quaternion>();
            if (!initialized)
            {
                Initialze(value);
                initialized = true;
                return;
            }

            targetRotation = value;
        }

        private void Initialze(Quaternion quaternion)
        {
            RotationBias = initial * Quaternion.Inverse(Quaternion.Euler(0, quaternion.eulerAngles.y, 0));
            initialized = true;

            sdk = ISingletonSystem<BuiltinSDKManagement>.GetChecked().GetValidProvider<IXRSDKProvider>();
            if (sdk == null) return;
            sdk.OnRecenterSuccessed += OnRecenter;
        }
        private void OnUserPresence(InputAction.CallbackContext context)
        {
            sdk?.Recenter();
        }
        private void OnRecenter()
        {
            this.Log("Senser Recentered.");
            RotationBias = initial * Quaternion.Inverse(Quaternion.Euler(0, HeadRotationInput.action.ReadValue<Quaternion>().eulerAngles.y, 0));
        }
    }
}