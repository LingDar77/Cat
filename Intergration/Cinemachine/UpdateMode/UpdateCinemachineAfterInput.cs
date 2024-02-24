namespace Cat.Intergration.XRIT.LocomotionSystem
{
    using Cinemachine;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using Cat.Utillities;

    [RequireComponent(typeof(CinemachineBrain))]
    [DefaultExecutionOrder(2001)]
    public class UpdateCinemachineAfterInput : MonoBehaviour
    {
        [SerializeField] private CinemachineBrain brain;
        private void OnValidate()
        {
            this.EnsureComponent(ref brain);
        }
        private void OnEnable()
        {
            brain = Camera.main.GetComponent<CinemachineBrain>();
            InputSystem.onAfterUpdate += brain.ManualUpdate;
        }
        private void OnDisable()
        {
            InputSystem.onAfterUpdate -= brain.ManualUpdate;
        }
    }
}