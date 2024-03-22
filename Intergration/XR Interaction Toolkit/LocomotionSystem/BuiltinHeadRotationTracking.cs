namespace Cat.Intergration.XRIT.LocomotionSystem
{
    using Cat.SDKManagementSystem;
    using Cat.SDKProvider;
    using Cat.Utilities;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.InputSystem;

    [DefaultExecutionOrder(9999)]
    public class BuiltinHeadRotationTracking : MonoBehaviour
    {
        [SerializeField] private Quaternion Bias = Quaternion.identity;
        [SerializeField] private Transform[] SyncTransforms;
        [SerializeField] private InputActionProperty HeadRotationInput;
        [SerializeField] private InputActionProperty UserPresenceInput;
        [Header("Simulation Input")]
        [SerializeField] private InputActionProperty RotateViewInput;
        public UnityEvent OnInitialized;
        public Quaternion RotationBias { get => Bias; set => Bias = value; }

        private bool initialized = false;
        private Quaternion initial;
        private IXRSDKProvider sdk;
        private Vector2 simulationInput;
        private Quaternion targetRotation;

        private void Start()
        {
            RotationBias = initial = transform.root.rotation;
            if (sdk != null)
                sdk.OnRecenterSuccessed += OnRecenter;
        }
        private void OnEnable()
        {
            HeadRotationInput.action.performed += HeadRotationPerformed;
            UserPresenceInput.action.performed += OnUserPresence;
        }
        private void OnDisable()
        {
            HeadRotationInput.action.performed -= HeadRotationPerformed;
            UserPresenceInput.action.performed -= OnUserPresence;
        }


        private void Update()
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
            OnInitialized.Invoke();
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