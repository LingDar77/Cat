#if XRIT
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace TUI.Intergration.XRIT.InteractionSystem.Utils
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class AutoRestore : MonoBehaviour
    {
        [SerializeField] private XRGrabInteractable GrabInteractable;
        [SerializeField] private XRBaseInteractor Interactor;
        [SerializeField] private float TimeBeforeAutoRestore = 1f;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (GrabInteractable == null) GrabInteractable = GetComponent<XRGrabInteractable>();
        }
#endif

        private void Start()
        {
            GrabInteractable.transform.SetParent(null);
            GrabInteractable.transform.SetPositionAndRotation(Interactor.attachTransform.position, Interactor.attachTransform.rotation);

            this.WaitUntil(() => Interactor.interactionManager.IsRegistered(GrabInteractable as IXRSelectInteractable), () =>
            {
                Interactor.interactionManager.SelectEnter(Interactor, GrabInteractable as IXRSelectInteractable);

                GrabInteractable.selectExited.AddListener(OnSelectExited);
                GrabInteractable.selectEntered.AddListener(OnSelectEntered);
            });
        }

        private void OnSelectEntered(SelectEnterEventArgs e)
        {
            StopAllCoroutines();
        }

        private void OnSelectExited(SelectExitEventArgs e)
        {
            if (!gameObject.activeSelf) return;
            StartCoroutine(DoAutoRestore());
        }

        private IEnumerator DoAutoRestore()
        {
            yield return new WaitForSeconds(TimeBeforeAutoRestore);
            Interactor.interactionManager.SelectEnter(Interactor, GrabInteractable as IXRSelectInteractable);
        }
    }
}
#endif