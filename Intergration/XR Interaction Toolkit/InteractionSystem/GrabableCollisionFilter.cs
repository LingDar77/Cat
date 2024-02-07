namespace Cat.Intergration.XRIT.InteractionSystem
{
    using global::Cat.LocomotionSystem.Filter;
    using global::Cat.Utillities;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    public class GrabableCollisionFilter : MonoBehaviour, IColliderFilter
    {
        [SerializeField] private XRGrabInteractable interactable;
        [SerializeField] private float delay = 1f;
        private bool grabed;
        private System.Action ResetState;

        private void OnValidate()
        {
            this.EnsureComponentInParent(ref interactable);
        }
        private void OnEnable()
        {
            if (interactable == null) return;
            ResetState = () => grabed = false;
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

        public bool ShouldCollide(Collider other)
        {
            return !grabed;
        }
    }
}