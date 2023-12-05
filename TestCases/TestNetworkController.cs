using Unity.Netcode;
using UnityEngine;


namespace SFC.Utillities
{
    public class TestNetworkController : MonoBehaviour
    {
        [SerializeField] private Transform NetworkPlayerPrefab;

        private void Start()
        {
#if !UNITY_EDITOR
            NetworkManager.Singleton.StartClient();
#endif
        }
        [ContextMenu("StartHost")]
        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
        }

    }
}
