#if FISHNET
using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace SFC.Intergration.FN
{
    [RequireComponent(typeof(Rigidbody))]
    public class NetworkRigidbody : NetworkBehaviour
    {
        [SerializeField] private Rigidbody Rigidbody;
        [SerializeField] private float SendRate = .2f;

        //A client-side SyncVar.
        [field: SyncVar(SendRate = .2f, OnChange = nameof(OnIsKinematicCacheChanged))]
        private bool IsKinematicCache { get; [ServerRpc] set; }


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
            while (true)
            {
                yield return new WaitForSeconds(SendRate);
                if (IsKinematicCache == Rigidbody.isKinematic || !IsOwner) continue;
                IsKinematicCache = Rigidbody.isKinematic;
            }
        }

        private void OnIsKinematicCacheChanged(bool prev, bool curr, bool asServer)
        {
            if (IsOwner) return;
            Rigidbody.isKinematic = IsKinematicCache;
        }
    }

}
#endif