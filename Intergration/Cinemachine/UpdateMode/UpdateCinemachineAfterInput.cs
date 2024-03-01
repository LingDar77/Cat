namespace Cat.Intergration.XRIT.LocomotionSystem
{
    using Cinemachine;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using Cat.Utillities;

    public class UpdateCinemachineAfterInput : MonoBehaviour
    {
        [SerializeField] private CinemachineVirtualCamera vitualCamera;

        private void OnValidate()
        {
            this.EnsureComponent(ref vitualCamera);
        }
        private void OnEnable()
        {
            InputSystem.onAfterUpdate += OnUpdate;
        }
        private void OnDisable()
        {
            InputSystem.onAfterUpdate -= OnUpdate;
        }
        private void OnUpdate()
        {
            if (vitualCamera)
            {
                vitualCamera.InternalUpdateCameraState(Vector3.up, 0);
            }

        }
    }
}