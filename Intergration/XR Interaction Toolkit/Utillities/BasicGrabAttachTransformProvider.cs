namespace TUI.Intergration.XRIT.InteractionSystem
{
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

    public class GrabAttachTransformProvider : MonoBehaviour
    {
        [SerializeField] private XRGrabInteractable GrabInteractable;
        [SerializeField] private Transform AttachTransform;
        [SerializeField] private string InteractorGroupName;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (GrabInteractable == null) GrabInteractable = GetComponentInParent<XRGrabInteractable>();
            if (AttachTransform == null) AttachTransform = transform;
        }
#endif
        private void Start()
        {
            GrabInteractable.hoverEntered.AddListener(OnHoverEntered);
        }

        private void OnHoverEntered(HoverEnterEventArgs e)
        {
            var group = e.interactorObject.transform.GetComponentInParent<XRInteractionGroup>();
            if (group != null && group.groupName == InteractorGroupName)
            {
                GrabInteractable.attachTransform = AttachTransform;
            }
        }
    }
}