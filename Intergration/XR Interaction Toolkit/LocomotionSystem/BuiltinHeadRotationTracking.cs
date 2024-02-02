namespace TUI.Intergration.XRIT.LocomotionSystem
{
    using System;
    using System.Collections;
    using TUI.SDKManagementSystem;
    using TUI.SDKProvider;
    using TUI.Utillities;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class BuiltinHeadRotationTracking : MonoBehaviour
    {
        [SerializeField] private Quaternion bias = Quaternion.identity;
        public Quaternion RotationBias { get => bias; set => bias = value; }
        public Transform[] SyncTransforms;
        [SerializeField] private InputActionProperty IsTrackedInput;
        [SerializeField] private InputActionProperty HeadRotationInput;
        [SerializeField] private InputActionProperty UserPresenceInput;

        [Header("Simulation Input")]
        [SerializeField] private InputActionProperty RotateViewInput;

        private bool initialized = false;
        public bool Initialized => initialized;

        private Vector2 simulationInput;
        private IXRSDKProvider sdk;
        private Quaternion initial;

        private IEnumerator Start()
        {
            RotationBias = initial = transform.root.rotation;

            if (UserPresenceInput != null)
            {
                UserPresenceInput.action.performed += OnUserPresence;
            }
            yield return new WaitUntil(() => IsTrackedInput.action.IsPressed());
            RotationBias = initial * Quaternion.Inverse(Quaternion.Euler(0, HeadRotationInput.action.ReadValue<Quaternion>().eulerAngles.y, 0));
            initialized = true;

            sdk = ISingletonSystem<BuiltinSDKManagement>.GetChecked().GetValidProvider<IXRSDKProvider>();
            if (sdk == null) yield break;
            sdk.OnRecenterSuccessed += OnRecenter;
        }

        private void OnDestroy()
        {
            if (UserPresenceInput != null)
            {
                UserPresenceInput.action.performed += OnUserPresence;
            }
            if (sdk == null) return;
            sdk.OnRecenterSuccessed -= OnRecenter;
        }
        private void OnUserPresence(InputAction.CallbackContext context)
        {
            sdk.Recenter();
        }
        private void OnRecenter()
        {
            this.Log("Senser Recentered.");
            RotationBias = initial * Quaternion.Inverse(Quaternion.Euler(0, HeadRotationInput.action.ReadValue<Quaternion>().eulerAngles.y, 0));
        }


        private void LateUpdate()
        {

            if (Initialized)
            {
                transform.rotation = RotationBias * HeadRotationInput.action.ReadValue<Quaternion>();
                if (SyncTransforms == null) return;
                foreach (var trans in SyncTransforms)
                {
                    trans.rotation = RotationBias;
                }
                return;
            }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX || STEAMWORKS_WIN || STEAMWORKS_LIN_OSX
            var input = Vector2.zero;
            if (Mouse.current.rightButton.isPressed) input = RotateViewInput.action.ReadValue<Vector2>();

            simulationInput += input;
            simulationInput.y = Mathf.Clamp(simulationInput.y, -90, 90);
            var rot = Quaternion.Euler(-simulationInput.y, simulationInput.x, 0);
            transform.rotation = RotationBias * rot;
#endif
        }

    }
}