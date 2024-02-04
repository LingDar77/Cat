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
        private Coroutine coroutine;

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
            if (coroutine != null) StopCoroutine(coroutine);
            grabed = true;
        }
        private void OnSelectExited(SelectExitEventArgs e)
        {
            coroutine = CoroutineHelper.WaitForSeconds(ResetState, delay);
        }
        private void ResetState()
        {
            grabed = false;
            coroutine = null;
        }
        public bool ShouldCollide(Collider other)
        {
            return !grabed;
        }
    }
}