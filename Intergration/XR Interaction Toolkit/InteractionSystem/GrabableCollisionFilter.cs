namespace TUI.Intergration.XRIT.InteractionSystem
{
    using System;
    using TUI.LocomotionSystem.Filter;
    using TUI.Utillities;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    public class GrabableCollisionFilter : MonoBehaviour, IColliderFilter
    {
        [SerializeField] private XRGrabInteractable interactable;
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
            grabed = true;
        }
        private void OnSelectExited(SelectExitEventArgs e)
        {
            grabed = false;
        }
        public bool ShouldCollide(Collider other)
        {
            return !grabed;
        }
    }
}