using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace SFC.Utillities
{
    public class TestNetworkController : MonoBehaviour
    {
        [SerializeField] private Transform NetworkPlayerPrefab;

        private void Start()
        {
            StartHost();
            NetworkManager.Singleton.SceneManager.LoadScene("Netcode Test", LoadSceneMode.Single);
        }
        [ContextMenu("StartHost")]
        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
        }

    }
}
