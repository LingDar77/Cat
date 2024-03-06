namespace Cat.Intergration.XRIT.LocomotionSystem
{
    using Cat.SDKManagementSystem;
    using Cat.SDKProvider;
    using Cat.Utillities;
    using Cinemachine;
    using UnityEngine;
    using UnityEngine.InputSystem;

    [DefaultExecutionOrder(0)]
    public class BuiltinHeadRotationTracking : MonoBehaviour
    {
        [SerializeField] private Quaternion Bias = Quaternion.identity;
        [SerializeField] private Transform[] SyncTransforms;
        [SerializeField] private InputActionProperty HeadRotationInput;
        [SerializeField] private InputActionProperty UserPresenceInput;
        [Header("Simulation Input")]
        [SerializeField] private InputActionProperty RotateViewInput;
        public Quaternion RotationBias { get => Bias; set => Bias = value; }

        private bool initialized = false;
        private Quaternion initial;
        private IXRSDKProvider sdk;
        private Vector2 simulationInput;
        private Quaternion targetRotation;
        private CinemachineBrain brain;
        private CinemachineVirtualCamera virtualCamera;

        private void Start()
        {
            RotationBias = initial = transform.root.rotation;
            HeadRotationInput.action.performed += HeadRotationPerformed;
            UserPresenceInput.action.performed += OnUserPresence;
            InputSystem.onAfterUpdate += OnUpdate;

            brain = Camera.main.GetComponent<CinemachineBrain>();
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            brain.m_UpdateMethod = CinemachineBrain.UpdateMethod.ManualUpdate;

        }

        private void OnDestroy()
        {
            HeadRotationInput.action.performed -= HeadRotationPerformed;
            UserPresenceInput.action.performed -= OnUserPresence;
            InputSystem.onAfterUpdate -= OnUpdate;

            if (sdk != null)
                sdk.OnRecenterSuccessed += OnRecenter;
        }

        private void OnUpdate()
        {
            if (initialized)
            {
                targetRotation = HeadRotationInput.action.ReadValue<Quaternion>();

            }
            else if (Mouse.current.rightButton.isPressed)
            {
                simulationInput += RotateViewInput.action.ReadValue<Vector2>();
                simulationInput.y = Mathf.Clamp(simulationInput.y, -90, 90);
                targetRotation = Quaternion.Euler(-simulationInput.y, simulationInput.x, 0);
            }

            transform.rotation = RotationBias * targetRotation;

            virtualCamera.UpdateCameraState(Vector3.up, 0);
            brain.ManualUpdate();
            virtualCamera.UpdateCameraState(Vector3.up, 0);

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
                HeadRotationInput.action.performed -= HeadRotationPerformed;
                Initialze(value);
                return;
            }
        }

        private void Initialze(Quaternion quaternion)
        {
            RotationBias = initial * Quaternion.Inverse(Quaternion.Euler(0, quaternion.eulerAngles.y, 0));
            initialized = true;
            this.Log("HMD Tracking Initialized.");
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