using System.Collections;
using SFC.SceneManagementSystem;
using Unity.Netcode;
using UnityEngine;


namespace SFC.Utillities
{
    public class TestNetworkController : MonoBehaviour
    {
        [SerializeField] private Transform NetworkPlayerPrefab;

        private IEnumerator Start()
        {
#if !UNITY_EDITOR
            StartHost();
#endif
            var time = Time.time;
            yield return ISceneManagementSystem.Singleton.LoadSceneAsync("Netcode Test");
            Debug.Log("Scene Loaded in " + (Time.time - time) + " seconds");

        }
        [ContextMenu("StartHost")]
        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
        }

    }
}
