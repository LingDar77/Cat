#if XRIT
using FishNet.Connection;
using FishNet.Object;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FishNetXRGrabableOwnershipTranfer : NetworkBehaviour
{
    [SerializeField] private XRBaseInteractable Interactable;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        if (Interactable == null) Interactable = GetComponent<XRBaseInteractable>();
    }
#endif
    private void Start()
    {
        Interactable.selectEntered.AddListener(OnSelectEntered);
        Interactable.selectExited.AddListener(OnSelectExited);
    }

    private void OnSelectEntered(SelectEnterEventArgs e)
    {
        TransferOwnershipServerRpc(LocalConnection);
        SetSelectStateServerRpc(true);
    }
    private void OnSelectExited(SelectExitEventArgs e)
    {
        SetSelectStateServerRpc(false);
    }
    [ServerRpc(RequireOwnership = false)]
    private void TransferOwnershipServerRpc(NetworkConnection newOwner)
    {
        GiveOwnership(newOwner);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetSelectStateServerRpc(bool state)
    {
        SetSelectStateClientRpc(state);
    }
    [ObserversRpc(ExcludeOwner = true)]
    private void SetSelectStateClientRpc(bool state)
    {
        Interactable.enabled = !state;
    }
}

#endif