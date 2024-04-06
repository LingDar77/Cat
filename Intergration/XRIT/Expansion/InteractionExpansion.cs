namespace Cat.Utilities
{
    using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
    using UnityEngine.XR.Interaction.Toolkit.Interactables;

    public static class InteractionExpansion
    {
        public static void SendHapticImpulse(this XRBaseInteractable interactable, float amplitude = 1f, float duration = .1f)
        {
            foreach (var interactor in interactable.interactorsSelecting)
            {
                if (interactor.handedness == UnityEngine.XR.Interaction.Toolkit.Interactors.InteractorHandedness.None) continue;
                if (!interactor.transform.TryGetComponentInParent<HapticImpulsePlayer>(out var comp)) continue;
                comp.SendHapticImpulse(amplitude, duration);
            }
        }
    }
}