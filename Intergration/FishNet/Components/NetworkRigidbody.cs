using System.Collections;
using FishNet.Object;
using UnityEngine;

namespace SFC.Intergration.FN
{
    [RequireComponent(typeof(Rigidbody))]
    public class NetworkRigidbody : NetworkBehaviour
    {
        [SerializeField] private Rigidbody Rigidbody;
        [SerializeField] private bool ClientAuthoritative = true;
        [SerializeField] private float SyncFrequency = 0.1f;
        [Header("Sync Options")]
        [SerializeField] private bool SyncIsKinematic = true;


#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (Rigidbody == null) Rigidbody = GetComponent<Rigidbody>();
        }
#endif
        public override void OnStartNetwork()
        {
            base.OnStartNetwork();
            StartCoroutine(StartSync());
        }
        private IEnumerator StartSync()
        {
            var wait = new WaitForSeconds(SyncFrequency);
            while (true)
            {
                yield return wait;
                if (!IsOwner) continue;
                SetIsKinematicServerRpc(Rigidbody.isKinematic);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetIsKinematicServerRpc(bool isKinematic)
        {
            SetIsKinematicClientRpc(isKinematic);
        }
        [ObserversRpc]
        private void SetIsKinematicClientRpc(bool isKinematic)
        {
            if (IsOwner) return;
            Rigidbody.isKinematic = isKinematic;
        }
    }

}
