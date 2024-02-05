namespace TUI.Intergration.XRIT.InteractionSystem
{
    using TUI.LocomotionSystem.Filter;
    using TUI.Utillities;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    public class GrabableCollisionFilter : MonoBehaviour, IColliderFilter
    {
        [SerializeField] private XRGrabInteractable interactable;
        [SerializeField] private float delay = 1f;
        private bool grabed;

        private void OnValidate()
        {
            this.EnsureComponentInParent(ref interactable);
        }
        private void OnEnable()
        {
            if (interactable == null) return;
            interactable.selectEntered.AddListener(OnSelectEntered);
            interactable.selectExited.AddListener(OnSelectExited);
        }
        private void OnDisable()
        {
            if (interactable == null) return;
            interactable.selectEntered.RemoveListener(OnSelectEntered);
            interactable.selectExited.RemoveListener(OnSelectExited);
        }
        private void OnSelectEntered(SelectEnterEventArgs e)
        {
            StopAllCoroutines();
            grabed = true;
        }
        private void OnSelectExited(SelectExitEventArgs e)
        {
            this.WaitForSeconds(ResetState, delay);
        }
        private void ResetState()
        {
            grabed = false;
        }
        public bool ShouldCollide(Collider other)
        {
            return !grabed;
        }
    }
}